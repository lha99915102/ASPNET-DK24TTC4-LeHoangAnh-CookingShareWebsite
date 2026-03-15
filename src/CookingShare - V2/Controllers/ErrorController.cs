using CookingShare.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace CookingShare.Controllers
{
    public class ErrorController : Controller
    {
        CookingShareDBEntities db = new CookingShareDBEntities();

        public ActionResult NotFound()
        {
            // Lấy ngẫu nhiên 4 công thức đã được duyệt (Status = 1) để làm gợi ý cho người dùng
            var suggestedRecipes = db.RECIPE
                                     .Where(r => r.Status == 1)
                                     .OrderBy(r => Guid.NewGuid())
                                     .Take(4)
                                     .ToList();

            return View(suggestedRecipes);
        }

        public ActionResult UnderConstruction()
        {
            return View();
        }
    }
}