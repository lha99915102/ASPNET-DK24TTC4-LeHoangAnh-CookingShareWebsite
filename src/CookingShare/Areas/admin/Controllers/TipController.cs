using CookingShare.Models;
using CookingShare.Helpers; 
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


        //  HIỂN THỊ DANH SÁCH MẸO VẶT
        public ActionResult Index()
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return Redirect("~/Admin/Auth/Login");

            var tips = db.TIP.Include(t => t.ACCOUNT).OrderByDescending(t => t.CreateDate).ToList();

            ViewBag.TotalTips = tips.Count;

            return View(tips);
        }


        // THÊM MỚI HOẶC CẬP NHẬT MẸO VẶT
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Cho phép lưu nội dung HTML
        public ActionResult Save(TIP model, HttpPostedFileBase ImageFile)
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return Redirect("~/Admin/Auth/Login");

            try
            {
                if (model.ID == 0) // THÊM MỚI 
                {
                    model.CreateDate = DateTime.Now;
                    model.AccountID = currentAcc.ID;

                    if (ImageFile != null && ImageFile.ContentLength > 0)
                    {
                        string prefix = "tip_" + Guid.NewGuid().ToString("N").Substring(0, 8);
                        model.ImageURL = FileHelper.UploadAndReplaceImage(ImageFile, "~/assets/images/tips/", prefix, null);
                    }

                    db.TIP.Add(model);
                    db.SaveChanges();
                    TempData["Success"] = "Đã đăng mẹo vặt mới thành công!";
                }
                else // CẬP NHẬT
                {
                    var existingTip = db.TIP.Find(model.ID);
                    if (existingTip != null)
                    {
                        existingTip.Title = model.Title;
                        existingTip.Content = model.Content;

                        if (ImageFile != null && ImageFile.ContentLength > 0)
                        {
                            string prefix = "tip_" + existingTip.ID;
                            existingTip.ImageURL = FileHelper.UploadAndReplaceImage(ImageFile, "~/assets/images/tips/", prefix, existingTip.ImageURL);
                        }

                        TempData["Success"] = "Đã cập nhật mẹo vặt thành công!";
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        //  XÓA MẸO VẶT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return Redirect("~/Admin/Auth/Login");

            try
            {
                var tip = db.TIP.Find(id);
                if (tip != null)
                {
                    FileHelper.DeleteImage("~/assets/images/tips/", tip.ImageURL);

                    db.TIP.Remove(tip);
                    db.SaveChanges();
                    TempData["Success"] = "Đã xóa mẹo vặt và dọn dẹp dữ liệu thành công!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi xóa: " + ex.Message;
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { db.Dispose(); }
            base.Dispose(disposing);
        }
    }
}