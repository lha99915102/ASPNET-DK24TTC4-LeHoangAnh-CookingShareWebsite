using CookingShare.Models;
using System.Linq;
using System.Web.Mvc;

namespace CookingShare.Areas.Admin.Controllers
{
    // Chặn người lạ chưa đăng nhập
    [Authorize]
    public class RecipeController : Controller
    {
        CookingShareDBEntities db = new CookingShareDBEntities();

        // ==========================================
        // 1. HIỂN THỊ TRANG DANH SÁCH TẤT CẢ CÔNG THỨC
        // ==========================================
        public ActionResult Index()
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return RedirectToAction("Login", "Auth", new { area = "Admin" });

            // Lấy tất cả công thức, sắp xếp mới nhất lên đầu
            var allRecipes = db.RECIPE.OrderByDescending(r => r.CreateDate).ToList();
            return View(allRecipes);
        }

        // ==========================================
        // 2. HIỂN THỊ TRANG CÁC BÀI ĐANG CHỜ DUYỆT
        // ==========================================
        public ActionResult Pending()
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return RedirectToAction("Login", "Auth", new { area = "Admin" });

            // Chỉ lấy công thức có Status = 0
            var pendingRecipes = db.RECIPE.Where(r => r.Status == 0).OrderByDescending(r => r.CreateDate).ToList();
            return View(pendingRecipes);
        }

        // ==========================================
        // 3. XỬ LÝ NÚT BẤM "DUYỆT BÀI"
        // ==========================================
        public ActionResult Approve(int id)
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return RedirectToAction("Login", "Auth", new { area = "Admin" });

            var recipe = db.RECIPE.Find(id);
            if (recipe != null && recipe.Status == 0)
            {
                recipe.Status = 1; // 1 là Đã duyệt
                db.SaveChanges();
            }

            // Mẹo hay: Duyệt xong thì trả người dùng về đúng trang họ vừa đứng (chứ không ép về Home nữa)
            if (Request.UrlReferrer != null)
            {
                return Redirect(Request.UrlReferrer.ToString());
            }
            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }

        // ==========================================
        // 4. XỬ LÝ NÚT BẤM "TỪ CHỐI BÀI" (THÊM CHO ĐỦ BỘ)
        // ==========================================
        public ActionResult Reject(int id)
        {
            var currentAcc = Session["Account"] as ACCOUNT;
            if (currentAcc == null || currentAcc.Role != 1) return RedirectToAction("Login", "Auth", new { area = "Admin" });

            var recipe = db.RECIPE.Find(id);
            if (recipe != null && recipe.Status == 0)
            {
                recipe.Status = 2; // Giả sử 2 là Từ chối / Vi phạm
                db.SaveChanges();
            }

            if (Request.UrlReferrer != null) return Redirect(Request.UrlReferrer.ToString());
            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }
    }
}