using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using TrackerSQL.Classes;
using TrackerSQL.Managers;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Tools
{
    /// <summary>One-time (and catch-up) GPS lookups for contacts delivered by our own drivers.</summary>
    public partial class CustomerLocations : Page
    {
        private const int MaxBatch = 200;

        private static readonly object FailedLock = new object();
        private static DateTime _failedDate = DateTime.Today;
        private static readonly System.Collections.Generic.HashSet<int> FailedToday = new System.Collections.Generic.HashSet<int>();

        /// <summary>Contacts that could not be found today are not retried until tomorrow (saves lookups).</summary>
        private static bool FailedEarlierToday(int contactId, bool add)
        {
            lock (FailedLock)
            {
                if (_failedDate != DateTime.Today)
                {
                    FailedToday.Clear();
                    _failedDate = DateTime.Today;
                }
                return add ? !FailedToday.Add(contactId) : FailedToday.Contains(contactId);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var user = Context?.User;
            bool isAdmin = user != null && user.Identity != null && user.Identity.IsAuthenticated
                && (user.IsInRole("Administrators") || user.IsInRole("Admin"));
            if (!isAdmin)
            {
                pnlMain.Visible = false;
                pnlAccessDenied.Visible = true;
                return;
            }

            if (!IsPostBack)
                BindPage();
        }

        private void BindPage()
        {
            var locations = new CustomerLocationRepository();
            if (!locations.TableExists())
            {
                ShowMessage("The GPS tables are not installed yet; restart the site.", true);
                btnRunBatch.Enabled = false;
                return;
            }

            GpsSettings settings = new GpsSettingsRepository().Get();
            lblToDo.Text = locations.CountContactsNeedingLookup(cbxOpenOrdersOnly.Checked)
                + (cbxOpenOrdersOnly.Checked ? " with open orders" : " contacts");
            lblUsedToday.Text = new CustomerLocationManager().LookupsUsedToday() + " of " + settings.GeocodeDailyLimit
                + (settings.HasApiKey ? string.Empty : " (no key saved: add it on System Preferences → GPS & Drivers)");
            btnRunBatch.Enabled = settings.HasApiKey;

            gvReview.DataSource = locations.ListNeedingReview(500).Select(p => new
            {
                p.Key.ContactID,
                CompanyName = p.Value,
                p.Key.SourceAddress,
                p.Key.GeocodedAddress,
                p.Key.Source
            }).ToList();
            gvReview.DataBind();
        }

        protected void btnRunBatch_Click(object sender, EventArgs e)
        {
            int size;
            if (!int.TryParse(txtBatchSize.Text, out size) || size < 1)
                size = 50;
            size = Math.Min(size, MaxBatch);

            var manager = new CustomerLocationManager();
            string userName = Context?.User?.Identity?.Name;
            int found = 0, rough = 0, notFound = 0;
            string stopReason = null;

            try
            {
                int done = 0;
                bool openOnly = cbxOpenOrdersOnly.Checked;
                foreach (int contactId in new CustomerLocationRepository().ListContactIdsNeedingLookup(size + MaxBatch, openOnly))
                {
                    if (done >= size)
                        break;
                    if (FailedEarlierToday(contactId, add: false))
                        continue;
                    done++;
                    LocationLookupOutcome outcome = manager.EnsureForContact(contactId, userName, force: openOnly);
                    if (outcome.StopBatch)
                    {
                        stopReason = outcome.Message;
                        break;
                    }
                    if (outcome.Success && !outcome.Skipped)
                    {
                        if (outcome.Location != null && outcome.Location.NeedsReview) rough++;
                        else found++;
                    }
                    else if (!outcome.Success)
                    {
                        notFound++;
                        FailedEarlierToday(contactId, add: true);
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "CustomerLocations batch: " + ex.Message);
                stopReason = "Stopped: " + ex.Message;
            }

            AppLogger.WriteLog(SystemConstants.LogTypes.Customers,
                "GPS batch lookup | found=" + found + " rough=" + rough + " notFound=" + notFound + (stopReason == null ? "" : " stopped"));

            BindPage();
            string summary = found + " found, " + rough + " found roughly (check below), " + notFound + " not found.";
            ShowMessage(stopReason == null ? summary : summary + " " + stopReason, stopReason != null);
        }

        protected void cbxOpenOrdersOnly_CheckedChanged(object sender, EventArgs e)
        {
            BindPage();
        }

        private void ShowMessage(string message, bool isError)
        {
            lblMessage.Visible = true;
            lblMessage.Text = HttpUtility.HtmlEncode(message);
            lblMessage.CssClass = "status-message " + (isError ? "status-error" : "status-success");
        }
    }
}
