# WooCommerce Integration — Status

**Last updated:** 2026-08-13  
**Version target:** 3.0.1.0  
**Branch:** `feature/woocommerce-3.0.1.0`

| Phase | Status | Notes |
|-------|--------|-------|
| 0 — Branch + docs | Done | Docs under `Documentation/WooIntegration/` |
| 1 — System Preferences + wizard + Test Connection + schema | Code complete | Section cookie, Enable/Disable UX |
| 2 — Categories + item mapping + SKU wildcards | Code complete | `Tools/WooCommerceMapping.aspx` |
| 3 — Staged order import | Pending | |
| 4 — Order Detail Woo panel | Pending | |
| 5 — Status / dispatch / tracking | Pending | |
| 6 — Contact import | Pending | |
| 7 — Hardening / release | Pending | |

## Phase 1 checklist

- [x] Branch from live baseline 3.0.0.5
- [x] Documentation folder + INDEX link
- [x] Replace System Data → System Preferences (sidebar)
- [x] Enable Woo + setup wizard (Woo-site prep copy + Test Connection)
- [x] `SQLCommands-WooCommerce-01.xml` + wizard Ensure schema
- [x] Encrypted Woo settings in companion table
- [x] Messages.resx keys for UI
- [x] Admin-only access
- [ ] Manual UAT: create tables, save keys, Test Connection, Finish

## Phase 2 checklist

- [x] Pull Woo categories → `WooCategoryFilterTbl`
- [x] Include / Exclude / ZZName flags + category filter mode
- [x] Pull products/variations → suggest maps (SKU + wildcards)
- [x] Save/delete exact item mappings
- [x] SKU wildcard rules (prefix / suffix / qty factor)
- [x] Dry-run + push Tracker `ItemEnabled` → Woo publish/private (no stock qty)
- [x] Menu / System Tools / Preferences link to Mapping
- [ ] Manual UAT on live store (categories → maps → wildcards → dry-run)

## How to smoke-test Phase 2

1. Woo integration must be **enabled** (Phase 1 done).
2. Open **Woo Mapping** (menu, System Tools, or Preferences → Open WooCommerce Mapping).
3. **Categories:** Pull from Woo → set Include/Exclude/ZZName → save mode (All / Include list / Exclude list).
4. **Mappings:** Pull products → review suggested Tracker items → Save rows.
5. **Wildcards:** Add prefix/suffix/qty rules (e.g. `9QRCcoDec` + `250g` → 0.25).
6. **Enabled sync:** Dry-run first, then push if OK. Check `App_Data/woo.log`.

## How to smoke-test Phase 1

1. Log in as Administrators / Admin.
2. Open **System → System Tools → System Preferences** (or menu **System Preferences**).
3. Confirm **General** still edits SysData.
4. Open **WooCommerce** → **Enable / Start setup wizard**.
5. Follow prep steps → **Create / verify tables** (or run `App_Data/SQLCommands-WooCommerce-01.xml` via XMLtoSQL).
6. Enter store URL + REST keys → Save → **Test connection** → Finish.
7. Confirm `WooCommerceCryptoKey` is set in `Web.config`.
