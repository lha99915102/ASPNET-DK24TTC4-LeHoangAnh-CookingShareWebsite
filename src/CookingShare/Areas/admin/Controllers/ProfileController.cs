using CookingShare.Models;
using CookingShare.Helpers; 
using System;
using System.Data.Entity; 
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CookingShare.Areas.Admin.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        CookingShareDBEntities db = new CookingShareDBEntities();

        // HIỂN THỊ TRANG PROFILE
        public ActionResult Index()
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null) return Redirect("~/Admin/Auth/Login");

            // Nạp lại data mới nhất từ DB
            var acc = db.ACCOUNT.Find(currentAcc.ID);
            var profile = db.PROFILE.FirstOrDefault(p => p.AccountID == currentAcc.ID);

            if (profile == null)
            {
                profile = new PROFILE { FullName = acc.UserName, Avatar = "default-avatar.png" };
            }

            ViewBag.Account = acc;
            return View(profile);
        }

        // CẬP NHẬT THÔNG TIN VÀ AVATAR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(string FullName, string Gender, string Email, string Phone, string Bio, HttpPostedFileBase AvatarFile)
        {
            try
            {
                var currentAcc = Session["Account"] as ACCOUNT;
                if (currentAcc == null) return Redirect("~/Admin/Auth/Login");

                //  Dùng Include() kéo Profile lên luôn để gán lại Session không bị lỗi vỡ giao diện
                var acc = db.ACCOUNT.Include(a => a.PROFILE).FirstOrDefault(a => a.ID == currentAcc.ID);
                var profile = acc.PROFILE;

                if (profile == null)
                {
                    profile = new PROFILE { AccountID = currentAcc.ID, Avatar = "default-avatar.png" };
                    db.PROFILE.Add(profile);
                    acc.PROFILE = profile; // Gắn liên kết lại
                }

                acc.Email = Email;
                acc.Phone = Phone;
                profile.FullName = FullName;
                profile.Gender = Gender;
                profile.Bio = Bio;


                // BẮT ĐẦU XỬ LÝ ẢNH BẰNG FILEHELPER
                if (AvatarFile != null && AvatarFile.ContentLength > 0)
                {
                    string prefix = "admin_avatar_" + acc.ID;
                    profile.Avatar = FileHelper.UploadAndReplaceImage(AvatarFile, "~/assets/images/avatars/", prefix, profile.Avatar);
                }

                db.SaveChanges();
                Session["Account"] = acc; // Ghi đè Session với đầy đủ Avatar và Tên mới
                TempData["Success"] = "Cập nhật hồ sơ cá nhân thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        //  ĐỔI MẬT KHẨU
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(string OldPassword, string NewPassword, string ConfirmPassword)
        {
            try
            {
                var currentAcc = Session["Account"] as ACCOUNT;
                if (currentAcc == null) return Redirect("~/Admin/Auth/Login");

                var acc = db.ACCOUNT.Find(currentAcc.ID);

                //  Sử dụng hàm mã hóa dùng chung từ SecurityHelper
                string hashedOldPass = CookingShare.Models.SecurityHelper.HashPasswordSHA256(OldPassword);

                if (acc.Password != hashedOldPass)
                {
                    TempData["Error"] = "Mật khẩu hiện tại không chính xác!";
                    return RedirectToAction("Index");
                }

                if (NewPassword != ConfirmPassword)
                {
                    TempData["Error"] = "Mật khẩu mới và mật khẩu xác nhận không khớp!";
                    return RedirectToAction("Index");
                }

                if (NewPassword.Length < 6)
                {
                    TempData["Error"] = "Mật khẩu mới phải có ít nhất 6 ký tự!";
                    return RedirectToAction("Index");
                }

                // Sử dụng hàm mã hóa dùng chung từ SecurityHelper
                acc.Password = CookingShare.Models.SecurityHelper.HashPasswordSHA256(NewPassword);
                db.SaveChanges();

                TempData["Success"] = "Đổi mật khẩu thành công! Hãy ghi nhớ mật khẩu mới.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { db.Dispose(); }
            base.Dispose(disposing);
        }
    }
}