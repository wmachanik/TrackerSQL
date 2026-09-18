using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Models;
using TrackerSQL.Repositories;
using TrackerSQL.Managers;
using TrackerSQL.Classes; // for TimeZoneUtils, AppLogger

namespace TrackerSQL.Pages
{
    public partial class ContactDetails : Page
    {
        protected global::System.Web.UI.WebControls.GridView gvPrediction;

        private const string CONST_URL_REQUEST_ID = "ID";
        private const string SESSION_RETURN_URL = "ContactDetailsReturnUrl";
        private const string DefaultReturnUrl = "~/Pages/Contacts.aspx";

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            WireItemUsageGridEvents();
            WireContactRepairsGridEvents();
            WireContactOrdersGridEvents();
        }

        private void WireItemUsageGridEvents()
        {
            if (gvContactItems == null)
                return;

            gvContactItems.RowEditing -= gvContactItems_RowEditing;
            gvContactItems.RowCancelingEdit -= gvContactItems_RowCancelingEdit;
            gvContactItems.RowUpdating -= gvContactItems_RowUpdating;
            gvContactItems.RowDeleting -= gvContactItems_RowDeleting;
            gvContactItems.PageIndexChanging -= gvContactItems_PageIndexChanging;
            gvContactItems.RowDataBound -= gvContactItems_RowDataBound;
            gvContactItems.RowCommand -= gvContactItems_RowCommand;

            gvContactItems.RowEditing += gvContactItems_RowEditing;
            gvContactItems.RowCancelingEdit += gvContactItems_RowCancelingEdit;
            gvContactItems.RowUpdating += gvContactItems_RowUpdating;
            gvContactItems.RowDeleting += gvContactItems_RowDeleting;
            gvContactItems.PageIndexChanging += gvContactItems_PageIndexChanging;
            gvContactItems.RowDataBound += gvContactItems_RowDataBound;
            gvContactItems.RowCommand += gvContactItems_RowCommand;
        }

        private void WireContactRepairsGridEvents()
        {
            if (gvContactRepairs == null)
                return;

            gvContactRepairs.PageIndexChanging -= gvContactRepairs_PageIndexChanging;
            gvContactRepairs.RowCreated -= gvContactRepairs_RowCreated;

            gvContactRepairs.PageIndexChanging += gvContactRepairs_PageIndexChanging;
            gvContactRepairs.RowCreated += gvContactRepairs_RowCreated;
        }

        private void WireContactOrdersGridEvents()
        {
            if (gvContactOrders == null)
                return;

            gvContactOrders.PageIndexChanging -= gvContactOrders_PageIndexChanging;
            gvContactOrders.RowCreated -= gvContactOrders_RowCreated;

            gvContactOrders.PageIndexChanging += gvContactOrders_PageIndexChanging;
            gvContactOrders.RowCreated += gvContactOrders_RowCreated;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CaptureReturnUrlIfNeeded();

                // Ensure all lookup lists are bound before selecting values
                DataBindLookups();

                int id = GetContactIdFromRequest();
                if (id > 0)
                {
                    LoadContact(id);
                    SetButtonStatus(true);
                }
                else
                {
                    SetButtonStatus(false);
                    enabledCheckBox.Checked = true;
                    ApplyNewContactDefaults();
                }
            }
        }

        /// <summary>
        /// Remembers the page that opened Contact Details (referrer or ?ReturnUrl=).
        /// Default fallback is Contacts list. Same pattern as OrderDetail.
        /// </summary>
        private void CaptureReturnUrlIfNeeded()
        {
            ReturnUrlHelper.CaptureIfNeeded(this, SESSION_RETURN_URL, DefaultReturnUrl, "ContactDetails.aspx");
        }

        private string GetReturnUrl()
        {
            return ReturnUrlHelper.Get(this, SESSION_RETURN_URL, DefaultReturnUrl);
        }

        private void ReturnToCaller()
        {
            Response.Redirect(GetReturnUrl(), false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private void DataBindLookups()
        {
            try
            {
                ddlAreas.DataBind();
                ddlContactTypes.DataBind();
                ddlEquipTypes.DataBind();
                ddlFirstPreference.DataBind();
                ddlItemPackagingTypes.DataBind();
                ddlDeliveryBy.DataBind();
                ddlAgent.DataBind();
                BindCourierServiceDropdown(null);
                accInvoiceTypesDropDownList.DataBind();
                accPaymentTermsDropDownList.DataBind();
                accPriceLevelsDropDownList.DataBind();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "ContactDetails DataBindLookups error: " + ex.Message);
            }
        }

        private void BindCourierServiceDropdown(int? selectedId)
        {
            try
            {
                var repo = new CourierServicesRepository();
                repo.EnsureExists();
                // Contact preference: default to None unless a preferred courier is set.
                int? select = selectedId;
                if (!select.HasValue || select.Value <= 0)
                {
                    var none = repo.GetByCode("None");
                    select = none?.CourierServiceID;
                }
                repo.FillDropDown(ddlCourierService, select, includeNone: true);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "ContactDetails BindCourierServiceDropdown: " + ex.Message);
            }
        }

        private void SetStatus(string message, bool? isError)
        {
            StatusMessageHelper.Set(pnlStatus, ltrlStatus, HttpUtility.HtmlEncode(message ?? string.Empty), isError);
            if (pnlStatus != null && !string.IsNullOrWhiteSpace(message))
                pnlStatus.Visible = true;
        }

        /// <summary>
        /// Writes a customers.log audit line. AppLogger already prefixes the logged-in user.
        /// </summary>
        private void LogContactAudit(string action, string details = null, int? contactIdOverride = null)
        {
            int contactId = contactIdOverride
                ?? (TryGetContactId(out int id) ? id : 0);

            string company = CompanyNameTextBox?.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(company) && contactId > 0)
                company = new ContactsRepository().GetContactNameById(contactId) ?? string.Empty;

            string contactPart = contactId > 0
                ? (string.IsNullOrWhiteSpace(company)
                    ? $"Contact={contactId}"
                    : $"Contact={contactId} ({company})")
                : (string.IsNullOrWhiteSpace(company)
                    ? "Contact=(new)"
                    : $"Contact=(new) ({company})");

            string line = $"{contactPart} | {action}";
            if (!string.IsNullOrWhiteSpace(details))
                line += $" | {details}";

            AppLogger.WriteLog(SystemConstants.LogTypes.Customers, line);
        }

        private bool TryGetContactId(out int contactId)
        {
            contactId = 0;
            return int.TryParse(CompanyIDLabel.Text, out contactId) && contactId > 0;
        }

        private bool EnsureValidForSave()
        {
            Page.Validate();
            if (Page.IsValid)
                return true;

            SetStatus("Save cancelled — fix the validation errors below.", true);
            return false;
        }

        private void RefreshAfterSave()
        {
            upnlContactDetails.Update();
            if (dvContactsAccInfoUpdatePanel != null)
                dvContactsAccInfoUpdatePanel.Update();
            if (uppnlTabContainer != null)
                uppnlTabContainer.Update();
        }

        private int GetContactIdFromRequest()
        {
            if (Request.QueryString[CONST_URL_REQUEST_ID] != null && int.TryParse(Request.QueryString[CONST_URL_REQUEST_ID], out int id))
                return id;
            return 0;
        }

        private void LoadContact(int id)
        {
            try
            {
                var repo = new ContactsRepository();
                var contact = repo.GetById(id);
                if (contact == null)
                {
                    SetStatus("Contact not found.", true);
                    return;
                }

                CompanyIDLabel.Text = contact.ContactID.ToString();
                CompanyNameTextBox.Text = contact.CompanyName;

                ContactFirstNameTextBox.Text = contact.ContactFirstName;
                ContactLastNameTextBox.Text = contact.ContactLastName;
                ContactTitleTextBox.Text = contact.ContactTitle;
                ContactAltFirstNameTextBox.Text = contact.ContactAltFirstName;
                ContactAltLastNameTextBox.Text = contact.ContactAltLastName;

                BillingAddressTextBox.Text = contact.BillingAddress;
                DepartmentTextBox.Text = contact.Department;
                ProvinceTextBox.Text = contact.StateOrProvince;
                PostalCodeTextBox.Text = contact.PostalCode;

                PhoneNumberTextBox.Text = contact.PhoneNumber;
                CellNumberTextBox.Text = contact.CellNumber;
                FaxNumberTextBox.Text = contact.FaxNumber;

                EmailAddressTextBox.Text = contact.EmailAddress;
                AltEmailAddressTextBox.Text = contact.AltEmailAddress;

                enabledCheckBox.Checked = contact.Enabled ?? false;
                autofulfillCheckBox.Checked = contact.AutoFulfill ?? false;
                UsesFilterCheckBox.Checked = contact.UsesFilter ?? false;
                PredictionDisabledCheckBox.Checked = contact.PredictionDisabled ?? false;
                AlwaysSendChkUpCheckBox.Checked = contact.AlwaysSendChkUp ?? false;
                NormallyRespondsCheckBox.Checked = contact.NormallyResponds ?? false;

                NotesTextBox.Text = contact.Notes;
                ReminderCountLabel.Text = (contact.ReminderCount ?? 0).ToString();
                LastReminderLabel.Text = contact.LastDateSentReminder.HasValue ? contact.LastDateSentReminder.Value.ToString("yyyy-MM-dd") : "never";

                TrySelectDropDownByValue(ddlAreas, contact.AreaID);
                TrySelectDropDownByValue(ddlContactTypes, contact.ContactTypeID);
                TrySelectDropDownByValue(ddlEquipTypes, contact.EquipTypeID);
                TrySelectDropDownByValue(ddlFirstPreference, contact.ItemPrefID);
                TrySelectDropDownByValue(ddlItemPackagingTypes, contact.PrefItemPackagingID);

                TrySelectDropDownByValue(ddlDeliveryBy, contact.PreferredAgentID);
                BindCourierServiceDropdown(contact.PreferredCourierServiceID);
                TrySelectDropDownByValue(ddlAgent, contact.SalesAgentID);

                PriPrefQtyTextBox.Text = contact.PriPrefQty.HasValue ? contact.PriPrefQty.Value.ToString("0.##") : string.Empty;
                MachineSNTextBox.Text = contact.EquipentSN;

                // Accounts section defaults - if you have ContactsAccInfo, use repository to load it.
                var accRepo = new ContactsAccInfoRepository();
                var acc = accRepo.GetByContactId(id);
                if (acc != null)
                {
                    accFullCoNameTextBox.Text = acc.FullCoName;
                    accContactVATNoTextBox.Text = acc.ContactVATNo;
                    TrySelectDropDownByValue(accInvoiceTypesDropDownList, acc.InvoiceTypeID);
                    accRequiresPurchOrderCheckBox.Checked = acc.RequiresPurchOrder ?? false;
                    accEnabledCheckBox.Checked = acc.Enabled ?? false;
                    accBillAddr1TextBox.Text = acc.BillAddr1;
                    accBillAddr2TextBox.Text = acc.BillAddr2;
                    accBillAddr3TextBox.Text = acc.BillAddr3;
                    accBillAddr4TextBox.Text = acc.BillAddr4;
                    accBillAddr5TextBox.Text = acc.BillAddr5;
                    accShipAddr1TextBox.Text = acc.ShipAddr1;
                    accShipAddr2TextBox.Text = acc.ShipAddr2;
                    accShipAddr3TextBox.Text = acc.ShipAddr3;
                    accShipAddr4TextBox.Text = acc.ShipAddr4;
                    accShipAddr5TextBox.Text = acc.ShipAddr5;
                    accFirstNameTextBox.Text = acc.AccFirstName;
                    accLastNameTextBox.Text = acc.AccLastName;
                    accAccEmailTextBox.Text = acc.AccEmail;
                    accAltFirstNameTextBox.Text = acc.AltAccFirstName;
                    accAltLastNameTextBox.Text = acc.AltAccLastName;
                    accAltEmailTextBox.Text = acc.AltAccEmail;
                    TrySelectDropDownByValue(accPaymentTermsDropDownList, acc.PaymentTermID);
                    TrySelectDropDownByValue(accPriceLevelsDropDownList, acc.PriceLevelID);
                    accRegNoTextBox.Text = acc.RegNo;
                    accLimitTextBox.Text = acc.Limit.HasValue ? acc.Limit.Value.ToString("0.##") : string.Empty;
                    accBankAccNoTextBox.Text = acc.BankAccNo;
                    accBankBranchTextBox.Text = acc.BankBranch;
                    accNotesTextBox.Text = acc.Notes;
                    ContactsAccInfoIDLabel.Text = acc.ContactsAccInfoID.ToString();
                }

                // Bind usage lines grid (last items used)
                BindUsageLines(id);

                // Orders / Recurring / Repairs history tabs
                BindHistoryTabs(id);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "Error loading contact: " + ex.Message);
                SetStatus("Error loading contact.", true);
            }
        }

        /// <summary>
        /// Recurring Orders and Repairs tabs — each only appears when it has data.
        /// Orders tab is always shown (like Item Usage). Repairs lists every repair for the contact.
        /// </summary>
        private void BindHistoryTabs(int contactId)
        {
            try
            {
                BindContactOrdersGrid(contactId);
                BindContactWaybillsGrid(contactId);

                var recurring = new RecurringOrdersRepository().GetSummariesByContactId(contactId);
                tabpnlRecurring.Visible = recurring.Count > 0;
                if (tabpnlRecurring.Visible)
                {
                    gvContactRecurring.DataSource = recurring;
                    gvContactRecurring.DataBind();
                }

                BindContactRepairsGrid(contactId);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    "ContactDetails.BindHistoryTabs error for ContactID=" + contactId + ": " + ex.Message);
                tabpnlRecurring.Visible = false;
                tabpnlRepairs.Visible = false;
                if (tabpnlWaybills != null)
                    tabpnlWaybills.Visible = false;
            }
        }

        private void BindContactOrdersGrid(int contactId)
        {
            if (gvContactOrders == null)
                return;

            var orders = new OrdersRepository().GetSummariesByContactId(contactId)
                ?? new List<ContactOrderSummary>();

            int pageCount = Math.Max(1, (int)Math.Ceiling(orders.Count / (double)gvContactOrders.PageSize));
            if (gvContactOrders.PageIndex >= pageCount)
                gvContactOrders.PageIndex = pageCount - 1;

            gvContactOrders.DataSource = orders;
            gvContactOrders.DataBind();

            if (upnlContactOrders != null)
                upnlContactOrders.Update();
        }

        private void BindContactWaybillsGrid(int contactId)
        {
            if (gvContactWaybills == null || tabpnlWaybills == null)
                return;

            var rows = new OrderWaybillRepository().GetByContactId(contactId) ?? new List<OrderWaybill>();
            tabpnlWaybills.Visible = rows.Count > 0;
            if (!tabpnlWaybills.Visible)
                return;

            gvContactWaybills.DataSource = rows;
            gvContactWaybills.DataBind();
        }

        protected void gvContactOrders_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvContactOrders.PageIndex = e.NewPageIndex;
            if (TryGetContactId(out int contactId))
                BindContactOrdersGrid(contactId);
        }

        protected void gvContactOrders_RowCreated(object sender, GridViewRowEventArgs e)
        {
            GridPager.BuildPager(gvContactOrders, e.Row);
        }

        private void BindContactRepairsGrid(int contactId)
        {
            var repairs = new RepairsRepository().GetByContactId(contactId) ?? new List<Repair>();
            tabpnlRepairs.Visible = repairs.Count > 0;
            if (!tabpnlRepairs.Visible)
                return;

            // Keep pager in range after deletes / smaller result sets.
            int pageCount = Math.Max(1, (int)Math.Ceiling(repairs.Count / (double)gvContactRepairs.PageSize));
            if (gvContactRepairs.PageIndex >= pageCount)
                gvContactRepairs.PageIndex = pageCount - 1;

            gvContactRepairs.DataSource = repairs;
            gvContactRepairs.DataBind();
        }

        protected void gvContactRepairs_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            int contactId = GetLoadedContactId();
            gvContactRepairs.PageIndex = e.NewPageIndex;
            if (contactId > 0)
                BindContactRepairsGrid(contactId);
            if (uppnlTabContainer != null)
                uppnlTabContainer.Update();
        }

        protected void gvContactRepairs_RowCreated(object sender, GridViewRowEventArgs e)
        {
            GridPager.BuildPager(gvContactRepairs, e.Row);
        }

        /// <summary>True while the repair is not done (used for Edit vs View tooltip).</summary>
        protected bool IsRepairEditable(object repairStatusId)
        {
            int statusId = repairStatusId == null ? 0 : Convert.ToInt32(repairStatusId);
            return statusId != RepairsRepository.DoneStatusId;
        }

        protected string GetRepairStatusDesc(object repairStatusId)
        {
            int statusId = repairStatusId == null ? 0 : Convert.ToInt32(repairStatusId);
            return statusId > 0 ? new RepairStatusesRepository().GetRepairStatusDesc(statusId) : string.Empty;
        }

        private void BindUsageLines(int contactId)
        {
            try
            {
                var usageRepo = new TrackerSQL.Repositories.ContactsUsageRepository();
                var usage = usageRepo.GetByContactId(contactId);

                if (usage != null)
                {
                    gvPrediction.DataSource = new[] { usage };
                }
                else
                {
                    gvPrediction.DataSource = new List<TrackerSQL.Models.ContactsUsage>();
                }
                gvPrediction.DataBind();

                BindItemUsageGrid(contactId);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "BindUsageLines error: " + ex.Message);
                SetStatus("Error loading usage data: " + ex.Message, true);
            }
        }

        private void BindItemUsageGrid(int contactId)
        {
            var itemsRepo = new ContactsItemUsageRepository();
            var items = itemsRepo.GetByContactId(contactId, "DeliveryDate DESC") ?? new List<ContactsItemUsage>();

            var itemLookup = new ItemsRepository().GetAll("ItemDesc") ?? new List<Item>();
            var prepLookup = new ItemPrepTypesRepository().GetAll("ItemPrepTypeDesc") ?? new List<ItemPrepType>();
            var packLookup = new ItemPackagingsRepository().GetAll("ItemPackagingDesc") ?? new List<ItemPackaging>();

            var uiItems = items.ConvertAll(x => new ContactItemUsageRow
            {
                ContactItemUsageLineNo = x.ContactItemUsageLineNo,
                DeliveryDate = x.DeliveryDate,
                ItemProvidedID = NullIfZero(x.ItemProvidedID),
                ItemProvided = ResolveLookupName(
                    x.ItemProvidedID,
                    id => itemLookup.Find(i => i.ItemID == id)?.ItemDesc),
                QtyProvided = x.QtyProvided,
                ItemPrepTypeID = NullIfZero(x.ItemPrepTypeID),
                PrepType = ResolveLookupName(
                    x.ItemPrepTypeID,
                    id => prepLookup.Find(p => p.ItemPrepID == id)?.ItemPrepTypeDesc),
                ItemPackagingID = NullIfZero(x.ItemPackagingID),
                Packaging = ResolveLookupName(
                    x.ItemPackagingID,
                    id => packLookup.Find(p => p.ItemPackagingID == id)?.ItemPackagingDesc),
                Notes = x.Notes
            });

            gvContactItems.DataSource = uiItems;
            gvContactItems.DataBind();
        }

        private static int? NullIfZero(int? value)
        {
            return value.HasValue && value.Value > 0 ? value : null;
        }

        /// <summary>
        /// Shows lookup text; id null/0 or unknown lookup → n/a (never raw ids).
        /// </summary>
        private static string ResolveLookupName(int? id, Func<int, string> resolve)
        {
            if (!id.HasValue || id.Value <= 0)
                return "n/a";

            string name = resolve(id.Value);
            return string.IsNullOrWhiteSpace(name) ? "n/a" : name;
        }

        private int GetLoadedContactId()
        {
            if (int.TryParse(CompanyIDLabel.Text, out int id) && id > 0)
                return id;
            return GetContactIdFromRequest();
        }

        private sealed class ContactItemUsageRow
        {
            public int ContactItemUsageLineNo { get; set; }
            public DateTime? DeliveryDate { get; set; }
            public int? ItemProvidedID { get; set; }
            public string ItemProvided { get; set; }
            public double? QtyProvided { get; set; }
            public int? ItemPrepTypeID { get; set; }
            public string PrepType { get; set; }
            public int? ItemPackagingID { get; set; }
            public string Packaging { get; set; }
            public string Notes { get; set; }
        }

        private void TrySelectDropDownByValue(DropDownList ddl, int? value)
        {
            try
            {
                if (ddl == null || !value.HasValue) return;
                var item = ddl.Items.FindByValue(value.Value.ToString());
                if (item != null)
                {
                    ddl.ClearSelection();
                    item.Selected = true;
                }
            }
            catch { }
        }

        private void SetButtonStatus(bool editMode)
        {
            btnUpdate.Enabled = editMode;
            btnUpdateAndReturn.Enabled = editMode;
            btnCopy2AccInfo.Enabled = editMode;
            btnAddLasOrder.Enabled = editMode;
            btnForceNext.Enabled = editMode;
            btnForceCheckup.Enabled = editMode;
            btnSendReminder.Enabled = editMode;
            btnRecalcAverage.Enabled = editMode;
            btnInsert.Enabled = !editMode;
            accAddDetailsButton.Enabled = !editMode;
            accUpdateButton.Enabled = editMode;
        }

        private void ApplyNewContactDefaults()
        {
            int salesAgentId = PersonDefaults.GetDefaultSalesAgentId();
            if (salesAgentId > 0)
                TrySelectDropDownByValue(ddlAgent, salesAgentId);

            // Default account / invoice type to Standard for new contacts.
            TrySelectDropDownByValue(
                accInvoiceTypesDropDownList,
                SystemConstants.InvoiceTypeConstants.DefaultForNewContact);
        }

        private int? ParseNullableInt(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (!int.TryParse(value, out int parsed) || parsed <= 0) return null;
            return parsed;
        }

        private double? ParseNullableDouble(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (!double.TryParse(value, out double parsed)) return null;
            return parsed;
        }

        private string NullIfEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        /// <summary>
        /// Legacy Copy2Acc behaviour: map main contact fields into Accounts Info,
        /// splitting billing address on comma/semicolon into address lines.
        /// </summary>
        private static ContactsAccInfo CopyContactDataToAccInfo(Contact contact, int contactId = 0)
        {
            var acc = new ContactsAccInfo
            {
                ContactID = contactId > 0 ? contactId : contact.ContactID,
                FullCoName = contact.CompanyName ?? string.Empty,
                AccFirstName = contact.ContactFirstName ?? string.Empty,
                AccLastName = contact.ContactLastName ?? string.Empty,
                AltAccFirstName = contact.ContactAltFirstName ?? string.Empty,
                AltAccLastName = contact.ContactAltLastName ?? string.Empty,
                AccEmail = contact.EmailAddress ?? string.Empty,
                AltAccEmail = contact.AltEmailAddress ?? string.Empty,
                InvoiceTypeID = SystemConstants.InvoiceTypeConstants.DefaultForNewContact,
                Enabled = true
            };

            string billing = contact.BillingAddress ?? string.Empty;
            string[] parts = billing.Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
            acc.BillAddr1 = parts.Length > 0 ? parts[0].Trim() : string.Empty;
            acc.BillAddr2 = parts.Length > 1 ? parts[1].Trim() : string.Empty;
            acc.BillAddr3 = parts.Length > 2 ? parts[2].Trim() : string.Empty;
            for (int index = 3; index < parts.Length; index++)
            {
                acc.BillAddr4 = parts[index].Trim();
                if (index + 1 < parts.Length)
                    acc.BillAddr4 += ";";
            }

            acc.BillAddr5 = contact.PostalCode ?? string.Empty;
            acc.ShipAddr1 = acc.BillAddr1;
            acc.ShipAddr2 = acc.BillAddr2;
            acc.ShipAddr3 = acc.BillAddr3;
            acc.ShipAddr4 = acc.BillAddr4;
            acc.ShipAddr5 = acc.BillAddr5;

            return acc;
        }

        private void PlaceAccInfoOnForm(ContactsAccInfo acc)
        {
            if (acc == null)
                return;

            accFullCoNameTextBox.Text = acc.FullCoName ?? string.Empty;
            accContactVATNoTextBox.Text = acc.ContactVATNo ?? string.Empty;
            if (acc.InvoiceTypeID.HasValue && accInvoiceTypesDropDownList.Items.FindByValue(acc.InvoiceTypeID.Value.ToString()) != null)
                accInvoiceTypesDropDownList.SelectedValue = acc.InvoiceTypeID.Value.ToString();
            accRequiresPurchOrderCheckBox.Checked = acc.RequiresPurchOrder == true;
            accEnabledCheckBox.Checked = acc.Enabled != false;
            accBillAddr1TextBox.Text = acc.BillAddr1 ?? string.Empty;
            accBillAddr2TextBox.Text = acc.BillAddr2 ?? string.Empty;
            accBillAddr3TextBox.Text = acc.BillAddr3 ?? string.Empty;
            accBillAddr4TextBox.Text = acc.BillAddr4 ?? string.Empty;
            accBillAddr5TextBox.Text = acc.BillAddr5 ?? string.Empty;
            accShipAddr1TextBox.Text = acc.ShipAddr1 ?? string.Empty;
            accShipAddr2TextBox.Text = acc.ShipAddr2 ?? string.Empty;
            accShipAddr3TextBox.Text = acc.ShipAddr3 ?? string.Empty;
            accShipAddr4TextBox.Text = acc.ShipAddr4 ?? string.Empty;
            accShipAddr5TextBox.Text = acc.ShipAddr5 ?? string.Empty;
            accFirstNameTextBox.Text = acc.AccFirstName ?? string.Empty;
            accLastNameTextBox.Text = acc.AccLastName ?? string.Empty;
            accAccEmailTextBox.Text = acc.AccEmail ?? string.Empty;
            accAltFirstNameTextBox.Text = acc.AltAccFirstName ?? string.Empty;
            accAltLastNameTextBox.Text = acc.AltAccLastName ?? string.Empty;
            accAltEmailTextBox.Text = acc.AltAccEmail ?? string.Empty;
            if (acc.PaymentTermID.HasValue && accPaymentTermsDropDownList.Items.FindByValue(acc.PaymentTermID.Value.ToString()) != null)
                accPaymentTermsDropDownList.SelectedValue = acc.PaymentTermID.Value.ToString();
            if (acc.PriceLevelID.HasValue && accPriceLevelsDropDownList.Items.FindByValue(acc.PriceLevelID.Value.ToString()) != null)
                accPriceLevelsDropDownList.SelectedValue = acc.PriceLevelID.Value.ToString();
            accRegNoTextBox.Text = acc.RegNo ?? string.Empty;
            accLimitTextBox.Text = acc.Limit.HasValue ? $"{acc.Limit.Value:0.00}" : string.Empty;
            accBankAccNoTextBox.Text = acc.BankAccNo ?? string.Empty;
            accBankBranchTextBox.Text = acc.BankBranch ?? string.Empty;
            accNotesTextBox.Text = acc.Notes ?? string.Empty;
            if (acc.ContactsAccInfoID > 0)
                ContactsAccInfoIDLabel.Text = acc.ContactsAccInfoID.ToString();
        }

        /// <summary>
        /// Builds a Contact from the form. When <paramref name="existing"/> is provided, preserves fields not on the form.
        /// </summary>
        private Contact ReadContactFromForm(Contact existing)
        {
            var contact = existing != null
                ? new Contact
                {
                    ContactID = existing.ContactID,
                    CountryOrRegion = existing.CountryOrRegion,
                    Extension = existing.Extension,
                    ContractNo = existing.ContractNo,
                    PrefItemPrepTypeID = existing.PrefItemPrepTypeID,
                    SecondaryItemPrefID = existing.SecondaryItemPrefID,
                    SecPrefQty = existing.SecPrefQty,
                    TypicallySecToo = existing.TypicallySecToo,
                    ReminderCount = existing.ReminderCount,
                    LastDateSentReminder = existing.LastDateSentReminder,
                    SendDeliveryConfirmation = existing.SendDeliveryConfirmation
                }
                : new Contact { ReminderCount = 0, Enabled = true };

            contact.CompanyName = NullIfEmpty(CompanyNameTextBox.Text);
            contact.ContactTitle = NullIfEmpty(ContactTitleTextBox.Text);
            contact.ContactFirstName = NullIfEmpty(ContactFirstNameTextBox.Text);
            contact.ContactLastName = NullIfEmpty(ContactLastNameTextBox.Text);
            contact.ContactAltFirstName = NullIfEmpty(ContactAltFirstNameTextBox.Text);
            contact.ContactAltLastName = NullIfEmpty(ContactAltLastNameTextBox.Text);
            contact.Department = NullIfEmpty(DepartmentTextBox.Text);
            contact.BillingAddress = NullIfEmpty(BillingAddressTextBox.Text);
            contact.AreaID = ParseNullableInt(ddlAreas.SelectedValue);
            contact.StateOrProvince = NullIfEmpty(ProvinceTextBox.Text);
            contact.PostalCode = NullIfEmpty(PostalCodeTextBox.Text);
            contact.PhoneNumber = NullIfEmpty(PhoneNumberTextBox.Text);
            contact.CellNumber = NullIfEmpty(CellNumberTextBox.Text);
            contact.FaxNumber = NullIfEmpty(FaxNumberTextBox.Text);
            contact.EmailAddress = NullIfEmpty(EmailAddressTextBox.Text);
            contact.AltEmailAddress = NullIfEmpty(AltEmailAddressTextBox.Text);
            contact.ContactTypeID = ParseNullableInt(ddlContactTypes.SelectedValue);
            contact.EquipTypeID = ParseNullableInt(ddlEquipTypes.SelectedValue);
            contact.ItemPrefID = ParseNullableInt(ddlFirstPreference.SelectedValue);
            contact.PriPrefQty = ParseNullableDouble(PriPrefQtyTextBox.Text);
            contact.PrefItemPackagingID = ParseNullableInt(ddlItemPackagingTypes.SelectedValue);
            contact.PreferredAgentID = ParseNullableInt(ddlDeliveryBy.SelectedValue);
            contact.PreferredCourierServiceID = ParseNullableInt(ddlCourierService.SelectedValue);
            contact.SalesAgentID = ParseNullableInt(ddlAgent.SelectedValue);
            contact.EquipentSN = NullIfEmpty(MachineSNTextBox.Text);
            contact.Enabled = enabledCheckBox.Checked;
            contact.AutoFulfill = autofulfillCheckBox.Checked;
            contact.UsesFilter = UsesFilterCheckBox.Checked;
            contact.PredictionDisabled = PredictionDisabledCheckBox.Checked;
            contact.AlwaysSendChkUp = AlwaysSendChkUpCheckBox.Checked;
            contact.NormallyResponds = NormallyRespondsCheckBox.Checked;
            contact.Notes = NullIfEmpty(NotesTextBox.Text);

            return contact;
        }

        private ContactsAccInfo ReadAccInfoFromForm(int contactId)
        {
            return new ContactsAccInfo
            {
                ContactsAccInfoID = int.TryParse(ContactsAccInfoIDLabel.Text, out int accId) ? accId : 0,
                ContactID = contactId,
                FullCoName = NullIfEmpty(accFullCoNameTextBox.Text),
                ContactVATNo = NullIfEmpty(accContactVATNoTextBox.Text),
                InvoiceTypeID = ParseNullableInt(accInvoiceTypesDropDownList.SelectedValue),
                RequiresPurchOrder = accRequiresPurchOrderCheckBox.Checked,
                Enabled = accEnabledCheckBox.Checked,
                BillAddr1 = NullIfEmpty(accBillAddr1TextBox.Text),
                BillAddr2 = NullIfEmpty(accBillAddr2TextBox.Text),
                BillAddr3 = NullIfEmpty(accBillAddr3TextBox.Text),
                BillAddr4 = NullIfEmpty(accBillAddr4TextBox.Text),
                BillAddr5 = NullIfEmpty(accBillAddr5TextBox.Text),
                ShipAddr1 = NullIfEmpty(accShipAddr1TextBox.Text),
                ShipAddr2 = NullIfEmpty(accShipAddr2TextBox.Text),
                ShipAddr3 = NullIfEmpty(accShipAddr3TextBox.Text),
                ShipAddr4 = NullIfEmpty(accShipAddr4TextBox.Text),
                ShipAddr5 = NullIfEmpty(accShipAddr5TextBox.Text),
                AccFirstName = NullIfEmpty(accFirstNameTextBox.Text),
                AccLastName = NullIfEmpty(accLastNameTextBox.Text),
                AccEmail = NullIfEmpty(accAccEmailTextBox.Text),
                AltAccFirstName = NullIfEmpty(accAltFirstNameTextBox.Text),
                AltAccLastName = NullIfEmpty(accAltLastNameTextBox.Text),
                AltAccEmail = NullIfEmpty(accAltEmailTextBox.Text),
                PaymentTermID = ParseNullableInt(accPaymentTermsDropDownList.SelectedValue),
                PriceLevelID = ParseNullableInt(accPriceLevelsDropDownList.SelectedValue),
                RegNo = NullIfEmpty(accRegNoTextBox.Text),
                Limit = ParseNullableDouble(accLimitTextBox.Text),
                BankAccNo = NullIfEmpty(accBankAccNoTextBox.Text),
                BankBranch = NullIfEmpty(accBankBranchTextBox.Text),
                Notes = NullIfEmpty(accNotesTextBox.Text)
            };
        }

        private static bool AccInfoHasAnyData(ContactsAccInfo acc)
        {
            if (acc == null) return false;
            return !string.IsNullOrWhiteSpace(acc.FullCoName)
                || !string.IsNullOrWhiteSpace(acc.ContactVATNo)
                || !string.IsNullOrWhiteSpace(acc.AccEmail)
                || !string.IsNullOrWhiteSpace(acc.AltAccEmail)
                || !string.IsNullOrWhiteSpace(acc.AccFirstName)
                || !string.IsNullOrWhiteSpace(acc.AccLastName)
                || !string.IsNullOrWhiteSpace(acc.BillAddr1)
                || !string.IsNullOrWhiteSpace(acc.ShipAddr1)
                || !string.IsNullOrWhiteSpace(acc.RegNo)
                || !string.IsNullOrWhiteSpace(acc.BankAccNo)
                || !string.IsNullOrWhiteSpace(acc.Notes)
                || acc.Limit.HasValue
                || acc.RequiresPurchOrder == true;
        }

        private static string CoalesceText(string formValue, string defaultValue)
        {
            return string.IsNullOrWhiteSpace(formValue) ? (defaultValue ?? string.Empty) : formValue.Trim();
        }

        private static int? CoalesceFk(int? formValue, int? defaultValue)
        {
            if (formValue.HasValue && formValue.Value > 0)
                return formValue;
            if (defaultValue.HasValue && defaultValue.Value > 0)
                return defaultValue;
            return null;
        }

        /// <summary>
        /// Main-sheet defaults merged with Accounts tab fields (tab wins when filled).
        /// </summary>
        private ContactsAccInfo BuildAccInfoForSave(Contact contact, int contactId)
        {
            var defaults = CopyContactDataToAccInfo(contact, contactId);
            var form = ReadAccInfoFromForm(contactId);

            return new ContactsAccInfo
            {
                ContactsAccInfoID = form.ContactsAccInfoID,
                ContactID = contactId,
                FullCoName = CoalesceText(form.FullCoName, defaults.FullCoName),
                ContactVATNo = CoalesceText(form.ContactVATNo, defaults.ContactVATNo),
                InvoiceTypeID = CoalesceFk(form.InvoiceTypeID, defaults.InvoiceTypeID)
                    ?? SystemConstants.InvoiceTypeConstants.DefaultForNewContact,
                RequiresPurchOrder = form.RequiresPurchOrder ?? defaults.RequiresPurchOrder,
                Enabled = form.Enabled ?? defaults.Enabled ?? true,
                BillAddr1 = CoalesceText(form.BillAddr1, defaults.BillAddr1),
                BillAddr2 = CoalesceText(form.BillAddr2, defaults.BillAddr2),
                BillAddr3 = CoalesceText(form.BillAddr3, defaults.BillAddr3),
                BillAddr4 = CoalesceText(form.BillAddr4, defaults.BillAddr4),
                BillAddr5 = CoalesceText(form.BillAddr5, defaults.BillAddr5),
                ShipAddr1 = CoalesceText(form.ShipAddr1, defaults.ShipAddr1),
                ShipAddr2 = CoalesceText(form.ShipAddr2, defaults.ShipAddr2),
                ShipAddr3 = CoalesceText(form.ShipAddr3, defaults.ShipAddr3),
                ShipAddr4 = CoalesceText(form.ShipAddr4, defaults.ShipAddr4),
                ShipAddr5 = CoalesceText(form.ShipAddr5, defaults.ShipAddr5),
                AccFirstName = CoalesceText(form.AccFirstName, defaults.AccFirstName),
                AccLastName = CoalesceText(form.AccLastName, defaults.AccLastName),
                AccEmail = CoalesceText(form.AccEmail, defaults.AccEmail),
                AltAccFirstName = CoalesceText(form.AltAccFirstName, defaults.AltAccFirstName),
                AltAccLastName = CoalesceText(form.AltAccLastName, defaults.AltAccLastName),
                AltAccEmail = CoalesceText(form.AltAccEmail, defaults.AltAccEmail),
                PaymentTermID = CoalesceFk(form.PaymentTermID, defaults.PaymentTermID),
                PriceLevelID = CoalesceFk(form.PriceLevelID, defaults.PriceLevelID),
                RegNo = CoalesceText(form.RegNo, defaults.RegNo),
                Limit = form.Limit ?? defaults.Limit,
                BankAccNo = CoalesceText(form.BankAccNo, defaults.BankAccNo),
                BankBranch = CoalesceText(form.BankBranch, defaults.BankBranch),
                Notes = CoalesceText(form.Notes, defaults.Notes)
            };
        }

        /// <summary>
        /// Inserts or updates ContactsAccInfoTbl from the Accounts Info tab.
        /// </summary>
        private bool TrySaveAccInfo(ContactsAccInfo acc, out bool accWasSaved, out string errorMessage)
        {
            accWasSaved = false;
            errorMessage = null;
            try
            {
                if (acc == null)
                    return true;

                var accRepo = new ContactsAccInfoRepository();

                if (acc.ContactsAccInfoID > 0)
                {
                    int rows = accRepo.Update(acc);
                    if (rows <= 0)
                    {
                        errorMessage = "Account info update failed — no rows changed.";
                        return false;
                    }

                    accWasSaved = true;
                    return true;
                }

                if (!AccInfoHasAnyData(acc))
                    return true;

                int newId = accRepo.Insert(acc);
                if (newId <= 0)
                {
                    errorMessage = "Account info insert failed.";
                    return false;
                }

                acc.ContactsAccInfoID = newId;
                ContactsAccInfoIDLabel.Text = newId.ToString();
                accAddDetailsButton.Enabled = false;
                accUpdateButton.Enabled = true;
                accWasSaved = true;
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "ContactDetails TrySaveAccInfo error: " + ex.Message);
                errorMessage = "Error saving account info: " + ex.Message;
                return false;
            }
        }

        private bool TrySaveAccInfo(int contactId, Contact contact, out bool accWasSaved, out string errorMessage)
        {
            var acc = BuildAccInfoForSave(contact, contactId);
            PlaceAccInfoOnForm(acc);
            return TrySaveAccInfo(acc, out accWasSaved, out errorMessage);
        }

        private bool TrySaveContact(out string errorMessage, bool allowDuplicateName = false)
        {
            errorMessage = null;
            try
            {
                if (string.IsNullOrWhiteSpace(CompanyNameTextBox.Text))
                {
                    errorMessage = "Company Name is required.";
                    return false;
                }

                if (!TryGetContactId(out int contactId))
                {
                    errorMessage = "No contact loaded to save. For a new contact, click Insert first.";
                    return false;
                }

                var repo = new ContactsRepository();
                var existing = repo.GetById(contactId);
                if (existing == null)
                {
                    errorMessage = "Contact not found.";
                    return false;
                }

                var contact = ReadContactFromForm(existing);
                contact.ContactID = contactId;

                string company = (contact.CompanyName ?? string.Empty).Trim();
                var nameHits = repo.FindByCompanyNameExact(company, excludeContactId: contactId);
                if (!allowDuplicateName && nameHits.Count > 0)
                {
                    string unique = repo.EnsureUniqueCompanyName(company, excludeContactId: contactId);
                    ShowDuplicatePrompt(
                        BuildDuplicatePromptHtml(company, nameHits, emailHits: null, forRename: true, suggestedUniqueName: unique),
                        DuplicatePromptKind.Rename,
                        mergeTargetId: 0);
                    errorMessage = null;
                    return false;
                }

                if (allowDuplicateName)
                {
                    string unique = repo.EnsureUniqueCompanyName(company, excludeContactId: contactId);
                    if (!string.Equals(unique, company, StringComparison.OrdinalIgnoreCase))
                    {
                        contact.CompanyName = unique;
                        CompanyNameTextBox.Text = unique;
                    }
                }

                if (!repo.Update(contact))
                {
                    errorMessage = "Contact save failed — no rows updated.";
                    return false;
                }

                if (!TrySaveAccInfo(contactId, contact, out bool accWasSaved, out string accError))
                {
                    errorMessage = accError ?? "Account info save failed.";
                    return false;
                }

                ClearDirtyState();
                errorMessage = accWasSaved
                    ? "Contact and account info saved."
                    : "Contact saved.";
                if (allowDuplicateName
                    && !string.Equals(company, contact.CompanyName ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                {
                    errorMessage += " Name saved as \"" + contact.CompanyName + "\" to keep dropdown names unique.";
                }
                LogContactAudit(
                    "Contact saved",
                    accWasSaved ? "accountInfo=yes" : "accountInfo=no",
                    contactIdOverride: contactId);
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "ContactDetails TrySaveContact error: " + ex.Message);
                LogContactAudit("Contact save failed", ex.Message);
                errorMessage = "Error saving contact: " + ex.Message;
                return false;
            }
        }

        private void ClearDirtyState()
        {
            if (hdnContactDirty != null)
                hdnContactDirty.Value = "0";

            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                "contactDetailsClearDirty",
                "if (window.TrackerUnsaved) { TrackerUnsaved.clearDirty(); } else if (window.contactDetailsClearDirty) { contactDetailsClearDirty(); }",
                true);
        }

        private void MarkDirtyFromServer()
        {
            if (hdnContactDirty != null)
                hdnContactDirty.Value = "1";

            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                "contactDetailsMarkDirty",
                "if (window.TrackerUnsaved) { TrackerUnsaved.markDirty(); } else if (window.contactDetailsMarkDirty) { contactDetailsMarkDirty(); }",
                true);
        }

        // EVENT HANDLERS (legacy names preserved; button captions use Save / Save & Return / Back)
        protected void btnSuggestPostal_Click(object sender, EventArgs e)
        {
            string address = BillingAddressTextBox.Text;
            if (string.IsNullOrWhiteSpace(address))
            {
                SetStatus("Add a billing address first — suburb/place is used to suggest a postcode.", true);
                return;
            }

            int? areaId = ParseNullableInt(ddlAreas.SelectedValue);
            var contact = new Contact
            {
                BillingAddress = address,
                AreaID = areaId
            };

            string areaName = ddlAreas.SelectedItem != null ? ddlAreas.SelectedItem.Text : null;
            if (ContactPostalFillManager.IsCollectArea(areaName))
            {
                SetStatus("Collect areas are skipped for postcode suggestion.", true);
                return;
            }

            var fill = new ContactPostalFillManager();
            var suggestion = fill.SuggestOne(contact);
            if (suggestion == null || string.IsNullOrWhiteSpace(suggestion.SuggestedPostalCode))
            {
                SetStatus(suggestion != null ? suggestion.Reason : "No postcode match from this address.", true);
                return;
            }

            PostalCodeTextBox.Text = suggestion.SuggestedPostalCode;
            var resolved = new WooCommerceAreaMappingManager().ResolveArea(
                suggestion.SuggestedPostalCode, address);
            if (resolved != null && resolved.AreaID.HasValue && !resolved.IsAmbiguous)
                TrySelectDropDownByValue(ddlAreas, resolved.AreaID);

            MarkDirtyFromServer();
            SetStatus("Suggested " + suggestion.SuggestedPostalCode
                + (string.IsNullOrWhiteSpace(suggestion.Reason) ? "." : " — " + suggestion.Reason), false);
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!EnsureValidForSave())
            {
                RefreshAfterSave();
                return;
            }

            if (TrySaveContact(out string message))
                SetStatus(message ?? "Contact saved.", false);
            else if (!string.IsNullOrEmpty(message))
                SetStatus(message ?? "Save failed.", true);

            RefreshAfterSave();
        }

        protected void btnUpdateAndReturn_Click(object sender, EventArgs e)
        {
            if (!EnsureValidForSave())
            {
                RefreshAfterSave();
                return;
            }

            if (!TrySaveContact(out string message))
            {
                if (!string.IsNullOrEmpty(message))
                {
                    SetStatus(message ?? "Save failed.", true);
                    MarkDirtyFromServer();
                }
                RefreshAfterSave();
                return;
            }

            ReturnToCaller();
        }

        protected void btnInsert_Click(object sender, EventArgs e)
        {
            try
            {
                if (!EnsureValidForSave())
                {
                    RefreshAfterSave();
                    return;
                }

                if (string.IsNullOrWhiteSpace(CompanyNameTextBox.Text))
                {
                    SetStatus("Company Name is required.", true);
                    RefreshAfterSave();
                    return;
                }

                if (TryGetContactId(out _))
                {
                    SetStatus("This contact already exists — use Save instead of Insert.", true);
                    RefreshAfterSave();
                    return;
                }

                var contact = ReadContactFromForm(null);
                if (!TryConfirmDuplicatesThenInsert(contact, allowDuplicates: false))
                    return;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "ContactDetails Insert error: " + ex.Message);
                LogContactAudit("Contact create failed", ex.Message);
                SetStatus("Error inserting contact: " + ex.Message, true);
                RefreshAfterSave();
            }
        }

        protected void btnDuplicateAddAnyway_Click(object sender, EventArgs e)
        {
            try
            {
                var kind = GetDuplicatePromptKind();
                HideDuplicatePrompt();

                if (kind == DuplicatePromptKind.Rename)
                {
                    if (!EnsureValidForSave())
                    {
                        RefreshAfterSave();
                        return;
                    }

                    if (TrySaveContact(out string message, allowDuplicateName: true))
                        SetStatus(message ?? "Contact saved.", false);
                    else if (!string.IsNullOrEmpty(message))
                        SetStatus(message ?? "Save failed.", true);
                    RefreshAfterSave();
                    return;
                }

                if (TryGetContactId(out _))
                {
                    SetStatus("This contact already exists — use Save instead of Insert.", true);
                    RefreshAfterSave();
                    return;
                }

                if (!EnsureValidForSave())
                {
                    RefreshAfterSave();
                    return;
                }

                var contact = ReadContactFromForm(null);
                if (!TryConfirmDuplicatesThenInsert(contact, allowDuplicates: true))
                    return;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "ContactDetails duplicate add-anyway error: " + ex.Message);
                SetStatus("Error inserting contact: " + ex.Message, true);
                RefreshAfterSave();
            }
        }

        protected void btnDuplicateMerge_Click(object sender, EventArgs e)
        {
            try
            {
                int targetId = GetDuplicateMergeTargetId();
                HideDuplicatePrompt();

                if (targetId <= 0)
                {
                    SetStatus("No existing contact selected to merge into.", true);
                    RefreshAfterSave();
                    return;
                }

                var repo = new ContactsRepository();
                var existing = repo.GetById(targetId);
                if (existing == null)
                {
                    SetStatus("Existing contact not found for merge.", true);
                    RefreshAfterSave();
                    return;
                }

                var incoming = ReadContactFromForm(null);
                MergeContactFields(existing, incoming);

                if (!repo.Update(existing))
                {
                    SetStatus("Merge failed — could not update the existing contact.", true);
                    RefreshAfterSave();
                    return;
                }

                TrySaveAccInfo(targetId, existing, out _, out _);

                ClearDirtyState();
                LogContactAudit("Contact fields merged into existing", "targetId=" + targetId, contactIdOverride: targetId);
                LoadContact(targetId);
                SetButtonStatus(true);
                SetStatus("Merged form fields into existing contact (ID " + targetId + "). Blank fields were filled from what you entered.", false);
                RefreshAfterSave();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "ContactDetails duplicate merge error: " + ex.Message);
                SetStatus("Error merging contact: " + ex.Message, true);
                RefreshAfterSave();
            }
        }

        protected void btnDuplicateCancel_Click(object sender, EventArgs e)
        {
            HideDuplicatePrompt();
            SetStatus("Cancelled — no changes were made.", isError: null);
            RefreshAfterSave();
        }

        private enum DuplicatePromptKind
        {
            Insert,
            Rename
        }

        private const string ViewStateDupKind = "ContactDupPromptKind";
        private const string ViewStateDupMergeId = "ContactDupMergeTargetId";

        private DuplicatePromptKind GetDuplicatePromptKind()
        {
            object raw = ViewState[ViewStateDupKind];
            if (raw is DuplicatePromptKind kind)
                return kind;
            if (raw is int asInt && Enum.IsDefined(typeof(DuplicatePromptKind), asInt))
                return (DuplicatePromptKind)asInt;
            return DuplicatePromptKind.Insert;
        }

        private int GetDuplicateMergeTargetId()
        {
            object raw = ViewState[ViewStateDupMergeId];
            if (raw is int id)
                return id;
            if (raw != null && int.TryParse(raw.ToString(), out int parsed))
                return parsed;
            return 0;
        }

        private void HideDuplicatePrompt()
        {
            if (pnlDuplicatePrompt != null)
                pnlDuplicatePrompt.Visible = false;
            if (litDuplicatePrompt != null)
                litDuplicatePrompt.Text = string.Empty;
            ViewState[ViewStateDupKind] = null;
            ViewState[ViewStateDupMergeId] = null;
        }

        private void ShowDuplicatePrompt(string htmlMessage, DuplicatePromptKind kind, int mergeTargetId)
        {
            ViewState[ViewStateDupKind] = kind;
            ViewState[ViewStateDupMergeId] = mergeTargetId;

            if (litDuplicateTitle != null)
            {
                litDuplicateTitle.Text = kind == DuplicatePromptKind.Rename
                    ? "Duplicate company name"
                    : "Possible duplicate contact";
            }

            if (btnDuplicateAddAnyway != null)
            {
                btnDuplicateAddAnyway.Text = kind == DuplicatePromptKind.Rename
                    ? "Save with unique name"
                    : "Add as new (unique name)";
            }

            if (btnDuplicateMerge != null)
                btnDuplicateMerge.Visible = kind == DuplicatePromptKind.Insert && mergeTargetId > 0;

            if (pnlDuplicatePrompt != null)
                pnlDuplicatePrompt.Visible = true;
            if (litDuplicatePrompt != null)
                litDuplicatePrompt.Text = htmlMessage ?? string.Empty;
            SetStatus(string.Empty, false);
            RefreshAfterSave();
        }

        /// <summary>
        /// Blocks insert when name/email matches unless the user confirmed via Add anyway.
        /// On allowDuplicates, appends a number so dropdown names stay unique.
        /// </summary>
        private bool TryConfirmDuplicatesThenInsert(Contact contact, bool allowDuplicates)
        {
            var repo = new ContactsRepository();
            string company = (contact.CompanyName ?? string.Empty).Trim();
            var nameHits = repo.FindByCompanyNameExact(company);
            var emailHits = repo.FindByAnyEmailExact(contact.EmailAddress, contact.AltEmailAddress);

            if (!allowDuplicates)
            {
                if (emailHits.Count > 0 || nameHits.Count > 0)
                {
                    int mergeTargetId = ResolveMergeTargetId(nameHits, emailHits);
                    ShowDuplicatePrompt(
                        BuildDuplicatePromptHtml(company, nameHits, emailHits, forRename: false, suggestedUniqueName: null),
                        DuplicatePromptKind.Insert,
                        mergeTargetId);
                    return false;
                }
            }
            else
            {
                string unique = repo.EnsureUniqueCompanyName(company);
                if (!string.Equals(unique, company, StringComparison.OrdinalIgnoreCase))
                {
                    contact.CompanyName = unique;
                    CompanyNameTextBox.Text = unique;
                    ViewState["ContactUniqueNameApplied"] = unique;
                }
            }

            return CompleteContactInsert(contact, repo);
        }

        private static int ResolveMergeTargetId(List<Contact> nameHits, List<Contact> emailHits)
        {
            Contact pick = PreferMergeCandidate(emailHits) ?? PreferMergeCandidate(nameHits);
            return pick != null ? pick.ContactID : 0;
        }

        private static Contact PreferMergeCandidate(List<Contact> hits)
        {
            if (hits == null || hits.Count == 0)
                return null;

            Contact enabled = null;
            foreach (Contact c in hits)
            {
                if (c == null || c.ContactID <= 0)
                    continue;
                if (c.Enabled != false)
                    return c;
                if (enabled == null)
                    enabled = c;
            }
            return enabled;
        }

        private static string BuildDuplicatePromptHtml(
            string company,
            List<Contact> nameHits,
            List<Contact> emailHits,
            bool forRename,
            string suggestedUniqueName)
        {
            var sb = new System.Text.StringBuilder();

            if (nameHits != null && nameHits.Count > 0)
            {
                sb.Append("<p>A contact with the name <strong>")
                    .Append(System.Web.HttpUtility.HtmlEncode(company))
                    .Append("</strong> already exists:</p><ul>");
                AppendContactListItems(sb, nameHits, includeEmails: false);
                sb.Append("</ul>");
            }

            if (!forRename && emailHits != null && emailHits.Count > 0)
            {
                sb.Append("<p>")
                    .Append(nameHits != null && nameHits.Count > 0
                        ? "Also, this email address is already used by:"
                        : "This email address is already used by:")
                    .Append("</p><ul>");
                AppendContactListItems(sb, emailHits, includeEmails: true);
                sb.Append("</ul>");
            }

            if (forRename)
            {
                sb.Append("<p>Company names must be unique in dropdown lists. Save as <strong>")
                    .Append(System.Web.HttpUtility.HtmlEncode(suggestedUniqueName ?? (company + " 2")))
                    .Append("</strong>, or cancel.</p>");
            }
            else
            {
                sb.Append("<p><strong>Add as new</strong> creates a separate contact with a unique name (a number is added if needed). ");
                sb.Append("<strong>Merge into existing</strong> fills blank fields on the matched contact from what you entered. ");
                sb.Append("<strong>Cancel</strong> leaves everything unchanged.</p>");
            }

            return sb.ToString();
        }

        private static void AppendContactListItems(System.Text.StringBuilder sb, List<Contact> hits, bool includeEmails)
        {
            int shown = 0;
            foreach (Contact c in hits)
            {
                if (shown >= 8)
                    break;
                sb.Append("<li>")
                    .Append(System.Web.HttpUtility.HtmlEncode(FormatDuplicateContactLabel(c)));
                if (includeEmails)
                {
                    sb.Append(" — ")
                        .Append(System.Web.HttpUtility.HtmlEncode(DescribeContactEmails(c)));
                }
                sb.Append("</li>");
                shown++;
            }
            if (hits.Count > 8)
                sb.Append("<li>…and ").Append(hits.Count - 8).Append(" more</li>");
        }

        private static string FormatDuplicateContactLabel(Contact c)
        {
            if (c == null)
                return "(unknown)";
            string name = string.IsNullOrWhiteSpace(c.CompanyName) ? "(no name)" : c.CompanyName.Trim();
            string enabled = c.Enabled == false ? " [disabled]" : string.Empty;
            return name + " (ID " + c.ContactID + ")" + enabled;
        }

        private static string DescribeContactEmails(Contact c)
        {
            if (c == null)
                return string.Empty;
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(c.EmailAddress))
                parts.Add(c.EmailAddress.Trim());
            if (!string.IsNullOrWhiteSpace(c.AltEmailAddress))
                parts.Add(c.AltEmailAddress.Trim());
            return parts.Count == 0 ? "(no email)" : string.Join(" / ", parts);
        }

        /// <summary>
        /// Fills blank/null fields on <paramref name="target"/> from <paramref name="source"/>.
        /// Does not overwrite existing values; does not change ContactID / reminder counters.
        /// </summary>
        private static void MergeContactFields(Contact target, Contact source)
        {
            if (target == null || source == null)
                return;

            if (string.IsNullOrWhiteSpace(target.ContactTitle)) target.ContactTitle = source.ContactTitle;
            if (string.IsNullOrWhiteSpace(target.ContactFirstName)) target.ContactFirstName = source.ContactFirstName;
            if (string.IsNullOrWhiteSpace(target.ContactLastName)) target.ContactLastName = source.ContactLastName;
            if (string.IsNullOrWhiteSpace(target.ContactAltFirstName)) target.ContactAltFirstName = source.ContactAltFirstName;
            if (string.IsNullOrWhiteSpace(target.ContactAltLastName)) target.ContactAltLastName = source.ContactAltLastName;
            if (string.IsNullOrWhiteSpace(target.Department)) target.Department = source.Department;
            if (string.IsNullOrWhiteSpace(target.BillingAddress)) target.BillingAddress = source.BillingAddress;
            if (!target.AreaID.HasValue || target.AreaID.Value <= 0) target.AreaID = source.AreaID;
            if (string.IsNullOrWhiteSpace(target.StateOrProvince)) target.StateOrProvince = source.StateOrProvince;
            if (string.IsNullOrWhiteSpace(target.PostalCode)) target.PostalCode = source.PostalCode;
            if (string.IsNullOrWhiteSpace(target.CountryOrRegion)) target.CountryOrRegion = source.CountryOrRegion;
            if (string.IsNullOrWhiteSpace(target.PhoneNumber)) target.PhoneNumber = source.PhoneNumber;
            if (string.IsNullOrWhiteSpace(target.CellNumber)) target.CellNumber = source.CellNumber;
            if (string.IsNullOrWhiteSpace(target.FaxNumber)) target.FaxNumber = source.FaxNumber;
            if (string.IsNullOrWhiteSpace(target.EmailAddress)) target.EmailAddress = source.EmailAddress;
            if (string.IsNullOrWhiteSpace(target.AltEmailAddress)) target.AltEmailAddress = source.AltEmailAddress;
            if (!target.ContactTypeID.HasValue || target.ContactTypeID.Value <= 0) target.ContactTypeID = source.ContactTypeID;
            if (!target.EquipTypeID.HasValue || target.EquipTypeID.Value <= 0) target.EquipTypeID = source.EquipTypeID;
            if (!target.ItemPrefID.HasValue || target.ItemPrefID.Value <= 0) target.ItemPrefID = source.ItemPrefID;
            if (!target.PriPrefQty.HasValue) target.PriPrefQty = source.PriPrefQty;
            if (!target.PrefItemPackagingID.HasValue || target.PrefItemPackagingID.Value <= 0)
                target.PrefItemPackagingID = source.PrefItemPackagingID;
            if (!target.PreferredAgentID.HasValue || target.PreferredAgentID.Value <= 0)
                target.PreferredAgentID = source.PreferredAgentID;
            if (!target.PreferredCourierServiceID.HasValue || target.PreferredCourierServiceID.Value <= 0)
                target.PreferredCourierServiceID = source.PreferredCourierServiceID;
            if (!target.SalesAgentID.HasValue || target.SalesAgentID.Value <= 0)
                target.SalesAgentID = source.SalesAgentID;
            if (string.IsNullOrWhiteSpace(target.EquipentSN)) target.EquipentSN = source.EquipentSN;
            if (string.IsNullOrWhiteSpace(target.Notes) && !string.IsNullOrWhiteSpace(source.Notes))
                target.Notes = source.Notes;
            else if (!string.IsNullOrWhiteSpace(source.Notes)
                && !string.IsNullOrWhiteSpace(target.Notes)
                && target.Notes.IndexOf(source.Notes, StringComparison.OrdinalIgnoreCase) < 0)
            {
                target.Notes = (target.Notes ?? string.Empty).TrimEnd()
                    + "\nMerged from new contact form: " + source.Notes.Trim();
            }
        }

        private bool CompleteContactInsert(Contact contact, ContactsRepository repo)
        {
            HideDuplicatePrompt();

            int newId = repo.Insert(contact);
            if (newId <= 0)
            {
                LogContactAudit("Contact create failed", "Insert returned 0");
                SetStatus("Insert failed — contact was not created.", true);
                RefreshAfterSave();
                return false;
            }

            contact.ContactID = newId;
            CompanyIDLabel.Text = newId.ToString();
            SetButtonStatus(true);

            if (!TrySaveAccInfo(newId, contact, out bool accWasSaved, out string accError))
            {
                LogContactAudit(
                    "Contact created (account info failed)",
                    accError,
                    contactIdOverride: newId);
                SetStatus("Contact created (ID " + newId + "), but account info failed: " + accError, true);
                RefreshAfterSave();
                return false;
            }

            ClearDirtyState();
            LogContactAudit(
                "Contact created",
                accWasSaved ? "accountInfo=yes" : "accountInfo=no",
                contactIdOverride: newId);

            string welcomeNote = string.Empty;
            try
            {
                bool sent = new CustomerManager().TrySendTrackingWelcomeEmail(contact);
                welcomeNote = sent
                    ? " Welcome email with preference / disable link sent."
                    : " Welcome email not sent (no email address, or send failed — check email log).";
            }
            catch (Exception welcomeEx)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email,
                    "ContactDetails tracking welcome: " + welcomeEx.Message);
                welcomeNote = " Welcome email failed — check email log.";
            }

            string nameNote = string.Empty;
            object uniqueApplied = ViewState["ContactUniqueNameApplied"];
            ViewState["ContactUniqueNameApplied"] = null;
            if (uniqueApplied != null && !string.IsNullOrWhiteSpace(uniqueApplied.ToString()))
                nameNote = " Saved as \"" + uniqueApplied + "\".";

            SetStatus((accWasSaved
                ? "Contact and account info created (ID " + newId + ")."
                : "Contact created (ID " + newId + ").")
                + nameNote
                + welcomeNote, false);
            RefreshAfterSave();
            return true;
        }

        protected void btnCopy2AccInfo_Click(object sender, EventArgs e)
        {
            Contact contact = ReadContactFromForm(null);
            if (int.TryParse(CompanyIDLabel.Text, out int contactId) && contactId > 0)
                contact.ContactID = contactId;

            var acc = CopyContactDataToAccInfo(contact, contact.ContactID);
            if (int.TryParse(ContactsAccInfoIDLabel.Text, out int accId) && accId > 0)
                acc.ContactsAccInfoID = accId;

            PlaceAccInfoOnForm(acc);

            MarkDirtyFromServer();
            SetStatus("Contact fields copied to Accounts Info — click Save to keep them.", null);
            RefreshAfterSave();
        }
        protected void btnAddLasOrder_Click(object sender, EventArgs e)
        {
            if (!TryGetContactId(out int contactId))
            {
                SetStatus("Open a saved contact before adding their last order.", true);
                upnlContactDetails.Update();
                new showMessageBox(Page, "Add Last", "Open a saved contact before adding their last order.");
                return;
            }

            // Same contract as Contacts.aspx / CustomerDetails: new draft + copy last order lines.
            string url = string.Format(
                "~/Pages/OrderDetail.aspx?NewOrder=true&{0}={1}&{2}=Y",
                SystemConstants.UrlParameterConstants.CustomerID,
                contactId,
                SystemConstants.UrlParameterConstants.LastOrder);
            Response.Redirect(url, true);
        }
        protected void btnForceNext_Click(object sender, EventArgs e)
        {
            try
            {
                if (!TryGetContactId(out int contactId))
                {
                    NotifyForceAction("Force Next", "No contact selected.", true);
                    return;
                }

                // Match legacy CustomerDetails: push NextCoffeeBy ~2 weeks + prep alignment + 3 days.
                DateTime nextDate = new TrackerTools()
                    .GetClosestNextPreparationDate(TimeZoneUtils.Now().AddDays(14.0).Date)
                    .AddDays(3.0);

                var usageRepo = new ContactsUsageRepository();
                if (usageRepo.GetByContactId(contactId) == null)
                {
                    NotifyForceAction(
                        "Force Next",
                        "No prediction record for this contact — cannot set Next Coffee date.",
                        true);
                    return;
                }

                if (!usageRepo.ForceNextCoffeeDate(contactId, nextDate))
                {
                    NotifyForceAction(
                        "Force Next",
                        "Failed to update Next Coffee date for contact " + contactId + ".",
                        true);
                    return;
                }

                new ContactsRepository().IncrementReminderCount(contactId);
                RefreshPredictionAfterForce(contactId);

                string name = string.IsNullOrWhiteSpace(CompanyNameTextBox.Text)
                    ? ("Contact " + contactId)
                    : CompanyNameTextBox.Text.Trim();
                string msg = name + " forced to skip a week of prediction. Next coffee set to "
                    + nextDate.ToString("d") + ".";

                LogContactAudit("Force Next", $"nextCoffee={nextDate:yyyy-MM-dd}", contactIdOverride: contactId);

                NotifyForceAction("Force Next", msg, false);
            }
            catch (Exception ex)
            {
                LogContactAudit("Force Next failed", ex.Message);
                NotifyForceAction("Force Next", "Error: " + ex.Message, true);
            }
        }

        protected void btnForceCheckup_Click(object sender, EventArgs e)
        {
            try
            {
                if (!TryGetContactId(out int contactId))
                {
                    NotifyForceAction("Force Checkup", "No contact selected.", true);
                    return;
                }

                DateTime forceDate = TimeZoneUtils.Now().Date.AddDays(5);
                var usageRepo = new ContactsUsageRepository();
                if (usageRepo.GetByContactId(contactId) == null)
                {
                    NotifyForceAction(
                        "Force Checkup",
                        "No prediction record for this contact — cannot force checkup.",
                        true);
                    return;
                }

                if (!usageRepo.ForceNextCoffeeDate(contactId, forceDate))
                {
                    NotifyForceAction(
                        "Force Checkup",
                        "Failed to force checkup for contact " + contactId + ".",
                        true);
                    return;
                }

                new ContactsRepository().ResetReminderCount(contactId, forceEnable: false);
                RefreshPredictionAfterForce(contactId);

                string msg = "Contact " + contactId
                    + " forced into next checkup cycle. Next coffee date set to "
                    + forceDate.ToString("d") + ".";

                LogContactAudit("Force Checkup", $"nextCoffee={forceDate:yyyy-MM-dd}", contactIdOverride: contactId);

                NotifyForceAction("Force Checkup", msg, false);
            }
            catch (Exception ex)
            {
                LogContactAudit("Force Checkup failed", ex.Message);
                NotifyForceAction("Force Checkup", "Error forcing checkup: " + ex.Message, true);
            }
        }

        protected void btnSendReminder_Click(object sender, EventArgs e)
        {
            try
            {
                if (!TryGetContactId(out int contactId))
                {
                    NotifyForceAction("Send Reminder", "No contact selected.", true);
                    return;
                }

                string error = new CoffeeCheckupManager().SendManualReminder(contactId);
                if (!string.IsNullOrEmpty(error))
                {
                    LogContactAudit("Send Reminder failed", error, contactIdOverride: contactId);
                    NotifyForceAction("Send Reminder", error, true);
                    return;
                }

                RefreshPredictionAfterForce(contactId);
                try
                {
                    var refreshed = new ContactsRepository().GetById(contactId);
                    if (LastReminderLabel != null)
                    {
                        LastReminderLabel.Text = refreshed?.LastDateSentReminder.HasValue == true
                            ? refreshed.LastDateSentReminder.Value.ToString("yyyy-MM-dd")
                            : TimeZoneUtils.Now().ToString("yyyy-MM-dd");
                    }
                }
                catch { /* labels already best-effort */ }

                string name = string.IsNullOrWhiteSpace(CompanyNameTextBox.Text)
                    ? ("Contact " + contactId)
                    : CompanyNameTextBox.Text.Trim();
                LogContactAudit("Send Reminder", "manual checkup email sent", contactIdOverride: contactId);
                NotifyForceAction("Send Reminder", "Reminder email sent to " + name + ".", false);
            }
            catch (Exception ex)
            {
                LogContactAudit("Send Reminder failed", ex.Message);
                NotifyForceAction("Send Reminder", "Error sending reminder: " + ex.Message, true);
            }
        }

        private void RefreshPredictionAfterForce(int contactId)
        {
            try
            {
                if (ReminderCountLabel != null)
                    ReminderCountLabel.Text = new ContactsRepository().GetReminderCount(contactId).ToString();
            }
            catch { }

            BindUsageLines(contactId);

            if (upnlNextItems != null)
                upnlNextItems.Update();
            if (uppnlTabContainer != null)
                uppnlTabContainer.Update();
        }

        private void NotifyForceAction(string title, string message, bool isError)
        {
            SetStatus(message, isError);
            upnlContactDetails.Update();
            // Alert so the user always sees the outcome (status bar alone was easy to miss).
            new showMessageBox(Page, title, message);
        }

        protected void btnRecalcAverage_Click(object sender, EventArgs e)
        {
            try
            {
                if (!TryGetContactId(out int contactId))
                {
                    NotifyForceAction("Recalc Ave", "No contact selected.", true);
                    return;
                }

                var usage = new ContactsUsageRepository().GetByContactId(contactId);
                if (usage == null)
                {
                    NotifyForceAction(
                        "Recalc Ave",
                        "No prediction record for this contact — cannot recalculate.",
                        true);
                    return;
                }

                bool ok = new PredictionManager().CalcAndSaveNextRequiredDates(contactId);
                if (!ok)
                {
                    NotifyForceAction(
                        "Recalc Ave",
                        "Failed to recalculate averages / next dates for contact " + contactId + ".",
                        true);
                    return;
                }

                RefreshPredictionAfterForce(contactId);

                usage = new ContactsUsageRepository().GetByContactId(contactId);
                string name = string.IsNullOrWhiteSpace(CompanyNameTextBox.Text)
                    ? ("Contact " + contactId)
                    : CompanyNameTextBox.Text.Trim();
                string next = usage?.NextCoffeeBy.HasValue == true
                    ? usage.NextCoffeeBy.Value.ToString("d")
                    : "(none)";
                string daily = usage?.DailyConsumption.HasValue == true
                    ? usage.DailyConsumption.Value.ToString("0.####")
                    : "(n/a)";
                string msg = name + " averages recalculated. Daily use=" + daily
                    + "; next coffee=" + next + ".";

                LogContactAudit(
                    "Recalc Ave",
                    $"daily={daily}; nextCoffee={usage?.NextCoffeeBy:yyyy-MM-dd}",
                    contactIdOverride: contactId);
                NotifyForceAction("Recalc Ave", msg, false);
            }
            catch (Exception ex)
            {
                LogContactAudit("Recalc Ave failed", ex.Message);
                NotifyForceAction("Recalc Ave", "Error: " + ex.Message, true);
            }
        }
        protected void btnCancel_Click(object sender, ImageClickEventArgs e)
        {
            ReturnToCaller();
        }
        private Contact ReadContactForAccMerge(int contactId)
        {
            var repo = new ContactsRepository();
            var existing = repo.GetById(contactId);
            var contact = existing != null ? ReadContactFromForm(existing) : ReadContactFromForm(null);
            contact.ContactID = contactId;
            return contact;
        }

        protected void accAddDetailsButton_Click(object sender, EventArgs e)
        {
            if (!TryGetContactId(out int contactId))
            {
                SetStatus("Save the contact first before adding account details.", true);
                RefreshAfterSave();
                return;
            }

            if (!TrySaveAccInfo(contactId, ReadContactForAccMerge(contactId), out bool accWasSaved, out string error))
            {
                LogContactAudit("Account info save failed", error, contactIdOverride: contactId);
                SetStatus(error ?? "Account info save failed.", true);
                RefreshAfterSave();
                return;
            }

            ClearDirtyState();
            if (accWasSaved)
                LogContactAudit("Account info saved", "via=add", contactIdOverride: contactId);
            SetStatus(accWasSaved ? "Account details saved." : "No account data to save.", accWasSaved ? (bool?)false : null);
            RefreshAfterSave();
        }

        protected void accUpdateButton_Click(object sender, EventArgs e)
        {
            if (!TryGetContactId(out int contactId))
            {
                SetStatus("No contact loaded.", true);
                RefreshAfterSave();
                return;
            }

            if (!TrySaveAccInfo(contactId, ReadContactForAccMerge(contactId), out bool accWasSaved, out string error))
            {
                LogContactAudit("Account info save failed", error, contactIdOverride: contactId);
                SetStatus(error ?? "Account info save failed.", true);
                RefreshAfterSave();
                return;
            }

            ClearDirtyState();
            if (accWasSaved)
                LogContactAudit("Account info saved", "via=update", contactIdOverride: contactId);
            SetStatus(accWasSaved ? "Account details saved." : "No account data to save.", accWasSaved ? (bool?)false : null);
            RefreshAfterSave();
        }

        protected void gvContactItems_RowEditing(object sender, GridViewEditEventArgs e)
        {
            int contactId = GetLoadedContactId();
            if (contactId <= 0) return;

            try
            {
                gvContactItems.EditIndex = e.NewEditIndex;
                BindItemUsageGrid(contactId);
                upnlItems.Update();
                uppnlTabContainer.Update();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "gvContactItems_RowEditing error: " + ex.Message);
                gvContactItems.EditIndex = -1;
                BindItemUsageGrid(contactId);
                SetStatus("Could not open item usage for edit: " + ex.Message, true);
                upnlItems.Update();
                upnlContactDetails.Update();
            }
        }

        protected void gvContactItems_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;
            if ((e.Row.RowState & DataControlRowState.Edit) == 0)
                return;

            var data = e.Row.DataItem as ContactItemUsageRow;
            if (data == null)
                return;

            SafeSelectDropDown((DropDownList)e.Row.FindControl("ddlItemsUsage"), data.ItemProvidedID);
            SafeSelectDropDown((DropDownList)e.Row.FindControl("ddlPrepTypeUsage"), data.ItemPrepTypeID);
            SafeSelectDropDown((DropDownList)e.Row.FindControl("ddlPackagingUsage"), data.ItemPackagingID);
        }

        private static void SafeSelectDropDown(DropDownList ddl, int? value)
        {
            if (ddl == null)
                return;

            string key = (value.HasValue && value.Value > 0) ? value.Value.ToString() : "0";
            var item = ddl.Items.FindByValue(key);
            if (item == null)
            {
                // Missing lookup value — keep n/a rather than throw
                item = ddl.Items.FindByValue("0");
            }

            if (item == null)
                return;

            ddl.ClearSelection();
            item.Selected = true;
        }

        /// <summary>
        /// Nested UpdatePanels / TabContainer sometimes suppress GridView Edit; RowCommand is a reliable fallback.
        /// </summary>
        protected void gvContactItems_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "Edit" && e.CommandName != "Cancel"
                && e.CommandName != "Update" && e.CommandName != "Delete")
                return;

            // Let the dedicated RowEditing/Updating/Deleting handlers own those commands once EditIndex is set.
            // This method only catches Edit when the dedicated event was skipped (rare).
            if (e.CommandName != "Edit")
                return;

            if (!(e.CommandSource is ImageButton btn))
                return;

            var row = btn.NamingContainer as GridViewRow;
            if (row == null)
                return;

            int contactId = GetLoadedContactId();
            if (contactId <= 0)
                return;

            try
            {
                gvContactItems.EditIndex = row.RowIndex;
                BindItemUsageGrid(contactId);
                upnlItems.Update();
                uppnlTabContainer.Update();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "gvContactItems_RowCommand Edit error: " + ex.Message);
                gvContactItems.EditIndex = -1;
                BindItemUsageGrid(contactId);
                SetStatus("Could not open item usage for edit: " + ex.Message, true);
                upnlItems.Update();
                upnlContactDetails.Update();
            }
        }

        protected void gvContactItems_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            int contactId = GetLoadedContactId();
            gvContactItems.EditIndex = -1;
            if (contactId > 0)
                BindItemUsageGrid(contactId);
            upnlItems.Update();
        }

        protected void gvContactItems_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            int contactId = GetLoadedContactId();
            gvContactItems.PageIndex = e.NewPageIndex;
            gvContactItems.EditIndex = -1;
            if (contactId > 0)
                BindItemUsageGrid(contactId);
            upnlItems.Update();
        }

        protected void gvContactItems_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            int contactId = GetLoadedContactId();
            if (contactId <= 0)
            {
                SetStatus("No contact loaded.", true);
                upnlContactDetails.Update();
                return;
            }

            try
            {
                GridViewRow row = gvContactItems.Rows[e.RowIndex];
                int lineNo = Convert.ToInt32(gvContactItems.DataKeys[e.RowIndex].Value);

                var tbxItemDate = (TextBox)row.FindControl("tbxItemDate");
                var ddlItemsUsage = (DropDownList)row.FindControl("ddlItemsUsage");
                var tbxAmountProvided = (TextBox)row.FindControl("tbxAmountProvided");
                var ddlPrepTypeUsage = (DropDownList)row.FindControl("ddlPrepTypeUsage");
                var ddlPackagingUsage = (DropDownList)row.FindControl("ddlPackagingUsage");
                var tbxNotes = (TextBox)row.FindControl("tbxNotes");

                if (!DateTime.TryParse(tbxItemDate?.Text, out DateTime deliveryDate))
                {
                    SetStatus("Invalid item date.", true);
                    upnlContactDetails.Update();
                    return;
                }

                var usage = new ContactsItemUsage
                {
                    ContactItemUsageLineNo = lineNo,
                    ContactID = contactId,
                    DeliveryDate = deliveryDate.Date,
                    ItemProvidedID = ParseNullableInt(ddlItemsUsage?.SelectedValue),
                    QtyProvided = ParseNullableDouble(tbxAmountProvided?.Text),
                    ItemPrepTypeID = ParseNullableInt(ddlPrepTypeUsage?.SelectedValue),
                    ItemPackagingID = ParseNullableInt(ddlPackagingUsage?.SelectedValue),
                    Notes = NullIfEmpty(tbxNotes?.Text)
                };

                new ContactsItemUsageRepository().Update(usage);
                LogContactAudit(
                    "Item usage updated",
                    $"line={lineNo}; itemId={usage.ItemProvidedID}; qty={usage.QtyProvided}",
                    contactIdOverride: contactId);

                gvContactItems.EditIndex = -1;
                BindItemUsageGrid(contactId);
                SetStatus("Item usage line updated.", false);
                upnlItems.Update();
                upnlContactDetails.Update();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "gvContactItems_RowUpdating error: " + ex.Message);
                LogContactAudit("Item usage update failed", ex.Message);
                SetStatus("Error updating item usage: " + ex.Message, true);
                upnlContactDetails.Update();
            }
        }

        protected void gvContactItems_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int contactId = GetLoadedContactId();
            if (contactId <= 0)
            {
                SetStatus("No contact loaded.", true);
                upnlContactDetails.Update();
                return;
            }

            try
            {
                int lineNo = Convert.ToInt32(gvContactItems.DataKeys[e.RowIndex].Value);
                new ContactsItemUsageRepository().DeleteUsageLine(lineNo, contactId);
                LogContactAudit("Item usage deleted", $"line={lineNo}", contactIdOverride: contactId);

                gvContactItems.EditIndex = -1;
                BindItemUsageGrid(contactId);
                SetStatus("Item usage line deleted.", false);
                upnlItems.Update();
                upnlContactDetails.Update();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "gvContactItems_RowDeleting error: " + ex.Message);
                LogContactAudit("Item usage delete failed", ex.Message);
                SetStatus("Error deleting item usage: " + ex.Message, true);
                upnlContactDetails.Update();
            }
        }

        protected void accPaymentTermsDropDownList_DataBound(object sender, EventArgs e)
        {
            if (accPaymentTermsDropDownList.SelectedIndex == 0 && accPaymentTermsDropDownList.Items.Count > 1)
                accPaymentTermsDropDownList.SelectedIndex = 1; // default selection
        }
    }
}
