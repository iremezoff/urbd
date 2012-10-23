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
using Ugoria.URBD.Core;
using Ugoria.URBD.Logging;


namespace Ugoria.URBD.WebControl.Controllers
{

    [ServiceAccess]
    [HandleError(ExceptionType=typeof(AccessPermissionDeniedException), View="AccessDenied", Order=1)]
    [HandleError(ExceptionType = typeof(CommunicationException), View = "CommError", Order = 1)]
    public class MainController : Controller
    {
        // задать параметром
        private ChannelFactory<IControlService> channelFactory = new ChannelFactory<IControlService>(new NetTcpBinding(SecurityMode.None), new EndpointAddress("net.tcp://localhost:8888/URBDControl"));

        
        public ActionResult Index(TableGrouper? sort)
        {
            try
            {
                URBD2Entities dataContext = new URBD2Entities();

                IBaseRepository baseRepo = new BaseRepository(dataContext, SessionStore.GetCurrentUser().UserId);

                TableGrouper grouper = sort ?? TableGrouper.group;
                ViewData["bases"] = baseRepo.GetBases(grouper);
                ViewData["sort"] = grouper.ToString();
            }
            catch (Exception ex)
            {
                LogHelper.Write2Log(URBDComponent.Web, ex);
            }

            return View();
        }

        [ServiceAccess(typeof(IBase), LimitedAccess=true)]
        public ActionResult Exchange(ModeType? mode)
        {
            int baseId = -1;
            int.TryParse((string)RouteData.Values["id"], out baseId);

            URBD2Entities dataContext = new URBD2Entities();
            IBaseRepository baseRepo = new BaseRepository(dataContext, SessionStore.GetCurrentUser().UserId);

            IBaseReportView baseView = baseRepo.GetBaseById(baseId);

            IControlService controlService = channelFactory.CreateChannel();
            ICommunicationObject comm = (ICommunicationObject)controlService;
            
                comm.Open();
                controlService.RunTask(SessionStore.GetCurrentUser().UserId, baseView.BaseId, mode ?? ModeType.Passive);
                comm.Close();
            
            
            //    LogHelper.Write2Log(URBDComponent.Web, ex);
            

            return RedirectToAction("Index");
        }

        [ServiceAccess(typeof(IBase), LimitedAccess = true)]
        public ActionResult Interrupt()
        {
            int baseId = -1;

            int.TryParse((string)RouteData.Values["id"], out baseId);

            URBD2Entities dataContext = new URBD2Entities();
            IBaseRepository baseRepo = new BaseRepository(dataContext, SessionStore.GetCurrentUser().UserId);

            IBaseReportView baseView = baseRepo.GetBaseById(baseId);

            IControlService controlService = channelFactory.CreateChannel();
            ICommunicationObject comm = (ICommunicationObject)controlService;
            
                comm.Open();
                controlService.InterruptTask(baseView.BaseId);
                comm.Close();
            
            
             //   LogHelper.Write2Log(URBDComponent.Web, ex);
            

            return RedirectToAction("Index");
        }
    }
}
