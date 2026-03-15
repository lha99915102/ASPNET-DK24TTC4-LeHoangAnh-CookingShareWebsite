using CookingShare.Models;
using System;
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;

namespace CookingShare.Areas.Admin.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        CookingShareDBEntities db = new CookingShareDBEntities();

        // ==========================================
        // 1. HIỂN THỊ DANH SÁCH BÌNH LUẬN
        // ==========================================
        public ActionResult Index(int? statusFilter)
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return Redirect("~/Admin/Auth/Login");

            // Thống kê nhanh
            ViewBag.TotalComments = db.COMMENT.Count();
            ViewBag.HiddenComments = db.COMMENT.Count(c => c.Status == 0); // Trạng thái 0 là bị ẩn
            ViewBag.CurrentFilter = statusFilter;

            // Lấy danh sách bình luận kèm theo thông tin Người đăng (ACCOUNT) và Món ăn (RECIPE)
            var comments = db.COMMENT.Include(c => c.ACCOUNT).Include(c => c.RECIPE).AsQueryable();

            // Lọc theo trạng thái nếu có
            if (statusFilter.HasValue)
            {
                comments = comments.Where(c => c.Status == statusFilter.Value);
            }

            // Sắp xếp bình luận mới nhất lên đầu
            return View(comments.OrderByDescending(c => c.CreateDate).ToList());
        }

        // ==========================================
        // 2. ẨN / HIỆN BÌNH LUẬN (TOGGLE STATUS)
        // ==========================================
        [HttpPost]
        public ActionResult ToggleStatus(int id)
        {
            try
            {
                var comment = db.COMMENT.Find(id);
                if (comment != null)
                {
                    // Đảo ngược trạng thái: 1 (Hiển thị) <-> 0 (Ẩn)
                    comment.Status = (comment.Status == 1) ? 0 : 1;
                    db.SaveChanges();
                    TempData["Success"] = comment.Status == 1 ? "Đã khôi phục hiển thị bình luận!" : "Đã ẩn bình luận thành công!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            }
            return RedirectToAction("Index");
        }

        // ==========================================
        // 3. XÓA BÌNH LUẬN
        // ==========================================
        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                var comment = db.COMMENT.Find(id);
                if (comment != null)
                {
                    db.COMMENT.Remove(comment);
                    db.SaveChanges();
                    TempData["Success"] = "Đã xóa vĩnh viễn bình luận!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Không thể xóa: " + ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}