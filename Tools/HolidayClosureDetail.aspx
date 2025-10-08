<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="HolidayClosureDetail.aspx.cs" Inherits="TrackerDotNet.Tools.HolidayClosureDetail"
    Title="Closure Detail" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
  <h1>Closure Detail</h1>
  <asp:ScriptManager ID="smClosureDetail" runat="server" />

  <asp:UpdatePanel ID="upnlDetail" runat="server">
    <ContentTemplate>
      <asp:Label ID="lblMsg" runat="server" CssClass="Msg" /><br /><br />
      <asp:Panel ID="pnlDetail" runat="server" CssClass="simpleForm">
        <table class="TblCoffee">
          <tr style="display:none;">
            <td>ID</td>
            <td><asp:Label ID="lblID" runat="server" /></td>
          </tr>
          <tr>
            <td>Start Date</td>
            <td>
              <asp:TextBox ID="txtDate" runat="server" Width="120" />
              <ajaxToolkit:CalendarExtender ID="calDetailDate" runat="server"
                  TargetControlID="txtDate" Format="yyyy-MM-dd" FirstDayOfWeek="Monday" />
            </td>
          </tr>
          <tr>
            <td>Days Closed</td>
            <td><asp:TextBox ID="txtDays" runat="server" Width="60" Text="1" /></td>
          </tr>
          <tr>
            <td>Strategy</td>
            <td>
              <asp:DropDownList ID="ddlStrategy" runat="server">
                <asp:ListItem>Forward</asp:ListItem>
                <asp:ListItem>Backward</asp:ListItem>
                <asp:ListItem>Skip</asp:ListItem>
              </asp:DropDownList>
            </td>
          </tr>
          <tr>
            <td>Applies</td>
            <td>
              <asp:CheckBox ID="chkPrep" runat="server" Text="Prep" Checked="true" />
              &nbsp;&nbsp;
              <asp:CheckBox ID="chkDelivery" runat="server" Text="Delivery" Checked="true" />
            </td>
          </tr>
            <tr>
              <td>Description</td>
              <td><asp:TextBox ID="txtDesc" runat="server" Width="380" MaxLength="255" /></td>
            </tr>
          <tr>
            <td colspan="2" class="rowC">
              <asp:Button ID="btnSave" runat="server" Text="Save" OnClick="btnSave_Click" />
              &nbsp;&nbsp;
              <asp:Button ID="btnDelete" runat="server" Text="Delete" OnClick="btnDelete_Click" />
              &nbsp;&nbsp;
              <asp:HyperLink ID="lnkBack" runat="server" NavigateUrl="HolidayClosures.aspx" Text="Back" />
            </td>
          </tr>
        </table>
      </asp:Panel>
    </ContentTemplate>
  </asp:UpdatePanel>
</asp:Content>