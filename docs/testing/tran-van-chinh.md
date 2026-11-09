# Phạm vi kiểm thử của Trần Văn Chính — trích từ báo cáo nhóm

Nguồn: `bao_cao_ck.docx`, bảng phân công Sprint 8–9 và chương kiểm thử.

- Sprint 8: **Kiểm thử đăng nhập** — cần kiểm tra đúng/sai thông tin đăng nhập, phân quyền thu ngân/quản lý. ZIP có `TaiKhoanController.cs` và Views/TaiKhoan nhưng đây là chức năng được phân công Nguyễn Minh Duy phát triển, **không sao chép vào phần code của Chính**.
- Sprint 8: **Kiểm thử thanh toán** — nghiệp vụ tính tiền, thu tiền mặt/chuyển khoản, đối soát, khuyến mãi/tích điểm. Các file `TinhTienTests.cs` và `DichVuKhuyenMaiTests.cs` được đưa lên dưới dạng **file kiểm thử liên quan**, chưa có bằng chứng xác định tác giả từng file.
- Sprint 9: **Kiểm tra kịch bản/kết quả**, sửa khi phát hiện lỗi.

Báo cáo có ghi số liệu **24/24 unit tests** và **13/14 kiểm thử thủ công** đạt, tổng **37/38** đạt. Trong bảng kịch bản thủ công, TC-07 lại được ghi Pass trong khi thống kê có một test không đạt và BUG-01 nêu trễ tự hủy. Cần làm rõ sự lệch giữa bảng case và bảng tổng hợp trước khi chốt số liệu. Đây là **thông tin trong báo cáo**, không phải kết quả test đã được chạy lại.

**Tình trạng hiện tại:** Test execution = NOT_RUN; CI = NOT_RUN; các thao tác kiểm tra này đòi hỏi tích hợp toàn dự án và môi trường .NET 8 + SQL Server.
