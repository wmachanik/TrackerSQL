<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="ContactPostalFill.aspx.cs" Inherits="TrackerSQL.Tools.ContactPostalFill"
    Title="Contact Postal Fill" MaintainScrollPositionOnPostback="true" %>

<asp:Content ID="cntContactPostalHdr" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        window.postalSetupMarkSaving = function (btn) {
            if (!btn) return true;
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
        window.postalFillTickRow = function (codeBox) {
            if (!codeBox) return;
            var row = codeBox.parentNode;
            while (row && row.tagName !== 'TR')
                row = row.parentNode;
            if (!row) return;
            var boxes = row.getElementsByTagName('input');
            var has = (codeBox.value || '').replace(/\s/g, '').length > 0;
            for (var i = 0; i < boxes.length; i++) {
                if (boxes[i].type === 'checkbox') {
                    boxes[i].checked = has;
                    break;
                }
            }
        };
        window.postalFillBindCodeTicks = function () {
            var grid = document.getElementById('<%= gvSuggestions.ClientID %>');
            if (!grid || grid.getAttribute('data-postal-tick-wired') === '1')
                return;
            grid.setAttribute('data-postal-tick-wired', '1');
            grid.addEventListener('input', function (ev) {
                var t = ev.target;
                if (t && t.id && t.id.indexOf('txtCode') >= 0)
                    postalFillTickRow(t);
            });
            grid.addEventListener('change', function (ev) {
                var t = ev.target;
                if (t && t.id && t.id.indexOf('txtCode') >= 0)
                    postalFillTickRow(t);
            });
        };
        if (document.readyState === 'loading')
            document.addEventListener('DOMContentLoaded', postalFillBindCodeTicks);
        else
            postalFillBindCodeTicks();
    </script>
</asp:Content>

<asp:Content ID="cntContactPostalBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlAccessDenied" runat="server" Visible="false" CssClass="status-message status-error">
        <asp:Label ID="lblAccessDenied" runat="server" />
    </asp:Panel>

    <asp:Panel ID="pnlMain" runat="server" CssClass="simpleForm page-tone-panel page-tone-tools">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/Toolbox.png" alt="" />
            <div>
                <h1 class="page-tone-title">Contact postal fill</h1>
                <p class="page-tone-subtitle">
                    Find contacts with no postal code and suggest one from the SA postcode table
                    (suburb in address) or from the contact’s Tracker area ranges.
                </p>
            </div>
        </div>

        <div class="complex-form-section">
            <p class="woo-map-section-note">
                Requires <strong>SaPostalCodeTbl</strong> (import via Postal Area Setup).
                Skips blank addresses and Collect areas. Rows with a suggested (or typed) postal code are ticked for Apply.
                Review before applying — this writes <code>ContactsTbl.PostalCode</code> only.
            </p>
            <div class="button-row">
                <asp:Button ID="btnScan" runat="server" CssClass="filter-panel-btn"
                    OnClick="btnScan_Click" CausesValidation="false" Text="Scan contacts" />
                <asp:CheckBox ID="chkEnabledOnly" runat="server" Checked="true"
                    Text="Enabled contacts only" CssClass="sys-prefs-check" />
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
                            placeholder="company, suburb, area…" />
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
                <asp:Button ID="btnSelectHigh" runat="server" CssClass="filter-panel-btn"
                    OnClick="btnSelectHigh_Click" CausesValidation="false" Text="Select high confidence" />
                <asp:Button ID="btnClearSelect" runat="server" CssClass="filter-panel-btn"
                    OnClick="btnClearSelect_Click" CausesValidation="false" Text="Clear selection" />
                <asp:Button ID="btnApply" runat="server" CssClass="filter-panel-btn"
                    OnClick="btnApply_Click" CausesValidation="false" Text="Apply selected"
                    OnClientClick="if (!confirm('Write suggested postal codes to the selected contacts?')) return false; return postalSetupMarkSaving(this);" />
            </div>
            <asp:GridView ID="gvSuggestions" runat="server" CssClass="results-table postal-area-setup-grid"
                AutoGenerateColumns="false" DataKeyNames="ContactID"
                AllowPaging="true" PageSize="25"
                OnPageIndexChanging="gvSuggestions_PageIndexChanging"
                OnRowCreated="gvSuggestions_RowCreated"
                OnRowDataBound="gvSuggestions_RowDataBound"
                EmptyDataText="Run Scan contacts to list missing postal codes.">
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
                    <asp:BoundField DataField="CompanyName" HeaderText="Company" />
                    <asp:BoundField DataField="AreaName" HeaderText="Area" />
                    <asp:TemplateField HeaderText="Address">
                        <ItemTemplate>
                            <asp:Label ID="lblAddress" runat="server"
                                Text='<%# Truncate(Eval("BillingAddress") as string, 80) %>'
                                ToolTip='<%# Eval("BillingAddress") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Suggested">
                        <ItemTemplate>
                            <asp:TextBox ID="txtCode" runat="server" CssClass="sys-prefs-input"
                                MaxLength="8" Width="5em"
                                Text='<%# Eval("SuggestedPostalCode") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="MatchPlace" HeaderText="Place" />
                    <asp:BoundField DataField="Confidence" HeaderText="Conf" ItemStyle-CssClass="col-tight" />
                    <asp:BoundField DataField="Reason" HeaderText="Why" />
                </Columns>
            </asp:GridView>
        </div>

        <div class="button-row">
            <asp:Button ID="btnBack" runat="server" CssClass="filter-panel-btn"
                PostBackUrl="~/Tools/SystemTools.aspx" Text="Back to System Tools" CausesValidation="false" />
            <asp:HyperLink ID="lnkPostalSetup" runat="server" NavigateUrl="~/Tools/PostalAreaSetup.aspx"
                CssClass="filter-panel-btn" Text="Postal Area Setup" />
        </div>
    </asp:Panel>
</asp:Content>
