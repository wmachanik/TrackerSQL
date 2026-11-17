# 09 — RainbowOF Functional Specification

**Product:** RainbowOF (Rainbow Order & Fulfillment)  
**Replaces:** QOnT / TrackerSQL 3.0.4.x without dropping the capabilities in `docs/01` through `docs/04`.  
**Domain language:** `docs/08-domain-modernisation.md`.  
**This document is the master product specification.** Architecture is `docs/11`. Screen technology is `docs/10`. Sequence is `docs/12`.

Status labels: **Parity** (QOnT has it), **Improve** (same job, cleaner), **New** (not in QOnT; do not block first cutover unless marked required).

---

## 1. Product principles

1. A delivery can be completed with no price on the line.
2. Consumption forecasting is optional per customer and empty when the customer has no service plan.
3. The production calendar and the customer promise are different dates and both visible.
4. One customer, many locations, many assets, many contacts.
5. Channels create orders; they do not have their own fulfilment store.
6. Guest orders do not require a permanent customer.
7. Permissions are explicit (`docs/07`), not a pile of role-name spellings.
8. Coffee is the first configured catalogue, not a compile-time assumption.

---

## 2. Module index

| Module | Purpose |
|---|---|
| CRM | Customers, locations, people, absence, programmes, portal |
| Products | Catalogue, packs, preparations, cycles, units |
| Inventory | Optional warehouse balances; customer on-hand is in CRM/fulfilment |
| Purchasing | **New.** Not in QOnT. Phase after parity |
| Fulfillment | Orders, standing orders, waves, promises |
| Delivery | Runs, proof, carriers, tracking |
| Billing | Document intent, external invoice status. Not a ledger at cutover |
| Servicing | Assets, meters, plans, tasks |
| Reporting | Operational queues and exports |
| Administration | Identity, settings, channels, audit, messages |
| Channels | WooCommerce first |
| Portal | Customer self-service |

---

## 3. CRM

**Purpose:** System of record for who is served and how.

**Features**

- Customer create and edit: trading name, relationship, enabled, notes, contract reference (**Parity** `ContactDetails`).
- Commercial profile: PO required, VAT, registration, bill-to, accounts email, payment terms, credit limit, price agreement (**Parity** `ContactsAccInfo`). Credit limit is displayed and **not** a hard stop until the business turns on **New** enforcement.
- Locations: primary address, GPS, extra ship-to addresses, address kind, “counts toward consumption” (**Parity** delivery on behalf).
- People at the customer, distinct from the trading name (**Improve** — today two name pairs on one row).
- Preferences: primary and secondary product, qty, preparation, pack, “usually include secondary”, preferred worker, preferred carrier, sales worker (**Parity**).
- Flags: auto-fulfil, prediction disabled, always invite, normally responds, send confirmation, uses linked consumable (the filter flag, generalised) (**Parity**).
- Absence calendar and reasons (**Parity**).
- Self-disable and staff disable: invitations only, or full stop. Standing orders listed for a human. Signed link (**Parity** `DisableClient`).
- Change log (**Parity**).
- Portal user invite and link (**Parity**).
- Change requests from the portal, staff approve (**Parity**, verify apply rules).
- Programme membership (Quaffee Lovers as the first programme) and push external role (**Parity**).
- Postal assist for South Africa (**Parity**), behind an address service so other countries can be added (**Improve**).
- Guest bucket is not a real customer id (**Improve**).
- Merge from an accounting extract (**Parity** QuickBooks tool) as an admin import.

**Screens:** Customer list, customer record (tabs: profile, commercial, locations, people, preferences, assets, orders, standing orders, service tasks, invitations, audit), absence editor, disable confirmation, merge wizard, programme invite.

**Workflows:** Onboarding (`docs/03` §2.1), away, disable, portal change approval.

**Permissions:** `Crm.View`, `Crm.Edit`, `Crm.ViewCommercial`, `Crm.Disable`.

---

## 4. Products

**Purpose:** What can be ordered, how it is prepared and packed, how a cycle rotates.

**Features**

- Product CRUD: sku, name, short name, enabled, characteristics, detail, kind, replacement, unit, units per qty, sort (**Parity** Lookups Items).
- Preparations and packs with sheet colour and symbol (**Parity**).
- Kinds: consumable, maintenance, meter, exchange, note, cycle header (**Parity** service types, renamed).
- Product categories (**New**, required for non-coffee navigation; coffee can sit in one category at cutover).
- Cycles and steps (**Parity** item groups). On completion, advance the customer’s position (**Parity**).
- Unit conversions, including the historical 100 cups per kg as data (**Improve**).
- Service-plan templates: which kind sets the usage rate, typical averages, dependent maintenance (**Parity** `TrackedServiceItemsTbl`).
- Warning product: “this should be a service task” (**Parity** item 36) as a product flag, not a hardcoded id.

**Screens:** Product list (MudDataGrid), product editor, pack list, preparation list, cycle editor, unit conversions, service-plan template.

**Workflows:** Discontinue a sku by setting replacement; next order suggests the replacement (**Improve** — field exists, suggestion behaviour **verify**).

**Permissions:** `Catalogue.Edit` for writes; all fulfilment roles can read enabled products.

---

## 5. Inventory

**Purpose:** Separate three things QOnT mixes in language only.

| Concept | Spec |
|---|---|
| Customer on-hand | Captured at delivery completion in sales units. Reduces the meter or the usage event. **Parity** |
| Meter | Optional reading on an asset. **Parity** cup count |
| Warehouse on-hand | **New.** Not required for coffee cutover. When enabled: product, site, qty, no bin complexity in v1 |

**Features for cutover:** Customer on-hand and meter only. No reservation, no negative-stock block.

**Screens:** None standalone at cutover. Fields sit on delivery completion. A later stock screen is out of parity scope.

**Permissions:** `Order.Complete` writes customer on-hand and meter.

**Purchasing** is a later module: supplier, purchase order, receipt into a site. QOnT’s collection stops (`DriverCollectionOnly`, collections sheet) are **fulfilment pickups**, not purchase orders. Spec them under Delivery so supplier collections still run. A purchase order module waits until the business wants matching and costing.

---

## 6. Fulfillment (orders)

**Purpose:** The unit of work from demand to “ready for a run”.

**Features**

- Create and edit order: customer or guest, location, ordered, prepare-on, required-on, worker, document intent, customer PO, notes, lines (product, qty, preparation, pack) (**Parity**).
- Duplicate rule: one open order per customer location per required date; offer to open the existing one (**Parity**).
- Qty minimum 1 with a visible correction, not a silent rewrite (**Improve** — today it silently becomes 1).
- PO required banner when the customer flag is set and PO is empty (**Parity**). Hard stop is a setting, default off to match today (**verify**).
- Move date, move line, delete emptied order (**Parity**).
- Packed, confirmed, done, billing-status flags (**Parity**).
- Standing orders with cycle, lines, worker, document intent, enabled. Email on add, update, disable (**Parity**).
- Generate the next real order from a standing order without duplicating a date (**Parity** — generator must be specified during build; see open question in `docs/04`).
- Do not send a replenishment invitation when the standing cycle is shorter than the configured minimum (default 20 days) (**Parity**).
- Auto-fulfil flag skips the need for a fresh “yes” when the business confirms that meaning (**Verify then Parity**).
- Replenishment invitation run: window (default 9, clamp 5–30), skip internal, skip bad email, skip over cap (10), skip absence, exclude-this-time, log every attempt (**Parity**).
- Invitation deep link can open an order with products prefilled (**Parity** SKU parameters).
- Guest identity in structured fields (name, email, phone, GPS), not only a coded note (**Improve**, and keep parsing old notes).
- Channel import creates the same order shape (**Parity** Woo).
- Sooner-than-promise indicator (**Parity**).
- Inter-site transfer as an order with document intent Transfer and a destination site (**Parity** if Vineyard/Buiten still used).

**Screens:** Order list with filters (date, customer, worker, status), order editor, standing-order list and editor, invitation run, invitation log, date-shift tool.

**Workflows:** `docs/03` §§2.2–2.6 and 2.8.

**Permissions:** `Order.Edit`, `Order.Complete`, `Order.OverridePromise`.

---

## 7. Delivery

**Purpose:** Get the prepared order to the location and prove it.

**Features**

- Territory and calendar: prep weekday, delay, default sequence (**Parity**).
- Promise policies (**Parity** `DeliveryPromiseManager`).
- Closures (**Parity**).
- Wave views: production list, summary, category summary (coffee summary is the consumable filter) (**Parity**).
- Run sheet for a date and worker, including pickups (**Parity** delivery sheet and collections).
- Sequence: worker override, else suggested from territory order and travel (**Parity**).
- Driver client (the current app can remain if it speaks the new API, or a MudBlazor mobile page): offline sync, client reference, outcomes, received-by, signature, terms, short quantities, note to office (**Parity**).
- Proof PDF and email (**Parity**).
- Completion queue when a meter is required and not yet entered (**Parity** awaiting done).
- Carrier consignments: required when mode is carrier and the setting says so; duplicate warning; pattern warning (**Parity**).
- Live location during work hours only (**Parity**).
- Depot, route speed, service minutes (**Parity** settings).
- Landline prefixes excluded from chat links (**Parity**).

**Screens:** Run sheet, wave sheets, sequence editor, awaiting completion, tracking map, carrier editor, outcome editor, depot and promise and closure admin, driver shell.

**Permissions:** `Prep.View`, `Delivery.Dispatch`, `Driver.Use`.

---

## 8. Billing

**Purpose:** Tell accounts and the driver what the paperwork is, without becoming the ledger at cutover.

**Features**

- Document intent on the order: sale, delivery note, dispatch note, gift, transfer (**Parity** invoice types).
- Billing status: not billed / billed, set by staff (**Parity** `InvoiceDone`).
- Customer price agreement and terms visible to commercial roles (**Parity**).
- External invoice id and link (**New**, optional field).
- No tax invoice PDF, no receipt, no dunning in the parity release.

**Screens:** Status on the order. Commercial tab on the customer. No invoice module screen in phase 1–7 of the roadmap.

**Permissions:** `Crm.ViewCommercial` to see money-related fields. `Order.Edit` to set billing status. A later `Billing.Post` permission stays unused until a ledger exists.

---

## 9. Servicing

**Purpose:** Assets and the work that is not “send more product”.

**Features**

- Assets at a location: type, serial, install date, condition (**Improve** from a single serial).
- Meters and readings, including the reading taken at delivery completion (**Parity**).
- Service schedules computed from the plan, usage, absence, and closures (**Parity** `PredictionManager`). Fallback when there is no install date: same offsets as today (20 / 20 / 30 / 30 / 365 days) stored as template defaults, not as C# constants only (**Improve**).
- Rolling window of 6 samples (**Parity**).
- Bulk recalculate, with a before/after grid, without creating orders (**Parity**).
- Service tasks: fault, description, status, email-on-status, link to an order, notes copied to that order (**Parity** repairs).
- Asset exchange task types (**Parity** swop types).
- “Should this be a service task?” prompt on flagged products (**Parity**).
- History window default 48 months (**Parity**).
- Driver can create and update tasks under the same rules (**Parity**).
- Customer sees their tasks (**Parity** portal).

**Screens:** Asset list on the customer, schedule panel, task list, task editor, status catalogue, fault catalogue, equipment types, bulk forecast tool.

**Permissions:** `Repair.Edit`. Completion of a linked order still needs `Order.Complete`.

---

## 10. Reporting

**Purpose:** Replace print-only operations with the same answers plus export.

**Features**

- Production wave, run sheet, invitation log, awaiting completion, forecast diff, channel import exceptions, application log (**Parity** content).
- Dashboard counts: due today, not packed, proof without completion, invitations outstanding, open service tasks, unmapped channel lines (**Improve** — home page is only links today).
- CSV export on those grids (**New**, required for parity of “I can take this to Excel”, which staff do today via print or copy).
- POD PDF (**Parity**).
- No revenue dashboard until prices exist.

**Permissions:** `Prep.View` and `Report.Export`.

---

## 11. Administration

**Purpose:** Run the platform.

**Features**

- Users, roles, password reset. No open self-registration in production (**Improve** — `Register.aspx` must not be anonymous).
- Permissions in § of each module.
- Workers and link to users (**Parity** people + security username).
- Message catalogue (**Parity**).
- Email test mode and diagnostics (**Parity**).
- Audit viewer (**Improve**).
- Channel settings with secrets outside git (**Parity** intent).
- Backup is an infrastructure job, not a page that runs `BACKUP DATABASE` from the web app (**Improve** — remove the pattern in `DatabaseBackup.aspx` from the product).
- Schema changes are EF migrations, not an XML-to-SQL page (**Improve**).
- Impersonation with reason and audit (**Improve** `ViewAs`).
- Holiday, territory, lookup editors that are normal screens, not one giant tab page (**Improve** `Lookups.aspx`).

**Permissions:** `Identity.Manage`, `Audit.View`, `Portal.Impersonate`, `Channel.Map`.

---

## 12. Channels

**Purpose:** WooCommerce parity, shaped so another shop can be added later.

**Features**

- Connection, encrypted credentials (**Parity**).
- Category allow-list (**Parity**).
- Product map, attribute role (qty, pack, both, notes) (**Parity**).
- Shipping method and postal code to territory and worker (**Parity**).
- Payment method stored, not charged (**Parity**).
- Idempotent order import and preview of failures (**Parity**).
- Customer link and bootstrap (**Parity**).
- Programme role push (**Parity**).
- Optional “check channel on new order” (**Parity** menu tooltip).

**Screens:** Connection, maps, import preview.

**Permissions:** `Channel.Import`, `Channel.Map`.

---

## 13. Portal

**Purpose:** My Quaffee, product-neutral (“My account”).

**Features**

- Sign in, change password (**Parity**).
- My details and change request (**Parity**).
- My orders and POD download for own delivered proofs only (**Parity**).
- Repeat order: **spec the verified behaviour**. Until verification, implement “creates a draft order for staff to confirm” rather than a silent duplicate. Mark the current page as **Verify**.
- My standing orders, read-only at cutover (**Parity** unless the business wants customer edits).
- My service tasks (**Parity**).
- Stop invitations (same signed flow as disable) (**Parity**).

**Permissions:** `Portal.Self` plus row-level customer id.

---

## 14. Cross-cutting acceptance

RainbowOF can replace QOnT when a UAT script can do all of the following on migrated data:

1. Find a contract customer, see preferences, PO flag, and two ship-to addresses.
2. See next replenishment and next maintenance from history, including a customer who is away and a closure date.
3. Run invitations, skip the right people, and log the send.
4. Create an order that merges into an existing same-day order.
5. Import a Woo order once only, with pack and qty from attributes.
6. Print or export the wave and the run.
7. Capture a short delivery with signature offline, then complete it with a meter reading and see the forecast move.
8. Warn on a bad or duplicate waybill.
9. Open a service task that emails the customer and rides an order.
10. Sign in as the customer and see only that customer’s orders, tasks, and proofs.
11. Disable invitations from a signed link and see the admin alert if a standing order remains.
12. Complete the day with billing status set and no price required.

---

## 15. Out of scope for the parity release

- General ledger, tax invoices, card capture.
- Warehouse stock and purchasing, except pickup stops that already exist.
- Coupons and discount engine.
- Hard credit-limit blocks.
- Multi-company SaaS billing (the model should not prevent a later `TenantId`, but the first deployment is one operator).
- Rebuilding the Quaffee Driver UI inside Blazor if the existing app can call a stable API. The API is in scope; a new mobile UI is optional.
