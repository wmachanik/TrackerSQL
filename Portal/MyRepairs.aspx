<%@ Page Title="My Repairs" Language="C#" MasterPageFile="~/Portal/Portal.Master"
    AutoEventWireup="true" CodeBehind="MyRepairs.aspx.cs" Inherits="TrackerSQL.Portal.MyRepairs" %>

<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server" />
<asp:Content ID="Body" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnl" runat="server" CssClass="simpleForm page-tone-panel page-tone-repairs portal-card">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/icons8-repair-tools-16.png" alt="" />
            <div>
                <h1 class="page-tone-title">My Repairs</h1>
                <p class="page-tone-subtitle">Machine repairs and their progress, newest first</p>
            </div>
        </div>
        <asp:UpdatePanel ID="upList" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
        <div class="portal-table-wrap">
            <asp:GridView ID="gv" runat="server" CssClass="results-table portal-table"
                AutoGenerateColumns="false" EmptyDataText="No repairs found." GridLines="None"
                AllowPaging="true" PageSize="10" OnPageIndexChanging="gv_PageIndexChanging">
                <PagerSettings Mode="NumericFirstLast" Position="Bottom" FirstPageText="First" LastPageText="Last" />
                <PagerStyle CssClass="pager-row" />
                <Columns>
                    <asp:BoundField DataField="JobCardNumber" HeaderText="Job Card" ItemStyle-CssClass="portal-col-nowrap" />
                    <asp:BoundField DataField="DateLoggedDisplay" HeaderText="Logged" ItemStyle-CssClass="portal-col-nowrap" />
                    <asp:BoundField DataField="EquipTypeName" HeaderText="Machine" />
                    <asp:BoundField DataField="FaultDisplay" HeaderText="Fault" />
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate><%# StatusBadgeHtml((string)Eval("StatusDisplay"), IsDoneStatus((int)Eval("RepairStatusID"))) %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="LastStatusChangeDisplay" HeaderText="Last Update" ItemStyle-CssClass="portal-col-nowrap" />
                    <asp:TemplateField HeaderText="" ItemStyle-CssClass="portal-col-action" HeaderStyle-CssClass="portal-col-action">
                        <ItemTemplate>
                            <span class="image-button" title="View this repair">
                                <img src="../images/imgButtons/View.png" alt="" />
                                <asp:HyperLink runat="server"
                                    NavigateUrl='<%# Eval("RepairID", "MyRepair.aspx?RepairID={0}") %>'
                                    Text="View" />
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </asp:Panel>
</asp:Content>
