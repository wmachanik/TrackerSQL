/*
================================================================================
TrackerSQL Woo go-live pack (generated from local OtterDb)
Folder: App_Data/GoLive
================================================================================

WHAT THIS IS
- Schema XMLs (create/alter tables) — same as Tools → Ensure schema / XMLtoSQL
- Config DATA scripts — your mappings/settings from this machine (not live yet)

DO NOT IMPORT (runtime / secrets)
- Consumer key/secret (re-enter on live)
- WooCatalogCacheTbl (re-pull products on live)
- WooOrderInfoTbl / OrderWaybillTbl / import batches (live will create these)

ASSUMPTIONS
- Live Tracker already has the same People / Areas / Items IDs used in mappings
  (usual for the same Quaffee Tracker database). If ItemIDs differ, mappings break.
- Take a live DB backup before running DELETE+INSERT scripts.

ORDER OF OPERATIONS
1. Deploy the website binaries + App_Data SQLCommands-WooCommerce-*.xml (and Postal if needed)
2. Set live Web.config:
   - connectionStrings → live OtterDb
   - WooCommerceCryptoKey → a strong live-only key (do not reuse the dev key if secrets must differ)
   - EmailTestMode=false when ready for real emails
3. On live: System Preferences / Postal Area Setup → Ensure schema
   (or XMLtoSQL run WooCommerce 01, then 02, then 03)
4. Mapping DATA — shared hosting (no SSMS): FTP SQLCommands-GoLive-00.xml … 09.xml into App_Data,
   then Tools → XMLtoSQL → Refresh → Execute each in order 00 → 09.
   (Raw .sql files are for SSMS only; XMLtoSQL cannot run .sql.)
5. On live: System Preferences → Woo → enter store URL if needed, Save API key+secret, Test connection
6. On live: Woo Mapping → confirm Categories / Mappings / Areas / Shipping / Payment
7. Pull catalog once; smoke-test one Woo order import + a Pargo/Courier Order Done waybill

SCRIPT LIST (SSMS .sql  /  XMLtoSQL .xml)
00-SystemPreferencesHdrTbl.sql  / SQLCommands-GoLive-00.xml   enable Woo flags
01-WooCommerceSettingsTbl.sql  / SQLCommands-GoLive-01.xml   behaviour (dispatch 5,7, etc.) — no secrets
02-WooCategoryFilterTbl.sql     / SQLCommands-GoLive-02.xml   category include/import defaults
03-WooItemMappingsTbl.sql       / SQLCommands-GoLive-03.xml   product/SKU maps (264 rows)
04-WooAttributeParentTbl.sql    / SQLCommands-GoLive-04.xml
05-WooAttributeMapTbl.sql       / SQLCommands-GoLive-05.xml
06-WooPackagingServiceTypeTbl.sql / SQLCommands-GoLive-06.xml (empty)
07-WooAreaDeliveryDefaultTbl.sql / SQLCommands-GoLive-07.xml  postcode ranges + default person
08-WooShippingMethodMapTbl.sql  / SQLCommands-GoLive-08.xml   Woo method → Delivered by
09-WooPaymentMethodMapTbl.sql   / SQLCommands-GoLive-09.xml   payment abbrevs

COPY OF SCHEMA PACKS IN THIS FOLDER
SQLCommands-WooCommerce-01.xml
SQLCommands-WooCommerce-02.xml
SQLCommands-WooCommerce-03.xml   ← includes OrdersTbl.Notes → NVARCHAR(MAX) + OrderWaybillTbl
SQLCommands-Postal-01.xml        ← only if live lacks SaPostalCodeTbl

================================================================================
*/
