<%@ Page Title="Log List" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="LogTable.aspx.cs" Inherits="TrackerDotNet.Pages.LogTable" %>
<asp:Content ID="cntLogHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntLogBdy" ContentPlaceHolderID="MainContent" runat="server">
  <h1>List of Log</h1>
  <asp:ScriptManager ID="smLogSummary" runat="server"></asp:ScriptManager>
  <asp:UpdateProgress ID="uprgLogSummary" runat="server" AssociatedUpdatePanelID="upnlLogSummary" >
    <ProgressTemplate>
      <img src="../images/animi/BlueArrowsUpdate.gif" alt="updating" width="16" height="16" />updating...
    </ProgressTemplate>
  </asp:UpdateProgress>
  <div class="simpleLightBrownForm">
    <asp:UpdatePanel ID="upnlSelection" runat="server" UpdateMode="Conditional" >
      <ContentTemplate>
        Filter by: 
        <asp:DropDownList ID="ddlFilterBy" runat="server" Font-Size="X-Small" ToolTip="select which item to search form">
          <asp:ListItem Selected="True" value="DateLogged" Text="none" />
          <asp:ListItem value="CompanyID" Text="Company Name" />
          <asp:ListItem value="UserID" Text="User Name" />
        </asp:DropDownList>
        &nbsp;
        <asp:TextBox ID="tbxFilterBy" runat="server" ToolTip="add '%' to beginning to find contains" 
          OnTextChanged="tbxFilterBy_TextChanged" Width="14em"  />
        &nbsp;&nbsp;
        <asp:Button ID="btnGo" Text="Go" runat="server" onclick="btnGo_Click" ToolTip="search for this item" />
        &nbsp;&nbsp;
        <asp:Button ID="btnReset" Text="Reset" runat="server" 
          onclick="btnReset_Click" />
        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
      </ContentTemplate>
       <Triggers>
        <asp:AsyncPostBackTrigger controlid="tbxFilterBy" EventName="TextChanged" />
         <asp:AsyncPostBackTrigger ControlID="btnGo" EventName="Click" />
    </Triggers>
    </asp:UpdatePanel>
  </div>
  <br />
  <asp:UpdatePanel ID="upnlLogSummary" runat="server" UpdateMode="Conditional" >
    <ContentTemplate>
      <div class="simpleForm" style="padding-left: 1em; padding-right: 1em">
        <asp:GridView ID="gvLog" runat="server" AutoGenerateColumns="False" 
          CssClass="TblWhite" AllowSorting="True" AllowPaging="True" 
          DataSourceID="odsLogTbl"  >
          <Columns>
            <asp:BoundField DataField="LogID" HeaderText="LogID" 
              SortExpression="LogID" />
            <asp:BoundField DataField="DateAdded" HeaderText="DateAdded" 
              SortExpression="DateAdded" />
            <asp:TemplateField HeaderText="User" SortExpression="UserID">
              <EditItemTemplate>
                <asp:TextBox ID="UserIDTextBox" runat="server" Text='<%# Bind("UserID") %>' />
              </EditItemTemplate>
              <ItemTemplate>
                <asp:Label ID="PersonUserNameLabel" runat="server" Text='<%# GetPersonsNameFromID( Eval("UserID").ToString() ) %>' />
              </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Section" SortExpression="SectionID">
              <EditItemTemplate>
                <asp:TextBox ID="SectionIDTextBox" runat="server" Text='<%# Bind("SectionID") %>' />
              </EditItemTemplate>
              <ItemTemplate>
                <asp:Label ID="SectionIDLabel" runat="server" Text='<%# GetSectionFromID(Eval("SectionID").ToString()) %>' />
              </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="TranactionType" SortExpression="TranactionTypeID">
              <EditItemTemplate>
                <asp:TextBox ID="TranactionTypeIDTextBox" runat="server" Text='<%# Bind("TranactionTypeID") %>' />
              </EditItemTemplate>
              <ItemTemplate>
                <asp:Label ID="TranactionTypeIDLabel" runat="server" Text='<%# GetTransactionFromID(Eval("TranactionTypeID").ToString()) %>' />
              </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Customer" SortExpression="CustomerID">
              <EditItemTemplate>
                <asp:TextBox ID="CustomerIDTextBox" runat="server" Text='<%# Bind("CustomerID") %>' />
              </EditItemTemplate>
              <ItemTemplate>
                <asp:Label ID="CustomerIDLabel" runat="server" Text='<%# GetCustomerFromID(Eval("CustomerID").ToString()) %>' />
              </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="Details" HeaderText="Details" 
              SortExpression="Details" />
            <asp:BoundField DataField="Notes" HeaderText="Notes" SortExpression="Notes" />
          </Columns>
        </asp:GridView>
        <asp:ObjectDataSource ID="odsLogTbl" runat="server" SelectMethod="GetAll" 
          TypeName="TrackerDotNet.Controls.LogTbl">
          <SelectParameters>
            <asp:Parameter DefaultValue="DateAdded" Name="SortBy" Type="String" />
          </SelectParameters>
        </asp:ObjectDataSource>
      </div>
    </ContentTemplate>
    <Triggers>
      <asp:AsyncPostBackTrigger ControlID="btnGo" EventName="Click" />
      <asp:AsyncPostBackTrigger ControlID="btnReset" EventName="Click" />
    </Triggers>
  </asp:UpdatePanel>
  <asp:Label ID="lblFilter" runat="server" />
</asp:Content>
