<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="QuaffeeCoffeeTastingSheet.aspx.cs" Inherits="TrackerDotNet.Pages.QuaffeeCoffeeTastingSheet" %>

<asp:Content ID="cntHeaderTasting" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntBodyTasting" ContentPlaceHolderID="MainContent" runat="server">
  <b>View the SCAA tasting wheel here:</b>
  <a href="http://www.scaa.org/chronicle/wp-content/uploads/2016/01/SCAA_FlavorWheel.01.18.15.jpg">2016 01 SCAA FlavorWheel</a><br />
  <iframe src="https://docs.google.com/forms/d/1IX10XYwA-6NmgLaVluEW2YfsrdDhDozla7zsb8uzyiM/viewform?embedded=true" width="100%" height="2200" 
    frameborder="0" marginheight="0" marginwidth="0">Loading...</iframe>

</asp:Content>
