using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ugoria.URBD.Contracts.Services;
using Ugoria.URBD.WebControl.ViewModels;
using System.ServiceModel;
using OpenFlashChart;
using Ugoria.URBD.WebControl.Models;
using System.Globalization;
using Ugoria.URBD.WebControl.Helpers;

namespace Ugoria.URBD.WebControl.Controllers
{
    [SecurityAccess(typeof(IBase))]
    public class BaseController : Controller
    {
        private ChannelFactory<IControlService> channelFactory = new ChannelFactory<IControlService>(new NetTcpBinding(SecurityMode.None), new EndpointAddress("net.tcp://localhost:8888/URBDControl"));
        private ChannelFactory<IRemoteService> remoteChannelFactory = new ChannelFactory<IRemoteService>(new NetTcpBinding(SecurityMode.None));
        private URBD2Entities dataContext = new URBD2Entities();
        private IServiceRepository serviceRepo;
        private IUser currentUser = SessionStore.GetCurrentUser();

        public BaseController()
        {
            serviceRepo = new ServiceRepository(dataContext);
        }

        public ActionResult Index()
        {
            return RedirectToAction("Index", "Service");
        }

        [HttpPost]
        [SecurityAccess(typeof(IBase), IsChange = true)]
        public ActionResult Parameters(BaseViewModel @base)
        {
            int baseId = int.Parse(RouteData.Values["id"].ToString());
            if (baseId == 0)
                throw new HttpException(404, "ИБ не найдена");

            if (string.IsNullOrEmpty(@base.Path))
                ModelState.AddModelError("base.Path", "пустой путь не допускается");
            if (string.IsNullOrEmpty(@base.Name))
                ModelState.AddModelError("base.Name", "пустое название не допускается");
            if (string.IsNullOrEmpty(@base.Username))
                ModelState.AddModelError("base.Username", "пустое имя пользователя не допускается");
            if (@base.PacketList != null)
            {
                foreach (PacketViewModel packetVM in @base.PacketList)
                {
                    if (!string.IsNullOrEmpty(packetVM.FileName))
                        continue;
                    ModelState.AddModelError("base.PacketList.FileName", "Имена пакетов не могут быть пустыми");
                    break;
                }
            }

            TempData["success"] = false;
            if (ModelState.IsValid)
            {
                // сохраняем, если конфигруация прошла
                using (UnitOfWork unitOfWork = new UnitOfWork())
                {
                    serviceRepo = new ServiceRepository(unitOfWork.DataContext);
                    serviceRepo.SaveBase(@base);

                    unitOfWork.Commit();
                }
                TempData["success"] = true;
                try
                {
                    // конфигурируем
                    IControlService controlService = channelFactory.CreateChannel();
                    ICommunicationObject commControlObject = (ICommunicationObject)controlService;
                    commControlObject.Open();
                    controlService.ReconfigureBaseOfService(@base.BaseId);
                    commControlObject.Close();
                }
                catch (Exception ex)
                {
                    TempData["ReconfigureFail"] = "Произошла ошибка при конфигурировании центрального сервиса. Причина: " + ex.Message;
                }
                return RedirectToActionPermanent("Parameters", new { id = @base.BaseId });
            }
            ViewData["base"] = @base;
            return Parameters();
        }

        public PartialViewResult Parameters()
        {
            int baseId = int.Parse(RouteData.Values["id"].ToString());

            BaseViewModel baseVM = ViewData["base"] == null ? serviceRepo.GetBaseById(baseId) : (BaseViewModel)ViewData["base"];

            if (TempData["ReconfigureFail"] != null)
                ModelState.AddModelError("ReconfigureFail", (string)TempData["ReconfigureFail"]);

            ViewData["service"] = serviceRepo.GetServiceById(baseVM.ServiceId);
            ViewData["extdirectories"] = serviceRepo.GetExtDirectories();
            ViewData["success"] = TempData["success"];
            return PartialView(baseVM);
        }

        public PartialViewResult ExchangeReport(DateTime? dateStart, DateTime? dateEnd)
        {
            int baseId = int.Parse(RouteData.Values["id"].ToString());
            IServiceRepository serviceRepo = new ServiceRepository(new URBD2Entities());

            var reports = serviceRepo.GetExchangeReportsByBaseId(baseId, dateStart.Value.Date, dateEnd.Value.Date);
            ViewData["dateStart"] = dateStart.Value.Date;
            ViewData["dateEnd"] = dateEnd.Value.Date;
            return PartialView(reports);
        }

        public PartialViewResult ExtDirectoryReport(DateTime? dateStart, DateTime? dateEnd)
        {
            int baseId = int.Parse(RouteData.Values["id"].ToString());
            IServiceRepository serviceRepo = new ServiceRepository(new URBD2Entities());

            var reports = serviceRepo.GetExtDirectoriesReportsByBaseId(baseId, dateStart.Value.Date, dateEnd.Value.Date);
            ViewData["dateStart"] = dateStart.Value.Date;
            ViewData["dateEnd"] = dateEnd.Value.Date;
            return PartialView(reports);
        }

        public PartialViewResult MlgCollectReport(DateTime? dateStart, DateTime? dateEnd)
        {
            int baseId = int.Parse(RouteData.Values["id"].ToString());
            IServiceRepository serviceRepo = new ServiceRepository(new URBD2Entities());

            var reports = serviceRepo.GetMlgCollectReportsByBaseId(baseId, dateStart.Value.Date, dateEnd.Value.Date);
            ViewData["dateStart"] = dateStart.Value.Date;
            ViewData["dateEnd"] = dateEnd.Value.Date;
            return PartialView(reports);
        }


        /*public JsonResult ReportData(DateTime dateStart, DateTime dateEnd, string componentName)
        {
            int baseId = int.Parse(RouteData.Values["id"].ToString());
            IServiceRepository serviceRepo = new ServiceRepository(dataContext);

            if ("Exchange".Equals(componentName))
            {
                IEnumerable<ReportViewModel> reports = serviceRepo.GetExchangeReportsByBaseId(baseId, dateStart, dateEnd);
                int count = reports.Count();

                return Json(new
                {
                    aaData = reports.Select(r => new string[] { 
                    r.CommandDate!=null&& r.CommandDate.HasValue?r.CommandDate.Value.ToString("dd.MM.yyyy HH:mm:ss"):"",
                    r.CompleteDate!=null && r.CompleteDate.HasValue?r.CompleteDate.Value.ToString("dd.MM.yyyy HH:mm:ss"):"",
                    r.Status, 
                    r.Message??"", 
                   string.Join("<br/>", r.PacketList.Where(p=>p.Packet.Type[0]==(char)PacketType.Load).Select(pl => string.Format("<b>{0}</b> ({1:0.00} Kb) - {2:dd.MM.yyyy HH:mm:ss}", pl.Packet.FileName, pl.Size / 1024f, pl.CreatedDate))), 
                   string.Join("<br/>", r.PacketList.Where(p=>p.Packet.Type[0]==(char)PacketType.Unload).Select(pl => string.Format("<b>{0}</b> ({1:0.00} Kb) - {2:dd.MM.yyyy HH:mm:ss}", pl.Packet.FileName, pl.Size / 1024f, pl.CreatedDate))), 
                   r.User 
                })
                }, JsonRequestBehavior.AllowGet);
            }
            else if ("MlgCollect".Equals(componentName))
            {
                IEnumerable<ReportViewModel> reports = serviceRepo.GetMlgCollectReportsByBaseId(baseId, dateStart, dateEnd);
                int count = reports.Count();

                return Json(new
                {
                    aaData = reports.Select(r => new string[] { 
                    r.CommandDate!=null&& r.CommandDate.HasValue?r.CommandDate.Value.ToString("dd.MM.yyyy HH:mm:ss"):"",
                    r.CompleteDate!=null && r.CompleteDate.HasValue?r.CompleteDate.Value.ToString("dd.MM.yyyy HH:mm:ss"):"",
                    r.Status,                     
                    r.Message??"", 
                    r.DateMlgDate!=null&& r.DateMlgDate.HasValue?r.DateMlgDate.Value.ToString("dd.MM.yyyy HH:mm:ss"):"",
                    r.User 
                })
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                IEnumerable<ReportViewModel> reports = serviceRepo.GetExtDirectoriesReportsByBaseId(baseId, dateStart, dateEnd);
                int count = reports.Count();

                return Json(new
                {
                    aaData = reports.Select(r => new string[] { 
                    r.CommandDate!=null&& r.CommandDate.HasValue?r.CommandDate.Value.ToString("dd.MM.yyyy HH:mm:ss"):"",
                    r.CompleteDate!=null && r.CompleteDate.HasValue?r.CompleteDate.Value.ToString("dd.MM.yyyy HH:mm:ss"):"",
                    r.Status, 
                    r.Message??"", 
                   string.Join("<br/>", r.Files.Select(c => string.Format("<b>{0}</b> ({1:0.00} Kb) - {2:dd.MM.yyyy HH:mm:ss}", c.Filename, c.Size / 1024f, c.DateCopied))), 
                   r.User 
                })
                }, JsonRequestBehavior.AllowGet);
            }
        }*/

        [HttpPost]
        public ActionResult Notifications(IEnumerable<ReportNotificationViewModel> notifies, int userId)
        {
            int baseId = int.Parse(RouteData.Values["id"].ToString());

            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                serviceRepo = new ServiceRepository(unitOfWork.DataContext);
                foreach (var notify in notifies)
                {
                    serviceRepo.SaveNotification(notify, baseId, userId);
                }
                unitOfWork.Commit();
            }
            TempData["success"] = true;
            return RedirectToActionPermanent("Notifications", new { id = RouteData.Values["id"], userId = userId });
        }

        public PartialViewResult Notifications(int? userId)
        {
            int baseId = int.Parse(RouteData.Values["id"].ToString());

            userId = (currentUser.IsAdmin && userId != null) ? userId : SessionStore.GetCurrentUser().UserId;

            var statuses = serviceRepo.GetComponentReportStatusByUserId(userId.Value, baseId);

            if (currentUser.IsAdmin)
            {
                var permissions = serviceRepo.GetBasePermissions(baseId);
                if (permissions.Count()==0)
                    statuses = new List<ComponentReportStatusView>();
                ViewData["permissions"] = permissions;
            }
            ViewData["success"] = TempData["success"];
            ViewData["user_id"] = userId.Value;
            return PartialView(statuses);
        }

        [HttpPost]
        [SecurityAccess(IsAdmin = true)]
        public ActionResult Permissions(IEnumerable<PermissionViewModel> permissions)
        {
            int baseId = int.Parse(RouteData.Values["id"].ToString());

            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                IServiceRepository serviceRepo = new ServiceRepository(unitOfWork.DataContext);
                foreach (var permission in permissions)
                {
                    serviceRepo.SavePermission(permission, baseId);
                }
                unitOfWork.Commit();
            }
            TempData["success"] = true;
            return RedirectToActionPermanent("Permissions", new { id = RouteData.Values["id"] });
        }

        [SecurityAccess(IsAdmin = true)]
        public PartialViewResult Permissions()
        {
            int baseId = int.Parse(RouteData.Values["id"].ToString());

            BaseViewModel baseVM = ViewData["base"] == null ? serviceRepo.GetBaseById(baseId) : (BaseViewModel)ViewData["base"];

            ViewData["success"] = TempData["success"];
            var permissions = serviceRepo.GetBasePermissions(baseId);

            return PartialView(permissions);
        }

        public ActionResult Edit()
        {
            int baseId = int.Parse(RouteData.Values["id"].ToString());

            BaseViewModel @base = ViewData["base"] == null ? serviceRepo.GetBaseById(baseId) : (BaseViewModel)ViewData["base"];
            ViewData["success"] = TempData["success"];
            if (TempData["ReconfigureFail"] != null)
                ModelState.AddModelError("ReconfigureFail", (string)TempData["ReconfigureFail"]);

            ViewData["base"] = @base;
            IUser user = SessionStore.GetCurrentUser();
            ViewData["bases"] = serviceRepo.GetBasesByServiceId(@base.ServiceId, user.UserId, user.IsAdmin);

            ViewData["reports"] = new List<ReportViewModel>();
            ViewData["report_statuses"] = serviceRepo.GetComponentReportStatusByUserId(SessionStore.GetCurrentUser().UserId, baseId);
            IEnumerable<ServiceViewModel> services = serviceRepo.GetServices();
            ServiceViewModel service = services.Where(s => s.ServiceId == @base.ServiceId).Single();
            ViewData["services"] = services;
            ViewData["service"] = service;


            return View();
        }

        public ContentResult RefBarData(string scale)
        {
            int chartScale = 0;
            int baseId = 0;
            int.TryParse(scale, out chartScale);
            if (chartScale == 0) chartScale = 1;
            int.TryParse(RouteData.Values["id"].ToString(), out baseId);

            OpenFlashChart.OpenFlashChart chart = new OpenFlashChart.OpenFlashChart();

            List<string> labels = new List<string>();
            List<List<BarStackValue>> values = new List<List<BarStackValue>>();

            double? max = 0;

            IServiceRepository serviceRepo = new ServiceRepository(dataContext);

            TimeSpan fillStart = TimeSpan.Zero;
            List<BarStackValue> storedValue = null;
            int lastBaseId = 0;
            Dictionary<string, string> baseValues = new Dictionary<string, string>();
            Random rand = new Random();
            Queue<string> colors = new Queue<string>();
            colors.Enqueue("#000000");
            colors.Enqueue("#DA0709");
            colors.Enqueue("#3AA8E9");
            colors.Enqueue("#3AA8E9");
            colors.Enqueue("#800080");
            colors.Enqueue("#008080");
            Queue<string> reserveColors = new Queue<string>(colors);
            int total = 0;
            foreach (BaseTreeSchedule schedule in serviceRepo.GetReferenceByBaseId(baseId))
            {
                while (fillStart <= TimeSpan.Parse(schedule.Time))
                {
                    labels.Add(fillStart.ToString(@"hh\:mm"));
                    fillStart = fillStart.Add(new TimeSpan(0, chartScale, 0));
                    values.Add(null);
                }
                storedValue = values[values.Count - 1];
                string currColor = null;
                if (!baseValues.ContainsKey(schedule.BaseName))
                {
                    currColor = colors.Dequeue();
                    baseValues.Add(schedule.BaseName, currColor);
                    if (colors.Count == 0)
                        colors = new Queue<string>(reserveColors);
                }
                else
                    currColor = baseValues[schedule.BaseName];
                if (storedValue == null)
                {
                    total = 1;
                    lastBaseId = 0;
                    storedValue = new List<BarStackValue>();
                    storedValue.Add(new BarStackValue(1) { Colour = currColor });
                    values[values.Count - 1] = storedValue;
                    //storedValue.Tip = "Время: #x_label#<br>" + schedule.BaseName;
                }
                else
                {
                    total++;
                    if (lastBaseId == schedule.BaseId)
                        storedValue[storedValue.Count - 1].Val++;
                    else
                        storedValue.Add(new BarStackValue(1, currColor));
                    lastBaseId = schedule.BaseId;
                }
                max = total > max ? total : max;
            }

            while (fillStart < new TimeSpan(1, 0, 0, 0))
            {
                labels.Add(fillStart.ToString(@"hh\:mm"));
                fillStart = fillStart.Add(new TimeSpan(0, chartScale, 0));
                values.Add(null);
            }

            XAxis xaxis = new XAxis();
            xaxis.Steps = (int)(60 / chartScale);
            xaxis.Labels.FontSize = 8;
            xaxis.SetLabels(labels);
            xaxis.Labels.Steps = (int)(60 / chartScale);

            YAxis yaxis = new YAxis();
            yaxis.SetRange(0, max + 1);
            yaxis.Offset = false;

            Legend ylegend = new Legend("количество запусков");
            ylegend.Style = "{font-size: 10px; color: #000000}";

            BarStack barStack = new BarStack();
            barStack.FontSize = 10;
            barStack.Values = values;
            barStack.OnShowAnimation = new Animation("pop", 1, 0);
            barStack.Tooltip = "Время: #x_label#<br>Всего запусков: #total#";

            foreach (KeyValuePair<string, string> basePair in baseValues)
            {
                Area area = new Area() { Colour = basePair.Value };
                area.Set_Key(basePair.Key, 10f);
                chart.AddElement(area);
            }

            chart.AddElement(barStack);
            chart.X_Axis = xaxis;
            chart.Y_Axis = yaxis;
            chart.Y_Legend = ylegend;
            chart.Bgcolor = "#ffffff";
            chart.Title = new Title("Распределение обменов связанных ИБ");
            chart.Title.Style = "{font-size: 12px;}";

            return Content(chart.ToPrettyString(), "text/html");
        }

        public ContentResult BarData(string scale)
        {
            int chartScale = 0;
            int baseId = 0;
            int.TryParse(scale, out chartScale);
            if (chartScale == 0) chartScale = 1;
            int.TryParse(RouteData.Values["id"].ToString(), out baseId);

            OpenFlashChart.OpenFlashChart chart = new OpenFlashChart.OpenFlashChart();

            List<string> labels = new List<string>();
            List<BarValue> values = new List<BarValue>();

            double? max = 0;

            IServiceRepository serviceRepo = new ServiceRepository(dataContext);

            TimeSpan fillStart = TimeSpan.Zero;
            BarValue storedValue = null;
            foreach (IServiceScheduleExchange schedule in serviceRepo.GetServiceScheduleExchangeByBaseId(baseId))
            {
                while (fillStart <= TimeSpan.Parse(schedule.Time))
                {
                    labels.Add(fillStart.ToString(@"hh\:mm"));
                    fillStart = fillStart.Add(new TimeSpan(0, chartScale, 0));
                    values.Add(null);
                }
                storedValue = values[values.Count - 1];
                if (storedValue == null)
                {
                    storedValue = new BarValue(1);
                    values[values.Count - 1] = storedValue;
                    storedValue.Tip = String.Format("Время: #x_label#<br>{0} - {1}", schedule.BaseName, schedule.Time);
                }
                else
                {
                    storedValue.Top++;
                    max = storedValue.Top > max ? storedValue.Top : max;
                    storedValue.Tip += String.Format("<br>{0} - {1}", schedule.BaseName, schedule.Time);
                }
                if (schedule.BaseId == baseId) storedValue.Color = "#DA0709";
            }

            while (fillStart < new TimeSpan(1, 0, 0, 0))
            {
                labels.Add(fillStart.ToString(@"hh\:mm"));
                fillStart = fillStart.Add(new TimeSpan(0, chartScale, 0));
                values.Add(null);
            }

            XAxis xaxis = new XAxis();
            xaxis.Steps = (int)(60 / chartScale);
            xaxis.Labels.FontSize = 8;
            xaxis.SetLabels(labels);
            xaxis.Labels.Steps = (int)(60 / chartScale);

            YAxis yaxis = new YAxis();
            yaxis.SetRange(0, max + 1);
            yaxis.Offset = false;

            Legend ylegend = new Legend("количество запусков");
            ylegend.Style = "{font-size: 10px; color: #000000}";

            Bar bar = new Bar();
            bar.Colour = "#000000";
            bar.Tooltip = "Время: #x_label#";
            bar.FontSize = 10;
            bar.Values = values;
            bar.OnShowAnimation = new Animation("pop", 1, 0);

            Area redArea = new Area();
            redArea.Colour = "#DA0709";
            redArea.Set_Key("с текущей ИБ", 12f);

            Area blackArea = new Area();
            blackArea.Colour = "#000000";
            blackArea.Set_Key("без текущей ИБ", 12f);

            chart.AddElement(redArea);
            chart.AddElement(blackArea);
            chart.AddElement(bar);
            chart.X_Axis = xaxis;
            chart.Y_Axis = yaxis;
            chart.Y_Legend = ylegend;
            chart.Bgcolor = "#ffffff";
            chart.Title = new Title("Распределение обменов сервиса");
            chart.Title.Style = "{font-size: 12px;}";

            return Content(chart.ToPrettyString(), "text/html");
        }
    }
}
