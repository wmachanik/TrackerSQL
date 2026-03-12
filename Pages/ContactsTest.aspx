<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ContactsTest.aspx.cs" Inherits="TrackerSQL.Pages.ContactsTest" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Contacts Data Test</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h1>Contact Data Diagnostic Test</h1>
            <asp:Button ID="btnTest" runat="server" Text="Run Test" OnClick="btnTest_Click" />
            <hr />
            <asp:Label ID="lblResults" runat="server" />
            <hr />
            <asp:GridView ID="gvTestData" runat="server" AutoGenerateColumns="true" />
        </div>
    </form>
</body>
</html>
