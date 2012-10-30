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
            });});
    </script>
    <h1>
        Сервис
        <%=ViewData["service_name"] %>
        (<%=ViewData["service_address"] %>)</h1>
    <div id="tabs">
        <%Html.RenderPartial("ServiceBaseMenu"); %>
        <div class="content">
            <h3>
                Общие настройки сервиса</h3>
            <%Html.BeginForm(); %>
            <%=Html.Hidden("Address", ViewData["service_address"])%>
            <table class="configuration">
                <tr>
                    <td class="param-name">
                        <%=Html.Label("Name","Имя сервиса: ") %>
                    </td>
                    <td>
                        <%=Html.TextBox("Name", ViewData["service_name"])%>
                    </td>
                </tr>
                <tr>
                    <td class="param-name">
                        <%=Html.Label("Path1c", "Путь до исполнительного файла 1С: ")%>
                    </td>
                    <td>
                        <%=Html.TextBox("Path1c", ViewData["service_path1c"])%>
                    </td>
                </tr>
            </table>
            <input type="submit" value="Сохранить" />
            <%Html.EndForm(); %>
        </div>
    </div>
</asp:Content>
