using CookingShare.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace CookingShare.Areas.admin.Controllers
{
    public class AuthController : Controller
    {
        CookingShareDBEntities db = new CookingShareDBEntities();

        // ==========================================
        // 1. HÀM BĂM MẬT KHẨU SHA-256
        // ==========================================
        private string HashPasswordSHA256(string password)
        {
            if (string.IsNullOrEmpty(password)) return string.Empty;
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // ==========================================
        // 2. HIỂN THỊ GIAO DIỆN ĐĂNG NHẬP
        // ==========================================
        [HttpGet]
        public ActionResult Login()
        {
            // Nếu đã đăng nhập và là Admin thì đẩy thẳng vào Dashboard
            if (Session["Account"] != null)
            {
                var acc = (ACCOUNT)Session["Account"];
                if (acc.Role == 1)
                {
                    return Redirect("~/Admin/Home/Index"); // Dùng đường dẫn tuyệt đối cho chắc ăn
                }
            }
            return View();
        }

        // ==========================================
        // 3. XỬ LÝ KHI NGƯỜI DÙNG ẤN NÚT ĐĂNG NHẬP
        // ==========================================
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            // Băm mật khẩu người dùng nhập vào trước khi so sánh
            string hashedPassword = HashPasswordSHA256(password);

            // Tìm tài khoản khớp Username, Password (đã băm) và bắt buộc phải là Admin (Role = 1)
            var admin = db.ACCOUNT.FirstOrDefault(a => a.UserName == username && a.Password == hashedPassword && a.Role == 1);

            if (admin != null)
            {
                // Kiểm tra xem tài khoản có bị khóa không (Status = 1 là đang hoạt động)
                if (admin.Status != 1)
                {
                    ViewBag.Error = "Tài khoản của bạn đã bị vô hiệu hóa!";
                    return View();
                }

                // Lưu Session và Cookie xác thực
                Session["Account"] = admin;
                FormsAuthentication.SetAuthCookie(admin.UserName, false);

                // Đăng nhập thành công -> Chuyển hướng thẳng tới Dashboard
                return Redirect("~/Admin/Home/Index");
            }

            // Đăng nhập thất bại
            ViewBag.Error = "Tài khoản hoặc mật khẩu không chính xác, hoặc bạn không có quyền Admin!";
            return View();
        }

        // ==========================================
        // 4. XỬ LÝ ĐĂNG XUẤT
        // ==========================================
        public ActionResult Logout()
        {
            Session.Remove("Account");
            FormsAuthentication.SignOut();
            return Redirect("~/Admin/Auth/Login");
        }
    }
}