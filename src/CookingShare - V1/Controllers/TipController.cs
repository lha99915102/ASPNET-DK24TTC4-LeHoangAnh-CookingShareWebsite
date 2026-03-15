using CookingShare.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace CookingShare.Controllers
{
    public class TipController : Controller
    {
        CookingShareDBEntities db = new CookingShareDBEntities();

        public ActionResult Index(string tag, int page = 1)
        {
            // 1. Lấy toàn bộ Mẹo vặt
            var query = db.TIP.AsQueryable();

            // 2. Lọc theo Tag (Bảo quản, Vệ sinh, Dinh dưỡng...) - Tạm dùng Title để lọc vì DB của bạn chưa có bảng TIP_TAG riêng
            if (!string.IsNullOrEmpty(tag))
            {
                if (tag == "Bảo quản") query = query.Where(t => t.Title.Contains("bảo quản") || t.Title.Contains("giữ"));
                else if (tag == "Vệ sinh") query = query.Where(t => t.Title.Contains("vệ sinh") || t.Title.Contains("sạch") || t.Title.Contains("rửa"));
                else if (tag == "Dinh dưỡng") query = query.Where(t => t.Title.Contains("dinh dưỡng") || t.Title.Contains("vitamin") || t.Title.Contains("calo"));
                else if (tag == "Kỹ năng") query = query.Where(t => t.Title.Contains("cách") || t.Title.Contains("mẹo") || t.Title.Contains("kỹ năng"));

                ViewBag.CurrentTag = tag;
            }
            else
            {
                ViewBag.CurrentTag = "Tất cả";
            }

            // Sắp xếp mới nhất đưa lên đầu
            query = query.OrderByDescending(t => t.CreateDate);

            // 3. Lấy Bài viết nổi bật (Lấy bài mới nhất làm nổi bật)
            var featuredTip = db.TIP.OrderByDescending(t => t.CreateDate).FirstOrDefault();
            ViewBag.FeaturedTip = featuredTip;

            // 4. Lấy Top 3 bài viết đọc nhiều nhất (Tạm thời lấy 3 bài cũ nhất để giả lập nếu DB chưa có cột Views cho Tip)
            var topTips = db.TIP.OrderBy(t => t.CreateDate).Take(3).ToList();
            ViewBag.TopTips = topTips;

            // 5. Phân trang
            int pageSize = 6;
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var tips = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(tips);
        }

        // Trang xem chi tiết mẹo vặt
        public ActionResult Detail(int id)
        {
            var tip = db.TIP.Find(id);
            if (tip == null) return HttpNotFound();

            return View(tip);
        }
    }
}