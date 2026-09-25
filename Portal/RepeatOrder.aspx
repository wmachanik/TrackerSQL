<%@ Page Title="Repeat My Last Order" Language="C#" MasterPageFile="~/Portal/Portal.Master"
    AutoEventWireup="true" CodeBehind="RepeatOrder.aspx.cs" Inherits="TrackerSQL.Portal.RepeatOrder" %>

<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server" />
<asp:Content ID="Body" ContentPlaceHolderID="MainContent" runat="server">
    <asp:UpdatePanel ID="upRepeat" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:Panel ID="pnl" runat="server" CssClass="simpleForm page-tone-panel page-tone-orders portal-card">
                <div class="page-tone-header tool-card-header">
                    <img class="tool-card-icon" src="../images/imgButtons/icons8-order-16.png" alt="" />
                    <div>
                        <h1 class="page-tone-title">Repeat My Last Order</h1>
                        <p class="page-tone-subtitle">We will send the same items as your last order on the next delivery / dispatch date for your area. Want something different? Tell us below.</p>
                    </div>
                </div>

                <asp:PlaceHolder ID="phForm" runat="server">
                    <div class="portal-form-grid">
                        <div class="portal-field">
                            <span class="portal-field-label">Delivery / Dispatch Date</span>
                            <span class="portal-field-value"><asp:Literal ID="litDelivery" runat="server" /></span>
                        </div>
                        <div class="portal-field">
                            <asp:Label ID="lblPO" runat="server" AssociatedControlID="txtPO" CssClass="portal-field-label" Text="Purchase Order (PO)" />
                            <asp:TextBox ID="txtPO" runat="server" MaxLength="30" CssClass="sys-prefs-input" />
                        </div>
                    </div>

                    <asp:Literal ID="litExisting" runat="server" />

                    <h2 class="sys-prefs-section-title">Items</h2>
                    <div class="portal-table-wrap">
                        <asp:GridView ID="gvLines" runat="server" CssClass="results-table portal-table"
                            AutoGenerateColumns="false" EmptyDataText="We could not find a previous order to repeat." GridLines="None">
                            <Columns>
                                <asp:BoundField DataField="ItemDesc" HeaderText="Item" />
                                <asp:BoundField DataField="Qty" HeaderText="Qty" DataFormatString="{0:0.##}"
                                    ItemStyle-CssClass="portal-col-num" HeaderStyle-CssClass="portal-col-num" />
                                <asp:BoundField DataField="PackagingDesc" HeaderText="Packaging" />
                            </Columns>
                        </asp:GridView>
                    </div>

                    <h2 class="sys-prefs-section-title">Anything To Change?</h2>
                    <p class="sys-prefs-help">Optional. For example a different coffee, quantity or grind. We will review it before your order goes out.</p>
                    <asp:TextBox ID="txtChanges" runat="server" TextMode="MultiLine" Rows="4" CssClass="sys-prefs-input" />

                    <div class="button-row">
                        <asp:Button ID="btnPlace" runat="server" Text="Place Order" CssClass="filter-panel-btn"
                            OnClick="btnPlace_Click"
                            OnClientClick="return confirm('Place this order?');" />
                        <asp:Button ID="btnBack" runat="server" Text="Back To My Orders" CssClass="filter-panel-btn"
                            OnClick="btnBack_Click" CausesValidation="false" />
                    </div>
                </asp:PlaceHolder>

                <asp:PlaceHolder ID="phDone" runat="server" Visible="false">
                    <div class="button-row">
                        <span class="image-button" title="View your new order">
                            <img src="../images/imgButtons/View.png" alt="" />
                            <asp:HyperLink ID="hlViewOrder" runat="server" Text="View My Order" />
                        </span>
                        <asp:Button ID="btnDoneBack" runat="server" Text="Back To My Orders" CssClass="filter-panel-btn"
                            OnClick="btnBack_Click" CausesValidation="false" />
                    </div>
                </asp:PlaceHolder>

                <asp:Literal ID="litMessage" runat="server" />
            </asp:Panel>
        </ContentTemplate>
        <Triggers>
            <asp:PostBackTrigger ControlID="btnBack" />
            <asp:PostBackTrigger ControlID="btnDoneBack" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>
