<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ReoccuringOrders.aspx.cs"
    Inherits="TrackerDotNet.Pages.ReoccuringOrders" %>

<asp:Content ID="cntReoccuringOrdersHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntReoccuringOrdersBdy" ContentPlaceHolderID="MainContent" runat="server">
    <h1>List of ReoccuringOrders</h1>
    <asp:ScriptManager ID="smReoccuringOrderSummary" runat="server"></asp:ScriptManager>
    <asp:UpdateProgress ID="uprgReoccuringOrderSummary" runat="server" AssociatedUpdatePanelID="upnlSelection">
        <ProgressTemplate>
            <img src="../images/animi/BlueArrowsUpdate.gif" alt="updating" width="16" height="16" />updating.....
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:UpdatePanel ID="upnlSelection" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="filter-toolbar">
                <div class="filter-section search-controls">
                    <div class="filter-control">
                        <asp:Label AssociatedControlID="ddlFilterBy" runat="server" Text="Filter by:" />
                        <asp:DropDownList ID="ddlFilterBy" runat="server" ToolTip="select which item to search form">
                            <asp:ListItem Value="0" Text="none" />
                            <asp:ListItem Selected="True" Value="CompanyName" Text="Company Name" />
                        </asp:DropDownList>
                        <asp:TextBox ID="tbxFilterBy" runat="server" ToolTip="add '%' to beginning to find contains"
                            OnTextChanged="tbxFilterBy_TextChanged" />
                        <asp:Button ID="btnGon" Text="Go" runat="server" OnClick="btnGon_Click" ToolTip="search for this item" />
                        <asp:Button ID="btnReset" Text="Reset" runat="server"
                            OnClick="btnReset_Click" />
                    </div>
                </div>
                <div class="filter-section admin-controls">
                    <div class="filter-control" style="margin-right: 12px">
                        <asp:Button ID="btnCalcNextRequiredDate" runat="server" Text="Calc Next Required" OnClick="btnCalcNextRequiredDate_Click" />
                    </div>
                    <div class="filter-control">
                        <asp:DropDownList ID="ddlReoccuringOrderEnabled" runat="server" AutoPostBack="true">
                            <asp:ListItem Selected="True" Value="1" Text="enabled only" />
                            <asp:ListItem Value="0" Text="disabled only" />
                            <asp:ListItem Value="-1" Text="both" />
                        </asp:DropDownList>
                    </div>
                    <div class="filter-section action-buttons">
                        <asp:HyperLink ImageUrl="~/images/imgButtons/AddItem.gif" ToolTip="New Contact"
                            NavigateUrl="~/Pages/ReoccuringOrderDetails.aspx" runat="server" />
                    </div>
                </div>
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="tbxFilterBy" EventName="TextChanged" />
            <asp:AsyncPostBackTrigger ControlID="btnGon" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnReset" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnCalcNextRequiredDate" EventName="Click" />
        </Triggers>
    </asp:UpdatePanel>
    <br />
    <asp:UpdatePanel ID="upnlReoccuringOrderSummary" runat="server">
        <ContentTemplate>
            <div class="results-container" style="padding-left: 1em; padding-right: 1em">
                <asp:GridView ID="gvReoccuringOrders" runat="server" AutoGenerateColumns="False" CssClass="results-table"
                    AllowSorting="True" DataSourceID="odsReoccuringOrderSummarys" AllowPaging="True" PageSize="25">
                    <RowStyle Font-Size="Large" />
                    <Columns>
                        <asp:TemplateField HeaderText="&nbsp;" ItemStyle-HorizontalAlign="Center"
                            HeaderStyle-CssClass="col-priority-1" ItemStyle-CssClass="col-priority-1">
                            <ItemTemplate>
                                <asp:HyperLink ID="hlEditReoccuringOrder" runat="server"
                                    ImageUrl="~/images/imgButtons/EditItem.gif"
                                    ToolTip="Edit Reoccuring Order"
                                    NavigateUrl='<%# Eval("ReoccuringOrderID", "~/Pages/ReoccuringOrderDetails.aspx?ID={0}&") %>' />
                            </ItemTemplate>
                            <ItemStyle HorizontalAlign="Center" />
                        </asp:TemplateField>
                        <asp:BoundField DataField="ID" HeaderText="ReoccuringOrderID" SortExpression="ID" Visible="false"
                            HeaderStyle-CssClass="col-priority-1" ItemStyle-CssClass="col-priority-1" />
                        <asp:HyperLinkField DataNavigateUrlFields="CustomerID" DataNavigateUrlFormatString="~/Pages/CustomerDetails.aspx?ID={0}"
                            DataTextField="CompanyName" HeaderText="Company Name" SortExpression="CompanyName"
                            HeaderStyle-CssClass="col-priority-1" ItemStyle-CssClass="col-priority-1" ItemStyle-Font-Size="Smaller" />
                        <asp:BoundField DataField="ReoccuranceValue" HeaderText="Value" SortExpression="ReoccuranceValue" ItemStyle-HorizontalAlign="Center"
                            HeaderStyle-CssClass="col-priority-1" ItemStyle-CssClass="col-priority-1" />
                        <asp:BoundField DataField="ReoccuranceTypeDesc" HeaderText="Type Desc" SortExpression="ReoccuranceTypeDesc" ItemStyle-Font-Size="Smaller"
                            HeaderStyle-CssClass="col-priority-1" ItemStyle-CssClass="col-priority-1" />
                        <asp:BoundField DataField="ItemTypeDesc" HeaderText="Item Type" SortExpression="ItemTypeDesc" ItemStyle-Font-Size="Smaller"
                            HeaderStyle-CssClass="col-priority-1" ItemStyle-CssClass="col-priority-1" />
                        <asp:BoundField DataField="QtyRequired" HeaderText="Qty" SortExpression="QtyRequired" ItemStyle-HorizontalAlign="Center"
                            HeaderStyle-CssClass="col-priority-1" ItemStyle-CssClass="col-priority-1" />
                        <asp:BoundField DataField="DateLastDone" HeaderText="Last Done" SortExpression="LastDone" DataFormatString="{0:d}"
                            HeaderStyle-CssClass="col-priority-3" ItemStyle-CssClass="col-priority-3" />
                        <asp:BoundField DataField="NextDateRequired" HeaderText="Next Date" SortExpression="NextDateRequired" DataFormatString="{0:d}"
                            HeaderStyle-CssClass="col-priority-4" ItemStyle-CssClass="col-priority-4" />
                        <asp:BoundField DataField="RequireUntilDate" HeaderText="Until Date" SortExpression="RequireUntilDate" DataFormatString="{0:d}"
                            HeaderStyle-CssClass="col-priority-5" ItemStyle-CssClass="col-priority-5" />
                        <asp:CheckBoxField DataField="enabled" HeaderText="Enbld" SortExpression="ReoccuringOrdersTbl.enabled"
                            HeaderStyle-CssClass="col-priority-3" ItemStyle-CssClass="col-priority-3" />
                    </Columns>

                </asp:GridView>
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnGon" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnReset" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="ddlReoccuringOrderEnabled" EventName="SelectedIndexChanged" />
            <asp:AsyncPostBackTrigger ControlID="btnCalcNextRequiredDate" EventName="Click" />
        </Triggers>
    </asp:UpdatePanel>

    <asp:ObjectDataSource ID="odsReoccuringOrderSummarys"
        TypeName="TrackerDotNet.Controls.ReoccuringOrderDAL"
        SortParameterName="SortBy"
        SelectMethod="GetAll"
        runat="server" OldValuesParameterFormatString="original_{0}">
        <SelectParameters>
            <asp:Parameter DefaultValue="CompanyName" Name="SortBy"
                Type="String" />
            <asp:ControlParameter ControlID="ddlReoccuringOrderEnabled" DefaultValue="-1"
                Name="IsEnabled" PropertyName="SelectedValue" Type="Int32" />
            <asp:SessionParameter DefaultValue="" Name="WhereFilter"
                SessionField="ReoccuringOrderSummaryWhereFilter" Type="String" />
        </SelectParameters>
    </asp:ObjectDataSource>

    <br />
    <asp:UpdatePanel runat="server" UpdateMode="Always">
        <ContentTemplate>
            <asp:Label ID="lblFilter" Text="" runat="server" />
        </ContentTemplate>
    </asp:UpdatePanel>

</asp:Content>
