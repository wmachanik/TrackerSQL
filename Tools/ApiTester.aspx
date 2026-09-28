<%@ Page Title="Mobile API tester" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="ApiTester.aspx.cs" Inherits="TrackerSQL.Tools.ApiTester" %>

<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server" />
<asp:Content ID="Body" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlAccessDenied" runat="server" Visible="false" CssClass="status-message status-error">
        Administrators only.
    </asp:Panel>

    <asp:Panel ID="pnlMain" runat="server" CssClass="portal-admin-stack api-tester">
        <asp:Literal ID="litMessage" runat="server" />

        <div class="simpleForm page-tone-panel page-tone-sysdata">
            <div class="page-tone-header tool-card-header">
                <img class="tool-card-icon" src="../images/imgButtons/SystemSettingsWithGlobe.png" alt="" />
                <div>
                    <h1 class="page-tone-title">Mobile API tester</h1>
                    <p class="page-tone-subtitle">Sends the same requests the delivery driver app will send to <code><%= ApiBase %></code>, and shows exactly what comes back.</p>
                </div>
            </div>

            <div id="apiWhoami" class="api-whoami">
                <div>
                    <strong>Testing as:</strong> <span id="apiWhoamiText">nobody yet. Sign in on the <a href="#" onclick="apiTester.showTab('signin'); return false;">Sign in</a> tab first; every call is then sent as that user, just like the phone.</span>
                </div>
                <button type="button" id="apiSignOutBtn" class="filter-panel-btn" onclick="apiTester.signOut()" style="display:none">Sign out</button>
            </div>

            <div class="api-tabs" role="tablist">
                <button type="button" class="api-tab" data-tab="signin">1. Sign in</button>
                <button type="button" class="api-tab" data-tab="sheet">Delivery sheet</button>
                <button type="button" class="api-tab" data-tab="deliver">Deliver an order</button>
                <button type="button" class="api-tab" data-tab="repairs">Repairs</button>
                <button type="button" class="api-tab" data-tab="lookups">Contacts &amp; lookups</button>
                <button type="button" class="api-tab" data-tab="devices">Devices</button>
                <button type="button" class="api-tab" data-tab="log">Request log</button>
                <button type="button" class="api-tab" data-tab="help">Settings &amp; security</button>
            </div>

            <div class="api-tab-panel" data-panel="signin">
                <p class="api-tab-intro">Choose who the calls are sent as. The phone app signs in once with the driver's normal Tracker login and keeps the token it gets back for <%= TokenDays %> days.</p>
                <h4>Sign in as any user (like the phone)</h4>
                <p class="sys-prefs-help">Use a driver's login to see what they will see. The login must be linked to a person in Lookups → People, unless it is an administrator.</p>
                <div class="api-tester-row">
                    <label>Username or email <input type="text" id="apiUser" class="sys-prefs-input" autocomplete="off" /></label>
                    <label>Password <input type="password" id="apiPassword" class="sys-prefs-input" autocomplete="new-password" /></label>
                    <label>Device name <input type="text" id="apiDevice" class="sys-prefs-input" value="API tester" /></label>
                    <button type="button" class="filter-panel-btn" onclick="apiTester.login()">Sign in</button>
                </div>
                <h4>Or use yourself</h4>
                <p class="sys-prefs-help">Gets a token for the account you are signed in to Tracker with, without typing your password.</p>
                <asp:Button ID="btnWebToken" runat="server" CssClass="filter-panel-btn" Text="Use my web sign-in"
                    OnClick="btnWebToken_Click" CausesValidation="false" />
                <p class="sys-prefs-help">
                    <button type="button" class="filter-panel-btn" onclick="apiTester.call('GET','/auth/me')">Who am I? (auth/me)</button>
                    <button type="button" class="filter-panel-btn" onclick="apiTester.call('GET','/ping')">Ping (no sign-in needed)</button>
                </p>
            </div>

            <div class="api-tab-panel" data-panel="sheet">
                <p class="api-tab-intro">What the driver downloads before leaving, to use offline: each stop with the person to hand it to, their phone and address, and the items.</p>
                <div class="api-tester-row">
                    <label>Dates with deliveries
                        <select id="apiSheetDates" class="sys-prefs-input" onchange="if (this.value) document.getElementById('apiSheetDate').value = this.value;">
                            <option value="">(sign in to load)</option>
                        </select>
                    </label>
                    <label>Date <input type="date" id="apiSheetDate" class="sys-prefs-input" /></label>
                    <label>Whose deliveries
                        <select id="apiSheetPerson" class="sys-prefs-input">
                            <option value="me">Mine (the signed-in driver)</option>
                            <option value="all">Everyone's</option>
                        </select>
                    </label>
                    <button type="button" class="filter-panel-btn" onclick="apiTester.sheet()">Get delivery sheet</button>
                    <button type="button" class="filter-panel-btn" onclick="apiTester.dates(false)">Reload dates</button>
                </div>
                <p class="sys-prefs-help">The list shows today plus every date that still has orders not done, with how many are the signed-in driver's. A driver with nothing today gets an empty sheet for today.</p>
                <label class="api-tester-check"><input type="checkbox" class="apiUseETag" /> Only if changed: send the version (ETag) from last time, so an unchanged sheet comes back empty (304) and costs no data</label>
            </div>

            <div class="api-tab-panel" data-panel="deliver">
                <p class="api-tab-intro">What the phone sends when the driver hands over an order, possibly hours later once it has signal again.<asp:Literal ID="litSyncNote" runat="server" /></p>
                <div class="api-tester-row">
                    <label>Order id <input type="number" id="apiSyncOrder" class="sys-prefs-input" /></label>
                    <label>Outcome <select id="apiSyncOutcome" class="sys-prefs-input"><option value="Delivered">Delivered</option><option value="NotDelivered">Not delivered</option></select></label>
                    <label>Received by <input type="text" id="apiSyncName" class="sys-prefs-input" /></label>
                    <label>Driver note <input type="text" id="apiSyncNote" class="sys-prefs-input" /></label>
                </div>
                <div class="api-tester-row">
                    <canvas id="apiSignature" width="360" height="120" class="api-tester-signature" title="Sign here"></canvas>
                    <div class="api-tester-stack">
                        <button type="button" class="filter-panel-btn" onclick="apiTester.clearSignature()">Clear signature</button>
                        <button type="button" class="filter-panel-btn" onclick="apiTester.sync()">Send delivery</button>
                    </div>
                </div>
                <p class="sys-prefs-help">
                    Delivery reference <input type="text" id="apiSyncRef" class="sys-prefs-input" size="38" />
                    <button type="button" class="filter-panel-btn" onclick="apiTester.newRef()">New reference</button><br />
                    The phone makes this reference when the driver captures the delivery. Sending again with the same reference is reported as "already received", which is how a phone can safely retry after losing signal.
                </p>
                <h4>What has been captured</h4>
                <div class="api-tester-row">
                    <label>Date <input type="date" id="apiProofDate" class="sys-prefs-input" /></label>
                    <label>or order id <input type="number" id="apiProofOrder" class="sys-prefs-input" /></label>
                    <button type="button" class="filter-panel-btn" onclick="apiTester.proofs()">Show proof of delivery</button>
                </div>
            </div>

            <div class="api-tab-panel" data-panel="repairs">
                <p class="api-tab-intro">Driver collects or returns a machine: look up repairs, log a new one, or change its status.</p>
                <h4>Find repairs</h4>
                <div class="api-tester-row">
                    <label class="api-tester-check"><input type="checkbox" id="apiRepairsOpen" checked="checked" /> Open only</label>
                    <label>Changed since <input type="text" id="apiRepairsSince" class="sys-prefs-input" placeholder="2026-09-01T00:00:00" /></label>
                    <button type="button" class="filter-panel-btn" onclick="apiTester.repairs()">List repairs</button>
                    <label>Repair id <input type="number" id="apiRepairId" class="sys-prefs-input" /></label>
                    <button type="button" class="filter-panel-btn" onclick="apiTester.call('GET','/repairs/' + apiTester.val('apiRepairId'))">Get one repair</button>
                </div>
                <label class="api-tester-check"><input type="checkbox" class="apiUseETag" /> Only if changed (304 when nothing changed)</label>
                <h4>Create or update a repair</h4>
                <p class="sys-prefs-help"><strong>This saves for real</strong> and may email the contact. To create, leave Repair id empty and fill in contactId. To update, enter the Repair id and keep only the fields to change.</p>
                <label>Repair id (empty = new) <input type="number" id="apiSaveRepairId" class="sys-prefs-input" /></label>
                <textarea id="apiRepairJson" class="sys-prefs-input api-tester-json" rows="8">{
  "contactId": 0,
  "equipTypeId": null,
  "serial": "",
  "faultId": null,
  "faultDesc": "",
  "statusId": null,
  "notes": "Collected by driver"
}</textarea>
                <p class="sys-prefs-help">Ids for equipTypeId, faultId and statusId are listed under Contacts &amp; lookups → Lookups.</p>
                <button type="button" class="filter-panel-btn" onclick="apiTester.saveRepair()">Save repair</button>
            </div>

            <div class="api-tab-panel" data-panel="lookups">
                <p class="api-tab-intro">Lists the phone keeps (repair statuses, faults, machine types, conditions, people) and contact search.</p>
                <div class="api-tester-row">
                    <button type="button" class="filter-panel-btn" onclick="apiTester.call('GET','/lookups')">Lookups</button>
                    <label>Find contact <input type="text" id="apiContactQ" class="sys-prefs-input" placeholder="at least 2 letters" /></label>
                    <button type="button" class="filter-panel-btn" onclick="apiTester.call('GET','/contacts?q=' + encodeURIComponent(apiTester.val('apiContactQ')))">Search</button>
                    <label>Contact id <input type="number" id="apiContactId" class="sys-prefs-input" /></label>
                    <button type="button" class="filter-panel-btn" onclick="apiTester.call('GET','/contacts/' + apiTester.val('apiContactId'))">Get contact</button>
                </div>
                <label class="api-tester-check"><input type="checkbox" class="apiUseETag" /> Only if changed (304 when nothing changed)</label>
            </div>

            <div class="api-tab-panel" data-panel="devices">
                <p class="api-tab-intro">Every phone (or tester) that has signed in. If a phone is lost or a driver leaves, revoke its token and it is signed out on its next call. Administrators only.</p>
                <button type="button" class="filter-panel-btn" onclick="apiTester.devices()">List devices</button>
                <div id="apiDevices"></div>
            </div>

            <div class="api-tab-panel" data-panel="log">
                <p class="api-tab-intro">Every call made to the API, by the phones and by this page: who, what, the result, how long it took and how many bytes went each way. Only the facts about each call are kept, never what was sent (so no passwords, tokens or signatures). Kept for <asp:Literal ID="litLogDays" runat="server" /> days. Administrators only.</p>
                <div class="api-tester-row">
                    <label>User <input type="text" id="apiLogUser" class="sys-prefs-input" placeholder="everyone" /></label>
                    <label class="api-tester-check"><input type="checkbox" id="apiLogProblems" /> Problems only (errors, refused sign-ins)</label>
                    <label>Show <select id="apiLogMax" class="sys-prefs-input"><option>50</option><option selected="selected">200</option><option>1000</option></select></label>
                    <button type="button" class="filter-panel-btn" onclick="apiTester.loadLog()">Refresh</button>
                    <label class="api-tester-check"><input type="checkbox" id="apiLogAuto" onchange="apiTester.autoLog(this.checked)" /> Refresh every 5 seconds</label>
                </div>
                <div id="apiLog" class="api-log-wrap"></div>
            </div>

            <div class="api-tab-panel" data-panel="help">
                <h4>Current settings</h4>
                <ul class="api-help-list">
                    <li>Sign-in tokens last <strong><%= TokenDays %> days</strong> (<code>MobileApi.TokenDays</code>), and a phone not used for <strong><%= IdleText %></strong> must sign in again (<code>MobileApi.TokenIdleDays</code>).</li>
                    <li>Sign-ins from one network are paused after <strong><%= LoginFailuresPerIp %> failures in 15 minutes</strong> (<code>MobileApi.LoginFailuresPerIp</code>); each phone may make <strong><%= RequestsPerMinuteText %></strong> (<code>MobileApi.RequestsPerMinute</code>).</li>
                    <li>When a driver sends a delivery, Tracker will <strong><asp:Literal ID="litAutoComplete" runat="server" /></strong> (set on <a href="DriverApp.aspx">System → Driver App</a>).</li>
                    <li>HTTPS required for phones: <strong><asp:Literal ID="litHttps" runat="server" /></strong> (<code>MobileApi.RequireHttps</code>; this server itself is always allowed).</li>
                    <li>Request log kept for <strong><%= LogDaysText %></strong> (<code>MobileApi.LogDays</code>; 0 turns it off).</li>
                </ul>
                <p class="sys-prefs-help">These are changed in Web.config.
                    <asp:Button ID="btnEnsureSchema" runat="server" CssClass="filter-panel-btn" Text="Check API tables"
                        OnClick="btnEnsureSchema_Click" CausesValidation="false"
                        ToolTip="Creates any missing mobile API tables (safe to run again)" /></p>
                <h4>How the API is protected</h4>
                <ul class="api-help-list">
                    <li><strong>Sign-in:</strong> the normal Tracker login and password. After 5 wrong passwords in 10 minutes the login is locked, the same as the website, and too many failures from one network (any usernames) pause sign-ins from there for 15 minutes. The login must be linked to a delivery person, or be an administrator; contacts cannot use it.</li>
                    <li><strong>Tokens:</strong> a long random key (256 bits) per phone. Tracker stores only a one-way fingerprint of it, so a copy of the database cannot be used to sign in. Tokens expire, stop working when the phone is not used for a while, are replaced when the phone signs in again, and can be revoked on the Devices tab.</li>
                    <li><strong>Deactivating a login works straight away:</strong> within 5 minutes the phone is refused if the login is deactivated, locked, deleted or unlinked from its delivery person.</li>
                    <li><strong>Signatures and the proof list</strong> can only be read by administrators, and are never cached.</li>
                    <li><strong>Rate limit:</strong> a phone making far more calls than the app needs gets "try again in a minute" (429).</li>
                    <li><strong>Replies</strong> are marked not to be cached, framed or content-sniffed; over HTTPS they tell the phone to stay on HTTPS. The web app only runs its own scripts (Content-Security-Policy).</li>
                    <li><strong>Every call</strong> except Ping and Sign in needs a valid token. Devices and Request log also need an administrator.</li>
                    <li><strong>Drivers see their own deliveries by default.</strong> They can open another driver's or everyone's sheet to help out; the Request log notes when they do.</li>
                    <li><strong>HTTPS only</strong> from phones, so tokens and signatures are encrypted in transit.</li>
                    <li><strong>Website sign-in is not accepted by the API</strong> (only tokens), so other websites cannot trick a browser into making calls, and only the app's own origins are allowed to call it from a web view.</li>
                    <li><strong>Input checks:</strong> all database access uses parameters; signatures must be real PNG/JPEG/WebP images up to 256 KB; a sync is limited to 100 deliveries.</li>
                    <li><strong>Errors</strong> never show server details to phones; they are written to the system log.</li>
                    <li><strong>Audit:</strong> every call is in the Request log, and sign-ins also go to the system log.</li>
                </ul>
            </div>
        </div>

        <div class="simpleForm page-tone-panel page-tone-sysdata" id="apiReplyPanel">
            <div class="page-tone-header tool-card-header">
                <div><h2 class="page-tone-title">Reply</h2></div>
            </div>
            <div id="apiResultSummary" class="api-result-summary">Nothing sent yet.</div>
            <div id="apiResultImages" class="api-tester-images"></div>
            <pre id="apiResult" class="api-tester-output"></pre>
        </div>
    </asp:Panel>

    <asp:Literal ID="litTokenScript" runat="server" />

    <script type="text/javascript">
        var apiTester = (function () {
            var base = '<%= ResolveUrl("~/api/v1") %>';
            var lastETag = {};
            var drawing = false, hasInk = false, canvas, ctx, logTimer = null;

            function $(id) { return document.getElementById(id); }
            function val(id) { var el = $(id); return el ? el.value.trim() : ''; }
            function esc(s) { var d = document.createElement('div'); d.textContent = s == null ? '' : String(s); return d.innerHTML; }
            function token() { return sessionStorage.getItem('trackerApiToken') || ''; }
            function authHeaders() { return token() ? { 'Authorization': 'Bearer ' + token() } : {}; }
            function uuid() {
                if (window.crypto && crypto.randomUUID) return crypto.randomUUID();
                return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                    var r = Math.random() * 16 | 0; return (c === 'x' ? r : (r & 0x3 | 0x8)).toString(16);
                });
            }
            function useETag() {
                var panel = document.querySelector('.api-tab-panel.active');
                var box = panel ? panel.querySelector('.apiUseETag') : null;
                return !!(box && box.checked);
            }

            // ---- who the calls are sent as ----
            function setToken(t) {
                if (t) sessionStorage.setItem('trackerApiToken', t); else sessionStorage.removeItem('trackerApiToken');
                refreshWhoami();
            }
            function refreshWhoami() {
                var box = $('apiWhoami'), text = $('apiWhoamiText'), btn = $('apiSignOutBtn');
                if (!token()) {
                    box.classList.remove('signed-in');
                    btn.style.display = 'none';
                    text.innerHTML = 'nobody yet. Sign in on the <a href="#" onclick="apiTester.showTab(\'signin\'); return false;">Sign in</a> tab first; every call is then sent as that user, just like the phone.';
                    return;
                }
                text.textContent = 'checking…';
                fetch(base + '/auth/me', { headers: authHeaders(), credentials: 'omit', cache: 'no-store' })
                    .then(function (r) { return r.ok ? r.json() : null; })
                    .then(function (me) {
                        if (!me) { setToken(null); text.textContent = 'nobody (the token has expired or was revoked). Sign in again.'; return; }
                        box.classList.add('signed-in');
                        btn.style.display = '';
                        if (sessionStorage.getItem('apiTesterTab') === 'sheet') dates(true);
                        text.innerHTML = '<strong>' + esc(me.userName) + '</strong>'
                            + (me.personName ? ' (delivery person: ' + esc(me.personName) + ')' : ' (not a delivery person, so "Mine" sheets are empty)')
                            + (me.isAdmin ? ', administrator' : '')
                            + '. Every call below is sent with this user\'s token.';
                    });
            }
            function signOut() {
                call('POST', '/auth/logout').then(function () { setToken(null); });
            }

            // ---- tabs ----
            function showTab(name) {
                document.querySelectorAll('.api-tab').forEach(function (b) { b.classList.toggle('active', b.getAttribute('data-tab') === name); });
                document.querySelectorAll('.api-tab-panel').forEach(function (p) { p.classList.toggle('active', p.getAttribute('data-panel') === name); });
                $('apiReplyPanel').style.display = (name === 'log' || name === 'help') ? 'none' : '';
                sessionStorage.setItem('apiTesterTab', name);
                if (name === 'log') loadLog();
                if (name === 'sheet') dates(true);
                if (name !== 'log') autoLog(false);
            }

            // ---- sending a call ----
            function call(method, path, body) {
                var url = base + path;
                var headers = authHeaders();
                headers['Accept'] = 'application/json';
                if (body !== undefined) headers['Content-Type'] = 'application/json';
                if (method === 'GET' && useETag() && lastETag[url]) headers['If-None-Match'] = lastETag[url];

                var sent = body === undefined ? undefined : JSON.stringify(body);
                var started = performance.now();
                $('apiResultSummary').textContent = method + ' ' + url + ' …';
                $('apiResultImages').innerHTML = '';

                return fetch(url, { method: method, headers: headers, body: sent, cache: 'no-store', credentials: 'omit' })
                    .then(function (res) {
                        var ms = Math.round(performance.now() - started);
                        var type = res.headers.get('Content-Type') || '';
                        var etag = res.headers.get('ETag');
                        if (etag) lastETag[url] = etag;
                        var reader = type.indexOf('image/') === 0 ? res.blob() : res.text();
                        return reader.then(function (data) {
                            var entry = performance.getEntriesByName(new URL(url, location.href).href).pop();
                            var wire = entry && entry.encodedBodySize ? entry.encodedBodySize : null;
                            var size = typeof data === 'string' ? new Blob([data]).size : data.size;
                            var cls = res.status < 300 || res.status === 304 ? 'ok' : res.status < 500 ? 'warn' : 'err';
                            $('apiResultSummary').innerHTML =
                                '<span class="api-status api-status-' + cls + '">' + res.status + ' ' + esc(res.statusText) + '</span> '
                                + '<code>' + esc(method + ' ' + url) + '</code>'
                                + '<span class="api-facts">' + ms + ' ms · ' + (res.status === 304 ? 'nothing downloaded (unchanged)' : size.toLocaleString() + ' bytes'
                                + (wire && wire !== size ? ' (' + wire.toLocaleString() + ' actually downloaded, compressed)' : ''))
                                + (sent ? ' · sent ' + new Blob([sent]).size.toLocaleString() + ' bytes' : '')
                                + (etag ? ' · version ' + esc(etag) : '') + '</span>';
                            var out = $('apiResult');
                            if (typeof data !== 'string') {
                                var img = document.createElement('img');
                                img.src = URL.createObjectURL(data);
                                $('apiResultImages').appendChild(img);
                                out.textContent = '';
                                return null;
                            }
                            var parsed = null;
                            try { parsed = data ? JSON.parse(data) : null; } catch (e) { }
                            out.textContent = parsed !== null ? JSON.stringify(parsed, null, 2) : (data || '(empty reply)');
                            return parsed;
                        });
                    })
                    .catch(function (err) {
                        $('apiResultSummary').textContent = method + ' ' + url + ' failed: ' + err;
                    });
            }

            function login() {
                var deviceId = sessionStorage.getItem('trackerApiDevice');
                if (!deviceId) { deviceId = uuid(); sessionStorage.setItem('trackerApiDevice', deviceId); }
                return call('POST', '/auth/login', {
                    userName: val('apiUser'), password: $('apiPassword').value,
                    deviceId: 'api-tester-' + deviceId, deviceName: val('apiDevice') || 'API tester', appVersion: 'tester'
                }).then(function (r) {
                    $('apiPassword').value = '';
                    if (r && r.token) setToken(r.token);
                });
            }

            function fillDates(list) {
                var sel = $('apiSheetDates');
                if (!Array.isArray(list)) { sel.innerHTML = '<option value="">(could not load)</option>'; return; }
                var today = val('apiSheetDate');
                sel.innerHTML = '<option value="">Choose a date…</option>' + list.map(function (d) {
                    var label = d.date + ': ' + (d.mine != null ? d.mine + ' mine, ' : '') + d.open + ' not done of ' + d.orders;
                    return '<option value="' + esc(d.date) + '"' + (d.date === today ? ' selected="selected"' : '') + '>' + esc(label) + '</option>';
                }).join('');
            }

            function dates(quiet) {
                if (!token()) { $('apiSheetDates').innerHTML = '<option value="">(sign in to load)</option>'; return; }
                if (!quiet) return call('GET', '/delivery/dates').then(fillDates);
                return fetch(base + '/delivery/dates', { headers: authHeaders(), credentials: 'omit', cache: 'no-store' })
                    .then(function (r) { return r.ok ? r.json() : null; })
                    .then(fillDates);
            }

            function sheet() {
                var d = val('apiSheetDate');
                return call('GET', '/delivery/sheet?person=' + encodeURIComponent(val('apiSheetPerson') || 'me') + (d ? '&date=' + d : ''));
            }

            function repairs() {
                var q = '?open=' + $('apiRepairsOpen').checked;
                if (val('apiRepairsSince')) q += '&since=' + encodeURIComponent(val('apiRepairsSince'));
                return call('GET', '/repairs' + q);
            }

            function saveRepair() {
                var body;
                try { body = JSON.parse($('apiRepairJson').value); }
                catch (e) { alert('The repair details are not valid JSON: ' + e.message); return; }
                Object.keys(body).forEach(function (k) { if (body[k] === '' || body[k] === null) delete body[k]; });
                var id = val('apiSaveRepairId');
                if (id) {
                    if (!confirm('Update repair ' + id + '? This saves to the database and may email the contact.')) return;
                    return call('PUT', '/repairs/' + id, body);
                }
                if (!body.clientRef) body.clientRef = uuid();
                if (!confirm('Create a new repair for contact ' + body.contactId + '? This saves to the database.')) return;
                return call('POST', '/repairs', body);
            }

            function sync() {
                if (!val('apiSyncRef')) newRef();
                var item = {
                    clientRef: val('apiSyncRef'),
                    orderId: parseInt(val('apiSyncOrder'), 10) || 0,
                    outcome: val('apiSyncOutcome'),
                    receivedBy: val('apiSyncName'),
                    note: val('apiSyncNote'),
                    deliveredAt: new Date().toISOString()
                };
                if (hasInk) item.signature = canvas.toDataURL('image/png');
                return call('POST', '/delivery/sync', { deliveries: [item] });
            }

            function proofs() {
                var order = val('apiProofOrder'), date = val('apiProofDate');
                return call('GET', '/delivery/proofs?' + (order ? 'orderId=' + order : 'date=' + (date || ''))).then(function (list) {
                    if (!list || !list.length) return;
                    list.filter(function (p) { return p.hasSignature; }).slice(0, 12).forEach(function (p) {
                        fetch(base + '/delivery/proofs/' + p.proofId + '/signature', { headers: authHeaders(), credentials: 'omit' })
                            .then(function (r) { return r.ok ? r.blob() : null; })
                            .then(function (b) {
                                if (!b) return;
                                var fig = document.createElement('figure');
                                var img = document.createElement('img');
                                img.src = URL.createObjectURL(b);
                                var cap = document.createElement('figcaption');
                                cap.textContent = 'Order ' + p.orderId + ': ' + (p.receivedBy || '');
                                fig.appendChild(img); fig.appendChild(cap);
                                $('apiResultImages').appendChild(fig);
                            });
                    });
                });
            }

            function devices() {
                return call('GET', '/devices').then(function (list) {
                    var host = $('apiDevices');
                    if (!Array.isArray(list)) { host.innerHTML = ''; return; }
                    host.innerHTML = '<table class="results-table api-log-table"><thead><tr><th>User</th><th>Device</th><th>Signed in</th><th>Last used</th><th>Expires</th><th>Status</th><th></th></tr></thead><tbody>'
                        + list.map(function (d) {
                            return '<tr><td>' + esc(d.userName) + '</td><td>' + esc(d.deviceName || d.deviceId || '') + '</td><td>' + esc(fmt(d.createdAt)) + '</td><td>'
                                + esc(fmt(d.lastUsedAt)) + '</td><td>' + esc(fmt(d.expiresAt)) + '</td><td>' + (d.active ? 'Active' : 'Signed out / expired') + '</td><td>'
                                + (d.active ? '<button type="button" class="filter-panel-btn" onclick="apiTester.revoke(' + d.id + ')">Revoke</button>' : '') + '</td></tr>';
                        }).join('') + '</tbody></table>';
                });
            }

            function revoke(id) {
                if (!confirm('Revoke this device? It will be signed out on its next call.')) return;
                call('POST', '/devices/' + id + '/revoke').then(function () { devices(); refreshWhoami(); });
            }

            function fmt(s) { return s ? s.replace('T', ' ').substring(0, 16) : ''; }
            function kb(n) { return n == null ? '' : n < 1024 ? n + ' B' : (n / 1024).toFixed(1) + ' KB'; }

            function loadLog() {
                var host = $('apiLog');
                if (!token()) { host.innerHTML = '<p class="sys-prefs-help">Sign in first (as an administrator) to see the log.</p>'; return; }
                var q = '?max=' + val('apiLogMax') + '&problems=' + $('apiLogProblems').checked + (val('apiLogUser') ? '&user=' + encodeURIComponent(val('apiLogUser')) : '');
                fetch(base + '/log' + q, { headers: authHeaders(), credentials: 'omit', cache: 'no-store' })
                    .then(function (r) { return r.json().then(function (j) { return { ok: r.ok, body: j }; }); })
                    .then(function (res) {
                        if (!res.ok) { host.innerHTML = '<p class="status-message status-error">' + esc(res.body && res.body.error || 'Could not load the log.') + '</p>'; return; }
                        var rows = res.body.entries || [];
                        if (!rows.length) { host.innerHTML = '<p class="sys-prefs-help">No calls logged yet.</p>'; return; }
                        host.innerHTML = '<table class="results-table api-log-table"><thead><tr><th>Time</th><th>User</th><th>Call</th><th>Result</th><th>Time taken</th><th>Sent</th><th>Received</th><th>From</th><th class="api-log-note">What happened</th></tr></thead><tbody>'
                            + rows.map(function (e) {
                                var cls = e.status < 300 || e.status === 304 ? 'ok' : e.status < 500 ? 'warn' : 'err';
                                return '<tr><td class="nowrap">' + esc(e.at.replace('T', ' ')) + '</td><td>' + esc(e.user || '-') + '</td>'
                                    + '<td class="api-log-call"><strong>' + esc(e.method) + '</strong> ' + esc(e.path.replace(/^.*\/api\/v1/, '')) + '</td>'
                                    + '<td><span class="api-status api-status-' + cls + '">' + e.status + '</span></td>'
                                    + '<td class="nowrap">' + e.ms + ' ms</td><td class="nowrap">' + kb(e.requestBytes) + '</td><td class="nowrap">' + kb(e.responseBytes) + '</td>'
                                    + '<td title="' + esc(e.agent || '') + '">' + esc(e.ip || '') + '</td><td class="api-log-note">' + esc(e.note || '') + '</td></tr>';
                            }).join('') + '</tbody></table>';
                    })
                    .catch(function (err) { host.innerHTML = '<p class="status-message status-error">' + esc(err) + '</p>'; });
            }

            function autoLog(on) {
                if (logTimer) { clearInterval(logTimer); logTimer = null; }
                var box = $('apiLogAuto');
                if (box) box.checked = !!on;
                if (on) logTimer = setInterval(loadLog, 5000);
            }

            function newRef() { $('apiSyncRef').value = uuid(); }

            function clearSignature() {
                ctx.clearRect(0, 0, canvas.width, canvas.height);
                hasInk = false;
            }

            function initSignature() {
                canvas = $('apiSignature');
                ctx = canvas.getContext('2d');
                ctx.lineWidth = 2; ctx.lineCap = 'round'; ctx.strokeStyle = '#1a1a1a';
                function pos(e) {
                    var r = canvas.getBoundingClientRect();
                    return { x: (e.clientX - r.left) * canvas.width / r.width, y: (e.clientY - r.top) * canvas.height / r.height };
                }
                canvas.addEventListener('pointerdown', function (e) {
                    drawing = true; var p = pos(e); ctx.beginPath(); ctx.moveTo(p.x, p.y);
                    try { canvas.setPointerCapture(e.pointerId); } catch (ex) { }
                });
                canvas.addEventListener('pointermove', function (e) { if (!drawing) return; var p = pos(e); ctx.lineTo(p.x, p.y); ctx.stroke(); hasInk = true; });
                canvas.addEventListener('pointerup', function () { drawing = false; });
                canvas.addEventListener('pointerleave', function () { drawing = false; });
            }

            document.addEventListener('DOMContentLoaded', function () {
                var today = new Date(Date.now() - new Date().getTimezoneOffset() * 60000).toISOString().substring(0, 10);
                $('apiSheetDate').value = today;
                $('apiProofDate').value = today;
                document.querySelectorAll('.api-tab').forEach(function (b) {
                    b.addEventListener('click', function () { showTab(b.getAttribute('data-tab')); });
                });
                initSignature();
                newRef();
                showTab(sessionStorage.getItem('apiTesterTab') || 'signin');
                refreshWhoami();
            });

            return { val: val, call: call, login: login, signOut: signOut, showTab: showTab, sheet: sheet, repairs: repairs,
                     saveRepair: saveRepair, sync: sync, dates: dates, proofs: proofs, devices: devices, revoke: revoke, loadLog: loadLog,
                     autoLog: autoLog, newRef: newRef, clearSignature: clearSignature, setToken: setToken };
        })();
    </script>
</asp:Content>
