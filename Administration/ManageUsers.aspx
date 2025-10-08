<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ManageUsers.aspx.cs" Inherits="TrackerDotNet.Administration.ManageUsers" %>
<asp:Content ID="cntManageUsersHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntManageUsersBdy" ContentPlaceHolderID="MainContent" runat="server">
  <h1>Manage Users</h1>
  <asp:GridView ID="gvUserAccounts" runat="server" AutoGenerateColumns="False" CellSpacing="1"
    CellPadding="4" ForeColor="#333333">
    <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
    <Columns>
      <asp:HyperLinkField DataNavigateUrlFields="UserName" 
        DataNavigateUrlFormatString="UserInformation.aspx?user={0}" Text="Manage" 
        HeaderImageUrl="~/images/imgButtons/World.gif" />
      <asp:BoundField DataField="UserName" HeaderText="UserName"/>
      <asp:BoundField DataField="Email" HeaderText="Email" />
      <asp:CheckBoxField DataField="IsApproved" HeaderText="Approved?" 
        ItemStyle-HorizontalAlign="Center" >
<ItemStyle HorizontalAlign="Center"></ItemStyle>
      </asp:CheckBoxField>
      <asp:CheckBoxField DataField="IsLockedOut" HeaderText="Locked Out?" 
        ItemStyle-HorizontalAlign="Center" >
<ItemStyle HorizontalAlign="Center"></ItemStyle>
      </asp:CheckBoxField>
      <asp:CheckBoxField DataField="IsOnline" HeaderText="Online?" 
        ItemStyle-HorizontalAlign="Center" >
<ItemStyle HorizontalAlign="Center"></ItemStyle>
      </asp:CheckBoxField>
      <asp:BoundField DataField="Comment" HeaderText="Comment" 
        ItemStyle-HorizontalAlign="Left" >
<ItemStyle HorizontalAlign="Left"></ItemStyle>
      </asp:BoundField>
    </Columns>
    <EditRowStyle BackColor="#999999" />
    <FooterStyle BackColor="#7B5D9D" Font-Bold="True" ForeColor="White" />
    <HeaderStyle BackColor="#7B5D9D" Font-Bold="True" ForeColor="White" />
    <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
    <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
    <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
    <SortedAscendingCellStyle BackColor="#E9E7E2" />
    <SortedAscendingHeaderStyle BackColor="#506C8C" />
    <SortedDescendingCellStyle BackColor="#FFFDF8" />
    <SortedDescendingHeaderStyle BackColor="#6F8DAE" />
  </asp:GridView>
</asp:Content>
