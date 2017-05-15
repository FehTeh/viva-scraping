using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using viva.scraping.Helper;
using viva.scraping.Models;

namespace viva.scraping.Controllers
{
    [Route("api/[controller]")]
    public class MyDataController : Controller
    {
        public const string MyDataURL = "https://www.portalviva.pt/lx/pt/myvivaclient/client-account-area/my-data.aspx";


        [HttpGet]
        [Authorize("Bearer")]
        public async Task<IActionResult> Get()
        {
            MyData result = new MyData();

            var response = await DataRequester.Get(User, MyDataURL);

            if(String.IsNullOrEmpty(response))
            {
                return StatusCode(401);
            }

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(response);

            var username = doc.DocumentNode.Descendants("span").Where(d => d.Attributes.Contains("id") && d.Attributes["id"].Value.Contains("ContentPlaceHolderDefault_content_AccountViva_Tabs_ClientProfile_10_lblFullName")).First();
            result.fullname = username.InnerText;

            var birthdate = doc.DocumentNode.Descendants("span").Where(d => d.Attributes.Contains("id") && d.Attributes["id"].Value.Contains("ContentPlaceHolderDefault_content_AccountViva_Tabs_ClientProfile_10_lblBirthDate")).First();
            result.birthdate = birthdate.InnerText;

            var gender = doc.DocumentNode.Descendants("span").Where(d => d.Attributes.Contains("id") && d.Attributes["id"].Value.Contains("ContentPlaceHolderDefault_content_AccountViva_Tabs_ClientProfile_10_lblGender")).First();
            result.gender = gender.InnerText;

            var address = doc.DocumentNode.Descendants("span").Where(d => d.Attributes.Contains("id") && d.Attributes["id"].Value.Contains("ContentPlaceHolderDefault_content_AccountViva_Tabs_ClientProfile_10_lblAddress")).First();
            result.address = address.InnerText;

            return Json(result);
        }
    }
}
