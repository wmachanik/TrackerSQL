<%@ Page Title="Repair Details" Language="C#" MasterPageFile="~/Portal/Portal.Master"
    AutoEventWireup="true" CodeBehind="MyRepair.aspx.cs" Inherits="TrackerSQL.Portal.MyRepair" %>

<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server" />
<asp:Content ID="Body" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnl" runat="server" CssClass="simpleForm page-tone-panel page-tone-repairs portal-card">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/icons8-repair-tools-16.png" alt="" />
            <div>
                <h1 class="page-tone-title">Repair Details</h1>
                <p class="page-tone-subtitle"><asp:Literal ID="litStatusNote" runat="server" /></p>
            </div>
        </div>
        <div class="portal-form-grid">
            <div class="portal-field">
                <span class="portal-field-label">Job Card</span>
                <span class="portal-field-value"><asp:Literal ID="litJob" runat="server" /></span>
            </div>
            <div class="portal-field">
                <span class="portal-field-label">Logged</span>
                <span class="portal-field-value"><asp:Literal ID="litLogged" runat="server" /></span>
            </div>
            <div class="portal-field">
                <span class="portal-field-label">Status</span>
                <span class="portal-field-value"><asp:Literal ID="litStatus" runat="server" /></span>
            </div>
            <div class="portal-field">
                <span class="portal-field-label">Last Update</span>
                <span class="portal-field-value"><asp:Literal ID="litLastUpdate" runat="server" /></span>
            </div>
            <div class="portal-field">
                <span class="portal-field-label">Machine</span>
                <span class="portal-field-value"><asp:Literal ID="litMachine" runat="server" /></span>
            </div>
            <div class="portal-field">
                <span class="portal-field-label">Serial Number</span>
                <span class="portal-field-value"><asp:Literal ID="litSerial" runat="server" /></span>
            </div>
            <div class="portal-field portal-field-span-2">
                <span class="portal-field-label">Fault</span>
                <span class="portal-field-value"><asp:Literal ID="litFault" runat="server" /></span>
            </div>
            <div class="portal-field portal-field-span-4">
                <span class="portal-field-label">Notes</span>
                <span class="portal-field-value"><asp:Literal ID="litNotes" runat="server" /></span>
            </div>
        </div>
    </asp:Panel>

    <asp:UpdatePanel ID="upRequest" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:Panel ID="pnlRequest" runat="server" CssClass="simpleForm page-tone-panel page-tone-users portal-card">
                <div class="page-tone-header tool-card-header">
                    <img class="tool-card-icon" src="../images/imgButtons/EditItem.gif" alt="" />
                    <div>
                        <h2 class="page-tone-title">Request A Change</h2>
                        <p class="page-tone-subtitle">Questions or changes about this repair? Let us know here.</p>
                    </div>
                </div>
                <asp:TextBox ID="txtRequest" runat="server" TextMode="MultiLine" Rows="4" CssClass="sys-prefs-input" />
                <div class="button-row">
                    <asp:Button ID="btnRequest" runat="server" Text="Submit Change Request" CssClass="filter-panel-btn"
                        OnClick="btnRequest_Click" />
                    <asp:Button ID="btnBack" runat="server" Text="Back To My Repairs" CssClass="filter-panel-btn"
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
