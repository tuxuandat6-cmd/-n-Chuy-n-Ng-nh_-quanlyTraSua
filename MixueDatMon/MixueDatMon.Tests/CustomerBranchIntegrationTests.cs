using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MixueDatMon.Controllers;
using MixueDatMon.DichVu;
using MixueDatMon.DuLieu;
using MixueDatMon.MoHinh;
using MixueDatMon.MoHinh.VM;
using Xunit;

namespace MixueDatMon.Tests;

// Checks the customer-facing integration contract, without invoking staff actions or a live DB.
public class CustomerBranchIntegrationTests
{
    private static DonHangController CustomerController(DuLieuContext db) =>
        new(db, null!, null!, null!, null!, new DichVuKhuyenMai(db), null!)
        { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() } };

    [Fact]
    public async Task TrackingEndpoint_ReflectsPersistedCompletionAndPayment()
    {
        using var db = HoTroKiemThu.TaoContext();
        var order = new DonHang { MaTheoDoi = "TEST-CUSTOMER-01", MaTrangThai = HangSoTrangThai.ChoXacNhan };
        db.DonHang.Add(order);
        await db.SaveChangesAsync();
        // Simulate the persisted result of staff processing; do not modify staff logic.
        order.MaTrangThai = HangSoTrangThai.HoanThanh;
        order.NgayThanhToan = DateTime.Now;
        await db.SaveChangesAsync();
        var result = Assert.IsType<JsonResult>(await CustomerController(db).LayTrangThai(order.MaTheoDoi));
        var payload = JsonSerializer.SerializeToElement(result.Value);
        Assert.Equal(HangSoTrangThai.HoanThanh, payload.GetProperty("maTrangThai").GetInt32());
        Assert.True(payload.GetProperty("daThanhToan").GetBoolean());
    }

    [Fact]
    public async Task TrackingEndpoint_UnknownOrderReturnsNotFound()
    {
        using var db = HoTroKiemThu.TaoContext();
        Assert.IsType<NotFoundResult>(await CustomerController(db).LayTrangThai("NONEXISTENT"));
    }

    [Fact]
    public async Task CustomerHistory_OnlyReturnsCodesInThisBrowsersCookie()
    {
        using var db = HoTroKiemThu.TaoContext();
        db.DonHang.AddRange(new DonHang { MaTheoDoi = "DEVICE-A" }, new DonHang { MaTheoDoi = "DEVICE-B" });
        await db.SaveChangesAsync();
        var controller = CustomerController(db);
        controller.Request.Headers.Cookie = "mx_don_cua_toi=DEVICE-A";
        var result = Assert.IsType<ViewResult>(await controller.DonCuaToi());
        var orders = Assert.IsType<List<DonHang>>(result.Model);
        Assert.Equal("DEVICE-A", Assert.Single(orders).MaTheoDoi);
    }

    [Fact]
    public async Task PointsPage_ReadsSharedServicePointsWithoutDoubleCredit()
    {
        using var db = HoTroKiemThu.TaoContext();
        var order = new DonHang
        {
            MaTheoDoi = "TEST-POINTS-01", SoDienThoaiKhach = "0900000000", TenKhach = "Test customer",
            MaTrangThai = HangSoTrangThai.HoanThanh, NgayThanhToan = DateTime.Now,
            DanhSachChiTiet = new List<ChiTietDonHang> { new() { DonGia = 50000m, SoLuong = 1 } }
        };
        db.DonHang.Add(order);
        await db.SaveChangesAsync();
        var sharedService = new DichVuKhuyenMai(db);
        await sharedService.GhiNhanThanhToanAsync(order);
        await sharedService.GhiNhanThanhToanAsync(order);
        var result = Assert.IsType<ViewResult>(await CustomerController(db).DiemCuaToi("+84900000000"));
        var points = Assert.IsType<DiemCuaToiVM>(result.Model);
        Assert.Equal(5, points.DiemTichLuy);
        Assert.Equal(50000m, points.TongChiTieu);
    }
}
