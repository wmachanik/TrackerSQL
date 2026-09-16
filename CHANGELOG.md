# TrackerSQL changelog

Release notes for shipped TrackerSQL versions. Newest first.

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
