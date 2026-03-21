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

        // HIỂN THỊ DANH SÁCH TAG
        public ActionResult Index(string typeFilter = "")
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return Redirect("~/Admin/Auth/Login");

            // Lấy danh sách Tag
            var tags = db.TAG.AsQueryable();

            // Lọc theo loại Tag
            if (!string.IsNullOrEmpty(typeFilter))
            {
                tags = tags.Where(t => t.TagType == typeFilter);
            }

            ViewBag.CurrentFilter = typeFilter;

            return View(tags.OrderByDescending(t => t.ID).ToList());
        }


        // THÊM MỚI HOẶC CẬP NHẬT TAG
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(TAG model)
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return Redirect("~/Admin/Auth/Login");

            try
            {
                //  Xóa khoảng trắng thừa ở đầu/cuối
                model.TagName = model.TagName?.Trim();

                if (model.ID == 0) // THÊM MỚI 
                {
                    var checkExist = db.TAG.FirstOrDefault(t => t.TagName == model.TagName);
                    if (checkExist != null)
                    {
                        TempData["Error"] = "Tên Tag này đã tồn tại trong hệ thống!";
                        return RedirectToAction("Index");
                    }

                    db.TAG.Add(model);
                    TempData["Success"] = "Đã thêm Tag mới thành công!";
                }
                else //  CẬP NHẬT
                {
                    var checkExist = db.TAG.FirstOrDefault(t => t.TagName == model.TagName && t.ID != model.ID);
                    if (checkExist != null)
                    {
                        TempData["Error"] = "Tên Tag này bị trùng với một Tag khác đang có!";
                        return RedirectToAction("Index");
                    }

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

        //  XÓA TAG
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return Redirect("~/Admin/Auth/Login");

            try
            {
                var tag = db.TAG.Find(id);
                if (tag != null)
                {
                    // KIỂM TRA CHỦ ĐỘNG KHÓA NGOẠI (Foreign Key)
                    if (tag.RECIPE != null && tag.RECIPE.Count > 0)
                    {
                        TempData["Error"] = $"Không thể xóa! Tag này đang được gắn trong {tag.RECIPE.Count} bài viết công thức.";
                        return RedirectToAction("Index");
                    }

                    db.TAG.Remove(tag);
                    db.SaveChanges();
                    TempData["Success"] = "Đã xóa Tag thành công!";
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy Tag để xóa.";
                }
            }
            catch (Exception)
            {
                // Bắt lỗi dự phòng nếu có lỗi hệ thống khác
                TempData["Error"] = "Không thể xóa! Tag này đang được sử dụng.";
            }
            return RedirectToAction("Index");
        }

        // Dọn dẹp kết nối
        protected override void Dispose(bool disposing)
        {
            if (disposing) { db.Dispose(); }
            base.Dispose(disposing);
        }
    }
}