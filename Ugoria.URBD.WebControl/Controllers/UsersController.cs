using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ugoria.URBD.WebControl.Models;
using Ugoria.URBD.WebControl.ViewModels;

namespace Ugoria.URBD.WebControl.Controllers
{
    [SecurityAccess(IsAdmin = true)]
    public class UsersController : Controller
    {
        public ActionResult Index()
        {
            IUsersRepository usersRepo = new UsersRepository(new URBD2Entities());
            if (ViewData["users"] == null)
                ViewData["users"] = usersRepo.GetUsers();
            return View();
        }

        [HttpPost]
        public ActionResult Index(IEnumerable<UserViewModel> users)
        {
            if (users.Any(u => string.IsNullOrEmpty(u.UserName)))
                ModelState.AddModelError("LoginError", "Поле логин не может быть пустым");
            ViewData["success"] = false;
            if (ModelState.IsValid)
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
                ViewData["success"] = true;
                return RedirectToActionPermanent("Index");
            }
            ViewData["users"] = users;
            return Index();
        }

        public ActionResult List()
        {
            IUsersRepository serviceRepo = new UsersRepository(new URBD2Entities());

            Dictionary<string, string> users = serviceRepo.GetUsers().ToDictionary(k => k.UserId.ToString(), v => v.UserName);

            return Json(users, JsonRequestBehavior.AllowGet);
        }
    }
}
