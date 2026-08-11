/**
 * Ajax Control Toolkit ComboBox option lists often open off-screen or clipped
 * inside overflow/grid/UpdatePanel ancestors. Park visible *_OptionList on
 * document.body with position:fixed under the matching textbox.
 */
(function (window, document) {
    'use strict';

    var pending = null;
    var homeParents = {};

    function scheduleFix() {
        if (pending) {
            window.cancelAnimationFrame(pending);
        }
        pending = window.requestAnimationFrame(function () {
            pending = null;
            fixAllVisibleLists();
        });
    }

    function scheduleBurst() {
        scheduleFix();
        window.setTimeout(scheduleFix, 0);
        window.setTimeout(scheduleFix, 30);
        window.setTimeout(scheduleFix, 100);
        window.setTimeout(scheduleFix, 250);
    }

    function listLooksOpen(list) {
        if (!list) return false;
        var inline = list.getAttribute('style') || '';
        if (/display\s*:\s*none/i.test(inline)) return false;
        var style = window.getComputedStyle(list);
        if (style.display === 'none' || style.visibility === 'hidden') return false;
        // Toolkit leaves closed lists display:none; open ones are block/list-item
        return style.display !== 'none';
    }

    function findTextBoxForList(list) {
        if (list && list.id) {
            var byConvention = document.getElementById(list.id.replace(/_OptionList$/i, '_TextBox'));
            if (byConvention) return byConvention;

            var comboId = list.id.replace(/_OptionList$/i, '');
            var comboRoot = document.getElementById(comboId);
            if (comboRoot) {
                var inRoot = comboRoot.querySelector('input[type="text"]');
                if (inRoot) return inRoot;
            }
        }

        var root = list.closest ? list.closest('.ajax__combobox') : null;
        if (!root) root = list.parentElement;
        if (root) {
            var input = root.querySelector('input[type="text"]');
            if (input) return input;
        }

        var prev = list.previousElementSibling;
        while (prev) {
            if (prev.classList && prev.classList.contains('ajax__combobox')) {
                input = prev.querySelector('input[type="text"]');
                if (input) return input;
            }
            if (prev.querySelector) {
                input = prev.querySelector('.ajax__combobox input[type="text"], input[type="text"]');
                if (input) return input;
            }
            prev = prev.previousElementSibling;
        }

        var cell = list.parentElement;
        if (cell && cell.querySelector) {
            return cell.querySelector('.ajax__combobox input[type="text"], input[type="text"]');
        }
        return null;
    }

    function ensureOnBody(list) {
        if (!list || !list.id) return;
        if (list.parentNode === document.body) return;

        if (!homeParents[list.id]) {
            homeParents[list.id] = list.parentNode;
        }
        document.body.appendChild(list);
    }

    function restoreHome(list) {
        if (!list || !list.id) return;
        var home = homeParents[list.id];
        if (!home || !home.parentNode) return;
        if (list.parentNode === home) return;
        try {
            home.appendChild(list);
        } catch (e) {
            /* home may have been replaced by UpdatePanel */
            delete homeParents[list.id];
        }
    }

    function placeOptionList(list) {
        var input = findTextBoxForList(list);
        if (!input) return;

        ensureOnBody(list);

        var rect = input.getBoundingClientRect();
        if (rect.width < 1 && rect.height < 1) return;

        var width = Math.max(rect.width, 220);
        var top = rect.bottom + 1;
        var left = rect.left;

        var estimatedHeight = Math.min(240, list.scrollHeight || 240);
        if (top + estimatedHeight > window.innerHeight - 8 && rect.top > estimatedHeight + 8) {
            top = Math.max(8, rect.top - estimatedHeight - 1);
        }

        // Keep inside horizontal viewport
        if (left + width > window.innerWidth - 8) {
            left = Math.max(8, window.innerWidth - width - 8);
        }
        if (left < 8) left = 8;

        list.style.setProperty('position', 'fixed', 'important');
        list.style.setProperty('left', left + 'px', 'important');
        list.style.setProperty('top', top + 'px', 'important');
        list.style.setProperty('right', 'auto', 'important');
        list.style.setProperty('bottom', 'auto', 'important');
        list.style.setProperty('width', width + 'px', 'important');
        list.style.setProperty('min-width', width + 'px', 'important');
        list.style.setProperty('max-height', '240px', 'important');
        list.style.setProperty('z-index', '2147483000', 'important');
        list.style.setProperty('display', 'block', 'important');
        list.style.setProperty('visibility', 'visible', 'important');
        list.style.setProperty('opacity', '1', 'important');
        list.style.setProperty('background', '#fff', 'important');
        list.style.setProperty('color', '#292909', 'important');
        list.style.setProperty('border', '1px solid #4e664d', 'important');
        list.style.setProperty('box-shadow', '0 4px 14px rgba(0,0,0,0.18)', 'important');
        list.style.setProperty('overflow-y', 'auto', 'important');
        list.style.setProperty('overflow-x', 'hidden', 'important');
        list.style.setProperty('pointer-events', 'auto', 'important');
        list.style.setProperty('clip', 'auto', 'important');
        list.style.setProperty('clip-path', 'none', 'important');
    }

    function fixAllVisibleLists() {
        var lists = document.querySelectorAll('ul[id$="_OptionList"], ul.ajax__combobox_itemlist');
        for (var i = 0; i < lists.length; i++) {
            var list = lists[i];
            if (listLooksOpen(list)) {
                placeOptionList(list);
            } else if (list.parentNode === document.body) {
                restoreHome(list);
            }
        }
    }

    function eventNearCombo(t) {
        if (!t) return false;
        if (t.id && /(_Button|_TextBox|_OptionList)$/i.test(t.id)) return true;
        if (t.closest) {
            return !!(t.closest('.ajax__combobox') ||
                t.closest('ul[id$="_OptionList"]') ||
                t.closest('.ajax__combobox_buttoncontainer') ||
                t.closest('.ajax__combobox_textboxcontainer'));
        }
        return false;
    }

    function onDocEvent(e) {
        if (eventNearCombo(e.target)) {
            scheduleBurst();
        }
    }

    function startObserver() {
        if (!window.MutationObserver) return;
        var obs = new MutationObserver(function () {
            scheduleFix();
        });
        obs.observe(document.documentElement, {
            subtree: true,
            childList: true,
            attributes: true,
            attributeFilter: ['style', 'class']
        });
    }

    function boot() {
        document.addEventListener('click', onDocEvent, true);
        document.addEventListener('mousedown', onDocEvent, true);
        document.addEventListener('mouseup', onDocEvent, true);
        document.addEventListener('keyup', onDocEvent, true);
        document.addEventListener('keydown', onDocEvent, true);
        document.addEventListener('focusin', onDocEvent, true);
        window.addEventListener('scroll', scheduleFix, true);
        window.addEventListener('resize', scheduleFix);

        startObserver();

        if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                homeParents = {};
                scheduleBurst();
            });
        } else {
            // Script may load before ScriptManager — retry once
            window.setTimeout(function () {
                if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                        homeParents = {};
                        scheduleBurst();
                    });
                }
            }, 500);
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', boot);
    } else {
        boot();
    }

    window.TrackerComboBoxFix = {
        fix: fixAllVisibleLists
    };
})(window, document);
