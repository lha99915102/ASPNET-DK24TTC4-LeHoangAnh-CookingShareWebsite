using CookingShare.Models;
using System.Threading.Tasks;
using Google.Apis.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CookingShare.Controllers
{
    public class AccountController : Controller
    {
        CookingShareDBEntities db = new CookingShareDBEntities();

        // ==========================================
        // 1. ĐĂNG NHẬP
        // ==========================================
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
                    var user = db.ACCOUNT.FirstOrDefault(u => u.ID == userId && u.Status == 1);
                    if (user != null)
                    {
                        Session["Account"] = user; // Đăng nhập tự động
                        if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
                        return RedirectToAction("Index", "Home");
                    }
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public ActionResult Login(string loginIdentifier, string password, bool rememberMe = false, string returnUrl = "")
        {
            // Mã hóa mật khẩu
            string hashedPassword = SecurityHelper.HashPasswordSHA256(password);

            // TÌM KIẾM ĐA NĂNG: Khớp Email, hoặc UserName, hoặc Số điện thoại
           
            var user = db.ACCOUNT.FirstOrDefault(u =>
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

                if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Tài khoản hoặc mật khẩu không chính xác!";
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // ==========================================
        // 2. ĐĂNG XUẤT
        // ==========================================
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

        // ==========================================
        // 3. ĐĂNG KÝ TÀI KHOẢN MỚI
        // ==========================================
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
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

                ACCOUNT newAcc = new ACCOUNT()
                {
                    UserName = userName,
                    Email = email,
                    Password = SecurityHelper.HashPasswordSHA256(password),
                    Role = 2,
                    Status = 1,
                    RegistDate = System.DateTime.Now
                };

                db.ACCOUNT.Add(newAcc);
                db.SaveChanges();

                PROFILE newProfile = new PROFILE()
                {
                    AccountID = newAcc.ID,
                    FullName = fullName,
                    Avatar = "default-avatar.png",
                    CaloDaily = 0
                };

                db.PROFILE.Add(newProfile);
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

        // ==========================================
        // 4. QUÊN MẬT KHẨU
        // ==========================================
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }


        // 4.2. Xử lý khi bấm nút "Gửi mã xác nhận"
        [HttpPost]
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
                // Gửi thành công thì chuyển trang, mang theo cái email trên thanh URL
                return RedirectToAction("ResetPassword", new { email = email });
            }
            else
            {
                ViewBag.Error = "Hệ thống gửi Email đang gặp sự cố. Vui lòng kiểm tra lại cấu hình Email!";
                return View();
            }
        }

        // 4.3. Hiển thị trang Nhập OTP và Mật khẩu mới
        [HttpGet]
        public ActionResult ResetPassword(string email)
        {
            if (string.IsNullOrEmpty(email)) return RedirectToAction("ForgotPassword");

            ViewBag.Email = email;
            return View();
        }

        // 4.4. Xử lý Đổi mật khẩu mới
        [HttpPost]
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
            user.Password = CookingShare.Models.SecurityHelper.HashPasswordSHA256(newPassword);

            // Dọn dẹp 2 cột Token để không bị dùng lại
            user.ResetToken = null;
            user.TokenExpiry = null;

            db.SaveChanges();

            TempData["SuccessMessage"] = "Khôi phục mật khẩu thành công! Hãy đăng nhập ngay.";
            return RedirectToAction("Login");
        }

        // ==========================================
        // 5. ĐĂNG NHẬP / ĐĂNG KÝ BẰNG GOOGLE
        // ==========================================
        [HttpPost]
        public async Task<ActionResult> GoogleLogin(string credential)
        {
            try
            {
                // 1. Xác thực Token với Google (Đảm bảo không bị làm giả)
                var payload = await GoogleJsonWebSignature.ValidateAsync(credential);

                // Lấy thông tin từ Google
                string email = payload.Email;
                string fullName = payload.Name;

                // 2. Tìm xem Email này đã tồn tại trong hệ thống chưa
                var user = db.ACCOUNT.FirstOrDefault(u => u.Email == email);

                if (user != null)
                {
                    // TRƯỜNG HỢP 1: ĐÃ CÓ TÀI KHOẢN -> TIẾN HÀNH ĐĂNG NHẬP
                    if (user.Status != 1)
                    {
                        return Json(new { success = false, message = "Tài khoản của bạn đã bị khóa!" });
                    }
                    Session["Account"] = user;
                }
                else
                {
                    // TRƯỜNG HỢP 2: CHƯA CÓ TÀI KHOẢN -> TỰ ĐỘNG ĐĂNG KÝ MỚI

                    // Tạo UserName tự động
                    string userName = email.Split('@')[0];
                    var checkUser = db.ACCOUNT.FirstOrDefault(u => u.UserName == userName);
                    if (checkUser != null) userName = userName + new Random().Next(100, 999).ToString();

                    ACCOUNT newAcc = new ACCOUNT()
                    {
                        UserName = userName,
                        Email = email,
                        // Tạo một mật khẩu ngẫu nhiên cực khó đoán vì họ dùng Google để đăng nhập
                        Password = SecurityHelper.HashPasswordSHA256(Guid.NewGuid().ToString()),
                        Role = 2,
                        Status = 1,
                        RegistDate = DateTime.Now
                    };
                    db.ACCOUNT.Add(newAcc);
                    db.SaveChanges(); // Lưu để lấy ID

                    PROFILE newProfile = new PROFILE()
                    {
                        AccountID = newAcc.ID,
                        FullName = fullName,
                        Avatar = "default-avatar.png", // Bạn có thể lấy luôn ảnh Google bằng: payload.Picture
                        CaloDaily = 0
                    };
                    db.PROFILE.Add(newProfile);
                    db.SaveChanges();

                    // Lưu Session đăng nhập
                    Session["Account"] = newAcc;
                }

                // Trả về tín hiệu thành công để Javascript chuyển trang
                return Json(new { success = true, redirectUrl = "/Home/Index" });
            }
            catch (Exception ex)
            {
                // Token không hợp lệ hoặc hết hạn
                return Json(new { success = false, message = "Xác thực Google thất bại: " + ex.Message });
            }
        }

    }
}