using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ugoria.URBD.WebControl.Models;
using System.ServiceModel;
using Ugoria.URBD.Contracts.Services;

namespace Ugoria.URBD.WebControl.Controllers
{
    [SecurityAccess(IsAdmin = true)]
    public class SettingsController : Controller
    {
        private ChannelFactory<IControlService> channelFactory = new ChannelFactory<IControlService>(new NetTcpBinding(SecurityMode.None), new EndpointAddress("net.tcp://localhost:8888/URBDControl"));

        public ActionResult Index()
        {
            return RedirectToAction("Edit");
        }

        [HttpPost]
        public ActionResult Edit(IEnumerable<SettingViewModel> settings)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                ISettingsRepository settingsRepo = new SettingsRepository(unitOfWork.DataContext);
                foreach (SettingViewModel setting in settings)
                {
                    settingsRepo.SaveSetting(setting);
                }
                unitOfWork.Commit();
            }
            ViewData["success"] = true;
            try
            {
                IControlService controlService = channelFactory.CreateChannel();
                ICommunicationObject commControlObject = (ICommunicationObject)controlService;
                commControlObject.Open();
                controlService.ReconfigureCentralService();
                commControlObject.Close();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("ReconfigureFail", "Ошибка при попытке переконфигурировать центральный сервис. <br>Ошибка:" + ex.Message);
            }            
            return RedirectToActionPermanent("Edit");
        }

        public ActionResult Edit()
        {
            ISettingsRepository settingsRepo = new SettingsRepository(new URBD2Entities());
            ViewData["settings"] = settingsRepo.GetSettings();
            return View();
        }
    }
}
