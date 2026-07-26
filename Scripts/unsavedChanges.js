/**
 * TrackerUnsaved — shared unsaved-changes guard for WebForms pages.
 *
 * Include once (Site.Master). Each page initialises with local options:
 *
 *   TrackerUnsaved.init({
 *     dirtyFieldId: '...',          // hidden field ClientID ("1"/"0")
 *     rootId: '...',                // optional: wire all inputs under this element
 *     fieldIds: ['id1','id2'],      // optional: wire these ClientIDs only
 *     leaveMessage: '...',          // confirmLeave() prompt
 *     unsavedStatusMessage: '...',  // optional status strip text when dirty
 *     statusLiteralId: '...',
 *     statusPanelId: '...',
 *     saveButtonSelector: '.save-btn', // optional: enable only when dirty
 *     aliases: {                    // optional window.* names for page/C# compat
 *       markDirty: 'contactDetailsMarkDirty',
 *       clearDirty: 'contactDetailsClearDirty',
 *       allowNavigate: 'contactDetailsAllowNavigate',
 *       confirmLeave: 'contactDetailsConfirmLeave',
 *       wireFields: 'contactDetailsWireFields'
 *     }
 *   });
 *
 * API: markDirty(), clearDirty(), allowNavigate(), confirmLeave(),
 *      wireFields(optionalFieldIds), isDirty(), restoreGuard()
 */
(function (window, document) {
    'use strict';

    var cfg = null;
    var dirty = false;
    var allowNavigate = false;
    var endRequestAttached = false;
    var WIRED_ATTR = 'data-tracker-unsaved-wired';

    function byId(id) {
        if (!id) {
            return null;
        }
        return document.getElementById(id);
    }

    function getHidden() {
        return cfg ? byId(cfg.dirtyFieldId) : null;
    }

    function setSaveButtonsEnabled(enabled) {
        if (!cfg || !cfg.saveButtonSelector) {
            return;
        }
        var nodes = document.querySelectorAll(cfg.saveButtonSelector);
        for (var i = 0; i < nodes.length; i++) {
            nodes[i].disabled = !enabled;
        }
    }

    function showUnsavedStatus() {
        if (!cfg || !cfg.unsavedStatusMessage) {
            return;
        }
        var statusLiteral = byId(cfg.statusLiteralId);
        var statusPanel = byId(cfg.statusPanelId);
        if (statusLiteral) {
            statusLiteral.innerHTML = cfg.unsavedStatusMessage;
        }
        if (statusPanel) {
            statusPanel.className = 'status-message status-info';
        }
    }

    function updateUi() {
        if (cfg && cfg.saveButtonSelector) {
            setSaveButtonsEnabled(dirty);
        }
        if (dirty) {
            showUnsavedStatus();
        }
        if (cfg && typeof cfg.onDirtyChange === 'function') {
            cfg.onDirtyChange(dirty);
        }
    }

    function setDirty(isDirty) {
        dirty = !!isDirty;
        var hidden = getHidden();
        if (hidden) {
            hidden.value = dirty ? '1' : '0';
        }
        // Keep OrderDetail-style global flag in sync when used
        window.TrackerUnsavedAllowNavigate = allowNavigate;
        updateUi();
    }

    function syncDirtyFromHidden() {
        var hidden = getHidden();
        dirty = !!(hidden && hidden.value === '1');
    }

    function onBeforeUnload(e) {
        if (allowNavigate || window.TrackerUnsavedAllowNavigate || !dirty) {
            return undefined;
        }
        e.preventDefault();
        e.returnValue = '';
        return '';
    }

    function restoreGuard() {
        allowNavigate = false;
        window.TrackerUnsavedAllowNavigate = false;
        window.removeEventListener('beforeunload', onBeforeUnload);
        window.addEventListener('beforeunload', onBeforeUnload);
    }

    function markDirty() {
        setDirty(true);
    }

    function clearDirty() {
        setDirty(false);
    }

    function allowNavigateFn() {
        allowNavigate = true;
        window.TrackerUnsavedAllowNavigate = true;
        window.removeEventListener('beforeunload', onBeforeUnload);
        window.onbeforeunload = null;
        return true;
    }

    function confirmLeave() {
        var message = (cfg && cfg.leaveMessage)
            ? cfg.leaveMessage
            : 'You have unsaved changes. Leave without saving?';

        if (!dirty) {
            allowNavigateFn();
            return true;
        }
        if (window.confirm(message)) {
            allowNavigateFn();
            return true;
        }
        return false;
    }

    function wireOneField(field) {
        if (!field || field.getAttribute(WIRED_ATTR) === '1') {
            return;
        }
        field.setAttribute(WIRED_ATTR, '1');
        field.addEventListener('change', markDirty);
        field.addEventListener('input', markDirty);
    }

    function wireFields(fieldIds) {
        var ids = fieldIds;
        if ((!ids || !ids.length) && cfg && cfg.fieldIds && cfg.fieldIds.length) {
            ids = cfg.fieldIds;
        }

        if (ids && ids.length) {
            for (var i = 0; i < ids.length; i++) {
                wireOneField(byId(ids[i]));
            }
            return;
        }

        if (!cfg || !cfg.rootId) {
            return;
        }

        var root = byId(cfg.rootId);
        if (!root) {
            return;
        }

        var fields = root.querySelectorAll(
            'input:not([type=hidden]):not([type=submit]):not([type=button]):not([type=image]), select, textarea');
        for (var j = 0; j < fields.length; j++) {
            wireOneField(fields[j]);
        }
    }

    function attachEndRequest() {
        if (endRequestAttached) {
            return true;
        }
        if (!window.Sys || !Sys.WebForms || !Sys.WebForms.PageRequestManager) {
            return false;
        }

        var prm = Sys.WebForms.PageRequestManager.getInstance();
        prm.add_endRequest(function () {
            wireFields();
            syncDirtyFromHidden();
            if (dirty) {
                restoreGuard();
            }
            updateUi();
        });
        endRequestAttached = true;
        return true;
    }

    function ensureEndRequest() {
        if (endRequestAttached) {
            return;
        }
        if (!attachEndRequest()) {
            window.setTimeout(function () {
                attachEndRequest();
            }, 50);
        }
    }

    function applyAliases(aliases) {
        if (!aliases) {
            return;
        }
        if (aliases.markDirty) {
            window[aliases.markDirty] = markDirty;
        }
        if (aliases.clearDirty) {
            window[aliases.clearDirty] = clearDirty;
        }
        if (aliases.allowNavigate) {
            window[aliases.allowNavigate] = allowNavigateFn;
        }
        if (aliases.confirmLeave) {
            window[aliases.confirmLeave] = confirmLeave;
        }
        if (aliases.wireFields) {
            window[aliases.wireFields] = wireFields;
        }
        // OrderDetail used a separate "from server" name
        if (aliases.markDirtyFromServer) {
            window[aliases.markDirtyFromServer] = markDirty;
        }
    }

    function init(options) {
        cfg = options || {};
        dirty = false;
        allowNavigate = false;
        window.TrackerUnsavedAllowNavigate = false;

        applyAliases(cfg.aliases);

        function start() {
            window.addEventListener('beforeunload', onBeforeUnload);
            wireFields();
            syncDirtyFromHidden();
            if (dirty) {
                updateUi();
            }
            ensureEndRequest();
        }

        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', start);
        } else {
            start();
        }

        return api;
    }

    var api = {
        init: init,
        markDirty: markDirty,
        clearDirty: clearDirty,
        allowNavigate: allowNavigateFn,
        confirmLeave: confirmLeave,
        wireFields: wireFields,
        restoreGuard: restoreGuard,
        isDirty: function () { return dirty; },
        syncFromHidden: syncDirtyFromHidden
    };

    window.TrackerUnsaved = api;
})(window, document);
