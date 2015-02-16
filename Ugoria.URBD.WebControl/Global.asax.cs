using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Ugoria.URBD.WebControl.Helpers;
using System.IO;
using System.Reflection;
using Microsoft.AspNet.SignalR;
using System.ServiceModel;
using Ugoria.URBD.Contracts.Service;
using Microsoft.AspNet.SignalR.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Ugoria.URBD.Contracts.Data.Reports;
using Ugoria.URBD.Contracts.Services;
using Ugoria.URBD.WebControl.SignalR;
using Ugoria.URBD.Contracts.Data.Commands;

namespace Ugoria.URBD.WebControl
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class WebControl : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorLoggerAttribute() { ExceptionType = typeof(AccessPermissionDeniedException), View = "AccessDenied", Order = 1 });
            filters.Add(new HandleErrorLoggerAttribute());
            //filters.Add(new SecurityAccessAttribute());
            //filters.Add()
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("signalr*");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Main", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            // SignalR and WCF->SignalR routing configuration
            var serializerSettings = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
                NullValueHandling = NullValueHandling.Ignore,
            };
            serializerSettings.Converters.Add(new IsoDateTimeConverter() { DateTimeFormat = "dd.MM.yyyy HH:mm:ss" });
            var serializer = new JsonNetSerializer(serializerSettings);

            GlobalHost.DependencyResolver.Register(typeof(IJsonSerializer), () => serializer);
            RouteTable.Routes.MapHubs();
            IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext("TransferHub");

            WebService webService = new WebService();
            CentralServiceDataRouter router = new CentralServiceDataRouter(hubContext, webService);
            router.Add(typeof(ExchangeCommand), typeof(ExchangeReport), new ExchangeHandler());
            router.Add(typeof(ExtDirectoriesCommand), typeof(ExtDirectoriesReport), new ExtDirectoriesHandler());
            router.Add(typeof(MlgCollectCommand), typeof(MlgCollectReport), new MlgCollectHandler());

            ServiceHost controlHost = new ServiceHost(webService);
            controlHost.AddServiceEndpoint(typeof(IWebService),
                    new NetTcpBinding(SecurityMode.None),
                    new Uri("net.tcp://localhost:9999/URBDWebService"));
            controlHost.Open();
            // end configuration

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            ModelBinders.Binders.Add(typeof(DateTime), new DateTimeBinder());
            ModelBinders.Binders.Add(typeof(DateTime?), new DateTimeBinder());

            List<string> skins = new List<string>();

            foreach (DirectoryInfo skinDir in new DirectoryInfo(HttpContext.Current.Server.MapPath("~/Content/themes")).GetDirectories())
            {
                skins.Add(skinDir.Name);
            }
            Application["skins"] = skins;
            Application["version"] = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}