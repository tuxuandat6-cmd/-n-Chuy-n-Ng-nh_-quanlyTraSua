# Nhánh A khách tự đặt hàng

Cập nhật nghiệm thu bảo mật 10/11/2026: xem [kết quả mới nhất](minh-chung/bao-mat-hai-vai-tro/README.md), [báo cáo Word](Bao_cao_Nguyen_Minh_Duy_Nhanh_A.docx) và [sơ đồ chỉnh sửa được tái dựng](uml-tai-dung/README.md). C# 36 PASS + 1 SKIP, JS 13 PASS, HTTP/SQL 24 PASS và replay sau restart 1 PASS. Các mô tả lịch sử bên dưới về cookie/Hub được thay thế bằng bản nghiệm thu này: cookie đã được bảo vệ, HTTP/SignalR kiểm tra quyền sở hữu, checkout có phiếu chống replay. Hub có thay đổi tích hợp quyền của nhóm khách; phần nhân viên giữ nguyên.

Cập nhật 10/11/2026: [ảnh ứng dụng và ma trận 7 bước](minh-chung/README.md), [đặc tả 2 Use Case khách hàng](USE-CASE-KHACH-HANG.md). Bộ HTTP/SQL mới đạt 14/14; số liệu cũ trong tài liệu lịch sử vẫn giữ đúng mốc.

Phạm vi Nguyễn Minh Duy (`nguyenminhduy090-alt`): phần khách hàng của Issue #1, giao diện khách hàng #4 và trang chủ/menu/banner #7. Assignees đã được kiểm tra qua GitHub API. Không coi toàn bộ Issue #1 là công việc riêng của Duy.

## Nguồn và cách ghi nhận

ZIP `Do_An_Chuyen_Nganh.zip` được giải nén riêng để kiểm tra, không upload lại. Khi đồng bộ, phát hiện `origin/master` đã có mã nguồn gốc do Từ Xuân Đạt upload: `06ba0cd` và `575c15e`. Branch khách hàng kế thừa nguyên các commit đó, giữ tác giả và lịch sử, rồi nối commit gốc của `main` vào branch riêng để có chung tổ tiên cho PR. Không merge vào `main` hoặc `master`.

`nguon-ma.csv` ghi SHA-256 của 17 file khách hàng/dependency UI trong ZIP trước sửa, phục vụ đối chiếu nguồn, không phải danh sách file mới do Duy viết. Code trang chủ, menu, giỏ hàng, checkout, lịch sử, điểm và các layout đều có sẵn trong nền của nhóm trưởng. Không commit lại các file đó dưới tên Duy. Các commit mới sửa phần khách hàng trong controller/JS, thêm biên nhận và phiếu checkout, bảo vệ nhóm khách của Hub, cùng kiểm thử và tài liệu/sơ đồ. Admin, POS, SQL, cấu hình và báo cáo gốc chỉ xuất hiện qua lịch sử có sẵn của nhóm trưởng.

Vì `main` chỉ có README, diff PR vào `main` sẽ bao gồm cả nền mã nguồn từ `master`. Reviewer cần phân biệt hai commit gốc của Từ Xuân Đạt với các commit mới của Nhánh A; dùng `git diff 575c15e..HEAD` để xem riêng phần thay đổi so với nền. Không coi toàn bộ diff PR là đóng góp của Duy. Nếu nhóm trưởng đưa nền lên main trước, cần đồng bộ lại PR theo lịch sử chuẩn của nhóm.

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
- Phía nhân viên có `ThuNganController.CapNhatTrangThai` và phát `TrangThaiThayDoi` tới nhóm `don-{maTheoDoi}`. Khách tham gia bằng `DonHangHub.ThamGiaDon`, hoặc GET `LayTrangThai` mỗi 30 giây. Giữ nguyên controller thu ngân; chỉ bổ sung kiểm tra quyền sở hữu khi khách tham gia nhóm đơn trong Hub dùng chung.
- Điểm trong ZIP được cộng tại `DichVuKhuyenMai.GhiNhanThanhToanAsync`, do thao tác thu tiền gọi; không phải lúc chuyển sang Hoàn thành. Vì thế chưa đạt yêu cầu “chỉ tích điểm khi hoàn thành”. Cần nhóm phụ trách nghiệp vụ thống nhất thời điểm cộng điểm; Nhánh A không tự sửa dịch vụ hoặc POS để thay đổi chính sách.
- Báo cáo nói chỉ đánh giá sau hoàn tất; mã hiện có cho đánh giá từ Sẵn sàng hoặc Hoàn thành. Giữ nguyên hành vi có sẵn, ghi rõ khác biệt cho review.

## Các dependency còn thiếu trên branch

Nền ứng dụng hiện đã được kế thừa từ `master`, không còn thiếu các file dưới đây. Đây là dependency dùng chung của nhóm, giữ nguyên nghiệp vụ và tác giả; không phải đóng góp mới của Duy. ZIP còn được giữ cục bộ trong thư mục bỏ qua `.review-source/` để đối chiếu, không stage.

| Dependency nền được giữ nguyên | Vai trò |
| --- | --- |
| `MixueDatMon.csproj`, `Program.cs` | Nền khởi động và đăng ký EF, DI, session, antiforgery, routes, SignalR; cần bản chuẩn của nhóm |
| `MoHinh/ThucThe.cs`, `MoHinh/ViewModels.cs`, `DuLieu/DuLieuContext.cs` | Chứa cả thực thể, view model, cấu hình dữ liệu của nhiều thành viên; dùng nguyên bản chuẩn khi ghép |
| `DichVu/DichVuDatHang.cs`, `DichVuCaiDat.cs`, `DichVuKho.cs`, `DichVuKhuyenMai.cs`, `DichVuGioiHanTanSuat.cs`, `DichVuVietQr.cs` | Dịch vụ nghiệp vụ dùng chung; không nhận làm phần riêng |
| `HubThoiGianThuc/DonHangHub.cs`, `TroGiup/DinhDang.cs` | Hợp đồng sự kiện và định dạng dùng chung |
| `wwwroot/css/site.css`, thư viện SignalR, ảnh sản phẩm | CSS có cả phần nhân viên, giữ nguyên; ảnh uploads trong ZIP không đưa lên, nền Git chỉ có `.gitkeep`. Kiểm tra CDN SignalR và bố trí ảnh khi triển khai |
| Database và cấu hình kết nối/VietQR | Các file nền đã có trong commit nhóm trưởng; không sửa hoặc commit lại. Cần SQL Server, dữ liệu thử và cấu hình môi trường được nhóm xác nhận để chạy kiểm thử end-to-end |

Không sửa phần quản lý/thu ngân, không đóng Issue. PR cần nhóm trưởng review phạm vi nhập mã và các điểm tích hợp trước merge.
