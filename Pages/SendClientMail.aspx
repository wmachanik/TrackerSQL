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

    <asp:Panel ID="pnlComposeCard" runat="server" CssClass="simpleForm page-tone-panel page-tone-clientmail">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/icons8-send-email-16.png" alt="" />
            <div>
                <h1 class="page-tone-title">Send Client Mail</h1>
                <p class="page-tone-subtitle">Write the message, then filter who should receive it</p>
            </div>
        </div>

        <asp:UpdatePanel ID="upnlCompose" runat="server" ChildrenAsTriggers="false" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="page-tone-toolbar filter-block" style="padding-top: 12px;">
                    <asp:Label AssociatedControlID="ddlSavedMessages" Text="Saved Message:" runat="server" />
                    <asp:DropDownList ID="ddlSavedMessages" runat="server" CssClass="small" AutoPostBack="true"
                        OnSelectedIndexChanged="ddlSavedMessages_SelectedIndexChanged" Width="18em" />
                    <asp:Label AssociatedControlID="tbxTemplateName" Text="Name:" runat="server" />
                    <asp:TextBox ID="tbxTemplateName" runat="server" Width="14em" />
                    <asp:Button ID="btnSaveMessage" Text="Save Message" runat="server" CssClass="filter-panel-btn"
                        OnClick="btnSaveMessage_Click" CausesValidation="false"
                        ToolTip="Save as a new message, or update the one selected in Saved Message" />
                    <asp:Button ID="btnReloadDefaults" Text="Reload Defaults" runat="server" CssClass="filter-panel-btn"
                        OnClick="btnReloadDefaults_Click" CausesValidation="false"
                        ToolTip="Reload the standard header, message, footer and subject from Messages.resx" />
                    <br />
                    <asp:Label AssociatedControlID="tbxEmailSubject" Text="Subject:" runat="server" />
                    <asp:TextBox ID="tbxEmailSubject" runat="server" Width="36em" />
                    <br /><br />

                    <ajaxToolkit:TabContainer ID="tabEmailBody" runat="server" Height="220px" Width="100%">
                        <ajaxToolkit:TabPanel ID="tpnlHeader" HeaderText="Header" runat="server">
                            <ContentTemplate>
                                <asp:TextBox ID="tbxEmailHeader" runat="server" TextMode="MultiLine" Height="100%" Width="99%" Rows="10" />
                                <ajaxToolkit:HtmlEditorExtender ID="HtmlEmailHeader" TargetControlID="tbxEmailHeader" runat="server"
                                    DisplaySourceTab="true" EnableSanitization="false" />
                                <br />
                                <span class="small">Address the contact in this header (for example Dear [#FIRSTNAME#],). Placeholders: [#FIRSTNAME#], [#LASTNAME#], [#COMPANY#], [#GREETING#].</span>
                            </ContentTemplate>
                        </ajaxToolkit:TabPanel>
                        <ajaxToolkit:TabPanel ID="tpnlBody" HeaderText="Message" runat="server">
                            <ContentTemplate>
                                <asp:TextBox ID="tbxEmailBody" runat="server" TextMode="MultiLine" Height="100%" Width="99%" Rows="10" />
                                <ajaxToolkit:HtmlEditorExtender ID="HtmlEmailBody" TargetControlID="tbxEmailBody" runat="server"
                                    DisplaySourceTab="true" EnableSanitization="false" />
                                <br />
                                <span class="small">Default message from ClientMail.DefaultBody. Same placeholders as the header.</span>
                            </ContentTemplate>
                        </ajaxToolkit:TabPanel>
                        <ajaxToolkit:TabPanel ID="tpnlFooter" HeaderText="Footer" runat="server">
                            <ContentTemplate>
                                <asp:TextBox ID="tbxEmailFooter" runat="server" TextMode="MultiLine" Height="100%" Width="99%" Rows="10" />
                                <ajaxToolkit:HtmlEditorExtender ID="HtmlEmailFooter" TargetControlID="tbxEmailFooter" runat="server"
                                    DisplaySourceTab="true" EnableSanitization="false" />
                                <br />
                                <span class="small">Default footer from ClientMail.DefaultFooter. The usual email signature is added after this.</span>
                            </ContentTemplate>
                        </ajaxToolkit:TabPanel>
                    </ajaxToolkit:TabContainer>

                    <h2 class="page-tone-title" style="font-size: 1rem; margin: 16px 0 8px;">Send History</h2>
                    <asp:GridView ID="gvSendHistory" runat="server" CssClass="results-table" Font-Size="Small"
                        AutoGenerateColumns="False" EmptyDataText="No client mail has been sent yet.">
                        <Columns>
                            <asp:BoundField DataField="MessageLabel" HeaderText="Message" />
                            <asp:BoundField DataField="SentAtText" HeaderText="Date" ItemStyle-Wrap="false" />
                            <asp:BoundField DataField="EmailsSent" HeaderText="Emails Sent" ItemStyle-HorizontalAlign="Right" />
                            <asp:BoundField DataField="FilterText" HeaderText="Filter" />
                            <asp:BoundField DataField="SuccessText" HeaderText="Success Rate" ItemStyle-Wrap="false" />
                        </Columns>
                    </asp:GridView>

                    <div class="button-row checkup-send-actions" style="margin-top: 12px;">
                        <asp:Button ID="btnOpenTest" Text="Send Test Email" runat="server" CssClass="filter-panel-btn"
                            OnClick="btnOpenTest_Click" CausesValidation="false"
                            OnClientClick="return clientMailMarkBusy(this, 'Sending...');" />
                        <asp:Button ID="btnOpenSend" Text="Send Mail" runat="server" CssClass="filter-panel-btn"
                            OnClick="btnOpenSend_Click" CausesValidation="false" />
                        <asp:Button ID="btnExportCsv" Text="Export Csv" runat="server" CssClass="filter-panel-btn"
                            OnClick="btnExportCsv_Click" CausesValidation="false"
                            ToolTip="Download the listed contacts and last items as a CSV file" />
                        <asp:ImageButton ID="imgBtnEmailTestMode" runat="server" Visible="false"
                            ImageUrl="~/images/imgButtons/Alert.gif"
                            CssClass="filter-panel-btn"
                            Style="vertical-align: middle; padding: 2px;"
                            CausesValidation="false"
                            OnClick="imgBtnEmailTestMode_Click"
                            ToolTip="Email TEST MODE is ON." />
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

                    <asp:Button ID="btnDummyTest" runat="server" Style="display: none;" CausesValidation="false" />
                    <asp:Button ID="btnDummySend" runat="server" Style="display: none;" CausesValidation="false" />

                    <ajaxToolkit:ModalPopupExtender ID="mpeTest" runat="server"
                        TargetControlID="btnDummyTest" PopupControlID="pnlTestPopup"
                        CancelControlID="btnTestCancel" BackgroundCssClass="clientmail-modal-bg" />
                    <asp:Panel ID="pnlTestPopup" runat="server" CssClass="clientmail-modal" Style="display: none;">
                        <h2 class="page-tone-title" style="font-size: 1.1em; margin: 0 0 10px;">Send Test Email</h2>
                        <p class="small">Send one copy using the first listed contact for names. With Email Test Mode on, it still goes to the test recipient.</p>
                        <asp:Label AssociatedControlID="tbxTestEmail" Text="Send Test To:" runat="server" />
                        <asp:TextBox ID="tbxTestEmail" runat="server" Width="22em" />
                        <div class="button-row" style="margin-top: 12px;">
                            <asp:Button ID="btnSendTestConfirm" Text="Send Test" runat="server" CssClass="filter-panel-btn"
                                OnClick="btnSendTestConfirm_Click"
                                OnClientClick="return clientMailMarkBusy(this, 'Sending...');" />
                            <asp:Button ID="btnTestCancel" Text="Cancel" runat="server" CssClass="filter-panel-btn" CausesValidation="false" />
                        </div>
                    </asp:Panel>

                    <ajaxToolkit:ModalPopupExtender ID="mpeSend" runat="server"
                        TargetControlID="btnDummySend" PopupControlID="pnlSendPopup"
                        CancelControlID="btnSendCancel" BackgroundCssClass="clientmail-modal-bg" />
                    <asp:Panel ID="pnlSendPopup" runat="server" CssClass="clientmail-modal" Style="display: none;">
                        <h2 class="page-tone-title" style="font-size: 1.1em; margin: 0 0 10px;">Send Mail</h2>
                        <p class="small">This sends to every contact currently listed in the results panel.</p>
                        <table class="small">
                            <tr>
                                <td><asp:Label AssociatedControlID="ddlGroupSize" Text="Send In Groups Of:" runat="server" /></td>
                                <td><asp:DropDownList ID="ddlGroupSize" runat="server" /></td>
                            </tr>
                            <tr>
                                <td><asp:Label AssociatedControlID="ddlDelay" Text="Delay Between Emails:" runat="server" /></td>
                                <td><asp:DropDownList ID="ddlDelay" runat="server" /></td>
                            </tr>
                            <tr>
                                <td colspan="2">
                                    <asp:CheckBox ID="chkCcOrdersEmail" runat="server" CssClass="small"
                                        Text="Send CC"
                                        ToolTip="Send CC to the system email from Web.config" />
                                </td>
                            </tr>
                        </table>
                        <div class="button-row" style="margin-top: 12px;">
                            <asp:Button ID="btnSendConfirm" Text="Send Now" runat="server" CssClass="filter-panel-btn"
                                OnClick="btnSendConfirm_Click" />
                            <asp:Button ID="btnSendCancel" Text="Cancel" runat="server" CssClass="filter-panel-btn" CausesValidation="false" />
                        </div>
                    </asp:Panel>
                </div>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnReloadDefaults" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnSaveMessage" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="ddlSavedMessages" EventName="SelectedIndexChanged" />
                <asp:AsyncPostBackTrigger ControlID="btnOpenTest" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnOpenSend" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnSendTestConfirm" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnSendConfirm" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="imgBtnEmailTestMode" EventName="Click" />
                <asp:PostBackTrigger ControlID="btnBack" />
                <asp:PostBackTrigger ControlID="btnExportCsv" />
            </Triggers>
        </asp:UpdatePanel>
    </asp:Panel>

    <asp:Panel ID="pnlFilterCard" runat="server" CssClass="simpleForm page-tone-panel page-tone-clientmail" Style="margin-top: 16px;">
        <div class="page-tone-header tool-card-header">
            <div>
                <h2 class="page-tone-title" style="font-size: 1.15em;">Filters</h2>
                <p class="page-tone-subtitle">Add one condition at a time. Use And / Or, and ( ) to group, for example (Area Is A Or Area Is B) And Last Item Type Is Coffee.</p>
            </div>
        </div>
        <asp:UpdatePanel ID="upnlFilters" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <ContentTemplate>
                <div class="page-tone-toolbar filter-block" style="padding-top: 12px;">
                    <div class="clientmail-builder">
                        <asp:Label AssociatedControlID="ddlField" Text="Filter By:" runat="server" />
                        <asp:DropDownList ID="ddlField" runat="server" CssClass="clientmail-combo" AutoPostBack="true"
                            OnSelectedIndexChanged="ddlField_SelectedIndexChanged" />
                        <asp:Label AssociatedControlID="ddlOperator" Text="Match:" runat="server" />
                        <asp:DropDownList ID="ddlOperator" runat="server" CssClass="clientmail-combo" AutoPostBack="true"
                            OnSelectedIndexChanged="ddlOperator_SelectedIndexChanged" />

                        <asp:PlaceHolder ID="phValueText" runat="server">
                            <asp:TextBox ID="tbxRuleValue" runat="server" Width="14em" />
                        </asp:PlaceHolder>
                        <asp:Panel ID="phValueDate" runat="server" CssClass="clientmail-inline" Style="display: none;">
                            <asp:TextBox ID="tbxRuleDate" runat="server" CssClass="date-field-clean" Width="9em" />
                            <span class="image-button" title="Pick Date">
                                <asp:ImageButton ID="btnCalDate" runat="server"
                                    ImageUrl="~/images/imgButtons/CalendarBtn.png"
                                    AlternateText="Calendar" CausesValidation="false" OnClientClick="return false;" />
                            </span>
                            <ajaxToolkit:CalendarExtender ID="calRuleDate" runat="server"
                                TargetControlID="tbxRuleDate" PopupButtonID="btnCalDate"
                                PopupPosition="BottomLeft" Format="yyyy-MM-dd" />
                        </asp:Panel>
                        <asp:Panel ID="phValueBetween" runat="server" CssClass="clientmail-inline" Style="display: none;">
                            <asp:TextBox ID="tbxRuleDateFrom" runat="server" CssClass="date-field-clean" Width="9em" />
                            <span class="image-button" title="Pick From Date">
                                <asp:ImageButton ID="btnCalDateFrom" runat="server"
                                    ImageUrl="~/images/imgButtons/CalendarBtn.png"
                                    AlternateText="Calendar" CausesValidation="false" OnClientClick="return false;" />
                            </span>
                            <ajaxToolkit:CalendarExtender ID="calRuleDateFrom" runat="server"
                                TargetControlID="tbxRuleDateFrom" PopupButtonID="btnCalDateFrom"
                                PopupPosition="BottomLeft" Format="yyyy-MM-dd" />
                            <asp:Label Text="To" runat="server" AssociatedControlID="tbxRuleDateTo" />
                            <asp:TextBox ID="tbxRuleDateTo" runat="server" CssClass="date-field-clean" Width="9em" />
                            <span class="image-button" title="Pick To Date">
                                <asp:ImageButton ID="btnCalDateTo" runat="server"
                                    ImageUrl="~/images/imgButtons/CalendarBtn.png"
                                    AlternateText="Calendar" CausesValidation="false" OnClientClick="return false;" />
                            </span>
                            <ajaxToolkit:CalendarExtender ID="calRuleDateTo" runat="server"
                                TargetControlID="tbxRuleDateTo" PopupButtonID="btnCalDateTo"
                                PopupPosition="BottomLeft" Format="yyyy-MM-dd" />
                        </asp:Panel>
                        <asp:PlaceHolder ID="phValueDays" runat="server" Visible="false">
                            <asp:TextBox ID="tbxRuleDays" runat="server" Width="4em" />
                            <span class="small">Days</span>
                        </asp:PlaceHolder>
                        <asp:PlaceHolder ID="phValueLookup" runat="server" Visible="false">
                            <asp:DropDownList ID="ddlRuleLookup" runat="server" CssClass="clientmail-combo" />
                        </asp:PlaceHolder>
                        <asp:PlaceHolder ID="phValueCustom" runat="server" Visible="false">
                            <asp:TextBox ID="tbxRuleCustom" runat="server" Width="28em"
                                ToolTip="SQL WHERE fragment. Aliases: c ContactsTbl, u usage, lastOrd last order, rec recurring." />
                        </asp:PlaceHolder>

                        <asp:Label ID="lblJoin" AssociatedControlID="ddlJoin" Text="Join:" runat="server" />
                        <asp:DropDownList ID="ddlJoin" runat="server" CssClass="clientmail-combo" />
                        <asp:Label AssociatedControlID="ddlGroupOpen" Text="(" runat="server" ToolTip="Start a bracketed group" />
                        <asp:DropDownList ID="ddlGroupOpen" runat="server" CssClass="clientmail-combo" ToolTip="Start a bracketed And / Or group" />
                        <asp:Label AssociatedControlID="ddlGroupClose" Text=")" runat="server" ToolTip="End a bracketed group" />
                        <asp:DropDownList ID="ddlGroupClose" runat="server" CssClass="clientmail-combo" ToolTip="End a bracketed And / Or group" />
                        <asp:Button ID="btnAddRule" Text="Add To Filter" runat="server" CssClass="filter-panel-btn"
                            OnClick="btnAddRule_Click" />
                    </div>
                    <asp:Panel ID="pnlCustomTip" runat="server" CssClass="small" Visible="false" Style="margin-top: 6px;">
                        Custom Where example: c.AreaID IN (1,2) — no SELECT, comments, or semicolons.
                    </asp:Panel>

                    <div class="clientmail-rules-wrap">
                    <asp:GridView ID="gvRules" runat="server" CssClass="results-table clientmail-rules-grid" Font-Size="Small"
                        AutoGenerateColumns="False" DataKeyNames="Id" EmptyDataText="No extra conditions yet."
                        OnRowCommand="gvRules_RowCommand">
                        <Columns>
                            <asp:BoundField DataField="Join" HeaderText="Join" ItemStyle-Wrap="false" />
                            <asp:BoundField DataField="Display" HeaderText="Condition" ItemStyle-Wrap="false" />
                            <asp:TemplateField HeaderText="">
                                <ItemStyle Wrap="false" Width="28px" />
                                <ItemTemplate>
                                    <asp:ImageButton ID="btnRemoveRule" runat="server" CommandName="RemoveRule"
                                        CommandArgument='<%# Eval("Id") %>'
                                        ImageUrl="~/images/imgButtons/Delete.gif"
                                        ToolTip="Remove this condition"
                                        CausesValidation="false" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                    </div>

                    <div class="button-row checkup-send-actions clientmail-filter-actions" style="margin-top: 12px;">
                        <asp:Button ID="btnApplyFilters" Text="Apply Filters" runat="server" CssClass="filter-panel-btn"
                            OnClick="btnApplyFilters_Click" ToolTip="Build the recipient list from these conditions"
                            OnClientClick="return clientMailMarkBusy(this, 'Applying...');" />
                        <asp:Button ID="btnClearRules" Text="Clear Conditions" runat="server" CssClass="filter-panel-btn clientmail-btn-clear"
                            OnClick="btnClearRules_Click" CausesValidation="false" />
                        <span class="clientmail-filter-checks">
                        <asp:CheckBox ID="chkEnabledOnly" runat="server" Checked="true" CssClass="small" Text="Enabled Contacts Only" />
                        <asp:CheckBox ID="chkMustHaveEmail" runat="server" Checked="true" CssClass="small" Text="Must Have Email" />
                        </span>
                    </div>
                    <div class="status-message clientmail-filter-status-line" id="pnlFilterStatus" runat="server" style="margin-top: 8px;">
                        <asp:Literal ID="ltrlFilterStatus" runat="server" />
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </asp:Panel>

    <asp:Panel ID="pnlResultsCard" runat="server" CssClass="simpleForm page-tone-panel page-tone-clientmail" Style="margin-top: 16px;">
        <div class="page-tone-header tool-card-header">
            <div>
                <h2 class="page-tone-title" style="font-size: 1.15em;">Contacts Who Will Get This Mail</h2>
                <p class="page-tone-subtitle">Apply Filters to fill this list. Use No to exclude someone from this send only.</p>
            </div>
        </div>
        <asp:UpdatePanel ID="upnlRecipients" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <ContentTemplate>
                <div class="results-container small" style="padding: 12px;">
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
                            <asp:BoundField DataField="LastItemTypes" HeaderText="Last Item Types" />
                            <asp:CheckBoxField DataField="IsRecurring" HeaderText="Recurring" />
                        </Columns>
                        <PagerStyle CssClass="pager-row" />
                    </asp:GridView>
                </div>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnApplyFilters" EventName="Click" />
            </Triggers>
        </asp:UpdatePanel>
    </asp:Panel>
    <script type="text/javascript">
        function clientMailMarkBusy(btn, busyText) {
            if (!btn) return true;
            if (!btn.getAttribute('data-idle-text'))
                btn.setAttribute('data-idle-text', btn.value);
            btn.value = busyText;
            window.setTimeout(function () { btn.disabled = true; }, 10);
            return true;
        }
        function clientMailBindBusy() {
            if (!window.Sys || !Sys.WebForms || !Sys.WebForms.PageRequestManager)
                return;
            var prm = Sys.WebForms.PageRequestManager.getInstance();
            if (prm._clientMailBusyBound)
                return;
            prm._clientMailBusyBound = true;
            prm.add_endRequest(function () {
                var buttons = document.querySelectorAll('.filter-panel-btn[data-idle-text]');
                for (var i = 0; i < buttons.length; i++) {
                    buttons[i].disabled = false;
                    buttons[i].value = buttons[i].getAttribute('data-idle-text');
                    buttons[i].removeAttribute('data-idle-text');
                }
            });
        }
        if (window.Sys && Sys.Application)
            Sys.Application.add_load(clientMailBindBusy);
        else if (document.readyState === 'loading')
            document.addEventListener('DOMContentLoaded', clientMailBindBusy);
        else
            clientMailBindBusy();
    </script>
</asp:Content>
