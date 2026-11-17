/* Global blocking please-wait for Web Forms async and full postbacks (Site.Master). */
(function (window) {
    'use strict';

    var activeCount = 0;
    var DEFAULT_MSG = 'Please wait\u2026';

    function overlayEl() {
        return document.getElementById('trackerWaitOverlay');
    }

    function labelEl() {
        return document.getElementById('trackerWaitLabel');
    }

    function ensureEllipsis(text) {
        var t = (text || '').trim();
        if (!t) return DEFAULT_MSG;
        if (/\u2026$|\.\.\.$/.test(t)) return t.replace(/\.\.\.$/, '\u2026');
        return t + '\u2026';
    }

    function attrWait(el) {
        if (!el || !el.getAttribute) return null;
        var v = el.getAttribute('data-tracker-wait');
        return v && v.trim() ? v.trim() : null;
    }

    function closestWait(el) {
        var node = el;
        while (node && node.nodeType === 1) {
            var v = attrWait(node);
            if (v) return v;
            node = node.parentElement;
        }
        return null;
    }

    function controlId(el) {
        return ((el && (el.id || el.name)) || '').toLowerCase();
    }

    function controlText(el) {
        if (!el) return '';
        if (el.tagName === 'INPUT') {
            var type = (el.type || '').toLowerCase();
            if (type === 'button' || type === 'submit')
                return (el.value || '').trim();
            return '';
        }
        if (el.tagName === 'BUTTON' || el.tagName === 'A')
            return (el.innerText || el.textContent || '').trim();
        if (el.tagName === 'IMG' || el.tagName === 'INPUT' && (el.type || '').toLowerCase() === 'image')
            return (el.alt || el.title || '').trim();
        return (el.getAttribute && (el.getAttribute('aria-label') || el.title) || '').trim();
    }

    function looksLikeIdOrKey(text) {
        var t = (text || '').trim();
        if (!t) return true;
        if (/^\d+$/.test(t)) return true;
        if (/^[0-9a-f-]{8,}$/i.test(t)) return true;
        if (/^(none|0|--)/i.test(t)) return true;
        return false;
    }

    function messageFromId(id) {
        if (!id) return null;
        if (id.indexOf('deliverto') >= 0 || id.indexOf('oddeliverto') >= 0)
            return 'Updating delivery address';
        if (id.indexOf('cbocontacts') >= 0 || id.indexOf('cbo_contacts') >= 0
            || (id.indexOf('contact') >= 0 && (id.indexOf('combo') >= 0 || id.indexOf('cbo') >= 0)))
            return 'Loading contact';
        if (id.indexOf('verify') >= 0 && id.indexOf('shop') >= 0)
            return 'Verifying shop login';
        if (id.indexOf('quaffee') >= 0 || id.indexOf('lover') >= 0)
            return 'Updating Quaffee Lovers';
        if (id.indexOf('woo') >= 0 && id.indexOf('check') >= 0)
            return 'Checking shop';
        if (id.indexOf('invite') >= 0)
            return 'Sending invite';
        if (id.indexOf('import') >= 0)
            return 'Importing';
        if (id.indexOf('save') >= 0)
            return 'Saving';
        if (id.indexOf('delete') >= 0 || id.indexOf('remove') >= 0)
            return 'Deleting';
        if (id.indexOf('refresh') >= 0 || id.indexOf('reload') >= 0)
            return 'Refreshing';
        if (id.indexOf('filter') >= 0 || id.indexOf('search') >= 0)
            return 'Searching';
        if (id.indexOf('send') >= 0 || id.indexOf('mail') >= 0)
            return 'Sending';
        if (id.indexOf('add') >= 0 && id.indexOf('item') >= 0)
            return 'Adding item';
        if (id.indexOf('prep') >= 0 || id.indexOf('requiredby') >= 0 || id.indexOf('deliverydate') >= 0)
            return 'Updating dates';
        if (id.indexOf('tobedelivered') >= 0 || id.indexOf('deliveredby') >= 0)
            return 'Updating delivery person';
        return null;
    }

    function messageFromLabel(text) {
        var lower = (text || '').toLowerCase();
        if (!lower || looksLikeIdOrKey(text)) return null;
        if (lower.indexOf('save') >= 0) return 'Saving';
        if (lower.indexOf('send') >= 0) return 'Sending';
        if (lower.indexOf('import') >= 0) return 'Importing';
        if (lower.indexOf('delete') >= 0 || lower.indexOf('remove') >= 0) return 'Deleting';
        if (lower.indexOf('filter') >= 0 || lower.indexOf('apply') >= 0) return 'Applying';
        if (lower.indexOf('refresh') >= 0 || lower.indexOf('reload') >= 0) return 'Refreshing';
        if (lower.indexOf('load') >= 0 || lower.indexOf('check') >= 0) return 'Loading';
        if (lower.indexOf('invite') >= 0) return 'Sending invite';
        if (lower.indexOf('verify') >= 0) return 'Verifying';
        if (lower.indexOf('force') >= 0 && lower.indexOf('member') >= 0) return 'Granting member';
        if (lower.indexOf('disable') >= 0 && lower.indexOf('member') >= 0) return 'Disabling member';
        if (text.length > 0 && text.length < 40) return text.replace(/\.\.\.$/, '');
        return null;
    }

    function messageFromElement(el) {
        if (!el) return DEFAULT_MSG;

        var explicit = closestWait(el);
        if (explicit) return ensureEllipsis(explicit);

        var idMsg = messageFromId(controlId(el));
        if (idMsg) return ensureEllipsis(idMsg);

        var tag = (el.tagName || '').toUpperCase();
        if (tag === 'SELECT') {
            // Never show the option value (often a numeric id). Prefer selected text only when meaningful.
            var opt = el.options && el.selectedIndex >= 0 ? el.options[el.selectedIndex] : null;
            var optText = opt ? (opt.text || '').trim() : '';
            if (optText && !looksLikeIdOrKey(optText) && optText.length < 36)
                return ensureEllipsis('Updating \u2014 ' + optText);
            return ensureEllipsis('Updating');
        }

        // ComboBox / text inputs: value is often an id or typed search — do not display it.
        if (tag === 'INPUT') {
            var type = (el.type || '').toLowerCase();
            if (type === 'text' || type === 'hidden' || type === 'search' || type === 'number') {
                var fromId = messageFromId(controlId(el));
                return ensureEllipsis(fromId || 'Updating');
            }
        }

        var labelMsg = messageFromLabel(controlText(el));
        if (labelMsg) return ensureEllipsis(labelMsg);

        return DEFAULT_MSG;
    }

    function isWooMappingPage() {
        return !!document.querySelector('.woo-map-busy');
    }

    function isWooMapOwnWait(el) {
        if (!window.wooMapBusy) return isWooMappingPage();
        if (document.body && document.body.classList.contains('woo-map-quiet-postback')) return true;
        if (!el) return isWooMappingPage();
        var op = window.wooMapBusy.resolveOp(el);
        if (window.wooMapBusy.isQuiet(op)) return true;
        if (isWooMappingPage()) return true;
        return op && op !== 'local';
    }

    function shouldSkip(args) {
        var el = args && args.get_postBackElement ? args.get_postBackElement() : null;
        if (el && el.getAttribute && el.getAttribute('data-tracker-no-wait') === '1') return true;
        if (isWooMapOwnWait(el)) return true;
        if (document.body && document.body.classList.contains('woo-map-working')) return true;
        return false;
    }

    function show(msg) {
        var overlay = overlayEl();
        var label = labelEl();
        if (!overlay) return;
        activeCount++;
        if (label) label.textContent = msg || DEFAULT_MSG;
        overlay.setAttribute('aria-hidden', 'false');
        document.body.classList.add('tracker-wait-active');
    }

    function hide() {
        var overlay = overlayEl();
        if (!overlay) return;
        activeCount = Math.max(0, activeCount - 1);
        if (activeCount > 0) return;
        overlay.setAttribute('aria-hidden', 'true');
        document.body.classList.remove('tracker-wait-active');
    }

    function forceHide() {
        activeCount = 0;
        hide();
    }

    function bindPrm() {
        if (!window.Sys || !Sys.WebForms || !Sys.WebForms.PageRequestManager) return false;
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        if (prm._trackerWaitBound) return true;
        prm._trackerWaitBound = true;

        prm.add_beginRequest(function (sender, args) {
            if (shouldSkip(args)) return;
            window.__trackerAsyncPostback = true;
            show(messageFromElement(args.get_postBackElement()));
        });

        prm.add_endRequest(function () {
            window.__trackerAsyncPostback = false;
            if (document.body && document.body.classList.contains('woo-map-working')) return;
            forceHide();
        });
        return true;
    }

    function bindForm() {
        var form = document.getElementById('frmMain') || document.querySelector('form[method="post"]');
        if (!form || form._trackerWaitSubmitBound) return;
        form._trackerWaitSubmitBound = true;
        form.addEventListener('submit', function () {
            window.setTimeout(function () {
                if (window.__trackerAsyncPostback) return;
                if (document.body && document.body.classList.contains('woo-map-working')) return;
                if (isWooMappingPage()) return;
                if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) return;
                var el = document.activeElement;
                show(messageFromElement(el));
            }, 0);
        }, true);
    }

    function isSameSiteNavigation(href) {
        if (!href) return false;
        var h = href.trim();
        if (!h || h.charAt(0) === '#' || /^javascript:/i.test(h)) return false;
        try {
            var url = new URL(h, window.location.href);
            return url.origin === window.location.origin;
        } catch (e) {
            return h.charAt(0) === '/';
        }
    }

    function bindNavigationWait() {
        if (document._trackerNavWaitBound) return;
        document._trackerNavWaitBound = true;
        document.addEventListener('click', function (ev) {
            var target = ev.target;
            if (!target || !target.closest) return;
            var anchor = target.closest('a[href]');
            if (!anchor) return;
            if (anchor.getAttribute('data-tracker-no-wait') === '1') return;
            if (anchor.target && anchor.target.toLowerCase() === '_blank') return;
            if (ev.ctrlKey || ev.metaKey || ev.shiftKey || ev.altKey) return;
            var href = anchor.getAttribute('href') || '';
            if (!isSameSiteNavigation(href)) return;
            var msg = attrWait(anchor) || messageFromLabel(controlText(anchor)) || 'Loading page';
            show(ensureEllipsis(msg));
        }, true);

        window.addEventListener('pageshow', function (ev) {
            if (ev.persisted) forceHide();
        });
    }

    function init() {
        bindForm();
        bindNavigationWait();
        bindPrm();
    }

    window.TrackerWait = {
        show: show,
        hide: hide,
        forceHide: forceHide,
        messageFromElement: messageFromElement
    };

    if (window.Sys && Sys.Application) {
        Sys.Application.add_load(init);
    } else if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    var tries = 0;
    var retry = window.setInterval(function () {
        if (bindPrm() || ++tries > 40) window.clearInterval(retry);
    }, 250);
})(window);
