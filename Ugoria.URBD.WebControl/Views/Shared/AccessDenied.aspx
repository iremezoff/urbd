<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    AccessDenied
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

<%var model = (HandleErrorInfo)ViewData.Model; %>
<h2><%=model.Exception.Message %></h2>

Текущая учетная запись: <b><%=HttpContext.Current.User.Identity.Name %></b><br />
<br />
По вопросам обращаться по адресу <a href="mailto:remezovis@ugsk.ru">remezovis@ugsk.ru</a>

</asp:Content>
