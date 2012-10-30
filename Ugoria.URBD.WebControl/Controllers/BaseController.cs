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

namespace Ugoria.URBD.WebControl.Controllers
{
    public class BaseController : Controller
    {
        private ChannelFactory<IControlService> channelFactory = new ChannelFactory<IControlService>(new NetTcpBinding(SecurityMode.None), new EndpointAddress("net.tcp://localhost:8888/URBDControl"));
        private URBD2Entities dataContext = new URBD2Entities();

        public ActionResult Index()
        {
            return RedirectToAction("Index", "Service");
        }

        public ActionResult Edit(BaseViewModel @base)
        {            
            IServiceRepository serviceRepo = new ServiceRepository(dataContext);

            int baseId = int.Parse(RouteData.Values["id"].ToString());

            IBase base1c = serviceRepo.GetBaseById(baseId);
            if(base1c==null)
                throw new HttpException(404, "Отсутствует Иб с данным ID");
            IService service = serviceRepo.GetServiceById(base1c.ServiceId);

            ViewData["service_id"] = service.ServiceId;
            ViewData["service_address"] = service.Address;
            ViewData["service_name"] = service.Name;
            ViewData["base"] = base1c;
            ViewData["bases"] = serviceRepo.GetBasesByServiceId(base1c.ServiceId);
            ViewData["reports"] = serviceRepo.GetReportsByBaseId(baseId);

            if ("POST".Equals(Request.RequestType))
            {
                IDictionary<string, string> validateReport = null;
                try
                {
                    IControlService controlService = channelFactory.CreateChannel();
                    validateReport = controlService.ValidateConfiguration(new Uri(String.Format("net.tcp://{0}", @base.ServiceAddress)), new Contracts.Data.RemoteConfiguration
                    {
                        baseList = new List<Contracts.Data.Base>() { 
                    new Contracts.Data.Base { basePath = @base.Path}
                }
                    });
                    if (validateReport != null)
                    {
                        foreach (KeyValuePair<string, string> reportItem in validateReport)
                        {
                            ModelState.AddModelError(reportItem.Key, reportItem.Value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("GeneralError", "Не удалось подключиться к сервису, попробуйте позже");
                }

                if (ModelState.IsValid)
                {
                    // сохраняем
                }
            }

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
            foreach (IReferenceSchedule schedule in serviceRepo.GetReferenceByBaseId(baseId))
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
                    lastBaseId = schedule.BaseId.Value;
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
