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
                <p class="page-tone-subtitle">Approve, unlock, and review accounts</p>
            </div>
        </div>

        <div class="results-container" style="margin-top: 8px;">
            <asp:GridView ID="gvUserAccounts" runat="server" AutoGenerateColumns="False"
                CssClass="results-table results-table-fit" GridLines="None"
                DataKeyNames="UserName"
                EmptyDataText="No user accounts found."
                OnRowCommand="gvUserAccounts_RowCommand"
                OnRowDataBound="gvUserAccounts_RowDataBound">
                <Columns>
                    <asp:HyperLinkField DataNavigateUrlFields="UserName"
                        DataNavigateUrlFormatString="UserInformation.aspx?user={0}" Text="Manage"
                        HeaderText="Manage"
                        HeaderStyle-CssClass="col-priority-1 col-align-center"
                        ItemStyle-CssClass="col-priority-1 col-align-center" />
                    <asp:BoundField DataField="UserName" HeaderText="User name"
                        HeaderStyle-CssClass="col-priority-1" ItemStyle-CssClass="col-priority-1" />
                    <asp:BoundField DataField="Email" HeaderText="Email"
                        HeaderStyle-CssClass="col-priority-2" ItemStyle-CssClass="col-priority-2" />
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

        <div class="status-message" id="pnlStatus" runat="server" style="margin-top: 12px;">
            <asp:Label ID="lblStatusMessage" runat="server" />
        </div>
    </asp:Panel>
</asp:Content>
