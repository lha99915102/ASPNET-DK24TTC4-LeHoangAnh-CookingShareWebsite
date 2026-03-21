using CookingShare.Models;
using System.Threading.Tasks;
using Google.Apis.Auth;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CookingShare.Controllers
{
    public class AccountController : Controller
    {
        CookingShareDBEntities db = new CookingShareDBEntities();

        // ĐĂNG NHẬP
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            // KIỂM TRA TÍNH NĂNG "GHI NHỚ ĐĂNG NHẬP" BẰNG COOKIE
            HttpCookie authCookie = Request.Cookies["CookingShareAuth"];
            if (authCookie != null && !string.IsNullOrEmpty(authCookie.Value))
            {
                int userId;
                if (int.TryParse(authCookie.Value, out userId))
                {
                    var user = db.ACCOUNT.Include(u => u.PROFILE).FirstOrDefault(u => u.ID == userId && u.Status == 1);
                    if (user != null)
                    {
                        Session["Account"] = user; // Đăng nhập tự động

                        // Chống Open Redirect
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                            return Redirect(returnUrl);

                        return RedirectToAction("Index", "Home");
                    }
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] //Chống tấn công giả mạo (CSRF)
        public ActionResult Login(string loginIdentifier, string password, bool rememberMe = false, string returnUrl = "")
        {
            // Mã hóa mật khẩu
            string hashedPassword = SecurityHelper.HashPasswordSHA256(password);

            // TÌM KIẾM: Khớp Email, hoặc UserName, hoặc Số điện thoại. Kéo theo PROFILE để chống lỗi vỡ Session.
            var user = db.ACCOUNT.Include(u => u.PROFILE).FirstOrDefault(u =>
                (u.Email == loginIdentifier || u.UserName == loginIdentifier || u.Phone == loginIdentifier)
                && u.Password == hashedPassword);

            if (user != null)
            {
                if (user.Status != 1)
                {
                    ViewBag.Error = "Tài khoản của bạn đã bị khóa!";
                    return View();
                }

                Session["Account"] = user;

                // XỬ LÝ "GHI NHỚ ĐĂNG NHẬP"
                if (rememberMe)
                {
                    HttpCookie authCookie = new HttpCookie("CookingShareAuth");
                    authCookie.Value = user.ID.ToString();
                    authCookie.Expires = DateTime.Now.AddDays(30); // Lưu giữ trong 30 ngày
                    Response.Cookies.Add(authCookie);
                }

                //  Chống Open Redirect
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Tài khoản hoặc mật khẩu không chính xác!";
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // ĐĂNG XUẤT
        public ActionResult Logout()
        {
            Session.Remove("Account");

            // Xóa Cookie ghi nhớ đăng nhập
            if (Request.Cookies["CookingShareAuth"] != null)
            {
                var c = new HttpCookie("CookingShareAuth");
                c.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(c);
            }

            return RedirectToAction("Index", "Home");
        }

        // ĐĂNG KÝ TÀI KHOẢN MỚI
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(string fullName, string email, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu nhập lại không khớp!";
                return View();
            }

            var checkEmail = db.ACCOUNT.FirstOrDefault(u => u.Email == email);
            if (checkEmail != null)
            {
                ViewBag.Error = "Email này đã được đăng ký. Vui lòng dùng email khác!";
                return View();
            }

            try
            {
                string userName = email.Split('@')[0];
                var checkUser = db.ACCOUNT.FirstOrDefault(u => u.UserName == userName);
                if (checkUser != null)
                {
                    userName = userName + new System.Random().Next(100, 999).ToString();
                }

                // Tối ưu Entity Framework (Lưu Account và Profile trong 1 lần duy nhất)
                ACCOUNT newAcc = new ACCOUNT()
                {
                    UserName = userName,
                    Email = email,
                    Password = SecurityHelper.HashPasswordSHA256(password),
                    Role = 2,
                    Status = 1,
                    RegistDate = System.DateTime.Now,
                    PROFILE = new PROFILE()
                    {
                        FullName = fullName,
                        Avatar = "default-avatar.png",
                        CaloDaily = 0
                    }
                };

                db.ACCOUNT.Add(newAcc);
                db.SaveChanges();

                ViewBag.Success = "Đăng ký tài khoản thành công! Bạn có thể đăng nhập ngay bây giờ.";
                return View();
            }
            catch (System.Exception ex)
            {
                ViewBag.Error = "Có lỗi xảy ra trong quá trình đăng ký: " + ex.Message;
                return View();
            }
        }

        // QUÊN MẬT KHẨU
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(string email)
        {
            var user = db.ACCOUNT.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Error = "Địa chỉ email này chưa được đăng ký trong hệ thống!";
                return View();
            }

            // Tạo mã OTP 6 số ngẫu nhiên
            Random rnd = new Random();
            string otp = rnd.Next(100000, 999999).ToString();

            // Lưu OTP và Thời gian hết hạn (15 phút) vào Database
            user.ResetToken = otp;
            user.TokenExpiry = DateTime.Now.AddMinutes(15);
            db.SaveChanges();

            // Gửi Email
            string subject = "Mã xác nhận khôi phục mật khẩu - CookingShare";
            string body = $"<h3>Xin chào {user.UserName},</h3>" +
                          $"<p>Mã xác nhận (OTP) của bạn là: <strong style='font-size:24px; color:#FF6600;'>{otp}</strong></p>" +
                          $"<p>Mã này sẽ hết hạn sau 15 phút. Vui lòng không chia sẻ cho bất kỳ ai.</p>";

            bool isSent = CookingShare.Models.EmailHelper.SendEmail(email, subject, body);

            if (isSent)
            {
                return RedirectToAction("ResetPassword", new { email = email });
            }
            else
            {
                ViewBag.Error = "Hệ thống gửi Email đang gặp sự cố. Vui lòng kiểm tra lại cấu hình Email!";
                return View();
            }
        }

        [HttpGet]
        public ActionResult ResetPassword(string email)
        {
            if (string.IsNullOrEmpty(email)) return RedirectToAction("ForgotPassword");

            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(string email, string otp, string newPassword, string confirmPassword)
        {
            ViewBag.Email = email;

            var user = db.ACCOUNT.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Error = "Tài khoản không tồn tại!";
                return View();
            }

            // Kiểm tra mã OTP có khớp trong Database không
            if (user.ResetToken != otp)
            {
                ViewBag.Error = "Mã xác nhận OTP không chính xác!";
                return View();
            }

            // Kiểm tra mã OTP có bị quá hạn 15 phút không
            if (user.TokenExpiry < DateTime.Now)
            {
                ViewBag.Error = "Mã xác nhận đã hết hạn! Vui lòng quay lại trang trước để lấy mã mới.";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu mới và Nhập lại mật khẩu không khớp!";
                return View();
            }

            // Cập nhật mật khẩu (Đã mã hóa SHA-256)
            user.Password = SecurityHelper.HashPasswordSHA256(newPassword);

            // Dọn dẹp 2 cột Token để không bị dùng lại
            user.ResetToken = null;
            user.TokenExpiry = null;

            db.SaveChanges();

            TempData["SuccessMessage"] = "Khôi phục mật khẩu thành công! Hãy đăng nhập ngay.";
            return RedirectToAction("Login");
        }


        // ĐĂNG NHẬP / ĐĂNG KÝ BẰNG GOOGLE
        [HttpPost]
        public async Task<ActionResult> GoogleLogin(string credential)
        {
            try
            {
                // Xác thực Token với Google
                var payload = await GoogleJsonWebSignature.ValidateAsync(credential);

                string email = payload.Email;
                string fullName = payload.Name;
                string googleAvatar = payload.Picture;

                var user = db.ACCOUNT.Include(u => u.PROFILE).FirstOrDefault(u => u.Email == email);

                if (user != null)
                {
                    if (user.Status != 1)
                    {
                        return Json(new { success = false, message = "Tài khoản của bạn đã bị khóa!" });
                    }
                    Session["Account"] = user;
                }
                else
                {
                    string userName = email.Split('@')[0];
                    var checkUser = db.ACCOUNT.FirstOrDefault(u => u.UserName == userName);
                    if (checkUser != null) userName = userName + new Random().Next(100, 999).ToString();

                    //  Lưu Account và Profile trong 1 lần duy nhất
                    ACCOUNT newAcc = new ACCOUNT()
                    {
                        UserName = userName,
                        Email = email,
                        Password = SecurityHelper.HashPasswordSHA256(Guid.NewGuid().ToString()),
                        Role = 2,
                        Status = 1,
                        RegistDate = DateTime.Now,
                        PROFILE = new PROFILE() // Lồng vào đây
                        {
                            FullName = fullName,
                            Avatar = string.IsNullOrEmpty(googleAvatar) ? "default-avatar.png" : googleAvatar,
                            CaloDaily = 0
                        }
                    };

                    db.ACCOUNT.Add(newAcc);
                    db.SaveChanges();

                    Session["Account"] = newAcc;
                }

                // Dùng Url.Action cho link trả về
                return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Xác thực Google thất bại: " + ex.Message });
            }
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