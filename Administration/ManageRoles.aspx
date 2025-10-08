<%@ Page Title="Roles" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ManageRoles.aspx.cs"
 Inherits="TrackerDotNet.Administration.ManageRoles" %>
<asp:Content ID="cntRolesHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntRolesBdy" ContentPlaceHolderID="MainContent" runat="server">
  <h1>Roles Managment</h1>
  <asp:GridView runat="server" ID="gvRolesManagement" OnSelectedIndexChanged="gvRolesManagement_OnSelectedIndexChanged" CssClass="TblSimple"
    AutoGenerateColumns="true">
  </asp:GridView>

  <h2>Create a Role</h2>
  <asp:Label id="MsgLabel" ForeColor="maroon" runat="server" /><br />
  Role name:  <asp:TextBox id="RoleTextBox" runat="server" />&nbsp;&nbsp;&nbsp;
  <asp:Button Text="Create Role" id="CreateRoleButton" runat="server" OnClick="CreateRole_OnClick" Visible="true" />
</asp:Content>
