<%@ Page Title="Contact Portal admin" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="ContactPortalAdmin.aspx.cs" Inherits="TrackerSQL.Tools.ContactPortalAdmin" %>

<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server" />
<asp:Content ID="Body" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlAccessDenied" runat="server" Visible="false" CssClass="status-message status-error">
        <asp:Label ID="lblAccessDenied" runat="server" Text="Administrators only." />
    </asp:Panel>

    <asp:Panel ID="pnlMain" runat="server" CssClass="portal-admin-stack">
        <asp:Literal ID="litMessage" runat="server" />

        <asp:Panel ID="pnlFields" runat="server" CssClass="simpleForm page-tone-panel page-tone-sysdata">
            <div class="page-tone-header tool-card-header">
                <img class="tool-card-icon" src="../images/imgButtons/User group.png" alt="" />
                <div>
                    <h1 class="page-tone-title">Contact Portal: fields contacts may edit</h1>
                    <p class="page-tone-subtitle">Ticked fields can be changed by the contact directly. Unticked fields are view only; the contact sends a change request instead.</p>
                </div>
            </div>
            <asp:CheckBoxList ID="cblFields" runat="server" RepeatColumns="2" RepeatDirection="Horizontal" CssClass="portal-field-list" />
            <div class="button-row">
                <asp:Button ID="btnSaveFields" runat="server" Text="Save field settings" CssClass="filter-panel-btn"
                    OnClick="btnSaveFields_Click" />
                <asp:Button ID="btnEnsureSchema" runat="server" Text="Check portal tables" CssClass="filter-panel-btn"
                    OnClick="btnEnsureSchema_Click" CausesValidation="false"
                    ToolTip="Creates any missing Contact Portal tables and the Contact role (safe to run again)" />
                <asp:HyperLink ID="lnkPortal" runat="server" NavigateUrl="~/Portal/Login.aspx?request=1"
                    CssClass="filter-panel-btn sys-prefs-mapping-link" Text="Open the contact access page" Target="_blank"
                    ToolTip="The page contacts use to request a temporary password" />
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlRequests" runat="server" CssClass="simpleForm page-tone-panel page-tone-sysdata">
            <div class="page-tone-header tool-card-header">
                <img class="tool-card-icon" src="../images/imgButtons/icons8-order-16.png" alt="" />
                <div>
                    <h2 class="page-tone-title">Change requests</h2>
                    <p class="page-tone-subtitle">Requests contacts sent from the Contact Portal. Marking one done or rejected keeps it on file and emails the contact, including your note.</p>
                </div>
            </div>
            <div class="filter-toolbar">
                <label for="<%= ddlStatus.ClientID %>">Show:</label>
                <asp:DropDownList ID="ddlStatus" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlStatus_SelectedIndexChanged">
                    <asp:ListItem Value="Open" Text="Open requests" Selected="True" />
                    <asp:ListItem Value="Done" Text="Done" />
                    <asp:ListItem Value="Rejected" Text="Rejected" />
                    <asp:ListItem Value="" Text="All requests" />
                </asp:DropDownList>
            </div>
            <div class="results-container">
                <asp:GridView ID="gvRequests" runat="server" CssClass="results-table"
                    AutoGenerateColumns="false" DataKeyNames="RequestID"
                    AllowPaging="true" PageSize="10"
                    EmptyDataText="No change requests to show."
                    OnRowCommand="gvRequests_RowCommand"
                    OnPageIndexChanging="gvRequests_PageIndexChanging"
                    OnRowCreated="gvRequests_RowCreated">
                    <PagerStyle CssClass="pager-row" />
                    <Columns>
                        <asp:BoundField DataField="RequestID" HeaderText="#" ItemStyle-CssClass="col-tight" />
                        <asp:BoundField DataField="CreatedAt" HeaderText="Received" DataFormatString="{0:dd MMM yyyy HH:mm}" ItemStyle-CssClass="col-tight" />
                        <asp:TemplateField HeaderText="Contact">
                            <ItemTemplate>
                                <asp:HyperLink runat="server" Text='<%# Eval("CompanyName") %>'
                                    NavigateUrl='<%# "~/Pages/ContactDetails.aspx?ID=" + Eval("ContactID") %>' />
                                <div class="sys-prefs-help">
                                    <asp:HyperLink runat="server" Text="View in portal" Target="_blank"
                                        ToolTip="Open My Quaffee as this contact in a new tab (read-only preview)"
                                        NavigateUrl='<%# "~/Portal/ViewAs.aspx?ContactID=" + Eval("ContactID") %>' />
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="About" ItemStyle-CssClass="col-tight">
                            <ItemTemplate><%# HttpUtility.HtmlEncode(AboutText(Container.DataItem)) %></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Request">
                            <ItemTemplate><%# HttpUtility.HtmlEncode(Eval("RequestText")) %></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Status">
                            <ItemTemplate><%# StatusHtml(Container.DataItem) %></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Action">
                            <ItemTemplate>
                                <asp:PlaceHolder runat="server" Visible='<%# IsOpen(Container.DataItem) %>'>
                                    <asp:TextBox ID="txtNote" runat="server" TextMode="MultiLine" Rows="2" MaxLength="1000"
                                        CssClass="sys-prefs-input portal-admin-note" placeholder="Note to the contact (optional)" />
                                    <div class="button-row">
                                        <asp:Button ID="btnDone" runat="server" Text="Done" CssClass="filter-panel-btn"
                                            CommandName="MarkDone" CommandArgument='<%# Eval("RequestID") %>' CausesValidation="false"
                                            OnClientClick="return confirm('Mark this request as done and email the contact?');" />
                                        <asp:Button ID="btnReject" runat="server" Text="Reject" CssClass="filter-panel-btn"
                                            CommandName="MarkReject" CommandArgument='<%# Eval("RequestID") %>' CausesValidation="false"
                                            OnClientClick="return confirm('Reject this request and email the contact?');" />
                                    </div>
                                </asp:PlaceHolder>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
            <div class="button-row">
                <asp:Button ID="btnBack" runat="server" Text="Back to System Tools" CssClass="filter-panel-btn"
                    PostBackUrl="~/Tools/SystemTools.aspx" CausesValidation="false" />
            </div>
        </asp:Panel>
    </asp:Panel>
</asp:Content>
