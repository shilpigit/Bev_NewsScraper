using System.Web.Mvc;
using System.Web.Routing;

namespace OilDiversity.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
               "news",
               "news",
               defaults: new { controller = "home", action = "index" });

            routes.MapRoute(
                "show",
                "news/show",
                defaults: new { controller = "show", action = "index" });

            routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
