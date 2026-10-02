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
                                     + WaybillFormats column with defaults (Pargo PAR*, Fastway @?#{10}; #@#{10}, Courier Guy @+).
                                     Tracker also adds it the first time couriers are used; safe to run again.

3.0.3.1 RELEASE (order flags + GPS & drivers), in order
a. Back up the live database and the server's Web.config.
b. Publish Release; upload bin/, pages, Handlers/, Scripts/leaflet/, Styles/, driver/, Downloads/ (QuaffeeDriver.apk + .json)
   and the new App_Data/SQLCommands-*.xml (OrderFlags-01/02, AreaDeliveryOrder-01, CourierServices-01, MobileApi-01,
   DriverGps-01/02/03). Upload web.live.config as Web.config. Never overwrite the App_Data log files.
c. Open https://tracker.quaffee.co.za/api/v1/ping (version 3.0.3.1) and sign in; the packs run at start-up.
d. XMLtoSQL: run SQLCommands-OrderFlags-02-ResetContacts.xml once (CHANGELOG 3.0.3.1 go-live notes).
e. Check Lookups -> Delivery Days -> All areas, then follow the GPS & DRIVERS steps below.

GPS & DRIVERS (customer locations, Azure Maps, learning from deliveries)
The packs run automatically at site start-up (OrderFlagsSchemaInstaller); the copies here are for XMLtoSQL
if start-up could not run them. Run 01, then 02, then 03.
SQLCommands-DriverGps-01.xml   ← PeopleTbl.DeliveryType, CustomerLocationTbl, GeoApiUsageTbl, GpsSettingsTbl, Collected option
SQLCommands-DriverGps-02.xml   ← GpsSettingsTbl.AzureMapsKey / MapProvider; CustomerLocationTbl.ExpiresDate / DriverFixes /
                                 LastDriverFixAt; MobileDeliveryProofTbl.AccuracyM; DeliveryVisitTbl (the order drivers
                                 really deliver in; filled once from existing proofs); DeliveryRouteTbl (the order a
                                 driver set in the app for a day); MobileApiSettingsTbl.LandlinePrefixes
SQLCommands-DriverGps-03.xml   ← DriverLocationPingTbl (driver tracking positions)
SQLCommands-DriverGps-04.xml   ← MobileDeliveryOptionsTbl.NotAtClient / MobileDeliveryProofTbl.NotAtClient; options
                                 "Collected by the client" and "Dropped off by the supplier" (3.0.3.2)
SQLCommands-DriverGps-05.xml   ← MobileDeliveryProofTbl.TermsAccepted / TermsText / HandedOver / PodSentAt / PodSentTo;
                                 MobileApiSettingsTbl.DeliveryTerms / TermsUrl (terms tick box, POD PDF; 3.0.3.2)
The Collected delivery outcome is now created with "needs a name" off (01 and MobileApi-01). Live already has the
row: untick "Needs name" for Collected on Lookups → Delivery Outcomes if the drivers should not be asked for one.
SQLCommands-MobileApi-01.xml also adds MobileApiSettingsTbl.LandlinePrefixes when it is missing (either pack may run first).
New files to upload: Handlers/MapTile.ashx (Azure Maps tiles for the GPS popup), Tools/DeliveryOrder.aspx
(the order deliveries were made) and Tools/Deliveries.aspx (the new Deliveries & Areas page; top menu "Deliveries");
the bin DLL holds their code. Site.Master and Default.aspx changed (new menu and home card).
Also new: Tools/DriverTracking.aspx and Handlers/DriverTrackingData.ashx (driver tracking map), Tools/DepotLocation.aspx
(depot map popup); Styles/Site.css and Tools/SystemPreferences.aspx changed.
Live steps:
1. Deploy; open the site once so the packs run. Check Tools → XMLtoSQL / the system log if a pack failed.
2. Azure portal: create an "Azure Maps Account" (pricing tier Gen2), open Authentication, copy the Primary Key.
   The account's region / "data location" list has no South Africa: that is only where Azure keeps the account,
   not which maps it has. Pick any (e.g. West Europe); South African addresses and maps work the same.
   Put a spending alert on the subscription (low volume: about 50 lookups a day stays inside the 5,000 free a month).
3. System Preferences → GPS & Drivers: Lookup service = Azure Maps, paste the key, Save (the key is stored in the
   database and never shown again; it is NOT in Web.config). Map = Azure Maps for the road / satellite popup map
   (optional; up to 15,000 tiles a day). Depot: press Set on map, drag the pin onto the depot (Satellite helps),
   then Save depot point, so every route starts at the right place.
4. Tools → Customer Locations: run a small batch, then "Look up rough ones again" for the Needs-checking list.
   Route order needs points: stops without one (and no located contact in their area) go last on the sheet.
5. Driver app: install the new APK once from Tools -> Driver App (0.5.4; upload Downloads/QuaffeeDriver.apk AND
   Downloads/QuaffeeDriver.json; from 0.5.1 on (so after this install) the app shows Update itself when a newer APK is uploaded; asks for location permission on the first delivery or when tracking
   starts). The web copy at /driver is already rebuilt in this release. Drivers must allow location, or proofs
   simply arrive without GPS. With tracking on, Android shows a "Sharing your position" notification during work
   hours; drivers should allow notifications and not "optimise" the app's battery use, or positions stop.
6. Tell drivers in writing that the phone's position is saved with each proof of delivery and, when driver tracking
   is on, every few minutes during work hours while they are signed in (POPIA). The app's More screen says so too.
7. Driver tracking: System Preferences → GPS & Drivers → tick "Drivers' phones send their position", check the
   interval, days and hours. Tick "Show driver tracking" on Administration → User Information for non-admin users
   who may see Deliveries → Driver tracking.
8. 3.0.3.2: Deliveries → Delivery order shows each driver's stops for today, done / next / expected times, and
   refreshes every minute (no setting). The Delivery Sheet shows no route numbers or times any more; dragging stops
   into a new order works for everyone (no setting).
   SQLCommands-DriverGps-04.xml (upload it to App_Data; runs at start-up) adds the "Not at client" tick to Lookups →
   Delivery Outcomes and the options "Collected by the client" and "Dropped off by the supplier". Proofs captured with
   them never change the client's GPS point or the driver's delivery order. Check the list on Lookups → Delivery
   Outcomes after deploying (rename or untick as you like; the tick is what counts, not the wording).
9. 3.0.3.2: upload SQLCommands-DriverGps-05.xml and Handlers/ProofOfDelivery.ashx. Tools -> Driver App: check the
   terms wording and link the person signing must tick and their name (app 0.5.4), and tick "Email the client the proof of
   delivery" to email the POD PDF automatically. Contact Details -> Proof of Delivery: PDF / Email per delivery.
   Also upload Portal/MyOrders.aspx and Portal/MyOrder.aspx: clients get View / Download for their own POD PDFs.
Rules to know:
- Azure Maps points are kept at most 180 days (Microsoft terms outside Azure). They are looked up again from 30 days
  before expiry (on the next order / contact save / batch) and removed once past expiry if not refreshed.
- A Delivered proof with a phone fix of ±75 m or better replaces a looked-up point with the driver's position
  (no expiry). A second visit within 60 m marks it checked. Hand-placed pins are never moved; two visits in a row more
  than 150 m away flag them "Needs checking". The sundry contact is skipped. Every change is in the Customers log.
- Contacts whose own Delivery By is a Courier (Pargo, Cour) or Collect person are never looked up or learnt.
- Route order: each driver's stops go from the depot to the nearest next stop (geohash points), then crossings are
  removed. Areas only matter for stops with no point (they use the area's busiest geohash cell, or go last).
- The driver's own order comes first: each proof sent from the app records which stop it was that day, and the next
  sheets move the contact towards where the driver usually delivers it (one visit counts half, four 80%). See
  Tools → Delivery order. Nothing to set up; it starts learning with the first deliveries.
- A driver can set the day's order in the app (Change order, ▲ ▼, Save order) before leaving. That order then comes
  first, also on the office delivery sheet, until the driver taps Suggested order. Logged in the Delivery log.
- Tools → Driver App → "Landline numbers start with" (default 021): the app offers no WhatsApp for those numbers.
  Add others (e.g. 011, 087) there; +27 21… numbers count as 021.

================================================================================
*/
