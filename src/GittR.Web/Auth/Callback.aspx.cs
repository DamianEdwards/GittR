using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;

namespace GittR.Web.Auth
{
    public partial class Callback : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var error = Request.QueryString["error"];
            
            if (!String.IsNullOrWhiteSpace(error))
            {
                errorMessage.Text = error;
                return;
            }

            var code = Request.QueryString["code"];
            var state = Request.QueryString["state"];
            var xsrfToken = GetXsrfToken();

            if (String.IsNullOrWhiteSpace(state) || state != xsrfToken)
            {
                throw new InvalidOperationException("XSRF token validation error.");
            }

            if (String.IsNullOrWhiteSpace(code))
            {
                throw new InvalidOperationException("Access token code missing.");
            }

            RegisterAsyncTask(new PageAsyncTask(async () =>
                {
                    var clientId = ConfigurationManager.AppSettings["GitHub:ClientID"];
                    var clientSecret = ConfigurationManager.AppSettings["GitHub:ClientSecret"];
                    var callbackUrl = ConfigurationManager.AppSettings["GitHub:CallbackHost"] + GetRouteUrl("OAuthCallback", null);

                    string authTokenResultJson;
                    using (var client = new HttpClient())
                    {
                        var url = BuildGitHubAccessTokenUrl(clientId, callbackUrl, clientSecret, code, state);
                        var request = new HttpRequestMessage(HttpMethod.Post, url);
                        request.Headers.Add("Accept", "application/json");
                        
                        using (var response = await client.SendAsync(request))
                        {
                            authTokenResultJson = await response.Content.ReadAsStringAsync();
                        }
                    }
                    var authTokenResult = JsonConvert.DeserializeAnonymousType(authTokenResultJson, new { access_token = "", token_type = "" });

                    // Get the user name from GitHub
                    string userResultJson;
                    using (var client = new HttpClient())
                    {
                        var url = "https://api.github.com/user?access_token=" + authTokenResult.access_token;
                        userResultJson = await client.GetStringAsync(url);
                    }
                    var userResult = JsonConvert.DeserializeAnonymousType(userResultJson, new { login = "", avatar_url = "" });

                    var authCookie = GetFormsAuthCookie(authTokenResult.access_token, userResult.login);
                    Response.Cookies.Add(authCookie);
                    Response.Redirect("~/");
                }));
        }

        private string GetXsrfToken()
        {
            var cookie = Request.Cookies["xsrftoken"];
            return cookie != null ? cookie.Value : String.Empty;
        }

        private static string BuildGitHubAccessTokenUrl(string clientId, string callbackUrl, string clientSecret, string code, string state)
        {
            var url = "https://github.com/login/oauth/access_token?client_id={0}&redirect_uri={1}&client_secret={2}&code={3}&state={4}";
            return String.Format(url, HttpUtility.UrlEncode(clientId), HttpUtility.UrlEncode(callbackUrl), HttpUtility.UrlEncode(clientSecret), HttpUtility.UrlEncode(code), HttpUtility.UrlEncode(state));
        }

        private static HttpCookie GetFormsAuthCookie(string accessToken, string login)
        {
            var formsAuthTicket = new FormsAuthenticationTicket(4, login, DateTime.Now, DateTime.Now.Add(FormsAuthentication.Timeout), true, accessToken, FormsAuthentication.FormsCookiePath);
            var encryptedTicket = FormsAuthentication.Encrypt(formsAuthTicket);

            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
            {
                HttpOnly = true,
                Path = FormsAuthentication.FormsCookiePath
            };

            if (FormsAuthentication.RequireSSL)
            {
                cookie.Secure = true;
            }

            if (FormsAuthentication.CookieDomain != null)
            {
                cookie.Domain = FormsAuthentication.CookieDomain;
            }

            if (formsAuthTicket.IsPersistent)
            {
                cookie.Expires = formsAuthTicket.Expiration;
            }

            return cookie;
        }
    }
}