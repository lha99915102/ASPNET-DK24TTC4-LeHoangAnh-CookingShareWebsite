using CookingShare.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CookingShare.Areas.Admin.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        CookingShareDBEntities db = new CookingShareDBEntities();

        //  HIỂN THỊ DANH SÁCH DANH MỤC
        public ActionResult Index()
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            //  Chuẩn hóa Routing
            if (currentAcc == null || currentAcc.Role != 1) return RedirectToAction("Login", "Auth", new { area = "admin" });

            // Lấy danh sách danh mục, sắp xếp mới nhất lên đầu
            var categories = db.CATEGORY.OrderByDescending(c => c.ID).ToList();

            return View(categories);
        }


        //  THÊM MỚI HOẶC CẬP NHẬT DANH MỤC
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(CATEGORY model, HttpPostedFileBase ImageFile)
        {
            // Chặn User thường dùng Postman chọc vào API của Admin
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return RedirectToAction("Login", "Auth", new { area = "admin" });

            try
            {
                if (ModelState.IsValid)
                {
                    if (model.ID == 0)
                    {
                        // Xử lý lưu ảnh bằng FileHelper
                        if (ImageFile != null && ImageFile.ContentLength > 0)
                        {
                            string prefix = "cat_" + Guid.NewGuid().ToString("N").Substring(0, 6);
                            model.ImageURL = FileHelper.UploadAndReplaceImage(ImageFile, "~/assets/images/categories/", prefix, null);
                        }

                        db.CATEGORY.Add(model);
                        db.SaveChanges();

                        // Đập nát Cache cũ để trang chủ nhận Danh mục mới
                        HttpContext.Cache.Remove("HomeCategories");

                        TempData["Success"] = "Đã thêm danh mục mới thành công!";
                    }
                    else
                    {
                        var existingCat = db.CATEGORY.Find(model.ID);
                        if (existingCat != null)
                        {
                            existingCat.Name = model.Name;

                            //  Nếu Admin upload ảnh mới -> tự động dọn rác ảnh cũ
                            if (ImageFile != null && ImageFile.ContentLength > 0)
                            {
                                string prefix = "cat_" + existingCat.ID;
                                existingCat.ImageURL = FileHelper.UploadAndReplaceImage(ImageFile, "~/assets/images/categories/", prefix, existingCat.ImageURL);
                            }

                            db.SaveChanges();

                            //  Đập nát Cache cũ để trang chủ cập nhật tên/ảnh Danh mục
                            HttpContext.Cache.Remove("HomeCategories");

                            TempData["Success"] = "Đã cập nhật danh mục thành công!";
                        }
                    }
                }
                else
                {
                    TempData["Error"] = "Vui lòng nhập đầy đủ tên danh mục.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi lưu: " + ex.Message;
            }

            return RedirectToAction("Index");
        }


        // XÓA DANH MỤC
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            // Chặn User thường gọi lệnh Xóa
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return RedirectToAction("Login", "Auth", new { area = "admin" });

            try
            {
                var cat = db.CATEGORY.Find(id);
                if (cat != null)
                {
                    // KIỂM TRA CHỦ ĐỘNG KHÓA NGOẠI: Nếu danh mục đang có chứa bài viết thì không cho xóa
                    int recipeCount = db.RECIPE.Count(r => r.CategoryID == id);
                    if (recipeCount > 0)
                    {
                        TempData["Error"] = $"Không thể xóa! Đang có {recipeCount} công thức thuộc danh mục này. Bạn cần xóa hoặc chuyển đổi các công thức đó trước.";
                        return RedirectToAction("Index");
                    }

                    // Dọn rác file ảnh vật lý của Danh mục này trước khi xóa Database
                    FileHelper.DeleteImage("~/assets/images/categories/", cat.ImageURL);

                    // Nếu danh mục trống (recipeCount == 0), tiến hành xóa
                    db.CATEGORY.Remove(cat);
                    db.SaveChanges();

                    //  Làm mới lại trang chủ sau khi xóa
                    HttpContext.Cache.Remove("HomeCategories");

                    TempData["Success"] = "Đã xóa danh mục và dọn dẹp ảnh thành công!";
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy danh mục để xóa.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi hệ thống: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // Dọn dẹp kết nối DB
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