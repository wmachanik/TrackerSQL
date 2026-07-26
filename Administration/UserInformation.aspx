<%@ Page Title="User Information" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="UserInformation.aspx.cs" Inherits="TrackerSQL.Administration.UserInformation" %>

<asp:Content ID="cntUserInformationHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntUserInformationBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlUserInformation" runat="server" CssClass="simpleForm page-tone-panel page-tone-users">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/Person.gif" alt="" />
            <div>
                <h1 class="page-tone-title">User Information</h1>
                <p class="page-tone-subtitle">Edit account status, timezone, and roles</p>
            </div>
        </div>

        <div class="user-info-layout">
            <div class="user-info-main">
                <table class="detail-form-table">
                    <tr>
                        <th>User name</th>
                        <td>
                            <asp:Label ID="lblUserName" runat="server" /></td>
                    </tr>
                    <tr>
                        <th>Approved</th>
                        <td>
                            <asp:CheckBox ID="cbxUserIsApproved" Text="" runat="server"
                                OnCheckedChanged="cbxUserIsApproved_CheckedChanged" /></td>
                    </tr>
                    <tr>
                        <th>Locked Out</th>
                        <td>
                            <asp:Label ID="lblUserLockedOut" Text="IsUserLockedOut" runat="server" />
                            &nbsp;
                            <asp:Button ID="btnUnlockUser" Text="Unlock User" runat="server"
                                CssClass="filter-panel-btn" OnClick="btnUnlockUser_Click" />
                        </td>
                    </tr>
                    <tr>
                        <th>Online Status</th>
                        <td>
                            <asp:Label runat="server" ID="OnlineLabel" Text="" /></td>
                    </tr>
                    <tr>
                        <th>Time Zone</th>
                        <td>
                            <asp:DropDownList ID="ddlTimeZone" runat="server" CssClass="form-control" /></td>
                    </tr>
                    <tr>
                        <th>Last Login</th>
                        <td>
                            <asp:Label runat="server" ID="LastLoginDateLabel" Text="" /></td>
                    </tr>
                    <tr>
                        <th>Comment</th>
                        <td>
                            <asp:Label runat="server" ID="EmailLabel" Text="" /></td>
                    </tr>
                </table>
            </div>
            <div class="user-info-roles">
                <h3 class="page-tone-subtitle" style="margin: 0 0 8px;">User's Roles</h3>
                <asp:CheckBoxList CssClass="small" runat="server" ID="UserRolesCheckBoxList" BorderStyle="None" />
            </div>
        </div>

        <div class="page-tone-toolbar button-row" style="margin-top: 16px;">
            <asp:Button ID="btnUpdate" Text="Update User" runat="server" CssClass="filter-panel-btn"
                OnClick="btnUpdateUser_Click" />
            <asp:Button ID="btnDeleteUser" Text="Delete User" runat="server" CssClass="filter-panel-btn"
                OnClick="btnDeleteUser_Click" CausesValidation="false" />
            <asp:Button ID="btnReturnToManagerUser" Text="Back" runat="server" CssClass="filter-panel-btn"
                PostBackUrl="~/Administration/ManageUsers.aspx" CausesValidation="false" />
        </div>

        <div class="status-message" style="margin-top: 12px;">
            <asp:Label ID="lblStatusMessage" Text="" runat="server" />
        </div>
    </asp:Panel>
</asp:Content>
