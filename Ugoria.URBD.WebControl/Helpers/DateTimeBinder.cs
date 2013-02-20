using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Ugoria.URBD.WebControl.Helpers
{
    public class DateTimeBinder : IModelBinder
    {
        /// <summary>
        /// Matches DateTime in the following format: "0000-00-00T00:00:00.000Z"
        /// </summary>
        private const string DateTimeJSONPattern = @"^(\d{2})\.(\d{2})\.(\d{4})([\s]?(\d{2})[:]?(\d{2})[:]?(\d{2}))?$";

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            ValueProviderResult result = bindingContext.ValueProvider.GetValue(bindingContext.ModelName) ?? new ValueProviderResult(string.Empty, string.Empty, CultureInfo.InvariantCulture);
            Match match = match = Regex.Match(result.AttemptedValue, DateTimeJSONPattern);
            if (!string.IsNullOrEmpty(result.AttemptedValue) && match.Success)
            {
                if (!string.IsNullOrEmpty(match.Groups[4].Value))
                {
                    return new DateTime(
                            int.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture),
                            int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture),
                            int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture),
                            int.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture),
                            int.Parse(match.Groups[6].Value, CultureInfo.InvariantCulture),
                            0, DateTimeKind.Unspecified);
                }
                else
                {
                    return new DateTime(
                        int.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture),
                        int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture),
                        int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture),
                        0, 0, 0,
                        DateTimeKind.Unspecified);
                }
            }
            else
            {
                return DateTime.Now;
            }
        }
    }
}