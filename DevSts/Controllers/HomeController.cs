using System.Web.Mvc;
using DevSts.Assets;

namespace DevSts.Controllers
{
    public class HomeController : Controller
    {
        public string Index()
        {
            var html = AssetManager.LoadString(DevStsConstants.IndexFile);
            return html;
        }
    }
}