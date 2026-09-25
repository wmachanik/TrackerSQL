<%@ Page Title="My Recurring Orders" Language="C#" MasterPageFile="~/Portal/Portal.Master"
    AutoEventWireup="true" CodeBehind="MyRecurring.aspx.cs" Inherits="TrackerSQL.Portal.MyRecurring" %>

<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server" />
<asp:Content ID="Body" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnl" runat="server" CssClass="simpleForm page-tone-panel page-tone-recurring portal-card">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/icons8-order-16.png" alt="" />
            <div>
                <h1 class="page-tone-title">My Recurring Orders</h1>
                <p class="page-tone-subtitle">Your standing orders — when they were last sent and when they are next due</p>
            </div>
        </div>
        <asp:UpdatePanel ID="upList" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
        <div class="portal-table-wrap">
            <asp:GridView ID="gv" runat="server" CssClass="results-table portal-table"
                AutoGenerateColumns="false" EmptyDataText="No recurring orders found." GridLines="None"
                AllowPaging="true" PageSize="10" OnPageIndexChanging="gv_PageIndexChanging">
                <PagerSettings Mode="NumericFirstLast" Position="Bottom" FirstPageText="First" LastPageText="Last" />
                <PagerStyle CssClass="pager-row" />
                <Columns>
                    <asp:BoundField DataField="ItemDesc" HeaderText="Item" />
                    <asp:BoundField DataField="QtyRequired" HeaderText="Qty" DataFormatString="{0:0.##}"
                        ItemStyle-CssClass="portal-col-num" HeaderStyle-CssClass="portal-col-num" />
                    <asp:BoundField DataField="ItemPackagingDesc" HeaderText="Packaging" />
                    <asp:BoundField DataField="CustomerPatternDisplay" HeaderText="How Often" />
                    <asp:BoundField DataField="DateLastDoneDisplay" HeaderText="Last Date" ItemStyle-CssClass="portal-col-nowrap" />
                    <asp:BoundField DataField="NextDateRequiredDisplay" HeaderText="Next Date" ItemStyle-CssClass="portal-col-nowrap" />
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate><%# StatusBadgeHtml((string)Eval("CustomerStatusDisplay"), (bool?)Eval("Enabled") == false) %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="" ItemStyle-CssClass="portal-col-action" HeaderStyle-CssClass="portal-col-action">
                        <ItemTemplate>
                            <span class="image-button" title="View this recurring order">
                                <img src="../images/imgButtons/View.png" alt="" />
                                <asp:HyperLink runat="server"
                                    NavigateUrl='<%# Eval("RecurringOrderID", "MyRecurringDetail.aspx?RecurringOrderID={0}") %>'
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
