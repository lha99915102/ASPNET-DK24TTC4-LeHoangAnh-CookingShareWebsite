using CookingShare.Models;
using System;
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
            // 1. Tạo câu truy vấn cơ bản (chưa lấy dữ liệu vội)
            var query = db.COOKSNAP
                .Include(c => c.ACCOUNT)
                .Include(c => c.ACCOUNT.PROFILE)
                .Include(c => c.RECIPE)
                .OrderByDescending(c => c.CreateDate);

            // 2. THUẬT TOÁN PHÂN TRANG (10 bài / trang)
            int pageSize = 10;
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            // Cắt đúng 10 bài của trang hiện tại
            var cooksnaps = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Truyền dữ liệu số trang ra View
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            // 3. Tính toán Top Đầu bếp
            var topChefs = db.ACCOUNT
                .Where(a => a.RECIPE.Count(r => r.Status == 1) > 0)
                .OrderByDescending(a => a.RECIPE.Count(r => r.Status == 1))
                .Take(5)
                .ToList();

            ViewBag.TopChefs = topChefs;

            return View(cooksnaps);
        }
    }
}