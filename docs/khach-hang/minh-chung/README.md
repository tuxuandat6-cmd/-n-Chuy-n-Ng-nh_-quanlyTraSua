# Minh chứng Nhánh A ngày 10 tháng 11 năm 2026

Ảnh trong thư mục này chụp trực tiếp ứng dụng đang chạy trên 127.0.0.1:5187 với SQL LocalDB riêng `DuyCustomerTest_20261110`; không phải ảnh minh họa được tạo và không dùng dữ liệu thật của nhóm. Mã nguồn kiểm tra trước commit evidence: `bff5c5b94fade4b50b5f4296dd2ebf662f5f16ee`. Không sửa code nghiệp vụ trong lượt này; bổ sung 3 kiểm tra HTTP/SQL.

| Bước Nhánh A | Trước lượt này | Kết quả/bằng chứng |
| --- | --- | --- |
| Trang chủ/banner/menu | PASS 09/11, thiếu ảnh riêng cho Issue | PASS kiểm lại; [trang chủ/banner/sản phẩm](trang-chu-banner-san-pham-20261110.jpg), [menu](menu-san-pham-20261110.jpg) |
| Thêm giỏ | PASS | PASS; [tùy chọn món](tuy-chon-mon-20261110.jpg) và [giỏ/checkout](gio-hang-checkout-20261110.jpg) |
| Cập nhật/tổng tiền/khôi phục | PASS qua tests và browser 09/11 | PASS kiểm thử JS 12/12; browser mới chọn 2 ly M, ít đá, không đường, topping 3.000, tổng 56.000 |
| Đặt/lưu đơn | PASS 09/11 | PASS HTTP/SQL 14/14; [theo dõi sau checkout](dat-hang-theo-doi-20261110.jpg); SQL xác nhận đúng một đơn tên giả DUY_EVIDENCE_20261110, tổng 56.000 |
| Lịch sử/theo dõi | PASS sau sửa lỗi topping | PASS kiểm lại; [lịch sử](lich-su-don-20261110.jpg) |
| Tích hợp trạng thái nhân viên | PASS phía khách với trạng thái giả lập 09/11 | NOT_RUN lại hai vai trò/máy ngày 10/11; giữ [evidence polling trước](../evidence/browser-polling-without-signalr.jpg), không biến thành kiểm thử POS mới |
| Điểm khi hoàn thành | BLOCKED khác chính sách | BLOCKED; dịch vụ chung cộng khi thu tiền, chưa thống nhất đổi sang hoàn thành; không sửa POS/dịch vụ |

Build PASS (0 lỗi, 1 cảnh báo null có sẵn), C# 29/29 PASS, JavaScript 12/12 PASS, HTTP/SQL **14/14 PASS**. [Kết quả HTTP/SQL](http-sql-20261110.json) bổ sung 3 kiểm tra mới: size không hợp lệ trả 400, số lượng 0 trả 400, số bản ghi SQL không tăng sau hai yêu cầu sai. C# và JS đã chạy lại, không phải kết quả suy từ CI. GitHub chưa có Actions/checks nên CI NOT_RUN.

Lần đặt qua browser: `MX261110-YLWM`, tên giả `DUY_EVIDENCE_20261110`, 2 ly, 56.000đ. Đây là kiểm tra một lượt gửi thành công, chưa chứng minh idempotency server. Tests JS chặn gửi lặp khi request đang chờ; không bao phủ mất phản hồi sau khi server đã lưu. Quyền theo dõi vẫn là bearer link/cookie chưa ký; không đánh dấu tiêu chí “chỉ chủ tài khoản xem đơn” là PASS.

## GitHub LIVE và phần còn thiếu

Ngày 10/11: 4 branch, main dd9db4d, master 575c15e, branch Chính b9d5fa5, branch Duy bff5c5b trước lượt bổ sung. PR #17 open, mergeable=true/clean, không review/comment mới. Bốn commit sau bec2a53 đã tồn tại: be14efb, dc89fa1, 404eca7, bff5c5b; không commit lại. Nhánh Chính chưa có thay đổi code tích hợp so với lần đối chiếu trước.

Có 5 milestone Sprint 1–5 đang mở. Project Board **BLOCKED_PERMISSION**: GitHub GraphQL trả INSUFFICIENT_SCOPES vì thiếu read:project; không tự đổi quyền token. Issues giao Duy vẫn #1, #4, #7, #8, #18, #19; không có task mới so với audit trước. #19 báo cáo tổng quan/khảo sát ngoài trọng tâm Nhánh A và Chương IV của lượt này, chưa có bản Word mới, không nhận là hoàn tất.

Issue #1 có [2 đặc tả khách hàng](../USE-CASE-KHACH-HANG.md), [Hình 21 gốc](../hinh-21-use-case-khach-hang.png), [Hình 27 gốc](../hinh-27-activity-dat-mon.png); chưa có file thiết kế gốc Draw.io/StarUML. #4 và #8 còn khác yêu cầu mẫu (size/mức đường/Session so với cấu hình thực tế/localStorage). #18 có trạng thái thực tế 1–5, tại bàn/mang đi; chưa có giao hàng địa chỉ. Không tự tạo model/controller trùng hoặc đổi quy tắc nhóm để tick checklist.

PR vào main vẫn chứa mã nền qua các commit của Từ Xuân Đạt. Trang chủ/menu/checkout nền có sẵn; đóng góp mới chỉ là kiểm thử, sửa lỗi trước đây và tài liệu/ảnh mới. Nhóm trưởng cần thống nhất nền main, chính sách bảo vệ mã đơn, idempotency và thời điểm cộng điểm. Không đóng Issue hoặc tự merge PR.
