using System.Web.Http;

namespace ZKTeco.PushAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Enable attribute routing
            config.MapHttpAttributeRoutes();

            // ZKTeco iclock routes
            
            // Route for: /PushAPI/iclock/ping
            config.Routes.MapHttpRoute(
                name: "PushApiIclockPing",
                routeTemplate: "PushAPI/iclock/ping",
                defaults: new { controller = "Iclock", action = "Ping" }
            );

            // Route for: /PushAPI/iclock/cdata (GET and POST)
            config.Routes.MapHttpRoute(
                name: "PushApiIclockCdata",
                routeTemplate: "PushAPI/iclock/cdata",
                defaults: new { controller = "Iclock", action = "GetDeviceInfo" }
            );

            // Route for: /PushAPI/iclock/getrequest
            config.Routes.MapHttpRoute(
                name: "PushApiIclockGetRequest",
                routeTemplate: "PushAPI/iclock/getrequest",
                defaults: new { controller = "Iclock", action = "GetRequest" }
            );

            // Route for: /PushAPI/iclock/test-adms
            config.Routes.MapHttpRoute(
                name: "PushApiIclockTestAdms",
                routeTemplate: "PushAPI/iclock/test-adms",
                defaults: new { controller = "Iclock", action = "TestAdms" }
            );

            // Generic route for other iclock actions
            config.Routes.MapHttpRoute(
                name: "PushApiIclock",
                routeTemplate: "PushAPI/iclock/{action}",
                defaults: new { controller = "Iclock" }
            );

            // Direct iclock routes (without PushAPI prefix)
            config.Routes.MapHttpRoute(
                name: "IclockPing",
                routeTemplate: "iclock/ping",
                defaults: new { controller = "Iclock", action = "Ping" }
            );

            config.Routes.MapHttpRoute(
                name: "IclockCdata",
                routeTemplate: "iclock/cdata",
                defaults: new { controller = "Iclock", action = "GetDeviceInfo" }
            );

            config.Routes.MapHttpRoute(
                name: "IclockGetRequest",
                routeTemplate: "iclock/getrequest",
                defaults: new { controller = "Iclock", action = "GetRequest" }
            );

            config.Routes.MapHttpRoute(
                name: "IclockTestAdms",
                routeTemplate: "iclock/test-adms",
                defaults: new { controller = "Iclock", action = "TestAdms" }
            );

            config.Routes.MapHttpRoute(
                name: "IclockApi",
                routeTemplate: "iclock/{action}",
                defaults: new { controller = "Iclock" }
            );

            // Default Web API route
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
