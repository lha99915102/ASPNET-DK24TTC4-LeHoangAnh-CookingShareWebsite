using CookingShare.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace CookingShare.Areas.Admin.Controllers
{
    [Authorize]
    public class TagController : Controller
    {
        CookingShareDBEntities db = new CookingShareDBEntities();

        // ==========================================
        // 1. HIỂN THỊ DANH SÁCH TAG
        // ==========================================
        public ActionResult Index(string typeFilter = "")
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return Redirect("~/Admin/Auth/Login");

            // Lấy danh sách Tag
            var tags = db.TAG.AsQueryable();

            // Lọc theo loại Tag nếu có
            if (!string.IsNullOrEmpty(typeFilter))
            {
                tags = tags.Where(t => t.TagType == typeFilter);
            }

            ViewBag.CurrentFilter = typeFilter;

            return View(tags.OrderByDescending(t => t.ID).ToList());
        }

        // ==========================================
        // 2. THÊM MỚI HOẶC CẬP NHẬT TAG
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(TAG model)
        {
            try
            {
                if (model.ID == 0) // THÊM MỚI
                {
                    // Kiểm tra xem tên Tag đã tồn tại chưa để tránh trùng lặp
                    var checkExist = db.TAG.FirstOrDefault(t => t.TagName.ToLower() == model.TagName.ToLower());
                    if (checkExist != null)
                    {
                        TempData["Error"] = "Tên Tag này đã tồn tại trong hệ thống!";
                        return RedirectToAction("Index");
                    }

                    db.TAG.Add(model);
                    TempData["Success"] = "Đã thêm Tag mới thành công!";
                }
                else // CẬP NHẬT
                {
                    var existingTag = db.TAG.Find(model.ID);
                    if (existingTag != null)
                    {
                        existingTag.TagName = model.TagName;
                        existingTag.TagType = model.TagType;
                        TempData["Success"] = "Đã cập nhật Tag thành công!";
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
        // 3. XÓA TAG
        // ==========================================
        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                var tag = db.TAG.Find(id);
                if (tag != null)
                {
                    db.TAG.Remove(tag);
                    db.SaveChanges();
                    TempData["Success"] = "Đã xóa Tag thành công!";
                }
            }
            catch (Exception)
            {
                // Bắt lỗi khóa ngoại nếu Tag này đang được gán cho một Công thức nào đó (bảng RECIPE_TAG_MAP)
                TempData["Error"] = "Không thể xóa! Tag này đang được sử dụng trong các bài viết Công thức.";
            }
            return RedirectToAction("Index");
        }
    }
}