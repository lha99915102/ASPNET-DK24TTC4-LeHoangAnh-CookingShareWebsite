using System;
using System.IO;
using System.Linq;
using System.Web;

namespace CookingShare.Models
{
    public static class FileHelper
    {
        //  Danh sách các định dạng ảnh an toàn (Whitelist)
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        /// Upload ảnh mới và tự động xóa ảnh cũ để dọn rác Server
        public static string UploadAndReplaceImage(HttpPostedFileBase file, string folderPath, string prefix, string oldFileName)
        {
            if (file == null || file.ContentLength == 0) return oldFileName;

            //  Kiểm tra đuôi file (Chống Upload Mã độc / Web Shell)
            string extension = Path.GetExtension(file.FileName).ToLower();
            if (!AllowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException("Chỉ cho phép tải lên các định dạng ảnh: JPG, JPEG, PNG, GIF, WEBP.");
            }

            // MapPath một lần để tái sử dụng
            string serverFolderPath = HttpContext.Current.Server.MapPath(folderPath);

            // Tự động tạo thư mục nếu chưa tồn tại (Chống lỗi DirectoryNotFound)
            if (!Directory.Exists(serverFolderPath))
            {
                Directory.CreateDirectory(serverFolderPath);
            }

            // XÓA FILE CŨ (Nếu có và không phải ảnh mặc định)
            if (!string.IsNullOrEmpty(oldFileName)
                && oldFileName != "default_recipe.jpg"
                && oldFileName != "default-avatar.png")
            {
                // Chỉ lấy tên file, đề phòng DB cũ lưu cả đường dẫn dài
                string cleanOldName = Path.GetFileName(oldFileName);

                // ĐÃ SỬA: Dùng Path.Combine thay vì cộng chuỗi thủ công để tránh lỗi mất dấu gạch chéo
                string oldFilePath = Path.Combine(serverFolderPath, cleanOldName);

                if (File.Exists(oldFilePath))
                {
                    try { File.Delete(oldFilePath); } catch { /* Bỏ qua nếu file đang bị khóa */ }
                }
            }

            // TẠO TÊN FILE MỚI: [Prefix]_[Ticks]_[Guid].[Đuôi]
            // ĐÃ SỬA: Dùng Ticks và Guid để đảm bảo tính duy nhất tuyệt đối 100% trong môi trường đa luồng
            string uniqueId = DateTime.Now.Ticks.ToString() + "_" + Guid.NewGuid().ToString("N").Substring(0, 4);
            string newFileName = $"{prefix}_{uniqueId}{extension}";

            // LƯU FILE VẬT LÝ
            string newFilePath = Path.Combine(serverFolderPath, newFileName);
            file.SaveAs(newFilePath);

            return newFileName;
        }

        /// Hàm Xóa ảnh khi User/Admin xóa bài/tài khoản
        public static void DeleteImage(string folderPath, string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || fileName == "default_recipe.jpg" || fileName == "default-avatar.png") return;

            string cleanName = Path.GetFileName(fileName);
            string filePath = Path.Combine(HttpContext.Current.Server.MapPath(folderPath), cleanName);

            if (File.Exists(filePath))
            {
                try { File.Delete(filePath); } catch { /* Bỏ qua lỗi nếu file đang mở */ }
            }
        }
    }
}