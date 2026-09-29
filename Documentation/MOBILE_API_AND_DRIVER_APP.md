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
TrackerDriver has its own `README.md` (dev, build, phone testing, code map). **TrackerDriver is not in git yet.**

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
| `Managers/MobileDeliveryEmailManager.cs` | Delivery confirmation email (signature attached) and driver note to the office. |
| `Managers/MobileRepairManager.cs` | Repairs list/get/create/update (same rules as Repair Detail). |
| `Managers/MobileApiSettingsManager.cs` + `Repositories/MobileApiSettingsRepository.cs` | System → Driver App settings (`MobileApiSettingsTbl`, 1-minute cache). |
| `Managers/MobileApiSchemaInstaller.cs` | Runs `App_Data/SQLCommands-MobileApi-01.xml` once per app start (idempotent). |
| `Repositories/MobileApiRepository.cs` | All mobile SQL (tokens, proofs, sync refs, log, options, sheet rows, lookups). |
| `Models/MobileApiModels.cs` | Request/response models. |
| `Tools/ApiTester.aspx` | Admin page to call every endpoint, view the request log and devices, and "Check API tables". |
| `Tools/DriverApp.aspx` | App download page + admin settings (run Order Done, confirmation email, notes to office, office email). |
| `Pages/Lookups.aspx` → Driver Options tab | The delivered / not-delivered choices shown in the app. |

## 2. Endpoints (`/api/v1`)

All replies are JSON (camelCase, nulls omitted). Every call except `ping` and `auth/login` needs
`Authorization: Bearer <token>`.

| Method + path | Who | Notes |
|---------------|-----|-------|
| `GET ping` | anyone | `{ ok, api, version, serverTime }` |
| `POST auth/login` | anyone | Tracker login; must be linked to a person (Lookups → People, Security Username) or be an admin. 429 when throttled. |
| `POST auth/logout`, `GET auth/me` | device | |
| `GET delivery/dates`, `GET delivery/sheet?date=&person=me\|all\|id` | device | ETag / 304 |
| `POST delivery/sync` | device | Up to 100 deliveries; `clientRef` makes retries safe; `lines` for short deliveries; `noteToOffice` |
| `GET delivery/proofs`, `GET delivery/proofs/{id}/signature` | **admin** | Signature is `no-store` |
| `GET repairs`, `GET repairs/{id}`, `POST repairs`, `PUT repairs/{id}` | device | ETag on list; `clientRef` on create |
| `GET lookups`, `GET contacts?q=`, `GET contacts/{id}` | device | Lookups include `settings` (Driver App settings) |
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
3. Server stores the proof. If System → Driver App **Run Order Done** is on and nothing is short, it runs
   `OrderDoneManager.CompleteOrder` (statusKey null when the confirmation email is sent, so the client gets one email).
4. **Confirmation email** (if on): to the contact (ZZName orders: address from the order notes), CC orders@,
   signature attached. **Office note** (if allowed and ticked): to the office email (default `SysCCEmailAddress`).
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
- **TrackerDriver** has no git repository yet (`git init` + first commit as 0.3.0 was offered).
- iPhone build (needs a Mac / Xcode) not done; the web copy works on iPhone.
