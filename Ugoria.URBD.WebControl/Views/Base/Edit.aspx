<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<%@ Import Namespace="Ugoria.URBD.WebControl.Models" %>
<%@ Import Namespace="Ugoria.URBD.Contracts.Services" %>
<%@ Import Namespace="Ugoria.URBD.WebControl.Helpers" %>
<%@ Import Namespace="Ugoria.URBD.WebControl.ViewModels" %>
<%@ Import Namespace="MvcContrib.UI.Grid" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    УРБД 2.0: Сервис
    <%=ViewData["service_name"] %>
    (<%=ViewData["service_address"] %>)
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%IBase baseVM = (IBase)ViewData["base"]; %>
    <script language="javascript" type="text/javascript">
        $(document).ready(function () {

 
        $.datepicker.setDefaults(
	        $.extend($.datepicker.regional["ru"])
	  );

            $("input.date").datepicker({
            defaultDate: null
            }).mask("99.99.9999").val($.datepicker.formatDate(
	'dd.mm.yy',
	new Date()
));

            $("table.reports").dataTable({
            "bJQueryUI": true,
            "bLengthChange": false,
             "aaSorting": [[0, "desc"]],
             "bDestroy": true,
            "bPaginate": false,
            "bFilter": false,
            "aoColumnDefs": [
                        { "bSearchable": false, "bSortable":false, "aTargets": [ 2 ] }
                    ],
                "oLanguage": {
                    "sUrl": "<%: Url.Content("~/Scripts/ru_RU.txt") %>"
                },
            });


            $('input:submit').button();
            $('input.withMD').button();
            $('div.modeset').buttonset().find("input").button("option", {disabled:true});

            $("table.schedule-packets a.edit-button").button({ icons: {
                primary: "ui-icon-wrench"
                },
                text:false
            });

            $("table.schedule-exchange a.edit-button").button({ icons: {
                primary: "ui-icon-wrench"
            },
                text: false
            }).toggle(function() {
                var parentTr = $(this).parent().parent();
                var label = $(parentTr).find("td.time label").hide();
                $(parentTr).find("td.time input").show().mask("99:99").css("width",$(label).css("width")).css("font-size", $(label).css("font-size"));
                $(parentTr).find("td.withMD input.withMD").button("option", {disabled: false});                
                $(parentTr).find("td.mode div.modeset").buttonset().find("input").button("option", {disabled:false});
                $(this).button({icons: {primary: "ui-icon-check"}});                
                return false;
            },
            function() {
                $(this).button({icons: {primary: "ui-icon-wrench"}});
                var parentTr = $(this).parent().parent();
                $(parentTr).find("td.time input").hide();
                $(parentTr).find("td.time label").show();
                $(parentTr).find("td.mode div.modeset input").button("option", {disabled: true}).find("input[checked='checked']");
                alert($(parentTr).find("td.mode div.modeset input[checked]").attr("value"));
                $(parentTr).find("td.mode input").attr("value", 1);
                $(parentTr).find("td.withMD input.withMD").button("option", {disabled: true});            
            });            
            
            $('div#chart_wrap').css('display', 'none');
            $('a.schedule_show').click(function () {
                var wrap = $('div#chart_wrap');
                if ($(wrap).parent()[0] === $(this).parent()[0]) {
                    $(wrap).toggle();
                }
                else $(wrap).insertAfter($(this)).show();
                $('div#chart_wrap select#scale').change();
                return false;
            });           
            
            $('div#chart_wrap select#scale').change(function () {
                var hashArr=/\d+/.exec(window.location.hash);
                var link = '<%=Url.Action("BarData", new {id=RouteData.Values["id"]}) %>?scale='+ $(this).val();                
                swfobject.embedSWF("<%: Url.Content("~/Content/open-flash-chart.swf") %>", "schedule_chart", "100%", "200",
                    "9.0.0", "expressInstall.swf", 
                    {"data-file": link}
                 );
            });

            $('div#ref_chart_wrap').css('display', 'none');
            $('a.reference_show').click(function () {
                var wrap = $('div#ref_chart_wrap');
                if ($(wrap).parent()[0] === $(this).parent()[0]) {
                    $(wrap).toggle();
                }
                else $(wrap).insertAfter($(this)).show();
                $('div#ref_chart_wrap select#scaleRef').change();
                return false;
            });           
            
            $('div#ref_chart_wrap select#scaleRef').change(function () {
                var link = '<%=Url.Action("RefBarData", new {id=RouteData.Values["id"]}) %>?scale='+ $(this).val();                
                swfobject.embedSWF("<%: Url.Content("~/Content/open-flash-chart.swf") %>", "reference_chart", "100%", "200",
                    "9.0.0", "expressInstall.swf", 
                    {"data-file": link}
                 );
            });

            $("div#tabs").tabs();

            swfobject.enableUriEncoding(); //turns on encodeURIComponent
            swfobject.enableUriEncoding(true);
            swfobject.embedSWF("<%: Url.Content("~/Content/open-flash-chart.swf") %>", "schedule_chart", "100%", "200", "9.0.0", "expressInstall.swf", { "data-file": "<%=Url.Action("BarData", "Base", new {id=RouteData.Values["id"],scale=30}) %>" });    
            swfobject.embedSWF("<%: Url.Content("~/Content/open-flash-chart.swf") %>", "reference_chart", "100%", "200", "9.0.0", "expressInstall.swf", { "data-file": "<%=Url.Action("RefBarData", "Base", new {id=RouteData.Values["id"],scale=30}) %>" });    
        });
    </script>
    <h1>
        Сервис
        <%=ViewData["service_name"] %>
        (<%=ViewData["service_address"] %>)</h1>
    <script type="text/javascript" src="<%: Url.Content("~/Scripts/swfobject.js") %>"></script>
    <%Html.RenderPartial("ServiceBaseMenu"); %>
    <div class="content">
        <h3>
            <%=ViewBag.Base.Name %></h3>
        <%=Html.ValidationMessage("GeneralError") %>
        <div id="tabs">
            <ul>
                <li><a href="#properties">Параметры</a></li>
                <li><a href="#report">Отчёты обмена</a></li>
                <li><a href="#extforms">Отчёты ExtForms</a></li>
            </ul>
            <div id="properties">
                <h4>
                    Основные параметры</h4>
                <%Html.BeginForm(); %>
                <%=Html.Hidden("base.ServiceId", baseVM.ServiceId)%>
                <%=Html.Hidden("base.ServiceAddress", ViewData["service_address"])%>
                <%=Html.Hidden("base.BaseId", baseVM.BaseId)%>
                <table class="configuration">
                    <tr>
                        <td class="param-name">
                            <%=Html.Label("Name","Имя ИБ: ") %>
                        </td>
                        <td>
                            <%=Html.TextBox("base.Name", baseVM.Name)%>
                        </td>
                        <td>
                        </td>
                    </tr>
                    <tr>
                        <td class="param-name">
                            <%=Html.Label("base.Path","Путь к директории ИБ: ") %>
                        </td>
                        <td>
                            <%=Html.TextBox("base.Path", baseVM.Path)%>
                        </td>
                        <td>
                            <%=Html.ValidationMessage("BasePath")%>
                        </td>
                    </tr>
                    <tr>
                        <td class="param-name">
                            <%=Html.Label("base.Username", "Имя пользователя: ")%>
                        </td>
                        <td>
                            <%=Html.TextBox("base.Username", baseVM.Username)%>
                        </td>
                        <td>
                        </td>
                    </tr>
                    <tr>
                        <td class="param-name">
                            <%=Html.Label("base.Password","Пароль: ") %>
                        </td>
                        <td>
                            <%=Html.TextBox("base.Password", baseVM.Password)%>
                        </td>
                        <td>
                        </td>
                    </tr>
                </table>
                <h4>
                    Пакеты</h4>
                <%=Html.Grid(baseVM.PacketList).Columns(c =>
            {
                int p = 0;
                c.Custom(r => Html.Hidden("base.PacketList[" + p + "].PacketId", r.PacketId) + "" + Html.Hidden("base.PacketList[" + p + "].FileName", r.FileName) + r.FileName).Named("Имя файла").HeaderAttributes(@class => "ui-state-default");
                c.Custom(r => Html.Hidden("base.PacketList[" + p + "].Type", r.Type) + "<div class=\"modeset\">" + Html.RadioButton(String.Format("base.PacketList[{0}].Type", p), (char)PacketType.Load, r.Type==PacketType.Load, new { id = "load" + r.PacketId, disabled = "disabled" }).ToString() + Html.Label("load" + r.PacketId, "Load") +
                            Html.RadioButton(String.Format("base.PacketList[{0}].Type", p++), (char)PacketType.Unload, r.Type==PacketType.Unload, new { id = "unload" + r.PacketId, disabled = "disabled" }) + Html.Label("unload" + r.PacketId, "Unload") + "</div>").Named("Тип пакета").HeaderAttributes(@class => "ui-state-default");
                c.Custom(r => Html.ActionLink("Редактировать", "Edit", new { }, new { @class = "edit-button packet" })).HeaderAttributes(@class => "ui-state-default");
            }).Attributes(@class => "schedule-packets")%>
                <h4>
                    Связанные ИБ</h4>
                <a href="#" class="reference_show">Посмотреть расписание связанных ИБ</a>
                <div id="ref_chart_wrap">
                    Временной интервал:
                    <%=Html.DropDownList("scaleRef", new List<SelectListItem>() { new SelectListItem { Value = "1", Text="1 мин." }, 
                    new SelectListItem { Value = "10", Text="10 мин." },
                    new SelectListItem { Value = "20", Text="20 мин." },
                    new SelectListItem { Value = "30", Text="30 мин.", Selected=true },
                    new SelectListItem { Value = "60", Text="60 мин." }            
                }, 
                new { style = "display: inline-block" })%>
                    <div id="reference_chart">
                    </div>
                </div>
                <h4>
                    Расписание обмена</h4>
                <table class="schedule-exchange">
                    <thead>
                        <tr>
                            <th class="ui-state-default">
                                Время
                            </th>
                            <th class="ui-state-default">
                                Режим
                            </th>
                            <th class="ui-state-default">
                            </th>
                        </tr>
                    </thead>
                    <%foreach (IScheduleExchange schedule in baseVM.ScheduleExchangeList)
                      {%>
                    <tr>
                        <td class="time">
                            <%=Html.TextBox(String.Format("time[{0}]", schedule.ScheduleId), schedule.Time, new { size=5, style="display: none;",})%>
                            <%=Html.Label(String.Format("time[{0}]", schedule.ScheduleId), schedule.Time)%>
                        </td>
                        <td class="mode">
                            <%=Html.Hidden(String.Format("mode[{0}]", schedule.ScheduleId), schedule.Mode)%>
                            <div class="modeset">
                                <%=Html.RadioFromEnum(String.Format("radio_mode[{0}]", schedule.ScheduleId), schedule.Mode, new Dictionary<string, object>() { })%>
                            </div>
                        </td>
                        <td>
                            <%=Html.ActionLink("Редактировать", "Edit", new {}, new {@class="edit-button schedule-exch"})%>
                        </td>
                    </tr>
                    <%
                      } %>
                </table>
                <a href="#" class="schedule_show">Посмотреть распределение обменов всего сервиса</a>
                <div id="chart_wrap">
                    Временной интервал:
                    <%=Html.DropDownList("scale", new List<SelectListItem>() { new SelectListItem { Value = "1", Text="1 мин." }, 
                    new SelectListItem { Value = "10", Text="10 мин." },
                    new SelectListItem { Value = "20", Text="20 мин." },
                    new SelectListItem { Value = "30", Text="30 мин.", Selected=true },
                    new SelectListItem { Value = "60", Text="60 мин." }            
                }, 
                new { style = "display: inline-block" })%>
                    <div id="schedule_chart">
                    </div>
                </div>
                <h4>
                    Расписание обновления ExtForms</h4>
                <table>
                    <thead>
                        <tr>
                            <th class="ui-state-default">
                                Время
                            </th>
                            <th class="ui-state-default">
                            </th>
                        </tr>
                    </thead>
                    <%foreach (IScheduleExtForms schedule in baseVM.ScheduleExtFormsList)
                      {%>
                    <tr>
                        <td>
                            <%=schedule.Time%>
                        </td>
                        <td>
                            <%=Html.ActionLink("Редактировать", "Edit", new {}, new {@class="edit-button schedule-ext"})%>
                        </td>
                    </tr>
                    <%
                      } %>
                </table>
                <br />
                <input type="submit" value="Сохранить" />
                <%Html.EndForm(); %>
            </div>
            <div id="report">
                <h4>
                    Отчеты</h4>
                    <div id="date_period">
                С даты  <input type="text" name="date_start" class="date" /> по: <input type="text" name="date_end" class="date" /> <input type="submit" name="view_reports" value="Показать" /></div>
                <%=Html.Grid<ReportViewModel>("reports").Columns(c => {
    c.For(r => r.CommandDate).Named("Дата команды").HeaderAttributes(width=>"10%");
    c.For(r => r.CompleteDate).Named("Дата выполнения").HeaderAttributes(width => "10%"); ;
    c.Custom(b => "").Attributes(new Func<GridRowViewData<ReportViewModel>, Dictionary<string, object>>(item => new Dictionary<string, object>(){{"class",item.Item.Status.Equals("ExchangeFail") 
        ? "fail"
        : item.Item.Status.Equals("ExchangeWarning") 
            ? "warning"
            : item.Item.Status.Equals("ExchangeSuccess") 
                ? "success"
                : item.Item.Status.Equals("Busy") 
                    ? "busy"
                    : item.Item.Status.Equals("Interrupt")
                        ?"interrupt"
                        :"blank"}})).HeaderAttributes(width => 5);
    c.For(r => r.Message).Named("Сообщение").HeaderAttributes(width => "30%"); ;
    c.Custom(r => string.Join("<br/>", r.PacketList.Where(p=>p.Packet.Type[0]==(char)PacketType.Load).ToList().ConvertAll<string>(pl => string.Format("<b>{0}</b> ({1:0.00} Kb) - {2:dd.MM.yyyy HH:mm:ss}", pl.Packet.FileName, pl.Size / 1024f, pl.CreatedDate)))).Named("Загруженные пакеты").HeaderAttributes(width=>"20%");;
    c.Custom(r => string.Join("<br/>", r.PacketList.Where(p => p.Packet.Type[0] == (char)PacketType.Load).ToList().ConvertAll<string>(pl => string.Format("<b>{0}</b> ({1:0.00} Kb) - {2:dd.MM.yyyy HH:mm:ss}", pl.Packet.FileName, pl.Size / 1024f, pl.CreatedDate)))).Named("Выгруженные пакеты").HeaderAttributes(width => "20%"); ;
    c.For(r => r.User).Named("Пользователь").HeaderAttributes(width => "10%"); ;
}).Attributes(@class => "reports").RenderUsing(new ReportRenderer())%>
            </div>
            <div id="extforms">
                :(</div>
        </div>
    </div>
</asp:Content>
