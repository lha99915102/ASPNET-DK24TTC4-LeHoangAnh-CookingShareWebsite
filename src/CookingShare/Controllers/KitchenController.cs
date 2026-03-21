using CookingShare.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace CookingShare.Controllers
{
    public class KitchenController : Controller
    {
        CookingShareDBEntities db = new CookingShareDBEntities();

        public ActionResult Index()
        {
            try
            {
                // LẤY HÌNH NỀN GÓC BẾP 
                // Sử dụng SettingHelper để đồng bộ Cache 100% với trang Admin
                string rawValue = CookingShare.Helpers.SettingHelper.GetValue("Kitchen_Cover_Image", "");
                string coverImg = "https://placehold.co/1200x400?text=My+Kitchen+Workspace"; // Ảnh mặc định

                if (!string.IsNullOrEmpty(rawValue))
                {
                    if (!rawValue.StartsWith("http") && !rawValue.StartsWith("/"))
                    {
                        coverImg = Url.Content("~/assets/images/banners/" + rawValue);
                    }
                    else
                    {
                        coverImg = rawValue;
                    }
                }
                ViewBag.CoverImage = coverImg;



                // RANDOM THỰC ĐƠN TRONG NGÀY (LƯU CACHE TỚI HẾT NGÀY)
                string menuCacheKey = "DailyKitchenMenu_" + DateTime.Today.ToString("yyyyMMdd");
                var dailyMenu = HttpContext.Cache[menuCacheKey] as List<RECIPE>;

                if (dailyMenu == null)
                {
                    // RANDOM THEO NGÀY (Giữ nguyên menu trong 24h)
                    int seed = DateTime.Today.Year * 10000 + DateTime.Today.Month * 100 + DateTime.Today.Day;
                    Random rnd = new Random(seed);

                    // TỐI ƯU HIỆU NĂNG: Chỉ kéo danh sách ID và Tên Danh Mục lên RAM
                    var activeRecipesInfo = db.RECIPE
                                              .Where(r => r.Status == 1 && r.CATEGORY != null)
                                              .Select(r => new { r.ID, CatName = r.CATEGORY.Name.ToLower() })
                                              .ToList();

                    dailyMenu = new List<RECIPE>();

                    if (activeRecipesInfo.Count > 0)
                    {
                        // Phân loại list ID theo từ khóa danh mục
                        var mainIds = activeRecipesInfo.Where(r => r.CatName.Contains("mặn") || r.CatName.Contains("chính")).Select(r => r.ID).ToList();
                        var soupIds = activeRecipesInfo.Where(r => r.CatName.Contains("canh") || r.CatName.Contains("súp")).Select(r => r.ID).ToList();
                        var dessertIds = activeRecipesInfo.Where(r => r.CatName.Contains("tráng miệng") || r.CatName.Contains("ngọt") || r.CatName.Contains("bánh")).Select(r => r.ID).ToList();
                        var allIds = activeRecipesInfo.Select(r => r.ID).ToList();

                        // CHỐNG TRÙNG LẶP MÓN ĂN TRONG THỰC ĐƠN
                        // Bốc món Mặn
                        int mainDishId = mainIds.Count > 0 ? mainIds[rnd.Next(mainIds.Count)] : allIds[rnd.Next(allIds.Count)];

                        // Bốc món Canh (Loại bỏ món Mặn đã bốc)
                        var safeSoupIds = soupIds.Where(id => id != mainDishId).ToList();
                        var safeAllIdsForSoup = allIds.Where(id => id != mainDishId).ToList();
                        int soupId = safeSoupIds.Count > 0
                                     ? safeSoupIds[rnd.Next(safeSoupIds.Count)]
                                     : (safeAllIdsForSoup.Count > 0 ? safeAllIdsForSoup[rnd.Next(safeAllIdsForSoup.Count)] : mainDishId);

                        // Bốc Tráng miệng (Loại bỏ món Mặn và Canh)
                        var safeDessertIds = dessertIds.Where(id => id != mainDishId && id != soupId).ToList();
                        var safeAllIdsForDessert = allIds.Where(id => id != mainDishId && id != soupId).ToList();
                        int dessertId = safeDessertIds.Count > 0
                                        ? safeDessertIds[rnd.Next(safeDessertIds.Count)]
                                        : (safeAllIdsForDessert.Count > 0 ? safeAllIdsForDessert[rnd.Next(safeAllIdsForDessert.Count)] : soupId);

                        // TRUY VẤN CHÍNH XÁC 3 MÓN ĐƯỢC CHỌN KÈM THÔNG TIN TÁC GIẢ 
                        var selectedIds = new List<int> { mainDishId, soupId, dessertId }.Distinct().ToList();
                        var finalRecipes = db.RECIPE
                                             .Include(r => r.ACCOUNT)
                                             .Include(r => r.ACCOUNT.PROFILE)
                                             .Include(r => r.CATEGORY)
                                             .Where(r => selectedIds.Contains(r.ID))
                                             .ToList();

                        // Sắp xếp đúng thứ tự: Mặn -> Canh -> Tráng miệng để lưu vào List
                        dailyMenu.Add(finalRecipes.FirstOrDefault(r => r.ID == mainDishId));
                        dailyMenu.Add(finalRecipes.FirstOrDefault(r => r.ID == soupId));
                        dailyMenu.Add(finalRecipes.FirstOrDefault(r => r.ID == dessertId));

                        // Lưu Cache đến nửa đêm (hết ngày hôm nay)
                        HttpContext.Cache.Insert(menuCacheKey, dailyMenu, null, DateTime.Today.AddDays(1), System.Web.Caching.Cache.NoSlidingExpiration);
                    }
                }

                // Đổ dữ liệu từ Cache ra ViewBag
                if (dailyMenu != null && dailyMenu.Count >= 3)
                {
                    ViewBag.MainDish = dailyMenu[0];
                    ViewBag.Soup = dailyMenu[1];
                    ViewBag.Dessert = dailyMenu[2];
                }
                else
                {
                    ViewBag.MainDish = null;
                    ViewBag.Soup = null;
                    ViewBag.Dessert = null;
                }

                return View();
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "Hệ thống gợi ý món ăn đang bảo trì, vui lòng quay lại sau.";
                return View();
            }
        }

        // Dọn dẹp kết nối Database
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}