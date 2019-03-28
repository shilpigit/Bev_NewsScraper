using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using OilDiversity.Web.Models;
using OilDiversity.Web.Repository;
using OilDiversity.Web.Utility;
using System.Runtime.Caching;

namespace OilDiversity.Web.Controllers
{
    public class HomeController : Controller
    {
        [Route("/news")]
        public ActionResult Index()
        {
            return View();
        }



        [ChildActionOnly]
        public ActionResult GetChart()
        {
            try
            {
                return Content(Task.Run(NewsPileUps.GetChartInOilPrice).Result, "text/html");
            }
            catch (Exception ex)
            {
                var message = $"Message: {ex.Message} \t Date: {DateTime.Now}";
                System.IO.File.WriteAllText(Server.MapPath(ConfigurationManager.AppSettings["LogPath"] + "log.txt"),
                    message, Encoding.UTF8);

                return Content("", "text/html");
            }
        }



        [ChildActionOnly]
        public ActionResult GetLatestNewsPileUps(NewsPileUp.RssSource sourceName)
        {
            try
            {
                var cachesourceName = System.Runtime.Caching.MemoryCache.Default[sourceName.ToString()];
                if (cachesourceName == null)
                {
                    var htmlString = "";
                    var isLine = false;
                    var latestNewsPileUps = Task.Run(() => NewsPileUps.GetLatestBySourceName(sourceName)).Result;

                    if (!latestNewsPileUps.Any()) return Content(htmlString, "text/html");
                    var path = Server.MapPath(ConfigurationManager.AppSettings["HtmlTemplatesPath"]);

                    if (sourceName == NewsPileUp.RssSource.OilPrice)
                    {
                        htmlString = GenerationCardTemplate(path, sourceName, latestNewsPileUps);
                    }
                    else
                    {
                        foreach (var news in latestNewsPileUps)
                            if (string.IsNullOrEmpty(news.ImageUrl) && string.IsNullOrEmpty(news.Summary))
                            {
                                isLine = true;
                                htmlString +=
                                    string.Format(
                                        System.IO.File.ReadAllText(path + "Line.html"),
                                        news.Title,
                                        CommonTools.Encrypt(news.Url),
                                        sourceName);
                            }
                            else if (string.IsNullOrEmpty(news.ImageUrl) && !string.IsNullOrEmpty(news.Summary))
                            {
                                htmlString +=
                                    string.Format(
                                        System.IO.File.ReadAllText(path + "WithSummary.html"),
                                        news.Title,
                                        news.Summary,
                                        CommonTools.Encrypt(news.Url),
                                        sourceName);
                            }
                            else
                            {
                                htmlString +=
                                    string.Format(
                                       System.IO.File.ReadAllText(path + (sourceName == NewsPileUp.RssSource.EnergyVoice ? "SmallImageLine.html" : "ImageLine.html")),
                                        news.ImageUrl,
                                        news.Title,
                                        news.Summary,
                                        CommonTools.Encrypt(news.Url),
                                        sourceName);
                            }
                    }
                    htmlString = isLine
                        ? $"<div class=\"well line-wrapper col-lg-12 col-md-12 col-sm-12 col-xs-12 text-left animated fadeIn x-slow-animated\">{htmlString}</div>"
                        : htmlString;
                    System.Runtime.Caching.MemoryCache.Default[sourceName.ToString()] = htmlString;
                    return Content(htmlString, "text/html");
                }
                else
                    return Content(cachesourceName.ToString(), "text/html");


            }
            catch (Exception ex)
            {
                var message = $"Message: {ex.Message} \t Date: {DateTime.Now}";
                System.IO.File.WriteAllText(Server.MapPath(ConfigurationManager.AppSettings["LogPath"] + "log.txt"),
                            message, Encoding.UTF8);

                return Content("", "text/html");
            }
        }

        private static string GenerationCardTemplate(string path, NewsPileUp.RssSource sourceName, IReadOnlyList<NewsPileUp> newsPileUps)
        {
            const string colTemplate = "<div class='card-item col-lg-3 col-md-3 col-sm-12 col-xs-12'>*|CONTENT|*</div>";

            var newsTemplate = System.IO.File.ReadAllText(path + "Card.html");
            var segmentSize = (int)Math.Round(newsPileUps.Count / 4.0);

            var cardHtml = "";
            var content = "";

            for (var i = 0; i < segmentSize; i++)
            {
                var news = newsPileUps[i];
                content += string.Format(newsTemplate, news.ImageUrl,
                    news.Title, news.Summary, CommonTools.Encrypt(news.Url), sourceName);
            }
            cardHtml += !string.IsNullOrEmpty(content) ? colTemplate.Replace("*|CONTENT|*", content) : "";

            content = "";
            for (var i = segmentSize; i < segmentSize * 2 && newsPileUps.Count >= segmentSize * 2; i++)
            {
                var news = newsPileUps[i];
                content += string.Format(newsTemplate, news.ImageUrl,
                     news.Title, news.Summary, CommonTools.Encrypt(news.Url), sourceName);
            }
            cardHtml += !string.IsNullOrEmpty(content) ? colTemplate.Replace("*|CONTENT|*", content) : "";

            content = "";
            for (var i = segmentSize * 2; i < segmentSize * 3 && newsPileUps.Count >= segmentSize * 3; i++)
            {
                var news = newsPileUps[i];
                content += string.Format(newsTemplate, news.ImageUrl,
                    news.Title, news.Summary, CommonTools.Encrypt(news.Url), sourceName);
            }
            cardHtml += !string.IsNullOrEmpty(content) ? colTemplate.Replace("*|CONTENT|*", content) : "";

            content = "";
            for (var i = segmentSize * 3; i < newsPileUps.Count; i++)
            {
                var news = newsPileUps[i];
                content += string.Format(newsTemplate, news.ImageUrl,
                    news.Title, news.Summary, CommonTools.Encrypt(news.Url), sourceName);
            }
            cardHtml += !string.IsNullOrEmpty(content) ? colTemplate.Replace("*|CONTENT|*", content) : "";
            return cardHtml;
        }
    }
}