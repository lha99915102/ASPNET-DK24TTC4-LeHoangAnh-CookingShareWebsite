using CookingShare.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace CookingShare.Areas.Admin.Controllers
{
    [Authorize]
    public class SettingController : Controller
    {
        CookingShareDBEntities db = new CookingShareDBEntities();

        // 1. HIỂN THỊ DANH SÁCH CẤU HÌNH
        public ActionResult Index()
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return Redirect("~/Admin/Auth/Login");

            var settings = db.SYSTEM_SETTING.OrderBy(s => s.SettingKey).ToList();
            return View(settings);
        }

        // 2. LƯU HOẶC CẬP NHẬT CẤU HÌNH
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(SYSTEM_SETTING model)
        {
            try
            {
                if (model.ID == 0) // Thêm mới
                {
                    var checkExist = db.SYSTEM_SETTING.FirstOrDefault(s => s.SettingKey == model.SettingKey);
                    if (checkExist != null)
                    {
                        TempData["Error"] = "Mã cấu hình (Key) này đã tồn tại!";
                        return RedirectToAction("Index");
                    }
                    db.SYSTEM_SETTING.Add(model);
                    TempData["Success"] = "Đã thêm cấu hình mới!";
                }
                else // Cập nhật
                {
                    var existing = db.SYSTEM_SETTING.Find(model.ID);
                    if (existing != null)
                    {
                        existing.SettingKey = model.SettingKey;
                        existing.SettingValue = model.SettingValue;
                        TempData["Success"] = "Đã cập nhật cấu hình thành công!";
                    }
                }
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
            }
            return RedirectToAction("Index");
        }

        // 3. XÓA CẤU HÌNH
        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                var setting = db.SYSTEM_SETTING.Find(id);
                if (setting != null)
                {
                    db.SYSTEM_SETTING.Remove(setting);
                    db.SaveChanges();
                    TempData["Success"] = "Đã xóa cấu hình!";
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