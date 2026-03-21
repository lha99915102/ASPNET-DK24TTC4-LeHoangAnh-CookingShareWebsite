using CookingShare.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace CookingShare.Areas.admin.Controllers
{
    public class AuthController : Controller
    {
        CookingShareDBEntities db = new CookingShareDBEntities();


        // HIỂN THỊ GIAO DIỆN ĐĂNG NHẬP
        [HttpGet]
        public ActionResult Login()
        {
            // Nếu đã đăng nhập và là Admin thì đẩy thẳng vào Dashboard
            if (Session["Account"] != null)
            {
                var acc = (ACCOUNT)Session["Account"];
                if (acc.Role == 1)
                {
                    // ĐÃ SỬA: Dùng RedirectToAction kèm Area để chống gãy link
                    return RedirectToAction("Index", "Home", new { area = "admin" });
                }
            }
            return View();
        }


        //  XỬ LÝ KHI NGƯỜI DÙNG ẤN NÚT ĐĂNG NHẬP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password, bool rememberMe = false)
        {
            //  Tái sử dụng hàm băm mật khẩu dùng chung của toàn hệ thống (DRY)
            string hashedPassword = CookingShare.Models.SecurityHelper.HashPasswordSHA256(password);

            var admin = db.ACCOUNT.Include(a => a.PROFILE).FirstOrDefault(a =>
                (a.UserName == username || a.Email == username || a.Phone == username) &&
                a.Password == hashedPassword &&
                a.Role == 1);

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
                FormsAuthentication.SetAuthCookie(admin.UserName, rememberMe);

                //  Dùng RedirectToAction kèm Area để chống gãy link
                return RedirectToAction("Index", "Home", new { area = "admin" });
            }

            // Đăng nhập thất bại
            ViewBag.Error = "Thông tin đăng nhập không chính xác, hoặc bạn không có quyền Admin!";
            return View();
        }


        //  XỬ LÝ ĐĂNG XUẤT
        public ActionResult Logout()
        {
            Session.Remove("Account");
            FormsAuthentication.SignOut();

            //  Dùng RedirectToAction kèm Area để chống gãy link
            return RedirectToAction("Login", "Auth", new { area = "admin" });
        }

        // Dọn dẹp kết nối Database giải phóng RAM
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