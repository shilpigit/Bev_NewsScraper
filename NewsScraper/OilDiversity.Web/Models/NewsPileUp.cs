namespace OilDiversity.Web.Models
{
    public class NewsPileUp
    {
        public enum RssSource
        {
            OilDiversityBlog,
            BloomBerg,
            FuelFix,
            Spe,
            ForBes,
            ShaleMarket,
            OilAndGasTechnology,
            OffShoreMag,
            Reuters,
            EnergyVoice,
            OilVoice,
            OilPrice
        }

        public string Title { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }

        public static string GetSourceName(
            string source)
        {
            const string baseTitle = "Courtesy of ";
            switch (source)
            {
                case "BloomBerg":
                    return baseTitle + "Bloomberg";

                case "EnergyVoice":
                    return baseTitle + "Energy Voice";

                case "ForBes":
                    return baseTitle + "Forbes";

                case "FuelFix":
                    return baseTitle + "Fuel Fix";

                case "OffShoreMag":
                    break;

                case "OilAndGasTechnology":
                    return baseTitle + "Oil and Gas Technology News";

                case "OilDiversityBlog":
                    return "Oil Diversity Blog";

                case "OilPrice":
                    return baseTitle + "Oil Price";

                case "OilVoice":
                    return baseTitle + "Oil Voice";

                case "Reuters":
                    break;

                case "ShaleMarket":
                    return baseTitle + "Shalemarket";

                case "Spe":
                    return baseTitle + "Society of Petroleum Engineers";

                default:
                    return "";
            }
            return "";
        }

        public static string GetSourceUrl(
            string source)
        {
            switch (source)
            {
                case "BloomBerg":
                    return "http://www.bloomberg.com/energy";

                case "EnergyVoice":
                    return "http://www.energyvoice.com";

                case "ForBes":
                    return "http://www.forbes.com";

                case "FuelFix":
                    return "http://fuelfix.com";

                case "OffShoreMag":
                    break;

                case "OilAndGasTechnology":
                    return "http://www.oilandgastechnology.net/news";

                case "OilDiversityBlog":
                    return "http://blog.oildiversity.com";

                case "OilPrice":
                    return "http://oilprice.com/Latest-Energy-News/World-News";

                case "OilVoice":
                    return "http://www.oilvoice.com";

                case "Reuters":
                    break;

                case "ShaleMarket":
                    return "http://www.shalemarket.com";

                case "Spe":
                    return "http://www.spe.org";

                default:
                    return "";
            }
            return "";
        }
    }
}