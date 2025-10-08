<%@ Page Title="Tracker Client Dump" Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeBehind="ClientList.aspx.cs" Inherits="TrackerDotNet.Pages.ClientListForm" %>
<%@ Register assembly="AjaxControlToolkit" namespace="AjaxControlToolkit.HTMLEditor" tagprefix="cc1" %>
<asp:Content ID="cntClientDumpHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntClientDump" ContentPlaceHolderID="MainContent" runat="server">
  <h1>Client List</h1>
  <p>Please select the type of client (enabled or disabled) and the nubmer of clients per page to display</p>
  <asp:Label ID="lblRoastDate" CssClass="floatRight" runat="server" Text="dt" />
  <table cellpadding="0" cellspacing="0" width="100%">
    <tr>
      <td>Company Name Contains&nbsp;<asp:TextBox ID="tbxCompanyName" runat="server" />&nbsp;&nbsp;&nbsp;
        <asp:Button ID="btnGo" Text="Go" runat="server" onclick="btnGo_Click" /></td>
      <td>Client is:&nbsp;<asp:DropDownList ID="ddlClientEnabled" runat="server" AutoPostBack="true"
          onselectedindexchanged="ddlClientsPerPage_SelectedIndexChanged">
          <asp:ListItem Value="true" Selected="true" >Enabled</asp:ListItem>
          <asp:ListItem Value="false">Disabled</asp:ListItem>
        </asp:DropDownList>
      </td>
      <td>&nbsp;</td>
      <td style="text-align: right">Clients/page:</td>
      <td>
        <asp:DropDownList ID="ddlClientsPerPage" runat="server"  AutoPostBack="true"
          onselectedindexchanged="ddlClientsPerPage_SelectedIndexChanged">
          <asp:ListItem>5</asp:ListItem>
          <asp:ListItem >10</asp:ListItem>
          <asp:ListItem>20</asp:ListItem>
          <asp:ListItem>30</asp:ListItem>
          <asp:ListItem Selected="True">50</asp:ListItem>
          <asp:ListItem>75</asp:ListItem>
          <asp:ListItem>100</asp:ListItem>
          <asp:ListItem>250</asp:ListItem>
          <asp:ListItem>500</asp:ListItem>
          <asp:ListItem>750</asp:ListItem>
          <asp:ListItem>1000</asp:ListItem>
        </asp:DropDownList>
      </td>
    </tr>
  </table>
  <br />
  <asp:Button ID="btnSendBulkEmail" runat="server" Text="EMail To All" onclick="btnSendBulkEmail_Click" />
  <br />
  <asp:ScriptManager ID="smClientDetails" runat="server">
  </asp:ScriptManager>
  <asp:UpdateProgress ID="uprgClientDetails" runat="server" >
    <ProgressTemplate>
      <img alt="Please Wait... " src="../images/animi/QuaffeeProgress.gif" width="128" height="15" /><asp:Literal ID="ltrlProgessTest" runat="server" Text="..." />
    </ProgressTemplate>
  </asp:UpdateProgress>
  <asp:UpdatePanel ID="upnlSendEmail" runat="server" UpdateMode="Conditional" >
    <ContentTemplate>
      <asp:Panel id="pnlSentEmail" runat="server" Visible="false">
        <table class="InputFrm" >
          <tr>
            <td>Subject</td>
            <td><asp:TextBox ID="tbxSubject" runat="server" Width="25em" /></td>
            <td>&nbsp;</td>
            <td>Time</td>
            <td><asp:TextBox ID="tbxTime" runat="server" /></td>
          </tr>
          <tr>
            <td colspan="5"><h4>Body</h4></td>
          </tr>
          <tr>
            <td colspan="5"><ajaxToolkit:Editor ID="edtBody" runat="server" Height="15em" /></td>
          </tr>
          <tr>
            <td colspan="5" >&nbsp;</td>
          </tr>
          <tr class="rowOdd" >
            <td colspan="5" ><asp:CheckBox ID="cbxSendTest" runat="server" Checked="false" Text="Send a Test email: " />
              &nbsp;<asp:TextBox ID="tbxTestEmail" runat="server" text="warrenm@quaffee.co.za" Width="30em" /> </td>
          </tr>
          <tr>
            <td>&nbsp;</td>
            <td colspan="3" class="rowC" style="text-align:center" >
              <asp:Button ID="btnSendIt" runat="server" Text="Send Email" onclick="btnSendIt_Click" />
              &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
              <asp:Button ID="btnCancel" runat="server" Text="Cancel" onclick="btnCancel_Click" />
            </td>
            <td>&nbsp;</td>
          </tr>
          <tr>
            <td>&nbsp;</td>
            <td class="rowC" colspan="3" style="text-align:center">
              <asp:Literal ID="ltrlEmailStatus" runat="server" /></td>
            <td>&nbsp;</td>
          </tr>
        </table>
      </asp:Panel>
    </ContentTemplate>
    <Triggers>
      <asp:AsyncPostBackTrigger ControlID="btnCancel" EventName="Click" />
    </Triggers>
  </asp:UpdatePanel>

  <asp:UpdatePanel ID="upnlClientDetails" runat="server" UpdateMode="Conditional" >
    <ContentTemplate>
    <asp:GridView ID="gvClients" runat="server" AllowPaging="True" BorderStyle="Outset" 
      AllowSorting="True" AutoGenerateColumns="False" BackColor="White" 
      BorderColor="#999999" BorderWidth="1px" CellPadding="3" 
      DataSourceID="odsCustomers" GridLines="Vertical" PageSize="50">
      <AlternatingRowStyle BackColor="#DCDCDC" />
      <Columns>
        <asp:HyperLinkField 
          DataNavigateUrlFields="CustomerID" 
          HeaderText="Company Name" SortExpression="CompanyName" 
          DataNavigateUrlFormatString="~/Pages/CustomerDetails.aspx?ID={0}&" 
          DataTextField="CompanyName"  />
        <asp:TemplateField HeaderText="First Name" SortExpression="ContactFirstName">
          <EditItemTemplate>
            <asp:TextBox ID="tbxContactFirstname" runat="server" 
              Text='<%# Bind("ContactFirstName") %>'></asp:TextBox>
          </EditItemTemplate>
          <ItemTemplate>
            <asp:Label ID="lblContactFirstName" runat="server" Text='<%# Bind("{0, 1}}"ContactFirstName") %>'></asp:Label>
          </ItemTemplate>
        </asp:TemplateField>
        <asp:TemplateField HeaderText="Last Name" SortExpression="ContactLastName">
          <EditItemTemplate>
            <asp:TextBox ID="tbxContactLastName" runat="server" Text='<%# Bind("ContactLastName") %>'></asp:TextBox>
          </EditItemTemplate>
          <ItemTemplate>
            <asp:Label ID="lblContactLastName" runat="server" Text='<%# Bind("ContactLastName") %>'></asp:Label>
          </ItemTemplate>
        </asp:TemplateField>
        <asp:BoundField DataField="PhoneNumber" HeaderText="Phone" 
          SortExpression="PhoneNumber" />
        <asp:BoundField DataField="CellNumber" HeaderText="Cell" 
          SortExpression="CellNumber" />
        <asp:BoundField DataField="EmailAddress" 
          HeaderText="Email" SortExpression="EmailAddress" />
        <asp:BoundField DataField="EquipTypeName" HeaderText="Equip" 
          SortExpression="EquipTypeName" />
        <asp:BoundField DataField="ItemDesc" HeaderText="Item" 
          SortExpression="ItemDesc" />
        <asp:BoundField DataField="PriPrefQty" HeaderText="Qty" 
          SortExpression="PriPrefQty" >
        <ItemStyle HorizontalAlign="Right" />
        </asp:BoundField>
        <asp:BoundField DataField="MachineSN" HeaderText="S/N" 
          SortExpression="MachineSN" />
        <asp:CheckBoxField DataField="autofulfill" HeaderText="auto?" 
          SortExpression="autofulfill" >
        <ItemStyle HorizontalAlign="Center" />
        </asp:CheckBoxField>
        <asp:BoundField DataField="AltEmailAddress" HeaderText="AltEmailAddress" 
          SortExpression="AltEmailAddress" Visible="False" />
        <asp:BoundField DataField="SalesInitials" HeaderText="Agent" 
          SortExpression="SalesInitials" />
        <asp:BoundField DataField="City" HeaderText="City" 
          SortExpression="City" Visible="False" />
        <asp:BoundField DataField="PreferedAgent" HeaderText="PreferedAgent" 
          SortExpression="PreferedAgent" Visible="False" />
        <asp:BoundField DataField="SalesAgentID" HeaderText="SalesAgentID" 
          SortExpression="SalesAgentID" Visible="False" />
        <asp:BoundField DataField="CoffeePreference" HeaderText="CoffeePreference" 
          SortExpression="CoffeePreference" Visible="False" />
        <asp:CheckBoxField DataField="UsesFilter" HeaderText="UsesFilter" 
          SortExpression="UsesFilter" Visible="False" />
        <asp:BoundField DataField="Abreviation" HeaderText="Abreviation" 
          SortExpression="Abreviation" Visible="False" />
        <asp:CheckBoxField DataField="PredictionDisabled" 
          HeaderText="PredictionDisabled" SortExpression="PredictionDisabled" 
          Visible="False" />
        <asp:CheckBoxField DataField="AlwaysSendChkUp" HeaderText="AlwaysSendChkUp" 
          SortExpression="AlwaysSendChkUp" Visible="False" />
        <asp:CheckBoxField DataField="NormallyResponds" HeaderText="NormallyResponds" 
          SortExpression="NormallyResponds" Visible="False" />
        <asp:CheckBoxField DataField="enabled" HeaderText="enabled" 
          SortExpression="enabled" Visible="False" />
      </Columns>
      <FooterStyle BackColor="#CCCCCC" ForeColor="Black" />
      <HeaderStyle BackColor="#000084" Font-Bold="True" ForeColor="White" />
      <PagerSettings Mode="NumericFirstLast" />
      <PagerStyle BackColor="#999999" ForeColor="Black" HorizontalAlign="Right" />
      <RowStyle BackColor="#EEEEEE" ForeColor="Black" />
      <SelectedRowStyle BackColor="#008A8C" Font-Bold="True" ForeColor="White" />
      <SortedAscendingCellStyle BackColor="#F1F1F1" />
      <SortedAscendingHeaderStyle BackColor="#0000A9" />
      <SortedDescendingCellStyle BackColor="#CAC9C9" />
      <SortedDescendingHeaderStyle BackColor="#000065" />
    </asp:GridView>
      <asp:Literal ID="ltrlFilter" Text="" runat="server" />
    </ContentTemplate>
    
    <Triggers>
      <asp:AsyncPostBackTrigger ControlID="ddlClientEnabled" 
        EventName="SelectedIndexChanged" />
      <asp:AsyncPostBackTrigger ControlID="ddlClientsPerPage" 
        EventName="SelectedIndexChanged" />
    </Triggers>
    
  </asp:UpdatePanel>

  <asp:ObjectDataSource ID="odsCustomers" runat="server" 
    OldValuesParameterFormatString="original_{0}" 
    SelectMethod="GetCustomersByEnabled" 
    TypeName="TrackerDotNet.DataSets.CustomersDataSetTableAdapters.CustomersTableAdapter">
    <SelectParameters>
      <asp:ControlParameter ControlID="ddlClientEnabled" DefaultValue="true" 
        Name="enabled" PropertyName="SelectedValue" Type="Boolean" />
    </SelectParameters>
  </asp:ObjectDataSource>
</asp:Content>
