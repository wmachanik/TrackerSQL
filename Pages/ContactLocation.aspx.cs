using System;
using System.Globalization;
using System.Web;
using System.Web.UI;
using TrackerSQL.Classes;
using TrackerSQL.Classes.Geo;
using TrackerSQL.Managers;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Pages
{
    /// <summary>Shown inside the Contact Details "Check / change" popup: confirm, drag or re-look-up the customer's GPS pin.</summary>
    public partial class ContactLocation : Page
    {
        private int ContactId
        {
            get
            {
                int.TryParse(Request.QueryString["ContactID"], out int id);
                return id;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                BindPage();
        }

        private void BindPage()
        {
            var manager = new CustomerLocationManager();
            Contact contact = ContactId > 0 ? new ContactsRepository().GetById(ContactId) : null;
            if (contact == null || !manager.IsReady)
            {
                ShowMessage(contact == null ? "Contact not found." : "The GPS tables are not installed yet.", true);
                btnSavePin.Enabled = false;
                btnLookupAgain.Enabled = false;
                return;
            }

            GpsSettings settings = new GpsSettingsRepository().Get();
            hdnTileUrl.Value = settings.MapTileUrl;

            litContactName.Text = HttpUtility.HtmlEncode(contact.CompanyName);
            string addressText = CustomerLocationManager.BuildLookupText(contact, CustomerLocationManager.AreaSuburbForLookup(contact));
            litAddress.Text = HttpUtility.HtmlEncode(addressText);
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
                txtSearch.Text = addressText.Replace(", South Africa", string.Empty);
            ViewState["SearchLabel"] = null;
            hdnDirty.Value = "0";

            CustomerLocation loc = manager.Get(ContactId);
            if (loc != null)
            {
                SetPoint(loc.Latitude, loc.Longitude, true);
                litMatched.Text = HttpUtility.HtmlEncode(!string.IsNullOrWhiteSpace(loc.GeocodedAddress) ? loc.GeocodedAddress
                    : loc.IsFromDriver ? "(where the driver delivered)" : "(set by hand)");
                litGeohash.Text = string.IsNullOrEmpty(loc.Geohash) ? string.Empty : "· geohash " + HttpUtility.HtmlEncode(loc.Geohash);
                lblState.Text = CustomerLocation.BadgeHtml(loc) + HttpUtility.HtmlEncode(loc.StatusDisplay + " (" + loc.OriginText + ")"
                    + (string.IsNullOrEmpty(loc.UpdatedBy) ? "" : " · " + loc.UpdatedBy + " " + loc.UpdatedDate.ToString("yyyy-MM-dd")));
                lblState.CssClass = loc.NeedsReview ? "contact-gps-value contact-gps-review"
                    : loc.IsVerified ? "contact-gps-value contact-gps-verified" : "contact-gps-value";
            }
            else
            {
                if (settings.HasDepotPoint)
                    SetPoint(settings.DepotLatitude.Value, settings.DepotLongitude.Value, false);
                else
                    SetPoint(-34.02m, 18.44m, false);
                litMatched.Text = "not looked up yet";
                litGeohash.Text = string.Empty;
                lblState.Text = CustomerLocation.BadgeHtml(null) + "No pin yet";
                lblState.CssClass = "contact-gps-value contact-gps-review";
            }
        }

        private void SetPoint(decimal lat, decimal lng, bool hasPoint)
        {
            hdnLat.Value = lat.ToString("0.######", CultureInfo.InvariantCulture);
            hdnLng.Value = lng.ToString("0.######", CultureInfo.InvariantCulture);
            hdnHasPoint.Value = hasPoint ? "1" : "0";
            litPoint.Text = hasPoint
                ? lat.ToString("0.00000", CultureInfo.InvariantCulture) + ", " + lng.ToString("0.00000", CultureInfo.InvariantCulture)
                : "not set";
        }

        protected void btnSavePin_Click(object sender, EventArgs e)
        {
            if (!decimal.TryParse(hdnLat.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal lat)
                || !decimal.TryParse(hdnLng.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal lng))
            {
                ShowMessage("Place the pin on the map first.", true);
                return;
            }

            try
            {
                new CustomerLocationManager().SetManual(ContactId, lat, lng, Context?.User?.Identity?.Name,
                    ViewState["SearchLabel"] as string);
                AppLogger.WriteLog(SystemConstants.LogTypes.Customers, "Contact=" + ContactId + " | GPS pin saved (checked)");
                BindPage();
                ShowMessage("Pin saved and marked as checked.", false);
                ClientScript.RegisterStartupScript(GetType(), "gpsSavedClose",
                    "if (window.parent && window.parent.trackerGpsChanged) { window.parent.trackerGpsChanged(); window.parent.trackerCloseGps(true); }", true);
            }
            catch (Exception ex)
            {
                ShowMessage("Could not save the pin: " + ex.Message, true);
            }
        }

        protected void btnLookupAgain_Click(object sender, EventArgs e)
        {
            try
            {
                LocationLookupOutcome outcome = new CustomerLocationManager()
                    .EnsureForContact(ContactId, Context?.User?.Identity?.Name, force: true);
                BindPage();
                ShowMessage(outcome.Message ?? (outcome.Success ? "Location found." : "Not found."), !outcome.Success);
                if (outcome.Success || outcome.UsedLookup)
                    NotifyParent();
            }
            catch (Exception ex)
            {
                ShowMessage("Lookup failed: " + ex.Message, true);
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                GeocodeResult result = new CustomerLocationManager().SearchAddress(txtSearch.Text);
                if (!result.Success)
                {
                    ShowMessage(result.Message ?? "Not found.", true);
                    return;
                }

                SetPoint(Math.Round((decimal)result.Latitude, 6), Math.Round((decimal)result.Longitude, 6), true);
                ViewState["SearchLabel"] = result.Label;
                hdnDirty.Value = "1";
                litMatched.Text = HttpUtility.HtmlEncode((result.Label ?? txtSearch.Text) + " (from search, not saved yet)");
                ShowMessage(result.IsPrecise
                    ? "Pin moved to the search result. Check it, then press Save this pin."
                    : "Only a rough match (suburb / town). Drag the pin to the right spot, then press Save this pin.",
                    !result.IsPrecise);
            }
            catch (Exception ex)
            {
                ShowMessage("Search failed: " + ex.Message, true);
            }
        }

        private void NotifyParent()
        {
            ClientScript.RegisterStartupScript(GetType(), "gpsChanged",
                "if (window.parent && window.parent.trackerGpsChanged) window.parent.trackerGpsChanged();", true);
        }

        private void ShowMessage(string message, bool isError)
        {
            lblMessage.Visible = true;
            lblMessage.Text = HttpUtility.HtmlEncode(message);
            lblMessage.CssClass = "status-message " + (isError ? "status-error" : "status-success");
        }
    }
}
