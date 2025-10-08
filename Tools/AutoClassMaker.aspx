<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AutoClassMaker.aspx.cs" Inherits="TrackerDotNet.test.AutoClassMaker" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Access Auto class maker</title>
</head>
<body>
    <form id="frmAutoClassMaker" runat="server">
    <div>
      <asp:ScriptManager ID="smgrORMClass" runat="server">
      </asp:ScriptManager>
      <asp:UpdateProgress ID="uprgORMCLass" runat="server">
        <ProgressTemplate>updating...<img src="../images/animi/BlueArrowsUpdate.gif" alt="please wait" /></ProgressTemplate>
      </asp:UpdateProgress>
      <p>
        Select a table that you want to make a Automatic Class:</p>
      <table border="0" cellpadding="3" style="line-height:2em; font-size: large; font-family: Calibri">
        <tr>
          <td>Table Names:</td>
          <td><asp:DropDownList ID="ddlTables" AutoPostBack="true" runat="server" 
              OnSelectedIndexChanged="ddlTables_SelectedIndexChanged" style="padding: 2px; border: 1px solid #EEE; height: 2em; vertical-align: middle" /></td>
        </tr>
        <tr>
          <td>File name to save to:</td>
          <td>
            <asp:UpdatePanel ID="upnlClassFileName" runat="server">
              <ContentTemplate>
                <asp:TextBox ID="tbxORMClassFileName" runat="server" Text="MyORMClass.cs" style="padding: 2px; border: 1px solid #EEE; height: 2em; vertical-align: middle; width:25em" />
              </ContentTemplate>
              <Triggers>
                <asp:AsyncPostBackTrigger ControlID="ddlTables" 
                  EventName="SelectedIndexChanged" />
              </Triggers>
            </asp:UpdatePanel>
           </td>
        </tr>
        <tr>
          <td align="center" colspan="2" >
            <asp:Button ID="btnGo" Text="Create Class" runat="server"  OnClick="btnGo_Click" />
            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;  
            <asp:Button ID="btnCreateGV" Text="Create Gridview and OnRowCommand" runat="server" OnClick="btnCreateGV_Click" />
            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;  
            <asp:Button ID="btnCreateDV" Text="Create Detailsview and OnRowCommand" runat="server" OnClick="btnCreateDV_Click" />
           </td>
        </tr>
      </table>
    </div>
    </form>
</body>
</html>
