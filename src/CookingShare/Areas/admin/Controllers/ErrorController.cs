using System.Web.Mvc;

namespace CookingShare.Areas.Admin.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult NotFound()
        {
            Response.StatusCode = 404;
            return View();
        }
    }
}