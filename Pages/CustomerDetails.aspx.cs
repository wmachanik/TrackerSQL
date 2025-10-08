// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.Pages.CustomerDetails
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using AjaxControlToolkit;
using System;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerDotNet.Classes;
using TrackerDotNet.Controls;

//- only form later versions #nullable disable
// TrackerDotNet.Pages.CustomerDetails
namespace TrackerDotNet.Pages
{
    public partial class CustomerDetails : Page
    {


        private const string CONST_URL_REQUEST_CustomerID = "ID";
        private const string CONST_URL_REQUEST_CUSTOMERACCFOCUS = "Focus_AccInfo";
        private const string CONST_FORCE_GVITEMS_BIND = "ForceGVItemsToBind";
        private const string CONST_LASTTABCONTER = "LastTabcCustomerIndex";
        private const int CONST_GVITEMS_COL_ITEMDATE = 1;
        private const int CONST_GVITEMS_COL_ITEMPROVIDEDID = 1;
        private static string prevPage = string.Empty;
        protected Label lblCustomerID;
        protected ScriptManager smCustomerDetails;
        protected UpdateProgress uprgCustomerDetails;
        protected UpdatePanel upnlCustomerDetails;
        protected TextBox CompanyNameTextBox;
        protected Label CompanyIDLabel;
        protected TextBox ContactFirstNameTextBox;
        protected TextBox ContactLastNameTextBox;
        protected TextBox ContactTitleTextBox;
        protected TextBox ContactAltFirstNameTextBox;
        protected TextBox ContactAltLastNameTextBox;
        protected TextBox BillingAddressTextBox;
        protected TextBox DepartmentTextBox;
        protected TextBox PostalCodeTextBox;
        protected DropDownList ddlCities;
        protected RequiredFieldValidator RequiredFieldValidatorddlCities;
        protected TextBox ProvinceTextBox;
        protected TextBox PhoneNumberTextBox;
        protected TextBox CellNumberTextBox;
        protected TextBox FaxNumberTextBox;
        protected TextBox EmailAddressTextBox;
        protected TextBox AltEmailAddressTextBox;
        protected DropDownList ddlCustomerTypes;
        protected DropDownList ddlEquipTypes;
        protected TextBox MachineSNTextBox;
        protected DropDownList ddlFirstPreference;
        protected TextBox PriPrefQtyTextBox;
        protected DropDownList ddlPackagingTypes;
        protected DropDownList ddlDeliveryBy;
        protected RequiredFieldValidator ddlDeliveryByRequiredFieldValidator;
        protected DropDownList ddlAgent;
        protected Label ReminderCountLabel;
        protected Label LastReminderLabel;
        protected CheckBox enabledCheckBox;
        protected CheckBox autofulfillCheckBox;
        protected CheckBox UsesFilterCheckBox;
        protected CheckBox PredictionDisabledCheckBox;
        protected CheckBox AlwaysSendChkUpCheckBox;
        protected CheckBox NormallyRespondsCheckBox;
        protected TextBox NotesTextBox;
        protected Button btnUpdate;
        protected Button btnUpdateAndReturn;
        protected Button btnInsert;
        protected Button btnCopy2AccInfo;
        protected Button btnAddLasOrder;
        protected Button btnForceNext;
        protected Button btnRecalcAverage;
        protected Button btnCancel;
        protected Literal ltrlStatus;
        protected UpdatePanel uppnlTabContainer;
        protected TabContainer tabcCustomer;
        protected TabPanel tabpnlAccountInfo;
        protected UpdatePanel dvCustomersAccInfoUpdatePanel;
        protected TextBox accFullCoNameTextBox;
        protected TextBox accCustomerVATNoTextBox;
        protected DropDownList accInvoiceTypesDropDownList;
        protected CheckBox accRequiresPurchOrderCheckBox;
        protected CheckBox accEnabledCheckBox;
        protected TextBox accBillAddr1TextBox;
        protected TextBox accBillAddr2TextBox;
        protected TextBox accBillAddr3TextBox;
        protected TextBox accBillAddr4TextBox;
        protected TextBox accBillAddr5TextBox;
        protected TextBox accShipAddr1TextBox;
        protected TextBox accShipAddr2TextBox;
        protected TextBox accShipAddr3TextBox;
        protected TextBox accShipAddr4TextBox;
        protected TextBox accShipAddr5TextBox;
        protected TextBox accFirstNameTextBox;
        protected TextBox accLastNameTextBox;
        protected TextBox accAccEmailTextBox;
        protected TextBox accAltFirstNameTextBox;
        protected TextBox accAltLastNameTextBox;
        protected TextBox accAltEmailTextBox;
        protected DropDownList accPaymentTermsDropDownList;
        protected DropDownList accPriceLevelsDropDownList;
        protected TextBox accRegNoTextBox;
        protected TextBox accLimitTextBox;
        protected TextBox accBankAccNoTextBox;
        protected TextBox accBankBranchTextBox;
        protected TextBox accNotesTextBox;
        protected Label CustomersAccInfoIDLabel;
        protected Button accAddDetailsButton;
        protected Button accUpdateButton;
        protected TabPanel tabpnlNextRequired;
        protected UpdatePanel upnlNextItems;
        protected DataGrid dgCustomerUsage;
        protected TabPanel tabpnlItems;
        protected UpdateProgress updtprgItems;
        protected UpdatePanel upnlItems;
        protected GridView gvItems;
        protected ObjectDataSource odsItemUsage;
        protected ObjectDataSource odsCities;
        protected ObjectDataSource odsItems;
        protected ObjectDataSource odsEquipTypes;
        protected ObjectDataSource odsCustomerTypes;
        protected ObjectDataSource odsPersons;
        protected ObjectDataSource odsPackagingTypes;
        protected ObjectDataSource odsInvoiceTypes;
        protected ObjectDataSource odsPaymentTerms;
        protected ObjectDataSource odsPriceLevels;
        protected ObjectDataSource dsCustomerUsage;

        private void SetButtonStatus(bool pEditMode)
        {
            string[] rolesForUser = Roles.GetRolesForUser();
            bool flag = !Roles.IsUserInRole("repair") || rolesForUser.Length != 1;
            if (pEditMode)
            {
                this.btnUpdate.Enabled = flag;
                this.btnUpdateAndReturn.Enabled = flag;
                this.btnCopy2AccInfo.Enabled = flag;
                this.btnAddLasOrder.Enabled = flag;
                this.btnForceNext.Enabled = flag;
                this.btnInsert.Enabled = false;
                this.tabcCustomer.Visible = true;
                this.btnRecalcAverage.Enabled = flag;
                this.accAddDetailsButton.Enabled = false;
                this.accUpdateButton.Enabled = flag;
            }
            else
            {
                this.btnUpdate.Enabled = false;
                this.btnUpdateAndReturn.Enabled = false;
                this.btnCopy2AccInfo.Enabled = false;
                this.btnAddLasOrder.Enabled = false;
                this.btnForceNext.Enabled = false;
                this.btnInsert.Enabled = flag;
                this.btnRecalcAverage.Enabled = false;
                this.enabledCheckBox.Checked = flag;
                this.tabcCustomer.Visible = false;
                this.accAddDetailsButton.Enabled = flag;
                this.accUpdateButton.Enabled = false;
            }
            this.upnlCustomerDetails.Update();
            this.dvCustomersAccInfoUpdatePanel.Update();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                CustomerDetails.prevPage = !(this.Request.UrlReferrer == (Uri)null) ? this.Request.UrlReferrer.ToString() : string.Empty;
                if (this.Request.QueryString["ID"] != null)
                {
                    CustomersTbl customersTbl = new CustomersTbl();
                    this.SetButtonStatus(true);
                    this.accInvoiceTypesDropDownList.DataBind();
                    this.accPaymentTermsDropDownList.DataBind();
                    this.accPriceLevelsDropDownList.DataBind();
                    this.gvItems.Sort("Date", SortDirection.Descending);
                    this.PutDataOnForm(customersTbl.GetCustomerByCustomerID(this.GetCustomerIDFromRequest(), ""));
                }
                else
                    this.SetButtonStatus(false);
                if (this.Session["LastTabcCustomerIndex"] != null)
                {
                    int num = (int)this.Session["LastTabcCustomerIndex"];
                    if (num > 0)
                        this.tabcCustomer.ActiveTabIndex = num;
                }
            }
            if (this.Session["ForceGVItemsToBind"] == null || !(bool)this.Session["ForceGVItemsToBind"])
                return;
            this.Session["ForceGVItemsToBind"] = (object)null;
        }

        protected void Page_LoadComplete(object sender, EventArgs e)
        {
            if (this.IsPostBack)
                return;
            string str = this.Request.QueryString["ID"];
            if (this.Request.QueryString["Focus_AccInfo"] == null || !this.Request.QueryString["Focus_AccInfo"].StartsWith("Y", StringComparison.OrdinalIgnoreCase))
                return;
            this.tabcCustomer.ActiveTabIndex = 0;
            this.Page.SetFocus(this.accFullCoNameTextBox);
            this.uppnlTabContainer.Update();
        }

        private long GetCustomerIDFromRequest()
        {
            long result = 0;
            if (this.Request.QueryString["ID"] != null && !long.TryParse(this.Request.QueryString["ID"], out result))
                result = 0;
            return result;
        }

        private void PutDataOnForm(CustomersTbl pCustomersTblData)
        {
            this.CompanyNameTextBox.Text = pCustomersTblData.CompanyName;
            this.CompanyIDLabel.Text = pCustomersTblData.CustomerID.ToString();
            this.ContactFirstNameTextBox.Text = pCustomersTblData.ContactFirstName;
            this.ContactLastNameTextBox.Text = pCustomersTblData.ContactLastName;
            this.ContactTitleTextBox.Text = pCustomersTblData.ContactTitle;
            this.ContactAltFirstNameTextBox.Text = pCustomersTblData.ContactAltFirstName;
            this.ContactAltLastNameTextBox.Text = pCustomersTblData.ContactAltLastName;
            this.BillingAddressTextBox.Text = pCustomersTblData.BillingAddress;
            this.DepartmentTextBox.Text = pCustomersTblData.Department;
            this.PostalCodeTextBox.Text = pCustomersTblData.PostalCode;
            this.ddlCities.SelectedValue = pCustomersTblData.City.ToString();
            this.ProvinceTextBox.Text = pCustomersTblData.Province;
            this.PhoneNumberTextBox.Text = pCustomersTblData.PhoneNumber;
            this.CellNumberTextBox.Text = pCustomersTblData.CellNumber;
            this.FaxNumberTextBox.Text = pCustomersTblData.FaxNumber;
            this.EmailAddressTextBox.Text = pCustomersTblData.EmailAddress;
            this.AltEmailAddressTextBox.Text = pCustomersTblData.AltEmailAddress;
            this.ddlCustomerTypes.SelectedValue = pCustomersTblData.CustomerTypeID.ToString();
            this.ddlEquipTypes.SelectedValue = pCustomersTblData.EquipType.ToString();
            this.MachineSNTextBox.Text = pCustomersTblData.MachineSN;
            this.ddlFirstPreference.SelectedValue = pCustomersTblData.CoffeePreference.ToString();
            this.PriPrefQtyTextBox.Text = pCustomersTblData.PriPrefQty.ToString("0.###");
            this.ddlPackagingTypes.SelectedValue = pCustomersTblData.PrefPackagingID.ToString();
            this.ddlDeliveryBy.SelectedValue = pCustomersTblData.PreferedAgent.ToString();
            this.ddlAgent.SelectedValue = pCustomersTblData.SalesAgentID.ToString();
            this.ReminderCountLabel.Text = pCustomersTblData.ReminderCount.ToString();
            this.LastReminderLabel.Text = $"{pCustomersTblData.LastDateSentReminder:d}";
            this.enabledCheckBox.Checked = pCustomersTblData.enabled;
            this.autofulfillCheckBox.Checked = pCustomersTblData.autofulfill;
            this.UsesFilterCheckBox.Checked = pCustomersTblData.UsesFilter;
            this.PredictionDisabledCheckBox.Checked = pCustomersTblData.PredictionDisabled;
            this.AlwaysSendChkUpCheckBox.Checked = pCustomersTblData.AlwaysSendChkUp;
            this.NormallyRespondsCheckBox.Checked = pCustomersTblData.NormallyResponds;
            this.NotesTextBox.Text = pCustomersTblData.Notes;
            this.PutAccDataOnForm(pCustomersTblData.CustomerID);
        }

        private void PlaceAccDataOnForm(CustomersAccInfoTbl pCustomersAccInfo)
        {
            this.accFullCoNameTextBox.Text = pCustomersAccInfo.FullCoName;
            this.accCustomerVATNoTextBox.Text = pCustomersAccInfo.CustomerVATNo;
            this.accInvoiceTypesDropDownList.SelectedValue = pCustomersAccInfo.InvoiceTypeID.ToString();
            this.accRequiresPurchOrderCheckBox.Checked = pCustomersAccInfo.RequiresPurchOrder;
            this.accEnabledCheckBox.Checked = pCustomersAccInfo.Enabled;
            this.accBillAddr1TextBox.Text = pCustomersAccInfo.BillAddr1;
            this.accBillAddr2TextBox.Text = pCustomersAccInfo.BillAddr2;
            this.accBillAddr3TextBox.Text = pCustomersAccInfo.BillAddr3;
            this.accBillAddr4TextBox.Text = pCustomersAccInfo.BillAddr4;
            this.accBillAddr5TextBox.Text = pCustomersAccInfo.BillAddr5;
            this.accShipAddr1TextBox.Text = pCustomersAccInfo.ShipAddr1;
            this.accShipAddr2TextBox.Text = pCustomersAccInfo.ShipAddr2;
            this.accShipAddr3TextBox.Text = pCustomersAccInfo.ShipAddr3;
            this.accShipAddr4TextBox.Text = pCustomersAccInfo.ShipAddr4;
            this.accShipAddr5TextBox.Text = pCustomersAccInfo.ShipAddr5;
            this.accFirstNameTextBox.Text = pCustomersAccInfo.AccFirstName;
            this.accLastNameTextBox.Text = pCustomersAccInfo.AccLastName;
            this.accAccEmailTextBox.Text = pCustomersAccInfo.AccEmail;
            this.accAltFirstNameTextBox.Text = pCustomersAccInfo.AltAccFirstName;
            this.accAltLastNameTextBox.Text = pCustomersAccInfo.AltAccLastName;
            this.accAltEmailTextBox.Text = pCustomersAccInfo.AltAccEmail;
            this.accPaymentTermsDropDownList.SelectedValue = pCustomersAccInfo.PaymentTermID.ToString();
            this.accPriceLevelsDropDownList.SelectedValue = pCustomersAccInfo.PriceLevelID.ToString();
            this.accRegNoTextBox.Text = pCustomersAccInfo.RegNo;
            this.accLimitTextBox.Text = $"{pCustomersAccInfo.Limit:0.00}";
            this.accBankAccNoTextBox.Text = pCustomersAccInfo.BankAccNo;
            this.accBankBranchTextBox.Text = pCustomersAccInfo.BankBranch;
            this.accNotesTextBox.Text = pCustomersAccInfo.Notes;
            this.CustomersAccInfoIDLabel.Text = pCustomersAccInfo.CustomersAccInfoID.ToString();
        }

        private void PutAccDataOnForm(long pCustomerID)
        {
            CustomersAccInfoTbl pCustomersAccInfo = new CustomersAccInfoTbl();
            if (pCustomerID > 0L)
            {
                pCustomersAccInfo = pCustomersAccInfo.GetByCustomerID(pCustomerID);
                pCustomerID = pCustomersAccInfo.CustomerID;
            }
            string[] rolesForUser = Roles.GetRolesForUser();
            bool flag = !Roles.IsUserInRole("repair") || rolesForUser.Length != 1;
            if (pCustomersAccInfo.CustomersAccInfoID == 0)
            {
                this.accAddDetailsButton.Enabled = flag;
                this.accUpdateButton.Enabled = false;
            }
            else
            {
                this.accAddDetailsButton.Enabled = false;
                this.accUpdateButton.Enabled = flag;
                this.PlaceAccDataOnForm(pCustomersAccInfo);
            }
            this.dvCustomersAccInfoUpdatePanel.Update();
        }

        private CustomersAccInfoTbl GetAccDataFromForm()
        {
            CustomersAccInfoTbl accDataFromForm = new CustomersAccInfoTbl();
            if (!string.IsNullOrEmpty(this.CustomersAccInfoIDLabel.Text))
                accDataFromForm.CustomersAccInfoID = this.StringToInt32(this.CustomersAccInfoIDLabel.Text);
            accDataFromForm.CustomerID = this.StringToInt64(this.CompanyIDLabel.Text);
            accDataFromForm.FullCoName = this.accFullCoNameTextBox.Text;
            accDataFromForm.CustomerVATNo = this.accCustomerVATNoTextBox.Text;
            accDataFromForm.InvoiceTypeID = this.StringToInt32(this.accInvoiceTypesDropDownList.SelectedValue);
            accDataFromForm.RequiresPurchOrder = this.accRequiresPurchOrderCheckBox.Checked;
            accDataFromForm.Enabled = this.accEnabledCheckBox.Checked;
            accDataFromForm.BillAddr1 = this.accBillAddr1TextBox.Text;
            accDataFromForm.BillAddr2 = this.accBillAddr2TextBox.Text;
            accDataFromForm.BillAddr3 = this.accBillAddr3TextBox.Text;
            accDataFromForm.BillAddr4 = this.accBillAddr4TextBox.Text;
            accDataFromForm.BillAddr5 = this.accBillAddr5TextBox.Text;
            accDataFromForm.ShipAddr1 = this.accShipAddr1TextBox.Text;
            accDataFromForm.ShipAddr2 = this.accShipAddr2TextBox.Text;
            accDataFromForm.ShipAddr3 = this.accShipAddr3TextBox.Text;
            accDataFromForm.ShipAddr4 = this.accShipAddr4TextBox.Text;
            accDataFromForm.ShipAddr5 = this.accShipAddr5TextBox.Text;
            accDataFromForm.AccFirstName = this.accFirstNameTextBox.Text;
            accDataFromForm.AccLastName = this.accLastNameTextBox.Text;
            accDataFromForm.AccEmail = this.accAccEmailTextBox.Text;
            accDataFromForm.AltAccFirstName = this.accAltFirstNameTextBox.Text;
            accDataFromForm.AltAccLastName = this.accAltLastNameTextBox.Text;
            accDataFromForm.AltAccEmail = this.accAltEmailTextBox.Text;
            accDataFromForm.PaymentTermID = this.StringToInt32(this.accPaymentTermsDropDownList.SelectedValue);
            accDataFromForm.PriceLevelID = this.StringToInt32(this.accPriceLevelsDropDownList.SelectedValue);
            accDataFromForm.RegNo = this.accRegNoTextBox.Text;
            accDataFromForm.Limit = this.StringToDouble(this.accLimitTextBox.Text);
            accDataFromForm.BankAccNo = this.accBankAccNoTextBox.Text;
            accDataFromForm.BankBranch = this.accBankBranchTextBox.Text;
            accDataFromForm.Notes = this.accNotesTextBox.Text;
            return accDataFromForm;
        }

        private int StringToInt32(string pValue)
        {
            int result = 0;
            int.TryParse(pValue, out result);
            return result;
        }

        private long StringToInt64(string pValue)
        {
            long result = 0;
            long.TryParse(pValue, out result);
            return result;
        }

        private double StringToDouble(string pValue)
        {
            double result = 0.0;
            double.TryParse(pValue, out result);
            return result;
        }

        private CustomersTbl GetDataFromForm()
        {
            return new CustomersTbl()
            {
                CompanyName = this.CompanyNameTextBox.Text,
                CustomerID = string.IsNullOrWhiteSpace(this.CompanyIDLabel.Text) ? 0 : this.StringToInt64(this.CompanyIDLabel.Text),
                ContactFirstName = this.ContactFirstNameTextBox.Text,
                ContactLastName = this.ContactLastNameTextBox.Text,
                ContactTitle = this.ContactTitleTextBox.Text,
                ContactAltFirstName = this.ContactAltFirstNameTextBox.Text,
                ContactAltLastName = this.ContactAltLastNameTextBox.Text,
                BillingAddress = this.BillingAddressTextBox.Text,
                Department = this.DepartmentTextBox.Text,
                PostalCode = this.PostalCodeTextBox.Text,
                City = this.StringToInt32(this.ddlCities.SelectedValue),
                Province = this.ProvinceTextBox.Text,
                PhoneNumber = this.PhoneNumberTextBox.Text,
                CellNumber = this.CellNumberTextBox.Text,
                FaxNumber = this.FaxNumberTextBox.Text,
                EmailAddress = this.EmailAddressTextBox.Text,
                AltEmailAddress = this.AltEmailAddressTextBox.Text,
                CustomerTypeID = this.StringToInt32(this.ddlCustomerTypes.SelectedValue),
                EquipType = this.StringToInt32(this.ddlEquipTypes.SelectedValue),
                MachineSN = this.MachineSNTextBox.Text,
                CoffeePreference = this.StringToInt32(this.ddlFirstPreference.SelectedValue),
                PriPrefQty = string.IsNullOrWhiteSpace(this.PriPrefQtyTextBox.Text) ? 0.0 : this.StringToDouble(this.PriPrefQtyTextBox.Text),
                PrefPackagingID = this.StringToInt32(this.ddlPackagingTypes.SelectedValue),
                PreferedAgent = this.StringToInt32(this.ddlDeliveryBy.SelectedValue),
                SalesAgentID = this.StringToInt32(this.ddlAgent.SelectedValue),
                enabled = this.enabledCheckBox.Checked,
                autofulfill = this.autofulfillCheckBox.Checked,
                UsesFilter = this.UsesFilterCheckBox.Checked,
                PredictionDisabled = this.PredictionDisabledCheckBox.Checked,
                AlwaysSendChkUp = this.AlwaysSendChkUpCheckBox.Checked,
                NormallyResponds = this.NormallyRespondsCheckBox.Checked,
                Notes = this.NotesTextBox.Text
            };
        }

        public string GetPackagingDesc(int pPackagingID)
        {
            return pPackagingID > 0 ? new PackagingTbl().GetPackagingDesc(pPackagingID) : string.Empty;
        }

        protected CustomersAccInfoTbl CopyCompanyData2AccInfo(CustomersTbl pCustomer)
        {
            CustomersAccInfoTbl customersAccInfoTbl = new CustomersAccInfoTbl();
            customersAccInfoTbl.CustomerID = pCustomer.CustomerID;
            string[] strArray = pCustomer.BillingAddress.Split(new string[2]
                {",",";"}, 
                StringSplitOptions.RemoveEmptyEntries);
            customersAccInfoTbl.BillAddr1 = strArray.Length > 0 ? strArray[0].Trim() : string.Empty;
            customersAccInfoTbl.BillAddr2 = strArray.Length > 1 ? strArray[1].Trim() : string.Empty;
            customersAccInfoTbl.BillAddr3 = strArray.Length > 2 ? strArray[2].Trim() : string.Empty;
            for (int index = 3; index < strArray.Length; ++index)
            {
                customersAccInfoTbl.BillAddr4 = strArray[index].Trim();
                if (index + 1 < strArray.Length)
                    customersAccInfoTbl.BillAddr4 += ";";
            }
            customersAccInfoTbl.BillAddr5 = pCustomer.PostalCode;
            customersAccInfoTbl.ShipAddr1 = customersAccInfoTbl.BillAddr1;
            customersAccInfoTbl.ShipAddr2 = customersAccInfoTbl.BillAddr2;
            customersAccInfoTbl.ShipAddr3 = customersAccInfoTbl.BillAddr3;
            customersAccInfoTbl.ShipAddr4 = customersAccInfoTbl.BillAddr4;
            customersAccInfoTbl.ShipAddr5 = customersAccInfoTbl.BillAddr5;
            customersAccInfoTbl.AccEmail = pCustomer.EmailAddress;
            customersAccInfoTbl.AltAccEmail = pCustomer.AltEmailAddress;
            customersAccInfoTbl.FullCoName = pCustomer.CompanyName;
            customersAccInfoTbl.AccFirstName = pCustomer.ContactFirstName;
            customersAccInfoTbl.AccLastName = pCustomer.ContactLastName;
            customersAccInfoTbl.AltAccFirstName = pCustomer.ContactAltFirstName;
            customersAccInfoTbl.AltAccLastName = pCustomer.ContactAltLastName;
            return customersAccInfoTbl;
        }

        protected void btnInsert_Click(object sender, EventArgs e)
        {
            string empty = string.Empty;
            CustomersTbl dataFromForm = this.GetDataFromForm();
            if (dataFromForm.InsertCustomer(dataFromForm, ref empty))
            {
                CustomersTbl customerByName = dataFromForm.GetCustomerByName(dataFromForm.CompanyName);
                if (customerByName.CustomerID > 0L)
                {
                    CustomersAccInfoTbl pCustomersAccInfoTbl = this.CopyCompanyData2AccInfo(customerByName);
                    pCustomersAccInfoTbl.Enabled = true;
                    if (string.IsNullOrEmpty(pCustomersAccInfoTbl.Insert(pCustomersAccInfoTbl)))
                    {
                        AppLogger.WriteLog(SystemConstants.LogTypes.Customers, $"User '{User.Identity.Name}' inserted customer {customerByName.CustomerID} ({customerByName.CompanyName}) and account info.");
                        showMessageBox showMessageBox = new showMessageBox(this.Page, "Insert", "Customer account info added, please edit.");
                        this.Response.Redirect($"{this.Page.ResolveUrl("~/Pages/CustomerDetails.aspx")}?{"ID"}={customerByName.CustomerID}&{"Focus_AccInfo"}=Y");
                    }
                    else
                    {
                        AppLogger.WriteLog(SystemConstants.LogTypes.Customers, $"User '{User.Identity.Name}' inserted customer {customerByName.CustomerID} ({customerByName.CompanyName}), but account info insert failed.");
                        showMessageBox showMessageBox1 = new showMessageBox(this.Page, "Insert", "Error inserting customer account info");
                    }
                    this.ltrlStatus.Text = "Customer Added";
                }
                else
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Customers, $"User '{User.Identity.Name}' inserted customer ({dataFromForm.CompanyName}), but could not retrieve new CustomerID.");
                    string script = $"redirect('{$"{this.Page.ResolveUrl("~/Pages/Customers.aspx")}?CompanyName={customerByName.CompanyName}"}');";
                    System.Web.UI.ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "CustomerInserted", script, true);
                }
            }
            else
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Customers, $"User '{User.Identity.Name}' failed to insert customer ({dataFromForm.CompanyName}): {empty}");
                this.ltrlStatus.Text = "ERROR: " + empty;
            }
        }
        protected void ServerButton_Click(object sender, EventArgs e)
        {
            this.ClientScript.RegisterStartupScript(this.GetType(), "key", "launchModal();", true);
        }

        protected void btnAddLasOrder_Click(object sender, EventArgs e)
        {
            this.Response.Redirect($"~/Pages/OrderDetail.aspx?{SystemConstants.UrlParameterConstants.CustomerID}={this.CompanyIDLabel.Text}&LastOrder=Y");
        }

        protected void btnForceNext_Click(object sender, EventArgs e)
        {
            new ClientUsageTbl().ForceNextCoffeeDate(new TrackerTools().GetClosestNextRoastDate(TimeZoneUtils.Now().AddDays(14.0).Date).AddDays(3.0), this.StringToInt64(this.CompanyIDLabel.Text));
            new CustomersTbl().IncrementReminderCount(this.StringToInt64(this.CompanyIDLabel.Text));
            this.dgCustomerUsage.DataBind();
            string script = $"showMessage('{$"{this.CompanyNameTextBox.Text} force to skip a week of prediction"}');";
            System.Web.UI.ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "CustomerInserted", script, true);
        }

        protected void btnRecalcAverage_Click(object sender, EventArgs e)
        {
            new GeneralTrackerDbTools().CalcAndSaveNextRequiredDates(this.StringToInt64(this.CompanyIDLabel.Text));
            this.dgCustomerUsage.DataBind();
            this.upnlNextItems.Update();
            string script = $"showMessage('{$"{this.CompanyNameTextBox.Text} average calculations updated"}');";
            System.Web.UI.ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "CustomerAverageCalcDone", script, true);
        }

        protected void tabcCustomer_OnActiveTabChanged(object sender, EventArgs e)
        {
            this.Session["LastTabcCustomerIndex"] = (object)this.tabcCustomer.ActiveTabIndex;
        }
        public string GetItemDesc(int pItemID)
        {
            return pItemID > 0 ? ItemTypeTbl.GetItemTypeDescById(pItemID) : string.Empty;
        }

        protected void accPaymentTermsDropDownList_DataBound(object sender, EventArgs e)
        {
            DropDownList dropDownList = (DropDownList)sender;
            if (dropDownList == null || !string.IsNullOrEmpty(dropDownList.SelectedValue))
                return;
            long CustomerIDFromRequest = this.GetCustomerIDFromRequest();
            if (CustomerIDFromRequest > 0L)
                new CustomersAccInfoTbl().GetByPaymentTypeIDByCustomerID(CustomerIDFromRequest);
            else
                dropDownList.SelectedIndex = 1;
        }

        protected void btnCopy2AccInfo_Click(object sender, EventArgs e)
        {
            this.PlaceAccDataOnForm(this.CopyCompanyData2AccInfo(this.GetDataFromForm()));
            this.uppnlTabContainer.Update();
            this.dvCustomersAccInfoUpdatePanel.Update();
        }

        protected void accUpdateButton_Click(object sender, EventArgs e)
        {
            this.UpdateAccountInfo(this.GetAccDataFromForm());
        }

        private void UpdateRecord()
        {
            string str = new CustomersTbl().UpdateCustomer(this.GetDataFromForm(), this.StringToInt64(this.CompanyIDLabel.Text));
            if (string.IsNullOrWhiteSpace(str))
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Customers, $"User '{User.Identity.Name}' updated customer {this.CompanyIDLabel.Text}.");
                this.ltrlStatus.Text = "Record Updated";
            }
            else
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Customers, $"User '{User.Identity.Name}' failed to update customer {this.CompanyIDLabel.Text}: {str}");
                this.ltrlStatus.Text = str;
            }
        }
        private void ReturnToPrevPage() => this.ReturnToPrevPage(false);

        private void ReturnToPrevPage(bool GoToCustomers)
        {
            if (GoToCustomers || string.IsNullOrWhiteSpace(CustomerDetails.prevPage))
                this.Response.Redirect("~/Pages/Customers.aspx");
            else
                this.Response.Redirect(CustomerDetails.prevPage);
        }
        protected void btnUpdate_Click(object sender, EventArgs e) => this.UpdateRecord();

        protected void btnUpdateAndReturn_Click(object sender, EventArgs e)
        {
            this.UpdateRecord();
            this.ReturnToPrevPage();
        }
        protected void btnCancel_Click(object sender, EventArgs e) => this.ReturnToPrevPage();

        protected void gvItems_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            bool flag = false;
            if (e.CommandName == "Delete")
            {
                ItemUsageTbl itemUsageTbl = new ItemUsageTbl();
                itemUsageTbl.ClientUsageLineNo = Convert.ToInt32(e.CommandArgument);
                itemUsageTbl.DeleteItemLine(itemUsageTbl.ClientUsageLineNo);
                AppLogger.WriteLog(SystemConstants.LogTypes.Customers, $"User '{User.Identity.Name}' deleted item line {itemUsageTbl.ClientUsageLineNo} for customer {this.CompanyIDLabel.Text}.");
                flag = true;
            }
            else if (e.CommandName == "Update")
            {
                int int32 = Convert.ToInt32(e.CommandArgument);
                TextBox control1 = (TextBox)this.gvItems.Rows[int32].FindControl("tbxItemDate");
                DropDownList control2 = (DropDownList)this.gvItems.Rows[int32].FindControl("ddlItems");
                TextBox control3 = (TextBox)this.gvItems.Rows[int32].FindControl("tbxAmountProvided");
                DropDownList control4 = (DropDownList)this.gvItems.Rows[int32].FindControl("ddlPackaging");
                TextBox control5 = (TextBox)this.gvItems.Rows[int32].FindControl("tbxPrepTypeID");
                TextBox control6 = (TextBox)this.gvItems.Rows[int32].FindControl("tbxNotes");
                Label control7 = (Label)this.gvItems.Rows[int32].FindControl("lblClientUsageLineNo");
                ItemUsageTbl ItemUsageLine = new ItemUsageTbl();
                ItemUsageLine.ItemDate = Convert.ToDateTime(control1.Text);
                ItemUsageLine.ItemProvidedID = this.StringToInt32(control2.SelectedValue);
                ItemUsageLine.AmountProvided = this.StringToDouble(control3.Text);
                ItemUsageLine.PackagingID = this.StringToInt32(control4.SelectedValue);
                ItemUsageLine.PrepTypeID = this.StringToInt32(control5.Text);
                ItemUsageLine.Notes = control6.Text;
                ItemUsageLine.CustomerID = this.StringToInt64(this.CompanyIDLabel.Text);
                ItemUsageLine.ClientUsageLineNo = this.StringToInt32(control7.Text);
                ItemUsageLine.UpdateItemsUsed(ItemUsageLine);
                AppLogger.WriteLog(SystemConstants.LogTypes.Customers, $"User '{User.Identity.Name}' updated item line {ItemUsageLine.ClientUsageLineNo} for customer {this.CompanyIDLabel.Text}.");
                flag = true;
            }
            if (!flag)
                return;
            this.odsItems.Select();
            this.gvItems.DataBind();
            this.upnlItems.Update();
        }

        protected void UpdateAccountInfo(CustomersAccInfoTbl pUpdateAccInfo)
        {
            string str = pUpdateAccInfo.Update(pUpdateAccInfo);
            if (string.IsNullOrEmpty(str))
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Customers, $"User '{User.Identity.Name}' updated account info {pUpdateAccInfo.CustomersAccInfoID} for customer {pUpdateAccInfo.CustomerID}.");
                showMessageBox showMessageBox1 = new showMessageBox(this.Page, "Update", "Customer Account Info Updated");
            }
            else
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Customers, $"User '{User.Identity.Name}' failed to update account info {pUpdateAccInfo.CustomersAccInfoID} for customer {pUpdateAccInfo.CustomerID}: {str}");
                showMessageBox showMessageBox2 = new showMessageBox(this.Page, "Update", "Error updating: " + str);
            }
        }

        protected void accAddDetailsButton_Click(object sender, EventArgs e)
        {
            CustomersAccInfoTbl accDataFromForm = this.GetAccDataFromForm();
            if (accDataFromForm.CustomersAccInfoID != 0)
                return;
            string str = accDataFromForm.Insert(accDataFromForm);
            if (string.IsNullOrEmpty(str))
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Customers, $"User '{User.Identity.Name}' inserted account info for customer {accDataFromForm.CustomerID}.");
                showMessageBox showMessageBox = new showMessageBox(this.Page, "Insert", "Customer Account Info Inserted");
                this.accAddDetailsButton.Enabled = false;
                this.accUpdateButton.Enabled = true;
                this.dvCustomersAccInfoUpdatePanel.Update();
            }
            else
            {
                CustomersAccInfoTbl byCustomerID = accDataFromForm.GetByCustomerID(accDataFromForm.CustomerID);
                if (!byCustomerID.CustomersAccInfoID.Equals(0))
                {
                    accDataFromForm.CustomersAccInfoID = byCustomerID.CustomersAccInfoID;
                    this.UpdateAccountInfo(accDataFromForm);
                }
                else
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Customers, $"User '{User.Identity.Name}' failed to insert account info for customer {accDataFromForm.CustomerID}: {str}");
                    showMessageBox showMessageBox = new showMessageBox(this.Page, "Insert", "Error inserting: " + str);
                }
            }
        }

        protected void btnForceCheckup_Click(object sender, EventArgs e)
        {
            try
            {
                long customerID = Convert.ToInt64(this.CompanyIDLabel.Text);
                if (customerID <= 0)
                {
                    var errorMsg = new showMessageBox(this.Page, "Error", "No customer selected");
                    return;
                }

                DateTime forceDate = TimeZoneUtils.Now().Date.AddDays(5);
                ClientUsageTbl clientUsage = new ClientUsageTbl();
                bool result = clientUsage.ForceNextCoffeeDate(forceDate, customerID);

                if (result)
                {
                    CustomersTbl customersTbl = new CustomersTbl();
                    customersTbl.ResetReminderCount(customerID);

                    this.upnlNextItems.Update();

                    AppLogger.WriteLog(SystemConstants.LogTypes.Customers, $"User '{User.Identity.Name}' forced checkup for customer {customerID}, next coffee date set to {forceDate:d}.");
                    var successMsg = new showMessageBox(this.Page, "Force Checkup", $"Customer {customerID} has been forced into next checkup cycle. Next coffee date set to {forceDate:d}");
                }
                else
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Customers, $"User '{User.Identity.Name}' failed to force checkup for customer {customerID}.");
                    var errorMsg = new showMessageBox(this.Page, "Error", $"Failed to force checkup for customer id: {customerID}");
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Customers, $"Error in btnForceCheckup_Click: {ex.Message}");
                var errorMsg = new showMessageBox(this.Page, "Error", $"Error forcing checkup: {ex.Message}");
            }
        }
    }
}
