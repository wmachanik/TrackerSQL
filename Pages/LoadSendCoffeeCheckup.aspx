<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
   CodeBehind="LoadSendCoffeeCheckup.aspx.cs" Inherits="TrackerDotNet.Pages.LoadSendCoffeeCheckup" %>

<asp:Content ID="cntSendCoffeeCheckupHdr" ContentPlaceHolderID="HeadContent" runat="server">
  <script type="text/javascript">
    window.onload = function () {
      window.location = "SendCoffeeCheckup.aspx";
    }
  
  </script>
</asp:Content>
<asp:Content ID="cntSendCoffeeCheckupBdy" ContentPlaceHolderID="MainContent" runat="server">
  <h1>Send Coffee Reminder to Customers</h1>
  <div id="DivLoading" runat="server">
      <img src="../images/animi/BlueArrowsUpdate.gif" alt="loading..." width="16" height="16" />loading.....
  </div>
</asp:Content>
