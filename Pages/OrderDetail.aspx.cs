using AjaxControlToolkit;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
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
    /// <summary>
    /// Order detail page. URL carries OrderID (?OrderID= saved order, ?NewOrder=true draft).
    /// After the first line is saved, OrderID is kept in ViewState and the browser URL is updated
    /// via history.replaceState (no full redirect).
    /// Contact id is stored in hdnSelectedContactId — not in the URL. Header changes are saved with Save Header.
    /// </summary>
    public partial class OrderDetail : Page
    {
        public const string CONST_QRYSTR_ORDERID = "OrderID";
        public const string CONST_QRYSTR_NEWORDER = "NewOrder";
        public const string CONST_QRYSTR_DELIVERYDATE = "DeliveryDate";
        public const string CONST_QRYSTR_NOTES = "Notes";
        public const string CONST_QRYSTR_DELIVERED = "Delivered";
        public const string CONST_QRYSTR_INVOICED = "Invoiced";
        public const string CONST_QRYSTR_ContactID = "ContactID";
        protected const string HeaderUnsavedStatusMessage = "You have unsaved changes. Click Save to keep them.";

        private const string SESSION_RETURN_URL = "OrderDetailReturnUrl";
        private const string DEFAULT_RETURN_URL = "~/Pages/DeliverySheet.aspx";

        private const string CONST_ORDERLINE_ITEM_COMBOBOX_ID = "cboItemDesc";
        private const string CONST_ORDERLINE_HIDDENFIELD_ITEM_LABEL = "lblItemDesc";
        private const string CONST_ORDERLINE_HIDDENFIELD_ITEM_ID = "hdnItemTypeID";
        private const string CONST_ORDERLINE_PACKAGING_COMBOBOX_ID = "cboPackaging";
        private const string CONST_ORDERLINE_HIDDENFIELD_PACKAGING_LABEL = "lblPackagingDesc";
        private const string CONST_ORDERLINE_HIDDENFIELD_PACKAGING_ID = "hdnPackagingID";
        private const string CONST_ORDERLINE_HIDDENFIELD_ORDER_ID = "hdnOrderID";

        private const string VSKEY_PERSISTED_ORDER_ID = "PersistedOrderId";
        private const string VSKEY_CONFLICT_ORDER_ID = "ConflictOrderId";
        private const string VSKEY_HEADER_UNDO = "HeaderUndoSnapshot";
        private const string VSKEY_PREFERRED_DELIVERY = "PreferredDeliveryPersonId";
        private const string VSKEY_LAST_CONTACT_DELIVERY = "LastContactIdForDeliveryPref";
        private const string VSKEY_PENDING_ITEM_ID = "PendingItemId";
        private const string VSKEY_PENDING_QTY = "PendingQty";
        private const string VSKEY_PENDING_PACKAGING = "PendingPackagingId";
        private const string VSKEY_PENDING_LAST_ORDER = "PendingLastOrder";
        private const string VSKEY_FORCE_NEW_HEADER_KEY = "ForceNewHeaderKey";
        private const string VSKEY_MERGEABLE_ORDER_ID = "MergeableOrderId";

        private readonly OrderManager _orderManager = new OrderManager();
        private readonly PersonsRepository _personsRepository = new PersonsRepository();

        private int PersistedOrderId
        {
            get => ViewState[VSKEY_PERSISTED_ORDER_ID] as int? ?? 0;
            set => ViewState[VSKEY_PERSISTED_ORDER_ID] = value;
        }

        private int OrderId
        {
            get
            {
                if (int.TryParse(Request.QueryString[CONST_QRYSTR_ORDERID], out int orderId) && orderId > 0)
                    return orderId;
                return PersistedOrderId;
            }
        }

        private int ConflictOrderId
        {
            get => ViewState[VSKEY_CONFLICT_ORDER_ID] as int? ?? 0;
            set => ViewState[VSKEY_CONFLICT_ORDER_ID] = value;
        }

        private int? PreferredDeliveryPersonId
        {
            get => ViewState[VSKEY_PREFERRED_DELIVERY] as int?;
            set => ViewState[VSKEY_PREFERRED_DELIVERY] = value;
        }

        private int LastContactIdForDeliveryPref
        {
            get => ViewState[VSKEY_LAST_CONTACT_DELIVERY] as int? ?? 0;
            set => ViewState[VSKEY_LAST_CONTACT_DELIVERY] = value;
        }

        private OrderHeaderData HeaderUndoSnapshot
        {
            get => ViewState[VSKEY_HEADER_UNDO] as OrderHeaderData;
            set => ViewState[VSKEY_HEADER_UNDO] = value;
        }

        private int PendingItemId
        {
            get => ViewState[VSKEY_PENDING_ITEM_ID] as int? ?? 0;
            set => ViewState[VSKEY_PENDING_ITEM_ID] = value;
        }

        private double PendingQty
        {
            get => ViewState[VSKEY_PENDING_QTY] as double? ?? 0;
            set => ViewState[VSKEY_PENDING_QTY] = value;
        }

        private int PendingPackagingId
        {
            get => ViewState[VSKEY_PENDING_PACKAGING] as int? ?? 0;
            set => ViewState[VSKEY_PENDING_PACKAGING] = value;
        }

        private bool PendingLastOrder
        {
            get => ViewState[VSKEY_PENDING_LAST_ORDER] as bool? ?? false;
            set => ViewState[VSKEY_PENDING_LAST_ORDER] = value;
        }

        private string ForceNewHeaderKey
        {
            get => ViewState[VSKEY_FORCE_NEW_HEADER_KEY] as string;
            set => ViewState[VSKEY_FORCE_NEW_HEADER_KEY] = value;
        }

        private int MergeableOrderId
        {
            get => ViewState[VSKEY_MERGEABLE_ORDER_ID] as int? ?? 0;
            set => ViewState[VSKEY_MERGEABLE_ORDER_ID] = value;
        }

        private bool HasPendingAddItem => PendingItemId > 0 && PendingQty > 0;

        private bool _redirectingAfterSave;
        private bool _orderLineSavedDuringHeaderSave;

        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncPostBackControls();

            if (!IsPostBack)
            {
                CaptureReturnUrlIfNeeded();

                if (TryRedirectLegacyOrderUrl())
                    return;

                if (Request.QueryString[CONST_QRYSTR_INVOICED] == "Y")
                {
                    MarkItemAsInvoiced();
                    return;
                }

                if (Request.QueryString[CONST_QRYSTR_DELIVERED] == "Y" && OrderId > 0)
                {
                    btnOrderDelivered_Click(this, EventArgs.Empty);
                    return;
                }

                if (IsNewOrderRequest())
                    InitializeNewOrder();
                else if (OrderId > 0)
                    LoadExistingOrder(OrderId);
                else
                    Response.Redirect("OrderDetail.aspx?NewOrder=true", true);
            }
        }

        protected void Page_LoadComplete(object sender, EventArgs e)
        {
            if (!IsPostBack || _redirectingAfterSave)
                return;

            ApplyHeaderUiState();
            UpdateNewItemButtonState();
            UpdateHeaderUndoButton();
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (_redirectingAfterSave)
                return;

            RepairContactComboSelection();
            EnsureContactDisplayed();
            UpdateNewItemButtonState();
            UpdateHeaderUndoButton();
            ConfigureMergeButtonConfirm();
            RegisterHeaderDirtyTracking();
        }

        /// <summary>
        /// Keeps Ajax ComboBox SelectedIndex in sync with hdnSelectedContactId so render/UpdatePanel never hits SelectedValue.
        /// </summary>
        private void RepairContactComboSelection()
        {
            if (cboContacts == null)
                return;

            int contactId = ReadPostedContactId();
            if (contactId <= 0)
                contactId = CurrentContactId;

            if (contactId > 0)
                EnsureContactComboSelectionValid(contactId);
        }

        private void EnsureContactComboSelectionValid(int contactId)
        {
            EnsureListControlSelectionByValue(cboContacts, contactId > 0 ? contactId.ToString() : null);
        }

        /// <summary>
        /// Sets list selection by item value using SelectedIndex only — never SelectedValue or Text (Ajax Toolkit safe).
        /// </summary>
        private static bool EnsureListControlSelectionByValue(ListControl list, string value)
        {
            if (list == null || string.IsNullOrEmpty(value))
                return false;

            int targetIndex = -1;
            for (int i = 0; i < list.Items.Count; i++)
            {
                if (string.Equals(list.Items[i].Value, value, StringComparison.Ordinal))
                {
                    targetIndex = i;
                    break;
                }
            }

            if (targetIndex < 0)
                return false;

            if (GetSafeComboSelectedIndex(list) == targetIndex)
                return true;

            list.ClearSelection();
            list.SelectedIndex = targetIndex;
            return true;
        }

        private static int GetSafeComboSelectedIndex(ListControl combo)
        {
            if (combo == null)
                return -1;

            int index = combo.SelectedIndex;
            return index >= 0 && index < combo.Items.Count ? index : -1;
        }

        protected void Page_PreRenderComplete(object sender, EventArgs e)
        {
            if (IsPostBack || Request.QueryString.Count == 0)
                return;

            if (Request.QueryString[SystemConstants.UrlParameterConstants.CustomerID] != null)
                SetContactById(Request.QueryString[SystemConstants.UrlParameterConstants.CustomerID]);
            else if (Request.QueryString[SystemConstants.UrlParameterConstants.CustomerName] != null)
                SetContactByName(
                    Request.QueryString[SystemConstants.UrlParameterConstants.CompanyName],
                    Request.QueryString[SystemConstants.UrlParameterConstants.CustomerName],
                    Request.QueryString[SystemConstants.UrlParameterConstants.Email]);

            if (IsNewOrderRequest()
                && Request.QueryString[SystemConstants.UrlParameterConstants.LastOrder] == "Y")
            {
                btnLastOrder_Click(this, EventArgs.Empty);
            }
        }

        #region Page initialization

        private bool IsNewOrderRequest()
        {
            return Request.QueryString[CONST_QRYSTR_NEWORDER] != null
                || OrderId <= 0;
        }

        private void InitializeNewOrder()
        {
            ApplyPageTone(isNewOrder: true);
            litPageTitle.Text = "New Order";
            Page.Title = "New Order";

            DateTime orderDate = TimeZoneUtils.Now().Date;
            var (prepDate, deliveryDate) = _orderManager.CalculateOrderDates(orderDate);
            tbxOrderDate.Text = orderDate.ToString("yyyy-MM-dd");
            tbxPrepDate.Text = prepDate.ToString("yyyy-MM-dd");
            tbxRequiredByDate.Text = deliveryDate.ToString("yyyy-MM-dd");

            cboContacts.ClearSelection();
            cboContacts.Text = string.Empty;
            SetContactId(0);
            BindContactDropdown();
            BindDeliveryPersonDropdown(forceRebind: true);
            BindNewItemLookups();
            btnLastOrder.Visible = false;
            btnConfirmOrder.Enabled = false;
            btnOrderDelivered.Enabled = false;
            btnUnDoDone.Enabled = false;
            SetStatusMessage("Select a contact, then add items.");
            PersistedOrderId = 0;
            ClearHeaderUndo();
            ClearDraftConflictState();
            SetNewItemPanelVisible(false);
            UpdateNewItemButtonState();
            BindOrderLines();
        }

        private void ApplyPageTone(bool isNewOrder)
        {
            if (pnlOrderShell != null)
            {
                pnlOrderShell.CssClass = isNewOrder
                    ? "simpleForm page-tone-panel page-tone-neworder"
                    : "simpleForm page-tone-panel page-tone-orders";
            }

            if (imgPageToneIcon != null)
            {
                imgPageToneIcon.ImageUrl = isNewOrder
                    ? "~/images/imgButtons/icons8-new-order-16.png"
                    : "~/images/imgButtons/icons8-view-orders-16.png";
            }

            if (litPageSubtitle != null)
            {
                litPageSubtitle.Text = isNewOrder
                    ? "Create a new customer order"
                    : "Manage existing orders and deliveries";
            }
        }

        private void LoadExistingOrder(int orderId)
        {
            var header = _orderManager.GetOrderHeader(orderId);
            if (header == null)
            {
                SetStatusMessage("Order not found.", isError: true);
                return;
            }

            ClearHeaderUndo();
            ApplyPageTone(isNewOrder: false);
            BindContactDropdown();
            BindHeaderToControls(header);
            BindNewItemLookups();
            ClearHeaderDirtyState();
            SyncSessionForDataSources(header);
            BindOrderLines();
            RefreshPageTitle();
            SetStatusMessage(string.Empty, log: false);
            btnLastOrder.Visible = header.CustomerID > 0;
            SetNewItemPanelVisible(false);
            UpdateDuplicateMergeState();
            ApplyHeaderUiState();
        }

        private bool TryRedirectLegacyOrderUrl()
        {
            if (Request.QueryString[CONST_QRYSTR_ORDERID] != null)
                return false;

            string contactParam = Request.QueryString["ContactID"] ?? Request.QueryString[CONST_QRYSTR_ContactID];
            if (contactParam == null || !long.TryParse(contactParam, out long contactId) || contactId <= 0)
                return false;

            var ordersRepository = new OrdersRepository();
            int? orderId = null;

            if (Request.QueryString[CONST_QRYSTR_DELIVERYDATE] != null
                && DateTime.TryParse(Request.QueryString[CONST_QRYSTR_DELIVERYDATE], out DateTime deliveryDate))
            {
                string notes = Request.QueryString[CONST_QRYSTR_NOTES] ?? string.Empty;
                orderId = ordersRepository.FindOrderIdByRequiredByDate(contactId, deliveryDate.Date, notes);
            }
            else if (Request.QueryString["PrepDate"] != null
                     && DateTime.TryParse(Request.QueryString["PrepDate"], out DateTime prepDate))
            {
                orderId = ordersRepository.FindOrderIdByPrepDate(contactId, prepDate.Date);
            }

            if (!orderId.HasValue)
                return false;

            Response.Redirect($"OrderDetail.aspx?{CONST_QRYSTR_ORDERID}={orderId.Value}", true);
            return true;
        }

        #endregion

        #region Header binding and persistence

        private OrderHeaderData ReadHeaderFromControls()
        {
            long contactId = GetEffectiveContactId();
            int? deliveryBy = PreferredDeliveryPersonId;
            if (!deliveryBy.HasValue || deliveryBy.Value <= 0)
            {
                if (TryGetListControlValue(ddlToBeDeliveredBy, out string deliveryValue)
                    && int.TryParse(deliveryValue, out int ddlDelivery)
                    && ddlDelivery > 0)
                {
                    deliveryBy = ddlDelivery;
                }
            }

            if (!deliveryBy.HasValue || deliveryBy.Value <= 0)
                deliveryBy = SystemConstants.DeliveryConstants.DefaultDeliveryPersonID;

            var header = new OrderHeaderData
            {
                OrderID = OrderId,
                CustomerID = contactId,
                ToBeDeliveredBy = deliveryBy.Value,
                Confirmed = cbxConfirmed.Checked,
                InvoiceDone = cbxInvoiceDone.Checked,
                Done = cbxDone.Checked,
                PurchaseOrder = tbxPurchaseOrder.Text ?? string.Empty,
                Notes = tbxNotes.Text ?? string.Empty
            };

            header.OrderDate = DateTime.TryParse(tbxOrderDate.Text, out DateTime orderDate) ? orderDate.Date : DateTime.MinValue;
            header.PrepDate = DateTime.TryParse(tbxPrepDate.Text, out DateTime prepDate) ? prepDate.Date : DateTime.MinValue;
            header.RequiredByDate = DateTime.TryParse(tbxRequiredByDate.Text, out DateTime requiredBy) ? requiredBy.Date : DateTime.MinValue;
            return header;
        }

        private void BindHeaderToControls(OrderHeaderData header)
        {
            if (header == null)
                return;

            SelectContactInCombo(header.CustomerID);
            tbxOrderDate.Text = header.OrderDate.ToString("yyyy-MM-dd");
            tbxPrepDate.Text = header.PrepDate.ToString("yyyy-MM-dd");
            tbxRequiredByDate.Text = header.RequiredByDate.ToString("yyyy-MM-dd");
            tbxPurchaseOrder.Text = header.PurchaseOrder ?? string.Empty;
            tbxNotes.Text = header.Notes ?? string.Empty;
            cbxConfirmed.Checked = header.Confirmed;
            cbxInvoiceDone.Checked = header.InvoiceDone;
            cbxDone.Checked = header.Done;

            BindDeliveryPersonDropdown(forceRebind: false);
            if (header.ToBeDeliveredBy > 0)
            {
                ListItem deliveryItem = ddlToBeDeliveredBy.Items.FindByValue(header.ToBeDeliveredBy.ToString());
                if (deliveryItem != null)
                    deliveryItem.Selected = true;
            }

            UpdateContactLink(header.CustomerID);
        }

        /// <summary>
        /// Saves order details when the user clicks Save.
        /// </summary>
        protected void btnSaveHeader_Click(object sender, EventArgs e)
        {
            TrySaveOrderChanges();
        }

        protected void btnSaveAndReturn_Click(object sender, EventArgs e)
        {
            if (hdnSaveReturnRedirectUrl != null)
                hdnSaveReturnRedirectUrl.Value = string.Empty;

            if (!TrySaveOrderChanges(navigatingAway: true))
                return;

            _redirectingAfterSave = true;
            ReturnToPreviousPage();
        }

        private bool TrySaveOrderChanges(bool navigatingAway = false)
        {
            if (OrderId <= 0)
            {
                SetStatusMessage("Add an item to the order before saving changes.", isError: true);
                return false;
            }

            _orderLineSavedDuringHeaderSave = gvOrderLines != null && gvOrderLines.EditIndex >= 0;
            if (!TrySaveActiveOrderLineEdit(exitEditMode: true))
                return false;

            if (!SavePersistedHeader(navigatingAway: navigatingAway))
                return false;

            if (!navigatingAway)
                ClearHeaderDirtyState();

            return true;
        }

        private void CaptureReturnUrlIfNeeded()
        {
            if (Request.UrlReferrer != null)
            {
                string referrer = Request.UrlReferrer.ToString();
                if (referrer.IndexOf("OrderDetail.aspx", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    Session[SESSION_RETURN_URL] = referrer;
                    return;
                }
            }

            if (Session[SESSION_RETURN_URL] == null)
                Session[SESSION_RETURN_URL] = ResolveUrl(DEFAULT_RETURN_URL);
        }

        private string GetReturnUrl()
        {
            string url = Session[SESSION_RETURN_URL] as string;
            if (string.IsNullOrWhiteSpace(url))
                url = ResolveUrl(DEFAULT_RETURN_URL);

            return url;
        }

        private void ReturnToPreviousPage()
        {
            if (hdnSaveReturnRedirectUrl != null)
                hdnSaveReturnRedirectUrl.Value = GetReturnUrl();

            upnlSaveReturnRedirect?.Update();
        }

        /// <summary>
        /// Writes header fields to OrdersTbl for the current OrderID. Order lines are not updated separately — they follow OrderID.
        /// </summary>
        private bool SavePersistedHeader(int? overrideCustomerId = null, bool navigatingAway = false)
        {
            var header = ReadHeaderFromControls();
            if (overrideCustomerId.HasValue && overrideCustomerId.Value > 0)
                header.CustomerID = overrideCustomerId.Value;

            if (header.CustomerID <= 0)
            {
                SetStatusMessage("Please select a contact.", isError: true);
                pnlOrderHeader.Update();
                return false;
            }

            int? conflictingId = _orderManager.FindDuplicateOrderForHeader(header, OrderId);
            if (conflictingId.HasValue)
            {
                SetMergeableOrder(conflictingId.Value);
                SetStatusMessage(
                    "Another order already exists for this contact and delivery date. Use Merge to combine orders.",
                    log: false);
                pnlOrderHeader.Update();
                return false;
            }

            ClearMergeableOrder();

            OrderHeaderData previousHeader = _orderManager.GetOrderHeader(OrderId);

            if (!_orderManager.UpdateOrderHeader(OrderId, header))
            {
                SetStatusMessage("Could not save your changes.", isError: true);
                pnlOrderHeader.Update();
                return false;
            }

            if (previousHeader != null && HeaderFieldsDiffer(previousHeader, header))
                HeaderUndoSnapshot = CloneHeader(previousHeader);

            SyncSessionForDataSources(header);

            if (navigatingAway)
                return true;

            ApplyHeaderUiState();
            pnlOrderHeader.Update();
            SetStatusMessage(
                _orderLineSavedDuringHeaderSave ? "Order and line changes saved." : "Changes saved.",
                isSuccess: true,
                logMessage: $"Order header saved (prep {header.PrepDate:yyyy-MM-dd}, delivery {header.RequiredByDate:yyyy-MM-dd}).");
            UpdateDuplicateMergeState();
            return true;
        }

        private static OrderHeaderData CloneHeader(OrderHeaderData source)
        {
            if (source == null)
                return null;

            return new OrderHeaderData
            {
                OrderID = source.OrderID,
                CustomerID = source.CustomerID,
                ToBeDeliveredBy = source.ToBeDeliveredBy,
                OrderDate = source.OrderDate,
                PrepDate = source.PrepDate,
                RequiredByDate = source.RequiredByDate,
                Confirmed = source.Confirmed,
                Done = source.Done,
                InvoiceDone = source.InvoiceDone,
                PurchaseOrder = source.PurchaseOrder,
                Notes = source.Notes
            };
        }

        private static bool HeaderFieldsDiffer(OrderHeaderData previous, OrderHeaderData current)
        {
            if (previous == null || current == null)
                return true;

            return previous.CustomerID != current.CustomerID
                || previous.ToBeDeliveredBy != current.ToBeDeliveredBy
                || previous.OrderDate.Date != current.OrderDate.Date
                || previous.PrepDate.Date != current.PrepDate.Date
                || previous.RequiredByDate.Date != current.RequiredByDate.Date
                || previous.Confirmed != current.Confirmed
                || previous.Done != current.Done
                || previous.InvoiceDone != current.InvoiceDone
                || (previous.PurchaseOrder ?? string.Empty) != (current.PurchaseOrder ?? string.Empty)
                || (previous.Notes ?? string.Empty) != (current.Notes ?? string.Empty);
        }

        protected void btnUndoHeader_Click(object sender, EventArgs e)
        {
            if (OrderId <= 0 || HeaderUndoSnapshot == null)
                return;

            var snapshot = HeaderUndoSnapshot;
            snapshot.OrderID = OrderId;

            if (!_orderManager.UpdateOrderHeader(OrderId, snapshot))
            {
                SetStatusMessage("Could not undo the last change.", isError: true);
                return;
            }

            BindHeaderToControls(snapshot);
            SyncSessionForDataSources(snapshot);
            ClearHeaderUndo();
            ClearHeaderDirtyState();
            ApplyHeaderUiState();
            SetStatusMessage("Last change undone.", isSuccess: true);
            pnlOrderHeader.Update();
        }

        private void ClearHeaderUndo()
        {
            HeaderUndoSnapshot = null;
            UpdateHeaderUndoButton();
        }

        private void UpdateHeaderUndoButton()
        {
            if (btnUndoHeader == null)
                return;

            bool show = OrderId > 0 && HeaderUndoSnapshot != null;
            btnUndoHeader.Visible = true;
            btnUndoHeader.Style["display"] = show ? "inline-block" : "none";
        }

        private void RegisterAsyncPostBackControls()
        {
            var scriptManager = ScriptManager.GetCurrent(Page);
            if (scriptManager == null)
                return;

            scriptManager.RegisterAsyncPostBackControl(cboContacts);
            scriptManager.RegisterAsyncPostBackControl(btnSaveHeader);
            scriptManager.RegisterAsyncPostBackControl(btnSaveAndReturn);
            scriptManager.RegisterAsyncPostBackControl(btnUndoHeader);
            scriptManager.RegisterAsyncPostBackControl(btnLastOrder);
            scriptManager.RegisterAsyncPostBackControl(btnNewItem);
            scriptManager.RegisterAsyncPostBackControl(btnAdd);
            scriptManager.RegisterAsyncPostBackControl(btnCancel);
            scriptManager.RegisterAsyncPostBackControl(btnUseExistingOrder);
            scriptManager.RegisterAsyncPostBackControl(btnCreateNewOrderAnyway);
            scriptManager.RegisterAsyncPostBackControl(btnOpenExistingOrder);
            scriptManager.RegisterAsyncPostBackControl(btnDismissConflict);
            scriptManager.RegisterAsyncPostBackControl(btnMerge);
        }

        /// <summary>Contact id from hdnSelectedContactId — single source of truth on the form.</summary>
        private int CurrentContactId
        {
            get
            {
                if (int.TryParse(hdnSelectedContactId?.Value, out int id) && id > 0)
                    return id;

                if (IsPostBack)
                {
                    id = ReadPostedContactId();
                    if (id > 0)
                        return id;
                }

                return 0;
            }
        }

        private void SetContactId(int contactId)
        {
            SyncContactHiddenField(contactId);
        }

        private void MarkHeaderDirty()
        {
            if (hdnHeaderDirty != null)
                hdnHeaderDirty.Value = "1";

            SetSaveButtonsEnabled(true);
            SetStatusMessage(HeaderUnsavedStatusMessage, log: false);
        }

        private void ClearHeaderDirtyState()
        {
            if (hdnHeaderDirty != null)
                hdnHeaderDirty.Value = "0";

            SetSaveButtonsEnabled(false);

            RegisterPageStartupScript("orderHeaderClearDirty",
                "if (window.TrackerUnsaved) { TrackerUnsaved.clearDirty(); } else if (window.orderHeaderClearDirty) { orderHeaderClearDirty(); }");
        }

        private void SetSaveButtonsEnabled(bool enabled)
        {
            if (btnSaveHeader != null)
                btnSaveHeader.Enabled = enabled;

            if (btnSaveAndReturn != null)
                btnSaveAndReturn.Enabled = enabled;
        }

        private void RegisterHeaderDirtyTracking()
        {
            if (OrderId <= 0 || cbxDone.Checked)
                return;

            if (btnSaveHeader != null)
                btnSaveHeader.Visible = true;

            if (btnSaveAndReturn != null)
                btnSaveAndReturn.Visible = true;

            string[] fieldIds =
            {
                tbxOrderDate?.ClientID,
                tbxPrepDate?.ClientID,
                tbxRequiredByDate?.ClientID,
                ddlToBeDeliveredBy?.ClientID,
                tbxPurchaseOrder?.ClientID,
                cbxConfirmed?.ClientID,
                cbxInvoiceDone?.ClientID,
                tbxNotes?.ClientID,
                cboContacts?.ClientID
            };

            var quotedIds = new List<string>();
            foreach (string fieldId in fieldIds)
            {
                if (!string.IsNullOrWhiteSpace(fieldId))
                    quotedIds.Add("'" + fieldId.Replace("'", "\\'") + "'");
            }

            if (quotedIds.Count == 0)
                return;

            string idsArray = string.Join(", ", quotedIds);
            RegisterPageStartupScript(
                "orderHeaderWireFields",
                $"window.orderHeaderTrackedFieldIds = [{idsArray}]; if (window.orderHeaderWireFields) {{ window.orderHeaderWireFields(window.orderHeaderTrackedFieldIds); }}");

            if (hdnHeaderDirty != null && hdnHeaderDirty.Value == "1")
            {
                SetSaveButtonsEnabled(true);

                RegisterPageStartupScript("orderHeaderMarkDirtyFromServer",
                    "if (window.TrackerUnsaved) { TrackerUnsaved.markDirty(); } else if (window.orderHeaderMarkDirtyFromServer) { orderHeaderMarkDirtyFromServer(); }");
            }
        }

        private void RegisterPageStartupScript(string key, string script)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), key + "_" + OrderId, script, true);
        }

        private void SyncContactHiddenField(int contactId)
        {
            if (hdnSelectedContactId == null)
                return;

            hdnSelectedContactId.Value = contactId > 0 ? contactId.ToString() : string.Empty;
        }

        /// <summary>Contact id from hdnSelectedContactId — set only when user picks a contact.</summary>
        private int ReadPostedContactId()
        {
            int id = ReadFormInt(Request.Form, hdnSelectedContactId?.UniqueID);
            if (id > 0)
                return id;

            if (int.TryParse(hdnSelectedContactId?.Value, out id) && id > 0)
                return id;

            return 0;
        }

        private void ApplyContactSelection(int contactId)
        {
            if (contactId <= 0)
                return;

            SetContactId(contactId);

            if (OrderId <= 0)
            {
                ClearDraftConflictState();
                btnLastOrder.Visible = !cbxDone.Checked && contactId > 0;
            }
            else
            {
                btnLastOrder.Visible = !cbxDone.Checked;
            }
        }

        private static int ReadFormInt(NameValueCollection form, string controlUniqueId)
        {
            if (form == null || string.IsNullOrEmpty(controlUniqueId))
                return 0;

            string formKey = controlUniqueId.Replace('$', '_');
            if (int.TryParse(form[formKey]?.Trim(), out int id) && id > 0)
                return id;

            return 0;
        }

        private int ReadContactIdFromPostedForm()
        {
            int id = ReadPostedContactId();
            if (id > 0)
                return id;

            if (cboContacts != null && IsPostBack && Request.Form != null)
            {
                string textBoxKey = FindComboTextBoxFormKey(cboContacts);
                if (!string.IsNullOrEmpty(textBoxKey))
                {
                    string postedText = Request.Form[textBoxKey]?.Trim();
                    if (!string.IsNullOrWhiteSpace(postedText))
                        return ResolveContactIdFromDisplayText(postedText);
                }
            }

            if (!IsPostBack && cboContacts != null)
                return ResolveContactIdFromDisplayText(cboContacts.Text);

            return 0;
        }

        private void SetNewItemPanelVisible(bool visible)
        {
            if (pnlNewItem != null)
            {
                pnlNewItem.Visible = visible;
                pnlNewItem.Style.Remove("display");
            }

            if (btnAdd != null)
            {
                btnAdd.Visible = visible;
                btnAdd.Style.Remove("display");
            }

            if (btnCancel != null)
            {
                btnCancel.Visible = visible;
                btnCancel.Style.Remove("display");
            }

            // Hide New Item while the add-line form is open (CSS !important was overriding inline display:none).
            if (btnNewItem != null)
                btnNewItem.Visible = !visible;
        }

        private void UpdateNewItemButtonState()
        {
            if (btnNewItem == null)
                return;

            btnNewItem.Enabled = ShouldEnableNewItemButton();
        }

        protected void cboContacts_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (!TryResolveContactIdFromComboPostback(out int contactId))
                {
                    SetContactId(0);
                    if (OrderId <= 0)
                    {
                        btnLastOrder.Visible = false;
                        SetStatusMessage("Select a contact to continue.");
                    }
                    UpdateNewItemButtonState();
                    RefreshOrderDetailPanels();
                    return;
                }

                EnsureContactComboSelectionValid(contactId);
                ApplyContactSelection(contactId);
                RefreshPageTitle();

                if (OrderId <= 0)
                {
                    string contactName = GetSelectedContactDisplayName();
                    SetStatusMessage("Loading customer preferences...", log: false);
                    ApplyContactPreferences(contactId);
                    SetStatusMessage(
                        string.IsNullOrEmpty(contactName)
                            ? $"Contact selected. Prep {tbxPrepDate.Text}, delivery {tbxRequiredByDate.Text}."
                            : $"{contactName} selected. Prep {tbxPrepDate.Text}, delivery {tbxRequiredByDate.Text}.",
                        isSuccess: true);
                }
                else
                {
                    MarkHeaderDirty();
                }

                UpdateNewItemButtonState();
                RefreshOrderDetailPanels();
            }
            catch (Exception ex)
            {
                ReportUserError("Error loading customer preferences", ex);
                RefreshOrderDetailPanels();
            }
        }

        private static bool TryParseContactListValue(string rawValue, out int contactId)
        {
            contactId = 0;
            return !string.IsNullOrWhiteSpace(rawValue)
                && int.TryParse(NormalizeContactId(rawValue), out contactId)
                && contactId > 0;
        }

        private void SyncComboDisplay(int contactId)
        {
            if (cboContacts == null || contactId <= 0 || IsPostBack)
                return;

            string idStr = contactId.ToString();
            ListItem item = cboContacts.Items.FindByValue(idStr);
            if (item == null)
                return;

            item.Selected = true;
            cboContacts.Text = item.Text;
        }

        private void RefreshOrderDetailPanels()
        {
            if (pnlOrderHeader != null && pnlOrderHeader.UpdateMode == UpdatePanelUpdateMode.Conditional)
                pnlOrderHeader.Update();

            RefreshNewItemPanel();
            RefreshStatusPanel();
        }

        private void RefreshNewItemPanel()
        {
            if (upnlNewOrderItem != null && upnlNewOrderItem.UpdateMode == UpdatePanelUpdateMode.Conditional)
                upnlNewOrderItem.Update();
        }

        private void RefreshStatusPanel()
        {
            if (upnlStatus != null && upnlStatus.UpdateMode == UpdatePanelUpdateMode.Conditional)
                upnlStatus.Update();
        }

        private void SetStatusMessage(string message, bool isError = false, bool isSuccess = false, bool log = true, string logMessage = null)
        {
            message = message ?? string.Empty;
            ltrlStatus.Text = message;

            if (pnlStatusMessage != null)
            {
                string cssClass = "status-message";
                if (!string.IsNullOrWhiteSpace(message))
                {
                    if (isError)
                        cssClass += " status-error";
                    else if (isSuccess)
                        cssClass += " status-success";
                    else
                        cssClass += " status-info";
                }

                pnlStatusMessage.CssClass = cssClass;
            }

            if (log && !string.IsNullOrWhiteSpace(message))
            {
                string prefix = OrderId > 0 ? $"Order {OrderId}" : "New order";
                string textForLog = string.IsNullOrWhiteSpace(logMessage) ? message : logMessage;
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"{prefix}: {textForLog}");
            }

            RefreshStatusPanel();
        }

        private string BuildOrderDetailUrl(int orderId)
        {
            return ResolveUrl($"~/Pages/OrderDetail.aspx?{CONST_QRYSTR_ORDERID}={orderId}");
        }

        private string BuildOrderDetailLink(int orderId, string linkText = null)
        {
            if (orderId <= 0)
                return string.Empty;

            string text = HttpUtility.HtmlEncode(linkText ?? "order");
            string url = HttpUtility.HtmlAttributeEncode(BuildOrderDetailUrl(orderId));
            return $"<a href=\"{url}\">{text}</a>";
        }

        /// <summary>
        /// Page title shows the contact name when known — never raw OrderID (not useful in the UI).
        /// </summary>
        private string GetSelectedContactDisplayName()
        {
            if (cboContacts?.SelectedItem != null
                && cboContacts.SelectedItem.Value != "0"
                && !string.IsNullOrWhiteSpace(cboContacts.SelectedItem.Text)
                && !string.Equals(cboContacts.SelectedItem.Text, "none", StringComparison.OrdinalIgnoreCase))
            {
                return cboContacts.SelectedItem.Text.Trim();
            }

            if (!string.IsNullOrWhiteSpace(cboContacts?.Text)
                && cboContacts.Text.IndexOf("Select name", StringComparison.OrdinalIgnoreCase) < 0)
            {
                return cboContacts.Text.Trim();
            }

            long contactId = GetEffectiveContactId();
            if (contactId > 0)
            {
                string name = new ContactsRepository().GetContactNameById((int)contactId);
                if (!string.IsNullOrWhiteSpace(name))
                    return name.Trim();
            }

            return string.Empty;
        }

        private void RefreshPageTitle()
        {
            bool isNew = OrderId <= 0;
            string contactName = GetSelectedContactDisplayName();

            if (!string.IsNullOrEmpty(contactName))
            {
                litPageTitle.Text = contactName;
                Page.Title = contactName;
            }
            else
            {
                litPageTitle.Text = isNew ? "New Order" : "Order Detail";
                Page.Title = litPageTitle.Text;
            }

            if (upnlPageTitle != null && upnlPageTitle.UpdateMode == UpdatePanelUpdateMode.Conditional)
                upnlPageTitle.Update();
        }

        private void ActivatePersistedOrder(int orderId, string message, bool isSuccess = true)
        {
            PersistedOrderId = orderId;

            var header = _orderManager.GetOrderHeader(orderId);
            if (header != null)
            {
                BindHeaderToControls(header);
                SyncSessionForDataSources(header);
            }

            HideNewOrderItemPanel();
            BindOrderLines();
            ClearHeaderDirtyState();
            ApplyHeaderUiState();
            UpdateDuplicateMergeState();
            RefreshPageTitle();
            pnlOrderHeader.Update();
            upnlOrderLines.Update();
            SetStatusMessage(message, isSuccess: isSuccess);

            string url = ResolveUrl($"~/Pages/OrderDetail.aspx?{CONST_QRYSTR_ORDERID}={orderId}");
            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                "orderUrlSync",
                $"if (window.history && window.history.replaceState) {{ window.history.replaceState(null, document.title, '{url}'); }}",
                true);
        }

        /// <summary>
        /// Sets prep/delivery dates, delivery person, PO hint, and default item from contact preferences (new orders only).
        /// </summary>
        private void ApplyContactPreferences(int contactId)
        {
            if (contactId <= 0 || OrderId > 0)
                return;

            DateTime deliveryDate = TimeZoneUtils.Now().Date;
            DateTime prepDate = new TrackerTools().GetNextPreparationDateByCustomerID(contactId, ref deliveryDate);

            if (prepDate <= DateTime.MinValue || deliveryDate <= DateTime.MinValue)
            {
                var fallback = _orderManager.CalculateOrderDates(TimeZoneUtils.Now().Date);
                prepDate = fallback.PrepDate;
                deliveryDate = fallback.deliveryDate;
            }

            tbxPrepDate.Text = prepDate.ToString("yyyy-MM-dd");
            tbxRequiredByDate.Text = deliveryDate.ToString("yyyy-MM-dd");

            var prefs = new TrackerTools().RetrieveCustomerPrefs(contactId);
            int deliveryId = prefs.PreferredDeliveryByID > 0
                ? prefs.PreferredDeliveryByID
                : SystemConstants.DeliveryConstants.DefaultDeliveryPersonID;

            int dayOfWeek = (int)(deliveryDate.DayOfWeek + 1);
            if (!IsNormalDeliveryDoW(deliveryId, dayOfWeek))
            {
                deliveryId = SystemConstants.DeliveryConstants.DefaultDeliveryPersonID;
                if (!string.IsNullOrEmpty(tbxNotes.Text))
                    tbxNotes.Text += " ";
                tbxNotes.Text += "Default delivery person changed due to day-of-week.";
            }

            PreferredDeliveryPersonId = deliveryId;
            LastContactIdForDeliveryPref = contactId;
            BindDeliveryPersonDropdown(forceRebind: true, preservePersonId: deliveryId);

            if (prefs.RequiresPurchOrder && string.IsNullOrWhiteSpace(tbxPurchaseOrder.Text))
                tbxPurchaseOrder.Text = SystemConstants.UIConstants.PORequiredText;

            if (prefs.PreferedItem > 0 && cboNewItemDesc != null
                && EnsureListControlSelectionByValue(cboNewItemDesc, prefs.PreferedItem.ToString()))
            {
                tbxNewQuantityOrdered.Text = prefs.PreferedQty.ToString();
            }
        }

        private void SyncSessionForDataSources(OrderHeaderData header)
        {
            if (header == null)
                return;

            Session[SystemConstants.SessionConstants.BoundCustomerID] = header.CustomerID;
            Session[SystemConstants.SessionConstants.BoundDeliveryDate] = header.RequiredByDate.Date;
            Session[SystemConstants.SessionConstants.BoundNotes] = header.Notes ?? string.Empty;
        }

        private void SetHeaderFieldsEnabled(bool enabled)
        {
            tbxOrderDate.Enabled = enabled;
            tbxPrepDate.Enabled = enabled;
            tbxRequiredByDate.Enabled = enabled;
            ddlToBeDeliveredBy.Enabled = enabled;
            tbxPurchaseOrder.Enabled = enabled;
            cbxConfirmed.Enabled = enabled;
            cbxInvoiceDone.Enabled = enabled;
            tbxNotes.Enabled = enabled;
        }

        private void ApplyHeaderUiState()
        {
            bool orderDone = cbxDone.Checked;

            if (cboContacts != null)
                cboContacts.Enabled = !orderDone;

            bool contactSelected = GetEffectiveContactId() > 0;
            if (!orderDone)
                SetHeaderFieldsEnabled(true);

            if (btnSaveHeader != null)
                btnSaveHeader.Visible = OrderId > 0 && !orderDone;

            if (btnSaveAndReturn != null)
                btnSaveAndReturn.Visible = OrderId > 0 && !orderDone;

            if (OrderId <= 0 || orderDone)
                SetSaveButtonsEnabled(false);

            btnNewItem.Enabled = ShouldEnableNewItemButton();
            btnConfirmOrder.Enabled = OrderId > 0;
            btnMerge.Visible = MergeableOrderId > 0 && OrderId > 0 && !orderDone;
            btnMerge.Enabled = MergeableOrderId > 0 && OrderId > 0 && !orderDone;
            btnOrderDelivered.Enabled = OrderId > 0 && !orderDone;
            btnUnDoDone.Enabled = OrderId > 0 && orderDone;
            btnLastOrder.Visible = contactSelected && !orderDone;
            ApplyCancelOrderButtonAccess(orderDone);
            UpdateHeaderUndoButton();
            RefreshFooterButtonPanel();
        }

        private void RefreshFooterButtonPanel()
        {
            if (updtButtonPanel != null && updtButtonPanel.UpdateMode == UpdatePanelUpdateMode.Conditional)
                updtButtonPanel.Update();
        }

        private void ApplyCancelOrderButtonAccess(bool orderMarkedDone = false)
        {
            if (btnOrderCancelled == null)
                return;

            bool isAdmin = SecurityManager.IsAdmin();
            btnOrderCancelled.Visible = isAdmin;
            btnOrderCancelled.Enabled = isAdmin && OrderId > 0 && !orderMarkedDone;
        }

        #endregion

        #region Duplicate order handling

        private static string BuildHeaderConflictKey(OrderHeaderData header)
        {
            if (header == null || header.CustomerID <= 0 || header.RequiredByDate <= DateTime.MinValue)
                return string.Empty;

            return $"{header.CustomerID}|{header.RequiredByDate:yyyy-MM-dd}";
        }

        private bool ShouldForceNewOrder(OrderHeaderData header)
        {
            string key = BuildHeaderConflictKey(header);
            return !string.IsNullOrEmpty(key)
                && string.Equals(ForceNewHeaderKey, key, StringComparison.Ordinal);
        }

        private void SetForceNewOrder(OrderHeaderData header)
        {
            ForceNewHeaderKey = BuildHeaderConflictKey(header);
        }

        private void ClearPendingConflictActions()
        {
            PendingItemId = 0;
            PendingQty = 0;
            PendingPackagingId = 0;
            PendingLastOrder = false;
        }

        private void ClearDraftConflictState()
        {
            ClearPendingConflictActions();
            ForceNewHeaderKey = null;
            HideOrderConflict();
            ClearMergeableOrder();
        }

        private void ClearMergeableOrder()
        {
            MergeableOrderId = 0;
        }

        private void SetMergeableOrder(int duplicateOrderId)
        {
            MergeableOrderId = duplicateOrderId > 0 ? duplicateOrderId : 0;
            ApplyHeaderUiState();
        }

        private void UpdateDuplicateMergeState()
        {
            if (OrderId <= 0 || cbxDone.Checked)
            {
                ClearMergeableOrder();
                return;
            }

            var header = ReadHeaderFromControls();
            if (header.CustomerID <= 0 || header.RequiredByDate <= DateTime.MinValue)
            {
                ClearMergeableOrder();
                return;
            }

            int? duplicateId = _orderManager.FindDuplicateOrderForHeader(header, OrderId);
            MergeableOrderId = duplicateId ?? 0;
            ApplyHeaderUiState();
        }

        private void ConfigureMergeButtonConfirm()
        {
            if (btnMerge == null)
                return;

            if (btnMerge.Visible && MergeableOrderId > 0 && OrderId > 0)
            {
                string message =
                    "Merge the other order for this contact and delivery date into this order?\n\n" +
                    "All lines from the other order will be moved here and that order will be removed.";
                btnMerge.OnClientClick =
                    "return confirm(\"" + System.Web.HttpUtility.JavaScriptStringEncode(message) + "\");";
            }
            else
            {
                btnMerge.OnClientClick = string.Empty;
            }
        }

        protected void btnMerge_Click(object sender, EventArgs e)
        {
            int mergeFromOrderId = MergeableOrderId;
            int keepOrderId = OrderId;
            if (mergeFromOrderId <= 0 || keepOrderId <= 0)
                return;

            try
            {
                var result = _orderManager.MergeOrderInto(keepOrderId, mergeFromOrderId);
                if (!result.Success)
                {
                    SetStatusMessage(result.Error, isError: true);
                    return;
                }

                ClearMergeableOrder();
                BindOrderLines();
                ApplyHeaderUiState();
                UpdateDuplicateMergeState();
                pnlOrderHeader.Update();
                upnlOrderLines.Update();

                string linesNote = result.LinesMoved == 1
                    ? "1 line was moved."
                    : $"{result.LinesMoved} lines were moved.";
                string message = $"Orders merged. {linesNote}";
                if (MergeableOrderId > 0)
                    message += " Another duplicate order still exists — use Merge again if needed.";
                SetStatusMessage(message, isSuccess: true);
                AppLogger.WriteLog(
                    SystemConstants.LogTypes.Orders,
                    $"Order {keepOrderId}: merged order #{mergeFromOrderId} ({result.LinesMoved} line(s)).");
            }
            catch (Exception ex)
            {
                ReportUserError("Error merging orders", ex);
            }
        }

        private void StorePendingAddItem(int itemId, double qty, int packagingId)
        {
            PendingLastOrder = false;
            PendingItemId = itemId;
            PendingQty = qty;
            PendingPackagingId = packagingId;
        }

        private void ShowOrderConflict(int existingOrderId)
        {
            ConflictOrderId = existingOrderId;
            litConflictMessage.Text =
                "Another order already exists for this contact on the required-by date. <br />" +
                "Merge, create a separate order, open the existing order, or cancel to drop the line.";
            pnlOrderConflictShell.Visible = true;
            upnlOrderConflict?.Update();
            SetStatusMessage(
                "Another order exists for this delivery date. Choose how to continue.",
                log: false);

            string shellId = pnlOrderConflictShell.ClientID;
            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                "scrollOrderConflict",
                $"var el = document.getElementById('{shellId}'); if (el) {{ el.scrollIntoView({{ behavior: 'smooth', block: 'start' }}); }}",
                true);
        }

        private void HideOrderConflict()
        {
            ConflictOrderId = 0;
            pnlOrderConflictShell.Visible = false;
            upnlOrderConflict?.Update();
        }

        protected void btnOpenExistingOrder_Click(object sender, EventArgs e)
        {
            int existingOrderId = ConflictOrderId;
            ClearPendingConflictActions();
            HideOrderConflict();

            if (existingOrderId > 0)
                Response.Redirect($"OrderDetail.aspx?{CONST_QRYSTR_ORDERID}={existingOrderId}", true);
        }

        protected void btnUseExistingOrder_Click(object sender, EventArgs e)
        {
            int existingOrderId = ConflictOrderId;
            if (existingOrderId <= 0)
                return;

            try
            {
                if (PendingLastOrder)
                {
                    CompleteLastOrderItems(existingOrderId, wasDraftOrder: OrderId <= 0);
                    ClearPendingConflictActions();
                    HideOrderConflict();
                    return;
                }

                if (HasPendingAddItem)
                {
                    var addResult = _orderManager.AddOrderLineToOrder(
                        existingOrderId,
                        PendingItemId,
                        PendingQty,
                        PendingPackagingId);
                    if (!addResult.Success)
                    {
                        SetStatusMessage("Error adding item: " + addResult.Error, isError: true);
                        return;
                    }

                    ClearPendingConflictActions();
                    HideOrderConflict();
                    HideNewOrderItemPanel();

                    if (OrderId <= 0)
                    {
                        ActivatePersistedOrder(existingOrderId, "Line merged into the existing order.");
                        return;
                    }

                    BindOrderLines();
                    ApplyHeaderUiState();
                    SetStatusMessage("Line merged into the existing order.", isSuccess: true);
                    pnlOrderHeader.Update();
                    RefreshNewItemPanel();
                    upnlOrderLines.Update();
                    return;
                }

                ClearPendingConflictActions();
                HideOrderConflict();
                ActivatePersistedOrder(existingOrderId, "Opened the existing order.");
            }
            catch (Exception ex)
            {
                ReportUserError("Error adding to existing order", ex);
            }
        }

        protected void btnCreateNewOrderAnyway_Click(object sender, EventArgs e)
        {
            var header = ReadHeaderFromControls();
            SetForceNewOrder(header);
            HideOrderConflict();

            try
            {
                if (PendingLastOrder)
                {
                    CompleteLastOrderItems(orderId: 0, wasDraftOrder: true, forceNewOrder: true);
                    ClearPendingConflictActions();
                    return;
                }

                if (HasPendingAddItem)
                {
                    var ensure = _orderManager.EnsureOrderHeader(header, forceNewOrder: true);
                    if (!ensure.Success)
                    {
                        SetStatusMessage(ensure.Error, isError: true);
                        return;
                    }

                    int itemId = PendingItemId;
                    double qty = PendingQty;
                    int packagingId = PendingPackagingId;
                    ClearPendingConflictActions();

                    var addResult = _orderManager.AddOrderLineToOrder(ensure.OrderId, itemId, qty, packagingId);
                    if (!addResult.Success)
                    {
                        SetStatusMessage("Error adding item: " + addResult.Error, isError: true);
                        return;
                    }

                    HideNewOrderItemPanel();
                    ActivatePersistedOrder(ensure.OrderId, "Order created. Item added.");
                    return;
                }

                ClearPendingConflictActions();
                SetStatusMessage("A new order will be created when you add items.", log: false);
            }
            catch (Exception ex)
            {
                ReportUserError("Error creating new order", ex);
            }
        }

        protected void btnDismissConflict_Click(object sender, EventArgs e)
        {
            ClearPendingConflictActions();
            HideOrderConflict();
            SetStatusMessage("Line not added.", log: false);
            RefreshNewItemPanel();
        }

        private void CompleteLastOrderItems(int orderId, bool wasDraftOrder, bool forceNewOrder = false)
        {
            long effectiveContactId = GetEffectiveContactId();
            int contactId = effectiveContactId > 0
                ? (int)effectiveContactId
                : ResolveContactIdFromCombo();
            if (contactId <= 0)
            {
                SetStatusMessage("Please select a contact first.", isError: true);
                return;
            }

            var header = ReadHeaderFromControls();
            header.CustomerID = contactId;

            if (orderId <= 0)
            {
                var ensure = _orderManager.EnsureOrderHeader(header, forceNewOrder: forceNewOrder);
                if (ensure.IsConflict)
                {
                    ShowOrderConflict(ensure.ConflictingOrderId);
                    PendingLastOrder = true;
                    return;
                }

                if (!ensure.Success)
                {
                    SetStatusMessage(ensure.Error, isError: true);
                    return;
                }

                orderId = ensure.OrderId;
            }

            var lastItems = _orderManager.GetLastOrderItems(contactId, setDates: false);
            if (lastItems.Count == 0)
            {
                SetStatusMessage("No previous order found for this contact.");
                return;
            }

            var lines = new List<OrderTblData>();
            foreach (var item in lastItems)
            {
                lines.Add(new OrderTblData
                {
                    CustomerID = contactId,
                    OrderDate = header.OrderDate,
                    PrepDate = header.PrepDate,
                    RequiredByDate = header.RequiredByDate,
                    ToBeDeliveredBy = header.ToBeDeliveredBy,
                    PurchaseOrder = header.PurchaseOrder,
                    Confirmed = header.Confirmed,
                    InvoiceDone = header.InvoiceDone,
                    Done = header.Done,
                    Notes = header.Notes,
                    ItemTypeID = item.ItemID,
                    QuantityOrdered = item.Qty,
                    PrepTypeID = item.PrepTypeID,
                    PackagingID = item.PackagingID
                });
            }

            var addResult = _orderManager.AddOrderLines(header, lines, orderId);
            if (!addResult.Success)
            {
                SetStatusMessage("Error adding last order items: " + addResult.Error, isError: true);
                return;
            }

            if (wasDraftOrder || OrderId <= 0)
            {
                ActivatePersistedOrder(addResult.OrderId, "Last order items added.");
                return;
            }

            BindOrderLines();
            ApplyHeaderUiState();
            SetStatusMessage("Last order items added.", isSuccess: true);
            pnlOrderHeader.Update();
            upnlOrderLines.Update();
        }

        #endregion

        #region Order lines

        private void BindOrderLines()
        {
            if (OrderId <= 0)
            {
                // Bind an empty list so EmptyDataTemplate ("Please add items…") always renders.
                gvOrderLines.DataSource = new List<OrderDetailData>();
                gvOrderLines.DataBind();
                if (upnlOrderLines != null && upnlOrderLines.UpdateMode == UpdatePanelUpdateMode.Conditional)
                    upnlOrderLines.Update();
                return;
            }

            gvOrderLines.DataSource = _orderManager.GetOrderLines(OrderId);
            gvOrderLines.DataBind();
            upnlOrderLines.Update();
        }

        protected void btnNewItem_Click(object sender, EventArgs e)
        {
            if (!ShouldEnableNewItemButton())
            {
                SetStatusMessage("Please select a contact before adding items.", isError: true);
                return;
            }

            SetNewItemPanelVisible(true);
            BindNewItemLookups();
            SetStatusMessage(string.Empty, log: false);
            RefreshNewItemPanel();
        }

        protected void btnCancel_Click(object sender, EventArgs e) => HideNewOrderItemPanel();

        private void HideNewOrderItemPanel()
        {
            SetNewItemPanelVisible(false);
            RefreshNewItemPanel();
        }

        protected void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                var header = ReadHeaderFromControls();
                if (header.CustomerID <= 0)
                {
                    int contactId = ResolveContactIdFromCombo();
                    if (contactId > 0)
                        header.CustomerID = contactId;
                }

                if (header.CustomerID <= 0)
                {
                    SetStatusMessage("Please select a contact before adding items.", isError: true);
                    return;
                }

                int itemId = ResolveComboIntValue(cboNewItemDesc);
                if (itemId <= 0)
                {
                    SetStatusMessage("Please select an item.", isError: true);
                    return;
                }

                if (string.IsNullOrEmpty(tbxNewQuantityOrdered?.Text))
                {
                    SetStatusMessage("Please enter a quantity.", isError: true);
                    return;
                }

                double qty = Convert.ToDouble(tbxNewQuantityOrdered.Text);
                int packagingId = ResolveComboIntValue(cboNewPackaging);

                int orderId = OrderId;
                bool isNewOrder = orderId <= 0;

                if (isNewOrder)
                {
                    var ensure = _orderManager.EnsureOrderHeader(header, forceNewOrder: ShouldForceNewOrder(header));
                    if (ensure.IsConflict)
                    {
                        StorePendingAddItem(itemId, qty, packagingId);
                        ShowOrderConflict(ensure.ConflictingOrderId);
                        RefreshNewItemPanel();
                        return;
                    }

                    if (!ensure.Success)
                    {
                        SetStatusMessage(ensure.Error, isError: true);
                        return;
                    }

                    orderId = ensure.OrderId;
                }

                var addResult = _orderManager.AddOrderLineToOrder(orderId, itemId, qty, packagingId);
                if (!addResult.Success)
                {
                    SetStatusMessage("Error adding item: " + addResult.Error, isError: true);
                    return;
                }

                if (isNewOrder)
                {
                    ActivatePersistedOrder(orderId, "Order created. Item added.");
                    return;
                }

                HideNewOrderItemPanel();
                BindOrderLines();
                ApplyHeaderUiState();
                SetStatusMessage("Item added.", isSuccess: true);
                pnlOrderHeader.Update();
                RefreshNewItemPanel();
                upnlOrderLines.Update();
            }
            catch (Exception ex)
            {
                ReportUserError("Error adding item", ex);
            }
        }

        protected void btnLastOrder_Click(object sender, EventArgs e)
        {
            long effectiveContactId = GetEffectiveContactId();
            int contactId = effectiveContactId > 0
                ? (int)effectiveContactId
                : ResolveContactIdFromCombo();
            if (contactId <= 0)
            {
                SetStatusMessage("Please select a contact first.", isError: true);
                pnlOrderHeader.Update();
                return;
            }

            try
            {
                bool wasDraftOrder = OrderId <= 0;
                var header = ReadHeaderFromControls();
                header.CustomerID = contactId;

                int orderId = OrderId;
                if (orderId <= 0)
                {
                    var ensure = _orderManager.EnsureOrderHeader(header, forceNewOrder: ShouldForceNewOrder(header));
                    if (ensure.IsConflict)
                    {
                        PendingLastOrder = true;
                        PendingItemId = 0;
                        PendingQty = 0;
                        PendingPackagingId = 0;
                        ShowOrderConflict(ensure.ConflictingOrderId);
                        return;
                    }

                    if (!ensure.Success)
                    {
                        SetStatusMessage(ensure.Error, isError: true);
                        return;
                    }

                    orderId = ensure.OrderId;
                }

                CompleteLastOrderItems(orderId, wasDraftOrder);
            }
            catch (Exception ex)
            {
                ReportUserError("Error loading last order", ex);
                RefreshStatusPanel();
            }
        }

        protected void gvOrderLines_RowUpdated(object sender, GridViewUpdatedEventArgs e)
        {
            gvOrderLines.EditIndex = -1;
            BindOrderLines();
        }

        protected void gvOrderLines_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvOrderLines.EditIndex = -1;
            BindOrderLines();
        }

        protected void gvOrderLines_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvOrderLines.EditIndex = e.NewEditIndex;
            BindOrderLines();
        }

        protected void gvOrderLines_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            e.Cancel = true;
            if (TrySaveEditedOrderLine(e.RowIndex, exitEditMode: true))
                SetStatusMessage("Line updated.", isSuccess: true);
        }

        private bool TrySaveActiveOrderLineEdit(bool exitEditMode)
        {
            if (gvOrderLines == null || gvOrderLines.EditIndex < 0)
                return true;

            return TrySaveEditedOrderLine(gvOrderLines.EditIndex, exitEditMode);
        }

        private bool TrySaveEditedOrderLine(int rowIndex, bool exitEditMode)
        {
            if (rowIndex < 0 || rowIndex >= gvOrderLines.DataKeys.Count)
            {
                SetStatusMessage("Could not identify the order line to update.", isError: true);
                return false;
            }

            if (rowIndex >= gvOrderLines.Rows.Count)
            {
                SetStatusMessage("Could not read the edited order line.", isError: true);
                return false;
            }

            long orderLineId = Convert.ToInt64(gvOrderLines.DataKeys[rowIndex].Value);
            GridViewRow row = gvOrderLines.Rows[rowIndex];
            OrderDetailData existingLine = _orderManager.GetOrderLines(OrderId)
                .FirstOrDefault(l => l.OrderLineID == orderLineId);

            var itemCombo = row.FindControl(CONST_ORDERLINE_ITEM_COMBOBOX_ID) as ComboBox;
            var packagingCombo = row.FindControl(CONST_ORDERLINE_PACKAGING_COMBOBOX_ID) as ComboBox;
            var hdnItem = row.FindControl(CONST_ORDERLINE_HIDDENFIELD_ITEM_ID) as HiddenField;
            var hdnPackaging = row.FindControl(CONST_ORDERLINE_HIDDENFIELD_PACKAGING_ID) as HiddenField;

            int itemId = ResolveGridEditComboValue(
                itemCombo,
                hdnItem,
                existingLine,
                line => line.ItemTypeID,
                BindItemCombo,
                GetItemDescById,
                allowZero: false,
                "0",
                "--Invalid Item--",
                inactiveSuffix: false);

            int packagingId = ResolveGridEditComboValue(
                packagingCombo,
                hdnPackaging,
                existingLine,
                line => line.PackagingID,
                BindPackagingCombo,
                GetPackagingDesc,
                allowZero: true,
                "0",
                "n/a",
                inactiveSuffix: true);

            if (!TryGetUpdatedQuantity(row, out double qty))
            {
                SetStatusMessage("Please enter a valid quantity.", isError: true);
                return false;
            }

            if (itemId <= 0)
            {
                SetStatusMessage("Please select an item.", isError: true);
                return false;
            }

            var header = ReadHeaderFromControls();
            if (!_orderManager.UpdateOrderLine(orderLineId, header.CustomerID, itemId, header.RequiredByDate, qty, packagingId))
            {
                SetStatusMessage("Error updating order line.", isError: true);
                return false;
            }

            if (exitEditMode)
                gvOrderLines.EditIndex = -1;

            BindOrderLines();
            return true;
        }

        private int ResolveGridEditComboValue(
            ComboBox combo,
            HiddenField hidden,
            OrderDetailData existingLine,
            Func<OrderDetailData, int> getExistingId,
            Action<ComboBox> bindCombo,
            Func<int, string> getDescription,
            bool allowZero,
            string fallbackValue,
            string fallbackText,
            bool inactiveSuffix)
        {
            if (combo == null)
                return existingLine != null ? getExistingId(existingLine) : 0;

            string postedText = null;
            int? postedIndex = null;
            if (Request.Form != null)
            {
                string textKey = FindComboTextBoxFormKey(combo);
                if (!string.IsNullOrEmpty(textKey))
                    postedText = Request.Form[textKey]?.Trim();

                string hiddenKey = FindComboHiddenFieldFormKey(combo);
                if (!string.IsNullOrEmpty(hiddenKey)
                    && int.TryParse(Request.Form[hiddenKey]?.Trim(), out int parsedIndex))
                {
                    postedIndex = parsedIndex;
                }
            }

            bindCombo(combo);

            if (existingLine != null)
            {
                EnsureComboBoxSelection(
                    combo,
                    getExistingId(existingLine),
                    getDescription,
                    fallbackValue,
                    fallbackText,
                    inactiveSuffix);
            }

            if (!string.IsNullOrWhiteSpace(postedText))
            {
                for (int i = 0; i < combo.Items.Count; i++)
                {
                    if (!ComboItemTextMatches(combo.Items[i].Text, postedText))
                        continue;

                    if (int.TryParse(combo.Items[i].Value, out int matchedId) && (matchedId > 0 || allowZero))
                        return matchedId;
                }
            }

            if (postedIndex.HasValue
                && postedIndex.Value >= 0
                && postedIndex.Value < combo.Items.Count
                && int.TryParse(combo.Items[postedIndex.Value].Value, out int indexedId)
                && (indexedId > 0 || allowZero))
            {
                return indexedId;
            }

            if (TryGetComboIntValue(combo, allowZero, out int resolvedId))
                return resolvedId;

            int hiddenId = ReadPostedHiddenFieldInt(hidden);
            if (hiddenId > 0 || (allowZero && hiddenId == 0))
                return hiddenId;

            return existingLine != null ? getExistingId(existingLine) : (allowZero ? 0 : 0);
        }

        private static bool ComboItemTextMatches(string itemText, string postedText)
        {
            if (string.IsNullOrWhiteSpace(itemText) || string.IsNullOrWhiteSpace(postedText))
                return false;

            string item = itemText.Trim();
            string posted = postedText.Trim();
            if (string.Equals(item, posted, StringComparison.OrdinalIgnoreCase))
                return true;

            if (item.StartsWith(posted, StringComparison.OrdinalIgnoreCase))
                return true;

            const string inactiveSuffix = "(Inactive)";
            if (item.EndsWith(inactiveSuffix, StringComparison.OrdinalIgnoreCase))
            {
                string withoutInactive = item.Substring(0, item.Length - inactiveSuffix.Length).TrimEnd();
                if (string.Equals(withoutInactive, posted, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private bool TryGetUpdatedQuantity(GridViewRow row, out double qty)
        {
            return TryGetUpdatedQuantity(null, row, out qty);
        }

        private bool TryGetUpdatedQuantity(GridViewUpdateEventArgs e, GridViewRow row, out double qty)
        {
            qty = 0;
            if (e?.NewValues != null && e.NewValues.Contains("QuantityOrdered") && e.NewValues["QuantityOrdered"] != null)
            {
                qty = Convert.ToDouble(e.NewValues["QuantityOrdered"]);
                return true;
            }

            var qtyBox = row.FindControl("tbxQuantityOrdered") as TextBox;
            if (TryReadPostedDouble(qtyBox, out qty))
                return true;

            if (Request.Form == null)
                return false;

            foreach (string key in Request.Form.AllKeys)
            {
                if (string.IsNullOrEmpty(key) || !key.EndsWith("tbxQuantityOrdered", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (double.TryParse(Request.Form[key], out qty))
                    return true;
            }

            return false;
        }

        private int ReadPostedHiddenFieldInt(HiddenField field)
        {
            if (field == null)
                return 0;

            string value = ReadPostedHiddenFieldValue(field);
            return int.TryParse(value, out int id) ? id : 0;
        }

        private string ReadPostedHiddenFieldValue(HiddenField field)
        {
            if (field == null)
                return string.Empty;

            if (Request.Form == null)
                return field.Value ?? string.Empty;

            string formKey = field.UniqueID.Replace('$', '_');
            return Request.Form[formKey] ?? field.Value ?? string.Empty;
        }

        protected void gvOrderLines_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "MoveOneDayOn")
            {
                if (OrderId <= 0)
                {
                    SetStatusMessage("Save the order before moving a line.", isError: true);
                    return;
                }

                if (!int.TryParse(e.CommandArgument?.ToString(), out int rowIndex)
                    || rowIndex < 0
                    || rowIndex >= gvOrderLines.DataKeys.Count)
                {
                    SetStatusMessage("Could not identify the order line to move.", isError: true);
                    return;
                }

                int orderLineId = Convert.ToInt32(gvOrderLines.DataKeys[rowIndex].Value);
                var result = _orderManager.MoveOrderLineToNextWorkingDay(OrderId, orderLineId);
                if (!result.Success)
                {
                    SetStatusMessage(result.Error, isError: true);
                    return;
                }

                if (result.MovedWholeOrder)
                {
                    var header = _orderManager.GetOrderHeader(result.TargetOrderId);
                    if (header != null)
                        BindHeaderToControls(header);
                }

                BindOrderLines();
                UpdateDuplicateMergeState();
                ApplyHeaderUiState();
                upnlOrderLines.Update();
                pnlOrderHeader.Update();

                if (result.MovedWholeOrder)
                {
                    string orderLink = BuildOrderDetailLink(result.TargetOrderId, "Open order");
                    SetStatusMessage(
                        $"Order rescheduled to delivery {result.NewRequiredByDate:yyyy-MM-dd} (prep unchanged). {orderLink}",
                        isSuccess: true,
                        logMessage: $"Order {result.TargetOrderId} rescheduled to delivery {result.NewRequiredByDate:yyyy-MM-dd} (prep unchanged).");
                }
                else
                {
                    string orderLink = BuildOrderDetailLink(result.TargetOrderId, "Open order");
                    SetStatusMessage(
                        $"Line moved to a new order for delivery {result.NewRequiredByDate:yyyy-MM-dd}. {orderLink}",
                        isSuccess: true,
                        logMessage: $"Line moved to new order {result.TargetOrderId} (delivery {result.NewRequiredByDate:yyyy-MM-dd}).");
                }
            }
            else if (e.CommandName == "DeleteOrder")
            {
                DeleteOrderLine(e.CommandArgument.ToString());
                BindOrderLines();
                pnlOrderHeader.Update();
            }
        }

        protected void gvOrderLines_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;
            if ((e.Row.RowState & DataControlRowState.Edit) == 0)
                return;

            var line = e.Row.DataItem as OrderDetailData;
            if (line == null)
                return;

            var itemCombo = e.Row.FindControl(CONST_ORDERLINE_ITEM_COMBOBOX_ID) as ComboBox;
            if (itemCombo != null)
            {
                BindItemCombo(itemCombo);
                EnsureComboBoxSelection(
                    itemCombo,
                    line.ItemTypeID, GetItemDescById, "0", "--Invalid Item--", inactiveSuffix: false);
            }

            var packagingCombo = e.Row.FindControl(CONST_ORDERLINE_PACKAGING_COMBOBOX_ID) as ComboBox;
            if (packagingCombo != null)
            {
                BindPackagingCombo(packagingCombo);
                EnsureComboBoxSelection(
                    packagingCombo,
                    line.PackagingID, GetPackagingDesc, "0", "n/a", inactiveSuffix: true);
            }
        }

        public void DeleteOrderLine(string orderLineId)
        {
            string result = _orderManager.DeleteOrderLine(Convert.ToInt32(orderLineId));
            if (string.IsNullOrEmpty(result))
                SetStatusMessage("Item deleted.", isSuccess: true);
            else
                SetStatusMessage("Error deleting item: " + result, isError: true);
        }

        #endregion

        #region Footer actions

        protected void btnConfirmOrder_Click(object sender, EventArgs e)
        {
            if (OrderId <= 0)
            {
                SetStatusMessage("Save the order before sending confirmation.", isError: true);
                return;
            }

            string contactId = GetEffectiveContactId().ToString();
            ContactEmailDetails contact = GetEmailDetails(contactId);
            OrderHeaderData header = ReadHeaderFromControls();
            header.OrderID = OrderId;

            var orderLines = BuildLineListFromGrid();
            string notes = header.Notes ?? string.Empty;
            var emailManager = new OrderDetailManager();
            bool success = emailManager.SendOrderConfirmation(contact, header, orderLines, notes, out string statusMsg);
            AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Order confirmation sent, status: {statusMsg}");
            string displayMessage = success ? statusMsg : FormatEmailSendError(statusMsg);
            SetStatusMessage(displayMessage, isError: !success, isSuccess: success);
        }

        private static string FormatEmailSendError(string fullMessage)
        {
            if (string.IsNullOrWhiteSpace(fullMessage))
                return "Error sending email. See App_Data/email.log for details.";

            string detail = GetShortUserMessage(fullMessage);
            detail = detail.Replace("ERROR:", string.Empty).Trim();

            int messageIndex = detail.IndexOf("Message:", StringComparison.OrdinalIgnoreCase);
            if (detail.StartsWith("Exception Type:", StringComparison.OrdinalIgnoreCase) && messageIndex >= 0)
                detail = detail.Substring(messageIndex + "Message:".Length).Trim();

            if (detail.IndexOf("connection attempt failed", StringComparison.OrdinalIgnoreCase) >= 0
                || detail.IndexOf("connected host has failed", StringComparison.OrdinalIgnoreCase) >= 0
                || detail.IndexOf("timed out", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                string host = ConfigHelper.GetString("EMailSMTP", "SMTP server");
                string port = ConfigHelper.GetString("EMailPort", "587");
                return $"Mail server {host}:{port} is reachable but SMTP did not complete (TLS or login timed out). " +
                    "Check EMailLogIn/EMailPassword, that SMTP AUTH is enabled for the mailbox, and try increasing EmailTimeout in Web.config. " +
                    "See App_Data/email.log for details.";
            }

            if (detail.IndexOf("535", StringComparison.OrdinalIgnoreCase) >= 0
                || detail.IndexOf("credentials were incorrect", StringComparison.OrdinalIgnoreCase) >= 0
                || detail.IndexOf("Authentication unsuccessful", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                string login = ConfigHelper.GetString("EMailLogIn", "SMTP login");
                return $"SMTP login failed for {login}. Update EMailPassword in Web.config (or Email Diagnostics → Save to Web.config), " +
                    "confirm the password works in Outlook, and ensure SMTP AUTH is enabled for that mailbox in Microsoft 365 admin. " +
                    "Office 365 on port 587 needs StartTls — a passing combo test on '587 None' does not count.";
            }

            if (detail.StartsWith("Error sending email", StringComparison.OrdinalIgnoreCase))
                return detail;

            return $"Error sending email: {detail}";
        }

        protected void btnOrderDelivered_Click(object sender, EventArgs e)
        {
            if (OrderId <= 0)
                return;

            OrderHeaderData headerData = ReadHeaderFromControls();
            headerData.OrderID = OrderId;
            var orderLines = new List<OrderManager.TempOrderLineData>();
            var itemsRepository = new ItemsRepository();

            foreach (GridViewRow row in gvOrderLines.Rows)
            {
                int itemId = GetControlSelectedValue(row, CONST_ORDERLINE_ITEM_COMBOBOX_ID, CONST_ORDERLINE_HIDDENFIELD_ITEM_ID);
                var qtyLbl = (Label)row.FindControl("lblQuantityOrdered");
                int packagingId = GetControlSelectedValue(row, CONST_ORDERLINE_PACKAGING_COMBOBOX_ID, CONST_ORDERLINE_HIDDENFIELD_PACKAGING_ID);

                orderLines.Add(new OrderManager.TempOrderLineData
                {
                    ItemID = itemId,
                    Qty = Convert.ToDouble(qtyLbl.Text),
                    PackagingID = packagingId,
                    ServiceTypeID = itemsRepository.GetServiceTypeForItem(itemId),
                    OriginalOrderID = OrderId
                });
            }

            if (!_orderManager.CompleteOrderDelivery(headerData, orderLines))
            {
                SetStatusMessage("Error preparing order done workflow.", isError: true);
                return;
            }

            Response.Redirect($"OrderDone.aspx?OrderID={OrderId}", true);
        }

        protected void btnCancelled_Click(object sender, EventArgs e)
        {
            if (!SecurityManager.IsAdmin() || OrderId <= 0)
                return;

            string result = _orderManager.DeleteOrderItem(OrderId);
            if (!string.IsNullOrEmpty(result))
            {
                SetStatusMessage(result, isError: true);
                return;
            }

            Response.Redirect("DeliverySheet.aspx", true);
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect(GetReturnUrl(), true);
        }

        protected void btnUnDoDone_Click(object sender, EventArgs e)
        {
            if (OrderId <= 0)
                return;

            string undoResult = _orderManager.UnDoOrderItem(OrderId);
            bool undoOk = string.IsNullOrEmpty(undoResult);
            SetStatusMessage(
                undoOk ? "Order undo completed." : undoResult,
                isSuccess: undoOk,
                isError: !undoOk);
            var header = _orderManager.GetOrderHeader(OrderId);
            if (header != null)
                BindHeaderToControls(header);

            BindOrderLines();
            ApplyHeaderUiState();
            pnlOrderHeader.Update();
        }

        protected void MarkItemAsInvoiced()
        {
            if (OrderId <= 0)
                return;

            _orderManager.MarkItemAsInvoiced(OrderId);
            var header = _orderManager.GetOrderHeader(OrderId);
            if (header != null)
                BindHeaderToControls(header);

            SetStatusMessage("Order marked as invoiced.", isSuccess: true);
            pnlOrderHeader.Update();
        }

        #endregion

        #region Contact helpers

        /// <summary>
        /// Populates the contact combo from ContactsRepository via OrderManager.
        /// Called on initial page load only — not on postback (ViewState keeps the list stable).
        /// </summary>
        private void BindContactDropdown()
        {
            if (cboContacts == null)
                return;

            cboContacts.Items.Clear();
            cboContacts.Items.Add(new ListItem("none", "0"));

            foreach (var contact in _orderManager.GetContactLookups())
            {
                cboContacts.Items.Add(new ListItem(
                    contact.CompanyName,
                    contact.ContactID.ToString()));
            }
        }

        private void SetContactById(string ContactID)
        {
            if (!int.TryParse(ContactID, out int id) || id <= 0)
                return;

            SelectContactInCombo(id);
            if (OrderId <= 0)
                ApplyContactPreferences(id);
            SetHeaderFieldsEnabled(true);
            btnLastOrder.Visible = true;
            if (OrderId <= 0)
                ClearDraftConflictState();
            UpdateNewItemButtonState();
        }

        private void SetContactByName(string companyName, string contactName, string email)
        {
            var result = _orderManager.SetCustomerPreferencesByContact(companyName, contactName, email);
            if (!result.Success)
            {
                SetStatusMessage(result.ErrorMessage, isError: true);
                return;
            }

            SelectContactInCombo(result.CustomerID);
            if (result.UseSundryCustomer && !string.IsNullOrEmpty(result.NoteText))
                tbxNotes.Text = result.NoteText;

            if (OrderId <= 0)
                ApplyContactPreferences((int)result.CustomerID);
            SetHeaderFieldsEnabled(true);
            btnLastOrder.Visible = true;
            if (OrderId <= 0)
                ClearDraftConflictState();
            UpdateNewItemButtonState();
        }

        private void SelectContactInCombo(long contactId)
        {
            SetContactId((int)contactId);
            UpdateContactLink(contactId);

            if (cboContacts == null)
                return;

            if (contactId <= 0)
            {
                cboContacts.ClearSelection();
                if (!IsPostBack)
                    cboContacts.Text = string.Empty;
                return;
            }

            string idStr = contactId.ToString();
            ListItem item = cboContacts.Items.FindByValue(idStr);
            if (item == null)
            {
                var contact = new ContactsRepository().GetById((int)contactId);
                string companyName = contact?.CompanyName;
                if (string.IsNullOrWhiteSpace(companyName))
                    companyName = new ContactsRepository().GetContactNameById((int)contactId);
                if (string.IsNullOrWhiteSpace(companyName))
                    companyName = $"Contact #{contactId}";

                companyName = LookupFormatter.FormatLookupText(companyName, contact?.Enabled);
                item = new ListItem(companyName, idStr);
                cboContacts.Items.Add(item);
            }

            item.Selected = true;
            if (!IsPostBack)
                cboContacts.Text = item.Text;
        }

        private void BindDeliveryPersonDropdown(bool forceRebind, int? preservePersonId = null)
        {
            if (!forceRebind && ddlToBeDeliveredBy.Items.Count > 1)
                return;

            int restoreId = preservePersonId ?? 0;
            if (!preservePersonId.HasValue
                && TryGetListControlValue(ddlToBeDeliveredBy, out string currentValue)
                && int.TryParse(currentValue, out int current)
                && current > 0)
            {
                restoreId = current;
            }

            ddlToBeDeliveredBy.ClearSelection();
            ddlToBeDeliveredBy.Items.Clear();

            ddlToBeDeliveredBy.DataSource = _orderManager.GetDeliveryPersons();
            ddlToBeDeliveredBy.DataTextField = nameof(Person.Abbreviation);
            ddlToBeDeliveredBy.DataValueField = nameof(Person.PersonID);
            ddlToBeDeliveredBy.DataBind();
            if (ddlToBeDeliveredBy.Items.FindByValue("0") == null)
                ddlToBeDeliveredBy.Items.Insert(0, new ListItem("n/a", "0"));

            if (restoreId > 0)
            {
                ListItem restoreItem = ddlToBeDeliveredBy.Items.FindByValue(restoreId.ToString());
                if (restoreItem != null)
                    restoreItem.Selected = true;
            }
        }

        private void ReportUserError(string context, Exception ex)
        {
            string detail = ex?.GetBaseException()?.Message;
            if (string.IsNullOrWhiteSpace(detail))
                detail = "Unknown error";

            SetStatusMessage($"{context}: {detail}", isError: true);
            AppLogger.WriteError(
                SystemConstants.LogTypes.Orders,
                $"{context}: {ex}",
                nameof(OrderDetail));
        }

        private static string GetShortUserMessage(string fullMessage)
        {
            if (string.IsNullOrWhiteSpace(fullMessage))
                return string.Empty;

            int stackIndex = fullMessage.IndexOf("Stack Trace:", StringComparison.OrdinalIgnoreCase);
            string message = stackIndex > 0 ? fullMessage.Substring(0, stackIndex).Trim() : fullMessage.Trim();
            return message.Length > 400 ? message.Substring(0, 397) + "..." : message;
        }

        private void BindNewItemLookups()
        {
            BindItemCombo(cboNewItemDesc);
            BindPackagingCombo(cboNewPackaging);
        }

        private void BindItemCombo(ComboBox combo)
        {
            if (combo == null)
                return;

            combo.ClearSelection();
            combo.Items.Clear();
            combo.DataSource = _orderManager.GetItemLookups();
            combo.DataTextField = nameof(OrderItemLookup.ItemDesc);
            combo.DataValueField = nameof(OrderItemLookup.ItemTypeID);
            combo.DataBind();
        }

        private void BindPackagingCombo(ComboBox combo)
        {
            if (combo == null)
                return;

            combo.ClearSelection();
            combo.Items.Clear();
            combo.DataSource = _orderManager.GetPackagingLookups();
            combo.DataTextField = nameof(OrderPackagingLookup.Description);
            combo.DataValueField = nameof(OrderPackagingLookup.PackagingID);
            combo.DataBind();

            if (combo.Items.FindByValue("0") == null)
                combo.Items.Insert(0, new ListItem("n/a", "0"));
        }

        private void UpdateContactLink(long ContactID)
        {
            if (hlContactHdr == null)
                return;

            if (ContactID > 0)
            {
                hlContactHdr.NavigateUrl = $"~/Pages/ContactDetails.aspx?ID={ContactID}";
                hlContactHdr.Enabled = true;
            }
            else
            {
                hlContactHdr.NavigateUrl = "#";
                hlContactHdr.Enabled = false;
            }
        }

        private long GetEffectiveContactId()
        {
            int id = CurrentContactId;
            if (id > 0)
                return id;

            if (OrderId > 0)
            {
                var header = _orderManager.GetOrderHeader(OrderId);
                if (header != null && header.CustomerID > 0)
                    return header.CustomerID;
            }

            return 0;
        }

        /// <summary>
        /// Disabled combos do not post back; re-select contact from the saved order for display.
        /// On postback the ComboBox already has the user's selection — re-applying breaks Ajax Toolkit.
        /// </summary>
        private void EnsureContactDisplayed()
        {
            if (cboContacts == null || IsPostBack)
                return;

            long contactId = GetEffectiveContactId();
            if (contactId > 0)
                SelectContactInCombo(contactId);
        }

        private int ResolveContactIdFromCombo()
        {
            int id = CurrentContactId;
            if (id > 0)
                return id;

            return TryResolveContactIdFromComboPostback(out id) ? id : 0;
        }

        /// <summary>
        /// Resolves contact id from raw form postback only — never reads ComboBox.SelectedValue/Text/SelectedItem.
        /// Ajax Toolkit HiddenField posts list index; ViewState may hold a stale contact id that makes SelectedValue throw.
        /// </summary>
        private bool TryResolveContactIdFromComboPostback(out int contactId)
        {
            contactId = 0;
            if (cboContacts == null || Request.Form == null)
                return false;

            string hiddenFieldKey = FindComboHiddenFieldFormKey(cboContacts);
            if (!string.IsNullOrEmpty(hiddenFieldKey)
                && int.TryParse(Request.Form[hiddenFieldKey]?.Trim(), out int postedIndex)
                && postedIndex > 0
                && postedIndex < cboContacts.Items.Count
                && TryParseContactListValue(cboContacts.Items[postedIndex].Value, out contactId))
            {
                return true;
            }

            string textBoxKey = FindComboTextBoxFormKey(cboContacts);
            if (!string.IsNullOrEmpty(textBoxKey))
            {
                string postedText = Request.Form[textBoxKey]?.Trim();
                if (!string.IsNullOrWhiteSpace(postedText))
                {
                    contactId = ResolveContactIdFromDisplayText(postedText);
                    if (contactId > 0)
                        return true;
                }
            }

            return false;
        }

        private static bool TryGetListControlValue(ListControl list, out string value)
        {
            value = null;
            if (list == null || list.SelectedIndex < 0 || list.SelectedIndex >= list.Items.Count)
                return false;

            value = list.Items[list.SelectedIndex].Value;
            return !string.IsNullOrEmpty(value);
        }

        private string FindComboHiddenFieldFormKey(Control combo)
        {
            if (Request.Form == null || combo == null)
                return null;

            foreach (string key in Request.Form.AllKeys)
            {
                if (string.IsNullOrEmpty(key))
                    continue;

                if (key.IndexOf(combo.ID, StringComparison.OrdinalIgnoreCase) >= 0
                    && key.IndexOf("HiddenField", StringComparison.OrdinalIgnoreCase) >= 0)
                    return key;
            }

            return null;
        }

        private string FindComboTextBoxFormKey(Control combo)
        {
            if (Request.Form == null || combo == null)
                return null;

            foreach (string key in Request.Form.AllKeys)
            {
                if (string.IsNullOrEmpty(key))
                    continue;

                if (key.IndexOf(combo.ID, StringComparison.OrdinalIgnoreCase) >= 0
                    && key.IndexOf("TextBox", StringComparison.OrdinalIgnoreCase) >= 0)
                    return key;
            }

            return null;
        }

        private int ResolveContactIdByCompanyName(string displayText)
        {
            string name = StripDisabledLookupPrefix(displayText?.Trim() ?? string.Empty);
            if (string.IsNullOrEmpty(name))
                return 0;

            var contact = new ContactsRepository().GetByContactNamePreferEnabled(name);
            return contact?.ContactID ?? 0;
        }

        private int ResolveContactIdFromDisplayText(string displayText)
        {
            if (string.IsNullOrWhiteSpace(displayText)
                || string.Equals(displayText.Trim(), "none", StringComparison.OrdinalIgnoreCase))
                return 0;

            if (cboContacts == null)
                return ResolveContactIdByCompanyName(displayText.Trim());

            string text = displayText.Trim();
            int enabledMatch = 0;
            int disabledMatch = 0;
            foreach (ListItem item in cboContacts.Items)
            {
                if (!ContactDisplayTextMatches(item.Text, text))
                    continue;

                if (!int.TryParse(NormalizeContactId(item.Value), out int id) || id <= 0)
                    continue;

                if (item.Text.StartsWith("_"))
                    disabledMatch = id;
                else
                    enabledMatch = id;
            }

            if (enabledMatch > 0)
                return enabledMatch;
            if (disabledMatch > 0)
                return disabledMatch;

            return ResolveContactIdByCompanyName(text);
        }

        private static bool ContactDisplayTextMatches(string itemText, string enteredText)
        {
            if (string.IsNullOrWhiteSpace(itemText) || string.IsNullOrWhiteSpace(enteredText))
                return false;

            string a = itemText.Trim();
            string b = enteredText.Trim();
            if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase))
                return true;

            return string.Equals(StripDisabledLookupPrefix(a), StripDisabledLookupPrefix(b), StringComparison.OrdinalIgnoreCase);
        }

        private static string StripDisabledLookupPrefix(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text.StartsWith("_") ? text.Substring(1) : text;
        }

        private static string NormalizeContactId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "0";
            return value.Trim();
        }

        private int ResolveComboIntValue(ComboBox combo)
        {
            if (combo == null)
                return 0;

            if (GetSafeComboSelectedIndex(combo) >= 0
                && int.TryParse(combo.Items[combo.SelectedIndex].Value, out int selectedId) && selectedId > 0)
            {
                return selectedId;
            }

            if (Request.Form == null)
                return 0;

            if (TryResolveComboIntFromForm(combo, out int postedId))
                return postedId;

            return 0;
        }

        private int ResolveOrderLineComboValue(ComboBox combo, Action<ComboBox> bindCombo, bool allowZero)
        {
            if (combo == null)
                return 0;

            if (TryGetComboIntValue(combo, allowZero, out int value))
                return value;

            bindCombo?.Invoke(combo);
            ApplyPostedComboSelection(combo);

            if (TryGetComboIntValue(combo, allowZero, out value))
                return value;

            return allowZero ? 0 : value;
        }

        private bool TryGetComboIntValue(ComboBox combo, bool allowZero, out int value)
        {
            value = 0;
            if (combo == null)
                return false;

            if (GetSafeComboSelectedIndex(combo) >= 0
                && int.TryParse(combo.Items[combo.SelectedIndex].Value, out value)
                && (value > 0 || allowZero))
            {
                return true;
            }

            return Request.Form != null && TryResolveComboIntFromForm(combo, out value, allowZero);
        }

        private int ResolveOrderLineComboValue(ComboBox combo, Action<ComboBox> bindCombo)
        {
            return ResolveOrderLineComboValue(combo, bindCombo, allowZero: false);
        }

        private bool TryResolveComboIntFromForm(ComboBox combo, out int value)
        {
            return TryResolveComboIntFromForm(combo, out value, allowZero: false);
        }

        private bool TryResolveComboIntFromForm(ComboBox combo, out int value, bool allowZero)
        {
            value = 0;
            if (combo == null || Request.Form == null)
                return false;

            string textBoxKey = FindComboTextBoxFormKey(combo);
            if (!string.IsNullOrEmpty(textBoxKey))
            {
                string postedText = Request.Form[textBoxKey]?.Trim();
                if (!string.IsNullOrWhiteSpace(postedText)
                    && TryResolveComboIntFromDisplayText(combo, postedText, out value, allowZero))
                {
                    return true;
                }
            }

            string hiddenFieldKey = FindComboHiddenFieldFormKey(combo);
            if (!string.IsNullOrEmpty(hiddenFieldKey)
                && int.TryParse(Request.Form[hiddenFieldKey]?.Trim(), out int postedIndex)
                && postedIndex >= 0
                && postedIndex < combo.Items.Count
                && int.TryParse(combo.Items[postedIndex].Value, out value)
                && (value > 0 || allowZero))
            {
                return true;
            }

            return false;
        }

        private static bool TryResolveComboIntFromDisplayText(ComboBox combo, string displayText, out int value)
        {
            return TryResolveComboIntFromDisplayText(combo, displayText, out value, allowZero: false);
        }

        private static bool TryResolveComboIntFromDisplayText(ComboBox combo, string displayText, out int value, bool allowZero)
        {
            value = 0;
            if (combo == null || string.IsNullOrWhiteSpace(displayText))
                return false;

            string text = displayText.Trim();
            foreach (ListItem item in combo.Items)
            {
                if (!string.Equals(item.Text?.Trim(), text, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (int.TryParse(item.Value, out value) && (value > 0 || allowZero))
                    return true;
            }

            return false;
        }

        private void ApplyPostedComboSelection(ComboBox combo)
        {
            if (combo == null || Request.Form == null)
                return;

            string textBoxKey = FindComboTextBoxFormKey(combo);
            if (!string.IsNullOrEmpty(textBoxKey))
            {
                string postedText = Request.Form[textBoxKey]?.Trim();
                if (!string.IsNullOrWhiteSpace(postedText))
                {
                    for (int i = 0; i < combo.Items.Count; i++)
                    {
                        if (!string.Equals(combo.Items[i].Text?.Trim(), postedText, StringComparison.OrdinalIgnoreCase))
                            continue;

                        combo.ClearSelection();
                        combo.SelectedIndex = i;
                        return;
                    }
                }
            }

            string hiddenFieldKey = FindComboHiddenFieldFormKey(combo);
            if (!string.IsNullOrEmpty(hiddenFieldKey)
                && int.TryParse(Request.Form[hiddenFieldKey]?.Trim(), out int postedIndex)
                && postedIndex >= 0
                && postedIndex < combo.Items.Count)
            {
                combo.ClearSelection();
                combo.SelectedIndex = postedIndex;
            }
        }

        private bool TryReadPostedDouble(TextBox textBox, out double value)
        {
            value = 0;
            if (textBox == null)
                return false;

            string postedText = ReadPostedTextBoxValue(textBox);
            return !string.IsNullOrWhiteSpace(postedText) && double.TryParse(postedText, out value);
        }

        private string ReadPostedTextBoxValue(TextBox textBox)
        {
            if (textBox == null)
                return string.Empty;

            if (Request.Form == null)
                return textBox.Text ?? string.Empty;

            string formKey = textBox.UniqueID.Replace('$', '_');
            return Request.Form[formKey] ?? textBox.Text ?? string.Empty;
        }

        private bool IsNormalDeliveryDoW(int personId, int dayOfWeek)
        {
            int? normalDoW = _personsRepository.GetNormalDeliveryDoW(personId);
            if (!normalDoW.HasValue || normalDoW.Value == 0)
                return true;
            return normalDoW.Value == dayOfWeek;
        }

        private void ApplyPreferredDeliveryForContact(int contactId)
        {
            if (contactId <= 0)
                return;

            if (LastContactIdForDeliveryPref == contactId && PreferredDeliveryPersonId.HasValue)
            {
                SelectDeliveryPerson(PreferredDeliveryPersonId.Value);
                return;
            }

            var prefs = _orderManager.SetCustomerPreferencesById(contactId.ToString());
            int deliveryId = prefs.PreferredDeliveryByID > 0
                ? prefs.PreferredDeliveryByID
                : SystemConstants.DeliveryConstants.DefaultDeliveryPersonID;

            PreferredDeliveryPersonId = deliveryId;
            LastContactIdForDeliveryPref = contactId;
            SelectDeliveryPerson(deliveryId);
        }

        private void SelectDeliveryPerson(int personId)
        {
            BindDeliveryPersonDropdown(forceRebind: false);
            ListItem item = ddlToBeDeliveredBy.Items.FindByValue(personId.ToString());
            if (item != null)
                item.Selected = true;
        }

        private bool ShouldEnableNewItemButton()
        {
            long contactId = GetEffectiveContactId();
            if (contactId <= 0)
                return false;

            if (contactId == SystemConstants.CustomerConstants.SundryCustomerID)
                return !string.IsNullOrWhiteSpace(tbxNotes?.Text);

            return true;
        }

        #endregion

        #region Display helpers

        public string GetItemDescById(int itemId)
        {
            return itemId > 0 ? new ItemsRepository().GetItemDescById(itemId) : string.Empty;
        }

        public string GetPackagingDesc(int packagingId)
        {
            return packagingId > 0 ? new ItemPackagingsRepository().GetPackagingDescById(packagingId) : string.Empty;
        }

        public string GetItemUoMObj(object itemId)
        {
            return itemId == null ? string.Empty : GetItemUoM(Convert.ToInt32(itemId));
        }

        public string GetItemUoM(int itemId)
        {
            return itemId > 0 ? new ItemsRepository().GetItemUnitOfMeasure(itemId) : string.Empty;
        }

        private List<OrderLineData> BuildLineListFromGrid()
        {
            var lines = new List<OrderLineData>();
            foreach (GridViewRow row in gvOrderLines.Rows)
            {
                var (itemId, itemDesc) = GetControlIdAndDescFromRow(
                    row, CONST_ORDERLINE_ITEM_COMBOBOX_ID, CONST_ORDERLINE_HIDDENFIELD_ITEM_LABEL, CONST_ORDERLINE_HIDDENFIELD_ITEM_ID);
                var qtyLbl = (Label)row.FindControl("lblQuantityOrdered");
                var (packagingIdStr, packagingDesc) = GetControlIdAndDescFromRow(
                    row, CONST_ORDERLINE_PACKAGING_COMBOBOX_ID, CONST_ORDERLINE_HIDDENFIELD_PACKAGING_LABEL, CONST_ORDERLINE_HIDDENFIELD_PACKAGING_ID);

                lines.Add(new OrderLineData
                {
                    ItemID = Convert.ToInt32(itemId),
                    ItemName = itemDesc,
                    Qty = Convert.ToDouble(qtyLbl.Text),
                    PackagingID = int.TryParse(packagingIdStr, out int packagingId) ? packagingId : 0,
                    PackagingName = packagingDesc
                });
            }

            return lines;
        }

        private static (string controlId, string controlDesc) GetControlIdAndDescFromRow(
            GridViewRow row, string comboBoxName, string labelName, string hiddenFieldName)
        {
            var combo = row.FindControl(comboBoxName) as ComboBox;
            if (combo != null && GetSafeComboSelectedIndex(combo) >= 0)
            {
                ListItem item = combo.Items[combo.SelectedIndex];
                return (item.Value, item.Text ?? string.Empty);
            }

            var hidden = row.FindControl(hiddenFieldName) as HiddenField;
            var label = row.FindControl(labelName) as Label;
            if (hidden != null && !string.IsNullOrEmpty(hidden.Value))
                return (hidden.Value, label?.Text ?? string.Empty);

            return (string.Empty, string.Empty);
        }

        private static int GetControlSelectedValue(GridViewRow row, string comboBoxControlName, string hiddenControlName)
        {
            var comboBox = row.FindControl(comboBoxControlName) as ComboBox;
            if (comboBox != null && GetSafeComboSelectedIndex(comboBox) >= 0
                && int.TryParse(comboBox.Items[comboBox.SelectedIndex].Value, out int comboValue))
            {
                return comboValue;
            }

            var hiddenField = row.FindControl(hiddenControlName) as HiddenField;
            if (hiddenField != null && !string.IsNullOrEmpty(hiddenField.Value)
                && int.TryParse(hiddenField.Value, out int hiddenValue))
                return hiddenValue;

            return SystemConstants.DatabaseConstants.InvalidID;
        }

        private static void EnsureComboBoxSelection(
            ComboBox combo, int selectedId, Func<int, string> getDescription,
            string fallbackValue, string fallbackText, bool inactiveSuffix)
        {
            if (combo == null)
                return;

            if (!combo.Items.Cast<ListItem>().Any(item => item.Value == fallbackValue))
                combo.Items.Insert(0, new ListItem(fallbackText, fallbackValue));

            string selectedIdStr = selectedId.ToString();
            if (!combo.Items.Cast<ListItem>().Any(item => item.Value == selectedIdStr) && selectedId > 0)
            {
                string description = getDescription(selectedId);
                if (!string.IsNullOrEmpty(description))
                {
                    string label = inactiveSuffix ? $"{description} (Inactive)" : description;
                    combo.Items.Add(new ListItem(label, selectedIdStr));
                }
            }

            if (!EnsureListControlSelectionByValue(combo, selectedIdStr))
                EnsureListControlSelectionByValue(combo, fallbackValue);
        }

        private ContactEmailDetails GetEmailDetails(string contactId)
        {
            if (contactId == SystemConstants.CustomerConstants.SundryCustomerIDStr)
            {
                string email = _orderManager.ExtractEmailFromNotes(tbxNotes.Text);
                return string.IsNullOrEmpty(email) ? null : new ContactEmailDetails { EmailAddress = email };
            }

            return new ContactsRepository().GetContactEmailDetails(Convert.ToInt32(contactId));
        }

        #endregion
    }
}
