<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="GroupItemDetail.aspx.cs"
  Inherits="TrackerDotNet.Pages.GroupItemDetail" Title="Group Item Detail" %>

<asp:Content ID="cntItemGroupDetailHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntItemGroupDetail" ContentPlaceHolderID="MainContent" runat="server">
  <asp:ScriptManager ID="scmGroupDetail" runat="server" />
  <asp:UpdatePanel ID="upnlGroupDetail" runat="server" UpdateMode="Always">
    <ContentTemplate>
      <h1>
        <asp:Label ID="lblAddOrEditItem" runat="server" Text="Add" />
        Group Item</h1>
      <div class="InputFrm">
        <table class="TblSimple">
          <tr>
            <td>Group Item Name:</td>
            <td>
              <asp:TextBox ID="tbxGroupItem" runat="server" Width="15em" />
              &nbsp;<asp:Label ID="lblGroupItemID" runat="server" CssClass="small" Visible="false" />
            </td>
          </tr>
          <tr>
            <td>Group Decription:</td>
            <td>
              <asp:TextBox ID="tbxGroupDesc" runat="server" Width="15em" /></td>
          </tr>
          <tr>
            <td>Group Short Name:</td>
            <td>
              <asp:TextBox ID="tbxGroupShortName" runat="server" Width="5em" /></td>
          </tr>
          <tr>
            <td colspan="2" class="rowC">
              <asp:Button ID="btnAdd" runat="server" Text="OK" OnClick="btnAdd_Click" />
              &nbsp;&nbsp;&nbsp;
          <asp:Button ID="btnUpdate" runat="server" Text="OK" OnClick="btnUpdate_Click" Visible="false" />
              &nbsp;&nbsp;&nbsp;
        <asp:Button ID="btnCancel" runat="server" Text="Cancel" OnClick="btnCancel_Click" />
            </td>
          </tr>
        </table>
      </div>
    </ContentTemplate>
  </asp:UpdatePanel>

</asp:Content>
