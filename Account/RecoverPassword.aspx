<%@ Page Title="Recover Password" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="RecoverPassword.aspx.cs" Inherits="TrackerSQL.Account.RecoverPassword" %>

<asp:Content ID="cntPasswordRecoveryHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntPasswordRecoveryBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlRecoverPassword" runat="server" CssClass="simpleForm page-tone-panel page-tone-users">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/Unlock.gif" alt="" />
            <div>
                <h1 class="page-tone-title">Recover Password</h1>
                <p class="page-tone-subtitle">Request a password reset email</p>
            </div>
        </div>

        <div class="accountInfo">
            <fieldset>
                <legend>Account</legend>
                <p>
                    Enter your user name or email address. A new password will be emailed to the address on your account.
                </p>
                <div class="account-form-row">
                    <asp:Label ID="UserNameLabel" runat="server" AssociatedControlID="tbxUserName">User Name or Email</asp:Label>
                    <asp:TextBox ID="tbxUserName" runat="server" CssClass="textEntry"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="UserNameRequired" runat="server"
                        ControlToValidate="tbxUserName" ErrorMessage="User name or email is required."
                        CssClass="failureNotification" ToolTip="User name or email is required."
                        ValidationGroup="RecoverPassword">*</asp:RequiredFieldValidator>
                </div>
                <div class="status-message status-error" id="pnlError" runat="server" visible="false">
                    <asp:Literal ID="ltrlError" runat="server" EnableViewState="False" />
                </div>
                <div class="status-message status-success" id="pnlSuccess" runat="server" visible="false">
                    <asp:Literal ID="ltrlSuccess" runat="server" EnableViewState="False" />
                </div>
            </fieldset>
            <p class="submitButton button-row">
                <asp:Button ID="btnSubmit" runat="server" Text="Submit"
                    CssClass="filter-panel-btn" ValidationGroup="RecoverPassword"
                    OnClick="btnSubmit_Click" />
            </p>
        </div>
    </asp:Panel>
</asp:Content>
