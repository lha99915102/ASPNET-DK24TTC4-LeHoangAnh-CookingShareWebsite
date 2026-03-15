using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CookingShare.Areas.admin.Controllers
{
    public class RecipeController : Controller
    {
        // GET: admin/Recipe
        public ActionResult Index()
        {
            return View();
        }
    }
}