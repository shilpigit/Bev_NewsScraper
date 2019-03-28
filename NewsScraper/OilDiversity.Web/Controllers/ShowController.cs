using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using OilDiversity.Web.Repository;
using OilDiversity.Web.Utility;

namespace OilDiversity.Web.Controllers
{
    public class ShowController : Controller
    {
        [AllowAnonymous]
        [Route("/news/show")]
        public async Task<ActionResult> Index()
        {
            var sourceName = Request.QueryString["s"];
            var url = CommonTools.Decrypt(Request.QueryString["u"]);
            TempData["SourceName"] = sourceName;

            try
            {
                return View(await NewsPileUps.GetDetailBySourceName(url, sourceName));
            }
            catch (Exception ex)
            {
                TempData["NotFound"] = "Sorry! News detail is not available anymore!";
                return View();
            }
        }
    }
}