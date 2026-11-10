<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="QuaffeeLoversInvite.aspx.cs" Inherits="TrackerSQL.Tools.QuaffeeLoversInvite"
    Title="Quaffee Lovers invites" MaintainScrollPositionOnPostback="true" %>

<asp:Content ID="cntQuaffeeLoversHdr" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="cntQuaffeeLoversBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlAccessDenied" runat="server" Visible="false" CssClass="status-message status-error">
        <asp:Label ID="lblAccessDenied" runat="server" />
    </asp:Panel>

    <asp:Panel ID="pnlMain" runat="server" CssClass="simpleForm page-tone-panel page-tone-tools">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/Woocommerce-Icon.png" alt="" />
            <div>
                <h1 class="page-tone-title">Quaffee Lovers invites</h1>
                <p class="page-tone-subtitle">
                    Find contacts with enough Woo-imported orders. Verify on the shop, then invite when you choose —
                    welcome email and member role are not sent automatically on Woo import.
                    Configure threshold in WooCommerce Mapping (General tab).
                </p>
            </div>
        </div>

        <asp:Literal ID="litFeatureNote" runat="server" />

        <div class="complex-form-section">
            <div class="button-row">
                <asp:Button ID="btnScan" runat="server" CssClass="filter-panel-btn"
                    OnClick="btnScan_Click" CausesValidation="false" Text="Scan contacts" />
            </div>
            <asp:Label ID="lblMessage" runat="server" CssClass="status-message" Visible="false"
                style="display:block; margin: 12px 0;" />
            <asp:Literal ID="litSummary" runat="server" />
        </div>

        <div class="complex-form-section">
            <div class="filter-toolbar">
                <div class="filter-section search-controls">
                    <div class="filter-control">
                        <asp:Label ID="lblFilter" runat="server" AssociatedControlID="txtFilter" Text="Filter:" />
                        <asp:TextBox ID="txtFilter" runat="server" CssClass="sys-prefs-input"
                            placeholder="company or email…" />
                    </div>
                    <asp:Button ID="btnFilter" runat="server" CssClass="filter-panel-btn"
                        OnClick="btnFilter_Click" CausesValidation="false" Text="Search" />
                    <asp:Button ID="btnFilterClear" runat="server" CssClass="filter-panel-btn"
                        OnClick="btnFilterClear_Click" CausesValidation="false" Text="Clear" />
                </div>
            </div>
            <div class="button-row">
                <asp:Button ID="btnSelectAll" runat="server" CssClass="filter-panel-btn"
                    OnClick="btnSelectAll_Click" CausesValidation="false" Text="Select all" />
                <asp:Button ID="btnClearSelect" runat="server" CssClass="filter-panel-btn"
                    OnClick="btnClearSelect_Click" CausesValidation="false" Text="Clear selection" />
                <asp:Button ID="btnCheckShop" runat="server" CssClass="filter-panel-btn"
                    OnClick="btnCheckShop_Click" CausesValidation="false" Text="Verify on shop"
                    ToolTip="Look up selected contacts on WooCommerce (or all in the list if none selected) and refresh shop role and member status." />
                <asp:Button ID="btnInvite" runat="server" CssClass="filter-panel-btn"
                    OnClick="btnInvite_Click" CausesValidation="false" Text="Invite selected"
                    OnClientClick="return confirm('Grant member role and send welcome email to selected contacts?');" />
                <asp:Button ID="btnDisableMember" runat="server" CssClass="filter-panel-btn"
                    OnClick="btnDisableMember_Click" CausesValidation="false" Text="Disable member"
                    OnClientClick="return confirm('Remove the shop member role (set to customer) and clear membership for selected contacts?');"
                    ToolTip="For contacts who are already members on the shop — sets role to customer and clears Tracker membership." />
                <asp:Button ID="btnDismiss" runat="server" CssClass="filter-panel-btn"
                    OnClick="btnDismiss_Click" CausesValidation="false" Text="Dismiss selected"
                    OnClientClick="return confirm('Remove selected contacts from this invite list (no email)?');" />
            </div>
            <div class="results-container">
            <asp:GridView ID="gvInvites" runat="server" CssClass="results-table"
                AutoGenerateColumns="false" DataKeyNames="ContactID"
                AllowPaging="true" PageSize="25"
                OnPageIndexChanging="gvInvites_PageIndexChanging"
                OnRowCreated="gvInvites_RowCreated"
                OnRowDataBound="gvInvites_RowDataBound"
                EmptyDataText="Run Scan contacts to list eligible contacts.">
                <PagerStyle CssClass="pager-row" />
                <PagerTemplate>
                    <asp:PlaceHolder ID="plhPager" runat="server" />
                </PagerTemplate>
                <Columns>
                    <asp:TemplateField HeaderText="" ItemStyle-CssClass="col-tight">
                        <ItemTemplate>
                            <asp:CheckBox ID="chkSelect" runat="server" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="ContactID" HeaderText="ID" ItemStyle-CssClass="col-tight" />
                    <asp:TemplateField HeaderText="Company">
                        <ItemTemplate>
                            <asp:HyperLink ID="lnkContact" runat="server"
                                NavigateUrl='<%# ResolveUrl("~/Pages/ContactDetails.aspx?ID=" + Eval("ContactID")) %>'
                                Text='<%# Eval("CompanyName") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="Email" HeaderText="Email" />
                    <asp:BoundField DataField="WooOrderCount" HeaderText="Woo orders" ItemStyle-CssClass="col-tight" />
                    <asp:TemplateField HeaderText="Shop role">
                        <ItemTemplate>
                            <asp:Label ID="lblShopRole" runat="server"
                                Text='<%# string.IsNullOrWhiteSpace((string)Eval("WooRoleOnShop")) ? "—" : Eval("WooRoleOnShop") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate>
                            <asp:Label ID="lblStatus" runat="server" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="Notes" HeaderText="Notes" />
                </Columns>
            </asp:GridView>
            </div>
        </div>

        <div class="button-row">
            <asp:Button ID="btnBack" runat="server" CssClass="filter-panel-btn"
                PostBackUrl="~/Tools/SystemTools.aspx" Text="Back to System Tools" CausesValidation="false" />
            <asp:Button ID="btnWooMapping" runat="server" CssClass="filter-panel-btn"
                PostBackUrl="~/Tools/WooCommerceMapping.aspx" Text="WooCommerce Mapping" CausesValidation="false" />
        </div>
    </asp:Panel>
</asp:Content>
