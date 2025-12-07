<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="UserInformation.aspx.cs" Inherits="TrackerSQL.Administration.UserInformation" %>

<asp:Content ID="cntUserInformationHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="cntUserInformationBdy" ContentPlaceHolderID="MainContent" runat="server">
    <h1 class="InputFrm">User Information</h1>
    <div class="responsive-layout-container">
        <div class="layout-main-panel">
            <table class="TblFlex">
                <tr>
                    <td><b>User name:</b></td>
                    <td>
                        <asp:Label ID="lblUserName" runat="server" /></td>
                </tr>
                <tr>
                    <td><b>Approved</b></td>
                    <td class="horizMiddle">
                        <asp:CheckBox ID="cbxUserIsApproved" Text="" runat="server" TextAlign="Right"
                            OnCheckedChanged="cbxUserIsApproved_CheckedChanged" /></td>
                </tr>
                <tr>
                </tr>
                <tr>
                    <td><b>Locked Out</b></td>
                    <td class="horizMiddle">
                        <asp:Label ID="lblUserLockedOut" Text="IsUserLockedOut" runat="server" />&nbsp;&nbsp;
                        <asp:Button ID="btnUnlockUser" Text="Unlock User" runat="server"
                            OnClick="btnUnlockUser_Click" />
                    </td>
                </tr>
                <tr>
                    <td>Online Status</td>
                    <td>
                        <asp:Label runat="server" ID="OnlineLabel" Text="" /></td>
                </tr>
                <tr>
                    <td><b>Time Zone</b></td>
                    <td>
                        <asp:DropDownList ID="ddlTimeZone" runat="server" CssClass="form-control" /></td>
                </tr>
                <tr>
                    <td>Last Login</td>
                    <td>
                        <asp:Label runat="server" ID="LastLoginDateLabel" Text="" /></td>
                </tr>
                <tr>
                    <td>Comment</td>
                    <td>
                        <asp:Label runat="server" ID="EmailLabel" Text="" /></td>
                </tr>
            </table>
        </div>
        <div class="layout-detail-panel">
            <table class="TblNoBorder">
                <tr>
                    <td><b>User's Roles</b></td>
                </tr>
                <tr>
                    <td style="clear: both" valign="top">
                        <asp:CheckBoxList CssClass="small" runat="server" ID="UserRolesCheckBoxList" BorderStyle="None" />
                    </td>
            </table>
        </div>
        <div class="layout-footer-panel">
            <asp:Button ID="btnDeleteUser" Text="Delete User" runat="server" OnClick="btnDeleteUser_Click" />&nbsp;&nbsp;
            <asp:Button ID="btnUpdate" Text="Update User" runat="server" OnClick="btnUpdateUser_Click" />&nbsp;&nbsp;
            <asp:Button ID="btnReturnToManagerUser" Text="Return" runat="server"
                PostBackUrl="~/Administration/ManageUsers.aspx" Style="text-align: center" />
        </div>
        <div>
            <asp:Label ID="lblStatusMessage" Text="" runat="server" />
        </div>
    </div>

</asp:Content>
