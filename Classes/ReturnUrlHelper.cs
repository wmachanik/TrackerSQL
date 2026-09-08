using System;
using System.Web;
using System.Web.UI;

namespace TrackerSQL.Classes
{
    /// <summary>
    /// Safe local return-URL capture for detail pages (Order / Contact / Repair).
    /// Blocks open redirects; prefers ?ReturnUrl= then referrer then default.
    /// </summary>
    public static class ReturnUrlHelper
    {
        public static void CaptureIfNeeded(
            Page page,
            string sessionKey,
            string defaultAppRelativeUrl,
            string selfPageNameFragment)
        {
            if (page == null || string.IsNullOrWhiteSpace(sessionKey))
                return;

            HttpRequest request = page.Request;
            string qsReturn = request.QueryString["ReturnUrl"];
            if (!string.IsNullOrWhiteSpace(qsReturn))
            {
                string candidate = qsReturn.Trim();
                try
                {
                    candidate = HttpUtility.UrlDecode(candidate) ?? candidate;
                }
                catch
                {
                    // keep raw
                }

                if (TryNormalizeLocal(page, candidate, out string fromQuery))
                {
                    page.Session[sessionKey] = fromQuery;
                    return;
                }
            }

            if (request.UrlReferrer != null)
            {
                string referrer = request.UrlReferrer.ToString();
                bool isSelf = !string.IsNullOrEmpty(selfPageNameFragment)
                    && referrer.IndexOf(selfPageNameFragment, StringComparison.OrdinalIgnoreCase) >= 0;
                if (!isSelf && IsSafe(page, referrer))
                {
                    page.Session[sessionKey] = referrer;
                    return;
                }
            }

            if (page.Session[sessionKey] == null)
                page.Session[sessionKey] = page.ResolveUrl(defaultAppRelativeUrl);
        }

        public static string Get(Page page, string sessionKey, string defaultAppRelativeUrl)
        {
            string url = page?.Session[sessionKey] as string;
            if (string.IsNullOrWhiteSpace(url) || !IsSafe(page, url))
                url = page.ResolveUrl(defaultAppRelativeUrl);
            return url;
        }

        public static bool TryNormalizeLocal(Page page, string candidate, out string normalized)
        {
            normalized = null;
            if (page == null || string.IsNullOrWhiteSpace(candidate))
                return false;

            candidate = candidate.Trim();
            if (candidate.StartsWith("~/") || (candidate.StartsWith("/") && !candidate.StartsWith("//")))
            {
                normalized = page.ResolveUrl(candidate.StartsWith("~/") ? candidate : "~" + candidate);
                return IsSafe(page, normalized);
            }

            if (IsSafe(page, candidate))
            {
                normalized = candidate;
                return true;
            }

            return false;
        }

        public static bool IsSafe(Page page, string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            if (url.StartsWith("~/") || (url.StartsWith("/") && !url.StartsWith("//")))
                return url.IndexOf("://", StringComparison.Ordinal) < 0;

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri absolute))
                return false;

            return page?.Request?.Url != null
                && string.Equals(absolute.Host, page.Request.Url.Host, StringComparison.OrdinalIgnoreCase);
        }
    }
}
