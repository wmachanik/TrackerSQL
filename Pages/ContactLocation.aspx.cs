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

        /// <summary>Unsaved main-contact address from the parent form (query string).</summary>
        private bool HasMainDraft => Request.QueryString["Address"] != null
            || Request.QueryString["Postal"] != null
            || Request.QueryString["AreaID"] != null;

        private string DraftAddress => Request.QueryString["Address"];
        private string DraftPostal => Request.QueryString["Postal"];

        private int? DraftAreaId
        {
            get
            {
                if (int.TryParse(Request.QueryString["AreaID"], out int id) && id > 0)
                    return id;
                return null;
            }
        }

        /// <summary>Unsaved deliver-to lines from the parent form.</summary>
        private bool HasDeliverToDraft => Request.QueryString["Addr1"] != null
            || Request.QueryString["Addr2"] != null
            || Request.QueryString["Addr3"] != null
            || Request.QueryString["Postal"] != null
            || Request.QueryString["AreaID"] != null;

        private string DraftAddr1 => Request.QueryString["Addr1"];
        private string DraftAddr2 => Request.QueryString["Addr2"];
        private string DraftAddr3 => Request.QueryString["Addr3"];

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                BindPage(autoSearchDraft: true);
        }

        private void BindPage(bool autoSearchDraft = false)
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
            string savedAddressText;
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
                savedAddressText = CustomerLocationManager.SearchTextForDeliverTo(row);
                if (HasDeliverToDraft)
                {
                    CustomerLocationManager.ApplyDraftDeliverTo(
                        row, DraftAddr1, DraftAddr2, DraftAddr3, DraftPostal, DraftAreaId);
                }
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
                savedAddressText = CustomerLocationManager.SearchTextFor(contact);
                if (HasMainDraft)
                {
                    contact = CustomerLocationManager.ApplyDraftAddress(
                        contact, DraftAddress, DraftPostal, DraftAreaId);
                }
                addressText = CustomerLocationManager.SearchTextFor(contact);
                loc = manager.Get(ContactId);
            }

            litAddress.Text = HttpUtility.HtmlEncode(addressText);
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
                txtSearch.Text = addressText;
            ViewState["SearchLabel"] = null;
            hdnDirty.Value = "0";

            bool draftDiffers = !string.IsNullOrWhiteSpace(addressText)
                && !string.Equals(
                    NormKey(addressText),
                    NormKey(savedAddressText),
                    StringComparison.Ordinal);

            if (loc != null && !(autoSearchDraft && draftDiffers))
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

            // Unsaved (or new) address in the parent form: try a search so the pin follows the box, not the DB.
            if (autoSearchDraft && !string.IsNullOrWhiteSpace(addressText) && (loc == null || draftDiffers))
                TryAutoSearch(addressText, manager);
        }

        private void TryAutoSearch(string addressText, CustomerLocationManager manager)
        {
            try
            {
                GeocodeResult result = manager.SearchAddress(addressText);
                if (!result.Success)
                {
                    ShowMessage(result.Message ?? "Address not found yet — edit the search and try again, or place the pin by hand.", true);
                    return;
                }

                SetPoint(Math.Round((decimal)result.Latitude, 6), Math.Round((decimal)result.Longitude, 6), true);
                ViewState["SearchLabel"] = result.Label;
                hdnDirty.Value = "1";
                double? km = manager.KmFromDepot(result.Latitude, result.Longitude);
                string distance = km.HasValue
                    ? " (" + km.Value.ToString(km.Value < 10 ? "0.0" : "0", CultureInfo.InvariantCulture) + " km from the depot)"
                    : string.Empty;
                litMatched.Text = HttpUtility.HtmlEncode((result.Label ?? addressText) + distance + " (from current address, not saved yet)");
                bool far = km.HasValue && km.Value > 150;
                ShowMessage(far
                    ? "The only match is " + km.Value.ToString("0", CultureInfo.InvariantCulture) + " km from the depot: " + result.Label
                      + ". Check the address before saving the pin."
                    : result.IsPrecise
                        ? "Pin placed from the current address" + distance + ". Check it, then press Save this pin."
                        : "Only a rough match (suburb / town)" + distance + ". Drag the pin, then press Save this pin.",
                    far || !result.IsPrecise);
            }
            catch (Exception ex)
            {
                ShowMessage("Lookup failed: " + ex.Message, true);
            }
        }

        private static string NormKey(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return string.Empty;
            return string.Join(" ", s.ToLowerInvariant().Split((char[])null, StringSplitOptions.RemoveEmptyEntries));
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

                BindPage(autoSearchDraft: false);
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
                var manager = new CustomerLocationManager();
                LocationLookupOutcome outcome;
                if (IsDeliverTo)
                {
                    outcome = HasDeliverToDraft
                        ? manager.EnsureForDeliverToAddress(
                            DeliverToAddressId, Context?.User?.Identity?.Name, force: true,
                            DraftAddr1, DraftAddr2, DraftAddr3, DraftPostal, DraftAreaId)
                        : manager.EnsureForDeliverToAddress(DeliverToAddressId, Context?.User?.Identity?.Name, force: true);
                }
                else
                {
                    outcome = HasMainDraft
                        ? manager.EnsureForContact(
                            ContactId, Context?.User?.Identity?.Name, force: true,
                            draftBillingAddress: DraftAddress, draftPostalCode: DraftPostal, draftAreaId: DraftAreaId)
                        : manager.EnsureForContact(ContactId, Context?.User?.Identity?.Name, force: true);
                }

                BindPage(autoSearchDraft: false);
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
