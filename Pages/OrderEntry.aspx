<%@ Page Title="View & Edit Orders" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="OrderEntry.aspx.cs" MaintainScrollPositionOnPostback="true"
    Inherits="TrackerSQL.Pages.OrderEntry" %>

<asp:Content ID="cntOrderEntryHder" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntOrderEntryBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="smOrderEntry" runat="server" EnablePartialRendering="true" />

    <asp:UpdateProgress ID="uprgOrderEntry" runat="server"
        AssociatedUpdatePanelID="upnlOrderEntry" DisplayAfter="0" DynamicLayout="true">
        <ProgressTemplate>
            <div class="status-message status-info page-tone-progress">
                <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
                &nbsp;Please wait...
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:UpdatePanel ID="upnlOrderEntry" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="chkbxOrderDone" EventName="CheckedChanged" />
            <asp:AsyncPostBackTrigger ControlID="btnGo" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnReset" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="tbxSearchFor" EventName="TextChanged" />
            <asp:PostBackTrigger ControlID="btnBack" />
        </Triggers>
        <ContentTemplate>
            <asp:Panel ID="pnlOrderEntry" runat="server" CssClass="simpleForm page-tone-panel page-tone-orders">
                <div class="page-tone-header tool-card-header">
                    <img class="tool-card-icon" src="../images/imgButtons/icons8-view-orders-16.png" alt="" />
                    <div>
                        <h1 class="page-tone-title">View &amp; Edit Orders</h1>
                        <p class="page-tone-subtitle">Manage existing orders and deliveries</p>
                    </div>
                </div>

                <div class="page-tone-toolbar filter-toolbar">
                    <div class="filter-section search-controls">
                        <asp:CheckBox ID="chkbxOrderDone" Text="Show done orders" runat="server"
                            Checked="false" TextAlign="Right" AutoPostBack="true"
                            OnCheckedChanged="chkbxOrderDone_CheckedChanged"
                            ToolTip="When checked, list completed orders instead of open ones" />
                        <div class="filter-control">
                            <asp:Label AssociatedControlID="ddlSearchFor" runat="server" Text="Search for:" CssClass="small" />
                            <asp:DropDownList ID="ddlSearchFor" runat="server">
                                <asp:ListItem Selected="True" Value="none" Text="--Select item--" />
                                <asp:ListItem Value="Company" Text="Company Name" />
                                <asp:ListItem Value="PrepDate" Text="Prep Date" />
                            </asp:DropDownList>
                        </div>
                        <div class="filter-control">
                            <asp:TextBox ID="tbxSearchFor" runat="server" Width="20em"
                                AutoPostBack="true" OnTextChanged="tbxSearchFor_TextChanged"
                                ToolTip="Company name or prep date (yyyy-MM-dd)" />
                        </div>
                        <asp:Button ID="btnGo" Text="Go" runat="server" CssClass="filter-panel-btn"
                            OnClick="btnGo_Click" ToolTip="Apply search filter" />
                        <asp:Button ID="btnReset" Text="Reset" runat="server" CssClass="filter-panel-btn"
                            OnClick="btnReset_Click" ToolTip="Clear search filters" />
                    </div>
                    <div class="filter-section admin-controls">
                        <span class="image-button" title="New order">
                            <img src="../images/imgButtons/GreenPlus.gif" alt="" />
                            <asp:HyperLink ID="btnNewOrder" runat="server" Text="New Order"
                                NavigateUrl="~/Pages/OrderDetail.aspx?NewOrder=true"
                                ToolTip="Create a new order" />
                        </span>
                        <asp:Button ID="btnBack" runat="server" Text="Back" CssClass="filter-panel-btn"
                            OnClick="btnBack_Click" CausesValidation="false" ToolTip="Return to home" />
                    </div>
                </div>

                <div class="results-container" style="margin-top: 8px;">
                    <asp:GridView ID="gvListOfOrders" runat="server" CssClass="results-table"
                        AllowPaging="True" AllowSorting="True" DataSourceID="odsDistinctOrders"
                        DataKeyNames="OrderID"
                        AutoGenerateColumns="False" PageSize="15"
                        EmptyDataText="">
                        <Columns>
                            <asp:TemplateField HeaderText="Open">
                                <ItemTemplate>
                                    <span class="image-button" title="Open order detail">
                                        <img src="../images/imgButtons/icons8-view-orders-16.png" alt="" />
                                        <asp:HyperLink ID="hlOpenOrder" runat="server" Text="Open"
                                            NavigateUrl='<%# Eval("OrderID", "~/Pages/OrderDetail.aspx?OrderID={0}") %>'
                                            ToolTip="Open order to view or edit all lines" />
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Company Name" SortExpression="CompanyName">
                                <ItemTemplate>
                                    <asp:HyperLink ID="hlCompany" runat="server"
                                        Text='<%# Eval("CompanyName") %>'
                                        NavigateUrl='<%# Eval("OrderID", "~/Pages/OrderDetail.aspx?OrderID={0}") %>'
                                        ToolTip="Open order detail" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="OrderDate" HeaderText="Order Date"
                                SortExpression="OrderDate" DataFormatString="{0:d}" ReadOnly="True">
                                <ItemStyle Width="7em" />
                            </asp:BoundField>
                            <asp:BoundField DataField="PrepDate" HeaderText="Prep Date"
                                SortExpression="PrepDate" DataFormatString="{0:d}" ReadOnly="True">
                                <ItemStyle Width="7em" />
                            </asp:BoundField>
                            <asp:BoundField DataField="Person" HeaderText="Person"
                                SortExpression="Person" ReadOnly="True" />
                            <asp:BoundField DataField="RequiredByDate" HeaderText="Required By"
                                SortExpression="RequiredByDate" DataFormatString="{0:d}" ReadOnly="True">
                                <ItemStyle Width="7em" />
                            </asp:BoundField>
                            <asp:TemplateField HeaderText="Item" SortExpression="ItemTypeID">
                                <ItemTemplate>
                                    <asp:Label ID="ItemDescLabel" runat="server"
                                        Text='<%# GetItemDesc((int)Eval("ItemTypeID")) %>'
                                        ToolTip="First line only — open order to see all items" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Qty" SortExpression="QuantityOrdered">
                                <ItemTemplate>
                                    <asp:Label ID="lblQuantityOrdered" runat="server"
                                        Text='<%# FormatQuantity((double)Eval("QuantityOrdered")) %>'
                                        ToolTip="First line only — open order to see all quantities" />
                                </ItemTemplate>
                                <ItemStyle Width="4em" HorizontalAlign="Right" />
                            </asp:TemplateField>
                            <asp:CheckBoxField DataField="Confirmed" HeaderText="Cnfrmd"
                                SortExpression="Confirmed" ReadOnly="True" />
                            <asp:CheckBoxField DataField="Done" HeaderText="Done"
                                SortExpression="Done" ReadOnly="True" />
                            <asp:BoundField DataField="Notes" HeaderText="Notes"
                                SortExpression="Notes" ReadOnly="True" />
                        </Columns>
                        <EmptyDataTemplate>
                            <div class="status-message status-info" style="padding: 16px;">
                                No orders found. Adjust the filter or create a new order.
                            </div>
                        </EmptyDataTemplate>
                        <PagerSettings Mode="NumericFirstLast" Position="Bottom" />
                    </asp:GridView>
                </div>

                <div class="status-message" id="pnlStatus" runat="server" style="margin-top: 12px;" visible="false">
                    <asp:Literal ID="ltrlStatus" runat="server" />
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>

    <asp:ObjectDataSource ID="odsDistinctOrders" runat="server"
        TypeName="TrackerSQL.Managers.OrderEntryDataSource"
        SelectMethod="GetDistinctOrders">
        <SelectParameters>
            <asp:ControlParameter ControlID="chkbxOrderDone" Name="pOrderDone"
                PropertyName="Checked" Type="Boolean" DefaultValue="false" />
            <asp:ControlParameter ControlID="ddlSearchFor" Name="pSearchFor"
                PropertyName="SelectedValue" DefaultValue="" Type="String" />
            <asp:ControlParameter ControlID="tbxSearchFor" Name="pSearchValue"
                PropertyName="Text" Type="String" />
        </SelectParameters>
    </asp:ObjectDataSource>
</asp:Content>
