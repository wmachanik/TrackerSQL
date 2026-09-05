<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="WooOrderImport.aspx.cs" Inherits="TrackerSQL.Tools.WooOrderImport"
    Title="Woo Order Import" MaintainScrollPositionOnPostback="true" %>

<asp:Content ID="cntWooOrderImportBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlAccessDenied" runat="server" Visible="false" CssClass="status-message status-error">
        <asp:Label ID="lblAccessDenied" runat="server" />
    </asp:Panel>

    <asp:Panel ID="pnlWooDisabled" runat="server" Visible="false" CssClass="simpleForm page-tone-panel page-tone-sysdata">
        <p><asp:Literal ID="litWooDisabled" runat="server" /></p>
        <p>
            <asp:Button ID="btnStartWooWizard" runat="server" CssClass="filter-panel-btn"
                PostBackUrl="~/Tools/SystemPreferences.aspx?section=woo&wizard=1" />
        </p>
    </asp:Panel>

    <asp:Panel ID="pnlMain" runat="server" CssClass="simpleForm page-tone-panel page-tone-tools">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/Toolbox.png" alt="" />
            <div>
                <h1 class="page-tone-title"><asp:Literal ID="litTitle" runat="server" Text="WooCommerce order import" /></h1>
                <p class="page-tone-subtitle">
                    Pull orders from WooCommerce, preview contact/area/line mapping, then import into Tracker.
                    Gear-only orders use <strong>ZZName</strong> with the customer name and shipping address in Notes.
                    PO is set to the Woo order number. Payment abbreviations (e.g. <code>[PF]</code>) are configured under
                    <asp:HyperLink ID="lnkPaymentMaps" runat="server" NavigateUrl="~/Tools/WooCommerceMapping.aspx?tab=payment" Text="Woo Mapping → Payment" />.
                    Delivery person comes from the contact’s area — Woo shipping method titles are not used.
                </p>
            </div>
        </div>

        <asp:Panel ID="pnlStatus" runat="server" Visible="false" CssClass="status-message">
            <asp:Literal ID="litStatus" runat="server" />
        </asp:Panel>

        <div class="complex-form-section">
            <h3>Pull orders</h3>
            <div class="filter-toolbar">
                <div class="filter-section">
                    <div class="filter-control">
                        <asp:Label ID="lblMode" runat="server" AssociatedControlID="ddlMode" Text="Mode:" />
                        <asp:DropDownList ID="ddlMode" runat="server" CssClass="sys-prefs-input" AutoPostBack="true"
                            OnSelectedIndexChanged="ddlMode_SelectedIndexChanged">
                            <asp:ListItem Value="Specific" Text="Specific order #" />
                            <asp:ListItem Value="Last" Text="Latest order" />
                            <asp:ListItem Value="SinceLastSync" Text="Since last import sync" Selected="True" />
                            <asp:ListItem Value="DateRange" Text="Date range" />
                        </asp:DropDownList>
                    </div>
                    <asp:Panel ID="pnlSpecific" runat="server" CssClass="filter-control" Visible="false">
                        <asp:Label ID="lblOrderId" runat="server" AssociatedControlID="txtOrderId" Text="Woo order #:" />
                        <asp:TextBox ID="txtOrderId" runat="server" CssClass="sys-prefs-input" />
                    </asp:Panel>
                    <asp:Panel ID="pnlDateRange" runat="server" CssClass="filter-control" Visible="false">
                        <asp:Label ID="lblFrom" runat="server" AssociatedControlID="txtFromDate" Text="From:" />
                        <asp:TextBox ID="txtFromDate" runat="server" CssClass="sys-prefs-input" TextMode="Date" />
                        <asp:Label ID="lblTo" runat="server" AssociatedControlID="txtToDate" Text="To:" />
                        <asp:TextBox ID="txtToDate" runat="server" CssClass="sys-prefs-input" TextMode="Date" />
                    </asp:Panel>
                    <asp:Button ID="btnPull" runat="server" CssClass="filter-panel-btn" Text="Pull preview"
                        OnClick="btnPull_Click" CausesValidation="false" />
                    <asp:Button ID="btnClearPreview" runat="server" CssClass="filter-panel-btn" Text="Clear preview"
                        OnClick="btnClearPreview_Click" CausesValidation="false"
                        OnClientClick="return confirm('Clear the current import preview and saved filters?');" />
                </div>
            </div>
            <p class="woo-map-section-note">
                <asp:Literal ID="litLastSync" runat="server" />
            </p>
        </div>

        <div class="complex-form-section">
            <h3>Preview</h3>
            <div class="button-row">
                <asp:CheckBox ID="chkUpdateExisting" runat="server" AutoPostBack="true"
                    OnCheckedChanged="chkUpdateExisting_CheckedChanged"
                    Text="Update orders already imported (refresh from Woo)" />
            </div>
            <div class="results-container scrollable-table-container woo-order-import-preview-scroll">
            <asp:GridView ID="gvPreview" runat="server" CssClass="results-table results-table-fit woo-order-import-preview-grid" AutoGenerateColumns="false"
                DataKeyNames="WooOrderId" AllowPaging="true" PageSize="15"
                OnPageIndexChanging="gvPreview_PageIndexChanging"
                OnRowCreated="gvPreview_RowCreated"
                OnRowDataBound="gvPreview_RowDataBound"
                OnRowCommand="gvPreview_RowCommand"
                EmptyDataText="Pull orders to see a preview.">
                <PagerStyle CssClass="pager-row" />
                <PagerTemplate>
                    <asp:PlaceHolder ID="plhPager" runat="server" />
                </PagerTemplate>
                <Columns>
                    <asp:TemplateField HeaderText="" ItemStyle-CssClass="col-cmd woo-import-actions-col">
                        <ItemTemplate>
                            <span class="toolbar-icon-row woo-import-row-actions">
                                <asp:PlaceHolder runat="server" Visible='<%# ShowAddOrderButton(Eval("CanImport"), Eval("AlreadyImported")) %>'>
                                    <asp:ImageButton ID="btnAddOrder" runat="server"
                                        CssClass="woo-import-action-btn"
                                        ImageUrl="~/images/imgButtons/Add-Order.png"
                                        AlternateText='<%# GetAddOrderButtonText(Eval("AlreadyImported")) %>'
                                        ToolTip='<%# GetAddOrderButtonText(Eval("AlreadyImported")) %>'
                                        CommandName="AddOrder"
                                        CommandArgument='<%# Eval("WooOrderId") %>'
                                        CausesValidation="false" />
                                </asp:PlaceHolder>
                                <asp:PlaceHolder ID="phOpenTrackerOrder" runat="server" Visible="false">
                                    <asp:HyperLink ID="hlOpenTrackerOrder" runat="server"
                                        CssClass="woo-import-action-link"
                                        ToolTip="Open Tracker order">
                                        <img src="../images/imgButtons/icons8-view-orders-16.png" alt="Open order" />
                                    </asp:HyperLink>
                                </asp:PlaceHolder>
                                <asp:PlaceHolder runat="server" Visible='<%# (bool)Eval("CanAddContact") %>'>
                                    <asp:ImageButton ID="btnAddContact" runat="server"
                                        CssClass="woo-import-action-btn"
                                        ImageUrl="~/images/imgButtons/Add-Contact-Card.png"
                                        AlternateText="Add contact"
                                        ToolTip="Add contact from Woo shipping address"
                                        CommandName="AddContact"
                                        CommandArgument='<%# Eval("WooOrderId") %>'
                                        CausesValidation="false"
                                        OnClientClick="return confirm('Create contact from this Woo order shipping address and open Contact Details?');" />
                                </asp:PlaceHolder>
                                <asp:PlaceHolder runat="server" Visible='<%# (bool)Eval("CanUpdateContact") %>'>
                                    <asp:ImageButton ID="btnUpdateContact" runat="server"
                                        CssClass="woo-import-action-btn"
                                        ImageUrl="~/images/imgButtons/Update-Contact-Card.png"
                                        AlternateText="Update contact"
                                        ToolTip="Update contact address, area, or phone from Woo shipping"
                                        CommandName="UpdateContact"
                                        CommandArgument='<%# Eval("WooOrderId") %>'
                                        CausesValidation="false"
                                        OnClientClick="return confirm('Update matched contact from Woo shipping address?');" />
                                </asp:PlaceHolder>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="WooOrderNumber" HeaderText="Woo #" ItemStyle-CssClass="col-tight" />
                    <asp:TemplateField HeaderText="Status" ItemStyle-CssClass="col-tight woo-import-status-col">
                        <ItemTemplate>
                            <span class='<%# GetWooStatusCssClass(Eval("WooStatus")) %>'><%# Eval("WooStatus") %></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Conflicts" ItemStyle-CssClass="woo-import-warn-col woo-import-warn-early">
                        <ItemTemplate>
                            <asp:Literal ID="litWarnings" runat="server"
                                Text='<%# FormatWarnings(Eval("Conflicts"), Eval("Warnings")) %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="OrderDate" HeaderText="Date" DataFormatString="{0:yyyy-MM-dd}" />
                    <asp:BoundField DataField="PaymentAbbrev" HeaderText="Pay" ItemStyle-CssClass="col-tight" />
                    <asp:TemplateField HeaderText="Contact" ItemStyle-CssClass="woo-import-contact-col">
                        <ItemTemplate>
                            <asp:Literal ID="litContactStatus" runat="server"
                                Visible='<%# !string.IsNullOrEmpty(Convert.ToString(Eval("ContactStatusLabel"))) %>'
                                Text='<%# Eval("ContactStatusLabel") + " — " %>' />
                            <asp:HyperLink ID="hlContact" runat="server"
                                Visible='<%# HasContactLink(Eval("MatchedContactId")) %>'
                                NavigateUrl='<%# GetContactUrl(Eval("MatchedContactId")) %>'
                                Text='<%# Eval("ContactDisplayName") %>'
                                ToolTip="Open contact" />
                            <asp:Literal ID="litContact" runat="server"
                                Visible='<%# !HasContactLink(Eval("MatchedContactId")) && !(bool)Eval("UseZzName") %>'
                                Text='<%# Eval("ContactDisplayName") %>' />
                            <asp:Literal ID="litZzContact" runat="server"
                                Visible='<%# (bool)Eval("UseZzName") %>'
                                Text='<%# Eval("ContactSummary") %>' />
                            <%# (bool)Eval("UseZzName") ? " <span class=\"woo-import-tag\">ZZName</span>" : "" %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="ResolvedAreaName" HeaderText="Area" />
                    <asp:TemplateField HeaderText="Tracker">
                        <ItemTemplate>
                            <asp:HyperLink ID="hlTrackerOrder" runat="server"
                                Visible='<%# HasTrackerOrderLink(Eval("AlreadyImported"), Eval("ExistingTrackerOrderId")) %>'
                                NavigateUrl='<%# GetTrackerOrderUrl(Eval("ExistingTrackerOrderId")) %>'
                                Text='<%# "Order #" + Eval("ExistingTrackerOrderId") %>'
                                ToolTip="Open Tracker order (Cancel there if this Woo order should be imported again)" />
                            <asp:Literal ID="litTrackerNew" runat="server"
                                Visible='<%# !(bool)Eval("AlreadyImported") %>'
                                Text="New" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="LinesSummary" HeaderText="Lines" ItemStyle-CssClass="woo-import-lines-col" />
                </Columns>
            </asp:GridView>
            </div>
        </div>

        <div class="complex-form-section">
            <h3>Import conflicts</h3>
            <p class="woo-map-section-note">Orders imported with conflicts (unmapped SKUs, duplicate delivery date, ambiguous area, etc.). Conflicts are also appended to the Tracker order Notes.</p>
            <div class="button-row">
                <asp:Button ID="btnRefreshConflicts" runat="server" CssClass="filter-panel-btn" Text="Refresh list"
                    OnClick="btnRefreshConflicts_Click" CausesValidation="false" />
            </div>
            <asp:GridView ID="gvConflicts" runat="server" CssClass="results-table" AutoGenerateColumns="false"
                EmptyDataText="No import conflicts recorded yet.">
                <Columns>
                    <asp:HyperLinkField DataTextField="OrderID" DataNavigateUrlFields="OrderID"
                        DataNavigateUrlFormatString="~/Pages/OrderDetail.aspx?OrderID={0}" HeaderText="Order #" />
                    <asp:BoundField DataField="WooOrderNumber" HeaderText="Woo #" />
                    <asp:BoundField DataField="OrderDate" HeaderText="Date" DataFormatString="{0:yyyy-MM-dd}" />
                    <asp:BoundField DataField="ImportConflicts" HeaderText="Conflicts" />
                    <asp:BoundField DataField="PurchaseOrder" HeaderText="PO" ItemStyle-CssClass="col-tight" />
                </Columns>
            </asp:GridView>
        </div>

        <asp:Panel ID="pnlResults" runat="server" Visible="false" CssClass="complex-form-section">
            <h3>Import results</h3>
            <asp:BulletedList ID="blResults" runat="server" CssClass="woo-import-results" />
        </asp:Panel>
    </asp:Panel>
</asp:Content>
