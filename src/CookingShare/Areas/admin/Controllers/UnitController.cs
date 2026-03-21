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

        // HIỂN THỊ DANH SÁCH ĐƠN VỊ ĐO LƯỜNG

        public ActionResult Index()
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return Redirect("~/Admin/Auth/Login");

            // Lấy danh sách, sắp xếp theo bảng chữ cái cho dễ tìm
            var units = db.UNIT.OrderBy(u => u.UnitName).ToList();

            return View(units);
        }

        //  THÊM MỚI HOẶC CẬP NHẬT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(UNIT model)
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return Redirect("~/Admin/Auth/Login");

            try
            {
                // Kiểm tra tên đơn vị không được để trống
                if (string.IsNullOrWhiteSpace(model.UnitName))
                {
                    TempData["Error"] = "Tên đơn vị không được để trống!";
                    return RedirectToAction("Index");
                }

                // Cắt khoảng trắng thừa
                model.UnitName = model.UnitName.Trim();

                var checkExist = db.UNIT.FirstOrDefault(u => u.UnitName == model.UnitName && u.ID != model.ID);
                if (checkExist != null)
                {
                    TempData["Error"] = $"Đơn vị '{model.UnitName}' đã tồn tại trong hệ thống!";
                    return RedirectToAction("Index");
                }

                if (model.ID == 0) // THÊM MỚI
                {
                    db.UNIT.Add(model);
                    TempData["Success"] = "Đã thêm đơn vị đo lường mới thành công!";
                }
                else // CẬP NHẬT
                {
                    var existingUnit = db.UNIT.Find(model.ID);
                    if (existingUnit != null)
                    {
                        existingUnit.UnitName = model.UnitName;
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

        //  XÓA ĐƠN VỊ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return Redirect("~/Admin/Auth/Login");

            try
            {
                var unit = db.UNIT.Find(id);
                if (unit != null)
                {
                    // KIỂM TRA CHỦ ĐỘNG KHÓA NGOẠI
                    if (unit.RECIPE_DETAIL != null && unit.RECIPE_DETAIL.Count > 0)
                    {
                        TempData["Error"] = $"Không thể xóa! Đơn vị này đang được sử dụng trong {unit.RECIPE_DETAIL.Count} chi tiết công thức nấu ăn.";
                        return RedirectToAction("Index");
                    }

                    db.UNIT.Remove(unit);
                    db.SaveChanges();
                    TempData["Success"] = "Đã xóa đơn vị đo lường thành công!";
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy đơn vị đo lường để xóa.";
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Không thể xóa! Đơn vị này đang được sử dụng trong hệ thống.";
            }
            return RedirectToAction("Index");
        }


        // Dọn dẹp kết nối Database 
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