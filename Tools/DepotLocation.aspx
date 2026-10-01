<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DepotLocation.aspx.cs" Inherits="TrackerSQL.Tools.DepotLocation" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Depot location</title>
    <link rel="stylesheet" href="../Styles/Site.css?v=20261001-04" />
    <link rel="stylesheet" href="../Scripts/leaflet/leaflet.css" />
    <script src="../Scripts/leaflet/leaflet.js"></script>
    <script>
        if (!window.L) {
            document.write('<link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" />'
                + '<script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"><\/script>');
        }
    </script>
</head>
<body class="contact-location-body">
    <form id="frmDepotLocation" runat="server" defaultbutton="btnSearch">
        <asp:HiddenField ID="hdnLat" runat="server" />
        <asp:HiddenField ID="hdnLng" runat="server" />
        <asp:HiddenField ID="hdnHasPoint" runat="server" />
        <asp:HiddenField ID="hdnTileUrl" runat="server" />
        <asp:HiddenField ID="hdnAzureTiles" runat="server" />
        <asp:HiddenField ID="hdnDirty" runat="server" Value="0" />

        <div class="contact-location-head">
            <div>
                <strong>Depot</strong> <span class="contact-location-muted">(where every route starts and ends)</span><br />
                <span class="contact-location-muted">Depot address: <asp:Literal ID="litAddress" runat="server" /></span><br />
                <span class="contact-location-muted">Found: <asp:Literal ID="litMatched" runat="server" /></span>
            </div>
            <div class="contact-location-state">
                <span class="contact-location-muted">Saved point: <asp:Literal ID="litSaved" runat="server" /></span><br />
                <span class="contact-location-muted">Pin: <span id="pinText"><asp:Literal ID="litPoint" runat="server" /></span></span>
            </div>
        </div>

        <asp:Label ID="lblMessage" runat="server" CssClass="status-message" Visible="false" />

        <div class="contact-location-tools">
            <label>
                GPS co-ordinates
                <input type="text" id="txtCoords" class="contact-location-input" placeholder="-34.02345, 18.44321"
                    title="Type or paste latitude, longitude (or a Google Maps link) and press Go" />
            </label>
            <button type="button" id="btnCoordsGo" class="filter-panel-btn contact-postal-suggest">Go</button>
            <label>
                Search address
                <asp:TextBox ID="txtSearch" runat="server" CssClass="contact-location-input contact-location-search"
                    ToolTip="Search for an address or place (uses one lookup); the pin moves there, then press Save depot point" />
            </label>
            <asp:Button ID="btnSearch" runat="server" CssClass="filter-panel-btn contact-postal-suggest" Text="Search" OnClick="btnSearch_Click" />
        </div>

        <div id="depotLocationMap" class="contact-location-map"></div>

        <div class="contact-location-actions">
            <asp:Button ID="btnSavePin" runat="server" CssClass="filter-panel-btn" Text="Save depot point" OnClick="btnSavePin_Click"
                ToolTip="Saves the pin as the depot point and closes this window" />
            <span class="contact-location-muted">Drag the pin, click the map, type co-ordinates or search, then press <em>Save depot point</em>. Satellite view helps find the gate.</span>
            <button type="button" class="filter-panel-btn sys-prefs-secondary-btn"
                onclick="if (window.parent && window.parent.trackerCloseDepot) window.parent.trackerCloseDepot();">Close</button>
        </div>

        <div id="depotUnsaved" class="contact-location-unsaved" style="display:none;" role="alertdialog" aria-label="Pin not saved">
            <div class="contact-location-unsaved-box">
                <p><strong>The pin was moved but not saved.</strong></p>
                <div class="contact-location-unsaved-buttons">
                    <button type="button" class="filter-panel-btn" id="btnUnsavedSave">Save and close</button>
                    <button type="button" class="filter-panel-btn sys-prefs-secondary-btn" id="btnUnsavedDiscard">Close without saving</button>
                    <button type="button" class="filter-panel-btn sys-prefs-secondary-btn" id="btnUnsavedStay">Keep editing</button>
                </div>
            </div>
        </div>
    </form>

    <script>
        (function () {
            function byId(id) { return document.getElementById(id); }
            var dirtyField = byId('<%= hdnDirty.ClientID %>');
            function markDirty() { dirtyField.value = '1'; }
            var unsaved = byId('depotUnsaved');
            window.depotRequestClose = function () {
                if (dirtyField.value !== '1') return true;
                unsaved.style.display = 'flex';
                return false;
            };
            byId('btnUnsavedSave').addEventListener('click', function () {
                unsaved.style.display = 'none';
                byId('<%= btnSavePin.ClientID %>').click();
            });
            byId('btnUnsavedDiscard').addEventListener('click', function () {
                dirtyField.value = '0';
                if (window.parent && window.parent.trackerCloseDepot) window.parent.trackerCloseDepot(true);
            });
            byId('btnUnsavedStay').addEventListener('click', function () { unsaved.style.display = 'none'; });

            var latField = byId('<%= hdnLat.ClientID %>');
            var lngField = byId('<%= hdnLng.ClientID %>');
            var hasPoint = byId('<%= hdnHasPoint.ClientID %>').value === '1';
            var tileUrl = byId('<%= hdnTileUrl.ClientID %>').value || 'https://tile.openstreetmap.org/{z}/{x}/{y}.png';
            var mapEl = byId('depotLocationMap');
            if (!window.L || !mapEl) {
                if (mapEl) mapEl.innerHTML = '<p class="contact-location-muted">The map could not be loaded.</p>';
                return;
            }

            var lat = parseFloat(latField.value) || -34.02;
            var lng = parseFloat(lngField.value) || 18.44;
            var map = L.map(mapEl).setView([lat, lng], hasPoint ? 17 : 11);
            function addOsm() {
                L.tileLayer(tileUrl, {
                    maxZoom: 19,
                    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright" target="_blank" rel="noopener">OpenStreetMap</a> contributors'
                }).addTo(map);
            }
            var azureTiles = byId('<%= hdnAzureTiles.ClientID %>').value;
            if (azureTiles) {
                var azureAttribution = '&copy; ' + new Date().getFullYear() + ' Microsoft Azure Maps, TomTom';
                function azureLayer(style) {
                    return L.tileLayer(azureTiles + '?style=' + style + '&z={z}&x={x}&y={y}', { maxZoom: 19, attribution: azureAttribution });
                }
                var road = azureLayer('road').addTo(map);
                var satellite = L.layerGroup([azureLayer('satellite'), azureLayer('labels')]);
                var layers = L.control.layers({ 'Road': road, 'Satellite': satellite }, null, { collapsed: false }).addTo(map);
                // Azure refused (key, daily cap): show OpenStreetMap instead of a blank map.
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

            var marker = L.marker([lat, lng], { draggable: true }).addTo(map);
            function setPoint(p) {
                latField.value = p.lat.toFixed(6);
                lngField.value = p.lng.toFixed(6);
                var t = byId('pinText');
                if (t) t.textContent = p.lat.toFixed(5) + ', ' + p.lng.toFixed(5);
            }
            marker.on('dragend', function () { setPoint(marker.getLatLng()); markDirty(); });
            map.on('click', function (e) { marker.setLatLng(e.latlng); setPoint(e.latlng); markDirty(); });

            // "-34.02, 18.44", "-34.02 18.44", or a Google Maps link (@lat,lng / q=lat,lng / !3dlat!4dlng)
            function parseCoords(text) {
                var s = (text || '').trim();
                var m = s.match(/!3d(-?\d+(?:\.\d+)?)!4d(-?\d+(?:\.\d+)?)/)
                    || s.match(/[@=](-?\d+(?:\.\d+)?),\s*(-?\d+(?:\.\d+)?)/)
                    || s.match(/^\s*(-?\d+(?:\.\d+)?)\s*[,;\s]\s*(-?\d+(?:\.\d+)?)\s*$/);
                if (!m) return null;
                var la = parseFloat(m[1]), ln = parseFloat(m[2]);
                if (isNaN(la) || isNaN(ln) || la < -90 || la > 90 || ln < -180 || ln > 180) return null;
                return L.latLng(la, ln);
            }
            var coordsBox = byId('txtCoords');
            function goToCoords() {
                var p = parseCoords(coordsBox.value);
                if (!p) { alert('Type latitude, longitude, e.g. -34.02345, 18.44321'); return; }
                marker.setLatLng(p);
                setPoint(p);
                markDirty();
                map.setView(p, 17);
            }
            byId('btnCoordsGo').addEventListener('click', goToCoords);
            coordsBox.addEventListener('keydown', function (e) {
                if (e.key === 'Enter') { e.preventDefault(); goToCoords(); }
            });
        })();
    </script>
</body>
</html>
