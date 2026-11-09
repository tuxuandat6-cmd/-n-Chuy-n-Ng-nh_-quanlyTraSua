# Kiểm thử Nhánh A

## Kết quả ngày 09 tháng 11 năm 2026

- `dotnet build MixueDatMon/MixueDatMon.csproj --nologo`: thành công, 0 lỗi, 1 cảnh báo CS8602 có sẵn tại `Views/DonHang/TheoDoi.cshtml:97`. Build bằng SDK 11 preview, target net8.0.
- `dotnet test MixueDatMon/MixueDatMon.Tests/MixueDatMon.Tests.csproj --nologo`: **29/29 PASS**, gồm 24 test có sẵn và 5 test tích hợp khách hàng. Môi trường dùng `DOTNET_ROLL_FORWARD=Major`; chưa xác nhận riêng trên runtime .NET 8 chuẩn.
- `node --test tests/customer-workflow.test.cjs`: **12/12 PASS**.
- `tests/customer-live-smoke.ps1`: **11/11 PASS**, HTTP thật tới ứng dụng và kiểm tra bản ghi SQL Server LocalDB thật, database riêng `DuyCustomerTest_20261109`. Kết quả: [customer-http-results.json](evidence/customer-http-results.json).
- `node --check` cho `thucdon.js`, `theodoi.js`: đạt.
- `git diff --check`: đạt.

## Phần kiểm tra mới

Kiểm thử JS chạy trực tiếp script thật trong môi trường DOM mô phỏng: khôi phục giỏ hỏng/sai kiểu, cộng giá topping, tăng/giảm với giới hạn 1–20, xóa món, gộp món giống nhau, tránh tùy chọn cũ trên món không phải đồ uống, giữ giỏ khi checkout thất bại và xóa giỏ/chuyển trang khi thành công. Theo dõi vẫn polling nếu thiếu SignalR, hiển thị hoàn thành/hủy và bỏ qua trạng thái không nhận diện.

Kiểm thử C# dùng EF InMemory: endpoint khách phản ánh trạng thái và thanh toán đã lưu, mã đơn không tồn tại trả 404, lịch sử chỉ trả mã trong cookie thiết bị, trang điểm đọc kết quả từ dịch vụ dùng chung sau thanh toán và không cộng lần hai trong tình huống đơn có 5 điểm. Không kiểm tra tính nguyên tử/chạy đồng thời của dịch vụ điểm.

Không sửa hoặc nhận các test có sẵn là đóng góp mới. Bổ sung trong lượt kiểm tra tiếp: 1 C# test hồi quy tổng tiền lịch sử, 4 JS test (giảm giá cũ, phản hồi giảm giá trễ, gửi giỏ trống và thao tác trong khi checkout), cùng script 11 kiểm tra HTTP/SQL. Các kết quả này chưa đủ để đóng toàn bộ Issues #1, #4, #7, #8 hoặc #18.

## Kiểm thử trình duyệt và SQL thật

Ứng dụng chạy loopback `127.0.0.1:5187`; chỉ dùng dữ liệu giả trong database LocalDB tách biệt. Không kết nối database của người dùng/nhóm, không sửa file SQL, cấu hình, Admin hoặc POS trong mã nguồn. Tắt thông tin ngân hàng bằng biến môi trường cho phiên thử. Database fixture được mở cả ngày và tắt tự hủy để kết quả ổn định.

| Tình huống đã chạy | Kết quả và bằng chứng |
| --- | --- |
| Trang chủ và menu đọc từ SQL; tìm không dấu | PASS; xem món thật từ 35 sản phẩm mẫu, tìm `tra sua tran chau duong den` |
| Size M, ít đá, không đường, 1 topping, 2 ly | PASS; trình duyệt hiển thị 56.000đ, tăng lên 3 ly là 84.000đ, giảm về 2 ly; reload giữ giỏ |
| Checkout qua trình duyệt | PASS; đơn giả `MX261109-KWMJ`, số lượng 2, đơn giá 25.000, topping 3.000, size 1, đá 2, đường 3; SQL view tổng 56.000. [Ảnh](evidence/browser-order-saved.jpg) |
| Áp MIXUE10 ở 50.000 rồi giảm còn 25.000 | FAIL trước sửa: còn giảm 5.000, hiển thị 20.000; PASS sau sửa: 25.000, yêu cầu áp mã lại. [Trước](evidence/browser-discount-before-fix.jpg), [sau](evidence/browser-discount-after-fix.jpg) |
| Lịch sử đơn có topping | FAIL trước sửa: hiển thị 50.000 thay vì 56.000; test C# tái hiện Expected 56000 / Actual 50000. PASS sau khi Include topping: lịch sử 56.000. [Ảnh sau sửa](evidence/browser-order-history.jpg) |
| Xóa dòng cuối | PASS; tổng 0đ, nút gửi đơn bị vô hiệu hóa |
| Theo dõi không có thư viện SignalR | PASS; proxy thử bỏ script CDN SignalR, trang tự cập nhật từ chờ sang Hoàn thành/Đã thanh toán bằng polling. [Ảnh](evidence/browser-polling-without-signalr.jpg) |

Trong bài polling, trạng thái hoàn thành được mô phỏng trực tiếp trên đúng đơn giả bằng SQL (kèm phương thức thanh toán để thỏa ràng buộc). Không gọi thao tác POS, không kiểm thử thanh toán hoặc cộng điểm thực tế, không coi đó là bằng chứng toàn bộ luồng nhân viên đã đạt.

Một lần build khi server thử còn chạy bị khóa apphost (MSB3027/MSB3021); đã dừng server rồi build lại thành công. Một lần thao tác fixture thiếu phương thức thanh toán bị CHECK constraint chặn; sau đó fixture hợp lệ được dùng. Các lỗi chuẩn bị môi trường không được ghi nhận thành PASS chức năng.

## Cách chạy lại

Tại thư mục gốc, chạy `tests/setup-customer-localdb.ps1 -Database DuyCustomerTest_<ten_moi>`. Script đã chạy thành công với `DuyCustomerTest_20261109_Repro`, chỉ tạo DB mới, từ chối ghi đè DB tồn tại. Nó dùng nguyên các script nền trong `MixueDatMon/Database/`, chỉ thay tên DB trong bản sao tạm ở `.review-source/`.

Chạy ứng dụng với `ConnectionStrings__MacDinh` trỏ tới DB fixture, `NganHang__MaBin` và `NganHang__SoTaiKhoan` rỗng, môi trường Development, địa chỉ `http://127.0.0.1:5187`. Chạy `tests/customer-live-smoke.ps1 -Database <ten_DB_fixture>`; script tạo đơn giả, kiểm tra SQL, không xóa đơn/DB. Giá fixture: món 1/size 1 = 25.000, topping 1 = 3.000. Chạy `node tests/customer-no-signalr-proxy.cjs` rồi mở trang theo dõi ở cổng 5188 để thử polling. Dừng server trước build trên Windows để tránh khóa apphost.

## Quyền xem đơn và giới hạn chưa kiểm thử

PASS: phiên đặt hàng thấy đơn trong lịch sử; phiên mới không cookie không liệt kê đơn đó; mã không tồn tại trả 404; POST thiếu antiforgery trả 400. Đây không phải xác thực tài khoản: cookie lịch sử hiện chứa danh sách mã không ký và liên kết theo dõi là bearer link. Người biết mã vẫn có thể truy cập đơn theo thiết kế khách vãng lai. Cần nhóm thống nhất yêu cầu bảo mật trước khi thay cơ chế này; không khẳng định đã có phân quyền sở hữu tài khoản.

- **NOT_RUN:** SignalR/WebSocket hai máy qua mạng, thanh toán ngân hàng, chính sách tích điểm sau hoàn thành, kiểm thử tải/concurrency SQL và runtime .NET 8 chuẩn.
- **BLOCKED bởi quyết định nhóm:** thay chính sách điểm/giao hàng, thống nhất bảo vệ liên kết và cookie khách vãng lai, đưa nền master vào main để tách diff PR.
- Không có ca chức năng tự động còn FAIL trong lần chạy cuối; không đồng nghĩa đã hoàn tất toàn bộ kiểm thử triển khai.

## Các điểm nhóm trưởng cần review

1. Nền mã nguồn đã nằm trên `master` trong hai commit của Từ Xuân Đạt. PR vào `main` còn hiển thị nền đó vì main chưa có ứng dụng; không tính nó vào đóng góp mới của Duy.
2. Điểm đang cộng khi thu tiền, chưa phải khi đơn Hoàn thành. Cần người phụ trách dịch vụ và POS thống nhất thay đổi, Nhánh A chỉ kiểm tra tích hợp và đọc điểm.
3. Mã hiện có hỗ trợ tại bàn/mang đi; chưa có giao hàng địa chỉ như một số mục UI trong Issue #4. Luồng trạng thái thực tế là Chờ xác nhận → Đang làm → Sẵn sàng → Hoàn thành/Đã hủy.
4. Sơ đồ Hình 21 và Hình 27 được trích từ DOCX, chưa có file thiết kế gốc. Những khác biệt sơ đồ/mã nguồn được ghi trong README.
5. Đã kiểm thử SQL LocalDB và trình duyệt với dữ liệu giả. Còn cần môi trường triển khai chuẩn, asset ảnh và thử tích hợp nhân viên/khách qua mạng. Các cấu hình/SQL hiện có là file nền của nhóm, không phải file mới do Nhánh A đưa vào.
