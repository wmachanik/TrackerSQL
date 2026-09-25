<%@ Page Title="Change Password" Language="C#" MasterPageFile="~/Portal/Portal.Master"
    AutoEventWireup="true" CodeBehind="ChangePassword.aspx.cs" Inherits="TrackerSQL.Portal.ChangePassword" %>

<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server" />
<asp:Content ID="Body" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnl" runat="server" CssClass="simpleForm page-tone-panel page-tone-users portal-card portal-login-card">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/Key.gif" alt="" />
            <div>
                <h1 class="page-tone-title"><asp:Literal ID="litTitle" runat="server" Text="Change Password" /></h1>
                <p class="page-tone-subtitle"><asp:Literal ID="litSubtitle" runat="server" /></p>
            </div>
        </div>
        <div class="portal-form-grid">
            <asp:PlaceHolder ID="phEmail" runat="server" Visible="false">
                <div class="portal-field portal-field-span-4">
                    <asp:Label runat="server" AssociatedControlID="txtEmail" Text="Email" />
                    <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" CssClass="sys-prefs-input" autocomplete="username" />
                </div>
            </asp:PlaceHolder>
            <div class="portal-field portal-field-span-4">
                <asp:Label ID="lblCurrent" runat="server" AssociatedControlID="txtCurrent" Text="Current Or Temporary Password" />
                <asp:TextBox ID="txtCurrent" runat="server" TextMode="Password" CssClass="sys-prefs-input" autocomplete="current-password" />
            </div>
            <div class="portal-field portal-field-span-4">
                <asp:Label runat="server" AssociatedControlID="txtNew" Text="New Password" />
                <asp:TextBox ID="txtNew" runat="server" TextMode="Password" CssClass="sys-prefs-input" autocomplete="new-password" />
            </div>
            <div class="portal-field portal-field-span-4">
                <asp:Label runat="server" AssociatedControlID="txtConfirm" Text="Confirm New Password" />
                <asp:TextBox ID="txtConfirm" runat="server" TextMode="Password" CssClass="sys-prefs-input" autocomplete="new-password" />
            </div>
        </div>
        <p class="sys-prefs-help"><asp:Literal ID="litRules" runat="server" /></p>
        <div class="button-row">
            <asp:Button ID="btnSave" runat="server" Text="Save Password" CssClass="filter-panel-btn"
                OnClick="btnSave_Click" />
            <asp:Button ID="btnCancel" runat="server" Text="Back To Home" CssClass="filter-panel-btn"
                OnClick="btnCancel_Click" CausesValidation="false" />
        </div>
        <asp:Literal ID="litMessage" runat="server" />
        <asp:PlaceHolder ID="phAnonHelp" runat="server" Visible="false">
            <p class="sys-prefs-help">
                Temporary password not working?
                <asp:HyperLink runat="server" NavigateUrl="~/Portal/Login.aspx?request=1" Text="Request A New One" />.
                Already chose a password?
                <asp:HyperLink runat="server" NavigateUrl="~/Account/Login.aspx" Text="Sign In" />.
            </p>
        </asp:PlaceHolder>
    </asp:Panel>
</asp:Content>
