using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CookingShare.Models;

namespace CookingShare.Controllers
{
    public class HomeController : Controller
    {
        // Khởi tạo kết nối DB
        CookingShareDBEntities db = new CookingShareDBEntities();

        public ActionResult Index()
        {
            // 1. Lấy danh sách 8 công thức mới nhất có Status = 1 (đã duyệt)
            var listCongThucMoi = db.RECIPE
                                    .Where(r => r.Status == 1)
                                    .OrderByDescending(r => r.CreateDate)
                                    .Take(8) // Nên để 8 để hiển thị thành 2 hàng đẹp mắt
                                    .ToList();

            // 2. LẤY DANH SÁCH DANH MỤC ĐỂ GẮN ID CHO TRANG CHỦ (Dòng quan trọng nhất)
            ViewBag.Categories = db.CATEGORY.ToList();

            // Truyền dữ liệu món ăn sang View
            return View(listCongThucMoi);
        }
    }
}