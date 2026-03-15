using CookingShare.Models;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CookingShare.Areas.Admin.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        CookingShareDBEntities db = new CookingShareDBEntities();

        // ==========================================
        // 1. HIỂN THỊ DANH SÁCH
        // ==========================================
        public ActionResult Index()
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return Redirect("~/Admin/Auth/Login");

            // Lấy danh sách danh mục, sắp xếp mới nhất lên đầu
            var categories = db.CATEGORY.OrderByDescending(c => c.ID).ToList();

            return View(categories);
        }

        // ==========================================
        // 2. THÊM MỚI HOẶC CẬP NHẬT
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(CATEGORY model, HttpPostedFileBase ImageFile)
        {
            try
            {
                // Xử lý Upload Hình ảnh
                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    string uploadDir = "~/Content/Images/Categories/";
                    bool exists = Directory.Exists(Server.MapPath(uploadDir));
                    if (!exists) Directory.CreateDirectory(Server.MapPath(uploadDir));

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                    string path = Path.Combine(Server.MapPath(uploadDir), fileName);
                    ImageFile.SaveAs(path);

                    model.ImageURL = "/Content/Images/Categories/" + fileName;
                }

                if (model.ID == 0) // THÊM MỚI
                {
                    db.CATEGORY.Add(model);
                    TempData["Success"] = "Đã thêm danh mục mới thành công!";
                }
                else // CẬP NHẬT
                {
                    var existingCat = db.CATEGORY.Find(model.ID);
                    if (existingCat != null)
                    {
                        existingCat.Name = model.Name;

                        // Chỉ cập nhật ảnh nếu người dùng có upload ảnh mới
                        if (!string.IsNullOrEmpty(model.ImageURL))
                        {
                            existingCat.ImageURL = model.ImageURL;
                        }

                        TempData["Success"] = "Đã cập nhật danh mục thành công!";
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
        // 3. XÓA DANH MỤC
        // ==========================================
        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                var cat = db.CATEGORY.Find(id);
                if (cat != null)
                {
                    db.CATEGORY.Remove(cat);
                    db.SaveChanges();
                    TempData["Success"] = "Đã xóa danh mục thành công!";
                }
            }
            catch (Exception)
            {
                // Bắt lỗi khi Danh mục này đang có Bài viết (Recipe) nằm bên trong
                TempData["Error"] = "Không thể xóa! Đang có công thức sử dụng danh mục này.";
            }
            return RedirectToAction("Index");
        }
    }
}