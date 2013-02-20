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
            var groupColumn = <%=!TableGrouper.group.Equals(ViewData["sort"]) ? !TableGrouper.service.Equals(ViewData["sort"]) ? 2 : 4 : 6 %>;
        $(document).ready(function () {
            $("input:submit, button").button();
            $("#modeset").buttonset();
            $("#withMD").button();

            $("a.edit-button").button({icons: {
                    primary: "ui-icon-wrench"
                },
                text:false
            });

            $("a.interrupt").addClass("ui-state-default-other");
            
            $("a.exchange, a.interrupt")
                .button()
                .click(function() {
                    $.cookie("scroll_position", $(window).scrollTop());
                    if($(this).hasClass("exchange")) {
                        $('#exchange_dialog').dialog('open');
                        $('#exchange_dialog').dialog('option', 'title', "Режим обмена для ИБ: "+$(this).parents("tr").find("td").eq(1).html());
                        $('#exchange_dialog form').attr('action', $(this).attr('href'));
                        //$(this).click();
                        //$('.ui-dialog-titlebar span').html("Режим обмена для ИБ: "+$(this).parents("tr").find("td").eq(1).html());
                    }
                    else if ($(this).hasClass("interrupt")) {                        
                        var base_name=$(this).parents("tr").find("td").eq(1).html();
                        $('#interrupt_confirm').dialog('open');
                        $('#interrupt_confirm form').attr('action', $(this).attr('href'));
                        $('#interrupt_confirm div').html('Отменить операцию обмена для ИБ <b>'+base_name+'</b>?<br/>' +
                                    '<b>Осторожно!</b> Процесс 1С может быть "убит" и потребуется запуск обмена вручную непосредественно на сервере');
                        //$(this).click();
                        //$('.ui-dialog-titlebar span').html("Режим обмена для ИБ: "+$(this).parents("tr").find("td").eq(1).html());
                    }
                    return false;
                })
                .next()
      .button( {
        text: false,
        icons: {
          primary: "ui-icon-triangle-1-s"
        }
      })
      .click( function() {
        var menu = $(this).parent().next().toggle().position({
          my: "right top",
          at: "right bottom",
          of: this
        });
        $(document).one("click", function() {
          menu.hide();
        });
        return false;
      })
    .parent()
      .buttonset()
    .next()
      .hide()
      .menu();

      $("a#component_menu")
      .button( {
        text: false,
        icons: {
          primary: "ui-icon-triangle-1-s"
        }
      })
      .click( function() {
        var menu = $(this).parent().next().toggle().position({
          my: "right top",
          at: "right bottom",
          of: this
        });
        $(document).one("click", function() {
          menu.hide();
        });
        return false;
      })
    .parent()
    .next()
      .hide()
      .menu()
      .parent();
      
      $("ul.func_menu li a").click(function (event, ui) {
       var location = $(this).attr("href");
       window.location = location;
       });
  //$("ul li a").click(function() {alert("xaxaxa");return true;});

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
        
            var oTable = $('#data').dataTable({
                "bJQueryUI": true,
                "sPaginationType": "full_numbers",
                "aaSorting": [],
                "bLengthChange": false,
                "bPaginate": false,        
                "aoColumnDefs": [
                        { "bSearchable": false, "bVisible": false, "aTargets": [ groupColumn,2 ] },
                        { "bSearchable": false, "bSortable":false, "aTargets": [ 7,9,10,11 ] },
                        { "sType": "date-eu", "aTargets": [ 5 ] }
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
                    var groupNum = 0;
                    var selectoName = "";
                    for (var i = 0; i < nTrs.length; i++) {
                        var iDisplayIndex = oSettings._iDisplayStart + i;
                        var sGroup = oSettings.aoData[oSettings.aiDisplay[iDisplayIndex]]._aData[groupColumn];
                        
                          if (sGroup != sLastGroup) {
                            groupNum++;

                            selectoName = "group_"+groupColumn+"_"+groupNum;
                            var cookie = $.cookie(selectoName);
                            var nGroup = document.createElement('tr');
                            var nCell = document.createElement('th');
                            var aLink = $(document.createElement('a')).attr("href", "#"+selectoName).addClass("group_toggle").html(cookie!="none" && cookie!=null?"свернуть":"развернуть");
                            $(nCell).addClass("group").html(sGroup).append(" (").append(aLink).append(")").attr("colspan", iColspan);
                            $(nGroup).wrapInner(nCell);
                            $(nTrs[i]).before(nGroup);
                            sLastGroup = sGroup;                            
                        }
                        $(nTrs[i]).addClass(selectoName).toggle(cookie!="none" && cookie!=null);
                    }
                }
            });
            new FixedHeader(oTable);

            $("a.group_toggle").live("click",function(){
                var groupName = $(this).attr("href").substring(1);
                var display = $("table#data tr."+groupName).toggle().css("display");
                $.cookie(groupName, display);
                $(this).html(display=="none"?"развернуть":"свернуть");
                return false;
            });

            if($.cookie("scroll_position")!=null)
                $("html").scrollTop($.cookie("scroll_position"));
            $.cookie("scroll_position", null);
        });
    </script>
    <div id="exchange_dialog" title="Режим обмена">
        <%Html.BeginForm("Exchange", "Main", FormMethod.Get); %>
        <div id="modeset">
            <%=Html.RadioFromEnum("mode", ModeType.Normal, new Dictionary<string, object>()) %>
        </div>
        <%Html.EndForm(); %>
    </div>
    <div id="interrupt_confirm" title="Подтверждение отмены операции" style="display: none">
        <%Html.BeginForm("Interrupt", "Main", FormMethod.Get); %>
        <div id="dialog-confirm">
            <b>Осторожно!</b> Процесс 1С может быть "убит" и потребуется запуск обмена вручную
            непосредественно на сервере
        </div>
        <%Html.EndForm(); %>
    </div>    
    <h2 class="component">
        Обмен пакетами</h2><span><a href="#" id="component_menu">Компоненты</a></span><ul class="func_menu"><li><%=Html.ActionLink("Обновление расширенных директорий", "ExtDirectories", "Main")%></li></ul><br />
    Группировать по <b>
        <%if (!TableGrouper.filial.Equals(ViewData["sort"]))
          {%><%= Html.ActionLink("филиалу", "Index", new { sort = TableGrouper.filial })%><%}
          else
          {%>филиалу<%} %></b>, <b>
                          <%if (!TableGrouper.service.Equals(ViewData["sort"]))
                            {%><%= Html.ActionLink("сервису", "Index", new { sort = TableGrouper.service })%><%}
                            else
                            {%>сервису<%} %></b>, <b>
                          <%if (!TableGrouper.group.Equals(ViewData["sort"]))
                            {%><%= Html.ActionLink("группе", "Index", new { sort = TableGrouper.group })%><%}
                            else
                            {%>группе<%} %></b>
    <%=Html.Grid<IBaseReportView>("bases").Columns(column =>
{
    column.For(b => b.BaseId).HeaderAttributes(width=>"2%").Named("#");
    column.For(b => b.BaseName).HeaderAttributes(width => "8%").Named("Имя ИБ");
    column.For(b => b.ParentBaseName).HeaderAttributes(width => "8%").Named("Имя родительской ИБ");
    column.For(b => b.MDRelease).HeaderAttributes(width => "5%").Named("Релиз");
    column.Custom(b => String.Format("{0} ({1})", b.ServiceAddress, b.ServiceName)).HeaderAttributes(width => "8%").Named("Адрес сервиса");
    column.For(b => b.DateComplete).HeaderAttributes(width => "8%").Named("Дата последнего обмена").Format("{0:dd.MM.yyyy HH:mm:ss}");
    column.For(b => b.GroupName).Encode(false).Named("Группа");
    column.Custom(b => "").Attributes(new Func<GridRowViewData<IBaseReportView>, Dictionary<string, object>>(item=> new Dictionary<string, object>(){{"class",
    "Critical".Equals(item.Item.Status)
    ?"critical"
        :"Fail".Equals(item.Item.Status)
            ? "fail"
            : "Warning".Equals(item.Item.Status)
                ? "warning"
                : "Success".Equals(item.Item.Status)
                    ? "success"
                    : "Busy".Equals(item.Item.Status)
                        ? "busy"
                        : "Interrupt".Equals(item.Item.Status)
                            ?"interrupt"
                            :"blank"}})).HeaderAttributes(width => "5");
    column.For(b => b.Message).HeaderAttributes(width => "23%").Named("Отчёт");
    column.Custom(b => string.Join("<br/>", b.Packets.Where(p=>p.Packet.Type[0]==(char)PacketType.Load).Select(c => string.Format("<b>{0}</b> ({1:0.00} Kb) - {2:dd.MM.yyyy HH:mm:ss}", c.Packet.FileName, c.Size / 1024f, c.CreatedDate)))).Named("Последние загруженные пакеты").HeaderAttributes(width => "20%");
    column.Custom(b => string.Join("<br/>", b.Packets.Where(p => p.Packet.Type[0] == (char)PacketType.Unload).Select(c => string.Format("<b>{0}</b> ({1:0.00} Kb) - {2:dd.MM.yyyy HH:mm:ss}", c.Packet.FileName, c.Size / 1024f, c.CreatedDate)))).Named("Последние выгруженные пакеты").HeaderAttributes(width => "20%");
    column.Custom(b => "<div style='white-space: nowrap'>" + ("Busy".Equals(b.Status) ?
        Html.ActionLink("Отмена", "Interrupt", new { id = b.BaseId }, new { @class = "interrupt", id = "func" }) :
        Html.ActionLink("Обмен", "Exchange", new { id = b.BaseId }, new { @class = "exchange", id = "func" })) + Html.ActionLink("Menu", "#") + "</div><ul class=\"func_menu\"><li>" + Html.ActionLink("Параметры ИБ", "Edit", "Base", new { id = b.BaseId }, new object()) + "</li><li>" + Html.ActionLink("Параметры сервиса", "Edit", "Service", new { id = b.ServiceId }, new object()) + "</li><li>" + Html.ActionLink("Логи сервиса", "Logs", "Service", new { id = b.ServiceId }, new object()) + "</li><li><a href=\"" + Url.Action("Edit", "Base", new { id = b.BaseId }) + "#exchange\">Отчёты обмена</a></li></ul>").HeaderAttributes(width => "8%");
    //column.For(b => 1).HeaderAttributes(width => "2%");
}).Attributes(id => "data", width => "100%")/*.RenderUsing(new BaseReportRenderer<IBaseReportView>())*/%><br />
    <table style="border: 0px;">
        <tr>
            <td colspan="12">
                Легенда:
            </td>
        </tr>
        <tr>
            <td class="success" width="10">
            </td>
            <td>
                - Успешно
            </td>
            <td class="fail" width="10">
            </td>
            <td>
                - Сбой
            </td>
            <td class="critical" width="10">
            </td>
            <td>
                - Серьезный сбой
            </td>
            <td class="warning" width="10">
            </td>
            <td>
                - Предупреждение
            </td>
            <td class="busy" width="10">
            </td>
            <td>
                - Идет процесс
            </td>
            <td class="interrupt" width="10">
            </td>
            <td>
                - Прервано
            </td>
        </tr>
    </table>
</asp:Content>
