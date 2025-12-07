<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="LogViewer.aspx.cs" Inherits="TrackerSQL.Tools.LogViewer" %>

<asp:Content ID="cntLogViewerHdr" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        function toggleCustomDateControls() {
            var ddl = document.getElementById('<%= ddlDateRange.ClientID %>');
            var customDiv = document.getElementById('customDateControls');
            if (ddl && customDiv) {
                customDiv.style.display = (ddl.value === 'Custom') ? '' : 'none';
            }
        }
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(toggleCustomDateControls);
        window.onload = toggleCustomDateControls;
    </script>
</asp:Content>
<asp:Content ID="cntLogViewerBdy" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Application Log Viewer</h1>
    <asp:ScriptManager ID="smLogViewer" runat="server" />
    <asp:UpdatePanel ID="upnlSelection" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="filter-toolbar" runat="server">
                <div class="filter-section search-controls" runat="server">
                    <div class="filter-control">
                        <asp:Label AssociatedControlID="ddlLogFile" Text="Log File:" runat="server" />
                        <asp:DropDownList ID="ddlLogFile" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlLogFile_SelectedIndexChanged" />
                    </div>
                    <div class="filter-control">
                        <asp:Label AssociatedControlID="tbxSearchText" Text="Search:" runat="server" />
                        <asp:TextBox ID="tbxSearchText" runat="server" />
                    </div>
                    <asp:Button ID="btnGo" Text="Go" runat="server" OnClick="btnGo_Click" />
                    <asp:Button ID="btnReset" Text="Reset" runat="server" OnClick="btnReset_Click" />
                    <asp:Button
                        ID="btnDeleteLog"
                        Text="Del Log"
                        runat="server"
                        OnClick="btnDeleteLog_Click"
                        OnClientClick="return confirm('Are you sure you want to delete this log? This action cannot be undone.');" />
                </div>
                <div class="filter-section date-controls">
                    <div class="filter-control date-filter-dropdown">
                        <asp:Label AssociatedControlID="ddlDateRange" Text="Date Range:" runat="server" />
                        <asp:DropDownList ID="ddlDateRange" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlDateRange_SelectedIndexChanged">
                            <asp:ListItem Value="ThisWeek" Text="This Week" Selected="True" />
                            <asp:ListItem Value="ThisMonth" Text="This Month" />
                            <asp:ListItem Value="LastMonth" Text="Last Month" />
                            <asp:ListItem Value="Custom" Text="Custom" />
                        </asp:DropDownList>
                    </div>
                    <div class="filter-control custom-date-range" id="divCustomDateRange" runat="server">
                        <div class="date-input-group">
                            <asp:Label AssociatedControlID="tbxFromDate" Text="From:" runat="server" />
                            <asp:TextBox ID="tbxFromDate" runat="server" TextMode="Date" />
                        </div>
                        <div class="date-input-group">
                            <asp:Label AssociatedControlID="tbxToDate" Text="To:" runat="server" />
                            <asp:TextBox ID="tbxToDate" runat="server" TextMode="Date" />
                        </div>
                    </div>
                </div>
                <div class="filter-section admin-controls" runat="server">
                    <div class="filter-section action-buttons" runat="server">
                        <asp:Button ID="btnApplyDateFilter" Text="Apply" runat="server" OnClick="btnApplyDateFilter_Click" />
                    </div>
                </div>
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnGo" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnReset" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnDeleteLog" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="ddlLogFile" EventName="SelectedIndexChanged" />
            <asp:AsyncPostBackTrigger ControlID="btnApplyDateFilter" EventName="Click" />
        </Triggers>
    </asp:UpdatePanel>
    <asp:UpdatePanel ID="upnlResults" runat="server">
        <ContentTemplate>
            <div class="results-container">
                <asp:GridView ID="gvLogResults" runat="server" AutoGenerateColumns="False" CssClass="results-table"
                    AllowPaging="True" PageSize="25" AllowSorting="True"
                    PagerSettings-Mode="NumericFirstLast"
                    PagerStyle-HorizontalAlign="Left"
                    OnPageIndexChanging="gvLogResults_PageIndexChanging"
                    PagerStyle-CssClass="aspNetPager"
                    PagerSettings-Position="Bottom"
                    PagerSettings-FirstPageImageUrl="../images/imgButtons/FirstPage.gif"
                    PagerSettings-LastPageImageUrl="../images/imgButtons/LastPage.gif"
                    PagerSettings-NextPageImageUrl="../images/imgButtons/NextPage.gif"
                    PagerSettings-PreviousPageImageUrl="../images/imgButtons/PrevPage.gif"
                    OnSorting="gvLogResults_Sorting">
                    <Columns>
                        <asp:BoundField DataField="Date" HeaderText="Date" DataFormatString="{0:yyyy-MM-dd HH:mm:ss}"
                            SortExpression="Date"
                            HeaderStyle-CssClass="col-priority-1" ItemStyle-CssClass="col-priority-1" />
                        <asp:BoundField DataField="User" HeaderText="User"
                            SortExpression="User"
                            HeaderStyle-CssClass="col-priority-2" ItemStyle-CssClass="col-priority-2" />
                        <asp:BoundField DataField="Message" HeaderText="Message"
                            SortExpression="Message"
                            HeaderStyle-CssClass="col-priority-1" ItemStyle-CssClass="col-priority-1" />
                    </Columns>
                </asp:GridView>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
    <asp:Label ID="lblStatus" runat="server" CssClass="status-label" />
</asp:Content>
