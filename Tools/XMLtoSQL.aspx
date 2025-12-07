<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="XMLtoSQL.aspx.cs"
  Inherits="TrackerSQL.test.XMLtoSQL" %>

<asp:Content ID="cntXMLtoSQLHdr" ContentPlaceHolderID="HeadContent" runat="server">
  <title>XML TO SQL Read and run</title>
  <script type="text/javascript">
      function showAppMessage(thisMessage) {
          alert(thisMessage);
      }

      function selectFile(filePath) {
          document.getElementById('<%= FileNameTextBox.ClientID %>').value = filePath;
      }
  </script>
  <style type="text/css">
    .file-browser {
      border: 1px solid #ccc;
      padding: 10px;
      margin: 10px 0;
      background-color: #f9f9f9;
      max-height: 200px;
      overflow-y: auto;
    }
    .file-item {
      cursor: pointer;
      padding: 3px;
      margin: 2px 0;
    }
    .file-item:hover {
      background-color: #e6f3ff;
    }
    .xml-file {
      color: #0066cc;
      font-weight: bold;
    }
  </style>
</asp:Content>

<asp:Content ID="cntXMLtoSQLBdy" ContentPlaceHolderID="MainContent" runat="server">
  <div style="font-family: Calibri">
    <h1>Read XML and Execute SQL</h1>
    
    <!-- File Selection Section -->
    <div>
      <label for="<%= FileNameTextBox.ClientID %>">File Path:</label><br />
      <asp:TextBox ID="FileNameTextBox" runat="server" Width="750px" />
      <asp:Button ID="RefreshFilesButton" runat="server" Text="Refresh Files" OnClick="RefreshFilesButton_Click" />
      <asp:Button ID="GoButton" Text="Execute" runat="server" OnClick="GoButton_Click" />
    </div>
    
    <!-- Available Files Browser -->
    <asp:Panel ID="pnlFileBrowser" runat="server" CssClass="file-browser">
      <strong>Available XML Files in App_Data:</strong><br />
      <asp:Literal ID="ltrlFileList" runat="server" />
    </asp:Panel>
    
    <br />
    
    <!-- Results Section -->
    <asp:GridView ID="gvSQLResults" runat="server"
      BorderColor="Tan" BorderWidth="1px" CellPadding="1" ForeColor="Black"
      Font-Size="Small" Font-Names="Calibri" CssClass="TblZebra">
      <AlternatingRowStyle BorderColor="#FFCC99" />
    </asp:GridView>
    
    <br />
    
    <asp:Panel ID="pnlSQLResults" runat="server">
    </asp:Panel>
  </div>
</asp:Content>