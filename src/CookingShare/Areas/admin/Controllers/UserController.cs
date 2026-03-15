using CookingShare.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;

namespace CookingShare.Areas.Admin.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        CookingShareDBEntities db = new CookingShareDBEntities();

        // Hàm băm mật khẩu (dùng khi tạo user mới)
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
        // 1. HIỂN THỊ DANH SÁCH & THỐNG KÊ
        // ==========================================
        public ActionResult Index()
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return Redirect("~/Admin/Auth/Login");

            var users = db.ACCOUNT.OrderByDescending(u => u.ID).ToList();

            // Thống kê cho 3 thẻ Card trên cùng
            ViewBag.TotalUsers = users.Count;
            ViewBag.ActiveUsers = users.Count(u => u.Status == 1);
            ViewBag.BannedUsers = users.Count(u => u.Status == 0);
            ViewBag.CurrentAdminID = currentAcc.ID; // Dùng để chặn nút tự khóa tài khoản của chính mình

            return View(users);
        }

        // ==========================================
        // 2. THÊM HOẶC CẬP NHẬT NGƯỜI DÙNG
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(int ID, string FullName, string Email, int Role, int Status)
        {
            try
            {
                if (ID == 0) // THÊM MỚI
                {
                    // Tạo Account
                    var newAcc = new ACCOUNT
                    {
                        UserName = Email.Split('@')[0] + new Random().Next(100, 999), // Tạo username tạm từ email
                        Email = Email,
                        Password = HashPasswordSHA256("123456"), // Mật khẩu mặc định
                        Role = Role,
                        Status = Status,
                        RegistDate = DateTime.Now
                    };
                    db.ACCOUNT.Add(newAcc);
                    db.SaveChanges(); // Phải save để lấy ID tạo Profile

                    // Tạo Profile
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
                else // CẬP NHẬT
                {
                    var acc = db.ACCOUNT.Find(ID);
                    if (acc != null)
                    {
                        acc.Email = Email;
                        acc.Role = Role;
                        acc.Status = Status;

                        var profile = db.PROFILE.FirstOrDefault(p => p.AccountID == ID);
                        if (profile != null)
                        {
                            profile.FullName = FullName;
                        }
                        else
                        {
                            // Nếu lỗi mất Profile thì tạo lại
                            db.PROFILE.Add(new PROFILE { AccountID = ID, FullName = FullName, Avatar = "default-avatar.png" });
                        }
                        db.SaveChanges();
                        TempData["Success"] = "Cập nhật thông tin thành công!";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi hệ thống: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // ==========================================
        // 3. KHÓA / MỞ KHÓA NHANH
        // ==========================================
        [HttpPost]
        public ActionResult ToggleStatus(int id)
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc != null && currentAcc.ID == id)
            {
                TempData["Error"] = "Bạn không thể tự khóa tài khoản của chính mình!";
                return RedirectToAction("Index");
            }

            var acc = db.ACCOUNT.Find(id);
            if (acc != null)
            {
                // Đảo ngược trạng thái: 1 thành 0, 0 thành 1
                acc.Status = (acc.Status == 1) ? 0 : 1;
                db.SaveChanges();
                TempData["Success"] = "Đã thay đổi trạng thái tài khoản!";
            }
            return RedirectToAction("Index");
        }
    }
}