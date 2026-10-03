# Deployment and Configuration

**Last Updated:** 2026-10-03 (3.0.3.3 live)

## 1. Live environment

| Item | Value |
|------|-------|
| Host | myasp.net shared hosting (IIS, .NET 4.8, integrated pipeline) |
| Addresses | **`https://tracker.quaffee.co.za`** (primary) and `https://quaffeetracker.co.za` (same site) |
| HTTPS | Terminated by IIS; `Web.config` redirects HTTP → HTTPS. Not behind a proxy. |
| Database | SQL Server on `SQL8001.site4now.net` (database name and login in the live `Web.config`) |
| `ApplicationBaseUrl` | `https://tracker.quaffee.co.za/` (used in email links) |
| Local dev | IIS Express `http://localhost:5000` (localhost only), SQL Server `.\SQLExpress2019`, database `OtterDb` |

## 2. Config files and secrets

**Real config files are not in git** (they hold the live DB, mailbox, `DisableClientSecret`, `WooCommerceCryptoKey`
and `machineKey` secrets). Only sanitized copies are committed, with every secret set to `mypassword`:

| Real file (git-ignored) | Committed template | Role |
|-------------------------|--------------------|------|
| `Web.config` | `Web.config.example` | Local / dev |
| `web.live.config` | `web.live.config.example` | Full live file; upload to the host **as `Web.config`** |
| `Web.Release.config` | `Web.Release.config.example` | XDT transform for a Release publish (not used for the FTP upload) |

Also ignored: `weblive.config`, `web..back.config` (old copies), `Documentation/repomix-output.xml`,
`App_Data/ErrorLog.txt`, `*.log`.

**Fresh clone:** copy each `.example` to its real name and fill in the real values.

**When a config changes:** edit the real file, then regenerate the matching `.example` with secrets replaced
(keys `EMailPassword`, `DisableClientSecret`, `WooCommerceCryptoKey`, connection-string `Password=`,
`<network password>`, `<machineKey decryptionKey/validationKey>`). Check no real secret remains before committing.

`web.live.config` must stay a copy of the **actual live Web.config** plus the new version's changes; when in doubt,
get the current file from the server first. Config files hold settings and short comments on what a setting does,
never release notes or change history (Hard Rule #0h): those go in `CHANGELOG.md` and section 6 below.

### Secrets exposure (2026-09-28)

The GitHub repo `wmachanik/TrackerSQL` is **public** and older commits (all branches) contain the real secrets,
including the live `machineKey` (forges sign-in cookies). Changing the files does not remove history. To fix:
rotate the live DB password, `noreply@quaffee.co.za` and `orders@quaffee.com` mailbox passwords,
`DisableClientSecret` (old disable links stop working), `WooCommerceCryptoKey` (re-enter Woo keys afterwards) and
generate a new `machineKey` (everyone signs in again); consider making the repo private; optionally scrub history
and force-push.

## 3. Release steps (Visual Studio publish + FTP)

1. Back up the live database and the server's `Web.config`.
2. If the driver app changed: in TrackerDriver run `npm run build:site` and/or `npm run apk` first.
3. Visual Studio → Publish → Folder, **Release**.
4. From the publish folder upload: `bin/` (whole folder), pages, `Styles/`, `Scripts/`, `driver/`, `Downloads/`,
   new `App_Data/SQLCommands-*.xml` files, and any sub-folder `Web.config` that changed (e.g. `Portal/Web.config`).
5. **Do not** upload the published `Web.config`; upload `web.live.config` renamed to `Web.config`.
6. **Do not** overwrite server log files in `App_Data` (`ErrorLog.txt`, `login.log`, `startup.log`, `system.log`).
7. Check `https://tracker.quaffee.co.za/api/v1/ping` (shows the version) and sign in.
8. Schema packs run themselves on first use (idempotent); for the mobile API also API Tester → **Check API tables**.

## 4. Schema packs

Feature tables are created by XML packs in `App_Data/` (`SQLCommands-*.xml`, live copies in `App_Data/GoLive/`),
run by `XmlSchemaCommandRunner` from each feature's schema installer. They only create/alter what is missing.
Every schema change ships this way (Hard Rule #0d).

## 5. Host requirements met in `web.live.config`

WebDAV removed and the extensionless handler allows every verb (PUT/OPTIONS for the app); anonymous `api` and
`driver` locations; `driver` default document `index.html` and CSP headers; `.apk` MIME type; Web API binding
redirects; `enableVersionHeader="false"`. If the host ever returns 405 on PUT, ask myasp.net support to allow
PUT/OPTIONS.

## 6. Live Web.config changes by version

What changed in `web.live.config` (uploaded as the live `Web.config`) in each release. Feature changes and the files
to upload are in `CHANGELOG.md`.

| Version | Config changes |
|---------|----------------|
| 3.0.3.4 | In progress: `Version` 3.0.3.3 → 3.0.3.4 so far. |
| 3.0.3.3 | `Version` 3.0.3.2 → 3.0.3.3 only. |
| 3.0.3.2 | `Version` 3.0.3.1 → 3.0.3.2 only (the APK download goes through `/api`, already anonymous). |
| 3.0.3.1 | `Version` 3.0.3.0 → 3.0.3.1 only. The Azure Maps / HeiGIT keys are entered on System Preferences → GPS & Drivers (database). After the first start, run `SQLCommands-OrderFlags-02-ResetContacts.xml` once (System Tools → XML to SQL) to put subscription contacts back to a standard account type. |
| 3.0.3.0 | See below. |
| 3.0.2.0 | `Version` 3.0.1.14 → 3.0.2.0; removed the anonymous `<location path="Default.aspx">` (home page needs sign-in); added anonymous `<location path="Styles">` and `<location path="images">` (login pages need CSS and logo). Also upload the new `Portal/Web.config`. |

**3.0.3.0 (mobile REST API at `/api/v1`):**

1. `Version` 3.0.2.0 → 3.0.3.0.
2. `MobileApi.*` appSettings.
3. `system.webServer`: WebDAV removed; the extensionless handler allows every verb (PUT / OPTIONS for the app).
4. Binding redirects for Newtonsoft.Json, System.Web.Http, System.Net.Http.Formatting,
   System.Threading.Tasks.Extensions and System.Runtime.CompilerServices.Unsafe.
5. Anonymous `<location path="api">` (the API checks its own Bearer tokens).
6. Anonymous `<location path="driver">` with `index.html` as its default page (upload the `driver` folder).
7. `staticContent` MIME type for `.apk` (upload the `Downloads` folder).
8. Content-Security-Policy, nosniff and no-referrer headers on `/driver`.
9. `ApplicationBaseUrl` `https://quaffeetracker.co.za/` → `https://tracker.quaffee.co.za/`.
10. `httpRuntime enableVersionHeader="false"`.

Upload the whole `bin` folder (new DLLs: System.Web.Http, System.Web.Http.WebHost, System.Net.Http.Formatting,
Newtonsoft.Json.Bson, System.Threading.Tasks.Extensions).
