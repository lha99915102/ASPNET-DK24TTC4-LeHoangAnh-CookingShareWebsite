using CookingShare.Models;
using CookingShare.Helpers;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CookingShare.Areas.Admin.Controllers
{
    [Authorize]
    public class BannerController : Controller
    {
        CookingShareDBEntities db = new CookingShareDBEntities();


        //  HIỂN THỊ DANH SÁCH BANNER
        public ActionResult Index(bool? isActiveFilter)
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            // Dùng RedirectToAction an toàn
            if (currentAcc == null || currentAcc.Role != 1) return RedirectToAction("Login", "Auth", new { area = "admin" });

            var banners = db.BANNER.AsQueryable();

            if (isActiveFilter.HasValue)
            {
                banners = banners.Where(b => b.IsActive == isActiveFilter.Value);
            }

            ViewBag.CurrentFilter = isActiveFilter;

            // Sắp xếp theo Position (thứ tự hiển thị) tăng dần
            return View(banners.OrderBy(b => b.Position).ToList());
        }


        // THÊM MỚI HOẶC CẬP NHẬT BANNER
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(BANNER model, HttpPostedFileBase ImageFile)
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return RedirectToAction("Login", "Auth", new { area = "admin" });

            try
            {
                if (model.ID == 0)
                {
                    if (ImageFile == null || ImageFile.ContentLength == 0)
                    {
                        TempData["Error"] = "Vui lòng chọn hình ảnh cho banner mới!";
                        return RedirectToAction("Index");
                    }

                    // Dùng FileHelper để lưu ảnh mới (Truyền null vào oldFileName vì là tạo mới)
                    string prefix = "banner_" + Guid.NewGuid().ToString("N").Substring(0, 8);
                    model.ImageURL = FileHelper.UploadAndReplaceImage(ImageFile, "~/assets/images/banners/", prefix, null);

                    db.BANNER.Add(model);
                    db.SaveChanges();

                    //  Đập nát Cache cũ để trang chủ nhận Banner mới
                    HttpContext.Cache.Remove("HomeBanners");

                    TempData["Success"] = "Đã thêm banner mới thành công!";
                }
                else
                {
                    var existingBanner = db.BANNER.Find(model.ID);
                    if (existingBanner != null)
                    {
                        existingBanner.Title = model.Title;
                        existingBanner.LinkURL = model.LinkURL;
                        existingBanner.Position = model.Position;
                        existingBanner.IsActive = model.IsActive;

                        // Dùng FileHelper để tự động dọn rác ảnh cũ và lưu ảnh mới
                        if (ImageFile != null && ImageFile.ContentLength > 0)
                        {
                            string prefix = "banner_" + Guid.NewGuid().ToString("N").Substring(0, 8);
                            existingBanner.ImageURL = FileHelper.UploadAndReplaceImage(ImageFile, "~/assets/images/banners/", prefix, existingBanner.ImageURL);
                        }

                        db.SaveChanges();

                        // BỌC THÉP CACHE: Đập nát Cache cũ để trang chủ nhận Banner mới cập nhật
                        HttpContext.Cache.Remove("HomeBanners");

                        TempData["Success"] = "Cập nhật banner thành công!";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            }

            return RedirectToAction("Index");
        }


        // XÓA BANNER
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return RedirectToAction("Login", "Auth", new { area = "admin" });

            try
            {
                var banner = db.BANNER.Find(id);
                if (banner != null)
                {
                    // Dùng FileHelper để dọn rác vật lý
                    FileHelper.DeleteImage("~/assets/images/banners/", banner.ImageURL);

                    db.BANNER.Remove(banner);
                    db.SaveChanges();

                    // BỌC THÉP CACHE: Đập nát Cache để trang chủ làm mất Banner bị xóa
                    HttpContext.Cache.Remove("HomeBanners");

                    TempData["Success"] = "Đã xóa banner và dọn dẹp ảnh vật lý thành công!";
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