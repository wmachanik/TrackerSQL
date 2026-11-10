/* Global blocking please-wait for Web Forms async and full postbacks (Site.Master). */
(function (window) {
    'use strict';

    var activeCount = 0;

    function overlayEl() {
        return document.getElementById('trackerWaitOverlay');
    }

    function labelEl() {
        return document.getElementById('trackerWaitLabel');
    }

    function messageFromElement(el) {
        if (!el) return 'Please wait\u2026';
        var val = (el.value || el.innerText || el.textContent || '').trim();
        var lower = val.toLowerCase();
        if (lower.indexOf('save') >= 0) return 'Saving\u2026';
        if (lower.indexOf('send') >= 0) return 'Sending\u2026';
        if (lower.indexOf('import') >= 0) return 'Importing\u2026';
        if (lower.indexOf('delete') >= 0) return 'Deleting\u2026';
        if (lower.indexOf('filter') >= 0 || lower.indexOf('apply') >= 0) return 'Applying\u2026';
        if (lower.indexOf('refresh') >= 0 || lower.indexOf('load') >= 0) return 'Loading\u2026';
        if (val.length > 0 && val.length < 48) return val.replace(/\.\.\.$/, '') + '\u2026';
        return 'Please wait\u2026';
    }

    function isWooMapOwnWait(el) {
        if (!el || !window.wooMapBusy) return false;
        if (document.body && document.body.classList.contains('woo-map-quiet-postback')) return true;
        var op = window.wooMapBusy.resolveOp(el);
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
        if (label && msg) label.textContent = msg;
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
                var el = document.activeElement;
                show(messageFromElement(el));
            }, 0);
        }, true);
    }

    function init() {
        bindForm();
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
