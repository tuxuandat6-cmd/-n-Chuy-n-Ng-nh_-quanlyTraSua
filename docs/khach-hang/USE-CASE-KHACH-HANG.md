# Đặc tả ca sử dụng phía khách hàng

Phạm vi bổ sung cho phần khách hàng của Issue #1. Hình 21 và Hình 27 được trích từ báo cáo nhóm, giữ nguyên nguồn đã ghi trong README. Nội dung dưới đây là đối chiếu với code thực tế, không phải mô tả chức năng Admin/POS hay khẳng định Duy viết mã nền.

## UC-KH-01 Đặt món từ giỏ hàng

- Tác nhân: khách vãng lai, không cần tài khoản.
- Mục tiêu: lưu đơn và chi tiết món theo giá server, nhận liên kết theo dõi.
- Tiền điều kiện: quán nhận đơn; món/size/topping hợp lệ; giỏ có 1–40 dòng, mỗi dòng 1–20 sản phẩm; trình duyệt có antiforgery token.
- Kích hoạt: khách bấm “Gửi đơn cho quán”.
- Luồng chính: mở menu → chọn size/đá/đường/topping → thêm hoặc cập nhật giỏ → kiểm tra tổng → nhập thông tin tùy chọn → gửi yêu cầu → server kiểm tra món, giá, kho và mã giảm giá qua dịch vụ chung → lưu DonHang/ChiTietDonHang/ChiTietTopping → ghi mã vào cookie lịch sử → trả đường dẫn theo dõi → trình duyệt xóa giỏ khi xác nhận thành công.
- Luồng thay thế: giỏ trống không gửi; size/số lượng sai trả 400 không tạo đơn; lỗi kết nối hoặc server từ chối giữ giỏ; đổi giỏ hủy kết quả giảm giá cũ; gửi lặp trong cùng request đang chờ bị chặn ở JS.
- Hậu điều kiện thành công: một lượt thao tác browser được thử tạo đúng một đơn, mã trạng thái 1, chưa thanh toán; tổng 2 × (25.000 + 3.000) = 56.000đ được xác minh bằng SQL.
- Giới hạn: chưa có idempotency key lưu bền trên server, không khẳng định chống replay HTTP hoặc trùng đơn sau mất phản hồi. Cần owner lưu đơn (#11) thống nhất nếu bổ sung hợp đồng chống trùng.
- Thành phần: ThucDonController, Views/ThucDon/Index.cshtml, thucdon.js, DonHangController.DatHang; DichVuDatHang và DuLieuContext là dependency dùng chung được giữ nguyên.

## UC-KH-02 Tra cứu lịch sử và theo dõi đơn

- Tác nhân: khách có cookie lịch sử trên thiết bị hoặc biết mã liên kết theo dõi.
- Mục tiêu: xem chi tiết, tổng tiền, trạng thái và thanh toán của đơn.
- Tiền điều kiện: đơn đã được lưu; mã tồn tại.
- Luồng chính: khách mở “Đơn của tôi” → server đọc danh sách mã trong cookie → truy vấn đơn, món và topping → hiển thị tổng → khách mở đơn → tham gia nhóm SignalR don-{ma} → nhận trạng thái từ nhân viên; polling LayTrangThai dự phòng mỗi 30 giây.
- Luồng thay thế: thiết bị không cookie thấy lịch sử trống; mã không tồn tại trả 404; thư viện SignalR không tải được vẫn polling; mã trạng thái lạ không làm hỏng màn hình; đơn mang đi chưa thanh toán hiển thị yêu cầu thanh toán tại quầy.
- Hậu điều kiện: thao tác tra cứu không thay đổi trạng thái, không cộng điểm, không thực hiện thanh toán.
- Giới hạn quyền truy cập: cookie hiện là danh sách mã chưa ký, URL theo dõi là bearer link khách vãng lai. Người biết mã có thể xem đơn; không khẳng định có xác thực chủ đơn theo tài khoản. Cần nhóm thống nhất chính sách trước thay cơ chế này.
- Thành phần: DonHangController.DonCuaToi/TheoDoi/LayTrangThai, Views/DonHang, theodoi.js. Việc chuyển trạng thái và cộng điểm do bộ phận khác phụ trách.
