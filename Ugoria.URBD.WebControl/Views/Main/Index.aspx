<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<%@ Import Namespace="MvcContrib.UI.Grid" %>
<%@ Import Namespace="Ugoria.URBD.WebControl.Models" %>
<%@ Import Namespace="Ugoria.URBD.WebControl.Helpers" %>
<%@ Import Namespace="Ugoria.URBD.WebControl.Controllers" %>
<%@ Import Namespace="Ugoria.URBD.Contracts.Services" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Управление распределенными базами данных
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <script type="text/javascript">
        var groupColumn = <%=((string)ViewData["sort"])=="service"?3:5 %>;
        $(document).ready(function () {
            $("input:submit, button").button();
            $("#modeset").buttonset();
            $("#withMD").button();

            $("a.edit-button").button({icons: {
                    primary: "ui-icon-wrench"
                },
                text:false
            });

            $("#exchange_dialog" ).dialog({
                autoOpen:false,
			    resizable: false,
                width: 370,
			    modal: true,
			    buttons: {
			    	"Выполнить": function() {
                        $('#exchange_dialog form').submit();
			    		$(this).dialog("close");                   
			    	},
			    	"Отмена": function() {
			    		$( this ).dialog( "close" );
			    	}
			    }
		    });

            $('a.exchange')
                .button()
                .click(function(){
                    $('#exchange_dialog').dialog('open');
                    $('#exchange_dialog').dialog('option', 'title', "Режим обмена для ИБ: "+$(this).parents("tr").find("td").eq(1).html());
                    $('#exchange_dialog form').attr('action', $(this).attr('href'));
                    //$(this).click();
                    //$('.ui-dialog-titlebar span').html("Режим обмена для ИБ: "+$(this).parents("tr").find("td").eq(1).html());
                    return false;
                })
                .next()
                .button();
            $("a.interrupt")
                .button()
                .addClass("ui-state-default-other");

            $('a.exchange').click(function () { return false; });
            var oTable = $('#data').dataTable({
                "bJQueryUI": true,
                "sPaginationType": "full_numbers",
                "aaSorting": [[groupColumn, "desc"]],
                "bLengthChange": false,
                "bPaginate": false,        
                "aoColumnDefs": [
                        { "bSearchable": false, "bVisible": false, "aTargets": [ groupColumn ] },
                        { "bSearchable": false, "bSortable":false, "aTargets": [ 7,8,9 ] }
                    ],
                "oLanguage": {
                    "sInfo": "Всего информационных баз: _TOTAL_ ",
                    "sSearch": "Поиск: "
                },
                "oColVis": {
                    "sAlign": "left"
                },
                "fnDrawCallback": function (oSettings) {
                    if (oSettings.aiDisplay.length == 0) {
                        return;
                    }

                    var nTrs = $('#data tbody tr');
                    var iColspan = nTrs[0].getElementsByTagName('td').length;
                    var sLastGroup = "";
                    for (var i = 0; i < nTrs.length; i++) {
                        var iDisplayIndex = oSettings._iDisplayStart + i;
                        var sGroup = oSettings.aoData[oSettings.aiDisplay[iDisplayIndex]]._aData[groupColumn];
                        if (sGroup != sLastGroup) {
                            var nGroup = document.createElement('tr');
                            var nCell = document.createElement('th');
                            nCell.colSpan = iColspan;
                            nCell.className = "group";
                            nCell.innerHTML = sGroup;
                            nGroup.appendChild(nCell);
                            nTrs[i].parentNode.insertBefore(nGroup, nTrs[i]);
                            sLastGroup = sGroup;
                        }
                    }
                }
            });
            new FixedHeader(oTable);
        });
    </script>
    <div id="exchange_dialog" title="Режим обмена">
        <%Html.BeginForm("Exchange", "Main", FormMethod.Get); %>
        <div id="modeset">
            <%=Html.RadioFromEnum("mode", ModeType.Normal, new Dictionary<string, object>()) %>
        </div>
        <%Html.EndForm(); %>
    </div>
    <%=Html.Grid<IBaseReportView>("bases").Columns(column =>
{
    column.For(b => b.BaseId).HeaderAttributes(width=>"2%").Named("#");
    column.For(b => b.BaseName).HeaderAttributes(width => "8%").Named("Имя ИБ");
    column.For(b => b.MDRelease).HeaderAttributes(width => "5%").Named("Релиз");
    column.For(b => b.ServiceAddress).Named("Адрес сервиса").HeaderAttributes(width => "8%");
    column.For(b => b.DateComplete).Named("Дата последнего обмена").HeaderAttributes(width => "7%");
    column.Custom(b => b.GroupName + " " + Html.ActionLink("Редактировать", "Edit", "Group", new { id = b.GroupId }, new { @class = "edit-button" }) /*Html.ImageActionLink("Edit", "Group", new { id = b.GroupId }, "Content/images/edit.png", "Редактировать")*/).Encode(false).Named("Группа");
    column.For(b => b.Status).Named("Статус").Attributes(new Func<GridRowViewData<IBaseReportView>, IDictionary<string, object>>(item =>
    {
        string @class = "blank";
        switch (item.Item.Status)
        {
            case "ExchangeFail":
            case "ExtFormsFail": @class = "fail"; break;
            case "ExchangeWarning": @class = "warning"; break;
            case "ExchangeSuccess":
            case "ExtFormsSuccess": @class = "success"; break;
            case "Busy": @class = "busy"; break;
            case "Interrupt": @class = "interrupt"; break;
        }
        return new Dictionary<string, object>() { { "class", @class } };
    })).HeaderAttributes(width => "10%");
    column.For(b => b.Message).Named("Сообщение отчета").HeaderAttributes(width => "29%");
    column.Custom(b => string.Join("<br/>", b.LoadPackets.ToList().ConvertAll<string>(c => string.Format("<b>{0}</b> ({1:0.00} Kb) - {2:dd.MM.yyyy HH:mm:ss}", c.Filename, c.Size / 1024f, c.DateCreated)))).Named("Последние загруженные пакеты").HeaderAttributes(width => "12%");
    column.Custom(b => string.Join("<br/>", b.UnloadPackets.ToList().ConvertAll<string>(c => string.Format("<b>{0}</b> ({1:0.00} Kb) - {2:dd.MM.yyyy HH:mm:ss}", c.Filename, c.Size / 1024f, c.DateCreated)))).Named("Последние выгруженные пакеты").HeaderAttributes(width => "12%");
    column.Custom(b => ("Busy".Equals(b.Status) ?
        Html.ActionLink("Отмена", "Interrupt", new { id = b.BaseId }, new { @class = "interrupt" }).ToHtmlString() :
        Html.ActionLink("Обмен", "Exchange", new { id = b.BaseId }, new { @class = "exchange" }).ToHtmlString()) + String.Format("<a href=\"{0}#base{1}\" class=\"edit-button\">Редактировать</a>",Url.Action("Edit", "Service", new {id=b.ServiceId}), b.BaseId)).HeaderAttributes(width => "5%");
    //column.For(b => 1).HeaderAttributes(width => "2%");
}).Attributes(id => "data", width => "100%")/*.RenderUsing(new BaseReportRenderer<IBaseReportView>())*/%>
</asp:Content>
