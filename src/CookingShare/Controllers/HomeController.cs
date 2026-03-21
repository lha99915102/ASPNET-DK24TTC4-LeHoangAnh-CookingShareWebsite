using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using CookingShare.Models;

namespace CookingShare.Controllers
{
    public class HomeController : Controller
    {
        // Khởi tạo kết nối DB
        CookingShareDBEntities db = new CookingShareDBEntities();

        public ActionResult Index()
        {
            try
            {
                // LẤY DANH SÁCH BANNER (LƯU CACHE 1 TIẾNG)
                var activeBanners = HttpContext.Cache["HomeBanners"] as List<BANNER>;
                if (activeBanners == null)
                {
                    activeBanners = db.BANNER
                                      .Where(b => b.IsActive == true)
                                      .OrderBy(b => b.Position)
                                      .ToList();
                    HttpContext.Cache.Insert("HomeBanners", activeBanners, null, DateTime.Now.AddHours(1), System.Web.Caching.Cache.NoSlidingExpiration);
                }
                ViewBag.Banners = activeBanners;


                // LẤY 8 CÔNG THỨC MỚI NHẤT (LƯU CACHE 10 PHÚT)
                var listCongThucMoi = HttpContext.Cache["HomeLatestRecipes"] as List<RECIPE>;
                if (listCongThucMoi == null)
                {
                    listCongThucMoi = db.RECIPE
                                        .Include(r => r.ACCOUNT)
                                        .Include(r => r.ACCOUNT.PROFILE)
                                        .Include(r => r.CATEGORY)
                                        .Where(r => r.Status == 1)
                                        .OrderByDescending(r => r.CreateDate)
                                        .Take(8)
                                        .ToList();
                    HttpContext.Cache.Insert("HomeLatestRecipes", listCongThucMoi, null, DateTime.Now.AddMinutes(10), System.Web.Caching.Cache.NoSlidingExpiration);
                }


                // LẤY DANH MỤC MÓN ĂN (LƯU CACHE 24 TIẾNG)
                var categories = HttpContext.Cache["HomeCategories"] as List<CATEGORY>;
                if (categories == null)
                {
                    categories = db.CATEGORY.ToList();
                    HttpContext.Cache.Insert("HomeCategories", categories, null, DateTime.Now.AddHours(24), System.Web.Caching.Cache.NoSlidingExpiration);
                }
                ViewBag.Categories = categories;


                // Truyền dữ liệu món ăn sang View
                return View(listCongThucMoi);
            }
            catch (Exception ex)
            {
                // Nếu DB quá tải hoặc mất kết nối, trang chủ vẫn load được (kèm list rỗng)
                ViewBag.ErrorMessage = "Hệ thống đang bảo trì dữ liệu, vui lòng quay lại sau ít phút: " + ex.Message;
                return View(new List<RECIPE>());
            }
        }


        // Giải phóng bộ nhớ ngay sau khi load xong trang chủ
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