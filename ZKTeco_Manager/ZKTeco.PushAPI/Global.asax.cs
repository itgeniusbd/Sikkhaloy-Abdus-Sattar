using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace ZKTeco.PushAPI
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            // Configure Web API routes
            GlobalConfiguration.Configure(WebApiConfig.Register);
            
            // Ensure routes are mapped
            RouteTable.Routes.RouteExistingFiles = false;
        }
    }
}
