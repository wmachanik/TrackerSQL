<%@ Page Title="Customer Locations" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="CustomerLocations.aspx.cs" Inherits="TrackerSQL.Tools.CustomerLocations" %>

<asp:Content ID="cntCustLocHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="cntCustLocBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlAccessDenied" runat="server" Visible="false" CssClass="simpleForm page-tone-panel page-tone-sysdata">
        <span class="status-message status-error">Only administrators can run customer location lookups.</span>
    </asp:Panel>

    <asp:Panel ID="pnlMain" runat="server" CssClass="simpleForm page-tone-panel page-tone-sysdata">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/Toolbox.png" alt="" />
            <div>
                <h1 class="page-tone-title">Customer Locations</h1>
                <p class="page-tone-subtitle">Look up GPS points for contacts delivered by our own drivers (done once, then only when an address changes)</p>
            </div>
        </div>

        <table class="detail-form-table sys-prefs-form-table">
            <tr>
                <td class="sys-prefs-label">Still to look up</td>
                <td class="sys-prefs-field">
                    <asp:Label ID="lblToDo" runat="server" /><br />
                    <asp:CheckBox ID="cbxOpenOrdersOnly" runat="server" AutoPostBack="true" OnCheckedChanged="cbxOpenOrdersOnly_CheckedChanged"
                        Text="Only contacts with open orders for our drivers"
                        ToolTip="Orders not done yet whose delivery person is set to Does deliveries (even if the contact's usual Delivery By is someone else)" />
                </td>
            </tr>
            <tr>
                <td class="sys-prefs-label">Lookups used today</td>
                <td class="sys-prefs-field"><asp:Label ID="lblUsedToday" runat="server" /></td>
            </tr>
            <tr>
                <td class="sys-prefs-label">Look up now</td>
                <td class="sys-prefs-field">
                    <asp:TextBox ID="txtBatchSize" runat="server" CssClass="gps-num-input" TextMode="Number" Text="50" />
                    contacts
                    <asp:Button ID="btnRunBatch" runat="server" CssClass="filter-panel-btn" Text="Look up" OnClick="btnRunBatch_Click"
                        ToolTip="Stops early at the per-minute or daily limit; run it again later for the rest" />
                </td>
            </tr>
        </table>
        <p class="sys-prefs-phase-note">
            Only contacts whose "Delivery By" person is set to <em>Does deliveries</em> (Lookups → People) are looked up.
            Rough matches (suburb or postcode only) are listed below for checking.
            <asp:HyperLink ID="hlSettings" runat="server" NavigateUrl="~/Tools/SystemPreferences.aspx?section=gps" Text="GPS settings" />
        </p>

        <asp:Label ID="lblMessage" runat="server" CssClass="status-message" Visible="false" />

        <h3 class="gps-settings-heading">Needs checking</h3>
        <p>
            <asp:Button ID="btnRedoRough" runat="server" CssClass="filter-panel-btn" Text="Look up rough ones again" OnClick="btnRedoRough_Click"
                ToolTip="Looks these up again (up to the batch size) with the area as suburb, only near the depot. Pins placed by hand are not touched." />
        </p>
        <asp:GridView ID="gvReview" runat="server" AutoGenerateColumns="false" CssClass="TblWhite small"
            EmptyDataText="Nothing to check.">
            <Columns>
                <asp:HyperLinkField HeaderText="Contact" DataTextField="CompanyName"
                    DataNavigateUrlFields="ContactID" DataNavigateUrlFormatString="~/Pages/ContactDetails.aspx?ID={0}" />
                <asp:BoundField HeaderText="Address looked up" DataField="SourceAddress" />
                <asp:BoundField HeaderText="Found" DataField="GeocodedAddress" />
                <asp:BoundField HeaderText="How" DataField="Source" />
            </Columns>
        </asp:GridView>
    </asp:Panel>
</asp:Content>
