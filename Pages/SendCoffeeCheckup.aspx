<%@ Page Title="Send Coffee Checkup" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    MaintainScrollPositionOnPostback="true" CodeBehind="SendCoffeeCheckup.aspx.cs"
    Inherits="TrackerSQL.Pages.SendCoffeeCheckup" %>

<asp:Content ID="cntSendCoffeeCheckupHdr" title="Send Coffee Checkup" ContentPlaceHolderID="HeadContent" runat="server">
    <%-- Keep HeadContent free of <%= %> — ScriptManager cannot modify <head> when it contains code blocks. --%>
</asp:Content>

<asp:Content ID="cntSendCoffeeCheckupBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="smCustomerCheckup" runat="server" EnablePartialRendering="true"
        AsyncPostBackTimeout="900" />

    <asp:UpdateProgress ID="uprgSendEmail" runat="server" AssociatedUpdatePanelID="upnlSendEmail"
        DisplayAfter="0" DynamicLayout="true">
        <ProgressTemplate>
            <div class="status-message status-info page-tone-progress page-tone-send-progress">
                <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
                &nbsp;Sending checkup emails... this may take several minutes. Please wait.
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:UpdateProgress ID="uprgCustomerCheckup" runat="server" AssociatedUpdatePanelID="upnlCustomerCheckup"
        DisplayAfter="0" DynamicLayout="true">
        <ProgressTemplate>
            <div class="status-message status-info page-tone-progress">
                <img src="../images/animi/BlueArrowsUpdate.gif" alt="preparing..." />
                &nbsp;Preparing contact data...
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:Panel ID="pnlCheckup" runat="server" CssClass="simpleForm page-tone-panel page-tone-checkup">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/icons8-send-email-16.png" alt="" />
            <div>
                <h1 class="page-tone-title">Send Coffee Checkup</h1>
                <p class="page-tone-subtitle">Send automated contact checkups</p>
            </div>
        </div>

        <asp:UpdatePanel ID="upnlSendEmail" runat="server" ChildrenAsTriggers="false" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="page-tone-toolbar filter-block" style="padding-top: 12px;">
                    <asp:Label AssociatedControlID="tbxEmailSubject" Text="Subject:" runat="server" />
                    <asp:TextBox ID="tbxEmailSubject" Text="Coffee Checkup" runat="server" Width="30em" Style="padding-right: 2px" />
                    <span class="small floatRight">
                        <asp:Literal ID="ltrlEmailTextID" runat="server" Text="" />
                    </span>
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
                                <span class="small">This text appears after the Intro, before the Summary of use. Use [#PREPDATE#], to place next PREPDATE,  [#DELIVERYDATE#] for contact deliver date</span>
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

                    <div class="button-row checkup-send-actions" style="margin-top: 12px;">
                        <asp:Label ID="lblRemincderWindow" runat="server" Text="Reminder Window (days):" AssociatedControlID="ddlReminderWindow" CssClass="small" />
                        <asp:DropDownList ID="ddlReminderWindow" Style="min-width: 16px" runat="server" CssClass="small"
                            AutoPostBack="true" OnSelectedIndexChanged="ddlReminderWindow_SelectedIndexChanged" />
                        &nbsp;
                        <asp:Button ID="btnPrepData" Text="Prep Data" runat="server" CssClass="filter-panel-btn"
                            OnClick="btnPrepData_Click" Visible="true"
                            ToolTip="Prepare contact data for reminders" />
                        <asp:Button ID="btnUpdate" Text="Update Email Text" runat="server" CssClass="filter-panel-btn"
                            OnClick="btnUpdate_Click" ToolTip="Save email template text" />
                        <asp:Button ID="btnReload" Text="Email Text Reload" runat="server" CssClass="filter-panel-btn"
                            OnClick="btnReload_Click" ToolTip="Reload email template from database" />
                        <asp:Button ID="btnSend" Text="Send Checkup" runat="server" CssClass="filter-panel-btn"
                            OnClick="btnSend_Click" ToolTip="Send checkup emails, then open reminder results" />
                        <asp:ImageButton ID="imgBtnEmailTestMode" runat="server" Visible="false"
                            ImageUrl="~/images/imgButtons/Alert.gif"
                            CssClass="filter-panel-btn"
                            Style="vertical-align: middle; padding: 2px;"
                            CausesValidation="false"
                            OnClick="imgBtnEmailTestMode_Click"
                            ToolTip="Email TEST MODE is ON — messages go to the test recipient from Web.config (EmailTestMode / EmailTestRecipient)." />
                        <asp:Button ID="btnClearTodaysData" Text="Clear Sents" runat="server" CssClass="filter-panel-btn"
                            OnClick="btnClearTodaysData_Click"
                            ToolTip="Clear today's sent-reminder log entries" />
                        <asp:Button ID="btnRefreshCustomerCheckupList" Text="Refresh List" runat="server" CssClass="filter-panel-btn"
                            OnClick="btnPrepData_Click"
                            ToolTip="Refresh the contact list (re-runs Prep — excluded contacts may return)" />
                        <asp:Button ID="btnBack" Text="Back" runat="server" CssClass="filter-panel-btn"
                            OnClick="btnBack_Click" CausesValidation="false"
                            ToolTip="Return to home without sending" />
                        <span class="checkup-send-cc">
                            <asp:CheckBox ID="chkCcOrdersEmail" runat="server" Checked="true" CssClass="small"
                                Text="Send CC"
                                ToolTip="Send CC to system email" />
                        </span>
                    </div>

                    <div class="status-message" id="pnlStatus" runat="server" style="margin-top: 12px;">
                        <asp:Literal ID="ltrlStatus" runat="server" />
                    </div>
                </div>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnPrepData" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnRefreshCustomerCheckupList" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnUpdate" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnReload" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnClearTodaysData" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="ddlReminderWindow" EventName="SelectedIndexChanged" />
                <asp:AsyncPostBackTrigger ControlID="btnSend" EventName="Click" />
                <asp:PostBackTrigger ControlID="btnBack" />
                <asp:AsyncPostBackTrigger ControlID="imgBtnEmailTestMode" EventName="Click" />
            </Triggers>
        </asp:UpdatePanel>

        <div style="margin-top: 16px;">
            <h2 class="page-tone-subtitle" style="font-size: 1.1em; margin: 0 0 8px;">Contacts to receive the checkup/reminder</h2>
            <asp:UpdatePanel ID="upnlCustomerCheckup" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                <ContentTemplate>
                    <div class="results-container small">
                        <table border="0" style="width: 100%;">
                            <tr style="text-align: left; font-size: large">
                                <td style="width: 65%;"><b>Contacts To Get Reminder</b></td>
                                <td><b>Details</b></td>
                            </tr>
                            <tr>
                                <td style="vertical-align: top;">
                                    <asp:GridView ID="gvCustomerCheckup" runat="server" CssClass="results-table" Font-Size="X-Small"
                                        AllowPaging="True" PageSize="25" AutoGenerateColumns="False" DataKeyNames="CustomerID"
                                        AllowSorting="False"
                                        OnSelectedIndexChanged="gvCustomerCheckup_SelectedIndexChanged"
                                        OnPageIndexChanging="gvCustomerCheckup_PageIndexChanging"
                                        OnRowCommand="gvCustomerCheckup_RowCommand"
                                        OnRowCreated="gvCustomerCheckup_RowCreated">
                                        <EmptyDataTemplate>
                                            <div class="status-message status-info" style="padding: 20px; text-align: left;">
                                                <strong>No contacts prepared yet.</strong><br />
                                                <span class="small">Click Prep Data to build the reminder list.</span>
                                            </div>
                                        </EmptyDataTemplate>
                                        <Columns>
                                            <asp:TemplateField HeaderText="">
                                                <ItemStyle Wrap="false" Width="48px" />
                                                <ItemTemplate>
                                                    <asp:ImageButton ID="btnSelectContact" runat="server" CommandName="Select"
                                                        ImageUrl="~/images/imgButtons/SelectItem.gif"
                                                        ToolTip="Show item details for this contact"
                                                        CausesValidation="false" />
                                                    <asp:ImageButton ID="btnExcludeContact" runat="server" CommandName="ExcludeThisTime"
                                                        CommandArgument='<%# Eval("CustomerID") %>'
                                                        ImageUrl="~/images/imgButtons/No.gif"
                                                        ToolTip="Exclude this contact from this checkup run only. Warning: Prep Data or Refresh List will add them back if still due."
                                                        CausesValidation="false"
                                                        OnClientClick="return confirm('Exclude this contact from this checkup run only?\n\nWarning: Prep Data or Refresh List will add them back if they are still due.');" />
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:BoundField DataField="CustomerID" HeaderText="ContactID" Visible="False" />
                                            <asp:HyperLinkField DataNavigateUrlFields="CustomerID" DataNavigateUrlFormatString="~/Pages/ContactDetails.aspx?ID={0}"
                                                DataTextField="CompanyName" HeaderText="Company Name" />
                                            <asp:BoundField DataField="ContactFirstName" HeaderText="First Name" />
                                            <asp:BoundField DataField="EmailAddress" HeaderText="Email" />
                                            <asp:BoundField DataField="ContactAltFirstName" HeaderText="Alt First Name" />
                                            <asp:BoundField DataField="AltEmailAddress" HeaderText="Alt Email" />
                                            <asp:TemplateField HeaderText="Area">
                                                <ItemTemplate>
                                                    <asp:Label ID="AreaNameLabel" runat="server" Text='<%# GetAreaName((int)Eval("AreaID")) %>' />
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:BoundField DataField="NextCoffee" HeaderText="NxtCoffee" DataFormatString="{0:d}" />
                                            <asp:BoundField DataField="NextClean" HeaderText="NxtCln" DataFormatString="{0:d}" />
                                            <asp:BoundField DataField="NextDescal" HeaderText="NxtDecal" DataFormatString="{0:d}" />
                                            <asp:BoundField DataField="NextDeliveryDate" HeaderText="NxtDlvry" DataFormatString="{0:d}" />
                                            <asp:CheckBoxField DataField="enabled" HeaderText="Enbld" />
                                            <asp:BoundField DataField="ReminderCount" HeaderText="RCnt" />
                                        </Columns>
                                        <SelectedRowStyle CssClass="results-row-selected" BackColor="#FFF3CD" />
                                        <PagerStyle CssClass="pager-row" />
                                        <PagerTemplate>
                                            <asp:PlaceHolder ID="plhPager" runat="server" />
                                        </PagerTemplate>
                                    </asp:GridView>
                                </td>
                                <td style="vertical-align: top; padding-left: 8px;">
                                    <div class="small" style="margin-bottom: 8px; font-size: x-small;">
                                        Click the select icon to view items. Click the <strong>No</strong> icon to exclude a contact from this run only
                                        (Prep/Refresh will bring them back if still due).
                                    </div>
                                    <asp:Literal ID="ltrlSelectedContact" runat="server" />
                                    <asp:GridView ID="gvItemsToConfirm" runat="server" CssClass="TblZebra small"
                                        Font-Size="X-Small" AutoGenerateColumns="false"
                                        EmptyDataText="Select a contact to see items...">
                                        <Columns>
                                            <asp:BoundField DataField="TCIID" HeaderText="TCIID" Visible="false" />
                                            <asp:TemplateField HeaderText="Item">
                                                <ItemTemplate>
                                                    <asp:Label ID="ItemDescLabel" runat="server" Text='<%# GetItemDesc((int)Eval("ItemID")) %>' />
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:BoundField DataField="CustomerID" HeaderText="ContactID" Visible="false" />
                                            <asp:TemplateField HeaderText="Qty" ItemStyle-HorizontalAlign="Right">
                                                <ItemTemplate>
                                                    <asp:Label ID="QtyLabel" runat="server"
                                                        Text='<%# FormatItemQty(Eval("ItemQty")) %>' />
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:BoundField DataField="ItemPrepID" HeaderText="PrepID" Visible="false" />
                                            <asp:BoundField DataField="ItemPackagID" HeaderText="PackagID" Visible="false" />
                                            <asp:CheckBoxField DataField="AutoFulfill" HeaderText="AFF" />
                                            <asp:CheckBoxField DataField="RecurringOrder" HeaderText="RO" />
                                        </Columns>
                                    </asp:GridView>
                                </td>
                            </tr>
                        </table>
                    </div>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="btnPrepData" EventName="Click" />
                    <asp:AsyncPostBackTrigger ControlID="btnRefreshCustomerCheckupList" EventName="Click" />
                    <asp:AsyncPostBackTrigger ControlID="ddlReminderWindow" EventName="SelectedIndexChanged" />
                </Triggers>
            </asp:UpdatePanel>
        </div>
    </asp:Panel>

    <%-- Init script in MainContent (not Head) so <%= ClientID %> does not block ScriptManager --%>
    <script type="text/javascript">
        function pageLoad(sender, args) {
            if (args && args.get_isPartialLoad && args.get_isPartialLoad())
                return;
            window.setTimeout(function () {
                var prepButton = document.getElementById('<%= btnPrepData.ClientID %>');
                if (prepButton)
                    __doPostBack('<%= btnPrepData.UniqueID %>', '');
            }, 500);
        }
    </script>
</asp:Content>
