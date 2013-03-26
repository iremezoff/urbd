using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Web.Mvc.Html;
using Ugoria.URBD.Contracts.Services;

namespace Ugoria.URBD.WebControl.Helpers
{
    public static class RadioButtonEnumClass
    {
        public static MvcHtmlString RadioFromEnum(this HtmlHelper htmlHelper, string name, Enum collect, IDictionary<string, object> htmlAttrubutes)
        {
            StringBuilder strBuilder = new StringBuilder();
            int i = 1;
            string @default = collect.ToString();
            string extName = name.Replace('[','_').Replace(']','_');


            var r = ((PacketType[])Enum.GetValues(collect.GetType())).Select(e=>string.Format("{0}: {1}", (char)e, Enum.GetName(e.GetType(),e)));
            

            foreach (string element in Enum.GetNames(collect.GetType()))
            {
                htmlAttrubutes["id"] = extName + i;
                MvcHtmlString inputString = htmlHelper.RadioButton(name, element, element.Equals(@default), htmlAttrubutes);
                MvcHtmlString labelString = htmlHelper.Label(extName + i++, element);
                strBuilder.Append(inputString.ToString() + labelString.ToString());
                //inputBuilder.MergeAttribute("id", "radio" + i);
                //strBuilder.AppendFormat("<input type=\"radio\" id=\"radio{0}\" name=\"{1}\" value=\"{2}\" {3}/><label for=\"radio{0}\">{2}</label>", i++, name, element, (element.Equals(@default)) ? "checked=\"checked\"" : "");
            }
            return new MvcHtmlString(strBuilder.ToString());
        }

    }
}