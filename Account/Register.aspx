<%@ Page Title="Register" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
  CodeBehind="Register.aspx.cs" Inherits="TrackerDotNet.Account.Register" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
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
          <h2>
            Create a New Account</h2>
          <p>
            Use the form below to create a new account.</p>
          <p>
            Passwords are required to be a minimum of
            <%= Membership.MinRequiredPasswordLength %>
            characters in length.</p>
          <span class="failureNotification">
            <asp:Literal ID="ErrorMessage" runat="server"></asp:Literal>
          </span>
          <asp:ValidationSummary ID="RegisterUserValidationSummary" runat="server" CssClass="failureNotification"
            ValidationGroup="RegisterUserValidationGroup" />
          <div class="accountInfo">
            <fieldset class="register">
              <legend>Account Information</legend>
              <p>
                <asp:Label ID="UserNameLabel" runat="server" AssociatedControlID="UserName">User Name:</asp:Label>
                <asp:TextBox ID="UserName" runat="server" CssClass="textEntry"></asp:TextBox>
                <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" ControlToValidate="UserName"
                  CssClass="failureNotification" ErrorMessage="User Name is required." ToolTip="User Name is required."
                  ValidationGroup="RegisterUserValidationGroup">*</asp:RequiredFieldValidator>
              </p>
              <p>
                <asp:Label ID="EmailLabel" runat="server" AssociatedControlID="Email">E-mail:</asp:Label>
                <asp:TextBox ID="Email" runat="server" CssClass="textEntry"></asp:TextBox>
                <asp:RequiredFieldValidator ID="EmailRequired" runat="server" ControlToValidate="Email"
                  CssClass="failureNotification" ErrorMessage="E-mail is required." ToolTip="E-mail is required."
                  ValidationGroup="RegisterUserValidationGroup">*</asp:RequiredFieldValidator>
              </p>
              <p>
                <asp:Label ID="PasswordLabel" runat="server" AssociatedControlID="Password">Password:</asp:Label>
                <asp:TextBox ID="Password" runat="server" CssClass="passwordEntry" TextMode="Password"></asp:TextBox>
                <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="Password"
                  CssClass="failureNotification" ErrorMessage="Password is required." ToolTip="Password is required."
                  ValidationGroup="RegisterUserValidationGroup">*</asp:RequiredFieldValidator>
              </p>
              <p>
                <asp:Label ID="ConfirmPasswordLabel" runat="server" AssociatedControlID="ConfirmPassword">Confirm Password:</asp:Label>
                <asp:TextBox ID="ConfirmPassword" runat="server" CssClass="passwordEntry" TextMode="Password"></asp:TextBox>
                <asp:RequiredFieldValidator ControlToValidate="ConfirmPassword" CssClass="failureNotification"
                  Display="Dynamic" ErrorMessage="Confirm Password is required." ID="ConfirmPasswordRequired"
                  runat="server" ToolTip="Confirm Password is required." ValidationGroup="RegisterUserValidationGroup">*</asp:RequiredFieldValidator>
                <asp:CompareValidator ID="PasswordCompare" runat="server" ControlToCompare="Password"
                  ControlToValidate="ConfirmPassword" CssClass="failureNotification" Display="Dynamic"
                  ErrorMessage="The Password and Confirmation Password must match." ValidationGroup="RegisterUserValidationGroup">*</asp:CompareValidator>
              </p>
            </fieldset>
            <p class="submitButton">
              <asp:Button ID="CreateUserButton" runat="server" CommandName="MoveNext" Text="Create User"
                ValidationGroup="RegisterUserValidationGroup" />
            </p>
          </div>
        </ContentTemplate>
        <CustomNavigationTemplate>
        </CustomNavigationTemplate>
      </asp:CreateUserWizardStep>
      <asp:CompleteWizardStep runat="server">
        <ContentTemplate>
          <table>
            <tr>
              <td align="center" colspan="2">
                <b>Complete</b>
              </td>
            </tr>
            <tr>
              <td>
                Your account has been successfully created. Please contact the administrator to activate your account.
              </td>
            </tr>
            <tr>
              <td align="right" colspan="2">
                <asp:Button ID="ContinueButton" runat="server" CausesValidation="False" CommandName="Continue"
                  Text="Continue" ValidationGroup="RegisterUser" />
              </td>
            </tr>
          </table>
        </ContentTemplate>
      </asp:CompleteWizardStep>
    </WizardSteps>
    <StepNavigationTemplate>
      <asp:Button ID="StepPreviousButton" runat="server" CausesValidation="False" CommandName="MovePrevious"
        Text="Previous" />
      <asp:Button ID="StepNextButton" runat="server" CommandName="MoveNext" Text="Next" />
    </StepNavigationTemplate>
  </asp:CreateUserWizard>
</asp:Content>
