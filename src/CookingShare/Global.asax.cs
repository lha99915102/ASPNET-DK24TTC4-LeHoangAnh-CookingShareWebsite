using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace CookingShare
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            // 1. Lấy lỗi cuối cùng xảy ra trên Server
            Exception exception = Server.GetLastError();
            HttpException httpException = exception as HttpException;

            // 2. Mặc định là lỗi 500 (Sập Server), nếu có mã cụ thể thì lấy mã đó
            int errorCode = httpException != null ? httpException.GetHttpCode() : 500;

            // 3. Kiểm tra xem người dùng đang đứng ở khu vực (URL) nào
            string currentUrl = Request.Path.ToLower();
            bool isAdminArea = currentUrl.StartsWith("/admin");

            // 4. Xóa lỗi trong bộ nhớ để IIS không tự động quăng trang lỗi vàng mặc định
            Server.ClearError();

            // 5. Phân luồng giao thông
            string route = "";

            if (isAdminArea)
            {
                // KHI LỖI Ở KHU VỰC ADMIN
                if (errorCode == 404) route = "~/Admin/Error/NotFound";
                else if (errorCode == 403) route = "~/Admin/Error/AccessDenied";
                else route = "~/Admin/Error/ServerError";
            }
            else
            {
                // KHI LỖI Ở KHU VỰC USER (Trang chủ)
                if (errorCode == 404) route = "~/Error/NotFound";
                else if (errorCode == 403) route = "~/Error/AccessDenied";
                else route = "~/Error/ServerError";
            }

            // 6. Đá người dùng sang đúng trang lỗi
            Response.Redirect(route);
        }
    }
}
