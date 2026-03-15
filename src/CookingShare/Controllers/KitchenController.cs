using CookingShare.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace CookingShare.Controllers
{
    public class KitchenController : Controller
    {
        CookingShareDBEntities db = new CookingShareDBEntities();

        public ActionResult Index()
        {
            // 1. TẠO HẠT GIỐNG RANDOM THEO NGÀY
            // Công thức: Năm * 10000 + Tháng * 100 + Ngày (VD: 20261025). 
            // Giúp Random giữ nguyên kết quả trong cùng 1 ngày, qua ngày mới sẽ tự đổi.
            int seed = DateTime.Today.Year * 10000 + DateTime.Today.Month * 100 + DateTime.Today.Day;
            Random rnd = new Random(seed);

            // Lấy danh sách toàn bộ công thức đã duyệt
            var allRecipes = db.RECIPE.Where(r => r.Status == 1).ToList();

            if (allRecipes.Count > 0)
            {
                // 2. PHÂN LOẠI CÔNG THỨC (Tìm theo tên Danh mục)
                // LƯU Ý: Chữ trong ngoặc kép phải khớp với tên danh mục bạn đặt trong Database
                var mainDishes = allRecipes.Where(r => r.CATEGORY != null &&
                                                 (r.CATEGORY.Name.ToLower().Contains("mặn") ||
                                                  r.CATEGORY.Name.ToLower().Contains("chính"))).ToList();

                var soups = allRecipes.Where(r => r.CATEGORY != null &&
                                            (r.CATEGORY.Name.ToLower().Contains("canh") ||
                                             r.CATEGORY.Name.ToLower().Contains("súp"))).ToList();

                var desserts = allRecipes.Where(r => r.CATEGORY != null &&
                                               (r.CATEGORY.Name.ToLower().Contains("tráng miệng") ||
                                                r.CATEGORY.Name.ToLower().Contains("ngọt") ||
                                                r.CATEGORY.Name.ToLower().Contains("bánh"))).ToList();

                // 3. BỐC THĂM MỖI LOẠI 1 MÓN (Nếu danh mục đó trống, sẽ lấy ngẫu nhiên 1 món bất kỳ bù vào)
                ViewBag.MainDish = mainDishes.Count > 0 ? mainDishes[rnd.Next(mainDishes.Count)] : allRecipes[rnd.Next(allRecipes.Count)];
                ViewBag.Soup = soups.Count > 0 ? soups[rnd.Next(soups.Count)] : allRecipes[rnd.Next(allRecipes.Count)];
                ViewBag.Dessert = desserts.Count > 0 ? desserts[rnd.Next(desserts.Count)] : allRecipes[rnd.Next(allRecipes.Count)];

                // 1. Lấy hình nền Góc bếp từ bảng SYSTEM_SETTING
                var coverImageSetting = db.SYSTEM_SETTING.FirstOrDefault(s => s.SettingKey == "Kitchen_Cover_Image");

                // Nếu trong DB chưa có hoặc lỗi, dùng ảnh mặc định
                ViewBag.CoverImage = coverImageSetting != null && !string.IsNullOrEmpty(coverImageSetting.SettingValue)
                                     ? coverImageSetting.SettingValue
                                     : "https://placehold.co/1200x400?text=My+Kitchen+Workspace";

                // 2. Logic cho phần "Hôm nay ăn gì?" (Lấy random 3 món Mặn, Canh, Ngọt)
                // (Đoạn này bạn có thể bổ sung logic lọc theo Category giống như Home)
                ViewBag.MainDish = db.RECIPE.FirstOrDefault(r => r.Status == 1 && r.CATEGORY.Name.Contains("Mặn"));
                ViewBag.Soup = db.RECIPE.FirstOrDefault(r => r.Status == 1 && r.CATEGORY.Name.Contains("Nước"));
                ViewBag.Dessert = db.RECIPE.FirstOrDefault(r => r.Status == 1 && r.CATEGORY.Name.Contains("Ngọt"));
            }
            else
            {
                ViewBag.MainDish = null;
                ViewBag.Soup = null;
                ViewBag.Dessert = null;
            }

            return View();
        }
    }
}