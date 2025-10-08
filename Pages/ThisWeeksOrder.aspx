<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ThisWeeksOrder.aspx.cs" Inherits="TrackerDotNet.Pages.ThisWeeksOrder" %>
<asp:Content ID="cntThisWeeksOrderHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntThisWeeksOrderBdy" ContentPlaceHolderID="MainContent" runat="server">
  <h1>Outstanding Orders</h1>
  <p>A list of orders for this week:</p>
  <asp:ScriptManager ID="smOrderSummary" runat="server">
  </asp:ScriptManager>
  <div class="floatRight">Clients per page:
    <asp:DropDownList ID="ddlOrdersPerPage" runat="server"  AutoPostBack="true" 
      onselectedindexchanged="ddlOrdersPerPage_SelectedIndexChanged">
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
  </div>
  <asp:UpdateProgress ID="uproOrderSummary" runat="server" >
    <ProgressTemplate>Refreshing?
    </ProgressTemplate>
  </asp:UpdateProgress>
  <br />
  <asp:UpdatePanel ID="upanOrderSummary" runat="server">
    <ContentTemplate>
      <p>This weeks orders:</p>
      <asp:GridView ID="gvOutstandingOrders" 
        runat="server" AllowPaging="True" AllowSorting="True" 
        AutoGenerateColumns="False" BackColor="White" BorderColor="#DEDFDE" 
        BorderStyle="Solid" BorderWidth="1px" CellPadding="4" 
        DataSourceID="odsOpenOrders" ForeColor="Black" GridLines="Vertical" 
        PageSize="50">
        <AlternatingRowStyle BackColor="White" />
        <Columns>
          <asp:BoundField DataField="CoName" HeaderText="Company" ReadOnly="True" 
            SortExpression="CoName" />
          <asp:BoundField DataField="ContactFirstName" HeaderText="FirstName" 
            SortExpression="ContactFirstName" />
          <asp:BoundField DataField="ContactLastName" HeaderText="LastName" 
            SortExpression="ContactLastName" />
          <asp:BoundField DataField="EmailAddress" HeaderText="Email" 
            SortExpression="EmailAddress" />
          <asp:BoundField DataField="OrderDate" DataFormatString="{0:d}" 
            HeaderText="OrderDate" SortExpression="OrderDate" />
          <asp:BoundField DataField="RoastDate" DataFormatString="{0:d}" 
            HeaderText="RoastDate" SortExpression="RoastDate" />
          <asp:BoundField DataField="ItemDesc" HeaderText="Item Order" 
            SortExpression="ItemDesc" />
          <asp:BoundField DataField="QuantityOrdered" HeaderText="Qty" 
            SortExpression="QuantityOrdered" />
          <asp:BoundField DataField="RequiredByDate" DataFormatString="{0:d}" 
            HeaderText="ByDate" SortExpression="RequiredByDate" />
          <asp:BoundField DataField="Abreviation" HeaderText="DlvrdBy" 
            SortExpression="Abreviation" />
          <asp:BoundField DataField="CompanyName" HeaderText="CompanyName" 
            SortExpression="CompanyName" Visible="False" />
          <asp:BoundField DataField="Expr1009" HeaderText="Expr1009" 
            SortExpression="Expr1009" Visible="False" />
          <asp:BoundField DataField="ToBeDeliveredBy" HeaderText="ToBeDeliveredBy" 
            SortExpression="ToBeDeliveredBy" Visible="False" />
          <asp:BoundField DataField="Person" HeaderText="Person" SortExpression="Person" 
            Visible="False" />
          <asp:CheckBoxField DataField="Done" HeaderText="Done" SortExpression="Done" 
            Visible="False" />
          <asp:BoundField DataField="Notes" HeaderText="Notes" SortExpression="Notes" 
            Visible="False" />
        </Columns>
        <FooterStyle BackColor="#CCCC99" />
        <HeaderStyle BackColor="#6B696B" Font-Bold="True" ForeColor="White" />
        <PagerStyle BackColor="#F7F7DE" ForeColor="Black" HorizontalAlign="Right" />
        <RowStyle BackColor="#F7F7DE" />
        <SelectedRowStyle BackColor="#CE5D5A" Font-Bold="True" ForeColor="White" />
        <SortedAscendingCellStyle BackColor="#FBFBF2" />
        <SortedAscendingHeaderStyle BackColor="#848384" />
        <SortedDescendingCellStyle BackColor="#EAEAD3" />
        <SortedDescendingHeaderStyle BackColor="#575357" />
      </asp:GridView>

      <asp:ObjectDataSource ID="odsOpenOrders" runat="server" 
        OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" 
        
        TypeName="QOnT.DataSets.OpenOrdersDataSetTableAdapters.OrdersToDoQryTableAdapter">
      </asp:ObjectDataSource>

    </ContentTemplate>
    <Triggers>
      
      <asp:AsyncPostBackTrigger ControlID="ddlOrdersPerPage" 
        EventName="SelectedIndexChanged" />
      
    </Triggers>
  </asp:UpdatePanel>
  <br />


</asp:Content>
