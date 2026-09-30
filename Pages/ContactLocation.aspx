<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ContactLocation.aspx.cs" Inherits="TrackerSQL.Pages.ContactLocation" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Customer GPS location</title>
    <link rel="stylesheet" href="../Styles/Site.css?v=20260930-01" />
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
    <form id="frmContactLocation" runat="server">
        <asp:HiddenField ID="hdnLat" runat="server" />
        <asp:HiddenField ID="hdnLng" runat="server" />
        <asp:HiddenField ID="hdnHasPoint" runat="server" />
        <asp:HiddenField ID="hdnTileUrl" runat="server" />

        <div class="contact-location-head">
            <div>
                <strong><asp:Literal ID="litContactName" runat="server" /></strong><br />
                <span class="contact-location-muted">Address looked up: <asp:Literal ID="litAddress" runat="server" /></span><br />
                <span class="contact-location-muted">Found: <asp:Literal ID="litMatched" runat="server" /></span>
            </div>
            <div class="contact-location-state">
                <asp:Label ID="lblState" runat="server" CssClass="contact-gps-value" /><br />
                <span class="contact-location-muted">Pin: <span id="pinText"><asp:Literal ID="litPoint" runat="server" /></span></span>
            </div>
        </div>

        <asp:Label ID="lblMessage" runat="server" CssClass="status-message" Visible="false" />

        <div id="contactLocationMap" class="contact-location-map"></div>
        <p class="contact-location-muted">Drag the pin (or click the map) to where the driver should stop, then press <em>Save this pin</em>.</p>

        <div class="button-row">
            <asp:Button ID="btnSavePin" runat="server" CssClass="filter-panel-btn" Text="Save this pin" OnClick="btnSavePin_Click"
                ToolTip="Marks the location as checked" />
            <asp:Button ID="btnLookupAgain" runat="server" CssClass="filter-panel-btn sys-prefs-secondary-btn" Text="Look up again"
                OnClick="btnLookupAgain_Click" ToolTip="Looks the address up again (uses one of today's lookups)" />
        </div>
    </form>

    <script>
        (function () {
            function byId(id) { return document.getElementById(id); }
            var latField = byId('<%= hdnLat.ClientID %>');
            var lngField = byId('<%= hdnLng.ClientID %>');
            var hasPoint = byId('<%= hdnHasPoint.ClientID %>').value === '1';
            var tileUrl = byId('<%= hdnTileUrl.ClientID %>').value || 'https://tile.openstreetmap.org/{z}/{x}/{y}.png';
            var mapEl = byId('contactLocationMap');
            if (!window.L || !mapEl) {
                if (mapEl) mapEl.innerHTML = '<p class="contact-location-muted">The map could not be loaded.</p>';
                return;
            }

            var lat = parseFloat(latField.value) || -34.02;
            var lng = parseFloat(lngField.value) || 18.44;
            var map = L.map(mapEl).setView([lat, lng], hasPoint ? 17 : 12);
            L.tileLayer(tileUrl, {
                maxZoom: 19,
                attribution: '&copy; <a href="https://www.openstreetmap.org/copyright" target="_blank" rel="noopener">OpenStreetMap</a> contributors'
            }).addTo(map);

            var marker = L.marker([lat, lng], { draggable: true }).addTo(map);
            function setPoint(p) {
                latField.value = p.lat.toFixed(6);
                lngField.value = p.lng.toFixed(6);
                var t = byId('pinText');
                if (t) t.textContent = p.lat.toFixed(5) + ', ' + p.lng.toFixed(5);
            }
            marker.on('dragend', function () { setPoint(marker.getLatLng()); });
            map.on('click', function (e) { marker.setLatLng(e.latlng); setPoint(e.latlng); });
        })();
    </script>
</body>
</html>
