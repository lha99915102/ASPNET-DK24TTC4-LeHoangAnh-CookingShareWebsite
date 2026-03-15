using CookingShare.Models;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace CookingShare.Areas.Admin.Controllers
{
    [Authorize]
    public class RecipeController : Controller
    {
        CookingShareDBEntities db = new CookingShareDBEntities();

        // ==========================================
        // 1. HIỂN THỊ DANH SÁCH & LỌC TRẠNG THÁI
        // ==========================================
        public ActionResult Index(int? statusFilter)
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return Redirect("~/Admin/Auth/Login");

            // Thống kê
            ViewBag.TotalRecipes = db.RECIPE.Count();
            ViewBag.PendingRecipes = db.RECIPE.Count(r => r.Status == 0);
            ViewBag.CurrentFilter = statusFilter;

            // Lấy danh sách Categories để đưa vào Dropdown sửa bài
            ViewBag.Categories = db.CATEGORY.ToList();

            var recipes = db.RECIPE.Include(r => r.ACCOUNT).Include(r => r.CATEGORY).AsQueryable();

            if (statusFilter.HasValue) recipes = recipes.Where(r => r.Status == statusFilter.Value);

            return View(recipes.OrderByDescending(r => r.CreateDate).ToList());
        }

        // ==========================================
        // 2. CẬP NHẬT NỘI DUNG & THAY ĐỔI TRẠNG THÁI
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(RECIPE model, HttpPostedFileBase ImageFile, string actionType, string rejectReason)
        {
            try
            {
                var recipe = db.RECIPE.Find(model.ID);
                if (recipe != null)
                {
                    // 1. Cập nhật nội dung văn bản
                    recipe.Name = model.Name;
                    recipe.CategoryID = model.CategoryID;
                    recipe.CookTime = model.CookTime;
                    recipe.PrepTime = model.PrepTime;
                    recipe.Servings = model.Servings;
                    recipe.Difficulty = model.Difficulty;
                    recipe.Describe = model.Describe;

                    // 2. Xử lý Upload Hình ảnh mới (Nếu có chọn ảnh)
                    if (ImageFile != null && ImageFile.ContentLength > 0)
                    {
                        string uploadDir = "~/Content/Images/Recipes/";
                        bool exists = Directory.Exists(Server.MapPath(uploadDir));
                        if (!exists) Directory.CreateDirectory(Server.MapPath(uploadDir));

                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                        string path = Path.Combine(Server.MapPath(uploadDir), fileName);
                        ImageFile.SaveAs(path);

                        recipe.MainImage = "/Content/Images/Recipes/" + fileName;
                    }

                    // 3. Xử lý Hành động (Duyệt / Từ chối / Lưu / Đổi trạng thái)
                    if (actionType == "approve")
                    {
                        recipe.Status = 1; // Đã duyệt
                        recipe.RejectReason = null;
                        TempData["Success"] = $"Đã PHÊ DUYỆT và lưu thay đổi bài viết: {recipe.Name}";
                    }
                    else if (actionType == "reject")
                    {
                        recipe.Status = 2; // Từ chối
                        recipe.RejectReason = rejectReason;
                        TempData["Success"] = $"Đã TỪ CHỐI bài viết: {recipe.Name}";
                    }
                    else if (actionType == "pending")
                    {
                        recipe.Status = 0; // Đưa về chờ duyệt
                        recipe.RejectReason = null;
                        TempData["Success"] = $"Đã đưa bài viết '{recipe.Name}' về trạng thái CHỜ DUYỆT.";
                    }
                    else // actionType == "save" (Chỉ cập nhật nội dung, giữ nguyên trạng thái)
                    {
                        TempData["Success"] = $"Đã LƯU thay đổi nội dung bài viết: {recipe.Name}";
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

        // ==========================================
        // 3. XÓA BÀI VIẾT
        // ==========================================
        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                var recipe = db.RECIPE.Find(id);
                if (recipe != null)
                {
                    db.RECIPE.Remove(recipe);
                    db.SaveChanges();
                    TempData["Success"] = "Đã xóa công thức thành công!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Không thể xóa bài viết này. " + ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}