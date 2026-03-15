using CookingShare.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace CookingShare.Areas.Admin.Controllers
{
    [Authorize]
    public class UnitController : Controller
    {
        CookingShareDBEntities db = new CookingShareDBEntities();

        // ==========================================
        // 1. HIỂN THỊ DANH SÁCH ĐƠN VỊ ĐO LƯỜNG
        // ==========================================
        public ActionResult Index()
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return Redirect("~/Admin/Auth/Login");

            // Lấy danh sách, sắp xếp theo bảng chữ cái cho dễ tìm
            var units = db.UNIT.OrderBy(u => u.UnitName).ToList();

            return View(units);
        }

        // ==========================================
        // 2. THÊM MỚI HOẶC CẬP NHẬT
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(UNIT model)
        {
            try
            {
                // Kiểm tra tên đơn vị không được để trống
                if (string.IsNullOrWhiteSpace(model.UnitName))
                {
                    TempData["Error"] = "Tên đơn vị không được để trống!";
                    return RedirectToAction("Index");
                }

                // Kiểm tra trùng lặp tên (Bỏ qua phân biệt hoa thường)
                var checkExist = db.UNIT.FirstOrDefault(u => u.UnitName.ToLower() == model.UnitName.ToLower().Trim() && u.ID != model.ID);
                if (checkExist != null)
                {
                    TempData["Error"] = $"Đơn vị '{model.UnitName}' đã tồn tại trong hệ thống!";
                    return RedirectToAction("Index");
                }

                if (model.ID == 0) // THÊM MỚI
                {
                    model.UnitName = model.UnitName.Trim();
                    db.UNIT.Add(model);
                    TempData["Success"] = "Đã thêm đơn vị đo lường mới thành công!";
                }
                else // CẬP NHẬT
                {
                    var existingUnit = db.UNIT.Find(model.ID);
                    if (existingUnit != null)
                    {
                        existingUnit.UnitName = model.UnitName.Trim();
                        TempData["Success"] = "Đã cập nhật đơn vị thành công!";
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
        // 3. XÓA ĐƠN VỊ
        // ==========================================
        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                var unit = db.UNIT.Find(id);
                if (unit != null)
                {
                    db.UNIT.Remove(unit);
                    db.SaveChanges();
                    TempData["Success"] = "Đã xóa đơn vị đo lường thành công!";
                }
            }
            catch (Exception)
            {
                // Lỗi khóa ngoại: Nếu Unit này đang được liên kết trong bảng RECIPE_DETAIL
                TempData["Error"] = "Không thể xóa! Đơn vị này đang được sử dụng trong chi tiết Công thức.";
            }
            return RedirectToAction("Index");
        }
    }
}