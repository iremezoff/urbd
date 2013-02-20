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
using Ugoria.URBD.Contracts.Data.Commands;


namespace Ugoria.URBD.WebControl.Controllers
{
    public class MainController : Controller
    {
        // задать параметром
        private ChannelFactory<IControlService> channelFactory = new ChannelFactory<IControlService>(new NetTcpBinding(SecurityMode.None), new EndpointAddress("net.tcp://localhost:8888/URBDControl"));

        private URBD2Entities dataContext = new URBD2Entities();
        private IUser currentUser = SessionStore.GetCurrentUser();

        public ActionResult Index(TableGrouper? sort)
        {
            IBaseRepository baseRepo = new BaseRepository(dataContext, currentUser.UserId, currentUser.IsAdmin);

            TableGrouper grouper = TableGrouper.group;

            if (sort != null && sort.HasValue)
            {
                grouper = sort.Value;
                HttpContext.Response.Cookies.Add(new HttpCookie("sort", grouper.ToString()));
            }
            else if (HttpContext.Request.Cookies["sort"] != null)
                grouper = (TableGrouper)Enum.Parse(typeof(TableGrouper), HttpContext.Request.Cookies["sort"].Value);

            //TableGrouper grouper = sort ?? TableGrouper.group;
            ViewData["bases"] = baseRepo.GetBases(grouper, SystemComponent.Exchange);
            ViewData["sort"] = grouper;

            return View();
        }

        public ActionResult ExtDirectories(TableGrouper? sort)
        {
            IBaseRepository baseRepo = new BaseRepository(dataContext, currentUser.UserId, currentUser.IsAdmin);

            TableGrouper grouper = TableGrouper.group;

            if (sort != null && sort.HasValue)
            {
                grouper = sort.Value;
                HttpContext.Response.Cookies.Add(new HttpCookie("sort", grouper.ToString()));
            }
            else if (HttpContext.Request.Cookies["sort"] != null)
                grouper = (TableGrouper)Enum.Parse(typeof(TableGrouper), HttpContext.Request.Cookies["sort"].Value);

            //TableGrouper grouper = sort ?? TableGrouper.group;
            ViewData["bases"] = baseRepo.GetBases(grouper, SystemComponent.ExtDirectories);
            ViewData["sort"] = grouper;

            return View();
        }

        [SecurityAccess(typeof(IBase))]
        public ActionResult Exchange(ModeType? mode)
        {
            int baseId = -1;
            int.TryParse((string)RouteData.Values["id"], out baseId);

            IBaseRepository baseRepo = new BaseRepository(dataContext, currentUser.UserId, currentUser.IsAdmin);

            IBaseReportView baseView = baseRepo.GetBaseById(baseId, SystemComponent.Exchange);

            IControlService controlService = channelFactory.CreateChannel();
            ICommunicationObject comm = (ICommunicationObject)controlService;

            ExchangeCommand cmd = new ExchangeCommand { baseId = baseView.BaseId, modeType = mode ?? ModeType.Passive, baseName = baseView.BaseName };
            comm.Open();
            controlService.RunTask(SessionStore.GetCurrentUser().UserId, cmd);
            comm.Close();

            return RedirectToAction("Index");
        }

        [SecurityAccess(typeof(IBase))]
        public ActionResult ExtUpdate()
        {
            int baseId = -1;
            int.TryParse((string)RouteData.Values["id"], out baseId);

            IBaseRepository baseRepo = new BaseRepository(dataContext, currentUser.UserId, currentUser.IsAdmin);

            IBaseReportView baseView = baseRepo.GetBaseById(baseId, SystemComponent.ExtDirectories);

            IControlService controlService = channelFactory.CreateChannel();
            ICommunicationObject comm = (ICommunicationObject)controlService;

            ExtDirectoriesCommand cmd = new ExtDirectoriesCommand { baseId = baseView.BaseId, baseName = baseView.BaseName };
            comm.Open();
            controlService.RunTask(SessionStore.GetCurrentUser().UserId, cmd);
            comm.Close();

            return RedirectToAction("ExtDirectories");
        }

        [SecurityAccess(typeof(IBase))]
        public ActionResult Interrupt()
        {
            int baseId = -1;

            int.TryParse((string)RouteData.Values["id"], out baseId);

            IBaseRepository baseRepo = new BaseRepository(dataContext, currentUser.UserId, currentUser.IsAdmin);

            IBaseReportView baseView = baseRepo.GetBaseById(baseId, SystemComponent.Exchange);

            IControlService controlService = channelFactory.CreateChannel();
            ICommunicationObject comm = (ICommunicationObject)controlService;

            ExchangeCommand cmd = new ExchangeCommand { baseId = baseView.BaseId, baseName = baseView.BaseName, modeType = ModeType.Passive };
            comm.Open();
            controlService.InterruptTask(cmd);
            comm.Close();

            return RedirectToAction("Index");
        }

        [SecurityAccess(typeof(IBase))]
        public ActionResult InterruptExtUpdate()
        {
            int baseId = -1;

            int.TryParse((string)RouteData.Values["id"], out baseId);

            IBaseRepository baseRepo = new BaseRepository(dataContext, currentUser.UserId, currentUser.IsAdmin);

            IBaseReportView baseView = baseRepo.GetBaseById(baseId, SystemComponent.ExtDirectories);

            IControlService controlService = channelFactory.CreateChannel();
            ICommunicationObject comm = (ICommunicationObject)controlService;

            ExtDirectoriesCommand cmd = new ExtDirectoriesCommand { baseId = baseView.BaseId, baseName = baseView.BaseName };
            comm.Open();
            controlService.InterruptTask(cmd);
            comm.Close();

            return RedirectToAction("ExtDirectories");
        }
    }
}
