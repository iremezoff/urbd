<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<%@ Import Namespace="Ugoria.URBD.WebControl.Models" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    УРБД 2.0: Сервис
    <%=ViewData["service_name"] %>
    (<%=ViewData["service_address"] %>)
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

<h1>
        Сервис
        <%=ViewData["service_name"] %>
        (<%=ViewData["service_address"] %>)</h1>
        <div id="tabs">
        <%Html.RenderPartial("ServiceBaseMenu"); %>
        <div class="content">
        <h3>Логи сервиса</h3>
            <%foreach (IServiceLog log in ((IEnumerable<IServiceLog>)ViewData["logs"]))
              {%>
            <%=log.Text %><br />
            <%} %>
        </div>
        </div>

</asp:Content>
