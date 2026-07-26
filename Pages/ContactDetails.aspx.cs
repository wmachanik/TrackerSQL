using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Models;
using TrackerSQL.Repositories;
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
                }
            }
        }

        /// <summary>
        /// Remembers the page that opened Contact Details (referrer or ?ReturnUrl=).
        /// Default fallback is Contacts list. Same pattern as OrderDetail.
        /// </summary>
        private void CaptureReturnUrlIfNeeded()
        {
            string qsReturn = Request.QueryString["ReturnUrl"];
            if (!string.IsNullOrWhiteSpace(qsReturn) && TryNormalizeLocalReturnUrl(qsReturn, out string fromQuery))
            {
                Session[SESSION_RETURN_URL] = fromQuery;
                return;
            }

            if (Request.UrlReferrer != null)
            {
                string referrer = Request.UrlReferrer.ToString();
                if (referrer.IndexOf("ContactDetails.aspx", StringComparison.OrdinalIgnoreCase) < 0
                    && IsSafeReturnUrl(referrer))
                {
                    Session[SESSION_RETURN_URL] = referrer;
                    return;
                }
            }

            if (Session[SESSION_RETURN_URL] == null)
                Session[SESSION_RETURN_URL] = ResolveUrl(DefaultReturnUrl);
        }

        private string GetReturnUrl()
        {
            string url = Session[SESSION_RETURN_URL] as string;
            if (string.IsNullOrWhiteSpace(url) || !IsSafeReturnUrl(url))
                url = ResolveUrl(DefaultReturnUrl);
            return url;
        }

        private void ReturnToCaller()
        {
            Response.Redirect(GetReturnUrl(), false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private bool TryNormalizeLocalReturnUrl(string candidate, out string normalized)
        {
            normalized = null;
            if (string.IsNullOrWhiteSpace(candidate))
                return false;

            candidate = candidate.Trim();
            if (candidate.StartsWith("~/") || (candidate.StartsWith("/") && !candidate.StartsWith("//")))
            {
                normalized = ResolveUrl(candidate.StartsWith("~/") ? candidate : "~" + candidate);
                return IsSafeReturnUrl(normalized);
            }

            if (IsSafeReturnUrl(candidate))
            {
                normalized = candidate;
                return true;
            }

            return false;
        }

        private bool IsSafeReturnUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            // Relative app paths
            if (url.StartsWith("~/") || (url.StartsWith("/") && !url.StartsWith("//")))
                return url.IndexOf("://", StringComparison.Ordinal) < 0;

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri absolute))
                return false;

            // Same host only (block open redirects)
            return Request.Url != null
                && string.Equals(absolute.Host, Request.Url.Host, StringComparison.OrdinalIgnoreCase);
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

        private void SetStatus(string message, bool? isError)
        {
            ltrlStatus.Text = HttpUtility.HtmlEncode(message ?? string.Empty);

            if (pnlStatus == null)
                return;

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
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "Error loading contact: " + ex.Message);
                SetStatus("Error loading contact.", true);
            }
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
            btnRecalcAverage.Enabled = editMode;
            btnInsert.Enabled = !editMode;
            accAddDetailsButton.Enabled = !editMode;
            accUpdateButton.Enabled = editMode;
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
                AltAccEmail = contact.AltEmailAddress ?? string.Empty
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
                InvoiceTypeID = CoalesceFk(form.InvoiceTypeID, defaults.InvoiceTypeID),
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

        private bool TrySaveContact(out string errorMessage)
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
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "ContactDetails TrySaveContact error: " + ex.Message);
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
        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!EnsureValidForSave())
            {
                RefreshAfterSave();
                return;
            }

            if (TrySaveContact(out string message))
                SetStatus(message ?? "Contact saved.", false);
            else
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
                SetStatus(message ?? "Save failed.", true);
                MarkDirtyFromServer();
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

                var repo = new ContactsRepository();
                int newId = repo.Insert(contact);
                if (newId <= 0)
                {
                    SetStatus("Insert failed — contact was not created.", true);
                    RefreshAfterSave();
                    return;
                }

                contact.ContactID = newId;
                CompanyIDLabel.Text = newId.ToString();
                SetButtonStatus(true);

                if (!TrySaveAccInfo(newId, contact, out bool accWasSaved, out string accError))
                {
                    SetStatus("Contact created (ID " + newId + "), but account info failed: " + accError, true);
                    RefreshAfterSave();
                    return;
                }

                ClearDirtyState();
                SetStatus(accWasSaved
                    ? "Contact and account info created (ID " + newId + ")."
                    : "Contact created (ID " + newId + ").", false);
                RefreshAfterSave();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "ContactDetails Insert error: " + ex.Message);
                SetStatus("Error inserting contact: " + ex.Message, true);
                RefreshAfterSave();
            }
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
            Response.Redirect("~/Pages/OrderDetail.aspx?NewOrder=true&CoID=" + CompanyIDLabel.Text);
        }
        protected void btnForceNext_Click(object sender, EventArgs e)
        {
            // TODO: implement force next logic using SQL equivalent
            SetStatus("Force Next (SQL) pending migration.", null);
            upnlContactDetails.Update();
        }
        protected void btnForceCheckup_Click(object sender, EventArgs e)
        {
            SetStatus("Force Checkup (SQL) pending migration.", null);
            upnlContactDetails.Update();
        }
        protected void btnRecalcAverage_Click(object sender, EventArgs e)
        {
            SetStatus("Recalc average (SQL) pending migration.", null);
            upnlContactDetails.Update();
        }
        protected void btnCancel_Click(object sender, EventArgs e)
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
                SetStatus(error ?? "Account info save failed.", true);
                RefreshAfterSave();
                return;
            }

            ClearDirtyState();
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
                SetStatus(error ?? "Account info save failed.", true);
                RefreshAfterSave();
                return;
            }

            ClearDirtyState();
            SetStatus(accWasSaved ? "Account details saved." : "No account data to save.", accWasSaved ? (bool?)false : null);
            RefreshAfterSave();
        }
        protected void tabcContact_OnActiveTabChanged(object sender, EventArgs e)
        {
            // could store active tab index if needed
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
                AppLogger.WriteLog(SystemConstants.LogTypes.Customers,
                    $"User '{User.Identity.Name}' updated item usage line {lineNo} for contact {contactId}.");

                gvContactItems.EditIndex = -1;
                BindItemUsageGrid(contactId);
                SetStatus("Item usage line updated.", false);
                upnlItems.Update();
                upnlContactDetails.Update();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "gvContactItems_RowUpdating error: " + ex.Message);
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
                AppLogger.WriteLog(SystemConstants.LogTypes.Customers,
                    $"User '{User.Identity.Name}' deleted item usage line {lineNo} for contact {contactId}.");

                gvContactItems.EditIndex = -1;
                BindItemUsageGrid(contactId);
                SetStatus("Item usage line deleted.", false);
                upnlItems.Update();
                upnlContactDetails.Update();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "gvContactItems_RowDeleting error: " + ex.Message);
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
