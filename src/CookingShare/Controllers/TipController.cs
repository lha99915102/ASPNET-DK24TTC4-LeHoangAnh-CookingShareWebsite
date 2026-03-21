using CookingShare.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace CookingShare.Controllers
{
    public class TipController : Controller
    {
        CookingShareDBEntities db = new CookingShareDBEntities();

        public ActionResult Index(string tag, int page = 1)
        {
            try
            {
                // Lấy toàn bộ Mẹo vặt 
                var query = db.TIP.Include(t => t.ACCOUNT).Include(t => t.ACCOUNT.PROFILE).AsQueryable();

                //  Lọc theo Tag
                if (!string.IsNullOrEmpty(tag))
                {
                    if (tag == "Bảo quản") query = query.Where(t => t.Title.Contains("bảo quản") || t.Title.Contains("giữ") || t.Title.Contains("Bảo quản"));
                    else if (tag == "Vệ sinh") query = query.Where(t => t.Title.Contains("vệ sinh") || t.Title.Contains("sạch") || t.Title.Contains("rửa") || t.Title.Contains("Vệ sinh"));
                    else if (tag == "Dinh dưỡng") query = query.Where(t => t.Title.Contains("dinh dưỡng") || t.Title.Contains("vitamin") || t.Title.Contains("calo") || t.Title.Contains("Dinh dưỡng"));
                    else if (tag == "Kỹ năng") query = query.Where(t => t.Title.Contains("cách") || t.Title.Contains("mẹo") || t.Title.Contains("kỹ năng") || t.Title.Contains("Cách"));

                    ViewBag.CurrentTag = tag;
                }
                else
                {
                    ViewBag.CurrentTag = "Tất cả";
                }

                // Sắp xếp mới nhất đưa lên đầu
                query = query.OrderByDescending(t => t.CreateDate);

                // TẠO CACHE CHO BÀI NỔI BẬT & TOP 3 BÀI TRÁNH QUERY NHIỀU LẦN
                var cachedTopTips = HttpContext.Cache["TipFeaturedAndTop"] as List<TIP>;
                if (cachedTopTips == null)
                {
                    // Lấy 4 bài mới nhất (1 cho Featured, 3 cho Top 3)
                    cachedTopTips = db.TIP.Include(t => t.ACCOUNT)
                                          .Include(t => t.ACCOUNT.PROFILE)
                                          .OrderByDescending(t => t.CreateDate)
                                          .Take(4)
                                          .ToList();

                    // Lưu Cache 1 tiếng
                    HttpContext.Cache.Insert("TipFeaturedAndTop", cachedTopTips, null, DateTime.Now.AddHours(1), System.Web.Caching.Cache.NoSlidingExpiration);
                }

                // Tách bài đầu tiên làm Featured, 3 bài còn lại làm Top
                ViewBag.FeaturedTip = cachedTopTips.FirstOrDefault();
                ViewBag.TopTips = cachedTopTips.Skip(1).Take(3).ToList();

                // Phân trang danh sách chính
                int pageSize = 6;
                int totalItems = query.Count();
                int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                if (page < 1) page = 1;
                if (page > totalPages && totalPages > 0) page = totalPages;

                var tips = new List<TIP>();
                if (totalItems > 0)
                {
                    tips = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                }

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;

                return View(tips);
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "Hệ thống đang bảo trì dữ liệu, vui lòng thử lại sau.";
                return View(new List<TIP>());
            }
        }

        // Trang xem chi tiết mẹo vặt
        public ActionResult Detail(int id)
        {
            try
            {
                // Nạp sẵn PROFILE để hiển thị Tên và Avatar tác giả
                var tip = db.TIP.Include(t => t.ACCOUNT).Include(t => t.ACCOUNT.PROFILE).FirstOrDefault(t => t.ID == id);
                if (tip == null) return HttpNotFound();

                //  DÙNG CACHE + RAM ĐỂ TRỘN BÀI THAY VÌ ORDER BY NEWID() TRONG SQL
                var cachedPool = HttpContext.Cache["TipRandomPool"] as List<TIP>;
                if (cachedPool == null)
                {
                    cachedPool = db.TIP.Include(t => t.ACCOUNT).Take(50).ToList(); // Lấy 50 bài nạp vào RAM
                    HttpContext.Cache.Insert("TipRandomPool", cachedPool, null, DateTime.Now.AddHours(1), System.Web.Caching.Cache.NoSlidingExpiration);
                }

                // Xáo trộn ngẫu nhiên trên RAM  và loại trừ bài hiện tại
                ViewBag.RelatedTips = cachedPool.Where(t => t.ID != id).OrderBy(t => Guid.NewGuid()).Take(3).ToList();

                return View(tip);
            }
            catch (Exception)
            {
                return HttpNotFound("Có lỗi xảy ra khi tải bài viết.");
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