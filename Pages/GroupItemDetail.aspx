<%@ Page Title="Group Item Detail" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="GroupItemDetail.aspx.cs" Inherits="TrackerSQL.Pages.GroupItemDetail" %>

<asp:Content ID="cntItemGroupDetailHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="cntItemGroupDetail" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="scmGroupDetail" runat="server" EnablePartialRendering="true" />

    <asp:UpdateProgress ID="uprgGroupDetail" runat="server"
        AssociatedUpdatePanelID="upnlGroupDetail" DisplayAfter="0" DynamicLayout="true">
        <ProgressTemplate>
            <div class="status-message status-info" style="margin: 8px 0;">
                <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
                &nbsp;Please wait...
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:UpdatePanel ID="upnlGroupDetail" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <asp:Panel ID="pnlGroupDetail" runat="server" CssClass="simpleForm page-tone-panel page-tone-groups">
                <div class="page-tone-header tool-card-header">
                    <img class="tool-card-icon" src="../images/imgButtons/icons8-Item-groups-32.png" alt="" />
                    <div>
                        <h1 class="page-tone-title">
                            <asp:Literal ID="litPageTitle" runat="server" Text="Add Group" />
                        </h1>
                        <p class="page-tone-subtitle">Manage product categories</p>
                    </div>
                </div>

                <table class="detail-form-table" style="margin-top: 12px;">
                    <tr>
                        <td>Group name</td>
                        <td>
                            <asp:TextBox ID="tbxGroupItem" runat="server" Width="18em" MaxLength="100" />
                            <asp:HiddenField ID="hdnGroupItemID" runat="server" Value="" />
                            <asp:Label ID="lblGroupItemID" runat="server" CssClass="small" Visible="false" />
                        </td>
                    </tr>
                    <tr>
                        <td>Description</td>
                        <td>
                            <asp:TextBox ID="tbxGroupDesc" runat="server" Width="18em" MaxLength="255" />
                        </td>
                    </tr>
                    <tr>
                        <td>Short name</td>
                        <td>
                            <asp:TextBox ID="tbxGroupShortName" runat="server" Width="8em" MaxLength="20" />
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2" class="button-row">
                            <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="filter-panel-btn"
                                OnClick="btnSave_Click" CausesValidation="false"
                                ToolTip="Save this group and return to Item Groups" />
                            <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="filter-panel-btn"
                                OnClick="btnCancel_Click" CausesValidation="false"
                                ToolTip="Return without saving" />
                        </td>
                    </tr>
                </table>

                <div class="status-message" id="pnlStatus" runat="server">
                    <asp:Literal ID="ltrlStatus" runat="server" />
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
