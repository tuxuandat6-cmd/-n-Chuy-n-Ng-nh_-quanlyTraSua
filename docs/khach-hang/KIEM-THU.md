# Kiểm thử Nhánh A

## Kết quả ngày 09 tháng 11 năm 2026

- `dotnet build MixueDatMon/MixueDatMon.csproj --nologo`: thành công, 0 lỗi, 1 cảnh báo CS8602 có sẵn tại `Views/DonHang/TheoDoi.cshtml:97`. Build bằng SDK 11 preview, target net8.0.
- `dotnet test MixueDatMon/MixueDatMon.Tests/MixueDatMon.Tests.csproj --nologo`: 28/28 đạt, gồm 24 test có sẵn và 4 test tích hợp khách hàng mới. Môi trường dùng `DOTNET_ROLL_FORWARD=Major`; chưa xác nhận riêng trên runtime .NET 8 chuẩn.
- `node --test tests/customer-workflow.test.cjs`: 8/8 đạt.
- `node --check` cho `thucdon.js`, `theodoi.js`: đạt.
- `git diff --check`: đạt.

## Phần kiểm tra mới

Kiểm thử JS chạy trực tiếp script thật trong môi trường DOM mô phỏng: khôi phục giỏ hỏng/sai kiểu, cộng giá topping, tăng/giảm với giới hạn 1–20, xóa món, gộp món giống nhau, tránh tùy chọn cũ trên món không phải đồ uống, giữ giỏ khi checkout thất bại và xóa giỏ/chuyển trang khi thành công. Theo dõi vẫn polling nếu thiếu SignalR, hiển thị hoàn thành/hủy và bỏ qua trạng thái không nhận diện.

Kiểm thử C# dùng EF InMemory: endpoint khách phản ánh trạng thái và thanh toán đã lưu, mã đơn không tồn tại trả 404, lịch sử chỉ trả mã trong cookie thiết bị, trang điểm đọc kết quả từ dịch vụ dùng chung sau thanh toán và không cộng lần hai trong tình huống đơn có 5 điểm. Không kiểm tra tính nguyên tử/chạy đồng thời của dịch vụ điểm.

Không sửa hoặc nhận các test có sẵn là đóng góp mới. Không chạy thực tế thao tác Admin/POS, không kết nối database thật, không xác nhận giao dịch SQL, SignalR qua mạng, UI trên trình duyệt, CDN hoặc thanh toán ngân hàng end-to-end. Các kết quả trên không đủ để đóng toàn bộ Issues #1, #4 hoặc #7.

## Các điểm nhóm trưởng cần review

1. Nền mã nguồn đã nằm trên `master` trong hai commit của Từ Xuân Đạt. PR vào `main` còn hiển thị nền đó vì main chưa có ứng dụng; không tính nó vào đóng góp mới của Duy.
2. Điểm đang cộng khi thu tiền, chưa phải khi đơn Hoàn thành. Cần người phụ trách dịch vụ và POS thống nhất thay đổi, Nhánh A chỉ kiểm tra tích hợp và đọc điểm.
3. Mã hiện có hỗ trợ tại bàn/mang đi; chưa có giao hàng địa chỉ như một số mục UI trong Issue #4. Luồng trạng thái thực tế là Chờ xác nhận → Đang làm → Sẵn sàng → Hoàn thành/Đã hủy.
4. Sơ đồ Hình 21 và Hình 27 được trích từ DOCX, chưa có file thiết kế gốc. Những khác biệt sơ đồ/mã nguồn được ghi trong README.
5. Cần SQL Server thử nghiệm, cấu hình môi trường, asset ảnh và kiểm thử trình duyệt trước xác nhận triển khai. Các cấu hình/SQL hiện có là file nền của nhóm, không phải file mới do Nhánh A đưa vào.
