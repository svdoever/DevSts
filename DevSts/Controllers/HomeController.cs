using System.Web.Mvc;
using DevSts.Assets;
using NLog;

namespace DevSts.Controllers
{
    public class HomeController : Controller
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public string Index()
        {
            logger.Trace("Sample trace message");
            logger.Debug("Sample debug message");
            logger.Info("Sample informational message");
            logger.Warn("Sample warning message");
            logger.Error("Sample error message");
            logger.Fatal("Sample fatal error message");
            var html = AssetManager.LoadString(DevStsConstants.IndexFile);
            return html;
        }
    }
}