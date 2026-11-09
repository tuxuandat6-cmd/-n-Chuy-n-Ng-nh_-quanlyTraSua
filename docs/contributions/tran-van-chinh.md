# Bàn giao mã nguồn liên quan đến phần việc của Trần Văn Chính

**Dự án:** Quản lý Quán Trà Sữa — `MixueDatMon` (.NET 8, ASP.NET Core MVC, SQL Server).
**Nguồn:** ZIP `Do_An_Chuyen_Nganh (1).zip` do nhóm cung cấp và bảng phân công trong `bao_cao_ck.docx`, bảng đầu tiên.
**GitHub:** `tranvanchinhbg2006-alt`.

## Phạm vi được ghi trong báo cáo

| Sprint | Công việc trong bảng phân công | Tình trạng bàn giao |
|---|---|---|
| 0 | Tạo khảo sát | Không tìm thấy file khảo sát độc lập trong ZIP, nên **không tự tạo bằng chứng** |
| 1 | Thiết kế cơ sở dữ liệu | Bàn giao `Database/MixueDatMonDb.sql` (file nguyên bản, chứa phần CSDL dùng chung) |
| 2 | Thiết kế giao diện nhân viên | Bàn giao `Views/ThuNgan/`, `_StaffLayout.cshtml`, JS nhân viên/thu ngân |
| 3 | Viết model chọn bàn/chọn ghế | Mã liên quan nằm trong `MoHinh/ThucThe.cs` (lớp `BanAn`), `MoHinh/ViewModels.cs`, controller và view dùng chung; **không tách riêng class gây trùng định nghĩa** |
| 4 | Tạo database tích điểm | Bàn giao `Database/CapNhat_MaGiamGia_TichDiem.sql` cùng dịch vụ/view liên quan |
| 6 | Payment, logic và cập nhật CSDL | Bàn giao các phần hiện có trong `ThuNganController.cs`, `DichVuKhuyenMai.cs`, JS giao diện nhân viên |
| 8 | Kiểm thử đăng nhập; kiểm thử thanh toán | Kịch bản thủ công được tóm tắt tại `docs/testing/tran-van-chinh.md`; không thấy file test riêng cho đăng nhập trong ZIP |
| 9 | Kiểm tra và sửa kịch bản/kết quả kiểm thử nếu có lỗi | Tài liệu kiểm thử trong báo cáo; không giả định người làm là tác giả của tất cả xUnit tests |

## Quy tắc ghi nhận đóng góp

Các file source trong nhánh này được **sao chép nguyên byte từ ZIP gốc**. Việc một file nằm trong commit không khẳng định toàn bộ file chỉ do Trần Văn Chính viết. Những file có `scope=shared` trong manifest chứa cả phần việc chung hoặc tác giả không thể xác minh riêng bằng ZIP.

**Không đưa vào branch:** `appsettings.json` (chứa thông tin ngân hàng/cấu hình kết nối), `Db.sql` (bản trích xuất dữ liệu lớn), `.vs/`, `bin/`, `obj/`, `wwwroot/uploads/`, tài liệu báo cáo đầy đủ của cả nhóm.

**Đồng bộ với nhóm:** `MoHinh/ThucThe.cs`, `MoHinh/ViewModels.cs`, `DuLieu/DuLieuContext.cs`, `Program.cs`, `wwwroot/css/site.css`, `.csproj` và phần lớn các chức năng khác cần người phụ trách chính đưa lên trước/khi merge. PR này chỉ gồm **mã liên quan đến đầu việc** và chưa phải ứng dụng đầy đủ có thể build độc lập.

**Lưu ý bảo mật:** schema SQL gốc có tài khoản/mật khẩu **demo cố định** cùng dữ liệu mẫu, không sử dụng nguyên vẹn cho môi trường công khai. Đổi thông tin demo nếu deploy. File cấu hình ngân hàng có trong ZIP gốc bị loại bỏ khỏi gói này.

**Phân công mâu thuẫn:** GitHub Issue #2 hiện gán cho `tranvanchinhbg2006-alt`, nhưng báo cáo ghi Class Diagram do **Từ Xuân Đạt** thiết kế. Không đưa sơ đồ thiết kế vào commit này hoặc tự nhận tác giả của sơ đồ.

## Kiểm tra

- SHA-256 từng file nguyên bản: `docs/contributions/tran-van-chinh-manifest.json`.
- JS: `node --check` nếu cài Node.
- Dotnet build / database integration / PR CI: **NOT_RUN** nếu chưa có đủ solution và SQL Server.
- Báo cáo gốc có nêu kết quả kiểm thử nhưng đó là số liệu báo cáo nhóm, **không phải kết quả kiểm thử mới chạy trong lúc tạo gói**.
