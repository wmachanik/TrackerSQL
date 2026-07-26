using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Pages
{
    public partial class RecurringOrderDetails : Page
    {
        private const string ReturnUrlSessionKey = "RecurringOrderDetails.ReturnUrl";
        private const string DefaultReturnUrl = SystemConstants.PageUrls.RecurringOrders;
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
        protected Button btnReturn;
        protected System.Web.UI.HtmlControls.HtmlGenericControl pnlStatus;
        protected Literal ltrlStatus;

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
                btnUpdate.Enabled = false;
                btnUpdateAndReturn.Enabled = false;
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
                SetStatus("New recurring order — choose a contact and add lines, then Insert.", isError: null);
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
            scriptManager.RegisterAsyncPostBackControl(btnAddLine);
            scriptManager.RegisterAsyncPostBackControl(btnConfirmAddLine);
            scriptManager.RegisterAsyncPostBackControl(btnCancelNewLine);
            scriptManager.RegisterAsyncPostBackControl(btnRevert);
            scriptManager.RegisterPostBackControl(btnUpdateAndReturn);
            scriptManager.RegisterPostBackControl(btnInsert);
            scriptManager.RegisterPostBackControl(btnDelete);
            scriptManager.RegisterPostBackControl(btnReturn);
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

        private void UpdateRecord()
        {
            var recurringOrder = GetDataFromForm();
            int calculatedCount = recurringOrdersRepository.AutoCalculateNextDatesForItems(recurringOrder);
            recurringOrdersRepository.Update(recurringOrder);
            LoadRecurringOrder(recurringOrder.RecurringOrderID, setStatus: false);
            ClearDirtyClientState();

            string forContact = FormatForContactPhrase();
            string status = calculatedCount > 0
                ? string.Format("Recurring order{0} updated. {1} next date(s) auto-calculated.", forContact, calculatedCount)
                : "Recurring order" + forContact + " updated.";
            SetStatus(status, isError: false);
            new showMessageBox(Page, "Recurring Order Update", status);
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

            Response.Redirect(returnUrl, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            UpdateRecord();
        }

        protected void btnUpdateAndReturn_Click(object sender, EventArgs e)
        {
            UpdateRecord();
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

        protected void btnReturn_Click(object sender, EventArgs e)
        {
            ReturnToPrevPage();
        }

        protected void btnInsert_Click(object sender, EventArgs e)
        {
            var recurringOrder = GetDataFromForm();
            int calculatedCount = recurringOrdersRepository.AutoCalculateNextDatesForItems(recurringOrder);
            recurringOrder.RecurringOrderID = recurringOrdersRepository.Insert(recurringOrder);
            string forContact = FormatForContactPhrase();
            string status = calculatedCount > 0
                ? string.Format("Recurring order{0} inserted. {1} next date(s) auto-calculated.", forContact, calculatedCount)
                : "Recurring order" + forContact + " inserted.";
            SetStatus(status, isError: false);
            new showMessageBox(Page, "Recurring Order Insert", status);
            ReturnToPrevPage(forceList: true);
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            string forContact = FormatForContactPhrase();
            recurringOrdersRepository.Delete(ResolveCurrentRecurringOrderId());
            string status = "Recurring order" + forContact + " deleted.";
            SetStatus(status, isError: false);
            new showMessageBox(Page, "Recurring Order Deleted", status);
            ReturnToPrevPage(forceList: true);
        }

        protected void btnAddLine_Click(object sender, EventArgs e)
        {
            try
            {
                int recurringOrderId = ResolveCurrentRecurringOrderId();
                SyncRecurringOrderIdControls(recurringOrderId);

                // Never trust a wiped GridView — Session/DB first
                var items = GetStableWorkingItems(recurringOrderId);
                StoreWorkingItems(items, force: true);
                BindRecurringItemsGrid(items);
                ShowNewLinePanel(true);
                BindNewLineLookups();
                ClearNewLineFields();
                SetStatus("Enter the new line details, then click Add.", isError: null);
                upnlReoccuringOrderDetails.Update();
            }
            catch (Exception ex)
            {
                SetStatus("Could not open add-line form: " + ex.Message, isError: true);
                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    "RecurringOrderDetails.btnAddLine_Click: " + ex.Message);
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
                    return;
                }

                if (!newItem.QtyRequired.HasValue)
                {
                    SetStatus("Please enter a quantity before adding the line.", isError: true);
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
                    SetStatus(status, isError: false);
                }
                else
                {
                    ShowNewLinePanel(false);
                    SetStatus("Line added. Click Insert to save the new recurring order.", isError: null);
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
                int recurringOrderItemId = ParsePositiveInt(Convert.ToString(e.CommandArgument));
                var source = e.CommandSource as Control;
                var gridRow = source != null ? source.NamingContainer as GridViewRow : null;
                if (gridRow != null)
                {
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
            }
        }

        /// <summary>
        /// If ViewState dropped the grid rows, rebind from Session/DB before RowCommand runs.
        /// </summary>
        private void EnsureRecurringItemsGridBoundFromPersisted()
        {
            int recurringOrderId = ResolveCurrentRecurringOrderId();
            if (recurringOrderId <= 0)
                return;

            if (gvRecurringOrderItems != null && gvRecurringOrderItems.Rows.Count > 0)
                return;

            var items = GetItemsForLineEdit(recurringOrderId);
            if (CountRealLines(items) == 0)
                return;

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

            var btnDeleteLine = e.Row.FindControl("btnDeleteLine") as LinkButton;
            if (btnDeleteLine != null)
            {
                var scriptManager = ScriptManager.GetCurrent(Page);
                if (scriptManager != null)
                    scriptManager.RegisterAsyncPostBackControl(btnDeleteLine);
            }

            var recurringOrderItem = e.Row.DataItem as RecurringOrderItem;
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
            if (pnlNewLine != null)
                pnlNewLine.Visible = visible;
            if (spnAddLine != null)
                spnAddLine.Visible = !visible;
            else if (btnAddLine != null)
                btnAddLine.Visible = !visible;
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
