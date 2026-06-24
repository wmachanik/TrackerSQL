<%@ Page Title="Recurring Order Details" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="RecurringOrderDetails.aspx.cs" Inherits="TrackerSQL.Pages.RecurringOrderDetails" %>

<asp:Content ID="cntRecurringOrderDetailsHdr" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        function markNextDateForRecalculation() {
            // Show the pending calculation indicator below the grid
            var pendingCalc = document.getElementById('divPendingCalc');
            if (pendingCalc) {
                pendingCalc.style.display = 'block';
            }
        }

        function attachChangeHandlers() {
            // Get the GridView table
            var gridView = document.getElementById('<%= gvRecurringOrderItems.ClientID %>');
            if (!gridView) return;

            // Attach handlers to all relevant fields
            var rows = gridView.querySelectorAll('tbody tr');
            rows.forEach(function(row) {
                // Last Date textbox
                var tbxLastDate = row.querySelector('input[id*="tbxLastDate"]');
                if (tbxLastDate) {
                    tbxLastDate.addEventListener('change', markNextDateForRecalculation);
                }

                // Value textbox
                var tbxValue = row.querySelector('input[id*="tbxValue"]');
                if (tbxValue) {
                    tbxValue.addEventListener('change', markNextDateForRecalculation);
                }

                // Recurrence Type dropdown
                var ddlReoccuranceType = row.querySelector('select[id*="ddlReoccuranceType"]');
                if (ddlReoccuranceType) {
                    ddlReoccuranceType.addEventListener('change', markNextDateForRecalculation);
                }
            });
        }

        // Attach handlers when page loads
        if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            var prm = Sys.WebForms.PageRequestManager.getInstance();
            prm.add_endRequest(function() {
                attachChangeHandlers();
            });
        }

        // Also attach on initial page load
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', attachChangeHandlers);
        } else {
            attachChangeHandlers();
        }
    </script>
</asp:Content>
<asp:Content ID="cntRecurringOrderDetailsBdy" ContentPlaceHolderID="MainContent" runat="server">
    <h2 class="InputFrm">Recurring Order Details</h2>
    <div class="recurring-order-header-note">
        This page edits a contacts recurring order. If a contact has both enabled and disabled recurring rows, they can appear under separate recurring orders.
    </div>
    <asp:Label ID="lblReoccuringOrderID" Visible="false" runat="server" />
    <asp:ScriptManager ID="smReoccuringOrderDetails" runat="server">
    </asp:ScriptManager>
    <asp:UpdateProgress ID="uprgReoccuringOrderDetails" runat="server" AssociatedUpdatePanelID="upnlReoccuringOrderDetails">
        <ProgressTemplate>
            <img src="../images/animi/BlueArrowsUpdate.gif" alt="updating" width="16" height="16" />updating.....
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:UpdatePanel ID="upnlReoccuringOrderDetails" runat="server">
        <ContentTemplate>
            <table class="TblMudZebra recurring-order-form" cellpadding="0" cellspacing="4" style="font-size: large; width: 100%">
                <tr>
                    <td>Company Name</td>
                    <td colspan="2">
                        <asp:DropDownList ID="ddlCompanyName" runat="server" AppendDataBoundItems="true">
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
                        <asp:DropDownList ID="ddlDeliveryBy" runat="server" AppendDataBoundItems="true">
                            <asp:ListItem Value="0" Text="n/a" />
                        </asp:DropDownList>
                    </td>
                    <td>&nbsp;</td>
                    <td>
                        <asp:CheckBox ID="EnabledCheckBox" runat="server" Text="Complete Recurring Order Enabled" ToolTip="Check this box when the complete recurring order should be enabled" />
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
                        <span class="small">Add each recurrence:  item, quantity, and dates here. <strong>Next Date is auto-calculated</strong> when you save based on Last Date + Recurrence pattern.</span>
                    </td>
                </tr>
                <tr>
                    <td colspan="4">
                        <asp:GridView ID="gvRecurringOrderItems" runat="server" AutoGenerateColumns="False"
                            CssClass="results-table recurring-orders-detail-table recurring-order-editor-grid no-sticky-last"
                            GridLines="None"
                            OnRowDataBound="gvRecurringOrderItems_RowDataBound"
                            OnRowCommand="gvRecurringOrderItems_RowCommand">
                            <Columns>
                                <asp:TemplateField HeaderText="&nbsp;" HeaderStyle-CssClass="col-tight" ItemStyle-CssClass="recurring-order-command-cell">
                                    <ItemTemplate>
                                        <asp:HiddenField ID="hfRecurringOrderItemID" runat="server" Value='<%# Bind("RecurringOrderItemID") %>' />
                                        <div class="recurring-order-line-actions">
                                            <asp:ImageButton ID="btnDeleteLine" runat="server" ImageUrl="~/images/imgButtons/DelItem.gif"
                                                AlternateText="Delete" ToolTip="Delete this line" CommandName="DeleteLine"
                                                CommandArgument='<%# Container.DataItemIndex %>' CausesValidation="false" />
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Item">
                                    <ItemTemplate>
                                        <asp:DropDownList ID="ddlItemType" runat="server" AppendDataBoundItems="true" Width="220px">
                                            <asp:ListItem Text="--- Select ---" Value="0" />
                                        </asp:DropDownList>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Qty">
                                    <ItemTemplate>
                                        <asp:TextBox ID="tbxQuantity" runat="server" Width="60px" Text='<%# Bind("QtyRequired") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Packaging">
                                    <ItemTemplate>
                                        <asp:DropDownList ID="ddlPackagingTypes" runat="server" AppendDataBoundItems="true" Width="140px">
                                            <asp:ListItem Text="none" Value="0" />
                                        </asp:DropDownList>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Value">
                                    <ItemTemplate>
                                        <asp:TextBox ID="tbxValue" runat="server" Width="50px" Text='<%# Bind("Value") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Recurrance">
                                    <ItemTemplate>
                                        <asp:DropDownList ID="ddlReoccuranceType" runat="server" AppendDataBoundItems="true" Width="140px">
                                            <asp:ListItem Value="0" Text="--- Select ---" />
                                        </asp:DropDownList>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Last Date">
                                    <ItemTemplate>
                                        <asp:TextBox ID="tbxLastDate" runat="server" Width="95px" Text='<%# Bind("DateLastDone", "{0:yyyy-MM-dd}") %>' />
                                        <ajaxToolkit:CalendarExtender ID="tbxLastDate_CalendarExtender" runat="server"
                                            Enabled="True" TargetControlID="tbxLastDate"></ajaxToolkit:CalendarExtender>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Next Date">
                                    <ItemTemplate>
                                        <asp:Label ID="lblNextDate" runat="server" Text='<%# Bind("NextDateRequired", "{0:yyyy-MM-dd}") %>' 
                                            CssClass="recurring-order-calculated-date" 
                                            ToolTip="Auto-calculated when you save. Based on Last Date + Recurrence pattern." />
                                        <asp:HiddenField ID="hfNextDate" runat="server" Value='<%# Bind("NextDateRequired") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Until Date">
                                    <ItemTemplate>
                                        <asp:TextBox ID="tbxUntilDate" runat="server" Width="95px" Text='<%# Bind("RequireUntilDate", "{0:yyyy-MM-dd}") %>' />
                                        <ajaxToolkit:CalendarExtender ID="tbxUntilDate_CalendarExtender" runat="server"
                                            Enabled="True" TargetControlID="tbxUntilDate"></ajaxToolkit:CalendarExtender>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                        <div class="recurring-order-form-toolbar">
                            <div class="recurring-order-pending-calc" id="divPendingCalc" style="display:none;">
                                <span class="pending-calc-indicator">? Next Date will be calculated when you update</span>
                            </div>
                            <asp:Button ID="btnAddLine" runat="server" Text="Add Line" OnClick="btnAddLine_Click" CausesValidation="false" />
                        </div>
                    </td>
                </tr>
                <tr>
                    <td colspan="4" class="rowOddC">
                        <asp:Button ID="btnUpdate" Text="Update" runat="server" OnClick="btnUpdate_Click" />
                        &nbsp;&nbsp;&nbsp;
                        <asp:Button ID="btnUpdateAndReturn" Text="Update & Return" runat="server" OnClick="btnUpdateAndReturn_Click" />
                        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                        <asp:Button ID="btnInsert" Text="Insert" runat="server" Enabled="false" OnClick="btnInsert_Click" />
                        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                        <asp:Button ID="btnDelete" runat="server" Text="Delete" OnClick="btnDelete_Click"
                            OnClientClick="return confirm('Are you sure you want to delete this recurring order?');" />
                        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                        <asp:Button ID="btnRevert" Text="Revert Changes" runat="server" OnClick="btnRevert_Click" />
                        &nbsp;&nbsp;&nbsp;
                        <asp:Button ID="btnReturn" Text="Return" runat="server" OnClick="btnReturn_Click" />
                    </td>
                </tr>
            </table>
            <div class="status-message">
                <asp:Literal ID="ltrlStatus" Text="" runat="server" /></div>
        </ContentTemplate>
    </asp:UpdatePanel>
    <br />
</asp:Content>
