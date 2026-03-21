using CookingShare.Models;
using System;
using System.Linq;
using System.Web;

namespace CookingShare.Helpers
{
    public static class SettingHelper
    {
        // Hàm lấy giá trị cấu hình dựa vào Mã (SettingKey) - CÓ TÍCH HỢP CACHE
        public static string GetValue(string key, string defaultValue = "")
        {
            // Khởi tạo khóa Cache độc nhất cho mỗi SettingKey
            string cacheKey = "CookingShare_Setting_" + key;

            // TỐI ƯU HIỆU NĂNG: Nếu RAM (Cache) đã lưu giá trị này rồi thì lấy ra dùng luôn, KHÔNG gọi Database
            if (HttpContext.Current.Cache[cacheKey] != null)
            {
                return HttpContext.Current.Cache[cacheKey].ToString();
            }

            // Nếu RAM chưa có (Lần đầu chạy web), mới mở kết nối xuống Database
            using (var db = new CookingShareDBEntities())
            {
                var setting = db.SYSTEM_SETTING.FirstOrDefault(s => s.SettingKey == key);
                string result = setting != null ? setting.SettingValue : defaultValue;

                //  LƯU VÀO RAM: Nhét kết quả vào Cache, giữ trong 24 giờ để lần sau lấy cho nhanh
                HttpContext.Current.Cache.Insert(
                    cacheKey,
                    result,
                    null,
                    DateTime.Now.AddHours(24),
                    System.Web.Caching.Cache.NoSlidingExpiration
                );

                return result;
            }
        }

        // Hàm dọn dẹp Cache (Dùng khi Admin cập nhật cấu hình mới)
        public static void ClearCache(string key)
        {
            string cacheKey = "CookingShare_Setting_" + key;
            HttpContext.Current.Cache.Remove(cacheKey);
        }
    }
}