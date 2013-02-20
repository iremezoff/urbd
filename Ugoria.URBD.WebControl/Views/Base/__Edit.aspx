﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<%@ Import Namespace="Ugoria.URBD.WebControl.Models" %>
<%@ Import Namespace="Ugoria.URBD.Contracts.Services" %>
<%@ Import Namespace="Ugoria.URBD.WebControl.Helpers" %>
<%@ Import Namespace="Ugoria.URBD.WebControl.ViewModels" %>
<%@ Import Namespace="MvcContrib.UI.Grid" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    ИБ
    <%=ViewBag.Base.Name %>
    (Сервис:
    <%=ViewData["service_name"] %>) - УРБД 2.5
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%BaseViewModel baseVM = (BaseViewModel)ViewData["base"]; %>
    <script language="javascript" type="text/javascript">
        

        function deleteRow(table) {
                $(".delete-row", table).button({ icons: {
                    primary: "ui-icon-closethick"
                },
                text: false
            }).click(function() {
                $(this).closest('tr').remove();
                $(table).trigger("changerows");
                return false;
            });                   
        }

        var extdirs = {<%=string.Join(",", ((IEnumerable<ExtDirectoryView>)ViewData["extdirectories"]).Select(d=>string.Format("\"{0}\":{{local:\"{1}\", ftp: \"{2}\"}}", d.DirId, d.LocalPath, d.FtpPath))) %>};
        var packetTypes = {<%=string.Join(",", ((PacketType[])Enum.GetValues(typeof(PacketType))).Select(e=>string.Format("\"{0}\": \"{1}\"", (char)e, e.ToString()))) %>};
        var modeTypes = {<%=string.Join(",", ((ModeType[])Enum.GetValues(typeof(ModeType))).Select(e=>string.Format("\"{0}\": \"{1}\"", (char)e, e.ToString()))) %>};
        
        $(document).ready(function () {
        
            $.datepicker.setDefaults(
	            $.extend($.datepicker.regional["ru"]));
                $("input.date").datepicker({
                    defaultDate: null
                }).mask("99.99.9999").val($.datepicker.formatDate(
	                'dd.mm.yy',
	                new Date()
                )
            );

            var dataGetterFunc = function(component) {
                var table = $("div#"+component+" table").dataTable();
                table.fnClearTable(0);                
                var dateStart = $("div#"+component+" input[name=date_start]").val();
                var dateEnd = $("div#"+component+" input[name=date_end]").val();
                $.getJSON("<%=Url.Action("ReportData", new {id = RouteData.Values["id"]}) %>", { "dateStart": dateStart, "dateEnd": dateEnd, "component": component}, 
                    function(data) {
                        $.each(data, function(i,rowData) {
                            table.fnAddData(rowData);
                        });
                    });
                }

            $("div.notify_group").buttonset();
            
            var reportTable = $("table.reports").dataTable({
                "bJQueryUI": true,        
                "fnRowCallback": function(nRow, aData, iDisplayIndex ) {
                    cssClass = aData[2] == "Critical"
                        ? "critical"
                        : aData[2] == "Fail"
                            ? "fail"
                            : aData[2] == "Warning" 
                                ? "warning"
                                : aData[2] == "Success" 
                                    ? "success"
                                    : aData[2] == "Busy" 
                                        ? "busy"
                                        : aData[2] == "Interrupt"
                                            ?"interrupt"
                                            :"blank";    
                    $('td:eq(2)', nRow).addClass(cssClass).html("");
                },
                "fnInitComplete": function(oSettings, json) {
                    var component = $(this).parent().parent().attr("id");
                    dataGetterFunc(component);
                },
                "aaSorting": [[0, "desc"]],
                "bLengthChange": false,
                "aaSorting": [[0, "desc"]],
                "bDestroy": true,
                "bPaginate": false,
                "bFilter": false,
                "bAutoWidth": false,
                //"bSort": false,
                "aoColumnDefs": [
                    { "sType": "date-eu", "aTargets": [ 0, 1 ] },
                    { "bSearchable": false, "bSortable":false, "aTargets": [ 2 ] }
                ],
                "oLanguage": {
                    "sUrl": "<%: Url.Content("~/Scripts/ru_RU.txt") %>"
                }
            });

            $("input.view_reports").click(function() {
                var component = $(this).parent().parent().attr("id");                 
                dataGetterFunc(component);
            });

            $('input:submit').button();
            
            <%if (ViewBag.allow_change)
                              { %>
            $.editable.addInputType('radio', {
                element : function(settings, original) {
                    var hidden = $('<input type="hidden"/>');
                    $(this).append(hidden);
                    return(hidden);
                },           
                content : function(data, settings, original) {
                    if (String == data.constructor) {      
                        eval ('var json = ' + data);
                    } else {                    
                        var json = data;
                    }
                    var div = $("<div>");                    
                    var form = this;
                    $(this).append(div);
                    
                    for (var key in json) {
                        if (!json.hasOwnProperty(key)) {
                            continue;
                        }
                        if ('checked' == key) {
                            continue;
                        }
                        var option = $('<input type="radio"/>').attr("name", $(original).attr("id")).attr("id", $(original).attr("id")+"_"+key).val(key).append(json[key]).one("click",function() {                        
                            $(':hidden', form).val($(this).val());  
                            form.submit();
                            return false;
                            });
                        var label = $('<label>').attr('for', $(original).attr("id")+"_"+key).val(key).append(json[key]);
                        $('div', this).append(option).append(label);    
                    }
                    $('div', this).children("input").each(function() {
                        if (//$(this).val() == data.checked || 
                            $(this).text() == $.trim(original.revert)) {
                                $(this).attr('checked', 'checked');
                        }
                    });                    
                    $(form).buttonset(); 
                }
            });

            $(".basename, .username, .password, .basepath").editable(function(value, settings) {
			        $(this).prev().attr("value", value);
			        return(value);
		        }, {
                    type: 'text',
                    placeholder: 'Нажмите для ввода'
                }).css("cursor", "pointer").each(function(el) { $(this).html($(this).html().trim());});

                $(".service").editable(function(value, settings) {
			        $(this).prev().attr("value", value);
			        return(settings.data[value]);
		        }, {
                    type: 'select',
                    data: $.extend({<%=string.Join(",", ((IEnumerable<IService>)ViewData["services"]).Select(s=>string.Format("\"{0}\": \"{1} ({2})\"", s.ServiceId, s.Name, s.Address))) %>}, <%=@baseVM.ServiceId %>)
                })

            $("table#scheduleextdirs tbody").on("changerows", function() {
                var trs = $("tr:not(.new-row)", this);
                $("input.ScheduleId:hidden", trs).attr("name", function(index) { return "base.ScheduleExtDirectoriesList["+index+"].ScheduleId"});
                $("input.Time:hidden", trs).attr("name", function(index) { return "base.ScheduleExtDirectoriesList["+index+"].Time"});

                $(".time").editable(function(value, settings) {
			        $(this).prev().attr("value", value);
			        return(value);
		        }).on("click", function() {            
                    $(':text',this).timeEntry({
                        show24Hours: true,
                        spinnerImage: ''
                    });
                }).css("cursor","pointer");
                deleteRow(this);
            }).trigger("changerows");


            $("table#scheduleexchange tbody").on("changerows", function() {
                var trs = $("tr:not(.new-row)", this);
                $("input.ScheduleId:hidden", trs).attr("name", function(index) { return "base.ScheduleExchangeList["+index+"].ScheduleId"});
                $("input.ModeType:hidden", trs).attr("name", function(index) { return "base.ScheduleExchangeList["+index+"].Mode"});
                $("input.Time:hidden", trs).attr("name", function(index) { return "base.ScheduleExchangeList["+index+"].Time"});
                
                $(".time").editable(function(value, settings) {
			        $(this).prev().attr("value", value);
			        return(value);
		        }).on("click", function() {            
                    $(':text',this).timeEntry({
                        show24Hours: true,
                        spinnerImage: ''
                    });
                }).css("cursor","pointer");                       
            
                $(".mode").editable(function(value, settings) {
                    $(this).prev().attr("value", value); 
			        return(settings.data[value]); 
                    },  {
                        type: 'radio',
                        data: modeTypes
                    }
                ).attr("id", function(arr) {
                    return "exchsched"+arr;
                    }).css("cursor","pointer");
                deleteRow(this);
            }).trigger("changerows");

            $("table#packets tbody").on("changerows", function() {
                var trs = $("tr:not(.new-row)", this);
                $("input.PacketId:hidden", trs).attr("name", function(index) { return "base.PacketList["+index+"].PacketId"});
                $("input.FileName:hidden", trs).attr("name", function(index) { return "base.PacketList["+index+"].FileName"});
                $("input.Type:hidden", trs).attr("name", function(index) { return "base.PacketList["+index+"].Type"});

                $(".packettype", this).editable(function(value, settings) {
                    $(this).prev().attr("value", value); 
			        return(settings.data[value]); 
                    },  {
                        type: 'radio',
                        data: packetTypes
                }).attr("id", function(arr) {return "packet"+arr;}).css("cursor","pointer");

                $(".packetname").editable(function(value, settings) {
                    $(this).prev().attr("value", value); 
			        return(value); 
                },  {
                    type: 'text',
                    placeholder: 'Нажмите для ввода'
                }).css("cursor","pointer").each(function() {$(this).html($(this).html().trim());});
                deleteRow(this);
            }).trigger("changerows");

            $("table#extdirs tbody").on("changerows", function() {
                var trs = $("tr:not(.new-row)", this);
                $("input.ExtDirectoryId:hidden", trs).attr("name", function(index) { return "base.ExtDirectoriesList["+index+"].ExtDirectoryId"});
                $("input.LocalPath:hidden", trs).attr("name", function(index) { return "base.ExtDirectoriesList["+index+"].LocalPath"});
                $("input.FtpPath:hidden", trs).attr("name", function(index) { return "base.ExtDirectoriesList["+index+"].FtpPath"});
                
                $(".localpath", this).editable(function(value, settings) {
                    $(this).prev().attr("value", value);
                    $(this).removeClass("localpath").off("click.editable").css("cursor", "default");
			        return(settings.data[value]); 
                },  {
                    type: 'select',
                    data: {<%=string.Join(",", ((IEnumerable<ExtDirectoryView>)ViewData["extdirectories"]).Select(d=>string.Format("\"{0}\":\"{1}\"", d.DirId, d.LocalPath))) %>},
                    onblur: 'ignore',
                    submit: 'OK'
                }).css("cursor","pointer").click();
                function selectDir(e) {                    
                    var tr = $(this).closest('tr');
                    $('td.ftp', tr).html(extdirs[$(e.target).val()].ftp);
                    $('input.LocalPath:hidden', tr).val(extdirs[$(e.target).val()].local);
                    $('input.FtpPath:hidden', tr).val(extdirs[$(e.target).val()].ftp);
                }
                $(".localpath select").change(selectDir).change();
                deleteRow(this);
            }).trigger("changerows");

            $("tr.new-row input:hidden").attr("disabled", "");

            $("a.addrow").click(function() {
                var table = $(this).prev();
                var newRow = $("tr.new-row", table).clone(false).removeClass("new-row"); 
                $(":disabled", newRow).removeAttr("disabled");
                $("tbody", table).append(newRow).trigger("changerows");
                return false;
            }).button({ icons: {
                primary: "ui-icon-plusthick"
                },
                text: false
            });
            <%} %>
            
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
    <%//=string.Join(",", ((IEnumerable<ExtDirectoryView>)ViewData["extdirectories"]).Select(d=>string.Format("\"{0}\":\"{1}\"", d.DirId, d.LocalPath))) %>
    <h1>
        Сервис
        <%=ViewData["service_name"] %>
        (<%=ViewData["service_address"] %>)</h1>
    <script type="text/javascript" src="<%: Url.Content("~/Scripts/swfobject.js") %>"></script>
    <%Html.RenderPartial("ServiceBaseMenu"); %>
    <div class="content">
        <h3>
            <%=ViewBag.Base.Name %></h3>
        <div id="tabs">
            <ul>
                <li><a href="#properties">Параметры</a></li>
                <li><a href="#notification">Оповещения</a></li>
                <li><a href="#exchange">Отчёты обмена</a></li>
                <li><a href="#extdirectories">Отчёты ExtDirectories</a></li>
            </ul>
            <div id="properties">
                <h4>
                    Основные параметры</h4>
                <%=Html.ValidationSummary(true,"Параметры не сохранены. Имеются ошибки") %>
                <%=Html.ValidationMessage("ServiceError") %>
                <%Html.BeginForm(); %>
                <%=Html.Hidden("base.ServiceAddress", ViewData["service_address"])%>
                <%=Html.Hidden("base.BaseId", baseVM.BaseId)%>
                <%=Html.Hidden("base.Name", baseVM.Name) %>
                <table class="configuration">
                <tr>
                        <td class="param-name">
                            <%=Html.Label("base.ServiceId","Сервис: ") %>
                        </td>
                        <%=Html.Hidden("base.ServiceId", baseVM.ServiceId)%>
                        <td class="service">
                            <%=ViewData["service_name"]%> (<%=ViewData["service_address"] %>)
                        </td>
                        <td>
                            <%=Html.ValidationMessage("base.Path")%>
                        </td>
                    </tr>
                    <tr>
                        <td class="param-name">
                            <%=Html.Label("base.Path","Путь к директории ИБ: ") %>
                        </td>
                        <%=Html.Hidden("base.Path", baseVM.Path)%>
                        <td class="basepath">
                            <%=baseVM.Path%>
                        </td>
                        <td>
                            <%=Html.ValidationMessage("base.Path")%>
                        </td>
                    </tr>
                    <tr>
                        <td class="param-name">
                            <%=Html.Label("base.Username", "Имя пользователя: ")%>
                        </td>
                        <%=Html.Hidden("base.Username", baseVM.Username)%>
                        <td class="username">
                            <%=baseVM.Username%>
                        </td>
                        <td>
                            <%=Html.ValidationMessage("base.Username")%>
                        </td>
                    </tr>
                    <tr>
                        <td class="param-name">
                            <%=Html.Label("base.Password","Пароль: ") %>
                        </td>
                        <%=Html.Hidden("base.Password", baseVM.Password)%>
                        <td class="password">
                            <%=baseVM.Password%>
                        </td>
                        <td>
                            <%=Html.ValidationMessage("base.Password")%>
                        </td>
                    </tr>
                </table>
                <h4>
                    Пакеты</h4>
                <%=Html.ValidationMessage("base.PacketList.FileName")%>
                <table id="packets">
                    <thead>
                        <tr>
                            <th class="ui-state-default">
                                Имя файла
                            </th>
                            <th class="ui-state-default">
                                Тип
                            </th>
                            <%if (ViewBag.allow_change)
                              { %>
                            <th class="ui-state-default">
                            </th>
                            <%} %>
                        </tr>
                    </thead>
                    <tbody>
                        <tr class="new-row">
                            <%=Html.Hidden("base.PacketList[0].PacketId", string.Empty, new { @class = "PacketId" })%>
                            <%=Html.Hidden("base.PacketList[0].FileName", string.Empty, new { @class = "FileName" })%>
                            <td class="packetname">
                            </td>
                            <%=Html.Hidden(String.Format("base.PacketList[0].Type"), "L", new { @class = "Type" })%>
                            <td class="packettype">
                                Load
                            </td>
                            <%if (ViewBag.allow_change)
                              { %>
                            <td>
                                <%=Html.ActionLink("Удалить", "Delete", new {  }, new { @class="delete-row"})%>
                            </td>
                            <%} %>
                        </tr>
                        <%foreach (PacketViewModel packet in baseVM.PacketList)
                          {%>
                        <tr>
                            <%=Html.Hidden("base.PacketList[0].PacketId", packet.PacketId, new { @class="PacketId"})%>
                            <%=Html.Hidden("base.PacketList[0].FileName", packet.FileName, new { @class="FileName"})%>
                            <td class="packetname">
                                <%=packet.FileName%>
                            </td>
                            <%=Html.Hidden("base.PacketList[0].Type", packet.Type, new { @class = "Type" })%>
                            <td class="packettype">
                                <%=(PacketType)packet.Type[0]%>
                            </td>
                            <%if (ViewBag.allow_change)
                              { %>
                            <td>
                                <%=Html.ActionLink("Удалить", "Delete", new { id = packet.PacketId }, new { @class="delete-row"})%>
                            </td>
                            <%} %>
                        </tr>
                        <%
                          } %>
                    </tbody>
                </table>
                <%if (ViewBag.allow_change)
                  { %>
                <a href="#" class="addrow">добавить</a>
                <%} %>
                <br />
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
                <table id="scheduleexchange">
                    <thead>
                        <tr>
                            <th class="ui-state-default">
                                Время
                            </th>
                            <th class="ui-state-default">
                                Режим
                            </th>
                            <%if (ViewBag.allow_change)
                              { %>
                            <th class="ui-state-default">
                            </th>
                            <%} %>
                        </tr>
                    </thead>
                    <tbody>
                        <tr class="new-row">
                            <%=Html.Hidden(String.Format("base.ScheduleExchangeList[0].ScheduleId"), "", new { @class = "ScheduleId" })%>
                            <%=Html.Hidden(String.Format("base.ScheduleExchangeList[0].Time"), "00:00", new { @class = "Time" })%>
                            <td class="time">
                                00:00
                            </td>
                            <%=Html.Hidden(String.Format("base.ScheduleExchangeList[0].Mode"), "P", new { @class = "ModeType" })%>
                            <td class="mode">
                                Passive
                            </td>
                            <%if (ViewBag.allow_change)
                              { %>
                            <td>
                                <%=Html.ActionLink("Удалить", "Delete", new {  }, new { @class="delete-row"})%>
                            </td>
                            <%} %>
                        </tr>
                        <%foreach (ScheduleExchangeViewModel schedule in baseVM.ScheduleExchangeList)
                          {%>
                        <tr>
                            <%=Html.Hidden(String.Format("base.ScheduleExchangeList[{0}].ScheduleId", schedule.ScheduleId), schedule.ScheduleId, new { @class = "ScheduleId" })%>
                            <%=Html.Hidden(String.Format("base.ScheduleExchangeList[{0}].Time", schedule.ScheduleId), schedule.Time, new { @class = "Time" })%>
                            <td class="time">
                                <%=schedule.Time%>
                            </td>
                            <%=Html.Hidden(String.Format("base.ScheduleExchangeList[{0}].Mode", schedule.ScheduleId), schedule.Mode, new { @class = "ModeType" })%>
                            <td class="mode">
                                <%=(ModeType)schedule.Mode[0]%>
                            </td>
                            <%if (ViewBag.allow_change)
                              { %>
                            <td>
                                <%=Html.ActionLink("Удалить", "Delete", new { id = schedule.ScheduleId }, new { @class="delete-row"})%>
                            </td>
                            <%} %>
                        </tr>
                        <%
                          } %>
                    </tbody>
                </table>
                <%if (ViewBag.allow_change)
                  { %>
                <a href="#" class="addrow">добавить</a>
                <%} %>
                <br />
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
                    Расширенные директории</h4>
                <table id="extdirs">
                    <thead>
                        <tr>
                            <th class="ui-state-default">
                                Локальный путь
                            </th>
                            <th class="ui-state-default">
                                Путь на FTP
                            </th>
                            <%if (ViewBag.allow_change)
                              { %>
                            <th class="ui-state-default">
                            </th>
                            <%} %>
                        </tr>
                    </thead>
                    <tbody>
                        <tr class="new-row">
                            <%=Html.Hidden("base.ExtDirectoriesList[0].LocalPath", string.Empty, new { @class = "LocalPath" })%>
                            <%=Html.Hidden("base.ExtDirectoriesList[0].FtpPath", string.Empty, new { @class = "FtpPath" })%>
                            <%=Html.Hidden("base.ExtDirectoriesList[0].ExtDirectoryId", string.Empty,new { @class = "ExtDirectoryId" })%>
                            <td class="localpath">
                            </td>
                            <td class="ftp">
                            </td>
                            <%if (ViewBag.allow_change)
                              { %>
                            <td>
                                <%=Html.ActionLink("Удалить", "Delete", new {  }, new { @class = "delete-row" })%>
                            </td>
                            <%} %>
                        </tr>
                        <%foreach (ExtDirectoryViewModel extDirectory in baseVM.ExtDirectoriesList)
                          {%>
                        <tr>
                            <%=Html.Hidden(String.Format("base.ExtDirectoriesList[{0}].LocalPath", extDirectory.ExtDirectoryId), extDirectory.LocalPath, new { @class = "LocalPath" })%>
                            <%=Html.Hidden(String.Format("base.ExtDirectoriesList[{0}].FtpPath", extDirectory.ExtDirectoryId), extDirectory.FtpPath, new { @class = "FtpPath" })%>
                            <%=Html.Hidden(String.Format("base.ExtDirectoriesList[{0}].ExtDirectoryId", extDirectory.ExtDirectoryId), extDirectory.ExtDirectoryId, new { @class = "ExtDirectoryId" })%>
                            <td>
                                <%=extDirectory.LocalPath%>
                            </td>
                            <td class="ftp">
                                <%=extDirectory.FtpPath%>
                            </td>
                            <%if (ViewBag.allow_change)
                              { %>
                            <td>
                                <%=Html.ActionLink("Удалить", "Delete", new { id = extDirectory.ExtDirectoryId }, new { @class = "delete-row" })%>
                            </td>
                            <%} %>
                        </tr>
                        <%
                          } %>
                    </tbody>
                </table>
                <%if (ViewBag.allow_change)
                  { %>
                <a href="#" class="addrow">добавить</a>
                <%} %>
                <h4>
                    Расписание обновления ExtDirectories</h4>
                <table id="scheduleextdirs">
                    <thead>
                        <tr>
                            <th class="ui-state-default">
                                Время
                            </th>
                            <%if (ViewBag.allow_change)
                              { %>
                            <th class="ui-state-default">
                            </th>
                            <%} %>
                        </tr>
                    </thead>
                    <tbody>
                        <tr class="new-row">
                            <%=Html.Hidden(String.Format("base.ScheduleExtDirectoriesList[0].ScheduleId"), "",new { @class = "ScheduleId" })%>
                            <%=Html.Hidden(String.Format("base.ScheduleExtDirectoriesList[0].Time"), "00:00",new { @class = "Time" })%>
                            <td class="time">
                                00:00
                            </td>
                            <td>
                                <%=Html.ActionLink("Удалить", "Delete", new {}, new {@class="delete-row"})%>
                            </td>
                        </tr>
                        <%foreach (ScheduleExtDirectoriesViewModel schedule in baseVM.ScheduleExtDirectoriesList)
                          {%>
                        <tr>
                            <%=Html.Hidden(String.Format("base.ScheduleExtDirectoriesList[{0}].ScheduleId", schedule.ScheduleId), schedule.ScheduleId, new { @class = "ScheduleId" })%>
                            <%=Html.Hidden(String.Format("base.ScheduleExtDirectoriesList[{0}].Time", schedule.Time), schedule.Time, new { @class = "Time" })%>
                            <td class="time">
                                <%=schedule.Time%>
                            </td>
                            <%if (ViewBag.allow_change)
                              { %>
                            <td>
                                <%=Html.ActionLink("Удалить", "Delete", new {}, new {@class="delete-row"})%>
                            </td>
                            <%} %>
                        </tr>
                        <%
                          } %>
                    </tbody>
                </table>
                <%if (ViewBag.allow_change)
                  { %>
                <a href="#" class="addrow">добавить</a>
                <%} %>
                <br />
                <br />
                <input type="submit" value="Сохранить" />
                <%Html.EndForm(); %>
            </div>
            <div id="notification">
                <h4>
                    Настройка оповещений</h4>
                <%Html.BeginForm(new { action="EditNotification", id=RouteData.Values["id"]}); %>
                <%=Html.Grid<ComponentReportStatusView>("report_statuses").Columns(c => {
    int i = 0;
    c.For(r => r.Component.Name).Named("Модуль").HeaderAttributes(@class => "ui-state-default");
    c.Custom(r => Html.Hidden(string.Format("notifies[{0}].NotificationId", i), r.Notification!=null?r.Notification.notification_id:0).ToHtmlString() + Html.Hidden(string.Format("notifies[{0}].ComponentStatusId", i), r.ComponentReportStatusId).ToHtmlString()).Attributes(new Func<GridRowViewData<ComponentReportStatusView>, Dictionary<string, object>>(item => new Dictionary<string, object>(){{"class",
    "Critical".Equals(item.Item.ReportStatus.name)
    ?"critical"
        :"Fail".Equals(item.Item.ReportStatus.name)
            ? "fail"
            : "Warning".Equals(item.Item.ReportStatus.name)
                ? "warning"
                : "Success".Equals(item.Item.ReportStatus.name)
                    ? "success"
                    : "Busy".Equals(item.Item.ReportStatus.name)
                        ? "busy"
                        : "Interrupt".Equals(item.Item.ReportStatus.name)
                            ?"interrupt"
                            :"blank"}})).HeaderAttributes(width => "5", @class=>"ui-state-default");
    c.For(r => r.ReportStatus.Name).HeaderAttributes(@class => "ui-state-default").Named("Статус");
    c.Custom(r => "<div class=\"notify_group\">" + Html.CheckBox(string.Format("notifies[{0}].OnMail", i), (r.Notification != null && r.Notification.on_mail.HasValue && r.Notification.on_mail.Value), new { @class = "notify_checkbox" }).ToHtmlString() + Html.Label(string.Format("notifies[{0}].OnMail", i), "E-mail").ToHtmlString() +
                  Html.CheckBox(string.Format("notifies[{0}].OnPhone", i), (r.Notification != null && r.Notification.on_phone.HasValue && r.Notification.on_phone.Value), new { @class = "notify_checkbox" }) + Html.Label(string.Format("notifies[{0}].OnPhone", i++), "SMS").ToHtmlString() + "</div>").HeaderAttributes(@class => "ui-state-default").Named("Оповещение");
}).Attributes(@class => "notifications")%>
                Подписка на оповещение по статусу "Busy" подразумевает получение сообщений о превышении
                максимального времени ожидания выполнения операции обмена или обновления.<br />
                <input type="submit" value="Сохранить" />
                <%Html.EndForm(); %>
            </div>
            <div id="exchange">
                <h4>
                    Отчеты</h4>
                <div id="date_period_exchange">
                    С даты
                    <input type="text" name="date_start" class="date" />
                    по:
                    <input type="text" name="date_end" class="date" />
                    <input type="submit" name="view_reports" class="view_reports" value="Показать" /></div>
                <%=Html.Grid<ReportViewModel>("reports").Columns(c => {
    c.For(r => "").Named("Дата команды").HeaderAttributes(width=>"10%");
    c.For(r => "").Named("Дата выполнения").HeaderAttributes(width => "10%");
    c.For(b => "").HeaderAttributes(width=>"2");
    c.For(r => "").Named("Сообщение").HeaderAttributes(width => "30%");
    c.Custom(r => "").Named("Загруженные пакеты").HeaderAttributes(width=>"22%");
    c.Custom(r => "").Named("Выгруженные пакеты").HeaderAttributes(width => "22%");
    c.For(r => "").Named("Пользователь").HeaderAttributes(width => "7%");
}).Attributes(@class => "reports").RenderUsing(new ReportRenderer())%>
            </div>
            <div id="extdirectories">
                <h4>
                    Отчеты</h4>
                <div id="date_period_extdirs">
                    С даты
                    <input type="text" name="date_start" class="date" />
                    по:
                    <input type="text" name="date_end" class="date" />
                    <input type="submit" name="view_reports" class="view_reports" value="Показать" /></div>
                <%=Html.Grid<ReportViewModel>("reports").Columns(c => {
    c.For(r => "").Named("Дата команды").HeaderAttributes(width=>"10%");
    c.For(r => "").Named("Дата выполнения").HeaderAttributes(width => "10%");
    c.For(b => "").HeaderAttributes(width=>"2");
    c.For(r => "").Named("Сообщение").HeaderAttributes(width => "30%");
    c.Custom(r => "").Named("Загруженные файлы").HeaderAttributes(width=>"44%");
    c.For(r => "").Named("Пользователь").HeaderAttributes(width => "7%");
}).Attributes(@class => "reports").RenderUsing(new ReportRenderer())%>
            </div>
        </div>
    </div>
</asp:Content>