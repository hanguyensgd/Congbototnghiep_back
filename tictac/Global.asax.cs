using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Newtonsoft.Json;
using tictac.Helper;

namespace tictac
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // API authorization registration.    
            GlobalConfiguration.Configuration.MessageHandlers.Add(new AuthorizationHeaderHandler());


            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings =
    new JsonSerializerSettings
    {
        DateFormatHandling = DateFormatHandling.IsoDateFormat,
        DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
        Culture = CultureInfo.GetCultureInfo("fr-FR")
    };
        }
    }
}
