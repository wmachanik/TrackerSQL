<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ShowTableStruct.aspx.cs" Inherits="TrackerDotNet.test.ShowTableStruct" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Access Table Structure Viewer</title>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" />
        <div style="font-family: Calibri; padding: 20px;">
            <h2>Access Table Structure Viewer</h2>
            <br />
            <asp:CheckBox ID="chkFullView" runat="server" AutoPostBack="true"
                Text=" Show full schema view" OnCheckedChanged="chkFullView_CheckedChanged" />
            <asp:DropDownList ID="ddlTables" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlTables_SelectedIndexChanged" />
            <br />
            <br />
            <asp:Button ID="btnShowData" runat="server" Text="Show Top 10 Records" 
                OnClick="btnShowData_Click" Visible="false" CssClass="btn" 
                style="background-color: #007cba; color: white; padding: 5px 10px; border: none; border-radius: 3px;" />
            <br />
            <br />
            <asp:GridView ID="gvStructure" runat="server" AutoGenerateColumns="true" 
                CssClass="table" style="border-collapse: collapse; width: 100%;" 
                HeaderStyle-BackColor="#f0f0f0" HeaderStyle-Font-Bold="true" 
                RowStyle-BorderWidth="1px" RowStyle-BorderColor="#ddd" 
                HeaderStyle-BorderWidth="1px" HeaderStyle-BorderColor="#ddd" />
        </div>
        <br />
    </form>
</body>
</html>