using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Ugoria.URBD.WebControl.Helpers
{
    public static class CheckBoxLabelClass
    {
        public static MvcHtmlString CheckBoxLabel(this HtmlHelper htmlHelper, string name, string label, object htmlAttributes)
        {
            return CheckBoxLabel(htmlHelper, name, label, false, htmlAttributes);
        }

        public static MvcHtmlString CheckBoxLabel(this HtmlHelper htmlHelper, string name, string label, bool isChecked, object htmlAttributes)
        {
            MvcHtmlString inputString = htmlHelper.CheckBox(name, isChecked, htmlAttributes);
            TagBuilder labelBuilder = new TagBuilder("label");
            labelBuilder.InnerHtml = label;
            labelBuilder.MergeAttribute("for", name);
            return new MvcHtmlString(inputString + labelBuilder.ToString());
            // <input type="checkbox" id="check" /><label for="check">Toggle</label>
        }
    }
}