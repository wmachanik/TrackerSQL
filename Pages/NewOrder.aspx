<%@ Page Language="C#"  MasterPageFile="~/Site.Master" AutoEventWireup="true" MaintainScrollPositionOnPostback="true" CodeBehind="NewOrder.aspx.cs" Inherits="TrackerDotNet.Pages.NewOrder" %>

<%--<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajc1" %>--%>
<asp:Content ID="cntOrderEditHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntOrderEditBdy" ContentPlaceHolderID="MainContent" runat="server">
  <h2>New Order</h2>
  <asp:ScriptManager ID="smgrOrdersEdit" runat="server">
  </asp:ScriptManager>
  <asp:UpdateProgress ID="uprgOrderEditDetail" runat="server" AssociatedUpdatePanelID="upnlOrderEditDetail" >
    <ProgressTemplate>
      <img src="../images/animi/BlueArrowsUpdate.gif" alt="updating" width="16" height="16" />&nbsp;Please wait..</ProgressTemplate>
  </asp:UpdateProgress>
  <asp:UpdatePanel ID="upnlOrderEditDetail" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
      <asp:DetailsView ID="dvOrderEdit" runat="server" AllowPaging="True" AutoGenerateRows="False"
        BackColor="LightGoldenrodYellow" BorderColor="Tan"  
        BorderWidth="1px" CellPadding="2" DefaultMode="Insert" OnItemInserted="dvOrderEdit_OnItemInserted"
        DataKeyNames="OrderID" DataSourceID="odsOrderDetail" OnDataBound="dvOrderEdit_OnDataBound"
        EmptyDataText="Please select an order, or add a New order" 
        EnablePagingCallbacks="True" ForeColor="Black" >
        <AlternatingRowStyle BackColor="PaleGoldenrod" />
        <CommandRowStyle HorizontalAlign="Center" VerticalAlign="Middle"  />
        <EditRowStyle BackColor="Honeydew" ForeColor="DarkOliveGreen" />
        <Fields>
          <asp:CommandField ButtonType="Button" ShowEditButton="True" 
            ShowInsertButton="True" />
          <asp:BoundField DataField="OrderID" HeaderText="OrderID" InsertVisible="False" ReadOnly="True"
            SortExpression="OrderID" Visible="False" />
          <asp:TemplateField HeaderText="Customer" SortExpression="CustomerId">
            <EditItemTemplate>
              <asp:DropDownList ID="ddlCustomer" runat="server" DataSourceID="odsCompanyNames"
                DataTextField="NameAndDetails" DataValueField="CustomerID" AppendDataBoundItems="true"
                SelectedValue='<%# Bind("CustomerId") %>' Font-Size="Small">
                <asp:ListItem Selected="True" Text="---Please select a Company---" Value="" />
              </asp:DropDownList>
            </EditItemTemplate>
            <InsertItemTemplate>
              <asp:DropDownList ID="ddlCustomer" runat="server" DataSourceID="odsCompanyNames"
                DataTextField="NameAndDetails" DataValueField="CustomerID" AppendDataBoundItems="true"
                SelectedValue='<%# Bind("CustomerId") %>' Font-Size="Small">
                <asp:ListItem Selected="True" Text="---Please select a Company---" Value="" />
              </asp:DropDownList>
            </InsertItemTemplate>
            <ItemTemplate>
                <a href="../classes/TrackerTools.cs">../classes/TrackerTools.cs</a>
              <asp:DropDownList ID="ddlCustomer" runat="server" DataSourceID="odsCompanyNames"
                DataTextField="CompanyName" DataValueField="CustomerID"  Enabled="false"
                SelectedValue='<%# Bind("CustomerId") %>' Font-Size="Small">
              </asp:DropDownList>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Order Date" SortExpression="OrderDate">
            <EditItemTemplate>
              <asp:TextBox ID="tbxDetailOrderDate" runat="server"  Text='<%# Bind("OrderDate", "{0:d}") %>' />
              <ajaxToolkit:CalendarExtender ID="tbxOrderDate_CalendarExtender" runat="server" Enabled="True"
                TargetControlID="tbxDetailOrderDate">
              </ajaxToolkit:CalendarExtender>
            </EditItemTemplate>
            <InsertItemTemplate>
              <asp:TextBox ID="tbxDetailOrderDate" runat="server" Text='<%# Bind("OrderDate", "{0:d}") %>' />
              <ajaxToolkit:CalendarExtender ID="tbxOrderDate_CalendarExtender" runat="server" Enabled="True"
                TargetControlID="tbxDetailOrderDate">
              </ajaxToolkit:CalendarExtender>
            </InsertItemTemplate>
            <ItemTemplate>
              <asp:Label ID="lblOrderDate" runat="server" Text='<%# Bind("OrderDate", "{0:d}") %>' />
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Roast Date" SortExpression="RoastDate">
            <EditItemTemplate>
              <asp:TextBox ID="tbxDetailRoastDate" runat="server" 
                Text='<%# Bind("RoastDate", "{0:d}") %>' Font-Size="Small" />
              <ajaxToolkit:CalendarExtender ID="tbxRoastDate_CalendarExtender" runat="server" Enabled="True"
                TargetControlID="tbxDetailRoastDate">
              </ajaxToolkit:CalendarExtender>
            </EditItemTemplate>
            <InsertItemTemplate>
              <asp:TextBox ID="tbxDetailRoastDate" runat="server" Text='<%# Bind("RoastDate", "{0:d}") %>'></asp:TextBox>
              <ajaxToolkit:CalendarExtender ID="tbxRoastDate_CalendarExtender" runat="server" Enabled="True"
                TargetControlID="tbxDetailRoastDate">
              </ajaxToolkit:CalendarExtender>
            </InsertItemTemplate>
            <ItemTemplate>
              <asp:Label ID="lblRoastDate" runat="server" Text='<%# Bind("RoastDate", "{0:d}") %>'></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Item Type" SortExpression="ItemTypeID">
            <EditItemTemplate>
              <asp:DropDownList ID="ddlItemDesc" runat="server" DataSourceID="odsItemTypes" DataTextField="ItemDesc"
                DataValueField="ItemTypeID" SelectedValue='<%# Bind("ItemTypeID") %>' AppendDataBoundItems="true">
                <asp:ListItem Selected="True" Text="--Select Item--" Value="" />
              </asp:DropDownList>
            </EditItemTemplate>
            <InsertItemTemplate>
              <asp:DropDownList ID="ddlItemDesc" runat="server" DataSourceID="odsItemTypes" DataTextField="ItemDesc"
                DataValueField="ItemTypeID" SelectedValue='<%# Bind("ItemTypeID") %>' AppendDataBoundItems="true">
                <asp:ListItem Selected="True" Text="--Select Item--" Value="" />
              </asp:DropDownList>
            </InsertItemTemplate>
            <ItemTemplate>
              <asp:DropDownList ID="ddlItemDesc" runat="server" DataSourceID="odsItemTypes" DataTextField="ItemDesc"
                DataValueField="ItemTypeID" SelectedValue='<%# Bind("ItemTypeID") %>' Enabled="false" />
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Quantity" SortExpression="QuantityOrdered">
            <EditItemTemplate>
              <asp:TextBox ID="tbxQuantity" runat="server" Text='<%# Bind("QuantityOrdered", "{0:D}") %>' />
            </EditItemTemplate>
            <InsertItemTemplate>
              <asp:TextBox ID="tbxQuantity" runat="server" Text='<%# Bind("QuantityOrdered", "{0:D}") %>' />
            </InsertItemTemplate>
            <ItemTemplate>
              <asp:Label ID="lblQuantity" runat="server" Text='<%# Bind("QuantityOrdered", "{0:D}") %>' />
            </ItemTemplate>
            <ItemStyle HorizontalAlign="Left" />
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Required By" SortExpression="RequiredByDate">
            <EditItemTemplate>
              <asp:TextBox ID="tbxRequiredByDate" runat="server" 
                Text='<%# Bind("RequiredByDate", "{0:d}") %>' Font-Size="Small" Width="5.25em" />
              <ajaxToolkit:CalendarExtender ID="tbxRequiredByDate_CalendarExtender" runat="server" Enabled="True"
                TargetControlID="tbxRequiredByDate">
              </ajaxToolkit:CalendarExtender>
            </EditItemTemplate>
            <InsertItemTemplate>
              <asp:TextBox ID="tbxRequiredByDate" runat="server" Text='<%# Bind("RequiredByDate", "{0:d}") %>' />
              <ajaxToolkit:CalendarExtender ID="tbxRequiredByDate_CalendarExtender" runat="server" Enabled="True"
                TargetControlID="tbxRequiredByDate">
              </ajaxToolkit:CalendarExtender>
            </InsertItemTemplate>
            <ItemTemplate>
              <asp:TextBox ID="tbxRequiredByDate" runat="server" Enabled="false" Text='<%# Bind("RequiredByDate", "{0:d}") %>' />
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Delivery By" SortExpression="ToBeDeliveredBy">
            <EditItemTemplate>
              <asp:DropDownList ID="ddlDeliveryBy" runat="server" DataSourceID="odsDeliveryBy"
                DataTextField="Abreviation" DataValueField="PersonID" 
                SelectedValue='<%# Bind("ToBeDeliveredBy") %>' Font-Size="Small" Width="6em">
              </asp:DropDownList>
            </EditItemTemplate>
            <InsertItemTemplate>
              <asp:DropDownList ID="ddlDeliveryBy" runat="server" DataSourceID="odsDeliveryBy"
                DataTextField="Abreviation" DataValueField="PersonID" SelectedValue='<%# Bind("ToBeDeliveredBy") %>' />
            </InsertItemTemplate>
            <ItemTemplate>
              <asp:DropDownList ID="ddlDeliveryBy" runat="server" DataSourceID="odsDeliveryBy" Enabled="false" 
                DataTextField="Abreviation" DataValueField="PersonID" SelectedValue='<%# Bind("ToBeDeliveredBy") %>' />
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Confirmed" SortExpression="Confirmed">
            <EditItemTemplate>
              <asp:CheckBox ID="cbxConfirmed" runat="server" Checked='<%# Bind("Confirmed") %>' />
            </EditItemTemplate>
            <InsertItemTemplate>
              <asp:CheckBox ID="cbxConfirmed" runat="server" Checked='<%# Bind("Confirmed") %>' />
            </InsertItemTemplate>
            <ItemTemplate>
              <asp:CheckBox ID="cbxConfirmed" runat="server" Checked='<%# Bind("Confirmed") %>' Enabled="false" />
            </ItemTemplate>
          </asp:TemplateField>
          <asp:CheckBoxField DataField="Done" HeaderText="Done" SortExpression="Done" />
          <asp:BoundField DataField="Notes" HeaderText="Notes" SortExpression="Notes" />
          <asp:CommandField />
        </Fields>
        <FooterStyle BackColor="Tan" />
        <HeaderStyle BackColor="Tan" Font-Bold="True" />
        <PagerSettings FirstPageImageUrl="~/images/imgButtons/FirstPage.gif" LastPageImageUrl="~/images/imgButtons/LastPage.gif"
          Mode="NextPreviousFirstLast" NextPageImageUrl="~/images/imgButtons/NextPage.gif"
          PreviousPageImageUrl="~/images/imgButtons/PrevPage.gif" />
        <PagerSettings FirstPageImageUrl="~/images/imgButtons/FirstPage.gif" LastPageImageUrl="~/images/imgButtons/LastPage.gif"
          Mode="NextPreviousFirstLast" NextPageImageUrl="~/images/imgButtons/NextPage.gif"
          PreviousPageImageUrl="~/images/imgButtons/PrevPage.gif" />
        <PagerStyle BackColor="PaleGoldenrod" ForeColor="DarkSlateBlue" 
          HorizontalAlign="Center" />
      </asp:DetailsView>
      <asp:ObjectDataSource ID="odsOrderDetail" runat="server" OldValuesParameterFormatString="original_{0}" 
        InsertMethod="InsertOrder" SelectMethod="GetData" 
        TypeName="TrackerDotNet.DataSets.OrdersDataSetTableAdapters.OrdersTableAdapter"
        UpdateMethod="UpdateByOrderID">
        <InsertParameters>
          <asp:Parameter Name="CustomerId" Type="Int32" />
          <asp:Parameter Name="OrderDate" Type="DateTime" />
          <asp:Parameter Name="RoastDate" Type="DateTime" />
          <asp:Parameter Name="ItemTypeID" Type="Int32" />
          <asp:Parameter Name="QuantityOrdered" Type="Decimal" />
          <asp:Parameter Name="RequiredByDate" Type="DateTime" />
          <asp:Parameter Name="ToBeDeliveredBy" Type="Int32" />
          <asp:Parameter Name="Confirmed" Type="Boolean" />
          <asp:Parameter Name="Done" Type="Boolean" />
          <asp:Parameter Name="Notes" Type="String" />
        </InsertParameters>
        <UpdateParameters>
          <asp:Parameter Name="CustomerId" Type="Int32" />
          <asp:Parameter Name="OrderDate" Type="DateTime" />
          <asp:Parameter Name="RoastDate" Type="DateTime" />
          <asp:Parameter Name="ItemTypeID" Type="Int32" />
          <asp:Parameter Name="QuantityOrdered" Type="Decimal" />
          <asp:Parameter Name="RequiredByDate" Type="DateTime" />
          <asp:Parameter Name="ToBeDeliveredBy" Type="Int32" />
          <asp:Parameter Name="Confirmed" Type="Boolean" />
          <asp:Parameter Name="Done" Type="Boolean" />
          <asp:Parameter Name="Notes" Type="String" />
          <asp:Parameter Name="Original_OrderID" Type="Int32" />
        </UpdateParameters>
      </asp:ObjectDataSource>
      &nbsp;
      <br />
      <asp:Literal ID="ltrlStatus" Text="" runat="server" />
      <br />
    </ContentTemplate>
    <Triggers>
      <asp:AsyncPostBackTrigger ControlID="dvOrderEdit" EventName="DataBound" />
      <asp:AsyncPostBackTrigger ControlID="dvOrderEdit" EventName="DataBinding" />
    </Triggers>
  </asp:UpdatePanel>
  <asp:ObjectDataSource ID="odsCompanyNames" runat="server" OldValuesParameterFormatString="original_{0}"
    SelectMethod="GetCustomerName" TypeName="TrackerDotNet.DataSets.LookUpDatSetsTableAdapters.CustomersLkupTableAdapter">
  </asp:ObjectDataSource>
  <asp:ObjectDataSource ID="odsItemTypes" runat="server" OldValuesParameterFormatString="original_{0}"
    SelectMethod="GetItems" TypeName="TrackerDotNet.DataSets.LookUpDatSetsTableAdapters.ItemTypeLkupTableAdapter">
  </asp:ObjectDataSource>
  <asp:ObjectDataSource ID="odsDeliveryBy" runat="server" OldValuesParameterFormatString="original_{0}"
    SelectMethod="GetPeople" TypeName="TrackerDotNet.DataSets.LookUpDatSetsTableAdapters.PersonsLkupTableAdapter">
  </asp:ObjectDataSource>
</asp:Content>
