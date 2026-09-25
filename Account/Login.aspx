<%@ Page Title="Log In" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Login.aspx.cs" Inherits="TrackerSQL.Account.Login" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <asp:Panel ID="pnlLogin" runat="server" CssClass="simpleForm page-tone-panel page-tone-users">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/Key.gif" alt="" />
            <div>
                <h1 class="page-tone-title">Sign in</h1>
                <p class="page-tone-subtitle">Quaffee staff use their username; customers use the email address we have on file</p>
            </div>
        </div>

        <asp:PlaceHolder ID="phAdmin" runat="server">
            <asp:Login ID="LoginUser" runat="server" OnLoggingIn="LoginUser_LoggingIn" OnLoggedIn="LoginUser_LoggedIn" EnableViewState="false" RenderOuterTable="false">
                <LayoutTemplate>
                    <span class="failureNotification">
                        <asp:Literal ID="FailureText" runat="server"></asp:Literal>
                    </span>
                    <asp:ValidationSummary ID="LoginUserValidationSummary" runat="server" CssClass="failureNotification"
                        ValidationGroup="LoginUserValidationGroup" />
                    <div class="accountInfo">
                        <fieldset class="login">
                            <legend>Account information</legend>
                            <p>
                                <asp:Label ID="UserNameLabel" runat="server" AssociatedControlID="UserName">Username or email</asp:Label>
                                <asp:TextBox ID="UserName" runat="server" CssClass="textEntry" autocomplete="username"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" ControlToValidate="UserName"
                                    CssClass="failureNotification" ErrorMessage="Username or email is required." ToolTip="Username or email is required."
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
                            <asp:Button ID="LoginButton" runat="server" CommandName="Login" Text="Sign in"
                                CssClass="filter-panel-btn" ValidationGroup="LoginUserValidationGroup" />
                        </p>
                    </div>
                </LayoutTemplate>
            </asp:Login>
            <p class="sys-prefs-help">
                Quaffee customer signing in to My Quaffee for the first time, or forgot your password?
                <asp:HyperLink ID="hlPortalRequest" runat="server" EnableViewState="false"
                    NavigateUrl="~/Portal/Login.aspx?request=1" Text="Request a temporary password" />.
            </p>
            <p class="sys-prefs-help">
                New Quaffee staff member?
                <asp:HyperLink ID="RegisterHyperLink" runat="server" EnableViewState="false">Register a staff account</asp:HyperLink>
                (an administrator must activate it).
            </p>
        </asp:PlaceHolder>
    </asp:Panel>
</asp:Content>
