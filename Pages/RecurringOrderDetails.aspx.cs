using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Managers;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Pages
{
    public partial class RecurringOrderDetails : Page
    {
        private const string ReturnUrlSessionKey = "RecurringOrderDetails.ReturnUrl";
        private const string DefaultReturnUrl = SystemConstants.PageUrls.RecurringOrders;
        private const string RecurringOrdersFlashStatusSessionKey = "RecurringOrders.FlashStatus";
        private const string WorkingItemsViewStateKey = "WorkingRecurringItems";
        private const string WorkingItemsSessionPrefix = "RecurringOrderDetails.WorkingItems.";
        private const string CurrentRecurringOrderIdViewStateKey = "CurrentRecurringOrderID";

        private readonly RecurringOrdersRepository recurringOrdersRepository = new RecurringOrdersRepository();
        private List<Item> _itemLookupCache;
        private List<ItemPackaging> _packagingLookupCache;
        private List<RecurringTypeLookup> _recurringTypeLookupCache;
        private PageStatePersister _sessionPageStatePersister;

        protected Label lblReoccuringOrderID;
        protected ScriptManager smReoccuringOrderDetails;
        protected Panel pnlRecurringOrderDetails;
        protected UpdateProgress uprgReoccuringOrderDetails;
        protected HiddenField hdnDirty;
        protected HiddenField hdnRecurringOrderId;
        protected UpdatePanel upnlReoccuringOrderDetails;
        protected DropDownList ddlCompanyName;
        protected Label ReoccuringOrderIDLabel;
        protected DropDownList ddlDeliveryBy;
        protected CheckBox EnabledCheckBox;
        protected TextBox NotesTextBox;
        protected GridView gvRecurringOrderItems;
        protected LinkButton btnAddLine;
        protected System.Web.UI.HtmlControls.HtmlGenericControl spnAddLine;
        protected Panel pnlNewLine;
        protected DropDownList ddlNewItemType;
        protected TextBox tbxNewQuantity;
        protected DropDownList ddlNewPackaging;
        protected TextBox tbxNewValue;
        protected DropDownList ddlNewRecurrence;
        protected TextBox tbxNewLastDate;
        protected TextBox tbxNewUntilDate;
        protected Button btnConfirmAddLine;
        protected Button btnCancelNewLine;
        protected Button btnUpdate;
        protected Button btnUpdateAndReturn;
        protected Button btnInsert;
        protected Button btnDelete;
        protected Button btnRevert;
        protected ImageButton btnReturn;
        protected System.Web.UI.HtmlControls.HtmlGenericControl pnlStatus;
        protected Literal ltrlStatus;
        protected Panel pnlInvoiceTypePrompt;
        protected Literal ltrlInvoiceTypePrompt;
        protected CheckBox chkInsertDisablePrediction;
        protected Button btnInvoiceTypeDeliveryNote;
        protected Button btnInvoiceTypeDispatch;
        protected Button btnInvoiceTypeKeep;
        protected Panel pnlDisablePrompt;
        protected Literal ltrlDisablePrompt;
        protected CheckBox chkDisableSwitchStandard;
        protected CheckBox chkDisableReenablePrediction;
        protected Button btnDisablePromptApply;
        protected Button btnDisablePromptSkip;

        /// <summary>
        /// Keep ViewState in Session — large dropdown lists were overflowing the hidden-field
        /// ViewState and aborting Add Line with Invalid viewstate.
        /// </summary>
        protected override PageStatePersister PageStatePersister
        {
            get
            {
                if (_sessionPageStatePersister == null)
                    _sessionPageStatePersister = new SessionPageStatePersister(this);
                return _sessionPageStatePersister;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterPostBackControls();

            // New-line form stays in the tree so Confirm Add / CalendarExtenders work with UpdatePanel.
            if (pnlNewLine != null)
                pnlNewLine.Visible = true;

            // Company/Delivery have EnableViewState=false — rebuild every request
            string postedCompany = Request.Form[ddlCompanyName.UniqueID];
            string postedDelivery = Request.Form[ddlDeliveryBy.UniqueID];
            BindDropDownLists();
            RestoreListSelection(ddlCompanyName, postedCompany);
            RestoreListSelection(ddlDeliveryBy, postedDelivery);

            // Keep order id stable across async postbacks (label/ViewState can be empty)
            SyncRecurringOrderIdControls(ResolveCurrentRecurringOrderId());

            if (IsPostBack)
            {
                // GridView ViewState often drops rows (large dropdowns). Restore before events
                // so DeleteLine still has a NamingContainer row / persisted list.
                EnsureRecurringItemsGridBoundFromPersisted();
                return;
            }

            CaptureReturnUrl();

            if (TryGetRecurringOrderIdFromQuery(out int loadId))
            {
                LoadRecurringOrder(loadId);
            }
            else
            {
                // New order: Save / Save & Return insert (Insert kept as an alias).
                btnUpdate.Enabled = true;
                btnUpdateAndReturn.Enabled = true;
                btnInsert.Enabled = true;
                btnDelete.Enabled = false;
                EnabledCheckBox.Checked = true;
                SyncRecurringOrderIdControls(0);
                BindRecurringItemsGrid(new List<RecurringOrderItem>
                {
                    new RecurringOrderItem
                    {
                        NextDateRequired = TimeZoneUtils.Now().Date
                    }
                });

                // Optional ?CoID= preselects contact and Delivery By from their preferred agent.
                if (int.TryParse(Request.QueryString[SystemConstants.UrlParameterConstants.CustomerID], out int contactId)
                    && contactId > 0)
                {
                    SetSelectedValueIfPresent(ddlCompanyName, contactId);
                    ApplyPreferredDeliveryByFromContact(contactId);
                }

                SetStatus("New recurring order — choose a contact, add lines, then Save.", isError: null);
            }
        }

        /// <summary>
        /// Prefer ?RecurringOrderID=… ; still accept legacy ?ID= for old bookmarks.
        /// </summary>
        private bool TryGetRecurringOrderIdFromQuery(out int recurringOrderId)
        {
            if (int.TryParse(Request.QueryString[SystemConstants.PageUrls.RecurringOrderIdQueryKey], out recurringOrderId) && recurringOrderId > 0)
                return true;

            if (int.TryParse(Request.QueryString[SystemConstants.PageUrls.LegacyRecurringOrderIdQueryKey], out recurringOrderId) && recurringOrderId > 0)
                return true;

            recurringOrderId = 0;
            return false;
        }

        /// <summary>
        /// Prefer querystring / hidden field / ViewState — Label text is often blank after async postbacks
        /// when large grids blow ViewState.
        /// </summary>
        private int ResolveCurrentRecurringOrderId()
        {
            int recurringOrderId;
            if (TryGetRecurringOrderIdFromQuery(out recurringOrderId))
                return recurringOrderId;

            string postedHidden = hdnRecurringOrderId != null
                ? Request.Form[hdnRecurringOrderId.UniqueID] ?? hdnRecurringOrderId.Value
                : null;
            if (int.TryParse(postedHidden, out recurringOrderId) && recurringOrderId > 0)
                return recurringOrderId;

            if (ViewState[CurrentRecurringOrderIdViewStateKey] != null
                && int.TryParse(Convert.ToString(ViewState[CurrentRecurringOrderIdViewStateKey]), out recurringOrderId)
                && recurringOrderId > 0)
                return recurringOrderId;

            if (int.TryParse(ReoccuringOrderIDLabel?.Text, out recurringOrderId) && recurringOrderId > 0)
                return recurringOrderId;

            if (int.TryParse(lblReoccuringOrderID?.Text, out recurringOrderId) && recurringOrderId > 0)
                return recurringOrderId;

            return 0;
        }

        private void SyncRecurringOrderIdControls(int recurringOrderId)
        {
            string text = recurringOrderId > 0 ? recurringOrderId.ToString() : string.Empty;
            ViewState[CurrentRecurringOrderIdViewStateKey] = recurringOrderId;
            if (hdnRecurringOrderId != null)
                hdnRecurringOrderId.Value = recurringOrderId > 0 ? recurringOrderId.ToString() : "0";
            if (ReoccuringOrderIDLabel != null)
                ReoccuringOrderIDLabel.Text = text;
            if (lblReoccuringOrderID != null)
                lblReoccuringOrderID.Text = text;
        }

        private static void RestoreListSelection(ListControl list, string postedValue)
        {
            if (list == null || string.IsNullOrEmpty(postedValue))
                return;
            if (list.Items.FindByValue(postedValue) != null)
                list.SelectedValue = postedValue;
        }

        private void RegisterPostBackControls()
        {
            var scriptManager = ScriptManager.GetCurrent(Page);
            if (scriptManager == null)
                return;

            scriptManager.RegisterAsyncPostBackControl(btnUpdate);
            scriptManager.RegisterAsyncPostBackControl(btnRevert);
            scriptManager.RegisterAsyncPostBackControl(ddlCompanyName);
            // Add Line / Confirm / Cancel must be full postbacks — async UpdatePanel sticks on Opening/Adding.
            scriptManager.RegisterPostBackControl(btnAddLine);
            scriptManager.RegisterPostBackControl(btnConfirmAddLine);
            scriptManager.RegisterPostBackControl(btnCancelNewLine);
            scriptManager.RegisterPostBackControl(btnUpdateAndReturn);
            scriptManager.RegisterPostBackControl(btnInsert);
            scriptManager.RegisterPostBackControl(btnDelete);
            scriptManager.RegisterPostBackControl(btnReturn);
            scriptManager.RegisterPostBackControl(btnInvoiceTypeDeliveryNote);
            scriptManager.RegisterPostBackControl(btnInvoiceTypeDispatch);
            scriptManager.RegisterPostBackControl(btnInvoiceTypeKeep);
            scriptManager.RegisterPostBackControl(btnDisablePromptApply);
            scriptManager.RegisterPostBackControl(btnDisablePromptSkip);
        }

        private bool? ReadViewStateBool(string key)
        {
            object raw = ViewState[key];
            if (raw == null)
                return null;
            if (raw is bool value)
                return value;
            return null;
        }

        private void CaptureReturnUrl()
        {
            string returnUrl = Request.QueryString["ReturnUrl"];
            if (string.IsNullOrWhiteSpace(returnUrl) && Request.UrlReferrer != null)
                returnUrl = Request.UrlReferrer.PathAndQuery;

            if (IsSafeLocalReturnUrl(returnUrl))
                Session[ReturnUrlSessionKey] = returnUrl;
            else if (Session[ReturnUrlSessionKey] == null)
                Session[ReturnUrlSessionKey] = DefaultReturnUrl;
        }

        private static bool IsSafeLocalReturnUrl(string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
                return false;
            if (returnUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return false;
            return returnUrl.StartsWith("~/") || returnUrl.StartsWith("/");
        }

        private void SetStatus(string message, bool? isError)
        {
            ltrlStatus.Text = HttpUtility.HtmlEncode(message ?? string.Empty);

            if (string.IsNullOrWhiteSpace(message))
            {
                pnlStatus.Attributes["class"] = "status-message";
                return;
            }

            if (isError == true)
                pnlStatus.Attributes["class"] = "status-message status-error";
            else if (isError == false)
                pnlStatus.Attributes["class"] = "status-message status-success";
            else
                pnlStatus.Attributes["class"] = "status-message status-info";
        }

        /// <summary>
        /// Writes a recurring.log audit line. AppLogger already prefixes the logged-in user.
        /// </summary>
        private void LogRecurringAudit(string action, string details = null, int? recurringOrderIdOverride = null)
        {
            int recurringOrderId = recurringOrderIdOverride ?? ResolveCurrentRecurringOrderId();
            int contactId = GetNullableInt(GetPostedOrControlValue(ddlCompanyName))
                ?? GetNullableInt(ddlCompanyName?.SelectedValue)
                ?? 0;
            string company = GetSelectedContactName();
            if (string.IsNullOrWhiteSpace(company) && contactId > 0)
                company = new ContactsRepository().GetContactNameById(contactId) ?? string.Empty;

            string orderPart = recurringOrderId > 0
                ? $"RecurringOrder {recurringOrderId}"
                : "New recurring order";
            string contactPart = contactId > 0
                ? (string.IsNullOrWhiteSpace(company)
                    ? $"Contact={contactId}"
                    : $"Contact={contactId} ({company})")
                : "Contact=(none)";

            string line = $"{orderPart} | {contactPart} | {action}";
            if (!string.IsNullOrWhiteSpace(details))
                line += $" | {details}";

            AppLogger.WriteLog(SystemConstants.LogTypes.Recurring, line);
        }

        private void ClearDirtyClientState()
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "recurringOrderClearDirty",
                "if (typeof recurringOrderClearDirty === 'function') recurringOrderClearDirty();", true);
        }

        private void BindDropDownLists()
        {
            BindCompanies();
            BindDeliveryBy();
        }

        private void BindCompanies()
        {
            ddlCompanyName.Items.Clear();
            ddlCompanyName.Items.Add(new ListItem("--- Select Contact Name ---", "0"));
            ddlCompanyName.AppendDataBoundItems = true;
            ddlCompanyName.DataSource = new ContactsRepository().GetAllCompanyNames();
            ddlCompanyName.DataTextField = nameof(ContactLookup.CompanyName);
            ddlCompanyName.DataValueField = nameof(ContactLookup.ContactID);
            ddlCompanyName.DataBind();
        }

        private void BindDeliveryBy()
        {
            ddlDeliveryBy.Items.Clear();
            ddlDeliveryBy.Items.Add(new ListItem("n/a", "0"));
            ddlDeliveryBy.AppendDataBoundItems = true;
            ddlDeliveryBy.DataSource = recurringOrdersRepository.GetDeliveryPeople();
            ddlDeliveryBy.DataTextField = nameof(DeliveryByLookup.DisplayName);
            ddlDeliveryBy.DataValueField = nameof(DeliveryByLookup.PersonID);
            ddlDeliveryBy.DataBind();
        }

        /// <summary>
        /// Sets Delivery By from the contact's PreferredAgentID (same source as normal orders).
        /// Falls back to the system default delivery person when the contact has no preference.
        /// </summary>
        private void ApplyPreferredDeliveryByFromContact(int contactId)
        {
            if (contactId <= 0 || ddlDeliveryBy == null)
                return;

            var prefs = new TrackerTools().RetrieveCustomerPrefs(contactId);
            if (prefs.PreferredDeliveryByID > 0)
                SetSelectedValueIfPresent(ddlDeliveryBy, prefs.PreferredDeliveryByID);
        }

        /// <summary>
        /// When Delivery By was left as n/a on a new recurring order, use the contact default.
        /// </summary>
        private void EnsureDeliveryByFromContactPreference(RecurringOrder recurringOrder)
        {
            if (recurringOrder == null)
                return;
            if (recurringOrder.DeliveryByID.HasValue && recurringOrder.DeliveryByID.Value > 0)
                return;
            if (!recurringOrder.ContactID.HasValue || recurringOrder.ContactID.Value <= 0)
                return;

            var prefs = new TrackerTools().RetrieveCustomerPrefs(recurringOrder.ContactID.Value);
            if (prefs.PreferredDeliveryByID > 0)
            {
                recurringOrder.DeliveryByID = prefs.PreferredDeliveryByID;
                SetSelectedValueIfPresent(ddlDeliveryBy, prefs.PreferredDeliveryByID);
            }
        }

        protected void ddlCompanyName_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Only auto-fill Delivery By for new recurring orders (existing ones keep their saved value
            // unless the user changes it). Changing contact on a new order always applies the preference.
            if (ResolveCurrentRecurringOrderId() > 0)
                return;

            int contactId = GetNullableInt(ddlCompanyName?.SelectedValue) ?? 0;
            if (contactId <= 0)
            {
                if (ddlDeliveryBy != null && ddlDeliveryBy.Items.FindByValue("0") != null)
                    ddlDeliveryBy.SelectedValue = "0";
                return;
            }

            ApplyPreferredDeliveryByFromContact(contactId);
        }

        private void LoadRecurringOrder(int recurringOrderId, bool setStatus = true)
        {
            ClearWorkingItemsState(recurringOrderId);

            var recurringOrder = recurringOrdersRepository.GetById(recurringOrderId);
            if (recurringOrder == null)
            {
                SetStatus("Recurring order not found.", isError: true);
                return;
            }

            StoreOriginalDataInViewState(recurringOrder);

            SyncRecurringOrderIdControls(recurringOrder.RecurringOrderID);
            SetSelectedValueIfPresent(ddlCompanyName, recurringOrder.ContactID);
            SetSelectedValueIfPresent(ddlDeliveryBy, recurringOrder.DeliveryByID);
            EnabledCheckBox.Checked = recurringOrder.Enabled ?? false;
            NotesTextBox.Text = recurringOrder.Notes;

            btnUpdate.Enabled = true;
            btnUpdateAndReturn.Enabled = true;
            btnInsert.Enabled = false;
            btnDelete.Enabled = true;

            var items = recurringOrder.Items ?? new List<RecurringOrderItem>();
            BindRecurringItemsGrid(items);

            if (setStatus)
            {
                string status = "Loaded recurring order" + FormatForContactPhrase() + ".";
                if (items.Count == 0)
                    status += " No lines found — use Add Line.";
                else
                    status += " " + items.Count + " line(s).";
                SetStatus(status, isError: null);
            }
        }

        private void ClearWorkingItemsState(int recurringOrderId)
        {
            ViewState.Remove(WorkingItemsViewStateKey);
            if (recurringOrderId > 0)
                Session.Remove(WorkingItemsSessionPrefix + recurringOrderId);
            Session.Remove(WorkingItemsSessionPrefix + "new");
        }

        private string GetSelectedContactName()
        {
            if (ddlCompanyName?.SelectedItem == null || ddlCompanyName.SelectedIndex < 0)
                return null;

            string name = ddlCompanyName.SelectedItem.Text?.Trim();
            if (string.IsNullOrWhiteSpace(name) ||
                name.Equals("none", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("--", StringComparison.Ordinal))
                return null;

            return name;
        }

        /// <summary>User-facing phrase: " for Acme Coffee" or empty when no contact selected.</summary>
        private string FormatForContactPhrase()
        {
            string name = GetSelectedContactName();
            return string.IsNullOrEmpty(name) ? string.Empty : " for " + name;
        }

        private RecurringOrder GetDataFromForm()
        {
            var dataFromForm = GetHeaderFromForm();
            dataFromForm.Items = GetStableWorkingItems(ResolveCurrentRecurringOrderId())
                .Where(item => !IsEmptyRecurringItem(item))
                .ToList();
            return dataFromForm;
        }

        private RecurringOrder GetHeaderFromForm()
        {
            var dataFromForm = new RecurringOrder();
            int recurringOrderId = ResolveCurrentRecurringOrderId();
            if (recurringOrderId > 0)
            {
                dataFromForm.RecurringOrderID = recurringOrderId;
                SyncRecurringOrderIdControls(recurringOrderId);
            }

            dataFromForm.ContactID = GetNullableInt(GetPostedOrControlValue(ddlCompanyName))
                ?? GetNullableInt(ddlCompanyName.SelectedValue);
            dataFromForm.DeliveryByID = GetNullableInt(GetPostedOrControlValue(ddlDeliveryBy))
                ?? GetNullableInt(ddlDeliveryBy.SelectedValue);
            dataFromForm.Enabled = EnabledCheckBox.Checked;
            dataFromForm.Notes = string.IsNullOrWhiteSpace(NotesTextBox.Text) ? null : NotesTextBox.Text.Trim();
            return dataFromForm;
        }

        /// <summary>
        /// Saves the recurring order. For a new order (no RecurringOrderID) this inserts.
        /// Returns true when a confirmation popup was shown (caller must not navigate away).
        /// </summary>
        private bool UpdateRecord()
        {
            var recurringOrder = GetDataFromForm();
            if (recurringOrder.RecurringOrderID <= 0)
                return InsertRecord(returnToListAfterSuccess: false);

            // Detect enabled<->disabled transitions from the DATABASE, not the ViewState
            // snapshot — session-backed ViewState is wiped by app restarts, which made the
            // disable popup silently not appear.
            var beforeSave = recurringOrdersRepository.GetById(recurringOrder.RecurringOrderID);
            bool manualDisable = (beforeSave?.Enabled ?? false)
                && !(recurringOrder.Enabled ?? false);
            bool manualEnable = beforeSave != null
                && !(beforeSave.Enabled ?? false)
                && (recurringOrder.Enabled ?? false);

            if (recurringOrder.Items == null || recurringOrder.Items.Count == 0)
            {
                SetStatus("No recurring lines were found to save — the order was NOT updated. "
                    + "Please check the lines and try again.",
                    isError: true);
                return false;
            }

            if (!recurringOrder.ContactID.HasValue || recurringOrder.ContactID.Value <= 0)
            {
                SetStatus("Select a contact before saving.", isError: true);
                return false;
            }

            int calculatedCount = recurringOrdersRepository.AutoCalculateNextDatesForItems(recurringOrder);
            recurringOrdersRepository.Update(recurringOrder);
            LoadRecurringOrder(recurringOrder.RecurringOrderID, setStatus: false);
            ClearDirtyClientState();

            string auditAction = manualDisable
                ? "Recurring order disabled"
                : (manualEnable ? "Recurring order enabled" : "Recurring order updated");
            LogRecurringAudit(auditAction, $"lines={recurringOrder.Items.Count}; nextDatesAuto={calculatedCount}");

            var notifyKind = manualDisable
                ? RecurringOrderNotificationManager.ChangeKind.Disabled
                : RecurringOrderNotificationManager.ChangeKind.Updated;
            string emailNote = NotifyContactQuietly(
                recurringOrder.ContactID, recurringOrder.RecurringOrderID, notifyKind);

            string forContact = FormatForContactPhrase();
            string status = calculatedCount > 0
                ? string.Format("Recurring order{0} updated. {1} next date(s) auto-calculated.", forContact, calculatedCount)
                : "Recurring order" + forContact + " updated.";
            if (!string.IsNullOrWhiteSpace(emailNote))
                status += " " + emailNote;
            SetStatus(status, isError: false);

            if (manualDisable && ShowDisablePrompt(recurringOrder.ContactID))
                return true;

            // Re-enabling is like adding: offer delivery note / dispatch + disable prediction.
            if (manualEnable && ShowInvoiceTypePrompt(recurringOrder.ContactID, "re-enabled"))
                return true;

            new showMessageBox(Page, "Recurring Order Update", status);
            return false;
        }

        /// <summary>
        /// Inserts a new recurring order. Returns true when the invoice-type popup was shown.
        /// </summary>
        private bool InsertRecord(bool returnToListAfterSuccess)
        {
            var recurringOrder = GetDataFromForm();

            if (!recurringOrder.ContactID.HasValue || recurringOrder.ContactID.Value <= 0)
            {
                SetStatus("Select a contact before saving.", isError: true);
                return false;
            }

            // Match normal orders: use the contact's preferred delivery person when left as n/a.
            EnsureDeliveryByFromContactPreference(recurringOrder);

            // A recurring order without lines does nothing and is invisible in the list —
            // never insert one (this also catches lines lost to an app restart mid-edit).
            if (recurringOrder.Items == null || recurringOrder.Items.Count == 0)
            {
                SetStatus("No recurring lines were found to save — the order was NOT inserted. "
                    + "Please check the lines (re-add them if the form was open a long time) and try again.",
                    isError: true);
                return false;
            }

            int calculatedCount = recurringOrdersRepository.AutoCalculateNextDatesForItems(recurringOrder);
            recurringOrder.RecurringOrderID = recurringOrdersRepository.Insert(recurringOrder);
            if (recurringOrder.RecurringOrderID <= 0)
            {
                SetStatus("Insert failed — the recurring order was not saved. Please try again.", isError: true);
                LogRecurringAudit("Insert failed", "Insert returned 0");
                return false;
            }

            SyncRecurringOrderIdControls(recurringOrder.RecurringOrderID);
            StoreOriginalDataInViewState(recurringOrder);
            btnUpdate.Enabled = true;
            btnUpdateAndReturn.Enabled = true;
            btnInsert.Enabled = false;
            btnDelete.Enabled = true;
            ClearDirtyClientState();

            LogRecurringAudit(
                "Recurring order created",
                $"lines={recurringOrder.Items.Count}; nextDatesAuto={calculatedCount}",
                recurringOrderIdOverride: recurringOrder.RecurringOrderID);

            string emailNote = NotifyContactQuietly(
                recurringOrder.ContactID, recurringOrder.RecurringOrderID,
                RecurringOrderNotificationManager.ChangeKind.Added);

            string forContact = FormatForContactPhrase();
            string status = calculatedCount > 0
                ? string.Format("Recurring order{0} saved. {1} next date(s) auto-calculated.", forContact, calculatedCount)
                : "Recurring order" + forContact + " saved.";
            if (!string.IsNullOrWhiteSpace(emailNote))
                status += " " + emailNote;
            SetStatus(status, isError: false);

            AppLogger.WriteLog(SystemConstants.LogTypes.System,
                "RecurringOrderDetails: inserted RecurringOrderID=" + recurringOrder.RecurringOrderID
                + " ContactID=" + recurringOrder.ContactID);

            if (ShowInvoiceTypePrompt(recurringOrder.ContactID))
            {
                // ApplyInvoiceTypeChoice uses ReturnAfterDisablePrompt (set by Save / Save & Return).
                if (ViewState["ReturnAfterDisablePrompt"] == null)
                    ViewState["ReturnAfterDisablePrompt"] = returnToListAfterSuccess;
                return true;
            }

            new showMessageBox(Page, "Recurring Order Saved", status);
            if (returnToListAfterSuccess)
                ReturnToPrevPage(forceList: true);
            else
            {
                Response.Redirect(
                    SystemConstants.PageUrls.RecurringOrderDetailsUrl(recurringOrder.RecurringOrderID),
                    false);
                Context.ApplicationInstance.CompleteRequest();
            }

            return false;
        }

        /// <summary>
        /// Best-effort contact email; never fails the save. Returns a short note for the status
        /// line when the send was skipped/failed, otherwise null/empty.
        /// </summary>
        private static string NotifyContactQuietly(int? contactId, int recurringOrderId,
            RecurringOrderNotificationManager.ChangeKind kind)
        {
            if (!contactId.HasValue || contactId.Value <= 0 || recurringOrderId <= 0)
                return null;

            string error = new RecurringOrderNotificationManager()
                .NotifyContact(contactId.Value, recurringOrderId, kind);
            if (string.IsNullOrWhiteSpace(error))
                return null;

            return "Contact email not sent: " + error;
        }

        private void StoreOriginalDataInViewState(RecurringOrder recurringOrder)
        {
            ViewState["OriginalRecurringOrder"] = new RecurringOrder
            {
                RecurringOrderID = recurringOrder.RecurringOrderID,
                ContactID = recurringOrder.ContactID,
                DeliveryByID = recurringOrder.DeliveryByID,
                Enabled = recurringOrder.Enabled,
                Notes = recurringOrder.Notes,
                Items = recurringOrder.Items == null
                    ? new List<RecurringOrderItem>()
                    : recurringOrder.Items.Select(CloneRecurringOrderItem).ToList()
            };
        }

        private static RecurringOrderItem CloneRecurringOrderItem(RecurringOrderItem item)
        {
            return new RecurringOrderItem
            {
                RecurringOrderItemID = item.RecurringOrderItemID,
                RecurringOrderID = item.RecurringOrderID,
                RecurringTypeID = item.RecurringTypeID,
                Value = item.Value,
                ItemRequiredID = item.ItemRequiredID,
                QtyRequired = item.QtyRequired,
                DateLastDone = item.DateLastDone,
                NextDateRequired = item.NextDateRequired,
                RequireUntilDate = item.RequireUntilDate,
                ItemPackagingID = item.ItemPackagingID
            };
        }

        private void RestoreOriginalDataFromViewState()
        {
            var originalOrder = ViewState["OriginalRecurringOrder"] as RecurringOrder;
            if (originalOrder == null)
                return;

            ReoccuringOrderIDLabel.Text = originalOrder.RecurringOrderID.ToString();
            SetSelectedValueIfPresent(ddlCompanyName, originalOrder.ContactID);
            SetSelectedValueIfPresent(ddlDeliveryBy, originalOrder.DeliveryByID);
            EnabledCheckBox.Checked = originalOrder.Enabled ?? false;
            NotesTextBox.Text = originalOrder.Notes;
            BindRecurringItemsGrid(originalOrder.Items);
        }

        private void ReturnToPrevPage(bool forceList = false)
        {
            string returnUrl = forceList
                ? DefaultReturnUrl
                : (Session[ReturnUrlSessionKey] as string ?? DefaultReturnUrl);

            if (!IsSafeLocalReturnUrl(returnUrl))
                returnUrl = DefaultReturnUrl;

            // Async UpdatePanel postbacks cannot rely on Response.Redirect — the browser
            // stays on this page with a stuck "Deleting..." / "Saving..." client overlay.
            var scriptManager = ScriptManager.GetCurrent(Page);
            if (scriptManager != null && scriptManager.IsInAsyncPostBack)
            {
                string clientUrl = ResolveClientUrl(returnUrl);
                string script = "window.location.replace('"
                    + HttpUtility.JavaScriptStringEncode(clientUrl)
                    + "');";
                ScriptManager.RegisterStartupScript(this, GetType(), "recurringOrderNavigateAway", script, true);
                return;
            }

            // endResponse:true — Redirect(false)+CompleteRequest can still finish rendering
            // this page and leave the user stuck on the details form after a successful delete.
            Response.Redirect(returnUrl, endResponse: true);
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            ViewState["ReturnAfterDisablePrompt"] = false;
            UpdateRecord();
        }

        protected void btnUpdateAndReturn_Click(object sender, EventArgs e)
        {
            ViewState["ReturnAfterDisablePrompt"] = true;
            var data = GetDataFromForm();
            if (data.RecurringOrderID <= 0)
            {
                if (!InsertRecord(returnToListAfterSuccess: true))
                    return;
                return;
            }

            if (!UpdateRecord())
                ReturnToPrevPage();
        }

        protected void btnRevert_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ReoccuringOrderIDLabel.Text))
            {
                RestoreOriginalDataFromViewState();
                ClearDirtyClientState();
                SetStatus("Changes reverted to last saved state.", isError: false);
            }
            else
            {
                BindRecurringItemsGrid(new List<RecurringOrderItem>
                {
                    new RecurringOrderItem
                    {
                        NextDateRequired = TimeZoneUtils.Now().Date
                    }
                });
                ddlCompanyName.SelectedIndex = 0;
                ddlDeliveryBy.SelectedIndex = 0;
                EnabledCheckBox.Checked = true;
                NotesTextBox.Text = string.Empty;
                ClearDirtyClientState();
                SetStatus("Form cleared.", isError: false);
            }
        }

        protected void btnReturn_Click(object sender, ImageClickEventArgs e)
        {
            ReturnToPrevPage();
        }

        protected void btnInsert_Click(object sender, EventArgs e)
        {
            ViewState["ReturnAfterDisablePrompt"] = true;
            InsertRecord(returnToListAfterSuccess: true);
        }

        private string GetContactInvoiceTypeDesc(int contactId)
        {
            int? currentTypeId = new ContactsAccInfoRepository().GetInvoiceTypeIdByContactId(contactId);
            if (currentTypeId.HasValue)
            {
                var invoiceType = new InvoiceTypesRepository().GetById(currentTypeId.Value);
                if (!string.IsNullOrWhiteSpace(invoiceType?.InvoiceTypeDesc))
                    return invoiceType.InvoiceTypeDesc;
            }

            return "unknown";
        }

        /// <summary>
        /// After inserting or re-enabling a recurring order, ask whether the contact's account
        /// type should become delivery note or dispatch note, and whether prediction should be
        /// disabled (recurring orders and the prediction system conflict).
        /// Returns false when there is no contact to ask about.
        /// </summary>
        private bool ShowInvoiceTypePrompt(int? contactId, string action = "inserted")
        {
            if (!contactId.HasValue || contactId.Value <= 0)
                return false;

            ViewState["PendingInvoiceTypeContactID"] = contactId.Value;

            string contactName = GetSelectedContactName() ?? "the contact";
            string currentType = GetContactInvoiceTypeDesc(contactId.Value);

            // Only offer to disable prediction when it isn't disabled already.
            // Delivery Note / Dispatch Note always disable prediction; this checkbox is for "No Change".
            bool predictionAlreadyDisabled =
                new ContactsRepository().GetById(contactId.Value)?.PredictionDisabled == true;
            chkInsertDisablePrediction.Visible = !predictionAlreadyDisabled;
            chkInsertDisablePrediction.Checked = !predictionAlreadyDisabled;
            ViewState["PendingDisablePredictionDefault"] = !predictionAlreadyDisabled;

            ltrlInvoiceTypePrompt.Text = string.Format(
                "Recurring order {0}. Should the account type for {1} be changed to delivery note or dispatch note? Current type: {2}.{3}",
                HttpUtility.HtmlEncode(action),
                HttpUtility.HtmlEncode(contactName), HttpUtility.HtmlEncode(currentType),
                predictionAlreadyDisabled
                    ? " Prediction is already disabled for this contact."
                    : " Choosing Delivery Note or Dispatch Note also disables prediction.");
            pnlInvoiceTypePrompt.Visible = true;
            upnlReoccuringOrderDetails.Update();
            ClearDirtyClientState();
            return true;
        }

        protected void btnInvoiceTypeDeliveryNote_Click(object sender, EventArgs e)
        {
            ApplyInvoiceTypeChoice(SystemConstants.InvoiceTypeConstants.DeliveryNote, "delivery note");
        }

        protected void btnInvoiceTypeDispatch_Click(object sender, EventArgs e)
        {
            ApplyInvoiceTypeChoice(SystemConstants.InvoiceTypeConstants.DispatchNote, "dispatch note");
        }

        protected void btnInvoiceTypeKeep_Click(object sender, EventArgs e)
        {
            ApplyInvoiceTypeChoice(0, null);
        }

        /// <summary>
        /// Contact for a popup choice: pending ViewState value first, then the posted contact
        /// dropdown, then the recurring order row itself — survives Session/ViewState loss
        /// (e.g. an app restart between rendering the popup and clicking a button).
        /// </summary>
        private int ResolvePromptContactId(string pendingViewStateKey)
        {
            int contactId = ViewState[pendingViewStateKey] as int? ?? 0;
            if (contactId > 0)
                return contactId;

            contactId = GetNullableInt(GetPostedOrControlValue(ddlCompanyName)) ?? 0;
            if (contactId > 0)
                return contactId;

            int recurringOrderId = ResolveCurrentRecurringOrderId();
            if (recurringOrderId > 0)
                contactId = recurringOrdersRepository.GetById(recurringOrderId)?.ContactID ?? 0;

            return contactId;
        }

        /// <summary>
        /// Applies the account-type popup choices (invoiceTypeId 0 = keep current account type).
        /// Delivery Note / Dispatch Note always disable prediction (it conflicts with recurring).
        /// On "No Change", the disable-prediction tick is optional. Returns to the list after an
        /// insert or "Update &amp; Return"; stays on the page after a plain "Save".
        /// </summary>
        private void ApplyInvoiceTypeChoice(int invoiceTypeId, string invoiceTypeName)
        {
            int contactId = ResolvePromptContactId("PendingInvoiceTypeContactID");
            var applied = new List<string>();

            bool wantsInvoiceChange = invoiceTypeId > 0;
            // Delivery/Dispatch always turn prediction off. For "No Change", honour the checkbox
            // (with ViewState fallback — UpdatePanel + full postback can drop the posted tick).
            bool wantsPredictionOff = wantsInvoiceChange
                ? (ReadViewStateBool("PendingDisablePredictionDefault") ?? true)
                : ResolveDisablePredictionFromPrompt();

            if (contactId <= 0 && (wantsInvoiceChange || wantsPredictionOff))
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Customers,
                    "RecurringOrderDetails: could not resolve the contact for the account-type popup — no changes applied.");
                pnlInvoiceTypePrompt.Visible = false;
                SetStatus("Could not identify the contact — the account type / prediction changes were NOT applied. "
                    + "Please make them on the contact's details page.", isError: true);
                return;
            }

            if (contactId > 0)
            {
                if (wantsInvoiceChange)
                {
                    bool changed = new ContactsAccInfoRepository().SetInvoiceTypeByContactId(
                        contactId, invoiceTypeId, "recurring order");
                    applied.Add(changed ? "account type set to " + invoiceTypeName : "account type change failed");
                    AppLogger.WriteLog(SystemConstants.LogTypes.Customers,
                        "RecurringOrderDetails: ContactID=" + contactId + " account type "
                        + (changed ? "changed to " + invoiceTypeName + "." : "could NOT be changed to " + invoiceTypeName + "."));
                }

                if (wantsPredictionOff)
                {
                    bool changed = new ContactsRepository().SetPredictionDisabled(
                        contactId, true, "recurring order");
                    applied.Add(changed ? "prediction disabled" : "prediction change failed");
                    AppLogger.WriteLog(SystemConstants.LogTypes.Customers,
                        "RecurringOrderDetails: ContactID=" + contactId + " prediction "
                        + (changed ? "disabled (recurring order active)." : "could NOT be disabled."));
                }
            }

            pnlInvoiceTypePrompt.Visible = false;
            ViewState.Remove("PendingInvoiceTypeContactID");
            ViewState.Remove("PendingDisablePredictionDefault");

            // Insert never sets the flag (return to the list); Save sets false (stay here),
            // Update & Return sets true — same convention as the disable prompt.
            // NOTE: do not use `as bool?` on a boxed bool — that always yields null.
            bool? returnAfter = ReadViewStateBool("ReturnAfterDisablePrompt");
            ViewState.Remove("ReturnAfterDisablePrompt");
            if (returnAfter == null)
            {
                ReturnToPrevPage(forceList: true);
                return;
            }
            if (returnAfter == true)
            {
                ReturnToPrevPage();
                return;
            }

            string contactChanges = applied.Count == 0
                ? "No contact changes made."
                : "Contact updated: " + string.Join(", ", applied) + ".";
            SetStatus("Recurring order saved. " + contactChanges, isError: false);

            if (ReadViewStateBool("OpenAddLineAfterInvoicePrompt") == true)
            {
                ViewState.Remove("OpenAddLineAfterInvoicePrompt");
                ShowNewLinePanel(true);
                BindNewLineLookups();
                ClearNewLineFields();
                SetStatus("Recurring order saved. " + contactChanges
                    + " Enter the new line details, then click Add.", isError: false);
            }

            upnlReoccuringOrderDetails.Update();
        }

        /// <summary>
        /// Reads the "disable prediction" tick for the "No Change" account-type path.
        /// Falls back to the recommended default stored when the prompt was shown, because a
        /// checked checkbox can fail to post after an UpdatePanel show + full postback.
        /// </summary>
        private bool ResolveDisablePredictionFromPrompt()
        {
            bool recommended = ReadViewStateBool("PendingDisablePredictionDefault") ?? false;
            if (!chkInsertDisablePrediction.Visible && !recommended)
                return false;

            // Prefer the live posted form value when the checkbox was rendered.
            string posted = Request.Form[chkInsertDisablePrediction.UniqueID];
            if (posted != null)
                return true; // HTML checkboxes only post when checked

            // Missing from the post: either unchecked, or not rendered. If we recommended
            // disable and the control still thinks it is checked, keep the recommendation.
            if (recommended && chkInsertDisablePrediction.Checked)
                return true;

            return false;
        }

        /// <summary>
        /// After a manual disable (Enabled checkbox unticked + save), ask whether the contact's
        /// account type should revert to standard and whether prediction should be re-enabled.
        /// Returns false when there is nothing to ask about.
        /// </summary>
        private bool ShowDisablePrompt(int? contactId)
        {
            if (!contactId.HasValue || contactId.Value <= 0)
                return false;

            bool predictionCurrentlyDisabled =
                new ContactsRepository().GetById(contactId.Value)?.PredictionDisabled == true;

            chkDisableSwitchStandard.Visible =
                SystemConstants.InvoiceTypeConstants.SwitchContactToStandardOnRecurringDisable;
            chkDisableSwitchStandard.Checked = chkDisableSwitchStandard.Visible;
            chkDisableReenablePrediction.Visible = predictionCurrentlyDisabled;
            chkDisableReenablePrediction.Checked = predictionCurrentlyDisabled;

            if (!chkDisableSwitchStandard.Visible && !chkDisableReenablePrediction.Visible)
                return false;

            string contactName = GetSelectedContactName() ?? "the contact";
            string currentType = GetContactInvoiceTypeDesc(contactId.Value);
            ltrlDisablePrompt.Text = string.Format(
                "This recurring order has been disabled. Current account type for {0}: {1}.{2}",
                HttpUtility.HtmlEncode(contactName), HttpUtility.HtmlEncode(currentType),
                predictionCurrentlyDisabled ? " Prediction is currently disabled for this contact." : string.Empty);

            ViewState["PendingDisableContactID"] = contactId.Value;
            pnlDisablePrompt.Visible = true;
            upnlReoccuringOrderDetails.Update();
            return true;
        }

        protected void btnDisablePromptApply_Click(object sender, EventArgs e)
        {
            int contactId = ResolvePromptContactId("PendingDisableContactID");
            var applied = new List<string>();

            if (contactId <= 0
                && ((chkDisableSwitchStandard.Visible && chkDisableSwitchStandard.Checked)
                    || (chkDisableReenablePrediction.Visible && chkDisableReenablePrediction.Checked)))
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Customers,
                    "RecurringOrderDetails: could not resolve the contact for the disable popup — no changes applied.");
                pnlDisablePrompt.Visible = false;
                SetStatus("Could not identify the contact — the account type / prediction changes were NOT applied. "
                    + "Please make them on the contact's details page.", isError: true);
                upnlReoccuringOrderDetails.Update();
                return;
            }

            if (contactId > 0)
            {
                if (chkDisableSwitchStandard.Visible && chkDisableSwitchStandard.Checked)
                {
                    bool changed = new ContactsAccInfoRepository().SetInvoiceTypeByContactId(
                        contactId, SystemConstants.InvoiceTypeConstants.Standard,
                        "recurring order disabled");
                    applied.Add(changed ? "account type set to standard" : "account type change failed");
                }

                if (chkDisableReenablePrediction.Visible && chkDisableReenablePrediction.Checked)
                {
                    bool changed = new ContactsRepository().SetPredictionDisabled(
                        contactId, false, "recurring order disabled");
                    applied.Add(changed ? "prediction re-enabled" : "prediction change failed");
                }

                if (applied.Count > 0)
                    AppLogger.WriteLog(SystemConstants.LogTypes.Customers,
                        "RecurringOrderDetails: ContactID=" + contactId + " on recurring order disable: "
                        + string.Join(", ", applied) + ".");
            }

            CloseDisablePrompt(applied);
        }

        protected void btnDisablePromptSkip_Click(object sender, EventArgs e)
        {
            CloseDisablePrompt(null);
        }

        private void CloseDisablePrompt(List<string> applied)
        {
            pnlDisablePrompt.Visible = false;
            ViewState.Remove("PendingDisableContactID");

            bool returnAfter = ReadViewStateBool("ReturnAfterDisablePrompt") ?? false;
            ViewState.Remove("ReturnAfterDisablePrompt");
            if (returnAfter)
            {
                ReturnToPrevPage();
                return;
            }

            string contactChanges = applied == null || applied.Count == 0
                ? "No contact changes made."
                : "Contact updated: " + string.Join(", ", applied) + ".";
            SetStatus("Recurring order disabled. " + contactChanges, isError: false);
            upnlReoccuringOrderDetails.Update();
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                int recurringOrderId = ResolveCurrentRecurringOrderId();
                if (recurringOrderId <= 0)
                {
                    SetStatus("Could not delete — recurring order id is missing. Reload the page and try again.", isError: true);
                    upnlReoccuringOrderDetails.Update();
                    return;
                }

                string forContact = FormatForContactPhrase();
                string contactNote = recurringOrdersRepository.Delete(recurringOrderId);
                LogRecurringAudit("Recurring order deleted", contactNote, recurringOrderIdOverride: recurringOrderId);
                string status = "Recurring order" + forContact + " deleted."
                    + (string.IsNullOrWhiteSpace(contactNote) ? string.Empty : " " + contactNote);

                // Redirect clears startup scripts — flash the message on the list page instead.
                Session[RecurringOrdersFlashStatusSessionKey] = status;
                ReturnToPrevPage(forceList: true);
            }
            catch (System.Threading.ThreadAbortException)
            {
                // Expected when Response.Redirect(endResponse: true) ends the request.
                throw;
            }
            catch (Exception ex)
            {
                SetStatus("Delete failed: " + ex.Message, isError: true);
                LogRecurringAudit("Delete failed", ex.Message);
                upnlReoccuringOrderDetails.Update();
            }
        }

        protected void btnAddLine_Click(object sender, EventArgs e)
        {
            try
            {
                int recurringOrderId = ResolveCurrentRecurringOrderId();
                SyncRecurringOrderIdControls(recurringOrderId);

                // Keep current grid edits in the working list — do NOT insert the order yet.
                // New orders stay in Session until the user clicks Save.
                var items = GetStableWorkingItems(recurringOrderId);
                StoreWorkingItems(items, force: true);
                BindRecurringItemsGrid(items);

                ShowNewLinePanel(true);
                BindNewLineLookups();
                ClearNewLineFields();
                SetStatus(
                    recurringOrderId > 0
                        ? "Enter the new line details, then click Add."
                        : "Enter the new line details, then click Add. The order is created when you click Save.",
                    isError: null);
                upnlReoccuringOrderDetails.Update();
            }
            catch (Exception ex)
            {
                SetStatus("Could not open add-line form: " + ex.Message, isError: true);
                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    "RecurringOrderDetails.btnAddLine_Click: " + ex.Message);
                upnlReoccuringOrderDetails.Update();
            }
        }

        protected void btnCancelNewLine_Click(object sender, EventArgs e)
        {
            int recurringOrderId = ResolveCurrentRecurringOrderId();
            SyncRecurringOrderIdControls(recurringOrderId);
            var items = GetStableWorkingItems(recurringOrderId);
            BindRecurringItemsGrid(items);
            ShowNewLinePanel(false);
            SetStatus(string.Empty, isError: null);
            upnlReoccuringOrderDetails.Update();
        }

        protected void btnConfirmAddLine_Click(object sender, EventArgs e)
        {
            try
            {
                int recurringOrderId = ResolveCurrentRecurringOrderId();
                SyncRecurringOrderIdControls(recurringOrderId);

                var newItem = BuildItemFromNewLinePanel();
                if (!newItem.ItemRequiredID.HasValue)
                {
                    SetStatus("Please select an item before adding the line.", isError: true);
                    upnlReoccuringOrderDetails.Update();
                    return;
                }

                if (!newItem.QtyRequired.HasValue)
                {
                    SetStatus("Please enter a quantity before adding the line.", isError: true);
                    upnlReoccuringOrderDetails.Update();
                    return;
                }

                var items = GetStableWorkingItems(recurringOrderId)
                    .Where(item => !IsEmptyRecurringItem(item))
                    .ToList();
                items.Add(newItem);
                StoreWorkingItems(items, force: true);
                BindRecurringItemsGrid(items);

                if (recurringOrderId > 0)
                {
                    var recurringOrder = GetHeaderFromForm();
                    recurringOrder.RecurringOrderID = recurringOrderId;
                    recurringOrder.Items = items;
                    int calculatedCount = recurringOrdersRepository.AutoCalculateNextDatesForItems(recurringOrder);
                    recurringOrdersRepository.Update(recurringOrder);
                    LoadRecurringOrder(recurringOrderId, setStatus: false);
                    ClearDirtyClientState();
                    ShowNewLinePanel(false);

                    string forContact = FormatForContactPhrase();
                    string status = calculatedCount > 0
                        ? string.Format("Line added and saved{0}. {1} next date(s) auto-calculated.", forContact, calculatedCount)
                        : "Line added and saved" + forContact + ".";
                    LogRecurringAudit(
                        "Line added",
                        $"ItemID={newItem.ItemRequiredID}; Qty={newItem.QtyRequired}; nextDatesAuto={calculatedCount}");
                    SetStatus(status, isError: false);
                }
                else
                {
                    ShowNewLinePanel(false);
                    LogRecurringAudit(
                        "Line added (draft)",
                        $"ItemID={newItem.ItemRequiredID}; Qty={newItem.QtyRequired}");
                    SetStatus("Line added. Click Save to create the new recurring order.", isError: null);
                }

                upnlReoccuringOrderDetails.Update();
            }
            catch (Exception ex)
            {
                SetStatus("Could not add line: " + ex.Message, isError: true);
                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    "RecurringOrderDetails.btnConfirmAddLine_Click: " + ex.Message);
            }
        }

        protected void gvRecurringOrderItems_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!string.Equals(e.CommandName, "DeleteLine", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            try
            {
                int recurringOrderId = ResolveCurrentRecurringOrderId();
                SyncRecurringOrderIdControls(recurringOrderId);

                // Authoritative list: Session/original/DB — never the posted grid (dropdown ViewState off).
                // Prefer DB when the working copy looks thinner than what we loaded.
                var items = GetItemsForLineEdit(recurringOrderId);
                int beforeCount = CountRealLines(items);

                int rowIndex = -1;
                int recurringOrderItemId = 0;
                ParseDeleteLineArgument(Convert.ToString(e.CommandArgument), out recurringOrderItemId, out rowIndex);

                var source = e.CommandSource as Control;
                var gridRow = source != null ? source.NamingContainer as GridViewRow : null;
                if (gridRow != null)
                {
                    if (rowIndex < 0)
                        rowIndex = gridRow.RowIndex;
                    var hfRecurringOrderItemID = gridRow.FindControl("hfRecurringOrderItemID") as HiddenField;
                    int fromHidden = ParsePositiveInt(GetPostedOrControlValue(hfRecurringOrderItemID));
                    if (fromHidden > 0)
                        recurringOrderItemId = fromHidden;
                }

                bool removed = false;
                if (recurringOrderItemId > 0)
                {
                    int matchIndex = items.FindIndex(item => item != null && item.RecurringOrderItemID == recurringOrderItemId);
                    if (matchIndex >= 0)
                    {
                        items.RemoveAt(matchIndex);
                        removed = true;
                    }
                }

                if (!removed && rowIndex >= 0 && rowIndex < items.Count)
                {
                    items.RemoveAt(rowIndex);
                    removed = true;
                }

                if (!removed)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.System,
                        "RecurringOrderDetails.DeleteLine: could not match line. id="
                        + recurringOrderId + " itemId=" + recurringOrderItemId
                        + " rowIndex=" + rowIndex + " beforeCount=" + beforeCount);

                    if (recurringOrderId > 0)
                    {
                        SetStatus("Could not identify which line to delete. Reloaded from database.", isError: true);
                        var reloaded = recurringOrdersRepository.GetById(recurringOrderId);
                        BindRecurringItemsGrid(reloaded?.Items ?? new List<RecurringOrderItem>());
                    }
                    else
                    {
                        SetStatus("Could not identify which line to delete. No changes made.", isError: true);
                        BindRecurringItemsGrid(items);
                    }

                    upnlReoccuringOrderDetails.Update();
                    return;
                }

                int afterCount = CountRealLines(items);
                // Guard against wiping the whole order when delete matching went wrong
                if (beforeCount > 1 && afterCount == 0)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.System,
                        "RecurringOrderDetails.DeleteLine: refused full wipe. id="
                        + recurringOrderId + " before=" + beforeCount);
                    SetStatus("Delete aborted — it would have cleared all lines. Reloaded from database.", isError: true);
                    var reloaded = recurringOrdersRepository.GetById(recurringOrderId);
                    BindRecurringItemsGrid(reloaded?.Items ?? new List<RecurringOrderItem>());
                    upnlReoccuringOrderDetails.Update();
                    return;
                }

                if (items.Count == 0)
                {
                    items.Add(new RecurringOrderItem
                    {
                        NextDateRequired = TimeZoneUtils.Now().Date
                    });
                }

                StoreWorkingItems(items, force: true);
                BindRecurringItemsGrid(items);
                LogRecurringAudit(
                    "Line removed",
                    $"itemId={recurringOrderItemId}; remaining={afterCount}");
                SetStatus(
                    "Line removed (" + afterCount + " remaining) — click Save to persist.",
                    isError: null);
                upnlReoccuringOrderDetails.Update();
            }
            catch (Exception ex)
            {
                SetStatus("Could not delete line: " + ex.Message, isError: true);
                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    "RecurringOrderDetails.DeleteLine: " + ex.Message);
                upnlReoccuringOrderDetails.Update();
            }
        }

        /// <summary>
        /// Rebind the lines grid on every postback. Item/Packaging/Recurrence dropdowns use
        /// EnableViewState=false, so ViewState can restore the row shell while leaving those
        /// lists empty (e.g. after company AutoPostBack for Delivery By preference).
        /// </summary>
        private void EnsureRecurringItemsGridBoundFromPersisted()
        {
            int recurringOrderId = ResolveCurrentRecurringOrderId();
            List<RecurringOrderItem> items = null;

            // Capture posted edits before DataBind replaces the rows
            if (gvRecurringOrderItems != null && gvRecurringOrderItems.Rows.Count > 0)
                items = GetRecurringItemsFromGrid(includeEmptyRows: true);

            if (items == null || items.Count == 0)
            {
                var fromPosted = GetRecurringItemsFromPostedForm();
                if (fromPosted.Count > 0)
                    items = fromPosted;
                else
                    items = GetItemsForLineEdit(recurringOrderId);
            }
            else if (CountItemsWithProduct(items) > 0)
            {
                // Keep Next Date / ids from session when the grid blanked calculated fields
                var working = GetItemsForLineEdit(recurringOrderId);
                MergeNextDatesFromWorking(items, working);
                MergeItemIdsFromWorking(items, working);
            }

            if (items == null || items.Count == 0)
            {
                items = new List<RecurringOrderItem>
                {
                    new RecurringOrderItem
                    {
                        NextDateRequired = TimeZoneUtils.Now().Date
                    }
                };
            }

            BindRecurringItemsGrid(items);
        }

        /// <summary>
        /// Working list for edit/delete — Session / original / DB. Never posted grid rows.
        /// </summary>
        private List<RecurringOrderItem> GetItemsForLineEdit(int recurringOrderId)
        {
            var working = GetWorkingItems();
            int workingCount = CountRealLines(working);

            List<RecurringOrderItem> fromDb = null;
            if (recurringOrderId > 0)
            {
                var order = recurringOrdersRepository.GetById(recurringOrderId);
                if (order?.Items != null)
                    fromDb = order.Items.Select(CloneRecurringOrderItem).ToList();
            }

            int dbCount = CountRealLines(fromDb);
            // Prefer session/working whenever it still has real lines (may be thinner after delete-before-save)
            if (workingCount > 0)
                return working;

            if (dbCount > 0)
                return fromDb;

            var originalOrder = ViewState["OriginalRecurringOrder"] as RecurringOrder;
            if (originalOrder?.Items != null && CountRealLines(originalOrder.Items) > 0)
                return originalOrder.Items.Select(CloneRecurringOrderItem).ToList();

            return working.Count > 0 ? working : (fromDb ?? new List<RecurringOrderItem>());
        }

        private static int ParsePositiveInt(string value)
        {
            // Ignore ImageButton-style "x,y" coordinates if they ever appear
            if (string.IsNullOrWhiteSpace(value) || value.IndexOf(',') >= 0)
                return 0;

            int parsed;
            return int.TryParse(value, out parsed) && parsed > 0 ? parsed : 0;
        }

        /// <summary>
        /// Accepts "itemId", "itemId|rowIndex", or legacy ImageButton "x,y" (ignored).
        /// </summary>
        private static void ParseDeleteLineArgument(string argument, out int itemId, out int rowIndex)
        {
            itemId = 0;
            rowIndex = -1;
            if (string.IsNullOrWhiteSpace(argument) || argument.IndexOf(',') >= 0)
                return;

            string[] parts = argument.Split('|');
            if (parts.Length > 0)
                itemId = ParsePositiveInt(parts[0]);
            if (parts.Length > 1)
            {
                int parsedRow;
                if (int.TryParse(parts[1], out parsedRow) && parsedRow >= 0)
                    rowIndex = parsedRow;
            }
        }

        private static int CountRealLines(IEnumerable<RecurringOrderItem> items)
        {
            return items == null
                ? 0
                : items.Count(item => item != null
                    && (item.RecurringOrderItemID > 0 || item.ItemRequiredID.HasValue));
        }

        /// <summary>
        /// Session/DB first. Grid capture only when it still has real item rows.
        /// </summary>
        private List<RecurringOrderItem> GetPersistedWorkingItems(int recurringOrderId)
        {
            return GetItemsForLineEdit(recurringOrderId);
        }

        /// <summary>
        /// Session/DB first. Grid capture only when it still has real item rows.
        /// </summary>
        private List<RecurringOrderItem> GetStableWorkingItems(int recurringOrderId)
        {
            var working = GetItemsForLineEdit(recurringOrderId);
            var fromGrid = GetRecurringItemsFromGrid(includeEmptyRows: true);

            if (CountItemsWithProduct(fromGrid) >= CountItemsWithProduct(working)
                && CountItemsWithProduct(fromGrid) > 0)
            {
                MergeNextDatesFromWorking(fromGrid, working);
                MergeItemIdsFromWorking(fromGrid, working);
                return fromGrid;
            }

            if (CountItemsWithProduct(working) > 0)
                return working;

            return fromGrid.Count > 0 ? fromGrid : working;
        }

        protected void gvRecurringOrderItems_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
            {
                return;
            }

            var recurringOrderItem = e.Row.DataItem as RecurringOrderItem;

            var btnDeleteLine = e.Row.FindControl("btnDeleteLine") as LinkButton;
            if (btnDeleteLine != null)
            {
                // Prefer id|rowIndex so delete still works when ViewState drops CommandArgument
                // or the line is unsaved (RecurringOrderItemID == 0).
                int itemId = recurringOrderItem != null ? recurringOrderItem.RecurringOrderItemID : 0;
                btnDeleteLine.CommandArgument = itemId + "|" + e.Row.RowIndex;
                btnDeleteLine.OnClientClick =
                    "return confirm('Remove this line from the recurring order?');";

                var scriptManager = ScriptManager.GetCurrent(Page);
                if (scriptManager != null)
                    scriptManager.RegisterAsyncPostBackControl(btnDeleteLine);
            }

            if (recurringOrderItem == null)
            {
                return;
            }

            var ddlItemType = e.Row.FindControl("ddlItemType") as DropDownList;
            if (ddlItemType != null)
            {
                var itemsRepository = new ItemsRepository();
                ddlItemType.DataSource = GetItemLookup(itemsRepository);
                ddlItemType.DataTextField = nameof(Item.FormattedDisplayText);
                ddlItemType.DataValueField = nameof(Item.ItemID);
                ddlItemType.DataBind();
                EnsureSelectedItemPresent(ddlItemType, recurringOrderItem.ItemRequiredID, itemsRepository);
            }

            var ddlPackagingTypes = e.Row.FindControl("ddlPackagingTypes") as DropDownList;
            if (ddlPackagingTypes != null)
            {
                ddlPackagingTypes.DataSource = GetPackagingLookup();
                ddlPackagingTypes.DataTextField = nameof(ItemPackaging.ItemPackagingDesc);
                ddlPackagingTypes.DataValueField = nameof(ItemPackaging.ItemPackagingID);
                ddlPackagingTypes.DataBind();
                SetSelectedValueIfPresent(ddlPackagingTypes, recurringOrderItem.ItemPackagingID);
            }

            var ddlReoccuranceType = e.Row.FindControl("ddlReoccuranceType") as DropDownList;
            if (ddlReoccuranceType != null)
            {
                ddlReoccuranceType.DataSource = GetRecurringTypeLookup();
                ddlReoccuranceType.DataTextField = nameof(RecurringTypeLookup.RecurringTypeDesc);
                ddlReoccuranceType.DataValueField = nameof(RecurringTypeLookup.RecurringTypeID);
                ddlReoccuranceType.DataBind();
                SetSelectedValueIfPresent(ddlReoccuranceType, recurringOrderItem.RecurringTypeID);
            }
        }

        private List<Item> GetItemLookup(ItemsRepository itemsRepository)
        {
            if (_itemLookupCache == null)
            {
                _itemLookupCache = itemsRepository.GetAll()
                    .OrderBy(item => item.ItemEnabled == false ? 1 : 0)
                    .ThenBy(item => item.SortOrder ?? int.MaxValue)
                    .ThenBy(item => item.ItemDesc ?? string.Empty)
                    .ToList();
            }

            return _itemLookupCache;
        }

        private List<ItemPackaging> GetPackagingLookup()
        {
            if (_packagingLookupCache == null)
            {
                _packagingLookupCache = new ItemPackagingsRepository().GetAll("ItemPackagingDesc");
            }

            return _packagingLookupCache;
        }

        private List<RecurringTypeLookup> GetRecurringTypeLookup()
        {
            if (_recurringTypeLookupCache == null)
            {
                _recurringTypeLookupCache = recurringOrdersRepository.GetRecurringTypes();
            }

            return _recurringTypeLookupCache;
        }

        private void SetSelectedValueIfPresent(ListControl control, int? value)
        {
            if (!value.HasValue)
            {
                return;
            }

            var listItem = control.Items.FindByValue(value.Value.ToString());
            if (listItem != null)
            {
                control.SelectedValue = listItem.Value;
            }
        }

        private void EnsureSelectedItemPresent(ListControl control, int? itemId, ItemsRepository itemsRepository)
        {
            if (!itemId.HasValue)
            {
                return;
            }

            var existingItem = control.Items.FindByValue(itemId.Value.ToString());
            if (existingItem == null)
            {
                var item = itemsRepository.GetById(itemId.Value);
                if (item != null)
                {
                    string itemText = string.IsNullOrWhiteSpace(item.ItemDesc)
                        ? "(item " + item.ItemID + ")"
                        : LookupFormatter.FormatLookupText(item.ItemDesc, item.ItemEnabled);

                    control.Items.Add(new ListItem(itemText, item.ItemID.ToString()));
                }
            }

            SetSelectedValueIfPresent(control, itemId);
        }

        private void BindRecurringItemsGrid(List<RecurringOrderItem> items)
        {
            var list = items != null && items.Count > 0
                ? items
                : new List<RecurringOrderItem> { new RecurringOrderItem() };
            StoreWorkingItems(list, force: true);
            gvRecurringOrderItems.DataSource = list;
            gvRecurringOrderItems.DataBind();
        }

        private void ShowNewLinePanel(bool visible)
        {
            // Never Visible=false on pnlNewLine during Confirm Add — that removes btnConfirmAddLine
            // from the UpdatePanel response and leaves the client stuck on "Please wait...".
            if (pnlNewLine != null)
            {
                pnlNewLine.Visible = true;
                pnlNewLine.Style["display"] = visible ? "block" : "none";
            }

            // .image-button uses display:inline-flex !important — must toggle a !important hide class.
            SetAddLineChromeVisible(!visible);
        }

        private void SetAddLineChromeVisible(bool show)
        {
            if (spnAddLine != null)
            {
                string cls = spnAddLine.Attributes["class"] ?? "image-button recurring-order-add-line";
                cls = cls.Replace("is-collapsed", string.Empty)
                    .Replace("  ", " ")
                    .Trim();
                if (!show)
                    cls = (cls + " is-collapsed").Trim();
                spnAddLine.Attributes["class"] = cls;
                return;
            }

            if (btnAddLine != null)
                btnAddLine.Style["display"] = show ? "" : "none";
        }

        private void BindNewLineLookups()
        {
            var itemsRepository = new ItemsRepository();

            ddlNewItemType.Items.Clear();
            ddlNewItemType.Items.Add(new ListItem("--- Select ---", "0"));
            ddlNewItemType.AppendDataBoundItems = true;
            ddlNewItemType.DataSource = GetItemLookup(itemsRepository);
            ddlNewItemType.DataTextField = nameof(Item.FormattedDisplayText);
            ddlNewItemType.DataValueField = nameof(Item.ItemID);
            ddlNewItemType.DataBind();

            ddlNewPackaging.Items.Clear();
            ddlNewPackaging.Items.Add(new ListItem("none", "0"));
            ddlNewPackaging.AppendDataBoundItems = true;
            ddlNewPackaging.DataSource = GetPackagingLookup();
            ddlNewPackaging.DataTextField = nameof(ItemPackaging.ItemPackagingDesc);
            ddlNewPackaging.DataValueField = nameof(ItemPackaging.ItemPackagingID);
            ddlNewPackaging.DataBind();

            ddlNewRecurrence.Items.Clear();
            ddlNewRecurrence.Items.Add(new ListItem("--- Select ---", "0"));
            ddlNewRecurrence.AppendDataBoundItems = true;
            ddlNewRecurrence.DataSource = GetRecurringTypeLookup();
            ddlNewRecurrence.DataTextField = nameof(RecurringTypeLookup.RecurringTypeDesc);
            ddlNewRecurrence.DataValueField = nameof(RecurringTypeLookup.RecurringTypeID);
            ddlNewRecurrence.DataBind();
        }

        private void ClearNewLineFields()
        {
            if (ddlNewItemType.Items.Count > 0)
                ddlNewItemType.SelectedIndex = 0;
            tbxNewQuantity.Text = "1";
            if (ddlNewPackaging.Items.Count > 0)
                ddlNewPackaging.SelectedIndex = 0;
            tbxNewValue.Text = string.Empty;
            if (ddlNewRecurrence.Items.Count > 0)
                ddlNewRecurrence.SelectedIndex = 0;
            tbxNewLastDate.Text = string.Empty;
            tbxNewUntilDate.Text = string.Empty;
        }

        private RecurringOrderItem BuildItemFromNewLinePanel()
        {
            return new RecurringOrderItem
            {
                ItemRequiredID = GetNullableInt(GetPostedOrControlValue(ddlNewItemType)),
                QtyRequired = GetNullableDouble(GetPostedOrControlValue(tbxNewQuantity)),
                ItemPackagingID = GetNullableInt(GetPostedOrControlValue(ddlNewPackaging)),
                Value = GetNullableInt(GetPostedOrControlValue(tbxNewValue)),
                RecurringTypeID = GetNullableInt(GetPostedOrControlValue(ddlNewRecurrence)),
                DateLastDone = ParseOptionalUserDate(GetPostedOrControlValue(tbxNewLastDate)),
                NextDateRequired = null,
                RequireUntilDate = ParseOptionalUserDate(GetPostedOrControlValue(tbxNewUntilDate))
            };
        }

        private void StoreWorkingItems(List<RecurringOrderItem> items)
        {
            StoreWorkingItems(items, force: false);
        }

        private void StoreWorkingItems(List<RecurringOrderItem> items, bool force)
        {
            var incoming = items == null
                ? new List<RecurringOrderItem>()
                : items.Select(CloneRecurringOrderItem).ToList();

            if (!force)
            {
                var existing = GetWorkingItemsRaw();
                int incomingCount = CountItemsWithProduct(incoming);
                int existingCount = CountItemsWithProduct(existing);
                // Never replace a richer working list with a failed/empty grid capture
                if (incomingCount < existingCount)
                    incoming = existing.Select(CloneRecurringOrderItem).ToList();
            }

            ViewState[WorkingItemsViewStateKey] = incoming;
            Session[GetWorkingItemsSessionKey()] = incoming.Select(CloneRecurringOrderItem).ToList();
        }

        private string GetWorkingItemsSessionKey()
        {
            int recurringOrderId = ResolveCurrentRecurringOrderId();
            return WorkingItemsSessionPrefix + (recurringOrderId > 0 ? recurringOrderId.ToString() : "new");
        }

        private List<RecurringOrderItem> GetWorkingItemsRaw()
        {
            var fromViewState = ViewState[WorkingItemsViewStateKey] as List<RecurringOrderItem>;
            if (fromViewState != null && CountItemsWithProduct(fromViewState) > 0)
                return fromViewState;

            // Try resolved key, then querystring / ViewState keys (label may be blank when stored)
            string primaryKey = GetWorkingItemsSessionKey();
            var fromSession = Session[primaryKey] as List<RecurringOrderItem>;
            if (fromSession != null && CountItemsWithProduct(fromSession) > 0)
                return fromSession;

            int knownId = ResolveCurrentRecurringOrderId();
            if (knownId > 0)
            {
                var idSession = Session[WorkingItemsSessionPrefix + knownId] as List<RecurringOrderItem>;
                if (idSession != null && CountItemsWithProduct(idSession) > 0)
                    return idSession;
            }

            int qsId;
            if (TryGetRecurringOrderIdFromQuery(out qsId) && qsId != knownId)
            {
                var qsSession = Session[WorkingItemsSessionPrefix + qsId] as List<RecurringOrderItem>;
                if (qsSession != null && CountItemsWithProduct(qsSession) > 0)
                    return qsSession;
            }

            var originalOrder = ViewState["OriginalRecurringOrder"] as RecurringOrder;
            if (originalOrder?.Items != null && CountItemsWithProduct(originalOrder.Items) > 0)
                return originalOrder.Items;

            // Do not return product-less shells — callers treat Count>0 as "have data"
            return new List<RecurringOrderItem>();
        }

        private List<RecurringOrderItem> GetWorkingItems()
        {
            return GetWorkingItemsRaw().Select(CloneRecurringOrderItem).ToList();
        }

        private static int CountItemsWithProduct(IEnumerable<RecurringOrderItem> items)
        {
            return items == null ? 0 : items.Count(item => item != null && item.ItemRequiredID.HasValue);
        }

        /// <summary>
        /// If the posted grid lost rows, fall back to Session / last-loaded order items.
        /// </summary>
        private List<RecurringOrderItem> EnsureItemsNotLost(List<RecurringOrderItem> candidate)
        {
            var list = candidate == null
                ? new List<RecurringOrderItem>()
                : candidate.Select(CloneRecurringOrderItem).ToList();

            if (CountItemsWithProduct(list) > 0)
                return list;

            var working = GetWorkingItems();
            if (CountItemsWithProduct(working) > 0)
                return working;

            return list;
        }

        /// <summary>
        /// Prefer posted grid values when they look intact; otherwise keep the last known working copy
        /// so Add/Delete/Save cannot wipe lines when dropdown ViewState is disabled.
        /// </summary>
        private List<RecurringOrderItem> CaptureCurrentItems(bool includeEmptyRows)
        {
            var fromGrid = GetRecurringItemsFromGrid(includeEmptyRows: true);
            var working = GetWorkingItems();

            int gridWithItems = CountItemsWithProduct(fromGrid);
            int workingWithItems = CountItemsWithProduct(working);

            List<RecurringOrderItem> result;
            if (gridWithItems > 0 && gridWithItems >= workingWithItems)
                result = fromGrid;
            else if (workingWithItems > 0)
                result = working;
            else
                result = fromGrid.Count > 0 ? fromGrid : working;

            MergeNextDatesFromWorking(result, working);
            MergeItemIdsFromWorking(result, working);

            if (!includeEmptyRows)
                result = result.Where(item => !IsEmptyRecurringItem(item)).ToList();

            StoreWorkingItems(result);
            return result;
        }

        private static void MergeItemIdsFromWorking(List<RecurringOrderItem> items, List<RecurringOrderItem> working)
        {
            if (items == null || working == null || working.Count == 0)
                return;

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].RecurringOrderItemID > 0)
                    continue;

                RecurringOrderItem match = null;
                if (items[i].ItemRequiredID.HasValue)
                {
                    match = working.FirstOrDefault(w =>
                        w.RecurringOrderItemID > 0
                        && w.ItemRequiredID == items[i].ItemRequiredID
                        && w.QtyRequired == items[i].QtyRequired);
                }

                if (match == null && i < working.Count && working[i].RecurringOrderItemID > 0)
                    match = working[i];

                if (match != null)
                    items[i].RecurringOrderItemID = match.RecurringOrderItemID;
            }
        }

        private static void MergeNextDatesFromWorking(List<RecurringOrderItem> items, List<RecurringOrderItem> working)
        {
            if (items == null || working == null || working.Count == 0)
                return;

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].NextDateRequired.HasValue)
                    continue;

                RecurringOrderItem match = null;
                if (items[i].RecurringOrderItemID > 0)
                {
                    match = working.FirstOrDefault(w => w.RecurringOrderItemID == items[i].RecurringOrderItemID);
                }
                else if (i < working.Count)
                {
                    match = working[i];
                }

                if (match != null && match.NextDateRequired.HasValue)
                    items[i].NextDateRequired = match.NextDateRequired;
            }
        }

        private List<RecurringOrderItem> GetRecurringItemsFromGrid(bool includeEmptyRows)
        {
            var items = new List<RecurringOrderItem>();

            // If the server-side grid lost its rows (Session ViewState wiped by an app
            // restart) the browser still posted the row inputs — recover them from the form.
            if (gvRecurringOrderItems.Rows.Count == 0)
                return GetRecurringItemsFromPostedForm();

            foreach (GridViewRow row in gvRecurringOrderItems.Rows)
            {
                var item = new RecurringOrderItem();

                var hfRecurringOrderItemID = row.FindControl("hfRecurringOrderItemID") as HiddenField;
                if (hfRecurringOrderItemID != null)
                {
                    item.RecurringOrderItemID = GetNullableInt(GetPostedOrControlValue(hfRecurringOrderItemID)) ?? 0;
                }

                var ddlItemType = row.FindControl("ddlItemType") as DropDownList;
                var tbxQuantity = row.FindControl("tbxQuantity") as TextBox;
                var ddlPackagingTypes = row.FindControl("ddlPackagingTypes") as DropDownList;
                var tbxValue = row.FindControl("tbxValue") as TextBox;
                var ddlReoccuranceType = row.FindControl("ddlReoccuranceType") as DropDownList;
                var tbxLastDate = row.FindControl("tbxLastDate") as TextBox;
                var tbxUntilDate = row.FindControl("tbxUntilDate") as TextBox;

                // Prefer posted form values — dropdown ViewState is disabled to keep postbacks small
                item.ItemRequiredID = GetNullableInt(GetPostedOrControlValue(ddlItemType));
                item.QtyRequired = GetNullableDouble(GetPostedOrControlValue(tbxQuantity));
                item.ItemPackagingID = GetNullableInt(GetPostedOrControlValue(ddlPackagingTypes));
                item.Value = GetNullableInt(GetPostedOrControlValue(tbxValue));
                item.RecurringTypeID = GetNullableInt(GetPostedOrControlValue(ddlReoccuranceType));
                item.DateLastDone = ParseOptionalUserDate(GetPostedOrControlValue(tbxLastDate));
                // NextDateRequired will be auto-calculated by AutoCalculateNextDatesForItems - don't read from UI
                item.NextDateRequired = null;
                item.RequireUntilDate = ParseOptionalUserDate(GetPostedOrControlValue(tbxUntilDate));

                if (includeEmptyRows || !IsEmptyRecurringItem(item))
                {
                    items.Add(item);
                }
            }

            return items;
        }

        /// <summary>
        /// Rebuilds the line items purely from Request.Form. Used when the grid has no
        /// server-side rows but the posted page did contain them, so the user's typed
        /// lines are never silently discarded.
        /// </summary>
        private List<RecurringOrderItem> GetRecurringItemsFromPostedForm()
        {
            var items = new List<RecurringOrderItem>();
            if (gvRecurringOrderItems == null || Request.Form.Count == 0)
                return items;

            const string itemDdlSuffix = "$ddlItemType";
            string gridPrefix = gvRecurringOrderItems.UniqueID + "$";

            var rowKeys = Request.Form.AllKeys
                .Where(key => key != null
                    && key.StartsWith(gridPrefix, StringComparison.Ordinal)
                    && key.EndsWith(itemDdlSuffix, StringComparison.Ordinal))
                .OrderBy(key => key, StringComparer.Ordinal)
                .ToList();

            foreach (string itemKey in rowKeys)
            {
                string rowPrefix = itemKey.Substring(0, itemKey.Length - "ddlItemType".Length);
                var item = new RecurringOrderItem
                {
                    RecurringOrderItemID = GetNullableInt(Request.Form[rowPrefix + "hfRecurringOrderItemID"]) ?? 0,
                    ItemRequiredID = GetNullableInt(Request.Form[itemKey]),
                    QtyRequired = GetNullableDouble(Request.Form[rowPrefix + "tbxQuantity"]),
                    ItemPackagingID = GetNullableInt(Request.Form[rowPrefix + "ddlPackagingTypes"]),
                    Value = GetNullableInt(Request.Form[rowPrefix + "tbxValue"]),
                    RecurringTypeID = GetNullableInt(Request.Form[rowPrefix + "ddlReoccuranceType"]),
                    DateLastDone = ParseOptionalUserDate(Request.Form[rowPrefix + "tbxLastDate"]),
                    NextDateRequired = null,
                    RequireUntilDate = ParseOptionalUserDate(Request.Form[rowPrefix + "tbxUntilDate"])
                };

                if (!IsEmptyRecurringItem(item))
                    items.Add(item);
            }

            if (items.Count > 0)
                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    "RecurringOrderDetails: recovered " + items.Count
                    + " line(s) from posted form after grid rows were lost.");

            return items;
        }

        private string GetPostedOrControlValue(Control control)
        {
            if (control == null)
            {
                return null;
            }

            string posted = Request.Form[control.UniqueID];
            if (posted != null)
            {
                return posted;
            }

            var listControl = control as ListControl;
            if (listControl != null)
            {
                return listControl.SelectedValue;
            }

            var textBox = control as TextBox;
            if (textBox != null)
            {
                return textBox.Text;
            }

            var hiddenField = control as HiddenField;
            return hiddenField != null ? hiddenField.Value : null;
        }

        private static bool IsEmptyRecurringItem(RecurringOrderItem recurringOrderItem)
        {
            return !recurringOrderItem.ItemRequiredID.HasValue
                && !recurringOrderItem.QtyRequired.HasValue
                && !recurringOrderItem.ItemPackagingID.HasValue
                && !recurringOrderItem.Value.HasValue
                && !recurringOrderItem.RecurringTypeID.HasValue
                && !recurringOrderItem.DateLastDone.HasValue
                && !recurringOrderItem.NextDateRequired.HasValue
                && !recurringOrderItem.RequireUntilDate.HasValue;
        }

        private int? GetNullableInt(string value)
        {
            if (int.TryParse(value, out var parsedValue) && parsedValue > 0)
            {
                return parsedValue;
            }

            return null;
        }

        private double? GetNullableDouble(string value)
        {
            if (double.TryParse(value, out var parsedValue))
            {
                return parsedValue;
            }

            return null;
        }

        private DateTime? ParseOptionalUserDate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var parsedValue = TrackerTools.ParseUserDate(value);
            return parsedValue <= SystemConstants.DatabaseConstants.SystemMinDate ? (DateTime?)null : parsedValue;
        }
    }
}
