using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace DevSts
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var x = Server.GetLastError();
        }
    }
}
