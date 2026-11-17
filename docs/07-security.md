# 07 — Security Model

This is the as-built model. It is role-based, coarse, and inconsistent in a few role-name strings. RainbowOF should not copy the inconsistencies.

---

## 1. Authentication

### 1.1 Staff and portal web

- **Mode:** ASP.NET Forms authentication (`Web.config`), cookie, timeout **2880 minutes** (48 hours).
- **Login page:** `~/Account/Login.aspx` for the staff shell. Portal uses `Portal/Login.aspx` and the same membership store.
- **Provider:** `SqlMembershipProvider` on `TrackerDataSQL`, application name `/`.
- **Password policy:** Minimum length 6, at least one non-alphanumeric, unique email required, password retrieval off, reset on, security question off.
- **Lockout:** 5 invalid attempts, 10-minute window.
- **Machine key:** Explicit `<machineKey>` in config. Rotating it signs everyone out. Do not copy production keys into documentation or source.
- **Anonymous:** Denied at the application root. `Account/Login.aspx` is allowed with `<location>`. Other public endpoints (`RecoverPassword`, `DisableClient`, `api`, `driver`, portal login, proof handler, `ViewMyOrder`) **must be confirmed** in the remainder of `Web.config` location blocks. A missing allow-rule is a production defect; an overly wide allow-rule is a data leak.

### 1.2 Driver API

- **Not cookie auth.** `Authorization: Bearer` only (`MobileApiHandlers`).
- **Issue:** `POST /api/v1/auth/login` with the same membership username and password.
- **Who may receive a token:** User linked to `PeopleTbl.SecurityUsername`, or an admin (`MobileAuthManager`). A normal agent without a person row should fail. **Verify** the exact error text.
- **Storage:** SHA-256 of a 256-bit token in `MobileDeviceTokensTbl`. Raw token is not stored.
- **Lifetime:** `MobileApi.TokenDays` 90. Idle revoke `TokenIdleDays` 30.
- **Re-login:** Replaces the previous token for that device (see mobile API doc).
- **HTTPS:** `MobileApi.RequireHttps` true for phones. `X-Forwarded-Proto` trusted only if `TrustForwardedProto` is true.
- **Throttles:** 10 failed logins per IP per 15 minutes; 120 calls per device per minute. In-memory, reset on app recycle.
- **Account re-check:** About every 5 minutes the token is rejected if the user is deleted, unapproved, locked, or unlinked from the person.
- **CORS:** App web-view origins only (`https://localhost`, `capacitor://localhost`), not a public browser origin list.
- **Headers:** `no-store`, `nosniff`, `X-Frame-Options: DENY`, `Referrer-Policy: no-referrer`, HSTS on HTTPS.

### 1.3 Customer token links (no login)

- **Disable client:** Signed with `DisableClientSecret`. Invalid token shows `DisableClient.ErrorInvalidToken` and does not change data.
- **Public order view:** `PublicOrderViewManager` / `ViewMyOrder.aspx`. **Verify** the token includes the order id and an expiry. If it is only an order id in the query string, that is an IDOR risk to fix in RainbowOF, not to preserve.
- **Proof PDF:** Handler checks staff login or portal ownership. Unknown id returns 404 so callers cannot probe.

---

## 2. Roles

Roles are strings in `aspnet_Roles`. Code does not seed a single canonical list except `Contact` (`MembershipRoleManager.EnsureRoleExists` from the portal installer).

| Role string | Where it is trusted | Intended person |
|---|---|---|
| `Administrators` | `Web.config` allow list; `SecurityManager` admin alias | Owner / IT |
| `Administrator` | `SecurityManager` alias only. **Not** in the `Web.config` allow list | If this role exists without `Administrators`, the user may be denied at the URL and still pass `IsAdmin` on a page that is reached another way. **Verify** live data |
| `Admin` | Page checks (Woo import, lookups, GPS, invites, several tools). **Not** a `SecurityManager` alias and **not** in `Web.config` allow list | Same hazard |
| `Agent Manager` | `Web.config` allow list | Senior operations |
| `AgentManager` | `SystemTools.aspx.cs` only | Spelling variant. A user in `Agent Manager` is allowed into the site but may fail this check; a user only in `AgentManager` is denied by `Web.config` |
| `Agents` | `Web.config` | Daily staff |
| `Repair` | `Web.config` | Technicians |
| `repair` | `CustomerDetails.aspx.cs` only | Case-variant. SQL role provider default is case-insensitive, but do not rely on that |
| `Contact` | Portal installer | Customer |

**Hardcoded user:** `warren` is admin in `SecurityManager` even with no role. RainbowOF must not hardcode a personal username. Use a break-glass account that is data, audited, and optional.

`Portal/ViewAs.aspx` lets someone see the portal as a customer. **Verify** it requires `IsAdmin`. If it only requires a staff login, any agent can impersonate. RainbowOF: admin permission `Portal.Impersonate`, reason captured, written to the audit log, banner always visible.

---

## 3. Authorisation map

There is no permission table. Page checks are ad hoc.

| Capability | Who, as coded |
|---|---|
| Entire staff site | Roles in the `Web.config` allow list |
| Manage users and roles | Administration pages. **Verify** they call `SecurityManager.DemandAdmin` or rely only on “any staff role can open the URL”. If the latter, any Agent can create administrators. That must not survive |
| Woo import, Woo mapping, system preferences, postal tools, Quaffee Lovers, delivery promise, depot, customer location tools | `IsInRole("Administrators") \|\| IsInRole("Admin")` |
| System tools hub | Those plus `AgentManager` |
| Driver tracking data feed | Administrators or Admin |
| API admin routes (`/devices`, `/log`, proof list and signature) | `MobileAuthorize(AdminOnly = true)` which uses `SecurityManager` admin, not the `Admin` role string |
| Repair-only user on old customer page | Blocked when the sole role is repair |
| Contact portal pages | Role Contact and a row in `ContactUserLinkTbl` |
| Own POD | `CanViewProof` |
| Another contact’s orders | Portal queries must filter by linked contact id. **Verify** `PortalPageBase` sets the contact and every repository call uses it |

`Lookups.aspx` contains an admin check around line 3436. **Verify** whether non-admins can still edit items and prices. If agents can edit `PriceLevelsTbl` and `ItemsTbl`, that is accepted today and should become a permission (`Catalogue.Edit`) rather than an accident.

---

## 4. Data restrictions

| Data | Restriction |
|---|---|
| Customer commercial fields (VAT, bank, limit) | No field-level security. Anyone who can open the contact can read them. RainbowOF should split `Customer.ViewCommercial` |
| Portal | One contact. Change requests rather than direct writes (**verify**) |
| Walk-in notes | May contain a personal email inside `[#email#]`. Treat notes as personal information |
| Signatures | `no-store` on the API signature route. Staff handler must not be cached publicly |
| GPS history | Only stored inside work hours when tracking is enabled. Still personal data (employee location). Access is admin on the tracking page |
| API log | No bodies or tokens. IP addresses are personal data; 30-day retention |
| Email test mode | Prevents accidental customer mail from dev. Not a security boundary |
| Woo keys | Encrypted with `WooCommerceCryptoKey` before `WooCommerceSettingsTbl` |
| Connection strings and mailbox passwords | Local `Web.config` is git-ignored because the GitHub repo is public (`Documentation/HARD_PROJECT_RULES.md`). RainbowOF secrets belong in a secret store, not in appsettings committed to git |
| Database backup files | Written to a folder SQL can access. Those files are full customer copies. Restrict NTFS and do not put them under the web root |
| Machine key and disable-client secret | Forgery of auth cookies and unsubscribe links if leaked |

---

## 5. Administrative functions

| Function | Risk if misused | Control today |
|---|---|---|
| Manage users / roles / reset password | Full takeover | Administration pages; tighten to admin |
| XML to SQL | Arbitrary DDL and DML | Admin tool. Remove from the product UI in RainbowOF; use migrations |
| Database backup | Data exfiltration | Admin page |
| Generate machine keys | Session forgery if published | `Tools/GenMachineKeys.aspx` |
| API tester and token revoke | See devices, kill a driver session | Admin |
| View as contact | Privacy | Portal page |
| Email diagnostics | Send from the orders mailbox | Admin |
| Merge from QuickBooks | Overwrite customer identity | Admin tool |
| System preferences | Turn off tracking, change Woo keys, change POD terms | Admin |
| Message editor | Change legal or customer wording | Tools |
| Log viewer | May contain emails inside log messages | Staff who can open the page |

---

## 6. Audit requirements to carry forward

| Event | Today | RainbowOF requirement |
|---|---|---|
| Contact field change | `ContactChangeLogTbl` | Keep, and include old and new value |
| Checkup send and skip | `SentRemindersLogTbl` | Keep |
| Client mail | `ClientMailSendLogTbl` | Keep |
| Application errors | `LogTbl` | Structured logs (Serilog) plus an admin viewer |
| API calls | `MobileApiRequestLogTbl` | OpenTelemetry spans; do not log bodies, tokens, signatures |
| Order line edits | **Not audited** | Add: who, when, old qty, new qty |
| Impersonation | **Not clearly audited** | Required |
| Role changes | Membership, no app audit | Required |
| Disable-client | Email to admin and log | Keep the email and add an audit row |
| Proof captured | Row in `MobileDeliveryProofTbl` | Keep signature access audit (who downloaded) |

---

## 7. Recommended RainbowOF permission set

Replace role-name spaghetti with permissions. Seed roles as bundles.

| Permission | Typical role |
|---|---|
| `Crm.View`, `Crm.Edit`, `Crm.ViewCommercial`, `Crm.Disable` | Agent, Agent Manager, Admin |
| `Order.Edit`, `Order.Complete`, `Order.OverridePromise` | Agent |
| `Prep.View`, `Delivery.Dispatch` | Agent |
| `Repair.Edit` | Repair, Agent |
| `Catalogue.Edit`, `Area.Edit` | Agent Manager |
| `Channel.Import`, `Channel.Map` | Admin |
| `Identity.Manage`, `Portal.Impersonate`, `Audit.View` | Admin |
| `Driver.Use` | Driver (person-linked) |
| `Portal.Self` | Customer, scoped by `CustomerUser` link |
| `Report.Export` | Agent Manager |

Repair-only users get `Repair.Edit` and `Crm.View` without `Crm.ViewCommercial` and without `Order.Complete` unless the business wants technicians to close deliveries.

Customer tokens for “view this order” and “disable reminders” should be signed, single-purpose, and expiring. Do not use a raw integer id.

---

## 8. Privacy notes (South Africa)

The system stores names, emails, phones, physical addresses, GPS of customers and of employees, signatures, VAT and bank numbers, and order history. Quaffee’s disable-client flow exists so a person can stop reminders. RainbowOF should keep:

- purpose limitation (GPS only while tracking is on, during work hours)
- a customer-visible stop-communications action
- retention (API log 30 days is the only explicit retention; define retention for GPS, signatures, and logs)
- no secrets in the git repository

This is not a legal opinion. Have the operator confirm POPIA roles (operator vs responsible party) when RainbowOF is hosted for more than one company.
