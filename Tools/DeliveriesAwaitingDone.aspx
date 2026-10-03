<%@ Page Title="Deliveries Waiting for Order Done" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="DeliveriesAwaitingDone.aspx.cs" Inherits="TrackerSQL.Tools.DeliveriesAwaitingDone" %>

<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server" />
<asp:Content ID="Body" ContentPlaceHolderID="MainContent" runat="server">
    <div class="simpleForm page-tone-panel page-tone-sysdata driver-app-page">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/icons8-delivery-16.png" alt="" />
            <div>
                <h1 class="page-tone-title">Deliveries Waiting for Order Done</h1>
                <p class="page-tone-subtitle">Deliveries the driver sent from the app whose order is still not done in Tracker. The driver gets a
                    reply as soon as the delivery is saved; Tracker then completes the order (Order Done, the Woo order and the emails) in the
                    background. Anything still listed here is being completed, was left for the office, or failed.</p>
            </div>
        </div>

        <asp:Panel ID="pnlNotAdmin" runat="server" Visible="false" CssClass="status-message status-error">
            Only administrators can see this page.
        </asp:Panel>

        <asp:Panel ID="pnlAwaiting" runat="server" CssClass="complex-form-section driver-app-awaiting">
            <asp:Literal ID="litAwaitingMessage" runat="server" />
            <div class="results-container">
                <asp:GridView ID="gvAwaiting" runat="server" CssClass="results-table"
                    AutoGenerateColumns="false" DataKeyNames="ProofID"
                    EmptyDataText="Every delivery sent from the app has its order done."
                    OnRowCommand="gvAwaiting_RowCommand" OnRowDataBound="gvAwaiting_RowDataBound">
                    <Columns>
                        <asp:TemplateField HeaderText="Order" ItemStyle-CssClass="col-tight">
                            <ItemTemplate>
                                <asp:HyperLink runat="server" Text='<%# Eval("OrderID") %>'
                                    NavigateUrl='<%# "~/Pages/OrderDetail.aspx?OrderID=" + Eval("OrderID") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Client">
                            <ItemTemplate><%# HttpUtility.HtmlEncode(Eval("CompanyName")) %></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Driver" ItemStyle-CssClass="col-tight">
                            <ItemTemplate><%# HttpUtility.HtmlEncode(Eval("UserName")) %></ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="DeliveredAt" HeaderText="Delivered" DataFormatString="{0:ddd d MMM HH:mm}" ItemStyle-CssClass="col-tight" />
                        <asp:TemplateField HeaderText="Status">
                            <ItemTemplate>
                                <strong><%# HttpUtility.HtmlEncode(StatusText(Container.DataItem)) %></strong>
                                <div class="sys-prefs-help"><%# HttpUtility.HtmlEncode(Eval("StatusMessage")) %></div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Action" ItemStyle-CssClass="col-tight">
                            <ItemTemplate>
                                <asp:Button runat="server" Text="Complete Now" CssClass="filter-panel-btn"
                                    Visible='<%# CanComplete(Container.DataItem) %>'
                                    CommandName="CompleteNow" CommandArgument='<%# Eval("ProofID") %>' CausesValidation="false"
                                    OnClientClick="return confirm('Run Order Done for this delivery now?');" />
                                <asp:HyperLink runat="server" Text="Open the order" Visible='<%# HasItemsToFollow(Container.DataItem) %>'
                                    NavigateUrl='<%# "~/Pages/OrderDetail.aspx?OrderID=" + Eval("OrderID") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
            <div class="button-row awaiting-buttons">
                <asp:Button ID="btnRefreshAwaiting" runat="server" Text="Refresh" CssClass="filter-panel-btn"
                    OnClick="btnRefreshAwaiting_Click" CausesValidation="false" />
                <a href="DriverApp.aspx">Driver App settings</a>
            </div>
        </asp:Panel>
    </div>
</asp:Content>
