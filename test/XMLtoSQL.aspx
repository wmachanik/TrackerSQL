<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="XMLtoSQL.aspx.cs" Inherits="TrackerDotNet.test.XMLtoSQL" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>XML TO SQL Read and run </title>
<script type="text/javascript">  QUA493299381
  function showAppMessage(thisMessage) {
    alert(thisMessage);
  }
</script>
</head>
<body>
    <form id="frmXMLToSQL" runat="server">
    <div style="font-family: Calibri">
      <h1>Read XML and do the SQL</h1>
      File name:&nbsp;
      <asp:TextBox ID="FileNameTextBox" runat="server" 
        Text="C:\MyDocuments\Projects\Websites\TrackerDotNet\TrackerDotNet\test\SQLCommandsAug4.xml" 
        Width="750px" />
      &nbsp;&nbsp;
      <asp:Button ID="GoButton" Text="Go" runat="server" onclick="GoButton_Click" />
      <br />
      <br />c:\websites\QonT\test\<br />
      <asp:GridView ID="gvSQLResults" runat="server" 
        BorderColor="Tan" BorderWidth="1px" CellPadding="1" ForeColor="Black" 
        Font-Size="Small" Font-Names="Calibri">
        <AlternatingRowStyle BorderColor="#FFCC99" />
      </asp:GridView>
      <br />
      <asp:Panel ID="pnlSQLResults" runat="server">  
        
      </asp:Panel>
      <br />

    </div>
    </form>
</body>
</html>
