using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ugoria.URBD.WebControl.Helpers
{
    public enum Align { center, left, right }
    public static class ImageActionLinkClass
    {
        public static MvcHtmlString ImageActionLink(this HtmlHelper htmlHelper, string actionName, string controllerName, object routeValues, string src, string alt)
        {
            UrlHelper urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            TagBuilder linkBuilder = new TagBuilder("a");
            linkBuilder.MergeAttribute("href", urlHelper.Action(actionName, controllerName, routeValues));
            TagBuilder img = new TagBuilder("img");
            img.MergeAttribute("src", src);
            img.MergeAttribute("alt", alt);
            img.MergeAttribute("align", "top");
            linkBuilder.InnerHtml = img.ToString(TagRenderMode.SelfClosing);
            return new MvcHtmlString(linkBuilder.ToString());
        }
    }
}