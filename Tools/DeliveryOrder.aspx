<%@ Page Title="Delivery order" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="DeliveryOrder.aspx.cs" Inherits="TrackerSQL.Tools.DeliveryOrder" %>

<asp:Content ID="cntDelOrderHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="cntDelOrderBdy" ContentPlaceHolderID="MainContent" runat="server">
    <div class="simpleForm page-tone-panel page-tone-sysdata">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/icons8-delivery-16.png" alt="" />
            <div>
                <h1 class="page-tone-title">Delivery order</h1>
                <p class="page-tone-subtitle">The order drivers really delivered in, from the driver app (one line per stop; several orders at one stop count once)</p>
            </div>
        </div>

        <table class="detail-form-table sys-prefs-form-table">
            <tr>
                <td class="sys-prefs-label">Day</td>
                <td class="sys-prefs-field">
                    <asp:DropDownList ID="ddlDate" runat="server" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed" />
                    <asp:DropDownList ID="ddlDriver" runat="server" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed" />
                </td>
            </tr>
        </table>
        <p class="sys-prefs-phase-note">
            The suggested order on the delivery sheet and in the app comes first from where each driver usually delivers a contact
            (their last <asp:Literal ID="litVisits" runat="server" /> visits; one visit counts half, four visits 80%), then from the GPS
            route from the depot, then from the area order. Days with fewer than <asp:Literal ID="litMinStops" runat="server" /> stops are
            not learnt from. <em>Suggested now</em> is where the stop would be on that day's sheet today.
        </p>

        <asp:Label ID="lblMessage" runat="server" CssClass="status-message" Visible="false" />

        <asp:GridView ID="gvVisits" runat="server" AutoGenerateColumns="false" CssClass="TblWhite small"
            EmptyDataText="No deliveries from the driver app on this day.">
            <Columns>
                <asp:BoundField HeaderText="Driver" DataField="Driver" />
                <asp:BoundField HeaderText="Delivered as stop" DataField="StopText" />
                <asp:BoundField HeaderText="Time" DataField="TimeText" />
                <asp:HyperLinkField HeaderText="Contact" DataTextField="Contact"
                    DataNavigateUrlFields="ContactID" DataNavigateUrlFormatString="~/Pages/ContactDetails.aspx?ID={0}" />
                <asp:BoundField HeaderText="Area" DataField="Area" />
                <asp:BoundField HeaderText="Suggested now" DataField="SuggestedText" />
            </Columns>
        </asp:GridView>
    </div>
</asp:Content>
