<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="SupportTables.aspx.cs" Inherits="TrackerDotNet.Pages.SupportTables" %>
<asp:Content ID="cntSupportTablesHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntSupporTablesBdy" ContentPlaceHolderID="MainContent" runat="server">
  <h1>Support Tables</h1>
  <asp:ScriptManager ID="smSupporTables" runat="server"></asp:ScriptManager>
  <asp:UpdateProgress ID="uprgSupporTables" runat="server" AssociatedUpdatePanelID="upnlSupporTables" >
    <ProgressTemplate>
      <img src="../images/animi/BlueArrowsUpdate.gif" alt="updating" width="16" height="16" />updating.....
    </ProgressTemplate>
  </asp:UpdateProgress>

  <div class="simpleLightBrownForm">
    Table: 
    <asp:DropDownList ID="ddlTables" runat="server" AutoPostBack="True" 
      OnSelectedIndexChanged="ddlTables_SelectedIndexChanged">
      <asp:ListItem Text="---Select---"></asp:ListItem>
      <asp:ListItem Text="Items"></asp:ListItem>
      <asp:ListItem Text="People / DelvieryBy"></asp:ListItem>
      <asp:ListItem Text="Equipment"></asp:ListItem>
      <asp:ListItem Text="CityPrepDays"></asp:ListItem>
      <asp:ListItem Text="Packaging"></asp:ListItem>
    </asp:DropDownList>

  </div>
  <asp:UpdatePanel ID="upnlSupporTables" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
    <ContentTemplate>
      <asp:GridView ID="gvSupporTable" runat="server" AutoGenerateColumns="True">
      </asp:GridView>
    </ContentTemplate>
    <Triggers>
      <asp:AsyncPostBackTrigger ControlID="ddlTables" EventName="SelectedIndexChanged" />
    </Triggers>
  </asp:UpdatePanel>


 <asp:ObjectDataSource ID="odsItemTypeTbl" runat="server" TypeName="TrackerDotNet.Controls.ItemTypeTbl" 
   SelectMethod="GetAll" SortParameterName="SortBy"
   UpdateMethod="UpdateItem" 
   DataObjectTypeName="TrackerDotNet.Controls.ItemTypeTbl" 
   OldValuesParameterFormatString="original_{0}" >
 </asp:ObjectDataSource>

</asp:Content>
