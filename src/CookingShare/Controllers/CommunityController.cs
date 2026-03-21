using CookingShare.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace CookingShare.Controllers
{
    public class CommunityController : Controller
    {
        CookingShareDBEntities db = new CookingShareDBEntities();

        public ActionResult Index(int page = 1)
        {
            try
            {
                // LẤY DỮ LIỆU ĐƯỢC PHÉP HIỂN THỊ
                var query = db.COOKSNAP
                    .Include(c => c.ACCOUNT)
                    .Include(c => c.ACCOUNT.PROFILE)
                    .Include(c => c.RECIPE)
                    // Chặn các Cooksnap bị lỗi không có ảnh từ Database
                    .Where(c => c.Status == 1 && !string.IsNullOrEmpty(c.ImageName))
                    .OrderByDescending(c => c.CreateDate);

                // THUẬT TOÁN PHÂN TRANG (10 bài / trang)
                int pageSize = 10;
                int totalItems = query.Count();
                int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                if (page < 1) page = 1;
                if (page > totalPages && totalPages > 0) page = totalPages;

                // Cắt đúng 10 bài của trang hiện tại
                var cooksnaps = new List<COOKSNAP>();
                if (totalItems > 0)
                {
                    cooksnaps = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                }

                // Truyền dữ liệu số trang ra View
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;


                //  TÍNH TOÁN TOP ĐẦU BẾP VỚI BỘ NHỚ ĐỆM (CACHE) 
                var topChefs = HttpContext.Cache["CommunityTopChefs"] as List<ACCOUNT>;
                if (topChefs == null)
                {
                    topChefs = db.ACCOUNT
                        .Include(a => a.PROFILE)
                        // ĐÃ SỬA: Dùng Any() thay cho Count() > 0 để tăng tốc độ truy vấn EXISTS trong SQL
                        .Where(a => a.Status == 1 && a.RECIPE.Any(r => r.Status == 1))
                        .OrderByDescending(a => a.RECIPE.Count(r => r.Status == 1))
                        .Take(5)
                        .ToList();

                    // Lưu kết quả vào Cache Server trong vòng 15 phút
                    HttpContext.Cache.Insert("CommunityTopChefs", topChefs, null, DateTime.Now.AddMinutes(15), System.Web.Caching.Cache.NoSlidingExpiration);
                }

                ViewBag.TopChefs = topChefs;

                return View(cooksnaps);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Hệ thống đang bảo trì dữ liệu, vui lòng thử lại sau: " + ex.Message;
                return View(new List<COOKSNAP>());
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