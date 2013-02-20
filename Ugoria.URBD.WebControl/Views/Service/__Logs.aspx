<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<%@ Import Namespace="Ugoria.URBD.WebControl.Models" %>
<%@ Import Namespace="Ugoria.URBD.WebControl.ViewModels" %>
<%@ Import Namespace="MvcContrib.UI.Grid" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Логи сервиса <%=ViewData["service_name"] %>
    (<%=ViewData["service_address"] %>) - УРБД 2.0
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
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
$('input:submit').button();
            var dataGetterFunc = function() {
                reportTable.fnClearTable(0);
                $.getJSON("<%=Url.Action("LogData", new {id = RouteData.Values["id"]}) %>", { "dateStart": $("input#date_start").val(), "dateEnd": $("input#date_end").val()  }, function(data) {
                //alert(data);
                //reportTable.fnAddData(data);
        $.each(data, function(i,rowData){
       reportTable.fnAddData( rowData );
                });
            }        );}

            var reportTable = $("table#logs").dataTable({
            "bJQueryUI": true,
        "bAutoWidth":false,
        "fnRowCallback": function( nRow, aData, iDisplayIndex ) {
        cssClass = aData[1] == "F"? "fail"
        : aData[1] == "W" 
            ? "warning"
            : aData[1] == "I" 
                ? "success"
                        :"blank";            
                $('td:eq(1)', nRow).addClass(cssClass).html("");
        },
        "fnInitComplete": function(oSettings, json) {
      dataGetterFunc();
    },
        "aaSorting": [[0, "desc"]],
            "bLengthChange": false,
             "aaSorting": [[0, "desc"]],
             "bDestroy": true,
            "bPaginate": false,
            "bFilter": false,
            "aoColumnDefs": [
                        { "bSearchable": false, "bSortable":false, "aTargets": [ 1 ] },
            { "sType": "date-eu", "aTargets": [ 0 ] }                        

                    ],
                "oLanguage": {
                    "sUrl": "<%: Url.Content("~/Scripts/ru_RU.txt") %>"
                }
            });

            $("#date-period").click(dataGetterFunc);
            });
            </script>
<h1>
        Сервис
        <%=ViewData["service_name"] %>
        (<%=ViewData["service_address"] %>)</h1>
        <%Html.RenderPartial("ServiceBaseMenu"); %>
        <div class="content">
        <h3>Логи сервиса</h3>
            <div id="logs">
                <div id="date_period">
                    С даты
                    <input type="text" id="date_start" class="date" />
                    по:
                    <input type="text" id="date_end" class="date" />
                    <input type="submit" name="view_reports" value="Показать" id="date-period" /></div>
                <%=Html.Grid<ServiceLogViewModel>("logs").Columns(c => {
    c.For(r => "").Named("Дата сообщения").HeaderAttributes(width=>"10%");
    c.For(r => "").HeaderAttributes(width => "2");
    c.For(r => "").Named("Сообщение").HeaderAttributes(width => "90%");
}).Attributes(id => "logs").RenderUsing(new Ugoria.URBD.WebControl.Helpers.LogRenderer())%>
            </div>
        </div>
</asp:Content>
