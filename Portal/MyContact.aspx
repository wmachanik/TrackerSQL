<%@ Page Title="My Details" Language="C#" MasterPageFile="~/Portal/Portal.Master"
    AutoEventWireup="true" CodeBehind="MyContact.aspx.cs" Inherits="TrackerSQL.Portal.MyContact" %>

<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server" />
<asp:Content ID="Body" ContentPlaceHolderID="MainContent" runat="server">
    <asp:UpdatePanel ID="upDetails" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:Panel ID="pnlDetails" runat="server" CssClass="simpleForm page-tone-panel page-tone-contacts portal-card">
                <div class="page-tone-header tool-card-header">
                    <img class="tool-card-icon" src="../images/imgButtons/icons8-new-contact-48.png" alt="" />
                    <div>
                        <h1 class="page-tone-title">My Details</h1>
                        <p class="page-tone-subtitle"><asp:Literal ID="litDetailsHelp" runat="server" /></p>
                    </div>
                </div>

                <div class="portal-form-grid">
                    <div class="portal-field portal-field-span-2">
                        <asp:Label runat="server" AssociatedControlID="txtCompany" Text="Company" />
                        <asp:TextBox ID="txtCompany" runat="server" CssClass="sys-prefs-input" />
                    </div>
                    <div class="portal-field">
                        <asp:Label runat="server" AssociatedControlID="txtTitle" Text="Title" />
                        <asp:TextBox ID="txtTitle" runat="server" CssClass="sys-prefs-input" />
                    </div>
                    <div class="portal-field">
                        <asp:Label runat="server" AssociatedControlID="txtDept" Text="Department" />
                        <asp:TextBox ID="txtDept" runat="server" CssClass="sys-prefs-input" />
                    </div>

                    <div class="portal-field">
                        <asp:Label runat="server" AssociatedControlID="txtFirst" Text="First Name" />
                        <asp:TextBox ID="txtFirst" runat="server" CssClass="sys-prefs-input" />
                    </div>
                    <div class="portal-field">
                        <asp:Label runat="server" AssociatedControlID="txtLast" Text="Last Name" />
                        <asp:TextBox ID="txtLast" runat="server" CssClass="sys-prefs-input" />
                    </div>
                    <div class="portal-field">
                        <asp:Label runat="server" AssociatedControlID="txtAltFirst" Text="Alt First Name" />
                        <asp:TextBox ID="txtAltFirst" runat="server" CssClass="sys-prefs-input" />
                    </div>
                    <div class="portal-field">
                        <asp:Label runat="server" AssociatedControlID="txtAltLast" Text="Alt Last Name" />
                        <asp:TextBox ID="txtAltLast" runat="server" CssClass="sys-prefs-input" />
                    </div>

                    <div class="portal-field portal-field-span-2">
                        <asp:Label runat="server" AssociatedControlID="txtAddress" Text="Address" />
                        <asp:TextBox ID="txtAddress" runat="server" CssClass="sys-prefs-input" TextMode="MultiLine" Rows="3" />
                    </div>
                    <div class="portal-field">
                        <asp:Label runat="server" AssociatedControlID="txtPostal" Text="Postal Code" />
                        <asp:TextBox ID="txtPostal" runat="server" CssClass="sys-prefs-input" />
                    </div>
                    <div class="portal-field">
                        <asp:Label runat="server" AssociatedControlID="txtProvince" Text="Province" />
                        <asp:TextBox ID="txtProvince" runat="server" CssClass="sys-prefs-input" />
                    </div>

                    <div class="portal-field">
                        <asp:Label runat="server" AssociatedControlID="txtPhone" Text="Phone" />
                        <asp:TextBox ID="txtPhone" runat="server" CssClass="sys-prefs-input" />
                    </div>
                    <div class="portal-field">
                        <asp:Label runat="server" AssociatedControlID="txtCell" Text="Cell" />
                        <asp:TextBox ID="txtCell" runat="server" CssClass="sys-prefs-input" />
                    </div>
                    <div class="portal-field">
                        <asp:Label runat="server" AssociatedControlID="txtEmail" Text="Email" />
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="sys-prefs-input" />
                    </div>
                    <div class="portal-field">
                        <asp:Label runat="server" AssociatedControlID="txtAltEmail" Text="Alt Email" />
                        <asp:TextBox ID="txtAltEmail" runat="server" CssClass="sys-prefs-input" />
                    </div>
                </div>

                <div class="button-row">
                    <asp:Button ID="btnSave" runat="server" Text="Save Changes" CssClass="filter-panel-btn"
                        OnClick="btnSave_Click" />
                </div>
                <asp:Literal ID="litMessage" runat="server" />
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>

    <asp:UpdatePanel ID="upPrefs" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:Panel ID="pnlPrefs" runat="server" CssClass="simpleForm page-tone-panel page-tone-checkup portal-card">
                <div class="page-tone-header tool-card-header">
                    <img class="tool-card-icon" src="../images/imgButtons/icons8-send-email-16.png" alt="" />
                    <div>
                        <h2 class="page-tone-title">Email Preferences</h2>
                        <p class="page-tone-subtitle">Coffee checkup reminders remind you when you may be running low.</p>
                    </div>
                </div>
                <div class="portal-switch-row">
                    <asp:CheckBox ID="chkReminders" runat="server" CssClass="portal-switch"
                        Text="Send me coffee checkup reminder emails"
                        AutoPostBack="true" OnCheckedChanged="chkReminders_CheckedChanged" />
                    <asp:Literal ID="litReminderState" runat="server" />
                </div>
                <asp:PlaceHolder ID="phRecurringLock" runat="server" Visible="false">
                    <p class="status-message status-info">You have an active recurring order, so reminder emails cannot be changed here.
                        Reminders and recurring deliveries are calculated differently and can conflict, so we manage this for you.
                        If you need it changed, send us a change request below.</p>
                </asp:PlaceHolder>
                <p class="sys-prefs-help">Order and repair updates are always sent. To close your account, send us a change request below.</p>
                <asp:Literal ID="litPrefsMessage" runat="server" />
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>

    <asp:UpdatePanel ID="upRequest" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:Panel ID="pnlRequest" runat="server" CssClass="simpleForm page-tone-panel page-tone-users portal-card">
                <div class="page-tone-header tool-card-header">
                    <img class="tool-card-icon" src="../images/imgButtons/EditItem.gif" alt="" />
                    <div>
                        <h2 class="page-tone-title">Request A Change</h2>
                        <p class="page-tone-subtitle">For anything you cannot change above, tell us what should change and we will update it.</p>
                    </div>
                </div>
                <asp:TextBox ID="txtRequest" runat="server" TextMode="MultiLine" Rows="4" CssClass="sys-prefs-input" />
                <div class="button-row">
                    <asp:Button ID="btnRequest" runat="server" Text="Submit Change Request" CssClass="filter-panel-btn"
                        OnClick="btnRequest_Click" />
                </div>
                <asp:Literal ID="litRequestMessage" runat="server" />
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
