using System;
using System.Globalization;
using System.Linq;
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

        private int DeliverToAddressId
        {
            get
            {
                int.TryParse(Request.QueryString["DeliverToAddressID"], out int id);
                return id;
            }
        }

        private bool IsDeliverTo => DeliverToAddressId > 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                BindPage();
        }

        private void BindPage()
        {
            var manager = new CustomerLocationManager();
            if (!manager.IsReady)
            {
                ShowMessage("The GPS tables are not installed yet.", true);
                btnSavePin.Enabled = false;
                btnLookupAgain.Enabled = false;
                return;
            }

            GpsSettings settings = new GpsSettingsRepository().Get();
            hdnTileUrl.Value = settings.MapTileUrl;
            hdnAzureTiles.Value = settings.UseAzureMap ? ResolveUrl("~/Handlers/MapTile.ashx") : string.Empty;

            string addressText;
            CustomerLocation loc;
            if (IsDeliverTo)
            {
                ContactDeliverToAddress row = new ContactDeliverToAddressRepository().GetById(DeliverToAddressId);
                if (row == null || row.ContactID != ContactId)
                {
                    ShowMessage("Deliver-to address not found.", true);
                    btnSavePin.Enabled = false;
                    btnLookupAgain.Enabled = false;
                    return;
                }

                string title = string.Join(" — ", new[] { row.Label, row.DeliverToPerson }.Where(s => !string.IsNullOrWhiteSpace(s)));
                litContactName.Text = HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(title) ? "Deliver-to address" : title);
                addressText = CustomerLocationManager.SearchTextForDeliverTo(row);
                loc = row.ToDisplayLocation();
            }
            else
            {
                Contact contact = ContactId > 0 ? new ContactsRepository().GetById(ContactId) : null;
                if (contact == null)
                {
                    ShowMessage("Contact not found.", true);
                    btnSavePin.Enabled = false;
                    btnLookupAgain.Enabled = false;
                    return;
                }

                litContactName.Text = HttpUtility.HtmlEncode(contact.CompanyName);
                addressText = CustomerLocationManager.SearchTextFor(contact);
                loc = manager.Get(ContactId);
            }

            litAddress.Text = HttpUtility.HtmlEncode(addressText);
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
                txtSearch.Text = addressText;
            ViewState["SearchLabel"] = null;
            hdnDirty.Value = "0";

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
                if (IsDeliverTo)
                {
                    new CustomerLocationManager().SetManualDeliverTo(DeliverToAddressId, lat, lng, Context?.User?.Identity?.Name,
                        ViewState["SearchLabel"] as string);
                    AppLogger.WriteLog(SystemConstants.LogTypes.Customers,
                        "DeliverTo=" + DeliverToAddressId + " | GPS pin saved (checked)");
                }
                else
                {
                    new CustomerLocationManager().SetManual(ContactId, lat, lng, Context?.User?.Identity?.Name,
                        ViewState["SearchLabel"] as string);
                    AppLogger.WriteLog(SystemConstants.LogTypes.Customers, "Contact=" + ContactId + " | GPS pin saved (checked)");
                }

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
                LocationLookupOutcome outcome = IsDeliverTo
                    ? new CustomerLocationManager().EnsureForDeliverToAddress(DeliverToAddressId, Context?.User?.Identity?.Name, force: true)
                    : new CustomerLocationManager().EnsureForContact(ContactId, Context?.User?.Identity?.Name, force: true);
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
                var manager = new CustomerLocationManager();
                GeocodeResult result = manager.SearchAddress(txtSearch.Text);
                if (!result.Success)
                {
                    ShowMessage(result.Message ?? "Not found.", true);
                    return;
                }

                SetPoint(Math.Round((decimal)result.Latitude, 6), Math.Round((decimal)result.Longitude, 6), true);
                ViewState["SearchLabel"] = result.Label;
                hdnDirty.Value = "1";
                double? km = manager.KmFromDepot(result.Latitude, result.Longitude);
                string distance = km.HasValue ? " (" + km.Value.ToString(km.Value < 10 ? "0.0" : "0", CultureInfo.InvariantCulture) + " km from the depot)" : string.Empty;
                litMatched.Text = HttpUtility.HtmlEncode((result.Label ?? txtSearch.Text) + distance + " (from search, not saved yet)");
                bool far = km.HasValue && km.Value > 150;
                ShowMessage(far
                    ? "The only match is " + km.Value.ToString("0", CultureInfo.InvariantCulture) + " km from the depot: " + result.Label
                      + ". Check the address (a wrong suburb or town can send it elsewhere) before saving."
                    : result.IsPrecise
                        ? "Pin moved to the search result" + distance + ". Check it, then press Save this pin."
                        : "Only a rough match (suburb / town)" + distance + ". Drag the pin to the right spot, then press Save this pin.",
                    far || !result.IsPrecise);
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
