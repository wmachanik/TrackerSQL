<%@ Page Title="Item Groups" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="ItemGroups.aspx.cs" Inherits="TrackerSQL.Pages.ItemGroups" %>

<asp:Content ID="cntItemGroupsHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="cntItemGroupsBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="scrmngItemGroups" runat="server" EnablePartialRendering="true" />

    <asp:UpdateProgress ID="uprgItemGroups" runat="server"
        AssociatedUpdatePanelID="upnlItemGroups" DisplayAfter="0" DynamicLayout="true">
        <ProgressTemplate>
            <div class="status-message status-info" style="margin: 8px 0;">
                <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
                &nbsp;Please wait...
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:UpdatePanel ID="upnlItemGroups" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <asp:Panel ID="pnlItemGroups" runat="server" CssClass="simpleForm page-tone-panel page-tone-groups">
                <div class="page-tone-header tool-card-header">
                    <img class="tool-card-icon" src="../images/imgButtons/icons8-Item-groups-32.png" alt="" />
                    <div>
                        <h1 class="page-tone-title">Item Groups</h1>
                        <p class="page-tone-subtitle">Manage product categories</p>
                    </div>
                </div>

                <div class="page-tone-toolbar button-row">
                    <asp:Label ID="lblGroup" runat="server" Text="Item group:" AssociatedControlID="ddlGroupItems" CssClass="small" />
                    <asp:DropDownList ID="ddlGroupItems" runat="server"
                        AutoPostBack="true"
                        OnSelectedIndexChanged="ddlGroupItems_SelectedIndexChanged"
                        ToolTip="Select a group to manage its members" />
                    <asp:ImageButton ID="imgbtnAddGroup" runat="server"
                        ImageUrl="~/images/imgButtons/AddButtonCaps.gif"
                        OnClick="btnAddGroup_Click" CausesValidation="false"
                        AlternateText="Add group" ToolTip="Create a new item group"
                        CssClass="toolbar-icon-btn" />
                    <asp:ImageButton ID="imgbtnEditGroup" runat="server"
                        ImageUrl="~/images/imgButtons/EditButton.gif"
                        OnClick="btnEditGroup_Click" CausesValidation="false"
                        AlternateText="Edit group" ToolTip="Edit the selected group name and details"
                        CssClass="toolbar-icon-btn" />
                    <asp:ImageButton ID="imgbtnBack" runat="server"
                        ImageUrl="~/images/imgButtons/Back.gif"
                        OnClick="btnBack_Click" CausesValidation="false"
                        AlternateText="Back" ToolTip="Return to home"
                        CssClass="toolbar-icon-btn" />
                </div>

                <asp:Panel ID="pnlDualList" runat="server" CssClass="dual-list-layout" Visible="false">
                    <div class="dual-list-panel dual-list-panel-in">
                        <h2 class="dual-list-panel-title">Items in group</h2>
                        <div class="dual-list-panel-body">
                        <asp:GridView ID="gvItemsInList" runat="server"
                            DataSourceID="odsItemInGroup" AutoGenerateColumns="False"
                            CssClass="results-table in-panel-grid no-sticky-last"
                            DataKeyNames="ItemTypeID"
                            AllowSorting="true" AllowPaging="true" PageSize="20"
                            OnRowCommand="gvItemsInList_RowCommand"
                            EmptyDataText="No items in this group yet.">
                            <Columns>
                                <asp:TemplateField HeaderText="" HeaderStyle-CssClass="col-cmd" ItemStyle-CssClass="col-cmd">
                                    <ItemTemplate>
                                        <asp:CheckBox ID="cbxRemoveItem" runat="server"
                                            ToolTip="Select to remove from group" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="ItemDesc" HeaderText="Item" SortExpression="ItemDesc"
                                    ReadOnly="true" />
                                <asp:TemplateField HeaderText="Pos" SortExpression="ItemTypeSortPos"
                                    HeaderStyle-CssClass="col-tight" ItemStyle-CssClass="col-tight col-align-center">
                                    <ItemTemplate>
                                        <asp:Label ID="lblItemSortPos" runat="server"
                                            Text='<%# Eval("ItemTypeSortPos") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:CheckBoxField DataField="Enabled" HeaderText="Enabled" SortExpression="Enabled"
                                    HeaderStyle-CssClass="col-tight" ItemStyle-CssClass="col-tight col-align-center"
                                    ReadOnly="true" />
                                <asp:TemplateField HeaderText="" HeaderStyle-CssClass="col-cmd" ItemStyle-CssClass="col-cmd">
                                    <ItemTemplate>
                                        <asp:ImageButton runat="server" ID="btnMoveUp"
                                            CommandArgument="<%# ((GridViewRow) Container).RowIndex %>"
                                            ImageUrl="~/images/imgButtons/arrow_up.gif" CommandName="MoveUp"
                                            AlternateText="Move up" ToolTip="Move up in group order"
                                            CausesValidation="false" />
                                        <asp:ImageButton runat="server" ID="btnMoveDown"
                                            CommandArgument="<%# ((GridViewRow) Container).RowIndex %>"
                                            ImageUrl="~/images/imgButtons/arrow_down.gif" CommandName="MoveDown"
                                            AlternateText="Move down" ToolTip="Move down in group order"
                                            CausesValidation="false" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            <PagerStyle CssClass="aspNetPager" />
                        </asp:GridView>
                        </div>
                    </div>

                    <div class="dual-list-actions dual-list-panel">
                        <asp:Button ID="btnAddToGroup" runat="server" Text="Add"
                            CssClass="filter-panel-btn dual-list-btn dual-list-btn-add"
                            OnClick="btnAddToGroup_Click" CausesValidation="false"
                            ToolTip="Add checked items to the selected group" />
                        <asp:Button ID="btnRemoveFromGroup" runat="server" Text="Remove"
                            CssClass="filter-panel-btn dual-list-btn dual-list-btn-remove"
                            OnClick="btnRemoveFromGroup_Click" CausesValidation="false"
                            ToolTip="Remove checked items from the selected group" />
                    </div>

                    <div class="dual-list-panel dual-list-panel-avail">
                        <h2 class="dual-list-panel-title">Items to add</h2>
                        <div class="dual-list-panel-body">
                        <asp:GridView ID="gvItemsNotInGroup" runat="server"
                            DataSourceID="odsItemsNotInGroup" AutoGenerateColumns="False"
                            CssClass="results-table in-panel-grid no-sticky-last"
                            DataKeyNames="ItemTypeID"
                            AllowPaging="true" PageSize="20"
                            PagerSettings-Mode="NumericFirstLast"
                            EmptyDataText="All coffee items are already in this group.">
                            <Columns>
                                <asp:TemplateField HeaderText="" HeaderStyle-CssClass="col-cmd" ItemStyle-CssClass="col-cmd">
                                    <ItemTemplate>
                                        <asp:CheckBox ID="cbxAddItem" runat="server"
                                            ToolTip="Select to add to group" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="ItemDesc" HeaderText="Item" ReadOnly="true" />
                                <asp:CheckBoxField DataField="ItemEnabled" HeaderText="Enabled"
                                    HeaderStyle-CssClass="col-tight" ItemStyle-CssClass="col-tight col-align-center"
                                    ReadOnly="true" />
                            </Columns>
                            <PagerStyle CssClass="aspNetPager" />
                        </asp:GridView>
                        </div>
                    </div>
                </asp:Panel>

                <div class="page-tone-footer">
                    <div class="status-message" id="pnlStatus" runat="server"><asp:Literal ID="ltrlStatus" runat="server" /></div>
                </div>

                <asp:ObjectDataSource ID="odsItemsNotInGroup" runat="server"
                    TypeName="TrackerSQL.Managers.ItemGroupDataSource"
                    SelectMethod="GetAllItemsNotInItemGroup"
                    OldValuesParameterFormatString="original_{0}">
                    <SelectParameters>
                        <asp:ControlParameter ControlID="ddlGroupItems" Name="groupItemTypeId"
                            PropertyName="SelectedValue" Type="Int32" />
                    </SelectParameters>
                </asp:ObjectDataSource>
                <asp:ObjectDataSource ID="odsItemInGroup" runat="server"
                    DataObjectTypeName="TrackerSQL.Models.ItemGroupGridRow"
                    DeleteMethod="DeleteItemGroup" InsertMethod="InsertItemGroup"
                    SelectMethod="GetAllByGroupItemTypeId" SortParameterName="sortBy"
                    TypeName="TrackerSQL.Managers.ItemGroupDataSource">
                    <SelectParameters>
                        <asp:ControlParameter ControlID="ddlGroupItems" Name="groupItemId"
                            PropertyName="SelectedValue" Type="Int32" />
                        <asp:Parameter Name="sortBy" Type="String" />
                    </SelectParameters>
                </asp:ObjectDataSource>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
