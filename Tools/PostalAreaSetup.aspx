<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="PostalAreaSetup.aspx.cs" Inherits="TrackerSQL.Tools.PostalAreaSetup"
    Title="Postal Area Setup" MaintainScrollPositionOnPostback="true" %>

<asp:Content ID="cntPostalAreaHdr" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        window.postalSetupSetSaveReady = function (btn, ready) {
            if (!btn) return;
            btn.setAttribute('data-woo-save-ready', ready ? '1' : '0');
        };
        window.postalSetupMarkSaving = function (btn) {
            if (!btn) return true;
            if (btn.getAttribute('data-woo-save-ready') === '0')
                return false;
            if (btn.getAttribute('data-postal-saving') === '1')
                return false;
            var cur = (btn.value != null && btn.tagName === 'INPUT')
                ? btn.value
                : (btn.innerText || btn.textContent || '');
            if (!btn.getAttribute('data-postal-save-label'))
                btn.setAttribute('data-postal-save-label', cur);
            if (btn.tagName === 'INPUT')
                btn.value = 'Saving...';
            else
                btn.innerText = 'Saving...';
            btn.setAttribute('data-postal-saving', '1');
            return true;
        };
        window.postalSetupWireDirty = function () {
            var grid = document.getElementById('<%= gvAreas.ClientID %>');
            var saveAreas = document.getElementById('<%= btnSaveAreas.ClientID %>');
            if (grid && saveAreas && !grid.getAttribute('data-postal-dirty-wired')) {
                grid.setAttribute('data-postal-dirty-wired', '1');
                grid.addEventListener('input', function () { postalSetupSetSaveReady(saveAreas, true); });
                grid.addEventListener('change', function () { postalSetupSetSaveReady(saveAreas, true); });
            }
            var ddlCatch = document.getElementById('<%= ddlCatchAllArea.ClientID %>');
            var saveCatch = document.getElementById('<%= btnSaveCatchAll.ClientID %>');
            if (ddlCatch && saveCatch && !ddlCatch.getAttribute('data-postal-dirty-wired')) {
                ddlCatch.setAttribute('data-postal-dirty-wired', '1');
                ddlCatch.addEventListener('change', function () { postalSetupSetSaveReady(saveCatch, true); });
            }
        };
        if (document.readyState === 'loading')
            document.addEventListener('DOMContentLoaded', postalSetupWireDirty);
        else
            postalSetupWireDirty();
    </script>
</asp:Content>

<asp:Content ID="cntPostalAreaBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="smPostalArea" runat="server" />

    <asp:Panel ID="pnlAccessDenied" runat="server" Visible="false" CssClass="status-message status-error">
        <asp:Label ID="lblAccessDenied" runat="server" />
    </asp:Panel>

    <asp:Panel ID="pnlMain" runat="server" CssClass="simpleForm page-tone-panel page-tone-tools">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/Toolbox.png" alt="" />
            <div>
                <h1 class="page-tone-title">Postal Area Setup</h1>
                <p class="page-tone-subtitle">
                    Map SA postcodes and delivery people to Tracker areas (vehicle in greater Cape Town;
                    courier / Pargo / FastWay / RegionalSA elsewhere). Used by Woo and contacts.
                </p>
            </div>
        </div>

        <asp:Label ID="lblMessage" runat="server" CssClass="status-message" Visible="false" />

        <div class="complex-form-section">
            <h3>1. Reference data</h3>
            <p class="woo-map-section-note">
                Source: <code>Data/geo-south-africa-postal_EN.csv</code> → <strong>SaPostalCodeTbl</strong>.
                Presets: <code>Data/area-place-presets.json</code>.
                <asp:Literal ID="litRowCount" runat="server" />
            </p>
            <div class="button-row">
                <asp:Button ID="btnEnsureSchema" runat="server" CssClass="filter-panel-btn"
                    OnClick="btnEnsureSchema_Click" CausesValidation="false" Text="Ensure schema" />
                <asp:Button ID="btnImportCsv" runat="server" CssClass="filter-panel-btn"
                    OnClick="btnImportCsv_Click" CausesValidation="false" Text="Import CSV" />
            </div>
        </div>

        <div class="complex-form-section">
            <h3>2. Catch-all area (unmapped postcodes)</h3>
            <p class="woo-map-section-note">
                Blank postcode ranges on an area are fine — that area simply claims no codes.
                When a postcode is not in any area’s ranges, resolve to this catch-all
                (typically <strong>Regional</strong> / RegionalSA for courier).
            </p>
            <div class="filter-toolbar">
                <div class="filter-section search-controls">
                    <div class="filter-control">
                        <asp:Label ID="lblCatchAll" runat="server" AssociatedControlID="ddlCatchAllArea"
                            Text="Catch-all area:" />
                        <asp:DropDownList ID="ddlCatchAllArea" runat="server" CssClass="sys-prefs-input" />
                    </div>
                    <asp:Button ID="btnSaveCatchAll" runat="server" CssClass="filter-panel-btn"
                        OnClick="btnSaveCatchAll_Click" CausesValidation="false" Text="Save catch-all"
                        OnClientClick="return postalSetupMarkSaving(this);" data-woo-save-ready="0" />
                </div>
            </div>
        </div>

        <div class="complex-form-section">
            <h3>3. Tracker areas — postcodes &amp; who delivers</h3>
            <p class="woo-map-section-note">
                Edit ranges (<code>7806</code> or <code>7800...7806</code>, separate with <code>;</code> —
                <code>..</code> or spaces around the dots are fine; saved as Woo-style <code>...</code>).
                Suggested ranges come from place-name presets.
            </p>
            <p class="woo-map-section-note woo-map-system-default"><asp:Literal ID="litPersonHint" runat="server" /></p>
            <div class="filter-toolbar">
                <div class="filter-section search-controls">
                    <div class="filter-control">
                        <asp:Label ID="lblAreaFilter" runat="server" AssociatedControlID="txtAreaFilter"
                            Text="Filter areas:" />
                        <asp:TextBox ID="txtAreaFilter" runat="server" CssClass="sys-prefs-input"
                            placeholder="e.g. Gauteng, Regional" />
                    </div>
                    <asp:Button ID="btnAreaFilter" runat="server" CssClass="filter-panel-btn"
                        OnClick="btnAreaFilter_Click" CausesValidation="false" Text="Search" />
                    <asp:Button ID="btnAreaFilterClear" runat="server" CssClass="filter-panel-btn"
                        OnClick="btnAreaFilterClear_Click" CausesValidation="false" Text="Clear" />
                </div>
            </div>
            <div class="button-row">
                <asp:Button ID="btnSaveAreas" runat="server" CssClass="filter-panel-btn"
                    OnClick="btnSaveAreas_Click" CausesValidation="false" Text="Save areas"
                    OnClientClick="return postalSetupMarkSaving(this);" data-woo-save-ready="0" />
                <asp:Button ID="btnFillSuggestions" runat="server" CssClass="filter-panel-btn"
                    OnClick="btnFillSuggestions_Click" CausesValidation="false"
                    Text="Fill empty from suggestions"
                    OnClientClick="if (!confirm('Fill blank postcode cells from suggestions (does not overwrite existing ranges)?')) return false; return postalSetupMarkSaving(this);" />
                <asp:Button ID="btnRefreshGrid" runat="server" CssClass="filter-panel-btn"
                    OnClick="btnRefreshGrid_Click" CausesValidation="false" Text="Refresh" />
            </div>
            <asp:GridView ID="gvAreas" runat="server" CssClass="results-table woo-map-area-defaults-grid postal-area-setup-grid"
                AutoGenerateColumns="false" DataKeyNames="AreaID"
                AllowPaging="true" PageSize="15"
                OnPageIndexChanging="gvAreas_PageIndexChanging"
                OnRowCreated="gvAreas_RowCreated"
                OnRowDataBound="gvAreas_RowDataBound"
                OnRowCommand="gvAreas_RowCommand"
                EmptyDataText="No Tracker areas found.">
                <PagerStyle CssClass="pager-row" />
                <PagerTemplate>
                    <asp:PlaceHolder ID="plhPager" runat="server" />
                </PagerTemplate>
                <Columns>
                    <asp:BoundField DataField="AreaName" HeaderText="Area" ReadOnly="true"
                        ItemStyle-CssClass="woo-map-area-name-col" />
                    <asp:BoundField DataField="DispatchHint" HeaderText="Dispatch" ReadOnly="true"
                        ItemStyle-CssClass="col-tight" />
                    <asp:TemplateField HeaderText="Delivers">
                        <ItemTemplate>
                            <asp:DropDownList ID="ddlPerson" runat="server" CssClass="sys-prefs-input" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Postcodes" ItemStyle-CssClass="postal-ranges-cell">
                        <ItemTemplate>
                            <asp:TextBox ID="txtRanges" runat="server" CssClass="sys-prefs-input woo-map-postal-ranges"
                                TextMode="MultiLine" Rows="3"
                                Text='<%# Eval("PostalRanges") %>'
                                ToolTip="7806 or 7800...7806 — separate with ;" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Suggested">
                        <ItemTemplate>
                            <asp:Literal ID="litSuggested" runat="server" />
                            <asp:LinkButton ID="btnUseSuggested" runat="server" CssClass="filter-panel-btn"
                                CommandName="UseSuggested" CommandArgument='<%# Eval("AreaID") %>'
                                CausesValidation="false" Text="Use" Visible="false" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="Status" HeaderText="Status" ReadOnly="true"
                        ItemStyle-CssClass="col-tight" />
                </Columns>
            </asp:GridView>
        </div>

        <div class="complex-form-section">
            <h3>4. Scan for gaps / conflicts</h3>
            <p class="woo-map-section-note">
                Compares the SA postcode table to the ranges above.
                <strong>Gaps</strong> are table codes that are not in any area (catch-all leftovers are omitted).
                <strong>Conflicts</strong> can be:
                <em>Shared postcode</em> (keep both — e.g. 7806 is Hout Bay and Constantia; Woo/contact suburb picks the area)
                or <em>Overlap</em> (trim one range). Fish Hoek and Simon’s Town share 7975; if they are separate Tracker areas, suburb chooses.
            </p>
            <div class="button-row">
                <asp:Button ID="btnGapScan" runat="server" CssClass="filter-panel-btn"
                    OnClick="btnGapScan_Click" CausesValidation="false" Text="Scan for gaps / conflicts" />
            </div>
            <asp:Literal ID="litGapSummary" runat="server" />

            <h4>Gaps (not in any area range)</h4>
            <asp:GridView ID="gvGaps" runat="server" CssClass="results-table" AutoGenerateColumns="false"
                DataKeyNames="SuggestedAreaID"
                OnRowCommand="gvGaps_RowCommand"
                EmptyDataText="No gaps, or only catch-all leftovers (those are fine).">
                <Columns>
                    <asp:BoundField DataField="SuggestedAreaName" HeaderText="Suggested area" />
                    <asp:BoundField DataField="CodeCount" HeaderText="Codes" ItemStyle-CssClass="col-tight" />
                    <asp:BoundField DataField="PlaceSummary" HeaderText="Sample places" />
                    <asp:BoundField DataField="Reason" HeaderText="Why" />
                    <asp:BoundField DataField="MatchNote" HeaderText="Note" />
                    <asp:TemplateField HeaderText="Ranges" ItemStyle-CssClass="postal-ranges-cell">
                        <ItemTemplate>
                            <asp:TextBox ID="txtGapRanges" runat="server" CssClass="sys-prefs-input woo-map-postal-ranges"
                                TextMode="MultiLine" Rows="2" ReadOnly="true"
                                Text='<%# Eval("RangeText") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="">
                        <ItemTemplate>
                            <asp:LinkButton ID="btnAssignGap" runat="server" CssClass="filter-panel-btn"
                                CommandName="AssignGap"
                                CommandArgument='<%# Eval("SuggestedAreaID") %>'
                                CausesValidation="false" Text="Assign"
                                OnClientClick="return confirm('Merge these postcodes into this area?');" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>

            <h4>Conflicts (same postcode in more than one area)</h4>
            <asp:GridView ID="gvConflicts" runat="server" CssClass="results-table" AutoGenerateColumns="false"
                EmptyDataText="No overlapping postcodes in the SA table.">
                <Columns>
                    <asp:BoundField DataField="Kind" HeaderText="Kind" />
                    <asp:BoundField DataField="AreaNames" HeaderText="Areas" />
                    <asp:BoundField DataField="CodeCount" HeaderText="Codes" ItemStyle-CssClass="col-tight" />
                    <asp:BoundField DataField="PlaceSummary" HeaderText="Sample places" />
                    <asp:TemplateField HeaderText="Overlapping codes" ItemStyle-CssClass="postal-ranges-cell">
                        <ItemTemplate>
                            <asp:TextBox ID="txtConflictRanges" runat="server" CssClass="sys-prefs-input woo-map-postal-ranges"
                                TextMode="MultiLine" Rows="2" ReadOnly="true"
                                Text='<%# Eval("RangeText") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

        <div class="complex-form-section">
            <h3>5. Search reference</h3>
            <asp:Panel ID="pnlSearch" runat="server" DefaultButton="btnSearch" CssClass="filter-toolbar">
                <div class="filter-section search-controls">
                    <div class="filter-control">
                        <asp:Label ID="lblSearch" runat="server" AssociatedControlID="txtSearch" Text="Postcode or place:" />
                        <asp:TextBox ID="txtSearch" runat="server" CssClass="sys-prefs-input" />
                    </div>
                    <asp:Button ID="btnSearch" runat="server" CssClass="filter-panel-btn"
                        OnClick="btnSearch_Click" CausesValidation="false" Text="Search" />
                </div>
            </asp:Panel>
            <asp:GridView ID="gvSearch" runat="server" CssClass="results-table" AutoGenerateColumns="false"
                EmptyDataText="No rows — import the CSV first, then search.">
                <Columns>
                    <asp:BoundField DataField="PostalCode" HeaderText="Code" DataFormatString="{0:0000}" ItemStyle-CssClass="col-tight" />
                    <asp:BoundField DataField="PlaceName" HeaderText="Place" />
                    <asp:BoundField DataField="Province" HeaderText="Province" />
                    <asp:BoundField DataField="Municipality" HeaderText="Municipality" />
                </Columns>
            </asp:GridView>
        </div>

        <div class="button-row">
            <asp:Button ID="btnBack" runat="server" CssClass="filter-panel-btn"
                PostBackUrl="~/Tools/SystemTools.aspx" Text="Back to System Tools" CausesValidation="false" />
        </div>
    </asp:Panel>
</asp:Content>
