# BÁO CÁO TIẾN ĐỘ TUẦN 02
(Từ ngày 19/01/2026 đến ngày 25/01/2026)

## 1. Thông tin chung
* **Họ tên:** Lê Hoàng Anh
* **Lớp:** DK24TTC4
* **Đề tài:** CookingShare - Website chia sẻ công thức nấu ăn

## 2. Các công việc đã thực hiện
* [x] Đã khởi tạo thành công Project ASP.NET theo mô hình MVC 5 (Thay thế Web Forms để tối ưu hóa cấu trúc).
* [x] Đã mời giảng viên hướng dẫn vào Repository và cấu hình quyền truy cập.
* [x] Phân tích và thiết kế sơ bộ các bảng trong CSDL (Users, Recipes, Ingredients).
* [ ] **Hoàn thiện Script SQL Server:** Đang thực hiện (Đạt khoảng 80%).
    * *Lý do chưa xong:* Đang tái cấu trúc lại bảng `Account` và `Profile` để tách biệt thông tin đăng nhập và thông tin y tế (chiều cao, cân nặng, dị ứng). Cần chuẩn hóa lại quan hệ để phục vụ tính năng "Tủ lạnh thông minh".

## 3. Kết quả đạt được
* Source code khung dự án MVC đã được đẩy lên GitHub (`src/CookingShare`).
* File thiết kế CSDL nháp (`setup/CookingShareDB.sql`) đã có các bảng cơ bản.

## 4. Vấn đề/Khó khăn
* Việc thiết kế Database cho tính năng "Gợi ý món ăn theo dị ứng" phức tạp hơn dự kiến. Cần chuyển từ lưu trữ dạng text sang bảng quan hệ (Many-to-Many) để đảm bảo truy vấn chính xác.

## 5. Kế hoạch tuần sau (26/01 - 01/02)
* **Ưu tiên 1:** Chốt cấu trúc Database cuối cùng và chạy script tạo bảng hoàn chỉnh.
* **Ưu tiên 2:** Sử dụng Entity Framework (Database First) để kết nối SQL Server vào Project MVC.
* **Ưu tiên 3:** Xây dựng Layout chung (Header, Footer) cho website và tính đăng đăng nhập.