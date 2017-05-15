using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using viva.scraping.Auth;
using viva.scraping.Models;

namespace viva.scraping.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        public const string AuthURL = "https://www.portalviva.pt/lx/pt/public/login.aspx";


        [HttpPost]
        public async Task<IActionResult> Post([FromBody]AuthRequest req)
        {
            var nvc = new List<KeyValuePair<string, string>>();
            nvc.Add(new KeyValuePair<string, string>("__EVENTTARGET", "ctl00$ctl00$ctl00$ContentPlaceHolderDefault$content$ControlLogin_9$lnkLogin"));
            nvc.Add(new KeyValuePair<string, string>("__EVENTARGUMENT", ""));
            nvc.Add(new KeyValuePair<string, string>("__LASTFOCUS", ""));
            nvc.Add(new KeyValuePair<string, string>("__VIEWSTATE", "/wEPDwUENTM4MQ9kFgJmD2QWAmYPZBYCZg9kFgJmD2QWAgIFEGRkFgQCAw9kFgJmD2QWAgIDDxAPFgYeDURhdGFUZXh0RmllbGQFBFRleHQeDkRhdGFWYWx1ZUZpZWxkBQVWYWx1ZR4LXyFEYXRhQm91bmRnZBAVAQZMaXNib2EVAQExFCsDAWcWAWZkAgcPZBYCZg9kFhACAw8PFgIeBFRleHQFASBkZAIFDw8WAh8DBQpwb3J0dWd1w6pzZGQCCQ8PFgIfAwUGTXlWSVZBZGQCDQ8PFgIfAwUEc2FpcmRkAhEPDxYCHwMFCHJlZ2lzdGFyZGQCEw9kFgQCAQ8WAh4HVmlzaWJsZWhkAgMPDxYCHwNlZGQCFQ8WBB8DBQRPbMOhHwRoZAIXDxYCHwRoZGTsdoN06+ATwOJFnWfP0Ivs1W3XIA=="));
            nvc.Add(new KeyValuePair<string, string>("ctl00$ctl00$ctl00$ContentPlaceHolderDefault$Region_3$ddlRegion", "1"));
            nvc.Add(new KeyValuePair<string, string>("ctl00$ctl00$ctl00$ContentPlaceHolderDefault$SearchBox_6$txtSearch", ""));
            nvc.Add(new KeyValuePair<string, string>("__VIEWSTATEGENERATOR", "CA0B0334"));
            nvc.Add(new KeyValuePair<string, string>("ctl00$ctl00$ctl00$ContentPlaceHolderDefault$content$ControlLogin_9$txtEmail", req.username));
            nvc.Add(new KeyValuePair<string, string>("ctl00$ctl00$ctl00$ContentPlaceHolderDefault$content$ControlLogin_9$txtPassword", req.password));

            var httpContent = new FormUrlEncodedContent(nvc);

            var result = new List<Cookie>();

            var token = String.Empty;

            CookieContainer cookies = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = cookies;

            using (var httpclient = new HttpClient(handler))
            {

                var httpResponse = await httpclient.PostAsync(AuthURL, httpContent);

                if (httpResponse.Content != null && httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var responseContent = await httpResponse.Content.ReadAsStringAsync();

                    if (responseContent.Contains("A sua conta VIVA"))
                    {
                        Uri uri = new Uri(AuthURL);
                        IEnumerable<Cookie> responseCookies = cookies.GetCookies(uri).Cast<Cookie>();
                        foreach (Cookie c in responseCookies)
                        {
                            result.Add(c);
                        }
                    }
                }
            }

            if(result.Count > 0)
            {
                var requestAt = DateTime.Now;
                var expiresIn = requestAt + TokenAuthOption.ExpiresSpan;
                token = GenerateToken(req.username, result, expiresIn);
            }

            if(String.IsNullOrEmpty(token))
            {
                return StatusCode(400);
            }

            return Json(new AuthResponse() { accessToken = token });
        }

        private string GenerateToken(string username, List<Cookie> cookies, DateTime expires)
        {
            var handler = new JwtSecurityTokenHandler();

            var claims = new List<Claim>();
            foreach(var c in cookies)
            {
                claims.Add(new Claim(c.Name, c.Value));
            }
            
            ClaimsIdentity identity = new ClaimsIdentity(
                new GenericIdentity(username, "TokenAuth"),
                claims.ToArray()
            );

            var securityToken = handler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = TokenAuthOption.Issuer,
                Audience = TokenAuthOption.Audience,
                SigningCredentials = TokenAuthOption.SigningCredentials,
                Subject = identity,
                Expires = expires
            });

            return handler.WriteToken(securityToken);
        }

    }
}
