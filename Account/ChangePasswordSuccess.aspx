<%@ Page Title="Change Password" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="ChangePasswordSuccess.aspx.cs" Inherits="TrackerSQL.Account.ChangePasswordSuccess" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <asp:Panel ID="pnlChangePasswordSuccess" runat="server" CssClass="simpleForm page-tone-panel page-tone-users">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/Lock.gif" alt="" />
            <div>
                <h1 class="page-tone-title">Change Password</h1>
                <p class="page-tone-subtitle">Password updated</p>
            </div>
        </div>

        <div class="status-message status-success">
            Your password has been changed successfully.
        </div>

        <div class="page-tone-toolbar button-row" style="margin-top: 12px;">
            <asp:Button ID="btnHome" runat="server" Text="Home" CssClass="filter-panel-btn"
                PostBackUrl="~/" CausesValidation="false" />
        </div>
    </asp:Panel>
</asp:Content>
