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


        // HIỂN THỊ GỘP 3 BẢNG VÀO CHUNG 1 TRANG
        public ActionResult Index(int? statusFilter)
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            // Chuẩn hóa Routing
            if (currentAcc == null || currentAcc.Role != 1) return RedirectToAction("Login", "Auth", new { area = "admin" });

            //  BÌNH LUẬN CÔNG THỨC (COMMENT) 
            ViewBag.TotalComments = db.COMMENT.Count();
            ViewBag.HiddenComments = db.COMMENT.Count(c => c.Status == 0);
            ViewBag.CurrentFilter = statusFilter;

            var comments = db.COMMENT.Include(c => c.ACCOUNT).Include(c => c.RECIPE).AsQueryable();
            if (statusFilter.HasValue) comments = comments.Where(c => c.Status == statusFilter.Value);

            //  BÌNH LUẬN THÀNH QUẢ (COOKSNAP_COMMENT) 
            ViewBag.CooksnapComments = db.COOKSNAP_COMMENT
                                         .Include(c => c.ACCOUNT)
                                         .Include(c => c.COOKSNAP)
                                         .OrderByDescending(c => c.CreateDate).ToList();

            //  BÁO CÁO VI PHẠM (REPORT)
            // CHỈ LẤY BÁO CÁO BÌNH LUẬN (Type = 2) VÀ BÁO CÁO BÌNH LUẬN THÀNH QUẢ (Type = 5)
            ViewBag.Reports = db.REPORT.Where(r => r.ReportType == 2 || r.ReportType == 5)
                                       .OrderByDescending(r => r.CreateDate).ToList();

            return View(comments.OrderByDescending(c => c.CreateDate).ToList());
        }


        //  ẨN BÌNH LUẬN CÔNG THỨC -> GỬI THÔNG BÁO & ĐÓNG REPORT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(int id)
        {
            // Chặn User thường
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return RedirectToAction("Login", "Auth", new { area = "admin" });

            try
            {
                var comment = db.COMMENT.Find(id);
                if (comment != null)
                {
                    if (comment.Status == 1)
                    {
                        comment.Status = 0;

                        // Kiểm tra HasValue để tránh lỗi sập web nếu AccountID bị null
                        if (comment.AccountID.HasValue)
                        {
                            db.NOTIFICATION.Add(new NOTIFICATION
                            {
                                AccountID = comment.AccountID.Value,
                                Content = "Bình luận của bạn tại một công thức đã bị Admin gỡ bỏ do vi phạm tiêu chuẩn cộng đồng.",
                                LinkURL = "#",
                                IsRead = false,
                                CreateDate = DateTime.Now
                            });
                        }

                        // TÌM VÀ ĐÓNG TẤT CẢ BÁO CÁO LIÊN QUAN (Type = 2)
                        var relatedReports = db.REPORT.Where(r => r.TargetID == comment.ID && r.ReportType == 2 && r.Status == 0).ToList();

                        foreach (var report in relatedReports)
                        {
                            report.Status = 1;

                            if (report.ReporterID != null)
                            {
                                db.NOTIFICATION.Add(new NOTIFICATION
                                {
                                    AccountID = (int)report.ReporterID,
                                    Content = "Cảm ơn bạn! Bình luận vi phạm mà bạn báo cáo đã được Admin xử lý và gỡ bỏ.",
                                    LinkURL = "#",
                                    IsRead = false,
                                    CreateDate = DateTime.Now
                                });
                            }
                        }

                        TempData["Success"] = "Đã ẨN bình luận, gửi cảnh báo và đóng các báo cáo liên quan!";
                    }
                    else
                    {
                        comment.Status = 1;

                        if (comment.AccountID.HasValue)
                        {
                            db.NOTIFICATION.Add(new NOTIFICATION
                            {
                                AccountID = comment.AccountID.Value,
                                Content = "Bình luận của bạn đã được Admin xem xét lại và khôi phục hiển thị.",
                                LinkURL = "#",
                                IsRead = false,
                                CreateDate = DateTime.Now
                            });
                        }

                        TempData["Success"] = "Đã khôi phục hiển thị bình luận!";
                    }

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            }
            return RedirectToAction("Index");
        }

        // ẨN/HIỆN BÌNH LUẬN THÀNH QUẢ (COOKSNAP)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleCooksnapStatus(int id)
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return RedirectToAction("Login", "Auth", new { area = "admin" });

            try
            {
                var comment = db.COOKSNAP_COMMENT.Find(id);
                if (comment != null)
                {
                    if (comment.Status == 1)
                    {
                        comment.Status = 0;

                        db.NOTIFICATION.Add(new NOTIFICATION
                        {
                            AccountID = comment.AccountID,
                            Content = "Bình luận của bạn tại phần Khoe thành quả đã bị Admin gỡ bỏ do vi phạm tiêu chuẩn cộng đồng.",
                            LinkURL = "#",
                            IsRead = false,
                            CreateDate = DateTime.Now
                        });

                        var relatedReports = db.REPORT.Where(r => r.TargetID == comment.ID && r.ReportType == 5 && r.Status == 0).ToList();
                        foreach (var report in relatedReports)
                        {
                            report.Status = 1;
                            if (report.ReporterID != null)
                            {
                                db.NOTIFICATION.Add(new NOTIFICATION
                                {
                                    AccountID = (int)report.ReporterID,
                                    Content = "Cảm ơn bạn! Bình luận vi phạm mà bạn báo cáo đã được Admin xử lý và gỡ bỏ.",
                                    LinkURL = "#",
                                    IsRead = false,
                                    CreateDate = DateTime.Now
                                });
                            }
                        }

                        TempData["Success"] = "Đã ẨN bình luận thành quả, gửi cảnh báo và đóng báo cáo liên quan!";
                    }
                    else
                    {
                        comment.Status = 1;
                        TempData["Success"] = "Đã khôi phục hiển thị bình luận thành quả!";
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex) { TempData["Error"] = "Lỗi: " + ex.Message; }
            return RedirectToAction("Index");
        }


        // XÓA VĨNH VIỄN 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            // BỌC THÉP BẢO MẬT
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return RedirectToAction("Login", "Auth", new { area = "admin" });

            try
            {
                var comment = db.COMMENT.Find(id);
                if (comment != null)
                {
                    // Kiểm tra HasValue
                    if (comment.AccountID.HasValue)
                    {
                        db.NOTIFICATION.Add(new NOTIFICATION
                        {
                            AccountID = comment.AccountID.Value,
                            Content = "Một bình luận của bạn đã bị xóa vĩnh viễn khỏi hệ thống do vi phạm nghiêm trọng.",
                            LinkURL = "#",
                            IsRead = false,
                            CreateDate = DateTime.Now
                        });
                    }

                    // Xóa luôn các report rác liên quan
                    var relatedReports = db.REPORT.Where(r => r.TargetID == comment.ID && r.ReportType == 2).ToList();
                    db.REPORT.RemoveRange(relatedReports);

                    db.COMMENT.Remove(comment);
                    db.SaveChanges();
                    TempData["Success"] = "Đã xóa vĩnh viễn bình luận và gửi thông báo!";
                }
            }
            catch (Exception ex) { TempData["Error"] = "Không thể xóa: " + ex.Message; }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCooksnapComment(int id)
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return RedirectToAction("Login", "Auth", new { area = "admin" });

            try
            {
                var comment = db.COOKSNAP_COMMENT.Find(id);
                if (comment != null)
                {
                    db.NOTIFICATION.Add(new NOTIFICATION
                    {
                        AccountID = comment.AccountID,
                        Content = "Bình luận của bạn tại phần Khoe thành quả đã bị xóa vĩnh viễn do vi phạm nghiêm trọng.",
                        LinkURL = "#",
                        IsRead = false,
                        CreateDate = DateTime.Now
                    });

                    // Xóa report rác
                    var relatedReports = db.REPORT.Where(r => r.TargetID == comment.ID && r.ReportType == 5).ToList();
                    db.REPORT.RemoveRange(relatedReports);

                    db.COOKSNAP_COMMENT.Remove(comment);
                    db.SaveChanges();
                    TempData["Success"] = "Đã xóa vĩnh viễn và gửi thông báo!";
                }
            }
            catch (Exception ex) { TempData["Error"] = "Lỗi xóa: " + ex.Message; }
            return RedirectToAction("Index");
        }


        //  XỬ LÝ BÁO CÁO ĐỘC LẬP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResolveReport(int id, int status)
        {
            // BỌC THÉP BẢO MẬT
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return RedirectToAction("Login", "Auth", new { area = "admin" });

            try
            {
                var report = db.REPORT.Find(id);
                if (report != null)
                {
                    report.Status = status;
                    db.SaveChanges();
                    TempData["Success"] = "Đã cập nhật trạng thái báo cáo thành công!";
                }
            }
            catch (Exception ex) { TempData["Error"] = "Lỗi cập nhật: " + ex.Message; }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { db.Dispose(); }
            base.Dispose(disposing);
        }
    }
}