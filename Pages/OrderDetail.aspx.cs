// Decompiled with JetBrains decompiler
// Type: TrackerSQL.Pages.OrderDetail
// Assembly: TrackerSQL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerSQL.dll

using AjaxControlToolkit;
using AjaxControlToolkit.HtmlEditor.ToolbarButtons;
using Org.BouncyCastle.Asn1.Cmp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Controls;
using TrackerSQL.Managers;
//- only form later versions #nullable disable
namespace TrackerSQL.Pages
{
    public partial class OrderDetail : Page
    {
        // Add new constants for New Order mode
        private const string CONST_NEWORDER_MODE = "NewOrderMode";
        private const string CONST_QRYSTR_NEWORDER = "NewOrder";

        // Add constants from NewOrderDetail for URL parameters
        //public const string CONST_URL_REQUEST_CustomerID = "CoID";
        //public const string CONST_URL_REQUEST_NAME = "Name";
        //public const string CONST_URL_REQUEST_COMPANYNAME = "CoName";
        //public const string CONST_URL_REQUEST_EMAIL = "EMail";
        //public const string CONST_URL_REQUEST_LASTORDER = "LastOrder";
        //public const string CONST_URL_REQUEST_SKU1 = "SKU1";
        // Add missing controls from NewOrderDetail for manual header mode
        protected ComboBox cboManualContacts;
        protected TextBox tbxManualOrderDate;
        protected TextBox tbxManualRoastDate;
        protected TextBox tbxManualRequiredByDate;
        protected DropDownList ddlManualToBeDeliveredBy;
        protected TextBox tbxManualPurchaseOrder;
        protected CheckBox cbxManualConfirmed;
        protected CheckBox cbxManualInvoiceDone;
        protected CheckBox cbxManualDone;
        protected TextBox tbxManualNotes;
        protected Button btnUpdate;
        protected Panel pnlManualHeader;

        // Missing controls from NewOrderDetail for new item section
        //protected ComboBox cboNewItemDesc;
        //protected ComboBox cboNewPackaging;

        // Add constants for NewOrderDetail session management
        private const string CONST_UPDATEORDERLINES = "UpdateOrderLines";
        private const string CONST_ORDERLINESADDED = "OrderLinesAdded";
        private const string CONST_ORDERLINEIDS = "OrderLineIDS";
        private const string CONST_ORDERLINEITEMIDS = "OrderLineItemIDS";
        // Constants for Order Detail
        public const string CONST_EMAILDELIMITERSTART = "[#";
        public const string CONST_EMAILDELIMITEREND = "#]";
        public const string CONST_QRYSTR_CustomerID = "CustomerID";
        public const string CONST_QRYSTR_DELIVERYDATE = "DeliveryDate";
        public const string CONST_QRYSTR_NOTES = "Notes";
        public const string CONST_QRYSTR_DELIVERED = "Delivered";
        public const string CONST_QRYSTR_INVOICED = "Invoiced";
        private const string CONST_FROMEMAIL = "orders@quaffee.co.za";
        private const string CONST_DELIVERYTYPEISCOLLECTION = "Cllct";
        private const string CONST_DELIVERYTYPEISCOURIER = "Cour";
        private const string CONST_ORDERHEADERVALUES = "OrderHeaderValues";
        private const string CONST_ORDERHEADER_CONTACT_ID = "cboContacts";
        private const string CONST_ORDERLINE_ITEM_COMBOBOX_ID = "cboItemDesc";
        private const string CONST_ORDERLINE_HIDDENFIELD_ITEM_LABEL = "lblItemDesc";
        private const string CONST_ORDERLINE_HIDDENFIELD_ITEM_ID = "hdnItemTypeID";
        private const string CONST_ORDERLINE_PACKAGING_COMBOBOX_ID = "cboPackaging";
        private const string CONST_ORDERLINE_HIDDENFIELD_PACKAGING_LABEL = "lblPackaging";
        private const string CONST_ORDERLINE_HIDDENFIELD_PACKAGING_ID = "hdnPackagingID";
        private const string CONST_ORDERLINE_HIDDENFIELD_ORDER_ID = "hdnOrderID";
        protected ScriptManager scrmOrderDetail;
        protected UpdateProgress udtpOrderDetail;
        protected UpdatePanel pnlOrderHeader;
        protected DetailsView dvOrderHeader;
        protected UpdatePanel upnlOrderLines;
        protected GridView gvOrderLines;
        protected UpdatePanel upnlNewOrderItem;
        protected Button btnNewItem;
        protected Panel pnlNewItem;
        protected TextBox tbxNewQuantityOrdered;
        protected Button btnAdd;
        protected Button btnCancel;
        protected Literal ltrlStatus;
        protected UpdatePanel updtButtonPanel;
        protected Button btnNewOrder;
        protected Button btnConfirmOrder;
        protected Button btnDlSheet;
        protected Button btnOrderCancelled;
        protected Button btnUnDoDone;
        protected Button btnOrderDelivered;
        protected ObjectDataSource odsOrderSummary;
        protected SqlDataSource sdsCompanys;
        protected ObjectDataSource odsOrderDetail;
        protected SqlDataSource sdsDeliveryBy;
        protected ObjectDataSource odsItemTypes;
        protected SqlDataSource sdsPackagingTypes;

        // Property to check if we're in New Order mode
        private bool IsNewOrderMode
        {
            get
            {
                return Session[CONST_NEWORDER_MODE] != null && (bool)Session[CONST_NEWORDER_MODE];
            }
            set
            {
                Session[CONST_NEWORDER_MODE] = value;
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                bool isNewOrder = DetermineOrderMode();
                if (isNewOrder) InitializeNewOrderMode();
                else InitializeExistingOrderMode();

                btnOrderCancelled.Enabled = SecurityManager.IsAdmin();   // simplified
                UpdateNewItemButtonStateWithRoleCheck();
                new TrackerTools().ClearTrackerSessionErrorString();
                HandleNonLastOrderQueryStringActions();
            }
            else
            {
                var trackerTools = new TrackerTools();
                var err = trackerTools.GetTrackerSessionErrorString();
                if (!string.IsNullOrEmpty(err))
                {
                    new showMessageBox(Page, "Tracker Error", "ERROR: " + err);
                    trackerTools.SetTrackerSessionErrorString(string.Empty);
                }
            }
        }
        private void HandleNonLastOrderQueryStringActions()
        {
            // Handle Invoiced parameter
            if (this.Request.QueryString[CONST_QRYSTR_INVOICED] != null &&
                this.Request.QueryString[CONST_QRYSTR_INVOICED].Equals("Y"))
            {
                this.MarkItemAsInvoiced();
                return;
            }

            // Handle Delivered parameter
            if (this.Request.QueryString[CONST_QRYSTR_DELIVERED] != null &&
                this.Request.QueryString[CONST_QRYSTR_DELIVERED].Equals("Y"))
            {
                this.btnOrderDelivered_Click(this, EventArgs.Empty);
                return;
            }

            // Handle SKU parameters
            if (IsNewOrderMode && this.Request.QueryString[SystemConstants.UrlParameterConstants.SKU1] != null)
            {
                ProcessSKUParameters();
            }
        }
        protected void Page_PreRenderComplete(object sender, EventArgs e)
        {
            // Only process query string on initial load, not postbacks
            if (!this.IsPostBack && this.Request.QueryString.Count > 0)
            {
                //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "Page_PreRenderComplete: Processing query string parameters");

                // Handle CoID parameter
                if (this.Request.QueryString[SystemConstants.UrlParameterConstants.CustomerID] != null)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Page_PreRenderComplete: Processing CoID={this.Request.QueryString[SystemConstants.UrlParameterConstants.CustomerID]}");
                    SetContactByID(this.Request.QueryString[SystemConstants.UrlParameterConstants.CustomerID]);
                }
                // Handle Name/CoName/Email parameters
                else if (this.Request.QueryString[SystemConstants.UrlParameterConstants.CustomerName] != null)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "Page_PreRenderComplete: Processing Name parameters");
                    SetContactValue(
                        this.Request.QueryString[SystemConstants.UrlParameterConstants.CompanyName],
                        this.Request.QueryString[SystemConstants.UrlParameterConstants.CustomerName],
                        this.Request.QueryString[SystemConstants.UrlParameterConstants.Email]);
                }

                // Handle LastOrder parameter ONLY here, after all controls are properly initialized
                if (IsNewOrderMode && this.Request.QueryString[SystemConstants.UrlParameterConstants.LastOrder] != null &&
                    this.Request.QueryString[SystemConstants.UrlParameterConstants.LastOrder] == "Y")
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "Page_PreRenderComplete: Processing LastOrder=Y");

                    // Ensure session is updated first
                    UpdateSessionFromManualControls();

                    if (ProcessLastOrderRequest(true))
                    {
                        AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "Page_PreRenderComplete: Last order items added, redirecting");
                        RedirectToExistingOrderMode();
                    }
                    else
                    {
                        AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "Page_PreRenderComplete: No last order items found");
                        ltrlStatus.Text = "No previous order found for this customer.";
                    }
                }
            }
        }


        /// <summary>
        /// Determines whether to show New Order mode or Existing Order mode.
        /// Defaults to New Order mode when no query string parameters are present.
        /// </summary>
        /// <returns>True for New Order mode, False for Existing Order mode</returns>
        private bool DetermineOrderMode()
        {
            // If explicitly requesting new order mode, ALWAYS honor it and clear session
            if (Request.QueryString[CONST_QRYSTR_NEWORDER] != null &&
                Request.QueryString[CONST_QRYSTR_NEWORDER].Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "Explicit new order mode requested - clearing session");
                ClearOrderSession(); // Clear session when explicitly requesting new order
                return true; // New Order mode
            }

            // If existing order parameters are present, use existing order mode
            if (Request.QueryString[CONST_QRYSTR_CustomerID] != null ||
                Request.QueryString[CONST_QRYSTR_DELIVERYDATE] != null ||
                Request.QueryString[CONST_QRYSTR_NOTES] != null ||
                Request.QueryString[CONST_QRYSTR_DELIVERED] != null ||
                Request.QueryString[CONST_QRYSTR_INVOICED] != null)
            {
                return false; // Existing Order mode
            }

            // If NewOrderDetail-style parameters are present, use new order mode
            if (Request.QueryString[SystemConstants.UrlParameterConstants.CustomerID] != null ||
                Request.QueryString[SystemConstants.UrlParameterConstants.CustomerName] != null ||
                Request.QueryString[SystemConstants.UrlParameterConstants.CompanyName] != null ||
                Request.QueryString[SystemConstants.UrlParameterConstants.Email] != null ||
                Request.QueryString[SystemConstants.UrlParameterConstants.LastOrder] != null ||
                Request.QueryString[SystemConstants.UrlParameterConstants.SKU1] != null)
            {
                return true; // New Order mode
            }

            // DEFAULT: If no query string parameters at all, assume New Order mode and clear session
            if (Request.QueryString.Count == 0)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "No query parameters - default new order mode, clearing session");
                ClearOrderSession(); // Clear session for clean new order
                return true; // New Order mode (DEFAULT BEHAVIOR)
            }

            // Fallback: if there are unknown parameters, assume New Order mode
            return true; // New Order mode
        }
        /// <summary>
        /// Clears all order-related session variables to ensure clean new order state
        /// </summary>
        private void ClearOrderSession()
        {
            Session.Remove(SystemConstants.SessionConstants.BoundCustomerID);
            Session.Remove(SystemConstants.SessionConstants.BoundDeliveryDate);
            Session.Remove(SystemConstants.SessionConstants.BoundNotes);
            Session.Remove(CONST_ORDERHEADERVALUES);
            Session.Remove(CONST_NEWORDER_MODE);
            Session.Remove(CONST_UPDATEORDERLINES);
            Session.Remove(CONST_ORDERLINESADDED);
            Session.Remove(CONST_ORDERLINEIDS);
            Session.Remove(CONST_ORDERLINEITEMIDS);

            //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "Order session cleared for new order");
        }
        private void InitializeNewOrderMode()
        {
            IsNewOrderMode = true;

            // Use OrderManager for date calculations
            var orderManager = new TrackerSQL.Managers.OrderManager();
            DateTime orderDate = TimeZoneUtils.Now().Date;
            var (roastDate, deliveryDate) = orderManager.CalculateOrderDates(orderDate);

            // Set default session values for new order
            this.Session[SystemConstants.SessionConstants.BoundCustomerID] = (long)0; // Changed: No customer by default
            this.Session[SystemConstants.SessionConstants.BoundDeliveryDate] = deliveryDate.Date;
            this.Session[SystemConstants.SessionConstants.BoundNotes] = string.Empty;
            this.Session[CONST_ORDERHEADERVALUES] = null;

            ProcessNewOrderQueryString();
            ShowManualHeaderMode();
        }
        /// <summary>
        /// Control btnNewItem based on both user roles AND customer selection/notes validation
        /// Enhanced to handle both manual and DetailsView scenarios
        /// </summary>
        private void UpdateNewItemButtonStateWithRoleCheck()
        {
            // First check if user has the required roles
            bool hasPermission = this.User.IsInRole("Administrators") || this.User.IsInRole("AgentManager") || this.User.IsInRole("Agents");

            if (!hasPermission)
            {
                btnNewItem.Enabled = false;
                //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "btnNewItem disabled - user lacks required roles");
                return;
            }

            // User has permission, now check customer/notes validation
            if (IsNewOrderMode)
            {
                bool shouldEnable = true;
                string currentNotes = tbxManualNotes?.Text?.Trim() ?? string.Empty;
                string currentCustomer = cboManualContacts?.SelectedValue ?? "0";

                // Check if customer is selected
                if (cboManualContacts == null || cboManualContacts.SelectedIndex <= 0)
                {
                    shouldEnable = false;
                    //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "New Order: No customer selected");
                }
                // Special validation for default customer ("ZZName") - must have notes
                else if (currentCustomer == SystemConstants.CustomerConstants.SundryCustomerIDStr &&
                         string.IsNullOrEmpty(currentNotes))
                {
                    shouldEnable = false;
                    //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"New Order: Default customer selected but no notes provided. Notes length: {currentNotes.Length}");
                }

                btnNewItem.Enabled = shouldEnable;
                //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"UpdateNewItemButtonStateWithRoleCheck: btnNewItem.Enabled = {shouldEnable} (New Order mode)");
            }
            else
            {
                // In existing order mode, check if we need to validate notes for sundry customer
                string existingOrderCustomer = dvOrderHeaderGetCBoControlSelectedValue(CONST_ORDERHEADER_CONTACT_ID);
                string existingNotes = GetOrderHeaderNotes()?.Trim() ?? string.Empty;

                bool shouldEnable = true;
                if (existingOrderCustomer == SystemConstants.CustomerConstants.SundryCustomerIDStr &&
                    string.IsNullOrEmpty(existingNotes))
                {
                    shouldEnable = false;
                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "Updating an Order: Existing order with default customer but no notes");
                }

                btnNewItem.Enabled = shouldEnable;
                //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"UpdateNewItemButtonStateWithRoleCheck: btnNewItem.Enabled = {shouldEnable} (Existing Order mode)");
            }

            // Update the UpdatePanel to reflect the change
            if (upnlNewOrderItem != null)
            {
                upnlNewOrderItem.Update();
            }
        }
        /// <summary>
        /// Shows the manual header mode for creating a new order
        /// </summary>
        private void ShowManualHeaderMode()
        {
            try
            {
                // Hide DetailsView
                if (pnlOrderHeader != null)
                    pnlOrderHeader.Visible = false;

                // Show manual header panel
                if (pnlManualHeader != null)
                    pnlManualHeader.Visible = true;

                // Initialize manual controls with calculated dates
                var orderManager = new TrackerSQL.Managers.OrderManager();
                DateTime orderDate = TimeZoneUtils.Now().Date;
                var (roastDate, deliveryDate) = orderManager.CalculateOrderDates(orderDate);

                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Initial dates calculated - Order: {orderDate:yyyy-MM-dd}, Roast: {roastDate:yyyy-MM-dd}, Delivery: {deliveryDate:yyyy-MM-dd}");

                // Set default values
                tbxManualOrderDate.Text = orderDate.ToString("yyyy-MM-dd");
                tbxManualRoastDate.Text = roastDate.ToString("yyyy-MM-dd");
                tbxManualRequiredByDate.Text = deliveryDate.ToString("yyyy-MM-dd");

                // Don't set default customer - let user select
                if (cboManualContacts != null)
                {
                    cboManualContacts.SelectedIndex = -1; // No selection
                   // AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "Customer combo cleared");
                }

                // DISABLE CONTROLS UNTIL CUSTOMER IS SELECTED
                SetControlsEnabledState(false);

                // Hide Last Order button initially
                if (btnLastOrder != null)
                {
                    btnLastOrder.Visible = false;
                    //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "btnLastOrder initially hidden");
                }

                // Update page title
                Page.Title = "New Order";
                litPageTitle.Text = "New Order";

                // Disable some buttons that only work with existing orders
                btnConfirmOrder.Enabled = false;
                btnOrderDelivered.Enabled = false;
                btnUnDoDone.Enabled = false;

                // Set initial status message
                ltrlStatus.Text = "Please select a customer to begin creating an order.";

                //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "ShowManualHeaderMode completed");
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Error in ShowManualHeaderMode: {ex}");
            }
        }
        /// <summary>
        /// Enables or disables manual controls based on whether a customer is selected
        /// </summary>
        /// <summary>
        /// Enables or disables manual controls based on whether a customer is selected
        /// </summary>
        private void SetControlsEnabledState(bool enabled)
        {
            try
            {
                //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"SetControlsEnabledState called with enabled: {enabled}");

                // Date controls - disable until customer selected
                if (tbxManualOrderDate != null)
                {
                    tbxManualOrderDate.Enabled = enabled;
                   // AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"tbxManualOrderDate.Enabled = {enabled}");
                }

                if (tbxManualRoastDate != null)
                {
                    tbxManualRoastDate.Enabled = enabled;
                  //  AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"tbxManualRoastDate.Enabled = {enabled}");
                }

                if (tbxManualRequiredByDate != null)
                {
                    tbxManualRequiredByDate.Enabled = enabled;
                   // AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"tbxManualRequiredByDate.Enabled = {enabled}");
                }

                // Other controls
                if (ddlManualToBeDeliveredBy != null) ddlManualToBeDeliveredBy.Enabled = enabled;
                if (tbxManualPurchaseOrder != null) tbxManualPurchaseOrder.Enabled = enabled;
                if (cbxManualConfirmed != null) cbxManualConfirmed.Enabled = enabled;
                if (cbxManualInvoiceDone != null) cbxManualInvoiceDone.Enabled = enabled;
                if (cbxManualDone != null) cbxManualDone.Enabled = enabled;
                if (tbxManualNotes != null) tbxManualNotes.Enabled = enabled;

                // Enable/disable the New Item button too
                UpdateNewItemButtonStateWithRoleCheck();

                //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "SetControlsEnabledState completed");
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Error in SetControlsEnabledState: {ex}");
            }
        }
        // Property to access the correct notes control based on mode
        private TextBox NotesControl
        {
            get
            {
                if (IsNewOrderMode)
                    return tbxManualNotes;
                else
                    return dvOrderHeader.CurrentMode == DetailsViewMode.Edit ?
                           (TextBox)dvOrderHeader.FindControl("tbxNotes") : null;
            }
        }

        // Property to access the correct delivery control based on mode
        private DropDownList DeliveryControl
        {
            get
            {
                return IsNewOrderMode ? ddlManualToBeDeliveredBy :
                       (DropDownList)dvOrderHeader.FindControl("ddlToBeDeliveredBy");
            }
        }
        // Property to access the correct contact control based on mode
        private ComboBox ContactsControl
        {
            get
            {
                return IsNewOrderMode ? cboManualContacts : (ComboBox)dvOrderHeader.FindControl(CONST_ORDERHEADER_CONTACT_ID);
            }
        }
        protected void SetContactByID(string pCoNameID)
        {
            //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"SetContactByID called with ID: {pCoNameID}");

            var contactsControl = ContactsControl;

            // Force databind to ensure ComboBox is populated
            if (contactsControl != null && IsNewOrderMode)
            {
                // Force the ComboBox to databind if it hasn't already
                var cboManualContacts = contactsControl as ComboBox;
                if (cboManualContacts?.Items.Count == 0)
                {
                    cboManualContacts.DataBind();
                    //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"SetContactByID: Force DataBind completed, Items.Count = {cboManualContacts.Items.Count}");
                }
            }

            var notesControl = NotesControl ?? tbxManualNotes;
            var deliveryControl = DeliveryControl;

            if (contactsControl?.Items?.FindByValue(pCoNameID) != null)
            {
                contactsControl.SelectedValue = pCoNameID;
                //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"SetContactByID: Successfully set contact to {pCoNameID}");

                // Enable controls when customer is selected via query string
                if (IsNewOrderMode)
                {
                    SetControlsEnabledState(true);

                    // Show Last Order button if we're in new order mode
                    if (btnLastOrder != null)
                    {
                        btnLastOrder.Visible = true;
                        //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "SetContactByID: Made btnLastOrder visible");
                    }

                    // Update dates for the selected customer
                    UpdateDatesForSelectedCustomer(pCoNameID);
                }

                // Use OrderManager for customer preference logic
                var orderManager = new TrackerSQL.Managers.OrderManager();
                var result = orderManager.SetCustomerPreferencesById(pCoNameID);

                if (result.Success && result.CustomerFound)
                {
                    if (deliveryControl?.Items.FindByValue(result.PreferredDeliveryByID.ToString()) != null)
                        deliveryControl.SelectedValue = result.PreferredDeliveryByID.ToString();
                }

                // Clear any previous error messages
                if (notesControl != null && notesControl.Text.Contains($"ID not found: {pCoNameID}"))
                {
                    notesControl.Text = notesControl.Text.Replace($"ID not found: {pCoNameID}: ", "");
                }
            }
            else
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"SetContactByID: Customer {pCoNameID} not found in ComboBox, using sundry customer");

                if (contactsControl != null)
                    contactsControl.SelectedValue = SystemConstants.CustomerConstants.SundryCustomerIDStr;

                if (notesControl != null)
                {
                    notesControl.Text = $"{notesControl.Text}ID not found: {pCoNameID}: ";
                }
            }

            UpdateSessionFromManualControls();
        }
        protected void SetContactValue(string pCoName, string pName, string pEmail)
        {
            var contactsControl = ContactsControl;
            var notesControl = NotesControl ?? tbxManualNotes;
            if (contactsControl == null) return;

            // Use OrderManager for customer lookup logic
            var orderManager = new TrackerSQL.Managers.OrderManager();
            var result = orderManager.SetCustomerPreferencesByContact(pCoName, pName, pEmail);

            if (result.Success)
            {
                if (result.CustomerFound)
                {
                    // Customer found - set the control value
                    string customerIdStr = result.CustomerID.ToString();
                    if (contactsControl.Items.FindByValue(customerIdStr) != null)
                    {
                        contactsControl.SelectedValue = customerIdStr;
                        var deliveryControl = DeliveryControl;
                        if (deliveryControl?.Items.FindByValue(result.PreferredDeliveryByID.ToString()) != null)
                            deliveryControl.SelectedValue = result.PreferredDeliveryByID.ToString();
                    }
                }
                else if (result.UseSundryCustomer)
                {
                    // Use sundry customer with notes
                    contactsControl.SelectedValue = SystemConstants.CustomerConstants.SundryCustomerIDStr;
                    if (notesControl != null)
                        notesControl.Text = $"{notesControl.Text}{result.NoteText}";
                }
            }
            else
            {
                // Error occurred - log it and use sundry customer
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"SetContactValue error: {result.ErrorMessage}");
                contactsControl.SelectedValue = SystemConstants.CustomerConstants.SundryCustomerIDStr;
            }

            UpdateSessionFromManualControls();
        }

        private void UpdateSessionFromManualControls()
        {
            if (!IsNewOrderMode) return;

            // Update session variables from manual controls
            if (cboManualContacts?.SelectedValue != null)
            {
                int customerId;
                if (int.TryParse(cboManualContacts.SelectedValue, out customerId))
                    this.Session[SystemConstants.SessionConstants.BoundCustomerID] = (long)customerId;
            }

            if (tbxManualRequiredByDate?.Text != null && DateTime.TryParse(tbxManualRequiredByDate.Text, out DateTime deliveryDate))
                this.Session[SystemConstants.SessionConstants.BoundDeliveryDate] = deliveryDate.Date;

            if (tbxManualNotes?.Text != null)
                this.Session[SystemConstants.SessionConstants.BoundNotes] = tbxManualNotes.Text;
        }

        private void ShowDetailsViewMode()
        {
            // Show DetailsView
            if (pnlOrderHeader != null)
                pnlOrderHeader.Visible = true;

            // Hide manual header panel
            if (pnlManualHeader != null)
                pnlManualHeader.Visible = false;

            // Update page title
            Page.Title = "Order Detail";
            litPageTitle.Text = "Order Detail";

            // Enable all buttons for existing orders
            btnConfirmOrder.Enabled = true;
            btnOrderDelivered.Enabled = true;
            btnUnDoDone.Enabled = true;

            // Update button panel
            updtButtonPanel.Update();
        }
        // New order handling
        // Add these event handlers for the manual controls
        protected void cboManualContacts_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Remove the alert for now and add more specific debugging
                // ScriptManager.RegisterStartupScript(this, GetType(), "test", $"alert('Contact changed to: {cboManualContacts.SelectedValue ?? "NULL"}');", true);

                //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"cboManualContacts_SelectedIndexChanged called. SelectedValue: {cboManualContacts.SelectedValue ?? "NULL"}");

                // Store ORIGINAL date values BEFORE any updates
                string originalOrderDate = tbxManualOrderDate.Text;
                string originalRoastDate = tbxManualRoastDate.Text;
                string originalRequiredDate = tbxManualRequiredByDate.Text;

                //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"ORIGINAL dates BEFORE updates - Order: {originalOrderDate}, Roast: {originalRoastDate}, Required: {originalRequiredDate}");

                UpdateSessionFromManualControls();

                // Set customer preferences when contact changes
                if (cboManualContacts.SelectedValue != null && cboManualContacts.SelectedValue != "0" && cboManualContacts.SelectedValue != "")
                {
                    //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "Customer selected, enabling controls and showing last order button");

                    // ENABLE CONTROLS WHEN CUSTOMER IS SELECTED
                    SetControlsEnabledState(true);

                    SetContactByID(cboManualContacts.SelectedValue);
                    UpdateDatesForSelectedCustomer(cboManualContacts.SelectedValue);

                    // Log FINAL date values AFTER all updates
                    string finalOrderDate = tbxManualOrderDate.Text;
                    string finalRoastDate = tbxManualRoastDate.Text;
                    string finalRequiredDate = tbxManualRequiredByDate.Text;

                    //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"FINAL dates AFTER all updates - Order: {finalOrderDate}, Roast: {finalRoastDate}, Required: {finalRequiredDate}");

                    // Show Last Order button using server-side control
                    btnLastOrder.Visible = true;
                    //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"btnLastOrder.Visible set to: {btnLastOrder.Visible}");

                    // Show success message
                    ltrlStatus.Text = $"Customer selected. Req dates: {finalRequiredDate}";
                }
                else
                {
                    //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "No customer selected, disabling controls and hiding last order button");

                    // DISABLE CONTROLS WHEN NO CUSTOMER SELECTED
                    SetControlsEnabledState(false);

                    // Hide Last Order button when no customer selected
                    ResetToDefaultDates();
                    btnLastOrder.Visible = false;

                    ltrlStatus.Text = "Please select a customer to continue.";
                }

                // Force UpdatePanel refresh
                //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "Updating UpdatePanels");
                upnlNewOrderSummary.Update();
                if (upnlNewOrderItem != null)
                    upnlNewOrderItem.Update();

                //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "cboManualContacts_SelectedIndexChanged completed");
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Error in cboManualContacts_SelectedIndexChanged: {ex}");
                ltrlStatus.Text = $"Error: {ex.Message}";
            }
        }
        private void WarnIfClosureConflict(DateTime roastDate, DateTime deliveryDate)
        {
            try
            {
                var provider = new HolidayClosureProvider();
                bool roastClosed = provider.IsClosed(roastDate, true);
                bool deliveryClosed = provider.IsClosed(deliveryDate, false);

                if (!roastClosed && !deliveryClosed)
                    return;

                var adj = provider.AdjustPair(roastDate, deliveryDate);

                string msg = "Selected dates occur during a closure period. " +
                             $"Prep {(roastClosed ? "CLOSED" : "OK")} / Delivery {(deliveryClosed ? "CLOSED" : "OK")}. " +
                             $"Suggested: Prep {adj.Prep:yyyy-MM-dd}, Delivery {adj.Delivery:yyyy-MM-dd}. " +
                             "Please review before saving (dates not auto-adjusted).";

                // Option A: show in a label
                ltrlStatus.Text = msg;

                // Option B: modal / popup (if showMessageBox helper exists)
                new showMessageBox(this.Page, "Closure Warning", msg);

                AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                    $"OrderDetail warning: user chose dates needing closure review. OrigPrep={roastDate:yyyy-MM-dd} OrigDel={deliveryDate:yyyy-MM-dd} SuggestPrep={adj.Prep:yyyy-MM-dd} SuggestDel={adj.Delivery:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "WarnIfClosureConflict failed: " + ex.Message);
            }
        }
        /// <summary>
        /// Updates preparation and delivery dates based on selected customer preferences
        /// </summary>
        private void UpdateDatesForSelectedCustomer(string customerId)
        {
            try
            {
                //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"UpdateDatesForSelectedCustomer called for customer: {customerId}");

                // Convert string to long for the TrackerTools method
                if (!long.TryParse(customerId, out long customerIdLong))
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Invalid customer ID: {customerId}");
                    return;
                }

                // Use TrackerTools to get customer-specific dates based on city
                TrackerTools trackerTools = new TrackerTools();
                DateTime deliveryDate = DateTime.MinValue; // This will be set by reference
                DateTime roastDate = trackerTools.GetNextRoastDateByCustomerID(customerIdLong, ref deliveryDate);
                DateTime orderDate = TimeZoneUtils.Now().Date;
                WarnIfClosureConflict(roastDate, deliveryDate);

                //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Customer-specific dates - Order: {orderDate:yyyy-MM-dd}, Roast: {roastDate:yyyy-MM-dd}, Delivery: {deliveryDate:yyyy-MM-dd}");

                // Store old values for comparison
                string oldOrderDate = tbxManualOrderDate.Text;
                string oldRoastDate = tbxManualRoastDate.Text;
                string oldDeliveryDate = tbxManualRequiredByDate.Text;

                // Update the manual controls with customer-specific dates
                tbxManualOrderDate.Text = orderDate.ToString("yyyy-MM-dd");
                tbxManualRoastDate.Text = roastDate.ToString("yyyy-MM-dd");
                tbxManualRequiredByDate.Text = deliveryDate.ToString("yyyy-MM-dd");

                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Date textbox values updated - Old: {oldOrderDate}, {oldRoastDate}, {oldDeliveryDate} | New: {tbxManualOrderDate.Text}, {tbxManualRoastDate.Text}, {tbxManualRequiredByDate.Text}");

                // Update session immediately
                UpdateSessionFromManualControls();

                //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Session updated for customer {customerId}");
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"UpdateDatesForSelectedCustomer error: {ex}");
                ltrlStatus.Text = $"Error updating dates: {ex.Message}";
            }
        }
        /// <summary>
        /// Resets to default calculated dates when no customer is selected
        /// </summary>
        private void ResetToDefaultDates()
        {
            var orderManager = new TrackerSQL.Managers.OrderManager();
            DateTime orderDate = TimeZoneUtils.Now().Date;
            var (roastDate, deliveryDate) = orderManager.CalculateOrderDates(orderDate);

            tbxManualOrderDate.Text = orderDate.ToString("yyyy-MM-dd");
            tbxManualRoastDate.Text = roastDate.ToString("yyyy-MM-dd");
            tbxManualRequiredByDate.Text = deliveryDate.ToString("yyyy-MM-dd");

            // Clear delivery person selection
            if (ddlManualToBeDeliveredBy != null)
                ddlManualToBeDeliveredBy.SelectedValue = "0";
        }

        protected void tbxManualOrderDate_TextChanged(object sender, EventArgs e)
        {
            UpdateSessionFromManualControls();
        }

        protected void tbxManualRoastDate_TextChanged(object sender, EventArgs e)
        {
            UpdateSessionFromManualControls();
        }

        protected void tbxManualRequiredByDate_TextChanged(object sender, EventArgs e)
        {
            UpdateSessionFromManualControls();
        }

        protected void ddlManualToBeDeliveredBy_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSessionFromManualControls();
        }

        protected void tbxManualPurchaseOrder_TextChanged(object sender, EventArgs e)
        {
            UpdateSessionFromManualControls();
        }

        protected void cbxManualConfirmed_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSessionFromManualControls();
        }

        protected void cbxManualInvoiceDone_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSessionFromManualControls();
        }

        protected void cbxManualDone_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSessionFromManualControls();
        }


        protected void tbxManualNotes_TextChanged(object sender, EventArgs e)
        {
            UpdateSessionFromManualControls();
            // Update New Item button state when notes change
            UpdateNewItemButtonStateWithRoleCheck();
        }
        /// <summary>
        /// Handle notes changed in DetailsView mode (when editing existing orders)
        /// </summary>
        protected void dvOrderHeader_tbxNotes_TextChanged(object sender, EventArgs e)
        {
            if (sender is TextBox notesTextBox)
            {
                HandleNotesChanged(notesTextBox.Text);
            }
        }
        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            // Handle the update button click for manual mode
            UpdateSessionFromManualControls();

            // Show success message
            ltrlStatus.Text = "Order details updated";

            // Hide the update button
            ScriptManager.RegisterStartupScript(this, GetType(), "hideUpdate", "hideUpdateButton();", true);
        }
        private bool ProcessLastOrderRequest(bool pSetDates)
        {
            try
            {
                //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "ProcessLastOrderRequest: Starting");

                // Get customer ID from session with better error handling
                long customerId = 0;
                if (this.Session[SystemConstants.SessionConstants.BoundCustomerID] != null)
                {
                    var sessionValue = this.Session[SystemConstants.SessionConstants.BoundCustomerID];
                    if (sessionValue is long)
                    {
                        customerId = (long)sessionValue;
                    }
                    else if (sessionValue is int)
                    {
                        customerId = (long)(int)sessionValue;
                    }
                    else if (long.TryParse(sessionValue.ToString(), out long parsedId))
                    {
                        customerId = parsedId;
                    }
                }

                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"ProcessLastOrderRequest: Customer ID from session: {customerId}");

                if (customerId <= 0)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "ProcessLastOrderRequest: Invalid customer ID from session");
                    return false;
                }

                var orderManager = new TrackerSQL.Managers.OrderManager();

                // Get the last order items
                List<OrderManager.OrderLineData> lastOrderItems = orderManager.GetLastOrderItems(customerId, pSetDates);

                if (lastOrderItems.Count > 0)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"ProcessLastOrderRequest: Found {lastOrderItems.Count} last order items");

                    // Add each item to the actual order
                    bool anyItemsAdded = false;
                    foreach (var orderLine in lastOrderItems)
                    {
                        // Create the order header data
                        OrderHeaderData headerData = this.Get_dvOrderHeaderData(false);

                        // Create the order data
                        OrderTblData orderData = new OrderTblData
                        {
                            CustomerID = customerId,
                            OrderDate = headerData.OrderDate,
                            RoastDate = headerData.RoastDate,
                            RequiredByDate = headerData.RequiredByDate,
                            ToBeDeliveredBy = headerData.ToBeDeliveredBy,
                            PurchaseOrder = headerData.PurchaseOrder,
                            Confirmed = headerData.Confirmed,
                            InvoiceDone = headerData.InvoiceDone,
                            Done = headerData.Done,
                            Notes = headerData.Notes,
                            ItemTypeID = orderLine.ItemID,
                            QuantityOrdered = orderLine.Qty,
                            PackagingID = orderLine.PackagingID
                        };

                        // Add the order line
                        string result = orderManager.AddOrderLine(headerData, orderData);
                        if (string.IsNullOrEmpty(result))
                        {
                            anyItemsAdded = true;
                            AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"ProcessLastOrderRequest: Successfully added item {orderLine.ItemName}");
                        }
                        else
                        {
                            AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"ProcessLastOrderRequest: Error adding item {orderLine.ItemName}: {result}");
                        }
                    }

                    //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"ProcessLastOrderRequest: Added {lastOrderItems.Count} items, success: {anyItemsAdded}");
                    return anyItemsAdded;
                }
                else
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "ProcessLastOrderRequest: No last order items found");
                    return false;
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"ProcessLastOrderRequest error: {ex.Message}");
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"ProcessLastOrderRequest stack trace: {ex.StackTrace}");
                return false;
            }
        }
        private void RedirectToExistingOrderMode()
        {
            // Build query string for existing order mode
            var customerId = (long)this.Session[SystemConstants.SessionConstants.BoundCustomerID];
            var deliveryDate = (DateTime)this.Session[SystemConstants.SessionConstants.BoundDeliveryDate];
            var notes = (string)this.Session[SystemConstants.SessionConstants.BoundNotes];

            string url = $"OrderDetail.aspx?{CONST_QRYSTR_CustomerID}={customerId}&{CONST_QRYSTR_DELIVERYDATE}={deliveryDate:yyyy-MM-dd}&{CONST_QRYSTR_NOTES}={HttpUtility.UrlEncode(notes)}";
            Response.Redirect(url);
        }

        private void ProcessNewOrderQueryString()
        {
            // Handle CoID parameter - USING THE CONSTANT
            if (this.Request.QueryString[SystemConstants.UrlParameterConstants.CustomerID] != null)
            {
                SetContactByID(this.Request.QueryString[SystemConstants.UrlParameterConstants.CustomerID]);
            }
            // Handle Name/CoName/Email parameters - USING THE CONSTANTS
            else if (this.Request.QueryString[SystemConstants.UrlParameterConstants.CustomerName] != null)
            {
                SetContactValue(
                    this.Request.QueryString[SystemConstants.UrlParameterConstants.CompanyName],
                    this.Request.QueryString[SystemConstants.UrlParameterConstants.CustomerName],
                    this.Request.QueryString[SystemConstants.UrlParameterConstants.Email]);
            }
        }

        private void InitializeExistingOrderMode()
        {
            IsNewOrderMode = false;

            // Original OrderDetail initialization logic - USING THE CONSTANTS
            long num = 1;
            DateTime date = TimeZoneUtils.Now().Date;
            string empty = string.Empty;

            if (this.Request.QueryString[CONST_QRYSTR_CustomerID] != null)
                num = (long)Convert.ToInt32(this.Request.QueryString[CONST_QRYSTR_CustomerID].ToString());
            if (this.Request.QueryString[CONST_QRYSTR_DELIVERYDATE] != null)
                date = Convert.ToDateTime(this.Request.QueryString[CONST_QRYSTR_DELIVERYDATE]).Date;
            if (this.Request.QueryString[CONST_QRYSTR_NOTES] != null)
                empty = this.Request.QueryString[CONST_QRYSTR_NOTES].ToString();

            this.Session[SystemConstants.SessionConstants.BoundCustomerID] = num;
            this.Session[SystemConstants.SessionConstants.BoundDeliveryDate] = date.Date;
            this.Session[SystemConstants.SessionConstants.BoundNotes] = empty;
            this.Session[CONST_ORDERHEADERVALUES] = null;

            // Show DetailsView, hide manual controls
            ShowDetailsViewMode();
        }
        private void ProcessSKUParameters()
        {
            var orderManager = new TrackerSQL.Managers.OrderManager();
            var skuParams = new Dictionary<string, double>();

            // Extract SKU parameters from query string
            if (this.Request.QueryString[SystemConstants.UrlParameterConstants.SKU1] != null)
            {
                if (double.TryParse(this.Request.QueryString[SystemConstants.UrlParameterConstants.SKU1], out double qty))
                    skuParams["SKU1"] = qty;
            }

            // Add more SKU parameters as needed...

            if (skuParams.Any())
            {
                long customerId = (long)this.Session[SystemConstants.SessionConstants.BoundCustomerID];
                DateTime deliveryDate = (DateTime)this.Session[SystemConstants.SessionConstants.BoundDeliveryDate];
                string notes = (string)this.Session[SystemConstants.SessionConstants.BoundNotes];

                string result = orderManager.ProcessSKUParameters(skuParams, customerId, deliveryDate, notes);

                if (!string.IsNullOrEmpty(result))
                {
                    // Error occurred
                    this.ltrlStatus.Text = $"SKU Processing Error: {result}";
                }
            }
        }
        private void HandleSpecialQueryStringActions()
        {
            // Handle Invoiced parameter
            if (this.Request.QueryString[CONST_QRYSTR_INVOICED] != null &&
                this.Request.QueryString[CONST_QRYSTR_INVOICED].Equals("Y"))
            {
                this.MarkItemAsInvoiced();
                return;
            }

            // Handle Delivered parameter
            if (this.Request.QueryString[CONST_QRYSTR_DELIVERED] != null &&
                this.Request.QueryString[CONST_QRYSTR_DELIVERED].Equals("Y"))
            {
                this.btnOrderDelivered_Click(this, EventArgs.Empty);
                return;
            }

            // Handle LastOrder parameter - COMPLETELY REWRITTEN
            if (IsNewOrderMode && this.Request.QueryString[SystemConstants.UrlParameterConstants.LastOrder] != null &&
                this.Request.QueryString[SystemConstants.UrlParameterConstants.LastOrder] == "Y")
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "HandleSpecialQueryStringActions: Processing LastOrder request");

                // Get customer ID from query string
                long customerId = 0;
                if (this.Request.QueryString[SystemConstants.UrlParameterConstants.CustomerID] != null)
                {
                    if (long.TryParse(this.Request.QueryString[SystemConstants.UrlParameterConstants.CustomerID], out customerId) && customerId > 0)
                    {
                        AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"HandleSpecialQueryStringActions: Customer ID from query string: {customerId}");

                        // Set customer in session FIRST
                        this.Session[SystemConstants.SessionConstants.BoundCustomerID] = customerId;

                        // Set customer in ComboBox if possible
                        if (cboManualContacts != null && cboManualContacts.Items.FindByValue(customerId.ToString()) != null)
                        {
                            cboManualContacts.SelectedValue = customerId.ToString();
                            AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"HandleSpecialQueryStringActions: Set ComboBox to customer {customerId}");

                            // Enable controls and update dates
                            SetControlsEnabledState(true);
                            UpdateDatesForSelectedCustomer(customerId.ToString());
                            UpdateSessionFromManualControls();
                        }
                        else
                        {
                            AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"HandleSpecialQueryStringActions: Customer {customerId} not found in ComboBox items");
                        }

                        // Now process the last order
                        if (ProcessLastOrderRequest(true))
                        {
                            AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "HandleSpecialQueryStringActions: Last order items added, redirecting");
                            RedirectToExistingOrderMode();
                        }
                        else
                        {
                            AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "HandleSpecialQueryStringActions: No last order items found");
                            ltrlStatus.Text = "No previous order found for this customer.";
                        }
                    }
                    else
                    {
                        AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"HandleSpecialQueryStringActions: Invalid customer ID: {this.Request.QueryString[SystemConstants.UrlParameterConstants.CustomerID]}");
                        ltrlStatus.Text = "Invalid customer ID provided.";
                    }
                }
                else
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "HandleSpecialQueryStringActions: No customer ID provided in query string");
                    ltrlStatus.Text = "No customer ID provided.";
                }
            }

            // Handle SKU parameters
            if (IsNewOrderMode && this.Request.QueryString[SystemConstants.UrlParameterConstants.SKU1] != null)
            {
                ProcessSKUParameters();
            }
        }
        private string GetOrderHeaderRequiredByDateStr()
        {
            string empty = string.Empty;
            return this.dvOrderHeader.CurrentMode != DetailsViewMode.Edit ? this.dvOrderHeaderGetLabelValue("lblRequiredByDate") : this.dvOrderHeaderGetTextBoxValue("tbxRequiredByDate");
        }

        private string GetOrderHeaderNotes()
        {
            if (IsNewOrderMode)
            {
                return tbxManualNotes?.Text ?? string.Empty;
            }
            else
            {
                return this.dvOrderHeader.CurrentMode != DetailsViewMode.Edit ?
                       this.dvOrderHeaderGetLabelValue("lblNotes") :
                       this.dvOrderHeaderGetTextBoxValue("tbxNotes");
            }
        }
        /// <summary>
        /// Handles the Last Order button click - loads items from customer's last order
        /// </summary>
        protected void btnLastOrder_Click(object sender, EventArgs e)
        {
            if (cboManualContacts?.SelectedValue == null || cboManualContacts.SelectedValue == "0")
            {
                ltrlStatus.Text = "Please select a customer first.";
                return;
            }

            try
            {
                long customerId = Convert.ToInt64(cboManualContacts.SelectedValue);

                // ENSURE session is updated BEFORE processing
                UpdateSessionFromManualControls();
                this.Session[SystemConstants.SessionConstants.BoundCustomerID] = customerId;

                //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"btnLastOrder_Click: Starting for customer {customerId}");

                // GET the last order items (changed to false as requested)
                var orderManager = new TrackerSQL.Managers.OrderManager();
                List<OrderManager.OrderLineData> lastOrderItems = orderManager.GetLastOrderItems(customerId, false);

                if (lastOrderItems.Count > 0)
                {
                    // Add each item to the actual order
                    int itemsAddedCount = 0;
                    foreach (var orderLine in lastOrderItems)
                    {
                        // Create the order header data
                        OrderHeaderData headerData = this.Get_dvOrderHeaderData(false);

                        // Create the order data
                        OrderTblData orderData = new OrderTblData
                        {
                            CustomerID = customerId,
                            OrderDate = headerData.OrderDate,
                            RoastDate = headerData.RoastDate,
                            RequiredByDate = headerData.RequiredByDate,
                            ToBeDeliveredBy = headerData.ToBeDeliveredBy,
                            PurchaseOrder = headerData.PurchaseOrder,
                            Confirmed = headerData.Confirmed,
                            InvoiceDone = headerData.InvoiceDone,
                            Done = headerData.Done,
                            Notes = headerData.Notes,
                            ItemTypeID = orderLine.ItemID,
                            QuantityOrdered = orderLine.Qty,
                            PackagingID = orderLine.PackagingID
                        };

                        // Add the order line
                        string result = orderManager.AddOrderLine(headerData, orderData);
                        if (string.IsNullOrEmpty(result))
                        {
                            itemsAddedCount++;
                            AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Successfully added item: {orderLine.ItemName} (ID: {orderLine.ItemID})");
                        }
                        else
                        {
                            AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Error adding item {orderLine.ItemName}: {result}");
                        }
                    }

                    if (itemsAddedCount > 0)
                    {
                        AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Added {itemsAddedCount} items to database, now redirecting");

                        // Build redirect URL with CURRENT session values
                        var currentCustomerId = (long)this.Session[SystemConstants.SessionConstants.BoundCustomerID];
                        var currentDeliveryDate = (DateTime)this.Session[SystemConstants.SessionConstants.BoundDeliveryDate];
                        var currentNotes = (string)this.Session[SystemConstants.SessionConstants.BoundNotes] ?? string.Empty;

                        string redirectUrl = $"OrderDetail.aspx?{CONST_QRYSTR_CustomerID}={currentCustomerId}&{CONST_QRYSTR_DELIVERYDATE}={currentDeliveryDate:yyyy-MM-dd}&{CONST_QRYSTR_NOTES}={HttpUtility.UrlEncode(currentNotes)}";

                        //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Redirecting to: {redirectUrl}");

                        // Use Response.Redirect with explicit end
                        Response.Redirect(redirectUrl, true); // Changed to true to end execution
                    }
                    else
                    {
                        ltrlStatus.Text = "Error adding last order items.";
                        upnlNewOrderSummary.Update();
                    }
                }
                else
                {
                    ltrlStatus.Text = "No previous order found for this customer.";
                    upnlNewOrderSummary.Update();
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
                // ThreadAbortException is expected when using Response.Redirect with endResponse=true
                // This is normal behavior and should not be logged as an error
                return;
            }
            catch (Exception ex)
            {
                ltrlStatus.Text = $"Error loading last order: {ex.Message}";
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"btnLastOrder_Click error: {ex}");
                upnlNewOrderSummary.Update();
            }
        }
        /// <summary>
        /// Builds the URL for redirecting to existing order mode
        /// </summary>
        private string BuildExistingOrderUrl()
        {
            var customerId = (long)this.Session[SystemConstants.SessionConstants.BoundCustomerID];
            var deliveryDate = (DateTime)this.Session[SystemConstants.SessionConstants.BoundDeliveryDate];
            var notes = (string)this.Session[SystemConstants.SessionConstants.BoundNotes] ?? string.Empty;

            return $"OrderDetail.aspx?{CONST_QRYSTR_CustomerID}={customerId}&{CONST_QRYSTR_DELIVERYDATE}={deliveryDate:yyyy-MM-dd}&{CONST_QRYSTR_NOTES}={HttpUtility.UrlEncode(notes)}";
        }
        protected void btnNewOrder_Click(object sender, EventArgs e)
        {
            AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "New Order button clicked - clearing session and redirecting");

            // Clear all order session data
            ClearOrderSession();

            // Redirect to clean new order page
            Response.Redirect("OrderDetail.aspx?NewOrder=true", true);
        }
        private void BindRowQueryParameters()
        {
            string controlSelectedValue = this.dvOrderHeaderGetCBoControlSelectedValue(CONST_ORDERHEADER_CONTACT_ID);
            this.Session[SystemConstants.SessionConstants.BoundCustomerID] = (object)Convert.ToInt32(controlSelectedValue);
            DateTime date = Convert.ToDateTime(this.GetOrderHeaderRequiredByDateStr()).Date;
            this.Session[SystemConstants.SessionConstants.BoundDeliveryDate] = (object)date.Date;
            string orderHeaderNotes = this.GetOrderHeaderNotes();
            this.Session[SystemConstants.SessionConstants.BoundNotes] = (object)orderHeaderNotes;
            UriBuilder uriBuilder = new UriBuilder(this.Request.Url);
            NameValueCollection queryString = HttpUtility.ParseQueryString(uriBuilder.Query);
            queryString.Set("CustomerID", controlSelectedValue);
            queryString.Set("DeliveryDate", $"{date:yyyy-MM-dd}");
            queryString.Set("Notes", orderHeaderNotes);
            uriBuilder.Query = queryString.ToString();
            string script = $"ChangeUrl('OrderDetail','{uriBuilder.Uri.ToString()}')";
            System.Web.UI.ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "Order Detail", script, true);
        }

        protected void btnNewItem_Click(object sender, EventArgs e)
        {
            this.btnAdd.Visible = true;
            this.btnCancel.Visible = true;
            this.pnlNewItem.Visible = true;
            this.btnNewItem.Visible = false;
            this.upnlNewOrderItem.Update();
        }

        private void HideNewOrderItemPanel()
        {
            this.btnAdd.Visible = false;
            this.btnCancel.Visible = false;
            this.pnlNewItem.Visible = false;
            this.btnNewItem.Visible = true;
            this.upnlNewOrderItem.Update();
            this.odsOrderDetail.DataBind();
            this.gvOrderLines.DataBind();
            this.upnlOrderLines.Update();
        }

        private string dvOrderHeaderGetDDLControlSelectedValue(string pDDLControlName)
        {
            DropDownList control = (DropDownList)this.dvOrderHeader.FindControl(pDDLControlName);
            return control.SelectedValue == null ? "0" : control.SelectedValue;
        }
        private string dvOrderHeaderGetCBoControlSelectedValue(string comboBoxControlName)
        {
            var coBox = (ComboBox)this.dvOrderHeader.FindControl(comboBoxControlName);
            return coBox.SelectedValue == null ? "0" : coBox.SelectedValue;
        }

        private string dvOrderHeaderGetTextBoxValue(string pTextBoxControlName)
        {
            TextBox control = (TextBox)this.dvOrderHeader.FindControl(pTextBoxControlName);
            return control != null ? control.Text : string.Empty;
        }

        private string dvOrderHeaderGetLabelValue(string pTextBoxControlName)
        {
            Label control = (Label)this.dvOrderHeader.FindControl(pTextBoxControlName);
            return control != null ? control.Text : string.Empty;
        }

        private bool dvOrderHeaderGetCheckBoxValue(string pCheckBoxControlName)
        {
            CheckBox control = (CheckBox)this.dvOrderHeader.FindControl(pCheckBoxControlName);
            return control != null && control.Checked;
        }
        /// <summary>
        /// Centralized method to handle note changes and validate button states.
        /// This ensures both manual and DetailsView note textboxes trigger the same validation.
        /// </summary>
        private void HandleNotesChanged(string notesText)
        {
            //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"HandleNotesChanged called with notes length: {notesText?.Length ?? 0}");

            // Update session in New Order mode
            if (IsNewOrderMode)
            {
                UpdateSessionFromManualControls();
            }

            // Validate button states - this is the key part that checks ZZName + empty notes
            UpdateNewItemButtonStateWithRoleCheck();

            AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"HandleNotesChanged completed note: {notesText}");
        }
        private OrderHeaderData Get_dvOrderHeaderData(bool pInEditMode)
        {
            OrderHeaderData dvOrderHeaderData = new OrderHeaderData();

            if (IsNewOrderMode)
            {
                // SAFER CONVERSION: Get data from manual controls in New Order mode
                string customerIdValue = cboManualContacts?.SelectedValue ?? "0";
                if (string.IsNullOrEmpty(customerIdValue))
                    customerIdValue = "0";
                dvOrderHeaderData.CustomerID = int.TryParse(customerIdValue, out int custId) ? custId : 0;

                string deliveryByValue = ddlManualToBeDeliveredBy?.SelectedValue ?? "0";
                if (string.IsNullOrEmpty(deliveryByValue))
                    deliveryByValue = "0";
                dvOrderHeaderData.ToBeDeliveredBy = int.TryParse(deliveryByValue, out int delBy) ? delBy : 0;

                dvOrderHeaderData.Confirmed = cbxManualConfirmed?.Checked ?? false;
                dvOrderHeaderData.Done = cbxManualDone?.Checked ?? false;
                dvOrderHeaderData.InvoiceDone = cbxManualInvoiceDone?.Checked ?? false;
                dvOrderHeaderData.PurchaseOrder = tbxManualPurchaseOrder?.Text ?? string.Empty;
                dvOrderHeaderData.Notes = tbxManualNotes?.Text ?? string.Empty;

                // Parse date fields safely
                if (DateTime.TryParse(tbxManualOrderDate?.Text, out DateTime orderDate))
                    dvOrderHeaderData.OrderDate = orderDate.Date;
                else
                    dvOrderHeaderData.OrderDate = DateTime.MinValue;

                if (DateTime.TryParse(tbxManualRoastDate?.Text, out DateTime roastDate))
                    dvOrderHeaderData.RoastDate = roastDate.Date;
                else
                    dvOrderHeaderData.RoastDate = DateTime.MinValue;

                if (DateTime.TryParse(tbxManualRequiredByDate?.Text, out DateTime requiredByDate))
                    dvOrderHeaderData.RequiredByDate = requiredByDate.Date;
                else
                    dvOrderHeaderData.RequiredByDate = DateTime.MinValue;

                // Log the values for debugging
                //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Get_dvOrderHeaderData: CustomerID={dvOrderHeaderData.CustomerID}, ToBeDeliveredBy={dvOrderHeaderData.ToBeDeliveredBy}");
                //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Get_dvOrderHeaderData: OrderDate={dvOrderHeaderData.OrderDate:yyyy-MM-dd}, RoastDate={dvOrderHeaderData.RoastDate:yyyy-MM-dd}, RequiredByDate={dvOrderHeaderData.RequiredByDate:yyyy-MM-dd}");
            }
            else
            {
                // Original DetailsView logic for existing orders
                string customerIdValue = this.dvOrderHeaderGetCBoControlSelectedValue(CONST_ORDERHEADER_CONTACT_ID) ?? "0";
                dvOrderHeaderData.CustomerID = int.TryParse(customerIdValue, out int custId) ? custId : 0;

                string deliveryByValue = this.dvOrderHeaderGetDDLControlSelectedValue("ddlToBeDeliveredBy") ?? "0";
                dvOrderHeaderData.ToBeDeliveredBy = int.TryParse(deliveryByValue, out int delBy) ? delBy : 0;

                dvOrderHeaderData.Confirmed = this.dvOrderHeaderGetCheckBoxValue("cbxConfirmed");
                dvOrderHeaderData.Done = this.dvOrderHeaderGetCheckBoxValue("cbxDone");
                dvOrderHeaderData.InvoiceDone = this.dvOrderHeaderGetCheckBoxValue("cbxInvoiceDone");
                dvOrderHeaderData.PurchaseOrder = string.Empty; // Not available in current structure

                string str1, str2, str3;
                if (pInEditMode)
                {
                    str1 = this.dvOrderHeaderGetTextBoxValue("tbxOrderDate");
                    str2 = this.dvOrderHeaderGetTextBoxValue("tbxRoastDate");
                    str3 = this.dvOrderHeaderGetTextBoxValue("tbxRequiredByDate");
                    dvOrderHeaderData.Notes = this.dvOrderHeaderGetTextBoxValue("tbxNotes");
                }
                else
                {
                    str3 = this.dvOrderHeaderGetLabelValue("lblRequiredByDate");
                    str1 = this.dvOrderHeaderGetLabelValue("lblOrderDate");
                    str2 = this.dvOrderHeaderGetLabelValue("lblRoastDate");
                    dvOrderHeaderData.Notes = this.dvOrderHeaderGetLabelValue("lblNotes");
                }

                dvOrderHeaderData.RequiredByDate = string.IsNullOrEmpty(str3) ? DateTime.MinValue : Convert.ToDateTime(str3).Date;
                dvOrderHeaderData.OrderDate = string.IsNullOrEmpty(str1) ? DateTime.MinValue : Convert.ToDateTime(str1).Date;
                dvOrderHeaderData.RoastDate = string.IsNullOrEmpty(str2) ? DateTime.MinValue : Convert.ToDateTime(str2).Date;
            }

            return dvOrderHeaderData;
        }
        protected void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                // ENSURE session is updated BEFORE processing
                UpdateSessionFromManualControls();

                OrderHeaderData headerData = this.Get_dvOrderHeaderData(false);

                // SAFE CONVERSION: Check for null/empty values before converting
                if (string.IsNullOrEmpty(this.cboNewItemDesc?.SelectedValue))
                {
                    this.ltrlStatus.Text = "Please select an item.";
                    upnlNewOrderItem.Update();
                    return;
                }

                if (string.IsNullOrEmpty(this.tbxNewQuantityOrdered?.Text))
                {
                    this.ltrlStatus.Text = "Please enter a quantity.";
                    upnlNewOrderItem.Update();
                    return;
                }

                OrderTblData orderData = new OrderTblData
                {
                    CustomerID = headerData.CustomerID,
                    OrderDate = headerData.OrderDate,
                    RoastDate = headerData.RoastDate,
                    RequiredByDate = headerData.RequiredByDate,
                    ToBeDeliveredBy = Convert.ToInt32(headerData.ToBeDeliveredBy),
                    PurchaseOrder = headerData.PurchaseOrder,
                    Confirmed = headerData.Confirmed,
                    InvoiceDone = headerData.InvoiceDone,
                    Done = headerData.Done,
                    Notes = headerData.Notes,
                    ItemTypeID = Convert.ToInt32(this.cboNewItemDesc.SelectedValue),
                    QuantityOrdered = Convert.ToDouble(this.tbxNewQuantityOrdered.Text),
                    PackagingID = string.IsNullOrEmpty(this.cboNewPackaging?.SelectedValue) ? 0 : Convert.ToInt32(this.cboNewPackaging.SelectedValue)
                };

                var manager = new TrackerSQL.Managers.OrderManager();
                string result = manager.AddOrderLine(headerData, orderData);

                if (string.IsNullOrWhiteSpace(result))
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "Item added successfully, redirecting to existing order mode");

                    // Build redirect URL with CURRENT session values  
                    var currentCustomerId = (long)this.Session[SystemConstants.SessionConstants.BoundCustomerID];
                    var currentDeliveryDate = (DateTime)this.Session[SystemConstants.SessionConstants.BoundDeliveryDate];
                    var currentNotes = (string)this.Session[SystemConstants.SessionConstants.BoundNotes] ?? string.Empty;

                    string redirectUrl = $"OrderDetail.aspx?{CONST_QRYSTR_CustomerID}={currentCustomerId}&{CONST_QRYSTR_DELIVERYDATE}={currentDeliveryDate:yyyy-MM-dd}&{CONST_QRYSTR_NOTES}={HttpUtility.UrlEncode(currentNotes)}";

                    Response.Redirect(redirectUrl, true); // Use true to end execution
                }
                else
                {
                    this.ltrlStatus.Text = "Error adding item: " + result;
                    upnlNewOrderItem.Update();
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
                // ThreadAbortException is expected when using Response.Redirect with endResponse=true
                // This is normal behavior and should not be logged as an error
                return;
            }
            catch (Exception ex)
            {
                this.ltrlStatus.Text = $"Error adding item: {ex.Message}";
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"btnAdd_Click error: {ex}");
                upnlNewOrderItem.Update();
            }

            this.HideNewOrderItemPanel();
        }        /// <summary>
                 /// Switches from manual header mode to DetailsView mode after first item is added
                 /// </summary>
        private void SwitchToDetailsViewMode()
        {
            // Update session from manual controls one final time
            UpdateSessionFromManualControls();

            // Switch mode
            IsNewOrderMode = false;

            // Show DetailsView mode
            ShowDetailsViewMode();

            // Force refresh of DetailsView with current session data
            dvOrderHeader.DataBind();
            pnlOrderHeader.Update();

            // Update the URL to reflect existing order mode
            UpdateUrlToExistingOrderMode();

            // Log the transition
            //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "Switched from manual mode to DetailsView mode after adding first item");
        }
        /// <summary>
        /// Updates the browser URL to reflect existing order mode - only for AJAX scenarios
        /// </summary>
        private void UpdateUrlToExistingOrderMode()
        {
            // DON'T update URL during redirect - the redirect itself updates the URL
            // This method should only be used for AJAX UpdatePanel scenarios

            var customerId = (long)this.Session[SystemConstants.SessionConstants.BoundCustomerID];
            var deliveryDate = (DateTime)this.Session[SystemConstants.SessionConstants.BoundDeliveryDate];
            var notes = (string)this.Session[SystemConstants.SessionConstants.BoundNotes];

            string newUrl = $"OrderDetail.aspx?{CONST_QRYSTR_CustomerID}={customerId}&{CONST_QRYSTR_DELIVERYDATE}={deliveryDate:yyyy-MM-dd}&{CONST_QRYSTR_NOTES}={HttpUtility.UrlEncode(notes)}";

            // Only update URL via JavaScript if we're NOT redirecting
            if (!Response.IsRequestBeingRedirected)
            {
                string script = $"ChangeUrl('Order Detail', '{newUrl}')";
                ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "SwitchMode", script, true);
                //AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Updated browser URL via JavaScript: {newUrl}");
            }
        }
        protected void btnCancel_Click(object sender, EventArgs e) => this.HideNewOrderItemPanel();

        protected void gvOrderLines_OnItemDelete(object sender, EventArgs e)
        {
            string pDataValue = ((CommandEventArgs)e).CommandArgument.ToString();
            var manager = new TrackerSQL.Managers.OrderManager();
            string result = manager.DeleteOrderItem(Convert.ToInt32(pDataValue));
            this.ltrlStatus.Text = string.IsNullOrEmpty(result) ? "Item Deleted" : "Error deleting item: " + result;
        }

        protected void gvOrderLines_RowUpdated(object sender, GridViewUpdatedEventArgs e)
        {
            // Exit edit mode first
            gvOrderLines.EditIndex = -1;

            // Only update the UpdatePanel, don't force DataBind
            upnlOrderLines.Update();

            // Don't call DataBind() here - it will happen automatically
            // The ObjectDataSource will refresh the GridView after the update
        }
        protected virtual void dvOrderHeader_OnModeChanging(object sender, DetailsViewModeEventArgs e)
        {
            if (e.NewMode == DetailsViewMode.Edit)
            {
                this.Session[CONST_ORDERHEADERVALUES] = (object)this.Get_dvOrderHeaderData(false);
            }
            else
            {
                if (e.NewMode != DetailsViewMode.ReadOnly || this.Session[CONST_ORDERHEADERVALUES] == null)
                    return;
                this.dvOrderHeader.DataBind();
            }
        }

        protected virtual void dvOrderHeader_OnItemUpdated(object sender, DetailsViewUpdatedEventArgs e)
        {
            this.upnlOrderLines.Update();
        }
        /// <summary>
        /// Gets an integer value from a control by name, supporting both ComboBox and HiddenField types.
        /// </summary>
        /// <param name="row">The GridViewRow to search in</param>
        /// <param name="comboBoxControlName">The name/ID of the control to find</param>
        /// <param name="fallbackControlName">Optional fallback control name to try if first control is not found</param>
        /// <returns>The integer value from the control, or 0 if not found or invalid</returns>
        private int GetControlSelectedValue(GridViewRow row, string comboBoxControlName, string hidddenControlName)
        {
            // Try to find as ComboBox first
            var comboBox = row.FindControl(comboBoxControlName) as AjaxControlToolkit.ComboBox;
            if (comboBox != null && !string.IsNullOrEmpty(comboBox.SelectedValue))
            {
                if (int.TryParse(comboBox.SelectedValue, out int comboValue))
                    return comboValue;
            }

            // Try to find as HiddenField
            var hiddenField = row.FindControl(hidddenControlName) as HiddenField;
            if (hiddenField != null && !string.IsNullOrEmpty(hiddenField.Value))
            {
                if (int.TryParse(hiddenField.Value, out int hiddenValue))
                    return hiddenValue;
            }
            return SystemConstants.DatabaseConstants.InvalidID; // Default value if control not found or invalid
        }
        protected virtual void dvOrderHeader_OnDataBound(object sender, EventArgs e)
        {
            if (this.dvOrderHeader.CurrentMode == DetailsViewMode.ReadOnly)
            {
                if (this.Session[CONST_ORDERHEADERVALUES] != null)
                {
                    OrderHeaderData orderHeaderData = (OrderHeaderData)this.Session[CONST_ORDERHEADERVALUES];
                    if (!this.GetOrderHeaderRequiredByDateStr().Equals(string.Empty))
                    {
                        DateTime date = Convert.ToDateTime(this.GetOrderHeaderRequiredByDateStr()).Date;
                        if (orderHeaderData.OrderDate.Date != date.Date)
                        {
                            UsedItemGroupTbl usedItemGroupTbl = new UsedItemGroupTbl();
                            foreach (GridViewRow row in this.gvOrderLines.Rows)
                            {
                                int itemTypeId = GetControlSelectedValue(row, CONST_ORDERLINE_ITEM_COMBOBOX_ID, CONST_ORDERLINE_HIDDENFIELD_ITEM_ID);
                                if (itemTypeId != 0)
                                {
                                    usedItemGroupTbl.UpdateIfGroupItem(orderHeaderData.CustomerID, itemTypeId, orderHeaderData.RequiredByDate, date);
                                }
                            }
                        }
                    }
                }
                Label control1 = (Label)this.dvOrderHeader.FindControl("lblPurchaseOrder");
                if (control1 != null && control1.Text.Equals(SystemConstants.UIConstants.PORequiredText))
                {
                    control1.BackColor = Color.Red;
                    control1.ForeColor = Color.White;
                }
            }
            CheckBox control2 = (CheckBox)this.dvOrderHeader.FindControl("cbxDone");
            if (control2 != null)
                this.btnOrderDelivered.Enabled = this.btnOrderCancelled.Enabled = !control2.Checked;
            this.updtButtonPanel.Update();
        }

        public void DeleteOrderItem(string pOrderID)
        {
            string str = new OrderTbl().DeleteOrderById(Convert.ToInt32(pOrderID));
            var statusStr = str.Length == 0 ? "Item deleted" : str;
            this.ltrlStatus.Text = statusStr;
            AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Ordered Item deleted status: {statusStr}");
        }

        protected void btnCancelled_Click(object sender, EventArgs e)
        {
            if (!SecurityManager.IsAdmin())
                return;
            foreach (TableRow row in this.gvOrderLines.Rows)
                this.DeleteOrderItem(((HiddenField)row.Cells[4].FindControl(CONST_ORDERLINE_HIDDENFIELD_ORDER_ID)).Value);
            this.Response.Redirect("DeliverySheet.aspx");
        }
        public string GetPackagingDesc(int pPackagingID)
        {
            return pPackagingID > 0 ? new PackagingTbl().GetPackagingDesc(pPackagingID) : string.Empty;
        }
        private ContactEmailDetails GetEmailAddressFromNote()
        {
            var orderManager = new TrackerSQL.Managers.OrderManager();
            string labelValue = this.dvOrderHeaderGetLabelValue("lblNotes");
            string emailAddress = orderManager.ExtractEmailFromNotes(labelValue);

            if (!string.IsNullOrEmpty(emailAddress))
            {
                return new ContactEmailDetails { EmailAddress = emailAddress };
            }

            return null;
        }

        private ContactEmailDetails GetEmailDetails(string pContactsID)
        {
            ContactEmailDetails contactEmailDetails = new ContactEmailDetails();
            return !pContactsID.Equals(SystemConstants.CustomerConstants.SundryCustomerIDStr) ?
                contactEmailDetails.GetContactsEmailDetails(Convert.ToInt32(pContactsID)) :
                this.GetEmailAddressFromNote();
        }

        private string AddUnitsToQty(string pItemTypeID, string pQty)
        {
            string itemUoM = this.GetItemUoM(Convert.ToInt32(pItemTypeID));
            if (string.IsNullOrEmpty(itemUoM))
                return pQty;
            double num = Convert.ToDouble(pQty);
            return $"{pQty} {(num == 1.0 ? itemUoM : itemUoM + "s")}";
        }

        private int GetItemSortOrderID(string pItemTypeID)
        {
            return new ItemTypeTbl().GetItemSortOrder(Convert.ToInt32(pItemTypeID));
        }

        private string UpCaseFirstLetter(string pString)
        {
            return string.IsNullOrEmpty(pString) ? string.Empty : char.ToUpper(pString[0]).ToString() + pString.Substring(1);
        }

        private string ResolveRecipientEmail(ContactEmailDetails details)
        {
            return !string.IsNullOrWhiteSpace(details.EmailAddress)
                ? details.EmailAddress
                : details.altEmailAddress;
        }
        private static (string controlId, string controlDesc) GetControlIdAndDescFromRow(GridViewRow row, string comboBoxName, string labelName, string hiddenFieldName)
        {
            // Try to get the ComboBox first (edit mode)
            var itemControl = row.FindControl(comboBoxName) as AjaxControlToolkit.ComboBox;
            if (itemControl != null && itemControl.SelectedValue != null)
            {
                string itemId = itemControl.SelectedValue;
                string itemName = itemControl.SelectedItem != null ? itemControl.SelectedItem.Text : string.Empty;
                return (itemId, itemName);
            }

            // Fallback: try to get the hidden field and label (display mode)
            var hndItemId = row.FindControl(hiddenFieldName) as HiddenField;
            var lblItemDesc = row.FindControl(labelName) as Label;
            if (hndItemId != null && !string.IsNullOrEmpty(hndItemId.Value))
            {
                string itemId = hndItemId.Value;
                string itemName = lblItemDesc != null ? lblItemDesc.Text : string.Empty;
                return (itemId, itemName);
            }

            // Not found
            return (string.Empty, string.Empty);
        }
        /*
         private void AppendOrderItemsToEmailBody(EmailMailKitCls email)
        {
            email.AddToBody("<ul>");

            foreach (GridViewRow row in gvOrderLines.Rows)
            {
                var (itemId, itemDesc) = GetControlIdAndDescFromRow(row, CONST_ORDERLINE_ITEM_COMBOBOX_ID, CONST_ORDERLINE_HIDDENFIELD_ITEM_LABEL, CONST_ORDERLINE_HIDDENFIELD_ITEM_ID);
                var qtyLbl = (Label)row.FindControl("lblQuantityOrdered");
                var (packagingId, packagingDesc) = GetControlIdAndDescFromRow(row, CONST_ORDERLINE_PACKAGING_COMBOBOX_ID, CONST_ORDERLINE_HIDDENFIELD_PACKAGING_LABEL, CONST_ORDERLINE_HIDDENFIELD_PACKAGING_ID); ;

                string qty = AddUnitsToQty(itemId, qtyLbl.Text);

                if (GetItemSortOrderID(itemId) == 10)
                {
                    string notes = EmailUtils.CleanNoteText(dvOrderHeaderGetLabelValue("lblNotes"));
                    email.AddFormatToBody("<li>{0}</li>", notes);
                }
                else
                {
                    if (packagingId.Equals("0"))
                        email.AddFormatToBody(MessageProvider.Get("OrderItemFormatBasic"), qty, itemDesc);
                    else
                        email.AddFormatToBody(MessageProvider.Get("OrderItemFormatWithPrep"), qty, itemDesc, packagingDesc);
                }
            }

            email.AddToBody("</ul>");
        }
        */
        protected void btnConfirmOrder_Click(object sender, EventArgs e)
        {
            // FIXED: Get ContactEmailDetails based on current mode
            string contactID;
            if (IsNewOrderMode)
            {
                contactID = cboManualContacts?.SelectedValue ?? "0";
            }
            else
            {
                contactID = dvOrderHeaderGetCBoControlSelectedValue(CONST_ORDERHEADER_CONTACT_ID);
            }

            ContactEmailDetails contact = GetEmailDetails(contactID);
            OrderHeaderData header = this.Get_dvOrderHeaderData(false);

            // Rest of method stays the same...
            var orderLines = new List<TrackerSQL.Managers.OrderLineData>();
            foreach (GridViewRow row in this.gvOrderLines.Rows)
            {
                var (itemId, itemDesc) = GetControlIdAndDescFromRow(row, CONST_ORDERLINE_ITEM_COMBOBOX_ID, CONST_ORDERLINE_HIDDENFIELD_ITEM_LABEL, CONST_ORDERLINE_HIDDENFIELD_ITEM_ID);
                var qtyLbl = (Label)row.FindControl("lblQuantityOrdered");
                var (packagingIdStr, packagingDesc) = GetControlIdAndDescFromRow(row, CONST_ORDERLINE_PACKAGING_COMBOBOX_ID, CONST_ORDERLINE_HIDDENFIELD_PACKAGING_LABEL, CONST_ORDERLINE_HIDDENFIELD_PACKAGING_ID);
                int packagingId = Convert.ToInt32(packagingIdStr);

                var line = new TrackerSQL.Managers.OrderLineData
                {
                    ItemID = Convert.ToInt32(itemId),
                    ItemName = itemDesc,
                    Qty = Convert.ToDouble(qtyLbl.Text),
                    PackagingID = packagingId > 0 ? packagingId : 0,
                    PackagingName = packagingId > 0 ? packagingDesc : string.Empty
                };
                orderLines.Add(line);
            }

            string notes = this.GetOrderHeaderNotes();
            var emailManager = new TrackerSQL.Managers.OrderDetailManager();
            string statusMsg;
            bool success = emailManager.SendOrderConfirmation(contact, header, orderLines, notes, out statusMsg);
            AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Order confirmation sent, status: {statusMsg}");
            ltrlStatus.Text = statusMsg;
            new showMessageBox(this.Page, "Order Confirmation", statusMsg);
            upnlNewOrderItem.Update();
        }
        public string GetToBeDeliveredBy(object pToBeDeliveredBy)
        {
            return pToBeDeliveredBy != null ? pToBeDeliveredBy.ToString() : "0";
        }

        public string GetItemUoMObj(object pItemID)
        {
            return pItemID == null ? string.Empty : this.GetItemUoM(Convert.ToInt32(pItemID.ToString()));
        }

        public string GetItemUoM(int pItemID)
        {
            return pItemID > 0 ? new ItemTypeTbl().GetItemUnitOfMeasure(pItemID) : string.Empty;
        }

        protected void odsOrderSummary_OnUpdated(object sender, ObjectDataSourceStatusEventArgs e)
        {
            if ((bool)e.ReturnValue)
            {
                var control1 = (ComboBox)this.dvOrderHeader.FindControl(CONST_ORDERHEADER_CONTACT_ID);
                TextBox control2 = (TextBox)this.dvOrderHeader.FindControl("tbxRequiredByDate");
                TextBox control3 = (TextBox)this.dvOrderHeader.FindControl("tbxNotes");
                if (control1 != null && control2 != null && control3 != null)
                    this.BindRowQueryParameters();
            }
            this.gvOrderLines.DataBind();
            this.upnlOrderLines.Update();
        }
        protected void MarkItemAsInvoiced()
        {
            var manager = new TrackerSQL.Managers.OrderManager();
            manager.MarkItemAsInvoiced(
                (long)this.Session[SystemConstants.SessionConstants.BoundCustomerID],
                ((DateTime)this.Session[SystemConstants.SessionConstants.BoundDeliveryDate]).Date,
                (string)this.Session[SystemConstants.SessionConstants.BoundNotes]);
            this.pnlOrderHeader.Update();
            AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Order for customer id: {this.Session[SystemConstants.SessionConstants.BoundCustomerID]}, marked as invoiced.");
        }
        protected void btnOrderDelivered_Click(object sender, EventArgs e)
        {
            OrderHeaderData headerData = this.Get_dvOrderHeaderData(false);
            var orderLines = new List<OrderManager.TempOrderLineData>();
            ItemTypeTbl itemTypeTbl = new ItemTypeTbl();

            foreach (GridViewRow row in this.gvOrderLines.Rows)
            {

                var itemId = GetControlSelectedValue(row, CONST_ORDERLINE_ITEM_COMBOBOX_ID, CONST_ORDERLINE_HIDDENFIELD_ITEM_ID);
                var qtyLbl = (Label)row.FindControl("lblQuantityOrdered");
                var packagingId = GetControlSelectedValue(row, CONST_ORDERLINE_PACKAGING_COMBOBOX_ID, CONST_ORDERLINE_HIDDENFIELD_PACKAGING_ID);
                var orderIdLbl = (HiddenField)row.FindControl(CONST_ORDERLINE_HIDDENFIELD_ORDER_ID);

                var line = new OrderManager.TempOrderLineData
                {
                    ItemID = itemId,
                    Qty = Convert.ToDouble(qtyLbl.Text),
                    PackagingID = Convert.ToInt32(packagingId),
                    ServiceTypeID = itemTypeTbl.GetServiceID(itemId),
                    OriginalOrderID = Convert.ToInt32(orderIdLbl.Value)
                };
                orderLines.Add(line);
            }

            var manager = new TrackerSQL.Managers.OrderManager();
            bool success = manager.CompleteOrderDelivery(headerData, orderLines);
            AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"Order for customer id: {this.Session[SystemConstants.SessionConstants.BoundCustomerID]}, marked as deliverred.");
            if (!success)
                this.ltrlStatus.Text = "Error deleting Temp Table";
            else
                this.Response.Redirect("OrderDone.aspx");
        }

        protected void gvOrderLines_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            // Only process DataRow types in Edit mode
            if (e.Row.RowType != DataControlRowType.DataRow)
            {
                return;
            }

            // Handle EDIT MODE ONLY - get values from ViewState
            if ((e.Row.RowState & DataControlRowState.Edit) == DataControlRowState.Edit)
            {
                int rowIndex = e.Row.RowIndex;

                // Get stored values from ViewState
                int packagingId = ViewState[$"PackagingID_{rowIndex}"] != null ? (int)ViewState[$"PackagingID_{rowIndex}"] : 0;
                int itemTypeId = ViewState[$"ItemTypeID_{rowIndex}"] != null ? (int)ViewState[$"ItemTypeID_{rowIndex}"] : 0;
                // Handle packaging ComboBox
                var cboPackaging = e.Row.FindControl("cboPackaging") as AjaxControlToolkit.ComboBox;
                if (cboPackaging != null)
                {
                    // DON'T CALL DataBind() - it causes the Bind() expression error
                    // The ComboBox should already be populated by its DataSourceID

                    // Ensure "n/a" option exists
                    if (!cboPackaging.Items.Cast<ListItem>().Any(item => item.Value == "0"))
                    {
                        cboPackaging.Items.Insert(0, new ListItem("n/a", "0"));
                    }

                    // Set the selected value safely
                    string packagingIdStr = packagingId.ToString();
                    if (cboPackaging.Items.Cast<ListItem>().Any(item => item.Value == packagingIdStr))
                    {
                        cboPackaging.SelectedValue = packagingIdStr;
                    }
                    else if (packagingId > 0)
                    {
                        // Add missing packaging option for inactive items
                        string description = GetPackagingDesc(packagingId);
                        if (!string.IsNullOrEmpty(description))
                        {
                            cboPackaging.Items.Add(new ListItem($"{description} (Inactive)", packagingIdStr));
                            cboPackaging.SelectedValue = packagingIdStr;
                        }
                        else
                        {
                            cboPackaging.SelectedValue = "0";
                        }
                    }
                    else
                    {
                        cboPackaging.SelectedValue = "0";
                    }
                }

                // Handle item ComboBox the same way
                var cboItemDesc = e.Row.FindControl("cboItemDesc") as AjaxControlToolkit.ComboBox;
                if (cboItemDesc != null)
                {
                    // DON'T CALL DataBind() - it causes the Bind() expression error
                    // The ComboBox should already be populated by its DataSourceID

                    if (!cboItemDesc.Items.Cast<ListItem>().Any(item => item.Value == "0"))
                    {
                        cboItemDesc.Items.Insert(0, new ListItem("--Invalid Item--", "0"));
                    }

                    string itemTypeIdStr = itemTypeId.ToString();
                    if (cboItemDesc.Items.Cast<ListItem>().Any(item => item.Value == itemTypeIdStr))
                    {
                        cboItemDesc.SelectedValue = itemTypeIdStr;  
                    }
                    else
                    {
                        cboItemDesc.SelectedValue = "0";
                    }
                }
                // Clear ViewState after use
                ViewState.Remove($"PackagingID_{rowIndex}");
                ViewState.Remove($"ItemTypeID_{rowIndex}");
            }
        }
        protected void gvOrderLines_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvOrderLines.EditIndex = -1;
            gvOrderLines.DataBind();
            upnlOrderLines.Update();
        }
        protected void gvOrderLines_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            GridViewRow row = gvOrderLines.Rows[e.RowIndex];

            // Use helper method for cleaner code
            e.NewValues["ItemTypeID"] = Convert.ToInt32(GetControlSelectedValue(row, CONST_ORDERLINE_ITEM_COMBOBOX_ID, CONST_ORDERLINE_HIDDENFIELD_ITEM_ID));
            e.NewValues["PackagingID"] = Convert.ToInt32(GetControlSelectedValue(row, CONST_ORDERLINE_PACKAGING_COMBOBOX_ID, CONST_ORDERLINE_HIDDENFIELD_PACKAGING_ID));

            // Get quantity from TextBox
            var tbxQuantityOrdered = row.FindControl("tbxQuantityOrdered") as TextBox;
            if (tbxQuantityOrdered != null)
            {
                e.NewValues["QuantityOrdered"] = Convert.ToDouble(tbxQuantityOrdered.Text);
            }

            // Get OrderID - this should be set as NewValues, not Keys
            var hdnOrderID = row.FindControl(CONST_ORDERLINE_HIDDENFIELD_ORDER_ID) as HiddenField;
            if (hdnOrderID != null)
            {
                e.NewValues["OrderID"] = Convert.ToInt64(hdnOrderID.Value);
            }

            // Debug output
            System.Diagnostics.Debug.WriteLine($"RowUpdating: OrderID = {e.NewValues["OrderID"]}, PackagingID = {e.NewValues["PackagingID"]}, ItemTypeID = {e.NewValues["ItemTypeID"]}");
        }
        protected void gvOrderLines_RowEditing(object sender, GridViewEditEventArgs e)
        {
            // Get values from hidden fields in view mode BEFORE setting edit mode
            GridViewRow row = gvOrderLines.Rows[e.NewEditIndex];

            var hdnPackagingID = row.FindControl("hdnPackagingID") as HiddenField;
            var hdnItemTypeID = row.FindControl("hdnItemTypeID") as HiddenField;

            int packagingId = 0;
            int itemTypeId = 0;

            if (hdnPackagingID != null && !string.IsNullOrEmpty(hdnPackagingID.Value))
            {
                int.TryParse(hdnPackagingID.Value, out packagingId);
            }

            if (hdnItemTypeID != null && !string.IsNullOrEmpty(hdnItemTypeID.Value))
            {
                int.TryParse(hdnItemTypeID.Value, out itemTypeId);
            }

            ViewState[$"PackagingID_{e.NewEditIndex}"] = packagingId;
            ViewState[$"ItemTypeID_{e.NewEditIndex}"] = itemTypeId;
            // Set edit mode and let the ObjectDataSource handle the binding
            gvOrderLines.EditIndex = e.NewEditIndex;

            // REMOVE THESE LINES - they cause the infinite loop!
            // odsOrderDetail.DataBind();
            // gvOrderLines.DataBind();

            upnlOrderLines.Update();
        }
        protected void btnUnDoDone_Click(object sender, EventArgs e)
        {
            var manager = new TrackerSQL.Managers.OrderManager();
            string empty = string.Empty;
            int count = 0;
            foreach (TableRow row in this.gvOrderLines.Rows)
            {
                var control = (HiddenField)row.Cells[4].FindControl(CONST_ORDERLINE_HIDDENFIELD_ORDER_ID);
                empty += manager.UnDoOrderItem(Convert.ToInt32(control.Value));
            }
            this.ltrlStatus.Text = empty;
            AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"UndoDone applied to {count} order line(s).");

            this.dvOrderHeader.DataBind();
            this.pnlOrderHeader.Update();
            this.gvOrderLines.DataBind();
            this.upnlOrderLines.Update();
        }
        protected void gvOrderLines_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            bool flag = false;
            if (e.CommandName == "MoveOneDayOn")
            {
                flag = true;
                var control = (HiddenField)this.gvOrderLines.Rows[Convert.ToInt32(e.CommandArgument)].FindControl(CONST_ORDERLINE_HIDDENFIELD_ORDER_ID);
                DateTime pNewDate = Convert.ToDateTime(this.dvOrderHeaderGetLabelValue("lblRequiredByDate")).Date;
                if (pNewDate.DayOfWeek < DayOfWeek.Friday)
                {
                    pNewDate = pNewDate.AddDays(1.0);
                }
                else
                {
                    int num = (int)(1 - pNewDate.DayOfWeek + 7) % 7;
                    pNewDate = pNewDate.AddDays((double)num);
                }
                new OrderTbl().UpdateOrderDeliveryDate(pNewDate, Convert.ToInt32(control.Value));
            }
            else if (e.CommandName == "DeleteOrder")
            {
                flag = true;
                this.DeleteOrderItem(e.CommandArgument.ToString());
            }
            if (!flag)
                return;
            this.dvOrderHeader.DataBind();
            this.pnlOrderHeader.Update();
            this.gvOrderLines.DataBind();
            this.upnlOrderLines.Update();
        }

        protected void tbxManualNotes_TextChanged1(object sender, EventArgs e)
        {

        }
        
    }
}