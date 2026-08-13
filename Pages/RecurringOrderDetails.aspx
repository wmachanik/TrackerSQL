<%@ Page Title="Recurring Order Details" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    MaintainScrollPositionOnPostback="true"
    CodeBehind="RecurringOrderDetails.aspx.cs" Inherits="TrackerSQL.Pages.RecurringOrderDetails" %>

<asp:Content ID="cntRecurringOrderDetailsHdr" title="Recurring Order Details" ContentPlaceHolderID="HeadContent" runat="server">
    <%-- Keep HeadContent free of server code expressions — ScriptManager cannot modify head when it contains them. --%>
    <script type="text/javascript">
        // Lock the clicked save/insert/delete/prompt button, swap its text and show the progress strip.
        // Full postbacks never fire UpdateProgress, hence this.
        function beginRecurringOrderSave(button, savingText, confirmMessage) {
            // Ask first — before unsaved-change checks — so Delete always prompts.
            if (confirmMessage && !window.confirm(confirmMessage)) {
                return false;
            }

            if (typeof recurringOrderAllowNavigate === "function" && !recurringOrderAllowNavigate()) {
                return false;
            }

            if (!button || button.getAttribute("data-saving") === "true") {
                return false;
            }

            button.setAttribute("data-saving", "true");
            button.setAttribute("aria-disabled", "true");
            // Do NOT set disabled=true on the clicked submit control — that can cancel the postback
            // after this script already flipped the UI to "Deleting.../Saving...".
            if (button.value !== undefined && button.tagName === "INPUT") {
                button.value = savingText || "Saving...";
            } else if (button.innerText !== undefined) {
                button.innerText = savingText || "Saving...";
            }

            // Soft-disable siblings for UX only (opacity) — keep them enabled so the form post is safe.
            var row = button.parentNode;
            if (row) {
                var siblings = row.querySelectorAll("input[type='submit'], input[type='button'], button, a");
                for (var i = 0; i < siblings.length; i++) {
                    if (siblings[i] !== button) {
                        siblings[i].style.opacity = "0.5";
                        siblings[i].style.pointerEvents = "none";
                    }
                }
            }

            var savingStatus = document.getElementById("recurringOrderSaving");
            if (savingStatus) {
                var label = savingStatus.querySelector("span");
                if (label) {
                    label.innerHTML = "&nbsp;" + (savingText || "Saving") + ", please wait...";
                }
                savingStatus.style.display = "flex";

                // Escape hatch if the server never navigates (hung postback / lost redirect).
                window.setTimeout(function () {
                    if (!savingStatus || savingStatus.style.display === "none") {
                        return;
                    }
                    var listUrl = savingStatus.getAttribute("data-list-url") || "RecurringOrders.aspx";
                    var escape = savingStatus.querySelector("span");
                    if (escape) {
                        escape.innerHTML = "&nbsp;" + (savingText || "Working")
                            + " is taking a long time. "
                            + "<a href=\"" + listUrl + "\">Return to recurring orders list</a>";
                    }
                }, 15000);
            }

            return true;
        }

        // Full postback Add Line / Confirm Add — UpdateProgress does not run; show strip + clear leave-guard.
        function beginRecurringOrderAddLine(button) {
            return beginRecurringOrderLinePost(button, "Opening...");
        }

        function beginRecurringOrderConfirmAddLine(button) {
            return beginRecurringOrderLinePost(button, "Adding...");
        }

        function beginRecurringOrderLinePost(button, workingText) {
            if (typeof recurringOrderAllowNavigate === "function") {
                recurringOrderAllowNavigate();
            }

            if (!button || button.getAttribute("data-saving") === "true") {
                return false;
            }
            button.setAttribute("data-saving", "true");
            button.setAttribute("aria-disabled", "true");
            if (button.value !== undefined && button.tagName === "INPUT") {
                button.value = workingText || "Please wait...";
            } else if (button.innerText !== undefined) {
                button.innerText = workingText || "Please wait...";
            }

            var savingStatus = document.getElementById("recurringOrderSaving");
            if (savingStatus) {
                var label = savingStatus.querySelector("span");
                if (label) {
                    label.innerHTML = "&nbsp;" + (workingText || "Working") + ", please wait...";
                }
                savingStatus.style.display = "flex";
            }
            return true;
        }
    </script>
</asp:Content>
<asp:Content ID="cntRecurringOrderDetailsBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Label ID="lblReoccuringOrderID" Visible="false" runat="server" />
    <asp:ScriptManager ID="smReoccuringOrderDetails" runat="server" EnablePartialRendering="true" />

    <asp:Panel ID="pnlRecurringOrderDetails" runat="server" CssClass="simpleForm page-tone-panel page-tone-recurring">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/icons8-order-16.png" alt="" />
            <div>
                <h1 class="page-tone-title">Recurring Order Details</h1>
                <p class="page-tone-subtitle">Edit a contact's recurring order lines and schedule</p>
            </div>
        </div>

        <asp:UpdateProgress ID="uprgReoccuringOrderDetails" runat="server"
            AssociatedUpdatePanelID="upnlReoccuringOrderDetails" DisplayAfter="0" DynamicLayout="true">
            <ProgressTemplate>
                <div class="status-message status-info page-tone-progress">
                    <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
                    &nbsp;Please wait...
                </div>
            </ProgressTemplate>
        </asp:UpdateProgress>

        <asp:HiddenField ID="hdnDirty" runat="server" Value="0" />

        <asp:UpdatePanel ID="upnlReoccuringOrderDetails" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnUpdate" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnRevert" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="ddlCompanyName" EventName="SelectedIndexChanged" />
                <asp:PostBackTrigger ControlID="btnAddLine" />
                <asp:PostBackTrigger ControlID="btnConfirmAddLine" />
                <asp:PostBackTrigger ControlID="btnCancelNewLine" />
                <asp:PostBackTrigger ControlID="btnInsert" />
                <asp:PostBackTrigger ControlID="btnUpdateAndReturn" />
                <asp:PostBackTrigger ControlID="btnDelete" />
                <asp:PostBackTrigger ControlID="btnReturn" />
                <asp:PostBackTrigger ControlID="btnInvoiceTypeDeliveryNote" />
                <asp:PostBackTrigger ControlID="btnInvoiceTypeDispatch" />
                <asp:PostBackTrigger ControlID="btnInvoiceTypeKeep" />
                <asp:PostBackTrigger ControlID="btnDisablePromptApply" />
                <asp:PostBackTrigger ControlID="btnDisablePromptSkip" />
            </Triggers>
            <ContentTemplate>
                <asp:HiddenField ID="hdnRecurringOrderId" runat="server" Value="0" />
                <p class="page-tone-subtitle" style="margin-bottom: 12px;">
                    If a contact has both enabled and disabled recurring rows, they appear as separate recurring orders.
                </p>

                <table class="TblMudZebra recurring-order-form" cellpadding="0" cellspacing="0">
                    <tr>
                        <td>Company Name</td>
                        <td colspan="2">
                            <asp:DropDownList ID="ddlCompanyName" runat="server" AppendDataBoundItems="true"
                                EnableViewState="false" AutoPostBack="true"
                                OnSelectedIndexChanged="ddlCompanyName_SelectedIndexChanged">
                                <asp:ListItem Value="0" Text="--- Select Contact Name ---" />
                            </asp:DropDownList>
                        </td>
                        <td>
                            <span class="rightColumn small">
                                <asp:Label ID="ReoccuringOrderIDLabel" runat="server" /></span>
                        </td>
                    </tr>
                    <tr>
                        <td>Delivery By</td>
                        <td>
                            <asp:DropDownList ID="ddlDeliveryBy" runat="server" AppendDataBoundItems="true"
                                EnableViewState="false">
                                <asp:ListItem Value="0" Text="n/a" />
                            </asp:DropDownList>
                        </td>
                        <td>&nbsp;</td>
                        <td>
                            <asp:CheckBox ID="EnabledCheckBox" runat="server" Text="Complete Recurring Order Enabled"
                                ToolTip="Check when this complete recurring order should be enabled" />
                        </td>
                    </tr>
                    <tr>
                        <td>Notes</td>
                        <td colspan="3">
                            <asp:TextBox ID="NotesTextBox" runat="server" TextMode="MultiLine" Height="4em" Width="98%" />
                        </td>
                    </tr>
                    <tr>
                        <td colspan="4">
                            <strong>Recurring Lines</strong><br />
                            <span class="small">Add each recurrence: item, quantity, and dates.
                                <strong>Next Date is auto-calculated</strong> when you save (Last Date + Recurrence pattern).</span>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="4">
                            <asp:GridView ID="gvRecurringOrderItems" runat="server" AutoGenerateColumns="False"
                                CssClass="results-table nested-results-table recurring-order-editor-grid no-sticky-last"
                                GridLines="None"
                                EnableViewState="true"
                                ViewStateMode="Enabled"
                                ShowHeaderWhenEmpty="true"
                                EmptyDataText="No lines yet — click Add Line."
                                OnRowDataBound="gvRecurringOrderItems_RowDataBound"
                                OnRowCommand="gvRecurringOrderItems_RowCommand">
                                <Columns>
                                    <asp:TemplateField HeaderText="&nbsp;"
                                        HeaderStyle-CssClass="col-ro-cmd" ItemStyle-CssClass="col-ro-cmd recurring-order-command-cell"
                                        HeaderStyle-Width="36px" ItemStyle-Width="36px">
                                        <ItemTemplate>
                                            <asp:HiddenField ID="hfRecurringOrderItemID" runat="server"
                                                Value='<%# Eval("RecurringOrderItemID") %>' />
                                            <div class="recurring-order-line-actions">
                                                <%-- Icon + link are siblings (same as Add Line / Recurring Orders).
                                                     Nesting <img> inside the LinkButton breaks the image-button hit target. --%>
                                                <span class="image-button" title="Delete this line">
                                                    <img src="../images/imgButtons/DelItem.gif" alt="" />
                                                    <asp:LinkButton ID="btnDeleteLine" runat="server"
                                                        Text=""
                                                        CommandName="DeleteLine"
                                                        CommandArgument='<%# Eval("RecurringOrderItemID") %>'
                                                        CausesValidation="false"
                                                        ToolTip="Delete this line"
                                                        OnClientClick="return confirm('Remove this line from the recurring order?');" />
                                                </span>
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Item"
                                        HeaderStyle-CssClass="col-ro-item" ItemStyle-CssClass="col-ro-item"
                                        HeaderStyle-Width="26%" ItemStyle-Width="26%">
                                        <ItemTemplate>
                                            <asp:DropDownList ID="ddlItemType" runat="server" AppendDataBoundItems="true"
                                                CssClass="col-ro-ddl" EnableViewState="false">
                                                <asp:ListItem Text="--- Select ---" Value="0" />
                                            </asp:DropDownList>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Qty"
                                        HeaderStyle-CssClass="col-ro-qty" ItemStyle-CssClass="col-ro-qty"
                                        HeaderStyle-Width="56px" ItemStyle-Width="56px"
                                        HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                                        <ItemTemplate>
                                            <asp:TextBox ID="tbxQuantity" runat="server" CssClass="col-ro-narrow-input" Text='<%# Bind("QtyRequired") %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Packaging"
                                        HeaderStyle-CssClass="col-ro-pack" ItemStyle-CssClass="col-ro-pack"
                                        HeaderStyle-Width="14%" ItemStyle-Width="14%">
                                        <ItemTemplate>
                                            <asp:DropDownList ID="ddlPackagingTypes" runat="server" AppendDataBoundItems="true"
                                                CssClass="col-ro-ddl" EnableViewState="false">
                                                <asp:ListItem Text="none" Value="0" />
                                            </asp:DropDownList>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Value"
                                        HeaderStyle-CssClass="col-ro-value" ItemStyle-CssClass="col-ro-value"
                                        HeaderStyle-Width="52px" ItemStyle-Width="52px"
                                        HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                                        <ItemTemplate>
                                            <asp:TextBox ID="tbxValue" runat="server" CssClass="col-ro-narrow-input" Text='<%# Bind("Value") %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Recurrence"
                                        HeaderStyle-CssClass="col-ro-recurrence" ItemStyle-CssClass="col-ro-recurrence"
                                        HeaderStyle-Width="14%" ItemStyle-Width="14%">
                                        <ItemTemplate>
                                            <asp:DropDownList ID="ddlReoccuranceType" runat="server" AppendDataBoundItems="true"
                                                CssClass="col-ro-ddl" EnableViewState="false">
                                                <asp:ListItem Value="0" Text="--- Select ---" />
                                            </asp:DropDownList>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Last Date"
                                        HeaderStyle-CssClass="col-ro-date" ItemStyle-CssClass="col-ro-date"
                                        HeaderStyle-Width="100px" ItemStyle-Width="100px">
                                        <ItemTemplate>
                                            <asp:TextBox ID="tbxLastDate" runat="server" CssClass="col-ro-date-input" Text='<%# Bind("DateLastDone", "{0:yyyy-MM-dd}") %>' />
                                            <ajaxToolkit:CalendarExtender ID="tbxLastDate_CalendarExtender" runat="server"
                                                Enabled="True" TargetControlID="tbxLastDate"></ajaxToolkit:CalendarExtender>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Next Date"
                                        HeaderStyle-CssClass="col-ro-date" ItemStyle-CssClass="col-ro-date"
                                        HeaderStyle-Width="100px" ItemStyle-Width="100px">
                                        <ItemTemplate>
                                            <asp:Label ID="lblNextDate" runat="server" Text='<%# Bind("NextDateRequired", "{0:yyyy-MM-dd}") %>'
                                                CssClass="recurring-order-calculated-date"
                                                ToolTip="Auto-calculated when you save. Based on Last Date + Recurrence pattern." />
                                            <asp:HiddenField ID="hfNextDate" runat="server" Value='<%# Bind("NextDateRequired") %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Until Date"
                                        HeaderStyle-CssClass="col-ro-date" ItemStyle-CssClass="col-ro-date"
                                        HeaderStyle-Width="100px" ItemStyle-Width="100px">
                                        <ItemTemplate>
                                            <asp:TextBox ID="tbxUntilDate" runat="server" CssClass="col-ro-date-input" Text='<%# Bind("RequireUntilDate", "{0:yyyy-MM-dd}") %>' />
                                            <ajaxToolkit:CalendarExtender ID="tbxUntilDate_CalendarExtender" runat="server"
                                                Enabled="True" TargetControlID="tbxUntilDate"></ajaxToolkit:CalendarExtender>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                                <div class="recurring-order-form-toolbar">
                                    <div class="recurring-order-pending-calc" id="divPendingCalc" style="display:none;">
                                        <span class="pending-calc-indicator">Next Date will be calculated when you save</span>
                                    </div>
                                    <span class="image-button recurring-order-add-line" id="spnAddLine" runat="server"
                                        title="Add another recurring line">
                                        <img src="../images/imgButtons/GreenPlus.gif" alt="" />
                                        <asp:LinkButton ID="btnAddLine" runat="server"
                                            Text="Add Line"
                                            OnClick="btnAddLine_Click" CausesValidation="false"
                                            OnClientClick="return beginRecurringOrderAddLine(this);"
                                            ToolTip="Add another recurring line (saved when you click Save)" />
                                    </span>
                                </div>

                                <%-- Always in the tree (CSS-hidden). Visible=false removes Confirm Add mid-async and sticks progress. --%>
                                <asp:Panel ID="pnlNewLine" runat="server" CssClass="new-order-item-form recurring-new-line-form"
                                    style="display: none;">
                                    <table class="TblFlex new-order-item-table recurring-new-line-table" cellpadding="0" cellspacing="0">
                                        <thead>
                                            <tr>
                                                <th>Item</th>
                                                <th>Qty</th>
                                                <th>Packaging</th>
                                                <th>Value</th>
                                                <th>Recurrence</th>
                                                <th>Last Date</th>
                                                <th>Until Date</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            <tr>
                                                <td data-label="Item">
                                                    <asp:DropDownList ID="ddlNewItemType" runat="server" AppendDataBoundItems="true"
                                                        CssClass="col-ro-ddl">
                                                        <asp:ListItem Text="--- Select ---" Value="0" />
                                                    </asp:DropDownList>
                                                </td>
                                                <td data-label="Qty">
                                                    <asp:TextBox ID="tbxNewQuantity" runat="server" CssClass="col-ro-narrow-input" Text="1" />
                                                </td>
                                                <td data-label="Packaging">
                                                    <asp:DropDownList ID="ddlNewPackaging" runat="server" AppendDataBoundItems="true"
                                                        CssClass="col-ro-ddl">
                                                        <asp:ListItem Text="none" Value="0" />
                                                    </asp:DropDownList>
                                                </td>
                                                <td data-label="Value">
                                                    <asp:TextBox ID="tbxNewValue" runat="server" CssClass="col-ro-narrow-input" />
                                                </td>
                                                <td data-label="Recurrence">
                                                    <asp:DropDownList ID="ddlNewRecurrence" runat="server" AppendDataBoundItems="true"
                                                        CssClass="col-ro-ddl">
                                                        <asp:ListItem Value="0" Text="--- Select ---" />
                                                    </asp:DropDownList>
                                                </td>
                                                <td data-label="Last Date">
                                                    <asp:TextBox ID="tbxNewLastDate" runat="server" CssClass="col-ro-date-input" />
                                                    <ajaxToolkit:CalendarExtender ID="calNewLastDate" runat="server"
                                                        Enabled="True" TargetControlID="tbxNewLastDate" Format="yyyy-MM-dd" />
                                                </td>
                                                <td data-label="Until Date">
                                                    <asp:TextBox ID="tbxNewUntilDate" runat="server" CssClass="col-ro-date-input" />
                                                    <ajaxToolkit:CalendarExtender ID="calNewUntilDate" runat="server"
                                                        Enabled="True" TargetControlID="tbxNewUntilDate" Format="yyyy-MM-dd" />
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                    <div class="new-order-item-actions button-row">
                                        <asp:Button ID="btnConfirmAddLine" Text="Add" runat="server" CssClass="filter-panel-btn"
                                            OnClick="btnConfirmAddLine_Click" CausesValidation="false"
                                            OnClientClick="return beginRecurringOrderConfirmAddLine(this);"
                                            ToolTip="Add this line to the recurring order" />
                                        <asp:Button ID="btnCancelNewLine" Text="Cancel" runat="server" CssClass="filter-panel-btn"
                                            OnClick="btnCancelNewLine_Click" CausesValidation="false"
                                            OnClientClick="return beginRecurringOrderLinePost(this, 'Cancelling...');"
                                            ToolTip="Cancel adding a line" />
                                    </div>
                                </asp:Panel>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="4" class="rowOddC">
                            <div class="button-row" style="margin-top: 8px;">
                                <asp:Button ID="btnUpdate" Text="Save" runat="server" CssClass="filter-panel-btn"
                                    OnClick="btnUpdate_Click"
                                    OnClientClick="return beginRecurringOrderSave(this, 'Saving...');"
                                    ToolTip="Save and stay on this page" />
                                <asp:Button ID="btnUpdateAndReturn" Text="Save &amp; Return" runat="server" CssClass="filter-panel-btn"
                                    OnClick="btnUpdateAndReturn_Click"
                                    OnClientClick="return beginRecurringOrderSave(this, 'Saving...');"
                                    ToolTip="Save and return to the list" />
                                <asp:Button ID="btnInsert" Text="Insert" runat="server" CssClass="filter-panel-btn" Enabled="false"
                                    OnClick="btnInsert_Click"
                                    OnClientClick="return beginRecurringOrderSave(this, 'Saving...');"
                                    ToolTip="Save a new recurring order (same as Save &amp; Return)"
                                    Visible="false" />
                                <asp:Button ID="btnDelete" runat="server" Text="Delete" CssClass="filter-panel-btn"
                                    OnClick="btnDelete_Click" CausesValidation="false"
                                    OnClientClick="return beginRecurringOrderSave(this, 'Deleting...', 'Are you sure you want to delete this recurring order?');"
                                    ToolTip="Delete this recurring order" />
                                <asp:Button ID="btnRevert" Text="Revert Changes" runat="server" CssClass="filter-panel-btn"
                                    OnClick="btnRevert_Click"
                                    OnClientClick="return recurringOrderAllowNavigate();"
                                    ToolTip="Reload last saved values" />
                                <span class="image-button" title="Return without saving">
                                    <asp:ImageButton ID="btnReturn" runat="server"
                                        ImageUrl="~/images/imgButtons/Back.gif"
                                        AlternateText="Back"
                                        ToolTip="Return without saving"
                                        OnClick="btnReturn_Click"
                                        CausesValidation="false"
                                        OnClientClick="return recurringOrderConfirmLeave();" />
                                </span>
                            </div>
                        </td>
                    </tr>
                </table>

                <div id="recurringOrderSaving" class="status-message status-info page-tone-progress"
                    style="display: none; margin-top: 12px;" role="status" aria-live="polite"
                    data-list-url="../Pages/RecurringOrders.aspx">
                    <img src="../images/animi/QuaffeeProgress.gif" alt="" />
                    <span>&nbsp;Saving recurring order, please wait...</span>
                </div>

                <div class="status-message" id="pnlStatus" runat="server" style="margin-top: 12px;">
                    <asp:Literal ID="ltrlStatus" runat="server" />
                </div>

                <asp:Panel ID="pnlInvoiceTypePrompt" runat="server" CssClass="confirm-modal-overlay" Visible="false">
                    <div class="confirm-modal">
                        <h2>Contact Account Type</h2>
                        <p><asp:Literal ID="ltrlInvoiceTypePrompt" runat="server" /></p>
                        <div class="confirm-modal-option">
                            <asp:CheckBox ID="chkInsertDisablePrediction" runat="server" Checked="true"
                                Text="If keeping account type: also disable prediction (recommended). Delivery Note / Dispatch Note always disable prediction." />
                        </div>
                        <div class="button-row confirm-modal-buttons">
                            <asp:Button ID="btnInvoiceTypeDeliveryNote" runat="server" Text="Delivery Note"
                                CssClass="filter-panel-btn" OnClick="btnInvoiceTypeDeliveryNote_Click" CausesValidation="false"
                                OnClientClick="return beginRecurringOrderSave(this, 'Saving...');"
                                ToolTip="Change the contact's account type to delivery note" />
                            <asp:Button ID="btnInvoiceTypeDispatch" runat="server" Text="Dispatch Note"
                                CssClass="filter-panel-btn" OnClick="btnInvoiceTypeDispatch_Click" CausesValidation="false"
                                OnClientClick="return beginRecurringOrderSave(this, 'Saving...');"
                                ToolTip="Change the contact's account type to dispatch note" />
                            <asp:Button ID="btnInvoiceTypeKeep" runat="server" Text="No Change"
                                CssClass="filter-panel-btn" OnClick="btnInvoiceTypeKeep_Click" CausesValidation="false"
                                OnClientClick="return beginRecurringOrderSave(this, 'Saving...');"
                                ToolTip="Keep the contact's current account type" />
                        </div>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlDisablePrompt" runat="server" CssClass="confirm-modal-overlay" Visible="false">
                    <div class="confirm-modal">
                        <h2>Recurring Order Disabled</h2>
                        <p><asp:Literal ID="ltrlDisablePrompt" runat="server" /></p>
                        <div class="confirm-modal-option">
                            <asp:CheckBox ID="chkDisableSwitchStandard" runat="server" Checked="true"
                                Text="Switch the contact's account type back to standard" />
                        </div>
                        <div class="confirm-modal-option">
                            <asp:CheckBox ID="chkDisableReenablePrediction" runat="server" Checked="true"
                                Text="Re-enable prediction (clear the contact's 'Prediction disabled' flag)" />
                        </div>
                        <div class="button-row confirm-modal-buttons">
                            <asp:Button ID="btnDisablePromptApply" runat="server" Text="Apply"
                                CssClass="filter-panel-btn" OnClick="btnDisablePromptApply_Click" CausesValidation="false"
                                OnClientClick="return beginRecurringOrderSave(this, 'Saving...');"
                                ToolTip="Apply the ticked changes to the contact" />
                            <asp:Button ID="btnDisablePromptSkip" runat="server" Text="No Changes"
                                CssClass="filter-panel-btn" OnClick="btnDisablePromptSkip_Click" CausesValidation="false"
                                OnClientClick="return beginRecurringOrderSave(this, 'Saving...');"
                                ToolTip="Leave the contact's account type and prediction setting as they are" />
                        </div>
                    </div>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
    </asp:Panel>

    <script type="text/javascript">
        function markNextDateForRecalculation() {
            var pendingCalc = document.getElementById('divPendingCalc');
            if (pendingCalc) {
                pendingCalc.style.display = 'block';
            }
        }

        function attachChangeHandlers() {
            var gridView = document.getElementById('<%= gvRecurringOrderItems.ClientID %>');
            if (!gridView) return;

            var rows = gridView.querySelectorAll('tbody tr');
            rows.forEach(function (row) {
                var tbxLastDate = row.querySelector('input[id*="tbxLastDate"]');
                if (tbxLastDate) {
                    tbxLastDate.addEventListener('change', markNextDateForRecalculation);
                }

                var tbxValue = row.querySelector('input[id*="tbxValue"]');
                if (tbxValue) {
                    tbxValue.addEventListener('change', markNextDateForRecalculation);
                }

                var ddlRecurrenceType = row.querySelector('select[id*="ddlReoccuranceType"]');
                if (ddlRecurrenceType) {
                    ddlRecurrenceType.addEventListener('change', markNextDateForRecalculation);
                }
            });
        }

        if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                attachChangeHandlers();
            });
        }

        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', attachChangeHandlers);
        } else {
            attachChangeHandlers();
        }

        TrackerUnsaved.init({
            dirtyFieldId: '<%= hdnDirty.ClientID %>',
            rootId: '<%= pnlRecurringOrderDetails.ClientID %>',
            leaveMessage: 'You have unsaved changes. Leave without saving?',
            aliases: {
                markDirty: 'recurringOrderMarkDirty',
                clearDirty: 'recurringOrderClearDirty',
                allowNavigate: 'recurringOrderAllowNavigate',
                confirmLeave: 'recurringOrderConfirmLeave'
            }
        });
    </script>
</asp:Content>
