<%@ Page Title="Log In" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Login.aspx.cs" Inherits="TrackerSQL.Account.Login" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <asp:Panel ID="pnlLogin" runat="server" CssClass="simpleForm page-tone-panel page-tone-users">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/Key.gif" alt="" />
            <div>
                <h1 class="page-tone-title">Log In</h1>
                <p class="page-tone-subtitle">Sign in to TrackerSQL</p>
            </div>
        </div>

        <p>
            Please enter your username and password.
            <asp:HyperLink ID="RegisterHyperLink" runat="server" EnableViewState="false">Register</asp:HyperLink>
            if you don't have an account.
        </p>

        <asp:Login ID="LoginUser" runat="server" OnLoggedIn="LoginUser_LoggedIn" EnableViewState="false" RenderOuterTable="false">
            <LayoutTemplate>
                <span class="failureNotification">
                    <asp:Literal ID="FailureText" runat="server"></asp:Literal>
                </span>
                <asp:ValidationSummary ID="LoginUserValidationSummary" runat="server" CssClass="failureNotification"
                    ValidationGroup="LoginUserValidationGroup" />
                <div class="accountInfo">
                    <fieldset class="login">
                        <legend>Account Information</legend>
                        <p>
                            <asp:Label ID="UserNameLabel" runat="server" AssociatedControlID="UserName">Username</asp:Label>
                            <asp:TextBox ID="UserName" runat="server" CssClass="textEntry"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" ControlToValidate="UserName"
                                CssClass="failureNotification" ErrorMessage="User Name is required." ToolTip="User Name is required."
                                ValidationGroup="LoginUserValidationGroup">*</asp:RequiredFieldValidator>
                        </p>
                        <p>
                            <asp:Label ID="PasswordLabel" runat="server" AssociatedControlID="Password">Password</asp:Label>
                            <asp:TextBox ID="Password" runat="server" CssClass="passwordEntry" TextMode="Password"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="Password"
                                CssClass="failureNotification" ErrorMessage="Password is required." ToolTip="Password is required."
                                ValidationGroup="LoginUserValidationGroup">*</asp:RequiredFieldValidator>
                        </p>
                        <p>
                            <asp:CheckBox ID="RememberMe" runat="server" />
                            <asp:Label ID="RememberMeLabel" runat="server" AssociatedControlID="RememberMe" CssClass="inline">Keep me logged in</asp:Label>
                        </p>
                    </fieldset>
                    <p class="submitButton button-row">
                        <asp:Button ID="LoginButton" runat="server" CommandName="Login" Text="Log In"
                            CssClass="filter-panel-btn" ValidationGroup="LoginUserValidationGroup" />
                    </p>
                </div>
            </LayoutTemplate>
        </asp:Login>
    </asp:Panel>
</asp:Content>
