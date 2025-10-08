<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="SummaryOFCoffeeRequired.aspx.cs" Inherits="TrackerDotNet.Pages.SummaryOFCoffeeRequired" %>
<asp:Content ID="cntSummaryOfCoffeeRequiredHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntSummaryOfCoffeeRequiredBdy" ContentPlaceHolderID="MainContent" runat="server">
  <h1>SummaryOFCoffeeRequired</h1>

  <div class="simpleForm">
    For Which week would you like the summary:&nbsp;
    <asp:DropDownList ID="ddlWhichWeek" runat="server" AutoPostBack="true" 
      onselectedindexchanged="ddlWhichWeek_SelectedIndexChanged">
    </asp:DropDownList>
  </div>
  
</asp:Content>
