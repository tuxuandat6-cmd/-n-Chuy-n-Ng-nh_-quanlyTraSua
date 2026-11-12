# Nghiệm thu khách hàng ngày 10 tháng 11 năm 2026

Mã được kiểm thử: `1df86e6` trên branch Nguyễn Minh Duy. Build, C#, JavaScript, HTTP/SQL và restart được chạy lại sau commit; ảnh hai vai trò được chụp trên cùng mã ứng dụng trước commit. Chỉ sử dụng database `DuyCustomerTest_20261110`, tài khoản mẫu và khách giả lập. Không dùng đơn hay thanh toán thật.

| Kiểm tra | Kết quả | Bằng chứng |
| --- | --- | --- |
| Build | PASS | security-build.txt |
| C# | 36 PASS, 1 SKIP, 0 FAIL | security-csharp-final.txt |
| JavaScript | 13 PASS | security-javascript.txt |
| HTTP và SQL Server thật | 24 PASS | security-replay-concurrent-final.json |
| Replay sau restart tiến trình | 1 PASS | restart-replay-results.json |
| Hai vai trò trình duyệt | PASS | 7 ảnh trong thư mục này |
| Gọi lặp dịch vụ điểm cho đơn 0 điểm | FAIL chẩn đoán; BLOCKED xử lý chung | points-zero-replay-red.txt |
| HTTPS, nhiều node, ngân hàng thật, giao hàng địa chỉ | NOT_RUN | Ngoài môi trường xác minh |

## Hai phiên độc lập

Khách dùng `127.0.0.1:5187`, nhân viên dùng `localhost:5187`, tách cookie đăng nhập. Khách tự gửi đơn giả lập; nhân viên dùng chức năng có sẵn để thu tiền mặt thử nghiệm, nhận pha chế, báo sẵn sàng và giao khách. Không dùng SQL để giả lập chuyển trạng thái ở lượt này. Nhân viên đã đăng xuất sau kiểm tra.

Ảnh `khach-cho-xac-nhan`, `khach-dang-pha-che`, `khach-san-sang-signalr`, `khach-hoan-thanh-signalr` ghi nhận chuỗi Chờ xác nhận → Đang pha chế → Sẵn sàng → Hoàn thành. Ảnh `nhan-vien-da-xu-ly` là phiên nhân viên. Phiên khách thứ hai qua proxy loopback `5188` chặn script SignalR vẫn nhận Sẵn sàng và Hoàn thành nhờ polling, có hai ảnh tương ứng. Đã quan sát thuộc tính DOM `data-nguon-cap-nhat` lần lượt là `signalr`/`polling`; không kết luận nguồn cập nhật chỉ từ ảnh.

## Quyền sở hữu và chống trùng

Cookie lịch sử được ký/mã hóa bằng ASP.NET Data Protection, HttpOnly, SameSite Lax, hạn 30 ngày và Secure khi HTTPS. HTTP theo dõi/trạng thái/QR/đánh giá cùng nhóm SignalR kiểm tra quyền sở hữu. Cookie cũ chưa ký không được tự động nâng quyền; sau nâng cấp không dùng lại lịch sử cookie cũ. Mất cookie/thiết bị chưa có luồng khôi phục. Response khách không được cache.

Checkout chuẩn bị phiếu hạn 24 giờ gắn nội dung request, giữ phiếu khi mất phản hồi. Request gửi lại dùng cùng phiếu; khóa UPDLOCK/HOLDLOCK trong giao dịch Serializable và unique index MaTheoDoi có sẵn bảo đảm một đơn/một chi tiết trong phép thử đồng thời và restart. Không đổi schema. Phiếu khác là một lần đặt mới. Khi triển khai cần lưu bền và chia sẻ Data Protection key ring giữa các instance. Hết hạn/mất key trả lỗi, giữ giỏ; không tự phát phiếu mới để che lỗi mất phản hồi. Chưa thử mất key hay nhiều node.

## Điểm và ranh giới thành viên

Dịch vụ nền cộng điểm lúc thu tiền, khác phân công lúc hoàn thành. Ca có điểm dương kiểm tra gọi lặp thành công; ca đơn 5.000đ nhận 0 điểm gọi trực tiếp dịch vụ hai lần cho chi tiêu Expected 5.000 / Actual 10.000 (FAIL thực tế). Chưa tái hiện qua endpoint POS, nơi có guard thanh toán riêng. Raw RED được giữ; regression tương ứng SKIP với lý do BLOCKED_SHARED, không tính là PASS. Nhóm trưởng/owner cần chốt thời điểm cộng điểm và guard ghi nhận một lần cả khi 0 điểm. Không sửa dịch vụ điểm, POS, Admin, database hoặc Class Diagram.

Thay đổi Hub chỉ kiểm tra quyền tham gia nhóm đơn của khách; xử lý nhóm nhân viên giữ nguyên. Mã nền của Từ Xuân Đạt được kế thừa, không nhận là mã mới của Duy.

## Báo cáo và Issue

Word bổ sung 8 trang khách hàng vào báo cáo nhóm; 93 trang gốc có kết quả render giống bản gốc và 75 phần OOXML khác được giữ nguyên byte. Xem `report-layout-qa.json`. Máy thiếu LibreOffice nên sau khi chẩn đoán renderer đã dùng Microsoft Word 16 xuất PDF nội bộ và PDFium so sánh/rà soát ảnh. Không đưa PDF nội bộ vào bàn giao.

Issues #1/#4/#7/#8/#18/#19 đều còn mở. #7 có bằng chứng đầy đủ trong kiến trúc hiện tại; #1 chỉ phần khách và sơ đồ tái dựng; #4/#8/#18 còn khác đặc tả Session/địa chỉ/giao hàng/enum so với nền thực tế. #19 đã có Word phần Duy, chưa xác nhận hoàn thành toàn bộ tổng quan/khảo sát nhóm. Giữ các checklist chưa đủ căn cứ, không đóng Issue. Các số liệu cũ trong tài liệu lịch sử giữ nguyên mốc và được thay thế bằng kết quả tại đây.
