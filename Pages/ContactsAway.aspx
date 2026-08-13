<%@ Page Title="Contacts Away" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="ContactsAway.aspx.cs" Inherits="TrackerSQL.Pages.ContactsAway" %>

<asp:Content ID="cntContactsAwayHdr" title="Contacts Away" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="cntContactsAwayBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="smContactsAway" runat="server" EnablePartialRendering="true" />

    <asp:UpdateProgress ID="uprgContactsAway" runat="server"
        AssociatedUpdatePanelID="upnlContactsAway" DisplayAfter="0" DynamicLayout="true">
        <ProgressTemplate>
            <div class="status-message status-info" style="margin: 8px 0;">
                <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
                &nbsp;Please wait...
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:UpdatePanel ID="upnlContactsAway" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <asp:Panel ID="pnlContactsAway" runat="server" CssClass="simpleForm page-tone-panel page-tone-contacts">
                <div class="page-tone-header tool-card-header">
                    <img class="tool-card-icon" src="../images/imgButtons/CalendarClock.gif" alt="" />
                    <div>
                        <h1 class="page-tone-title">Contacts Away</h1>
                        <p class="page-tone-subtitle">View and manage contact away periods</p>
                    </div>
                </div>

                <div class="page-tone-toolbar filter-toolbar">
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
                        <asp:Button ID="btnGo" Text="Go" runat="server" CssClass="filter-panel-btn"
                            OnClick="btnGo_Click" ToolTip="search for this item" />
                        <asp:Button ID="btnReset" Text="Reset" runat="server" CssClass="filter-panel-btn"
                            OnClick="btnReset_Click" />
                    </div>

                    <div class="filter-section date-controls">
                        <div class="filter-control date-filter-dropdown">
                            <asp:Label AssociatedControlID="ddlDateFilter" runat="server" Text="Date:" />
                            <asp:DropDownList ID="ddlDateFilter" runat="server" AutoPostBack="True"
                                OnSelectedIndexChanged="ddlDateFilter_SelectedIndexChanged">
                                <asp:ListItem Value="Current" Text="Currently Away" />
                                <asp:ListItem Value="ThisMonth" Text="This Month" />
                                <asp:ListItem Selected="True" Value="Next3Months" Text="Next 3 Months" />
                                <asp:ListItem Value="ThisYear" Text="This Year" />
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
                            <asp:Button ID="btnApplyDateFilter" Text="Apply" runat="server" CssClass="filter-panel-btn"
                                OnClick="btnApplyDateFilter_Click" />
                        </div>
                    </div>

                    <div class="filter-section action-buttons">
                        <asp:HyperLink ID="hlAddAway" ImageUrl="~/images/imgButtons/AddItem.gif" ToolTip="Add Away Period"
                            NavigateUrl="~/Pages/ContactsAwayDetail.aspx" runat="server" />
                        <span class="image-button" title="Return to Contacts">
                            <asp:ImageButton ID="btnBack" runat="server"
                                ImageUrl="~/images/imgButtons/Back.gif"
                                AlternateText="Back"
                                ToolTip="Return to Contacts"
                                OnClick="btnBack_Click"
                                CausesValidation="false" />
                        </span>
                    </div>
                </div>

                <div class="results-container" style="margin-top: 8px;">
                    <asp:GridView ID="gvContactsAway" runat="server" AutoGenerateColumns="False" CssClass="results-table"
                        AllowSorting="True" AllowPaging="True" PageSize="25"
                        EmptyDataText="No contacts away for the selected range. Change the filter or add an away period."
                        OnPageIndexChanging="gvContactsAway_PageIndexChanging"
                        OnSorting="gvContactsAway_Sorting"
                        OnRowCommand="gvContactsAway_RowCommand"
                        DataKeyNames="AwayPeriodID">
                        <Columns>
                            <asp:TemplateField HeaderText="&nbsp;" HeaderStyle-CssClass="col-priority-1" ItemStyle-CssClass="col-priority-1">
                                <ItemTemplate>
                                    <asp:ImageButton ID="btnEdit" runat="server" CausesValidation="False"
                                        ImageUrl="~/images/imgButtons/EditItem.gif"
                                        ToolTip="Edit Away Details"
                                        CommandName="EditPeriod"
                                        CommandArgument='<%# Eval("AwayPeriodID") %>' />&nbsp;
                                    <asp:ImageButton ID="btnDelete" runat="server" CausesValidation="False"
                                        ImageUrl="~/images/imgButtons/DelItem.gif"
                                        ToolTip="Delete Away Period"
                                        CommandName="DeletePeriod"
                                        CommandArgument='<%# Eval("AwayPeriodID") %>'
                                        OnClientClick="return confirm('Are you sure you want to delete this away period?');" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="CompanyName" HeaderText="Company Name" SortExpression="CompanyName"
                                HeaderStyle-CssClass="col-priority-1" ItemStyle-CssClass="col-priority-1" />
                            <asp:TemplateField HeaderText="Away Start" SortExpression="AwayStartDate"
                                HeaderStyle-CssClass="col-priority-2" ItemStyle-CssClass="col-priority-2">
                                <ItemTemplate>
                                    <%# FormatAwayDate(Eval("AwayStartDate")) %>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Away End" SortExpression="AwayEndDate"
                                HeaderStyle-CssClass="col-priority-2" ItemStyle-CssClass="col-priority-2">
                                <ItemTemplate>
                                    <%# FormatAwayDate(Eval("AwayEndDate")) %>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="ReasonDesc" HeaderText="Reason"
                                HeaderStyle-CssClass="col-priority-3" ItemStyle-CssClass="col-priority-3" />
                        </Columns>
                    </asp:GridView>
                </div>

                <div class="status-message" id="pnlStatus" runat="server" style="margin-top: 12px;">
                    <asp:Literal ID="ltrlStatus" runat="server" Mode="PassThrough" />
                </div>
            </asp:Panel>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="tbxFilterBy" EventName="TextChanged" />
            <asp:AsyncPostBackTrigger ControlID="btnGo" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnReset" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="ddlDateFilter" EventName="SelectedIndexChanged" />
            <asp:AsyncPostBackTrigger ControlID="btnApplyDateFilter" EventName="Click" />
            <asp:PostBackTrigger ControlID="btnBack" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>
