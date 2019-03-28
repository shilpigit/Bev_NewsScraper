using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using OilDiversity.Web.Models;
using OilDiversity.Web.Utility;

namespace OilDiversity.Web.Repository
{
    public class NewsPileUps
    {
        private static string[] GetChartPrice()
        {
            var jsonPrice = new WebClient().DownloadString("http://oilprice.com/ajax/get_chart_prices");
            return !string.IsNullOrEmpty(jsonPrice)
                ? jsonPrice.Replace("\n", "").Replace("[", "").Replace("]", "").Split(',')
                : new[] { "" };
        }

        // latest oil price
        public static async Task<string> GetChartInOilPrice()
        {
            var http = new HttpClient();
            // awaitable
            const string BaseUrl = "http://oilprice.com";
            var response = await http.GetByteArrayAsync(BaseUrl + "/Latest-Energy-News/World-News");
            var source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
            source = WebUtility.HtmlDecode(source);
            var document = new HtmlDocument();
            document.LoadHtml(source);

            var chartHolders = (from div in document.DocumentNode.Descendants("div")
                where div.Attributes.Contains("class") && (div.Attributes["class"].Value == "chart_holder")
                select div).ToList();

            var chartPrice = GetChartPrice();

            var i = 0;
            var htmlString = "";
            foreach (var chartHolder in chartHolders)
            {
                (from a in chartHolder.Descendants("a") select a).ToList().ForEach(n => n.Remove());

                var firstPriceTag = (from p in chartHolder.Descendants("p")
                    where p.Attributes.Contains("class") && (p.Attributes["class"].Value == "chart_current_price")
                    select p).FirstOrDefault();

                var secondPriceTag = (from p in chartHolder.Descendants("p")
                    where p.Attributes.Contains("class") && (p.Attributes["class"].Value == "chart_change")
                    select p).FirstOrDefault();

                var thirdPriceTag = (from p in chartHolder.Descendants("p")
                    where p.Attributes.Contains("class") && (p.Attributes["class"].Value == "chart_change_percent")
                    select p).FirstOrDefault();

                if (firstPriceTag != null)
                {
                    firstPriceTag.InnerHtml = chartPrice[i];
                    firstPriceTag.Attributes["class"].Value =
                        chartPrice[i + 1].Contains('-') || chartPrice[i + 2].Contains('-')
                            ? "chart-current-price chart-current-price-down"
                            : "chart-current-price chart-current-price-up";
                    i++;
                }
                if (secondPriceTag != null)
                {
                    var secondPrice = chartPrice[i];
                    secondPriceTag.InnerHtml = secondPrice.Contains("-") ? secondPrice : $"+{secondPrice}";
                    secondPriceTag.Attributes["class"].Value =
                        secondPrice.Contains("-") ? "chart-price-down" : "chart-price-up";
                    i++;
                }
                if (thirdPriceTag != null)
                {
                    var thirdPrice = $"{chartPrice[i]}%";
                    thirdPriceTag.InnerHtml = thirdPrice.Contains("-") ? thirdPrice : $"+{thirdPrice}";
                    thirdPriceTag.Attributes["class"].Value = thirdPrice.Contains("-")
                        ? "chart-price-percent-down"
                        : "chart-price-percent-up";
                    i++;
                }

                chartHolder.Attributes["class"].Value = "chart-container";
                htmlString += chartHolder.OuterHtml;
            }
            return await Task.Run(() => htmlString);
        }

        // latest bloom berg
        private static async Task<List<NewsPileUp>> GetLatest4BloomBerg()
        {
            var http = new HttpClient();
            // awaitable
            const string BaseUrl = "http://www.bloomberg.com";
            var response = await http.GetByteArrayAsync(BaseUrl + "/energy");
            var source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
            source = WebUtility.HtmlDecode(source);
            var document = new HtmlDocument();
            document.LoadHtml(source);

            var articles = from article in document.DocumentNode.Descendants("article")
                where article.Attributes.Contains("class") && (article.Attributes["class"].Value == "news__story")
                select article;

            var newsList = (from htmlNode in articles
                select (from a in htmlNode.Descendants("a")
                    where a.Attributes.Contains("class") && (a.Attributes["class"].Value == "news__story__url")
                    select a).FirstOrDefault()
                into firstHeader
                let header = firstHeader != null ? firstHeader.InnerText : ""
                let url = firstHeader != null ? firstHeader.Attributes["href"].Value : ""
                select new NewsPileUp { Title = header, Url = url }).ToList();
            return await Task.Run(() => newsList);
        }

        // latest Forbes
        private static async Task<List<NewsPileUp>> GetLatest4Forbes()
        {
            var http = new HttpClient();
            // awaitable
            const string BaseUrl = "http://www.forbes.com";
            var response = await http.GetByteArrayAsync(BaseUrl);
            var source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
            source = WebUtility.HtmlDecode(source);
            var document = new HtmlDocument();
            document.LoadHtml(source);

            var firstDiv = (from div in document.DocumentNode.Descendants("div")
                where div.Attributes.Contains("id") && div.Attributes["id"].Value == "list_headline_hp"
                select div).FirstOrDefault();
            if (firstDiv == null) return await Task.Run(() => new List<NewsPileUp>());

            var lis = (from li in firstDiv.Descendants("li") select li).ToList();

            var newsList = (from htmlNode in lis
                select (from a in htmlNode.Descendants("a") select a).FirstOrDefault()
                into firstHeader
                let header = firstHeader != null ? firstHeader.InnerText : ""
                let url = firstHeader != null ? firstHeader.Attributes["href"].Value : ""
                select new NewsPileUp { Title = header, Url = url }).ToList();
            return await Task.Run(() => newsList);
        }

        // latest fuel fix
        private static async Task<List<NewsPileUp>> GetLatest4FuelFix()
        {
            var http = new HttpClient();
            // awaitable
            const string BaseUrl = "http://fuelfix.com";
            var response = await http.GetByteArrayAsync(BaseUrl);
            var source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
            source = WebUtility.HtmlDecode(source);
            var document = new HtmlDocument();
            document.LoadHtml(source);

            var firstDiv = (from div in document.DocumentNode.Descendants("div")
                where div.Attributes.Contains("class")
                      && (div.Attributes["class"].Value == "widget widget_recent_entries")
                select div).FirstOrDefault();
            if (firstDiv == null) return await Task.Run(() => new List<NewsPileUp>());

            var lis = from li in firstDiv.Descendants("li") select li;

            var newsList = (from htmlNode in lis
                select (from a in htmlNode.Descendants("a") select a).FirstOrDefault()
                into firstHeader
                let header = firstHeader != null ? firstHeader.InnerText : ""
                let url = firstHeader != null ? firstHeader.Attributes["href"].Value : ""
                select new NewsPileUp { Title = header, Url = url }).ToList();
            return await Task.Run(() => newsList);
        }

        // latest oil diversity global
        private static async Task<List<NewsPileUp>> GetLatest4OilDiversityBlog()
        {
            var http = new HttpClient();
            // awaitable
            const string BaseUrl = "http://blog.oildiversity.com";
            var response = await http.GetByteArrayAsync(BaseUrl);
            var source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
            source = WebUtility.HtmlDecode(source);
            var document = new HtmlDocument();
            document.LoadHtml(source);

            var articles = from article in document.DocumentNode.Descendants("article")
                where article.Attributes.Contains("class") && (article.Attributes["class"].Value == "post")
                select article;

            var newsList = (from htmlNode in articles
                let firstHeader = (from a in htmlNode.Descendants("a") select a).FirstOrDefault()
                let header = firstHeader != null ? firstHeader.InnerText : ""
                let url = firstHeader != null ? firstHeader.Attributes["href"].Value : ""
                let firstSummary = (from p in htmlNode.Descendants("p") select p).FirstOrDefault()
                let summary = firstSummary != null ? firstSummary.InnerText : ""
                select new NewsPileUp
                       {
                           Summary = CommonTools.TrimByWord(summary, 16),
                           Title = header,
                           Url = BaseUrl + url
                       }).ToList();
            return await Task.Run(() => newsList);
        }

        // latest oil price
        private static async Task<List<NewsPileUp>> GetLatest4OilPrice()
        {
            var http = new HttpClient();
            // awaitable
            const string BaseUrl = "http://oilprice.com";
            var response = await http.GetByteArrayAsync(BaseUrl + "/Latest-Energy-News/World-News");
            var source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
            source = WebUtility.HtmlDecode(source);
            var document = new HtmlDocument();
            document.LoadHtml(source);

            var divs = from div in document.DocumentNode.Descendants("div")
                where div.Attributes.Contains("class") && (div.Attributes["class"].Value == "categoryArticle")
                select div;

            var newsList = (from htmlNode in divs
                let firstHeader =
                (from a in htmlNode.Descendants("a")
                    where a.ParentNode.Attributes.Contains("class")
                          && a.ParentNode.Attributes["class"].Value == "categoryArticle__content"
                    select a).FirstOrDefault()
                let header = firstHeader != null ? firstHeader.InnerText : ""
                let url = firstHeader != null ? firstHeader.Attributes["href"].Value : ""
                let firstSummary =
                (from p in htmlNode.Descendants("p")
                    where p.Attributes.Contains("class") && (p.Attributes["class"].Value == "categoryArticle__excerpt")
                    select p).FirstOrDefault()
                let summary = firstSummary != null ? firstSummary.InnerText : ""
                let firstImage = (from img in htmlNode.Descendants("img") select img).FirstOrDefault()
                let image = firstImage != null ? firstImage.Attributes["src"].Value : ""
                select new NewsPileUp
                       {
                           Summary = CommonTools.TrimByWord(summary, 8),
                           Title = header,
                           Url = url,
                           ImageUrl = image
                       }).Take(8).ToList();
            return await Task.Run(() => newsList);
        }

        // latest oil voice
        private static async Task<List<NewsPileUp>> GetLatest4OilVoice()
        {
            var http = new HttpClient();
            // awaitable
            const string BaseUrl = "http://www.oilvoice.com";
            var response = await http.GetByteArrayAsync(BaseUrl);
            var source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
            source = WebUtility.HtmlDecode(source);
            var document = new HtmlDocument();
            document.LoadHtml(source);

            var contentDiv = (from div in document.DocumentNode.Descendants("div")
                where div.Attributes.Contains("class") && div.Attributes["class"].Value == "col-md-8 body-content"
                select div).FirstOrDefault();

            if (contentDiv == null) return await Task.Run(() => new List<NewsPileUp>());

            var divs = from div in contentDiv.Descendants("div")
                where div.Attributes.Contains("class") && div.Attributes["class"].Value == "panel-body"
                select div;

            var newsList = (from htmlNode in divs

                    // link a
                    let firstHeader =
                    (from a in htmlNode.Descendants("a")
                        where a.ParentNode.Attributes.Contains("class")
                              && a.ParentNode.Attributes["class"].Value == "articleheadline"
                        select a).FirstOrDefault()
                    let header = firstHeader != null ? firstHeader.InnerText : ""
                    let url = firstHeader != null ? firstHeader.Attributes["href"].Value : ""

                    // summary
                    let firstSummary = (from small in htmlNode.Descendants("small") select small).FirstOrDefault()
                    let summary = firstSummary != null ? firstSummary.InnerText : ""
                    select new NewsPileUp { Summary = CommonTools.TrimByWord(summary, 16), Title = header, Url = url })
                .ToList();
            return await Task.Run(() => newsList);
        }

        // latest SPE
        private static async Task<List<NewsPileUp>> GetLatest4Spe()
        {
            var http = new HttpClient();
            // awaitable
            const string BaseUrl = "http://www.spe.org";
            var response = await http.GetByteArrayAsync(BaseUrl);
            var source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
            source = WebUtility.HtmlDecode(source);
            var document = new HtmlDocument();
            document.LoadHtml(source);

            var articles = from article in document.DocumentNode.Descendants("article")
                where article.Attributes.Contains("class") && (article.Attributes["class"].Value == "tile white story")
                select article;

            var newsList = (from htmlNode in articles
                    let firstHeader = (from a in htmlNode.Descendants("h3") select a).FirstOrDefault()
                    let header = firstHeader != null ? firstHeader.InnerText : ""
                    let firstUrl = (from a in htmlNode.Descendants("a") select a).FirstOrDefault()
                    let url = firstUrl != null ? firstUrl.Attributes["href"].Value : ""
                    let firstSummary = (from p in htmlNode.Descendants("p") select p).FirstOrDefault()
                    let summary = firstSummary != null ? firstSummary.InnerText : ""
                    select new NewsPileUp { Summary = CommonTools.TrimByWord(summary, 20), Title = header, Url = url })
                .ToList();
            return await Task.Run(() => newsList);
        }

        // latest shalemarket
        private static async Task<List<NewsPileUp>> GetLatest4Shalemarket()
        {
            var http = new HttpClient();
            // awaitable
            const string BaseUrl = "http://www.shalemarket.com";
            var response = await http.GetByteArrayAsync(BaseUrl + "/?reqp=1&reqr=");
            var source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
            source = WebUtility.HtmlDecode(source);
            var document = new HtmlDocument();
            document.LoadHtml(source);

            var firstDiv = (from div in document.DocumentNode.Descendants("div")
                where div.Attributes.Contains("id") && div.Attributes["id"].Value == "adBlock"
                select div).FirstOrDefault();
            if (firstDiv == null) return await Task.Run(() => new List<NewsPileUp>());

            var divs = (from div in firstDiv.Descendants("div")
                where div.Attributes.Contains("class") && div.Attributes["class"].Value.Contains("ad ")
                select div).ToList();

            var newsList = (from htmlNode in divs
                    let firstHeader = (from a in htmlNode.Descendants("a") select a).FirstOrDefault()
                    let header = firstHeader != null ? firstHeader.InnerText : ""
                    let url = firstHeader != null ? firstHeader.Attributes["href"].Value : ""
                    let firstSummary =
                    (from span in htmlNode.Descendants("span")
                        where span.Attributes.Contains("class") && span.Attributes["class"].Value == "descText"
                        select span).FirstOrDefault()
                    let summary = firstSummary != null ? firstSummary.InnerText : ""
                    select new NewsPileUp { Summary = CommonTools.TrimByWord(summary, 16), Title = header, Url = url })
                .ToList();
            return await Task.Run(() => newsList);
        }

        // latest shalemarket
        private static async Task<List<NewsPileUp>> GetLatest4OilAndGasTechnology()
        {
            var http = new HttpClient();
            // awaitable
            const string BaseUrl = "http://www.oilandgastechnology.net";
            var response = await http.GetByteArrayAsync(BaseUrl + "/news");
            var source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
            source = WebUtility.HtmlDecode(source);
            var document = new HtmlDocument();
            document.LoadHtml(source);

            var firstTable = (from table in document.DocumentNode.Descendants("table")
                where table.Attributes.Contains("id") && table.Attributes["id"].Value == "this-is-from-template"
                select table).FirstOrDefault();
            if (firstTable == null) return await Task.Run(() => new List<NewsPileUp>());

            var trs = (from tr in firstTable.Descendants("tr") select tr).ToList();

            var newsList = (from htmlNode in trs
                let firstHeader =
                (from a in htmlNode.Descendants("a")
                    where a.ParentNode.Attributes.Contains("class")
                          && a.ParentNode.Attributes["class"].Value == "in-top-title"
                    select a).FirstOrDefault()
                let header = firstHeader != null ? firstHeader.InnerText : ""
                let url = firstHeader != null ? firstHeader.Attributes["href"].Value : ""
                let firstSummary =
                (from td in htmlNode.Descendants("td")
                    where td.Attributes.Contains("class")
                          && td.Attributes["class"].Value == "views-field views-field-nothing"
                    select td).FirstOrDefault()
                let summary = firstSummary != null ? firstSummary.InnerText : ""
                let firstImage = (from img in htmlNode.Descendants("img") select img).FirstOrDefault()
                let image = firstImage != null ? firstImage.Attributes["src"].Value : ""
                select new NewsPileUp
                       {
                           Summary = CommonTools.TrimByWord(summary, 35),
                           Title = header,
                           Url = BaseUrl + url,
                           ImageUrl = image
                       }).ToList();
            return await Task.Run(() => newsList);
        }


        // latest energy voice
        private static async Task<List<NewsPileUp>> GetLatest4EnergyVoice()
        {
            var http = new HttpClient();
            // awaitable
            const string BaseUrl = "http://www.energyvoice.com";
            var response = await http.GetByteArrayAsync(BaseUrl + "/category/oilandgas");
            var source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
            source = WebUtility.HtmlDecode(source);
            var document = new HtmlDocument();
            document.LoadHtml(source);

            var firstDivs = from div in document.DocumentNode.Descendants("div")
                where div.ParentNode.ParentNode.Attributes.Contains("id")
                      && div.ParentNode.ParentNode.Attributes["id"].Value == "main-content"
                select div;

            var newsList = (from htmlNode in firstDivs
                let firstHeader =
                (from a in htmlNode.Descendants("a") where !a.Attributes.Contains("class") select a).FirstOrDefault()
                let header = firstHeader != null ? firstHeader.InnerText : ""
                let url = firstHeader != null ? firstHeader.Attributes["href"].Value : ""
                let image = htmlNode.Attributes["style"].Value
                select new NewsPileUp
                       {
                           Summary = "",
                           Title = header,
                           Url = url,
                           ImageUrl = image.Replace("background-image: url(", "").Replace(");", "")
                       }).ToList();
            return await Task.Run(() => newsList);
        }

        // detail bloom berg
        private static async Task<NewsPileUp> GetDetail4BloomBerg(
            string url)
        {
            var http = new HttpClient();
            // awaitable
            var response = await http.GetByteArrayAsync(url);
            var source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
            source = WebUtility.HtmlDecode(source);
            var document = new HtmlDocument();
            document.LoadHtml(source);

            // title
            var title = (from span in document.DocumentNode.Descendants("span")
                where span.Attributes.Contains("class")
                      && span.Attributes["class"].Value == "lede-headline__highlighted"
                select span).FirstOrDefault();
            if (title == null) return new NewsPileUp();

            // content
            var firstDiv = (from div in document.DocumentNode.Descendants("div")
                where div.Attributes.Contains("class") && div.Attributes["class"].Value == "article-body__content"
                select div).FirstOrDefault();

            if (firstDiv == null) return new NewsPileUp();

            // image
            var image = "";
            var firstImage = (from img in document.DocumentNode.Descendants("img")
                where img.ParentNode.Attributes.Contains("class") && img.ParentNode.Attributes["class"].Value
                      == "inline-media__unlinked-image"
                select img).FirstOrDefault();

            if (firstImage == null)
            {
                image = "";
                firstImage = (from img in firstDiv.Descendants("img") select img).FirstOrDefault();
            }

            if (firstImage != null)
            {
                image = firstImage.Attributes["src"].Value;
            }

            (from div in firstDiv.Descendants("div") select div).ToList().ForEach(n => n.Remove());
            (from div in firstDiv.Descendants("img") select div).ToList().ForEach(n => n.Remove());

            var content = CommonTools.RemoveUselessTags(firstDiv.InnerHtml);
            content = CommonTools.RemoveH2Tags(content);
            content = content.Replace("<a ", "<a target='_blank' ");
            return new NewsPileUp { Title = title.InnerText, Description = content, ImageUrl = image, Url = url };
        }

        // detail Forbes
        private static async Task<NewsPileUp> GetDetail4Forbes(
            string url)
        {
            var http = new HttpClient();
            // awaitable
            var response = await http.GetByteArrayAsync(url);
            var source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
            source = WebUtility.HtmlDecode(source);
            var document = new HtmlDocument();
            document.LoadHtml(source);

            // title
            var title = (from h1 in document.DocumentNode.Descendants("h1")
                where h1.Attributes.Contains("class") && h1.Attributes["class"].Value == "article-headline ng-binding"
                select h1).FirstOrDefault();
            if (title == null) return new NewsPileUp();

            // content
            var firstDiv = (from div in document.DocumentNode.Descendants("div")
                where div.Attributes.Contains("class") && div.Attributes["class"].Value
                      == "article-injected-body ng-scope"
                select div).FirstOrDefault();

            if (firstDiv == null) return new NewsPileUp();

            // image
            var image = "";
            var firstImage = (from img in firstDiv.Descendants("img") select img).FirstOrDefault();

            if (firstImage != null)
            {
                image = firstImage.Attributes["src"].Value;
            }
            (from div in firstDiv.Descendants("div") select div).ToList().ForEach(n => n.Remove());
            (from div in firstDiv.Descendants("img") select div).ToList().ForEach(n => n.Remove());

            var content = CommonTools.RemoveUselessTags(firstDiv.InnerHtml);
            content = CommonTools.RemoveH2Tags(content);
            content = content.Replace("<a ", "<a target='_blank' ");
            return new NewsPileUp { Title = title.InnerText, Description = content, ImageUrl = image, Url = url };
        }

        // detail fuel fix
        private static async Task<NewsPileUp> GetDetail4FuelFix(
            string url)
        {
            var http = new HttpClient();
            // awaitable
            var response = await http.GetByteArrayAsync(url);
            var source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
            source = WebUtility.HtmlDecode(source);
            var document = new HtmlDocument();
            document.LoadHtml(source);

            // title
            var title = (from h1 in document.DocumentNode.Descendants("h1")
                where h1.Attributes.Contains("class") && h1.Attributes["class"].Value == "entry-title"
                select h1).FirstOrDefault();
            if (title == null) return new NewsPileUp();

            // content
            var firstDiv = (from div in document.DocumentNode.Descendants("div")
                where div.Attributes.Contains("class") && div.Attributes["class"].Value == "entry-content clearfix aaaa"
                select div).FirstOrDefault();

            if (firstDiv == null) return new NewsPileUp();

            // image
            var image = "";
            var firstImage = (from img in firstDiv.Descendants("img") select img).FirstOrDefault();

            if (firstImage != null)
            {
                image = firstImage.Attributes["src"].Value;
            }
            (from div in firstDiv.Descendants("div")
                where div.Attributes.Contains("class") && div.Attributes["class"].Value != "headerbar"
                      && div.Attributes["class"].Value != "right"
                select div).ToList().ForEach(n => n.Remove());
            (from img in firstDiv.Descendants("img") select img).ToList().ForEach(n => n.Remove());

            var content = CommonTools.RemoveUselessTags(firstDiv.InnerHtml);
            content = content.Replace("<a ", "<a target='_blank' ");
            return new NewsPileUp { Title = title.InnerText, Description = content, ImageUrl = image, Url = url };
        }

        // detail oil diversity global
        private static async Task<NewsPileUp> GetDetail4OilDiversityBlog(
            string url)
        {
            var http = new HttpClient();
            // awaitable
            var response = await http.GetByteArrayAsync(url);
            var source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
            source = WebUtility.HtmlDecode(source);
            var document = new HtmlDocument();
            document.LoadHtml(source);

            var firstArticle = (from article in document.DocumentNode.Descendants("article")
                let firstA = (from a in article.Descendants("a") select a).FirstOrDefault()
                let href = firstA != null ? firstA.Attributes["href"].Value : ""
                where article.Attributes.Contains("class") && (article.Attributes["class"].Value == "post")
                      && url.Contains(href)
                select article).FirstOrDefault();
            if (firstArticle == null) return new NewsPileUp();
            // title
            var title = (from a in firstArticle.Descendants("a") select a).FirstOrDefault();
            if (title == null) return new NewsPileUp();

            // content
            var firstSection = (from section in firstArticle.Descendants("section")
                where section.Attributes.Contains("class") && section.Attributes["class"].Value == "post-body text"
                select section).FirstOrDefault();

            if (firstSection == null) return new NewsPileUp();

            // image
            var image = "";
            var firstImage = (from img in firstSection.Descendants("img") select img).FirstOrDefault();

            if (firstImage != null)
            {
                image = firstImage.Attributes["src"].Value;
            }
            (from div in firstSection.Descendants("div") select div).ToList().ForEach(n => n.Remove());
            (from div in firstSection.Descendants("img") select div).ToList().ForEach(n => n.Remove());

            var content = CommonTools.RemoveUselessTags(firstSection.InnerHtml);
            content = CommonTools.RemoveH2Tags(content);
            content = content.Replace("<a ", "<a target='_blank' ");
            return new NewsPileUp { Title = title.InnerText, Description = content, ImageUrl = image, Url = url };
        }

        // detail bloom berg
        private static async Task<NewsPileUp> GetDetail4OilPrice(
            string url)
        {
            var http = new HttpClient();
            // awaitable
            var response = await http.GetByteArrayAsync(url);
            var source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
            source = WebUtility.HtmlDecode(source);
            var document = new HtmlDocument();
            document.LoadHtml(source);

            var holderDiv = (from div in document.DocumentNode.Descendants("div")
                where div.Attributes.Contains("class") && div.Attributes["class"].Value == "singleArticle__content"
                select div).FirstOrDefault();
            if (holderDiv == null) return new NewsPileUp();

            // title
            var title = (from h1 in holderDiv.Descendants("h1") select h1).FirstOrDefault();
            if (title == null) return new NewsPileUp();

            // content
            var firstContent = (from div in document.DocumentNode.Descendants("div")
                where div.Attributes.Contains("class") && div.Attributes["class"].Value == "wysiwyg clear"
                select div).FirstOrDefault();

            if (firstContent == null) return new NewsPileUp();

            // image
            var image = "";
            var firstImage = (from img in holderDiv.Descendants("img") select img).FirstOrDefault();

            if (firstImage != null)
            {
                image = firstImage.Attributes["src"].Value;
            }

            (from div in firstContent.Descendants("div") select div).ToList().ForEach(n => n.Remove());
            (from img in firstContent.Descendants("img") select img).ToList().ForEach(n => n.Remove());
            (from p in firstContent.Descendants("p")
                where p.InnerText.Contains("More Top Reads From Oilprice.com:")
                select p).ToList().ForEach(n => n.Remove());

            var content = CommonTools.RemoveUselessTags(firstContent.InnerHtml);
            content = CommonTools.RemoveUl(content);
            content = content.Replace("<a ", "<a target='_blank' ").Replace("<br><br>", "<br>")
                .Replace("style=\"color: #800000;\"", "");
            return new NewsPileUp { Title = title.InnerText, Description = content, ImageUrl = image, Url = url };
        }

        // detail oil voice
        private static async Task<NewsPileUp> GetDetail4OilVoice(
            string url)
        {
            var http = new HttpClient();

            // awaitable
            var response = await http.GetByteArrayAsync(url);
            var source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
            source = WebUtility.HtmlDecode(source);
            var document = new HtmlDocument();
            document.LoadHtml(source);

            // content
            var contentDiv = (from div in document.DocumentNode.Descendants("div")
                where div.Attributes.Contains("class") && div.Attributes["class"].Value == "col-md-8 body-content"
                select div).FirstOrDefault();

            if (contentDiv == null) return new NewsPileUp();

            // title
            var title = (from h2 in contentDiv.Descendants("h2")
                where h2.Attributes.Contains("class") && h2.Attributes["class"].Value == "newsheadline"
                select h2).FirstOrDefault();

            if (title == null) return new NewsPileUp();

            // content
            var text = (from div in contentDiv.Descendants("div")
                where div.Attributes.Contains("class") && div.Attributes["class"].Value == "ovbodyheight"
                select div).FirstOrDefault();

            if (text == null) return new NewsPileUp();

            // image
            var image = "";
            var firstImage = (from img in text.Descendants("img") select img).FirstOrDefault();

            if (firstImage != null)
            {
                image = firstImage.Attributes["src"].Value;
            }

            (from div in text.Descendants("div") select div).ToList().ForEach(n => n.Remove());
            (from div in text.Descendants("img") select div).ToList().ForEach(n => n.Remove());

            var content = CommonTools.RemoveUselessTags(text.InnerHtml);
            content = CommonTools.RemoveH2Tags(content);
            content = content.Replace("<a ", "<a target='_blank' ").Replace("<br><br>", "<br>");
            return new NewsPileUp { Title = title.InnerText, Description = content, ImageUrl = image, Url = url };
        }

        // detail SPE
        private static async Task<NewsPileUp> GetDetail4Spe(
            string url)
        {
            var http = new HttpClient();
            const string BaseUrl = "http://www.spe.org";
            // awaitable
            var response = await http.GetByteArrayAsync(url);
            var source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
            source = WebUtility.HtmlDecode(source);
            var document = new HtmlDocument();
            document.LoadHtml(source);

            // title
            var title = (from h1 in document.DocumentNode.Descendants("h1")
                where h1.ParentNode.Attributes.Contains("id") && h1.ParentNode.Attributes["id"].Value == "article-info"
                select h1).FirstOrDefault();
            if (title == null) return new NewsPileUp();

            // content
            var firstDiv = (from div in document.DocumentNode.Descendants("div")
                where div.Attributes.Contains("class") && div.Attributes["class"].Value == "story-text"
                select div).FirstOrDefault();

            if (firstDiv == null) return new NewsPileUp();

            // image
            var imageWrapper = (from div in document.DocumentNode.Descendants("div")
                where div.Attributes.Contains("class") && div.Attributes["class"].Value == "img-wrap"
                select div).FirstOrDefault();

            if (imageWrapper == null) return new NewsPileUp();

            string image;
            var firstImage = (from img in imageWrapper.Descendants("img") select img).FirstOrDefault();
            if (firstImage != null)
            {
                image = firstImage.Attributes["src"].Value;
            }
            else
            {
                var alternateImage = (from div in firstDiv.Descendants("div")
                    where div.Attributes.Contains("class") && div.Attributes["class"].Value == "js-delayed-image-load"
                    select div).FirstOrDefault();
                if (alternateImage != null)
                {
                    image = alternateImage.Attributes["data-src"] != null
                        ? alternateImage.Attributes["data-src"].Value
                        : "";
                }
                else
                {
                    image = "";
                }
            }
            var content = CommonTools.RemoveUselessTags(firstDiv.InnerHtml);
            content = content.Replace("<a ", "<a target='_blank' ");
            return new NewsPileUp
                   {
                       Title = title.InnerText,
                       Description = content,
                       ImageUrl = BaseUrl + image,
                       Url = url
                   };
        }

        // detail oil and gas technology
        private static async Task<NewsPileUp> GetDetail4OilAndGasTechnology(
            string url)
        {
            var http = new HttpClient();
            // awaitable
            var response = await http.GetByteArrayAsync(url);
            var source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
            source = WebUtility.HtmlDecode(source);
            var document = new HtmlDocument();
            document.LoadHtml(source);

            // title
            var title = (from h1 in document.DocumentNode.Descendants("h1")
                where h1.Attributes.Contains("class") && h1.Attributes["class"].Value == "title"
                select h1).FirstOrDefault();
            if (title == null) return new NewsPileUp();

            // content
            var firstDiv = (from div in document.DocumentNode.Descendants("div")
                where div.Attributes.Contains("id") && div.Attributes["id"].Value == "block-system-main"
                select div).FirstOrDefault();

            if (firstDiv == null) return new NewsPileUp();

            var contentDiv = (from div in firstDiv.Descendants("div")
                where div.Attributes.Contains("class")
                      && (div.Attributes["class"].Value
                          == "field field-name-field-teaser field-type-text-long field-label-hidden "
                          || div.Attributes["class"].Value
                          != "field field-name-body field-type-text-with-summary field-label-hidden")
                select div).FirstOrDefault();
            if (contentDiv == null) return new NewsPileUp();

            // image
            var image = "";
            var firstImage = (from img in firstDiv.Descendants("img") select img).FirstOrDefault();
            if (firstImage != null)
            {
                image = firstImage.Attributes["src"].Value;
            }

            (from div in contentDiv.Descendants("div")
                where div.Attributes.Contains("class") && (div.Attributes["class"].Value == "created"
                                                           || div.Attributes["class"].Value
                                                           == "easy_social_box clearfix vertical easy_social_lang_en"
                                                           || div.Attributes["class"].Value
                                                           == "field field-name-field-image field-type-image field-label-hidden"
                                                           || div.Attributes["class"].Value
                                                           == "field field-name-taxonomy-vocabulary-4 field-type-taxonomy-term-reference field-label-inline clearfix"
                                                           || div.Attributes["class"].Value == "terms"
                                                           || div.Attributes["class"].Value == "links"
                                                           || div.Attributes["class"].Value == "clear clearfix")
                select div).ToList().ForEach(n => n.Remove());

            (from span in contentDiv.Descendants("span")
                where span.Attributes.Contains("class") && span.Attributes["class"].Value == "submitted"
                select span).ToList().ForEach(n => n.Remove());
            (from div in firstDiv.Descendants("img") select div).ToList().ForEach(n => n.Remove());

            var content = CommonTools.RemoveUselessTags(contentDiv.InnerHtml);
            content = CommonTools.RemoveH2Tags(content);
            content = content.Replace("<a ", "<a target='_blank' ");
            return new NewsPileUp { Title = title.InnerText, Description = content, ImageUrl = image, Url = url };
        }

        // detail energy voice
        private static async Task<NewsPileUp> GetDetail4EnergyVoice(
            string url)
        {
            var http = new HttpClient();
            // awaitable
            var response = await http.GetByteArrayAsync(url);
            var source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
            source = WebUtility.HtmlDecode(source);
            var document = new HtmlDocument();
            document.LoadHtml(source);

            // title
            var title = (from h1 in document.DocumentNode.Descendants("h1")
                where h1.Attributes.Contains("itemprop") && h1.Attributes["itemprop"].Value == "headline"
                select h1).FirstOrDefault();
            if (title == null) return new NewsPileUp();

            // content            
            var firstDiv = (from div in document.DocumentNode.Descendants("div")
                where div.Attributes.Contains("class") && div.Attributes["class"].Value == "col-md-11 article-body"
                select div).FirstOrDefault();

            if (firstDiv == null) return new NewsPileUp();

            var contentDiv = (from div in firstDiv.Descendants("div")
                where div.Attributes.Contains("class") && div.Attributes["class"].Value.Contains("article-body-inner")
                      || div.Attributes.Contains("itemprop") && div.Attributes["itemprop"].Value == "articleBody"
                select div).FirstOrDefault();
            if (contentDiv == null) return new NewsPileUp();

            // image
            var image = "";
            var firstImage = (from div in firstDiv.Descendants("div")
                where div.Attributes.Contains("style") && div.Attributes["style"].Value.Contains("background-image")
                select div).FirstOrDefault();
            if (firstImage != null)
            {
                image = firstImage.Attributes["style"].Value.Replace("background-image: url(", "").Replace(");", "");
            }

            (from div in contentDiv.Descendants("div") select div).ToList().ForEach(n => n.Remove());
            (from div in firstDiv.Descendants("img") select div).ToList().ForEach(n => n.Remove());

            var content = CommonTools.RemoveUselessTags(contentDiv.InnerHtml);
            content = CommonTools.RemoveH2Tags(content);
            content = content.Replace("<a ", "<a target='_blank' ");
            return new NewsPileUp { Title = title.InnerText, Description = content, ImageUrl = image, Url = url };
        }

        //latest
        public static async Task<List<NewsPileUp>> GetLatestBySourceName(
            NewsPileUp.RssSource sourceName)
        {
            switch (sourceName)
            {
                case NewsPileUp.RssSource.BloomBerg: return Task.Run(GetLatest4BloomBerg).Result;

                case NewsPileUp.RssSource.EnergyVoice: return Task.Run(GetLatest4EnergyVoice).Result;

                case NewsPileUp.RssSource.ForBes: return Task.Run(GetLatest4Forbes).Result;

                case NewsPileUp.RssSource.FuelFix: return Task.Run(GetLatest4FuelFix).Result;

                case NewsPileUp.RssSource.OffShoreMag:
                    //baseUrl = "http://www.offshore_mag.com";
                    break;

                case NewsPileUp.RssSource.OilAndGasTechnology: return Task.Run(GetLatest4OilAndGasTechnology).Result;

                case NewsPileUp.RssSource.OilDiversityBlog: return Task.Run(GetLatest4OilDiversityBlog).Result;

                case NewsPileUp.RssSource.OilPrice: return Task.Run(GetLatest4OilPrice).Result;

                case NewsPileUp.RssSource.OilVoice: return Task.Run(GetLatest4OilVoice).Result;

                case NewsPileUp.RssSource.Reuters:
                    //baseUrl = "http://blogs.reuters.com";
                    break;
                case NewsPileUp.RssSource.ShaleMarket: return Task.Run(GetLatest4Shalemarket).Result;

                case NewsPileUp.RssSource.Spe: return Task.Run(GetLatest4Spe).Result;

                default: return Task.Run(GetLatest4OilDiversityBlog).Result;
            }
            return await Task.Run(() => new List<NewsPileUp>());
        }

        // details
        public static async Task<NewsPileUp> GetDetailBySourceName(
            string url,
            string sourceName)
        {
            switch (sourceName)
            {
                case "BloomBerg": return await GetDetail4BloomBerg(url);

                case "EnergyVoice": return await GetDetail4EnergyVoice(url);

                case "ForBes": return await GetDetail4Forbes(url);

                case "FuelFix": return await GetDetail4FuelFix(url);

                case "OffShoreMag":
                    //baseUrl = "http://www.offshore_mag.com";
                    break;

                case "OilAndGasTechnology": return await GetDetail4OilAndGasTechnology(url);

                case "OilDiversityBlog": return await GetDetail4OilDiversityBlog(url);

                case "OilPrice": return await GetDetail4OilPrice(url);

                case "OilVoice": return await GetDetail4OilVoice(url);

                case "Reuters":
                    //baseUrl = "http://blogs.reuters.com";
                    break;
                case "ShaleMarket":
                    //baseUrl = "http://www.shalemarket.com";
                    break;

                case "Spe": return await Task.Run(() => GetDetail4Spe(url));

                default: return await Task.Run(() => new NewsPileUp());
            }
            return await Task.Run(() => new NewsPileUp());
        }
    }
}