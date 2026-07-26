<%@ Page Title="Register" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Register.aspx.cs" Inherits="TrackerSQL.Account.Register" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <asp:Panel ID="pnlRegister" runat="server" CssClass="simpleForm page-tone-panel page-tone-users">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/Person.gif" alt="" />
            <div>
                <h1 class="page-tone-title">Register</h1>
                <p class="page-tone-subtitle">Create a new TrackerSQL account</p>
            </div>
        </div>

        <asp:CreateUserWizard ID="RegisterUser" runat="server" EnableViewState="False"
            OnCreatedUser="RegisterUser_CreatedUser" DisableCreatedUser="True"
            LoginCreatedUser="False">
            <LayoutTemplate>
                <asp:PlaceHolder ID="wizardStepPlaceholder" runat="server"></asp:PlaceHolder>
                <asp:PlaceHolder ID="navigationPlaceholder" runat="server"></asp:PlaceHolder>
            </LayoutTemplate>
            <MailDefinition BodyFileName="~/Account/NewAccountTemplate.htm"
                CC="webmaster@quaffee.co.za" From="webmaster@quaffee.co.za" IsBodyHtml="True"
                Subject="New Account on Tracker.NET">
            </MailDefinition>
            <WizardSteps>
                <asp:CreateUserWizardStep ID="RegisterUserWizardStep" runat="server">
                    <ContentTemplate>
                        <p>
                            Passwords must be at least
                            <%= Membership.MinRequiredPasswordLength %>
                            characters.
                        </p>
                        <span class="failureNotification">
                            <asp:Literal ID="ErrorMessage" runat="server"></asp:Literal>
                        </span>
                        <asp:ValidationSummary ID="RegisterUserValidationSummary" runat="server" CssClass="failureNotification"
                            ValidationGroup="RegisterUserValidationGroup" />
                        <div class="accountInfo">
                            <fieldset class="register">
                                <legend>Account Information</legend>
                                <p>
                                    <asp:Label ID="UserNameLabel" runat="server" AssociatedControlID="UserName">User Name</asp:Label>
                                    <asp:TextBox ID="UserName" runat="server" CssClass="textEntry"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" ControlToValidate="UserName"
                                        CssClass="failureNotification" ErrorMessage="User Name is required." ToolTip="User Name is required."
                                        ValidationGroup="RegisterUserValidationGroup">*</asp:RequiredFieldValidator>
                                </p>
                                <p>
                                    <asp:Label ID="EmailLabel" runat="server" AssociatedControlID="Email">E-mail</asp:Label>
                                    <asp:TextBox ID="Email" runat="server" CssClass="textEntry"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="EmailRequired" runat="server" ControlToValidate="Email"
                                        CssClass="failureNotification" ErrorMessage="E-mail is required." ToolTip="E-mail is required."
                                        ValidationGroup="RegisterUserValidationGroup">*</asp:RequiredFieldValidator>
                                </p>
                                <p>
                                    <asp:Label ID="PasswordLabel" runat="server" AssociatedControlID="Password">Password</asp:Label>
                                    <asp:TextBox ID="Password" runat="server" CssClass="passwordEntry" TextMode="Password"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="Password"
                                        CssClass="failureNotification" ErrorMessage="Password is required." ToolTip="Password is required."
                                        ValidationGroup="RegisterUserValidationGroup">*</asp:RequiredFieldValidator>
                                </p>
                                <p>
                                    <asp:Label ID="ConfirmPasswordLabel" runat="server" AssociatedControlID="ConfirmPassword">Confirm Password</asp:Label>
                                    <asp:TextBox ID="ConfirmPassword" runat="server" CssClass="passwordEntry" TextMode="Password"></asp:TextBox>
                                    <asp:RequiredFieldValidator ControlToValidate="ConfirmPassword" CssClass="failureNotification"
                                        Display="Dynamic" ErrorMessage="Confirm Password is required." ID="ConfirmPasswordRequired"
                                        runat="server" ToolTip="Confirm Password is required." ValidationGroup="RegisterUserValidationGroup">*</asp:RequiredFieldValidator>
                                    <asp:CompareValidator ID="PasswordCompare" runat="server" ControlToCompare="Password"
                                        ControlToValidate="ConfirmPassword" CssClass="failureNotification" Display="Dynamic"
                                        ErrorMessage="The Password and Confirmation Password must match." ValidationGroup="RegisterUserValidationGroup">*</asp:CompareValidator>
                                </p>
                            </fieldset>
                            <p class="submitButton button-row">
                                <asp:Button ID="CreateUserButton" runat="server" CommandName="MoveNext" Text="Create User"
                                    CssClass="filter-panel-btn" ValidationGroup="RegisterUserValidationGroup" />
                            </p>
                        </div>
                    </ContentTemplate>
                    <CustomNavigationTemplate>
                    </CustomNavigationTemplate>
                </asp:CreateUserWizardStep>
                <asp:CompleteWizardStep runat="server">
                    <ContentTemplate>
                        <div class="status-message status-success">
                            Your account has been created. Contact an administrator to activate it.
                        </div>
                        <div class="button-row" style="margin-top: 12px;">
                            <asp:Button ID="ContinueButton" runat="server" CausesValidation="False" CommandName="Continue"
                                Text="Continue" CssClass="filter-panel-btn" ValidationGroup="RegisterUser" />
                        </div>
                    </ContentTemplate>
                </asp:CompleteWizardStep>
            </WizardSteps>
            <StepNavigationTemplate>
                <asp:Button ID="StepPreviousButton" runat="server" CausesValidation="False" CommandName="MovePrevious"
                    Text="Previous" CssClass="filter-panel-btn" />
                <asp:Button ID="StepNextButton" runat="server" CommandName="MoveNext" Text="Next"
                    CssClass="filter-panel-btn" />
            </StepNavigationTemplate>
        </asp:CreateUserWizard>
    </asp:Panel>
</asp:Content>
