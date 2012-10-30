using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ugoria.URBD.WebControl.Models;
using System.ServiceModel;
using Ugoria.URBD.Contracts.Services;
using Ugoria.URBD.WebControl.Helpers;
using System.IO;
using Ugoria.URBD.Shared;
using Ugoria.URBD.WebControl.ViewModels;


namespace Ugoria.URBD.WebControl.Controllers
{
    public class MainController : Controller
    {
        // задать параметром
        private ChannelFactory<IControlService> channelFactory = new ChannelFactory<IControlService>(new NetTcpBinding(SecurityMode.None), new EndpointAddress("net.tcp://localhost:8888/URBDControl"));

        public ActionResult Index(TableGrouper? sort)
        {
            URBD2Entities dataContext = new URBD2Entities();

            IUser currentUser = SessionStore.GetCurrentUser();
            IBaseRepository baseRepo = new BaseRepository(dataContext, currentUser.UserId, currentUser.IsAdmin);

            TableGrouper grouper = sort ?? TableGrouper.group;
            ViewData["bases"] = baseRepo.GetBases(grouper);
            ViewData["sort"] = grouper.ToString();

            return View();
        }

        [ServiceAccess(typeof(IBase), LimitedAccess = true)]
        public ActionResult Exchange(ModeType? mode)
        {
            int baseId = -1;
            int.TryParse((string)RouteData.Values["id"], out baseId);

            URBD2Entities dataContext = new URBD2Entities();
            IUser currentUser = SessionStore.GetCurrentUser();
            IBaseRepository baseRepo = new BaseRepository(dataContext, currentUser.UserId, currentUser.IsAdmin);

            IBaseReportView baseView = baseRepo.GetBaseById(baseId);

            IControlService controlService = channelFactory.CreateChannel();
            ICommunicationObject comm = (ICommunicationObject)controlService;

            comm.Open();
            controlService.RunTask(SessionStore.GetCurrentUser().UserId, baseView.BaseId, mode ?? ModeType.Passive);
            comm.Close();

            return RedirectToAction("Index");
        }

        [ServiceAccess(typeof(IBase), LimitedAccess = true)]
        public ActionResult Interrupt()
        {
            int baseId = -1;

            int.TryParse((string)RouteData.Values["id"], out baseId);

            IUser currentUser = SessionStore.GetCurrentUser();
            URBD2Entities dataContext = new URBD2Entities();
            IBaseRepository baseRepo = new BaseRepository(dataContext, currentUser.UserId, currentUser.IsAdmin);

            IBaseReportView baseView = baseRepo.GetBaseById(baseId);

            IControlService controlService = channelFactory.CreateChannel();
            ICommunicationObject comm = (ICommunicationObject)controlService;

            comm.Open();
            controlService.InterruptTask(baseView.BaseId);
            comm.Close();

            return RedirectToAction("Index");
        }
    }
}
