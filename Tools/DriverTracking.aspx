<%@ Page Title="Driver tracking" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="DriverTracking.aspx.cs" Inherits="TrackerSQL.Tools.DriverTracking" %>

<asp:Content ID="cntTrackingHead" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" href="../Scripts/leaflet/leaflet.css" />
    <script src="../Scripts/leaflet/leaflet.js"></script>
    <script>
        if (!window.L) {
            document.write('<link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" />'
                + '<script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"><\/script>');
        }
    </script>
</asp:Content>

<asp:Content ID="cntTrackingBody" ContentPlaceHolderID="MainContent" runat="server">
    <div class="simpleForm page-tone-panel page-tone-sysdata driver-tracking-page">
        <div class="page-tone-header tool-card-header">
            <img class="tool-card-icon" src="../images/imgButtons/Compass.gif" alt="" />
            <div>
                <h1 class="page-tone-title">Driver tracking</h1>
                <p class="page-tone-subtitle">Where the drivers are today, the stops they have left and when they should get there. Refreshes every minute.</p>
            </div>
        </div>

        <asp:Label ID="lblMessage" runat="server" CssClass="status-message" Visible="false" />

        <asp:Panel ID="pnlBoard" runat="server">
            <asp:HiddenField ID="hdnDataUrl" runat="server" />
            <asp:HiddenField ID="hdnTileUrl" runat="server" />
            <asp:HiddenField ID="hdnAzureTiles" runat="server" />

            <div class="driver-tracking-bar">
                <span id="dtUpdated" class="driver-tracking-muted">Loading...</span>
                <span id="dtWarning" class="driver-tracking-warning"></span>
                <button type="button" id="dtRefresh" class="filter-panel-btn">Refresh now</button>
            </div>

            <div class="driver-tracking-layout">
                <div class="driver-tracking-side">
                    <table class="TblWhite small driver-tracking-list">
                        <thead>
                            <tr><th>Driver</th><th>Status</th><th>Last seen</th><th>Stops</th><th>Finish</th></tr>
                        </thead>
                        <tbody id="dtDrivers"></tbody>
                    </table>
                    <div id="dtStops" class="driver-tracking-stops"></div>
                </div>
                <div id="dtMap" class="driver-tracking-map"></div>
            </div>

            <p class="sys-prefs-phase-note driver-tracking-legend">
                <span><i class="driver-tracking-dot green"></i>within <asp:Literal ID="litFresh" runat="server" /> minutes</span>
                <span><i class="driver-tracking-dot orange"></i>within <asp:Literal ID="litStale" runat="server" /> minutes</span>
                <span><i class="driver-tracking-dot red"></i>older</span>
                <span><i class="driver-tracking-dot grey"></i>no position today</span>
            </p>
            <p class="sys-prefs-phase-note">
                Click a driver to see the stops left, numbered in route order, with straight lines between them. Arrival times use the
                straight-line distance &times; 1.3 at the average speed, plus the time per stop (System Preferences &rarr; GPS &amp; Drivers),
                starting from the driver's last position (or the depot). Stops without a GPS point have no time.
                Positions are only sent while tracking is on, inside the work days and hours, with the driver signed in to the app.
            </p>
        </asp:Panel>
    </div>

    <script>
        (function () {
            function byId(id) { return document.getElementById(id); }
            var dataField = byId('<%= hdnDataUrl.ClientID %>');
            var mapEl = byId('dtMap');
            if (!dataField || !mapEl) return;
            if (!window.L) {
                mapEl.innerHTML = '<p class="driver-tracking-muted">The map could not be loaded.</p>';
            }

            var dataUrl = dataField.value;
            var tileUrl = byId('<%= hdnTileUrl.ClientID %>').value || 'https://tile.openstreetmap.org/{z}/{x}/{y}.png';
            var azureTiles = byId('<%= hdnAzureTiles.ClientID %>').value;
            var colours = { green: '#2e9d4f', orange: '#e8902a', red: '#c83a32', grey: '#9a9a9a' };
            var selectedId = null, board = null, fitted = false, fittedFor = null;

            function esc(s) {
                return String(s == null ? '' : s).replace(/[&<>"']/g, function (c) {
                    return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c];
                });
            }

            var map = null, driversLayer = null, routeLayer = null, depotLayer = null;
            if (window.L) {
                map = L.map(mapEl).setView([-34.02, 18.45], 11);
                var osmAttribution = '&copy; <a href="https://www.openstreetmap.org/copyright" target="_blank" rel="noopener">OpenStreetMap</a> contributors';
                function addOsm() { L.tileLayer(tileUrl, { maxZoom: 19, attribution: osmAttribution }).addTo(map); }
                if (azureTiles) {
                    var azureAttribution = '&copy; ' + new Date().getFullYear() + ' Microsoft Azure Maps, TomTom';
                    function azureLayer(style) {
                        return L.tileLayer(azureTiles + '?style=' + style + '&z={z}&x={x}&y={y}', { maxZoom: 19, attribution: azureAttribution });
                    }
                    var road = azureLayer('road').addTo(map);
                    var satellite = L.layerGroup([azureLayer('satellite'), azureLayer('labels')]);
                    var layers = L.control.layers({ 'Road': road, 'Satellite': satellite }, null, { collapsed: true }).addTo(map);
                    var loaded = 0, failed = 0;
                    road.on('tileload', function () { loaded++; });
                    road.on('tileerror', function () {
                        if (++failed < 4 || loaded > 0) return;
                        road.off('tileerror');
                        map.removeLayer(road);
                        map.removeLayer(satellite);
                        map.removeControl(layers);
                        addOsm();
                    });
                } else {
                    addOsm();
                }
                depotLayer = L.layerGroup().addTo(map);
                routeLayer = L.layerGroup().addTo(map);
                driversLayer = L.layerGroup().addTo(map);
            }

            function ago(d) {
                if (d.minutesAgo == null) return 'not today';
                if (d.minutesAgo < 1) return d.lastSeen + ' (now)';
                if (d.minutesAgo < 60) return d.lastSeen + ' (' + d.minutesAgo + ' min ago)';
                return d.lastSeen + ' (' + Math.floor(d.minutesAgo / 60) + 'h ' + (d.minutesAgo % 60) + 'm ago)';
            }

            function sourceText(s) {
                return s === 'DELIVERY_COMPLETE' ? 'at a delivery' : s === 'APP_OPEN' ? 'app opened' : s ? 'regular update' : '';
            }

            function selected() {
                if (!board) return null;
                for (var i = 0; i < board.drivers.length; i++)
                    if (board.drivers[i].personId === selectedId) return board.drivers[i];
                return null;
            }

            function renderList() {
                var rows = board.drivers.map(function (d) {
                    return '<tr data-id="' + d.personId + '"' + (d.personId === selectedId ? ' class="selected"' : '') + '>'
                        + '<td><i class="driver-tracking-dot ' + esc(d.colour) + '"></i>' + esc(d.name) + '</td>'
                        + '<td>' + esc(d.status) + '</td>'
                        + '<td>' + esc(ago(d)) + '</td>'
                        + '<td>' + d.stopsDone + ' done, ' + d.stopsLeft + ' left</td>'
                        + '<td>' + esc(d.finishEta || '-') + '</td></tr>';
                });
                byId('dtDrivers').innerHTML = rows.length ? rows.join('')
                    : '<tr><td colspan="5" class="driver-tracking-muted">No drivers with stops or positions today.</td></tr>';
            }

            function renderStops() {
                var d = selected(), el = byId('dtStops');
                if (!d) { el.innerHTML = board.drivers.length ? '<p class="driver-tracking-muted">Click a driver to see their stops.</p>' : ''; return; }
                var html = '<h3>' + esc(d.name) + ': ' + d.stopsLeft + ' stop(s) left'
                    + (d.finishEta ? ', done about ' + esc(d.finishEta) : '') + '</h3>';
                if (d.stopsLeft > 0 && d.fromDepot) html += '<p class="driver-tracking-muted">No position today: times are from the depot.</p>';
                if (d.stopsNoPoint > 0) html += '<p class="driver-tracking-muted">' + d.stopsNoPoint + ' stop(s) have no GPS point and no time.</p>';
                if (d.stops.length) {
                    html += '<table class="TblWhite small"><thead><tr><th>#</th><th>Stop</th><th>Area</th><th>ETA</th></tr></thead><tbody>'
                        + d.stops.map(function (s) {
                            return '<tr' + (s.done ? ' class="done"' : '') + '><td>' + s.seq + '</td><td>' + esc(s.company)
                                + (s.orders > 1 ? ' (' + s.orders + ' orders)' : '') + '</td><td>' + esc(s.area || '') + '</td><td>'
                                + (s.done ? (s.notAtClient ? esc(s.notAtClient) : 'done') : esc(s.eta || '-')) + '</td></tr>';
                        }).join('') + '</tbody></table>';
                }
                el.innerHTML = html;
            }

            function stopIcon(s) {
                return L.divIcon({ className: 'driver-tracking-stop-icon' + (s.done ? ' done' : ''), html: '<span>' + s.seq + '</span>', iconSize: [26, 26], iconAnchor: [13, 13] });
            }

            function renderMap() {
                if (!map) return;
                driversLayer.clearLayers();
                routeLayer.clearLayers();
                depotLayer.clearLayers();
                var bounds = [];
                if (board.depotLat != null) {
                    L.marker([board.depotLat, board.depotLng], {
                        icon: L.divIcon({ className: 'driver-tracking-depot-icon', html: '<span>Depot</span>', iconSize: null, iconAnchor: [18, 9] }),
                        interactive: false
                    }).addTo(depotLayer);
                }
                board.drivers.forEach(function (d) {
                    if (d.lat == null) return;
                    var m = L.circleMarker([d.lat, d.lng], {
                        radius: d.personId === selectedId ? 11 : 9, color: '#fff', weight: 2,
                        fillColor: colours[d.colour] || colours.grey, fillOpacity: 0.95
                    }).addTo(driversLayer);
                    m.bindTooltip(esc(d.abbreviation || d.name), { permanent: true, direction: 'right', offset: [8, 0] });
                    m.bindPopup('<strong>' + esc(d.name) + '</strong><br/>' + esc(d.status) + '<br/>Last seen ' + esc(ago(d))
                        + (d.source ? ' - ' + esc(sourceText(d.source)) : '')
                        + '<br/>' + d.lat.toFixed(5) + ', ' + d.lng.toFixed(5) + (d.accuracy ? ' (&plusmn;' + Math.round(d.accuracy) + ' m)' : '')
                        + '<br/>' + d.stopsDone + ' done, ' + d.stopsLeft + ' left' + (d.finishEta ? ', finish about ' + esc(d.finishEta) : ''));
                    m.on('click', function () { select(d.personId); });
                    bounds.push([d.lat, d.lng]);
                });

                var sel = selected();
                if (sel) {
                    var line = [];
                    if (sel.lat != null) line.push([sel.lat, sel.lng]);
                    else if (board.depotLat != null) line.push([board.depotLat, board.depotLng]);
                    var selBounds = line.slice();
                    sel.stops.forEach(function (s) {
                        if (s.lat == null) return;
                        L.marker([s.lat, s.lng], { icon: stopIcon(s) })
                            .bindTooltip(esc(s.seq + '. ' + s.company) + (s.done ? ' (' + esc(s.notAtClient || 'done') + ')' : s.eta ? ' - ETA ' + esc(s.eta) : ''))
                            .addTo(routeLayer);
                        selBounds.push([s.lat, s.lng]);
                        if (!s.done) line.push([s.lat, s.lng]);
                    });
                    if (line.length > 1)
                        L.polyline(line, { color: '#3d5a80', weight: 3, opacity: 0.8, dashArray: '6 6' }).addTo(routeLayer);
                    if (fittedFor !== selectedId && selBounds.length) {
                        fittedFor = selectedId;
                        map.fitBounds(selBounds, { padding: [30, 30], maxZoom: 15 });
                    }
                } else if (!fitted) {
                    if (board.depotLat != null) bounds.push([board.depotLat, board.depotLng]);
                    if (bounds.length) {
                        fitted = true;
                        map.fitBounds(bounds, { padding: [30, 30], maxZoom: 14 });
                    }
                }
            }

            function render() {
                byId('dtUpdated').textContent = 'Updated ' + board.now + ' (' + board.day + ')';
                byId('dtWarning').textContent = board.trackingEnabled ? ''
                    : 'Driver tracking is off (System Preferences \u2192 GPS & Drivers), so there are no new positions.';
                renderList();
                renderStops();
                renderMap();
            }

            function select(id) {
                selectedId = selectedId === id ? null : id;
                fittedFor = null;
                if (selectedId === null) fitted = false;
                render();
            }

            byId('dtDrivers').addEventListener('click', function (e) {
                var tr = e.target.closest ? e.target.closest('tr[data-id]') : null;
                if (tr) select(parseInt(tr.getAttribute('data-id'), 10));
            });

            function load() {
                fetch(dataUrl, { credentials: 'same-origin', cache: 'no-store' })
                    .then(function (r) {
                        if (r.status === 401 || r.status === 403) throw new Error('You are signed out or may not see driver tracking.');
                        return r.json().then(function (j) { if (!r.ok) throw new Error(j && j.error || ('HTTP ' + r.status)); return j; });
                    })
                    .then(function (j) { board = j; render(); })
                    .catch(function (err) {
                        byId('dtUpdated').textContent = 'Could not refresh: ' + err.message + (board ? ' (showing ' + board.now + ')' : '');
                    });
            }

            byId('dtRefresh').addEventListener('click', load);
            load();
            setInterval(load, 60000);
        })();
    </script>
</asp:Content>
