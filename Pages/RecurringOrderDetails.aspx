<%@ Page Title="Recurring Order Details" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    MaintainScrollPositionOnPostback="true"
    CodeBehind="RecurringOrderDetails.aspx.cs" Inherits="TrackerSQL.Pages.RecurringOrderDetails" %>

<asp:Content ID="cntRecurringOrderDetailsHdr" ContentPlaceHolderID="HeadContent" runat="server">
    <%-- Keep HeadContent free of <%= %> — ScriptManager cannot modify <head> when it contains code blocks. --%>
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
                <asp:AsyncPostBackTrigger ControlID="btnAddLine" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnConfirmAddLine" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnCancelNewLine" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnRevert" EventName="Click" />
                <asp:PostBackTrigger ControlID="btnInsert" />
                <asp:PostBackTrigger ControlID="btnUpdateAndReturn" />
                <asp:PostBackTrigger ControlID="btnDelete" />
                <asp:PostBackTrigger ControlID="btnReturn" />
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
                                EnableViewState="false">
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
                                                <span class="image-button" title="Delete this line">
                                                    <asp:LinkButton ID="btnDeleteLine" runat="server"
                                                        CommandName="DeleteLine"
                                                        CommandArgument='<%# Eval("RecurringOrderItemID") %>'
                                                        CausesValidation="false"
                                                        ToolTip="Delete this line"
                                                        OnClientClick="return confirm('Remove this line from the recurring order?');">
                                                        <img src="../images/imgButtons/DelItem.gif" alt="Delete" />
                                                    </asp:LinkButton>
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
                                            ToolTip="Add another recurring line" />
                                    </span>
                                </div>

                                <asp:Panel ID="pnlNewLine" runat="server" CssClass="new-order-item-form recurring-new-line-form" Visible="false">
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
                                            ToolTip="Add this line to the recurring order" />
                                        <asp:Button ID="btnCancelNewLine" Text="Cancel" runat="server" CssClass="filter-panel-btn"
                                            OnClick="btnCancelNewLine_Click" CausesValidation="false"
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
                                    OnClientClick="return recurringOrderAllowNavigate();"
                                    ToolTip="Save and stay on this page" />
                                <asp:Button ID="btnUpdateAndReturn" Text="Save &amp; Return" runat="server" CssClass="filter-panel-btn"
                                    OnClick="btnUpdateAndReturn_Click"
                                    OnClientClick="return recurringOrderAllowNavigate();"
                                    ToolTip="Save and return to the list" />
                                <asp:Button ID="btnInsert" Text="Insert" runat="server" CssClass="filter-panel-btn" Enabled="false"
                                    OnClick="btnInsert_Click"
                                    OnClientClick="return recurringOrderAllowNavigate();"
                                    ToolTip="Insert a new recurring order" />
                                <asp:Button ID="btnDelete" runat="server" Text="Delete" CssClass="filter-panel-btn"
                                    OnClick="btnDelete_Click"
                                    OnClientClick="return recurringOrderAllowNavigate() && confirm('Are you sure you want to delete this recurring order?');"
                                    ToolTip="Delete this recurring order" />
                                <asp:Button ID="btnRevert" Text="Revert Changes" runat="server" CssClass="filter-panel-btn"
                                    OnClick="btnRevert_Click"
                                    OnClientClick="return recurringOrderAllowNavigate();"
                                    ToolTip="Reload last saved values" />
                                <asp:Button ID="btnReturn" Text="Back" runat="server" CssClass="filter-panel-btn"
                                    OnClick="btnReturn_Click" CausesValidation="false"
                                    OnClientClick="return recurringOrderConfirmLeave();"
                                    ToolTip="Return without saving" />
                            </div>
                        </td>
                    </tr>
                </table>

                <div class="status-message" id="pnlStatus" runat="server" style="margin-top: 12px;">
                    <asp:Literal ID="ltrlStatus" runat="server" />
                </div>
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
