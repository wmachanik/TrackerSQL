<%@ Page Title="My Quaffee access" Language="C#" MasterPageFile="~/Portal/Portal.Master"
    AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="TrackerSQL.Portal.Login" %>

<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server" />
<asp:Content ID="Body" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlLogin" runat="server" CssClass="simpleForm page-tone-panel page-tone-users portal-card portal-login-card">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/Key.gif" alt="" />
            <div>
                <h1 class="page-tone-title">My Quaffee Access</h1>
                <p class="page-tone-subtitle">First time here, or forgot your password? We will email you a temporary password.</p>
            </div>
        </div>

        <asp:PlaceHolder ID="phRequest" runat="server">
            <p class="sys-prefs-help">
                Enter the email address we have on file for you. The email includes a link to choose your own password.
                If that email is on more than one account, the email lists them so you can choose which one to use.
            </p>
            <table class="results-table results-table-fit">
                <tr>
                    <td>Email</td>
                    <td><asp:TextBox ID="txtInviteEmail" runat="server" CssClass="sys-prefs-input" TextMode="Email" autocomplete="email" /></td>
                </tr>
            </table>
            <div class="button-row">
                <asp:Button ID="btnRequestAccess" runat="server" Text="Send Temporary Password" CssClass="filter-panel-btn"
                    OnClick="btnRequestAccess_Click" />
            </div>
        </asp:PlaceHolder>

        <asp:Literal ID="litMessage" runat="server" />

        <p class="sys-prefs-help">
            Already have a password?
            <asp:HyperLink ID="hlSignIn" runat="server" NavigateUrl="~/Account/Login.aspx" Text="Sign In" />.
        </p>
    </asp:Panel>
</asp:Content>
