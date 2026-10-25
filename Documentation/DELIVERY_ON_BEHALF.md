# Delivery on Behalf (planned)

**Status:** phase 1 in progress (Contact Details checkbox + Deliver-To Addresses tab; orders/sheet/driver API not wired yet).  
**Goal:** Some contacts are invoiced to one address but coffee is delivered to another person/place (or several). Route planning and the driver app must use the **deliver-to** GPS and address, not only the contact's main billing point (`CustomerLocationTbl`).

This is separate from **alternative person** (`ContactAltFirstName` / `ContactAltLastName`), which is who may receive the order at the same address. Delivery on behalf is a **named deliver-to destination** with its own address and GPS.

---

## 1. Contact Details (office)

- Next to **Contact Portal**, add checkbox **Delivery on Behalf** (stored on the contact or account row; TBD: `ContactsTbl` vs `ContactsAccInfoTbl`).
- When enabled, show a new tab **Deliver-To Addresses** (Title Case), alongside existing tabs.
- Grid / form per row:
  - **Short name** (label shown on orders and the sheet, e.g. "Helen's home", "Reception – Sandton").
  - **Deliver to person** (display name for notes and driver; optional if label is enough).
  - Address lines (reuse billing/shipping field pattern or a single multiline address).
  - **Area** (lookup), postal code, province as today.
  - **GPS** — same workflow as Contact Location: lookup from address, manual pin, status badge; stored per deliver-to row, not in `CustomerLocationTbl` (one row per contact there stays the "main" delivery point for normal orders).
  - **Sort order** within the contact (for combo default ordering).
  - **Enabled** (hide retired rows from new orders).

Optional later: link a deliver-to row to an existing **shipping address** block on the account tab (read-only copy or shared fields).

---

## 2. New order / order header

- When the selected contact has **Delivery on Behalf** and at least one enabled deliver-to address, show **Deliver to** (dropdown; required before save if multiple; default sole row or "Main address" = contact's normal delivery).
- Persist on the order: `DeliverToAddressID` (FK to new table) and/or snapshot fields on the order so history stays correct if the contact edits addresses later (TBD: snapshot `DeliverToLabel`, `DeliverToPerson`, address text on `OrdersTbl` or child table).
- Recurring orders: same choice on the recurring header when applicable.

---

## 3. Delivery sheet and office UI

- For orders with a deliver-to row, append to **notes** (or a dedicated sheet column) in **bold**: deliver-to person and short name, e.g. **Deliver to: Helen Kneale (Helen's home)**. Do not rely on drivers reading only the billing company line.
- Route order / area grouping: use deliver-to **AreaID** and **GPS** for `RoutePlanner`, road matrix, and learned order — same rules as today but keyed off the stop's effective location (see driver API below).
- `CustomerLocationManager.LearnFromDelivery` should still update the **contact's main** point unless we explicitly decide deliver-to rows get their own learning table (recommended: separate `ContactDeliverToLocation` columns on the deliver-to row, learn only when delivering to that row).

---

## 4. Driver app and mobile API

- `GET delivery/sheet`: for each stop with `deliverToAddressId` (or embedded snapshot), set:
  - `address`, `postalCode`, `area`, `gps` from the deliver-to row.
  - Optional `deliverToLabel` / `deliverToPerson` for the stop screen (in addition to `company` / `contactName`).
- Navigate / GPS check / expected times: use deliver-to coordinates; `noGpsLearning` rules unchanged for collections and not-at-client.
- Proof of delivery PDF: show deliver-to person/label when present.

---

## 5. Database (sketch)

New table e.g. `ContactDeliverToAddressTbl`:

| Column | Notes |
|--------|--------|
| DeliverToAddressID | PK |
| ContactID | FK |
| Label | nvarchar(50) |
| DeliverToPerson | nvarchar(100) optional |
| Address1..5 or AddressText | match existing address style |
| AreaID, PostalCode, StateOrProvince | |
| Latitude, Longitude, Geohash, LocationStatus, Source, … | mirror `CustomerLocation` subset or FK to a small location table |
| SortOrder, Enabled | |

`OrdersTbl`: `DeliverToAddressID` int NULL.

Ship in `App_Data/SQLCommands-DriverGps-08.xml` (idempotent), GoLive copy.

---

## 6. Phased delivery

1. Schema + repository + Contact Details tab (CRUD, GPS per row).
2. Order Detail deliver-to dropdown + sheet notes formatting.
3. Mobile sheet + route planner effective location.
4. Driver UI copy ("Deliver to: …") and tests on dev orders ≥ 999999990.

---

## 7. Testing

- Contact with two deliver-to pins in different areas: same-day orders should sort by road time to each pin, not the billing GPS.
- Order without deliver-to: unchanged behaviour.
- Woo / portal imports: deliver-to not required until contact flag is set.
