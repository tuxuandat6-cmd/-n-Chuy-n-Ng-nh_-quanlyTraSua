using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MixueDatMon.DichVu;
using MixueDatMon.DuLieu;
using MixueDatMon.HubThoiGianThuc;
using MixueDatMon.MoHinh;
using MixueDatMon.MoHinh.VM;

namespace MixueDatMon.Controllers;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class DonHangController : Controller
{
    private readonly DuLieuContext _csdl;
    private readonly BaoVeDonKhach _quyenDon;
    private readonly IHubContext<DonHangHub> _hub;
    private readonly DichVuDatHang _dichVuDatHang;
    private readonly DichVuVietQr _vietQr;
    private readonly DichVuCaiDat _caiDat;
    private readonly DichVuKhuyenMai _khuyenMai;
    private readonly DichVuGioiHanTanSuat _gioiHan;
    public DonHangController(DuLieuContext csdl, IHubContext<DonHangHub> hub, DichVuDatHang dichVuDatHang, DichVuVietQr vietQr,
        DichVuCaiDat caiDat, DichVuKhuyenMai khuyenMai, DichVuGioiHanTanSuat gioiHan, IDataProtectionProvider protection)
    { _quyenDon = new BaoVeDonKhach(protection); _csdl = csdl; _hub = hub; _dichVuDatHang = dichVuDatHang; _vietQr = vietQr; _caiDat = caiDat; _khuyenMai = khuyenMai; _gioiHan = gioiHan; }

    /// <summary>
    /// Khách gửi giỏ hàng. Khách quét QR bàn thì đơn gắn vào bàn (thanh toán sau tại quầy);
    /// khách không ngồi bàn là đơn MANG ĐI: phải thanh toán tại quầy trước khi quán bắt đầu pha chế.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DatHang([FromBody] YeuCauDatHang yeuCau)
    {
        if (yeuCau is null) return BadRequest();
        var checkoutCode = _quyenDon.CheckoutCode(Request.Headers["Checkout-Token"], yeuCau);
        if (checkoutCode is null) return BadRequest(new { thanhCong = false, thongBao = "Phiếu đặt hàng sai, đã hết hạn hoặc nội dung đã thay đổi. Vui lòng chuẩn bị lại đơn." });
        await using var transaction = await _csdl.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        // The existing unique index on MaTheoDoi plus update/range locks serializes concurrent replays.
        var existing = await _csdl.DonHang.FromSqlInterpolated(
            $"SELECT * FROM dbo.DonHang WITH (UPDLOCK, HOLDLOCK) WHERE MaTheoDoi = {checkoutCode}").FirstOrDefaultAsync();
        if (existing is not null)
        {
            GhiDonVaoTrinhDuyet(existing.MaTheoDoi);
            await transaction.CommitAsync();
            return Ok(new { thanhCong = true, maTheoDoi = existing.MaTheoDoi, duongDan = Url.Action(nameof(TheoDoi), "DonHang", new { ma = existing.MaTheoDoi }) });
        }
        var (duocDat, thongBaoDong) = await _caiDat.KiemTraNhanDonAsync();
        if (!duocDat) return BadRequest(new { thanhCong = false, thongBao = thongBaoDong });

        int? maBan = HttpContext.Session.GetInt32("MaBan");

        var ip = DichVuGioiHanTanSuat.LayIp(HttpContext);
        if (!_gioiHan.ChoPhep($"dathang-ip-{ip}", 5, TimeSpan.FromMinutes(10), out var choIp))
            return StatusCode(429, new { thanhCong = false, thongBao = $"Bạn đặt hơi nhanh, vui lòng thử lại sau {DichVuGioiHanTanSuat.DinhDangThoiGianCho(choIp)}." });
        if (maBan != null && !_gioiHan.ChoPhep($"dathang-ban-{maBan}", 10, TimeSpan.FromMinutes(10), out var choBan))
            return StatusCode(429, new { thanhCong = false, thongBao = $"Bàn này vừa đặt nhiều đơn liên tiếp, vui lòng thử lại sau {DichVuGioiHanTanSuat.DinhDangThoiGianCho(choBan)}." });

        string noiPhucVu = "Mang đi";
        if (maBan != null)
        {
            var ban = await _csdl.BanAn.AsNoTracking().FirstOrDefaultAsync(b => b.MaBan == maBan && b.DangHoatDong);
            if (ban == null) maBan = null; else noiPhucVu = ban.TenBan;
        }

        var (donHang, loi) = await _dichVuDatHang.DungDonAsync(yeuCau, maBan, kiemTraKho: true);
        if (donHang == null) return BadRequest(new { thanhCong = false, thongBao = loi });
        donHang.MaTheoDoi = checkoutCode;

        _csdl.DonHang.Add(donHang);
        await _csdl.SaveChangesAsync();
        await _khuyenMai.GiuLuotAsync(donHang);
        await transaction.CommitAsync();

        // Báo thời gian thực cho thu ngân
        await _hub.Clients.Group("nhanvien").SendAsync("CoDonMoi", new
        {
            maDonHang = donHang.MaDonHang, maTheoDoi = donHang.MaTheoDoi, noiPhucVu, mangDi = maBan == null
        });

        GhiDonVaoTrinhDuyet(donHang.MaTheoDoi);
        return Ok(new { thanhCong = true, maTheoDoi = donHang.MaTheoDoi, duongDan = Url.Action(nameof(TheoDoi), "DonHang", new { ma = donHang.MaTheoDoi }) });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ChuanBiDatHang([FromBody] YeuCauDatHang request) =>
        request?.DanhSachMon is { Count: > 0 and <= 40 }
            ? Ok(new { phieu = _quyenDon.IssueTicket(request) })
            : BadRequest(new { thongBao = "Giỏ hàng không hợp lệ." });

    /// <summary>Khách nhập mã giảm giá vào giỏ hàng trước khi gửi đơn: kiểm tra và trả về số tiền giảm thực tế.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> KiemTraMaGiamGia([FromBody] YeuCauKiemTraMa yeuCau)
    {
        var ketQua = await _khuyenMai.KiemTraMaAsync(yeuCau?.MaCode, yeuCau?.DanhSachMon ?? new List<MonDat>());
        return Ok(ketQua);
    }

    /// <summary>Khách nhập số điện thoại để xem số điểm tích lũy hiện có.</summary>
    [HttpGet]
    public async Task<IActionResult> DiemCuaToi(string? soDienThoai)
    {
        var soChuan = DichVuKhuyenMai.ChuanHoaSoDienThoai(soDienThoai);
        if (soChuan == null) return View(new DiemCuaToiVM());
        return View(await _khuyenMai.LayDiemAsync(soChuan));
    }

    /// <summary>Khách ngồi bàn bấm "Gọi nhân viên" hoặc "Xin thanh toán" ngay tại chỗ.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> GoiNhanVien([FromBody] YeuCauGoiNhanVien yeuCau)
    {
        var maBan = HttpContext.Session.GetInt32("MaBan");
        if (maBan == null) return BadRequest(new { thanhCong = false, thongBao = "Chức năng này chỉ dùng khi bạn đang ngồi tại bàn." });
        if (yeuCau.Loai != HangSoYeuCau.GoiNhanVien && yeuCau.Loai != HangSoYeuCau.XinThanhToan)
            return BadRequest(new { thanhCong = false, thongBao = "Yêu cầu không hợp lệ." });

        if (!_gioiHan.ChoPhep($"goinv-ban-{maBan}", 6, TimeSpan.FromMinutes(5), out var choGoi))
            return StatusCode(429, new { thanhCong = false, thongBao = $"Bạn vừa gọi nhân viên rồi, vui lòng chờ thêm {DichVuGioiHanTanSuat.DinhDangThoiGianCho(choGoi)}." });

        var ban = await _csdl.BanAn.AsNoTracking().FirstOrDefaultAsync(b => b.MaBan == maBan && b.DangHoatDong);
        if (ban == null) return BadRequest(new { thanhCong = false, thongBao = "Không xác định được bàn của bạn." });

        // Không tạo thêm yêu cầu trùng loại nếu bàn đang có yêu cầu cùng loại chưa được xử lý
        var daCo = await _csdl.YeuCauTaiBan.AnyAsync(y => y.MaBan == maBan && y.LoaiYeuCau == yeuCau.Loai && !y.DaXuLy);
        if (!daCo)
        {
            _csdl.YeuCauTaiBan.Add(new YeuCauTaiBan { MaBan = maBan.Value, LoaiYeuCau = yeuCau.Loai, NgayTao = DateTime.Now });
            await _csdl.SaveChangesAsync();
            await _hub.Clients.Group("nhanvien").SendAsync("YeuCauThayDoi");
        }
        return Ok(new { thanhCong = true, thongBao = yeuCau.Loai == HangSoYeuCau.GoiNhanVien ? "Đã báo nhân viên, bạn chờ chút nhé." : "Đã báo quầy, nhân viên sẽ ra thu tiền." });
    }

    private void GhiDonVaoTrinhDuyet(string maTheoDoi) => _quyenDon.Remember(HttpContext, maTheoDoi);

    /// <summary>Trang "Đơn của tôi": liệt kê các đơn khách từng đặt trên chính máy này (đọc từ cookie), mới nhất trước.</summary>
    public async Task<IActionResult> DonCuaToi()
    {
        var danhSachMa = _quyenDon.Read(HttpContext).ToList();
        if (danhSachMa.Count == 0) return View(new List<DonHang>());

        var danhSachDon = await _csdl.DonHang.AsNoTracking().AsSplitQuery()
            .Include(d => d.BanAn).Include(d => d.DanhSachChiTiet).ThenInclude(c => c.MonAn)
            .Include(d => d.DanhSachChiTiet).ThenInclude(c => c.DanhSachTopping)
            .Where(d => danhSachMa.Contains(d.MaTheoDoi)).ToListAsync();
        var theoThuTu = danhSachMa.Select(ma => danhSachDon.FirstOrDefault(d => d.MaTheoDoi == ma)).Where(d => d != null).Cast<DonHang>().ToList();
        return View(theoThuTu);
    }

    /// <summary>Trang khách theo dõi đơn của mình.</summary>
    public async Task<IActionResult> TheoDoi(string ma)
    {
        if (!_quyenDon.Owns(HttpContext, ma)) return NotFound();
        var donHang = await _csdl.DonHang.AsNoTracking().AsSplitQuery()
            .Include(d => d.BanAn)
            .Include(d => d.DanhSachChiTiet).ThenInclude(c => c.MonAn)
            .Include(d => d.DanhSachChiTiet).ThenInclude(c => c.MucDa)
            .Include(d => d.DanhSachChiTiet).ThenInclude(c => c.MucDuong)
            .Include(d => d.DanhSachChiTiet).ThenInclude(c => c.KichCo)
            .Include(d => d.DanhSachChiTiet).ThenInclude(c => c.DanhSachTopping).ThenInclude(t => t.Topping)
            .Include(d => d.DanhSachChiTiet).ThenInclude(c => c.DanhGia)
            .Include(d => d.DanhGia)
            .FirstOrDefaultAsync(d => d.MaTheoDoi == ma);
        if (donHang == null) return NotFound();
        return View(donHang);
    }

    /// <summary>
    /// Khách gửi đánh giá cho đơn của mình (chấm sao từng món + đánh giá chung về quán).
    /// Chỉ đánh giá được khi đồ đã Sẵn sàng / Hoàn thành, và mỗi dòng món / mỗi đơn chỉ đánh giá 1 lần.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> GuiDanhGia([FromBody] YeuCauDanhGia yeuCau)
    {
        if (yeuCau != null && !_quyenDon.Owns(HttpContext, yeuCau.MaTheoDoi)) return NotFound();
        if (yeuCau == null || string.IsNullOrWhiteSpace(yeuCau.MaTheoDoi))
            return BadRequest(new { thanhCong = false, thongBao = "Thiếu mã đơn." });

        var donHang = await _csdl.DonHang
            .Include(d => d.DanhSachChiTiet).ThenInclude(c => c.DanhGia)
            .Include(d => d.DanhGia)
            .FirstOrDefaultAsync(d => d.MaTheoDoi == yeuCau.MaTheoDoi);
        if (donHang == null) return NotFound(new { thanhCong = false, thongBao = "Không tìm thấy đơn." });

        if (donHang.MaTrangThai != HangSoTrangThai.SanSang && donHang.MaTrangThai != HangSoTrangThai.HoanThanh)
            return BadRequest(new { thanhCong = false, thongBao = "Bạn có thể đánh giá khi đồ đã sẵn sàng." });

        var soLuongMoi = 0;
        foreach (var mon in yeuCau.DanhSachMon ?? new List<DanhGiaMonGui>())
        {
            if (mon.SoSao < 1 || mon.SoSao > 5) continue;
            var chiTiet = donHang.DanhSachChiTiet.FirstOrDefault(c => c.MaChiTiet == mon.MaChiTiet);
            if (chiTiet == null || chiTiet.DanhGia != null) continue;      // sai dòng món hoặc đã đánh giá rồi
            _csdl.DanhGiaMon.Add(new DanhGiaMon
            {
                MaChiTiet = chiTiet.MaChiTiet, SoSao = mon.SoSao, NhanXet = DichVuDatHang.LamSach(mon.NhanXet, 500), NgayTao = DateTime.Now
            });
            soLuongMoi++;
        }

        if (yeuCau.SoSaoDichVu is int soSaoDichVu && soSaoDichVu >= 1 && soSaoDichVu <= 5 && donHang.DanhGia == null)
        {
            _csdl.DanhGiaDonHang.Add(new DanhGiaDonHang
            {
                MaDonHang = donHang.MaDonHang, SoSao = soSaoDichVu, GopY = DichVuDatHang.LamSach(yeuCau.GopY, 1000), NgayTao = DateTime.Now
            });
            soLuongMoi++;
        }

        if (soLuongMoi == 0)
            return BadRequest(new { thanhCong = false, thongBao = "Bạn chưa chọn số sao nào, hoặc đơn này đã được đánh giá." });

        try { await _csdl.SaveChangesAsync(); }
        catch (DbUpdateException)   // hai lần gửi cùng lúc: ràng buộc UNIQUE chặn đánh giá trùng
        {
            return BadRequest(new { thanhCong = false, thongBao = "Đơn này đã được đánh giá trước đó." });
        }
        return Ok(new { thanhCong = true, thongBao = "Cảm ơn bạn! Đánh giá của bạn giúp quán phục vụ tốt hơn." });
    }

    /// <summary>Ảnh mã VietQR của một đơn: đúng số tiền của đơn, nội dung là mã đơn. Chỉ có với đơn chưa thanh toán và chưa hủy.</summary>
    [HttpGet]
    public async Task<IActionResult> AnhQr(string ma)
    {
        if (!_quyenDon.Owns(HttpContext, ma)) return NotFound();
        if (!_vietQr.DaCauHinh) return NotFound();
        var don = await (from d in _csdl.DonHang
                         join t in _csdl.TongTienDon on d.MaDonHang equals t.MaDonHang
                         where d.MaTheoDoi == ma && d.NgayThanhToan == null && d.MaTrangThai != HangSoTrangThai.DaHuy
                         select new { d.MaTheoDoi, t.TongTien }).FirstOrDefaultAsync();
        if (don == null || don.TongTien <= 0) return NotFound();

        Response.Headers.CacheControl = "no-store";
        return File(_vietQr.TaoAnhPng(_vietQr.TaoChuoiVietQr(don.TongTien, _vietQr.NoiDungChoDon(don.MaTheoDoi))), "image/png");
    }

    /// <summary>Dự phòng khi mất kết nối SignalR.</summary>
    [HttpGet]
    public async Task<IActionResult> LayTrangThai(string ma)
    {
        if (!_quyenDon.Owns(HttpContext, ma)) return NotFound();
        var don = await _csdl.DonHang.AsNoTracking().Where(d => d.MaTheoDoi == ma)
            .Select(d => new { d.MaTrangThai, daThanhToan = d.NgayThanhToan != null }).FirstOrDefaultAsync();
        if (don == null) return NotFound();
        return Json(new { maTrangThai = don.MaTrangThai, tenTrangThai = HangSoTrangThai.Ten(don.MaTrangThai), don.daThanhToan });
    }
}
