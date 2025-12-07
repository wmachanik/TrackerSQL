using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerDotNet.Classes.Poco;
using TrackerDotNet.Classes.Sql;
using TrackerSQL.Classes; // for TimeZoneUtils, AppLogger

namespace TrackerSQL.Pages
{
    public partial class ContactDetails : Page
    {
        protected global::System.Web.UI.WebControls.GridView gvPrediction;
        protected global::System.Web.UI.WebControls.GridView gvContactItems;

        private const string CONST_URL_REQUEST_ID = "ID";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
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
                }
            }
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
                accInvoiceTypesDropDownList.DataBind();
                accPaymentTermsDropDownList.DataBind();
                accPriceLevelsDropDownList.DataBind();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "ContactDetails DataBindLookups error: " + ex.Message);
            }
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
                    ltrlStatus.Text = "Contact not found.";
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

                TrySelectDropDownByValue(ddlAreas, contact.Area);
                TrySelectDropDownByValue(ddlContactTypes, contact.ContactTypeID);
                TrySelectDropDownByValue(ddlEquipTypes, contact.EquipTypeID);
                TrySelectDropDownByValue(ddlFirstPreference, contact.ItemPrefID);
                TrySelectDropDownByValue(ddlItemPackagingTypes, contact.PrefItemPackagingID);

                TrySelectDropDownByValue(ddlDeliveryBy, contact.PreferedAgentID);
                TrySelectDropDownByValue(ddlAgent, contact.SalesAgentID);

                PriPrefQtyTextBox.Text = contact.PriPrefQty.HasValue ? contact.PriPrefQty.Value.ToString("0.##") : string.Empty;
                MachineSNTextBox.Text = contact.MachineSN;

                // Accounts section defaults - if you have ContactsAccInfo, use repository to load it.
                var accRepo = new ContactsAccInfoRepository();
                var acc = accRepo.GetByCustomerId(id);
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
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "Error loading contact: " + ex.Message);
                ltrlStatus.Text = "Error loading contact.";
            }
        }

        private void BindUsageLines(int contactId)
        {
            try
            {
                var usageRepo = new TrackerDotNet.Classes.Sql.ContactsUsageRepository();
                var usage = usageRepo.GetByContactId(contactId);
                gvPrediction.DataSource = usage != null ? new[] { usage } : new TrackerDotNet.Classes.Sql.ContactsUsage[] { };
                gvPrediction.DataBind();

                var itemsRepo = new TrackerDotNet.Classes.Sql.ContactsItemUsageRepository();
                var items = itemsRepo.GetByContactId(contactId, "DeliveryDate DESC");

                var itemLookup = new ItemsRepository().GetAll("ItemDesc");
                var prepLookup = new ItemPrepTypesRepository().GetAll("ItemPrepTypeName");
                var packLookup = new ItemPackagingsRepository().GetAll("ItemPackagingDesc");

                var uiItems = items.ConvertAll(x => new {
                    ClientUsageLineNo = x.ContactUsageLineNo,
                    ItemDate = x.DeliveryDate,
                    ItemProvided = x.ItemProvidedID.HasValue ? (itemLookup.Find(i => i.ItemID == x.ItemProvidedID.Value)?.ItemDesc ?? x.ItemProvidedID.Value.ToString()) : string.Empty,
                    AmountProvided = x.QtyProvided,
                    PrepType = x.ItemPrepTypeID.HasValue ? (prepLookup.Find(p => p.ItemPrepID == x.ItemPrepTypeID.Value)?.ItemPrepTypeName ?? x.ItemPrepTypeID.Value.ToString()) : string.Empty,
                    Packaging = x.ItemPackagingID.HasValue ? (packLookup.Find(p => p.ItemPackagingID == x.ItemPackagingID.Value)?.ItemPackagingDesc ?? x.ItemPackagingID.Value.ToString()) : string.Empty,
                    Notes = x.Notes
                });
                gvContactItems.DataSource = uiItems;
                gvContactItems.DataBind();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "BindUsageLines error: " + ex.Message);
            }
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
            btnRecalcAverage.Enabled = editMode;
            btnInsert.Enabled = !editMode;
            accAddDetailsButton.Enabled = !editMode;
            accUpdateButton.Enabled = editMode;
        }

        // EVENT HANDLERS (legacy names preserved)
        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            // TODO: implement update via ContactsRepository Update(contact)
            ltrlStatus.Text = "Update (SQL) not yet implemented.";
        }
        protected void btnUpdateAndReturn_Click(object sender, EventArgs e)
        {
            btnUpdate_Click(sender, e);
            btnCancel_Click(sender, e);
        }
        protected void btnInsert_Click(object sender, EventArgs e)
        {
            // TODO: implement insert
            ltrlStatus.Text = "Insert (SQL) not yet implemented.";
        }
        protected void btnCopy2AccInfo_Click(object sender, EventArgs e)
        {
            // TODO: copy basic contact info to account info controls
        }
        protected void btnAddLasOrder_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Pages/OrderDetail.aspx?NewOrder=true&CoID=" + CompanyIDLabel.Text);
        }
        protected void btnForceNext_Click(object sender, EventArgs e)
        {
            // TODO: implement force next logic using SQL equivalent
            ltrlStatus.Text = "Force Next (SQL) pending migration.";
        }
        protected void btnForceCheckup_Click(object sender, EventArgs e)
        {
            ltrlStatus.Text = "Force Checkup (SQL) pending migration.";
        }
        protected void btnRecalcAverage_Click(object sender, EventArgs e)
        {
            ltrlStatus.Text = "Recalc average (SQL) pending migration.";
        }
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Pages/Contacts.aspx");
        }
        protected void accAddDetailsButton_Click(object sender, EventArgs e)
        {
            ltrlStatus.Text = "Add Account (SQL) pending migration.";
        }
        protected void accUpdateButton_Click(object sender, EventArgs e)
        {
            ltrlStatus.Text = "Update Account (SQL) pending migration.";
        }
        protected void tabcContact_OnActiveTabChanged(object sender, EventArgs e)
        {
            // could store active tab index if needed
        }
        protected void gvItems_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // TODO: implement items grid commands
        }
        protected void accPaymentTermsDropDownList_DataBound(object sender, EventArgs e)
        {
            if (accPaymentTermsDropDownList.SelectedIndex == 0 && accPaymentTermsDropDownList.Items.Count > 1)
                accPaymentTermsDropDownList.SelectedIndex = 1; // default selection
        }
    }
}
