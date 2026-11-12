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
- Chống trùng: chuẩn bị phiếu checkout có bảo vệ, hạn 24 giờ và gắn nội dung; gửi cùng phiếu qua header Checkout-Token. SQL sử dụng mã duy nhất có sẵn và khóa giao dịch để replay/request đồng thời trả cùng đơn. Trình duyệt giữ phiếu khi mất phản hồi. Đã thử lại sau restart, đúng một đơn/một dòng SQL. Hết hạn/mất key ring từ chối, giữ giỏ; chưa xác minh nhiều node.
- Thành phần: ThucDonController, Views/ThucDon/Index.cshtml, thucdon.js, DonHangController.DatHang; DichVuDatHang và DuLieuContext là dependency dùng chung được giữ nguyên.

## UC-KH-02 Tra cứu lịch sử và theo dõi đơn

- Tác nhân: khách có cookie biên nhận sở hữu hợp lệ trên thiết bị đặt đơn.
- Mục tiêu: xem chi tiết, tổng tiền, trạng thái và thanh toán của đơn.
- Tiền điều kiện: đơn đã được lưu; mã tồn tại và thuộc biên nhận được server bảo vệ, chưa hết hạn.
- Luồng chính: khách mở “Đơn của tôi” → server đọc danh sách mã trong cookie → truy vấn đơn, món và topping → hiển thị tổng → khách mở đơn → tham gia nhóm SignalR don-{ma} → nhận trạng thái từ nhân viên; polling LayTrangThai dự phòng mỗi 30 giây.
- Luồng thay thế: thiết bị không cookie thấy lịch sử trống; mã không tồn tại trả 404; thư viện SignalR không tải được vẫn polling; mã trạng thái lạ không làm hỏng màn hình; đơn mang đi chưa thanh toán hiển thị yêu cầu thanh toán tại quầy.
- Hậu điều kiện: thao tác tra cứu không thay đổi trạng thái, không cộng điểm, không thực hiện thanh toán.
- Quyền truy cập: biết mã chưa đủ. HTTP theo dõi/trạng thái/QR/đánh giá và nhóm SignalR yêu cầu biên nhận Data Protection, HttpOnly, hạn 30 ngày. Cookie cũ chưa ký bị từ chối. Chưa có khôi phục quyền khi mất thiết bị/cookie hoặc thu hồi từng biên nhận; cần nhóm trưởng thống nhất nếu bổ sung.
- Thành phần: DonHangController.DonCuaToi/TheoDoi/LayTrangThai, Views/DonHang, theodoi.js. Việc chuyển trạng thái và cộng điểm do bộ phận khác phụ trách.
