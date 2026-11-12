# Kiểm tra GitHub trực tiếp ngày 09 tháng 11 năm 2026

Đọc GitHub REST API và `git fetch origin --prune` lúc 09:14 giờ Việt Nam (02:14 UTC), trước các commit sửa bổ sung của lượt này. Không dùng lại ảnh chụp trạng thái cũ. Đã đọc toàn bộ danh sách trả về (4 branch, 1 PR, 19 Issue thực, không có trang kết quả tiếp theo).

| Branch | SHA tại lúc kiểm tra |
| --- | --- |
| main | `dd9db4db957ce6a5066c9c709d1c67b9e2ea3ac2` |
| master | `575c15ee8781bf1ab513499adc6997e9cbee0fc8` |
| feature/nguyen-minh-duy-khach-hang-online | `bec2a53d2ab242b4acefe2521b7ed84632d4050a` |
| feature/tvchinh-scope-from-completed-project | `b9d5fa5ea661b5dfd6a2048290c0872f4e525542` |

[PR #17](https://github.com/tuxuandat6-cmd/-n-Chuy-n-Ng-nh_-quanlyTraSua/pull/17): open, chưa merge; mergeable=true, mergeable_state=clean; 0 review, 0 comment thảo luận, 0 inline comment. Người được yêu cầu review vẫn là `tuxuandat6-cmd`. GitHub Actions: 0 runs; không coi việc chưa có CI là CI PASS.

Không có commit mới trên main/master/branch Duy so với lượt push trước. Nhánh Chính có commit `b9d5fa5` nhập phạm vi nhân viên/thanh toán/điểm/SQL. So sánh các file giao tiếp `DichVuKhuyenMai.cs`, `ThuNganController.cs`, `Views/DonHang/DiemCuaToi.cshtml` với master: nội dung giống nền, chưa thấy thay đổi chức năng tích hợp ở các file này; không nhập nhánh đó vào branch Duy.

Issues #1, #4, #7 vẫn open, giao Duy, không có comment; thời điểm cập nhật lần lượt 07:29:04, 07:34:42, 08:15:12 cùng ngày. Không thấy nội dung mới so với phần đã đối chiếu. Các Issue Admin của Đạt #3, #6, #12, #13, #15, #21 hiện closed; trạng thái closed không tự chứng minh có thay đổi code mới.

## Phân công mới cần phối hợp

- [#18](https://github.com/tuxuandat6-cmd/-n-Chuy-n-Ng-nh_-quanlyTraSua/issues/18), tạo đơn/trạng thái/tra cứu, cập nhật 08:49:10, giao Duy. Nội dung còn mô tả enum 0–3, Session và Đang giao khác mã hiện có (1–5, localStorage, tại bàn/mang đi). Không tạo model/controller trùng để chạy theo tên ví dụ.
- [#19](https://github.com/tuxuandat6-cmd/-n-Chuy-n-Ng-nh_-quanlyTraSua/issues/19), phần mở đầu/tổng quan/khảo sát báo cáo, cập nhật 08:51:31, giao Duy. Ngoài trọng tâm Nhánh A và Chương IV của yêu cầu này; chưa tự nhận đã hoàn thành tài liệu toàn nhóm.
- #8 giỏ hàng giao Duy nhưng mô tả Session; ghi nhận khác biệt với localStorage hiện có.
- #11 tạo đơn từ giỏ/lưu DB và #16 tầng dữ liệu khách hàng hiện giao Chính. Giữ nguyên dịch vụ và model dùng chung; sửa trong lượt này chỉ là phần đọc lịch sử của khách trong DonHangController, giỏ JS và kiểm thử. Cần nhóm trưởng xác nhận ranh giới #11/#18 trước đổi nghiệp vụ lưu đơn hoặc trạng thái.

## Phạm vi thực tế PR

PR #17 vào main vẫn chứa nền ứng dụng, Admin/POS/SQL và báo cáo gốc thông qua hai commit của Từ Xuân Đạt `06ba0cd` và `575c15e`. Không thể gọi toàn bộ diff PR là phần Duy. Các file ngoài Nhánh A đó không bị sửa trong commit mới. Đối chiếu riêng đóng góp so với nền bằng `git diff 575c15e..HEAD`; lịch sử nối main được giữ để push thường, không force push hoặc sửa tác giả.

Nhóm trưởng cần thống nhất cách đưa lịch sử nền vào main. Nhánh Duy chỉ được tiếp tục commit các lỗi khách hàng có bằng chứng và tài liệu kiểm thử; không tự đổi base PR, merge PR hoặc đưa mã nhóm trưởng thành commit của Duy.
