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

            // Khởi tạo đối tượng mã hóa SHA-256
            using (SHA256 sha256 = SHA256.Create())
            {
                // Chuyển chuỗi mật khẩu thành mảng byte
                byte[] inputBytes = Encoding.UTF8.GetBytes(password);

                // Mã hóa mảng byte
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                // Chuyển mảng byte đã mã hóa thành chuỗi Hexadecimal (Hệ 16)
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }

                // Kết quả trả về luôn là một chuỗi dài đúng 64 ký tự
                return sb.ToString();
            }
        }
    }
}