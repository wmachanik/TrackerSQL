# WooCommerce Integration (TrackerSQL 3.0.1.0)

**Status:** In progress (Phase 2 code complete — UAT)  
**Branch:** `feature/woocommerce-3.0.1.0`  
**Live baseline before this work:** Release 3.0.0.5 (`26f7411`)

## Documents

| File | Purpose |
|------|---------|
| [PLAN_3.0.1.0.md](PLAN_3.0.1.0.md) | Full design, phases, test gates |
| [STATUS.md](STATUS.md) | Current phase progress |

## Quick rules

- Companion tables only — do not add Woo columns to `OrdersTbl` / `ItemsTbl` / `ContactsTbl`.
- Schema via `App_Data/SQLCommands-WooCommerce-01.xml` (XMLtoSQL) + setup wizard Ensure.
- User-facing strings in `Resources/Messages.resx` (`MessageKeys.WooCommerce` / `MessageKeys.SystemPreferences`).
- Admin roles only for Preferences / wizard / sync tools.
- Secrets encrypted in DB; app key `WooCommerceCryptoKey` in config (never commit live keys).
- **One** store-level Woo connection; track **who** changed things via Tracker `UpdatedBy` / `woo.log` and Woo order notes (`Updated from Tracker by {user}`) — not per-user API keys.

## Phases (summary)

0. Docs + branch  
1. System Preferences + Enable wizard + Test Connection + schema  
2. Categories + item mapping + enabled sync  
3. Staged order import  
4. Order Detail Woo panel  
5. Status / dispatch / tracking prefs  
6. Staged contact import  
7. Hardening / release  

See [PLAN_3.0.1.0.md](PLAN_3.0.1.0.md) for detail.
