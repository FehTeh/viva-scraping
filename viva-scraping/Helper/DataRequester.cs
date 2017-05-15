using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace viva.scraping.Helper
{
    public class DataRequester
    {
        public static async Task<string> Get(ClaimsPrincipal user, string url)
        {
            CookieContainer cookieContainer = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = cookieContainer;

            var claimsIdentity = user.Identity as ClaimsIdentity;
            var claims = claimsIdentity.Claims;

            foreach (var c in claims)
            {
                if (c.Type == "ASP.NET_SessionId" || c.Type == "yourAuthCookie")
                {
                    handler.CookieContainer.Add(new Uri(url), new Cookie(c.Type, c.Value)); // Adding a Cookie
                }
            }

            using (var httpclient = new HttpClient(handler))
            {
                var httpResponse = await httpclient.GetAsync(url);

                if (httpResponse.Content != null && httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var responseContent = await httpResponse.Content.ReadAsStringAsync();

                    if (responseContent.Contains("A sua conta VIVA"))
                    {
                        return responseContent;
                    }
                }
            }

            return null;
        }

        public static async Task<string> Post(ClaimsPrincipal user, string url, List<KeyValuePair<string, string>> lkvp)
        {
            CookieContainer cookieContainer = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = cookieContainer;

            var claimsIdentity = user.Identity as ClaimsIdentity;
            var claims = claimsIdentity.Claims;

            foreach (var c in claims)
            {
                if (c.Type == "ASP.NET_SessionId" || c.Type == "yourAuthCookie")
                {
                    handler.CookieContainer.Add(new Uri(url), new Cookie(c.Type, c.Value)); // Adding a Cookie
                }
            }

            var httpContent = new FormUrlEncodedContent(lkvp);

            using (var httpclient = new HttpClient(handler))
            {
                var httpResponse = await httpclient.PostAsync(url, httpContent);

                if (httpResponse.Content != null && httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var responseContent = await httpResponse.Content.ReadAsStringAsync();

                    if (responseContent.Contains("A sua conta VIVA"))
                    {
                        return responseContent;
                    }
                }
            }

            return null;
        }
    }
}
