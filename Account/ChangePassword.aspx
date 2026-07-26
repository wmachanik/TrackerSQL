<%@ Page Title="Change Password" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="ChangePassword.aspx.cs" Inherits="TrackerSQL.Account.ChangePassword" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <asp:Panel ID="pnlChangePassword" runat="server" CssClass="simpleForm page-tone-panel page-tone-users">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/Lock.gif" alt="" />
            <div>
                <h1 class="page-tone-title">Change Password</h1>
                <p class="page-tone-subtitle">Update your account password</p>
            </div>
        </div>

        <p>
            New passwords must be at least <%= Membership.MinRequiredPasswordLength %> characters.
        </p>

        <asp:ChangePassword ID="ChangeUserPassword" runat="server" CancelDestinationPageUrl="~/" EnableViewState="false"
            RenderOuterTable="false" SuccessPageUrl="ChangePasswordSuccess.aspx">
            <ChangePasswordTemplate>
                <span class="failureNotification">
                    <asp:Literal ID="FailureText" runat="server"></asp:Literal>
                </span>
                <asp:ValidationSummary ID="ChangeUserPasswordValidationSummary" runat="server" CssClass="failureNotification"
                    ValidationGroup="ChangeUserPasswordValidationGroup" />
                <div class="accountInfo">
                    <fieldset class="changePassword">
                        <legend>Account Information</legend>
                        <p>
                            <asp:Label ID="CurrentPasswordLabel" runat="server" AssociatedControlID="CurrentPassword">Old Password</asp:Label>
                            <asp:TextBox ID="CurrentPassword" runat="server" CssClass="passwordEntry" TextMode="Password"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="CurrentPasswordRequired" runat="server" ControlToValidate="CurrentPassword"
                                CssClass="failureNotification" ErrorMessage="Password is required." ToolTip="Old Password is required."
                                ValidationGroup="ChangeUserPasswordValidationGroup">*</asp:RequiredFieldValidator>
                        </p>
                        <p>
                            <asp:Label ID="NewPasswordLabel" runat="server" AssociatedControlID="NewPassword">New Password</asp:Label>
                            <asp:TextBox ID="NewPassword" runat="server" CssClass="passwordEntry" TextMode="Password"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="NewPasswordRequired" runat="server" ControlToValidate="NewPassword"
                                CssClass="failureNotification" ErrorMessage="New Password is required." ToolTip="New Password is required."
                                ValidationGroup="ChangeUserPasswordValidationGroup">*</asp:RequiredFieldValidator>
                        </p>
                        <p>
                            <asp:Label ID="ConfirmNewPasswordLabel" runat="server" AssociatedControlID="ConfirmNewPassword">Confirm New Password</asp:Label>
                            <asp:TextBox ID="ConfirmNewPassword" runat="server" CssClass="passwordEntry" TextMode="Password"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="ConfirmNewPasswordRequired" runat="server" ControlToValidate="ConfirmNewPassword"
                                CssClass="failureNotification" Display="Dynamic" ErrorMessage="Confirm New Password is required."
                                ToolTip="Confirm New Password is required." ValidationGroup="ChangeUserPasswordValidationGroup">*</asp:RequiredFieldValidator>
                            <asp:CompareValidator ID="NewPasswordCompare" runat="server" ControlToCompare="NewPassword"
                                ControlToValidate="ConfirmNewPassword" CssClass="failureNotification" Display="Dynamic"
                                ErrorMessage="The Confirm New Password must match the New Password entry."
                                ValidationGroup="ChangeUserPasswordValidationGroup">*</asp:CompareValidator>
                        </p>
                    </fieldset>
                    <p class="submitButton button-row">
                        <asp:Button ID="ChangePasswordPushButton" runat="server" CommandName="ChangePassword" Text="Change Password"
                            CssClass="filter-panel-btn" ValidationGroup="ChangeUserPasswordValidationGroup" />
                        <asp:Button ID="CancelPushButton" runat="server" CausesValidation="False" CommandName="Cancel"
                            Text="Cancel" CssClass="filter-panel-btn" />
                    </p>
                </div>
            </ChangePasswordTemplate>
        </asp:ChangePassword>
    </asp:Panel>
</asp:Content>
