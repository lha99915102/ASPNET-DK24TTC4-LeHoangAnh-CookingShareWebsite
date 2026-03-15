using CookingShare.Models;
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

        // ==========================================
        // 1. HIỂN THỊ DANH SÁCH BANNER
        // ==========================================
        public ActionResult Index(bool? isActiveFilter)
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return Redirect("~/Admin/Auth/Login");

            var banners = db.BANNER.AsQueryable();

            if (isActiveFilter.HasValue)
            {
                banners = banners.Where(b => b.IsActive == isActiveFilter.Value);
            }

            ViewBag.CurrentFilter = isActiveFilter;

            // Sắp xếp theo Position (thứ tự hiển thị) tăng dần
            return View(banners.OrderBy(b => b.Position).ToList());
        }

        // ==========================================
        // 2. THÊM MỚI HOẶC CẬP NHẬT BANNER
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(BANNER model, HttpPostedFileBase ImageFile)
        {
            try
            {
                // Xử lý Upload Ảnh
                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    string uploadDir = "~/Content/Images/Banners/";
                    bool exists = Directory.Exists(Server.MapPath(uploadDir));
                    if (!exists) Directory.CreateDirectory(Server.MapPath(uploadDir));

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                    string path = Path.Combine(Server.MapPath(uploadDir), fileName);
                    ImageFile.SaveAs(path);

                    model.ImageURL = "/Content/Images/Banners/" + fileName;
                }

                if (model.ID == 0) // THÊM MỚI
                {
                    if (string.IsNullOrEmpty(model.ImageURL))
                    {
                        TempData["Error"] = "Vui lòng chọn hình ảnh cho banner mới!";
                        return RedirectToAction("Index");
                    }
                    db.BANNER.Add(model);
                    TempData["Success"] = "Đã thêm banner mới thành công!";
                }
                else // CẬP NHẬT
                {
                    var existingBanner = db.BANNER.Find(model.ID);
                    if (existingBanner != null)
                    {
                        existingBanner.Title = model.Title;
                        existingBanner.LinkURL = model.LinkURL;
                        existingBanner.Position = model.Position;
                        existingBanner.IsActive = model.IsActive;

                        // Chỉ cập nhật ảnh nếu Admin up ảnh mới
                        if (!string.IsNullOrEmpty(model.ImageURL))
                        {
                            existingBanner.ImageURL = model.ImageURL;
                        }

                        TempData["Success"] = "Cập nhật banner thành công!";
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
        // 3. XÓA BANNER
        // ==========================================
        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                var banner = db.BANNER.Find(id);
                if (banner != null)
                {
                    db.BANNER.Remove(banner);
                    db.SaveChanges();
                    TempData["Success"] = "Đã xóa banner thành công!";
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