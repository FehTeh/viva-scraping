using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using viva.scraping.Helper;
using viva.scraping.Models;

namespace viva.scraping.Controllers
{
    [Route("api/[controller]")]
    public class TransactionsController : Controller
    {
        public const string TransactionURL = "https://www.portalviva.pt/lx/pt/myvivaclient/client-account-area/movimentos.aspx";


        [HttpGet]
        [Authorize("Bearer")]
        public async Task<IActionResult> Get()
        {
            Cards result = new Cards();
            result.cards = new List<Card>();

            var response = await DataRequester.Get(User, TransactionURL);

            if (String.IsNullOrEmpty(response))
            {
                return StatusCode(401);
            }

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(response);

            var cardsSelect = doc.DocumentNode.Descendants("select").Where(d => d.Attributes.Contains("id") && d.Attributes["id"].Value.Contains("ContentPlaceHolderDefault_content_AccountViva_Tabs_ClientTransactions_10_dropMyCards")).First();

            var cardsOptions = cardsSelect.Descendants("option").Where(d => d.Attributes.Contains("value"));

            foreach(var c in cardsOptions)
            {
                result.cards.Add(new Card() { id = c.Attributes["value"].Value, name = c.InnerText });
            }

            return Json(result);
        }

        [HttpGet("{id}")]
        [Authorize("Bearer")]
        public async Task<IActionResult> Get(string id)
        {
            var responseGet = await DataRequester.Get(User, TransactionURL);

            if (String.IsNullOrEmpty(responseGet))
            {
                return StatusCode(401);
            }

            HtmlDocument docGet = new HtmlDocument();
            docGet.LoadHtml(responseGet);

            var nvc = new List<KeyValuePair<string, string>>();

            var inputs = docGet.DocumentNode.Descendants("input").ToList();
            foreach (var i in inputs)
            {
                var name = i.Attributes["name"] != null ? i.Attributes["name"].Value : i.Id;

                if(!String.IsNullOrEmpty(name))
                {
                    var value = i.Attributes["value"] != null ? i.Attributes["value"].Value : String.Empty;

                    nvc.Add(new KeyValuePair<string, string>(name, value));
                }
            }
                    
            var selects = docGet.DocumentNode.Descendants("select").ToList();
            foreach (var s in selects)
            {
                var name = s.Attributes["name"] != null ? s.Attributes["name"].Value : s.Id;

                if(name == "regiao")
                {
                    name = "ctl00$ctl00$ctl00$ctl00$ContentPlaceHolderDefault$Region_3$ddlRegion";
                }

                if (!String.IsNullOrEmpty(name))
                {
                    var value = s.Descendants("option").Where(d => d.Attributes.Contains("selected")).First().Attributes["value"].Value;

                    if (name.Contains("dropMyCards"))
                    {
                        value = id;
                    }

                    nvc.Add(new KeyValuePair<string, string>(name, value));
                }
            }

            nvc.Add(new KeyValuePair<string, string>("__EVENTTARGET", "ctl00$ctl00$ctl00$ctl00$ContentPlaceHolderDefault$content$AccountViva_Tabs$ClientTransactions_10$lnkSearch"));
            nvc.Add(new KeyValuePair<string, string>("__EVENTARGUMENT", ""));
            nvc.Add(new KeyValuePair<string, string>("__LASTFOCUS", ""));

            Transactions result = new Transactions();
            result.transactions = new List<Transaction>();

            var response = await DataRequester.Post(User, TransactionURL, nvc);

            if (String.IsNullOrEmpty(response))
            {
                return StatusCode(401);
            }

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(response);

            var transTableDiv = doc.DocumentNode.Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("lastLoads_list")).First();

            var transTable = transTableDiv.Descendants("td").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("titleNameCell"));

            foreach(var t in transTable)
            {
                var transTableTd = t.ParentNode.Descendants("td").ToList();

                var trans = new Transaction();
                trans.title = transTableTd[1].InnerText.Trim();
                trans.amount = transTableTd[2].InnerText.Trim();
                trans.charged_in = transTableTd[3].InnerText.Trim();
                trans.charged_in_place = transTableTd[4].InnerText.Trim();
                trans.validated_in = transTableTd[5].InnerText.Trim();
                trans.validated_in_place = transTableTd[6].InnerText.Trim();
                trans.expires_in = transTableTd[7].InnerText.Trim();

                result.transactions.Add(trans);
            }

            return Json(result);
        }
    }
}
