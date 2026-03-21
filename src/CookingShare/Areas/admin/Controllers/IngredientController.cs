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


        //  HIỂN THỊ DANH SÁCH NGUYÊN LIỆU
        public ActionResult Index()
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return Redirect("~/Admin/Auth/Login");

            // Lấy toàn bộ nguyên liệu từ CSDL (Sắp xếp mới nhất lên đầu)
            var ingredients = db.INGREDIENT.OrderByDescending(i => i.ID).ToList();

            return View(ingredients);
        }


        // THÊM MỚI HOẶC CẬP NHẬT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(INGREDIENT model)
        {
            // BẢO MẬT CHỐNG POSTMAN (Chặn User thường gọi API Admin)
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return Redirect("~/Admin/Auth/Login");

            try
            {
                if (ModelState.IsValid)
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
                else
                {
                    TempData["Error"] = "Vui lòng nhập đầy đủ thông tin hợp lệ.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            }

            return RedirectToAction("Index");
        }


        //  XÓA NGUYÊN LIỆU
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            //  BẢO MẬT CHỐNG POSTMAN (Chặn User thường gọi API Admin)
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return RedirectToAction("Login", "Auth", new { area = "admin" });

            try
            {
                var item = db.INGREDIENT.Find(id);
                if (item != null)
                {
                    //  Kiểm tra xem có công thức nào đang dùng nguyên liệu này không
                    int usageCount = db.RECIPE_DETAIL.Count(r => r.IngredientID == id);
                    if (usageCount > 0)
                    {
                        TempData["Error"] = $"Không thể xóa! Đang có {usageCount} công thức sử dụng nguyên liệu này.";
                        return RedirectToAction("Index");
                    }

                    // Kiểm tra xem có User nào đang khai báo dị ứng nguyên liệu này không
                    if (item.ACCOUNT != null && item.ACCOUNT.Count > 0)
                    {
                        TempData["Error"] = $"Không thể xóa! Đang có {item.ACCOUNT.Count} người dùng khai báo dị ứng với nguyên liệu này.";
                        return RedirectToAction("Index");
                    }

                    db.INGREDIENT.Remove(item);
                    db.SaveChanges();
                    TempData["Success"] = "Đã xóa nguyên liệu thành công!";
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy nguyên liệu để xóa.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi hệ thống: " + ex.Message;
            }

            return RedirectToAction("Index");
        }


        // Dọn dẹp kết nối Database khi xong việc
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