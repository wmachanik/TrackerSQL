# GPS delivery areas, customer locations, driver tracking and ETAs — plan

**Status:** planned, not built (agreed 2026-09-30). Replaces the earlier "delivery map" idea, which was dropped before any code was written.
**Principles:** geocode once and store; never geocode per order; delivery logic only for people whose delivery type is Deliver; location data kept out of the contacts table; the existing Areas table stays in charge of prep and delivery dates; no paid routing APIs in version 1.

---

## How Tracker works today (the starting point)

- **Customers** are in `ContactsTbl`. **People** (`PeopleTbl`) are staff: drivers, couriers and others.
- A contact's **default delivery person** is `ContactsTbl.PreferredAgentID` (the "Delivery By" dropdown on Contact Details), set by hand. Each order copies it into `OrdersTbl.ToBeDeliveredByID`, where it can be changed per order (a customer can ask for collection or courier on one order).
- **Areas** (`AreasTbl` plus `AreaPrepDaysTbl`) set the prep and delivery ("prep by") dates, the Delivery Days order, promise rules, WooCommerce postcode mapping and the area pill. They stay.
- `PeopleTbl.IsDispatched` marks courier people (Cour, Pargo); Order Done uses it for waybills.
- The contact's address in the main table (`BillingAddress`, postcode, province) is the **shipping address**, built from WooCommerce addresses by the rules in settings.
- The driver app (`C:\SRC\ASP.net\TrackerDriver`, Vue + Capacitor) is Android only; iPhone drivers use the web copy at `/driver`.

---

## 1. Delivery type on People

- New column `PeopleTbl.DeliveryType`: `DELIVER`, `COURIER`, `COLLECT` or `NONE` (people who never deliver).
- Initial values:
  - SQ, WM, MX → `DELIVER`
  - Cour, Pargo (and anyone with `IsDispatched = 1`) → `COURIER`
  - the Collect entry (customers collecting from the shop) → `COLLECT`
  - everyone else → `NONE`
- Edited on the People lookup. `IsDispatched` stays and is kept equal to "DeliveryType = COURIER", so Order Done does not change.
- Courier, Pargo and Collect are not real people but are entries in `PeopleTbl` so they can be chosen as "Delivery By". The delivery type is what makes them behave differently.
- A contact or order counts as a "delivery" when its delivery person's type is `DELIVER`. This is why GPS follows the delivery person, not the area. Durban, for example, is served by courier today, so its contacts get no location. If we ever deliver there ourselves (for example from a branch), choosing a `DELIVER` person for those contacts makes them get a location with no other change.

## 2. Customer locations (`CustomerLocationTbl`)

One row per contact that gets our own deliveries; separate from `ContactsTbl`.

| Column | Notes |
|---|---|
| `ContactID` | primary key, links to `ContactsTbl` |
| `Latitude`, `Longitude` | `DECIMAL(9,6)` |
| `Geohash` | 9 characters (about 5 m), used for grouping and sorting |
| `MapAreaID` | area whose map shape contains the point (links to `AreasTbl`); null when outside all shapes |
| `GeocodedAddress` | the exact address text that was looked up |
| `LocationStatus` | `VERIFIED` (a person confirmed or moved the pin), `APPROXIMATE` (automatic lookup), `REVIEW_REQUIRED` (no good match, outside all shapes, or map area differs from the contact's area) |
| `CreatedDate`, `UpdatedDate` | |

### When a location is created or updated
- **On contact save:** a contact is saved with a delivery person of type `DELIVER` and has no location yet.
- **On address change:** the address changes and no longer matches `GeocodedAddress`. The pin is looked up again; if it was `VERIFIED`, it becomes `REVIEW_REQUIRED`.
- **On order capture:** an order is captured for a `DELIVER` person and the contact has no location. The lookup happens once, not on every order.
- Changing to a collect or courier person leaves the stored location alone. It is simply ignored while the contact is not delivered by us.

### Contact Details display
- In the Packaging column: the label **"GPS"** under "Packaging", and the value as text under the packaging dropdown, for example `APPROXIMATE · Constantia & Surrounds`.
- The value is not editable in place. A **Check / change** button opens a popup with:
  - a small map and a draggable pin;
  - a "Look up from address again" button;
  - a note when the map area differs from the contact's area, for example "Map says Bellville; contact is Northern Suburbs". Changing the contact's area stays a deliberate choice, because it changes prep dates.
- Saving from the popup sets `VERIFIED`.

### Address lookup (geocoding)
- **Provider:** OpenRouteService / HeiGIT, via `https://api.heigit.org/pelias/v1/search`. The free Standard key allows 3,000 lookups a day and 100 a minute.
- The lookup is behind a swappable interface, so another provider can be used later.
- Only the address text, postcode and "South Africa" are sent — never names, phone numbers or emails.
- The daily cap is counted in `GeoApiUsageTbl` so it survives restarts. HTTP 429 means wait and retry once; HTTP 403 means stop for the day.
- If the lookup fails, the postcode centre from `SaPostalCodeTbl` is used, with status `REVIEW_REQUIRED`.

### One-time migration
A batch page runs the following for every contact whose default delivery person is `DELIVER` and who has no location:
1. Read the address.
2. Look it up.
3. Work out the geohash.
4. Find the map area.
5. Save the location.

It respects the daily cap, so it can take more than one day. It lists every `REVIEW_REQUIRED` contact for checking.

## 3. Map areas linked to Areas

- New `AreaMapShapeTbl`:
  - `ShapeID`
  - `AreaID` (links to `AreasTbl`)
  - `PolygonJson` (GeoJSON)
  - `Active`
- An area can have **several shapes**, for example two separate pockets served on the same run.
- `RouteSequence` is the area's existing delivery order from Lookups → Delivery Days (`AreaPrepDaysTbl.DeliveryOrder`), so there is only one order to maintain.
- Areas that are really delivery methods (courier, collect, regional) simply have no shapes.
- **Map editor** (admins): shows all shapes coloured by area, plus customer pins. You can draw, edit and delete shapes (Leaflet with Leaflet.draw) and assign each shape to an area.
- After shapes change, "Recheck customers" re-runs the point-in-shape test for every stored location. Mismatches are flagged `REVIEW_REQUIRED`; contacts are never moved automatically.

## 4. Stop order and ETAs

### Stop order (per driver per day) — built 2026-10-01
Areas no longer decide the order (the owner's decision: "it should be using the geohash").
1. SQL sorts each driver's stops by geohash: the contact's point, or for a contact without one its area's busiest 5-character cell (`CustomerLocationRepository.RouteJoinSql` / `RouteHashSql`).
2. `Classes/Geo/RoutePlanner` then walks from the **depot** to the **nearest next stop** each time and removes crossings (2-opt on the open route). Ties keep the geohash order. Used by `DeliverySheetRepository` (per driver, per day, open and delivered separately) and `MobileDeliveryManager` (per driver; delivered stops keep their place, `Seq` = 1, 2, 3…).
3. Stops with no point and no located contact in their area go last, in the old order (off-day, area delivery order, area, company).
4. **The driver's own order comes first** (owner's idea, 2026-10-01: drivers don't trust the suggested order and deliver in their own). Each proof from the app records a visit in `DeliveryVisitTbl` (day, driver, contact, first/last time, Position = nth stop that day by time, Stops = the larger of the day's planned stops and visits). `DeliveryVisitRepository.LearnedJoinSql` averages a contact's place (0 = first .. 1 = last) over its last 10 visits before the sheet's day (the route's driver first, days with 3+ stops only), and `RoutePlanner.ApplyLearned` blends it with the GPS place: `(visits × learned + gps) / (visits + 1)`. Stable sort, so unvisited stops keep the GPS / area order. Same in both sheets. **Tools → Delivery order** lists the visits; the pack backfills visits once from existing proofs.
5. **The driver's plan for the day beats everything** (owner, 2026-10-01). In the app (*Mine*) **Change order** → ▲ ▼ → **Save order** posts `POST delivery/route {date, orderIds}`; Tracker maps the orders to contacts and replaces that day's rows in `DeliveryRouteTbl` (RouteDate, PersonID, ContactID, Seq). `DeliveryRouteRepository.PlanJoinSql` gives each stop its PlanSeq / PlanStops and `RoutePlanner.ApplyPlan` runs after `ApplyLearned` (planned place = (seq-1)/(stops-1); unplanned stops keep their place among them). An empty list (**Suggested order**) clears the day. Offline, the app keeps the order in localStorage (`driver.route`) and sends it from `syncOutbox`. The visits recorded with each proof still show how the day really went (*Driver's plan* column on Tools → Delivery order).

Tested 2026-10-01 with simulated syncs (since removed): 30 Sep delivered in reverse GPS order moved Ana and Greg ahead of Brent and Charne on 1 Oct, exactly as calculated; a late-arriving sync was renumbered by time; a resend (same clientRef) and the sundry contact were not counted; a 2-stop day and same-day visits were ignored; another driver's visit counted; the backfill rebuilt identical rows.

A plain geohash sort was tried first and zig-zagged (Hout Bay → Constantia → Sea Point → CBD → Plumstead → Pinelands → UCT on 30 Sep). The nearest-next walk is free, needs no routing API and zig-zags much less.
Starting from the driver's latest position instead of the depot is left for the tracking step. Distances are straight-line (haversine) behind a replaceable distance service, so OSRM, GraphHopper or OpenRouteService can be plugged in later.

### ETA
- Inputs: the driver's position (latest ping, else the depot), the current time, and the remaining stops in order.
- Travel time = distance ÷ average speed (default 30 km/h). Arrival = departure from the previous stop + travel time. Departure = arrival + service time (default 5 minutes).
- Recalculated when a new ping arrives, a delivery is completed, or the sequence changes. In practice it is worked out whenever the sheet or tracking page loads or refreshes, from the latest data.

### Delivery sheet
- New columns: **Seq**, **Area**, **ETA**, **Status** (Pending / Delivered / Failed), in addition to the existing columns.
- Shown only for users with the user preference **"Show route and ETA on Delivery Sheet"** (default off; intended for drivers and an admin). Screen only; the printed sheet is unchanged.

## 5. Driver tracking

### Driver app (Android; iPhone uses the web copy)
- Sends a ping on app open, every 10 minutes while logged in and active, and on delivery complete:
  - driver (person ID)
  - latitude and longitude
  - time
  - source: `APP_OPEN`, `PERIODIC` or `DELIVERY_COMPLETE`
- Android uses **`@capacitor-community/background-geolocation`** (decided 2026-09-30: free, MIT licence, peer `@capacitor/core >= 3`, so it works with Capacitor 8). It keeps sending positions with the app in the background or the screen off. Android shows the standard "tracking active" notification while it runs, and the driver must allow location "all the time". The app starts it only when tracking is on and inside the allowed days and hours, and stops it outside them or on "Finished". (Not Transistorsoft: paid licence for Android release builds.)
- The iPhone web copy can only ping while the page is open.
- Pings are only sent when tracking is enabled in settings and inside the allowed days and hours.
- New API endpoint `POST /api/v1/location` with the same token login as the app. The server also rejects pings outside the allowed days and hours.

### `DriverLocationPingTbl`
- Columns: `PingID`, `PersonID`, `Latitude`, `Longitude`, `PingTime`, `Source`.
- Old pings are deleted after the retention period set in settings.

### Deliveries → Driver tracking page (built 2026-10-01, see "Step 3 status" below)
- **Map of active drivers.** Each marker shows the latest ping, colour-coded:
  - green: updated within 15 minutes
  - orange: within 30 minutes
  - red: older than 30 minutes
- **Marker tooltip:** driver name, last seen, latitude and longitude, current area (from the map shapes).
- **Driver status:**
  - **On route:** recent ping and stops left.
  - **Finished:** no stops left.
  - **Offline:** no ping for 30+ minutes.
- **Selecting a driver** shows their current position, remaining and completed deliveries, and estimated finish time. On the map it draws the driver marker, the remaining stops (numbered, with ETAs) and the planned route as straight lines between stops.
- The page refreshes itself every minute.
- Visible only to users with the user preference **"Driver tracking"** (default off) or to admins.

## 6. Settings

### System Preferences → new "GPS & Drivers" tab (admins)

**Address lookup**
- provider, API key (stored in the database, shown masked, never logged)
- daily cap (default 2,500) and per-minute limit (default 90)
- lookups used today

**Map**
- tile address (default `https://tile.openstreetmap.org/{z}/{x}/{y}.png`)
- map: OpenStreetMap or Azure Maps (`GpsSettingsTbl.MapProvider`, built 2026-10-01). Azure road / satellite tiles go through `Handlers/MapTile.ashx` (signed-in users, key stays on the server, 15,000 tiles a day, counted as `AZURE_TILES`); the popup falls back to OpenStreetMap when there is no key or Azure refuses

**Depot**
- address (default "Quaffee AT Buitenverwachting; Klein Constantia Rd; Constantia; Cape Town"), with a "Look up" button that fills in latitude and longitude

**Driver tracking**
- enabled on/off
- ping interval (default 10 minutes)
- weekdays only on/off
- work hours from/to (for example 07:00–18:00)
- ping retention in days (default 30)

**ETA**
- average speed (default 30 km/h)
- service time per stop (default 5 minutes)
- marker colour thresholds (15 and 30 minutes)

### User preferences (`UserPreferences` table, edited on Administration → User Information)
- "Show route and ETA on Delivery Sheet": default off
- "Driver tracking": default off

## 7. Privacy (POPIA)

- Tracking runs only when enabled, only while the driver is logged in, and only within the configured weekdays and work hours. Old pings are deleted automatically.
- Drivers should be told in writing that their location is recorded during working hours while using the app.
- Customer addresses (not names) are sent to HeiGIT (Germany) for the one-time lookup.

## 8. Third-party services

| Service | Registration | Limits and rules |
|---|---|---|
| HeiGIT / OpenRouteService geocoding | Yes, free, at account.heigit.org | Standard plan: 3,000/day, 100/minute. Old `api.openrouteservice.org` shut down 2026-09-28; use `api.heigit.org` |
| OpenStreetMap map tiles | **No** | Show "© OpenStreetMap contributors"; normal interactive viewing only (no bulk or offline downloads); browsers already meet the technical rules; the tile URL should be switchable (it is in settings) |
| OSM Nominatim geocoding | No | Not used for customer addresses: max 1 request/second, bulk use discouraged, and the policy asks not to send personal data |
| Azure Maps geocoding (Get Geocoding, api-version 2025-01-01) | Yes, Azure subscription + Maps account; key in System Preferences | Gen2 pricing: 5,000 free a month, then per 1,000. Results stored outside Azure may be kept at most 6 months (we use 180 days + refresh); only co-ordinates are kept for our own deliveries, no reusable address database; may be shown on non-Microsoft maps. `bbox` is only a hint, so Tracker also checks the 150 km limit itself |
| Google Geocoding (not built) | Yes | 30-day caching limit and results meant for Google maps; kept as an option if Azure disappoints |

## 9. Build order

1. **People delivery type, settings tab, customer locations.** Includes geocoding, the Contact Details GPS line and popup, and the migration batch.
2. **Map areas.** Shape editor, customer pins, point-in-shape check.
3. **Driver pings.** API endpoint, Android app update, iPhone web copy, Driver Tracking page.
4. **Stop order and ETA.** Delivery sheet columns (user preference), route view on the tracking page.

Each step is released and used before the next starts.

## Resolved points (2026-09-30)

- **Collect:** "Collect" is an entry in People for customers collecting from the shop, so its type is `COLLECT`.
- **Courier / collect contacts (2026-10-01):** a contact whose own Delivery By is a `COURIER` or `COLLECT` person is never looked up automatically, even when an order for one of our drivers is captured, and delivery fixes for them are not kept. Contacts with `NONE` (e.g. Q, PS) are still looked up when an order for a driver is captured.
- **Courier-served areas (Durban, Gauteng and so on):** no special rule. Their contacts have a courier as delivery person, so the migration and lookups skip them automatically. This changes on its own if a `DELIVER` person is chosen later.
- **API key:** registered at HeiGIT. It is entered on System Preferences → GPS & Drivers once step 1 is built, and is never put in code, docs or git.

## Step 1 status (2026-09-30): builds and pages compile; not yet tested in the browser

- Schema: `SQLCommands-DriverGps-01.xml` (+ GoLive copy), run by `OrderFlagsSchemaInstaller`. `CustomerLocationTbl` also has `SourceAddress` (the address text that was looked up, to spot address changes) and `UpdatedBy`.
- The `UserPreferences` table is in the membership database (`TrackerDataSQL`), so its two new columns are added by `UserPreferencesHelper.EnsureUserPreferencesTableExists`, not by the pack.
- People: `PersonsRepository` keeps `IsDispatched` equal to `DeliveryType = COURIER` (Support Tables still edits only the tick box, which moves the type to or from Courier).
- Geocoding: `Classes/Geo` (`IGeocoder`, `HeigitGeocoder`, `Geohash`), `CustomerLocationManager` (daily cap in `GeoApiUsageTbl`, per-minute throttle in memory, postcode centroid fallback marked `REVIEW_REQUIRED`).
- Pages: System Preferences → GPS & Drivers, Contact Details GPS line + `Pages/ContactLocation.aspx` popup (Leaflet), `Tools/CustomerLocations.aspx` batch, User Information options.
- Leaflet 1.9.4 is in `Scripts/leaflet/` (the popup falls back to unpkg if it is missing).
- To do before release: check the pages in the browser, enter the key, look up the depot, run a small batch.

## Starting points and learning from drivers (2026-09-30)

Decision: OpenStreetMap coverage of South African street addresses is too patchy. The *starting* point comes from a
better geocoder (Azure Maps first; Google remains an option), and the *real* point is learned from the driver's phone
at delivery. Once a driver point exists, the provider's data is discarded, so the provider's storage limits stop mattering.

- Schema: `SQLCommands-DriverGps-02.xml` (+ GoLive copy): `GpsSettingsTbl.AzureMapsKey`, `CustomerLocationTbl.ExpiresDate / DriverFixes / LastDriverFixAt`, `MobileDeliveryProofTbl.AccuracyM`.
- Provider: `GpsSettings.GeocodeProvider` = `HeiGIT` or `AzureMaps`; `Classes/Geo/AzureMapsGeocoder.cs` (key in the `subscription-key` header; `coordinates` = depot focus, `bbox` from the 150 km radius; type → layer and High/Medium/Low → 0.9/0.6/0.3, capped at 0.6 when Ambiguous / UpHierarchy; 401/403 = bad key, 429 = throttled). Daily counts per service (`GEOCODE` / `AZURE_GEOCODE` in `GeoApiUsageTbl`). `CustomerLocationManager.Geocode` rejects any match beyond the radius (`GeoDistance`).
- Sources on `CustomerLocationTbl.Source`: `GEOCODER` (HeiGIT), `AZURE_MAPS` (ExpiresDate = +180 days), `POSTCODE`, `MANUAL`, `DRIVER`.
- Expiry: `RefreshDue` (30 days before) makes `EnsureForContact` look up again even when the address is unchanged; the batch list includes those contacts; `PurgeExpiredOncePerDay` deletes points past `ExpiresDate`.
- Driver app (TrackerDriver 0.4.0): `src/location.ts` watches the position while the capture screen is open; on save it uses a fix under 2 minutes old or tries once for 3 s. `lat`, `lng`, `accuracy` go in the outbox item and `/delivery/sync`. Android manifest has coarse + fine location (foreground only; Capacitor's WebView asks for the permission).
- Server: `MobileDeliveryManager.SyncOne` stores `AccuracyM` (only when the column exists) and calls `CustomerLocationManager.LearnFromDelivery` after a Delivered proof with a fix. Rules: accuracy ≤ 75 m (or unknown); no point / looked-up / postcode → `DRIVER`, `APPROXIMATE`, 1 fix; driver point within 60 m on another visit (≥ 6 h apart) → weighted average (older fixes capped at weight 5), fixes + 1, `VERIFIED` from 2; further → moved, 1 fix; `MANUAL` → never moved, agreeing visits counted, two visits in a row > 150 m → `REVIEW_REQUIRED` (cleared by a later agreeing visit); sundry contact skipped. Every outcome is logged (Customers log).
- Later: Google as a third provider (30-day expiry uses the same `ExpiresDate` path).

## Step 3 status (2026-10-01): driver tracking built and tested on dev (not live)

- Schema: `SQLCommands-DriverGps-03.xml` (+ GoLive copy): `DriverLocationPingTbl` (PingID, PersonID, Latitude, Longitude, AccuracyM, PingTime in SA time, ReceivedAt, Source, UserName; indexes on PersonID + PingTime and on PingTime).
- Server: `POST /api/v1/location` (`LocationController`) → `DriverTrackingManager.Receive` (tracking on, sign-in linked to a person, inside the work days and hours by the point's own time, not in the future or older than 24 h, at most 200 per call). A Delivered / Not delivered proof with a fix adds a `DELIVERY_COMPLETE` position (same rules). Positions older than `PingRetentionDays` are deleted at most once a day (on the first position or page load of the day). The app gets the tracking settings in `GET lookups` → `settings.tracking`.
- Page: **Deliveries → Driver tracking** (`Tools/DriverTracking.aspx`, data from `Handlers/DriverTrackingData.ashx`; admins or *Show driver tracking*). Drivers = anyone with a position today plus *Does deliveries* people with stops today. Stops come from the same route as the app (`MobileDeliveryManager.GetSheet`), one per contact; points from `CustomerLocationTbl`. ETA = straight line × 1.3 at the average speed, plus the minutes per stop, from the latest position (else the depot), stops without a point skipped. Today only; refreshes every minute.
- Driver app 0.5.0: `src/tracking.ts` (see `Documentation/MOBILE_API_AND_DRIVER_APP.md` section 7). Android shows the plugin's foreground notification while tracking runs; "Allow all the time" is not needed because the service starts while the app is open.
- Not built: current area from map shapes in the marker tooltip (map shapes are step 2, not built); starting the route order from the driver's position (the route still starts at the depot); history / trail of the day's positions.
