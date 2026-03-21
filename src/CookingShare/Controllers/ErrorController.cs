using CookingShare.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace CookingShare.Controllers
{
    public class ErrorController : Controller
    {
        CookingShareDBEntities db = new CookingShareDBEntities();

        // TRANG LỖI 404 (KHÔNG TÌM THẤY TRANG)
        public ActionResult NotFound()
        {
            // Ép Server trả về đúng mã 404 cho chuẩn SEO
            Response.StatusCode = 404;
            Response.TrySkipIisCustomErrors = true;

            try
            {
                // TỐI ƯU HIỆU NĂNG BẰNG CACHE (Chống lỗi quá tải CPU SQL Server)
                var cachedPool = HttpContext.Cache["ErrorPageRecipePool"] as List<RECIPE>;
                if (cachedPool == null)
                {
                    // Lấy 50 bài viết mới nhất đã được duyệt lưu vào RAM (Không dùng NEWID ở SQL)
                    cachedPool = db.RECIPE
                                   .Include(r => r.ACCOUNT)
                                   .Include(r => r.ACCOUNT.PROFILE)
                                   .Where(r => r.Status == 1)
                                   .OrderByDescending(r => r.CreateDate)
                                   .Take(50)
                                   .ToList();

                    // Lưu Pool này trong 1 tiếng
                    HttpContext.Cache.Insert("ErrorPageRecipePool", cachedPool, null, DateTime.Now.AddHours(1), System.Web.Caching.Cache.NoSlidingExpiration);
                }

                // Xáo trộn ngẫu nhiên 50 bài trong RAM và lấy 4 bài
                var suggestedRecipes = cachedPool.OrderBy(r => Guid.NewGuid()).Take(4).ToList();

                return View(suggestedRecipes);
            }
            catch (Exception)
            {
                // Nếu Database mất kết nối, trả về một danh sách rỗng 
                return View(new List<RECIPE>());
            }
        }

        // TRANG BẢO TRÌ HOẶC CHƯA HOÀN THIỆN
        public ActionResult UnderConstruction()
        {
            return View();
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