<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ItemGroups.aspx.cs" Inherits="TrackerDotNet.Pages.ItemGroups" %>

<asp:Content ID="cntItemGroupsHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntItemGroupsBdy" ContentPlaceHolderID="MainContent" runat="server">
  <asp:ScriptManager ID="scrmngItemGroups" runat="server">
  </asp:ScriptManager>
  <h2>Item Group Tables...
  </h2>
  <asp:UpdatePanel ID="updtPnlItems" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional" >
    <ContentTemplate>
      <div class="simpleForm" style="vertical-align:bottom">
      Item Group:
      <asp:DropDownList ID="ddlGroupItems" runat="server" DataSourceID="odsItemGroups" DataTextField="ItemDesc"
        DataValueField="ItemTypeID" AppendDataBoundItems="true" AutoPostBack="true"  
        OnSelectedIndexChanged="ddlGroupItems_SelectedIndexChanged"> 
        <asp:ListItem Text="--Please select or add group--" Value="-1" />
      </asp:DropDownList>
      &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
      <asp:ImageButton id="imgbtnAddGroup" runat="server" ImageUrl="~/images/imgButtons/AddButtonCaps.gif" OnClick="btnAddGroup_Click" ToolTip="Add a item type of group" />
      &nbsp;&nbsp;&nbsp;&nbsp;
      <asp:ImageButton id="imgbtnEditGroup" runat="server" ImageUrl="~/images/imgButtons/EditButton.gif" OnClick="btnEditGroup_Click" ToolTip="Add a item type of group" />
      </div>
    </ContentTemplate>
  </asp:UpdatePanel>
  <br />
  <asp:UpdateProgress ID="uprgItemGroups" runat="server" AssociatedUpdatePanelID="updtpnlItems" >
    <ProgressTemplate>
      Please Wait&nbsp;<img src="../images/animi/QuaffeeProgress.gif" alt="Please Wait..." />&nbsp;...
    </ProgressTemplate>
  </asp:UpdateProgress>
  <asp:UpdatePanel ID="updtPnlItemsInList" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
    <ContentTemplate>
      <table class="TblSimple">
        <thead>
          <tr>
            <th>Items in group</th>
            <th>&nbsp;</th>
            <th>Items to add</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td style="vertical-align: top">
              <asp:GridView ID="gvItemsInList" runat="server" DataSourceID="odsItemInGroup" AutoGenerateColumns="False"
                AllowSorting="true" AllowPaging="true" OnRowCommand="gvItemsInList_RowCommand" >
                <EmptyDataTemplate>
                  <%# GiveInStatus() %>
                </EmptyDataTemplate>
                <Columns>
                  <asp:TemplateField HeaderText="?" HeaderImageUrl="~/images/imgButtons/Trashcan.gif">
                    <ItemTemplate>
                      <asp:CheckBox ID="cbxRemoveItem" runat="server" />
                    </ItemTemplate>
                  </asp:TemplateField>
                  <asp:TemplateField HeaderText="ItemTypeID">
                    <ItemTemplate>
                      <asp:DropDownList ID="ddlItemDesc" runat="server" DataSourceID="odsItemTypes"
                        DataTextField="ItemDesc" DataValueField="ItemTypeID" Enabled="False"
                        AppendDataBoundItems="true"
                        SelectedValue='<%# Bind("ItemTypeID") == null ? "0" : Bind("ItemTypeID") %>'>
                        <asp:ListItem Value="0" Text="please select an item" />
                      </asp:DropDownList>
                    </ItemTemplate>
                  </asp:TemplateField>
                  <asp:TemplateField HeaderText="Pos" SortExpression="ItemTypeSortPos" ItemStyle-HorizontalAlign="Center">
                    <ItemTemplate>
                      &nbsp;
                      <asp:Label ID="lblItemSortPos" runat="server" Text='<%# Bind("ItemTypeSortPos") %>' />
                    </ItemTemplate>
                  </asp:TemplateField>
                  <asp:CheckBoxField DataField="Enabled" HeaderText="Enbld" SortExpression="Enabled" ItemStyle-HorizontalAlign="Right" />
                  <asp:TemplateField HeaderImageUrl="~/images/imgButtons/ArrowUpDown.gif" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                    <ItemTemplate>
                      <asp:ImageButton runat="server" ID="btnMoveUp" CommandArgument="<%# ((GridViewRow) Container).RowIndex %>"
                        ImageUrl="~/images/imgButtons/arrow_up.gif" CommandName="MoveUp" />
                      <asp:ImageButton runat="server" ID="btnMoveDown" CommandArgument="<%# ((GridViewRow) Container).RowIndex %>"
                        ImageUrl="~/images/imgButtons/arrow_down.gif" CommandName="MoveDown" />
                    </ItemTemplate>
                  </asp:TemplateField>
                </Columns>
                <PagerStyle HorizontalAlign="Right" CssClass="GridPager" />
              </asp:GridView>
            </td>
            <td class="rowOddC" style="text-align: center; vertical-align: middle">
              <asp:Button ID="btnAddItem" runat="server" Text="<-Add" OnClick="btnAddItem_Click" /><br />
              <br />
              <asp:Button ID="btnRemove" runat="server" Text="Remove->" OnClick="btnRemove_Click" />
            </td>
            <td style="vertical-align: top">
              <asp:GridView ID="gvItemsNotInGroup" runat="server" DataSourceID="odsItemsNotInGroup" AutoGenerateColumns="False"
                AllowPaging="true" PagerSettings-Mode="NumericFirstLast"
                PagerSettings-FirstPageImageUrl="~/images/imgButtons/FirstPage.gif"
                PagerSettings-LastPageImageUrl="~/images/imgButtons/LastPage.gif" PagerStyle-CssClass="GridPager">
                <EmptyDataTemplate>
                  Select a group to list items
                </EmptyDataTemplate>
                <Columns>
                  <asp:TemplateField HeaderText="?" HeaderImageUrl="~/images/imgButtons/AddItem.gif">
                    <ItemTemplate>
                      <asp:CheckBox ID="cbxAddItem" runat="server" />
                    </ItemTemplate>
                  </asp:TemplateField>
                  <asp:TemplateField HeaderText="ItemDesc" SortExpression="Item Type">
                    <ItemTemplate>
                      <asp:DropDownList ID="ddlItemTypeDesc" runat="server" DataSourceID="odsItemTypes"
                        DataTextField="ItemDesc" DataValueField="ItemTypeID" Enabled="False"
                        SelectedValue='<%# Bind("ItemTypeID") == null ? "0" : Bind("ItemTypeID") %>'>
                        <asp:ListItem Value="0" Text="please select an item" />
                      </asp:DropDownList>
                    </ItemTemplate>
                  </asp:TemplateField>
                  <asp:CheckBoxField DataField="ItemEnabled" HeaderText="Enabled" SortExpression="ItemEnabled" ItemStyle-HorizontalAlign="Center" />
                </Columns>
              </asp:GridView>
            </td>
          </tr>
        </tbody>
      </table>
    </ContentTemplate>
    <Triggers>
      <asp:AsyncPostBackTrigger ControlID="ddlGroupItems" EventName="SelectedIndexChanged" />
    </Triggers>
  </asp:UpdatePanel>
  <%# GiveInStatus() %>
  <asp:ObjectDataSource ID="odsItemGroups" runat="server" TypeName="TrackerDotNet.Controls.ItemTypeTbl"
    SelectMethod="GetAllGroupTypeItems" OldValuesParameterFormatString="original_{0}"></asp:ObjectDataSource>
  <asp:ObjectDataSource ID="odsItemsNotInGroup" runat="server" TypeName="TrackerDotNet.Controls.ItemTypeTbl"
    SelectMethod="GetAllItemsNotInItemGroup" OldValuesParameterFormatString="original_{0}">
    <SelectParameters>
      <asp:ControlParameter ControlID="ddlGroupItems" Name="pGroupItemTypeID" PropertyName="SelectedValue" Type="Int32" />
    </SelectParameters>
  </asp:ObjectDataSource>
  <asp:ObjectDataSource ID="odsItemInGroup" runat="server" DataObjectTypeName="TrackerDotNet.Controls.ItemGroupTbl"
    DeleteMethod="DeleteItemGroup" InsertMethod="InsertItemGroup" SelectMethod="GetAllByGroupItemTypeID" SortParameterName="SortBy"
    TypeName="TrackerDotNet.Controls.ItemGroupTbl" UpdateMethod="UpdateItemGroup">
    <SelectParameters>
      <asp:ControlParameter ControlID="ddlGroupItems" Name="pGroupItemID" PropertyName="SelectedValue" Type="Int32" />
      <asp:Parameter Name="SortBy" Type="String" />
    </SelectParameters>
  </asp:ObjectDataSource>
  <asp:ObjectDataSource ID="odsItemTypes" runat="server" SelectMethod="GetAllItemDesc" TypeName="TrackerDotNet.Controls.ItemTypeTbl" />
</asp:Content>
