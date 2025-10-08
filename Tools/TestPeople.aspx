<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
  CodeBehind="TestPeople.aspx.cs" Inherits="TrackerDotNet.Tools.TestPeople" MaintainScrollPositionOnPostback="true" %>

<asp:Content ID="cntLookupHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntLookupBdy" ContentPlaceHolderID="MainContent" runat="server">
              <asp:GridView ID="gvPeople" runat="server" AllowPaging="True" AllowSorting="True"
                AutoGenerateColumns="False" CellPadding="1" PageSize="20" DataKeyNames="PersonID"
                DataSourceID="odsPeople" ForeColor="#333333" GridLines="Vertical">
                <AlternatingRowStyle BackColor="White" />
                <Columns>
                  <asp:CommandField ShowEditButton="True" ButtonType="Button" />
                  <asp:BoundField DataField="PersonID" HeaderText="PersonID" InsertVisible="False"
                    ReadOnly="True" SortExpression="PersonID" />
                  <asp:BoundField DataField="Person" HeaderText="Person" SortExpression="Person" />
                  <asp:BoundField DataField="Abreviation" HeaderText="Abreviation" SortExpression="Abreviation" />
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
  <asp:ObjectDataSource ID="odsPeople" runat="server"
    TypeName="TrackerDotNet.Controls.PersonsTbl" 
    DataObjectTypeName="TrackerDotNet.Controls.PersonsTbl"  
    SelectMethod="GetAll" SortParameterName="SortBy"
    UpdateMethod="UpdatePerson" OldValuesParameterFormatString="original_{0}" 
    DeleteMethod="DeletePerson" InsertMethod="InsertPerson" >
    <SelectParameters>
      <asp:Parameter DefaultValue="&quot;Abreviation&quot;" Name="SortBy" Type="String" />
    </SelectParameters>
    <UpdateParameters>
      <asp:Parameter Name="pPerson" Type="Object" />
    </UpdateParameters>
    <InsertParameters>
      <asp:Parameter Name="pPerson" Type="Object" />
    </InsertParameters>
    <DeleteParameters>
      <asp:Parameter Name="pPersonID" Type="Int32" />
    </DeleteParameters>
  </asp:ObjectDataSource>
</asp:Content>
