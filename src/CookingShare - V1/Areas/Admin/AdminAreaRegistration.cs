using System.Web.Mvc;

namespace CookingShare.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                // THÊM controller = "Home" VÀO ĐÂY NHÉ:
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                new[] { "CookingShare.Areas.Admin.Controllers" }
            );
        }
    }
}