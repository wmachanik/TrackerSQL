<%@ Page Title="Delivery order" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="DeliveryOrder.aspx.cs" Inherits="TrackerSQL.Tools.DeliveryOrder" %>

<asp:Content ID="cntDelOrderHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="cntDelOrderBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="smDeliveryOrder" runat="server" EnablePartialRendering="true" />
    <div class="simpleForm page-tone-panel page-tone-sysdata">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/icons8-delivery-16.png" alt="" />
            <div>
                <h1 class="page-tone-title">Delivery order</h1>
                <p class="page-tone-subtitle">Each driver's stops in route order: what is done, what is next and when the rest are expected (as in the driver app). Today's list refreshes every minute.</p>
            </div>
        </div>

        <table class="detail-form-table sys-prefs-form-table">
            <tr>
                <td class="sys-prefs-label">Day</td>
                <td class="sys-prefs-field">
                    <asp:Button ID="btnPrevDay" runat="server" Text="&#9664;" ToolTip="Day before" CssClass="filter-panel-btn"
                        OnClick="btnPrevDay_Click" CausesValidation="false" />
                    <asp:TextBox ID="txtDate" runat="server" TextMode="Date" AutoPostBack="true" OnTextChanged="Date_Changed" />
                    <asp:Button ID="btnNextDay" runat="server" Text="&#9654;" ToolTip="Day after" CssClass="filter-panel-btn"
                        OnClick="btnNextDay_Click" CausesValidation="false" />
                    <asp:Button ID="btnToday" runat="server" Text="Today" CssClass="filter-panel-btn"
                        OnClick="btnToday_Click" CausesValidation="false" />
                    <asp:DropDownList ID="ddlDriver" runat="server" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed" />
                </td>
            </tr>
        </table>

        <asp:Label ID="lblMessage" runat="server" CssClass="status-message" Visible="false" />

        <asp:UpdatePanel ID="upnlBoard" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:Timer ID="tmrRefresh" runat="server" Interval="60000" OnTick="tmrRefresh_Tick" Enabled="false" />
                <div class="dorder-board">
                    <asp:Literal ID="litBoard" runat="server" EnableViewState="false" />
                </div>
                <p class="small dorder-updated"><asp:Literal ID="litUpdated" runat="server" EnableViewState="false" /></p>

                <h2 class="page-tone-title" style="font-size: 1rem; margin-top: 1.25rem;">The order they really delivered in</h2>
                <p class="sys-prefs-phase-note">
                    Each stop the driver app recorded, in the order it happened (several orders at one stop count once).
                    When a driver sets the order in the app before leaving (<em>Driver's plan</em>), or the office drags it on the Delivery Sheet, that order is used for the day.
                    Otherwise the suggested order on the delivery sheet and in the app comes first from where each driver usually delivers a contact
                    (their last <asp:Literal ID="litVisits" runat="server" /> visits; one visit counts half, four visits 80%), then from the GPS
                    route from the depot, then from the area order. Days with fewer than <asp:Literal ID="litMinStops" runat="server" /> stops are
                    not learnt from. Collections by the client and supplier drop-offs are not counted. <em>Suggested now</em> is where the stop would be on that day's sheet today.
                    Other days: <asp:Literal ID="litRecentDays" runat="server" />
                </p>

                <asp:GridView ID="gvVisits" runat="server" AutoGenerateColumns="false" CssClass="TblWhite small"
                    EmptyDataText="No deliveries from the driver app on this day.">
                    <Columns>
                        <asp:BoundField HeaderText="Driver" DataField="Driver" />
                        <asp:BoundField HeaderText="Delivered as stop" DataField="StopText" />
                        <asp:BoundField HeaderText="Time" DataField="TimeText" />
                        <asp:HyperLinkField HeaderText="Contact" DataTextField="Contact"
                            DataNavigateUrlFields="ContactID" DataNavigateUrlFormatString="~/Pages/ContactDetails.aspx?ID={0}" />
                        <asp:BoundField HeaderText="Area" DataField="Area" />
                        <asp:BoundField HeaderText="Driver's plan" DataField="PlanText" />
                        <asp:BoundField HeaderText="Suggested now" DataField="SuggestedText" />
                    </Columns>
                </asp:GridView>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="tmrRefresh" EventName="Tick" />
            </Triggers>
        </asp:UpdatePanel>
    </div>
</asp:Content>
