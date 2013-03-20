using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ugoria.URBD.WebControl.Models;
using System.ServiceModel;
using Ugoria.URBD.Contracts.Services;
using Ugoria.URBD.WebControl.ViewModels;

namespace Ugoria.URBD.WebControl.Controllers
{
    [SecurityAccess(IsAdmin = true)]
    public class ControlController : Controller
    {
        private ChannelFactory<IControlService> channelFactory = new ChannelFactory<IControlService>(new NetTcpBinding(SecurityMode.None), new EndpointAddress("net.tcp://localhost:8888/URBDControl"));

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Settings(IEnumerable<SettingViewModel> settings)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                ISettingsRepository settingsRepo = new SettingsRepository(unitOfWork.DataContext);
                foreach (SettingViewModel setting in settings)
                {
                    settingsRepo.SaveSetting(setting);
                }
                settingsRepo.UpdateServicesDataChange(DateTime.Now);
                unitOfWork.Commit();
            }
            TempData["success"] = true;
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
                ModelStateDictionary errors = new ModelStateDictionary();
                errors.AddModelError("ReconfigureFail", "Ошибка при попытке переконфигурировать центральный сервис. Ошибка:" + ex.Message);
                TempData["errors"] = errors;
            }
            return RedirectToActionPermanent("Settings");
        }

        public PartialViewResult Settings()
        {
            ISettingsRepository settingsRepo = new SettingsRepository(new URBD2Entities());
            if (TempData["errors"] != null)
            {
                foreach (var error in (ModelStateDictionary)TempData["errors"])
                {
                    ModelState.Add(error);
                }
            }
            ViewData["success"] = TempData["success"];
            ViewData["settings"] = TempData["settings"] != null ? TempData["settings"] : settingsRepo.GetSettings();
            return PartialView();
        }

        [HttpPost]
        public ActionResult Users(IEnumerable<UserViewModel> users)
        {
            ModelStateDictionary errors = new ModelStateDictionary();
            if (users.Any(u => string.IsNullOrEmpty(u.UserName)))
                errors.AddModelError("LoginError", "Поле логин не может быть пустым");
            if (errors.IsValid)
            {
                using (UnitOfWork unitOfWork = new UnitOfWork())
                {
                    IUsersRepository usersRepo = new UsersRepository(unitOfWork.DataContext);
                    foreach (var user in users)
                    {
                        usersRepo.SaveUser(user);
                    }
                    unitOfWork.Commit();
                }
                TempData["success"] = true;
                return RedirectToActionPermanent("Users");
            }
            TempData["success"] = false;
            TempData["errors"] = errors;
            TempData["users"] = users;
            return Users();
        }

        public PartialViewResult Users()
        {
            IUsersRepository usersRepo = new UsersRepository(new URBD2Entities());
            if (TempData["errors"] != null)
            {
                foreach (var error in (ModelStateDictionary)TempData["errors"])
                {
                    ModelState.Add(error);
                }
            }
            ViewData["success"] = TempData["success"];
            ViewData["users"] = TempData["users"] != null ? TempData["users"] : usersRepo.GetUsers();
            return PartialView();
        }

        [HttpPost]
        public ActionResult ExtDirectories(IEnumerable<ExtDirectoryViewModel> dirs)
        {
            ModelStateDictionary errors = new ModelStateDictionary();
            if (dirs.Any(u => string.IsNullOrEmpty(u.LocalPath) || string.IsNullOrEmpty(u.FtpPath) && !(string.IsNullOrEmpty(u.LocalPath) && string.IsNullOrEmpty(u.LocalPath))))
                errors.AddModelError("PathError", "Пути не могут быть пустыми");
            if (errors.IsValid)
            {
                using (UnitOfWork unitOfWork = new UnitOfWork())
                {
                    ISettingsRepository settingsRepo = new SettingsRepository(unitOfWork.DataContext);
                    foreach (ExtDirectoryViewModel dir in dirs)
                    {
                        settingsRepo.SaveExtDirectory(dir);
                    }
                    settingsRepo.UpdateServicesDataChange(DateTime.Now);
                    unitOfWork.Commit();
                }
                TempData["success"] = true;
                return RedirectToActionPermanent("ExtDirectories");
            }
            TempData["success"] = false;
            TempData["errors"] = errors;
            TempData["extdirectories"] = dirs;
            return ExtDirectories();
        }

        public PartialViewResult ExtDirectories()
        {
            ISettingsRepository settingsRepo = new SettingsRepository(new URBD2Entities());
            if (TempData["errors"] != null)
            {
                foreach (var error in (ModelStateDictionary)TempData["errors"])
                {
                    ModelState.Add(error);
                }
            }
            ViewData["success"] = TempData["success"];
            ViewData["extdirectories"] = TempData["extdirectories"] != null ? TempData["extdirectories"] : settingsRepo.GetExtDirectories();
            return PartialView();
        }

        public ActionResult UsersList()
        {
            IUsersRepository serviceRepo = new UsersRepository(new URBD2Entities());

            Dictionary<string, string> users = serviceRepo.GetUsers().ToDictionary(k => k.UserId.ToString(), v => v.UserName);

            return Json(users, JsonRequestBehavior.AllowGet);
        }
    }
}
