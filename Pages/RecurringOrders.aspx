<%@ Page Title="Recurring Orders" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="RecurringOrders.aspx.cs"
    Inherits="TrackerSQL.Pages.RecurringOrders" %>

<asp:Content ID="cntRecurringOrdersHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntRecurringOrdersBdy" ContentPlaceHolderID="MainContent" runat="server">
    <h1 class="recurring-orders-page-title">Recurring Orders</h1>
    <asp:ScriptManager ID="smRecurringOrders" runat="server" />
    <asp:UpdateProgress ID="uprgRecurringOrders" runat="server" DisplayAfter="0">
        <ProgressTemplate>
            <img src="../images/animi/BlueArrowsUpdate.gif" alt="updating" width="16" height="16" />updating.....
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:UpdatePanel ID="upnlSelection" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="filter-toolbar recurring-orders-toolbar">
                <div class="filter-section search-controls">
                    <div class="filter-control">
                        <asp:Label AssociatedControlID="ddlFilterBy" runat="server" Text="Filter by:" />
                        <asp:DropDownList ID="ddlFilterBy" runat="server" ToolTip="select which item to search for">
                            <asp:ListItem Value="0" Selected="True" Text="none" />
                            <asp:ListItem Value="CompanyName" Text="Company Name" />
                        </asp:DropDownList>
                    </div>
                    <div class="filter-control">
                        <asp:TextBox ID="tbxFilterBy" runat="server" ToolTip="enter a company name"
                            AutoPostBack="True"
                            OnTextChanged="tbxFilterBy_TextChanged" />
                    </div>
                    <asp:Button ID="btnGo" Text="Go" runat="server" OnClick="btnGo_Click" ToolTip="search for this item" />
                    <asp:Button ID="btnReset" Text="Reset" runat="server" OnClick="btnReset_Click" />
                </div>
                <div class="filter-section admin-controls">
                    <asp:Button ID="btnCalcNextRequired" Text="Calc Next Required" runat="server"
                        OnClick="btnCalcNextRequired_Click" CssClass="recurring-orders-calc-button" />
                    <div class="filter-control" style="margin-right: 12px">
                        <asp:DropDownList ID="ddlEnabledFilter" runat="server" AutoPostBack="True"
                            OnSelectedIndexChanged="ddlEnabledFilter_SelectedIndexChanged">
                            <asp:ListItem Selected="True" Value="1" Text="enabled only" />
                            <asp:ListItem Value="0" Text="disabled only" />
                            <asp:ListItem Value="-1" Text="both" />
                        </asp:DropDownList>
                    </div>
                    <div class="filter-section action-buttons">
                        <asp:HyperLink ID="hlAddRecurringOrder" ImageUrl="~/images/imgButtons/AddItem.gif" ToolTip="Add Recurring Order"
                            CssClass="recurring-orders-add-link"
                            NavigateUrl="~/Pages/RecurringOrderDetails.aspx" runat="server" />
                    </div>
                </div>
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="tbxFilterBy" EventName="TextChanged" />
            <asp:AsyncPostBackTrigger ControlID="btnGo" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnReset" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnCalcNextRequired" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="ddlEnabledFilter" EventName="SelectedIndexChanged" />
        </Triggers>
    </asp:UpdatePanel>
    <br />
    <asp:UpdatePanel ID="upnlRecurringOrdersSummary" runat="server">
        <ContentTemplate>
            <div class="results-container" style="padding-left: 1em; padding-right: 1em">
                <asp:GridView ID="gvRecurringOrders" runat="server" AutoGenerateColumns="False" CssClass="results-table recurring-orders-groups-table"
                    AllowSorting="True" AllowPaging="True" PageSize="20" ShowHeader="False"
                    OnPageIndexChanging="gvRecurringOrders_PageIndexChanging"
                    OnRowCommand="gvRecurringOrders_RowCommand"
                    OnSorting="gvRecurringOrders_Sorting"
                    OnRowDataBound="gvRecurringOrders_RowDataBound">
                    <EmptyDataTemplate>
                        <div class="simpleLightBrownForm">
                            <h2>No recurring orders found</h2>
                            Either change the filter or add a recurring order.
                        </div>
                    </EmptyDataTemplate>
                    <Columns>
                        <asp:TemplateField HeaderText="Recurring Orders" SortExpression="CompanyName"
                            HeaderStyle-CssClass="col-priority-1" ItemStyle-CssClass="col-priority-1 wrap">
                            <ItemTemplate>
                                <div class="recurring-order-group">
                                    <div class="recurring-order-group-title">
                                        <div class="recurring-order-group-title-main">
                                            <asp:HyperLink ID="hlEditRecurringOrder" runat="server"
                                                CssClass="recurring-order-edit-link"
                                                ImageUrl="~/images/imgButtons/EditItem.gif"
                                                ToolTip="Edit Recurring Order" />
                                            <asp:HyperLink ID="hlContactDetails" runat="server"
                                                CssClass="recurring-order-company-link"
                                                Visible="false" />
                                            <asp:Label ID="lblCompanyName" runat="server"
                                                CssClass="recurring-order-company-label"
                                                Visible="false" />
                                        </div>
                                        <div class="recurring-order-group-title-meta">
                                            <asp:Label ID="lblRecurringOrderStatus" runat="server" CssClass="recurring-order-status-badge"
                                                Text="" />
                                            <asp:ImageButton ID="btnDeleteRecurringOrder" runat="server"
                                                CssClass="recurring-order-delete-link"
                                                ImageUrl="~/images/imgButtons/DelItem.gif"
                                                AlternateText="Delete"
                                                ToolTip="Delete this complete recurring order"
                                                CommandName="DeleteRecurringOrder"
                                                CommandArgument='<%# Eval("RecurringOrderID") %>'
                                                CausesValidation="false"
                                                OnClientClick="return confirm('Are you sure you want to delete this complete recurring order?');" />
                                            <asp:Label ID="lblRecurringOrderCount" runat="server" CssClass="recurring-order-group-count"
                                                Text="" />
                                        </div>
                                    </div>
                                    <asp:GridView ID="gvRecurringOrdersForContact" runat="server" AutoGenerateColumns="False"
                                        CssClass="results-table recurring-orders-detail-table no-sticky-last" GridLines="None"
                                        ShowHeader="True">
                                        <Columns>
                                            <asp:BoundField DataField="ItemDesc" HeaderText="Item"
                                                HeaderStyle-CssClass="wrap col-priority-1" ItemStyle-CssClass="wrap col-priority-1" />
                                            <asp:BoundField DataField="QtyRequired" HeaderText="Qty" DataFormatString="{0:0.##}"
                                                HeaderStyle-CssClass="col-tight col-priority-1" ItemStyle-CssClass="col-tight col-priority-1" />
                                            <asp:BoundField DataField="ItemPackagingDesc" HeaderText="Packaging"
                                                HeaderStyle-CssClass="wrap col-priority-3" ItemStyle-CssClass="wrap col-priority-3" />
                                            <asp:BoundField DataField="Value" HeaderText="Value"
                                                HeaderStyle-CssClass="col-tight col-priority-2" ItemStyle-CssClass="col-tight col-priority-2" />
                                            <asp:BoundField DataField="RecurringTypeDesc" HeaderText="Recurrance"
                                                HeaderStyle-CssClass="col-tight col-priority-2" ItemStyle-CssClass="col-tight col-priority-2" />
                                            <asp:BoundField DataField="DateLastDone" HeaderText="Last Date" DataFormatString="{0:yyyy-MM-dd}"
                                                HeaderStyle-CssClass="col-tight col-priority-4" ItemStyle-CssClass="col-tight col-priority-4" />
                                            <asp:BoundField DataField="NextDateRequired" HeaderText="Next Date" DataFormatString="{0:yyyy-MM-dd}"
                                                HeaderStyle-CssClass="col-tight col-priority-1" ItemStyle-CssClass="col-tight col-priority-1 text-right" />
                                        </Columns>
                                    </asp:GridView>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnGo" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnReset" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnCalcNextRequired" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="ddlEnabledFilter" EventName="SelectedIndexChanged" />
        </Triggers>
    </asp:UpdatePanel>

    <br />
    <asp:UpdatePanel runat="server" UpdateMode="Always">
        <ContentTemplate>
            <asp:Label ID="lblFilter" Text="" runat="server" />
        </ContentTemplate>
    </asp:UpdatePanel>

</asp:Content>
