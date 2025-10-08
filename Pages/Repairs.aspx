<%@ Page Title="Repair List" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Repairs.aspx.cs" Inherits="TrackerDotNet.Pages.Repairs" %>

<asp:Content ID="cntRepairsHdr" ContentPlaceHolderID="HeadContent" runat="server">

</asp:Content>
<asp:Content ID="cntRepairsBdy" ContentPlaceHolderID="MainContent" runat="server">
    <h1>List of Repairs</h1>
    <asp:ScriptManager runat="server" ID="smRepairsSummary" />
    <asp:UpdatePanel ID="upnlSelection" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <div class="filter-toolbar">
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
                        <asp:Button ID="btnGo" Text="Go" runat="server" OnClick="btnGo_Click" ToolTip="search for this item" />
                        <asp:Button ID="btnReset" Text="Reset" runat="server" OnClick="btnReset_Click" />
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
                        <asp:Button ID="btnApplyDateFilter" Text="Apply" runat="server" OnClick="btnApplyDateFilter_Click" />
                    </div>
                </div>

                <div class="filter-section admin-controls">
                    <div class="filter-control">
                        <asp:Label AssociatedControlID="ddlRepairStatus" runat="server" Text="Status:" />
                        <asp:DropDownList ID="ddlRepairStatus" runat="server" AutoPostBack="True"
                            ToolTip="Filter by repairs of a particualr status"
                            AppendDataBoundItems="True" DataSourceID="odsRepairsStatuses"
                            DataTextField="RepairStatusDesc" DataValueField="RepairStatusID"
                            OnSelectedIndexChanged="ddlRepairStatus_SelectedIndexChanged">
                            <asp:ListItem Selected="True" Value="OPEN" Text="-all open repairs-" />
                        </asp:DropDownList>
                    </div>
                <div class="filter-section action-buttons">
                    <asp:HyperLink ID="hlAddRepair" ImageUrl="~/images/imgButtons/AddItem.gif" ToolTip="New Repair"
                        NavigateUrl="~/Pages/RepairDetail.aspx" runat="server" />
                </div>
                </div>

            </div>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="tbxFilterBy" EventName="TextChanged" />
            <asp:AsyncPostBackTrigger ControlID="btnGo" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="ddlDateFilter" EventName="SelectedIndexChanged" />
            <asp:AsyncPostBackTrigger ControlID="btnApplyDateFilter" EventName="Click" />
        </Triggers>
    </asp:UpdatePanel>
    <asp:UpdateProgress ID="uprgToolbar" runat="server" AssociatedUpdatePanelID="upnlSelection" DisplayAfter="100">
        <ProgressTemplate>
            <div style="color: blue; font-weight: bold;">
                <img src="../images/animi/BlueArrowsUpdate.gif" alt="updating" width="16" height="16" />
                filtering repairs...
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:UpdateProgress ID="uprgRepairsSummary" runat="server" AssociatedUpdatePanelID="upnlRepairsSummary" DisplayAfter="100">
        <ProgressTemplate>
            <div style="color: blue; font-weight: bold;">
                <img src="../images/animi/BlueArrowsUpdate.gif" alt="updating" width="16" height="16" />
                Loading repairs...
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:UpdatePanel ID="upnlRepairsSummary" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="results-container full-width" style="padding-left: 1em; padding-right: 1em">
                <asp:GridView ID="gvRepairs" runat="server" AutoGenerateColumns="False"
                    DataSourceID="odsRepairs" CssClass="results-table" AllowSorting="True" PagerSettings-Mode="NextPreviousFirstLast">
                    <EmptyDataTemplate>
                        <div class="simpleLightBrownForm">
                            <h2>No repairs found</h2>
                            Either change the filter or add a repair.
                        </div>
                    </EmptyDataTemplate>
                    <Columns>
                        <asp:TemplateField HeaderText="&nbsp;" HeaderStyle-CssClass="col-priority-1" ItemStyle-CssClass="col-priority-1" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <asp:HyperLink ID="hlEditRepair" runat="server"
                                    ImageUrl="~/images/imgButtons/EditItem.gif"
                                    ToolTip="Edit Repair"
                                    NavigateUrl='<%# Eval("RepairID", "~/Pages/RepairDetail.aspx?RepairID={0}&") %>' />
                            </ItemTemplate>
                            <ItemStyle HorizontalAlign="Center" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Status" HeaderStyle-CssClass="col-priority-1" ItemStyle-CssClass="col-priority-1">
                            <EditItemTemplate>
                                <asp:HyperLink ID="StatusUpdateHyperLink" runat="server" Text='<%# GetRepairStatusDesc((int)Eval("RepairStatusID")) %>'
                                    NavigateUrl='<%# Eval("RepairID", "~/Pages/RepairStatusChange.aspx?RepairID={0}") %>' />
                            </EditItemTemplate>
                            <ItemTemplate>
                                <asp:HyperLink ID="StatusUpdateHyperLink" runat="server" Text='<%# GetRepairStatusDesc((int)Eval("RepairStatusID")) %>'
                                    NavigateUrl='<%# Eval("RepairID", "~/Pages/RepairStatusChange.aspx?RepairID={0}") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Customer" ItemStyle-Font-Size="Small" HeaderStyle-CssClass="col-priority-1" ItemStyle-CssClass="col-priority-1">
                            <EditItemTemplate>
                                <asp:TextBox ID="CustomerTextBox" runat="server" Text='<%# Bind("CustomerID") %>'></asp:TextBox>
                            </EditItemTemplate>
                            <ItemTemplate>
                                <asp:HyperLink ID="CustomerHyperLink" runat="server" Text='<%# GetCompanyName((long)Eval("CustomerID")) %>'
                                    NavigateUrl='<%# Eval("CustomerID", "~/Pages/CustomerDetails.aspx?ID={0}") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="DateLogged" HeaderText="Logged" HeaderStyle-CssClass="col-priority-3" ItemStyle-CssClass="col-priority-3"
                            SortExpression="DateLogged" DataFormatString="{0:d}" />
                        <asp:BoundField DataField="ContactName" HeaderText="Name" HeaderStyle-CssClass="col-priority-2" ItemStyle-CssClass="col-priority-2"
                            SortExpression="ContactName" />
                        <asp:BoundField DataField="JobCardNumber" HeaderText="J/C" HeaderStyle-CssClass="col-priority-4" ItemStyle-CssClass="col-priority-4"
                            SortExpression="JobCardNumber" />
                        <asp:TemplateField HeaderText="Machine" HeaderStyle-CssClass="col-priority-3" ItemStyle-CssClass="col-priority-3">
                            <EditItemTemplate>
                                <asp:Label ID="EquipLabel" runat="server" Text='<%# GetMachineDesc((int)Eval("MachineTypeID")) %>' />
                            </EditItemTemplate>
                            <ItemTemplate>
                                <asp:Label ID="EquipLabel" runat="server" Text='<%# GetMachineDesc((int)Eval("MachineTypeID")) %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="MachineSerialNumber" HeaderStyle-CssClass="col-priority-4" ItemStyle-CssClass="col-priority-4"
                            HeaderText="S/N" SortExpression="MachineSerialNumber" />
                        <asp:TemplateField HeaderText="Fault" ItemStyle-Font-Size="Smaller"
                            HeaderStyle-CssClass="col-priority-4" ItemStyle-CssClass="col-priority-4">
                            <EditItemTemplate>
                                <asp:Label ID="FaultLabel" runat="server" Text='<%# GetRepairFaultDesc((int)Eval("RepairFaultID")) %>' />
                            </EditItemTemplate>
                            <ItemTemplate>
                                <asp:Label ID="FaultLabel" runat="server" Text='<%# GetRepairFaultDesc((int)Eval("RepairFaultID")) %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="RepairFaultDesc" HeaderText="FaultDesc" HeaderStyle-CssClass="col-priority-5" ItemStyle-CssClass="col-priority-5"
                            SortExpression="RepairFaultDesc" />
                        <asp:BoundField DataField="RelatedOrderID" SortExpression="RelatedOrderID" HeaderText="R/OID" HeaderStyle-CssClass="col-priority-5" ItemStyle-CssClass="col-priority-5" />
                    </Columns>
                </asp:GridView>
                <asp:ObjectDataSource ID="odsRepairs" runat="server"
                    SortParameterName="SortBy" SelectMethod="GetRepairsByStatusAndDateRange"
                    TypeName="TrackerDotNet.Controls.RepairsTbl"
                    OldValuesParameterFormatString="original_{0}"
                    DataObjectTypeName="TrackerDotNet.Controls.RepairsTbl"
                    DeleteMethod="DeleteRepair" InsertMethod="InsertRepair"
                    UpdateMethod="UpdateRepair">
                    <DeleteParameters>
                        <asp:Parameter Name="RepairID" Type="Int32" />
                    </DeleteParameters>
                    <SelectParameters>
                        <asp:Parameter Name="SortBy" Type="String" DefaultValue="" />
                        <asp:ControlParameter ControlID="ddlRepairStatus" DefaultValue=""
                            Name="repairStatus" PropertyName="SelectedValue" Type="String" />
                        <asp:SessionParameter Name="fromDate" SessionField="RepairFilterFromDate"
                            Type="Object" ConvertEmptyStringToNull="true" />
                        <asp:SessionParameter Name="toDate" SessionField="RepairFilterToDate"
                            Type="Object" ConvertEmptyStringToNull="true" />
                        <asp:SessionParameter Name="filterBy" SessionField="RepairFilterBySelection" Type="String" />
                        <asp:SessionParameter Name="filterText" SessionField="RepairFilterByText" Type="String" />
                    </SelectParameters>
                </asp:ObjectDataSource>
                <asp:ObjectDataSource ID="odsRepairsStatuses" runat="server"
                    SelectMethod="GetAll" TypeName="TrackerDotNet.Controls.RepairStatusesTbl"
                    OldValuesParameterFormatString="original_{0}">
                    <SelectParameters>
                        <asp:Parameter DefaultValue="RepairStatusID" Name="SortBy" Type="String" />
                    </SelectParameters>
                </asp:ObjectDataSource>
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnGo" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnReset" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="ddlRepairStatus"
                EventName="SelectedIndexChanged" />
            <asp:AsyncPostBackTrigger ControlID="ddlDateFilter" EventName="SelectedIndexChanged" />
            <asp:AsyncPostBackTrigger ControlID="btnApplyDateFilter" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="ddlRepairStatus" EventName="SelectedIndexChanged" />
        </Triggers>
    </asp:UpdatePanel>
    <asp:Label ID="lblFilter" runat="server" />
</asp:Content>