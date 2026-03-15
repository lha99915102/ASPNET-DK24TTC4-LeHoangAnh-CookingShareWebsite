using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CookingShare.Areas.admin.Controllers
{
    public class ErrorController : Controller
    {
        // GET: admin/Error
        public ActionResult NotFound()
        {
            return View();
        }
    }
}