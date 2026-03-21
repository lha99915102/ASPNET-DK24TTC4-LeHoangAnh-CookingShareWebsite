using CookingShare.Models;
using System;
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;

namespace CookingShare.Areas.admin.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        CookingShareDBEntities db = new CookingShareDBEntities();

        //  HIỂN THỊ DANH SÁCH & THỐNG KÊ
        public ActionResult Index()
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            // Dùng RedirectToAction kèm tham số area để an toàn tuyệt đối
            if (currentAcc == null || currentAcc.Role != 1) return RedirectToAction("Login", "Auth", new { area = "admin" });

            // Include thêm PROFILE để lấy được họ tên hiển thị ngoài View
            var users = db.ACCOUNT.Include(a => a.PROFILE).Include(a => a.RECIPE).OrderByDescending(u => u.ID).ToList();

            ViewBag.TotalUsers = users.Count;
            ViewBag.ActiveUsers = users.Count(u => u.Status == 1);
            ViewBag.BannedUsers = users.Count(u => u.Status == 0);
            ViewBag.CurrentAdminID = currentAcc.ID;

            return View(users);
        }

        // THÊM MỚI HOẶC CẬP NHẬT TÀI KHOẢN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(int ID, string FullName, string UserName, string Email, string Phone, string Password, int Role, int Status)
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return RedirectToAction("Login", "Auth", new { area = "admin" });

            try
            {
                // Chuẩn hóa dữ liệu đầu vào (cắt khoảng trắng thừa)
                Email = Email?.Trim();
                UserName = UserName?.Trim();
                Phone = Phone?.Trim();

                if (ID == 0)
                {
                    var checkExist = db.ACCOUNT.FirstOrDefault(a => a.Email == Email || a.UserName == UserName || (!string.IsNullOrEmpty(Phone) && a.Phone == Phone));
                    if (checkExist != null)
                    {
                        TempData["Error"] = "Email, Tên đăng nhập hoặc Số điện thoại đã được sử dụng bởi tài khoản khác!";
                        return RedirectToAction("Index");
                    }

                    string passToHash = string.IsNullOrEmpty(Password) ? "123456" : Password;

                    var newAcc = new ACCOUNT
                    {
                        UserName = string.IsNullOrEmpty(UserName) ? Email.Split('@')[0] + new Random().Next(10, 99).ToString() : UserName,
                        Email = Email,
                        Phone = Phone,
                        Password = CookingShare.Models.SecurityHelper.HashPasswordSHA256(passToHash),
                        Role = Role,
                        Status = Status,
                        RegistDate = DateTime.Now
                    };
                    db.ACCOUNT.Add(newAcc);
                    db.SaveChanges(); // Lưu để lấy ID tài khoản mới

                    var newProfile = new PROFILE
                    {
                        AccountID = newAcc.ID,
                        FullName = FullName,
                        Avatar = "default-avatar.png"
                    };
                    db.PROFILE.Add(newProfile);
                    db.SaveChanges();

                    TempData["Success"] = "Đã tạo tài khoản mới thành công!";
                }
                else //  CẬP NHẬT 
                {
                    // Kiểm tra trùng lặp (loại trừ chính tài khoản đang sửa)
                    var checkExist = db.ACCOUNT.FirstOrDefault(a => a.ID != ID && (a.Email == Email || a.UserName == UserName || (!string.IsNullOrEmpty(Phone) && a.Phone == Phone)));
                    if (checkExist != null)
                    {
                        TempData["Error"] = "Email, Tên đăng nhập hoặc Số điện thoại này đã bị trùng với một người dùng khác!";
                        return RedirectToAction("Index");
                    }

                    var acc = db.ACCOUNT.Find(ID);
                    if (acc != null)
                    {
                        acc.UserName = string.IsNullOrEmpty(UserName) ? acc.UserName : UserName;
                        acc.Email = Email;
                        acc.Phone = Phone;
                        acc.Role = Role;
                        acc.Status = Status;

                        // Chỉ đổi mật khẩu nếu Admin có nhập mật khẩu mới
                        if (!string.IsNullOrEmpty(Password))
                        {
                            acc.Password = CookingShare.Models.SecurityHelper.HashPasswordSHA256(Password);
                        }

                        var profile = db.PROFILE.FirstOrDefault(p => p.AccountID == ID);
                        if (profile != null)
                        {
                            profile.FullName = FullName;
                        }
                        else
                        {
                            db.PROFILE.Add(new PROFILE { AccountID = ID, FullName = FullName, Avatar = "default-avatar.png" });
                        }

                        db.SaveChanges();
                        TempData["Success"] = "Cập nhật thông tin tài khoản thành công!";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi hệ thống: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        //  KHÓA / MỞ KHÓA NHANH
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(int id)
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return RedirectToAction("Login", "Auth", new { area = "admin" });

            if (currentAcc.ID == id)
            {
                TempData["Error"] = "Bạn không thể tự khóa tài khoản của chính mình!";
                return RedirectToAction("Index");
            }

            try
            {
                var acc = db.ACCOUNT.Find(id);
                if (acc != null)
                {
                    acc.Status = (acc.Status == 1) ? 0 : 1;
                    db.SaveChanges();
                    TempData["Success"] = acc.Status == 0 ? "Đã khóa (Ban) tài khoản thành công!" : "Đã mở khóa tài khoản!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // XEM CHI TIẾT HOẠT ĐỘNG CỦA USER
        public ActionResult Details(int id)
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return RedirectToAction("Login", "Auth", new { area = "admin" });

            // Nạp toàn bộ dữ liệu liên quan của user này
            var user = db.ACCOUNT
                         .Include(a => a.PROFILE)
                         .Include(a => a.RECIPE)
                         .Include(a => a.COMMENT)
                         .Include(a => a.COOKSNAP)
                         .FirstOrDefault(u => u.ID == id);

            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng này.";
                return RedirectToAction("Index");
            }

            return View(user);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { db.Dispose(); }
            base.Dispose(disposing);
        }
    }
}