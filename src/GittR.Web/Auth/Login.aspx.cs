using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GittR.Web.Auth
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (User.Identity.IsAuthenticated)
            {
                Response.Redirect("~/");
            }

            var clientId = ConfigurationManager.AppSettings["GitHub:ClientID"];
            var callbackUrl = ConfigurationManager.AppSettings["GitHub:CallbackHost"] + GetRouteUrl("OAuthCallback", null);
            var scopes = "repo";
            var state = Guid.NewGuid().ToString();

            Response.Cookies.Add(new HttpCookie("xsrftoken", state) { HttpOnly = true });

            var url = BuildGitHubUrl(clientId, callbackUrl, scopes, state);
            Response.Redirect(url);
        }

        private static string BuildGitHubUrl(string clientId, string callbackUrl, string scopes, string state)
        {
            var url = "https://github.com/login/oauth/authorize?client_id={0}&redirect_uri={1}&scope={2}&state={3}";
            return String.Format(url, HttpUtility.UrlEncode(clientId), HttpUtility.UrlEncode(callbackUrl), HttpUtility.UrlEncode(scopes), HttpUtility.UrlEncode(state));
        }
    }
}