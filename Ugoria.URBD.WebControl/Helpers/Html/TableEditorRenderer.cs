using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MvcContrib.UI.Grid;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Mvc.Html;

namespace Ugoria.URBD.WebControl.Helpers.Html
{
    public class TableEditorRenderer:HtmlTableGridRenderer<string>
    {   HtmlHelper helper;
        public TableEditorRenderer() {
             helper = new HtmlHelper(new ViewContext(Context.Controller.ControllerContext, Context.View, Context.ViewData, Context.TempData, Context.Writer), new ViewPage());
        }

        protected override void RenderHeadEnd()
        {
            RenderText("<th></th>");
            base.RenderHeadEnd();            
        }

        protected override void RenderRowEnd()
        {
            string link = helper.ActionLink("Удалить", "Delete", new {  }, new { @class="delete-row"}).ToHtmlString();
            RenderText("<td><a href=\"#\">"+link+"</td>");
            base.RenderRowEnd();            
        }

        protected override void RenderBodyStart()
        {
           
            //GridModel.Columns
            /*<tr class="new-row">
                            <%=Html.Hidden(String.Format("base.ScheduleExchangeList[0].ScheduleId"),"")%>
                            <td>
                                <%=Html.Hidden(String.Format("base.ScheduleExchangeList[0].Time"), "00:00")%>
                                <div class="time">00:00</div>
                            </td>
                            <td>
                                <%=Html.Hidden(String.Format("base.ScheduleExchangeList[0].Mode"), "Passive")%>
                                <div class="mode">
                                    Passive</div>
                            </td>
                            <td>
                                <%=Html.ActionLink("Удалить", "Delete", new {  }, new { @class="delete-row"})%>
                            </td>
                        </tr>*/
            base.RenderBodyStart();
        }
    }
}