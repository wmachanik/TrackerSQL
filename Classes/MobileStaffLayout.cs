using System;
using System.Web;
using System.Web.UI.HtmlControls;

namespace TrackerSQL.Classes
{
    /// <summary>
    /// Marks key staff Web Forms pages for phone-first CSS (see Styles/MobileStaff.css).
    /// </summary>
    public static class MobileStaffLayout
    {
        public const string PageClassPrefix = "staff-mobile-page-";

        public static void ApplyBodyClasses(HtmlGenericControl body, HttpRequest request)
        {
            if (body == null || request == null)
                return;

            string key = GetPageKey(request.AppRelativeCurrentExecutionFilePath);
            if (string.IsNullOrEmpty(key))
                return;

            string existing = body.Attributes["class"] ?? string.Empty;
            if (existing.IndexOf("staff-mobile-page", StringComparison.OrdinalIgnoreCase) >= 0)
                return;

            body.Attributes["class"] = (existing + " staff-mobile-page " + PageClassPrefix + key).Trim();
        }

        /// <summary>Short slug used for CSS and tab bar active state.</summary>
        public static string GetPageKey(string appRelativePath)
        {
            if (string.IsNullOrEmpty(appRelativePath))
                return null;

            string path = appRelativePath.Replace('\\', '/');
            if (path.StartsWith("~/", StringComparison.Ordinal))
                path = path.Substring(2);

            switch (path.ToLowerInvariant())
            {
                case "default.aspx":
                    return "home";
                case "pages/deliverysheet.aspx":
                    return "delivery";
                case "pages/orderdetail.aspx":
                    return "order";
                case "pages/contacts.aspx":
                    return "contacts";
                case "pages/contactdetails.aspx":
                    return "contact-detail";
                case "pages/sendcoffeecheckup.aspx":
                    return "checkup";
                default:
                    return null;
            }
        }
    }
}
