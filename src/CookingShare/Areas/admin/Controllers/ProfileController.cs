using CookingShare.Models;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace CookingShare.Areas.Admin.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        CookingShareDBEntities db = new CookingShareDBEntities();

        // Hàm băm mật khẩu
        private string HashPasswordSHA256(string rawData)
        {
            if (string.IsNullOrEmpty(rawData)) return string.Empty;
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++) builder.Append(bytes[i].ToString("x2"));
                return builder.ToString();
            }
        }

        // ==========================================
        // 1. HIỂN THỊ TRANG HỒ SƠ
        // ==========================================
        public ActionResult Index()
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null) return Redirect("~/Admin/Auth/Login");

            // Lấy thông tin tài khoản
            var account = db.ACCOUNT.Find(currentAcc.ID);
            if (account == null) return Redirect("~/Admin/Auth/Login");

            // Lấy thông tin Profile (Nếu chưa có thì tạo mặc định)
            var profile = db.PROFILE.FirstOrDefault(p => p.AccountID == currentAcc.ID);
            if (profile == null)
            {
                profile = new PROFILE { AccountID = currentAcc.ID, FullName = account.UserName, Avatar = "default-avatar.png" };
                db.PROFILE.Add(profile);
                db.SaveChanges();
            }

            ViewBag.Account = account;
            return View(profile);
        }

        // ==========================================
        // 2. CẬP NHẬT THÔNG TIN & AVATAR
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(PROFILE model, string Email, string Phone, HttpPostedFileBase AvatarFile)
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null) return Redirect("~/Admin/Auth/Login");

            try
            {
                // 1. Cập nhật Account (Email, Phone)
                var account = db.ACCOUNT.Find(currentAcc.ID);
                account.Email = Email;
                account.Phone = Phone;

                // 2. Cập nhật Profile (FullName, Bio, Gender)
                var profile = db.PROFILE.FirstOrDefault(p => p.AccountID == currentAcc.ID);
                if (profile != null)
                {
                    profile.FullName = model.FullName;
                    profile.Bio = model.Bio;
                    profile.Gender = model.Gender;

                    // Xử lý Upload Avatar
                    if (AvatarFile != null && AvatarFile.ContentLength > 0)
                    {
                        string uploadDir = "~/Content/Images/Users/";
                        if (!Directory.Exists(Server.MapPath(uploadDir))) Directory.CreateDirectory(Server.MapPath(uploadDir));

                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(AvatarFile.FileName);
                        string path = Path.Combine(Server.MapPath(uploadDir), fileName);
                        AvatarFile.SaveAs(path);

                        profile.Avatar = fileName; // Lưu tên file vào DB
                    }
                }

                db.SaveChanges();
                Session["Account"] = account; // Cập nhật lại Session
                TempData["Success"] = "Cập nhật hồ sơ thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi cập nhật: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // ==========================================
        // 3. ĐỔI MẬT KHẨU
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(string OldPassword, string NewPassword, string ConfirmPassword)
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null) return Redirect("~/Admin/Auth/Login");

            try
            {
                if (NewPassword != ConfirmPassword)
                {
                    TempData["Error"] = "Mật khẩu xác nhận không khớp!";
                    return RedirectToAction("Index");
                }

                var account = db.ACCOUNT.Find(currentAcc.ID);
                string hashedOldPwd = HashPasswordSHA256(OldPassword);

                // Kiểm tra mật khẩu cũ
                if (account.Password != hashedOldPwd)
                {
                    TempData["Error"] = "Mật khẩu cũ không chính xác!";
                    return RedirectToAction("Index");
                }

                // Cập nhật mật khẩu mới (đã mã hóa)
                account.Password = HashPasswordSHA256(NewPassword);
                db.SaveChanges();

                TempData["Success"] = "Đổi mật khẩu thành công! Vui lòng dùng mật khẩu mới cho lần đăng nhập sau.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi đổi mật khẩu: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}