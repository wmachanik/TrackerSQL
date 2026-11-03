<%@ Page Title="Send Client Mail" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    MaintainScrollPositionOnPostback="true" CodeBehind="SendClientMail.aspx.cs"
    Inherits="TrackerSQL.Pages.SendClientMail" %>

<asp:Content ID="cntSendClientMailHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="cntSendClientMailBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="smClientMail" runat="server" EnablePartialRendering="true"
        AsyncPostBackTimeout="900" />

    <asp:UpdateProgress ID="uprgSend" runat="server" AssociatedUpdatePanelID="upnlCompose"
        DisplayAfter="0" DynamicLayout="true">
        <ProgressTemplate>
            <div class="status-message status-info page-tone-progress page-tone-send-progress">
                <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
                &nbsp;Working... this may take several minutes. Please wait.
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:Panel ID="pnlClientMail" runat="server" CssClass="simpleForm page-tone-panel page-tone-clientmail">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/icons8-send-email-16.png" alt="" />
            <div>
                <h1 class="page-tone-title">Send Client Mail</h1>
                <p class="page-tone-subtitle">Filter contacts and send a one-off HTML email</p>
            </div>
        </div>

        <asp:UpdatePanel ID="upnlCompose" runat="server" ChildrenAsTriggers="false" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="page-tone-toolbar filter-block" style="padding-top: 12px;">
                    <asp:Label AssociatedControlID="tbxEmailSubject" Text="Subject:" runat="server" />
                    <asp:TextBox ID="tbxEmailSubject" runat="server" Width="36em" />
                    <asp:CheckBox ID="chkIncludeGreeting" runat="server" Checked="true" CssClass="small"
                        Text="Address Contact (Hi First Name)"
                        ToolTip="Prepend the standard greeting from Messages.resx (ClientMail.GreetingWithName)" />
                    <br /><br />

                    <ajaxToolkit:TabContainer ID="tabEmailBody" runat="server" Height="220px" Width="100%">
                        <ajaxToolkit:TabPanel ID="tpnlHeader" HeaderText="Header" runat="server">
                            <ContentTemplate>
                                <asp:TextBox ID="tbxEmailHeader" runat="server" TextMode="MultiLine" Height="100%" Width="99%" Rows="10" />
                                <ajaxToolkit:HtmlEditorExtender ID="HtmlEmailHeader" TargetControlID="tbxEmailHeader" runat="server"
                                    DisplaySourceTab="true" EnableSanitization="false" />
                                <br />
                                <span class="small">Standard header. Placeholders: [#FIRSTNAME#], [#LASTNAME#], [#COMPANY#], [#GREETING#].</span>
                            </ContentTemplate>
                        </ajaxToolkit:TabPanel>
                        <ajaxToolkit:TabPanel ID="tpnlBody" HeaderText="Message" runat="server">
                            <ContentTemplate>
                                <asp:TextBox ID="tbxEmailBody" runat="server" TextMode="MultiLine" Height="100%" Width="99%" Rows="10" />
                                <ajaxToolkit:HtmlEditorExtender ID="HtmlEmailBody" TargetControlID="tbxEmailBody" runat="server"
                                    DisplaySourceTab="true" EnableSanitization="false" />
                                <br />
                                <span class="small">The message sent between the header and footer. Same placeholders as the header.</span>
                            </ContentTemplate>
                        </ajaxToolkit:TabPanel>
                        <ajaxToolkit:TabPanel ID="tpnlFooter" HeaderText="Footer" runat="server">
                            <ContentTemplate>
                                <asp:TextBox ID="tbxEmailFooter" runat="server" TextMode="MultiLine" Height="100%" Width="99%" Rows="10" />
                                <ajaxToolkit:HtmlEditorExtender ID="HtmlEmailFooter" TargetControlID="tbxEmailFooter" runat="server"
                                    DisplaySourceTab="true" EnableSanitization="false" />
                                <br />
                                <span class="small">Standard footer. The usual email signature is added after this.</span>
                            </ContentTemplate>
                        </ajaxToolkit:TabPanel>
                    </ajaxToolkit:TabContainer>

                    <div class="clientmail-filters" style="margin-top: 14px;">
                        <h2 class="page-tone-subtitle" style="font-size: 1.05em; margin: 0 0 8px;">Filters</h2>
                        <table class="small" style="width: 100%;">
                            <tr>
                                <td style="width: 16%;"><asp:Label AssociatedControlID="tbxName" Text="Name:" runat="server" /></td>
                                <td style="width: 34%;"><asp:TextBox ID="tbxName" runat="server" Width="95%" /></td>
                                <td style="width: 16%;"><asp:Label AssociatedControlID="ddlContactType" Text="Contact Type:" runat="server" /></td>
                                <td><asp:DropDownList ID="ddlContactType" runat="server" Width="95%" /></td>
                            </tr>
                            <tr>
                                <td><asp:Label AssociatedControlID="tbxLastOrderedFrom" Text="Last Ordered From:" runat="server" /></td>
                                <td>
                                    <asp:TextBox ID="tbxLastOrderedFrom" runat="server" Width="9em" />
                                    <asp:Label AssociatedControlID="tbxLastOrderedTo" Text="To:" runat="server" />
                                    <asp:TextBox ID="tbxLastOrderedTo" runat="server" Width="9em" />
                                </td>
                                <td><asp:Label AssociatedControlID="tbxNextFrom" Text="Next Items From:" runat="server" /></td>
                                <td>
                                    <asp:TextBox ID="tbxNextFrom" runat="server" Width="9em" ToolTip="Next Coffee By from date" />
                                    <asp:Label AssociatedControlID="tbxNextTo" Text="To:" runat="server" />
                                    <asp:TextBox ID="tbxNextTo" runat="server" Width="9em" />
                                </td>
                            </tr>
                            <tr>
                                <td><asp:Label AssociatedControlID="ddlEquipType" Text="Equipment:" runat="server" /></td>
                                <td><asp:DropDownList ID="ddlEquipType" runat="server" Width="95%" /></td>
                                <td><asp:Label AssociatedControlID="ddlRecurring" Text="Recurring Client:" runat="server" /></td>
                                <td><asp:DropDownList ID="ddlRecurring" runat="server" Width="12em" /></td>
                            </tr>
                            <tr>
                                <td style="vertical-align: top;"><asp:Label AssociatedControlID="lstAreas" Text="Area Or Areas:" runat="server" /></td>
                                <td style="vertical-align: top;">
                                    <asp:ListBox ID="lstAreas" runat="server" SelectionMode="Multiple" Rows="6" Width="95%"
                                        ToolTip="Hold Ctrl to select more than one area" />
                                </td>
                                <td style="vertical-align: top;"><asp:Label AssociatedControlID="lstLastItems" Text="Last Item(s) Ordered:" runat="server" /></td>
                                <td style="vertical-align: top;">
                                    <asp:ListBox ID="lstLastItems" runat="server" SelectionMode="Multiple" Rows="6" Width="95%"
                                        ToolTip="Match contacts whose most recent done order includes any of these items" />
                                </td>
                            </tr>
                            <tr>
                                <td style="vertical-align: top;"><asp:Label AssociatedControlID="tbxCustomWhere" Text="Custom Where:" runat="server" /></td>
                                <td colspan="3">
                                    <asp:TextBox ID="tbxCustomWhere" runat="server" TextMode="MultiLine" Rows="3" Width="99%"
                                        ToolTip="Optional extra SQL WHERE fragment, ANDed onto the query. Aliases: c ContactsTbl, u usage, lastOrd last order, rec recurring." />
                                    <div class="small">Example: c.AreaID IN (1,2) AND u.NextCoffeeBy IS NOT NULL — no SELECT, comments, or semicolons.</div>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td colspan="3">
                                    <asp:CheckBox ID="chkEnabledOnly" runat="server" Checked="true" CssClass="small" Text="Enabled Contacts Only" />
                                    <asp:CheckBox ID="chkMustHaveEmail" runat="server" Checked="true" CssClass="small" Text="Must Have Email" />
                                </td>
                            </tr>
                        </table>
                    </div>

                    <div class="button-row checkup-send-actions checkup-toolbar-actions" style="margin-top: 12px;">
                        <asp:Button ID="btnApplyFilters" Text="Apply Filters" runat="server" CssClass="filter-panel-btn"
                            OnClick="btnApplyFilters_Click" ToolTip="Refresh the recipient list" />
                        <asp:Label AssociatedControlID="ddlGroupSize" Text="Send In Groups Of:" runat="server" CssClass="small" />
                        <asp:DropDownList ID="ddlGroupSize" runat="server" CssClass="small" />
                        <asp:Label AssociatedControlID="ddlDelay" Text="Delay Between Emails:" runat="server" CssClass="small" />
                        <asp:DropDownList ID="ddlDelay" runat="server" CssClass="small" />
                        <asp:Label AssociatedControlID="tbxTestEmail" Text="Test To:" runat="server" CssClass="small" />
                        <asp:TextBox ID="tbxTestEmail" runat="server" Width="16em" />
                        <asp:Button ID="btnSendTest" Text="Send Test Email" runat="server" CssClass="filter-panel-btn"
                            OnClick="btnSendTest_Click" ToolTip="Send one test using the first listed contact for personalisation" />
                        <asp:Button ID="btnSend" Text="Send Mail" runat="server" CssClass="filter-panel-btn"
                            OnClick="btnSend_Click"
                            OnClientClick="return confirm('Send this mail to every contact currently listed?');"
                            ToolTip="Send to the listed contacts in the chosen group size" />
                        <asp:ImageButton ID="imgBtnEmailTestMode" runat="server" Visible="false"
                            ImageUrl="~/images/imgButtons/Alert.gif"
                            CssClass="filter-panel-btn"
                            Style="vertical-align: middle; padding: 2px;"
                            CausesValidation="false"
                            OnClick="imgBtnEmailTestMode_Click"
                            ToolTip="Email TEST MODE is ON." />
                        <asp:CheckBox ID="chkCcOrdersEmail" runat="server" CssClass="small"
                            Text="Send CC"
                            ToolTip="Send CC to the system email from Web.config" />
                        <span class="image-button" title="Return to home">
                            <asp:ImageButton ID="btnBack" runat="server"
                                ImageUrl="~/images/imgButtons/Back.gif"
                                AlternateText="Back"
                                ToolTip="Return to home"
                                OnClick="btnBack_Click"
                                CausesValidation="false" />
                        </span>
                    </div>

                    <div class="status-message" id="pnlStatus" runat="server" style="margin-top: 12px;">
                        <asp:Literal ID="ltrlStatus" runat="server" />
                    </div>
                </div>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnApplyFilters" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnSendTest" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnSend" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="imgBtnEmailTestMode" EventName="Click" />
                <asp:PostBackTrigger ControlID="btnBack" />
            </Triggers>
        </asp:UpdatePanel>

        <div style="margin-top: 16px;">
            <h2 class="page-tone-subtitle" style="font-size: 1.1em; margin: 0 0 8px;">Contacts Who Will Get This Mail</h2>
            <asp:UpdatePanel ID="upnlRecipients" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                <ContentTemplate>
                    <div class="results-container small">
                        <asp:Label ID="lblRecipientCount" runat="server" CssClass="small" />
                        <asp:GridView ID="gvRecipients" runat="server" CssClass="results-table staff-checkup-grid" Font-Size="X-Small"
                            AllowPaging="True" PageSize="25" AutoGenerateColumns="False" DataKeyNames="ContactID"
                            OnPageIndexChanging="gvRecipients_PageIndexChanging"
                            OnRowCommand="gvRecipients_RowCommand"
                            EmptyDataText="Apply Filters to build the list.">
                            <Columns>
                                <asp:TemplateField HeaderText="">
                                    <ItemStyle Wrap="false" Width="28px" />
                                    <ItemTemplate>
                                        <asp:ImageButton ID="btnExclude" runat="server" CommandName="Exclude"
                                            CommandArgument='<%# Eval("ContactID") %>'
                                            ImageUrl="~/images/imgButtons/No.gif"
                                            ToolTip="Exclude this contact from this send only"
                                            CausesValidation="false" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:HyperLinkField DataNavigateUrlFields="ContactID" DataNavigateUrlFormatString="~/Pages/ContactDetails.aspx?ID={0}"
                                    DataTextField="CompanyName" HeaderText="Company Name" />
                                <asp:BoundField DataField="ContactFirstName" HeaderText="First Name" />
                                <asp:BoundField DataField="EmailAddress" HeaderText="Email" />
                                <asp:BoundField DataField="ContactTypeDesc" HeaderText="Type" />
                                <asp:BoundField DataField="AreaName" HeaderText="Area" />
                                <asp:BoundField DataField="EquipTypeName" HeaderText="Equipment" />
                                <asp:BoundField DataField="LastOrderDate" HeaderText="Last Ordered" DataFormatString="{0:d}" />
                                <asp:BoundField DataField="NextCoffeeBy" HeaderText="Next Coffee" DataFormatString="{0:d}" />
                                <asp:BoundField DataField="LastItemsOrdered" HeaderText="Last Items" />
                                <asp:CheckBoxField DataField="IsRecurring" HeaderText="Recurring" />
                            </Columns>
                            <PagerStyle CssClass="pager-row" />
                        </asp:GridView>
                    </div>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="btnApplyFilters" EventName="Click" />
                    <asp:AsyncPostBackTrigger ControlID="btnSend" EventName="Click" />
                </Triggers>
            </asp:UpdatePanel>
        </div>
    </asp:Panel>
</asp:Content>
