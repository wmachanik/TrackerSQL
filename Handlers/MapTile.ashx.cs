using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Web;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Handlers
{
    /// <summary>
    /// Azure Maps map tiles for the GPS popup (Render v2 Get Map Tile). The browser asks this page, so the
    /// subscription key never leaves the server. Signed-in users only; capped per day (GpsSettingsRepository.AzureTileDailyLimit).
    /// </summary>
    public class MapTile : IHttpHandler
    {
        private const string TileUrl = "https://atlas.microsoft.com/map/tile?api-version=2024-04-01&tilesetId={0}&zoom={1}&x={2}&y={3}&tileSize=256&language=en-US";
        private const int TimeoutMs = 10000;
        private const int MaxZoom = 19;

        private static DateTime _lastFailureLogged = DateTime.MinValue;
        private static readonly object FailureLogLock = new object();

        public bool IsReusable => true;

        public void ProcessRequest(HttpContext context)
        {
            if (context.User == null || context.User.Identity == null || !context.User.Identity.IsAuthenticated)
            {
                context.Response.StatusCode = 401;
                return;
            }

            string tileset = TilesetFor(context.Request.QueryString["style"]);
            if (tileset == null
                || !int.TryParse(context.Request.QueryString["z"], out int z)
                || !int.TryParse(context.Request.QueryString["x"], out int x)
                || !int.TryParse(context.Request.QueryString["y"], out int y)
                || z < 0 || z > MaxZoom || x < 0 || y < 0 || x >= (1 << z) || y >= (1 << z))
            {
                context.Response.StatusCode = 400;
                return;
            }

            GpsSettings settings;
            var repo = new GpsSettingsRepository();
            try
            {
                settings = repo.Get();
            }
            catch (Exception ex)
            {
                LogFailure("settings could not be read: " + ex.Message, context);
                context.Response.StatusCode = 503;
                return;
            }
            if (!settings.UseAzureMap)
            {
                context.Response.StatusCode = 404;
                return;
            }

            try
            {
                if (!repo.TryCountCall(GpsSettingsRepository.AzureTileService, GpsSettingsRepository.AzureTileDailyLimit))
                {
                    LogFailure("today's cap of " + GpsSettingsRepository.AzureTileDailyLimit + " tiles is used up", context);
                    context.Response.StatusCode = 429;
                    return;
                }
            }
            catch (Exception ex)
            {
                LogFailure("tile count failed: " + ex.Message, context);
            }

            string url = string.Format(CultureInfo.InvariantCulture, TileUrl, tileset, z, x, y);
            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.Headers["subscription-key"] = settings.AzureMapsKey.Trim();
                request.Timeout = TimeoutMs;
                request.ReadWriteTimeout = TimeoutMs;
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var buffer = new MemoryStream())
                {
                    stream.CopyTo(buffer);
                    context.Response.ContentType = string.IsNullOrEmpty(response.ContentType) ? "image/png" : response.ContentType;
                    context.Response.Cache.SetCacheability(HttpCacheability.Private);
                    context.Response.Cache.SetMaxAge(TimeSpan.FromDays(1));
                    context.Response.BinaryWrite(buffer.ToArray());
                }
            }
            catch (WebException ex)
            {
                var http = ex.Response as HttpWebResponse;
                int status = http != null ? (int)http.StatusCode : 0;
                if (status == 404 || status == 204)
                {
                    context.Response.StatusCode = 404;
                    return;
                }
                LogFailure(status == 401 || status == 403
                    ? "Azure Maps refused the key (HTTP " + status + ") - check the Azure Maps key in System Preferences"
                    : "Azure Maps tile failed (" + (status > 0 ? "HTTP " + status : ex.Status.ToString()) + ")", context);
                context.Response.StatusCode = 502;
            }
            catch (Exception ex)
            {
                LogFailure("Azure Maps tile failed: " + ex.Message, context);
                context.Response.StatusCode = 502;
            }
        }

        private static string TilesetFor(string style)
        {
            switch ((style ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "road": return "microsoft.base.road";
                case "satellite": return "microsoft.imagery";
                case "labels": return "microsoft.base.hybrid.road";
                default: return null;
            }
        }

        /// <summary>A broken key fails every tile on the map; log at most once a minute.</summary>
        private static void LogFailure(string message, HttpContext context)
        {
            lock (FailureLogLock)
            {
                if ((DateTime.Now - _lastFailureLogged).TotalSeconds < 60)
                    return;
                _lastFailureLogged = DateTime.Now;
            }
            AppLogger.WriteLog(SystemConstants.LogTypes.System, "GPS map tiles: " + message, context?.User?.Identity?.Name);
        }
    }
}
