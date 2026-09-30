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
SQLCommands-CourierServices-01.xml ← courier track URLs + deep-link params (Fastway ?l=, Pargo ?code=, Courier Guy ?ref=)

GPS & DRIVERS (customer locations, Azure Maps, learning from deliveries)
Both packs run automatically at site start-up (OrderFlagsSchemaInstaller); the copies here are for XMLtoSQL
if start-up could not run them. Run 01 before 02.
SQLCommands-DriverGps-01.xml   ← PeopleTbl.DeliveryType, CustomerLocationTbl, GeoApiUsageTbl, GpsSettingsTbl, Collected option
SQLCommands-DriverGps-02.xml   ← GpsSettingsTbl.AzureMapsKey; CustomerLocationTbl.ExpiresDate / DriverFixes / LastDriverFixAt;
                                 MobileDeliveryProofTbl.AccuracyM
Live steps:
1. Deploy; open the site once so the packs run. Check Tools → XMLtoSQL / the system log if a pack failed.
2. Azure portal: create an "Azure Maps Account" (pricing tier Gen2; region any), open Authentication, copy the Primary Key.
   Put a spending alert on the subscription (low volume: about 50 lookups a day stays inside the 5,000 free a month).
3. System Preferences → GPS & Drivers: Lookup service = Azure Maps, paste the key, Save (the key is stored in the
   database and never shown again; it is NOT in Web.config). Look up depot. Keep the HeiGIT key as a fallback if wanted.
4. Tools → Customer Locations: run a small batch, then "Look up rough ones again" for the Needs-checking list.
5. Driver app: install the new APK (0.4.0, asks for location permission on the first delivery). The web copy at
   /driver is already rebuilt in this release. Drivers must allow location, or proofs simply arrive without GPS.
6. Tell drivers in writing that the phone's position is saved with each proof of delivery (POPIA).
Rules to know:
- Azure Maps points are kept at most 180 days (Microsoft terms outside Azure). They are looked up again from 30 days
  before expiry (on the next order / contact save / batch) and removed once past expiry if not refreshed.
- A Delivered proof with a phone fix of ±75 m or better replaces a looked-up point with the driver's position
  (no expiry). A second visit within 60 m marks it checked. Hand-placed pins are never moved; two visits in a row more
  than 150 m away flag them "Needs checking". The sundry contact is skipped. Every change is in the Customers log.

================================================================================
*/
