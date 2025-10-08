<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" MaintainScrollPositionOnPostback="true"
    CodeBehind="SendCoffeeCheckup.aspx.cs" Inherits="TrackerDotNet.Pages.SendCoffeeCheckup" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        // Auto-start data preparation when page is ready
        function pageLoadComplete() {
            // Give the page a moment to fully render, then auto-prep data
            setTimeout(function () {
                var prepButton = document.getElementById('<%= btnPrepData.ClientID %>');
                if (prepButton && prepButton.style.display !== 'none') {
                    // Trigger auto-prep
                    __doPostBack('<%= btnPrepData.UniqueID %>', '');
                }
            }, 500); // Wait 1 second for page to fully load
        }

        // Call when page loads
        window.onload = pageLoadComplete;
    </script>
</asp:Content>

<asp:Content ID="cntSendCoffeeCheckupBdy" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Send Coffee Reminder to Customers</h1>
    <asp:ScriptManager ID="smCustomerCheckup" runat="server" />
    <!-- Enhanced auto-loading progress -->
    <asp:UpdateProgress ID="uprgSendEmail" runat="server" AssociatedUpdatePanelID="upnlSendEmail" ViewStateMode="Enabled">
        <ProgressTemplate>
            <div style="text-align: left; padding: 10px; background-color: #fff3cd; border: 1px solid #ffeaa7; border-radius: 5px;">
                <img src="../images/animi/QuaffeeProgress.gif" alt="sending..." width="16" height="16" />&nbsp;              
                <strong>Please wait...</strong> This may take several minutes.          
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:UpdateProgress ID="uprgCustomerCheckup" runat="server" AssociatedUpdatePanelID="upnlCustomerCheckup">
        <ProgressTemplate>
            <div style="text-align: left; padding: 10px; background-color: #d1ecf1; border: 1px solid #bee5eb; border-radius: 5px;">
                <img src="../images/animi/BlueArrowsUpdate.gif" alt="updating..." width="16" height="16" />&nbsp;               
                <strong>Preparing customer data...</strong> Please wait...           
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>
    <div class="filter-toolbar" style="padding: 10px">
        <asp:UpdatePanel ID="upnlSendEmail" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional" style="width:100%">
            <ContentTemplate>
                <div class="filter-block">
                    <div>
                        <asp:Label AssociatedControlID="tbxEmailSubject" Text="Subject:" runat="server" />
                        <asp:TextBox ID="tbxEmailSubject" Text="Coffee Checkup" runat="server" Width="30em" Style="padding-right: 2px" />
                        <span class="small floatRight">
                            <asp:Literal ID="ltrlEmailTextID" runat="server" Text="" /></span>
                        <br />
                        <br />
                        <ajaxToolkit:TabContainer ID="tabEmailBody" runat="server" Height="200px" Width="100%">
                            <ajaxToolkit:TabPanel ID="tpnlEmailIntro" TabIndex="0" HeaderText="Checkup Intro" runat="server">
                                <ContentTemplate>
                                    <asp:TextBox ID="tbxEmailIntro" runat="server" TextMode="MultiLine" Height="100%" Width="99%" Rows="10"
                                        Text="Welcome to Quaffee's coffee checkup or reminder" CausesValidation="false" />
                                    <ajaxToolkit:HtmlEditorExtender ID="HtmlEditorExtenderEmailIntro" TargetControlID="tbxEmailIntro" runat="server"
                                        DisplaySourceTab="true" EnableSanitization="false" />
                                    <br />
                                    <span class="small">Enter the text that will appear as the Introduction to the emails.</span><br />
                                </ContentTemplate>
                            </ajaxToolkit:TabPanel>
                            <ajaxToolkit:TabPanel ID="tpnlEmailBody" TabIndex="1" HeaderText="Checkup Body" runat="server">
                                <ContentTemplate>
                                    <asp:TextBox ID="tbxEmailBody" runat="server" TextMode="MultiLine" Height="100%" Width="99%" Rows="10"
                                        Text="" />
                                    <ajaxToolkit:HtmlEditorExtender ID="HtmlEditorExtenderEmailBody" TargetControlID="tbxEmailBody" runat="server"
                                        DisplaySourceTab="true" EnableSanitization="false" />
                                    <br />
                                    <span class="small">This text appears after the Intro, before the Summary of use. Use [#PREPDATE#], to place next PREPDATE,  [#DELIVERYDATE#] for customer deliver date</span>
                                </ContentTemplate>
                            </ajaxToolkit:TabPanel>
                            <ajaxToolkit:TabPanel ID="tpnlEmailFooter" TabIndex="2" HeaderText="Checkup Footer" runat="server">
                                <ContentTemplate>
                                    <asp:TextBox ID="tbxEmailFooter" runat="server" TextMode="MultiLine" Height="100%" Width="99%" Rows="10" Text="" />
                                    <ajaxToolkit:HtmlEditorExtender ID="HtmlEmailFooter" TargetControlID="tbxEmailFooter" runat="server"
                                        DisplaySourceTab="true" EnableSanitization="false" ClientIDMode="Predictable" />
                                    <br />
                                    <span class="small">Enter the footer, which appears under the summary data.</span>
                                </ContentTemplate>
                            </ajaxToolkit:TabPanel>
                        </ajaxToolkit:TabContainer>
                        <asp:Literal ID="ltrlStatus" Text="" runat="server" />
                    </div>
                    <div class="button-toolbar-rounded">
                        <asp:Label ID="lblRemincderWindow" runat="server" Text="Reminder Window (days):" AssociatedControlID="ddlReminderWindow" CssClass="small" />
                        <asp:DropDownList ID="ddlReminderWindow" Style="min-width: 16px" runat="server" CssClass="small" AutoPostBack="true" OnSelectedIndexChanged="ddlReminderWindow_SelectedIndexChanged" />
                        &nbsp;&nbsp;&nbsp;&nbsp;
                        <!-- Make prep data button visible for auto-prep to work -->
                        <asp:Button ID="btnPrepData" Text="Prep Data" runat="server"
                            OnClick="btnPrepData_Click" Visible="true"
                            ToolTip="Prepare customer data for reminders" />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                        <asp:Button ID="btnUpdate" Text="Update Email Text" runat="server"
                            OnClick="btnUpdate_Click" />&nbsp;&nbsp;&nbsp;
                        <asp:Button ID="btnReload" Text="Email Text Reload" runat="server"
                            OnClick="btnReload_Click" />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                        <asp:Button ID="btnSend" Text="Send Checkup" runat="server" OnClick="btnSend_Click" />&nbsp;&nbsp;&nbsp;&nbsp;
<%--                        <asp:Button ID="btnTestSingleCustomer" Text="Test Single Email" runat="server" OnClick="btnTestSingleCustomer_Click"
                            ToolTip="Send a test email to verify email formatting and content" />&nbsp;&nbsp;&nbsp;&nbsp;--%>
                        <asp:Button ID="btnClearTodaysData" Text="Clear Sents" runat="server" OnClick="btnClearTodaysData_Click"
                            ToolTip="Clear the sent today's table" />&nbsp;&nbsp;&nbsp;&nbsp;
                        <asp:Button ID="btnRefreshCustomerCheckupList" Text="Refresh List" runat="server" OnClick="btnPrepData_Click"
                            ToolTip="Refresh the customer list" />
                    </div>
                </div>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnSend" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnPrepData" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnRefreshCustomerCheckupList" EventName="Click" />
            </Triggers>
        </asp:UpdatePanel>
    </div>
    <h2>Customers to receive the checkup/reminder</h2>
    <!-- Customer status area -->
    <div class="results-container small">
        <table border="0">
            <tr style="text-align: center; font-size: large">
                <td><b>Customers To Get Reminder</b></td>
                <td><b>Details</b></td>
            </tr>
            <tr>
                <td>
                    <asp:UpdatePanel ID="upnlCustomerCheckup" runat="server" Visible="true" ChildrenAsTriggers="true" UpdateMode="Conditional">
                        <ContentTemplate>
                            <asp:GridView ID="gvCustomerCheckup" runat="server" CssClass="results-table" Font-Size="X-Small"
                                AllowPaging="True" PageSize="25" AutoGenerateColumns="False" DataKeyNames="CustomerID"
                                DataSourceID="odsContactsToSendCheckup" AllowSorting="True">
                                <EmptyDataTemplate>
                                    <div style="padding: 20px; text-align: left;">
                                        <img src="../images/animi/QuaffeeProgress.gif" alt="loading..." width="16" height="16" /><br />
                                        <strong>Loading customer data...</strong><br />
                                        <span class="small">Please wait while we prepare the customer list.</span>
                                    </div>
                                </EmptyDataTemplate>
                                <Columns>
                                    <asp:CommandField ButtonType="Image" SelectImageUrl="~/images/imgButtons/SelectItem.gif" ShowSelectButton="true" />
                                    <asp:BoundField DataField="CustomerID" HeaderText="CustomerID" SortExpression="CustomerID" Visible="False" />
                                    <asp:HyperLinkField DataNavigateUrlFields="CustomerID" DataNavigateUrlFormatString="~/Pages/CustomerDetails.aspx?ID={0}"
                                        DataTextField="CompanyName" HeaderText="Company Name" SortExpression="CompanyName" />
                                    <asp:BoundField DataField="ContactFirstName" HeaderText="First Name" SortExpression="ContactFirstName" />
                                    <asp:BoundField DataField="EmailAddress" HeaderText="Email" SortExpression="EmailAddress" />
                                    <asp:BoundField DataField="ContactAltFirstName" HeaderText="Alt First Name" SortExpression="ContactAltFirstName" />
                                    <asp:BoundField DataField="AltEmailAddress" HeaderText="Alt Email" SortExpression="AltEmailAddress" />
                                    <asp:TemplateField HeaderText="City" SortExpression="CityID">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="CityNameTextBox" runat="server" Text='<%# Bind("CityID") %>'></asp:TextBox>
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="CityNameLabel" runat="server" Text='<%# GetCityName((int)Eval("CityID")) %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="NextCoffee" HeaderText="NxtCoffee" SortExpression="NextCoffee" DataFormatString="{0:d}" />
                                    <asp:BoundField DataField="NextClean" HeaderText="NxtCln" SortExpression="NextClean" DataFormatString="{0:d}" />
                                    <asp:BoundField DataField="NextDescal" HeaderText="NxtDecal" SortExpression="NextDescal" DataFormatString="{0:d}" />
                                    <asp:BoundField DataField="NextDeliveryDate" HeaderText="NxtDlvry" SortExpression="NextDeliveryDate" DataFormatString="{0:d}" />
                                    <asp:CheckBoxField DataField="enabled" HeaderText="Enbld" SortExpression="enabled" />
                                    <asp:BoundField DataField="ReminderCount" HeaderText="RCnt" SortExpression="ReminderCount" />
                                </Columns>
                            </asp:GridView>
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="btnPrepData" EventName="Click" />
                            <asp:AsyncPostBackTrigger ControlID="btnRefreshCustomerCheckupList" EventName="Click" />
                        </Triggers>
                    </asp:UpdatePanel>
                </td>
                <td style="vertical-align: text-top; vertical-align: top; padding-left: 8px">
                    <asp:UpdatePanel ID="upnlContactItems" Visible="true" runat="server">
                        <ContentTemplate>
                            <asp:GridView ID="gvItemsToConfirm" runat="server" CssClass="TblZebra small"
                                DataSourceID="odsContactToBeRemindedItems" AutoGenerateColumns="false">
                                <Columns>
                                    <asp:BoundField DataField="TCIID" HeaderText="TCIID" SortExpression="TCIID" Visible="false" />
                                    <asp:TemplateField HeaderText="Item">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="ItemDescTextBox" runat="server" Text='<%# Bind("ItemID") %>'></asp:TextBox>
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="ItemDescLabel" runat="server" Text='<%# GetItemDesc((int)Eval("ItemID")) %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="CustomerID" HeaderText="CustomerID" Visible="false" SortExpression="CustomerID" />
                                    <asp:BoundField DataField="ItemQty" HeaderText="Qty" SortExpression="ItemQty" />
                                    <asp:BoundField DataField="ItemPrepID" HeaderText="PrepID" SortExpression="ItemPrepID" Visible="false" />
                                    <asp:BoundField DataField="ItemPackagID" HeaderText="PackagID" Visible="false" SortExpression="ItemPackagID" />
                                    <asp:CheckBoxField DataField="AutoFulfill" HeaderText="AFF" SortExpression="AutoFulfill" />
                                    <asp:CheckBoxField DataField="ReoccurOrder" HeaderText="RO" SortExpression="ReoccurOrder" />
                                </Columns>
                                <EmptyDataTemplate>
                                    <div style="padding: 20px; text-align: center;">
                                        <strong>Select a customer to see items...</strong><br />
                                        <span class="small">Click on a customer row to view their typical order items.</span>
                                    </div>
                                </EmptyDataTemplate>
                            </asp:GridView>
                        </ContentTemplate>
                        <Triggers>
                        </Triggers>
                    </asp:UpdatePanel>
                </td>
            </tr>
        </table>

<%--<asp:Button ID="btnShowMatrix" runat="server"
    Text="Show Delivery Matrix"
    OnClick="btnShowMatrix_Click"
    CssClass="btn btn-secondary"
    Style="margin-top:8px;margin-right:8px;" />

<asp:Panel ID="pnlMatrixDump" runat="server" Visible="false" Style="margin-top:10px;">
    <asp:Literal ID="ltrlMatrixDump" runat="server" Mode="PassThrough" />
</asp:Panel>--%>
        <asp:ObjectDataSource ID="odsContactsToSendCheckup" runat="server"
            DataObjectTypeName="TrackerDotNet.Controls.ContactToRemindDetails"
            InsertMethod="InsertContacts" SelectMethod="GetAllContacts"
            SortParameterName="SortBy"
            TypeName="TrackerDotNet.Controls.TempCoffeeCheckup"
            OldValuesParameterFormatString="original_{0}">
            <SelectParameters>
                <asp:Parameter DefaultValue="CompanyName" Name="SortBy" Type="String" />
            </SelectParameters>
        </asp:ObjectDataSource>
        <asp:ObjectDataSource ID="odsContactToBeRemindedItems" runat="server"
            SelectMethod="GetAllContactItems" SortParameterName="SortBy"
            TypeName="TrackerDotNet.Controls.TempCoffeeCheckup"
            OldValuesParameterFormatString="original_{0}">
            <SelectParameters>
                <asp:ControlParameter ControlID="gvCustomerCheckup" Name="CustomerID"
                    PropertyName="SelectedValue" Type="Int64" />
                <asp:Parameter Name="SortBy" Type="String" />
            </SelectParameters>
        </asp:ObjectDataSource>
        <br />
    </div>
</asp:Content>

