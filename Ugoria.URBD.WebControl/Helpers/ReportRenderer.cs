using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MvcContrib.UI.Grid;
using Ugoria.URBD.WebControl.ViewModels;

namespace Ugoria.URBD.WebControl.Helpers
{
    public class ReportRenderer : HtmlTableGridRenderer<ReportViewModel>
    {
        protected override bool ShouldRenderHeader()
        {
            return true;
        }
    }

    public class LogRenderer : HtmlTableGridRenderer<ServiceLogViewModel>
    {
        protected override bool ShouldRenderHeader()
        {
            return true;
        }
    }
}