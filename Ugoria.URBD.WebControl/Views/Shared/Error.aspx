<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<System.Web.Mvc.HandleErrorInfo>" %>

<asp:Content ID="errorTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Error
</asp:Content>
<asp:Content ID="errorContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        Произошла ошибка. Подробности ниже.
    </h2>
    <%=Html.TextArea("error", ViewData.Model.Exception.ToString(), new { style="width: 100%; height: 700"})%>
</asp:Content>
