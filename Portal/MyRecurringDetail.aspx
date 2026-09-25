<%@ Page Title="Recurring Order" Language="C#" MasterPageFile="~/Portal/Portal.Master"
    AutoEventWireup="true" CodeBehind="MyRecurringDetail.aspx.cs" Inherits="TrackerSQL.Portal.MyRecurringDetail" %>

<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server" />
<asp:Content ID="Body" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnl" runat="server" CssClass="simpleForm page-tone-panel page-tone-recurring portal-card">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/icons8-order-16.png" alt="" />
            <div>
                <h1 class="page-tone-title">Recurring Order</h1>
                <p class="page-tone-subtitle">Items on this standing order and when they are next due. Want to change it? Use Request A Change below.</p>
            </div>
        </div>

        <div class="portal-form-grid">
            <div class="portal-field">
                <span class="portal-field-label">Status</span>
                <span class="portal-field-value"><asp:Literal ID="litStatus" runat="server" /></span>
            </div>
            <div class="portal-field">
                <span class="portal-field-label">Last Date</span>
                <span class="portal-field-value"><asp:Literal ID="litLastDate" runat="server" /></span>
            </div>
            <div class="portal-field">
                <span class="portal-field-label">Next Date</span>
                <span class="portal-field-value"><asp:Literal ID="litNextDate" runat="server" /></span>
            </div>
            <div class="portal-field">
                <span class="portal-field-label">Items</span>
                <span class="portal-field-value"><asp:Literal ID="litItemCount" runat="server" /></span>
            </div>
            <div class="portal-field portal-field-span-4">
                <span class="portal-field-label">Notes</span>
                <span class="portal-field-value"><asp:Literal ID="litNotes" runat="server" /></span>
            </div>
        </div>

        <h2 class="sys-prefs-section-title">Recurring Items</h2>
        <div class="portal-table-wrap">
            <asp:GridView ID="gvItems" runat="server" CssClass="results-table portal-table"
                AutoGenerateColumns="false" EmptyDataText="No items on this recurring order." GridLines="None">
                <Columns>
                    <asp:BoundField DataField="ItemDesc" HeaderText="Item" />
                    <asp:BoundField DataField="QtyRequired" HeaderText="Qty" DataFormatString="{0:0.##}"
                        ItemStyle-CssClass="portal-col-num" HeaderStyle-CssClass="portal-col-num" />
                    <asp:BoundField DataField="ItemPackagingDesc" HeaderText="Packaging" />
                    <asp:BoundField DataField="CustomerPatternDisplay" HeaderText="How Often" />
                    <asp:BoundField DataField="DateLastDoneDisplay" HeaderText="Last Date" ItemStyle-CssClass="portal-col-nowrap" />
                    <asp:BoundField DataField="NextDateRequiredDisplay" HeaderText="Next Date" ItemStyle-CssClass="portal-col-nowrap" />
                    <asp:BoundField DataField="RequireUntilDateDisplay" HeaderText="Until" ItemStyle-CssClass="portal-col-nowrap" />
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
                        <p class="page-tone-subtitle">Want to change, pause or stop this recurring order? Tell us here.</p>
                    </div>
                </div>
                <asp:TextBox ID="txtRequest" runat="server" TextMode="MultiLine" Rows="4" CssClass="sys-prefs-input" />
                <div class="button-row">
                    <asp:Button ID="btnRequest" runat="server" Text="Submit Change Request" CssClass="filter-panel-btn"
                        OnClick="btnRequest_Click" />
                    <asp:Button ID="btnBack" runat="server" Text="Back To My Recurring Orders" CssClass="filter-panel-btn"
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
