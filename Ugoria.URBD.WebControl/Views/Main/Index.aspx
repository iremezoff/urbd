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

            $("a.interrupt")
                .button()
                .addClass("ui-state-default-other")
                .click(function(){
                    var base_name=$(this).parents("tr").find("td").eq(1).html();
                    $('#interrupt_confirm').dialog('open');
                    $('#interrupt_confirm form').attr('action', $(this).attr('href'));
                    $('#interrupt_confirm div').html('Отменить операцию обмена для ИБ <b>'+base_name+'</b>?<br/>'+$('#interrupt_confirm div').html());
                    //$(this).click();
                    //$('.ui-dialog-titlebar span').html("Режим обмена для ИБ: "+$(this).parents("tr").find("td").eq(1).html());
                    return false;
                });
                
		$( "#interrupt_confirm" ).dialog({
            autoOpen:false,
			resizable: false,
            width:370,
			modal: true,
			buttons: {
				"Да": function() {
                $('#interrupt_confirm form').submit();
					$( this ).dialog( "close" );
				},
				"Отмена": function() {
					$( this ).dialog( "close" );
				}
			}
		});
        
            $('a.exchange').click(function () { return false; });
            var oTable = $('#data').dataTable({
                "bJQueryUI": true,
                "sPaginationType": "full_numbers",
                "aaSorting": [[groupColumn, "desc"]],
                "bLengthChange": false,
                "bPaginate": false,        
                "aoColumnDefs": [
                        { "bSearchable": false, "bVisible": false, "aTargets": [ groupColumn ] },
                        { "bSearchable": false, "bSortable":false, "aTargets": [ 6,8,9,10 ] }
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
    <div id="interrupt_confirm" title="Подтверждение отмены операции">
        <%Html.BeginForm("Interrupt", "Main", FormMethod.Get); %>
        <div id="dialog-confirm">
            <b>Осторожно!</b> Процесс 1С может быть "убит" и потребуется запуск обмена вручную
            непосредественно на сервере
        </div>
        <%Html.EndForm(); %>
    </div>
    <%=Html.Grid<IBaseReportView>("bases").Columns(column =>
{
    column.For(b => b.BaseId).HeaderAttributes(width=>"2%").Named("#");
    column.For(b => b.BaseName).HeaderAttributes(width => "8%").Named("Имя ИБ");
    column.For(b => b.MDRelease).HeaderAttributes(width => "5%").Named("Релиз");
    column.For(b => b.ServiceAddress).HeaderAttributes(width => "8%").Named("Адрес сервиса");
    column.For(b => b.DateComplete).HeaderAttributes(width => "8%").Named("Дата последнего обмена");
    column.For(b => b.GroupName).Encode(false).Named("Группа");
    column.Custom(b => "").Attributes(new Func<GridRowViewData<IBaseReportView>, Dictionary<string, object>>(item=> new Dictionary<string, object>(){{"class",item.Item.Status.Equals("ExchangeFail") 
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
    column.For(b => b.Message).HeaderAttributes(width => "23%").Named("Отчёт");
    column.Custom(b => string.Join("<br/>", b.LoadPackets.ToList().ConvertAll<string>(c => string.Format("<b>{0}</b> ({1:0.00} Kb) - {2:dd.MM.yyyy HH:mm:ss}", c.Filename, c.Size / 1024f, c.DateCreated)))).Named("Последние загруженные пакеты").HeaderAttributes(width => "19%");
    column.Custom(b => string.Join("<br/>", b.UnloadPackets.ToList().ConvertAll<string>(c => string.Format("<b>{0}</b> ({1:0.00} Kb) - {2:dd.MM.yyyy HH:mm:ss}", c.Filename, c.Size / 1024f, c.DateCreated)))).Named("Последние выгруженные пакеты").HeaderAttributes(width => "19%");
    column.Custom(b => ("Busy".Equals(b.Status) ?
        Html.ActionLink("Отмена", "Interrupt", new { id = b.BaseId }, new { @class = "interrupt" }).ToHtmlString() :
        Html.ActionLink("Обмен", "Exchange", new { id = b.BaseId }, new { @class = "exchange" }).ToHtmlString()) + Html.ActionLink("Редактировать","Edit", "Base", new { id = b.BaseId }, new { @class = "edit-button" })).HeaderAttributes(width => "5%");
    //column.For(b => 1).HeaderAttributes(width => "2%");
}).Attributes(id => "data", width => "100%")/*.RenderUsing(new BaseReportRenderer<IBaseReportView>())*/%><br />
<table style="border: 0px;">
<tr><td colspan="10">Легенда:</td></tr>
<tr><td class="success" width="10"></td><td>- Успешно</td>
<td class="fail" width="10"></td><td>- Сбой</td>
<td class="warning" width="10"></td><td>- Предупреждение</td>
<td class="busy" width="10"></td><td>- Идет процесс</td>
<td class="interrupt" width="10"></td><td>- Прервано</td></tr></table>
</asp:Content>
