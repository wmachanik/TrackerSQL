# Deployment and Configuration

**Last Updated:** 2026-09-29 (3.0.3.0 live)

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

`web.live.config` has a header listing each version's changes (search "3.0.3.0"). It must stay a copy of the
**actual live Web.config** plus the new version's changes; when in doubt, get the current file from the server first.

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
Core schema changes still belong in **TrackerMigration** (Hard Rule #0d).

## 5. Host requirements met in `web.live.config`

WebDAV removed and the extensionless handler allows every verb (PUT/OPTIONS for the app); anonymous `api` and
`driver` locations; `driver` default document `index.html` and CSP headers; `.apk` MIME type; Web API binding
redirects; `enableVersionHeader="false"`. If the host ever returns 405 on PUT, ask myasp.net support to allow
PUT/OPTIONS.
