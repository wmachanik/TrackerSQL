<%@ Page Title="WooCommerce Mapping" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="WooCommerceMapping.aspx.cs" Inherits="TrackerSQL.Tools.WooCommerceMapping"
    MaintainScrollPositionOnPostback="true" %>

<asp:Content ID="cntWooMapHdr" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript">
        window.wooMapDirty = false;

        /* Do not HTML-disable save buttons (WebForms will not post). Gate clicks with data-woo-save-ready. Keep the standard green. */
        window.wooMapSetDisabled = function (btn, disabled) {
            if (!btn) return;
            btn.removeAttribute('disabled');
            btn.disabled = false;
            btn.removeAttribute('aria-disabled');
            if (disabled)
                btn.setAttribute('data-woo-save-ready', '0');
            else
                btn.setAttribute('data-woo-save-ready', '1');
        };

        window.wooMapPrepareSave = function () {
            window.wooMapDirty = false;
            var ok = document.getElementById('<%= hdnPullSaveOk.ClientID %>');
            if (ok) ok.value = '0';
            return true;
        };

        window.wooMapControlValue = function (el) {
            if (!el) return '';
            if (el.type === 'checkbox' || el.type === 'radio')
                return el.checked ? '1' : '0';
            return el.value || '';
        };

        window.wooMapOriginalValue = function (el) {
            if (!el) return null;
            if (el.getAttribute('data-original') != null)
                return el.getAttribute('data-original') || '';
            var p = el.parentNode;
            if (p && p.getAttribute && p.getAttribute('data-original') != null)
                return p.getAttribute('data-original') || '';
            return null;
        };

        window.wooMapHasGridDirty = function (grid) {
            if (!grid) return false;
            var els = grid.querySelectorAll('input, select, textarea');
            for (var i = 0; i < els.length; i++) {
                var orig = window.wooMapOriginalValue(els[i]);
                if (orig == null) continue;
                if (window.wooMapControlValue(els[i]) !== orig) return true;
            }
            return false;
        };

        window.wooMapHasVisibleCatDirty = function () {
            return window.wooMapHasGridDirty(document.getElementById('<%= gvCategories.ClientID %>'));
        };

        window.wooMapHasVisibleAttrParentDirty = function () {
            return window.wooMapHasGridDirty(document.getElementById('<%= gvAttrParents.ClientID %>'));
        };

        window.wooMapHasVisibleAttrMapDirty = function () {
            return window.wooMapHasGridDirty(document.getElementById('<%= gvAttr.ClientID %>'));
        };

        window.wooMapHasVisiblePullDirty = function () {
            return window.wooMapHasGridDirty(document.getElementById('<%= gvPull.ClientID %>'));
        };

        window.wooMapHasVisibleMissingDirty = function () {
            return window.wooMapHasGridDirty(document.getElementById('<%= gvMissingSku.ClientID %>'));
        };

        window.wooMapPendingCount = function () {
            var el = document.getElementById('<%= hdnPendingCount.ClientID %>');
            if (!el) return 0;
            var n = parseInt(el.value || '0', 10);
            return isNaN(n) ? 0 : n;
        };

        window.wooMapSnapshotOriginals = function (root) {
            root = root || document;
            var els = root.querySelectorAll('input, select, textarea');
            for (var i = 0; i < els.length; i++) {
                if (window.wooMapOriginalValue(els[i]) == null) continue;
                var val = window.wooMapControlValue(els[i]);
                els[i].setAttribute('data-original', val);
                if (els[i].parentNode && els[i].parentNode.getAttribute
                    && els[i].parentNode.getAttribute('data-original') != null)
                    els[i].parentNode.setAttribute('data-original', val);
            }
            var mode = document.getElementById('<%= ddlCatMode.ClientID %>');
            if (mode)
                mode.setAttribute('data-original', mode.value || '');
        };

        window.wooMapClearDirty = function () {
            window.wooMapDirty = false;
            window.wooMapSnapshotOriginals();
            var ids = [
                '<%= hdnPendingCount.ClientID %>',
                '<%= hdnUnsavedCat.ClientID %>',
                '<%= hdnUnsavedAttrParents.ClientID %>',
                '<%= hdnUnsavedAttrMaps.ClientID %>',
                '<%= hdnUnsavedPull.ClientID %>',
                '<%= hdnUnsavedMissing.ClientID %>',
                '<%= hdnPullDirty.ClientID %>'
            ];
            for (var i = 0; i < ids.length; i++) {
                var el = document.getElementById(ids[i]);
                if (el) el.value = '0';
            }
            var discard = document.getElementById('<%= hdnDiscardLeave.ClientID %>');
            if (discard) discard.value = '1';
            window.wooMapRefreshDirty();
        };

        window.wooMapMarkUnsaved = function (hdnId, visibleDirty) {
            var hdn = document.getElementById(hdnId);
            if (!hdn) return !!visibleDirty;
            hdn.value = visibleDirty ? '1' : '0';
            return visibleDirty;
        };

        window.wooMapRefreshDirty = function () {
            var catGrid = document.getElementById('<%= gvCategories.ClientID %>');
            var catSectionDirty = window.wooMapHasGridDirty(catGrid);
            var mode = document.getElementById('<%= ddlCatMode.ClientID %>');
            if (mode && mode.getAttribute('data-original') != null
                && mode.value !== mode.getAttribute('data-original'))
                catSectionDirty = true;
            var attrParentVisible = window.wooMapHasVisibleAttrParentDirty();
            var attrMapVisible = window.wooMapHasVisibleAttrMapDirty();
            var pullVisible = window.wooMapHasVisiblePullDirty();
            var pullEdits = document.getElementById('<%= hdnPullDirty.ClientID %>');
            if (pullEdits && pullEdits.value === '1')
                pullVisible = true;
            var missingVisible = window.wooMapHasVisibleMissingDirty();

            window.wooMapMarkUnsaved('<%= hdnUnsavedCat.ClientID %>', catSectionDirty);
            window.wooMapMarkUnsaved('<%= hdnUnsavedAttrParents.ClientID %>', attrParentVisible);
            window.wooMapMarkUnsaved('<%= hdnUnsavedAttrMaps.ClientID %>', attrMapVisible);
            window.wooMapMarkUnsaved('<%= hdnUnsavedPull.ClientID %>', pullVisible);
            window.wooMapMarkUnsaved('<%= hdnUnsavedMissing.ClientID %>', missingVisible);
            window.wooMapDirty = catSectionDirty || attrParentVisible || attrMapVisible || pullVisible || missingVisible;

            window.wooMapSetDisabled(document.getElementById('<%= btnSaveCatIncludes.ClientID %>'), !catSectionDirty);
            window.wooMapSetDisabled(document.getElementById('<%= btnSaveAttrParents.ClientID %>'), !attrParentVisible);
            window.wooMapSetDisabled(document.getElementById('<%= btnSaveAttrMaps.ClientID %>'), !attrMapVisible);
            window.wooMapSetDisabled(document.getElementById('<%= btnSaveSelectedMaps.ClientID %>'), !pullVisible);
            window.wooMapSetDisabled(document.getElementById('<%= btnWriteMissingSkus.ClientID %>'), !missingVisible);
        };

        window.wooMapHasApplyChecked = function (gridId) {
            var grid = document.getElementById(gridId);
            if (!grid) return false;
            var checks = grid.querySelectorAll('input[type="checkbox"]');
            for (var i = 0; i < checks.length; i++) {
                var id = checks[i].id || '';
                if (id.indexOf('chkApply') >= 0 || id.indexOf('chkMissingApply') >= 0) {
                    if (checks[i].checked) return true;
                }
            }
            return false;
        };

        window.wooMapTickApplyInRow = function (el) {
            if (!el) {
                window.wooMapRefreshDirty();
                return;
            }
            var tr = el.closest ? el.closest('tr') : null;
            if (!tr) {
                var p = el.parentNode;
                while (p && p.tagName !== 'TR') p = p.parentNode;
                tr = p;
            }
            if (tr) {
                var marks = tr.querySelectorAll('span.woo-map-saved-yes, span.woo-map-saved-dirty, span.woo-map-saved-no');
                for (var i = 0; i < marks.length; i++) {
                    var sp = marks[i];
                    // Only flip a clean ✓ to ✎ — leave — (never saved) alone.
                    if (sp.className.indexOf('woo-map-saved-yes') >= 0
                        || (sp.className.indexOf('woo-map-saved-dirty') < 0
                            && sp.className.indexOf('woo-map-saved-no') < 0
                            && (sp.textContent || '').indexOf('✓') >= 0)) {
                        sp.className = 'woo-map-saved-dirty';
                        sp.textContent = '✎';
                        sp.title = 'Rule was applied — you have unsaved edits';
                    }
                    break;
                }
            }
            var pullDirty = document.getElementById('<%= hdnPullDirty.ClientID %>');
            if (pullDirty) pullDirty.value = '1';
            window.wooMapRefreshDirty();
        };

        window.wooMapRefreshApply = function () { window.wooMapRefreshDirty(); };
        window.wooMapRefreshMissingApply = function () { window.wooMapRefreshDirty(); };

        window.wooMapConfirmLeave = function () {
            window.wooMapRefreshDirty();
            if (!window.wooMapDirty) return true;
            var msgEl = document.getElementById('<%= hdnUnsavedLeave.ClientID %>');
            var msg = (msgEl && msgEl.value) ? msgEl.value : 'You have unsaved changes. Leave anyway?';
            if (!window.confirm(msg)) return false;
            window.wooMapClearDirty();
            return true;
        };

        window.wooMapConfirmPullCats = function () {
            if (!window.wooMapConfirmLeave()) return false;
            var msgEl = document.getElementById('<%= hdnPullCatsConfirm.ClientID %>');
            var msg = (msgEl && msgEl.value) ? msgEl.value
                : 'This will refresh category names and parents from Woo. Include ticks on existing categories are kept. Continue?';
            return window.confirm(msg);
        };

        /* ---- Busy progress (elapsed / ETA / bar) for Woo pulls & syncs ---- */
        window.wooMapBusy = {
            timer: null,
            startedAt: 0,
            opKey: 'default',
            estimateMs: 60000,
            defaults: {
                cats: 20000,
                attrParents: 45000,
                attrOptions: 30000,
                products: 180000,
                dryPush: 60000,
                push: 90000,
                local: 2500,
                default: 60000
            },
            storageKey: function (op) { return 'wooMap.estMs.' + (op || 'default'); },
            getEstimate: function (op) {
                try {
                    var raw = window.localStorage.getItem(this.storageKey(op));
                    var n = parseInt(raw || '0', 10);
                    if (!isNaN(n) && n >= 3000) return n;
                } catch (ex) { }
                return this.defaults[op] || this.defaults.default;
            },
            saveEstimate: function (op, ms) {
                if (!ms || ms < 1000) return;
                var prev = this.getEstimate(op);
                var blended = prev ? Math.round((prev * 0.4) + (ms * 0.6)) : ms;
                try { window.localStorage.setItem(this.storageKey(op), String(blended)); } catch (ex) { }
            },
            formatMs: function (ms) {
                if (ms < 0) ms = 0;
                var s = Math.round(ms / 1000);
                var m = Math.floor(s / 60);
                s = s % 60;
                return m + ':' + (s < 10 ? '0' : '') + s;
            },
            resolveOp: function (el) {
                if (!el) return 'local';
                var op = el.getAttribute('data-woo-op');
                if (op) return op;
                if (el.id) {
                    if (el.id.indexOf('btnPullCats') >= 0) return 'cats';
                    if (el.id.indexOf('btnPullAttrParents') >= 0) return 'attrParents';
                    if (el.id.indexOf('btnPullAttributes') >= 0) return 'attrOptions';
                    if (el.id.indexOf('btnPullProducts') >= 0) return 'products';
                    if (el.id.indexOf('btnResetCatalog') >= 0) return 'local';
                    if (el.id.indexOf('btnDryPush') >= 0) return 'dryPush';
                    if (el.id.indexOf('btnPushEnabled') >= 0) return 'push';
                    if (el.id.indexOf('btnTab') >= 0) return 'local';
                }
                return 'local';
            },
            isQuiet: function (op) {
                return !op || op === 'local' || op === 'default';
            },
            tick: function () {
                var root = document.querySelector('.woo-map-busy');
                if (!root) return;
                var elapsed = Date.now() - this.startedAt;
                var pct = Math.min(95, Math.floor((elapsed / this.estimateMs) * 100));
                var remain = Math.max(0, this.estimateMs - elapsed);
                var fill = root.querySelector('.woo-map-busy-fill');
                var elElapsed = root.querySelector('.woo-map-busy-elapsed');
                var elPct = root.querySelector('.woo-map-busy-pct');
                var elEta = root.querySelector('.woo-map-busy-eta');
                var elLabel = root.querySelector('.woo-map-busy-label');
                if (fill) fill.style.width = pct + '%';
                if (elElapsed) elElapsed.textContent = 'Elapsed ' + this.formatMs(elapsed);
                if (elPct) elPct.textContent = pct + '%';
                if (elEta) {
                    if (remain > 0) {
                        elEta.textContent = 'Est. ' + this.formatMs(this.estimateMs)
                            + ' total / ~' + this.formatMs(remain) + ' left';
                    } else {
                        elEta.textContent = 'Almost done...';
                    }
                }
                if (elLabel) {
                    elLabel.textContent = (this.opKey === 'local')
                        ? 'Loading saved data...'
                        : 'Talking to WooCommerce...';
                }
            },
            start: function (op) {
                this.stop(false);
                this.opKey = op || 'local';
                this.estimateMs = this.getEstimate(this.opKey);
                this.startedAt = Date.now();
                var self = this;
                if (this.timer) window.clearInterval(this.timer);
                this.timer = window.setInterval(function () { self.tick(); }, 250);
                this.tick();
                return true;
            },
            stop: function (save) {
                if (this.timer) {
                    window.clearInterval(this.timer);
                    this.timer = null;
                }
                if (save !== false && this.startedAt > 0) {
                    this.saveEstimate(this.opKey, Date.now() - this.startedAt);
                }
                this.startedAt = 0;
            }
        };

        document.addEventListener('click', function (e) {
            var n = e.target;
            while (n && n !== document) {
                if (n.classList && n.classList.contains('pager-btn')) {
                    window._wooMapPaging = true;
                    break;
                }
                n = n.parentNode;
            }
        }, true);

        window.addEventListener('beforeunload', function (e) {
            if (window._wooMapPaging) {
                window._wooMapPaging = false;
                return;
            }
            window.wooMapRefreshDirty();
            if (!window.wooMapDirty) return;
            e.preventDefault();
            e.returnValue = '';
        });

        document.addEventListener('change', function (e) {
            var t = e.target;
            if (!t) return;
            var id = t.id || '';
            if (id.indexOf('chkApply') < 0 && id.indexOf('chkMissingApply') < 0
                && (id.indexOf('ddlItem') >= 0 || id.indexOf('ddlImportMode') >= 0
                    || id.indexOf('txtItemSku') >= 0 || id.indexOf('ddlSortOrder') >= 0
                    || id.indexOf('txtQty') >= 0 || id.indexOf('ddlPack') >= 0
                    || id.indexOf('chkImport') >= 0
                    || id.indexOf('ddlMissing') >= 0 || id.indexOf('txtMissing') >= 0))
                window.wooMapTickApplyInRow(t);
            if (window.wooMapOriginalValue(t) == null) return;
            window.wooMapRefreshDirty();
        }, false);
        document.addEventListener('input', function (e) {
            var t = e.target;
            if (!t) return;
            var id = t.id || '';
            if (id.indexOf('txtItemSku') >= 0 || id.indexOf('txtQty') >= 0
                || id.indexOf('txtMissing') >= 0)
                window.wooMapTickApplyInRow(t);
            if (window.wooMapOriginalValue(t) == null) return;
            window.wooMapRefreshDirty();
        }, false);
        document.addEventListener('DOMContentLoaded', function () {
            window.wooMapSnapshotOriginals();
            window.wooMapRefreshDirty();
        });

        /* ScriptManager loads after head scripts — attach when Sys is ready. */
        window.wooMapAttachBusyHandlers = function () {
            if (window._wooMapBusyHandlersAttached) return true;
            if (!window.Sys || !Sys.WebForms || !Sys.WebForms.PageRequestManager) return false;
            var prm = Sys.WebForms.PageRequestManager.getInstance();
            window._wooMapBusyHandlersAttached = true;
            prm.add_beginRequest(function (sender, args) {
                var el = args.get_postBackElement();
                var op = window.wooMapBusy.resolveOp(el);
                window._wooMapRestore = {
                    y: window.pageYOffset || (document.documentElement && document.documentElement.scrollTop) || 0,
                    id: el && el.id ? el.id : '',
                    savedMaps: !!(el && el.id && el.id.indexOf('btnSaveSelectedMaps') >= 0)
                };
                if (window.wooMapBusy.isQuiet(op)) {
                    if (document.body)
                        document.body.className += (document.body.className ? ' ' : '') + 'woo-map-quiet-postback';
                    return;
                }
                window.wooMapBusy.start(op);
            });
            prm.add_endRequest(function () {
                window.wooMapBusy.stop(true);
                if (document.body)
                    document.body.className = (document.body.className || '').replace(/\bwoo-map-quiet-postback\b/g, '').replace(/\s+/g, ' ').trim();
                var r = window._wooMapRestore;
                if (r && r.savedMaps) {
                    var ok = document.getElementById('<%= hdnPullSaveOk.ClientID %>');
                    if (ok && ok.value === '1') {
                        var pullDirty = document.getElementById('<%= hdnPullDirty.ClientID %>');
                        if (pullDirty) pullDirty.value = '0';
                        var unsavedPull = document.getElementById('<%= hdnUnsavedPull.ClientID %>');
                        if (unsavedPull) unsavedPull.value = '0';
                        window.wooMapDirty = false;
                    }
                }
                window.wooMapSnapshotOriginals();
                window.wooMapRefreshDirty();
                window._wooMapRestore = null;
                var restore = function () {
                    if (!r) return;
                    if (typeof window.scrollTo === 'function')
                        window.scrollTo(0, r.y || 0);
                    if (!r.id) return;
                    var focusEl = document.getElementById(r.id);
                    if (!focusEl) return;
                    if (focusEl.scrollIntoView)
                        focusEl.scrollIntoView({ block: 'nearest', inline: 'nearest' });
                    try { if (focusEl.focus) focusEl.focus(); } catch (ex) { }
                };
                restore();
                if (window.requestAnimationFrame)
                    window.requestAnimationFrame(restore);
                window.setTimeout(restore, 50);
            });
            return true;
        };
        (function wooMapWaitForSys() {
            if (window.wooMapAttachBusyHandlers()) return;
            var tries = 0;
            var id = window.setInterval(function () {
                tries++;
                if (window.wooMapAttachBusyHandlers() || tries > 80) window.clearInterval(id);
            }, 50);
            window.addEventListener('load', function () { window.wooMapAttachBusyHandlers(); });
        })();
    </script>
</asp:Content>

<asp:Content ID="cntWooMapBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="smWooMap" runat="server" EnablePartialRendering="true" AsyncPostBackTimeout="600" />

    <asp:UpdateProgress ID="upgWooMap" runat="server" AssociatedUpdatePanelID="upnlWooMap" DisplayAfter="150">
        <ProgressTemplate>
            <div class="woo-map-busy status-message status-info page-tone-progress" role="status" aria-live="polite">
                <div class="woo-map-busy-head">
                    <img src="../images/animi/QuaffeeProgress.gif" alt="" />
                    <span class="woo-map-busy-label">Talking to WooCommerce...</span>
                </div>
                <div class="woo-map-busy-bar" aria-hidden="true">
                    <div class="woo-map-busy-fill"></div>
                </div>
                <div class="woo-map-busy-meta">
                    <span class="woo-map-busy-elapsed">Elapsed 0:00</span>
                    <span class="woo-map-busy-pct">0%</span>
                    <span class="woo-map-busy-eta">Est. timing from last run...</span>
                </div>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:UpdatePanel ID="upnlWooMap" runat="server" UpdateMode="Conditional">
        <Triggers>
            <asp:PostBackTrigger ControlID="btnBack" />
        </Triggers>
        <ContentTemplate>
            <asp:Panel ID="pnlAccessDenied" runat="server" Visible="false" CssClass="simpleForm page-tone-panel page-tone-sysdata">
                <asp:Label ID="lblAccessDenied" runat="server" CssClass="status-message status-error" />
            </asp:Panel>

            <asp:Panel ID="pnlWooDisabled" runat="server" Visible="false" CssClass="simpleForm page-tone-panel page-tone-sysdata">
                <div class="page-tone-header tool-card-header">
                    <img class="tool-card-icon" src="../images/imgButtons/Toolbox.png" alt="" />
                    <div>
                        <h1 class="page-tone-title">WooCommerce Mapping</h1>
                    </div>
                </div>
                <p class="status-message status-info"><asp:Literal ID="litWooDisabled" runat="server" /></p>
                <div class="button-row">
                    <asp:Button ID="btnStartWooWizard" runat="server" CssClass="filter-panel-btn"
                        OnClick="btnStartWooWizard_Click" CausesValidation="false" />
                    <asp:HyperLink ID="hlBackToPrefs" runat="server" NavigateUrl="~/Tools/SystemPreferences.aspx?section=woo"
                        CssClass="filter-panel-btn" Text="System Preferences" />
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlMain" runat="server" CssClass="simpleForm page-tone-panel page-tone-sysdata">
                <div class="page-tone-header tool-card-header">
                    <img class="tool-card-icon" src="../images/imgButtons/Toolbox.png" alt="" />
                    <div>
                        <h1 class="page-tone-title"><asp:Literal ID="litTitle" runat="server" /></h1>
                        <p class="page-tone-subtitle"><asp:Literal ID="litSubtitle" runat="server" /></p>
                    </div>
                </div>

                <div class="sys-prefs-tabs-wrap">
                    <nav class="sys-prefs-tabs" aria-label="Mapping sections">
                        <asp:LinkButton ID="btnTabCat" runat="server" CssClass="sys-prefs-tab" OnClick="btnTabCat_Click" CausesValidation="false" OnClientClick="return wooMapConfirmLeave();" />
                        <asp:LinkButton ID="btnTabAttrParents" runat="server" CssClass="sys-prefs-tab" OnClick="btnTabAttrParents_Click" CausesValidation="false" OnClientClick="return wooMapConfirmLeave();" />
                        <asp:LinkButton ID="btnTabAttrVariants" runat="server" CssClass="sys-prefs-tab" OnClick="btnTabAttrVariants_Click" CausesValidation="false" OnClientClick="return wooMapConfirmLeave();" />
                        <asp:LinkButton ID="btnTabMap" runat="server" CssClass="sys-prefs-tab" OnClick="btnTabMap_Click" CausesValidation="false" OnClientClick="return wooMapConfirmLeave();" />
                        <asp:LinkButton ID="btnTabSavedMaps" runat="server" CssClass="sys-prefs-tab" OnClick="btnTabSavedMaps_Click" CausesValidation="false" OnClientClick="return wooMapConfirmLeave();" />
                        <asp:LinkButton ID="btnTabMissingSku" runat="server" CssClass="sys-prefs-tab" OnClick="btnTabMissingSku_Click" CausesValidation="false" OnClientClick="return wooMapConfirmLeave();" />
                        <asp:LinkButton ID="btnTabSync" runat="server" CssClass="sys-prefs-tab" OnClick="btnTabSync_Click" CausesValidation="false" OnClientClick="return wooMapConfirmLeave();" />
                    </nav>
                </div>

                <asp:MultiView ID="mvTabs" runat="server" ActiveViewIndex="0">
                    <asp:View ID="viewCat" runat="server">
                        <p><asp:Literal ID="litCatHelp" runat="server" /></p>
                        <div class="button-row woo-map-cat-toolbar">
                            <asp:Literal ID="litCatModeLbl" runat="server" />
                            <asp:DropDownList ID="ddlCatMode" runat="server" onchange="wooMapRefreshDirty();">
                                <asp:ListItem Value="All" Text="All categories" />
                                <asp:ListItem Value="IncludeList" Text="Included only" />
                            </asp:DropDownList>
                            <asp:Button ID="btnSaveCatIncludes" runat="server" CssClass="filter-panel-btn" OnClick="btnSaveCatIncludes_Click" CausesValidation="false" UseSubmitBehavior="true" OnClientClick="return wooMapPrepareSave();" data-woo-save-ready="0" />
                            <asp:Button ID="btnPullCats" runat="server" CssClass="filter-panel-btn" OnClick="btnPullCats_Click" CausesValidation="false" OnClientClick="if (!wooMapConfirmPullCats()) return false; return wooMapBusy.start('cats');" data-woo-op="cats" />
                        </div>
                        <asp:Literal ID="litCatPageInfo" runat="server" />
                        <asp:GridView ID="gvCategories" runat="server" CssClass="results-table woo-map-cat-grid" AutoGenerateColumns="false"
                            DataKeyNames="FilterID" AllowPaging="true" PageSize="25"
                            OnPageIndexChanging="gvCategories_PageIndexChanging"
                            OnRowCreated="gvCategories_RowCreated"
                            OnRowDataBound="gvCategories_RowDataBound">
                            <PagerStyle CssClass="pager-row" />
                            <PagerTemplate>
                                <asp:PlaceHolder ID="plhPager" runat="server" />
                            </PagerTemplate>
                            <Columns>
                                <asp:BoundField DataField="WooCategoryId" HeaderText="Woo ID" />
                                <asp:TemplateField HeaderText="Category">
                                    <ItemTemplate>
                                        <span class='<%# "woo-cat-name woo-cat-depth-" + Convert.ToInt32(Eval("Depth")) %>'>
                                            <%# Server.HtmlEncode(Convert.ToString(Eval("CategoryName"))) %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="S/O">
                                    <ItemTemplate>
                                        <asp:DropDownList ID="ddlCatSort" runat="server" CssClass="lookups-item-so woo-map-so"
                                            ToolTip="Default Tracker sort order for new items. (parent) uses the parent category." />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Import">
                                    <ItemTemplate>
                                        <asp:DropDownList ID="ddlCatImportMode" runat="server" CssClass="sys-prefs-input woo-map-cat-import"
                                            ToolTip="Default import mode for unmapped products. (inherit) = use parent category. Not the same as Parent → order notes — set that on Gear (or another root) so children inherit it." />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Include">
                                    <ItemTemplate>
                                        <asp:CheckBox ID="chkInclude" runat="server"
                                            Checked='<%# Eval("IncludeInSync") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </asp:View>

                    <asp:View ID="viewAttrParents" runat="server">
                        <p><asp:Literal ID="litAttrParentsHelp" runat="server" /></p>
                        <div class="button-row woo-map-attr-toolbar">
                            <asp:Button ID="btnPullAttrParents" runat="server" CssClass="filter-panel-btn" OnClick="btnPullAttrParents_Click" CausesValidation="false" OnClientClick="return wooMapBusy.start('attrParents');" data-woo-op="attrParents" />
                            <asp:Button ID="btnSaveAttrParents" runat="server" CssClass="filter-panel-btn" OnClick="btnSaveAttrParents_Click" CausesValidation="false" UseSubmitBehavior="true" OnClientClick="return wooMapPrepareSave();" data-woo-save-ready="0" />
                        </div>
                        <asp:GridView ID="gvAttrParents" runat="server" CssClass="results-table woo-map-attr-parents-grid" AutoGenerateColumns="false"
                            DataKeyNames="ParentID" OnRowDataBound="gvAttrParents_RowDataBound">
                            <Columns>
                                <asp:BoundField DataField="AttributeName" HeaderText="Attribute" />
                                <asp:BoundField DataField="TermCount" HeaderText="Options" />
                                <asp:TemplateField HeaderText="Priority">
                                    <ItemTemplate>
                                        <asp:DropDownList ID="ddlAttrPriority" runat="server" CssClass="sys-prefs-input">
                                            <asp:ListItem Value="10" Text="Highest" />
                                            <asp:ListItem Value="25" Text="High" />
                                            <asp:ListItem Value="50" Text="Average" />
                                            <asp:ListItem Value="75" Text="Low" />
                                            <asp:ListItem Value="100" Text="Lowest" />
                                        </asp:DropDownList>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Use for variants">
                                    <ItemTemplate>
                                        <asp:CheckBox ID="chkUseForVariants" runat="server"
                                            Checked='<%# Eval("UseForVariants") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </asp:View>

                    <asp:View ID="viewAttrVariants" runat="server">
                        <asp:Panel ID="pnlAttrVariantsLocked" runat="server" Visible="false" CssClass="status-message status-info">
                            <asp:Literal ID="litAttrVariantsLocked" runat="server" />
                        </asp:Panel>
                        <asp:Panel ID="pnlAttrVariants" runat="server" CssClass="woo-map-attr-variants">
                            <p><asp:Literal ID="litAttrOptionsHelp" runat="server" /></p>
                            <div class="button-row">
                                <asp:Button ID="btnPullAttributes" runat="server" CssClass="filter-panel-btn" OnClick="btnPullAttributes_Click" CausesValidation="false" OnClientClick="return wooMapBusy.start('attrOptions');" data-woo-op="attrOptions" />
                                <asp:Button ID="btnSaveAttrMaps" runat="server" CssClass="filter-panel-btn" OnClick="btnSaveAttrMaps_Click" CausesValidation="false" UseSubmitBehavior="true" OnClientClick="return wooMapPrepareSave();" data-woo-save-ready="0" />
                            </div>
                            <asp:GridView ID="gvAttr" runat="server" CssClass="results-table" AutoGenerateColumns="false"
                                DataKeyNames="RowKey" AllowPaging="true" PageSize="25"
                                OnPageIndexChanging="gvAttr_PageIndexChanging"
                                OnRowCreated="gvAttr_RowCreated"
                                OnRowDataBound="gvAttr_RowDataBound">
                                <PagerStyle CssClass="pager-row" />
                                <PagerTemplate>
                                    <asp:PlaceHolder ID="plhPager" runat="server" />
                                </PagerTemplate>
                                <Columns>
                                    <asp:BoundField DataField="AttributeName" HeaderText="Attribute" />
                                    <asp:BoundField DataField="AttributeOption" HeaderText="Option" />
                                    <asp:BoundField DataField="SampleCount" HeaderText="Used on" />
                                    <asp:TemplateField HeaderText="Role">
                                        <ItemTemplate>
                                            <asp:DropDownList ID="ddlAttrRole" runat="server" CssClass="sys-prefs-input">
                                                <asp:ListItem Value="Both" Text="Both" />
                                                <asp:ListItem Value="QtyOnly" Text="Qty only" />
                                                <asp:ListItem Value="PackagingOnly" Text="Packaging only" />
                                            </asp:DropDownList>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Qty factor">
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtAttrQty" runat="server" Text='<%# Eval("QtyFactor") %>' Width="60px" />
                                            <asp:HiddenField ID="hdnAttrMapId" runat="server" Value='<%# Eval("MapID") %>' />
                                            <asp:HiddenField ID="hdnAttrName" runat="server" Value='<%# Eval("AttributeName") %>' />
                                            <asp:HiddenField ID="hdnAttrOption" runat="server" Value='<%# Eval("AttributeOption") %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Packaging">
                                        <ItemTemplate>
                                            <asp:DropDownList ID="ddlAttrPack" runat="server" CssClass="sys-prefs-input" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Service type">
                                        <ItemTemplate>
                                            <asp:DropDownList ID="ddlAttrSvc" runat="server" CssClass="sys-prefs-input" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </asp:Panel>
                    </asp:View>

                    <asp:View ID="viewMap" runat="server">
                        <p><asp:Literal ID="litMapHelp" runat="server" /></p>
                        <div class="button-row">
                            <asp:Button ID="btnPullProducts" runat="server" CssClass="filter-panel-btn" OnClick="btnPullProducts_Click" CausesValidation="false" OnClientClick="return wooMapBusy.start('products');" data-woo-op="products" />
                            <asp:Button ID="btnSaveSelectedMaps" runat="server" CssClass="filter-panel-btn" OnClick="btnSaveSelectedMaps_Click" CausesValidation="false" UseSubmitBehavior="true" OnClientClick="return wooMapPrepareSave();" data-woo-save-ready="0" />
                            <asp:Button ID="btnResetCatalog" runat="server" CssClass="filter-panel-btn" OnClick="btnResetCatalog_Click" CausesValidation="false"
                                OnClientClick="return confirm('Clear the saved Woo catalog? Mappings you already saved are kept. Click Sync products afterwards to download again.');" />
                            <asp:Button ID="btnExpandAllGroups" runat="server" CssClass="filter-panel-btn" OnClick="btnExpandAllGroups_Click" CausesValidation="false" />
                            <asp:Button ID="btnCollapseAllGroups" runat="server" CssClass="filter-panel-btn" OnClick="btnCollapseAllGroups_Click" CausesValidation="false" />
                        </div>
                        <asp:Panel ID="pnlPullSearch" runat="server" DefaultButton="btnFindSku" CssClass="filter-toolbar">
                            <div class="filter-section search-controls">
                                <div class="filter-control">
                                    <asp:Label ID="lblFindSku" runat="server" AssociatedControlID="txtFindSku" />
                                    <asp:TextBox ID="txtFindSku" runat="server"
                                        ToolTip="search SKU, parent SKU, name, or variant" />
                                </div>
                                <asp:Button ID="btnFindSku" runat="server" CssClass="filter-panel-btn"
                                    OnClick="btnFindSku_Click" CausesValidation="false" ToolTip="search products" />
                                <asp:Button ID="btnClearFindSku" runat="server" CssClass="filter-panel-btn"
                                    OnClick="btnClearFindSku_Click" CausesValidation="false" ToolTip="Clear search" />
                            </div>
                        </asp:Panel>
                        <asp:Literal ID="litPullPageInfo" runat="server" />
                        <asp:GridView ID="gvPull" runat="server" CssClass="results-table woo-map-pull-grid" AutoGenerateColumns="false"
                            DataKeyNames="RowKey" AllowPaging="true" PageSize="40"
                            OnPageIndexChanging="gvPull_PageIndexChanging"
                            OnRowCreated="gvPull_RowCreated"
                            OnRowCommand="gvPull_RowCommand"
                            OnRowDataBound="gvPull_RowDataBound">
                            <PagerStyle CssClass="pager-row" />
                            <PagerTemplate>
                                <asp:PlaceHolder ID="plhPager" runat="server" />
                            </PagerTemplate>
                            <Columns>
                                <asp:TemplateField HeaderText="Applied" ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="3.5em"
                                    ItemStyle-CssClass="woo-map-saved-col" HeaderStyle-CssClass="woo-map-saved-col">
                                    <ItemTemplate>
                                        <span class='<%# Eval("SavedFlagCss") %>' title='<%# Eval("SavedFlagTitle") %>'>
                                            <%# Eval("SavedFlagLabel") %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Import" ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="3.5em">
                                    <ItemTemplate>
                                        <asp:CheckBox ID="chkImport" runat="server" Checked='<%# Eval("ShowIncludeInImport") %>'
                                            Enabled="true"
                                            OnCheckedChanged="chkImport_CheckedChanged" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="SKU">
                                    <ItemTemplate>
                                        <span class='<%# Eval("SkuCssClass") %>'>
                                            <asp:LinkButton ID="btnToggleGroup" runat="server" CssClass="woo-map-toggle"
                                                Visible='<%# Eval("IsParentGroup") %>'
                                                CommandName="ToggleGroup"
                                                CommandArgument='<%# Eval("WooProductId") %>'
                                                CausesValidation="false"
                                                Text='<%# (bool)Eval("GroupExpanded") ? "-" : "+" %>'
                                                ToolTip='<%# (bool)Eval("GroupExpanded") ? "Hide variants" : "Show variants" %>' />
                                            <asp:Literal ID="litSkuBadge" runat="server" Visible='<%# Eval("IsVariation") %>' Text="var " />
                                            <asp:Literal ID="litParentBadge" runat="server" Visible='<%# Eval("IsParentGroup") %>' Text="group " />
                                            <%# Server.HtmlEncode(Convert.ToString(Eval("ListSku"))) %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Name">
                                    <ItemTemplate>
                                        <span class='<%# Eval("NameCssClass") %>'>
                                            <%# Server.HtmlEncode(Convert.ToString(Eval("ListName"))) %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="AttributesLabel" HeaderText="Variant" ItemStyle-CssClass="woo-map-variant-col" />
                                <asp:TemplateField HeaderText="Mode">
                                    <ItemTemplate>
                                        <asp:DropDownList ID="ddlImportMode" runat="server" CssClass="sys-prefs-input"
                                            Visible='<%# Eval("IsParentGroup") %>'
                                            AutoPostBack="true"
                                            OnSelectedIndexChanged="ddlImportMode_SelectedIndexChanged" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="MatchReason" HeaderText="Match" ItemStyle-CssClass="woo-map-match-col" />
                                <asp:TemplateField HeaderText="Destination">
                                    <ItemTemplate>
                                        <asp:DropDownList ID="ddlItem" runat="server" CssClass="sys-prefs-input woo-map-dest"
                                            Visible='<%# !(bool)Eval("IsParentGroup") || !(bool)Eval("ImportParentExcluded") %>'
                                            OnSelectedIndexChanged="ddlItem_SelectedIndexChanged" />
                                        <asp:Literal ID="litParentDest" runat="server"
                                            Visible='<%# (bool)Eval("IsParentGroup") && (bool)Eval("ImportParentExcluded") %>'
                                            Text='<%# Eval("ParentDestPlaceholder") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Item SKU">
                                    <ItemTemplate>
                                        <asp:TextBox ID="txtItemSku" runat="server" Text='<%# Eval("CreateSku") %>'
                                            CssClass="sys-prefs-input" Width="150px"
                                            Visible='<%# Eval("ShowItemCreateFields") %>'
                                            ToolTip="Tracker SKU to create or rename" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="S/O" HeaderStyle-CssClass="woo-map-so-col" ItemStyle-CssClass="woo-map-so-col">
                                    <ItemTemplate>
                                        <asp:DropDownList ID="ddlSortOrder" runat="server" CssClass="lookups-item-so woo-map-so"
                                            Visible='<%# Eval("ShowItemCreateFields") %>'
                                            AutoPostBack="true"
                                            OnSelectedIndexChanged="ddlSortOrder_SelectedIndexChanged"
                                            ToolTip="Tracker item sort order (major category)" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Qty" HeaderStyle-Width="4em">
                                    <ItemTemplate>
                                        <asp:TextBox ID="txtQty" runat="server" Text='<%# Eval("QtyFactor") %>' Width="48px" CssClass="sys-prefs-input"
                                            Visible='<%# !(bool)Eval("IsParentGroup") || (bool)Eval("ImportParentAsItem") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Pack">
                                    <ItemTemplate>
                                        <asp:DropDownList ID="ddlPack" runat="server" CssClass="sys-prefs-input"
                                            Visible='<%# !(bool)Eval("IsParentGroup") || (bool)Eval("ImportParentAsItem") %>'
                                            AutoPostBack="true"
                                            OnSelectedIndexChanged="ddlPack_SelectedIndexChanged" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </asp:View>

                    <asp:View ID="viewSavedMaps" runat="server">
                        <p><asp:Literal ID="litSavedMapsHelp" runat="server" /></p>
                        <asp:Literal ID="litSavedMapsPageInfo" runat="server" />
                        <asp:GridView ID="gvMaps" runat="server" CssClass="results-table woo-map-saved-grid" AutoGenerateColumns="false"
                            DataKeyNames="MappingID" AllowPaging="true" PageSize="40"
                            OnPageIndexChanging="gvMaps_PageIndexChanging"
                            OnRowCreated="gvMaps_RowCreated"
                            OnRowCommand="gvMaps_RowCommand">
                            <PagerStyle CssClass="pager-row" />
                            <PagerTemplate>
                                <asp:PlaceHolder ID="plhPager" runat="server" />
                            </PagerTemplate>
                            <Columns>
                                <asp:BoundField DataField="MappingID" HeaderText="ID" />
                                <asp:BoundField DataField="MapType" HeaderText="Type" />
                                <asp:TemplateField HeaderText="Import">
                                    <ItemTemplate>
                                        <%# (bool)Eval("IncludeInImport") ? "Yes" : "No" %>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="ItemSku" HeaderText="Item SKU" />
                                <asp:BoundField DataField="ItemDesc" HeaderText="Item" />
                                <asp:BoundField DataField="SkuPattern" HeaderText="Woo SKU" />
                                <asp:BoundField DataField="WooProductId" HeaderText="Product" />
                                <asp:BoundField DataField="WooVariationId" HeaderText="Variation" />
                                <asp:BoundField DataField="QtyFactor" HeaderText="Qty" />
                                <asp:BoundField DataField="PackagingID" HeaderText="Pack" />
                                <asp:BoundField DataField="LastWooStatus" HeaderText="Last Woo" />
                                <asp:TemplateField>
                                    <ItemTemplate>
                                        <asp:Button ID="btnDelMap" runat="server" CssClass="filter-panel-btn" CommandName="DelMap"
                                            CommandArgument='<%# Eval("MappingID") %>' Text="Delete"
                                            OnClientClick="return confirm('Delete this mapping?');" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </asp:View>

                    <asp:View ID="viewMissingSku" runat="server">
                        <p><asp:Literal ID="litMissingSkuHelp" runat="server" /></p>
                        <div class="button-row">
                            <asp:Button ID="btnWriteMissingSkus" runat="server" CssClass="filter-panel-btn"
                                OnClick="btnWriteMissingSkus_Click" CausesValidation="false"
                                OnClientClick="wooMapPrepareSave(); return wooMapBusy.start('products');" data-woo-op="products" data-woo-save-ready="0" />
                        </div>
                        <asp:Literal ID="litMissingSkuPageInfo" runat="server" />
                        <asp:GridView ID="gvMissingSku" runat="server" CssClass="results-table woo-map-pull-grid" AutoGenerateColumns="false"
                            DataKeyNames="RowKey" AllowPaging="true" PageSize="25"
                            OnPageIndexChanging="gvMissingSku_PageIndexChanging"
                            OnRowCreated="gvMissingSku_RowCreated"
                            OnRowDataBound="gvMissingSku_RowDataBound">
                            <PagerStyle CssClass="pager-row" />
                            <PagerTemplate>
                                <asp:PlaceHolder ID="plhPager" runat="server" />
                            </PagerTemplate>
                            <Columns>
                                <asp:TemplateField HeaderText="Apply" ItemStyle-HorizontalAlign="Center" HeaderStyle-Width="3.5em">
                                    <ItemTemplate>
                                        <asp:CheckBox ID="chkMissingApply" runat="server" Checked='<%# Eval("ApplySelected") %>'
                                            onchange="wooMapRefreshDirty();" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="DisplayName" HeaderText="Name" />
                                <asp:BoundField DataField="AttributesLabel" HeaderText="Variant" />
                                <asp:BoundField DataField="MatchReason" HeaderText="Why" />
                                <asp:BoundField DataField="Status" HeaderText="Status" />
                                <asp:TemplateField HeaderText="New SKU">
                                    <ItemTemplate>
                                        <asp:TextBox ID="txtNewSku" runat="server" Text='<%# Eval("NewSku") %>' CssClass="sys-prefs-input" Width="140px" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </asp:View>

                    <asp:View ID="viewSync" runat="server">
                        <p><asp:Literal ID="litSyncHelp" runat="server" /></p>
                        <div class="button-row">
                            <asp:Button ID="btnDryPush" runat="server" CssClass="filter-panel-btn" OnClick="btnDryPush_Click" CausesValidation="false" OnClientClick="return wooMapBusy.start('dryPush');" data-woo-op="dryPush" />
                            <asp:Button ID="btnPushEnabled" runat="server" CssClass="filter-panel-btn sys-prefs-enable-btn" OnClick="btnPushEnabled_Click" CausesValidation="false" OnClientClick="return wooMapBusy.start('push');" data-woo-op="push" />
                        </div>
                    </asp:View>
                </asp:MultiView>

                <asp:DropDownList ID="ddlItemLookup" runat="server" Visible="false" />

                <asp:HiddenField ID="hdnUnsavedLeave" runat="server" />
                <asp:HiddenField ID="hdnPullCatsConfirm" runat="server" />
                <asp:HiddenField ID="hdnPendingCount" runat="server" Value="0" />
                <asp:HiddenField ID="hdnUnsavedCat" runat="server" Value="0" />
                <asp:HiddenField ID="hdnUnsavedAttrParents" runat="server" Value="0" />
                <asp:HiddenField ID="hdnUnsavedAttrMaps" runat="server" Value="0" />
                <asp:HiddenField ID="hdnUnsavedPull" runat="server" Value="0" />
                <asp:HiddenField ID="hdnUnsavedMissing" runat="server" Value="0" />
                <asp:HiddenField ID="hdnDiscardLeave" runat="server" Value="0" />
                <asp:HiddenField ID="hdnPullDirty" runat="server" Value="0" />
                <asp:HiddenField ID="hdnPullSaveOk" runat="server" Value="0" />

                <div class="page-tone-footer button-row">
                    <span class="image-button" title="Return to System Preferences">
                        <asp:ImageButton ID="btnBack" runat="server"
                            ImageUrl="~/images/imgButtons/Back.gif"
                            AlternateText="Back"
                            ToolTip="Return to System Preferences"
                            OnClick="btnBack_Click"
                            OnClientClick="return wooMapConfirmLeave();"
                            CausesValidation="false" />
                    </span>
                    <asp:Label ID="lblMessage" runat="server" CssClass="status-message" Visible="false" />
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
