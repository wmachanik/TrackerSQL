using System;
using System.Globalization;
using System.Web;
using System.Web.UI;
using TrackerSQL.Classes;
using TrackerSQL.Handlers;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Tools
{
    /// <summary>Map of drivers for a delivery day; data from Handlers/DriverTrackingData.ashx.</summary>
    public partial class DriverTracking : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            DateTime day = TimeZoneUtils.Now().Date;
            string dateParam = Request.QueryString["date"];
            if (!string.IsNullOrWhiteSpace(dateParam) && DriverTrackingData.TryParseQueryDate(dateParam, out DateTime parsed))
                day = parsed;
            string dayIso = day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            hdnSelectedDay.Value = dayIso;

            if (!DriverTrackingData.MaySee(User))
            {
                pnlBoard.Visible = false;
                lblMessage.Text = "Driver tracking is for administrators and users with \"Show driver tracking\" ticked on Administration &rarr; User Information.";
                lblMessage.CssClass = "status-message status-error";
                lblMessage.Visible = true;
                return;
            }

            GpsSettings settings = new GpsSettingsRepository().Get();
            hdnDataUrl.Value = ResolveUrl("~/Handlers/DriverTrackingData.ashx");
            lnkDeliveryOrder.NavigateUrl = ResolveUrl("~/Tools/DeliveryOrder.aspx?date=" + HttpUtility.UrlEncode(hdnSelectedDay.Value));
            hdnTileUrl.Value = settings.MapTileUrl;
            hdnAzureTiles.Value = settings.UseAzureMap ? ResolveUrl("~/Handlers/MapTile.ashx") : string.Empty;
            litFresh.Text = settings.FreshMinutes.ToString(CultureInfo.InvariantCulture);
            litStale.Text = settings.StaleMinutes.ToString(CultureInfo.InvariantCulture);

            if (!DriverLocationRepository.TableSeen())
            {
                lblMessage.Text = "The driver position table is not installed yet; restart the site (SQLCommands-DriverGps-03.xml).";
                lblMessage.CssClass = "status-message status-error";
                lblMessage.Visible = true;
            }
        }
    }
}
