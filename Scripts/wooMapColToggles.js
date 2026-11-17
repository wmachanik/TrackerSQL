/* Woo Mapping — Mappings tab column visibility (CSS :has on #wooMapPullHost; this file = localStorage + overflow tips). */
(function (w) {
    'use strict';

    var specs = [
        { id: 'wooMapShowName', key: 'wooMapShowName' },
        { id: 'wooMapShowPack', key: 'wooMapShowPack' },
        { id: 'wooMapShowMode', key: 'wooMapShowMode' },
        { id: 'wooMapShowVariant', key: 'wooMapShowVariant' },
        { id: 'wooMapShowMatch', key: 'wooMapShowMatch' },
        { id: 'wooMapShowCreate', key: 'wooMapShowCreate' }
    ];

    w.wooMapOverflowTips = function () {
        var host = document.getElementById('wooMapPullHost');
        if (!host) return;
        var grid = host.querySelector('table.woo-map-pull-grid');
        if (!grid) return;
        var cells = grid.querySelectorAll('td');
        for (var i = 0; i < cells.length; i++) {
            var td = cells[i];
            if (!td || td.offsetParent === null) continue;
            var sel = td.querySelector('select');
            var text = sel
                ? (sel.options[sel.selectedIndex] ? sel.options[sel.selectedIndex].text : sel.value)
                : (td.innerText || td.textContent || '').replace(/\s+/g, ' ').trim();
            if (!text) {
                if (!td.getAttribute('data-woo-tip-fixed'))
                    td.removeAttribute('title');
                continue;
            }
            if (td.scrollWidth > td.clientWidth + 2 || (sel && sel.scrollWidth > sel.clientWidth + 2))
                td.setAttribute('title', text);
            else if (td.getAttribute('data-woo-tip'))
                td.setAttribute('title', td.getAttribute('data-woo-tip'));
            else if (!td.getAttribute('data-woo-tip-fixed'))
                td.removeAttribute('title');
        }
    };

    function loadPrefs() {
        for (var i = 0; i < specs.length; i++) {
            var el = document.getElementById(specs[i].id);
            if (!el) continue;
            try {
                var stored = w.localStorage.getItem(specs[i].key);
                if (stored === '0' || stored === '1')
                    el.checked = stored === '1';
            } catch (ex) { }
        }
    }

    function bindOnce() {
        for (var i = 0; i < specs.length; i++) {
            var spec = specs[i];
            var el = document.getElementById(spec.id);
            if (!el || el.getAttribute('data-woo-col-bound') === '1') continue;
            el.setAttribute('data-woo-col-bound', '1');
            el.addEventListener('change', function () {
                var box = this;
                for (var j = 0; j < specs.length; j++) {
                    if (specs[j].id === box.id) {
                        try { w.localStorage.setItem(specs[j].key, box.checked ? '1' : '0'); } catch (ex) { }
                        break;
                    }
                }
                w.wooMapOverflowTips();
                w.setTimeout(w.wooMapOverflowTips, 80);
            }, false);
        }
    }

    w.wooMapColTogglesInit = function () {
        loadPrefs();
        bindOnce();
        w.wooMapOverflowTips();
    };

    function boot() {
        if (document.getElementById('wooMapPullHost'))
            w.wooMapColTogglesInit();
    }

    if (document.readyState === 'loading')
        document.addEventListener('DOMContentLoaded', boot);
    else
        boot();

    if (w.Sys && w.Sys.WebForms && w.Sys.WebForms.PageRequestManager) {
        w.Sys.WebForms.PageRequestManager.getInstance().add_endRequest(boot);
    }
})(window);