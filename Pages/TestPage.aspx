<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestPage.aspx.cs" Inherits="TrackerDotNet.Pages.TestPage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
    </div>
      <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" DataSourceID="SqlDataSource1">
        <Columns>
          <asp:BoundField DataField="CustomerID" HeaderText="CustomerID" SortExpression="CustomerID" />
          <asp:BoundField DataField="OrderDate" HeaderText="OrderDate" SortExpression="OrderDate" />
        </Columns>
      </asp:GridView>
      <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:Tracker08ConnectionString %>" ProviderName="<%$ ConnectionStrings:Tracker08ConnectionString.ProviderName %>" SelectCommand="SELECT DISTINCT [CustomerID], [OrderDate] FROM [OrdersTbl] WHERE ([OrderDate] &gt; ?)">
        <SelectParameters>
          <asp:Parameter DefaultValue="#2016/01/01#" Name="OrderDate" Type="DateTime" />
        </SelectParameters>
      </asp:SqlDataSource>
    </form>
</body>
</html>
