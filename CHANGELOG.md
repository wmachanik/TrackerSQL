# TrackerSQL changelog

Release notes for shipped TrackerSQL versions. Newest first.

## 3.0.3.0 — 2026-09-28 (live 2026-09-29)

### Deployment
- **Live address:** `https://tracker.quaffee.co.za` (primary; `https://quaffeetracker.co.za` serves the same site). `ApplicationBaseUrl` and the driver app's default server use it.
- **Config files and secrets:** real `Web.config`, `web.live.config` and `Web.Release.config` are no longer in git; sanitized `*.example` copies (every secret `mypassword`) are committed instead. See `Documentation/DEPLOYMENT_AND_CONFIG.md`. Secrets in older public commits must be rotated.
- **CORS** trimmed to the app's own origins (`https://localhost`, `capacitor://localhost`).
- New docs: `Documentation/MOBILE_API_AND_DRIVER_APP.md`, `Documentation/DEPLOYMENT_AND_CONFIG.md`; Hard Rule #0f (no secrets in git).

### Features
- **Mobile REST API for a delivery driver app** — JSON API at `/api/v1` (ASP.NET Web API 2, inside the Tracker site) for an Android / iPhone app. Drivers sign in with their Tracker login, which must be linked to a person in Lookups → People (Security Username). Administrators can sign in without being linked.
  - **Sign-in:** `POST auth/login` returns a long-lived device token (90 days, `MobileApi.TokenDays`), sent as `Authorization: Bearer <token>`. Only a hash of the token is stored. Signing in again on the same device replaces its old token; `POST auth/logout` revokes it, and administrators can list and revoke devices (`GET devices`, `POST devices/{id}/revoke`). `GET auth/me` returns the user and person.
  - **Delivery sheet offline:** `GET delivery/dates` and `GET delivery/sheet?date=&person=me|all|id` (the driver's own deliveries by default; they can open another driver's or everyone's to help out, and the request log notes it) return each stop with the contact person's name (for ZZName orders, the name from the order notes), phone, cell, address, area, notes, PO, items and any proof already captured, in delivery order.
  - **Proof of delivery, captured offline:** `POST delivery/sync` takes a batch of deliveries (received-by name, signature image, note, time, location, or Not Delivered). Each carries a phone-generated `clientRef`, so retrying a sync never records it twice. By default the proof is stored and the office completes the order; System → Driver App can run Order Done straight away instead. `GET delivery/proofs` and `GET delivery/proofs/{id}/signature` show what was captured.
  - **Repairs:** `GET repairs` (open only, by contact, or changed since a time), `GET repairs/{id}`, `POST repairs` (create; `clientRef` makes offline creates safe to retry) and `PUT repairs/{id}` (send only the fields that change). Saves follow Repair Detail: the related order is kept in step and the contact is emailed on a status change.
  - **Lookups and contacts:** `GET lookups` (repair statuses, faults, machine types, conditions, people, driver options) and `GET contacts?q=` / `GET contacts/{id}`.
  - **Driver options:** the choices the app offers when capturing a delivery ("Left at reception", "Handed to an assistant", "No signature obtained"; "Nobody there", "Rejected / refused" …) are edited in Lookups → Driver Options. "Needs name" makes the driver enter who received it or get a signature. The chosen option is stored with the proof (`Reason`).
  - **Items not handed over:** the driver ticks off what was handed over; anything short or to follow is stored with the proof (`MissingItems`), and such a delivery is never auto-completed, so the office can sort out the rest.
  - **Repairs on the sheet:** each stop lists its item lines with order line ids, marks repair (Service) lines, which are not handed over, and includes the repairs linked to the order (job card, machine, serial, fault, status).
  - **Data efficient:** replies are gzip-compressed (the sheet is about 7 KB, 2 KB on the wire), nulls are left out, and the sheet, dates, repairs and lookups carry an ETag, so the app gets a 304 with no body when nothing has changed. Phones may also gzip what they send.
  - **Security:** HTTPS is required except on the server itself (`MobileApi.RequireHttps`), and cross-origin calls are only accepted from the app origins in `MobileApi.CorsOrigins`. Tables are created automatically on first use (`SQLCommands-MobileApi-01.xml`).
  - **Request log:** every call is recorded (time, user, call, result, time taken, bytes each way, IP address, and a short note such as "Sign-in failed for 'x'" or "3 deliveries: 3 saved"). Only these facts are kept, never what was sent, so passwords, tokens and signatures are not logged. Kept for 30 days (`MobileApi.LogDays`; 0 turns it off). Administrators can read it with `GET log`.
- **Mobile API tester** — `Tools/ApiTester.aspx` (System Tools, administrators only).
  - A bar at the top always shows who the calls are being sent as, with a Sign out button.
  - Tabs: Sign in (like the phone, or with your web sign-in), Delivery sheet, Deliver an order (with a signature pad), Repairs, Contacts & lookups, Devices (list and revoke), Request log (filter by user or problems only, optional auto-refresh), and Settings & security (current settings, how the API is protected, and a Check API tables button).
  - Each reply shows the result, time taken, size downloaded (and compressed size) and the version (ETag), with an "only if changed" option to test 304s.
- **Lookups → Driver Options** — new tab to add, edit, hide or delete the delivery choices shown in the driver app.
- **Driver app on the website** — the Quaffee Driver app is also served from `/driver/` (built by TrackerDriver `npm run build:site`). Drivers open it on their phone and choose "Install app" / "Add to Home Screen"; it keeps working with no signal. `/driver` is anonymous in Web.config (the app signs in through the API) and uses `index.html` as its start page.
- **System → Driver App** (`Tools/DriverApp.aspx`, also a System Tools card and a Home page card) — download page for the Android app (`Downloads/QuaffeeDriver.apk`, built by TrackerDriver `npm run apk`) with install steps, plus the web app link. Web.config maps `.apk` to `application/vnd.android.package-archive`.
  - **Settings (administrators):** run Order Done when a driver sends a delivery; email the client a delivery confirmation; let drivers email their note to the office, and the office address for those notes (empty = `SysCCEmailAddress`). Stored in `MobileApiSettingsTbl`; until saved, "run Done" follows the old `MobileApi.AutoCompleteDeliveries` appSetting.
  - **Delivery confirmation email:** who received it, when, how it was delivered, what was delivered and what is still to follow, with the signature attached; copied to the orders address. ZZName orders use the address in the order notes. When Order Done also runs, its "delivered" email is skipped so the client gets one email.
  - **Driver notes to the office:** the driver can tick "Email this note to the office" (e.g. the client asked for a change); it is emailed with the client, order, PO and delivery details.
- **Driver app quantities** are rounded with Tracker's `NumDecimalPoints` (3) instead of 2.
- **Short deliveries:** the driver can enter the quantity actually handed over per item (`lines` on sync); the shortfall is stored as "1 × Kenya AA (1kg) (1 of 2 delivered)" and the delivery is left for the office.
- **Driver app list:** the Unconfirmed tag and the order number are no longer shown; the PO is shown instead when there is one.
- **Mobile API hardening:**
  - Too many failed sign-ins from one IP address (any usernames) pause sign-ins from there for 15 minutes with a 429 (`MobileApi.LoginFailuresPerIp`, default 10), on top of the normal lockout after 5 wrong passwords.
  - A phone not used for 30 days must sign in again (`MobileApi.TokenIdleDays`).
  - A token stops working within 5 minutes when its login is deactivated, locked, deleted or unlinked from its delivery person; the app is told why.
  - Each phone may make 120 calls a minute (`MobileApi.RequestsPerMinute`); more get a 429 with Retry-After.
  - Signatures and the proof list are administrators only, and signatures are never cached.
  - Every API reply is marked no-store (unless it carries an ETag), nosniff, no framing and no referrer; HTTPS replies send Strict-Transport-Security.
  - `X-Forwarded-Proto` is only trusted when `MobileApi.TrustForwardedProto` is true.
  - `/driver/` sends a Content-Security-Policy (only its own scripts, only calls to this site).
  - The ASP.NET version header is no longer sent.
  - The API Tester's Settings & security tab shows the new settings.
- **Driver app data status:** the top bar shows where the lists come from: Live (from Tracker), Updating (downloading), Offline or Phone copy (saved on the phone), or Wi-Fi only. The delivery and repair lists say "From Tracker …" or "Phone copy from … (why)".
- **Driver app mobile data options** (More): "Send deliveries and repairs using mobile data" and "Update lists using mobile data". When off, captured deliveries wait for Wi-Fi ("2 for Wi-Fi"; "Send now using mobile data" still works), and saved lists update only on Wi-Fi or when ⟳ is tapped. "Save everything on this phone" sends anything waiting and saves the day's deliveries (the driver's and everyone's), open repairs and lookups before leaving.

### Fixes
- Order Done can now run outside a web page session (needed for auto-completing deliveries from the API).

## 3.0.2.0 — 2026-09-25

### Features
- **Contact Portal** — contact-facing portal (shown to contacts as "My Quaffee") (`/Portal/`) for contact details, orders, recurring orders and repairs; admin invite from Contact Details; admin page `Tools/ContactPortalAdmin.aspx`. Schema pack `SQLCommands-ContactPortal-01.xml`. Portal header carries the Quaffee logo; portal home uses clickable dashboard cards (2 per row, incl. Change Password and Sign Out).
- **Contact Portal pages** — Title Case headings/menus (My Details, My Orders, My Repairs, My Recurring Orders); 4-column field layouts; full-width tables with green View buttons; status shown on orders, repairs and recurring orders; orders list shows every item on one line (cut off with "…" to fit, full list in the tooltip) and no order number; order detail shows Delivery / Dispatch Date, PO and item/packaging names (no IDs), plus Courier Details (courier, waybill and a Track Parcel link) once dispatched; repairs show machine, fault and status names; recurring shows last and next dates; Request A Change in its own panel on each page, with the Back button beside Submit Change Request and a subtitle pointing contacts to it. Fax and Extension removed from editable contact fields.
- **Portal reminder toggle** — My Details → Email Preferences has an on/off switch for checkup reminder emails (contact stays enabled; turning off notifies orders@; both directions are recorded in the Change Log). The switch is locked, with an explanation, while the contact has an active recurring order, because reminder and recurring calculations can conflict. Closing an account is via change request.
- **Portal next coffee and paging** — Home and My Orders show when the contact next needs coffee: the next recurring delivery date, or otherwise the next predicted coffee date (NextCoffeeBy, shown whether or not reminders are on). My Orders, My Repairs and My Recurring Orders show 10 rows per page.
- **Portal notifications to orders@** — orders@ is emailed when an invited contact activates the portal (sets their own password), as well as for change requests. Both emails link to the contact, and change-request emails also link to Contact Portal Admin. Every send is recorded in `system.log` (success, failure reason, or test-mode redirect).
- **Contact Details portal status** — the long portal description is now a short badge (*Not Linked* / *Invited* / *Active*) beside Reminders, with details in the tooltip.
- **Email portal blurb** — the raw URL after "request access here" / "Sign in here" is removed (the link remains), and the portal is named "Quaffee Customer Portal".
- **Portal Repeat My Last Order** — contacts can repeat their last order from the portal (`Portal/RepeatOrder.aspx`, via a green button in the next-coffee box on Home and My Orders).
  - The delivery / dispatch date comes from the contact's area schedule, the same as a new order in Order Detail.
  - There is a PO box (required when the account needs a PO) and an optional "Anything To Change?" text box.
  - The order is created unconfirmed, with the requested changes in the order notes. It is recorded in the Change Log and emailed to orders@ with links to the order and the contact.
  - The contact gets an acknowledgement email, not the "good news" confirmation. It says we will endeavour to supply the order as placed, but it may need to change depending on stock and availability. Staff send the normal confirmation from Order Detail once the order has been checked.
  - Not offered while the contact has an order that is not completed. The next-coffee box then shows "You have an order in progress" with a View My Order button, and the page itself redirects to that order.
- **Contact Details layout** — Alt Email sits under Cell. Delivery By, Agent and Courier are in that order, and Courier only appears when the delivery person is a dispatch person (Courier / Pargo). The Courier list sizes to its content.
- **Portal please-wait** — a Please wait overlay covers the portal while Submit Change Request, Save or the reminder switch is processing.
- **Portal link in repair status emails** — repair status emails now include the join / already-joined portal blurb.
- **Contact change log** — audit of contact edits (`SQLCommands-ContactChangeLog-01.xml`).
- **Single sign-in** — `Account/Login` is the one sign-in page for everyone: staff use their username, contacts their email, and the account's role decides where they land (Contact role to the portal, staff to Tracker). The only difference is registration: staff use Register (an administrator activates the account), contacts use `Portal/Login.aspx?request=1` to get a temporary password. Old links to `Portal/Login.aspx` forward to the sign-in page, and portal sign-out returns there.
- **Set your password straight from the email** — the temporary-password email now links to `Portal/ChangePassword.aspx?u=<email>`. It opens signed out with the email filled in; the contact enters the temporary password and a new one, and is signed in (orders@ still gets the activation email). The page states the password rules (6 characters, 1 symbol) up front instead of failing with an error.
- **Contact Portal admin** — the editable fields and the change requests are now separate panels, and fields show plain-English names (e.g. *First name*, *Alternate email address*). Change requests:
  - are paged (10 per page), with a filter for Open, Done, Rejected or All;
  - are never deleted: Done / Reject sets the status and keeps who resolved it and when;
  - take an optional note to the contact, and the contact is emailed the outcome (with the note and their original request);
  - are recorded in the contact's Change Log when resolved.
- **Disabled contacts in the portal** — a signed-in contact whose record is disabled sees an *Account Disabled* notice on every portal page. The next-coffee box offers Repeat My Last Order instead of a stale prediction, and staff emails for such orders flag that the contact is disabled.
- **Contacts list remembers the last search** — the Filter By field, the search text and the Enabled / Disabled / Both setting are kept in a browser cookie (90 days) and restored when the Contacts page is opened again. Reset clears the search but keeps the Enabled setting.
- **Portal access page** — the email box is wider, so full email addresses fit.
- **Portal order tracking for older waybills** — when a waybill has no courier saved (older ones only say "Courier"), the portal works out the courier from the carrier name, or else the contact's preferred / default courier (the same rule used when the waybill is captured), so Track Parcel appears.
- **Portal online shop order** — orders imported from WooCommerce show the shop order number on the portal order page. When the shop order belongs to a shop account (not a guest checkout), it links to that order in the contact's Quaffee shop account (*View In Online Shop*).
- **Contact Details** — Contact Portal status moved to the second column pair beside Reminders; "Last sent" takes one column.
- **Admin preview of the Contact Portal** — administrators get a *View in portal* button on Contact Details that opens the portal in a new tab as that contact (`Portal/ViewAs.aspx`). Every page shows exactly what the contact sees, with a blue *Admin Preview* banner and an *Exit Preview* option (returns to the contact). The preview is read-only: saving details, change requests, repeat orders, the reminder switch and Change Password are blocked, so nothing is saved or emailed in the contact's name. Each preview start is recorded in the Login log. `Portal/Web.config` now also allows the Administrators role (non-preview admins are sent back to Tracker).
- **"Contact" everywhere internally** — System Tools, the Contact Portal admin page, Contact Details, Change Log entries and the emails to orders@ now say *Contact Portal* instead of *Customer Portal*. Internally (staff screens, logs, Change Log, staff emails, code comments and staff/log messages in Messages.resx) it is always *Contact*; only text contacts themselves see uses the portal's public name.
- **Portal renamed "My Quaffee" for contacts** — the portal header, footer, access page, Home cards, Change Password, the invite / temporary-password / multi-account emails, the email-footer blurb, the order acknowledgement and the change-request outcome email now say *My Quaffee*. The Contact Portal admin page also has a *View in portal* link on each change request (admin preview).
- **Contact Portal layout** — like the main system, the page body is a white backdrop panel (98% of the window, was capped at 1100px) with the panels and grids in a 94% column inside it. My Orders shows each order's items on one line, cut off with "…" to fit (full list in the tooltip). *View in portal* on Contact Details is a standard-size brown button. System Tools' Contact Portal card uses the new `ContactPortalIcon.jpg`.
- **Choose contact on self-serve access** — when a requested email is on more than one enabled contact, the contact is emailed a list of those contacts with signed links (valid 24 hours). Following a link sends a temporary password for that contact and moves the portal login to it.
- **Contacts email search** — the Email filter now matches main or alt email.

### Fixes
- **Portal invite "role 'Contact' was not found"** — role creation now uses the migrated-schema `RoleId` fallback (shared `MembershipRoleManager`, also used by Manage Roles).
- **Contact Portal changes go to the contact Change Log** — reminder on/off, portal access granted (and login moved between contacts), password changes and change requests are recorded in the Change Log (source *Portal*) instead of contact Notes. The email-link opt-out is also recorded (source *Disable*). Contact field edits were already logged.
- **Delivery sheet logo size** — uses the new 80px `QuaffeeLogoSmallest.jpg` with space to its right; the opt-out page is fixed at 100px.
- **ZZName never invited** — sundry contacts (ID 9 or company starting "ZZ") are refused on every invite path and never get the portal email blurb.
- **Last Order with no history** — no longer inserts an `ItemID = 0` line (FK error) and no longer creates an empty order header first; shows a warning instead.
- **Default.aspx required no login** — removed the anonymous `<location path="Default.aspx">` rule (Web.config and sample/live configs); the home page now redirects to sign in.
- **Portal invite duplicate block** — admin invites are tied to the contact ID; other contacts sharing the email no longer block, and are listed with links to open them (disabled / has-login badges). Only an enabled contact already holding the login blocks; a disabled holder's login is moved over.
- **Contacts `?CompanyName=` filter** — quotes escaped in the list filter.

## 3.0.1.11 — 2026-09-16

### Features
- **Editable waybills after Order Done** — Order Detail shows waybill as display-only with a pencil; popup to correct number/courier, optional customer notify email (`Order.WaybillUpdated*`), and Woo customer note when linked.
- **Tracking welcome email** — New contacts (Contact Details insert and Woo create) that are tracking-enabled get a thank-you / checkup intro from Messages.resx with a Disable Client link.
- **Disable Client choices** — Clear options for reminders-only vs disable account entirely, with short explanations and matching success copy from Messages.resx.
- **Send Reminder on Contact Details** — Manual one-off checkup reminder (no order created); uses Messages.resx wording, updates reminder count/log after a successful send.
- **Duplicate waybill guard** — Order Done and waybill edit block reuse of the same tracking number + courier; warning names the previous customer and dispatch date.
- **Courier deep-link tracking** — `CourierServicesTbl.TrackingUrlParam` (e.g. Fastway `?l=`, Pargo `?code=`, Courier Guy `?ref=`). Emails and Order Detail link the waybill number when a param is set. Go-live pack: `App_Data/GoLive/SQLCommands-CourierServices-01.xml` (and `App_Data/SQLCommands-CourierServices-01.xml`).

### Fixes
- **Send Reminder SQL** — `GetCustomerTypicalItems` no longer uses illegal `SELECT DISTINCT` + `ORDER BY` on a non-selected column.
- **Send Reminder templates** — Manual reminder no longer depends on `SendCheckupEmailTextsTbl` column/name mismatches; content comes from Messages.resx.
- **SendCheckEmailTexts repository** — Legacy Access table name corrected to `SendCheckupEmailTextsTbl` (for the batch Send Coffee Checkup page).
- **Order Detail waybill UI** — Removed cluttered inline editors and the separate “track” label; number is the hyperlink when a deep-link param exists.
- **HtmlEditor toolbar** — Site.css exclusions so AjaxControlToolkit editor chrome is not crushed by global button styles (cache bust on Site.Master).

### Go-live notes
- Run **XMLtoSQL → `SQLCommands-CourierServices-01.xml`** (or open Lookups so `EnsureExists` seeds) to add `TrackingUrlParam` and refresh Fastway / Pargo / Courier Guy URLs.
- Deploy Messages.resx / rebuilt resources with the site.

## 3.0.1.10 — 2026-09-15

- People **IsDispatched**, Woo import polish, delivery-promise group labels (see git commit `9d5d452`).
