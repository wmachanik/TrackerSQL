# TrackerSQL 3.0.3.11 — release notes

**Release date:** 2026-11-01  
**Quaffee Driver (mobile app):** 0.5.8 (unchanged this release; no new APK required)  
**Next development version:** 3.0.3.12 (`Web.config` / `AssemblyInfo`)

Full history: [CHANGELOG.md](../CHANGELOG.md). Deploy checklist: [DEPLOYMENT_AND_CONFIG.md](DEPLOYMENT_AND_CONFIG.md).

---

## Summary

This release completes the **ZZName / alternative-address** rework (Ad hoc display, Sundry stored code), **Lookups → Alt Address Types**, schema packs **12–15**, and several **stability fixes** (delivery sheet SQL, Contact Details ViewState, Lookups → Areas selection, Order Detail Recipient visibility).

---

## Features

### Alternative addresses and ZZName (Ad hoc / Sundry)

- **Order Detail** — **Recipient** is **Standard** (notes only) or an alternative (**New alternative…**, pencil edit, GPS on the alt via Contact Location popup). Shown only when the contact uses alternatives (**ZZName contact 9** or **This contact has alternative delivery addresses** on Contact Details).
- **Ad hoc** is the UI label; **Sundry** is the stored `AddressKind` (accounting-friendly). Schema pack **`SQLCommands-DriverGps-15.xml`** renames legacy **Casual** → **Sundry** / **Ad hoc** and sets **`IsSundryDefault`** on the lookup row.
- **Lookups → Alt Address Types** — type code, name, **Counts toward tracking**, sort, on/off (`SQLCommands-DriverGps-14.xml` / `-15.xml`). Gift, on their behalf, and Ad hoc skip usage / Next Coffee on Order Done by default.
- **Woo / ZZName (not collect)** — matches or creates a Sundry alt and sets **`DeliverToAddressID`** (not walk-in collect-at-depot).
- **Create contact** from a ZZName alt (Order Detail prompt or Contact Details **AddContact**). Done Ad hoc orders move to the new contact; **tracked item lines** (coffee, filter, etc.) import into usage and **predictions recalc**.
- **System Tools** — purge unused ZZName alts after 12 months.
- **Walk-in collect-at-depot** remains separate (order **`WalkInAreaID`** / lat/lng, pack 12); not the same as ZZName guest alts.

### Other (carried from 3.0.3.10 work)

- Schema packs run at start-up only when the target table/column is missing.
- Woo import **`orders.log`**; checkup holiday HTML; default driver **start time** (pack 11); POD **Change** outcome; mobile-friendly Lookups areas; delivery sheet / driver tracking **area codes**; filter prediction cadence; System Tools **Recalc** includes filter/clean/descale/service.

---

## Fixes (3.0.3.11)

| Area | Issue | Fix |
|------|--------|-----|
| **Delivery Sheet** | `Invalid viewstate` / SQL error after ZZName courier orders: `ORDER BY` used `ar.AreaName` / `c.CompanyName` with `SELECT DISTINCT` | [`DeliverySheetRepository.cs`](../Repositories/DeliverySheetRepository.cs) — `ORDER BY` uses selected aliases **`AreaName`**, **`ContactName`**, **`ToBeDeliveredByID`** |
| **Lookups → Areas** | After paging, wrong row looked selected; delivery days panel still showed previous area | [`Lookups.aspx.cs`](../Pages/Lookups.aspx.cs) — persist selected **AreaID** in ViewState; title **Delivery Days for {name} Area** |
| **Contact Details (contact 9)** | **Invalid viewstate** on postback (large POD signatures + alt grid) | [`ContactDetails.aspx.cs`](../Pages/ContactDetails.aspx.cs) — **SessionPageStatePersister**; signature images **`EnableViewState="false"`**; [`Global.asax.cs`](../Global.asax.cs) — stale ViewState reloads page instead of error banner |
| **Order Detail** | **Recipient** row always visible | [`OrderDetail.aspx.cs`](../Pages/OrderDetail.aspx.cs) — show only when [`ZzNameDeliverTo.ShowChoice`](../Managers/ZzNameDeliverTo.cs) (alt enabled or ZZName); removed “show because order already has deliver-to” |

---

## Schema packs (recycle site after deploy)

| Pack | Purpose |
|------|---------|
| `SQLCommands-DriverGps-12.xml` | Walk-in area/GPS on order |
| `SQLCommands-DriverGps-13.xml` | Alt address Created/LastUsed |
| `SQLCommands-DriverGps-14.xml` | **AltAddressKindsTbl** |
| `SQLCommands-DriverGps-15.xml` | **Sundry / Ad hoc**, **IsSundryDefault**, migrate **Casual** |

Files in `App_Data/` and `App_Data/GoLive/`. Check `App_Data/startup.log` for “Order flag schema ready”.

---

## Go-live (short)

1. Upload **`bin/`**, changed **Pages**, **Managers**, **Repositories**, **Models**, **`Styles/Site.css`**, **`Site.Master`**, **`images/imgButtons/AddContact.png`**, schema XML above, **`App_Data/GoLive/`** copies as needed.
2. On live **`Web.config`**, set **`Version`** to **`3.0.3.11`** only (merge secrets from your template; do not commit live config).
3. **Recycle** the site (or touch `Web.config`) so packs **12–15** run.
4. Verify **`/api/v1/ping`** and open **Order Detail** on a ZZName/courier order on the **Delivery Sheet**.
5. **Quaffee Driver:** still **0.5.8** — no APK change unless you rebuild the driver separately.

---

## Key files touched (reference)

- `Managers/ZzNameDeliverTo.cs`, `WalkInOrderSite.cs`, `AlternativeAddressKind.cs`
- `Pages/OrderDetail.aspx` (+ `.cs`, `.designer`)
- `Pages/ContactDetails.aspx` (+ `.cs`)
- `Pages/Lookups.aspx` (+ `.cs`)
- `Repositories/DeliverySheetRepository.cs`, `AltAddressKindsRepository.cs`
- `Global.asax.cs`
- `App_Data/SQLCommands-DriverGps-12.xml` … `-15.xml`
