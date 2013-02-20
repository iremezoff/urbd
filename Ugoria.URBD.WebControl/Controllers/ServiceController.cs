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
using System.Globalization;
using Ugoria.URBD.WebControl.Helpers;

namespace Ugoria.URBD.WebControl.Controllers
{
    [SecurityAccess(typeof(IService))]
    public class ServiceController : Controller
    {
        private ChannelFactory<IControlService> channelFactory = new ChannelFactory<IControlService>(new NetTcpBinding(SecurityMode.None), new EndpointAddress("net.tcp://localhost:8888/URBDControl"));

        public ActionResult Index()
        {
            return RedirectToAction("Index", "Main", new { sort = TableGrouper.service });
        }
        
        public ActionResult Edit()
        {
            IServiceRepository serviceRepo = new ServiceRepository(new URBD2Entities());
            int serviceId = int.Parse(RouteData.Values["id"].ToString());
            ServiceViewModel serviceVM = ViewData["service"] == null ? serviceRepo.GetServiceById(serviceId) : (ServiceViewModel)ViewData["service"];
            if (serviceVM == null)
                return RedirectToAction("Index", "Main");

            if (TempData["ReconfigureFail"] != null)
                ModelState.AddModelError("ReconfigureFail", (string)TempData["ReconfigureFail"]);

            IUser user = SessionStore.GetCurrentUser();
            ViewData["bases"] = serviceRepo.GetBasesByServiceId(serviceId, user.UserId, user.IsAdmin);
            ViewData["success"] = TempData["success"];
            ViewData["service"] = serviceVM;
            return View();
        }

        [HttpPost]
        [SecurityAccess(typeof(IService), IsChange = true)]
        public ActionResult Edit(ServiceViewModel service)
        {
            int serviceId = int.Parse(RouteData.Values["id"].ToString());

            if (string.IsNullOrWhiteSpace(service.Path1C))
                ModelState.AddModelError("service.Path1c", "Путь не может быть пустым");
            if (string.IsNullOrWhiteSpace(service.Name))
                ModelState.AddModelError("service.Name", "Название сервиса не может быть пустым");
            if (string.IsNullOrWhiteSpace(service.Address))
                ModelState.AddModelError("service.Address", "Адрес сервиса не может быть пустым");
            TempData["success"] = false;
            if (ModelState.IsValid)
            {
                // сохраняем
                IServiceRepository serviceRepo = new ServiceRepository(new URBD2Entities());
                serviceRepo.SaveService(service);

                TempData["success"] = true;
                try
                {
                    // конфигурируем
                    IControlService controlService = channelFactory.CreateChannel();
                    ICommunicationObject commControlObject = (ICommunicationObject)controlService;
                    commControlObject.Open();
                    controlService.ReconfigureRemoteService(service.ServiceId);
                    commControlObject.Close();
                }
                catch (Exception ex)
                {
                    TempData["ReconfigureFail"] = "Произошла ошибка при конфигурировании центрального сервиса. Причина: " + ex.Message;
                }
                RedirectToActionPermanent("Edit");
            }
            ViewData["service"] = service;
            return Edit();
        }

        public ActionResult Logs()
        {
            URBD2Entities dataContext = new URBD2Entities();
            IServiceRepository serviceRepo = new ServiceRepository(dataContext);

            int serviceId = int.Parse(RouteData.Values["id"].ToString());
            ServiceViewModel service = serviceRepo.GetServiceById(serviceId);
            if (service == null)
                return RedirectToAction("Index", "Main");

            ViewData["service"] = service;
            IUser user = SessionStore.GetCurrentUser();
            ViewData["bases"] = serviceRepo.GetBasesByServiceId(serviceId, user.UserId, user.IsAdmin);
            ViewData["logs"] = new List<ServiceLogViewModel>();
            return View();
        }


        public JsonResult LogData(string dateStart, string dateEnd)
        {
            DateTime startDate = DateTime.MinValue;
            DateTime endDate = DateTime.MinValue;

            DateTime.TryParseExact(dateStart, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate);
            DateTime.TryParseExact(dateEnd, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate);

            int serviceId = int.Parse(RouteData.Values["id"].ToString());
            URBD2Entities dataContext = new URBD2Entities();
            IServiceRepository serviceRepo = new ServiceRepository(dataContext);
            IEnumerable<ServiceLogViewModel> reports = serviceRepo.GetServiceLogs(serviceId, startDate == DateTime.MinValue ? DateTime.Now : startDate, endDate == DateTime.MinValue ? DateTime.Now : endDate);
            int count = reports.Count();

            return Json(new
            {
                aaData = reports.Select(l => new string[] { 
                    l.Date.Value.ToString("dd.MM.yyyy HH:mm:ss"),
                    l.Status, 
                    l.Text                   
                })
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
