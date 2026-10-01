using System;
using System.Globalization;
using System.Web.UI;
using TrackerSQL.Handlers;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Tools
{
    /// <summary>Map of today's drivers (latest position, stops left, ETAs); data from Handlers/DriverTrackingData.ashx.</summary>
    public partial class DriverTracking : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
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
