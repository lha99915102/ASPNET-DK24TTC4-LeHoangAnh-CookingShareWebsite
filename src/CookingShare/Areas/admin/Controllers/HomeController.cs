using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity; 
using System.Web;
using System.Web.Mvc;
using CookingShare.Models;

namespace CookingShare.Areas.Admin.Controllers
{
    [Authorize] 
    public class HomeController : Controller
    {
        CookingShareDBEntities db = new CookingShareDBEntities();

        public ActionResult Index()
        {
            try
            {
                // 1. KIỂM TRA BẢO MẬT: Bắt buộc là Admin (Role = 1)
                var currentAcc = Session["Account"] as ACCOUNT;
                if (currentAcc == null || currentAcc.Role != 1)
                {
                    return RedirectToAction("Login", "Auth", new { area = "admin" });
                }

                // 2. THỐNG KÊ CÁC CON SỐ TRUYỀN VÀO VIEW 
                // Tổng số công thức đã được duyệt (Status = 1)
                ViewBag.TotalRecipes = db.RECIPE.Count(r => r.Status == 1);

                // Công thức đang chờ duyệt (Status = 0)
                ViewBag.PendingRecipesCount = db.RECIPE.Count(r => r.Status == 0);

                // Tổng số nguyên liệu gốc trong hệ thống
                ViewBag.TotalIngredients = db.INGREDIENT.Count();

                // Tổng số thành viên (Chỉ đếm người dùng bình thường, Role = 2)
                ViewBag.TotalUsers = db.ACCOUNT.Count(a => a.Role == 2);

                // 3. LẤY DANH SÁCH VIỆC CẦN LÀM (Gửi qua Model)
                // Lấy 5 công thức mới nhất đang chờ duyệt (Status = 0) để Admin xử lý nhanh
                var pendingRecipes = db.RECIPE
                                       .Include(r => r.ACCOUNT)
                                       .Include(r => r.ACCOUNT.PROFILE) 
                                       .Where(r => r.Status == 0)
                                       .OrderByDescending(r => r.CreateDate)
                                       .Take(5)
                                       .ToList();

                // Truyền danh sách công thức chờ duyệt sang View
                return View(pendingRecipes);
            }
            catch (Exception ex)
            {
                // Bắt lỗi nếu DB có vấn đề khi load trang chủ Admin
                TempData["Error"] = "Lỗi khi tải dữ liệu Dashboard: " + ex.Message;
                return View(new List<RECIPE>()); // Trả về list rỗng để tránh lỗi View
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