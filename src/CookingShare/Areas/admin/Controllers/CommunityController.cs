using CookingShare.Models;
using CookingShare.Helpers;
using System;
using System.IO;
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;

namespace CookingShare.Areas.Admin.Controllers
{
    [Authorize]
    public class CommunityController : Controller
    {
        CookingShareDBEntities db = new CookingShareDBEntities();


        //  HIỂN THỊ LƯỚI ẢNH THÀNH QUẢ (KÈM TÌM KIẾM)
        public ActionResult Index(string searchString)
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            // Chuẩn hóa Routing
            if (currentAcc == null || currentAcc.Role != 1) return RedirectToAction("Login", "Auth", new { area = "admin" });

            var cooksnaps = db.COOKSNAP
                              .Include(c => c.ACCOUNT)
                              .Include(c => c.ACCOUNT.PROFILE)
                              .Include(c => c.RECIPE)
                              .AsQueryable();

            // LỌC THEO TỪ KHÓA TÌM KIẾM
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower().Trim();

                // MẸO XỬ LÝ MÃ #SNAP: Bóc tách phần số ra khỏi từ khóa
                int searchId = -1;
                string cleanSearch = searchString.Replace("#snap", "").Replace("snap", "").Trim();
                bool isNumeric = int.TryParse(cleanSearch, out searchId);

                // Áp dụng bộ lọc
                cooksnaps = cooksnaps.Where(c =>
                    (isNumeric && c.ID == searchId) || // Nếu tìm ra số, lọc chính xác theo ID (Mã ảnh)
                    (c.ACCOUNT != null && c.ACCOUNT.UserName.ToLower().Contains(searchString)) ||
                    (c.ACCOUNT != null && c.ACCOUNT.PROFILE != null && c.ACCOUNT.PROFILE.FullName.ToLower().Contains(searchString)) ||
                    (c.RECIPE != null && c.RECIPE.Name.ToLower().Contains(searchString)) ||
                    (c.Content != null && c.Content.ToLower().Contains(searchString))
                );
            }

            ViewBag.CurrentSearch = searchString;

            // Lấy danh sách Báo cáo
            ViewBag.Reports = db.REPORT.Where(r => r.ReportType == 3).OrderByDescending(r => r.CreateDate).ToList();

            return View(cooksnaps.OrderByDescending(c => c.CreateDate).ToList());
        }

        //  ẨN / HIỆN HÌNH ẢNH THÀNH QUẢ
        [HttpPost]
        [ValidateAntiForgeryToken] // Chống giả mạo request (CSRF)
        public ActionResult ToggleCooksnapStatus(int id)
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return RedirectToAction("Login", "Auth", new { area = "admin" });

            try
            {
                var cooksnap = db.COOKSNAP.Find(id);
                if (cooksnap != null)
                {
                    cooksnap.Status = cooksnap.Status == 1 ? 0 : 1;

                    // Gửi thông báo cho người dùng nếu ảnh bị ẩn
                    if (cooksnap.Status == 0)
                    {
                        db.NOTIFICATION.Add(new NOTIFICATION
                        {
                            AccountID = cooksnap.AccountID,
                            Content = "Một bức ảnh Khoe thành quả của bạn đã bị Admin tạm ẩn do vi phạm tiêu chuẩn cộng đồng.",
                            LinkURL = "#",
                            IsRead = false,
                            CreateDate = DateTime.Now
                        });

                        //  TỰ ĐỘNG ĐÓNG REPORT VÀ CẢM ƠN NGƯỜI BÁO CÁO NHƯ COMMENT CONTROLLER
                        var relatedReports = db.REPORT.Where(r => r.TargetID == cooksnap.ID && r.ReportType == 3 && r.Status == 0).ToList();
                        foreach (var report in relatedReports)
                        {
                            report.Status = 1;
                            if (report.ReporterID != null)
                            {
                                db.NOTIFICATION.Add(new NOTIFICATION
                                {
                                    AccountID = (int)report.ReporterID,
                                    Content = "Cảm ơn bạn! Bức ảnh thành quả vi phạm mà bạn báo cáo đã được Admin xử lý và gỡ bỏ.",
                                    LinkURL = "#",
                                    IsRead = false,
                                    CreateDate = DateTime.Now
                                });
                            }
                        }

                        TempData["Success"] = "Đã ẨN bức ảnh thành quả, gửi cảnh báo và xử lý các báo cáo liên quan!";
                    }
                    else
                    {
                        TempData["Success"] = "Đã KHÔI PHỤC hiển thị bức ảnh thành quả!";
                    }

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật trạng thái: " + ex.Message;
            }
            return RedirectToAction("Index");
        }


        // XÓA VĨNH VIỄN ẢNH VÀ BÌNH LUẬN KÈM THEO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCooksnap(int id)
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return RedirectToAction("Login", "Auth", new { area = "admin" });

            try
            {
                var cooksnap = db.COOKSNAP.Find(id);
                if (cooksnap != null)
                {
                    //  Xóa tất cả các BÌNH LUẬN (Cooksnap_Comment) thuộc về bức ảnh này
                    var relatedComments = db.COOKSNAP_COMMENT.Where(c => c.CooksnapID == id).ToList();
                    if (relatedComments.Any())
                    {
                        db.COOKSNAP_COMMENT.RemoveRange(relatedComments);
                    }

                    //  Xóa tất cả các BÁO CÁO (Report) liên quan đến bức ảnh này
                    var relatedReports = db.REPORT.Where(r => r.TargetID == id && r.ReportType == 3).ToList();
                    if (relatedReports.Any())
                    {
                        db.REPORT.RemoveRange(relatedReports);
                    }

                    //  DỌN RÁC FILE VẬT LÝ BẰNG FILEHELPER
                    FileHelper.DeleteImage("~/assets/images/cooksnaps/", cooksnap.ImageName);

                    // Gửi thông báo cho User khi xóa vĩnh viễn
                    db.NOTIFICATION.Add(new NOTIFICATION
                    {
                        AccountID = cooksnap.AccountID,
                        Content = "Một bức ảnh Khoe thành quả của bạn đã bị xóa vĩnh viễn khỏi hệ thống do vi phạm nghiêm trọng.",
                        LinkURL = "#",
                        IsRead = false,
                        CreateDate = DateTime.Now
                    });

                    //  An toàn để xóa bức ảnh chính trong Database (COOKSNAP)
                    db.COOKSNAP.Remove(cooksnap);
                    db.SaveChanges();

                    TempData["Success"] = "Đã xóa vĩnh viễn Thành quả, dọn dẹp file ảnh và toàn bộ dữ liệu liên quan!";
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy dữ liệu ảnh thành quả để xóa.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Không thể xóa: " + ex.Message;
            }
            return RedirectToAction("Index");
        }

        // XỬ LÝ BÁO CÁO (BỎ QUA BÁO CÁO SAI SỰ THẬT)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResolveReport(int id, int status)
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return RedirectToAction("Login", "Auth", new { area = "admin" });

            try
            {
                var report = db.REPORT.Find(id);
                if (report != null)
                {
                    report.Status = status;
                    db.SaveChanges();
                    TempData["Success"] = "Đã bỏ qua báo cáo này thành công!";
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy dữ liệu báo cáo trong hệ thống.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi xử lý: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // Dọn dẹp kết nối Database khi Controller bị hủy
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