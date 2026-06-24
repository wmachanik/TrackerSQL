<%@ Page Title="Contacts Away" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ContactsAway.aspx.cs" Inherits="TrackerSQL.Pages.ContactsAway" %>

<asp:Content ID="cntContactsAwayHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntContactsAwayBdy" ContentPlaceHolderID="MainContent" runat="server">
    <h1>List of Contacts Away</h1>
    <asp:ScriptManager ID="smContactsAway" runat="server" />
    <asp:UpdateProgress ID="uprgContactsAway" runat="server" AssociatedUpdatePanelID="upnlContactsAwaySummary">
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
                        <asp:Button ID="btnApplyDateFilter" Text="Apply" runat="server" OnClick="btnApplyDateFilter_Click" />
                    </div>
                </div>
                <div class="filter-section action-buttons">
                    <asp:HyperLink ID="hlAddAway" ImageUrl="~/images/imgButtons/AddItem.gif" ToolTip="Add Away Period"
                        NavigateUrl="~/Pages/ContactsAwayDetail.aspx" runat="server" />
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
    <asp:UpdatePanel ID="upnlContactsAwaySummary" runat="server">
        <ContentTemplate>
            <div class="results-container">
                <asp:GridView ID="gvContactsAway" runat="server" AutoGenerateColumns="False" CssClass="results-table"
                    AllowSorting="True" AllowPaging="True" PageSize="25"
                    OnPageIndexChanging="gvContactsAway_PageIndexChanging"
                    OnSorting="gvContactsAway_Sorting"
                    OnRowCommand="gvContactsAway_RowCommand"
                    DataKeyNames="AwayPeriodID">
                    <EmptyDataTemplate>
                        <div class="simpleLightBrownForm">
                            <h2>No contacts away for selected range</h2>
                            Either change the filter or add an away period.
                        </div>
                    </EmptyDataTemplate>
                    <Columns>
                        <asp:TemplateField HeaderText="&nbsp;" HeaderStyle-CssClass="col-priority-1" ItemStyle-CssClass="col-priority-1" >
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
                        <asp:TemplateField HeaderText="Away Start" SortExpression="AwayStartDate" HeaderStyle-CssClass="col-priority-2" ItemStyle-CssClass="col-priority-2">
                            <ItemTemplate>
                                <%# FormatAwayDate(Eval("AwayStartDate")) %>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Away End" SortExpression="AwayEndDate" HeaderStyle-CssClass="col-priority-2" ItemStyle-CssClass="col-priority-2">
                            <ItemTemplate>
                                <%# FormatAwayDate(Eval("AwayEndDate")) %>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="ReasonDesc" HeaderText="Reason"
                            HeaderStyle-CssClass="col-priority-3" ItemStyle-CssClass="col-priority-3" />
                    </Columns>
                </asp:GridView>
            </div>
                        </div>
        </ContentTemplate>
    </asp:UpdatePanel>
    <asp:UpdatePanel runat="server" UpdateMode="Always">
        <ContentTemplate>
            <asp:Label ID="lblFilter" Text="" runat="server" />
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>


