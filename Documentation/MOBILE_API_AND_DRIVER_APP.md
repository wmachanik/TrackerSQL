# Mobile REST API and Quaffee Driver App

**Released:** 3.0.3.0 (live 2026-09-29)  
**Last Updated:** 2026-09-29

Tracker exposes a JSON API at `/api/v1` for the **Quaffee Driver** app (delivery drivers: delivery sheet, proof of
delivery captured offline, repairs, contacts). The app lives in a separate folder, `C:\SRC\ASP.net\TrackerDriver`
(Vite + Vue 3 + TypeScript inside Capacitor 8). It is built into Tracker two ways:

| Build | Command (in TrackerDriver) | Output in TrackerSQL | Server it calls |
|-------|----------------------------|----------------------|-----------------|
| Web copy (Android/iPhone "Add to Home Screen") | `npm run build:site` | `driver/` (served at `/driver/`) | The site it was opened from |
| Android app | `npm run apk` (~7-12 min on this PC; then `gradlew --stop`) | `Downloads/QuaffeeDriver.apk` | `https://tracker.quaffee.co.za` (changeable under "Server settings" on sign-in) |

Both folders are in the csproj as `driver\**\*` and `Downloads\**\*`, so a Visual Studio publish includes them.
TrackerDriver has its own `README.md` (dev, build, phone testing, code map). It is a **local git repo only**
(branch `main`, first commit 0.3.0; no remote yet).

---

## 1. Server code map (TrackerSQL)

| File | What it does |
|------|--------------|
| `Api/WebApiConfig.cs` | Registers Web API 2 (attribute routes, camelCase JSON, handlers). Called from `Global.asax`. |
| `Api/MobileApiHandlers.cs` | Message handlers, outermost first: **CORS** → **security headers** → **request log** → **gzip** → **auth** (Bearer token, rate limit). Also `MobileAuthorizeAttribute` and the exception filter. |
| `Api/MobileApiController.cs` | Base controller: current device user, JSON errors, `JsonWithETag` (304 when unchanged). |
| `Api/MobileApiControllers.cs` | All endpoints (see section 2). |
| `Managers/MobileAuthManager.cs` | Sign-in, token issue/validate/revoke (SHA-256 hash stored), idle timeout, appSettings. |
| `Managers/MobileApiSecurity.cs` | In-memory throttles: failed sign-ins per IP, calls per device, 5-minute account check cache. |
| `Managers/MobileDeliveryManager.cs` | Sheet, offline delivery sync (`clientRef` de-duplication), proofs, auto Order Done, emails. |
| `Managers/MobileDeliveryCompletionQueue.cs` | Runs Order Done and the emails for synced deliveries after the reply (Pending proofs), as the driver. |
| `Managers/MobileDeliveryEmailManager.cs` | Proof of delivery email (hands over to `ProofOfDeliveryManager`) and driver note to the office. |
| `Managers/ProofOfDeliveryManager.cs` + `Classes/SimplePdf.cs` | Proof of delivery PDF (details, items, terms tick, signature) and sending it; `Handlers/ProofOfDelivery.ashx?id=` serves it to staff (any proof) and to Contact Portal logins (only their own delivered proofs, `ContactPortalManager.CanViewProof`; otherwise 404). The portal links it from My Orders and Order Details. |
| `Classes/SundryNotes.cs` | Walk-in (ZZName) order notes: name before the `:`, details after it, `[#email#]`. |
| `Managers/MobileRepairManager.cs` | Repairs list/get/create/update (same rules as Repair Detail). |
| `Managers/MobileApiSettingsManager.cs` + `Repositories/MobileApiSettingsRepository.cs` | System → Driver App settings (`MobileApiSettingsTbl`, 1-minute cache). |
| `Managers/MobileApiSchemaInstaller.cs` | Runs `App_Data/SQLCommands-MobileApi-01.xml` once per app start (idempotent). |
| `Repositories/MobileApiRepository.cs` | All mobile SQL (tokens, proofs, sync refs, log, options, sheet rows, lookups). |
| `Models/MobileApiModels.cs` | Request/response models. |
| `Tools/ApiTester.aspx` | Admin page to call every endpoint, view the request log and devices, and "Check API tables". |
| `Tools/DriverApp.aspx` | App download page + admin settings (run Order Done, proof of delivery email, delivery terms and link, notes to office, office email, landline prefixes). |
| `Tools/DeliveriesAwaitingDone.aspx` | Admin list of app deliveries whose order is not done, with **Complete Now**; `?OrderID=` highlights a row (Delivery Sheet red flag). |
| `Pages/Lookups.aspx` → Delivery Outcomes tab (was Driver Options) | The delivered / not-delivered choices shown in the app. |
| `Managers/DriverTrackingManager.cs` + `Repositories/DriverLocationRepository.cs` | Driver tracking: `POST location`, positions with deliveries, daily clean-up, the Driver tracking page's data (`Handlers/DriverTrackingData.ashx`, `Tools/DriverTracking.aspx`). |

## 2. Endpoints (`/api/v1`)

All replies are JSON (camelCase, nulls omitted). Every call except `ping` and `auth/login` needs
`Authorization: Bearer <token>`.

| Method + path | Who | Notes |
|---------------|-----|-------|
| `GET ping` | anyone | `{ ok, api, version, serverTime }` |
| `POST auth/login` | anyone | Tracker login; must be linked to a person (Lookups → People, Security Username) or be an admin. 429 when throttled. |
| `POST auth/logout`, `GET auth/me` | device | |
| `GET delivery/dates`, `GET delivery/sheet?date=&person=me\|all\|id` | device | ETag / 304 |
| `POST delivery/sync` | device | Up to 100 deliveries; `clientRef` makes retries safe; `lines` for short deliveries; `noteToOffice`; `termsAccepted` and `terms` (the wording shown) for signed deliveries (0.5.3+) |
| `POST delivery/route` | device | `{date: "yyyy-MM-dd", orderIds: [...]}` the driver's own stop order for the day (first to last; at most 500); `[]` goes back to the suggested order. Returns `{ok, stops}`. Used before the learned / GPS order on both sheets (`DeliveryRouteTbl`) |
| `GET delivery/proofs`, `GET delivery/proofs/{id}/signature` | **admin** | Signature is `no-store` |
| `GET repairs`, `GET repairs/{id}`, `POST repairs`, `PUT repairs/{id}` | device | ETag on list; `clientRef` on create |
| `GET lookups`, `GET contacts?q=`, `GET contacts/{id}` | device | Lookups include `settings` (Driver App settings, incl. `landlinePrefixes` such as `["021"]`: no WhatsApp for those numbers, `terms` and `termsUrl` for the tick box above the signature, `tracking`: `{ enabled, minutes, weekdaysOnly, from, to }`, and `app`: `{ version, versionCode, url }` from `Downloads/QuaffeeDriver.json`, missing when there is none) |
| `GET app/download` | device | `{ version, url }`: a download link for the APK valid 30 minutes (`MachineKey.Protect`, purpose `DriverApkDownload`) |
| `GET app/apk?t=` | none (the link is the permission) | The APK as `QuaffeeDriver-<version>.apk`; 403 text when the link expired. The request log keeps the path without the link |
| `POST location` | device | `{ points: [{ lat, lng, accuracy, at, source }] }` (oldest first, at most 200; `source` APP_OPEN or PERIODIC). Stored only while tracking is on and inside the work days and hours; future or day-old points are skipped. Needs a sign-in linked to a person (400 otherwise). Returns `{ ok, saved, skipped, trackingEnabled }` (`DriverLocationPingTbl`) |
| `GET devices`, `POST devices/{id}/revoke`, `GET log` | **admin** | |

## 3. Security (as built)

- **HTTPS only** from phones (`MobileApi.RequireHttps`; the server itself is exempt). `X-Forwarded-Proto` trusted only if `MobileApi.TrustForwardedProto=true`.
- **Tokens:** 256-bit random, only the SHA-256 hash is stored (`MobileDeviceTokensTbl`), 90-day life (`TokenDays`), revoked after 30 days unused (`TokenIdleDays`), replaced on re-sign-in, revocable from the API Tester Devices tab.
- **Account checks** every 5 minutes per token: login deleted, not approved, locked, or unlinked from its person → 401 with the reason.
- **Throttling:** 10 failed sign-ins per IP per 15 minutes (`LoginFailuresPerIp`) → 429; 120 calls per device per minute (`RequestsPerMinute`) → 429 + Retry-After. Normal membership lockout (5 wrong passwords) also applies. Counters are in memory (reset on app restart).
- **Headers on every API reply:** `Cache-Control: no-store, private` (unless the action set one, e.g. ETag replies use `private, no-cache`), `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, `Referrer-Policy: no-referrer`, HSTS on HTTPS.
- **`/driver/`** sends a Content-Security-Policy (own scripts only, API calls to the same site only).
- **CORS** (`MobileApi.CorsOrigins`) lists the app's own web-view origins, **not** the server address: `https://localhost` (Android) and `capacitor://localhost` (iPhone). The web copy is same-origin and needs no CORS.
- **Request log** (`MobileApiRequestLogTbl`, `LogDays`=30): time, user, call, status, duration, bytes, IP, short note. Never bodies, headers, tokens or signatures.
- Website sign-in cookies are **not** accepted by the API (tokens only).

Not built (possible later): admin approval of each new phone; PIN / fingerprint lock in the app.

## 4. appSettings

`MobileApi.TokenDays` (90), `TokenIdleDays` (30), `LoginFailuresPerIp` (10), `RequestsPerMinute` (120),
`TrustForwardedProto` (false), `RequireHttps` (true), `LogDays` (30), `CorsOrigins`
(`https://localhost,capacitor://localhost`), `AutoCompleteDeliveries` (false; only the default until
System → Driver App settings are saved).

## 5. Database (new tables only)

Created by `App_Data/SQLCommands-MobileApi-01.xml` (copy in `App_Data/GoLive/`) on first API use or via
API Tester → **Check API tables**. Idempotent. No existing table is altered.

`MobileDeviceTokensTbl`, `MobileDeliveryProofTbl` (+ `Reason`, `MissingItems`), `MobileSyncRefTbl`,
`MobileApiRequestLogTbl`, `MobileDeliveryOptionsTbl` (seeded), `MobileApiSettingsTbl` (SettingsID = 1).

## 6. Delivery flow

1. Driver captures delivered / not delivered offline: how (driver option), received-by name, signature, note,
   items handed over (qty per line when short), optional "email this note to the office".
2. App queues it in its outbox and sends it with `POST delivery/sync` when allowed (see section 7).
3. Server stores the proof and replies at once. If System → Driver App **Run Order Done** is on and nothing is short,
   the proof is stored as `Pending` and `MobileDeliveryCompletionQueue` runs `OrderDoneManager.CompleteOrder` in the
   background (`HostingEnvironment.QueueBackgroundWorkItem`, one proof at a time, as the driver's login), statusKey null
   when the confirmation email is sent so the client gets one email; the proof then becomes `Completed`,
   `CompleteFailed` or `AlreadyDone` with the result in `StatusMessage`. Emails (POD, office note) are also sent after
   the reply. Pending proofs left by an app restart are picked up at start-up, on the next sync, or when System →
   Driver App or Waiting for Order Done is opened. `Tools/DeliveriesAwaitingDone.aspx` lists what is not done
   (`MobileApiRepository.ListAwaitingDone`) and **Complete Now** queues a proof again; the Delivery Sheet shows a red
   flag (`a.dlv-await-flag`) on those orders linking to that page with `?OrderID=`.
4. **Proof of delivery email** (if on): to the contact (ZZName orders: the `[#email#]` in the order notes), CC orders@,
   with the POD PDF attached (order, PO, delivered to, when / how, received by, driver, items handed over from the
   stored `HandedOver`, still to follow, the terms with the tick and link, the signature). Staff can open or (re)send it
   from Contact Details → Proof of Delivery (**PDF** / **Email**; `PodSentAt` / `PodSentTo`). **Office note** (if allowed and ticked): to the office email (default `SysCCEmailAddress`).
5. Short deliveries store `MissingItems` text only; **order quantities in Tracker are not changed** (open item).

## 7. App behaviour worth knowing

- **Offline first:** sheet, dates, lookups and open repairs are cached in IndexedDB and refreshed with ETags.
- **Status pill** (top bar): Live / Updating / Offline / Phone copy / Wi-Fi only. Lists say "From Tracker …" or
  "Phone copy from … (why)".
- **Mobile data options** (More): send deliveries/repairs over mobile data; update lists over mobile data. When off,
  deliveries wait for Wi-Fi ("N for Wi-Fi"); ⟳ and "Send now using mobile data" still work. Unknown connection = allowed.
- **Save everything on this phone** (More): sends the outbox and saves the day's sheets (mine + everyone's), repairs, lookups.
- On 401 the app clears cached data but **keeps the outbox** (sent after the next sign-in).
- ZZName (contact 9) is never matched as a client for "new repairs for this client".
- **Driver tracking** (0.5.0, `src/tracking.ts`): runs while `settings.tracking.enabled`, the user has a `personId` and the
  phone's clock is inside the work days and hours. Android uses `@capacitor-community/background-geolocation` (foreground
  service with a notification; `android.useLegacyBridge` in `capacitor.config.ts`; positions only after 30 m of movement,
  so a parked phone re-sends its last position) and posts with native `CapacitorHttp` because Android throttles web-view
  requests in the background. The web copy uses `watchPosition` while open. Up to 300 positions wait in IndexedDB
  (`tracking.queue`); they are dropped on sign-out so they are never sent under the next driver's name.
- **Update** (0.5.1, `src/appUpdate.ts`): when `settings.app.version` is newer than the app (Android only; the web copy
  updates itself), the delivery list and More show Update. It calls `GET app/download` and navigates to the link, which
  Capacitor opens in the phone's browser; Android downloads the APK and the driver taps Install. Android only installs
  over the old app when the APK is signed with the same key (the debug keystore in `%USERPROFILE%\.android` on the build
  PC; back it up) and `versionCode` is higher.
- **Times and Next** (0.5.2, `src/routeTimes.ts`): the list marks the first unfinished stop **Next** (today) and shows the
  delivered time. On "Mine", expected times ("about HH:mm"; "roughly" when the stop or the one before has no point) start
  once a position is more than `settings.route.leftDepotMetres` (300) from the depot (remembered for the day in
  localStorage `route.left`) or something was delivered today. They chain from the latest of the last delivery (that
  stop's point) and the last position: straight line × 1.3 at `speedKmh`, plus `serviceMinutes` per stop; consecutive
  orders for one client are one stop; the first time is never before now. `RouteTimesManager` does the same for the
  office board on Tools → Delivery order (`ForDay(day, sheetOrders)`), using the tracking pings.
- **Office order note:** when the office dragged the driver's stops on the Delivery Sheet (`DeliveryRouteTbl.SetBy` ends
  in " (office)"), the sheet's `routeNote` says who and when; it goes once the driver saves their own order. The sheet
  ETag includes it.
- **GPS check on delivery** (0.5.2): with Delivered chosen, the capture screen compares the phone's fix with the stop's
  `gps` (`kind` pin / driver / lookup / none, `checked`, `review`): too rough (worse than ±75 m), will set it (none),
  pin within 150 m OK / far (kept, flagged), lookup will be replaced, driver point within 60 m confirms / will move,
  and over 1 km a warning to capture at the client. Nothing for `noGpsLearning` stops (collects / courier) or ZZName.
  Contact Details → Proof of Delivery shows the same check per proof (`CustomerLocationManager.DescribeDeliveryFix`).
- **Not at the client** (DriverGps-04): delivery options with `notAtClient` (Lookups → Delivery Outcomes, e.g. "Collected
  by the client", "Dropped off by the supplier"). On sync the server matches the proof's `reason` to such an option and
  stores `MobileDeliveryProofTbl.NotAtClient`: the position is kept but `LearnFromDelivery` and the delivery-order visit
  are skipped (the tracking ping is still stored: it is where the driver really was). The sheet's `proof.notAtClient`
  stops it being used as the last delivery point for expected times; the capture screen says the position is not used.
- **Terms tick box** (0.5.3, DriverGps-05): with Delivered chosen, the capture screen shows `settings.terms` with a tick
  box and **Read the terms** (`settings.termsUrl`). A signature can't be sent until it is ticked, and (0.5.4) not
  without the signer's name in *Received by*. The proof stores
  `TermsAccepted` and `TermsText` (the wording shown, or the current setting for older apps, whose `termsAccepted` is
  null). Defaults in `MobileApiSettingsManager` (`DefaultDeliveryTerms`, `DefaultTermsUrl`).
- **Walk-in stops:** for ZZName orders the sheet's `company` / `contactName` is the walk-in name from the notes and
  `notes` is the address / goods after the `:` without `[#…#]` tags (`SundryNotes`).

## 8. Data use (measured)

APK 4.6 MB once; web copy ~230 KB once (~70 KB compressed). Sheet ~7 KB (~2 KB gzip); unchanged checks < 1 KB;
each delivery ~10-30 KB (mostly the signature, max 256 KB). A 20-30 stop day is under 1 MB.

## 9. Testing tips

- **API Tester** (System Tools) covers every call; sign in "like the phone" or with your web session.
- **Local IIS Express** is `http://localhost:5000` (localhost only). The app in a browser: `npm run dev` in
  TrackerDriver (Vite proxies `/api` to port 5000). Emulator: `adb reverse tcp:5000 tcp:5000` or `npm run android:dev`.
- **Test token without a password:** insert a row in `MobileDeviceTokensTbl` with `TokenHash` = lowercase hex
  SHA-256 of a made-up token, then call with `Bearer <token>`. **Delete the row afterwards.**
- **Headless UI test:** Edge `--headless=new --remote-debugging-port`, set `localStorage['capacitor-storage_apiToken']`
  = JSON string of the token and `driver.user`, or stub `window.fetch` via `Page.addScriptToEvaluateOnNewDocument`.
  Mobile data can be faked by overriding `Navigator.prototype.connection` (`type: 'cellular'`).
- Sign-in throttling is per IP and in memory: after testing bad logins from localhost, restart the site (or touch
  `Web.config`) to clear it.

## 10. Open items

- **Short deliveries** do not change order quantities in Tracker (offered, not built).
- **Security:** rotate the secrets that were public on GitHub (see `DEPLOYMENT_AND_CONFIG.md`), including the
  `machineKey`; consider making the GitHub repo private; optional phone approval / app PIN.
- **TrackerDriver** git repo is local only (no remote / backup yet).
- iPhone build (needs a Mac / Xcode) not done; the web copy works on iPhone.
