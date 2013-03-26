using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ugoria.URBD.WebControl.Helpers
{
    public static class HtmlButton
    {
        public static MvcHtmlString Button(this HtmlHelper helper, string text,
                                           IDictionary<string, object> htmlAttributes)
        {
            var builder = new TagBuilder("button");
            builder.InnerHtml = text;
            builder.MergeAttributes(htmlAttributes);
            return MvcHtmlString.Create(builder.ToString());
        }
    }
}

