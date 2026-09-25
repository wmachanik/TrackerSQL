<%@ Page Title="Order Details" Language="C#" MasterPageFile="~/Portal/Portal.Master"
    AutoEventWireup="true" CodeBehind="MyOrder.aspx.cs" Inherits="TrackerSQL.Portal.MyOrder" %>

<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server" />
<asp:Content ID="Body" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnl" runat="server" CssClass="simpleForm page-tone-panel page-tone-orders portal-card">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/icons8-view-orders-16.png" alt="" />
            <div>
                <h1 class="page-tone-title">Order Details</h1>
                <p class="page-tone-subtitle">Need something changed on this order? Use Request A Change below.</p>
            </div>
        </div>

        <div class="portal-form-grid">
            <div class="portal-field">
                <span class="portal-field-label">Order Date</span>
                <span class="portal-field-value"><asp:Literal ID="litOrderDate" runat="server" /></span>
            </div>
            <div class="portal-field">
                <span class="portal-field-label">Delivery / Dispatch Date</span>
                <span class="portal-field-value"><asp:Literal ID="litDelivery" runat="server" /></span>
            </div>
            <div class="portal-field">
                <span class="portal-field-label">Purchase Order (PO)</span>
                <span class="portal-field-value"><asp:Literal ID="litPO" runat="server" /></span>
            </div>
            <div class="portal-field">
                <span class="portal-field-label">Status</span>
                <span class="portal-field-value"><asp:Literal ID="litStatus" runat="server" /></span>
            </div>
            <asp:PlaceHolder ID="phShop" runat="server" Visible="false">
                <div class="portal-field">
                    <span class="portal-field-label">Online Shop Order</span>
                    <span class="portal-field-value"><asp:Literal ID="litShopOrder" runat="server" /></span>
                </div>
                <asp:PlaceHolder ID="phShopView" runat="server">
                    <div class="portal-field portal-field-action">
                        <span class="portal-field-label">&nbsp;</span>
                        <span class="image-button" title="Open this order in your Quaffee shop account">
                            <img src="../images/imgButtons/View.png" alt="" />
                            <asp:HyperLink ID="hlShopView" runat="server" Target="_blank" Text="View In Online Shop" />
                        </span>
                    </div>
                </asp:PlaceHolder>
            </asp:PlaceHolder>
            <div class="portal-field portal-field-span-4">
                <span class="portal-field-label">Notes</span>
                <span class="portal-field-value"><asp:Literal ID="litNotes" runat="server" /></span>
            </div>
        </div>

        <asp:PlaceHolder ID="phCourier" runat="server" Visible="false">
            <h2 class="sys-prefs-section-title">Courier Details</h2>
            <div class="portal-form-grid">
                <div class="portal-field">
                    <span class="portal-field-label">Courier</span>
                    <span class="portal-field-value"><asp:Literal ID="litCourier" runat="server" /></span>
                </div>
                <div class="portal-field">
                    <span class="portal-field-label">Waybill / Tracking Number</span>
                    <span class="portal-field-value"><asp:Literal ID="litWaybill" runat="server" /></span>
                </div>
                <div class="portal-field">
                    <span class="portal-field-label">Dispatched</span>
                    <span class="portal-field-value"><asp:Literal ID="litDispatched" runat="server" /></span>
                </div>
                <asp:PlaceHolder ID="phTrack" runat="server">
                    <div class="portal-field portal-field-action">
                        <span class="portal-field-label">&nbsp;</span>
                        <span class="image-button" title="Track this parcel on the courier's website">
                            <img src="../images/imgButtons/View.png" alt="" />
                            <asp:HyperLink ID="hlTrack" runat="server" Target="_blank" Text="Track Parcel" />
                        </span>
                    </div>
                </asp:PlaceHolder>
            </div>
        </asp:PlaceHolder>

        <h2 class="sys-prefs-section-title">Order Items</h2>
        <div class="portal-table-wrap">
            <asp:GridView ID="gvLines" runat="server" CssClass="results-table portal-table"
                AutoGenerateColumns="false" EmptyDataText="No items on this order." GridLines="None">
                <Columns>
                    <asp:BoundField DataField="ItemDesc" HeaderText="Item" />
                    <asp:BoundField DataField="Qty" HeaderText="Qty" DataFormatString="{0:0.##}"
                        ItemStyle-CssClass="portal-col-num" HeaderStyle-CssClass="portal-col-num" />
                    <asp:BoundField DataField="PackagingDesc" HeaderText="Packaging" />
                </Columns>
            </asp:GridView>
        </div>
    </asp:Panel>

    <asp:UpdatePanel ID="upRequest" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:Panel ID="pnlRequest" runat="server" CssClass="simpleForm page-tone-panel page-tone-users portal-card">
                <div class="page-tone-header tool-card-header">
                    <img class="tool-card-icon" src="../images/imgButtons/EditItem.gif" alt="" />
                    <div>
                        <h2 class="page-tone-title">Request A Change</h2>
                        <p class="page-tone-subtitle">Tell us what should change on this order and we will update it.</p>
                    </div>
                </div>
                <asp:TextBox ID="txtRequest" runat="server" TextMode="MultiLine" Rows="4" CssClass="sys-prefs-input" />
                <div class="button-row">
                    <asp:Button ID="btnRequest" runat="server" Text="Submit Change Request" CssClass="filter-panel-btn"
                        OnClick="btnRequest_Click" />
                    <asp:Button ID="btnBack" runat="server" Text="Back To My Orders" CssClass="filter-panel-btn"
                        OnClick="btnBack_Click" CausesValidation="false" />
                </div>
                <asp:Literal ID="litMessage" runat="server" />
            </asp:Panel>
        </ContentTemplate>
        <Triggers>
            <asp:PostBackTrigger ControlID="btnBack" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>
