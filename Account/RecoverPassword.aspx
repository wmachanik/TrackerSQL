<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="RecoverPassword.aspx.cs" Inherits="TrackerDotNet.Account.RecoverPassword" %>
<asp:Content ID="cntPasswordRecoveryHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntPasswordRecoveryBdy" ContentPlaceHolderID="MainContent" runat="server">

  <h2>Password Recovery</h2>
  <asp:PasswordRecovery ID="TrackerPasswordRecovery" runat="server" 
    BackColor="Transparent" BorderColor="#CCCC99" BorderStyle="Double" 
    BorderWidth="1px" >
    <TitleTextStyle BackColor="#6B696B" Font-Bold="True" ForeColor="#FFFFFF" />
    <UserNameTemplate>
      <table cellpadding="10" cellspacing="10" style="border-collapse:collapse;">
        <tr>
          <td>
            <table cellpadding="0">
              <tr>
                <td align="center" colspan="2" height="40px">
                  Enter your User Name to receive your password.</td>
              </tr>
              <tr>
                <td align="right">
                  <asp:Label ID="UserNameLabel" runat="server" AssociatedControlID="UserName">User Name:</asp:Label>
                </td>
                <td>
                  <asp:TextBox ID="UserName" runat="server"></asp:TextBox>
                  <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" 
                    ControlToValidate="UserName" ErrorMessage="User Name is required." 
                    ToolTip="User Name is required." ValidationGroup="TrackerPasswordRecovery">*</asp:RequiredFieldValidator>
                </td>
              </tr>
              <tr>
                <td align="center" colspan="2" style="color:Red; min-height:2em;" height="30px">
                  <asp:Literal ID="FailureText" runat="server" EnableViewState="False"></asp:Literal>
                </td>
              </tr>
              <tr>
                <td align="center" colspan="2">
                  <asp:Button ID="SubmitButton" runat="server" CommandName="Submit" Text="Submit" 
                    ValidationGroup="TrackerPasswordRecovery" />
                </td>
              </tr>
            </table>
          </td>
        </tr>
      </table>
    </UserNameTemplate>
  </asp:PasswordRecovery>

</asp:Content>
