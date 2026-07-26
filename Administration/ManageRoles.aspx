<%@ Page Title="Manage Roles" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="ManageRoles.aspx.cs" Inherits="TrackerSQL.Administration.ManageRoles" %>

<asp:Content ID="cntRolesHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntRolesBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="smManageRoles" runat="server" EnablePartialRendering="true" />

    <asp:UpdateProgress ID="uprgManageRoles" runat="server"
        AssociatedUpdatePanelID="upnlManageRoles" DisplayAfter="0" DynamicLayout="true">
        <ProgressTemplate>
            <div class="status-message status-info" style="margin: 8px 0;">
                <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
                &nbsp;Please wait...
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:UpdatePanel ID="upnlManageRoles" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <asp:Panel ID="pnlManageRoles" runat="server" CssClass="simpleForm page-tone-panel page-tone-users">
                <div class="page-tone-header tool-card-header">
                    <img class="tool-card-icon" src="../images/imgButtons/Lock.gif" alt="" />
                    <div>
                        <h1 class="page-tone-title">Manage Roles</h1>
                        <p class="page-tone-subtitle">Create, rename, and delete membership roles</p>
                    </div>
                </div>

                <div class="results-container" style="margin-top: 8px;">
                    <asp:GridView runat="server" ID="gvRolesManagement"
                        CssClass="results-table results-table-fit"
                        GridLines="None"
                        AutoGenerateColumns="False" DataKeyNames="RoleName"
                        EmptyDataText="No roles found."
                        OnRowEditing="gvRolesManagement_RowEditing"
                        OnRowCancelingEdit="gvRolesManagement_RowCancelingEdit"
                        OnRowUpdating="gvRolesManagement_RowUpdating"
                        OnRowDeleting="gvRolesManagement_RowDeleting"
                        OnRowDataBound="gvRolesManagement_RowDataBound">
                        <Columns>
                            <asp:TemplateField HeaderText="Role Name"
                                HeaderStyle-CssClass="col-priority-1 col-align-left"
                                ItemStyle-CssClass="col-priority-1 col-align-left">
                                <ItemTemplate>
                                    <%# Server.HtmlEncode(Convert.ToString(Eval("RoleName"))) %>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:TextBox ID="tbxRoleName" runat="server"
                                        Text='<%# Bind("RoleName") %>'
                                        CssClass="textEntry" MaxLength="256" />
                                </EditItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="UserCount" HeaderText="User Count" ReadOnly="True"
                                HeaderStyle-CssClass="col-tight col-priority-2 col-align-center"
                                ItemStyle-CssClass="col-tight col-priority-2 col-align-center"
                                HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                            <asp:TemplateField HeaderText=""
                                HeaderStyle-CssClass="col-cmd col-priority-1"
                                ItemStyle-CssClass="col-cmd col-priority-1">
                                <ItemTemplate>
                                    <span class="cmd-button-row">
                                        <span class="image-button" title="Rename this role">
                                            <img src="../images/imgButtons/EditItem.gif" alt="" />
                                            <asp:LinkButton ID="btnEditRole" runat="server" CommandName="Edit"
                                                Text="Edit" ToolTip="Rename this role" CausesValidation="false" />
                                        </span>
                                        <span class="image-button" title="Delete this role">
                                            <img src="../images/imgButtons/DelItem.gif" alt="" />
                                            <asp:LinkButton ID="btnDeleteRole" runat="server" CommandName="Delete"
                                                Text="Delete" ToolTip="Delete this role" CausesValidation="false" />
                                        </span>
                                    </span>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <span class="cmd-button-row">
                                        <span class="image-button" title="Save role name">
                                            <img src="../images/imgButtons/UpdateItem.gif" alt="" />
                                            <asp:LinkButton ID="btnUpdateRole" runat="server" CommandName="Update"
                                                Text="Save" ToolTip="Save role name" CausesValidation="false" />
                                        </span>
                                        <span class="image-button" title="Cancel edit">
                                            <img src="../images/imgButtons/CancelItem.gif" alt="" />
                                            <asp:LinkButton ID="btnCancelRole" runat="server" CommandName="Cancel"
                                                Text="Cancel" ToolTip="Cancel edit" CausesValidation="false" />
                                        </span>
                                    </span>
                                </EditItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>

                <div class="page-tone-toolbar filter-toolbar" style="margin-top: 16px;">
                    <div class="filter-section search-controls">
                        <div class="filter-control">
                            <asp:Label ID="lblRoleName" runat="server" AssociatedControlID="RoleTextBox" Text="Role name:" CssClass="small" />
                            <asp:TextBox ID="RoleTextBox" runat="server" />
                        </div>
                        <asp:Button Text="Create Role" ID="CreateRoleButton" runat="server" OnClick="CreateRole_OnClick"
                            CssClass="filter-panel-btn" Visible="true" />
                    </div>
                </div>

                <div class="status-message" id="pnlStatus" runat="server" style="margin-top: 12px;">
                    <asp:Label ID="MsgLabel" runat="server" />
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
