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

        window.wooMapMarkSaving = function (btn) {
            if (!btn) return;
            var cur = (btn.value != null && btn.tagName === 'INPUT')
                ? btn.value
                : (btn.innerText || btn.textContent || '');
            if (!btn.getAttribute('data-woo-save-label'))
                btn.setAttribute('data-woo-save-label', cur);
            if (btn.tagName === 'INPUT')
                btn.value = 'Saving...';
            else
                btn.innerText = 'Saving...';
            btn.setAttribute('data-woo-saving', '1');
        };

        window.wooMapPrepareSave = function (btn) {
            if (btn) {
                if (btn.getAttribute('data-woo-save-ready') === '0')
                    return false;
                if (btn.getAttribute('data-woo-saving') === '1')
                    return false;
            }
            window.wooMapDirty = false;
            var ok = document.getElementById('<%= hdnPullSaveOk.ClientID %>');
            if (ok) ok.value = '0';
            window.wooMapMarkSaving(btn);
            // Keep both product Save buttons in sync when either is clicked.
            if (btn && btn.id && btn.id.indexOf('btnSaveSelectedMaps') >= 0) {
                window.wooMapMarkSaving(document.getElementById('<%= btnSaveSelectedMaps.ClientID %>'));
                window.wooMapMarkSaving(document.getElementById('<%= btnSaveSelectedMapsBottom.ClientID %>'));
            }
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
            var grid = document.getElementById('<%= gvPull.ClientID %>');
            // Server-rendered pencils mean unsaved edits even when data-original was re-snapshotted.
            if (grid && grid.querySelector('.woo-map-saved-dirty'))
                return true;
            return window.wooMapHasGridDirty(grid);
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
            var areaGridDirty = window.wooMapHasGridDirty(document.getElementById('<%= gvAreaDefaults.ClientID %>'));
            var shipGridDirty = window.wooMapHasGridDirty(document.getElementById('<%= gvShippingMaps.ClientID %>'));
            var payGridDirty = window.wooMapHasGridDirty(document.getElementById('<%= gvPaymentMaps.ClientID %>'));
            var defArea = document.getElementById('<%= ddlDefaultImportArea.ClientID %>');
            var defAreaDirty = !!(defArea && defArea.getAttribute('data-original') != null
                && (defArea.value || '') !== (defArea.getAttribute('data-original') || ''));
            var notesItem = document.getElementById('<%= ddlImportNotesItem.ClientID %>');
            var notesItemDirty = !!(notesItem && notesItem.getAttribute('data-original') != null
                && (notesItem.value || '') !== (notesItem.getAttribute('data-original') || ''));

            window.wooMapMarkUnsaved('<%= hdnUnsavedCat.ClientID %>', catSectionDirty);
            window.wooMapMarkUnsaved('<%= hdnUnsavedAttrParents.ClientID %>', attrParentVisible);
            window.wooMapMarkUnsaved('<%= hdnUnsavedAttrMaps.ClientID %>', attrMapVisible);
            window.wooMapMarkUnsaved('<%= hdnUnsavedPull.ClientID %>', pullVisible);
            window.wooMapMarkUnsaved('<%= hdnUnsavedMissing.ClientID %>', missingVisible);
            window.wooMapDirty = catSectionDirty || attrParentVisible || attrMapVisible || pullVisible || missingVisible
                || areaGridDirty || shipGridDirty || payGridDirty || defAreaDirty || notesItemDirty;

            window.wooMapSetDisabled(document.getElementById('<%= btnSaveCatIncludes.ClientID %>'), !catSectionDirty);
            window.wooMapSetDisabled(document.getElementById('<%= btnSaveAttrParents.ClientID %>'), !attrParentVisible);
            window.wooMapSetDisabled(document.getElementById('<%= btnSaveAttrMaps.ClientID %>'), !attrMapVisible);
            window.wooMapSetDisabled(document.getElementById('<%= btnSaveSelectedMaps.ClientID %>'), !pullVisible);
            window.wooMapSetDisabled(document.getElementById('<%= btnSaveSelectedMapsBottom.ClientID %>'), !pullVisible);
            window.wooMapSetDisabled(document.getElementById('<%= btnWriteMissingSkus.ClientID %>'), !missingVisible);
            window.wooMapSetDisabled(document.getElementById('<%= btnSaveAreaDefaults.ClientID %>'), !areaGridDirty);
            window.wooMapSetDisabled(document.getElementById('<%= btnSaveShippingMaps.ClientID %>'), !shipGridDirty);
            window.wooMapSetDisabled(document.getElementById('<%= btnSavePaymentMaps.ClientID %>'), !payGridDirty);
            window.wooMapSetDisabled(document.getElementById('<%= btnSaveDefaultArea.ClientID %>'), !defAreaDirty);
            window.wooMapSetDisabled(document.getElementById('<%= btnSaveImportNotesItem.ClientID %>'), !notesItemDirty);
        };

        window.wooMapMissingSkuTyped = function (el) {
            if (!el) return;
            var tr = el.parentNode;
            while (tr && tr.tagName !== 'TR') tr = tr.parentNode;
            if (tr && (el.value || '').replace(/\s/g, '').length > 0) {
                var checks = tr.querySelectorAll('input[type="checkbox"]');
                for (var i = 0; i < checks.length; i++) {
                    var id = checks[i].id || '';
                    if (id.indexOf('chkMissingApply') >= 0) {
                        checks[i].checked = true;
                        break;
                    }
                }
            }
            window.wooMapRefreshDirty();
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
            // Only mark Applied dirty when the control actually changed from its bound original.
            var orig = window.wooMapOriginalValue(el);
            if (orig != null) {
                var cur;
                if (el.type === 'checkbox')
                    cur = el.checked ? '1' : '0';
                else
                    cur = el.value || '';
                if (cur === orig) {
                    window.wooMapRefreshDirty();
                    return;
                }
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
                    // Only flip a clean check to pencil - leave "-" (never saved) alone.
                    var text = (sp.textContent || '').replace(/\s/g, '');
                    if (sp.className.indexOf('woo-map-saved-yes') >= 0
                        || (sp.className.indexOf('woo-map-saved-dirty') < 0
                            && sp.className.indexOf('woo-map-saved-no') < 0
                            && (text.indexOf('\u2713') >= 0 || text.indexOf('✓') >= 0))) {
                        sp.className = 'woo-map-saved-dirty';
                        sp.textContent = '\u270E';
                        sp.title = 'Rule was applied - you have unsaved edits';
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
                writeSkus: 15000,
                dryPush: 60000,
                push: 90000,
                local: 2500,
                default: 60000
            },
            /* One hung run must not make the next bar sit at 9% for minutes. */
            maxEstimateMs: 12 * 60 * 1000,
            storageKey: function (op) { return 'wooMap.estMs.' + (op || 'default'); },
            getEstimate: function (op) {
                try {
                    var raw = window.localStorage.getItem(this.storageKey(op));
                    var n = parseInt(raw || '0', 10);
                    if (!isNaN(n) && n >= 3000)
                        return Math.min(n, this.maxEstimateMs);
                } catch (ex) { }
                return this.defaults[op] || this.defaults.default;
            },
            saveEstimate: function (op, ms) {
                if (!ms || ms < 1000) return;
                var prev = this.getEstimate(op);
                var blended = prev ? Math.round((prev * 0.4) + (ms * 0.6)) : ms;
                blended = Math.min(blended, this.maxEstimateMs);
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
                    if (el.id.indexOf('btnWriteMissingSkus') >= 0) return 'writeSkus';
                    if (el.id.indexOf('btnDryPush') >= 0) return 'dryPush';
                    if (el.id.indexOf('btnPushEnabled') >= 0) return 'push';
                    if (el.id.indexOf('btnSave') >= 0) return 'save';
                    if (el.id.indexOf('btnTab') >= 0) return 'local';
                }
                return 'local';
            },
            labelFor: function (op) {
                if (op === 'local') return 'Working — please wait...';
                if (op === 'save') return 'Saving...';
                if (op === 'products') return 'Downloading products and variations from Woo...';
                if (op === 'writeSkus') return 'Writing SKUs to WooCommerce...';
                if (op === 'dryPush') return 'Dry-run: checking enabled state for Woo...';
                if (op === 'push') return 'Pushing enabled/disabled status to Woo...';
                if (op === 'cats') return 'Syncing categories from Woo...';
                if (op === 'attrParents') return 'Syncing attribute parents from Woo...';
                if (op === 'attrOptions') return 'Pulling attribute options from Woo...';
                return 'Talking to WooCommerce...';
            },
            overdueHint: function (op) {
                if (op === 'products')
                    return 'Still downloading from Woo (one long request; % cannot reach 100 until it finishes)';
                if (op === 'writeSkus')
                    return 'Still writing SKUs to Woo...';
                return 'Still talking to Woo...';
            },
            isQuiet: function (op) {
                // Only hide the wait chrome for grid paging — every other click needs feedback.
                if (window._wooMapPaging) {
                    window._wooMapPaging = false;
                    return true;
                }
                return false;
            },
            tick: function () {
                var root = document.querySelector('.woo-map-busy');
                if (!root) return;
                var elapsed = Date.now() - this.startedAt;
                var est = Math.max(this.estimateMs, 3000);
                var pct;
                if (elapsed < est) {
                    pct = Math.floor((elapsed / est) * 90);
                } else {
                    var extra = elapsed - est;
                    pct = 90 + Math.min(9, Math.floor(9 * (1 - Math.exp(-extra / Math.max(est, 30000)))));
                }
                var remain = Math.max(0, est - elapsed);
                var fill = root.querySelector('.woo-map-busy-fill');
                var elElapsed = root.querySelector('.woo-map-busy-elapsed');
                var elPct = root.querySelector('.woo-map-busy-pct');
                var elEta = root.querySelector('.woo-map-busy-eta');
                var elLabel = root.querySelector('.woo-map-busy-label');
                if (fill) fill.style.width = pct + '%';
                if (elElapsed) elElapsed.textContent = 'Elapsed ' + this.formatMs(elapsed);
                if (elPct) elPct.textContent = pct + '% (guess)';
                if (elEta) {
                    if (remain > 0) {
                        elEta.textContent = 'Guess ' + this.formatMs(est)
                            + ' total / ~' + this.formatMs(remain) + ' left - not Woo real %';
                    } else {
                        elEta.textContent = this.overdueHint(this.opKey);
                    }
                }
                if (elLabel)
                    elLabel.textContent = this.labelFor(this.opKey);
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

        /* Compact pull-grid selects: short label when closed, full labels in the open list. */
        window.wooMapExpandCompactSelect = function (sel) {
            if (!sel || !sel.options) return;
            for (var i = 0; i < sel.options.length; i++) {
                var opt = sel.options[i];
                var full = opt.getAttribute('data-full');
                if (full) opt.text = full;
            }
            sel._wooCompactExpanded = true;
        };
        window.wooMapCollapseCompactSelect = function (sel) {
            if (!sel || !sel.options) return;
            for (var i = 0; i < sel.options.length; i++) {
                var opt = sel.options[i];
                var compact = opt.getAttribute('data-compact');
                if (compact) opt.text = compact;
            }
            sel._wooCompactExpanded = false;
        };
        window.wooMapBindCompactSelects = function (root) {
            root = root || document;
            var list = root.querySelectorAll('select.woo-map-compact-select');
            for (var i = 0; i < list.length; i++) {
                var sel = list[i];
                if (sel._wooCompactBound) {
                    window.wooMapCollapseCompactSelect(sel);
                    continue;
                }
                sel._wooCompactBound = true;
                sel.addEventListener('mousedown', function () { window.wooMapExpandCompactSelect(this); });
                sel.addEventListener('focus', function () { window.wooMapExpandCompactSelect(this); });
                sel.addEventListener('blur', function () { window.wooMapCollapseCompactSelect(this); });
                sel.addEventListener('change', function () {
                    var me = this;
                    window.setTimeout(function () { window.wooMapCollapseCompactSelect(me); }, 0);
                });
                window.wooMapCollapseCompactSelect(sel);
            }
        };

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
                // Clear any stuck quiet flag from a prior aborted request.
                if (document.body)
                    document.body.className = (document.body.className || '').replace(/\bwoo-map-quiet-postback\b/g, '').replace(/\s+/g, ' ').trim();
                if (window.wooMapBusy.isQuiet(op)) {
                    if (document.body)
                        document.body.className += (document.body.className ? ' ' : '') + 'woo-map-quiet-postback';
                    return;
                }
                if (document.body && document.body.className.indexOf('woo-map-working') < 0)
                    document.body.className += (document.body.className ? ' ' : '') + 'woo-map-working';
                if (el) {
                    try {
                        el.setAttribute('aria-busy', 'true');
                        if (el.disabled !== undefined) el.disabled = true;
                    } catch (ex) { }
                }
                window.wooMapBusy.start(op);
            });
            prm.add_endRequest(function () {
                window.wooMapBusy.stop(true);
                if (document.body) {
                    document.body.className = (document.body.className || '')
                        .replace(/\bwoo-map-quiet-postback\b/g, '')
                        .replace(/\bwoo-map-working\b/g, '')
                        .replace(/\s+/g, ' ').trim();
                }
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
                window.wooMapBindCompactSelects();
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
            window.addEventListener('load', function () {
                window.wooMapAttachBusyHandlers();
                window.wooMapBindCompactSelects();
            });
        })();
    </script>
</asp:Content>

<asp:Content ID="cntWooMapBdy" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="smWooMap" runat="server" EnablePartialRendering="true" AsyncPostBackTimeout="600" />

    <asp:UpdateProgress ID="upgWooMap" runat="server" AssociatedUpdatePanelID="upnlWooMap" DisplayAfter="0" DynamicLayout="true">
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
                        <asp:LinkButton ID="btnTabGeneral" runat="server" CssClass="sys-prefs-tab" OnClick="btnTabGeneral_Click" CausesValidation="false" OnClientClick="return wooMapConfirmLeave();" />
                        <asp:LinkButton ID="btnTabCat" runat="server" CssClass="sys-prefs-tab" OnClick="btnTabCat_Click" CausesValidation="false" OnClientClick="return wooMapConfirmLeave();" />
                        <asp:LinkButton ID="btnTabAttrParents" runat="server" CssClass="sys-prefs-tab" OnClick="btnTabAttrParents_Click" CausesValidation="false" OnClientClick="return wooMapConfirmLeave();" />
                        <asp:LinkButton ID="btnTabAttrVariants" runat="server" CssClass="sys-prefs-tab" OnClick="btnTabAttrVariants_Click" CausesValidation="false" OnClientClick="return wooMapConfirmLeave();" />
                        <asp:LinkButton ID="btnTabMap" runat="server" CssClass="sys-prefs-tab" OnClick="btnTabMap_Click" CausesValidation="false" OnClientClick="return wooMapConfirmLeave();" />
                        <asp:LinkButton ID="btnTabAreas" runat="server" CssClass="sys-prefs-tab" OnClick="btnTabAreas_Click" CausesValidation="false" OnClientClick="return wooMapConfirmLeave();" />
                        <asp:LinkButton ID="btnTabShipping" runat="server" CssClass="sys-prefs-tab" OnClick="btnTabShipping_Click" CausesValidation="false" OnClientClick="return wooMapConfirmLeave();" />
                        <asp:LinkButton ID="btnTabPayment" runat="server" CssClass="sys-prefs-tab" OnClick="btnTabPayment_Click" CausesValidation="false" OnClientClick="return wooMapConfirmLeave();" />
                        <asp:LinkButton ID="btnTabSavedMaps" runat="server" CssClass="sys-prefs-tab" OnClick="btnTabSavedMaps_Click" CausesValidation="false" OnClientClick="return wooMapConfirmLeave();" />
                        <asp:LinkButton ID="btnTabMissingSku" runat="server" CssClass="sys-prefs-tab" OnClick="btnTabMissingSku_Click" CausesValidation="false" OnClientClick="return wooMapConfirmLeave();" />
                        <asp:LinkButton ID="btnTabSync" runat="server" CssClass="sys-prefs-tab" OnClick="btnTabSync_Click" CausesValidation="false" OnClientClick="return wooMapConfirmLeave();" />
                    </nav>
                </div>

                <asp:Label ID="lblMessage" runat="server" CssClass="status-message" Visible="false" />

                <asp:MultiView ID="mvTabs" runat="server" ActiveViewIndex="0">
                    <asp:View ID="viewCat" runat="server">
                        <p><asp:Literal ID="litCatHelp" runat="server" /></p>
                        <div class="button-row woo-map-cat-toolbar">
                            <asp:Literal ID="litCatModeLbl" runat="server" />
                            <asp:DropDownList ID="ddlCatMode" runat="server" onchange="wooMapRefreshDirty();">
                                <asp:ListItem Value="All" Text="All categories" />
                                <asp:ListItem Value="IncludeList" Text="Included only" />
                            </asp:DropDownList>
                            <asp:Button ID="btnSaveCatIncludes" runat="server" CssClass="filter-panel-btn" OnClick="btnSaveCatIncludes_Click" CausesValidation="false" UseSubmitBehavior="true" OnClientClick="return wooMapPrepareSave(this);" data-woo-save-ready="0" />
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
                            <asp:Button ID="btnSaveAttrParents" runat="server" CssClass="filter-panel-btn" OnClick="btnSaveAttrParents_Click" CausesValidation="false" UseSubmitBehavior="true" OnClientClick="return wooMapPrepareSave(this);" data-woo-save-ready="0" />
                        </div>
                        <asp:GridView ID="gvAttrParents" runat="server" CssClass="results-table woo-map-attr-parents-grid" AutoGenerateColumns="false"
                            DataKeyNames="ParentID" OnRowDataBound="gvAttrParents_RowDataBound">
                            <Columns>
                                <asp:BoundField DataField="AttributeName" HeaderText="Attribute" />
                                <asp:BoundField DataField="TermCount" HeaderText="Options" />
                                <asp:TemplateField HeaderText="Use on variants">
                                    <ItemTemplate>
                                        <asp:CheckBox ID="chkUseForVariants" runat="server"
                                            Checked='<%# Eval("UseForVariants") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Qty source">
                                    <ItemTemplate>
                                        <asp:DropDownList ID="ddlQtyRank" runat="server" CssClass="sys-prefs-input"
                                            ToolTip="1 = primary qty; 2 = if primary missing. Leave (none) if this attribute does not set qty.">
                                            <asp:ListItem Value="0" Text="(none)" />
                                            <asp:ListItem Value="1" Text="1 primary" />
                                            <asp:ListItem Value="2" Text="2 if missing" />
                                            <asp:ListItem Value="3" Text="3" />
                                            <asp:ListItem Value="4" Text="4" />
                                            <asp:ListItem Value="5" Text="5" />
                                        </asp:DropDownList>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Pack source">
                                    <ItemTemplate>
                                        <asp:DropDownList ID="ddlPackRank" runat="server" CssClass="sys-prefs-input"
                                            ToolTip="1 = primary packaging (e.g. Prep/grind); 2 = fallback (e.g. size). Tracker Packaging also stores grind today.">
                                            <asp:ListItem Value="0" Text="(none)" />
                                            <asp:ListItem Value="1" Text="1 primary" />
                                            <asp:ListItem Value="2" Text="2 if missing" />
                                            <asp:ListItem Value="3" Text="3" />
                                            <asp:ListItem Value="4" Text="4" />
                                            <asp:ListItem Value="5" Text="5" />
                                        </asp:DropDownList>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Notes">
                                    <ItemTemplate>
                                        <asp:DropDownList ID="ddlNoteRank" runat="server" CssClass="sys-prefs-input"
                                            ToolTip="Append Woo option text to the Tracker order note (1 first). Can combine with Pack source.">
                                            <asp:ListItem Value="0" Text="(none)" />
                                            <asp:ListItem Value="1" Text="1" />
                                            <asp:ListItem Value="2" Text="2" />
                                            <asp:ListItem Value="3" Text="3" />
                                        </asp:DropDownList>
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
                                <asp:Button ID="btnSaveAttrMaps" runat="server" CssClass="filter-panel-btn" OnClick="btnSaveAttrMaps_Click" CausesValidation="false" UseSubmitBehavior="true" OnClientClick="return wooMapPrepareSave(this);" data-woo-save-ready="0" />
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
                                    <asp:TemplateField HeaderText="Sets">
                                        <ItemTemplate>
                                            <asp:Literal ID="litAttrSets" runat="server"
                                                Text='<%# TrackerSQL.Models.WooAttributeMapRoles.DisplayLabel(Eval("MapRole") as string) %>' />
                                            <asp:HiddenField ID="hdnAttrMapId" runat="server" Value='<%# Eval("MapID") %>' />
                                            <asp:HiddenField ID="hdnAttrName" runat="server" Value='<%# Eval("AttributeName") %>' />
                                            <asp:HiddenField ID="hdnAttrOption" runat="server" Value='<%# Eval("AttributeOption") %>' />
                                            <asp:HiddenField ID="hdnAttrRole" runat="server" Value='<%# Eval("MapRole") %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Qty factor">
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtAttrQty" runat="server" Text='<%# Eval("QtyFactor") %>' Width="60px" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Packaging">
                                        <ItemTemplate>
                                            <asp:DropDownList ID="ddlAttrPack" runat="server" CssClass="sys-prefs-input" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Service type" Visible="false">
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
                        <div class="filter-toolbar woo-map-notes-item">
                            <div class="filter-section">
                                <div class="filter-control">
                                    <asp:Label ID="lblImportNotesItem" runat="server" AssociatedControlID="ddlImportNotesItem" />
                                    <asp:DropDownList ID="ddlImportNotesItem" runat="server" CssClass="sys-prefs-input"
                                        ToolTip="Tracker item added when a Woo SKU is written to order notes." />
                                </div>
                                <asp:Button ID="btnSaveImportNotesItem" runat="server" CssClass="filter-panel-btn"
                                    OnClick="btnSaveImportNotesItem_Click" CausesValidation="false"
                                    OnClientClick="return wooMapPrepareSave(this);" data-woo-save-ready="0" />
                            </div>
                        </div>
                        <div class="button-row">
                            <asp:Button ID="btnPullProducts" runat="server" CssClass="filter-panel-btn" OnClick="btnPullProducts_Click" CausesValidation="false" OnClientClick="return wooMapBusy.start('products');" data-woo-op="products" />
                            <asp:Button ID="btnSaveSelectedMaps" runat="server" CssClass="filter-panel-btn" OnClick="btnSaveSelectedMaps_Click" CausesValidation="false" UseSubmitBehavior="true" OnClientClick="return wooMapPrepareSave(this);" data-woo-save-ready="0" />
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
                                <div class="filter-control woo-map-new-since">
                                    <asp:CheckBox ID="chkNewSinceSync" runat="server" AutoPostBack="true"
                                        OnCheckedChanged="chkNewSinceSync_CheckedChanged" CausesValidation="false"
                                        ToolTip="Show only Woo products first seen on the most recent Sync products pull" />
                                </div>
                            </div>
                        </asp:Panel>
                        <asp:Literal ID="litPullPageInfo" runat="server" />
                        <div class="results-container scrollable-table-container">
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
                                <asp:TemplateField HeaderText="Svd" ItemStyle-HorizontalAlign="Center"
                                    ItemStyle-CssClass="woo-map-saved-col woo-map-flag-col" HeaderStyle-CssClass="woo-map-saved-col woo-map-flag-col">
                                    <ItemTemplate>
                                        <span class='<%# Eval("SavedFlagCss") %>' title='<%# Eval("SavedFlagTitle") %>'>
                                            <%# Eval("SavedFlagLabel") %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Impt" ItemStyle-HorizontalAlign="Center"
                                    ItemStyle-CssClass="woo-map-flag-col" HeaderStyle-CssClass="woo-map-flag-col">
                                    <ItemTemplate>
                                        <asp:CheckBox ID="chkImport" runat="server" Checked='<%# Eval("ShowIncludeInImport") %>'
                                            Enabled="true"
                                            OnCheckedChanged="chkImport_CheckedChanged" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Woo SKU" HeaderStyle-CssClass="woo-map-sku-col" ItemStyle-CssClass="woo-map-sku-col">
                                    <ItemTemplate>
                                        <span class='<%# Eval("SkuCssClass") %>'>
                                            <asp:LinkButton ID="btnToggleGroup" runat="server" CssClass="woo-map-toggle"
                                                Visible='<%# Eval("IsParentGroup") %>'
                                                CommandName="ToggleGroup"
                                                CommandArgument='<%# Eval("WooProductId") %>'
                                                CausesValidation="false"
                                                Text='<%# (bool)Eval("GroupExpanded") ? "-" : "+" %>'
                                                ToolTip='<%# (bool)Eval("GroupExpanded") ? "Hide variants" : "Show variants" %>' />
                                            <asp:Literal ID="litSkuBadge" runat="server" Visible='<%# Eval("IsVariation") %>' Text="v " />
                                            <asp:Literal ID="litParentBadge" runat="server" Visible='<%# Eval("IsParentGroup") %>' Text="grp " />
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
                                <asp:BoundField DataField="NotesAttributeSummary" HeaderText="Pack cascade / notes" ItemStyle-CssClass="woo-map-variant-col" />
                                <asp:TemplateField HeaderText="Mode">
                                    <ItemTemplate>
                                        <asp:DropDownList ID="ddlImportMode" runat="server" CssClass="sys-prefs-input"
                                            Visible='<%# Eval("IsParentGroup") %>'
                                            AutoPostBack="true"
                                            OnSelectedIndexChanged="ddlImportMode_SelectedIndexChanged" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="MatchReason" HeaderText="Match" ItemStyle-CssClass="woo-map-match-col" />
                                <asp:TemplateField HeaderText="Tracker SKU">
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
                                        <asp:DropDownList ID="ddlSortOrder" runat="server" CssClass="lookups-item-so woo-map-so woo-map-compact-select"
                                            Visible='<%# Eval("ShowItemCreateFields") %>'
                                            AutoPostBack="true"
                                            OnSelectedIndexChanged="ddlSortOrder_SelectedIndexChanged"
                                            ToolTip="Tracker item sort order (major category)" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Qty" HeaderStyle-CssClass="woo-map-qty-col" ItemStyle-CssClass="woo-map-qty-col">
                                    <ItemTemplate>
                                        <asp:TextBox ID="txtQty" runat="server" Text='<%# Eval("QtyFactor") %>' CssClass="sys-prefs-input woo-map-qty"
                                            Visible='<%# !(bool)Eval("IsParentGroup") || (bool)Eval("ImportParentAsItem") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Pack" HeaderStyle-CssClass="woo-map-pack-col" ItemStyle-CssClass="woo-map-pack-col">
                                    <ItemTemplate>
                                        <asp:DropDownList ID="ddlPack" runat="server" CssClass="sys-prefs-input woo-map-pack woo-map-compact-select"
                                            Visible='<%# !(bool)Eval("IsParentGroup") || (bool)Eval("ImportParentAsItem") %>'
                                            AutoPostBack="true"
                                            OnSelectedIndexChanged="ddlPack_SelectedIndexChanged" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                        </div>
                        <div class="button-row woo-map-pull-toolbar-bottom">
                            <asp:Button ID="btnSaveSelectedMapsBottom" runat="server" CssClass="filter-panel-btn"
                                OnClick="btnSaveSelectedMaps_Click" CausesValidation="false" UseSubmitBehavior="true"
                                OnClientClick="return wooMapPrepareSave(this);" data-woo-save-ready="0" />
                        </div>
                    </asp:View>

                    <asp:View ID="viewAreas" runat="server">
                        <p><asp:Literal ID="litAreasHelp" runat="server" /></p>
                        <p class="woo-map-section-note">
                            Area postcodes and delivery people are stored in <strong>one</strong> Tracker table
                            (<code>WooAreaDeliveryDefaultTbl</code>), shared with
                            <a href="PostalAreaSetup.aspx">Postal Area Setup</a>.
                            WooCommerce does not store a second copy of these ranges.
                            Test resolve and Contact Details Suggest read this table.
                            (An old <code>WooPostalAreaMapTbl</code> is only used once to migrate into these ranges.)
                        </p>
                        <div class="filter-toolbar woo-map-areas-default">
                            <div class="filter-section">
                                <div class="filter-control">
                                    <asp:Label ID="lblDefaultImportArea" runat="server" AssociatedControlID="ddlDefaultImportArea" />
                                    <asp:DropDownList ID="ddlDefaultImportArea" runat="server" CssClass="sys-prefs-input"
                                        ToolTip="Used when a Woo order postcode is not in any configured range below." />
                                </div>
                                <asp:Button ID="btnSaveDefaultArea" runat="server" CssClass="filter-panel-btn"
                                    OnClick="btnSaveDefaultArea_Click" CausesValidation="false"
                                    OnClientClick="return wooMapPrepareSave(this);" data-woo-save-ready="0" />
                            </div>
                        </div>

                        <h3 class="woo-map-section-title"><asp:Literal ID="litAddressConfigTitle" runat="server" /></h3>
                        <p class="woo-map-section-note"><asp:Literal ID="litAddressConfigNote" runat="server" /></p>
                        <div class="filter-toolbar woo-map-address-config">
                            <div class="woo-map-address-config-grid">
                                <div class="woo-map-address-config-item">
                                    <asp:CheckBox ID="chkImportAddressIncludeProvince" runat="server"
                                        Text="Include province/state in billing address" />
                                </div>
                                <div class="woo-map-address-config-item">
                                    <asp:CheckBox ID="chkImportAddressIncludeCountry" runat="server"
                                        Text="Include country in billing address" />
                                </div>
                                <div class="woo-map-address-config-item">
                                    <asp:CheckBox ID="chkImportAddressDeduplicateSuburb" runat="server"
                                        Text="Remove duplicate suburb/city tokens (e.g. Claremont; Claremont)" />
                                </div>
                                <div class="woo-map-address-config-item">
                                    <asp:CheckBox ID="chkImportAddressStripCapeTown" runat="server"
                                        Text="Remove &quot;Cape Town&quot; when delivery area is Cape Town*" />
                                </div>
                                <div class="woo-map-address-config-item">
                                    <asp:CheckBox ID="chkImportAddressTitleCase" runat="server"
                                        Text="Title-case address lines (not ALL CAPS / all lowercase)" />
                                </div>
                                <div class="woo-map-address-config-item">
                                    <asp:CheckBox ID="chkImportPhoneReplacePlus27" runat="server"
                                        Text="Replace +27 with 0 on phone numbers" />
                                </div>
                                <div class="woo-map-address-config-item">
                                    <asp:CheckBox ID="chkImportPhoneFormatSa" runat="server"
                                        Text="Format SA phone numbers (aaa bbb-cccc)" />
                                </div>
                            </div>
                            <div class="woo-map-address-config-actions">
                                <asp:Button ID="btnSaveAddressConfig" runat="server" CssClass="filter-panel-btn"
                                    OnClick="btnSaveAddressConfig_Click" CausesValidation="false"
                                    OnClientClick="return wooMapPrepareSave(this);" data-woo-save-ready="0" />
                            </div>
                        </div>

                        <h3 class="woo-map-section-title"><asp:Literal ID="litAreaSectionTitle" runat="server" /></h3>
                        <p class="woo-map-section-note"><asp:Literal ID="litAreaDefaultsNote" runat="server" /></p>
                        <p class="woo-map-section-note woo-map-system-default"><asp:Literal ID="litSystemDefaultPersonHint" runat="server" /></p>
                        <div class="filter-toolbar">
                            <div class="filter-section search-controls">
                                <div class="filter-control">
                                    <asp:Label ID="lblAreaFilter" runat="server" AssociatedControlID="txtAreaFilter" Text="Filter areas:" />
                                    <asp:TextBox ID="txtAreaFilter" runat="server" CssClass="sys-prefs-input"
                                        placeholder="e.g. Hout Bay, Gauteng" />
                                </div>
                                <asp:Button ID="btnAreaFilter" runat="server" CssClass="filter-panel-btn"
                                    OnClick="btnAreaFilter_Click" CausesValidation="false" Text="Search" />
                                <asp:Button ID="btnAreaFilterClear" runat="server" CssClass="filter-panel-btn"
                                    OnClick="btnAreaFilterClear_Click" CausesValidation="false" Text="Clear" />
                            </div>
                        </div>
                        <div class="button-row">
                            <asp:Button ID="btnSaveAreaDefaults" runat="server" CssClass="filter-panel-btn"
                                OnClick="btnSaveAreaDefaults_Click" CausesValidation="false"
                                OnClientClick="return wooMapPrepareSave(this);" data-woo-save-ready="0" />
                        </div>
                        <asp:GridView ID="gvAreaDefaults" runat="server" CssClass="results-table woo-map-area-defaults-grid postal-area-setup-grid"
                            AutoGenerateColumns="false" DataKeyNames="AreaID"
                            AllowPaging="true" PageSize="15"
                            OnPageIndexChanging="gvAreaDefaults_PageIndexChanging"
                            OnRowCreated="gvAreaDefaults_RowCreated"
                            OnRowDataBound="gvAreaDefaults_RowDataBound"
                            EmptyDataText="No Tracker areas found.">
                            <PagerStyle CssClass="pager-row" />
                            <PagerTemplate>
                                <asp:PlaceHolder ID="plhPager" runat="server" />
                            </PagerTemplate>
                            <Columns>
                                <asp:BoundField DataField="AreaName" HeaderText="Area" ReadOnly="true" ItemStyle-CssClass="woo-map-area-name-col" />
                                <asp:TemplateField HeaderText="Default person">
                                    <ItemTemplate>
                                        <asp:DropDownList ID="ddlDefaultPerson" runat="server" CssClass="sys-prefs-input" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Postcodes" ItemStyle-CssClass="postal-ranges-cell">
                                    <ItemTemplate>
                                        <asp:TextBox ID="txtPostalRanges" runat="server" CssClass="sys-prefs-input woo-map-postal-ranges"
                                            TextMode="MultiLine" Rows="3"
                                            Text='<%# Eval("PostalRanges") %>'
                                            ToolTip="7806 or 7800...7806 — separate ranges with ;" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>

                        <h3 class="woo-map-section-title">Test resolve</h3>
                        <asp:Panel ID="pnlTestResolve" runat="server" DefaultButton="btnTestResolve" CssClass="filter-toolbar">
                            <div class="filter-section search-controls">
                                <div class="filter-control">
                                    <asp:Label ID="lblTestPostal" runat="server" AssociatedControlID="txtTestPostal" />
                                    <asp:TextBox ID="txtTestPostal" runat="server" CssClass="sys-prefs-input col-tight" MaxLength="10" />
                                </div>
                                <div class="filter-control">
                                    <asp:Label ID="lblTestSuburb" runat="server" AssociatedControlID="txtTestSuburb" />
                                    <asp:TextBox ID="txtTestSuburb" runat="server" CssClass="sys-prefs-input" />
                                </div>
                                <asp:Button ID="btnTestResolve" runat="server" CssClass="filter-panel-btn"
                                    OnClick="btnTestResolve_Click" CausesValidation="false" />
                            </div>
                        </asp:Panel>
                        <asp:Literal ID="litTestResolveResult" runat="server" />
                    </asp:View>

                    <asp:View ID="viewShipping" runat="server">
                        <p><asp:Literal ID="litShippingHelp" runat="server" /></p>

                        <h3 class="woo-map-section-title">Woo shipping method → Delivered by</h3>
                        <p class="woo-map-section-note"><asp:Literal ID="litShippingMapsNote" runat="server" /></p>
                        <p class="woo-map-section-note" id="pnlShippingEmptyHint" runat="server" visible="false">
                            <asp:Literal ID="litShippingEmptyHint" runat="server" />
                        </p>
                        <div class="results-container woo-map-shipping-grid-wrap">
                        <asp:GridView ID="gvShippingMaps" runat="server" CssClass="results-table woo-map-shipping-grid"
                            AutoGenerateColumns="false" DataKeyNames="MapID" ShowFooter="true"
                            OnRowDataBound="gvShippingMaps_RowDataBound"
                            OnRowCommand="gvShippingMaps_RowCommand">
                            <Columns>
                                <asp:BoundField DataField="MapID" HeaderText="ID" ItemStyle-CssClass="col-tight" ReadOnly="true" />
                                <asp:TemplateField HeaderText="Woo method contains">
                                    <ItemTemplate>
                                        <asp:TextBox ID="txtMethodMatch" runat="server" CssClass="sys-prefs-input"
                                            Text='<%# Eval("MethodMatch") %>' ToolTip="Matched case-insensitively against Woo shipping method title" />
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        <asp:TextBox ID="txtNewMethodMatch" runat="server" CssClass="sys-prefs-input"
                                            ToolTip="e.g. Pargo, Local pickup" placeholder="e.g. Pargo" />
                                    </FooterTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Delivered by">
                                    <ItemTemplate>
                                        <asp:DropDownList ID="ddlShipPerson" runat="server" CssClass="sys-prefs-input" />
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        <asp:DropDownList ID="ddlNewShipPerson" runat="server" CssClass="sys-prefs-input" />
                                    </FooterTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Active" ItemStyle-CssClass="col-tight">
                                    <ItemTemplate>
                                        <asp:CheckBox ID="chkShipActive" runat="server" Checked='<%# Eval("IsActive") %>' />
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        <asp:CheckBox ID="chkNewShipActive" runat="server" Checked="true" />
                                    </FooterTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Notes" ItemStyle-CssClass="postal-ranges-cell">
                                    <ItemTemplate>
                                        <asp:TextBox ID="txtShipNotes" runat="server" CssClass="sys-prefs-input woo-map-notes-wide"
                                            TextMode="MultiLine" Rows="2"
                                            Text='<%# Eval("Notes") %>' />
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        <asp:TextBox ID="txtNewShipNotes" runat="server" CssClass="sys-prefs-input woo-map-notes-wide"
                                            TextMode="MultiLine" Rows="2" />
                                    </FooterTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="" ItemStyle-CssClass="col-tight">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="btnDeleteShipping" runat="server" CssClass="filter-panel-btn"
                                            CommandName="DeleteShipping" CommandArgument='<%# Eval("MapID") %>'
                                            CausesValidation="false"
                                            OnClientClick="return confirm('Delete this shipping map?');" />
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        <span class="woo-map-footer-hint">New row ↑</span>
                                    </FooterTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                        </div>
                        <div class="button-row">
                            <asp:Button ID="btnSaveShippingMaps" runat="server" CssClass="filter-panel-btn woo-map-btn-wide"
                                OnClick="btnSaveShippingMaps_Click" CausesValidation="false"
                                OnClientClick="return wooMapPrepareSave(this);" data-woo-save-ready="0" />
                        </div>

                        <h3 class="woo-map-section-title"><asp:Literal ID="litDispatchWaybillTitle" runat="server" /></h3>
                        <p class="woo-map-section-note"><asp:Literal ID="litDispatchWaybillNote" runat="server" /></p>
                        <h4 class="woo-map-section-subtitle"><asp:Literal ID="lblDispatchPeople" runat="server" /></h4>
                        <div class="results-container woo-map-dispatch-grid-wrap">
                            <asp:GridView ID="gvDispatchPeople" runat="server"
                                CssClass="results-table woo-map-dispatch-grid"
                                AutoGenerateColumns="false" DataKeyNames="PersonID"
                                AllowPaging="true" PageSize="8"
                                OnPageIndexChanging="gvDispatchPeople_PageIndexChanging"
                                OnRowCreated="gvDispatchPeople_RowCreated"
                                EmptyDataText="No delivery people found.">
                                <PagerStyle CssClass="pager-row" />
                                <PagerTemplate>
                                    <asp:PlaceHolder ID="plhPager" runat="server" />
                                </PagerTemplate>
                                <Columns>
                                    <asp:TemplateField HeaderText="" ItemStyle-CssClass="col-tight woo-map-dispatch-check-col"
                                        HeaderStyle-CssClass="col-tight">
                                        <ItemTemplate>
                                            <asp:CheckBox ID="chkUseWaybill" runat="server"
                                                Checked='<%# Eval("UseWaybill") %>'
                                                ToolTip="Require waybill / leave Woo open for this Delivered by" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="DisplayName" HeaderText="Delivered by"
                                        ItemStyle-CssClass="woo-map-dispatch-person-col" />
                                </Columns>
                            </asp:GridView>
                        </div>
                        <div class="woo-map-dispatch-options">
                            <asp:CheckBox ID="chkTrackingNumberRequired" runat="server" />
                            <p class="woo-map-section-note woo-map-dispatch-msg-hint">
                                Order Done currently always sends the hard-coded “dispatched” message for these carriers.
                                Per-carrier message text can be configured here later.
                            </p>
                        </div>
                        <div class="button-row">
                            <asp:Button ID="btnSaveDispatchWaybill" runat="server" CssClass="filter-panel-btn woo-map-btn-wide"
                                OnClick="btnSaveDispatchWaybill_Click" CausesValidation="false"
                                OnClientClick="return wooMapPrepareSave(this);" data-woo-save-ready="0" />
                        </div>
                    </asp:View>

                    <asp:View ID="viewPayment" runat="server">
                        <p><asp:Literal ID="litPaymentHelp" runat="server" /></p>
                        <p class="woo-map-section-note"><asp:Literal ID="litPaymentMapsNote" runat="server" /></p>
                        <div class="button-row">
                            <asp:Button ID="btnSavePaymentMaps" runat="server" CssClass="filter-panel-btn"
                                OnClick="btnSavePaymentMaps_Click" CausesValidation="false"
                                OnClientClick="return wooMapPrepareSave(this);" data-woo-save-ready="0" />
                        </div>
                        <div class="results-container scrollable-table-container">
                        <asp:GridView ID="gvPaymentMaps" runat="server" CssClass="results-table results-table-fit"
                            AutoGenerateColumns="false" DataKeyNames="MapID" ShowFooter="true"
                            OnRowDataBound="gvPaymentMaps_RowDataBound"
                            OnRowCommand="gvPaymentMaps_RowCommand">
                            <Columns>
                                <asp:TemplateField HeaderText="Method match" ItemStyle-CssClass="col-fill">
                                    <ItemTemplate>
                                        <asp:TextBox ID="txtPayMethodMatch" runat="server" CssClass="sys-prefs-input"
                                            Text='<%# Eval("MethodMatch") %>' ToolTip="e.g. payfast, yoco, class_yoco" />
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        <asp:TextBox ID="txtNewPayMethodMatch" runat="server" CssClass="sys-prefs-input" />
                                    </FooterTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Abbrev" ItemStyle-CssClass="col-tight">
                                    <ItemTemplate>
                                        <asp:TextBox ID="txtPayAbbrev" runat="server" CssClass="sys-prefs-input" MaxLength="4" Width="4em"
                                            Text='<%# Eval("PaymentAbbrev") %>' />
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        <asp:TextBox ID="txtNewPayAbbrev" runat="server" CssClass="sys-prefs-input" MaxLength="4" Width="4em" />
                                    </FooterTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Active" ItemStyle-CssClass="col-tight">
                                    <ItemTemplate>
                                        <asp:CheckBox ID="chkPayActive" runat="server" Checked='<%# Eval("IsActive") %>' />
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        <asp:CheckBox ID="chkNewPayActive" runat="server" Checked="true" />
                                    </FooterTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="" ItemStyle-CssClass="col-tight">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="btnDeletePayment" runat="server" CssClass="filter-panel-btn"
                                            CommandName="DeletePayment" CommandArgument='<%# Eval("MapID") %>'
                                            CausesValidation="false"
                                            OnClientClick="return confirm('Delete this payment map?');" />
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        <span class="woo-map-footer-hint">New row ↑</span>
                                    </FooterTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                        </div>
                    </asp:View>

                    <asp:View ID="viewSavedMaps" runat="server">
                        <p><asp:Literal ID="litSavedMapsHelp" runat="server" /></p>
                        <asp:Panel ID="pnlSavedMapsSearch" runat="server" DefaultButton="btnFindSavedMap" CssClass="filter-toolbar">
                            <div class="filter-section search-controls">
                                <div class="filter-control">
                                    <asp:Label ID="lblFindSavedMap" runat="server" AssociatedControlID="txtFindSavedMap" />
                                    <asp:TextBox ID="txtFindSavedMap" runat="server"
                                        ToolTip="Search Woo SKU, Tracker SKU, item name, type, or mapping ID" />
                                </div>
                                <asp:Button ID="btnFindSavedMap" runat="server" CssClass="filter-panel-btn"
                                    OnClick="btnFindSavedMap_Click" CausesValidation="false" ToolTip="Search saved maps" />
                                <asp:Button ID="btnClearFindSavedMap" runat="server" CssClass="filter-panel-btn"
                                    OnClick="btnClearFindSavedMap_Click" CausesValidation="false" ToolTip="Clear search" />
                            </div>
                        </asp:Panel>
                        <asp:Literal ID="litSavedMapsPageInfo" runat="server" />
                        <div class="woo-map-saved-scroll">
                        <asp:GridView ID="gvMaps" runat="server" CssClass="results-table woo-map-saved-grid" AutoGenerateColumns="false"
                            DataKeyNames="MappingID" AllowPaging="true" PageSize="40" AllowSorting="true"
                            OnPageIndexChanging="gvMaps_PageIndexChanging"
                            OnSorting="gvMaps_Sorting"
                            OnRowCreated="gvMaps_RowCreated"
                            OnRowCommand="gvMaps_RowCommand">
                            <PagerStyle CssClass="pager-row" />
                            <PagerTemplate>
                                <asp:PlaceHolder ID="plhPager" runat="server" />
                            </PagerTemplate>
                            <Columns>
                                <asp:BoundField DataField="MappingID" HeaderText="ID" SortExpression="MappingID" ItemStyle-CssClass="col-tight" />
                                <asp:BoundField DataField="MapType" HeaderText="Type" SortExpression="MapType" ItemStyle-CssClass="col-tight" />
                                <asp:TemplateField HeaderText="Import" SortExpression="IncludeInImport" ItemStyle-CssClass="col-tight">
                                    <ItemTemplate>
                                        <%# (bool)Eval("IncludeInImport") ? "Yes" : "No" %>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="ItemSku" HeaderText="Item SKU" SortExpression="ItemSku" ItemStyle-CssClass="col-tight" />
                                <asp:BoundField DataField="ItemDesc" HeaderText="Item" SortExpression="ItemDesc" ItemStyle-CssClass="woo-map-saved-item" />
                                <asp:TemplateField HeaderText="Woo SKU" SortExpression="SkuPattern" ItemStyle-CssClass="woo-map-saved-sku">
                                    <ItemTemplate>
                                        <span class="woo-map-saved-sku-text" title='<%# Eval("SkuPattern") %>'><%# Eval("SkuPattern") %></span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Product" SortExpression="WooProductLabel" ItemStyle-CssClass="woo-map-saved-item">
                                    <ItemTemplate>
                                        <span title='<%# Eval("WooProductId") %>'><%# FormatWooProductCell(Container.DataItem) %></span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Variation" SortExpression="WooVariationLabel" ItemStyle-CssClass="woo-map-saved-item">
                                    <ItemTemplate>
                                        <span title='<%# Eval("WooVariationId") %>'><%# FormatWooVariationCell(Container.DataItem) %></span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="QtyFactor" HeaderText="Qty" SortExpression="QtyFactor" ItemStyle-CssClass="col-tight" />
                                <asp:TemplateField HeaderText="Pack" SortExpression="PackagingDesc" ItemStyle-CssClass="col-tight">
                                    <ItemTemplate>
                                        <%# string.IsNullOrEmpty(Eval("PackagingDesc") as string)
                                            ? (Eval("PackagingID") == null || Convert.ToInt32(Eval("PackagingID")) <= 0 ? "-" : Eval("PackagingID").ToString())
                                            : Eval("PackagingDesc") %>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="LastWooStatus" HeaderText="Last Woo" SortExpression="LastWooStatus" ItemStyle-CssClass="col-tight" />
                                <asp:TemplateField ItemStyle-CssClass="col-cmd">
                                    <ItemTemplate>
                                        <asp:Button ID="btnDelMap" runat="server" CssClass="filter-panel-btn" CommandName="DelMap"
                                            CommandArgument='<%# Eval("MappingID") %>' Text="Delete"
                                            OnClientClick="return confirm('Delete this mapping?');" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                        </div>
                    </asp:View>

                    <asp:View ID="viewMissingSku" runat="server">
                        <p><asp:Literal ID="litMissingSkuHelp" runat="server" /></p>
                        <div class="button-row">
                            <asp:Button ID="btnWriteMissingSkus" runat="server" CssClass="filter-panel-btn"
                                OnClick="btnWriteMissingSkus_Click" CausesValidation="false"
                                OnClientClick="if (!wooMapPrepareSave(this)) return false; return wooMapBusy.start('writeSkus');" data-woo-op="writeSkus" data-woo-save-ready="0" />
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
                                        <asp:TextBox ID="txtNewSku" runat="server" Text='<%# Eval("NewSku") %>' CssClass="sys-prefs-input" Width="140px"
                                            oninput="wooMapMissingSkuTyped(this);" onchange="wooMapMissingSkuTyped(this);" />
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
                        <asp:Literal ID="litEnabledPushSummary" runat="server" />
                        <div class="results-container scrollable-table-container">
                            <asp:GridView ID="gvEnabledPush" runat="server" CssClass="results-table" AutoGenerateColumns="false"
                                AllowPaging="true" PageSize="40"
                                DataKeyNames="MappingID"
                                OnPageIndexChanging="gvEnabledPush_PageIndexChanging"
                                OnRowCreated="gvEnabledPush_RowCreated"
                                OnRowDataBound="gvEnabledPush_RowDataBound">
                                <PagerStyle CssClass="pager-row" />
                                <PagerTemplate>
                                    <asp:PlaceHolder ID="plhEnabledPushPager" runat="server" />
                                </PagerTemplate>
                                <Columns>
                                    <asp:BoundField DataField="ItemID" HeaderText="Item #" />
                                    <asp:BoundField DataField="ItemDesc" HeaderText="Item (Tracker)" />
                                    <asp:BoundField DataField="ItemSku" HeaderText="SKU" />
                                    <asp:TemplateField HeaderText="Enbld">
                                        <ItemTemplate>
                                            <%# (bool)Eval("TrackerEnabled") ? "Yes" : "No" %>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="WooProductId" HeaderText="Woo product" />
                                    <asp:TemplateField HeaderText="Variation">
                                        <ItemTemplate>
                                            <%# Eval("WooVariationId") == null || Convert.ToInt64(Eval("WooVariationId")) <= 0
                                                ? "-"
                                                : Eval("WooVariationId") %>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="CurrentWooStatus" HeaderText="Woo now" />
                                    <asp:BoundField DataField="NewWooStatus" HeaderText="Will set" />
                                    <asp:BoundField DataField="Result" HeaderText="Result" />
                                </Columns>
                            </asp:GridView>
                        </div>
                    </asp:View>

                    <asp:View ID="viewGeneral" runat="server">
                        <p><asp:Literal ID="litGeneralHelp" runat="server" /></p>

                        <h3 class="woo-map-section-title">Company name on existing contacts</h3>
                        <p class="woo-map-section-note"><asp:Literal ID="litGeneralCompanyModeNote" runat="server" /></p>
                        <div class="filter-toolbar">
                            <div class="filter-section">
                                <div class="filter-control">
                                    <asp:Label ID="lblGeneralCompanyMode" runat="server" AssociatedControlID="ddlGeneralCompanyMode" />
                                    <asp:DropDownList ID="ddlGeneralCompanyMode" runat="server" CssClass="sys-prefs-input" />
                                </div>
                            </div>
                        </div>

                        <h3 class="woo-map-section-title">Order note line format</h3>
                        <p class="woo-map-section-note"><asp:Literal ID="litGeneralNoteLineFormatNote" runat="server" /></p>
                        <div class="filter-toolbar">
                            <div class="filter-section">
                                <div class="filter-control">
                                    <asp:Label ID="lblGeneralNoteLineFormat" runat="server" AssociatedControlID="ddlGeneralNoteLineFormat" />
                                    <asp:DropDownList ID="ddlGeneralNoteLineFormat" runat="server" CssClass="sys-prefs-input" />
                                </div>
                            </div>
                        </div>

                        <h3 class="woo-map-section-title">Order note parts (all imports)</h3>
                        <p class="woo-map-section-note"><asp:Literal ID="litGeneralNotePartOrderNote" runat="server" /></p>
                        <div class="filter-toolbar">
                            <div class="filter-section">
                                <div class="filter-control">
                                    <asp:Label ID="lblGeneralNotePartOrder" runat="server" AssociatedControlID="lstGeneralNotePartOrder" />
                                    <asp:ListBox ID="lstGeneralNotePartOrder" runat="server" CssClass="sys-prefs-input"
                                        Rows="8" Width="36em" />
                                </div>
                                <div class="filter-control" style="display:flex; flex-direction:column; gap:6px;">
                                    <asp:Button ID="btnNotePartMoveUp" runat="server" CssClass="filter-panel-btn"
                                        Text="Move up" OnClick="btnNotePartMoveUp_Click" CausesValidation="false" />
                                    <asp:Button ID="btnNotePartMoveDown" runat="server" CssClass="filter-panel-btn"
                                        Text="Move down" OnClick="btnNotePartMoveDown_Click" CausesValidation="false" />
                                </div>
                            </div>
                        </div>

                        <h3 class="woo-map-section-title">Dispatch tracking</h3>
                        <p class="woo-map-section-note"><asp:Literal ID="litGeneralAppendTrackingNote" runat="server" /></p>
                        <div class="filter-toolbar">
                            <div class="filter-section">
                                <div class="filter-control">
                                    <asp:CheckBox ID="chkGeneralAppendTracking" runat="server" />
                                </div>
                            </div>
                        </div>

                        <h3 class="woo-map-section-title">Order Import auto-pull</h3>
                        <p class="woo-map-section-note"><asp:Literal ID="litGeneralAutoPullNote" runat="server" /></p>
                        <div class="filter-toolbar">
                            <div class="filter-section">
                                <div class="filter-control">
                                    <asp:Label ID="lblGeneralAutoPull" runat="server" AssociatedControlID="ddlGeneralAutoPull" />
                                    <asp:DropDownList ID="ddlGeneralAutoPull" runat="server" CssClass="sys-prefs-input" />
                                </div>
                            </div>
                        </div>

                        <div class="button-row">
                            <asp:Button ID="btnSaveGeneralSettings" runat="server" CssClass="filter-panel-btn"
                                OnClick="btnSaveGeneralSettings_Click" CausesValidation="false"
                                OnClientClick="return wooMapPrepareSave(this);" data-woo-save-ready="0" />
                        </div>
                    </asp:View>
                </asp:MultiView>

                <asp:DropDownList ID="ddlItemLookup" runat="server" Visible="false" EnableViewState="false" />

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
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
