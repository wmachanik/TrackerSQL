<%@ Page Title="Activity dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="ActivityDashboard.aspx.cs" Inherits="TrackerSQL.Pages.ActivityDashboard" %>

<asp:Content ID="cntActivityHdr" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%= ResolveUrl("~/Styles/ActivityDashboard.css") %>" rel="stylesheet" type="text/css" />
</asp:Content>

<asp:Content ID="cntActivityBdy" ContentPlaceHolderID="MainContent" runat="server">
    <div class="simpleForm page-tone-panel page-tone-summary activity-dashboard">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/icons8-weekly-summary-16.png" alt="" />
            <div>
                <h1 class="page-tone-title">Activity dashboard</h1>
                <p class="page-tone-subtitle">
                    Compare each period to the one before it (through <asp:Literal ID="litAsOf" runat="server" />).
                </p>
            </div>
        </div>

        <p class="activity-toolbar-block-title">Period:</p>
        <div class="page-tone-toolbar activity-toolbar activity-toolbar-block">
            <label class="activity-toolbar-label" for="<%= ddlDays.ClientID %>">Length</label>
            <asp:DropDownList ID="ddlDays" runat="server" CssClass="activity-days-ddl" AutoPostBack="true"
                OnSelectedIndexChanged="ddlDays_SelectedIndexChanged">
                <asp:ListItem Text="7 days" Value="7" />
                <asp:ListItem Text="30 days" Value="30" Selected="True" />
                <asp:ListItem Text="90 days" Value="90" />
                <asp:ListItem Text="180 days" Value="180" />
                <asp:ListItem Text="360 days" Value="360" />
                <asp:ListItem Text="Custom date range…" Value="custom" />
            </asp:DropDownList>
            <asp:Panel ID="pnlCustomRange" runat="server" Visible="false" CssClass="activity-custom-range">
                <label class="activity-toolbar-label" for="<%= tbxCustomFrom.ClientID %>">From</label>
                <asp:TextBox ID="tbxCustomFrom" runat="server" CssClass="activity-date-field date-field-clean" />
                <label class="activity-toolbar-label" for="<%= tbxCustomTo.ClientID %>">To</label>
                <asp:TextBox ID="tbxCustomTo" runat="server" CssClass="activity-date-field date-field-clean" />
                <asp:Button ID="btnApplyCustomRange" runat="server" Text="Apply" CssClass="activity-apply-btn"
                    OnClick="btnApplyCustomRange_Click" CausesValidation="false" />
            </asp:Panel>
            <span class="activity-range-hint">
                This period: <asp:Literal ID="litCurrentRange" runat="server" />
                &nbsp;|&nbsp; Prior period: <asp:Literal ID="litPreviousRange" runat="server" />
            </span>
        </div>
        <asp:Panel ID="pnlRangeStatus" runat="server" Visible="false" CssClass="activity-range-status">
            <asp:Literal ID="litRangeStatus" runat="server" />
        </asp:Panel>

        <div class="complex-form-section activity-forward-section">
            <h3>Forward look (same length as selected period)</h3>
            <p class="activity-hint">
                <asp:Literal ID="litForwardRange" runat="server" /> —
                recurring lines due and contacts with a predicted next coffee in this window.
            </p>
            <div class="activity-panel-row activity-panel-row-pair">
                <asp:Panel ID="pnlProjected" runat="server" CssClass="activity-metric-panel activity-panel-projected">
                    <h4>Projected orders</h4>
                    <div class="activity-panel-compare activity-panel-compare-projected">
                        <div class="activity-panel-stat">
                            <span class="activity-panel-value"><asp:Literal ID="litProjectedRecurring" runat="server" Text="0" /></span>
                            <span class="activity-panel-sublabel">Recurring lines due</span>
                        </div>
                        <div class="activity-panel-stat">
                            <span class="activity-panel-value"><asp:Literal ID="litProjectedPredicted" runat="server" Text="0" /></span>
                            <span class="activity-panel-sublabel">Predicted (usage)</span>
                        </div>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlCoffee" runat="server" CssClass="activity-metric-panel activity-panel-coffee">
                    <h4>Coffee delivered</h4>
                    <p class="activity-panel-delta"><asp:Literal ID="litCoffeeDelta" runat="server" /></p>
                    <div class="activity-panel-compare activity-panel-compare-bars">
                        <div class="activity-compare-block">
                            <span class="activity-compare-label">This period</span>
                            <span class="activity-panel-value activity-panel-value-sm"><asp:Literal ID="litCoffeeCurrent" runat="server" /></span>
                            <div class="activity-col-bar">
                                <div id="barCoffeeCurrent" runat="server" class="activity-bar activity-bar-current"></div>
                            </div>
                        </div>
                        <div class="activity-compare-block">
                            <span class="activity-compare-label">Prior period</span>
                            <span class="activity-panel-value activity-panel-value-sm"><asp:Literal ID="litCoffeePrevious" runat="server" /></span>
                            <div class="activity-col-bar">
                                <div id="barCoffeePrior" runat="server" class="activity-bar activity-bar-prior"></div>
                            </div>
                        </div>
                    </div>
                </asp:Panel>
            </div>
        </div>

        <div class="complex-form-section">
            <h3>Activity by area</h3>
            <p class="activity-hint">Each tile compares this period to the prior period of the same length.
                Order confirmation emails from Order Detail are not in SQL — see orders.log.</p>
            <div class="dashboard-links activity-panel-grid">
                <asp:Repeater ID="rptPanels" runat="server" OnItemDataBound="rptPanels_ItemDataBound">
                    <ItemTemplate>
                        <div class="dashboard-card activity-metric-panel home-tone-summary" title="<%# Eval("Hint") %>">
                            <h4><%# Eval("Label") %></h4>
                            <p class="activity-panel-delta"><%# Eval("DeltaText") %></p>
                            <div class="activity-panel-compare activity-panel-compare-bars">
                                <div class="activity-compare-block">
                                    <span class="activity-compare-label">This period</span>
                                    <asp:Literal ID="litCurrentVal" runat="server" />
                                    <div class="activity-col-bar">
                                        <div class="activity-bar activity-bar-current" style="width:<%# Eval("BarCurrentPercent") %>%;"></div>
                                    </div>
                                </div>
                                <div class="activity-compare-block">
                                    <span class="activity-compare-label">Prior period</span>
                                    <asp:Literal ID="litPreviousVal" runat="server" />
                                    <div class="activity-col-bar">
                                        <div class="activity-bar activity-bar-prior" style="width:<%# Eval("BarPreviousPercent") %>%;"></div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>

        <div class="complex-form-section activity-coffee-section">
            <h3>Coffee delivered by item</h3>
            <p class="activity-hint">
                Quantities on completed orders — this period vs the prior period of the same length.
            </p>
            <div class="activity-coffee-strips">
                <div class="activity-coffee-strip activity-coffee-strip-head" aria-hidden="true">
                    <div class="activity-coffee-col-item">Item</div>
                    <div class="activity-coffee-col-qty">This period</div>
                    <div class="activity-coffee-col-bar"></div>
                    <div class="activity-coffee-col-qty">Prior period</div>
                    <div class="activity-coffee-col-bar"></div>
                    <div class="activity-coffee-col-delta">Change</div>
                </div>
                <asp:Repeater ID="rptCoffeeByItem" runat="server">
                    <ItemTemplate>
                        <div class="activity-coffee-strip">
                            <div class="activity-coffee-col-item" title="<%# Eval("ItemDesc") %>"><%# Eval("ItemDesc") %></div>
                            <div class="activity-coffee-col-qty"><%# Eval("CurrentQty", "{0:N2}") %></div>
                            <div class="activity-coffee-col-bar">
                                <div class="activity-bar activity-bar-current" style="width:<%# Eval("BarCurrentPercent") %>%;"></div>
                            </div>
                            <div class="activity-coffee-col-qty"><%# Eval("PreviousQty", "{0:N2}") %></div>
                            <div class="activity-coffee-col-bar">
                                <div class="activity-bar activity-bar-prior" style="width:<%# Eval("BarPreviousPercent") %>%;"></div>
                            </div>
                            <div class="activity-coffee-col-delta"><%# Eval("DeltaText") %></div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
                <asp:Panel ID="pnlCoffeeStripsEmpty" runat="server" Visible="false" CssClass="activity-coffee-strip activity-coffee-strip-empty">
                    <p class="activity-hint">No coffee deliveries in this period or the prior one.</p>
                </asp:Panel>
            </div>
        </div>
    </div>
</asp:Content>