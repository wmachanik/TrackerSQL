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
    /// Order detail page keyed by OrderID. Header uses plain controls; changes persist when OrderID exists.
    /// </summary>
    public partial class OrderDetail : Page
    {
        public const string CONST_QRYSTR_ORDERID = "OrderID";
        public const string CONST_QRYSTR_NEWORDER = "NewOrder";
        public const string CONST_QRYSTR_DELIVERYDATE = "DeliveryDate";
        public const string CONST_QRYSTR_NOTES = "Notes";
        public const string CONST_QRYSTR_DELIVERED = "Delivered";
        public const string CONST_QRYSTR_INVOICED = "Invoiced";
        public const string CONST_QRYSTR_CustomerID = "CustomerID";

        private const string CONST_ORDERLINE_ITEM_COMBOBOX_ID = "cboItemDesc";
        private const string CONST_ORDERLINE_HIDDENFIELD_ITEM_LABEL = "lblItemDesc";
        private const string CONST_ORDERLINE_HIDDENFIELD_ITEM_ID = "hdnItemTypeID";
        private const string CONST_ORDERLINE_PACKAGING_COMBOBOX_ID = "cboPackaging";
        private const string CONST_ORDERLINE_HIDDENFIELD_PACKAGING_LABEL = "lblPackagingDesc";
        private const string CONST_ORDERLINE_HIDDENFIELD_PACKAGING_ID = "hdnPackagingID";
        private const string CONST_ORDERLINE_HIDDENFIELD_ORDER_ID = "hdnOrderID";

        private const string VSKEY_CONFLICT_ORDER_ID = "ConflictOrderId";
        private const string VSKEY_HEADER_UNDO = "HeaderUndoSnapshot";
        private const string VSKEY_SELECTED_CONTACT = "SelectedContactId";
        private const string VSKEY_PREFERRED_DELIVERY = "PreferredDeliveryPersonId";
        private const string VSKEY_LAST_CONTACT_DELIVERY = "LastContactIdForDeliveryPref";

        private readonly OrderManager _orderManager = new OrderManager();
        private readonly PersonsRepository _personsRepository = new PersonsRepository();

        private int OrderId
        {
            get
            {
                if (int.TryParse(Request.QueryString[CONST_QRYSTR_ORDERID], out int orderId) && orderId > 0)
                    return orderId;
                return 0;
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

        private int SelectedContactId
        {
            get => ViewState[VSKEY_SELECTED_CONTACT] as int? ?? 0;
            set => ViewState[VSKEY_SELECTED_CONTACT] = value;
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            if (!IsPostBack || OrderId > 0)
                return;

            int contactId = ReadContactIdFromPostedForm();
            if (contactId > 0)
                SelectedContactId = contactId;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncPostBackControls();

            if (!IsPostBack)
            {
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
            else
            {
                BindContactDropdown(forceRebind: false);
                CaptureContactSelectionFromForm();
            }
        }

        protected void Page_LoadComplete(object sender, EventArgs e)
        {
            if (!IsPostBack)
                return;

            CaptureContactSelectionFromForm();
            ApplyHeaderUiState();
            UpdateNewItemButtonState();
            UpdateHeaderUndoButton();
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            EnsureContactDisplayed();
            UpdateNewItemButtonState();
            UpdateHeaderUndoButton();
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
            litPageTitle.Text = "New Order";
            Page.Title = "New Order";

            DateTime orderDate = TimeZoneUtils.Now().Date;
            var (prepDate, deliveryDate) = _orderManager.CalculateOrderDates(orderDate);
            tbxOrderDate.Text = orderDate.ToString("yyyy-MM-dd");
            tbxPrepDate.Text = prepDate.ToString("yyyy-MM-dd");
            tbxRequiredByDate.Text = deliveryDate.ToString("yyyy-MM-dd");

            cboContacts.SelectedIndex = -1;
            SelectedContactId = 0;
            BindContactDropdown(forceRebind: true);
            BindDeliveryPersonDropdown(forceRebind: true);
            BindNewItemLookups();
            btnLastOrder.Visible = false;
            btnConfirmOrder.Enabled = false;
            btnOrderDelivered.Enabled = false;
            btnUnDoDone.Enabled = false;
            ltrlStatus.Text = "Select a contact, then add items.";
            ClearHeaderUndo();
            SetNewItemPanelVisible(false);
            UpdateNewItemButtonState();
        }

        private void LoadExistingOrder(int orderId)
        {
            var header = _orderManager.GetOrderHeader(orderId);
            if (header == null)
            {
                ltrlStatus.Text = "Order not found.";
                return;
            }

            ClearHeaderUndo();
            SelectedContactId = (int)header.CustomerID;
            BindContactDropdown(forceRebind: true);
            BindHeaderToControls(header);
            BindNewItemLookups();
            SyncSessionForDataSources(header);
            BindOrderLines();
            litPageTitle.Text = $"Order #{orderId}";
            Page.Title = $"Order #{orderId}";
            ltrlStatus.Text = string.Empty;
            btnLastOrder.Visible = header.CustomerID > 0;
            SetNewItemPanelVisible(false);
        }

        private bool TryRedirectLegacyOrderUrl()
        {
            if (Request.QueryString[CONST_QRYSTR_ORDERID] != null)
                return false;

            string contactParam = Request.QueryString["ContactID"] ?? Request.QueryString[CONST_QRYSTR_CustomerID];
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
            int deliveryBy = PreferredDeliveryPersonId ?? 0;
            if (deliveryBy <= 0 && int.TryParse(ddlToBeDeliveredBy.SelectedValue, out int ddlDelivery) && ddlDelivery > 0)
                deliveryBy = ddlDelivery;
            if (deliveryBy <= 0)
                deliveryBy = SystemConstants.DeliveryConstants.DefaultDeliveryPersonID;

            var header = new OrderHeaderData
            {
                OrderID = OrderId,
                CustomerID = contactId,
                ToBeDeliveredBy = deliveryBy,
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
            if (header.ToBeDeliveredBy > 0
                && ddlToBeDeliveredBy.Items.FindByValue(header.ToBeDeliveredBy.ToString()) != null)
            {
                ddlToBeDeliveredBy.SelectedValue = header.ToBeDeliveredBy.ToString();
            }

            UpdateContactLink(header.CustomerID);
        }

        /// <summary>
        /// When OrderID exists, header changes save immediately (AutoPostBack). Before that, values stay on the form only.
        /// Lines are linked by OrderID only; changing ContactID on the header moves the whole order to the new contact.
        /// </summary>
        protected void HeaderField_Changed(object sender, EventArgs e)
        {
            if (OrderId <= 0)
            {
                CheckOrderConflict();
                UpdateNewItemButtonState();
                upnlNewOrderItem.Update();
                return;
            }

            SavePersistedHeader();
        }

        /// <summary>
        /// Writes header fields to OrdersTbl for the current OrderID. Order lines are not updated separately — they follow OrderID.
        /// </summary>
        private bool SavePersistedHeader()
        {
            var header = ReadHeaderFromControls();
            if (header.CustomerID <= 0)
            {
                ltrlStatus.Text = "Please select a contact.";
                pnlOrderHeader.Update();
                return false;
            }

            int? conflictingId = _orderManager.FindExistingOrderForHeader(header);
            if (conflictingId.HasValue && conflictingId.Value != OrderId)
            {
                ShowOrderConflict(conflictingId.Value);
                return false;
            }

            OrderHeaderData previousHeader = _orderManager.GetOrderHeader(OrderId);

            if (!_orderManager.UpdateOrderHeader(OrderId, header))
            {
                ltrlStatus.Text = "Error saving order header.";
                pnlOrderHeader.Update();
                return false;
            }

            if (previousHeader != null && HeaderFieldsDiffer(previousHeader, header))
                HeaderUndoSnapshot = CloneHeader(previousHeader);

            SyncSessionForDataSources(header);
            ApplyHeaderUiState();
            pnlOrderHeader.Update();
            upnlNewOrderItem.Update();
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
                ltrlStatus.Text = "Could not undo header change.";
                upnlNewOrderItem.Update();
                return;
            }

            BindHeaderToControls(snapshot);
            SyncSessionForDataSources(snapshot);
            ClearHeaderUndo();
            ApplyHeaderUiState();
            ltrlStatus.Text = "Header change undone.";
            pnlOrderHeader.Update();
            upnlNewOrderItem.Update();
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
            scriptManager.RegisterAsyncPostBackControl(tbxOrderDate);
            scriptManager.RegisterAsyncPostBackControl(tbxPrepDate);
            scriptManager.RegisterAsyncPostBackControl(tbxRequiredByDate);
            scriptManager.RegisterAsyncPostBackControl(ddlToBeDeliveredBy);
            scriptManager.RegisterAsyncPostBackControl(tbxPurchaseOrder);
            scriptManager.RegisterAsyncPostBackControl(cbxConfirmed);
            scriptManager.RegisterAsyncPostBackControl(cbxInvoiceDone);
            scriptManager.RegisterAsyncPostBackControl(tbxNotes);
            scriptManager.RegisterAsyncPostBackControl(btnUndoHeader);
            scriptManager.RegisterAsyncPostBackControl(btnLastOrder);
            scriptManager.RegisterAsyncPostBackControl(btnNewItem);
            scriptManager.RegisterAsyncPostBackControl(btnAdd);
            scriptManager.RegisterAsyncPostBackControl(btnCancel);
        }

        private void CaptureContactSelectionFromForm()
        {
            // Only sync ViewState from posted form; preferences run in SelectedIndexChanged.
            int contactId = ReadContactIdFromPostedForm();
            if (contactId > 0 && SelectedContactId != contactId)
                SelectedContactId = contactId;
        }

        private void ApplyContactSelection(int contactId)
        {
            if (contactId <= 0)
                return;

            bool contactChanged = contactId != SelectedContactId;
            SelectedContactId = contactId;

            if (contactChanged)
            {
                if (OrderId > 0)
                    ApplyPreferredDeliveryForContact(contactId);
                else
                    CheckOrderConflict();
            }

            if (OrderId <= 0)
            {
                btnLastOrder.Visible = !cbxDone.Checked && contactId > 0;
            }
            else
            {
                btnLastOrder.Visible = !cbxDone.Checked;
            }
        }

        private static int ReadContactIdFromPostedForm(NameValueCollection form)
        {
            if (form == null)
                return 0;

            foreach (string key in form.AllKeys)
            {
                if (string.IsNullOrEmpty(key))
                    continue;

                if (key.IndexOf("cboContacts", StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                if (key.IndexOf("HiddenField", StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                if (int.TryParse(form[key]?.Trim(), out int id) && id > 0)
                    return id;
            }

            return 0;
        }

        private int ReadContactIdFromPostedForm()
        {
            int id = ReadContactIdFromPostedForm(Request.Form);
            if (id > 0)
                return id;

            return ResolveContactIdFromDisplayText(cboContacts?.Text);
        }

        private bool IsExplicitNoneContactSelection()
        {
            int hiddenId = ReadContactIdFromPostedForm(Request.Form);
            if (hiddenId > 0)
                return false;

            if (cboContacts == null)
                return false;

            if (string.Equals(cboContacts.SelectedValue, "0", StringComparison.OrdinalIgnoreCase))
                return true;

            string text = cboContacts.Text?.Trim();
            return string.IsNullOrEmpty(text)
                || string.Equals(text, "none", StringComparison.OrdinalIgnoreCase)
                || string.Equals(text, "----Select name----", StringComparison.OrdinalIgnoreCase);
        }

        private void SetNewItemPanelVisible(bool visible)
        {
            if (pnlNewItem != null)
            {
                pnlNewItem.Visible = true;
                pnlNewItem.Style["display"] = visible ? "block" : "none";
            }

            if (btnAdd != null)
            {
                btnAdd.Visible = true;
                btnAdd.Style["display"] = visible ? "inline-block" : "none";
            }

            if (btnCancel != null)
            {
                btnCancel.Visible = true;
                btnCancel.Style["display"] = visible ? "inline-block" : "none";
            }

            if (btnNewItem != null)
            {
                btnNewItem.Visible = true;
                btnNewItem.Style["display"] = visible ? "none" : "inline-block";
            }
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
                ltrlStatus.Text = "Loading customer preferences...";
                RefreshOrderDetailPanels();

                int contactId = 0;
                if (sender is ListControl listControl
                    && int.TryParse(NormalizeContactId(listControl.SelectedValue), out contactId)
                    && contactId > 0)
                {
                    ProcessContactSelectionFromPostback(contactId);
                    return;
                }

                ProcessContactSelectionFromPostback();
            }
            catch (Exception ex)
            {
                ltrlStatus.Text = "Error loading customer preferences. Please try again.";
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                    $"cboContacts_SelectedIndexChanged failed: {ex}");
                RefreshOrderDetailPanels();
            }
        }

        private void ProcessContactSelectionFromPostback(int knownContactId = 0)
        {
            int contactId = knownContactId > 0 ? knownContactId : ResolveContactIdFromCombo();
            if (contactId <= 0)
                contactId = ReadContactIdFromPostedForm();

            if (contactId <= 0)
            {
                if (IsExplicitNoneContactSelection())
                {
                    SelectedContactId = 0;
                    if (OrderId <= 0)
                    {
                        btnLastOrder.Visible = false;
                        ltrlStatus.Text = "Select a contact to continue.";
                    }
                }

                UpdateNewItemButtonState();
                RefreshOrderDetailPanels();
                return;
            }

            ApplyContactSelection(contactId);

            if (OrderId <= 0)
                ApplyContactPreferences(contactId);
            else
                SavePersistedHeader();

            if (OrderId <= 0)
                ltrlStatus.Text = $"Contact selected (ID {contactId}). Prep {tbxPrepDate.Text}, delivery {tbxRequiredByDate.Text}.";

            UpdateNewItemButtonState();
            RefreshOrderDetailPanels();
        }

        private void RefreshOrderDetailPanels()
        {
            if (pnlOrderHeader != null && pnlOrderHeader.UpdateMode == UpdatePanelUpdateMode.Conditional)
                pnlOrderHeader.Update();

            if (upnlNewOrderItem != null && upnlNewOrderItem.UpdateMode == UpdatePanelUpdateMode.Conditional)
                upnlNewOrderItem.Update();
        }

        /// <summary>
        /// Sets prep/delivery dates, delivery person, and PO hint from contact preferences (new orders).
        /// </summary>
        private void ApplyContactPreferences(int contactId)
        {
            if (contactId <= 0)
                return;

            DateTime deliveryDate = TimeZoneUtils.Now().Date;
            DateTime prepDate = new TrackerTools().GetNextPreperationDateByCustomerID(contactId, ref deliveryDate);

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
            BindDeliveryPersonDropdown(forceRebind: true);
            SelectDeliveryPerson(deliveryId);

            if (prefs.RequiresPurchOrder && string.IsNullOrWhiteSpace(tbxPurchaseOrder.Text))
                tbxPurchaseOrder.Text = SystemConstants.UIConstants.PORequiredText;

            if (cboNewItemDesc != null && cboNewItemDesc.Items.FindByValue(prefs.PreferedItem.ToString()) != null)
            {
                cboNewItemDesc.SelectedValue = prefs.PreferedItem.ToString();
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

            btnNewItem.Enabled = ShouldEnableNewItemButton();
            btnConfirmOrder.Enabled = OrderId > 0;
            btnOrderDelivered.Enabled = OrderId > 0 && !orderDone;
            btnUnDoDone.Enabled = OrderId > 0 && orderDone;
            btnLastOrder.Visible = contactSelected && !orderDone;
            ApplyCancelOrderButtonAccess(orderDone);
            UpdateHeaderUndoButton();
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

        private void CheckOrderConflict()
        {
            if (OrderId > 0)
            {
                HideOrderConflict();
                return;
            }

            var header = ReadHeaderFromControls();
            if (header.CustomerID <= 0 || header.RequiredByDate <= DateTime.MinValue)
            {
                HideOrderConflict();
                return;
            }

            int? existingId = _orderManager.FindExistingOrderForHeader(header);
            if (existingId.HasValue)
                ShowOrderConflict(existingId.Value);
            else
                HideOrderConflict();
        }

        private void ShowOrderConflict(int existingOrderId)
        {
            ConflictOrderId = existingOrderId;
            litConflictMessage.Text =
                $"An order (<strong>#{existingOrderId}</strong>) already exists for this contact and required-by date. " +
                "Open it to view or add your lines there.";
            pnlOrderConflict.Visible = true;
        }

        private void HideOrderConflict()
        {
            ConflictOrderId = 0;
            pnlOrderConflict.Visible = false;
        }

        protected void btnOpenExistingOrder_Click(object sender, EventArgs e)
        {
            if (ConflictOrderId > 0)
                Response.Redirect($"OrderDetail.aspx?{CONST_QRYSTR_ORDERID}={ConflictOrderId}", true);
        }

        protected void btnUseExistingOrder_Click(object sender, EventArgs e)
        {
            btnOpenExistingOrder_Click(sender, e);
        }

        protected void btnDismissConflict_Click(object sender, EventArgs e)
        {
            HideOrderConflict();
        }

        #endregion

        #region Order lines

        private void BindOrderLines()
        {
            if (OrderId <= 0)
            {
                gvOrderLines.DataSource = null;
                gvOrderLines.DataBind();
                return;
            }

            gvOrderLines.DataSource = _orderManager.GetOrderLines(OrderId);
            gvOrderLines.DataBind();
            upnlOrderLines.Update();
        }

        protected void btnNewItem_Click(object sender, EventArgs e)
        {
            CaptureContactSelectionFromForm();

            if (!ShouldEnableNewItemButton())
            {
                ltrlStatus.Text = "Please select a contact before adding items.";
                upnlNewOrderItem.Update();
                return;
            }

            SetNewItemPanelVisible(true);
            BindNewItemLookups();
            ltrlStatus.Text = string.Empty;
            upnlNewOrderItem.Update();
        }

        protected void btnCancel_Click(object sender, EventArgs e) => HideNewOrderItemPanel();

        private void HideNewOrderItemPanel()
        {
            SetNewItemPanelVisible(false);
            upnlNewOrderItem.Update();
        }

        protected void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                var header = ReadHeaderFromControls();
                if (header.CustomerID <= 0)
                {
                    ltrlStatus.Text = "Please select a contact before adding items.";
                    upnlNewOrderItem.Update();
                    return;
                }

                int itemId = ResolveComboIntValue(cboNewItemDesc);
                if (itemId <= 0)
                {
                    ltrlStatus.Text = "Please select an item.";
                    upnlNewOrderItem.Update();
                    return;
                }

                if (string.IsNullOrEmpty(tbxNewQuantityOrdered?.Text))
                {
                    ltrlStatus.Text = "Please enter a quantity.";
                    upnlNewOrderItem.Update();
                    return;
                }

                double qty = Convert.ToDouble(tbxNewQuantityOrdered.Text);
                int packagingId = ResolveComboIntValue(cboNewPackaging);

                int orderId = OrderId;
                bool isNewOrder = orderId <= 0;

                if (isNewOrder)
                {
                    var ensure = _orderManager.EnsureOrderHeader(header);
                    if (ensure.IsConflict)
                    {
                        ShowOrderConflict(ensure.ConflictingOrderId);
                        ltrlStatus.Text = ensure.Error;
                        upnlNewOrderItem.Update();
                        return;
                    }

                    orderId = ensure.OrderId;
                }

                var addResult = _orderManager.AddOrderLineToOrder(orderId, itemId, qty, packagingId);
                if (!addResult.Success)
                {
                    ltrlStatus.Text = "Error adding item: " + addResult.Error;
                    upnlNewOrderItem.Update();
                    return;
                }

                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Item added successfully for order {orderId}");

                if (isNewOrder)
                {
                    Response.Redirect($"OrderDetail.aspx?{CONST_QRYSTR_ORDERID}={orderId}", true);
                    return;
                }

                HideNewOrderItemPanel();
                BindOrderLines();
                ApplyHeaderUiState();
                ltrlStatus.Text = "Item added.";
                pnlOrderHeader.Update();
                upnlNewOrderItem.Update();
                upnlOrderLines.Update();
            }
            catch (Exception ex)
            {
                ltrlStatus.Text = "Error adding item: " + ex.Message;
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"btnAdd_Click error: {ex}");
                upnlNewOrderItem.Update();
            }
        }

        protected void btnLastOrder_Click(object sender, EventArgs e)
        {
            CaptureContactSelectionFromForm();
            long effectiveContactId = GetEffectiveContactId();
            int contactId = effectiveContactId > 0
                ? (int)effectiveContactId
                : ResolveContactIdFromCombo();
            if (contactId <= 0)
            {
                ltrlStatus.Text = "Please select a contact first.";
                pnlOrderHeader.Update();
                upnlNewOrderItem.Update();
                return;
            }

            try
            {
                var header = ReadHeaderFromControls();
                header.CustomerID = contactId;

                int orderId = OrderId;
                if (orderId <= 0)
                {
                    var ensure = _orderManager.EnsureOrderHeader(header, useExistingIfFound: true);
                    if (ensure.IsConflict && ensure.ConflictingOrderId > 0)
                        orderId = ensure.ConflictingOrderId;
                    else if (!ensure.Success)
                    {
                        ltrlStatus.Text = ensure.Error;
                        upnlNewOrderItem.Update();
                        return;
                    }
                    else
                    {
                        orderId = ensure.OrderId;
                    }
                }

                var lastItems = _orderManager.GetLastOrderItems(contactId, setDates: false);
                if (lastItems.Count == 0)
                {
                    ltrlStatus.Text = "No previous order found for this contact.";
                    upnlNewOrderItem.Update();
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
                        PackagingID = item.PackagingID
                    });
                }

                var addResult = _orderManager.AddOrderLines(header, lines, orderId);
                if (!addResult.Success)
                {
                    ltrlStatus.Text = "Error adding last order items: " + addResult.Error;
                    upnlNewOrderItem.Update();
                    return;
                }

                Response.Redirect($"OrderDetail.aspx?{CONST_QRYSTR_ORDERID}={addResult.OrderId}", true);
            }
            catch (Exception ex)
            {
                ltrlStatus.Text = "Error loading last order: " + ex.Message;
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"btnLastOrder_Click error: {ex}");
                upnlNewOrderItem.Update();
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

            GridViewRow row = gvOrderLines.Rows[e.RowIndex];
            long orderLineId = Convert.ToInt64(gvOrderLines.DataKeys[e.RowIndex].Value);
            int itemId = GetControlSelectedValue(row, CONST_ORDERLINE_ITEM_COMBOBOX_ID, CONST_ORDERLINE_HIDDENFIELD_ITEM_ID);
            int packagingId = GetControlSelectedValue(row, CONST_ORDERLINE_PACKAGING_COMBOBOX_ID, CONST_ORDERLINE_HIDDENFIELD_PACKAGING_ID);

            var qtyBox = row.FindControl("tbxQuantityOrdered") as TextBox;
            if (qtyBox == null || !double.TryParse(qtyBox.Text, out double qty))
            {
                ltrlStatus.Text = "Please enter a valid quantity.";
                return;
            }

            var header = ReadHeaderFromControls();
            if (!_orderManager.UpdateOrderLine(orderLineId, header.CustomerID, itemId, header.RequiredByDate, qty, packagingId))
            {
                ltrlStatus.Text = "Error updating order line.";
                return;
            }

            gvOrderLines.EditIndex = -1;
            BindOrderLines();
            ltrlStatus.Text = "Line updated.";
        }

        protected void gvOrderLines_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "MoveOneDayOn")
            {
                if (!DateTime.TryParse(tbxRequiredByDate.Text, out DateTime requiredBy))
                    return;

                DateTime newDate = requiredBy.Date;
                if (newDate.DayOfWeek < DayOfWeek.Friday)
                    newDate = newDate.AddDays(1);
                else
                    newDate = newDate.AddDays((7 - (int)newDate.DayOfWeek + 1) % 7);

                if (OrderId > 0)
                {
                    _orderManager.MoveOrderDeliveryDate(newDate, OrderId);
                    tbxRequiredByDate.Text = newDate.ToString("yyyy-MM-dd");
                    HeaderField_Changed(tbxRequiredByDate, EventArgs.Empty);
                }
            }
            else if (e.CommandName == "DeleteOrder")
            {
                DeleteOrderLine(e.CommandArgument.ToString());
            }

            BindOrderLines();
            pnlOrderHeader.Update();
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
            ltrlStatus.Text = string.IsNullOrEmpty(result) ? "Item deleted" : "Error deleting item: " + result;
            AppLogger.WriteLog(SystemConstants.LogTypes.Orders, ltrlStatus.Text);
        }

        #endregion

        #region Footer actions

        protected void btnConfirmOrder_Click(object sender, EventArgs e)
        {
            if (OrderId <= 0)
            {
                ltrlStatus.Text = "Save the order before sending confirmation.";
                upnlNewOrderItem.Update();
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
            ltrlStatus.Text = statusMsg;
            new showMessageBox(Page, "Order Confirmation", statusMsg);
            upnlNewOrderItem.Update();
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
                ltrlStatus.Text = "Error preparing order done workflow.";
                upnlNewOrderItem.Update();
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
                ltrlStatus.Text = result;
                upnlNewOrderItem.Update();
                return;
            }

            Response.Redirect("DeliverySheet.aspx", true);
        }

        protected void btnUnDoDone_Click(object sender, EventArgs e)
        {
            if (OrderId <= 0)
                return;

            ltrlStatus.Text = _orderManager.UnDoOrderItem(OrderId);
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

            pnlOrderHeader.Update();
        }

        #endregion

        #region Contact helpers

        private void SetContactById(string customerId)
        {
            if (!int.TryParse(customerId, out int id) || id <= 0)
                return;

            SelectContactInCombo(id);
            SelectedContactId = id;
            ApplyPreferredDeliveryForContact(id);
            SetHeaderFieldsEnabled(true);
            btnLastOrder.Visible = true;
            CheckOrderConflict();
            UpdateNewItemButtonState();
        }

        private void SetContactByName(string companyName, string contactName, string email)
        {
            var result = _orderManager.SetCustomerPreferencesByContact(companyName, contactName, email);
            if (!result.Success)
            {
                ltrlStatus.Text = result.ErrorMessage;
                return;
            }

            SelectContactInCombo(result.CustomerID);
            SelectedContactId = (int)result.CustomerID;
            if (result.UseSundryCustomer && !string.IsNullOrEmpty(result.NoteText))
                tbxNotes.Text = result.NoteText;

            ApplyPreferredDeliveryForContact((int)result.CustomerID);
            SetHeaderFieldsEnabled(true);
            btnLastOrder.Visible = true;
            CheckOrderConflict();
            UpdateNewItemButtonState();
        }

        private void SelectContactInCombo(long customerId)
        {
            if (cboContacts == null)
            {
                UpdateContactLink(customerId);
                return;
            }

            if (customerId <= 0)
            {
                cboContacts.ClearSelection();
                cboContacts.Text = string.Empty;
                UpdateContactLink(0);
                return;
            }

            EnsureContactComboItemsLoaded();

            string idStr = customerId.ToString();
            ListItem item = cboContacts.Items.FindByValue(idStr);
            if (item == null)
            {
                var contact = new ContactsRepository().GetById((int)customerId);
                string companyName = contact?.CompanyName;
                if (string.IsNullOrWhiteSpace(companyName))
                    companyName = new ContactsRepository().GetContactNameById((int)customerId);
                if (string.IsNullOrWhiteSpace(companyName))
                    companyName = $"Contact #{customerId}";

                companyName = LookupFormatter.FormatLookupText(companyName, contact?.Enabled);
                item = new ListItem(companyName, idStr);
                cboContacts.Items.Add(item);
            }

            item.Selected = true;
            cboContacts.Text = item.Text;

            SelectedContactId = (int)customerId;
            UpdateContactLink(customerId);
        }

        private void EnsureContactComboItemsLoaded()
        {
            BindContactDropdown(forceRebind: false);
        }

        private void BindContactDropdown(bool forceRebind)
        {
            if (cboContacts == null)
                return;

            bool needsBind = forceRebind
                || !cboContacts.EnableViewState
                || cboContacts.Items.Count <= 1;
            if (!needsBind)
                return;

            int preserveContactId = SelectedContactId;
            if (preserveContactId <= 0)
            {
                int fromPosted = ReadContactIdFromPostedForm();
                if (fromPosted > 0)
                    preserveContactId = fromPosted;
            }

            cboContacts.ClearSelection();
            cboContacts.Items.Clear();
            cboContacts.Items.Add(new ListItem("none", "0"));

            foreach (var contact in _orderManager.GetContactLookups())
            {
                cboContacts.Items.Add(new ListItem(
                    contact.CompanyName,
                    contact.ContactID.ToString()));
            }

            if (preserveContactId > 0)
                SelectContactInCombo(preserveContactId);
        }

        private void BindDeliveryPersonDropdown(bool forceRebind)
        {
            if (!forceRebind && ddlToBeDeliveredBy.Items.Count > 1)
                return;

            ddlToBeDeliveredBy.DataSource = _orderManager.GetDeliveryPersons();
            ddlToBeDeliveredBy.DataTextField = nameof(Person.Abbreviation);
            ddlToBeDeliveredBy.DataValueField = nameof(Person.PersonID);
            ddlToBeDeliveredBy.DataBind();
            if (ddlToBeDeliveredBy.Items.FindByValue("0") == null)
                ddlToBeDeliveredBy.Items.Insert(0, new ListItem("n/a", "0"));
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

            combo.DataSource = _orderManager.GetItemLookups();
            combo.DataTextField = nameof(OrderItemLookup.ItemDesc);
            combo.DataValueField = nameof(OrderItemLookup.ItemTypeID);
            combo.DataBind();
        }

        private void BindPackagingCombo(ComboBox combo)
        {
            if (combo == null)
                return;

            combo.DataSource = _orderManager.GetPackagingLookups();
            combo.DataTextField = nameof(OrderPackagingLookup.Description);
            combo.DataValueField = nameof(OrderPackagingLookup.PackagingID);
            combo.DataBind();

            if (combo.Items.FindByValue("0") == null)
                combo.Items.Insert(0, new ListItem("n/a", "0"));
        }

        private void UpdateContactLink(long customerId)
        {
            if (hlContactHdr == null)
                return;

            if (customerId > 0)
            {
                hlContactHdr.NavigateUrl = $"~/Pages/ContactDetails.aspx?ID={customerId}";
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
            int fromCombo = ResolveContactIdFromCombo();
            if (fromCombo > 0)
            {
                SelectedContactId = fromCombo;
                return fromCombo;
            }

            if (SelectedContactId > 0)
                return SelectedContactId;

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
            if (cboContacts == null)
                return 0;

            int id = ReadContactIdFromPostedForm(Request.Form);
            if (id > 0)
                return id;

            EnsureContactComboItemsLoaded();

            if (int.TryParse(NormalizeContactId(cboContacts.SelectedValue), out id) && id > 0)
                return id;

            if (cboContacts.SelectedItem != null
                && int.TryParse(NormalizeContactId(cboContacts.SelectedItem.Value), out id) && id > 0)
                return id;

            if (cboContacts.SelectedIndex > 0 && cboContacts.SelectedIndex < cboContacts.Items.Count
                && int.TryParse(NormalizeContactId(cboContacts.Items[cboContacts.SelectedIndex].Value), out id) && id > 0)
                return id;

            string comboText = cboContacts.Text?.Trim();
            if (!string.IsNullOrEmpty(comboText))
            {
                foreach (ListItem item in cboContacts.Items)
                {
                    if (ContactDisplayTextMatches(item.Text, comboText)
                        && int.TryParse(NormalizeContactId(item.Value), out id) && id > 0)
                        return id;
                }

                id = ResolveContactIdByCompanyName(comboText);
                if (id > 0)
                    return id;
            }

            return SelectedContactId > 0 ? SelectedContactId : 0;
        }

        private int ResolveContactIdByCompanyName(string displayText)
        {
            string name = StripDisabledLookupPrefix(displayText?.Trim() ?? string.Empty);
            if (string.IsNullOrEmpty(name))
                return 0;

            var contact = new ContactsRepository().GetByContactName(name);
            return contact?.ContactID ?? 0;
        }

        private int ResolveContactIdFromDisplayText(string displayText)
        {
            if (string.IsNullOrWhiteSpace(displayText)
                || string.Equals(displayText.Trim(), "none", StringComparison.OrdinalIgnoreCase))
                return 0;

            string text = displayText.Trim();
            EnsureContactComboItemsLoaded();

            foreach (ListItem item in cboContacts.Items)
            {
                if (!ContactDisplayTextMatches(item.Text, text))
                    continue;

                if (int.TryParse(NormalizeContactId(item.Value), out int id) && id > 0)
                    return id;
            }

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

            if (int.TryParse(combo.SelectedValue, out int id) && id > 0)
                return id;

            if (combo.SelectedItem != null
                && int.TryParse(combo.SelectedItem.Value, out id) && id > 0)
                return id;

            if (Request.Form == null)
                return 0;

            foreach (string key in Request.Form.AllKeys)
            {
                if (string.IsNullOrEmpty(key))
                    continue;

                if (key.IndexOf(combo.ID, StringComparison.OrdinalIgnoreCase) < 0
                    || key.IndexOf("HiddenField", StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                if (int.TryParse(Request.Form[key], out id) && id > 0)
                    return id;
            }

            return 0;
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
            string value = personId.ToString();
            if (ddlToBeDeliveredBy.Items.FindByValue(value) != null)
                ddlToBeDeliveredBy.SelectedValue = value;
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
            if (combo != null && combo.SelectedValue != null)
                return (combo.SelectedValue, combo.SelectedItem?.Text ?? string.Empty);

            var hidden = row.FindControl(hiddenFieldName) as HiddenField;
            var label = row.FindControl(labelName) as Label;
            if (hidden != null && !string.IsNullOrEmpty(hidden.Value))
                return (hidden.Value, label?.Text ?? string.Empty);

            return (string.Empty, string.Empty);
        }

        private static int GetControlSelectedValue(GridViewRow row, string comboBoxControlName, string hiddenControlName)
        {
            var comboBox = row.FindControl(comboBoxControlName) as ComboBox;
            if (comboBox != null && !string.IsNullOrEmpty(comboBox.SelectedValue)
                && int.TryParse(comboBox.SelectedValue, out int comboValue))
                return comboValue;

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

            combo.SelectedValue = combo.Items.Cast<ListItem>().Any(item => item.Value == selectedIdStr)
                ? selectedIdStr
                : fallbackValue;
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
