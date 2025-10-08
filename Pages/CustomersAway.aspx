<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CustomersAway.aspx.cs" Inherits="TrackerDotNet.Pages.CustomersAway" %>

<asp:Content ID="cntCustomersAwayHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntCustomersAwayBdy" ContentPlaceHolderID="MainContent" runat="server">
    <h1>List of Customers Away</h1>
    <asp:ScriptManager ID="smCustomersAway" runat="server" />
    <asp:UpdateProgress ID="uprgCustomersAway" runat="server" AssociatedUpdatePanelID="upnlCustomersAwaySummary">
        <ProgressTemplate>
            <img src="../images/animi/BlueArrowsUpdate.gif" alt="updating" width="16" height="16" />updating.....
       
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:UpdatePanel ID="upnlSelection" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="filter-toolbar">
                <div class="filter-section search-controls">
                    <div class="filter-control">
                        <asp:Label AssociatedControlID="ddlFilterBy" Text="Filter by:" runat="server" />
                        <asp:DropDownList ID="ddlFilterBy" runat="server" ToolTip="select which item to search for">
                            <asp:ListItem Value="0" Selected="True" Text="none" />
                            <asp:ListItem Value="CompanyName" Text="Company Name" />
                        </asp:DropDownList>
                    </div>
                    <div class="filter-control">
                        <asp:TextBox ID="tbxFilterBy" runat="server"
                            ToolTip="add '%' to beginning to find contains"
                            OnTextChanged="tbxFilterBy_TextChanged" />
                    </div>
                    <asp:Button ID="btnGo" Text="Go" runat="server" OnClick="btnGo_Click" ToolTip="search for this item" />
                    <asp:Button ID="btnReset" Text="Reset" runat="server" OnClick="btnReset_Click" />
                </div>
                <div class="filter-section date-controls">
                    <div class="filter-control date-filter-dropdown">
                        <asp:Label AssociatedControlID="ddlDateFilter" runat="server" Text="Date:" />
                        <asp:DropDownList ID="ddlDateFilter" runat="server" AutoPostBack="True"
                            OnSelectedIndexChanged="ddlDateFilter_SelectedIndexChanged">
                            <asp:ListItem Selected="True" Value="Current" Text="Currently Away" />
                            <asp:ListItem Value="All" Text="All Periods" />
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
                <div class="filter-section action-buttons">
                    <asp:HyperLink ID="hlAddAway" ImageUrl="~/images/imgButtons/AddItem.gif" ToolTip="Add Away Period"
                        NavigateUrl="~/Pages/CustomersAwayDetail.aspx" runat="server" />
                </div>
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="tbxFilterBy" EventName="TextChanged" />
            <asp:AsyncPostBackTrigger ControlID="btnGo" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnReset" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="ddlDateFilter" EventName="SelectedIndexChanged" />
            <asp:AsyncPostBackTrigger ControlID="btnApplyDateFilter" EventName="Click" />
        </Triggers>
    </asp:UpdatePanel>
    <br />
    <asp:UpdatePanel ID="upnlCustomersAwaySummary" runat="server">
        <ContentTemplate>
            <div class="results-container">
                <asp:GridView ID="gvCustomersAway" runat="server" AutoGenerateColumns="False" CssClass="results-table"
                    AllowSorting="True" DataSourceID="odsCustomersAway" AllowPaging="True" PageSize="25">
                    <EmptyDataTemplate>
                        <div class="simpleLightBrownForm">
                            <h2>No customers are currently away</h2>
                            Either change the filter or add an away period.   
                        </div>
                    </EmptyDataTemplate>
                    <Columns>
                        <asp:TemplateField HeaderText="&nbsp;" HeaderStyle-CssClass="col-priority-1" ItemStyle-CssClass="col-priority-1" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <asp:HyperLink ID="hlEditAwayDetails" runat="server"
                                    ImageUrl="~/images/imgButtons/EditItem.gif"
                                    ToolTip="Edit Away Details"
                                    NavigateUrl='<%# Eval("AwayPeriodID", "~/Pages/CustomersAwayDetail.aspx?AwayPeriodID={0}&") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="CompanyName" HeaderText="Company Name" SortExpression="CompanyName"
                            HeaderStyle-CssClass="col-priority-1" ItemStyle-CssClass="col-priority-1" />
                        <asp:BoundField DataField="AwayStartDate" HeaderText="Away Start" DataFormatString="{0:yyyy-MM-dd}"
                            HeaderStyle-CssClass="col-priority-2" ItemStyle-CssClass="col-priority-2" />
                        <asp:BoundField DataField="AwayEndDate" HeaderText="Away End" DataFormatString="{0:yyyy-MM-dd}"
                            HeaderStyle-CssClass="col-priority-2" ItemStyle-CssClass="col-priority-2" />
                        <asp:BoundField DataField="ReasonDesc" HeaderText="Reason"
                            HeaderStyle-CssClass="col-priority-3" ItemStyle-CssClass="col-priority-3" />
                    </Columns>
                </asp:GridView>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
    <asp:ObjectDataSource ID="odsCustomersAway"
        TypeName="TrackerDotNet.Controls.CustomersAwayTbl"
        SortParameterName="SortBy"
        SelectMethod="GetCustomersAway"
        runat="server" OldValuesParameterFormatString="original_{0}">
        <SelectParameters>
            <asp:Parameter DefaultValue="CompanyName" Name="SortBy" Type="String" />
            <asp:SessionParameter DefaultValue="" Name="WhereFilter" SessionField="CustomersAwayWhereFilter" Type="String" />
        </SelectParameters>
    </asp:ObjectDataSource>
    <asp:UpdatePanel runat="server" UpdateMode="Always">
        <ContentTemplate>
            <asp:Label ID="lblFilter" Text="" runat="server" />
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
