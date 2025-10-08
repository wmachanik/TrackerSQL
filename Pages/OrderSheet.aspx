<%@ Page Title="Order Summary Sheet" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="OrderSheet.aspx.cs" Inherits="TrackerDotNet.Pages.OrderSheet" %>
<%@ Register assembly="AjaxControlToolkit" namespace="AjaxControlToolkit" tagprefix="cc1" %>
<asp:Content ID="cntOrderSheetHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntOrderSheetBdy" ContentPlaceHolderID="MainContent" runat="server">
  <asp:ScriptManager ID="smgrOrdersSheet" runat="server" />
  <h1>Order Sheet</h1>
  <asp:UpdateProgress ID="uprgOrderEditDetail" runat="server" AssociatedUpdatePanelID="upnlButtonBar" >
    <ProgressTemplate>
      <img src="../images/animi/QuaffeeProgress.gif" alt="updating" width="128" height="15" />&nbsp;...</ProgressTemplate>
  </asp:UpdateProgress>
  <asp:UpdatePanel ID="upnlButtonBar" runat="server" UpdateMode="Conditional" >
    <ContentTemplate>
      <table id="ButtonBar" style="border: 0; width:100%">
        <tr>
          <td>
            Filter by:&nbsp;<asp:DropDownList ID="ddlFilterBy" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlFilterBy_SelectedIndexChanged">
              <asp:ListItem Selected="True" Value="none" Text="none" />
              <asp:ListItem Value="1" Text="Company name" />
              <asp:ListItem Value="2" Text="Delivery By" />
              <asp:ListItem Value="3" Text="Delivery Date" />
              <asp:ListItem Value="4" Text="Roast Date" />
              <asp:ListItem Value="5" Text="Agent & Delivery Date" />
              <asp:ListItem Value="6" Text="Agent & Roast Date" />
            </asp:DropDownList>&nbsp;
            <asp:TextBox id= "tbxFilter" runat="server" Width="20em" Enabled="false" />
            <asp:DropDownList ID="ddlDeliveryBy" runat="server" AutoPostBack="True" Visible="False"
              DataSourceID="odsDeliveryBy" DataTextField="Person" 
              DataValueField="Abreviation" />
            &nbsp;&nbsp;
            <asp:Button ID="btnGo" Text="Go" runat="server" onclick="btnGo_Click" />
          </td>
          <td align="right">
            <asp:CheckBox ID="cbxOrdersToDo" runat="server" Checked="true" 
              AutoPostBack="true" Text="ToDo" 
              oncheckedchanged="cbxOrdersToDo_CheckedChanged" />&nbsp;&nbsp;
            <asp:Button ID="btnPrint" runat="server" Text="Print" onclick="btnPrint_Click" />&nbsp;&nbsp;
          </td>
        </tr>
      </table>
    </ContentTemplate>
    <Triggers>
      <asp:AsyncPostBackTrigger ControlID="ddlFilterBy" 
        EventName="SelectedIndexChanged" />
    </Triggers>
  </asp:UpdatePanel>
  <asp:UpdatePanel ID="upnlOrderSheet" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
      <asp:GridView ID="gvOrderSheet" runat="server" BorderStyle="Solid" CellPadding="4" 
        OnRowCreated="gvOrderSheet_OnRowCreated" OnRowDataBound="gvOrderSheet_OnRowDataBound"
        DataSourceID="odsOrderSheet" ForeColor="#333333" AllowSorting="True" 
        Width="100%" ShowFooter="True" 
        onselectedindexchanged="gvOrderSheet_SelectedIndexChanged" >
        <AlternatingRowStyle BackColor="White" />
        <Columns>
          <asp:ButtonField ButtonType="Image" CommandName="Select"  
            ImageUrl="~/images/imgButtons/OpenItem.gif" Text="Open" />
        </Columns>
        <EditRowStyle BackColor="#7C6F57" />
        <EmptyDataTemplate>
          no un done orders found
        </EmptyDataTemplate>
        <FooterStyle BackColor="#FFFFCC" Font-Bold="True" ForeColor="Black" />
        <HeaderStyle BackColor="#CCFFCC" Font-Bold="True" ForeColor="Black" />
        <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
        <RowStyle BackColor="#E3EAEB" />
        <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333" />
        <SortedAscendingCellStyle BackColor="#F8FAFA" />
        <SortedAscendingHeaderStyle BackColor="#246B61" />
        <SortedDescendingCellStyle BackColor="#D4DFE1" />
        <SortedDescendingHeaderStyle BackColor="#15524A" />
      </asp:GridView>
      <br />
      <asp:Literal ID="ltrlFilter" Text="" runat="server" />
    </ContentTemplate>
    <Triggers>
      <asp:AsyncPostBackTrigger ControlID="btnGo" EventName="Click" />
    </Triggers>
  </asp:UpdatePanel>
  <asp:ObjectDataSource ID="odsOrderSheet" runat="server" 
    OldValuesParameterFormatString="original_{0}" SelectMethod="GetOrdersToDoSheet" 
    TypeName="TrackerDotNet.DataSets.OrderSheetTableAdapters.OrderSheetTableAdapter">
    <SelectParameters>
      <asp:Parameter DefaultValue="false" Name="Done" Type="Boolean" />
    </SelectParameters>
  </asp:ObjectDataSource>
  <asp:ObjectDataSource ID="odsDeliveryBy" runat="server" OldValuesParameterFormatString="original_{0}"
    SelectMethod="GetPeople" TypeName="TrackerDotNet.DataSets.LookUpDatSetsTableAdapters.PersonsLkupTableAdapter">
  </asp:ObjectDataSource>
<br /><br />
<asp:Table ID="tblTotalsSummary" runat="server">
  <asp:TableFooterRow>
    <asp:TableCell>Footer</asp:TableCell>
  </asp:TableFooterRow>
</asp:Table>

</asp:Content>
