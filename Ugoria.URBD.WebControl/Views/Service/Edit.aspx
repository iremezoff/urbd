<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<%@ Import Namespace="Ugoria.URBD.WebControl.Models" %>
<%@ Import Namespace="Ugoria.URBD.Contracts.Services" %>
<%@ Import Namespace="Ugoria.URBD.WebControl.Helpers" %>
<%@ Import Namespace="MvcContrib.UI.Grid" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    УРБД 2.0: Сервис
    <%=ViewData["service_name"] %>
    (<%=ViewData["service_address"] %>)
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <script language="javascript" type="text/javascript">
        $(document).ready(function () {
            var hash = window.location.hash;            
            if(hash == undefined || hash.length == 0) hash = '#service-main';

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

            $('div#tabs > div:not(#sidebar)').attr('class', 'content-empty');

            $('div#tabs div#sidebar a:not(.empty)').click(function () {
                $('div#tabs div#sidebar a:not(.empty)').removeAttr('class');
                $(this).attr('class', 'selected');
                $('div#tabs > div:not(#sidebar)').attr('class', 'content-empty');
                $('div#tabs > div' + $(this).attr('href')).attr('class', 'content');
            });

            $('div#tabs div#sidebar a[href="'+hash+'"]').click();
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
                var link = '<%=Url.Action("BarData", new {id=RouteData.Values["id"]}) %>?current=' +hashArr[0] +'&scale='+ $(this).val();                
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
                var hashArr=/\d+/.exec(window.location.hash);
                var link = '<%=Url.Action("RefBarData", new {id=RouteData.Values["id"]}) %>?current=' +hashArr[0] +'&scale='+ $(this).val();                
                swfobject.embedSWF("<%: Url.Content("~/Content/open-flash-chart.swf") %>", "reference_chart", "100%", "200",
                    "9.0.0", "expressInstall.swf", 
                    {"data-file": link}
                 );
            });

            swfobject.enableUriEncoding(); //turns on encodeURIComponent
            swfobject.enableUriEncoding(true);
            swfobject.embedSWF("<%: Url.Content("~/Content/open-flash-chart.swf") %>", "schedule_chart", "100%", "200", "9.0.0", "expressInstall.swf", { "data-file": "<%=Url.Action("BarData", "Service", new {id=RouteData.Values["id"],scale=30}) %>" });    
            swfobject.embedSWF("<%: Url.Content("~/Content/open-flash-chart.swf") %>", "reference_chart", "100%", "200", "9.0.0", "expressInstall.swf", { "data-file": "<%=Url.Action("RefBarData", "Service", new {id=RouteData.Values["id"],scale=30}) %>" });    
        });
    </script>
    <h1>
        Сервис
        <%=ViewData["service_name"] %>
        (<%=ViewData["service_address"] %>)</h1>
    <script type="text/javascript" src="<%: Url.Content("~/Scripts/swfobject.js") %>"></script>
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
    <div id="tabs">
        <div id="sidebar">
            <ul>
                <li><a href="#service-main">Общие настройки сервиса</a></li>
                <li><a href="#logs">Логи сервиса</a></li>
                <li><a class="empty">Информационные базы:</a></li>
                <li>
                    <ul>
                        <%foreach (IBase @base in (IEnumerable<IBase>)ViewData["bases"])
                          {%>
                        <li><a href="#base<%=@base.BaseId %>">&nbsp;&nbsp;
                            <%=@base.Name %></a></li>
                        <%} %>
                    </ul>
                </li>
            </ul>
        </div>
        <div id="service-main">
            <h3>
                Общие настройки сервиса</h3>
            <%Html.BeginForm(); %>
            <%=Html.Hidden("type", "service") %>
            <table class="configuration">
                <tr>
                    <td class="param-name">
                        <%=Html.Label("name","Имя сервиса: ") %>
                    </td>
                    <td>
                        <%=Html.TextBox("name", ViewData["service_name"])%>
                    </td>
                </tr>
                <tr>
                    <td class="param-name">
                        <%=Html.Label("path1c", "Путь до исполнительного файла 1С: ")%>
                    </td>
                    <td>
                        <%=Html.TextBox("path1c", ViewData["service_path1c"])%>
                    </td>
                </tr>
            </table>
            <input type="submit" value="Сохранить" />
            <%Html.EndForm(); %>
        </div>
        <div id="logs">
            <h3>
                Логи сервиса</h3>
            <%foreach (IServiceLog log in ((IEnumerable<IServiceLog>)ViewData["logs"]))
              {%>
            <%=log.Text %><br />
            <%} %>
        </div>
        <%foreach (IBase @base in (IEnumerable<IBase>)ViewData["bases"])
          {%>
        <div id="base<%=@base.BaseId %>">
            <h3>
                <%=@base.Name %></h3>
            <h4>
                Основные параметры</h4>
            <%Html.BeginForm(); %>
            <%=Html.Hidden("type", "base") %>
            <%=Html.Hidden("base", @base.BaseId) %>
            <table class="configuration">
                <tr>
                    <td class="param-name">
                        <%=Html.Label("name","Имя ИБ: ") %>
                    </td>
                    <td>
                        <%=Html.TextBox("name", @base.Name)%>
                    </td>
                </tr>
                <tr>
                    <td class="param-name">
                        <%=Html.Label("name","Путь к директории ИБ: ") %>
                    </td>
                    <td>
                        <%=Html.TextBox("path", @base.Path)%>
                    </td>
                </tr>
                <tr>
                    <td class="param-name">
                        <%=Html.Label("name","Имя пользователя: ") %>
                    </td>
                    <td>
                        <%=Html.TextBox("username", @base.Username)%>
                    </td>
                </tr>
                <tr>
                    <td class="param-name">
                        <%=Html.Label("name","Пароль: ") %>
                    </td>
                    <td>
                        <%=Html.TextBox("password", @base.Password)%>
                    </td>
                </tr>
            </table>
            <h4>
                Пакеты</h4>
            <%=Html.Grid(@base.PacketList).Columns(c=>{
                c.For(r => r.FileName).Named("Имя файла").HeaderAttributes(@class => "ui-state-default");
                c.Custom(r => "<div class=\"modeset\">"+Html.RadioButton(String.Format("type[{0}]", r.PacketId), (char)PacketType.Load, r.Type == PacketType.Load, new { id = "load" + r.PacketId, disabled = "disabled" }).ToString()+Html.Label("load" + r.PacketId, "Load")+
                            Html.RadioButton(String.Format("type[{0}]", r.PacketId), (char)PacketType.Unload, r.Type == PacketType.Unload, new { id = "unload" + r.PacketId, disabled = "disabled" }) + Html.Label("unload" + r.PacketId, "Unload") + "</div>").Named("Тип пакета").HeaderAttributes(@class=>"ui-state-default" );
                c.Custom(r => Html.ActionLink("Редактировать", "Edit", new { }, new { @class = "edit-button packet" })).HeaderAttributes(@class => "ui-state-default");
            }).Attributes(@class => "schedule-packets")%>
            <h4>
                Связанные ИБ</h4>
            <a href="#" class="reference_show">Посмотреть расписание связанных ИБ</a>
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
                            MD
                        </th>
                        <th class="ui-state-default">
                        </th>
                    </tr>
                </thead>
                <%foreach (IScheduleExchange schedule in @base.ScheduleExchangeList)
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
                <%foreach (IScheduleExtForms schedule in @base.ScheduleExtFormsList)
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
        <%} %>
    </div>
</asp:Content>
