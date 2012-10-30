using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ugoria.URBD.WebControl.Models;
using OpenFlashChart;
using System.Drawing;
using System.ServiceModel;
using Ugoria.URBD.WebControl.ViewModels;
using Ugoria.URBD.Contracts.Services;

namespace Ugoria.URBD.WebControl.Controllers
{
    public class ServiceController : Controller
    {
        private ChannelFactory<IControlService> channelFactory = new ChannelFactory<IControlService>(new NetTcpBinding(SecurityMode.None), new EndpointAddress("net.tcp://localhost:8888/URBDControl"));

        public ActionResult Index()
        {
            return RedirectToAction("Index", "Main", new { sort = TableGrouper.service });
        }

        public ActionResult Edit()
        {
            URBD2Entities dataContext = new URBD2Entities();
            IServiceRepository serviceRepo = new ServiceRepository(dataContext);

            int serviceId = int.Parse(RouteData.Values["id"].ToString());
            IService service = serviceRepo.GetServiceById(serviceId);
            if (service == null)
                return RedirectToAction("Index", "Main");
            ViewData["service_id"] = service.ServiceId;
            ViewData["service_address"] = service.Address;
            ViewData["service_name"] = service.Name;
            ViewData["service_path1c"] = service.Path1C;
            ViewData["bases"] = serviceRepo.GetBasesByServiceId(serviceId);
            return View();
        }

        public ActionResult Logs()
        {
            URBD2Entities dataContext = new URBD2Entities();
            IServiceRepository serviceRepo = new ServiceRepository(dataContext);

            int serviceId = int.Parse(RouteData.Values["id"].ToString());
            IService service = serviceRepo.GetServiceById(serviceId);
            if (service == null)
                return RedirectToAction("Index", "Main");
            ViewData["service_id"] = service.ServiceId;
            ViewData["service_address"] = service.Address;
            ViewData["service_name"] = service.Name;
            ViewData["bases"] = serviceRepo.GetBasesByServiceId(serviceId);
            ViewData["logs"] = serviceRepo.GetServiceLogs(serviceId);
            return View();
        }
    }
}
