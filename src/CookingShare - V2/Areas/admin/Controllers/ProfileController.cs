using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CookingShare.Areas.admin.Controllers
{
    public class ProfileController : Controller
    {
        // GET: admin/Profile
        public ActionResult Index()
        {
            return View();
        }
    }
}