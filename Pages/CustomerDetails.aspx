<%@ Page Title="Customer Details" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" MaintainScrollPositionOnPostback="true"
    CodeBehind="CustomerDetails.aspx.cs" Inherits="TrackerDotNet.Pages.CustomerDetails" %>

<asp:Content ID="cntCustomerDetailsHdr" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        function redirect(url) {
            alert("Customer Added");
            window.location = url;
        }
        function showMessage(thisMessage) {
            alert(thisMessage);
        }
    </script>
</asp:Content>
<asp:Content ID="cntCustomerDetailsBdy" ContentPlaceHolderID="MainContent" runat="server">
    <h2 class="InputFrm">Customer Details</h2>
    <asp:Label ID="lblCustomerID" Visible="false" runat="server" />
    <asp:ScriptManager ID="smCustomerDetails" runat="server">
    </asp:ScriptManager>
    <asp:UpdateProgress ID="uprgCustomerDetails" runat="server" AssociatedUpdatePanelID="upnlCustomerDetails">
        <ProgressTemplate>
            <img src="../images/animi/BlueArrowsUpdate.gif" alt="updating" width="16" height="16" />updating.....
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:UpdatePanel ID="upnlCustomerDetails" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnForceNext" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnForceCheckup" EventName="Click" />

        </Triggers>

        <ContentTemplate>
            <table class="TblMudZebra" cellpadding="0" cellspacing="0">
                <tr>
                    <td>Company Name</td>
                    <td colspan="4">
                        <asp:TextBox ID="CompanyNameTextBox" runat="server" Text='<%# Bind("CompanyName") %>' Width="33em" /></td>
                    <td>
                        <asp:Label ID="CompanyIDLabel" runat="server" Text='<%# Eval("CustomerID") %>' /></td>
                </tr>
                <tr>
                    <td>First Name</td>
                    <td>
                        <asp:TextBox ID="ContactFirstNameTextBox" runat="server" Text='<%# Bind("ContactFirstName") %>' /></td>
                    <td>Last Name</td>
                    <td>
                        <asp:TextBox ID="ContactLastNameTextBox" runat="server" Text='<%# Bind("ContactLastName") %>' /></td>
                    <td>Title</td>
                    <td>
                        <asp:TextBox ID="ContactTitleTextBox" runat="server" Text='<%# Bind("ContactTitle") %>' Width="3em" /></td>
                </tr>
                <tr>
                    <td>Alt First Name</td>
                    <td>
                        <asp:TextBox ID="ContactAltFirstNameTextBox" runat="server" Text='<%# Bind("ContactAltFirstName") %>' /></td>
                    <td>Alt Last Name</td>
                    <td>
                        <asp:TextBox ID="ContactAltLastNameTextBox" runat="server" Text='<%# Bind("ContactAltLastName") %>' /></td>
                    <td>&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td>Address</td>
                    <td colspan="3">
                        <asp:TextBox ID="BillingAddressTextBox" runat="server" Text='<%# Bind("BillingAddress") %>' Width="33em" /></td>
                    <td>Department</td>
                    <td>
                        <asp:TextBox ID="DepartmentTextBox" runat="server" Text='<%# Bind("Department") %>' /></td>
                </tr>
                <tr>
                    <td>Post Code</td>
                    <td>
                        <asp:TextBox ID="PostalCodeTextBox" runat="server" Text='<%# Bind("PostalCode") %>' /></td>
                    <td>Delivery Area</td>
                    <td>
                        <asp:DropDownList ID="ddlCities" runat="server" AppendDataBoundItems="True"
                            DataSourceID="odsCities" DataTextField="City" DataValueField="ID">
                            <asp:ListItem Value="0" Text="-please select-" />
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidatorddlCities" runat="server"
                            ErrorMessage="Please Select a city" ControlToValidate="ddlCities" InitialValue="0" />
                    </td>
                    <td>Prov</td>
                    <td>
                        <asp:TextBox ID="ProvinceTextBox" runat="server" Text='<%# Bind("Province") %>' /></td>
                </tr>
                <tr>
                    <td>Phone</td>
                    <td>
                        <asp:TextBox ID="PhoneNumberTextBox" runat="server" Text='<%# Bind("PhoneNumber") %>' /></td>
                    <td>Cell</td>
                    <td>
                        <asp:TextBox ID="CellNumberTextBox" runat="server" Text='<%# Bind("CellNumber") %>' /></td>
                    <td>Fax</td>
                    <td>
                        <asp:TextBox ID="FaxNumberTextBox" runat="server" Text='<%# Bind("FaxNumber") %>' /></td>
                </tr>
                <tr>
                    <td>Email</td>
                    <td colspan="2">
                        <asp:TextBox ID="EmailAddressTextBox" runat="server" Text='<%# Bind("EmailAddress") %>'
                            Width="25em" /></td>
                    <td>Alt Email Addr</td>
                    <td colspan="2">
                        <asp:TextBox ID="AltEmailAddressTextBox" runat="server" Text='<%# Bind("AltEmailAddress") %>'
                            Width="25em" /></td>
                </tr>
                <tr>
                    <td>Customer Type</td>
                    <td>
                        <asp:DropDownList ID="ddlCustomerTypes" runat="server" DataSourceID="odsCustomerTypes" DataTextField="CustTypeDesc"
                            DataValueField="CustTypeID" AppendDataBoundItems="true">
                            <asp:ListItem Value="0" Text="-please select-" />
                        </asp:DropDownList>
                    </td>
                    <td>Equip Type</td>
                    <td>
                        <asp:DropDownList ID="ddlEquipTypes" runat="server" AppendDataBoundItems="true"
                            DataSourceID="odsEquipTypes" DataTextField="EquipTypeName" DataValueField="EquipTypeId">
                            <asp:ListItem Text="none" Value="0" />
                        </asp:DropDownList>
                    </td>
                    <td>S/N</td>
                    <td>
                        <asp:TextBox ID="MachineSNTextBox" runat="server" Text='<%# Bind("MachineSN") %>' /></td>
                </tr>
                <tr>
                    <td>1st Preference</td>
                    <td>
                        <asp:DropDownList ID="ddlFirstPreference" runat="server" AppendDataBoundItems="true"
                            DataSourceID="odsItems" DataTextField="ItemDesc" DataValueField="ItemTypeID">
                            <asp:ListItem Text="none" Value="0" />
                        </asp:DropDownList>
                    </td>
                    <td>Pri Pref Qty</td>
                    <td>
                        <asp:TextBox ID="PriPrefQtyTextBox" runat="server" Text='<%# String.Format("{0:0.###}",Eval("PriPrefQty")) %>' /></td>
                    <td>Packaging</td>
                    <td>
                        <asp:DropDownList ID="ddlPackagingTypes" runat="server" AppendDataBoundItems="true"
                            DataSourceID="odsPackagingTypes" DataTextField="Description" DataValueField="PackagingID">
                            <asp:ListItem Text="none" Value="0" />
                        </asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td>Delivery By</td>
                    <td>
                        <asp:DropDownList ID="ddlDeliveryBy" runat="server" AppendDataBoundItems="true"
                            DataSourceID="odsPersons" DataTextField="Abreviation" DataValueField="PersonID">
                            <asp:ListItem Text="- ? -" Value="0" />
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator ID="ddlDeliveryByRequiredFieldValidator" runat="server"
                            ErrorMessage="Please select who will deliver" ControlToValidate="ddlDeliveryBy" InitialValue="0" />
                    </td>
                    <td>Agent</td>
                    <td>
                        <asp:DropDownList ID="ddlAgent" runat="server" AppendDataBoundItems="true"
                            DataSourceID="odsPersons" DataTextField="Abreviation" DataValueField="PersonID">
                            <asp:ListItem Text="none" Value="0" />
                        </asp:DropDownList>
                    </td>
                    <td>Reminders: [<asp:Label ID="ReminderCountLabel" runat="server" Text='<%# Eval("ReminderCount") %>' />]</td>
                    <td>LastReminderSent:
                        <asp:Label ID="LastReminderLabel" runat="server" Text='<%# Eval("LastDateSentReminder") %>' />
                    </td>
                </tr>
                <tr>
                    <td>Uses/Enabled/Filters</td>
                    <td colspan="5">&nbsp;&nbsp;&nbsp;
            <asp:CheckBox ID="enabledCheckBox" runat="server" Text="enabled" TextAlign="Right" Checked='<%# Bind("enabled") %>' />
                        &nbsp;&nbsp;&nbsp;
            <asp:CheckBox ID="autofulfillCheckBox" runat="server" Text="auto fulfill" TextAlign="Right"
                Checked='<%# Bind("autofulfill") %>' />
                        &nbsp;&nbsp;&nbsp;
            <asp:CheckBox ID="UsesFilterCheckBox" runat="server" Text="Uses Filter" TextAlign="Right"
                Checked='<%# Bind("UsesFilter") %>' />
                        &nbsp;&nbsp;&nbsp;
            <asp:CheckBox ID="PredictionDisabledCheckBox" runat="server" Text="PredictionDisabled"
                TextAlign="Right" Checked='<%# Bind("PredictionDisabled") %>' />
                        &nbsp;&nbsp;&nbsp;
            <asp:CheckBox ID="AlwaysSendChkUpCheckBox" runat="server" Text="AlwaysSendChkUp"
                TextAlign="Right" Checked='<%# Bind("AlwaysSendChkUp") %>' />
                        &nbsp;&nbsp;&nbsp;
            <asp:CheckBox ID="NormallyRespondsCheckBox" runat="server" Text="NormallyResponds"
                TextAlign="Right" Checked='<%# Bind("NormallyResponds") %>' />
                    </td>
                </tr>
                <tr>
                    <td>Notes</td>
                    <td colspan="5">
                        <asp:TextBox ID="NotesTextBox" runat="server" Text='<%# Bind("Notes") %>' TextMode="MultiLine"
                            Height="4em" Width="93%" /></td>
                </tr>
                <tr>
                    <td colspan="6" class="rowOddC">
                        <asp:Button ID="btnUpdate" Text="Update" runat="server" OnClick="btnUpdate_Click" />
                        &nbsp;&nbsp;&nbsp;
                        <asp:Button ID="btnUpdateAndReturn" Text="Update & Return" runat="server" OnClick="btnUpdateAndReturn_Click" AccessKey="U" ToolTip="update and return (AltShftU)" />
                        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                        <asp:Button ID="btnInsert" Text="Insert" runat="server" Enabled="false" OnClick="btnInsert_Click" />
                        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                        <asp:Button ID="btnCopy2AccInfo" Text="Copy2Acc" runat="server" Enabled="false" OnClick="btnCopy2AccInfo_Click" />
                        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                        <asp:Button ID="btnAddLasOrder" Text="Add Last" runat="server" OnClick="btnAddLasOrder_Click" AccessKey="L" ToolTip="add Last order to sheet (AltShftL)" />
                        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                        <asp:Button ID="btnForceNext" Text="Force Next" runat="server" ToolTip="force client to skip a week (AltShftF)" AccessKey="F" OnClick="btnForceNext_Click" />
                        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                        <asp:Button ID="btnForceCheckup" Text="Force Checkup" runat="server" ToolTip="Force customer into next checkup cycle (AltShftC)" AccessKey="C" OnClick="btnForceCheckup_Click" />
                        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                        <asp:Button ID="btnRecalcAverage" Text="Recalc Ave" runat="server" AccessKey="A"
                            ToolTip="force a recaluation of the clients consumption average (AltShftA)"
                            OnClick="btnRecalcAverage_Click" />
                        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                        <asp:Button ID="btnCancel" Text="Return" runat="server" OnClick="btnCancel_Click" />
                    </td>
            </table>
            <!-- ModalPopupExtender -->
        </ContentTemplate>
    </asp:UpdatePanel>
    <div class="status-message"><asp:Literal ID="ltrlStatus" Text="" runat="server" /></div>
    <br />
    <asp:UpdatePanel ID="uppnlTabContainer" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <ajaxToolkit:TabContainer ID="tabcCustomer" runat="server" AutoPostBack="true" OnActiveTabChanged="tabcCustomer_OnActiveTabChanged">
                <ajaxToolkit:TabPanel ID="tabpnlAccountInfo" runat="server" HeaderText="Accounts Info">
                    <ContentTemplate>
                        <asp:UpdatePanel ID="dvCustomersAccInfoUpdatePanel" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
                            <ContentTemplate>
                                <table class="TblCoffee" cellpadding="0" cellspacing="0">
                                    <tr>
                                        <td>Acc Company Name</td>
                                        <td>
                                            <asp:TextBox ID="accFullCoNameTextBox" runat="server" Width="20em" /></td>
                                        <td>Customer VAT No</td>
                                        <td>
                                            <asp:TextBox ID="accCustomerVATNoTextBox" runat="server" Width="15em" /></td>
                                    </tr>
                                    <tr>
                                        <td>Invoice Type</td>
                                        <td>
                                            <asp:DropDownList ID="accInvoiceTypesDropDownList" runat="server" DataSourceID="odsInvoiceTypes" DataTextField="InvoiceTypeDesc"
                                                DataValueField="InvoiceTypeID" AppendDataBoundItems="true">
                                                <asp:ListItem Text="-select-" Value="0" />
                                            </asp:DropDownList>
                                        </td>
                                        <td colspan="2" align="center">&nbsp;&nbsp;<asp:CheckBox ID="accRequiresPurchOrderCheckBox" runat="server" Text="Requires Purchurse Order" TextAlign="Left" />
                                            &nbsp;&nbsp;&nbsp;&nbsp;<asp:CheckBox ID="accEnabledCheckBox" runat="server" Text="Account Enabled" TextAlign="Left" /></td>
                                    </tr>
                                    <tr>
                                        <td>Billing Address</td>
                                        <td>1:<asp:TextBox ID="accBillAddr1TextBox" runat="server" Width="20em" /><br />
                                            2:<asp:TextBox ID="accBillAddr2TextBox" runat="server" Width="20em" /><br />
                                            3:<asp:TextBox ID="accBillAddr3TextBox" runat="server" Width="20em" /><br />
                                            4:<asp:TextBox ID="accBillAddr4TextBox" runat="server" Width="20em" /><br />
                                            5:<asp:TextBox ID="accBillAddr5TextBox" runat="server" Width="15em" /></td>
                                        <td>Shipping Address</td>
                                        <td>1:<asp:TextBox ID="accShipAddr1TextBox" runat="server" Width="20em" /><br />
                                            2:<asp:TextBox ID="accShipAddr2TextBox" runat="server" Width="20em" /><br />
                                            3:<asp:TextBox ID="accShipAddr3TextBox" runat="server" Width="20em" /><br />
                                            4:<asp:TextBox ID="accShipAddr4TextBox" runat="server" Width="20em" /><br />
                                            5:<asp:TextBox ID="accShipAddr5TextBox" runat="server" Width="15em" /></td>
                                    </tr>
                                    <tr>
                                        <td>Accounts First Name</td>
                                        <td>
                                            <asp:TextBox ID="accFirstNameTextBox" runat="server" Width="20em" /></td>
                                        <td>Accounts Last Name</td>
                                        <td>
                                            <asp:TextBox ID="accLastNameTextBox" runat="server" Width="20em" /></td>
                                    </tr>
                                    <tr>
                                        <td>Accounts Email</td>
                                        <td colspan="3">
                                            <asp:TextBox ID="accAccEmailTextBox" runat="server" Width="30em" /></td>
                                    </tr>
                                    <tr>
                                        <td>Accounts CC FirstName</td>
                                        <td>
                                            <asp:TextBox ID="accAltFirstNameTextBox" runat="server" Width="20em" /></td>
                                        <td>Accounts CC LastName</td>
                                        <td>
                                            <asp:TextBox ID="accAltLastNameTextBox" runat="server" Width="20em" /></td>
                                    </tr>
                                    <tr>
                                        <td>Accounts CC Email</td>
                                        <td colspan="3">
                                            <asp:TextBox ID="accAltEmailTextBox" runat="server" Width="30em" /></td>
                                    </tr>
                                    <tr>
                                        <td>Payment Terms</td>
                                        <td>
                                            <asp:DropDownList ID="accPaymentTermsDropDownList" runat="server" DataSourceID="odsPaymentTerms"
                                                DataTextField="PaymentTermDesc" DataValueField="PaymentTermID" AppendDataBoundItems="true"
                                                OnDataBound="accPaymentTermsDropDownList_DataBound">
                                                <asp:ListItem Text="-select-" Value="0" />
                                            </asp:DropDownList>
                                        </td>
                                        <td>Price Level</td>
                                        <td>
                                            <asp:DropDownList ID="accPriceLevelsDropDownList" runat="server" DataSourceID="odsPriceLevels"
                                                DataTextField="PriceLevelDesc" DataValueField="PriceLevelID" AppendDataBoundItems="true">
                                                <asp:ListItem Text="-select-" Value="" />
                                            </asp:DropDownList>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>Registration No.</td>
                                        <td>
                                            <asp:TextBox ID="accRegNoTextBox" runat="server" Width="15em" /></td>
                                        <td>Limit</td>
                                        <td>
                                            <asp:TextBox ID="accLimitTextBox" runat="server" Width="15em" /></td>
                                    </tr>
                                    <tr>
                                        <td>Bank Account No.</td>
                                        <td>
                                            <asp:TextBox ID="accBankAccNoTextBox" runat="server" Width="10em" /></td>
                                        <td>Bank Branch</td>
                                        <td>
                                            <asp:TextBox ID="accBankBranchTextBox" runat="server" Width="10em" /></td>
                                    </tr>
                                    <tr>
                                        <td>Accounts Notes</td>
                                        <td colspan="3">
                                            <asp:TextBox ID="accNotesTextBox" runat="server" Width="40em" TextMode="MultiLine" />
                                            <asp:Label ID="CustomersAccInfoIDLabel" runat="server" CssClass="small" />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan="4" class="horizMiddle ">
                                            <asp:Button ID="accAddDetailsButton" runat="server" Text="Add Account Details" OnClick="accAddDetailsButton_Click" Enabled="false" />
                                            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                                            <asp:Button ID="accUpdateButton" runat="server" Text="Update Account Details" OnClick="accUpdateButton_Click" Enabled="false" />
                                        </td>
                                    </tr>
                                </table>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </ContentTemplate>
                </ajaxToolkit:TabPanel>
                <ajaxToolkit:TabPanel runat="server" HeaderText="Next Required" ID="tabpnlNextRequired">
                    <HeaderTemplate>Next Items Required</HeaderTemplate>
                    <ContentTemplate>
                        <asp:UpdatePanel ID="upnlNextItems" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <div style="padding: 4px">
                                    <asp:DataGrid ID="dgCustomerUsage" runat="server" CssClass="TblWhite small" HeaderStyle-BackColor="Khaki" DataSourceID="dsCustomerUsage" AutoGenerateColumns="false">
                                        <Columns>
                                            <asp:BoundColumn DataField="CustomerID" Visible="false" />
                                            <asp:BoundColumn DataField="LastCupCount" HeaderText="Last Count" ItemStyle-HorizontalAlign="Right" />
                                            <asp:BoundColumn DataField="NextCoffeeBy" HeaderText="Next Coffee By" DataFormatString="{0:d}" />
                                            <asp:BoundColumn DataField="DailyConsumption" HeaderText="Ave" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:0.##}" />
                                            <asp:BoundColumn DataField="NextCleanOn" HeaderText="Next Clean Est" DataFormatString="{0:d}" />
                                            <asp:BoundColumn DataField="CleanAveCount" HeaderText="Clean Ave" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:0.##}" />
                                            <asp:BoundColumn DataField="NextFilterEst" HeaderText="Next Filter" DataFormatString="{0:d}" />
                                            <asp:BoundColumn DataField="FilterAveCount" HeaderText="Filter Ave" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:0.##}" />
                                            <asp:BoundColumn DataField="NextDescaleEst" HeaderText="Next Descale" DataFormatString="{0:d}" />
                                            <asp:BoundColumn DataField="DescaleAveCount" HeaderText="Descale Ave" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:0.##}" />
                                            <asp:BoundColumn DataField="NextServiceEst" HeaderText="Next Service" DataFormatString="{0:d}" />
                                            <asp:BoundColumn DataField="ServiceAveCount" HeaderText="Service Ave" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:0.##}" />
                                        </Columns>
                                    </asp:DataGrid>
                                </div>
                            </ContentTemplate>
                            <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="btnForceNext" EventName="Click" />
                            </Triggers>
                        </asp:UpdatePanel>
                    </ContentTemplate>
                </ajaxToolkit:TabPanel>
                <ajaxToolkit:TabPanel runat="server" HeaderText="Items" ID="tabpnlItems">
                    <HeaderTemplate>Customer Usage</HeaderTemplate>
                    <ContentTemplate>
                        <asp:UpdateProgress ID="updtprgItems" runat="server" AssociatedUpdatePanelID="upnlItems">
                            <ProgressTemplate>
                                <img src="../images/animi/BlueArrowsUpdate.gif" alt="updating" width="16" height="16" />please wait.....
                            </ProgressTemplate>
                        </asp:UpdateProgress>
                        <asp:UpdatePanel ID="upnlItems" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <div class="simpleForm" style="padding: 4px">
                                    <div class="simpleLightGreenForm">
                                        <asp:GridView ID="gvItems" runat="server" AllowSorting="True" CssClass="TblWhite small" EmptyDataText="no data yet"
                                            AutoGenerateColumns="False" DataSourceID="odsItemUsage" AllowPaging="True" PagerSettings-Mode="NextPreviousFirstLast"
                                            DataKeyNames="ClientUsageLineNo" OnRowCommand="gvItems_RowCommand"
                                            PageSize="15" PagerSettings-FirstPageImageUrl="~/images/imgButtons/FirstPage.gif"
                                            PagerSettings-LastPageImageUrl="~/images/imgButtons/LastPage.gif"
                                            PagerSettings-NextPageImageUrl="~/images/imgButtons/NextPage.gif" PagerSettings-PreviousPageImageUrl="~/images/imgButtons/LastPage.gif">
                                            <Columns>
                                                <asp:TemplateField ShowHeader="False">
                                                    <EditItemTemplate>
                                                        <asp:ImageButton ID="UpdateButton" runat="server" CausesValidation="True" CommandName="Update"
                                                            AlternateText="Update" ImageUrl="~/images/imgButtons/UpdateItem.gif"
                                                            CommandArgument='<%# ((GridViewRow) Container).RowIndex %>' />
                                                        &nbsp;<asp:ImageButton ID="CancelButton" runat="server" CausesValidation="False" CommandName="Cancel"
                                                            AlternateText="Cancel" ImageUrl="~/images/imgButtons/CancelItem.gif" />
                                                    </EditItemTemplate>
                                                    <ItemTemplate>
                                                        <asp:ImageButton ID="EditButton" runat="server" CausesValidation="False" CommandName="Edit"
                                                            AlternateText="Edit" ImageUrl="~/images/imgButtons/EditItem.gif" />&nbsp;
                                                        <asp:ImageButton ID="DeleteButton" runat="server" CausesValidation="False" CommandName="Delete"
                                                            AlternateText="Delete" ImageUrl="~/images/imgButtons/DelItem.gif"
                                                            OnClientClick="return confirm('Are you sure you want to delete this item?');"
                                                            CommandArgument='<%# Eval("ClientUsageLineNo") %>' />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Date" SortExpression="ItemDate">
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="tbxItemDate" runat="server" Text='<%# Bind("ItemDate", "{0:d}") %>' Width="7em" />
                                                    </EditItemTemplate>
                                                    <ItemTemplate>
                                                        <asp:Label ID="lblItemDate" runat="server" Text='<%# Bind("ItemDate", "{0:d}") %>' />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Item">
                                                    <EditItemTemplate>
                                                        <asp:DropDownList ID="ddlItems" runat="server" DataSourceID="odsItems"
                                                            DataTextField="ItemDesc" DataValueField="ItemTypeID" AppendDataBoundItems="true"
                                                            SelectedValue='<%# string.IsNullOrEmpty(Convert.ToString(Eval("ItemProvidedID"))) ? "0" : Eval("ItemProvidedID") %>'>
                                                            <asp:ListItem Value="0" Text="n/a" />
                                                        </asp:DropDownList>
                                                    </EditItemTemplate>
                                                    <ItemTemplate>
                                                        <asp:Label ID="ItemDescLabel" runat="server" Text='<%# GetItemDesc((int)Eval("ItemProvidedID")) %>' />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Qty" SortExpression="AmountProvided">
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="tbxAmountProvided" runat="server" Text='<%# Bind("AmountProvided","{0:0.###}") %>' Width="3em" />
                                                    </EditItemTemplate>
                                                    <ItemTemplate>
                                                        <asp:Label ID="lblAmountProvided" runat="server" Text='<%# Eval("AmountProvided","{0:0.###}") %>' />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Packaging" SortExpression="PackagingID">
                                                    <EditItemTemplate>
                                                        <asp:DropDownList ID="ddlPackaging" runat="server" DataSourceID="odsPackagingTypes"
                                                            DataTextField="Description" DataValueField="PackagingID" AppendDataBoundItems="true"
                                                            SelectedValue='<%# string.IsNullOrEmpty(Convert.ToString(Eval("PackagingID"))) ? "0" : Eval("PackagingID") %>'>
                                                            <asp:ListItem Value="0" Text="n/a" />
                                                        </asp:DropDownList>
                                                    </EditItemTemplate>
                                                    <ItemTemplate>
                                                        <asp:Label ID="PackagingIDLabel" runat="server" Text='<%# GetPackagingDesc((int)Eval("PackagingID")) %>'></asp:Label>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Prep Type" SortExpression="PrepTypeID">
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="tbxPrepTypeID" runat="server" Text='<%# Bind("PrepTypeID") %>' Width="3em" />
                                                    </EditItemTemplate>
                                                    <ItemTemplate>
                                                        <asp:Label ID="lblPrepTypeID" runat="server" Text='<%# Bind("PrepTypeID") %>' />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Notes" SortExpression="Notes">
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="tbxNotes" runat="server" Text='<%# Bind("Notes") %>' TextMode="MultiLine" />
                                                    </EditItemTemplate>
                                                    <ItemTemplate>
                                                        <asp:Label ID="lblNotes" runat="server" Text='<%# Bind("Notes") %>' />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="ClientUsageLineNo" SortExpression="ClientUsageLineNo" ControlStyle-Font-Size="XX-Small">
                                                    <EditItemTemplate>
                                                        <asp:Label ID="lblClientUsageLineNo" runat="server" Text='<%# Bind("ClientUsageLineNo") %>' />
                                                    </EditItemTemplate>
                                                    <ItemTemplate>
                                                        <asp:Label ID="lblClientUsageLineNo" runat="server" Text='<%# Bind("ClientUsageLineNo") %>' />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                            </Columns>
                                            <PagerSettings FirstPageImageUrl="~/images/imgButtons/FirstPage.gif" LastPageImageUrl="~/images/imgButtons/LastPage.gif" Mode="NumericFirstLast" NextPageImageUrl="~/images/imgButtons/NextPage.gif" PreviousPageImageUrl="~/images/imgButtons/LastPage.gif" />
                                        </asp:GridView>
                                    </div>
                                </div>
                                <asp:ObjectDataSource ID="odsItemUsage" runat="server" TypeName="TrackerDotNet.Controls.ItemUsageTbl"
                                    SortParameterName="SortBy"
                                    SelectMethod="GetAllItemsUsed"
                                    OldValuesParameterFormatString="original_{0}" UpdateMethod="UpdateItemsUsed" DeleteMethod="DeleteItemLine">
                                    <DeleteParameters>
                                        <asp:Parameter Name="ClientUsageLineNo" Type="Int64" />
                                    </DeleteParameters>
                                    <SelectParameters>
                                        <asp:ControlParameter ControlID="CompanyIDLabel" DefaultValue="0"
                                            Name="pCustomerID" PropertyName="Text" Type="Int64" />
                                        <asp:Parameter DefaultValue="&quot;Date&quot;" Name="SortBy" Type="String" />
                                    </SelectParameters>
                                    <UpdateParameters>
                                        <asp:Parameter Name="CustomerID" Type="Int64" />
                                        <asp:Parameter Name="ItemDate" Type="DateTime" />
                                        <asp:Parameter Name="ItemProvidedID" Type="Int32" />
                                        <asp:Parameter Name="AmountProvided" Type="Double" />
                                        <asp:Parameter Name="PrepTypeID" Type="Int32" />
                                        <asp:Parameter Name="PackagingID" Type="Int32" />
                                        <asp:Parameter Name="Notes" Type="String" />
                                        <asp:Parameter Name="original_ClientUsageLineNo" Type="Int64" />
                                    </UpdateParameters>
                                </asp:ObjectDataSource>

                            </ContentTemplate>
                            <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="gvItems" EventName="RowCommand" />
                            </Triggers>
                        </asp:UpdatePanel>
                    </ContentTemplate>
                </ajaxToolkit:TabPanel>

            </ajaxToolkit:TabContainer>
        </ContentTemplate>
    </asp:UpdatePanel>
    <asp:ObjectDataSource ID="odsCities" runat="server" TypeName="TrackerDotNet.Controls.CityTblDAL"
        SortParameterName="SortBy" SelectMethod="GetAllCityTblData"
        OldValuesParameterFormatString="original_{0}">
        <SelectParameters>
            <asp:Parameter DefaultValue="City" Name="SortBy" Type="String" />
        </SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsItems" runat="server" TypeName="TrackerDotNet.Controls.ItemTypeTbl"
        SortParameterName="SortBy" SelectMethod="GetAll"
        OldValuesParameterFormatString="original_{0}">
        <SelectParameters>
            <asp:Parameter DefaultValue="ItemDesc" Name="SortBy" Type="String" />
        </SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsEquipTypes" runat="server" TypeName="TrackerDotNet.Controls.EquipTypeTbl"
        SortParameterName="SortBy" SelectMethod="GetAll"
        OldValuesParameterFormatString="original_{0}">
        <SelectParameters>
            <asp:Parameter DefaultValue="EquipTypeName" Name="SortBy" Type="String" />
        </SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsCustomerTypes" runat="server" TypeName="TrackerDotNet.Controls.CustomerTypeTbl"
        SortParameterName="SortBy" SelectMethod="GetAll"
        OldValuesParameterFormatString="original_{0}">
        <SelectParameters>
            <asp:Parameter DefaultValue="CustTypeDesc" Name="SortBy" Type="String" />
        </SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsPersons" runat="server" TypeName="TrackerDotNet.Controls.PersonsTbl"
        SortParameterName="SortBy" SelectMethod="GetAll"
        OldValuesParameterFormatString="original_{0}">
        <SelectParameters>
            <asp:Parameter DefaultValue="Abreviation" Name="SortBy" Type="String" />
        </SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsPackagingTypes" runat="server" TypeName="TrackerDotNet.Controls.PackagingTbl"
        SortParameterName="SortBy" SelectMethod="GetAll"
        OldValuesParameterFormatString="original_{0}">
        <SelectParameters>
            <asp:Parameter DefaultValue="Description" Name="SortBy" Type="String" />
        </SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsInvoiceTypes" runat="server" DataObjectTypeName="TrackerDotNet.Controls.InvoiceTypeTbl" DeleteMethod="Delete" InsertMethod="Insert" SelectMethod="GetAll" TypeName="TrackerDotNet.Controls.InvoiceTypeTbl" UpdateMethod="Update">
        <SelectParameters>
            <asp:Parameter DefaultValue="InvoiceTypeDesc" Name="SortBy" Type="String" />
        </SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsPaymentTerms" runat="server" DataObjectTypeName="TrackerDotNet.Controls.PaymentTermsTbl" DeleteMethod="Delete" InsertMethod="Insert" SelectMethod="GetAll" TypeName="TrackerDotNet.Controls.PaymentTermsTbl" UpdateMethod="Update">
        <SelectParameters>
            <asp:Parameter DefaultValue="PaymentTermDesc" Name="SortBy" Type="String" />
        </SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsPriceLevels" runat="server" DataObjectTypeName="TrackerDotNet.Controls.PriceLevelsTbl" DeleteMethod="Delete" InsertMethod="Insert" SelectMethod="GetAll" TypeName="TrackerDotNet.Controls.PriceLevelsTbl" UpdateMethod="Update">
        <SelectParameters>
            <asp:Parameter DefaultValue="PriceLevelDesc" Name="SortBy" Type="String" />
        </SelectParameters>
    </asp:ObjectDataSource>

    <!--
  <asp:ObjectDataSource ID="dsCustomerUsage" runat="server" TypeName="TrackerDotNet.Controls.ClientUsageTbl"
      SelectMethod="GetUsageData" 
      OldValuesParameterFormatString="original_{0}" >
      <SelectParameters>
        <asp:ControlParameter ControlID="CompanyIDLabel" DefaultValue="0" 
          Name="pCustomerID" PropertyName="Text" Type="Int64" />
      </SelectParameters>
  </asp:ObjectDataSource>
-->
</asp:Content>

