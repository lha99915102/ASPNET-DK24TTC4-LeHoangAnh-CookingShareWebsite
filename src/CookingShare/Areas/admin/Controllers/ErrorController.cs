using System.Web.Mvc;

namespace CookingShare.Areas.Admin.Controllers
{
    //  Mở cửa tự do cho trang Lỗi để chống vòng lặp Redirect vô tận
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        // TRANG LỖI 404 (Không tìm thấy trang)
        public ActionResult NotFound()
        {
            // Ép Server trả về đúng mã 404 thay vì 200
            Response.StatusCode = 404;

            // Chặn IIS hiển thị trang lỗi màu vàng xấu xí mặc định của máy chủ
            Response.TrySkipIisCustomErrors = true;

            return View("NotFound");
        }

        // TRANG LỖI 500 (Lỗi hệ thống, đứt cáp DB, code văng Exception...)
        public ActionResult ServerError()
        {
            Response.StatusCode = 500;
            Response.TrySkipIisCustomErrors = true;

            return View("ServerError");
        }

        // TRANG LỖI 403 (Cấm truy cập - Dành cho User thường lén gõ link Admin)
        public ActionResult AccessDenied()
        {
            Response.StatusCode = 403;
            Response.TrySkipIisCustomErrors = true;

            return View("AccessDenied");
        }
    }
}