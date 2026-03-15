using CookingShare.Models;
using System;
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;

namespace CookingShare.Areas.Admin.Controllers
{
    [Authorize]
    public class CommunityController : Controller
    {
        CookingShareDBEntities db = new CookingShareDBEntities();

        // ==========================================
        // 1. HIỂN THỊ DANH SÁCH BÁO CÁO (REPORT)
        // ==========================================
        public ActionResult Index(int? statusFilter)
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return Redirect("~/Admin/Auth/Login");

            // Thống kê nhanh
            ViewBag.TotalReports = db.REPORT.Count();
            ViewBag.PendingReports = db.REPORT.Count(r => r.Status == 0); // 0: Chờ xử lý
            ViewBag.ResolvedReports = db.REPORT.Count(r => r.Status == 1 || r.Status == 2); // Đã xử lý hoặc bỏ qua
            ViewBag.CurrentFilter = statusFilter;

            // Lấy danh sách báo cáo, kèm thông tin người gửi báo cáo
            var reports = db.REPORT.Include(r => r.ACCOUNT).AsQueryable();

            if (statusFilter.HasValue)
            {
                reports = reports.Where(r => r.Status == statusFilter.Value);
            }

            return View(reports.OrderByDescending(r => r.CreateDate).ToList());
        }

        // ==========================================
        // 2. CẬP NHẬT TRẠNG THÁI XỬ LÝ
        // ==========================================
        [HttpPost]
        public ActionResult UpdateStatus(int id, int status)
        {
            try
            {
                var report = db.REPORT.Find(id);
                if (report != null)
                {
                    report.Status = status; // 1: Đã xử lý vi phạm, 2: Bỏ qua (Báo cáo không hợp lệ)
                    db.SaveChanges();
                    TempData["Success"] = "Đã cập nhật trạng thái xử lý báo cáo!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            }
            return RedirectToAction("Index");
        }

        // ==========================================
        // 3. XÓA BÁO CÁO
        // ==========================================
        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                var report = db.REPORT.Find(id);
                if (report != null)
                {
                    db.REPORT.Remove(report);
                    db.SaveChanges();
                    TempData["Success"] = "Đã xóa bản ghi báo cáo khỏi hệ thống!";
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