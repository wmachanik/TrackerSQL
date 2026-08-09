<%@ Page Title="Repair List" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Repairs.aspx.cs" Inherits="TrackerSQL.Pages.Repairs" %>

<asp:Content ID="cntRepairsHdr" title="Repair List" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="cntRepairsBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager runat="server" ID="smRepairsSummary" EnablePartialRendering="true" />

    <asp:UpdateProgress ID="uprgRepairsSummary" runat="server"
        AssociatedUpdatePanelID="upnlRepairs" DisplayAfter="0" DynamicLayout="true">
        <ProgressTemplate>
            <div class="status-message status-info" style="margin: 8px 0;">
                <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
                &nbsp;Please wait...
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:UpdatePanel ID="upnlRepairs" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <asp:Panel ID="pnlRepairs" runat="server" CssClass="simpleForm page-tone-panel page-tone-repairs">
                <div class="page-tone-header tool-card-header">
                    <img class="tool-card-icon" src="../images/imgButtons/icons8-repair-tools-16.png" alt="" />
                    <div>
                        <h1 class="page-tone-title">Repairs</h1>
                        <p class="page-tone-subtitle">View and manage repair requests</p>
                    </div>
                </div>

                <div class="page-tone-toolbar filter-toolbar">
                    <div class="filter-section search-controls">
                        <div class="filter-control">
                            <asp:Label AssociatedControlID="ddlFilterBy" Text="Filter by:" runat="server" />
                            <asp:DropDownList ID="ddlFilterBy" runat="server" Font-Size="X-Small" ToolTip="select which item to search form">
                                <asp:ListItem Selected="True" Value="DateLogged" Text="none" />
                                <asp:ListItem Value="CompanyID" Text="Company Name" />
                                <asp:ListItem Value="MachineSerialNumber" Text="Serial Number" />
                            </asp:DropDownList>
                            <asp:TextBox ID="tbxFilterBy" runat="server" ToolTip="add '%' to beginning to find contains"
                                OnTextChanged="tbxFilterBy_TextChanged" Width="10em" />
                            <asp:Button ID="btnGo" Text="Go" runat="server" CssClass="filter-panel-btn"
                                OnClick="btnGo_Click" ToolTip="search for this item" />
                            <asp:Button ID="btnReset" Text="Reset" runat="server" CssClass="filter-panel-btn"
                                OnClick="btnReset_Click" />
                        </div>
                    </div>

                    <div class="filter-section date-controls">
                        <div class="filter-control date-filter-dropdown">
                            <asp:Label AssociatedControlID="ddlDateFilter" runat="server" Text="Date:" />
                            <asp:DropDownList ID="ddlDateFilter" runat="server" AutoPostBack="True"
                                OnSelectedIndexChanged="ddlDateFilter_SelectedIndexChanged">
                                <asp:ListItem Selected="True" Value="All" Text="All Dates" />
                                <asp:ListItem Value="ThisWeek" Text="This Week" />
                                <asp:ListItem Value="LastWeek" Text="Last Week" />
                                <asp:ListItem Value="ThisMonth" Text="This Month" />
                                <asp:ListItem Value="LastMonth" Text="Last Month" />
                                <asp:ListItem Value="Custom" Text="Custom Range" />
                            </asp:DropDownList>
                        </div>
                        <div class="filter-control custom-date-range" id="divCustomDateRange" runat="server" visible="false">
                            <div class="date-input-group">
                                <asp:Label AssociatedControlID="tbxFromDate" runat="server" Text="From:" />
                                <asp:TextBox ID="tbxFromDate" runat="server" TextMode="Date" />
                            </div>
                            <div class="date-input-group">
                                <asp:Label AssociatedControlID="tbxToDate" runat="server" Text="To:" />
                                <asp:TextBox ID="tbxToDate" runat="server" TextMode="Date" />
                            </div>
                            <asp:Button ID="btnApplyDateFilter" Text="Apply" runat="server" CssClass="filter-panel-btn"
                                OnClick="btnApplyDateFilter_Click" />
                        </div>
                    </div>

                    <div class="filter-section admin-controls">
                        <div class="filter-control">
                            <asp:Label AssociatedControlID="ddlRepairStatus" runat="server" Text="Status:" />
                            <asp:DropDownList ID="ddlRepairStatus" runat="server" AutoPostBack="True"
                                ToolTip="Filter by repairs of a particular status"
                                AppendDataBoundItems="True" DataSourceID="odsRepairsStatuses"
                                DataTextField="RepairStatusDesc" DataValueField="RepairStatusID"
                                OnSelectedIndexChanged="ddlRepairStatus_SelectedIndexChanged">
                                <asp:ListItem Selected="True" Value="OPEN" Text="-all open repairs-" />
                            </asp:DropDownList>
                        </div>
                        <div class="filter-section action-buttons">
                            <asp:HyperLink ID="hlAddRepair" ImageUrl="~/images/imgButtons/AddItem.gif" ToolTip="New Repair"
                                NavigateUrl="~/Pages/RepairDetail.aspx" runat="server" />
                            <asp:Button ID="btnBack" runat="server" Text="Back" CssClass="filter-panel-btn"
                                CausesValidation="false"
                                ToolTip="Return to home" />
                        </div>
                    </div>
                </div>

                <div class="results-container full-width" style="margin-top: 8px;">
                    <asp:GridView ID="gvRepairs" runat="server" AutoGenerateColumns="False"
                        DataSourceID="odsRepairs" CssClass="results-table" AllowSorting="True"
                        PagerSettings-Mode="NextPreviousFirstLast"
                        EmptyDataText="No repairs found. Change the filter or add a repair.">
                        <Columns>
                            <asp:TemplateField HeaderText="&nbsp;" HeaderStyle-CssClass="col-priority-1" ItemStyle-CssClass="col-priority-1" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <asp:HyperLink ID="hlEditRepair" runat="server"
                                        ImageUrl="~/images/imgButtons/EditItem.gif"
                                        ToolTip="Edit Repair"
                                        NavigateUrl='<%# Eval("RepairID", "~/Pages/RepairDetail.aspx?RepairID={0}") %>' />
                                </ItemTemplate>
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Status" HeaderStyle-CssClass="col-priority-1" ItemStyle-CssClass="col-priority-1">
                                <ItemTemplate>
                                    <asp:HyperLink ID="StatusUpdateHyperLink" runat="server" Text='<%# GetRepairStatusDesc((int)Eval("RepairStatusID")) %>'
                                        NavigateUrl='<%# Eval("RepairID", "~/Pages/RepairStatusChange.aspx?RepairID={0}&ReturnUrl=%2fPages%2fRepairs.aspx") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Customer" ItemStyle-Font-Size="Small" HeaderStyle-CssClass="col-priority-1" ItemStyle-CssClass="col-priority-1">
                                <ItemTemplate>
                                    <asp:HyperLink ID="CustomerHyperLink" runat="server" Text='<%# GetCompanyName((long)Eval("CustomerID")) %>'
                                        NavigateUrl='<%# Eval("CustomerID", "~/Pages/ContactDetails.aspx?ID={0}") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="DateLogged" HeaderText="Logged" HeaderStyle-CssClass="col-priority-3" ItemStyle-CssClass="col-priority-3"
                                SortExpression="DateLogged" DataFormatString="{0:d}" />
                            <asp:BoundField DataField="ContactName" HeaderText="Name" HeaderStyle-CssClass="col-priority-2" ItemStyle-CssClass="col-priority-2"
                                SortExpression="ContactName" />
                            <asp:BoundField DataField="JobCardNumber" HeaderText="J/C" HeaderStyle-CssClass="col-priority-4" ItemStyle-CssClass="col-priority-4"
                                SortExpression="JobCardNumber" />
                            <asp:TemplateField HeaderText="Machine" HeaderStyle-CssClass="col-priority-3" ItemStyle-CssClass="col-priority-3">
                                <ItemTemplate>
                                    <asp:Label ID="EquipLabel" runat="server" Text='<%# GetMachineDesc((int)Eval("MachineTypeID")) %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="MachineSerialNumber" HeaderStyle-CssClass="col-priority-4" ItemStyle-CssClass="col-priority-4"
                                HeaderText="S/N" SortExpression="MachineSerialNumber" />
                            <asp:TemplateField HeaderText="Fault" ItemStyle-Font-Size="Smaller"
                                HeaderStyle-CssClass="col-priority-4" ItemStyle-CssClass="col-priority-4">
                                <ItemTemplate>
                                    <asp:Label ID="FaultLabel" runat="server" Text='<%# GetRepairFaultDesc((int)Eval("RepairFaultID")) %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="RepairFaultDesc" HeaderText="FaultDesc" HeaderStyle-CssClass="col-priority-5" ItemStyle-CssClass="col-priority-5"
                                SortExpression="RepairFaultDesc" />
                            <asp:TemplateField HeaderText="R/OLID" SortExpression="RelatedOrderLineID"
                                HeaderStyle-CssClass="col-priority-5" ItemStyle-CssClass="col-priority-5">
                                <ItemTemplate>
                                    <asp:HyperLink ID="hlRelatedOrder" runat="server"
                                        Text='<%# Eval("RelatedOrderLineID") %>'
                                        NavigateUrl='<%# GetRelatedOrderNavigateUrl(Eval("RelatedOrderID"), Eval("RelatedOrderLineID")) %>'
                                        CssClass="repair-related-order-link"
                                        Target="_blank"
                                        ToolTip="Open the related order"
                                        Visible='<%# Convert.ToInt32(Eval("RelatedOrderID") ?? 0) > 0 %>' />
                                    <asp:Label ID="lblRelatedOrder" runat="server"
                                        Text='<%# Eval("RelatedOrderLineID") %>'
                                        Visible='<%# Convert.ToInt32(Eval("RelatedOrderID") ?? 0) <= 0 %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>

                    <asp:ObjectDataSource ID="odsRepairs" runat="server"
                        SortParameterName="SortBy" SelectMethod="GetRepairsByStatusAndDateRange"
                        TypeName="TrackerSQL.Managers.RepairManager"
                        OldValuesParameterFormatString="original_{0}"
                        DataObjectTypeName="TrackerSQL.Models.RepairFormData"
                        DeleteMethod="DeleteRepair" InsertMethod="InsertRepair"
                        UpdateMethod="UpdateRepair">
                        <DeleteParameters>
                            <asp:Parameter Name="RepairID" Type="Int32" />
                        </DeleteParameters>
                        <SelectParameters>
                            <asp:Parameter Name="SortBy" Type="String" DefaultValue="" />
                            <asp:ControlParameter ControlID="ddlRepairStatus" DefaultValue="OPEN"
                                Name="repairStatus" PropertyName="SelectedValue" Type="String" />
                            <asp:ControlParameter ControlID="ddlDateFilter" DefaultValue="All"
                                Name="dateFilter" PropertyName="SelectedValue" Type="String" />
                            <asp:ControlParameter ControlID="tbxFromDate" Name="customFromDate"
                                PropertyName="Text" Type="String" ConvertEmptyStringToNull="true" />
                            <asp:ControlParameter ControlID="tbxToDate" Name="customToDate"
                                PropertyName="Text" Type="String" ConvertEmptyStringToNull="true" />
                            <asp:ControlParameter ControlID="ddlFilterBy" DefaultValue="DateLogged"
                                Name="filterBy" PropertyName="SelectedValue" Type="String" />
                            <asp:ControlParameter ControlID="tbxFilterBy" Name="filterText"
                                PropertyName="Text" Type="String" ConvertEmptyStringToNull="true" />
                        </SelectParameters>
                    </asp:ObjectDataSource>
                    <asp:ObjectDataSource ID="odsRepairsStatuses" runat="server"
                        SelectMethod="GetRepairStatuses" TypeName="TrackerSQL.Managers.RepairLookupDataSource"
                        OldValuesParameterFormatString="original_{0}">
                        <SelectParameters>
                            <asp:Parameter DefaultValue="RepairStatusID" Name="SortBy" Type="String" />
                        </SelectParameters>
                    </asp:ObjectDataSource>
                </div>

                <div class="status-message" id="pnlStatus" runat="server" style="margin-top: 12px;">
                    <asp:Label ID="lblFilter" Text="" runat="server" />
                </div>
            </asp:Panel>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="tbxFilterBy" EventName="TextChanged" />
            <asp:AsyncPostBackTrigger ControlID="btnGo" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnReset" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="ddlDateFilter" EventName="SelectedIndexChanged" />
            <asp:AsyncPostBackTrigger ControlID="btnApplyDateFilter" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="ddlRepairStatus" EventName="SelectedIndexChanged" />
            <asp:PostBackTrigger ControlID="btnBack" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>
