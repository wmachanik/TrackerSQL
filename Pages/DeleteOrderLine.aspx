<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="DeleteOrderLine.aspx.cs" Inherits="TrackerDotNet.Pages.DeleteOrderLine" %>
<asp:Content ID="cntDeleteOrderLineHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntDeleteOrderLineBody" ContentPlaceHolderID="MainContent" runat="server">
  <asp:DetailsView ID="dvDeleteOrderItem" runat="server" CssClass="TblZebra"
    AutoGenerateRows="False" DataSourceID="sdsOrderLine">
    <Fields>
      <asp:BoundField DataField="CompanyName" HeaderText="CompanyName" 
        SortExpression="CompanyName" />
      <asp:BoundField DataField="OrderDate" HeaderText="OrderDate" 
        SortExpression="OrderDate" DataFormatString="{0:d}" />
      <asp:BoundField DataField="RoastDate" HeaderText="RoastDate" 
        SortExpression="RoastDate" DataFormatString="{0:d}" />
      <asp:BoundField DataField="ItemDesc" HeaderText="ItemDesc" 
        SortExpression="ItemDesc" />
      <asp:BoundField DataField="QuantityOrdered" HeaderText="QuantityOrdered" 
        SortExpression="QuantityOrdered" />
      <asp:BoundField DataField="RequiredByDate" HeaderText="RequiredByDate" 
        SortExpression="RequiredByDate" DataFormatString="{0:d}" />
    </Fields>
  </asp:DetailsView>
  <asp:SqlDataSource ID="sdsOrderLine" runat="server" 
    ConnectionString="<%$ ConnectionStrings:Tracker08ConnectionString %>" 
    ProviderName="<%$ ConnectionStrings:Tracker08ConnectionString.ProviderName %>" 
    SelectCommand="SELECT CustomersTbl.CompanyName, OrdersTbl.OrderDate, OrdersTbl.RoastDate, ItemTypeTbl.ItemDesc, OrdersTbl.QuantityOrdered, OrdersTbl.RequiredByDate FROM (((OrdersTbl LEFT OUTER JOIN ItemTypeTbl ON OrdersTbl.ItemTypeID = ItemTypeTbl.ItemTypeID) LEFT OUTER JOIN CustomersTbl ON OrdersTbl.CustomerID = CustomersTbl.CustomerID) LEFT OUTER JOIN PackagingTbl ON OrdersTbl.PackagingID = PackagingTbl.PackagingID) WHERE (OrdersTbl.OrderID = ?)">
    <SelectParameters>
      <asp:QueryStringParameter Name="?" QueryStringField="OrderId" />
    </SelectParameters>
  </asp:SqlDataSource> &nbsp;&nbsp;&nbsp;
  <asp:Button ID="btnDelete" Text="Delete" runat="server" 
    onclick="btnDelete_Click" /> &nbsp;&nbsp;&nbsp;
  <asp:Button ID="btnReturn" Text="Return" runat="server" 
    onclick="btnReturn_Click" />

  <br />
  <br />
  <asp:Literal ID="ltrlStatus" Text="" runat="server" />
  

</asp:Content>
