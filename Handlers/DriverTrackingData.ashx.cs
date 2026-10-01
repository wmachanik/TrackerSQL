using System;
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
    /// Today's drivers, positions, stops left and ETAs for Tools/DriverTracking.aspx (JSON, refreshed every minute).
    /// Administrators, and users with "Show driver tracking" on Administration → User Information.
    /// </summary>
    public class DriverTrackingData : IHttpHandler, IReadOnlySessionState
    {
        public bool IsReusable => true;

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
                var board = new DriverTrackingManager().Board();
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
