# Nhánh A khách tự đặt hàng

Phạm vi Nguyễn Minh Duy (`nguyenminhduy090-alt`): phần khách hàng của Issue #1, giao diện khách hàng #4 và trang chủ/menu/banner #7. Assignees đã được kiểm tra qua GitHub API. Không coi toàn bộ Issue #1 là công việc riêng của Duy.

## Nguồn và cách ghi nhận

Mã được nhập có chọn lọc từ `Do_An_Chuyen_Nganh.zip` do người dùng cung cấp, giữ cấu trúc `MixueDatMon/`. Đây là mã có sẵn của dự án nhóm; ZIP không có lịch sử chứng minh ai viết từng dòng. Các commit `chore(customer): import ...` chỉ ghi nhận việc tách/nhập mã theo phạm vi được giao, không khẳng định Duy vừa viết các chức năng đó. Các sửa lỗi mới được ghi trong commit riêng.

`nguon-ma.csv` ghi SHA-256 của các file gốc trước sửa. Không nhập Admin, POS, SQL, cấu hình kết nối, dữ liệu khách hàng, ảnh uploads, DLL, bin/obj/.vs hoặc toàn bộ báo cáo. Những file shared được nhập chỉ là layout, partial và chỉ dẫn Razor phục vụ giao diện khách hàng; vẫn ghi nhận là dependency có sẵn của nhóm.

## Sơ đồ từ báo cáo

Hai ảnh được trích nguyên byte từ `bao_cao_ck.docx`, không vẽ lại:

- [Hình 21 Use Case khách hàng](hinh-21-use-case-khach-hang.png): `word/media/image22.png`, relationship `rId25`, mục 1.1, chương có tiêu đề “CHƯƠNG IV: PHÂN TÍCH VÀ THIẾT KẾ CSDL”.
- [Hình 27 Activity đặt món](hinh-27-activity-dat-mon.png): `word/media/image28.png`, relationship `rId31`, mục “Đặt món”.

Báo cáo dùng trùng số Chương IV cho phần kết luận; ở đây lấy phần phân tích, không lấy chương kết luận. Ảnh Hình 21 gốc có một số nhãn/đường nối chồng nhau; giữ nguyên và không nhận là sơ đồ vừa thiết kế. Chưa có file Draw.io/StarUML trong ZIP, vì vậy chưa đáp ứng đầy đủ yêu cầu file thiết kế gốc của Issue #1.

## Quy trình và mã tương ứng

| Bước khách hàng | Thành phần thực tế |
| --- | --- |
| Xem banner, nhóm món, món nổi bật | `Controllers/TrangChuController.cs`, `Views/TrangChu/Index.cshtml`, `TrangChuVM` |
| Xem menu, tìm món, chọn size/đá/đường/topping | `Controllers/ThucDonController.cs`, `Views/ThucDon/Index.cshtml`, `ThucDonVM` |
| Thêm, tăng/giảm, xóa, tính tổng giỏ | `wwwroot/js/thucdon.js`; giỏ lưu localStorage `mx_gio_hang_v1`, không có GioHangController riêng |
| Checkout | `DonHangController.DatHang`, POST `/DonHang/DatHang`, `YeuCauDatHang` → `DichVuDatHang.DungDonAsync` → `DuLieuContext.SaveChangesAsync` |
| Lịch sử trên thiết bị | `DonHangController.DonCuaToi`, cookie `mx_don_cua_toi`, `Views/DonHang/DonCuaToi.cshtml` |
| Theo dõi trạng thái | `DonHangController.TheoDoi/LayTrangThai`, `Views/DonHang/TheoDoi.cshtml`, `wwwroot/js/theodoi.js` |
| Xem điểm | `DonHangController.DiemCuaToi`, `Views/DonHang/DiemCuaToi.cshtml`, dịch vụ dùng chung `DichVuKhuyenMai.LayDiemAsync` |

Khách không cần đăng nhập. Quét QR hợp lệ ghi bàn vào session; không có bàn thì đặt mang đi, cần thanh toán tại quầy trước khi pha chế. Đây là hành vi hiện có của ZIP, chưa phải giao hàng đến địa chỉ. Không khẳng định đã có giao hàng online đầy đủ theo mọi mục của Issue #4.

Luồng đặt hàng: xem menu → tùy chọn món → thêm/cập nhật giỏ → kiểm tra tổng và mã giảm giá → gửi thông tin khách → server xác minh giá/kho, lưu đơn → chuyển đến trang theo dõi. Giá ở trình duyệt chỉ là tạm tính; server dùng dữ liệu gốc qua dịch vụ dùng chung.

## Tích hợp và ranh giới

- ZIP thực tế dùng mã trạng thái 1 Chờ xác nhận → 2 Đang làm → 3 Sẵn sàng → 4 Hoàn thành; 5 Đã hủy. Không đổi sang chuỗi trạng thái “Đang giao” của mô tả phân công khi hệ thống chưa hỗ trợ giao hàng.
- Phía nhân viên có `ThuNganController.CapNhatTrangThai` và phát `TrangThaiThayDoi` tới nhóm `don-{maTheoDoi}`. Khách tham gia bằng `DonHangHub.ThamGiaDon`, hoặc GET `LayTrangThai` mỗi 30 giây. Chỉ đọc đối chiếu; không sửa/nhập controller thu ngân hoặc Hub dùng chung.
- Điểm trong ZIP được cộng tại `DichVuKhuyenMai.GhiNhanThanhToanAsync`, do thao tác thu tiền gọi; không phải lúc chuyển sang Hoàn thành. Vì thế chưa đạt yêu cầu “chỉ tích điểm khi hoàn thành”. Cần nhóm phụ trách nghiệp vụ thống nhất thời điểm cộng điểm; Nhánh A không tự sửa dịch vụ hoặc POS để thay đổi chính sách.
- Báo cáo nói chỉ đánh giá sau hoàn tất; mã hiện có cho đánh giá từ Sẵn sàng hoặc Hoàn thành. Giữ nguyên hành vi có sẵn, ghi rõ khác biệt cho review.

## Các dependency còn thiếu trên branch

Đây là phần mã khách hàng để ghép vào dự án gốc, chưa phải ứng dụng độc lập. Không tạo project thay thế hoặc model trùng tên để che thiếu dependency.

| Dependency có trong ZIP, chưa nhập | Lý do/cách ghép |
| --- | --- |
| `MixueDatMon.csproj`, `Program.cs` | Nền khởi động và đăng ký EF, DI, session, antiforgery, routes, SignalR; cần bản chuẩn của nhóm |
| `MoHinh/ThucThe.cs`, `MoHinh/ViewModels.cs`, `DuLieu/DuLieuContext.cs` | Chứa cả thực thể, view model, cấu hình dữ liệu của nhiều thành viên; dùng nguyên bản chuẩn khi ghép |
| `DichVu/DichVuDatHang.cs`, `DichVuCaiDat.cs`, `DichVuKho.cs`, `DichVuKhuyenMai.cs`, `DichVuGioiHanTanSuat.cs`, `DichVuVietQr.cs` | Dịch vụ nghiệp vụ dùng chung; không nhận làm phần riêng |
| `HubThoiGianThuc/DonHangHub.cs`, `TroGiup/DinhDang.cs` | Hợp đồng sự kiện và định dạng dùng chung |
| `wwwroot/css/site.css`, thư viện SignalR, ảnh sản phẩm | CSS có cả phần nhân viên; cần ghép asset gốc/thư viện chuẩn, không nhập toàn bộ CSS hoặc uploads |
| Database và cấu hình kết nối/VietQR | Do nhóm cung cấp bằng cấu hình môi trường an toàn; không commit SQL hoặc thông tin thật |

Không sửa phần quản lý/thu ngân, không đóng Issue. PR cần nhóm trưởng review phạm vi nhập mã và các điểm tích hợp trước merge.
