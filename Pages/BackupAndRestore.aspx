<%@ Page Title="Backup and Restore" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
 CodeBehind="BackupAndRestore.aspx.cs" Inherits="TrackerDotNet.Pages.BackupAndRestore" %>

<asp:Content ID="cntBackupHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntRestorHdr" ContentPlaceHolderID="MainContent" runat="server">
  <h1>Backup and Restore</h1>
  <div class="simpleForm">
    <table class="TblWhite">
      <tbody>
        <tr>
          <td>Backup and restore Security From Server to Local</td>
          <td>
            <asp:Button Text="Go Security 2 Local" ID="btnSecurty2Local" runat="server" 
              onclick="btnSecurty2Local_Click" /></td>
        </tr>
      </tbody>
    </table>
  </div>
  <br />
  <asp:Literal ID="ltrlMsg" runat="server" />
  <br />
  <br />
  <br />
  <br />
  <br />
  </asp:Content>
