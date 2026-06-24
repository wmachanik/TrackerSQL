<%@ Page Title="Support Tables" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="SupportTables.aspx.cs" Inherits="TrackerSQL.Pages.SupportTables" %>
<asp:Content ID="cntSupportTablesHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntSupporTablesBdy" ContentPlaceHolderID="MainContent" runat="server">
  <h1>Support Tables (Backup Editor)</h1>
  <p style="font-size: 0.85em; color: #666;">Simple editor for lookup/dropdown tables. For full editing use <a href="Lookups.aspx">Lookups</a> page.</p>
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
      <asp:ListItem Text="---Select a Table---" Value=""></asp:ListItem>
      <asp:ListItem Text="Areas" Value="Areas"></asp:ListItem>
      <asp:ListItem Text="Area Prep Days" Value="AreaPrepDays"></asp:ListItem>
      <asp:ListItem Text="Equipment Types" Value="EquipmentTypes"></asp:ListItem>
      <asp:ListItem Text="Invoice Types" Value="InvoiceTypes"></asp:ListItem>
      <asp:ListItem Text="Item Packaging" Value="ItemPackaging"></asp:ListItem>
      <asp:ListItem Text="Items" Value="Items"></asp:ListItem>
      <asp:ListItem Text="Payment Terms" Value="PaymentTerms"></asp:ListItem>
      <asp:ListItem Text="People / Delivery By" Value="People"></asp:ListItem>
      <asp:ListItem Text="Price Levels" Value="PriceLevels"></asp:ListItem>
      <asp:ListItem Text="Repair Statuses" Value="RepairStatuses"></asp:ListItem>
    </asp:DropDownList>
  </div>
  <asp:UpdatePanel ID="upnlSupporTables" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
    <ContentTemplate>
      <asp:Label ID="lblStatus" runat="server" />
      <asp:GridView ID="gvSupporTable" runat="server" AutoGenerateColumns="False"
          AllowPaging="True" AllowSorting="True" PageSize="20"
          OnPageIndexChanging="gvSupporTable_PageIndexChanging"
          OnSorting="gvSupporTable_Sorting"
          OnRowEditing="gvSupporTable_RowEditing"
          OnRowUpdating="gvSupporTable_RowUpdating"
          OnRowCancelingEdit="gvSupporTable_RowCancelingEdit"
          CssClass="results-table">
      </asp:GridView>
    </ContentTemplate>
    <Triggers>
      <asp:AsyncPostBackTrigger ControlID="ddlTables" EventName="SelectedIndexChanged" />
    </Triggers>
  </asp:UpdatePanel>
</asp:Content>


