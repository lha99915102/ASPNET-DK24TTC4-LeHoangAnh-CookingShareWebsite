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


        // HIỂN THỊ DANH SÁCH CẤU HÌNH
        public ActionResult Index()
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            //Dùng RedirectToAction an toàn
            if (currentAcc == null || currentAcc.Role != 1) return RedirectToAction("Login", "Auth", new { area = "admin" });

            var settings = db.SYSTEM_SETTING.OrderBy(s => s.SettingKey).ToList();
            return View(settings);
        }


        //  LƯU HOẶC CẬP NHẬT CẤU HÌNH
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(SYSTEM_SETTING model)
        {
            //  BẢO MẬT CHỐNG POSTMAN (Chặn User thường gọi API Admin)
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return RedirectToAction("Login", "Auth", new { area = "admin" });

            try
            {
                if (model.ID == 0) //  THÊM MỚI 
                {
                    var checkExist = db.SYSTEM_SETTING.FirstOrDefault(s => s.SettingKey == model.SettingKey);
                    if (checkExist != null)
                    {
                        TempData["Error"] = "Mã cấu hình (Key) này đã tồn tại!";
                        return RedirectToAction("Index");
                    }

                    db.SYSTEM_SETTING.Add(model);
                    db.SaveChanges();

                    //  Xóa RAM để web cập nhật ngay
                    CookingShare.Helpers.SettingHelper.ClearCache(model.SettingKey);

                    TempData["Success"] = "Đã thêm cấu hình mới thành công!";
                }
                else // CẬP NHẬT
                {
                    // Kiểm tra xem Key mới nhập vào có bị trùng với Key của một cấu hình KHÁC hay không
                    var checkExist = db.SYSTEM_SETTING.FirstOrDefault(s => s.SettingKey == model.SettingKey && s.ID != model.ID);
                    if (checkExist != null)
                    {
                        TempData["Error"] = "Mã cấu hình (Key) này đã bị trùng lặp với một cấu hình khác!";
                        return RedirectToAction("Index");
                    }

                    var existing = db.SYSTEM_SETTING.Find(model.ID);
                    if (existing != null)
                    {
                        string oldKey = existing.SettingKey; // Lưu lại Key cũ phòng trường hợp Admin đổi tên Key

                        existing.SettingKey = model.SettingKey;
                        existing.SettingValue = model.SettingValue;
                        db.SaveChanges();

                        // Xóa RAM của cả Key cũ (nếu bị đổi tên) và Key mới
                        CookingShare.Helpers.SettingHelper.ClearCache(oldKey);
                        CookingShare.Helpers.SettingHelper.ClearCache(model.SettingKey);

                        TempData["Success"] = "Đã cập nhật cấu hình thành công!";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi hệ thống: " + ex.Message;
            }
            return RedirectToAction("Index");
        }


        //  XÓA CẤU HÌNH
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return RedirectToAction("Login", "Auth", new { area = "admin" });

            try
            {
                var setting = db.SYSTEM_SETTING.Find(id);
                if (setting != null)
                {
                    string keyToClear = setting.SettingKey; // Phải lấy Key trước khi dòng này bị xóa khỏi DB

                    db.SYSTEM_SETTING.Remove(setting);
                    db.SaveChanges();

                    // Quét sạch dấu vết cấu hình này trên RAM
                    CookingShare.Helpers.SettingHelper.ClearCache(keyToClear);

                    TempData["Success"] = "Đã xóa cấu hình thành công!";
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy cấu hình để xóa.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi xóa: " + ex.Message;
            }
            return RedirectToAction("Index");
        }

        // Dọn dẹp kết nối Database
        protected override void Dispose(bool disposing)
        {
            if (disposing) { db.Dispose(); }
            base.Dispose(disposing);
        }
    }
}