# Project: CookingShare - Website Chia sẻ Công thức Nấu ăn

![Badge](https://img.shields.io/badge/Language-C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![Badge](https://img.shields.io/badge/Framework-ASP.NET_MVC_5-512BD4?style=for-the-badge&logo=.net&logoColor=white)
![Badge](https://img.shields.io/badge/Database-SQL_Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)
![Badge](https://img.shields.io/badge/Status-Completed-success?style=for-the-badge)

**CookingShare** là đồ án chuyên đề ASP.NET sử dụng ngôn ngữ C# và cấu trúc ASP.NET MVC nhằm xây dựng một website chia sẻ công thức nấu ăn chuyên sâu. Dự án không chỉ dừng lại ở việc đăng tải món ăn, mà còn tích hợp các tính năng tính toán dinh dưỡng (Calo, TDEE/BMR), gợi ý thực đơn thông minh dựa trên tủ lạnh và loại trừ dị ứng cá nhân, tạo ra một cộng đồng ẩm thực lành mạnh và tiện ích.

> **🌐 TRẢI NGHIỆM THỰC TẾ (LIVE DEMO):** > Dự án đã được triển khai và vận hành thực tế trên máy chủ Internet.  
> 👉 **[Bấm vào đây để truy cập Website CookingShare](https://hoanganhle.id.vn/)** ---
---

## 🎓 Thông tin Sinh viên (Student Information)
* **Họ và tên:** Lê Hoàng Anh
* **Lớp:** DK24TTC4
* **MSSV:** 170124379
* **Email:** anhlh240199@tvu-onschool.edu.vn 
* **Điện thoại:** (+84) 338.684.934
* **Giảng viên hướng dẫn:** TS. Đoàn Phước Miền

---

## 🚀 Tính năng nổi bật (Key Features)

### 1. Quản lý tài khoản & Hồ sơ sức khỏe cá nhân
* Đăng nhập linh hoạt (Username, Email hoặc SĐT) kèm phân quyền bảo mật chặt chẽ (Admin - User).
* Tính toán chỉ số **TDEE/BMR** dựa trên chiều cao, cân nặng và mức độ vận động của người dùng.
* Khai báo hồ sơ dị ứng thực phẩm để hệ thống tự động cảnh báo.

### 2. Công thức & Dinh dưỡng (Core System)
* Hệ thống hiển thị công thức trực quan, cho phép thao tác thả tim (yêu thích), lưu vào danh sách đi chợ và xuất file Word.
* **Tính Calo tự động:** Tự động tính toán tổng lượng Calo, Protein, Fat, Carb của món ăn dựa trên định lượng nguyên liệu cấu thành.

### 3. Tủ lạnh thông minh (Smart Fridge) 🧊
* Người dùng chọn các nguyên liệu đang có sẵn trong tủ lạnh nhà mình.
* Thuật toán gợi ý chính xác các món ăn có thể nấu được từ nguyên liệu đó.
* **Smart Filter:** Tự động "ẩn" các món ăn có chứa thành phần mà người dùng đã khai báo dị ứng.

### 4. Mạng xã hội ẩm thực (Cooksnap)
* Người dùng thực hành nấu ăn và đăng tải hình ảnh "Trả bài" (Cooksnap) để khoe thành quả với tác giả.
* Tích hợp hệ thống bình luận, đánh giá sao (Rating), thông báo (Notification) và báo cáo vi phạm (Report) xử lý theo thời gian thực.

---

## 🛠 Công nghệ sử dụng (Tech Stack)

* **Backend:** ASP.NET MVC 5 (.NET Framework 4.7.2), C#.
* **Database:** SQL Server (Entity Framework - Database First).
* **Frontend:** HTML5, CSS3, Bootstrap 4, jQuery, AJAX.
* **Công cụ:** Visual Studio Community 2026, SQL Server Management Studio (SSMS) 2021, IIS (Internet Information Services).
* **Tối ưu hóa (Performance):** Ứng dụng kỹ thuật `HttpContext.Cache` giúp tăng tốc độ tải dữ liệu (Danh mục, Nguyên liệu, Banner, Trang chủ) và giảm tải tối đa cho Database.

---

## 📂 Cấu trúc Thư mục (Repository Structure)

Dự án được tổ chức nghiêm ngặt theo quy định đồ án chuyên đề ngành Công nghệ Thông tin bậc Đại học của Đại học Trà Vinh:
* `scr/` : Chứa toàn bộ tập tin nguồn (Source Code C#, ASP.NET MVC) và tập tin dữ liệu thử nghiệm.
* `setup/` : Chứa tập tin cài đặt, Script Database (`.sql`) và bản đóng gói `CookingShare_Web` dùng để triển khai (Deploy) lên máy chủ IIS.
* `progress-report/` : Chứa các file báo cáo tiến độ thực hiện đồ án hàng tuần.
* `thesis/` : Chứa tập tin tài liệu văn bản của Đồ án.
  * `doc/` : Tài liệu báo cáo định dạng Word (.DOCX).
  * `pdf/` : Tài liệu báo cáo định dạng PDF.
  * `abs/` : Slide báo cáo thuyết trình (.PPTX).
  * `refs/` : Các tài liệu tham khảo sử dụng trong nghiên cứu.

---

## ⚙️ Hướng dẫn Cài đặt & Chạy Ứng dụng (Setup & Installation)

Phần này sẽ hướng dẫn bạn từng bước cài đặt để chạy hệ thống thành công trên máy tính cá nhân (Localhost) dưới dạng Máy chủ Web ảo (IIS).

### 🛠️ Giai đoạn 1: Chuẩn bị phần mềm nền tảng
Để website hoạt động, máy tính của bạn cần được thiết lập 2 nền tảng cốt lõi:
1. **Phần mềm Cơ sở dữ liệu:** Tải và cài đặt phần mềm quản lý **SQL Server Management Studio (SSMS)**.
2. **Bật Máy chủ Web ảo (IIS) có sẵn trên Windows:**
   * Bấm phím `Windows`, gõ tìm kiếm `Turn Windows features on or off` và mở nó lên.
   * Tìm đến mục **Internet Information Services**.
   * Mở rộng theo đường dẫn: `World Wide Web Services` -> `Application Development Features` -> Tích chọn vào ô **ASP.NET 4.8** (hoặc 4.7/4.x tùy máy), **.NET Extensibility 4.8**, **ISAPI Extensions** và **ISAPI Filters**.
   * Mở rộng mục `Common HTTP Features` -> Tích chọn ô **Static Content** (Bắt buộc để web tải được màu sắc và hình ảnh giao diện).
   * Mở rộng mục `Web Management Tools` -> Tích chọn ô **IIS Management Console** để truy cập giao diện IIS.
   * Bấm **OK** và đợi Windows cài đặt trong khoảng 1 - 3 phút.

### Giai đoạn 2: Khởi tạo Cơ sở dữ liệu (Database)
1. Mở phần mềm **SSMS** và kết nối vào Server SQL của bạn.
2. Trên thanh menu, chọn `File` -> `Open` -> `File...` và trỏ đến file Script SQL nằm trong thư mục `setup/`.
3. Bấm nút **Execute** (hoặc phím `F5`). Máy tính sẽ tự động tạo Database `CookingShareDB` và bơm sẵn dữ liệu mẫu.
4. **Kết nối Web với Database:** Mở file `Web.config` (nằm trong thư mục `setup/CookingShare_Web`) bằng phần mềm Notepad. Tìm đoạn `<connectionStrings>` và sửa chữ nằm sau `data source=` thành Tên Server SQL trên máy tính của bạn. Lưu file lại (`Ctrl + S`).

### Giai đoạn 3: Đưa Website "lên sóng" bằng IIS
Đây là bước cấu hình máy chủ web IIS và giải quyết các lỗi phân quyền thường gặp.

**Bước 1: Tạo trang web trên IIS Manager**
* Bấm tổ hợp phím `Windows + R`, gõ **`inetmgr`** và ấn Enter để mở phần mềm **IIS Manager**.
* Ở cột bên trái, mở rộng tên máy tính -> Click chuột phải vào chữ **Sites** -> Chọn **Add Website...**
* Điền thông tin cấu hình:
  * **Site name:** `CookingShare`
  * **Physical path:** Bấm nút `...` và trỏ tìm đến thư mục `setup/CookingShare_Web`.
  * **Port:** Đổi từ 80 thành **`8888`** (Tránh xung đột cổng). Bấm **OK**.

**Bước 3: Cấp quyền kết nối Database (Chống lỗi HTTP 500)**
* Ở cột bên trái của IIS, bấm vào mục **Application Pools**.
* Click chuột phải vào tên `CookingShare` vừa tạo -> Chọn **Advanced Settings...**
* Đảm bảo ô `.NET CLR version` đang chọn là **v4.0.30319**.
* Kéo xuống tìm dòng `Identity`, bấm vào nút `...` ở đuôi -> Chọn **LocalSystem** -> Bấm **OK** liên tục để lưu.

**Bước 4: Cấp quyền tải giao diện CSS/JS (Chống lỗi vỡ layout)**
* Ở cột bên trái, bấm vào trang web `CookingShare` (nằm dưới mục Sites).
* Ở màn hình giữa, click đúp vào biểu tượng **Authentication**.
* Click chuột phải vào dòng `Anonymous Authentication` chọn **Enable** (nếu đang tắt).
* Click chuột phải lần nữa chọn **Edit...** -> Đánh dấu tích vào ô **Application pool identity** -> Bấm **OK**.

**Bước 5: Tận hưởng thành quả**
* Mở trình duyệt Chrome/Edge, truy cập vào đường dẫn: **`http://localhost:8888`**. Website CookingShare đã chính thức hoạt động với tốc độ tối ưu nhất!

---

## 🔐 Tài khoản Demo (Demo Accounts)
Các tài khoản mẫu sau để trải nghiệm nhanh toàn bộ chức năng của hệ thống:

* **Tài khoản Admin (Quản trị viên duyệt bài, quản lý danh mục, xử lý báo cáo):**
  * Tên đăng nhập: `admin`
  * Mật khẩu: `123456`
* **Tài khoản User (Đăng công thức, bình luận, quản lý tủ lạnh thông minh):**
  * Tên đăng nhập: `bepxinh`
  * Mật khẩu: `123456`