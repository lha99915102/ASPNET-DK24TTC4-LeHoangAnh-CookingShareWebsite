using CookingShare.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace CookingShare.Areas.Admin.Controllers
{
    [Authorize]
    public class IngredientController : Controller
    {
        CookingShareDBEntities db = new CookingShareDBEntities();

        // ==========================================
        // 1. HIỂN THỊ DANH SÁCH NGUYÊN LIỆU
        // ==========================================
        public ActionResult Index()
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return Redirect("~/Admin/Auth/Login");

            // Lấy toàn bộ nguyên liệu từ CSDL (Sắp xếp mới nhất lên đầu)
            var ingredients = db.INGREDIENT.OrderByDescending(i => i.ID).ToList();

            return View(ingredients);
        }

        // ==========================================
        // 2. THÊM MỚI HOẶC CẬP NHẬT
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(INGREDIENT model)
        {
            try
            {
                if (model.ID == 0) // THÊM MỚI
                {
                    db.INGREDIENT.Add(model);
                    TempData["Success"] = "Đã thêm nguyên liệu mới thành công!";
                }
                else // CẬP NHẬT
                {
                    var existingItem = db.INGREDIENT.Find(model.ID);
                    if (existingItem != null)
                    {
                        // Chỉ map những cột thực sự có trong Database
                        existingItem.Name = model.Name;
                        existingItem.Calo = model.Calo;
                        existingItem.Protein = model.Protein;
                        existingItem.Fat = model.Fat;
                        existingItem.Sugar = model.Sugar;

                        TempData["Success"] = "Đã cập nhật nguyên liệu thành công!";
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
        // 3. XÓA NGUYÊN LIỆU
        // ==========================================
        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                var item = db.INGREDIENT.Find(id);
                if (item != null)
                {
                    db.INGREDIENT.Remove(item);
                    db.SaveChanges();
                    TempData["Success"] = "Đã xóa nguyên liệu thành công!";
                }
            }
            catch (Exception)
            {
                // Bắt lỗi khóa ngoại (Foreign Key) khi nguyên liệu đã được dùng trong Recipe_Detail
                TempData["Error"] = "Không thể xóa! Nguyên liệu này đang được sử dụng trong công thức nấu ăn.";
            }

            return RedirectToAction("Index");
        }
    }
}