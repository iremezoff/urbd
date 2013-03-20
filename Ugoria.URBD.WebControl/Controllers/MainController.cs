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
using Ugoria.URBD.WebControl.Filters;


namespace Ugoria.URBD.WebControl.Controllers
{
    [ComponentAttribute]
    public class MainController : Controller
    {
        // задать параметром
        private ChannelFactory<IControlService> channelFactory = new ChannelFactory<IControlService>(new NetTcpBinding(SecurityMode.None), new EndpointAddress("net.tcp://localhost:8888/URBDControl"));

        private URBD2Entities dataContext = new URBD2Entities();
        private IUser currentUser = SessionStore.GetCurrentUser();

        public ActionResult Index()
        {
            return RedirectToAction("Exchange");
        }

        public ActionResult Exchange(TableGrouper? sort)
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
            ViewData["bases"] = baseRepo.GetBases(grouper, "Exchange");
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
            ViewData["bases"] = baseRepo.GetBases(grouper, "ExtDirectories");
            ViewData["sort"] = grouper;

            return View();
        }

        public ActionResult MlgCollect(TableGrouper? sort)
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
            ViewData["bases"] = baseRepo.GetBases(grouper, "MlgCollect");
            ViewData["sort"] = grouper;
            ViewData["object_types"] = baseRepo.GetObjectTypes();
            ViewData["base_codes"] = baseRepo.GetBaseCodes();

            return View();
        }

        [HttpPost]
        public ActionResult MlgCollect(string number, string baseCode, int type, DateTime? startDate, DateTime? endDate)
        {
            IBaseRepository baseRepo = new BaseRepository(dataContext, currentUser.UserId, currentUser.IsAdmin);
            IEnumerable<ReportLog> logs = baseRepo.GetReportLogByObject(type, baseCode, number, startDate.Value.Date, endDate.Value.Date);
            ViewData["logs"] = logs;
            ViewData["object_types"] = baseRepo.GetObjectTypes();
            ViewData["base_codes"] = baseRepo.GetBaseCodes();
            ViewData["number"] = number;
            ViewData["base_code"] = baseCode;
            ViewData["type"] = type;
            ViewData["date_start"] = startDate;
            ViewData["date_end"] = endDate;

            return View("MlgCollectSearch");
        }

        [SecurityAccess(typeof(IBase))]
        public ActionResult MlgCollectGo()
        {
            int baseId = -1;
            int.TryParse((string)RouteData.Values["id"], out baseId);

            IBaseRepository baseRepo = new BaseRepository(dataContext, currentUser.UserId, currentUser.IsAdmin);

            IBaseReportView baseView = baseRepo.GetBaseById(baseId, "MlgCollect");

            IControlService controlService = channelFactory.CreateChannel();
            ICommunicationObject comm = (ICommunicationObject)controlService;

            MlgCollectCommand cmd = new MlgCollectCommand { baseId = baseView.BaseId, baseName = baseView.BaseName };
            comm.Open();
            controlService.RunTask(SessionStore.GetCurrentUser().UserId, cmd);
            comm.Close();

            return RedirectToAction("MlgCollect");
        }

        [SecurityAccess(typeof(IBase))]
        public ActionResult MlgCollectInterrupt()
        {
            int baseId = -1;

            int.TryParse((string)RouteData.Values["id"], out baseId);

            IBaseRepository baseRepo = new BaseRepository(dataContext, currentUser.UserId, currentUser.IsAdmin);

            IBaseReportView baseView = baseRepo.GetBaseById(baseId, "Exchange");

            IControlService controlService = channelFactory.CreateChannel();
            ICommunicationObject comm = (ICommunicationObject)controlService;

            MlgCollectCommand cmd = new MlgCollectCommand { baseId = baseView.BaseId, baseName = baseView.BaseName };
            comm.Open();
            controlService.InterruptTask(cmd);
            comm.Close();

            return RedirectToAction("MlgCollect");
        }

        [SecurityAccess(typeof(IBase))]
        public ActionResult ExchangeGo(ModeType? mode)
        {
            int baseId = -1;
            int.TryParse((string)RouteData.Values["id"], out baseId);

            IBaseRepository baseRepo = new BaseRepository(dataContext, currentUser.UserId, currentUser.IsAdmin);

            IBaseReportView baseView = baseRepo.GetBaseById(baseId, "Exchange");

            IControlService controlService = channelFactory.CreateChannel();
            ICommunicationObject comm = (ICommunicationObject)controlService;

            ExchangeCommand cmd = new ExchangeCommand { baseId = baseView.BaseId, modeType = mode ?? ModeType.Passive, baseName = baseView.BaseName };
            comm.Open();
            controlService.RunTask(SessionStore.GetCurrentUser().UserId, cmd);
            comm.Close();

            return RedirectToAction("Exchange");
        }

        [SecurityAccess(typeof(IBase))]
        public ActionResult ExtDirectoriesGo()
        {
            int baseId = -1;
            int.TryParse((string)RouteData.Values["id"], out baseId);

            IBaseRepository baseRepo = new BaseRepository(dataContext, currentUser.UserId, currentUser.IsAdmin);

            IBaseReportView baseView = baseRepo.GetBaseById(baseId, "ExtDirectories");

            IControlService controlService = channelFactory.CreateChannel();
            ICommunicationObject comm = (ICommunicationObject)controlService;

            ExtDirectoriesCommand cmd = new ExtDirectoriesCommand { baseId = baseView.BaseId, baseName = baseView.BaseName };
            comm.Open();
            controlService.RunTask(SessionStore.GetCurrentUser().UserId, cmd);
            comm.Close();

            return RedirectToAction("ExtDirectories");
        }

        [SecurityAccess(typeof(IBase))]
        public ActionResult ExchangeInterrupt()
        {
            int baseId = -1;

            int.TryParse((string)RouteData.Values["id"], out baseId);

            IBaseRepository baseRepo = new BaseRepository(dataContext, currentUser.UserId, currentUser.IsAdmin);

            IBaseReportView baseView = baseRepo.GetBaseById(baseId, "Exchange");

            IControlService controlService = channelFactory.CreateChannel();
            ICommunicationObject comm = (ICommunicationObject)controlService;

            ExchangeCommand cmd = new ExchangeCommand { baseId = baseView.BaseId, baseName = baseView.BaseName, modeType = ModeType.Passive };
            comm.Open();
            controlService.InterruptTask(cmd);
            comm.Close();

            return RedirectToAction("Index");
        }

        [SecurityAccess(typeof(IBase))]
        public ActionResult ExtDirectoriesInterrupt()
        {
            int baseId = -1;

            int.TryParse((string)RouteData.Values["id"], out baseId);

            IBaseRepository baseRepo = new BaseRepository(dataContext, currentUser.UserId, currentUser.IsAdmin);

            IBaseReportView baseView = baseRepo.GetBaseById(baseId, "ExtDirectories");

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
