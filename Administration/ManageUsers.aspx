<%@ Page Title="Manage Users" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="ManageUsers.aspx.cs" Inherits="TrackerSQL.Administration.ManageUsers" %>

<asp:Content ID="cntManageUsersHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntManageUsersBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlManageUsers" runat="server" CssClass="simpleForm page-tone-panel page-tone-users">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/User group.png" alt="" />
            <div>
                <h1 class="page-tone-title">Manage Users</h1>
                <p class="page-tone-subtitle">Approve, unlock, review and tidy up accounts (staff and Contact Portal logins)</p>
            </div>
        </div>

        <div class="page-tone-toolbar filter-toolbar">
            <div class="filter-section search-controls">
                <div class="filter-control">
                    <asp:Label AssociatedControlID="ddlShow" runat="server" Text="Show:" />
                    <asp:DropDownList ID="ddlShow" runat="server" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                        <asp:ListItem Value="all" Text="All logins" Selected="True" />
                        <asp:ListItem Value="staff" Text="Staff" />
                        <asp:ListItem Value="contact" Text="Contact Portal" />
                        <asp:ListItem Value="unused" Text="Not used for 2 years" />
                    </asp:DropDownList>
                </div>
                <div class="filter-control">
                    <asp:TextBox ID="txtSearch" runat="server" ToolTip="Part of the user name, email or company" />
                </div>
                <asp:Button ID="btnSearch" runat="server" Text="Find" CssClass="filter-panel-btn"
                    OnClick="Filter_Changed" CausesValidation="false" />
                <asp:Button ID="btnClear" runat="server" Text="Reset" CssClass="filter-panel-btn"
                    OnClick="btnClear_Click" CausesValidation="false" />
            </div>
        </div>

        <asp:Panel ID="pnlUnused" runat="server" CssClass="users-unused-bar" Visible="false">
            <asp:Literal ID="litUnused" runat="server" />
            <asp:Button ID="btnShowUnused" runat="server" Text="Show them" CssClass="filter-panel-btn"
                OnClick="btnShowUnused_Click" CausesValidation="false" />
            <asp:Button ID="btnDeleteUnused" runat="server" CssClass="filter-panel-btn users-delete-btn"
                OnClick="btnDeleteUnused_Click" CausesValidation="false" />
        </asp:Panel>

        <div class="results-container" style="margin-top: 8px;">
            <asp:GridView ID="gvUserAccounts" runat="server" AutoGenerateColumns="False"
                CssClass="results-table results-table-fit" GridLines="None"
                DataKeyNames="UserName"
                AllowPaging="True" PageSize="25" AllowSorting="True"
                EmptyDataText="No user accounts found."
                OnPageIndexChanging="gvUserAccounts_PageIndexChanging"
                OnSorting="gvUserAccounts_Sorting"
                OnRowCommand="gvUserAccounts_RowCommand"
                OnRowDataBound="gvUserAccounts_RowDataBound">
                <PagerSettings Mode="NumericFirstLast" Position="Bottom" />
                <Columns>
                    <asp:HyperLinkField DataNavigateUrlFields="UserName"
                        DataNavigateUrlFormatString="UserInformation.aspx?user={0}" Text="Manage"
                        HeaderText="Manage"
                        HeaderStyle-CssClass="col-priority-1 col-align-center"
                        ItemStyle-CssClass="col-priority-1 col-align-center" />
                    <asp:BoundField DataField="UserName" HeaderText="User name" SortExpression="UserName"
                        HeaderStyle-CssClass="col-priority-1" ItemStyle-CssClass="col-priority-1" />
                    <asp:BoundField DataField="Email" HeaderText="Email" SortExpression="Email"
                        HeaderStyle-CssClass="col-priority-2" ItemStyle-CssClass="col-priority-2" />
                    <asp:TemplateField HeaderText="Type" SortExpression="Type"
                        HeaderStyle-CssClass="col-priority-1" ItemStyle-CssClass="col-priority-1">
                        <ItemTemplate>
                            <asp:Literal ID="litType" runat="server" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:CheckBoxField DataField="IsApproved" HeaderText="Approved?"
                        HeaderStyle-CssClass="col-priority-2 col-align-center"
                        ItemStyle-CssClass="col-priority-2 col-align-center"
                        ItemStyle-HorizontalAlign="Center" />
                    <asp:CheckBoxField DataField="IsLockedOut" HeaderText="Locked Out?"
                        HeaderStyle-CssClass="col-priority-2 col-align-center"
                        ItemStyle-CssClass="col-priority-2 col-align-center"
                        ItemStyle-HorizontalAlign="Center" />
                    <asp:TemplateField HeaderText=""
                        HeaderStyle-CssClass="col-cmd col-priority-1"
                        ItemStyle-CssClass="col-cmd col-priority-1">
                        <ItemTemplate>
                            <span class="image-button" title="Unlock this account" runat="server" id="spnUnlockUser">
                                <img src="../images/imgButtons/Unlock.gif" alt="" />
                                <asp:LinkButton ID="btnUnlockUser" runat="server"
                                    Text="Unlock"
                                    ToolTip="Unlock this account"
                                    CommandName="UnlockUser"
                                    CommandArgument='<%# Eval("UserName") %>'
                                    CausesValidation="false" />
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Last login" SortExpression="LastUsed"
                        HeaderStyle-CssClass="col-priority-1" ItemStyle-CssClass="col-priority-1">
                        <ItemTemplate>
                            <asp:Literal ID="litLastUsed" runat="server" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:CheckBoxField DataField="IsOnline" HeaderText="Online?"
                        HeaderStyle-CssClass="col-priority-3 col-align-center"
                        ItemStyle-CssClass="col-priority-3 col-align-center"
                        ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField DataField="Comment" HeaderText="Comment"
                        HeaderStyle-CssClass="col-priority-3" ItemStyle-CssClass="col-priority-3"
                        ItemStyle-HorizontalAlign="Left" />
                </Columns>
            </asp:GridView>
        </div>

        <p class="small users-note">
            <asp:Literal ID="litCount" runat="server" />
            <em>Last login</em> is the latest of signing in to Tracker, to the Contact Portal and using the driver app.
        </p>

        <div class="status-message" id="pnlStatus" runat="server" style="margin-top: 12px;">
            <asp:Label ID="lblStatusMessage" runat="server" />
        </div>
    </asp:Panel>
</asp:Content>
