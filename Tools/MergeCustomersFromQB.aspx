<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MergeCustomersFromQB.aspx.cs"
   Inherits="TrackerDotNet.Tools.MergeCustomersFromQB" %>
<asp:Content ID="hdrMergeCustomersFromQB" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="bodyMergeCustomersFromQB" ContentPlaceHolderID="MainContent" runat="server">
  <br />
  <div style="padding: 4px; vertical-align: middle">
    Select file to upload: <asp:FileUpload ID="MergeFileUpload" runat="server" ToolTip="Merge file to import" BackColor="YellowGreen" Width="20em" />&nbsp;&nbsp;&nbsp;
  Start at: <asp:DropDownList runat="server" ID="StartDropDownList" />&nbsp;&nbsp;&nbsp;
  Finish at: <asp:DropDownList runat="server" ID="FinishDropDownList" />&nbsp;&nbsp;&nbsp;&nbsp;
  Max rec: <asp:DropDownList runat="server" ID="MaxRecsDropDownList" />&nbsp;&nbsp;&nbsp;&nbsp;
  <asp:Button ID="SelectImportFileButton" Text="Import" runat="server" OnClick="SelectImportFileButton_Click" />
  </div>
  Status:&nbsp;<asp:Label ID="StatusLabel" Text="" runat="server" />
  <asp:GridView ID="gvCustomers" runat="server" AllowPaging="true" 
    OnPageIndexChanging="gvCustomers_PageIndexChanging" />
</asp:Content>
