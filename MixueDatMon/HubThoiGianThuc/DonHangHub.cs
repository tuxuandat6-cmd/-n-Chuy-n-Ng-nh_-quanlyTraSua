using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.DataProtection;
using MixueDatMon.DichVu;
using MixueDatMon.MoHinh;

namespace MixueDatMon.HubThoiGianThuc;

/// <summary>
/// Nhóm "nhanvien": thu ngân / quản lý nhận đơn mới và thay đổi.
/// Nhóm "don-{ma}": khách đang theo dõi đúng đơn của mình.
/// Sự kiện gửi xuống trình duyệt: CoDonMoi, DonThayDoi, TrangThaiThayDoi.
/// </summary>
public class DonHangHub : Hub
{
    private readonly BaoVeDonKhach _ownership;
    public DonHangHub(IDataProtectionProvider protection) => _ownership = new(protection);
    public async Task ThamGiaNhanVien()
    {
        var nguoiDung = Context.User;
        if (nguoiDung?.Identity?.IsAuthenticated == true &&
            (nguoiDung.IsInRole(HangSoVaiTro.ThuNgan) || nguoiDung.IsInRole(HangSoVaiTro.QuanLy)))
            await Groups.AddToGroupAsync(Context.ConnectionId, "nhanvien");
    }

    public Task ThamGiaDon(string maTheoDoi) =>
        Context.GetHttpContext() is { } http && _ownership.Owns(http, maTheoDoi)
            ? Groups.AddToGroupAsync(Context.ConnectionId, "don-" + maTheoDoi)
            : Task.FromException(new HubException("Không có quyền theo dõi đơn này."));
}
