<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" MaintainScrollPositionOnPostback="true" 
  CodeBehind="OrdersEdit.aspx.cs" Inherits="TrackerSQL.Pages.OrdersEdit" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="cntOrderEditHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntOrderEditBdy" ContentPlaceHolderID="MainContent" runat="server">
  <asp:ScriptManager ID="smgrOrdersEdit" runat="server">
  </asp:ScriptManager>
  <h1>Orders </h1>
  <p>List, filter, sort, edit and insert an order...</p>
  <asp:UpdatePanel ID="upnlButtonBar" runat="server" >
    <ContentTemplate>
      <table style="border:0; width: 100%">
        <tr>
          <td style="text-align: left" >
            <asp:Button ID="btnNew" runat="server" Text="New Order" OnClick="btnNew_Click" /></td>
          <td style="text-align: left; vertical-align: text-top" >
            Filter by:&nbsp;<asp:DropDownList ID="ddlFilterBy" runat="server" AutoPostBack="true"
              onselectedindexchanged="ddlFilterBy_SelectedIndexChanged">
              <asp:ListItem Selected="True" Value="none" Text="none" />
              <asp:ListItem Value="1" Text="Company name" />
              <asp:ListItem Value="2" Text="Delivery By" />
              <asp:ListItem Value="3" Text="Notes" />
              <asp:ListItem Value="4" Text="Delivery Date" />
              <asp:ListItem Enabled="false" Value="5" Text="Roast Date" />
            </asp:DropDownList>&nbsp;
            <asp:TextBox id= "tbxFilter" runat="server" Width="15em" Enabled="true" />
            <asp:DropDownList ID="ddlDeliveryBy" runat="server" AutoPostBack="True" Visible="false"
              DataSourceID="odsDeliveryBy" DataTextField="Person" DataValueField="PersonID" />
            &nbsp;&nbsp;
            <asp:CheckBox ID="cbxOnlyToDo" runat="server" OnCheckedChanged="cbxOnlyToDo_CheckedChanged" Text="Only To Do" />
            &nbsp;&nbsp;
            <asp:Button ID="btnGo" Text="Go" runat="server" onclick="btnGo_Click"  />
          </td>
          <td>
          </td>
          <td style="text-align: right">
            Order per page:</td>
          <td style="text-align: right">
            <asp:DropDownList ID="ddlOrdersPerPage" runat="server"  AutoPostBack="true"
              OnSelectedIndexChanged="ddlOrdersPerPage_SelectedIndexChanged">
              <asp:ListItem>5</asp:ListItem>
              <asp:ListItem>10</asp:ListItem>
              <asp:ListItem Selected="True">20</asp:ListItem>
              <asp:ListItem>30</asp:ListItem>
              <asp:ListItem>50</asp:ListItem>
              <asp:ListItem>75</asp:ListItem>
              <asp:ListItem>100</asp:ListItem>
              <asp:ListItem>250</asp:ListItem>
              <asp:ListItem>500</asp:ListItem>
              <asp:ListItem>750</asp:ListItem>
              <asp:ListItem>1000</asp:ListItem>
            </asp:DropDownList>
          </td>
        </tr>
      </table>
    </ContentTemplate>
    <Triggers>
      <asp:AsyncPostBackTrigger ControlID="ddlFilterBy" 
        EventName="SelectedIndexChanged" />
    </Triggers>
  </asp:UpdatePanel>
  <h2>Order List</h2>
  <asp:UpdateProgress ID="uprgOrderEditDetail" runat="server" AssociatedUpdatePanelID="upnlOrderEditDetail" DisplayAfter="200" >
    <ProgressTemplate>
      <img src="../images/animi/BlueArrowsUpdate.gif" alt="updating" width="16" height="16" />&nbsp;Please wait..</ProgressTemplate>
  </asp:UpdateProgress>
  <asp:UpdatePanel ID="upnlOrderEditDetail" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
      <asp:Literal ID="ltrlStatus" Text="" runat="server" />
      <asp:DetailsView ID="dvOrderEdit" runat="server" AllowPaging="True" AutoGenerateRows="False"
        BackColor="LightGoldenrodYellow" BorderColor="Tan"  OnModeChanged="dvOrderEdit_OnModeChanged" 
        OnItemDeleting="BindOrdersGV" OnItemInserted="dvOrderEdit_OnItemInserted" OnItemUpdated="BindOrdersGV"
        BorderWidth="1px" CellPadding="2" OnDataBound="dvOrderEdit_OnDataBound"  
        DataKeyNames="OrderID" DataSourceID="odsOrderDetail" 
        EmptyDataText="Please select an order, or add a New order" 
        EnablePagingCallbacks="True" ForeColor="Black" >
        <AlternatingRowStyle BackColor="PaleGoldenrod" />
        <CommandRowStyle HorizontalAlign="Center" VerticalAlign="Middle"  />
        <EditRowStyle BackColor="Honeydew" ForeColor="DarkOliveGreen" />
        <Fields>
          <asp:CommandField ButtonType="Button" ShowEditButton="True" 
            ShowInsertButton="True"  />
          <asp:BoundField DataField="OrderID" HeaderText="OrderID" InsertVisible="False" ReadOnly="True"
            SortExpression="OrderID" Visible="False" />
          <asp:TemplateField HeaderText="Customer" SortExpression="CustomerID">
            <EditItemTemplate>
              <asp:DropDownList ID="ddlCustomer" runat="server" DataSourceID="odsCompanyNames"
                DataTextField="NameAndDetails" DataValueField="CustomerID" AppendDataBoundItems="true"
                SelectedValue='<%# Bind("CustomerID") %>' Font-Size="Small" >
                <asp:ListItem Selected="True" Text="---Please select a Company---" Value="" />
              </asp:DropDownList>
            </EditItemTemplate>
            <InsertItemTemplate>
              <asp:DropDownList ID="ddlCustomer" runat="server" DataSourceID="odsCompanyNames"
                DataTextField="NameAndDetails" DataValueField="CustomerID" AppendDataBoundItems="true"
                SelectedValue='<%# Bind("CustomerID") %>' Font-Size="Small">
                <asp:ListItem Selected="True" Text="---Please select a Company---" Value="" />
              </asp:DropDownList>
            </InsertItemTemplate>
            <ItemTemplate>
              <asp:DropDownList ID="ddlCustomer" runat="server" DataSourceID="odsCompanyNames"
                DataTextField="CompanyName" DataValueField="CustomerID"  Enabled="false"
                SelectedValue='<%# Bind("CustomerID") %>' Font-Size="Small">
              </asp:DropDownList>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Order Date" SortExpression="OrderDate">
            <EditItemTemplate>
              <asp:TextBox ID="tbxDetailOrderDate" runat="server"  Text='<%# Bind("OrderDate", "{0:d}") %>' />
              <ajaxToolkit:CalendarExtender ID="tbxOrderDate_CalendarExtender" runat="server" Enabled="True"
                TargetControlID="tbxDetailOrderDate">
              </cc1:CalendarExtender>
            </EditItemTemplate>
            <InsertItemTemplate>
              <asp:TextBox ID="tbxDetailOrderDate" runat="server" Text='<%# Bind("OrderDate", "{0:d}") %>' />
              <ajaxToolkit:CalendarExtender ID="tbxOrderDate_CalendarExtender" runat="server" Enabled="True"
                TargetControlID="tbxDetailOrderDate">
              </cc1:CalendarExtender>
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
              </cc1:CalendarExtender>
            </EditItemTemplate>
            <InsertItemTemplate>
              <asp:TextBox ID="tbxDetailRoastDate" runat="server" Text='<%# Bind("RoastDate", "{0:d}") %>'></asp:TextBox>
              <ajaxToolkit:CalendarExtender ID="tbxRoastDate_CalendarExtender" runat="server" Enabled="True"
                TargetControlID="tbxDetailRoastDate">
              </cc1:CalendarExtender>
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
              <asp:TextBox ID="tbxQuantity" runat="server" Text='<%# Bind("QuantityOrdered") %>' />
            </EditItemTemplate>
            <InsertItemTemplate>
              <asp:TextBox ID="tbxQuantity" runat="server" Text='<%# Bind("QuantityOrdered") %>' />
            </InsertItemTemplate>
            <ItemTemplate>
              <asp:Label ID="lblQuantity" runat="server" Text='<%# Bind("QuantityOrdered") %>' />
            </ItemTemplate>
            <ItemStyle HorizontalAlign="Left" />
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Required By" SortExpression="RequiredByDate">
            <EditItemTemplate>
              <asp:TextBox ID="tbxRequiredByDate" runat="server" 
                Text='<%# Bind("RequiredByDate", "{0:d}") %>' Font-Size="Small" Width="5.25em" />
              <ajaxToolkit:CalendarExtender ID="tbxRequiredByDate_CalendarExtender" runat="server" Enabled="True"
                TargetControlID="tbxRequiredByDate">
              </cc1:CalendarExtender>
            </EditItemTemplate>
            <InsertItemTemplate>
              <asp:TextBox ID="tbxRequiredByDate" runat="server" Text='<%# Bind("RequiredByDate", "{0:d}") %>' />
              <ajaxToolkit:CalendarExtender ID="tbxRequiredByDate_CalendarExtender" runat="server" Enabled="True"
                TargetControlID="tbxRequiredByDate">
              </cc1:CalendarExtender>
            </InsertItemTemplate>
            <ItemTemplate>
              <asp:TextBox ID="tbxRequiredByDate" runat="server" Enabled="false" Text='<%# Bind("RequiredByDate", "{0:d}") %>' />
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Delivery By" SortExpression="ToBeDeliveredBy">
            <EditItemTemplate>
              <asp:DropDownList ID="ddlDeliveryBy" runat="server" DataSourceID="odsDeliveryBy"
                DataTextField="Abbreviation" DataValueField="PersonID" 
                SelectedValue='<%# Bind("ToBeDeliveredBy") %>' Font-Size="Small" Width="6em">
              </asp:DropDownList>
            </EditItemTemplate>
            <InsertItemTemplate>
              <asp:DropDownList ID="ddlDeliveryBy" runat="server" DataSourceID="odsDeliveryBy"
                DataTextField="Abbreviation" DataValueField="PersonID" SelectedValue='<%# Bind("ToBeDeliveredBy") %>' />
            </InsertItemTemplate>
            <ItemTemplate>
              <asp:DropDownList ID="ddlDeliveryBy" runat="server" DataSourceID="odsDeliveryBy" Enabled="false" 
                DataTextField="Abbreviation" DataValueField="PersonID" SelectedValue='<%# Bind("ToBeDeliveredBy") %>' />
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
          <asp:CheckBoxField DataField="Done" HeaderText="Done" SortExpression="Done" 
            ReadOnly="True" />
          <asp:TemplateField HeaderText="Notes" SortExpression="Notes">
            <EditItemTemplate>
              <asp:TextBox ID="tbxNotes" runat="server" Rows="3" Text='<%# Bind("Notes") %>' TextMode="MultiLine" Width="25em" />
            </EditItemTemplate>
            <InsertItemTemplate>
              <asp:TextBox ID="tbxNotes" runat="server" Rows="3" Text='<%# Bind("Notes") %>' TextMode="MultiLine" Width="25em" />
            </InsertItemTemplate>
            <ItemTemplate>
              <asp:Label ID="lblNotes" runat="server" Text='<%# Bind("Notes") %>' />
            </ItemTemplate>
          </asp:TemplateField>
          <asp:CommandField />
        </Fields>
        <FooterStyle BackColor="Tan" />
        <HeaderStyle BackColor="Tan" Font-Bold="True" />
        <PagerSettings PagerStyle-CssClass="aspNetPager" FirstPageImageUrl="~/images/imgButtons/FirstPage.gif" 
          LastPageImageUrl="~/images/imgButtons/LastPage.gif" NextPageImageUrl="~/images/imgButtons/NextPage.gif"
          PreviousPageImageUrl="~/images/imgButtons/PrevPage.gif" />
        <PagerStyle BackColor="PaleGoldenrod" ForeColor="DarkSlateBlue" 
          HorizontalAlign="Center" />
      </asp:DetailsView>
      <asp:ObjectDataSource ID="odsOrderDetail" runat="server" OldValuesParameterFormatString="original_{0}" 
        InsertMethod="InsertOrder" SelectMethod="GetDataByOrderID" 
        TypeName="TrackerSQL.DataSets.OrdersDataSetTableAdapters.OrdersTableAdapter"
        UpdateMethod="UpdateByOrderID">
        <InsertParameters>
          <asp:Parameter Name="CustomerID" Type="Int32" />
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
        <SelectParameters>
          <asp:ControlParameter ControlID="gvOrdersEditList" DefaultValue="1" 
            Name="OrderID" PropertyName="SelectedValue" Type="Int32" />
        </SelectParameters>
        <UpdateParameters>
          <asp:Parameter Name="CustomerID" Type="Int32" />
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
      <br />
    </ContentTemplate>
    <Triggers>
      <asp:AsyncPostBackTrigger ControlID="dvOrderEdit" EventName="DataBound" />
      <asp:AsyncPostBackTrigger ControlID="dvOrderEdit" EventName="DataBinding" />
      <asp:AsyncPostBackTrigger ControlID="gvOrdersEditList" EventName="SelectedIndexChanged" />
      <asp:AsyncPostBackTrigger ControlID="gvOrdersEditList" EventName="RowUpdated" />
    </Triggers>
  </asp:UpdatePanel>
  <asp:ObjectDataSource ID="odsCompanyNames" runat="server" OldValuesParameterFormatString="original_{0}"
    SelectMethod="GetCustomerName" TypeName="TrackerSQL.DataSets.LookUpDatSetsTableAdapters.CustomersLkupTableAdapter" >
  </asp:ObjectDataSource>
  <asp:ObjectDataSource ID="odsItemTypes" runat="server" OldValuesParameterFormatString="original_{0}"
    SelectMethod="GetItems" TypeName="TrackerSQL.DataSets.LookUpDatSetsTableAdapters.ItemTypeLkupTableAdapter">
  </asp:ObjectDataSource>
  <asp:ObjectDataSource ID="odsDeliveryBy" runat="server" OldValuesParameterFormatString="original_{0}"
    SelectMethod="GetPeople" TypeName="TrackerSQL.DataSets.LookUpDatSetsTableAdapters.PersonsLkupTableAdapter">
  </asp:ObjectDataSource>
  <asp:UpdateProgress ID="uprgOrdersEdit" runat="server" AssociatedUpdatePanelID="upnlOrdersEditList" DisplayAfter="200" >
    <ProgressTemplate>
      <img src="../images/animi/BlueArrowsUpdate.gif" alt="updating" width="16" height="16" />&nbsp;Please wait..</ProgressTemplate>
  </asp:UpdateProgress>
  <asp:UpdatePanel ID="upnlOrdersEditList" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
      <asp:GridView ID="gvOrdersEditList" runat="server" AllowPaging="True" Font-Size="Smaller"
        AllowSorting="True" AutoGenerateColumns="False" BackColor="White" BorderColor="#DEDFDE" PageSize="20"
        BorderStyle="None" BorderWidth="1px" CellPadding="4" DataKeyNames="OrderID" DataSourceID="odsOrdersTbl"
        ForeColor="Black">
        <AlternatingRowStyle BackColor="White" />
        <Columns>
          <asp:CommandField ShowEditButton="True" ShowSelectButton="True" CancelImageUrl="~/images/imgButtons/CancelItem.gif"
            DeleteImageUrl="~/images/imgButtons/DelItem.gif" EditImageUrl="~/images/imgButtons/EditItem.gif"
            InsertImageUrl="~/images/imgButtons/AddItem.gif" NewImageUrl="~/images/imgButtons/GreenPlus.gif"
            SelectImageUrl="~/images/imgButtons/SelectItem.gif" UpdateImageUrl="~/images/imgButtons/UpdateItem.gif"
            ButtonType="Image" />
          <asp:BoundField DataField="OrderID" HeaderText="OrderID" InsertVisible="False" ReadOnly="True"
            SortExpression="OrderID" Visible="False" />
          <asp:TemplateField HeaderText="Customer" SortExpression="Customer">
            <EditItemTemplate>
              <asp:DropDownList ID="ddlCustomer" runat="server" DataSourceID="odsCompanyNames"
                DataTextField="NameAndDetails" DataValueField="CustomerID" Width="20em" 
                SelectedValue='<%# Bind("CustomerID") %>' Font-Size="Small" AppendDataBoundItems="true" >
                <asp:ListItem Selected="True" Text="---Please select a Company---" Value="" />
              </asp:DropDownList>
            </EditItemTemplate>
            <InsertItemTemplate>
              <asp:DropDownList ID="ddlCustomer" runat="server" DataSourceID="odsCompanyNames"
                DataTextField="NameAndDetails" DataValueField="CustomerID" AppendDataBoundItems="true" >
                <asp:ListItem Selected="True" Text="---Please select a Company---" Value="" />
              </asp:DropDownList>
            </InsertItemTemplate>
            <ItemTemplate>
<!--              <asp:Label ID="lblCompany" runat="server" Text='<%# Eval("CompanyName") %>'></asp:Label>   -->
              <asp:HyperLink ID="hlCompany" runat="server" 
                NavigateUrl='<%# Eval("CustomerID", "CustomerDetails.aspx?ID={0}") %>' 
                Text='<%# Eval("CompanyName") %>'></asp:HyperLink>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Order Date" SortExpression="OrderDate">
            <EditItemTemplate>
              <asp:TextBox ID="tbxOrderDate" runat="server" 
                Text='<%# Bind("OrderDate", "{0:d}") %>' Font-Size="Small" Width="5.25em"></asp:TextBox>
              <ajaxToolkit:CalendarExtender ID="tbxOrderDate_CalendarExtender" runat="server" Enabled="True"
                TargetControlID="tbxOrderDate">
              </cc1:CalendarExtender>
            </EditItemTemplate>
            <InsertItemTemplate>
              <asp:TextBox ID="tbxOrderDate" runat="server" Text='<%# Bind("OrderDate", "{0:d}") %>'></asp:TextBox>
              <ajaxToolkit:CalendarExtender ID="tbxOrderDate_CalendarExtender" runat="server" Enabled="True"
                TargetControlID="tbxOrderDate">
              </cc1:CalendarExtender>
            </InsertItemTemplate>
            <ItemTemplate>
              <asp:Label ID="lblOrderDate" runat="server" Text='<%# Bind("OrderDate", "{0:d}") %>'></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Roast Date" SortExpression="RoastDate">
            <EditItemTemplate>
              <asp:TextBox ID="tbxRoastDate" runat="server" 
                Text='<%# Bind("RoastDate", "{0:d}") %>' Font-Size="Small" Width="5.25em"></asp:TextBox>
              <ajaxToolkit:CalendarExtender ID="tbxRoastDate_CalendarExtender" runat="server" Enabled="True"
                TargetControlID="tbxRoastDate">
              </cc1:CalendarExtender>
            </EditItemTemplate>
            <InsertItemTemplate>
              <asp:TextBox ID="tbxRoastDate" runat="server" Text='<%# Bind("RoastDate", "{0:d}") %>'></asp:TextBox>
              <ajaxToolkit:CalendarExtender ID="tbxRoastDate_CalendarExtender" runat="server" Enabled="True"
                TargetControlID="tbxRoastDate">
              </cc1:CalendarExtender>
            </InsertItemTemplate>
            <ItemTemplate>
              <asp:Label ID="lblRoastDate" runat="server" Text='<%# Bind("RoastDate", "{0:d}") %>'></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="ItemTypeID" SortExpression="ItemTypeID">
            <EditItemTemplate>
              <asp:DropDownList ID="ddlItemDesc" runat="server" DataSourceID="odsItemTypes" DataTextField="ItemDesc"
                DataValueField="ItemTypeID" SelectedValue='<%# Bind("ItemTypeID") %>' 
                Font-Size="Small" Width="7em" AppendDataBoundItems="true">
                <asp:ListItem Selected="True" Text="--Select Item--" Value="" />
              </asp:DropDownList>
            </EditItemTemplate>
            <InsertItemTemplate>
              <asp:DropDownList ID="ddlItemDesc" runat="server" DataSourceID="odsItemTypes" DataTextField="ItemDesc"
                DataValueField="ItemTypeID" AppendDataBoundItems="true">
                <asp:ListItem Selected="True" Text="--Select Item--" Value="" />
              </asp:DropDownList>
            </InsertItemTemplate>
            <ItemTemplate>
              <asp:Label ID="lblItemDesc" runat="server" Text='<%# Eval("ItemDesc") %>'></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:BoundField DataField="QuantityOrdered" HeaderText="Qty" 
            SortExpression="QuantityOrdered" ItemStyle-HorizontalAlign="Right" >
          <ItemStyle HorizontalAlign="Right" />
          </asp:BoundField>
          <asp:TemplateField HeaderText="Required By" SortExpression="RequiredByDate">
            <EditItemTemplate>
              <asp:TextBox ID="tbxRequiredByDate" runat="server" 
                Text='<%# Bind("RequiredByDate", "{0:d}") %>' Font-Size="Small" Width="5.25em" />
              <ajaxToolkit:CalendarExtender ID="tbxRequiredByDate_CalendarExtender" runat="server" Enabled="True"
                TargetControlID="tbxRequiredByDate">
              </cc1:CalendarExtender>
            </EditItemTemplate>
            <InsertItemTemplate>
              <asp:TextBox ID="tbxRequiredByDate" runat="server" Text='<%# Bind("RequiredByDate", "{0:d}") %>'></asp:TextBox>
              <ajaxToolkit:CalendarExtender ID="tbxRequiredByDate_CalendarExtender" runat="server" Enabled="True"
                TargetControlID="tbxRequiredByDate">
              </cc1:CalendarExtender>
            </InsertItemTemplate>
            <ItemTemplate>
              <asp:Label ID="lblRequiredByDate" runat="server" Text='<%# Bind("RequiredByDate", "{0:d}") %>'></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Delivery By" SortExpression="ToBeDeliveredBy">
            <EditItemTemplate>
              <asp:DropDownList ID="ddlDeliveryBy" runat="server" DataSourceID="odsDeliveryBy"
                DataTextField="Abbreviation" DataValueField="PersonID" 
                SelectedValue='<%# Bind("ToBeDeliveredBy") %>' Font-Size="Small" Width="4em">
              </asp:DropDownList>
            </EditItemTemplate>
            <InsertItemTemplate>
              <asp:DropDownList ID="ddlDeliveryBy" runat="server" DataSourceID="odsDeliveryBy"
                DataTextField="Abbreviation" DataValueField="PersonID">
              </asp:DropDownList>
            </InsertItemTemplate>
            <ItemTemplate>
              <asp:Label ID="lblItemTemplate" runat="server" Text='<%# Eval("DelivertByInit") %>'></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:CheckBoxField DataField="Confirmed" HeaderText="Confirmed" SortExpression="Confirmed" />
          <asp:CheckBoxField DataField="Done" HeaderText="Done" SortExpression="Done" />
          <asp:BoundField DataField="Notes" HeaderText="Notes" SortExpression="Notes" />
        </Columns>
        <FooterStyle BackColor="#CCCC99" />
        <HeaderStyle BackColor="#6B696B" Font-Bold="True" ForeColor="White" />
        <PagerSettings FirstPageImageUrl="~/images/imgButtons/FirstPage.gif" LastPageImageUrl="~/images/imgButtons/LastPage.gif"
          Mode="NextPreviousFirstLast" NextPageImageUrl="~/images/imgButtons/NextPage.gif"
          PreviousPageImageUrl="~/images/imgButtons/PrevPage.gif" />
        <PagerStyle BackColor="#F7F7DE" ForeColor="Black" HorizontalAlign="Right" />
        <RowStyle BackColor="#F7F7DE" />
        <SelectedRowStyle BackColor="#CE5D5A" Font-Bold="True" ForeColor="White" />
        <SortedAscendingCellStyle BackColor="#FBFBF2" />
        <SortedAscendingHeaderStyle BackColor="#848384" />
        <SortedDescendingCellStyle BackColor="#EAEAD3" />
        <SortedDescendingHeaderStyle BackColor="#575357" />
      </asp:GridView>
      <asp:ObjectDataSource ID="odsOrdersTbl" runat="server"
        OldValuesParameterFormatString="original_{0}" SelectMethod="GetData"    
        TypeName="TrackerSQL.DataSets.OrdersDataSetTableAdapters.OrdersTableAdapter" UpdateMethod="UpdateByOrderID"
        OnSelecting="odsOrdersTbl_Selecting"  MaximumRowsParameterName="\*32">
        <UpdateParameters>
          <asp:Parameter Name="CustomerID" Type="Int32" />
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
    </ContentTemplate>
    <Triggers>
      <asp:AsyncPostBackTrigger ControlID="ddlOrdersPerPage" 
        EventName="SelectedIndexChanged" />
      <asp:AsyncPostBackTrigger ControlID="dvOrderEdit" EventName="ItemDeleted" />
      <asp:AsyncPostBackTrigger ControlID="dvOrderEdit" EventName="ItemCreated" />
      <asp:AsyncPostBackTrigger ControlID="dvOrderEdit" EventName="ItemUpdated" />
      <asp:AsyncPostBackTrigger ControlID="btnGo" EventName="Click" />
      <asp:AsyncPostBackTrigger ControlID="cbxOnlyToDo" 
        EventName="CheckedChanged" />
    </Triggers>
  </asp:UpdatePanel>
  <br />
  <asp:Literal ID="ltrlFilter" Text="" runat="server" />
</asp:Content>