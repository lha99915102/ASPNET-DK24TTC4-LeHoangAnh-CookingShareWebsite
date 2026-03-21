using System;
using System.Text;
using System.Security.Cryptography;

namespace CookingShare.Models
{
    public static class SecurityHelper
    {
        // Hàm băm mật khẩu bằng chuẩn SHA-256
        public static string HashPasswordSHA256(string password)
        {
            if (string.IsNullOrEmpty(password)) return "";

            // Tính năng "Pepper" 

            string pepper = "CookingShare_Secret_Key_2026!";
            password = password + pepper;

            // Khởi tạo đối tượng mã hóa SHA-256
            using (SHA256 sha256 = SHA256.Create())
            {
                // Chuyển chuỗi mật khẩu thành mảng byte
                byte[] inputBytes = Encoding.UTF8.GetBytes(password);

                // Mã hóa mảng byte
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                // BitConverter sinh ra chuỗi có dấu gạch ngang (VD: A1-B2-C3), Replace nó đi và chuyển thành chữ thường
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}