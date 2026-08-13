<%@ Page Title="Recurring Orders" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="RecurringOrders.aspx.cs"
    Inherits="TrackerSQL.Pages.RecurringOrders" %>

<asp:Content ID="cntRecurringOrdersHdr" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        // Delete must be a full postback. Do NOT set data-saving before OnClientClick returns true —
        // that previously cancelled the postback and left the UI stuck on "Deleting...".
        function applyRecurringOrderDeleteUi(link) {
            if (!link) return;
            link.setAttribute("data-saving", "true");
            link.setAttribute("aria-disabled", "true");
            if (link.innerText !== undefined) {
                link.innerText = "Deleting...";
            }
            var strip = document.getElementById("recurringOrdersDeleting");
            if (strip) {
                strip.style.display = "flex";
            }
        }

        function beginRecurringOrderListDelete(link, message) {
            if (!link) return false;

            // Capture-phase already confirmed this click — allow the postback.
            // Must check this BEFORE data-saving (capture may have applied UI already).
            if (link.getAttribute("data-confirm-handled") === "1") {
                link.removeAttribute("data-confirm-handled");
                applyRecurringOrderDeleteUi(link);
                return true;
            }

            // Block true double-submits only after a confirmed attempt.
            if (link.getAttribute("data-saving") === "true") {
                return false;
            }

            var prompt = message
                || link.getAttribute("data-confirm-delete")
                || "Delete this complete recurring order? This cannot be undone.";
            if (!window.confirm(prompt)) {
                return false;
            }
            applyRecurringOrderDeleteUi(link);
            return true;
        }

        // Capture phase: icon/shell clicks still confirm, then force the LinkButton click.
        if (!window._recurringOrdersDeleteConfirmWired) {
            window._recurringOrdersDeleteConfirmWired = true;
            document.addEventListener("click", function (e) {
                if (!e.target || !e.target.closest) return;

                var link = e.target.closest("a[data-confirm-delete]");
                var clickedDirectLink = !!link;
                if (!link) {
                    var shell = e.target.closest("span.image-button");
                    if (shell) {
                        link = shell.querySelector("a[data-confirm-delete]");
                    }
                }
                if (!link) return;
                if (link.getAttribute("data-confirm-handled") === "1") return;
                if (link.getAttribute("data-saving") === "true") return;

                var message = link.getAttribute("data-confirm-delete");
                if (!message) return;

                if (!window.confirm(message)) {
                    e.preventDefault();
                    e.stopPropagation();
                    if (typeof e.stopImmediatePropagation === "function") {
                        e.stopImmediatePropagation();
                    }
                    return;
                }

                // Mark confirmed for OnClientClick. Do NOT set data-saving here when clicking
                // the link text — OnClientClick applies UI and must return true.
                link.setAttribute("data-confirm-handled", "1");

                if (!clickedDirectLink) {
                    // Icon/shell: stop this event and click the real LinkButton.
                    e.preventDefault();
                    e.stopPropagation();
                    if (typeof e.stopImmediatePropagation === "function") {
                        e.stopImmediatePropagation();
                    }
                    applyRecurringOrderDeleteUi(link);
                    link.click();
                }
            }, true);
        }
    </script>
</asp:Content>
<asp:Content ID="cntRecurringOrdersBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="smRecurringOrders" runat="server" EnablePartialRendering="true" />

    <asp:UpdateProgress ID="uprgRecurringOrders" runat="server"
        AssociatedUpdatePanelID="upnlRecurringOrders" DisplayAfter="0" DynamicLayout="true">
        <ProgressTemplate>
            <div class="status-message status-info page-tone-progress">
                <img src="../images/animi/QuaffeeProgress.gif" alt="please wait..." />
                &nbsp;Please wait...
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <div id="recurringOrdersDeleting" class="status-message status-info page-tone-progress"
        style="display: none; margin: 8px 0;" role="status" aria-live="polite">
        <img src="../images/animi/QuaffeeProgress.gif" alt="" />
        <span>&nbsp;Deleting recurring order, please wait...</span>
    </div>

    <asp:UpdatePanel ID="upnlRecurringOrders" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <asp:Panel ID="pnlRecurringOrders" runat="server" CssClass="simpleForm page-tone-panel page-tone-recurring">
                <div class="page-tone-header tool-card-header">
                    <img class="tool-card-icon" src="../images/imgButtons/icons8-order-16.png" alt="" />
                    <div>
                        <h1 class="page-tone-title">Recurring Orders</h1>
                        <p class="page-tone-subtitle">Set up and manage recurring orders</p>
                    </div>
                </div>

                <div class="page-tone-toolbar filter-toolbar">
                    <div class="filter-section search-controls">
                        <div class="filter-control">
                            <asp:Label AssociatedControlID="ddlFilterBy" runat="server" Text="Filter by:" />
                            <asp:DropDownList ID="ddlFilterBy" runat="server" ToolTip="select which item to search for">
                                <asp:ListItem Value="0" Selected="True" Text="none" />
                                <asp:ListItem Value="CompanyName" Text="Company Name" />
                            </asp:DropDownList>
                        </div>
                        <div class="filter-control">
                            <asp:TextBox ID="tbxFilterBy" runat="server" ToolTip="enter a company name"
                                AutoPostBack="True"
                                OnTextChanged="tbxFilterBy_TextChanged" />
                        </div>
                        <asp:Button ID="btnGo" Text="Go" runat="server" CssClass="filter-panel-btn"
                            OnClick="btnGo_Click" ToolTip="search for this item" />
                        <asp:Button ID="btnReset" Text="Reset" runat="server" CssClass="filter-panel-btn"
                            OnClick="btnReset_Click" ToolTip="Clear filters" />
                    </div>
                    <div class="filter-section admin-controls">
                        <asp:Button ID="btnCalcNextRequired" Text="Calc Next Required" runat="server"
                            CssClass="filter-panel-btn"
                            OnClick="btnCalcNextRequired_Click"
                            ToolTip="Recalculate next required dates for enabled recurring lines" />
                        <div class="filter-control">
                            <asp:DropDownList ID="ddlEnabledFilter" runat="server" AutoPostBack="True"
                                OnSelectedIndexChanged="ddlEnabledFilter_SelectedIndexChanged">
                                <asp:ListItem Selected="True" Value="1" Text="enabled only" />
                                <asp:ListItem Value="0" Text="disabled only" />
                                <asp:ListItem Value="-1" Text="both" />
                            </asp:DropDownList>
                        </div>
                        <div class="filter-section action-buttons">
                            <asp:HyperLink ID="hlAddRecurringOrder" ImageUrl="~/images/imgButtons/AddItem.gif"
                                ToolTip="Add Recurring Order"
                                runat="server" />
                            <span class="image-button" title="Return to home">
                                <asp:ImageButton ID="btnBack" runat="server"
                                    ImageUrl="~/images/imgButtons/Back.gif"
                                    AlternateText="Back"
                                    ToolTip="Return to home"
                                    OnClick="btnBack_Click"
                                    CausesValidation="false" />
                            </span>
                        </div>
                    </div>
                </div>

                <div class="results-container" style="margin-top: 8px;">
                    <asp:GridView ID="gvRecurringOrders" runat="server" AutoGenerateColumns="False"
                        CssClass="results-table grouping-table"
                        AllowSorting="True" AllowPaging="True" PageSize="20" ShowHeader="False"
                        OnPageIndexChanging="gvRecurringOrders_PageIndexChanging"
                        OnRowCommand="gvRecurringOrders_RowCommand"
                        OnSorting="gvRecurringOrders_Sorting"
                        OnRowDataBound="gvRecurringOrders_RowDataBound"
                        OnRowCreated="gvRecurringOrders_RowCreated">
                        <PagerStyle CssClass="pager-row" />
                        <PagerTemplate>
                            <asp:PlaceHolder ID="plhPager" runat="server" />
                        </PagerTemplate>
                        <EmptyDataTemplate>
                            <div class="status-message status-info">
                                No recurring orders found. Change the filter or add a recurring order.
                            </div>
                        </EmptyDataTemplate>
                        <Columns>
                            <asp:TemplateField HeaderText="Recurring Orders" SortExpression="CompanyName"
                                HeaderStyle-CssClass="col-priority-1" ItemStyle-CssClass="col-priority-1 wrap">
                                <ItemTemplate>
                                    <div class="grouping-card">
                                        <div class="grouping-card-header">
                                            <div class="grouping-card-header-main">
                                                <span class="image-button" title="Edit this recurring order">
                                                    <img src="../images/imgButtons/EditItem.gif" alt="" />
                                                    <asp:HyperLink ID="hlEditRecurringOrder" runat="server"
                                                        Text="Edit"
                                                        ToolTip="Edit this recurring order" />
                                                </span>
                                                <asp:HyperLink ID="hlContactDetails" runat="server"
                                                    CssClass="grouping-card-title-link"
                                                    Visible="false" />
                                                <asp:Label ID="lblCompanyName" runat="server"
                                                    CssClass="grouping-card-title-label"
                                                    Visible="false" />
                                            </div>
                                            <div class="grouping-card-header-meta">
                                                <span class="image-button" title="Delete this complete recurring order">
                                                    <img src="../images/imgButtons/DelItem.gif" alt="" />
                                                    <asp:LinkButton ID="btnDeleteRecurringOrder" runat="server"
                                                        Text="Delete"
                                                        ToolTip="Delete this complete recurring order"
                                                        CommandName="DeleteRecurringOrder"
                                                        CommandArgument='<%# Eval("RecurringOrderID") %>'
                                                        CausesValidation="false"
                                                        OnClientClick="return beginRecurringOrderListDelete(this);" />
                                                </span>
                                                <asp:Label ID="lblRecurringOrderStatus" runat="server"
                                                    CssClass="status-badge" />
                                                <asp:Label ID="lblRecurringOrderCount" runat="server"
                                                    CssClass="grouping-card-count" />
                                            </div>
                                        </div>
                                        <asp:GridView ID="gvRecurringOrdersForContact" runat="server"
                                            AutoGenerateColumns="False"
                                            CssClass="results-table nested-results-table recurring-summary-grid no-sticky-last"
                                            GridLines="None" ShowHeader="True">
                                            <Columns>
                                                <asp:BoundField DataField="ItemDesc" HeaderText="Item"
                                                    HeaderStyle-CssClass="col-ro-sum-item col-priority-1 col-align-left"
                                                    ItemStyle-CssClass="col-ro-sum-item col-priority-1 col-align-left"
                                                    HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="Left" />
                                                <asp:TemplateField HeaderText="Qty"
                                                    HeaderStyle-CssClass="col-ro-sum-qty col-priority-1 col-align-center"
                                                    ItemStyle-CssClass="col-ro-sum-qty col-priority-1 col-align-center"
                                                    HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                                                    <ItemTemplate>
                                                        <%# FormatQty(Eval("QtyRequired")) %>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:BoundField DataField="ItemPackagingDesc" HeaderText="Packaging"
                                                    HeaderStyle-CssClass="col-ro-sum-pack col-priority-4 col-align-left"
                                                    ItemStyle-CssClass="col-ro-sum-pack col-priority-4 col-align-left"
                                                    HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="Left" />
                                                <asp:BoundField DataField="Value" HeaderText="Value"
                                                    HeaderStyle-CssClass="col-ro-sum-value col-priority-3 col-align-center"
                                                    ItemStyle-CssClass="col-ro-sum-value col-priority-3 col-align-center"
                                                    HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                                                <asp:BoundField DataField="RecurringTypeDesc" HeaderText="Recurrence"
                                                    HeaderStyle-CssClass="col-ro-sum-recur col-priority-2 col-align-left"
                                                    ItemStyle-CssClass="col-ro-sum-recur col-priority-2 col-align-left"
                                                    HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="Left" />
                                                <asp:BoundField DataField="DateLastDone" HeaderText="Last Date" DataFormatString="{0:yyyy-MM-dd}"
                                                    HeaderStyle-CssClass="col-ro-sum-last col-ro-sum-date col-priority-3 col-align-left"
                                                    ItemStyle-CssClass="col-ro-sum-last col-ro-sum-date col-priority-3 col-align-left"
                                                    HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="Left" />
                                                <asp:BoundField DataField="NextDateRequired" HeaderText="Next Date" DataFormatString="{0:yyyy-MM-dd}"
                                                    HeaderStyle-CssClass="col-ro-sum-next col-ro-sum-date col-priority-1 col-align-left"
                                                    ItemStyle-CssClass="col-ro-sum-next col-ro-sum-date col-priority-1 col-align-left"
                                                    HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="Left" />
                                                <asp:TemplateField HeaderText="Until"
                                                    HeaderStyle-CssClass="col-ro-sum-until col-priority-5 col-align-left"
                                                    ItemStyle-CssClass="col-ro-sum-until col-priority-5 col-align-left"
                                                    HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="Left">
                                                    <ItemTemplate>
                                                        <%# FormatRequireUntilDate(Eval("RequireUntilDate")) %>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                            </Columns>
                                        </asp:GridView>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>

                <div class="status-message" id="pnlStatus" runat="server" style="margin-top: 12px;">
                    <asp:Literal ID="ltrlStatus" runat="server" />
                </div>
            </asp:Panel>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="tbxFilterBy" EventName="TextChanged" />
            <asp:AsyncPostBackTrigger ControlID="btnGo" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnReset" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnCalcNextRequired" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="ddlEnabledFilter" EventName="SelectedIndexChanged" />
            <%-- Delete LinkButtons are RegisterPostBackControl in PreRender (full refresh). --%>
            <asp:PostBackTrigger ControlID="btnBack" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>
