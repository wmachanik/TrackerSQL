<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="OrderEntry.aspx.cs"
  MaintainScrollPositionOnPostback="true" Inherits="TrackerDotNet.Pages.OrderEntry" %>
<asp:Content ID="cntOrderEntryHder" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntOrderEntryBdy" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Order Detail</h2>
   <div class="simpleLightBrownForm">
     <table class="TblWhite">
       <thead>
         <tr>
           <td>Done/Undone</td>
           <td>Search for:</td>
           <td>Value</td>
          </tr>
        </thead>
        <tbody>
         <tr>
            <td><asp:CheckBox ID="chkbxOrderDone" Text="Order Done" runat="server" Checked="false" TextAlign="Right" AutoPostBack="true"  /></td>
            <td>
              <asp:DropDownList ID="ddlSearchFor" runat="server">
                <asp:ListItem Selected="True" Value="none" Text="--Select item--" />
                <asp:ListItem Value="Company" Text="Company Name" />
                <asp:ListItem Value="PrepDate" Text="Preperation Date" />
              </asp:DropDownList>
            </td>
            <td>
              <asp:TextBox ID="tbxSearchFor" runat="server" Width="35em" ontextchanged="tbxSearchFor_TextChanged" />&nbsp;&nbsp;
                <asp:Button ID="btnGo" Text="Go" runat="server" onclick="btnGo_Click" />&nbsp;&nbsp;&nbsp;&nbsp;
                <asp:Button ID="btnReset" Text="Reset" runat="server" OnClick="btnReset_Click" />
            </td>
         </tr>
        </tbody>
      </table>
   </div>

  <asp:ObjectDataSource ID="odsDistinctOrders" runat="server" TypeName="TrackerDotNet.Controls.OrderData" 
    SelectMethod="GetDistinctOrders" OldValuesParameterFormatString="original_{0}" DataObjectTypeName="TrackerDotNet.Controls.OrderData" UpdateMethod="UpdateOrderData" >
    <SelectParameters>
      <asp:ControlParameter ControlID="chkbxOrderDone" Name="pOrderDone" 
        PropertyName="Checked" Type="Boolean" 
        DefaultValue="false" />
      <asp:ControlParameter ControlID="ddlSearchFor" Name="pSearchFor" PropertyName="SelectedValue" DefaultValue="" Type="String" />
      <asp:ControlParameter ControlID="tbxSearchFor" Name="pSearchValue" PropertyName="Text" Type="String" />
    </SelectParameters>
  </asp:ObjectDataSource>

  
  <p>&nbsp;</p>
  <asp:GridView ID="gvListOfOrders" runat="server" CssClass="TblCoffee" 
    AllowPaging="True" AllowSorting="True" DataSourceID="odsDistinctOrders" 
    onselectedindexchanged="gvCurrent_SelectedIndexChanged"  Font-Size="Smaller"
    AutoGenerateColumns="False" PageSize="15">
    <AlternatingRowStyle BackColor="#EFFFEF" />
    <Columns>
        <asp:CommandField ButtonType="Image" ShowEditButton="True" EditImageUrl="~/images/imgButtons/EditItem.gif"
            UpdateImageUrl="~/images/imgButtons/UpdateItem.gif" CancelImageUrl="~/images/imgButtons/CancelItem.gif" />
        <asp:BoundField DataField="OrderID" HeaderText="OrderID" SortExpression="OrderID" ReadOnly="true" />
        <asp:TemplateField HeaderText="Company Name" SortExpression="CompanyName">
            <EditItemTemplate>
              <asp:DropDownList ID="ddlCompany" runat="server" DataSourceID="odsCompanys"  Font-Size="X-Small"
                DataTextField="CompanyName" DataValueField="CustomerID" AppendDataBoundItems="True" SelectedValue='<%# Bind("CustomerID") %>'  >
                  <asp:ListItem Value="0">none</asp:ListItem>
                </asp:DropDownList>
            </EditItemTemplate>
            <ItemTemplate>
                <asp:Label ID="lblCompany" runat="server" Text='<%# Bind("CompanyName") %>'></asp:Label>
            </ItemTemplate>
        </asp:TemplateField>
        <asp:BoundField DataField="CustomerID" HeaderText="CustomerID" SortExpression="CustomerID" Visible="False" />
        <asp:BoundField DataField="OrderDate" HeaderText="Order Date" SortExpression="OrderDate" DataFormatString="{0:d}" ApplyFormatInEditMode="true"  ControlStyle-Width="6em" >
        <ItemStyle Width="7em" />
        </asp:BoundField>
        <asp:BoundField DataField="RoastDate" HeaderText="Roast Date" SortExpression="RoastDate" DataFormatString="{0:d}" ApplyFormatInEditMode="true"  ControlStyle-Width="6em" >
        <ItemStyle Width="7em" />
        </asp:BoundField>
        <asp:TemplateField HeaderText="Person" SortExpression="Person">
            <EditItemTemplate>
              <asp:DropDownList ID="ddlPersons" runat="server" AppendDataBoundItems="True" 
               DataSourceID="odsPersons" DataTextField="Abreviation" DataValueField="PersonID"
               SelectedValue='<%# Bind("ToBeDeliveredBy") %>' Font-Size="X-Small" >
              <asp:ListItem Text="none" Value="0" />
              </asp:DropDownList>
            </EditItemTemplate>
            <ItemTemplate>
                <asp:Label ID="lblPerson" runat="server" Text='<%# Bind("Person") %>' />
            </ItemTemplate>
        </asp:TemplateField>
        <asp:BoundField DataField="ToBeDeliveredBy" HeaderText="Delivery By" SortExpression="ToBeDeliveredBy" Visible="False" />
        <asp:BoundField DataField="RequiredByDate" HeaderText="Required By" SortExpression="RequiredByDate" DataFormatString="{0:d}" ApplyFormatInEditMode="true"  ControlStyle-Width="6em" >
        <ItemStyle Width="7em" />
        </asp:BoundField>
        <asp:TemplateField HeaderText="ItemType ID" SortExpression="ItemTypeID">
            <EditItemTemplate>
                <asp:DropDownList ID="ddlImte" runat="server" AppendDataBoundItems="True" Font-Size="X-Small"
                  DataSourceID="odsItems" DataTextField="ItemDesc" DataValueField="ItemTypeID" SelectedValue='<%# Bind("ItemTypeID") %>' >
                  <asp:ListItem Text="none" Value="0" />
            </asp:DropDownList>
            </EditItemTemplate>
            <ItemTemplate>
               <asp:Label ID="ItemDescLabel" runat="server" Text='<%# GetItemDesc((int)Eval("ItemTypeID")) %>' />
            </ItemTemplate>
        </asp:TemplateField>
        <asp:BoundField DataField="QuantityOrdered" HeaderText="Qty" SortExpression="QuantityOrdered" />
        <asp:CheckBoxField DataField="Confirmed" HeaderText="Cnfrmd" SortExpression="Confirmed" />
        <asp:CheckBoxField DataField="Done" HeaderText="Done" SortExpression="Done" />
        <asp:BoundField DataField="Notes" HeaderText="Notes" SortExpression="Notes" />
    </Columns>
    <PagerSettings Mode="NumericFirstLast" Position="Bottom" />
    <SelectedRowStyle BackColor="#66FF66" />
  </asp:GridView>
<%--  <asp:SqlDataSource ID="sdsOrdersDetail" runat="server" 
    ConnectionString="<%$ ConnectionStrings:Tracker08ConnectionString %>" 
    ProviderName="<%$ ConnectionStrings:Tracker08ConnectionString.ProviderName %>" 
   
    SelectCommand="SELECT DISTINCT CustomersTbl.CompanyName, OrdersTbl.CustomerID, OrdersTbl.OrderDate, OrdersTbl.RoastDate, OrdersTbl.RequiredByDate, PersonsTbl.Person, OrdersTbl.Confirmed, OrdersTbl.Done FROM ((OrdersTbl LEFT OUTER JOIN PersonsTbl ON OrdersTbl.ToBeDeliveredBy = PersonsTbl.PersonID) LEFT OUTER JOIN CustomersTbl ON OrdersTbl.CustomerID = CustomersTbl.CustomerID) WHERE (OrdersTbl.Done = ?)">
    <SelectParameters>
      <asp:ControlParameter ControlID="chkbxOrderDone" Name="Done" 
        PropertyName="Checked" Type="Boolean" 
        DefaultValue="false" />
    </SelectParameters>
  </asp:SqlDataSource>
--%>
  <asp:GridView ID="gvOrderDetails" runat="server" CssClass="TblZebra" AutoGenerateEditButton="true" 
    OnRowUpdated="gvOrderDetails_OnRowUpdated" OnRowEditing="gvOrderDetails_OnRowEditing">
  </asp:GridView>
  <br />


  <asp:ObjectDataSource ID="odsCompanys" runat="server" TypeName="TrackerDotNet.Controls.CompanyNames"
    SelectMethod="GetAll" OldValuesParameterFormatString="original_{0}">
  </asp:ObjectDataSource>
  <asp:ObjectDataSource ID="odsPersons" runat="server" TypeName="TrackerDotNet.Controls.PersonsTbl"
      SortParameterName="SortBy" SelectMethod="GetAll"
      OldValuesParameterFormatString="original_{0}" DataObjectTypeName="TrackerDotNet.Controls.PersonsTbl" DeleteMethod="DeletePerson" InsertMethod="InsertPerson" UpdateMethod="UpdatePerson">
      <DeleteParameters>
          <asp:Parameter Name="pPersonID" Type="Int32" />
      </DeleteParameters>
      <SelectParameters>
        <asp:Parameter DefaultValue="Abreviation" Name="SortBy" Type="String" />
      </SelectParameters>
   </asp:ObjectDataSource>
  <asp:ObjectDataSource ID="odsItems" runat="server" TypeName="TrackerDotNet.Controls.ItemTypeTbl"
      SortParameterName="SortBy" SelectMethod="GetAll"
      OldValuesParameterFormatString="original_{0}">
      <SelectParameters>
        <asp:Parameter DefaultValue="ItemDesc" Name="SortBy" Type="String" />
      </SelectParameters>
  </asp:ObjectDataSource>


<%--
  <table class="TblLHCol-brown">
    <th>
      <tr class="TblLHColHdr">
        <td>Item</td>
        <td>Value</td>
      </tr>
    </th>
    <tbody>
    <tr>
      <td class="TblLHCol-first">
        Customer
      </td>
      <td>

      </td>
    </tr>
    <tr>
      <td class="TblLHCol-first">
        Order Date:
      </td>
      <td></td>
    </tr>
    <tr>
      <td class="TblLHCol-first">
        Preperation Date</td>
      <td>&nbsp;</td>
    </tr>
    <tr>
      <td class="TblLHCol-first">
        Delivery By</td>
      <td>&nbsp;</td>
    </tr>
    <tr>
      <td class="TblLHCol-first">
        Delivery Date</td>
      <td>&nbsp;</td>
    </tr>
    <tr>
      <td class="TblLHCol-first">
        Notes</td>
      <td>&nbsp;</td>
    </tr>
    </tbody>
  </table>
  <br />
--%>  

</asp:Content>
