using CookingShare.Models;
using System;
using System.IO;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;

namespace CookingShare.Areas.Admin.Controllers
{
    [Authorize]
    public class TipController : Controller
    {
        CookingShareDBEntities db = new CookingShareDBEntities();

        // ==========================================
        // 1. HIỂN THỊ DANH SÁCH MẸO VẶT
        // ==========================================
        public ActionResult Index()
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return Redirect("~/Admin/Auth/Login");

            // Lấy danh sách Mẹo vặt, kèm theo thông tin người đăng (ACCOUNT)
            var tips = db.TIP.Include(t => t.ACCOUNT).OrderByDescending(t => t.CreateDate).ToList();

            ViewBag.TotalTips = tips.Count;

            return View(tips);
        }

        // ==========================================
        // 2. THÊM MỚI HOẶC CẬP NHẬT MẸO VẶT
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Cho phép lưu nội dung HTML nếu bạn dùng bộ gõ như Summernote
        public ActionResult Save(TIP model, HttpPostedFileBase ImageFile)
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null) return Redirect("~/Admin/Auth/Login");

            try
            {
                // Xử lý Upload Hình ảnh
                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    string uploadDir = "~/Content/Images/Tips/";
                    bool exists = Directory.Exists(Server.MapPath(uploadDir));
                    if (!exists) Directory.CreateDirectory(Server.MapPath(uploadDir));

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                    string path = Path.Combine(Server.MapPath(uploadDir), fileName);
                    ImageFile.SaveAs(path);

                    model.ImageURL = "/Content/Images/Tips/" + fileName;
                }

                if (model.ID == 0) // THÊM MỚI
                {
                    model.CreateDate = DateTime.Now; // Lấy ngày giờ hiện tại
                    model.AccountID = currentAcc.ID; // Gán ID của người đang thao tác

                    db.TIP.Add(model);
                    TempData["Success"] = "Đã đăng mẹo vặt mới thành công!";
                }
                else // CẬP NHẬT
                {
                    var existingTip = db.TIP.Find(model.ID);
                    if (existingTip != null) 
                    {
                        existingTip.Title = model.Title;
                        existingTip.Content = model.Content;
                        // Không cập nhật CreateDate và AccountID để giữ nguyên lịch sử

                        if (!string.IsNullOrEmpty(model.ImageURL))
                        {
                            existingTip.ImageURL = model.ImageURL;
                        }

                        TempData["Success"] = "Đã cập nhật mẹo vặt thành công!";
                    }
                }
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // ==========================================
        // 3. XÓA MẸO VẶT
        // ==========================================
        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                var tip = db.TIP.Find(id);
                if (tip != null)
                {
                    db.TIP.Remove(tip);
                    db.SaveChanges();
                    TempData["Success"] = "Đã xóa mẹo vặt thành công!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi xóa: " + ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}