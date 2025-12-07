<%@ Page Title="Contact Details" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" MaintainScrollPositionOnPostback="true"
    CodeBehind="ContactDetails.aspx.cs" Inherits="TrackerSQL.Pages.ContactDetails" %>

<asp:Content ID="cntContactDetailsHdr" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        function redirect(url) {
            alert("Contact Added");
            window.location = url;
        }
        function showMessage(thisMessage) {
            alert(thisMessage);
        }
    </script>
</asp:Content>
<asp:Content ID="cntContactDetailsBdy" ContentPlaceHolderID="MainContent" runat="server">
    <h2 class="InputFrm">Contact Details</h2>
    <asp:Label ID="lblContactID" Visible="false" runat="server" />
    <asp:ScriptManager ID="smContactDetails" runat="server"></asp:ScriptManager>
    <asp:UpdateProgress ID="uprgContactDetails" runat="server" AssociatedUpdatePanelID="upnlContactDetails">
        <ProgressTemplate>
            <img src="../images/animi/BlueArrowsUpdate.gif" alt="updating" width="16" height="16" />updating.....
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:UpdatePanel ID="upnlContactDetails" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnForceNext" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnForceCheckup" EventName="Click" />
        </Triggers>
        <ContentTemplate>
            <!-- Original form layout restored -->
            <table class="TblMudZebra" cellpadding="0" cellspacing="0">
                <tr>
                    <td>Company Name</td>
                    <td colspan="4"><asp:TextBox ID="CompanyNameTextBox" runat="server" Width="33em" /></td>
                    <td><asp:Label ID="CompanyIDLabel" runat="server" /></td>
                </tr>
                <tr>
                    <td>First Name</td>
                    <td><asp:TextBox ID="ContactFirstNameTextBox" runat="server" /></td>
                    <td>Last Name</td>
                    <td><asp:TextBox ID="ContactLastNameTextBox" runat="server" /></td>
                    <td>Title</td>
                    <td><asp:TextBox ID="ContactTitleTextBox" runat="server" Width="3em" /></td>
                </tr>
                <tr>
                    <td>Alt First Name</td>
                    <td><asp:TextBox ID="ContactAltFirstNameTextBox" runat="server" /></td>
                    <td>Alt Last Name</td>
                    <td><asp:TextBox ID="ContactAltLastNameTextBox" runat="server" /></td>
                    <td>&nbsp;</td><td>&nbsp;</td>
                </tr>
                <tr>
                    <td>Address</td>
                    <td colspan="3"><asp:TextBox ID="BillingAddressTextBox" runat="server" Width="33em" /></td>
                    <td>Department</td>
                    <td><asp:TextBox ID="DepartmentTextBox" runat="server" /></td>
                </tr>
                <tr>
                    <td>Post Code</td>
                    <td><asp:TextBox ID="PostalCodeTextBox" runat="server" /></td>
                    <td>Delivery Area</td>
                    <td>
                        <asp:DropDownList ID="ddlAreas" runat="server" AppendDataBoundItems="True" DataSourceID="odsAreas" DataTextField="AreaName" DataValueField="AreaID">
                            <asp:ListItem Value="0" Text="-please select-" />
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidatorAreas" runat="server"
                            ErrorMessage="Please select an area" ControlToValidate="ddlAreas" InitialValue="0" />
                    </td>
                    <td>Prov</td>
                    <td><asp:TextBox ID="ProvinceTextBox" runat="server" /></td>
                </tr>
                <tr>
                    <td>Phone</td>
                    <td><asp:TextBox ID="PhoneNumberTextBox" runat="server" /></td>
                    <td>Cell</td>
                    <td><asp:TextBox ID="CellNumberTextBox" runat="server" /></td>
                    <td>Fax</td>
                    <td><asp:TextBox ID="FaxNumberTextBox" runat="server" /></td>
                </tr>
                <tr>
                    <td>Email</td>
                    <td colspan="2"><asp:TextBox ID="EmailAddressTextBox" runat="server" Width="25em" /></td>
                    <td>Alt Email Addr</td>
                    <td colspan="2"><asp:TextBox ID="AltEmailAddressTextBox" runat="server" Width="25em" /></td>
                </tr>
                <tr>
                    <td>Contact Type</td>
                    <td>
                        <asp:DropDownList ID="ddlContactTypes" runat="server" DataSourceID="odsContactTypes" DataTextField="ContactTypeDesc" DataValueField="ContactTypeID" AppendDataBoundItems="true">
                            <asp:ListItem Value="0" Text="-please select-" />
                        </asp:DropDownList>
                    </td>
                    <td>Equip Type</td>
                    <td>
                        <asp:DropDownList ID="ddlEquipTypes" runat="server" AppendDataBoundItems="true" DataSourceID="odsEquipTypes" DataTextField="EquipTypeName" DataValueField="EquipTypeId">
                            <asp:ListItem Text="none" Value="0" />
                        </asp:DropDownList>
                    </td>
                    <td>S/N</td>
                    <td><asp:TextBox ID="MachineSNTextBox" runat="server" /></td>
                </tr>
                <tr>
                    <td>1st Preference</td>
                    <td>
                        <asp:DropDownList ID="ddlFirstPreference" runat="server" AppendDataBoundItems="true" DataSourceID="odsItems" DataTextField="ItemDesc" DataValueField="ItemID">
                            <asp:ListItem Text="none" Value="0" />
                        </asp:DropDownList>
                    </td>
                    <td>Pri Pref Qty</td>
                    <td><asp:TextBox ID="PriPrefQtyTextBox" runat="server" /></td>
                    <td>Packaging</td>
                    <td>
                        <asp:DropDownList ID="ddlItemPackagingTypes" runat="server" AppendDataBoundItems="true" DataSourceID="odsItemPackagingTypes" DataTextField="ItemPackagingDesc" DataValueField="ItemPackagingID">
                            <asp:ListItem Text="none" Value="0" />
                        </asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td>Delivery By</td>
                    <td>
                        <asp:DropDownList ID="ddlDeliveryBy" runat="server" AppendDataBoundItems="true" DataSourceID="odsPersons" DataTextField="Abbreviation" DataValueField="PersonID">
                            <asp:ListItem Text="- ? -" Value="0" />
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator ID="ddlDeliveryByRequiredFieldValidator" runat="server" ErrorMessage="Please select who will deliver" ControlToValidate="ddlDeliveryBy" InitialValue="0" />
                    </td>
                    <td>Agent</td>
                    <td>
                        <asp:DropDownList ID="ddlAgent" runat="server" AppendDataBoundItems="true" DataSourceID="odsPersons" DataTextField="Abbreviation" DataValueField="PersonID">
                            <asp:ListItem Text="none" Value="0" />
                        </asp:DropDownList>
                    </td>
                    <td>Reminders: [<asp:Label ID="ReminderCountLabel" runat="server" />]</td>
                    <td>LastReminderSent:<asp:Label ID="LastReminderLabel" runat="server" /></td>
                </tr>
                <tr>
                    <td>Uses/Enabled/Filters</td>
                    <td colspan="5">
                        <asp:CheckBox ID="enabledCheckBox" runat="server" Text="enabled" TextAlign="Right" />
                        <asp:CheckBox ID="autofulfillCheckBox" runat="server" Text="auto fulfill" TextAlign="Right" />
                        <asp:CheckBox ID="UsesFilterCheckBox" runat="server" Text="Uses Filter" TextAlign="Right" />
                        <asp:CheckBox ID="PredictionDisabledCheckBox" runat="server" Text="PredictionDisabled" TextAlign="Right" />
                        <asp:CheckBox ID="AlwaysSendChkUpCheckBox" runat="server" Text="AlwaysSendChkUp" TextAlign="Right" />
                        <asp:CheckBox ID="NormallyRespondsCheckBox" runat="server" Text="NormallyResponds" TextAlign="Right" />
                    </td>
                </tr>
                <tr>
                    <td>Notes</td>
                    <td colspan="5"><asp:TextBox ID="NotesTextBox" runat="server" TextMode="MultiLine" Height="4em" Width="93%" /></td>
                </tr>
                <tr>
                    <td colspan="6" class="rowOddC">
                        <asp:Button ID="btnUpdate" Text="Update" runat="server" OnClick="btnUpdate_Click" />
                        <asp:Button ID="btnUpdateAndReturn" Text="Update & Return" runat="server" OnClick="btnUpdateAndReturn_Click" />
                        <asp:Button ID="btnInsert" Text="Insert" runat="server" OnClick="btnInsert_Click" />
                        <asp:Button ID="btnCopy2AccInfo" Text="Copy2Acc" runat="server" OnClick="btnCopy2AccInfo_Click" />
                        <asp:Button ID="btnAddLasOrder" Text="Add Last" runat="server" OnClick="btnAddLasOrder_Click" />
                        <asp:Button ID="btnForceNext" Text="Force Next" runat="server" OnClick="btnForceNext_Click" />
                        <asp:Button ID="btnForceCheckup" Text="Force Checkup" runat="server" OnClick="btnForceCheckup_Click" />
                        <asp:Button ID="btnRecalcAverage" Text="Recalc Ave" runat="server" OnClick="btnRecalcAverage_Click" />
                        <asp:Button ID="btnCancel" Text="Return" runat="server" OnClick="btnCancel_Click" />
                    </td>
                </tr>
            </table>
        </ContentTemplate>
    </asp:UpdatePanel>
    <div class="status-message"><asp:Literal ID="ltrlStatus" Text="" runat="server" /></div>
    <br />
    <asp:UpdatePanel ID="uppnlTabContainer" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <ajaxToolkit:TabContainer ID="tabcContact" runat="server" AutoPostBack="true" OnActiveTabChanged="tabcContact_OnActiveTabChanged">
                <ajaxToolkit:TabPanel ID="tabpnlAccountInfo" runat="server" HeaderText="Accounts Info">
                    <ContentTemplate>
                        <asp:UpdatePanel ID="dvContactsAccInfoUpdatePanel" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
                            <ContentTemplate>
                                <table class="TblCoffee" cellpadding="0" cellspacing="0">
                                    <tr>
                                        <td>Acc Company Name</td>
                                        <td><asp:TextBox ID="accFullCoNameTextBox" runat="server" Width="20em" /></td>
                                        <td>Contact VAT No</td>
                                        <td><asp:TextBox ID="accContactVATNoTextBox" runat="server" Width="15em" /></td>
                                    </tr>
                                    <tr>
                                        <td>Invoice Type</td>
                                        <td>
                                            <asp:DropDownList ID="accInvoiceTypesDropDownList" runat="server" DataSourceID="odsInvoiceTypes" DataTextField="InvoiceTypeDesc" DataValueField="InvoiceTypeID" AppendDataBoundItems="true">
                                                <asp:ListItem Text="-select-" Value="0" />
                                            </asp:DropDownList>
                                        </td>
                                        <td colspan="2" align="center">
                                            <asp:CheckBox ID="accRequiresPurchOrderCheckBox" runat="server" Text="Requires Purchase Order" />
                                            <asp:CheckBox ID="accEnabledCheckBox" runat="server" Text="Account Enabled" />
                                        </td>
                                    </tr>
                                    <!-- Billing / Shipping etc (restored) -->
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
                                        <td><asp:TextBox ID="accFirstNameTextBox" runat="server" Width="20em" /></td>
                                        <td>Accounts Last Name</td>
                                        <td><asp:TextBox ID="accLastNameTextBox" runat="server" Width="20em" /></td>
                                    </tr>
                                    <tr>
                                        <td>Accounts Email</td>
                                        <td>Accounts Email</td>
                                        <td colspan="3"><asp:TextBox ID="accAccEmailTextBox" runat="server" Width="30em" /></td>
                                    </tr>
                                    <tr>
                                        <td>Accounts CC FirstName</td>
                                        <td><asp:TextBox ID="accAltFirstNameTextBox" runat="server" Width="20em" /></td>
                                        <td>Accounts CC LastName</td>
                                        <td><asp:TextBox ID="accAltLastNameTextBox" runat="server" Width="20em" /></td>
                                    </tr>
                                    <tr>
                                        <td>Accounts CC Email</td>
                                        <td colspan="3"><asp:TextBox ID="accAltEmailTextBox" runat="server" Width="30em" /></td>
                                    </tr>
                                    <tr>
                                        <td>Payment Terms</td>
                                        <td>
                                            <asp:DropDownList ID="accPaymentTermsDropDownList" runat="server" DataSourceID="odsPaymentTerms" DataTextField="PaymentTermDesc" DataValueField="PaymentTermID" AppendDataBoundItems="true" OnDataBound="accPaymentTermsDropDownList_DataBound">
                                                <asp:ListItem Text="-select-" Value="0" />
                                            </asp:DropDownList>
                                        </td>
                                        <td>Price Level</td>
                                        <td>
                                            <asp:DropDownList ID="accPriceLevelsDropDownList" runat="server" DataSourceID="odsPriceLevels" DataTextField="PriceLevelDesc" DataValueField="PriceLevelID" AppendDataBoundItems="true">
                                                <asp:ListItem Text="-select-" Value="" />
                                            </asp:DropDownList>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>Registration No.</td>
                                        <td><asp:TextBox ID="accRegNoTextBox" runat="server" Width="15em" /></td>
                                        <td>Limit</td>
                                        <td><asp:TextBox ID="accLimitTextBox" runat="server" Width="15em" /></td>
                                    </tr>
                                    <tr>
                                        <td>Bank Account No.</td>
                                        <td><asp:TextBox ID="accBankAccNoTextBox" runat="server" Width="10em" /></td>
                                        <td>Bank Branch</td>
                                        <td><asp:TextBox ID="accBankBranchTextBox" runat="server" Width="10em" /></td>
                                    </tr>
                                    <tr>
                                        <td>Accounts Notes</td>
                                        <td colspan="3"><asp:TextBox ID="accNotesTextBox" runat="server" Width="40em" TextMode="MultiLine" /><asp:Label ID="ContactsAccInfoIDLabel" runat="server" CssClass="small" /></td>
                                    </tr>
                                    <tr>
                                        <td colspan="4" class="horizMiddle ">
                                            <asp:Button ID="accAddDetailsButton" runat="server" Text="Add Account Details" OnClick="accAddDetailsButton_Click" />
                                            <asp:Button ID="accUpdateButton" runat="server" Text="Update Account Details" OnClick="accUpdateButton_Click" />
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
                                <div style="padding:4px">
                                    <asp:GridView ID="gvPrediction" runat="server" CssClass="TblWhite small" AutoGenerateColumns="False">
                                        <Columns>
                                            <asp:BoundField DataField="LastCupCount" HeaderText="Last Cups" />
                                            <asp:BoundField DataField="NextCoffeeBy" HeaderText="Next Coffee" DataFormatString="{0:d}" />
                                            <asp:BoundField DataField="NextCleanOn" HeaderText="Next Clean" DataFormatString="{0:d}" />
                                            <asp:BoundField DataField="NextFilterEst" HeaderText="Next Filter" DataFormatString="{0:d}" />
                                            <asp:BoundField DataField="NextDescaleEst" HeaderText="Next Descale" DataFormatString="{0:d}" />
                                            <asp:BoundField DataField="NextServiceEst" HeaderText="Next Service" DataFormatString="{0:d}" />
                                            <asp:BoundField DataField="DailyConsumption" HeaderText="Daily Cons" DataFormatString="{0:0.###}" />
                                        </Columns>
                                    </asp:GridView>
                                </div>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </ContentTemplate>
                </ajaxToolkit:TabPanel>
                <ajaxToolkit:TabPanel runat="server" HeaderText="Items" ID="tabpnlItems">
                    <HeaderTemplate>Contact Usage</HeaderTemplate>
                    <ContentTemplate>
                        <asp:UpdatePanel ID="upnlItems" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <asp:GridView ID="gvContactItems" runat="server" AllowSorting="True" CssClass="TblWhite small" EmptyDataText="no data yet"
                                    AutoGenerateColumns="False" AllowPaging="True" PageSize="15" OnRowCommand="gvItems_RowCommand" DataKeyNames="ClientUsageLineNo">
                                    <Columns>
                                        <asp:BoundField DataField="ClientUsageLineNo" HeaderText="#" />
                                        <asp:BoundField DataField="ItemDate" HeaderText="Date" DataFormatString="{0:d}" />
                                        <asp:BoundField DataField="ItemProvided" HeaderText="Item" />
                                        <asp:BoundField DataField="AmountProvided" HeaderText="Qty" DataFormatString="{0:0.###}" />
                                        <asp:BoundField DataField="PrepType" HeaderText="PrepType" />
                                        <asp:BoundField DataField="Packaging" HeaderText="Packaging" />
                                        <asp:BoundField DataField="Notes" HeaderText="Notes" />
                                    </Columns>
                                </asp:GridView>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </ContentTemplate>
                </ajaxToolkit:TabPanel>
            </ajaxToolkit:TabContainer>
        </ContentTemplate>
    </asp:UpdatePanel>

    <asp:ObjectDataSource ID="odsAreas" runat="server" TypeName="TrackerDotNet.Classes.Sql.AreasRepository" SelectMethod="GetAll">
        <SelectParameters><asp:Parameter DefaultValue="AreaName" Name="SortBy" Type="String" /></SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsItems" runat="server" TypeName="TrackerDotNet.Classes.Sql.ItemsRepository" SelectMethod="GetAll">
        <SelectParameters><asp:Parameter DefaultValue="ItemDesc" Name="SortBy" Type="String" /></SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsEquipTypes" runat="server" TypeName="TrackerDotNet.Classes.Sql.EquipTypesRepository" SelectMethod="GetAll">
        <SelectParameters><asp:Parameter DefaultValue="EquipTypeName" Name="SortBy" Type="String" /></SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsContactTypes" runat="server" TypeName="TrackerDotNet.Classes.Sql.ContactTypesRepository" SelectMethod="GetAll">
        <SelectParameters><asp:Parameter DefaultValue="ContactTypeDesc" Name="SortBy" Type="String" /></SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsPersons" runat="server" TypeName="TrackerDotNet.Classes.Sql.PersonsRepository" SelectMethod="GetAll">
        <SelectParameters><asp:Parameter DefaultValue="Abbreviation" Name="SortBy" Type="String" /></SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsItemPackagingTypes" runat="server" TypeName="TrackerDotNet.Classes.Sql.ItemPackagingsRepository" SelectMethod="GetAll">
        <SelectParameters><asp:Parameter DefaultValue="ItemPackagingDesc" Name="SortBy" Type="String" /></SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsInvoiceTypes" runat="server" TypeName="TrackerDotNet.Classes.Sql.InvoiceTypesRepository" SelectMethod="GetAll">
        <SelectParameters><asp:Parameter DefaultValue="InvoiceTypeDesc" Name="SortBy" Type="String" /></SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsPaymentTerms" runat="server" TypeName="TrackerDotNet.Classes.Sql.PaymentTermsRepository" SelectMethod="GetAll">
        <SelectParameters><asp:Parameter DefaultValue="PaymentTermDesc" Name="SortBy" Type="String" /></SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsPriceLevels" runat="server" TypeName="TrackerDotNet.Classes.Sql.PriceLevelsRepository" SelectMethod="GetAll">
        <SelectParameters><asp:Parameter DefaultValue="PriceLevelDesc" Name="SortBy" Type="String" /></SelectParameters>
    </asp:ObjectDataSource>
</asp:Content>
