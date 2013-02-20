<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<%@ Import Namespace="Ugoria.URBD.WebControl.ViewModels" %>
<div class="sidebar">
    <ul>
        <li><a href="<%=Url.Action("Edit", "Service", new {id=ViewData["service_id"]}) %>">Общие
            настройки сервиса</a></li>
        <li><a href="<%=Url.Action("Logs", "Service", new {id=ViewData["service_id"]}) %>">Логи сервиса</a></li>
    </ul>
    <br />
    <b>Информационные базы:</b>
    <ul>
        <%foreach (BaseViewModel @base in (IEnumerable<BaseViewModel>)ViewData["bases"])
          {%>
        <li><a href="<%=Url.Action("Edit", "Base", new {id=@base.BaseId}) %>">&nbsp;&nbsp;
            <%=@base.Name %></a></li>
        <%} %>
    </ul>
</div>

