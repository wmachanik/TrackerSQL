using System;
using System.Globalization;
using System.Security.Principal;
using System.Web;
using System.Web.SessionState;
using Newtonsoft.Json;
using TrackerSQL.Api;
using TrackerSQL.Classes;
using TrackerSQL.Managers;

namespace TrackerSQL.Handlers
{
    /// <summary>
    /// Drivers, positions, stops and ETAs for Tools/DriverTracking.aspx (JSON). Optional query <c>date=yyyy-MM-dd</c> (default today).
    /// Administrators, and users with "Show driver tracking" on Administration → User Information.
    /// </summary>
    public class DriverTrackingData : IHttpHandler, IRequiresSessionState
    {
        public bool IsReusable => true;

        /// <summary>Parses <c>yyyy-MM-dd</c>, <c>yyyy/MM/dd</c>, or other invariant date strings from the query string.</summary>
        public static bool TryParseQueryDate(string text, out DateTime day)
        {
            day = default;
            if (string.IsNullOrWhiteSpace(text))
                return false;
            text = text.Trim();
            string[] formats =
            {
                "yyyy-MM-dd", "yyyy/MM/dd", "yyyy-M-d", "yyyy/M/d",
                "dd-MM-yyyy", "dd/MM/yyyy", "d-M-yyyy", "d/M/yyyy"
            };
            if (DateTime.TryParseExact(text, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out day))
            {
                day = day.Date;
                return true;
            }
            if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out day))
            {
                day = day.Date;
                return true;
            }
            return false;
        }

        public static bool MaySee(IPrincipal user)
        {
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
                return false;
            if (user.IsInRole("Administrators") || user.IsInRole("Admin"))
                return true;
            return UserPreferencesHelper.GetCurrentPreferences().DriverTracking;
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.Response.Cache.SetNoStore();
            if (context.User?.Identity == null || !context.User.Identity.IsAuthenticated)
            {
                context.Response.StatusCode = 401;
                return;
            }
            if (!MaySee(context.User))
            {
                context.Response.StatusCode = 403;
                return;
            }

            try
            {
                if (string.Equals(context.Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(context.Request.QueryString["action"], "hide", StringComparison.OrdinalIgnoreCase))
                {
                    UserPreferencesHelper.SaveCurrentHideTrackingCouriers(context.Request.QueryString["hide"] != "0");
                    context.Response.ContentType = "application/json";
                    context.Response.Write("{\"ok\":true}");
                    return;
                }
                if (string.Equals(context.Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(context.Request.QueryString["action"], "optimize", StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryParseQueryDate(context.Request.QueryString["date"], out DateTime routeDay))
                        routeDay = TimeZoneUtils.Now().Date;
                    int personId = 0;
                    int.TryParse(context.Request.QueryString["person"], out personId);
                    int saved = new MobileDeliveryManager().OptimizeRoute(routeDay, personId, context.User.Identity.Name, out string routeError);
                    context.Response.ContentType = "application/json";
                    if (routeError != null)
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Write(JsonConvert.SerializeObject(new { error = routeError }));
                        return;
                    }
                    context.Response.Write(JsonConvert.SerializeObject(new { ok = true, stops = saved }));
                    return;
                }

                string dateParam = context.Request.QueryString["date"];
                DateTime day;
                if (string.IsNullOrWhiteSpace(dateParam))
                    day = TimeZoneUtils.Now().Date;
                else if (!TryParseQueryDate(dateParam, out day))
                {
                    context.Response.StatusCode = 400;
                    context.Response.ContentType = "application/json";
                    context.Response.Write(JsonConvert.SerializeObject(new { error = "Invalid date \"" + dateParam.Trim() + "\". Use yyyy-MM-dd." }));
                    return;
                }

                var board = new DriverTrackingManager().Board(day);
                context.Response.ContentType = "application/json";
                context.Response.Write(JsonConvert.SerializeObject(board, WebApiConfig.JsonSettings));
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "Driver tracking data failed: " + ex.Message, context.User.Identity.Name);
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                context.Response.Write(JsonConvert.SerializeObject(new { error = "Could not load the drivers: " + ex.Message }));
            }
        }
    }
}
