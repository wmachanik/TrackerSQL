using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Managers;
using TrackerSQL.Models;

namespace TrackerSQL.Tools
{
    public partial class WooOrderImport : Page
    {
        private readonly WooCommerceOrderImportManager _manager = new WooCommerceOrderImportManager();
        private readonly WooCommerceSettingsManager _settings = new WooCommerceSettingsManager();

        private const string VsPreview = "WooOrderImport.Preview";
        private const string SessionUiState = "WooOrderImport.UiState";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!UserCanManage())
            {
                pnlMain.Visible = false;
                pnlWooDisabled.Visible = false;
                pnlAccessDenied.Visible = true;
                lblAccessDenied.Text = MessageProvider.Get(MessageKeys.SystemPreferences.AccessDenied);
                return;
            }

            if (!_settings.IsIntegrationEnabled())
            {
                pnlMain.Visible = false;
                pnlAccessDenied.Visible = false;
                pnlWooDisabled.Visible = true;
                litWooDisabled.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapNeedWooEnabled);
                btnStartWooWizard.Text = MessageProvider.Get(MessageKeys.WooCommerce.StartWizard);
                return;
            }

            pnlWooDisabled.Visible = false;
            _settings.EnsureSchemaOnce();

            if (!IsPostBack)
            {
                SyncModePanels();
                BindLastSyncHint();
                BindConflicts();
                // Always restore last pull (filters + preview) from session when returning
                // from Order Detail / Contact / menu — not only when ?restore=1 is present.
                if (TryRestorePreviewFromSession())
                    SetStatus("Preview restored — continue importing without re-pulling.", false);
            }
        }

        private bool UserCanManage()
        {
            var user = Context?.User;
            return user != null && user.Identity != null && user.Identity.IsAuthenticated &&
                   (user.IsInRole("Administrators") || user.IsInRole("Admin"));
        }

        protected void ddlMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            SyncModePanels();
            SaveUiStateToSession(GetPreviewRows(), keepFiltersWhenEmpty: true);
        }

        protected void btnClearPreview_Click(object sender, EventArgs e)
        {
            ViewState[VsPreview] = null;
            Session.Remove(SessionUiState);
            gvPreview.PageIndex = 0;
            BindPreview();
            pnlResults.Visible = false;
            SetStatus("Preview cleared.", false);
        }

        protected void chkUpdateExisting_CheckedChanged(object sender, EventArgs e)
        {
            BindPreview();
            SaveUiStateToSession(GetPreviewRows());
        }

        protected bool ShowAddOrderButton(object canImport, object alreadyImported)
        {
            if (!(canImport is bool can) || !can)
                return false;
            if (alreadyImported is bool imported && imported)
                return chkUpdateExisting.Checked;
            return true;
        }

        protected string GetAddOrderButtonText(object alreadyImported)
        {
            return alreadyImported is bool b && b
                ? "Update order in Tracker"
                : "Add order to Tracker";
        }

        protected string GetWooStatusCssClass(object status)
        {
            string s = Convert.ToString(status, CultureInfo.InvariantCulture)?.Trim() ?? string.Empty;
            if (string.Equals(s, "cancelled", StringComparison.OrdinalIgnoreCase))
                return "status-badge woo-import-status is-cancelled";
            if (string.Equals(s, "completed", StringComparison.OrdinalIgnoreCase))
                return "status-badge woo-import-status is-completed";
            return "woo-import-status";
        }

        protected void gvPreview_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;

            var preview = e.Row.DataItem as WooOrderImportPreviewRow;
            if (preview == null)
                return;

            var btn = e.Row.FindControl("btnAddOrder") as ImageButton;
            if (btn != null)
            {
                string msg = BuildAddOrderConfirmMessage(preview);
                btn.OnClientClick = "return confirm(" + HttpUtility.JavaScriptStringEncode(msg) + ");";
            }

            // Already imported → show view-orders icon to open the Tracker order.
            var phOpen = e.Row.FindControl("phOpenTrackerOrder") as PlaceHolder;
            var hlOpen = e.Row.FindControl("hlOpenTrackerOrder") as HyperLink;
            if (phOpen != null && hlOpen != null
                && preview.AlreadyImported
                && preview.ExistingTrackerOrderId.HasValue
                && preview.ExistingTrackerOrderId.Value > 0)
            {
                phOpen.Visible = true;
                hlOpen.NavigateUrl = GetTrackerOrderUrl(preview.ExistingTrackerOrderId.Value);
                hlOpen.ToolTip = "Open Tracker order #"
                    + preview.ExistingTrackerOrderId.Value.ToString(CultureInfo.InvariantCulture);
            }
            else if (phOpen != null)
            {
                phOpen.Visible = false;
            }
        }

        private string BuildAddOrderConfirmMessage(WooOrderImportPreviewRow preview)
        {
            if (preview == null)
                return "Add this Woo order to Tracker?";

            var parts = new List<string>();
            if (IsWooStatusCompleted(preview.WooStatus))
                parts.Add("This Woo order is marked completed in WooCommerce.");

            if (preview.AlreadyImported)
                parts.Add("Update this order in Tracker (refresh lines and notes from Woo)?");
            else if (preview.CanAddContact)
            {
                string name = string.IsNullOrWhiteSpace(preview.ContactDisplayName)
                    ? "the Woo customer"
                    : preview.ContactDisplayName.Trim();
                parts.Add("No matching contact was found. Create contact \""
                    + name
                    + "\" from the Woo shipping address (with default coffee/type settings where applicable) and add this order?");
            }
            else
                parts.Add("Add this Woo order to Tracker?");

            return string.Join(" ", parts);
        }

        private static bool IsWooStatusCompleted(string wooStatus)
        {
            return string.Equals(wooStatus?.Trim(), "completed", StringComparison.OrdinalIgnoreCase);
        }

        private void SyncModePanels()
        {
            string mode = ddlMode.SelectedValue ?? string.Empty;
            pnlSpecific.Visible = string.Equals(mode, "Specific", StringComparison.OrdinalIgnoreCase);
            pnlDateRange.Visible = string.Equals(mode, "DateRange", StringComparison.OrdinalIgnoreCase);
        }

        private void BindLastSyncHint()
        {
            try
            {
                var s = _settings.GetSettings();
                if (s.LastOrdersSyncUtc.HasValue)
                {
                    litLastSync.Text = string.Format(CultureInfo.InvariantCulture,
                        "Last orders sync (UTC): {0:yyyy-MM-dd HH:mm}. “Since last import sync” pulls orders after this time.",
                        s.LastOrdersSyncUtc.Value);
                }
                else
                {
                    litLastSync.Text = "No previous order import sync — “Since last import sync” defaults to the last 30 days.";
                }
            }
            catch
            {
                litLastSync.Text = string.Empty;
            }
        }

        protected void btnPull_Click(object sender, EventArgs e)
        {
            try
            {
                var mode = ParseMode(ddlMode.SelectedValue);
                long orderId = ParseLong(txtOrderId.Text);
                DateTime? from = ParseDate(txtFromDate.Text);
                DateTime? to = ParseDate(txtToDate.Text);

                var rows = _manager.PullPreview(mode, orderId, from, to, out string error);
                if (!string.IsNullOrWhiteSpace(error))
                {
                    SetStatus(error, true);
                    return;
                }

                ViewState[VsPreview] = rows;
                gvPreview.PageIndex = 0;
                BindPreview();
                SaveUiStateToSession(rows);
                SetStatus(string.Format(CultureInfo.InvariantCulture, "Pulled {0} order(s) for preview.", rows.Count), false);
                WooCommerceUserLog.Write("Order import pull preview (UI)",
                    string.Format(CultureInfo.InvariantCulture, "mode={0}, count={1}", mode, rows.Count),
                    UserName());
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
        }

        protected void gvPreview_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            long wooOrderId = ParseLong(Convert.ToString(e.CommandArgument, CultureInfo.InvariantCulture));
            if (wooOrderId <= 0)
            {
                SetStatus("Invalid Woo order.", true);
                return;
            }

            if (string.Equals(e.CommandName, "AddOrder", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var result = _manager.ImportOne(wooOrderId, chkUpdateExisting.Checked, UserName());
                    RefreshPreviewAfterImport(wooOrderId);
                    SaveUiStateToSession(GetPreviewRows());
                    BindLastSyncHint();
                    BindConflicts();

                    if (result.Failed > 0)
                    {
                        string failMsg = result.Messages != null && result.Messages.Count > 0
                            ? result.Messages[0]
                            : "Add order failed.";
                        ShowImportResult(result, failMsg);
                        return;
                    }

                    if (result.LastTrackerOrderId.HasValue
                        && result.LastTrackerOrderId.Value > 0
                        && (result.Imported > 0 || result.Updated > 0))
                    {
                        RedirectToOrderDetail(result.LastTrackerOrderId.Value);
                        return;
                    }

                    ShowImportResult(result, "Add order finished.");
                }
                catch (Exception ex)
                {
                    SetStatus(ex.Message, true);
                }
                return;
            }

            if (string.Equals(e.CommandName, "AddContact", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    int contactId = _manager.CreateContactFromWooOrder(wooOrderId, UserName(), out string error);
                    if (contactId <= 0)
                    {
                        SetStatus(error ?? "Could not create contact.", true);
                        return;
                    }

                    RefreshPreviewAfterImport(wooOrderId);
                    SaveUiStateToSession(GetPreviewRows());
                    RedirectToContactDetails(contactId);
                }
                catch (Exception ex)
                {
                    SetStatus(ex.Message, true);
                }
                return;
            }

            if (string.Equals(e.CommandName, "UpdateContact", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    int contactId = _manager.UpdateContactFromWooOrder(wooOrderId, UserName(), out string error);
                    if (contactId <= 0)
                    {
                        SetStatus(error ?? "Could not update contact.", true);
                        return;
                    }

                    RefreshPreviewAfterImport(wooOrderId);
                    SaveUiStateToSession(GetPreviewRows());
                    RedirectToContactDetails(contactId);
                }
                catch (Exception ex)
                {
                    SetStatus(ex.Message, true);
                }
            }
        }

        private void RedirectToContactDetails(int contactId)
        {
            string returnUrl = ResolveUrl("~/Tools/WooOrderImport.aspx?restore=1");
            string url = ResolveUrl("~/Pages/ContactDetails.aspx?ID="
                + contactId.ToString(CultureInfo.InvariantCulture)
                + "&ReturnUrl=" + HttpUtility.UrlEncode(returnUrl));
            Response.Redirect(url, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private void RedirectToOrderDetail(int orderId)
        {
            string returnUrl = ResolveUrl("~/Tools/WooOrderImport.aspx?restore=1");
            string url = ResolveUrl("~/Pages/OrderDetail.aspx?OrderID="
                + orderId.ToString(CultureInfo.InvariantCulture)
                + "&ReturnUrl=" + HttpUtility.UrlEncode(returnUrl));
            Response.Redirect(url, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void gvPreview_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvPreview.PageIndex = e.NewPageIndex;
            BindPreview();
            SaveUiStateToSession(GetPreviewRows());
        }

        protected void gvPreview_RowCreated(object sender, GridViewRowEventArgs e)
        {
            GridPager.BuildPager(gvPreview, e.Row);
        }

        private void BindPreview()
        {
            gvPreview.DataSource = GetPreviewRows();
            gvPreview.DataBind();
        }

        private List<WooOrderImportPreviewRow> GetPreviewRows()
        {
            return ViewState[VsPreview] as List<WooOrderImportPreviewRow>
                ?? new List<WooOrderImportPreviewRow>();
        }

        private void RefreshPreviewAfterImport(long wooOrderId)
        {
            var rows = GetPreviewRows();
            if (rows.Count == 0)
                return;

            var updated = _manager.GetPreviewForOrder(wooOrderId, out string error);
            if (updated == null)
            {
                BindPreview();
                SaveUiStateToSession(rows);
                return;
            }

            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i].WooOrderId == wooOrderId)
                {
                    rows[i] = updated;
                    break;
                }
            }

            ViewState[VsPreview] = rows;
            BindPreview();
            SaveUiStateToSession(rows);
        }

        private void SaveUiStateToSession(List<WooOrderImportPreviewRow> rows, bool keepFiltersWhenEmpty = false)
        {
            if (rows == null || rows.Count == 0)
            {
                if (!keepFiltersWhenEmpty)
                {
                    Session.Remove(SessionUiState);
                    return;
                }

                var existing = Session[SessionUiState] as WooOrderImportUiState
                    ?? new WooOrderImportUiState();
                existing.WooOrderIds = existing.WooOrderIds ?? new List<long>();
                existing.PageIndex = 0;
                existing.Mode = ddlMode.SelectedValue;
                existing.OrderId = txtOrderId.Text;
                existing.FromDate = txtFromDate.Text;
                existing.ToDate = txtToDate.Text;
                existing.UpdateExisting = chkUpdateExisting.Checked;
                Session[SessionUiState] = existing;
                return;
            }

            Session[SessionUiState] = new WooOrderImportUiState
            {
                WooOrderIds = rows.Select(r => r.WooOrderId).ToList(),
                PageIndex = gvPreview.PageIndex,
                Mode = ddlMode.SelectedValue,
                OrderId = txtOrderId.Text,
                FromDate = txtFromDate.Text,
                ToDate = txtToDate.Text,
                UpdateExisting = chkUpdateExisting.Checked,
                StatusMessage = litStatus.Text,
                StatusIsError = pnlStatus.CssClass != null && pnlStatus.CssClass.IndexOf("status-error", StringComparison.OrdinalIgnoreCase) >= 0
            };
        }

        private bool TryRestorePreviewFromSession()
        {
            var state = Session[SessionUiState] as WooOrderImportUiState;
            if (state == null)
                return false;

            try
            {
                if (!string.IsNullOrWhiteSpace(state.Mode)
                    && ddlMode.Items.FindByValue(state.Mode) != null)
                    ddlMode.SelectedValue = state.Mode;
            }
            catch
            {
                // ignore invalid saved mode
            }

            txtOrderId.Text = state.OrderId ?? string.Empty;
            txtFromDate.Text = state.FromDate ?? string.Empty;
            txtToDate.Text = state.ToDate ?? string.Empty;
            chkUpdateExisting.Checked = state.UpdateExisting;
            SyncModePanels();

            if (state.WooOrderIds == null || state.WooOrderIds.Count == 0)
                return !string.IsNullOrWhiteSpace(state.Mode);

            var rows = new List<WooOrderImportPreviewRow>();
            foreach (long wooOrderId in state.WooOrderIds)
            {
                var row = _manager.GetPreviewForOrder(wooOrderId, out _);
                if (row != null)
                    rows.Add(row);
            }

            if (rows.Count == 0)
                return true;

            ViewState[VsPreview] = rows;
            int page = state.PageIndex;
            if (page < 0)
                page = 0;
            int pageCount = (int)Math.Ceiling(rows.Count / (double)Math.Max(1, gvPreview.PageSize));
            if (pageCount > 0 && page >= pageCount)
                page = pageCount - 1;
            gvPreview.PageIndex = page;
            BindPreview();
            return true;
        }

        private void ShowImportResult(WooOrderImportBatchResult result, string statusMessage)
        {
            pnlResults.Visible = true;
            blResults.Items.Clear();
            blResults.Items.Add(string.Format(CultureInfo.InvariantCulture,
                "Added: {0}, updated: {1}, skipped: {2}, failed: {3}.",
                result.Imported, result.Updated, result.Skipped, result.Failed));
            foreach (string msg in result.Messages)
                blResults.Items.Add(msg);

            SetStatus(statusMessage, result.Failed > 0);
            WooCommerceUserLog.Write("Order import add order (UI)",
                string.Format(CultureInfo.InvariantCulture,
                    "added={0}, updated={1}, skipped={2}, failed={3}",
                    result.Imported, result.Updated, result.Skipped, result.Failed),
                UserName());
        }

        private static WooOrderImportMode ParseMode(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return WooOrderImportMode.SinceLastSync;
            if (Enum.TryParse(value, true, out WooOrderImportMode mode))
                return mode;
            return WooOrderImportMode.SinceLastSync;
        }

        private static long ParseLong(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;
            long n;
            return long.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out n) ? n : 0;
        }

        private static DateTime? ParseDate(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;
            if (DateTime.TryParse(text.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime dt))
                return dt;
            return null;
        }

        private string UserName()
        {
            return Context?.User?.Identity?.Name ?? "woo-import";
        }

        private void SetStatus(string message, bool isError)
        {
            pnlStatus.Visible = !string.IsNullOrWhiteSpace(message);
            litStatus.Text = message ?? string.Empty;
            pnlStatus.CssClass = isError ? "status-message status-error" : "status-message status-info";
        }

        protected string FormatWarnings(object conflictsObj, object warningsObj)
        {
            var parts = new List<string>();
            var conflicts = conflictsObj as List<string>;
            if (conflicts != null)
                parts.AddRange(conflicts);
            var warnings = warningsObj as List<string>;
            if (warnings != null)
            {
                foreach (string w in warnings)
                {
                    if (!parts.Contains(w))
                        parts.Add(w);
                }
            }
            return string.Join("; ", parts);
        }

        protected bool HasContactLink(object contactIdObj)
        {
            return TryContactId(contactIdObj, out int id) && id > 0;
        }

        protected string GetContactUrl(object contactIdObj)
        {
            if (!TryContactId(contactIdObj, out int id))
                return "#";
            return ResolveUrl("~/Pages/ContactDetails.aspx?ID=" + id.ToString(CultureInfo.InvariantCulture));
        }

        protected bool HasTrackerOrderLink(object alreadyImportedObj, object trackerOrderIdObj)
        {
            return alreadyImportedObj is bool already && already
                && TryParseInt(trackerOrderIdObj, out int orderId)
                && orderId > 0;
        }

        protected string GetTrackerOrderUrl(object trackerOrderIdObj)
        {
            if (!TryParseInt(trackerOrderIdObj, out int orderId) || orderId <= 0)
                return "#";
            return GetTrackerOrderUrl(orderId);
        }

        protected string GetTrackerOrderUrl(int orderId)
        {
            if (orderId <= 0)
                return "#";
            string returnUrl = ResolveUrl("~/Tools/WooOrderImport.aspx?restore=1");
            return ResolveUrl("~/Pages/OrderDetail.aspx?OrderID="
                + orderId.ToString(CultureInfo.InvariantCulture)
                + "&ReturnUrl=" + HttpUtility.UrlEncode(returnUrl));
        }

        private static bool TryContactId(object contactIdObj, out int contactId)
        {
            return TryParseInt(contactIdObj, out contactId);
        }

        private static bool TryParseInt(object value, out int result)
        {
            result = 0;
            if (value == null || value == DBNull.Value)
                return false;
            if (value is int i)
            {
                result = i;
                return true;
            }
            if (value is long l && l <= int.MaxValue && l >= int.MinValue)
            {
                result = (int)l;
                return true;
            }
            return int.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture),
                NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
        }

        private void BindConflicts()
        {
            gvConflicts.DataSource = _manager.GetImportConflicts();
            gvConflicts.DataBind();
        }

        protected void btnRefreshConflicts_Click(object sender, EventArgs e)
        {
            BindConflicts();
            SetStatus("Conflict list refreshed.", false);
        }
    }
}
