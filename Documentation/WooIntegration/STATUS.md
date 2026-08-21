# WooCommerce Integration — Status

**Last updated:** 2026-08-14  
**Version target:** 3.0.1.0  
**Branch:** `feature/woocommerce-3.0.1.0`

| Phase | Status | Notes |
|-------|--------|-------|
| 0 — Branch + docs | Done | Docs under `Documentation/WooIntegration/` |
| 1 — System Preferences + wizard + Test Connection + schema | Code complete | Section cookie, Enable/Disable UX |
| 2 — Categories + item mapping + **variation attributes** | In progress | SKU wildcards removed; Attributes tab → qty/packaging |
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
- [x] Include flags + category filter mode (saved with Save includes)
- [x] Pull products/variations → suggest maps (SKU + parent + attributes)
- [x] Save/delete exact item mappings (qty + packaging)
- [x] **Variation attribute maps** — parents (`Use for variants` + Priority) then options (`MapRole` Both/QtyOnly/PackagingOnly) → qty + packaging
- [x] Drop SKU wildcards (`WooSkuWildcardRulesTbl`)
- [x] Dry-run + push Tracker `ItemEnabled` → Woo publish/private (no stock qty)
- [x] Menu / System Tools / Preferences link to Mapping
- [ ] Manual UAT: re-run SQLCommands XML (drop wildcards / create attribute tables) → Attributes pull → map 250g → Mappings pull

## How to smoke-test Phase 2 (attributes)

1. Woo integration must be **enabled** (Phase 1 done).
2. Re-run **`App_Data/SQLCommands-WooCommerce-01.xml`** via XMLtoSQL or Preferences Ensure schema (drops wildcards; creates attribute + packaging-service tables).
3. Open **Woo Mapping**.
4. **Categories:** Pull → Include ticks + mode → **Save includes**.
5. **Attribute parents:** Pull parents → set **Priority** (lower wins) + **Use for variants** → **Save selection**.
6. **Variant attributes:** Pull options → set **Role** (Both / Qty only / Packaging only) + qty/packaging → **Save attribute maps**.
7. **Mappings:** Sync products (published + in-stock/not managed). Parents nest variants; Mode = Import variants | Parent → notes. Search / Apply / **Save selected**.
8. **Missing SKUs:** Enter New SKU → Apply → **Write SKUs to Woo** → re-sync Mappings.
9. **Enabled sync:** Dry-run first. Check `App_Data/woo.log`. Notes mappings are skipped. If status warns catalog cap, raise limit or narrow categories.
