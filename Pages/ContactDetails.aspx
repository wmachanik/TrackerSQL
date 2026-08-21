<%@ Page Title="Contact Details" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" MaintainScrollPositionOnPostback="true"
    CodeBehind="ContactDetails.aspx.cs" Inherits="TrackerSQL.Pages.ContactDetails" %>

<asp:Content ID="cntContactDetailsHdr" title="Contact Details" ContentPlaceHolderID="HeadContent" runat="server">
    <%-- Keep HeadContent free of <%= %> — ScriptManager cannot modify <head> when it contains code blocks. --%>
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
    <asp:Label ID="lblContactID" Visible="false" runat="server" />
    <asp:ScriptManager ID="smContactDetails" runat="server" EnablePartialRendering="true" />

    <asp:Panel ID="pnlContactDetails" runat="server" CssClass="simpleForm page-tone-panel page-tone-contacts">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/icons8-new-contact-48.png" alt="" />
            <div>
                <h1 class="page-tone-title">Contact Details</h1>
                <p class="page-tone-subtitle">Manage contact information and accounts</p>
            </div>
        </div>

        <%-- No AssociatedUpdatePanelID: show for all async postbacks (form, tabs, account actions) --%>
        <asp:UpdateProgress ID="uprgContactDetails" runat="server"
            DisplayAfter="0" DynamicLayout="true">
        <ProgressTemplate>
                <div class="status-message status-info page-tone-progress">
                    <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
                    &nbsp;Please wait...
                </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

        <asp:HiddenField ID="hdnContactDirty" runat="server" Value="0" />

    <asp:UpdatePanel ID="upnlContactDetails" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnUpdate" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnInsert" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnCopy2AccInfo" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnForceNext" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnForceCheckup" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnRecalcAverage" EventName="Click" />
            <asp:PostBackTrigger ControlID="btnUpdateAndReturn" />
            <asp:PostBackTrigger ControlID="btnAddLasOrder" />
            <asp:PostBackTrigger ControlID="btnCancel" />
        </Triggers>
        <ContentTemplate>
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
                            ErrorMessage="Please select a delivery area" ControlToValidate="ddlAreas" InitialValue="0"
                            Display="None" />
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
                        <asp:RequiredFieldValidator ID="ddlDeliveryByRequiredFieldValidator" runat="server"
                            ErrorMessage="Please select who will deliver" ControlToValidate="ddlDeliveryBy" InitialValue="0"
                            Display="None" />
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
                        <asp:CheckBox ID="enabledCheckBox" runat="server" Text="Enabled" TextAlign="Right" Checked="true" />
                        <asp:CheckBox ID="autofulfillCheckBox" runat="server" Text="Auto Fulfill" TextAlign="Right" />
                        <asp:CheckBox ID="UsesFilterCheckBox" runat="server" Text="Uses Filter" TextAlign="Right" />
                        <asp:CheckBox ID="PredictionDisabledCheckBox" runat="server" Text="Prediction Disabled" TextAlign="Right" />
                        <asp:CheckBox ID="AlwaysSendChkUpCheckBox" runat="server" Text="Always Send Checkup" TextAlign="Right" />
                        <asp:CheckBox ID="NormallyRespondsCheckBox" runat="server" Text="Normally Responds" TextAlign="Right" />
                    </td>
                </tr>
                <tr>
                    <td>Notes</td>
                    <td colspan="5"><asp:TextBox ID="NotesTextBox" runat="server" TextMode="MultiLine" Height="4em" Width="93%" /></td>
                </tr>
                <tr>
                    <td colspan="6" class="rowOddC button-row">
                        <asp:Button ID="btnUpdate" Text="Save" runat="server" CssClass="filter-panel-btn"
                            OnClick="btnUpdate_Click" ToolTip="Save this contact and stay on the page" />
                        <asp:Button ID="btnUpdateAndReturn" Text="Save &amp; Return" runat="server" CssClass="filter-panel-btn"
                            OnClick="btnUpdateAndReturn_Click"
                            OnClientClick="return contactDetailsAllowNavigate();"
                            ToolTip="Save and return to the page you came from" />
                        <asp:Button ID="btnInsert" Text="Insert" runat="server" CssClass="filter-panel-btn"
                            OnClick="btnInsert_Click" ToolTip="Insert a new contact" />
                        <asp:Button ID="btnCopy2AccInfo" Text="Copy2Acc" runat="server" CssClass="filter-panel-btn"
                            OnClick="btnCopy2AccInfo_Click" CausesValidation="false" ToolTip="Copy contact fields into account info" />
                        <asp:Button ID="btnAddLasOrder" Text="Add Last" runat="server" CssClass="filter-panel-btn"
                            OnClick="btnAddLasOrder_Click" CausesValidation="false"
                            OnClientClick="return contactDetailsConfirmLeave();"
                            ToolTip="Create order from last order" />
                        <asp:Button ID="btnForceNext" Text="Force Next" runat="server" CssClass="filter-panel-btn"
                            OnClick="btnForceNext_Click" CausesValidation="false"
                            ToolTip="Skip about a week of prediction (sets Next Coffee forward)" />
                        <asp:Button ID="btnForceCheckup" Text="Force Checkup" runat="server" CssClass="filter-panel-btn"
                            OnClick="btnForceCheckup_Click" CausesValidation="false"
                            ToolTip="Force contact into next checkup cycle (Next Coffee in 5 days, reset reminders)" />
                        <asp:Button ID="btnRecalcAverage" Text="Recalc Ave" runat="server" CssClass="filter-panel-btn"
                            OnClick="btnRecalcAverage_Click" CausesValidation="false" />
                        <span class="image-button" title="Return to the page you came from without saving">
                            <asp:ImageButton ID="btnCancel" runat="server"
                                ImageUrl="~/images/imgButtons/Back.gif"
                                AlternateText="Back"
                                ToolTip="Return to the page you came from without saving"
                                OnClick="btnCancel_Click"
                                CausesValidation="false"
                                OnClientClick="return contactDetailsConfirmLeave();" />
                        </span>
                    </td>
                </tr>
            </table>

            <div class="status-message" id="pnlStatus" runat="server" style="margin-top: 12px;">
                <asp:Literal ID="ltrlStatus" Text="" runat="server" />
            </div>
            <asp:ValidationSummary ID="valContactSave" runat="server" CssClass="status-message status-error"
                HeaderText="Please fix the following:" DisplayMode="BulletList" ShowSummary="true"
                style="margin-top: 8px;" />
        </ContentTemplate>
    </asp:UpdatePanel>

    <asp:UpdatePanel ID="uppnlTabContainer" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <%-- No AutoPostBack: all tab content is bound on load, so switching is instant
                 client-side (a server round trip here left the progress overlay stuck). --%>
            <ajaxToolkit:TabContainer ID="tabcContact" runat="server" AutoPostBack="false">
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
                                        <td colspan="4" class="horizMiddle button-row">
                                            <asp:Button ID="accAddDetailsButton" runat="server" Text="Add Account Details"
                                                CssClass="filter-panel-btn" OnClick="accAddDetailsButton_Click"
                                                ToolTip="Create account details for this contact" />
                                            <asp:Button ID="accUpdateButton" runat="server" Text="Update Account Details"
                                                CssClass="filter-panel-btn" OnClick="accUpdateButton_Click"
                                                ToolTip="Save account details for this contact" />
                                        </td>
                                    </tr>
                                </table>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </ContentTemplate>
                </ajaxToolkit:TabPanel>
                <ajaxToolkit:TabPanel runat="server" HeaderText="Predicted Items" ID="tabpnlNextRequired">
                    <HeaderTemplate>Predicted Items</HeaderTemplate>
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
                <ajaxToolkit:TabPanel runat="server" HeaderText="Item Usage" ID="tabpnlItems">
                    <HeaderTemplate>Item Usage</HeaderTemplate>
                    <ContentTemplate>
                        <asp:UpdatePanel ID="upnlItems" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                            <ContentTemplate>
                                <div style="padding:4px">
                                    <asp:GridView ID="gvContactItems" runat="server" AllowSorting="False" CssClass="TblWhite small"
                                        EmptyDataText="no data yet" AutoGenerateColumns="False" AllowPaging="True" PageSize="15"
                                        DataKeyNames="ContactItemUsageLineNo">
                                        <Columns>
                                            <asp:TemplateField ShowHeader="False">
                                                <ItemTemplate>
                                                    <asp:ImageButton ID="btnEditItem" runat="server" CausesValidation="False" CommandName="Edit"
                                                        AlternateText="Edit" ToolTip="Edit this usage line"
                                                        ImageUrl="~/images/imgButtons/EditItem.gif" />
                                                    &nbsp;
                                                    <asp:ImageButton ID="btnDeleteItem" runat="server" CausesValidation="False" CommandName="Delete"
                                                        AlternateText="Delete" ToolTip="Delete this usage line"
                                                        ImageUrl="~/images/imgButtons/DelItem.gif"
                                                        OnClientClick="return confirm('Are you sure you want to delete this item usage line?');" />
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:ImageButton ID="btnUpdateItem" runat="server" CausesValidation="False" CommandName="Update"
                                                        AlternateText="Update" ToolTip="Save this usage line"
                                                        ImageUrl="~/images/imgButtons/UpdateItem.gif" />
                                                    &nbsp;
                                                    <asp:ImageButton ID="btnCancelItem" runat="server" CausesValidation="False" CommandName="Cancel"
                                                        AlternateText="Cancel" ToolTip="Cancel edit"
                                                        ImageUrl="~/images/imgButtons/CancelItem.gif" />
                                                </EditItemTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="#">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblUsageLineNo" runat="server" Text='<%# Eval("ContactItemUsageLineNo") %>' CssClass="small" />
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:Label ID="lblUsageLineNoEdit" runat="server" Text='<%# Eval("ContactItemUsageLineNo") %>' CssClass="small" />
                                                </EditItemTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="Date">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblItemDate" runat="server" Text='<%# Eval("DeliveryDate", "{0:d}") %>' />
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:TextBox ID="tbxItemDate" runat="server" Text='<%# Bind("DeliveryDate", "{0:d}") %>' Width="7em" />
                                                </EditItemTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="Item">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblItemProvided" runat="server" Text='<%# Eval("ItemProvided") %>' />
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:DropDownList ID="ddlItemsUsage" runat="server" DataSourceID="odsItems"
                                                        DataTextField="ItemDesc" DataValueField="ItemID" AppendDataBoundItems="true">
                                                        <asp:ListItem Value="0" Text="n/a" />
                                                    </asp:DropDownList>
                                                </EditItemTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="Qty">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblQty" runat="server" Text='<%# Eval("QtyProvided", "{0:0.###}") %>' />
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:TextBox ID="tbxAmountProvided" runat="server" Text='<%# Bind("QtyProvided", "{0:0.###}") %>' Width="4em" />
                                                </EditItemTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="Prep Type">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblPrepType" runat="server" Text='<%# Eval("PrepType") %>' />
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:DropDownList ID="ddlPrepTypeUsage" runat="server" DataSourceID="odsItemPrepTypes"
                                                        DataTextField="ItemPrepTypeDesc" DataValueField="ItemPrepID" AppendDataBoundItems="true">
                                                        <asp:ListItem Value="0" Text="n/a" />
                                                    </asp:DropDownList>
                                                </EditItemTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="Packaging">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblPackaging" runat="server" Text='<%# Eval("Packaging") %>' />
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:DropDownList ID="ddlPackagingUsage" runat="server" DataSourceID="odsItemPackagingTypes"
                                                        DataTextField="ItemPackagingDesc" DataValueField="ItemPackagingID" AppendDataBoundItems="true">
                                                        <asp:ListItem Value="0" Text="n/a" />
                                                    </asp:DropDownList>
                                                </EditItemTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="Notes">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblNotes" runat="server" Text='<%# Eval("Notes") %>' />
                                                </ItemTemplate>
                                                <EditItemTemplate>
                                                    <asp:TextBox ID="tbxNotes" runat="server" Text='<%# Bind("Notes") %>' TextMode="MultiLine" Rows="2" Width="12em" />
                                                </EditItemTemplate>
                                            </asp:TemplateField>
                                        </Columns>
                                    </asp:GridView>
                                </div>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </ContentTemplate>
                </ajaxToolkit:TabPanel>
                <ajaxToolkit:TabPanel runat="server" HeaderText="Orders" ID="tabpnlOrders">
                    <HeaderTemplate>Orders</HeaderTemplate>
                    <ContentTemplate>
                        <asp:UpdatePanel ID="upnlContactOrders" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                            <ContentTemplate>
                                <div style="padding:4px">
                                    <asp:GridView ID="gvContactOrders" runat="server" CssClass="TblWhite small"
                                        AutoGenerateColumns="False" EmptyDataText="no orders yet"
                                        AllowPaging="True" PageSize="15"
                                        DataKeyNames="OrderID">
                                    <Columns>
                                            <asp:TemplateField ShowHeader="False">
                                                <ItemTemplate>
                                                    <asp:HyperLink ID="hlEditOrder" runat="server"
                                                        Visible='<%# (bool)Eval("IsEditable") %>'
                                                        ImageUrl="~/images/imgButtons/EditItem.gif"
                                                        ToolTip="Edit this order"
                                                        NavigateUrl='<%# Eval("EditNavigateUrl") %>' />
                                                    <asp:Image ID="imgOrderDone" runat="server"
                                                        Visible='<%# (bool)Eval("Done") %>'
                                                        ImageUrl="~/images/imgButtons/DoneButton.gif"
                                                        AlternateText="Done"
                                                        ToolTip="Order is done" />
                                                </ItemTemplate>
                                                <ItemStyle HorizontalAlign="Center" />
                                            </asp:TemplateField>
                                            <asp:BoundField DataField="OrderID" HeaderText="Order #" />
                                            <asp:BoundField DataField="OrderDate" HeaderText="Ordered" DataFormatString="{0:yyyy-MM-dd}" />
                                            <asp:BoundField DataField="PrepDate" HeaderText="Prep" DataFormatString="{0:yyyy-MM-dd}" />
                                            <asp:BoundField DataField="RequiredByDate" HeaderText="Required By" DataFormatString="{0:yyyy-MM-dd}" />
                                            <asp:BoundField DataField="ItemsDisplay" HeaderText="Item(s)" />
                                            <asp:TemplateField HeaderText="Confirmed">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblOrderConfirmed" runat="server"
                                                        Text='<%# (bool)Eval("Confirmed") ? "Y" : "" %>' />
                                                </ItemTemplate>
                                                <ItemStyle HorizontalAlign="Center" />
                                            </asp:TemplateField>
                                        <asp:BoundField DataField="Notes" HeaderText="Notes" />
                                    </Columns>
                                        <PagerStyle CssClass="pager-row" />
                                        <PagerTemplate>
                                            <asp:PlaceHolder ID="plhPager" runat="server" />
                                        </PagerTemplate>
                                </asp:GridView>
                                </div>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </ContentTemplate>
                </ajaxToolkit:TabPanel>
                <%-- Only visible when the contact has recurring orders (enabled or disabled) --%>
                <ajaxToolkit:TabPanel runat="server" HeaderText="Recurring Orders" ID="tabpnlRecurring" Visible="false">
                    <HeaderTemplate>Recurring Orders</HeaderTemplate>
                    <ContentTemplate>
                        <div style="padding:4px">
                            <asp:GridView ID="gvContactRecurring" runat="server" CssClass="TblWhite small"
                                AutoGenerateColumns="False" EmptyDataText="no enabled recurring orders">
                                <Columns>
                                    <asp:TemplateField ShowHeader="False">
                                        <ItemTemplate>
                                            <asp:HyperLink ID="hlEditRecurring" runat="server"
                                                ImageUrl="~/images/imgButtons/EditItem.gif"
                                                ToolTip="Edit this recurring order"
                                                NavigateUrl='<%# Eval("DetailsNavigateUrl") %>' />
                                        </ItemTemplate>
                                        <ItemStyle HorizontalAlign="Center" />
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="EnabledDisplay" HeaderText="Status" />
                                    <asp:BoundField DataField="ItemsDisplay" HeaderText="Item" />
                                    <asp:BoundField DataField="RecurringPatternDisplay" HeaderText="Recurrence" />
                                    <asp:BoundField DataField="DateLastDone" HeaderText="Last Done" DataFormatString="{0:yyyy-MM-dd}" />
                                    <asp:BoundField DataField="NextDateRequired" HeaderText="Next Date" DataFormatString="{0:yyyy-MM-dd}" />
                                    <asp:BoundField DataField="RequireUntilDate" HeaderText="Until" DataFormatString="{0:yyyy-MM-dd}" />
                                    <asp:BoundField DataField="DeliveryByDisplay" HeaderText="Delivery By" />
                                </Columns>
                            </asp:GridView>
                        </div>
                    </ContentTemplate>
                </ajaxToolkit:TabPanel>
                <%-- Visible when the contact has any repairs (open or done) --%>
                <ajaxToolkit:TabPanel runat="server" HeaderText="Repairs" ID="tabpnlRepairs" Visible="false">
                    <HeaderTemplate>Repairs</HeaderTemplate>
                    <ContentTemplate>
                        <div style="padding:4px">
                            <asp:GridView ID="gvContactRepairs" runat="server" CssClass="TblWhite small"
                                AutoGenerateColumns="False" EmptyDataText="no repairs"
                                AllowPaging="True" PageSize="10">
                                <Columns>
                                    <asp:TemplateField ShowHeader="False">
                                        <ItemTemplate>
                                            <asp:HyperLink ID="hlOpenRepair" runat="server"
                                                ImageUrl="~/images/imgButtons/EditItem.gif"
                                                ToolTip='<%# IsRepairEditable(Eval("RepairStatusID")) ? "Edit this repair" : "View this repair" %>'
                                                NavigateUrl='<%# Eval("RepairID", "~/Pages/RepairDetail.aspx?RepairID={0}") %>' />
                                        </ItemTemplate>
                                        <ItemStyle HorizontalAlign="Center" />
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="JobCardNumber" HeaderText="Job Card" />
                                    <asp:BoundField DataField="DateLogged" HeaderText="Logged" DataFormatString="{0:yyyy-MM-dd}" />
                                    <asp:TemplateField HeaderText="Status">
                                        <ItemTemplate>
                                            <asp:Label ID="lblRepairStatus" runat="server"
                                                Text='<%# GetRepairStatusDesc(Eval("RepairStatusID")) %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="LastStatusChange" HeaderText="Last Change" DataFormatString="{0:yyyy-MM-dd}" />
                                    <asp:BoundField DataField="EquipSerialNumber" HeaderText="Machine S/N" />
                                    <asp:BoundField DataField="RepairFaultDesc" HeaderText="Fault" />
                                </Columns>
                                <PagerStyle CssClass="pager-row" />
                                <PagerTemplate>
                                    <asp:PlaceHolder ID="plhPager" runat="server" />
                                </PagerTemplate>
                            </asp:GridView>
                        </div>
                    </ContentTemplate>
                </ajaxToolkit:TabPanel>
            </ajaxToolkit:TabContainer>
        </ContentTemplate>
    </asp:UpdatePanel>
    </asp:Panel>

    <asp:ObjectDataSource ID="odsAreas" runat="server" TypeName="TrackerSQL.Repositories.AreasRepository" SelectMethod="GetAll">
        <SelectParameters><asp:Parameter DefaultValue="AreaName" Name="SortBy" Type="String" /></SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsItems" runat="server" TypeName="TrackerSQL.Repositories.ItemsRepository" SelectMethod="GetAll">
        <SelectParameters><asp:Parameter DefaultValue="ItemDesc" Name="SortBy" Type="String" /></SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsEquipTypes" runat="server" TypeName="TrackerSQL.Repositories.EquipTypesRepository" SelectMethod="GetAll">
        <SelectParameters><asp:Parameter DefaultValue="EquipTypeName" Name="SortBy" Type="String" /></SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsContactTypes" runat="server" TypeName="TrackerSQL.Repositories.ContactTypesRepository" SelectMethod="GetAll">
        <SelectParameters><asp:Parameter DefaultValue="ContactTypeDesc" Name="SortBy" Type="String" /></SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsPersons" runat="server" TypeName="TrackerSQL.Repositories.PersonsRepository" SelectMethod="GetAll">
        <SelectParameters><asp:Parameter DefaultValue="Abbreviation" Name="SortBy" Type="String" /></SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsItemPackagingTypes" runat="server" TypeName="TrackerSQL.Repositories.ItemPackagingsRepository" SelectMethod="GetAll">
        <SelectParameters><asp:Parameter DefaultValue="ItemPackagingDesc" Name="SortBy" Type="String" /></SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsItemPrepTypes" runat="server" TypeName="TrackerSQL.Repositories.ItemPrepTypesRepository" SelectMethod="GetAll">
        <SelectParameters><asp:Parameter DefaultValue="ItemPrepTypeDesc" Name="SortBy" Type="String" /></SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsInvoiceTypes" runat="server" TypeName="TrackerSQL.Repositories.InvoiceTypesRepository" SelectMethod="GetAll">
        <SelectParameters><asp:Parameter DefaultValue="InvoiceTypeDesc" Name="SortBy" Type="String" /></SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsPaymentTerms" runat="server" TypeName="TrackerSQL.Repositories.PaymentTermsRepository" SelectMethod="GetAll">
        <SelectParameters><asp:Parameter DefaultValue="PaymentTermDesc" Name="SortBy" Type="String" /></SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="odsPriceLevels" runat="server" TypeName="TrackerSQL.Repositories.PriceLevelsRepository" SelectMethod="GetAll">
        <SelectParameters><asp:Parameter DefaultValue="PriceLevelDesc" Name="SortBy" Type="String" /></SelectParameters>
    </asp:ObjectDataSource>

    <%-- Init in MainContent (not Head) so <%= ClientID %> does not block ScriptManager --%>
    <script type="text/javascript">
        TrackerUnsaved.init({
            dirtyFieldId: '<%= hdnContactDirty.ClientID %>',
            rootId: '<%= pnlContactDetails.ClientID %>',
            leaveMessage: 'You have unsaved changes. Leave without saving?',
            aliases: {
                markDirty: 'contactDetailsMarkDirty',
                clearDirty: 'contactDetailsClearDirty',
                allowNavigate: 'contactDetailsAllowNavigate',
                confirmLeave: 'contactDetailsConfirmLeave',
                wireFields: 'contactDetailsWireFields'
            }
        });
    </script>
</asp:Content>
