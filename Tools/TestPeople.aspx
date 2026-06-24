<%@ Page Title="Test People" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
  CodeBehind="TestPeople.aspx.cs" Inherits="TrackerSQL.Tools.TestPeople" MaintainScrollPositionOnPostback="true" %>

<asp:Content ID="cntLookupHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntLookupBdy" ContentPlaceHolderID="MainContent" runat="server">
    <h2>People Management</h2>
    <asp:GridView ID="gvPeople" runat="server" AllowPaging="True" AllowSorting="True"
        AutoGenerateColumns="False" CellPadding="1" PageSize="20" DataKeyNames="PersonID"
        ForeColor="#333333" GridLines="Vertical"
        OnPageIndexChanging="gvPeople_PageIndexChanging"
        OnSorting="gvPeople_Sorting"
        OnRowEditing="gvPeople_RowEditing"
        OnRowCancelingEdit="gvPeople_RowCancelingEdit"
        OnRowUpdating="gvPeople_RowUpdating">
        <AlternatingRowStyle BackColor="White" />
        <Columns>
            <asp:CommandField ShowEditButton="True" ButtonType="Button" />
            <asp:BoundField DataField="PersonID" HeaderText="PersonID" InsertVisible="False"
                ReadOnly="True" SortExpression="PersonID" />
            <asp:BoundField DataField="PersonName" HeaderText="Name" SortExpression="PersonName" />
            <asp:BoundField DataField="Abbreviation" HeaderText="Abbreviation" SortExpression="Abbreviation" />
            <asp:CheckBoxField DataField="Enabled" HeaderText="Enabled" SortExpression="Enabled" />
        </Columns>
        <EditRowStyle BackColor="#7C6F57" />
        <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
        <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
        <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
        <RowStyle BackColor="#E3EAEB" />
        <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333" />
        <SortedAscendingCellStyle BackColor="#F8FAFA" />
        <SortedAscendingHeaderStyle BackColor="#246B61" />
        <SortedDescendingCellStyle BackColor="#D4DFE1" />
        <SortedDescendingHeaderStyle BackColor="#15524A" />
    </asp:GridView>
</asp:Content>
