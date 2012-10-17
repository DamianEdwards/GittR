using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace GittR.Web
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapPageRoute("Login", "auth/login", "~/Auth/Login.aspx");
            routes.MapPageRoute("Logout", "auth/logout", "~/Auth/Logout.aspx");
            routes.MapPageRoute("OAuthCallback", "oauth/callback", "~/Auth/Callback.aspx");
        }
    }
}