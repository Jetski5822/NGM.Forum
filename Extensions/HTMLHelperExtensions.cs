using System;
using System.Web.Mvc;
using NGM.Forum.Models;
using Orchard.ContentManagement;
using Orchard.UI.Navigation;
using Orchard.Utility.Extensions;
using Orchard.Mvc.Html;
using Orchard.Mvc.Extensions;

namespace NGM.Forum.Extensions
{
    public static class HTMLHelperExtensions
    {
        public static DateTime ConvertToDisplayTime(this HtmlHelper html, DateTime dateTimeUtc)
        {
            //var workContext = html.ViewContext.RequestContext.GetWorkContext();

            //var timeZone = workContext.CurrentTimeZone;

          //  return TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, timeZone);
            return new DateTime();
        }

        public static MvcHtmlString ConvertToDisplayTime(this HtmlHelper html, DateTime dateTimeUtc, string format)
        {
            var dateTimeDisplay = html.ConvertToDisplayTime(dateTimeUtc);

            return MvcHtmlString.Create(dateTimeDisplay.ToString(format));
        }
    }
}