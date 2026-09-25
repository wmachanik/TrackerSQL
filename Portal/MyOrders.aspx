<%@ Page Title="My Orders" Language="C#" MasterPageFile="~/Portal/Portal.Master"
    AutoEventWireup="true" CodeBehind="MyOrders.aspx.cs" Inherits="TrackerSQL.Portal.MyOrders" %>
<%@ Import Namespace="TrackerSQL.Models" %>

<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server" />
<asp:Content ID="Body" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnl" runat="server" CssClass="simpleForm page-tone-panel page-tone-orders portal-card">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/icons8-view-orders-16.png" alt="" />
            <div>
                <h1 class="page-tone-title">My Orders</h1>
                <p class="page-tone-subtitle">Your orders, newest delivery first</p>
            </div>
        </div>
        <asp:Literal ID="litNextCoffee" runat="server" />
        <asp:UpdatePanel ID="upOrders" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
        <div class="portal-table-wrap">
            <asp:GridView ID="gvOrders" runat="server" CssClass="results-table portal-table"
                AutoGenerateColumns="false" EmptyDataText="No orders found." GridLines="None"
                AllowPaging="true" PageSize="10" OnPageIndexChanging="gvOrders_PageIndexChanging">
                <PagerSettings Mode="NumericFirstLast" Position="Bottom" FirstPageText="First" LastPageText="Last" />
                <PagerStyle CssClass="pager-row" />
                <Columns>
                    <asp:BoundField DataField="OrderDate" HeaderText="Order Date" DataFormatString="{0:dd MMM yyyy}" ItemStyle-CssClass="portal-col-nowrap" HeaderStyle-CssClass="portal-col-nowrap" />
                    <asp:BoundField DataField="RequiredByDate" HeaderText="Delivery / Dispatch Date" DataFormatString="{0:dd MMM yyyy}" ItemStyle-CssClass="portal-col-nowrap" HeaderStyle-CssClass="portal-col-nowrap" />
                    <asp:TemplateField HeaderText="Items" ItemStyle-CssClass="portal-col-fill">
                        <ItemTemplate><%# ItemLinesHtml((ContactOrderSummary)Container.DataItem) %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="PurchaseOrder" HeaderText="PO" ItemStyle-CssClass="portal-col-nowrap" />
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate><%# StatusBadgeHtml((string)Eval("CustomerStatusDisplay"), (bool)Eval("Done")) %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="" ItemStyle-CssClass="portal-col-action" HeaderStyle-CssClass="portal-col-action">
                        <ItemTemplate>
                            <span class="image-button" title="View this order">
                                <img src="../images/imgButtons/View.png" alt="" />
                                <asp:HyperLink runat="server"
                                    NavigateUrl='<%# Eval("OrderID", "MyOrder.aspx?OrderID={0}") %>'
                                    Text="View" />
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </asp:Panel>
</asp:Content>
