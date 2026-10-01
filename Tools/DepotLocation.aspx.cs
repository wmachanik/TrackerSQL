using System;
using System.Globalization;
using System.Web;
using System.Web.UI;
using TrackerSQL.Classes;
using TrackerSQL.Classes.Geo;
using TrackerSQL.Managers;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Tools
{
    /// <summary>Shown inside the System Preferences → GPS &amp; Drivers "Set on map" popup: place the depot pin by hand.</summary>
    public partial class DepotLocation : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                BindPage();
        }

        private bool UserIsAdmin()
        {
            var user = Context?.User;
            return user != null && user.Identity != null && user.Identity.IsAuthenticated
                && (user.IsInRole("Administrators") || user.IsInRole("Admin"));
        }

        private void BindPage()
        {
            var repo = new GpsSettingsRepository();
            if (!UserIsAdmin() || !repo.TableExists())
            {
                ShowMessage(UserIsAdmin() ? "The GPS tables are not installed yet." : "Only administrators can set the depot.", true);
                btnSavePin.Enabled = false;
                btnSearch.Enabled = false;
                return;
            }

            GpsSettings settings = repo.Get();
            hdnTileUrl.Value = settings.MapTileUrl;
            hdnAzureTiles.Value = settings.UseAzureMap ? ResolveUrl("~/Handlers/MapTile.ashx") : string.Empty;

            string address = settings.DepotAddress ?? string.Empty;
            litAddress.Text = HttpUtility.HtmlEncode(address.Length > 0 ? address : "(none)");
            if (string.IsNullOrWhiteSpace(txtSearch.Text) && !CustomerLocationManager.TryParseCoordinates(address, out _, out _))
                txtSearch.Text = address.Replace(';', ',');
            litMatched.Text = "-";
            hdnDirty.Value = "0";

            if (settings.HasDepotPoint)
            {
                SetPoint(settings.DepotLatitude.Value, settings.DepotLongitude.Value, true);
                litSaved.Text = PointText(settings.DepotLatitude.Value, settings.DepotLongitude.Value);
            }
            else
            {
                SetPoint(-34.02m, 18.44m, false);
                litSaved.Text = "not set";
            }
        }

        private static string PointText(decimal lat, decimal lng)
        {
            return lat.ToString("0.00000", CultureInfo.InvariantCulture) + ", " + lng.ToString("0.00000", CultureInfo.InvariantCulture);
        }

        private void SetPoint(decimal lat, decimal lng, bool hasPoint)
        {
            hdnLat.Value = lat.ToString("0.######", CultureInfo.InvariantCulture);
            hdnLng.Value = lng.ToString("0.######", CultureInfo.InvariantCulture);
            hdnHasPoint.Value = hasPoint ? "1" : "0";
            litPoint.Text = hasPoint ? PointText(lat, lng) : "not set";
        }

        protected void btnSavePin_Click(object sender, EventArgs e)
        {
            if (!UserIsAdmin())
                return;
            if (!decimal.TryParse(hdnLat.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal lat)
                || !decimal.TryParse(hdnLng.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal lng)
                || lat < -90 || lat > 90 || lng < -180 || lng > 180)
            {
                ShowMessage("Place the pin on the map first.", true);
                return;
            }

            try
            {
                lat = Math.Round(lat, 6);
                lng = Math.Round(lng, 6);
                var repo = new GpsSettingsRepository();
                GpsSettings before = repo.Get();
                repo.SaveDepotPoint(lat, lng);
                string point = PointText(lat, lng);
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "GPS depot point set on the map: " + point
                    + (before.HasDepotPoint ? " (was " + PointText(before.DepotLatitude.Value, before.DepotLongitude.Value) + ")" : string.Empty),
                    Context?.User?.Identity?.Name);
                BindPage();
                ShowMessage("Depot point saved.", false);
                string text = HttpUtility.JavaScriptStringEncode("GPS: " + point + " (set on the map)");
                ClientScript.RegisterStartupScript(GetType(), "depotSavedClose",
                    "if (window.parent && window.parent.trackerDepotSaved) { window.parent.trackerDepotSaved('" + text + "'); window.parent.trackerCloseDepot(true); }", true);
            }
            catch (Exception ex)
            {
                ShowMessage("Could not save the depot point: " + ex.Message, true);
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (!UserIsAdmin())
                return;
            try
            {
                GeocodeResult result = new CustomerLocationManager().SearchAddress(txtSearch.Text);
                if (!result.Success)
                {
                    ShowMessage(result.Message ?? "Not found.", true);
                    return;
                }

                SetPoint(Math.Round((decimal)result.Latitude, 6), Math.Round((decimal)result.Longitude, 6), true);
                hdnDirty.Value = "1";
                litMatched.Text = HttpUtility.HtmlEncode((result.Label ?? txtSearch.Text) + " (from search, not saved yet)");
                ShowMessage(result.IsPrecise
                        ? "Pin moved to the search result. Check it (zoom in, try Satellite), then press Save depot point."
                        : "Only a rough match (suburb / town). Drag the pin to the depot, then press Save depot point.",
                    !result.IsPrecise);
            }
            catch (Exception ex)
            {
                ShowMessage("Search failed: " + ex.Message, true);
            }
        }

        private void ShowMessage(string message, bool isError)
        {
            lblMessage.Visible = true;
            lblMessage.Text = HttpUtility.HtmlEncode(message);
            lblMessage.CssClass = "status-message " + (isError ? "status-error" : "status-success");
        }
    }
}
