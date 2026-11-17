# 02 — Functional Inventory

Each row is a capability that exists in QOnT today. “Screens” are the staff or portal pages that expose it. “Database” names the tables the code actually reads or writes. Where a page still contains SQL, that is noted; the target pattern in this repo is Repository → Manager → page (`README.md`).

**User roles** use the names in `Web.config` and `SecurityManager`. “Staff” means any of Administrators, Agent Manager, Agents, Repair, unless a tighter role is stated. “Admin” means `SecurityManager.IsAdmin` (roles Administrators / Administrator, or username `warren`).

---

## A. Identity and shell

### A1. Staff sign-in
- **Purpose:** Forms authentication for the back office. Timeout 2880 minutes. Password min length 6, at least one non-alphanumeric, lockout after 5 failures in 10 minutes (`Web.config` membership).
- **User role:** Anonymous until login; then staff roles.
- **Inputs:** Username, password (`Account/Login.aspx`).
- **Outputs:** Forms auth cookie. Redirect into the site.
- **Dependencies:** `aspnet_Membership`, `aspnet_Users`, `aspnet_Roles` (SQL membership schema, not a `*Tbl` class).
- **Business value:** Only employees operate orders and customer data.
- **Screens:** `Account/Login.aspx`, `RecoverPassword.aspx`, `ChangePassword.aspx`, `ChangePasswordSuccess.aspx`, `Account/Register.aspx`.
- **Database:** ASP.NET membership tables. **Verify** they live in `OtterDb`.

### A2. User and role administration
- **Purpose:** Create users, assign roles, reset passwords.
- **User role:** Admin (`Administration/ManageUsers.aspx`, `ManageRoles.aspx`, `UserInformation.aspx`, `ResetPasswordHelper.aspx`).
- **Inputs:** Username, email, roles, new password.
- **Outputs:** Membership and role rows.
- **Dependencies:** `Managers/UserAccountManager.cs`, `Managers/MembershipRoleManager.cs`, `Repositories/MembershipRepository.cs`.
- **Business value:** Joiners and leavers without a developer.
- **Screens:** Administration folder; also linked from `Default.aspx`.
- **Database:** Membership tables.

### A3. Home dashboard
- **Purpose:** Launch pad grouped as contacts, orders, preparation, repairs, deliveries, system.
- **User role:** Staff.
- **Inputs:** None.
- **Outputs:** Navigation.
- **Screens:** `Default.aspx`.
- **Database:** None for the menu itself. Header shows version from config (`Site.Master` literals).

### A4. Application error banner and log
- **Purpose:** Surface the last application error and let staff open the log.
- **User role:** Staff (log viewer is a system tool; treat as admin-leaning).
- **Screens:** Banner in `Site.Master`; `Tools/LogViewer.aspx`; `Pages/LogTable.aspx`.
- **Database:** `LogTbl` via `Repositories/LogRepository.cs`. File logs under `App_Data` when `EnableEmailLogging` or `EnableRequestTiming` is on.
- **Business value:** Operations can see why a checkup or import failed.

---

## B. Customers (contacts)

### B1. Contact list and search
- **Purpose:** Find a customer and jump to orders, details, away periods.
- **User role:** Staff. Repair-only users are limited on `Pages/CustomerDetails.aspx` (legacy page; confirm it is still linked).
- **Inputs:** Search text, enabled filter (page behaviour).
- **Outputs:** Grid of companies.
- **Dependencies:** `Repositories/ContactsRepository.cs`, `Managers/CustomerManager.cs`.
- **Business value:** Every other process starts from the right account.
- **Screens:** `Pages/Contacts.aspx`. `Pages/ContactsTest.aspx` and `Pages/ClientList.aspx` look like older list variants. **Verify** they are unused before dropping them.
- **Database:** `ContactsTbl`, joins to `AreasTbl`, `ContactTypesTbl`, `PeopleTbl`.

### B2. Contact create and edit
- **Purpose:** Maintain the commercial and operational profile.
- **User role:** Staff.
- **Inputs:** Company and person names, phones, emails, postal address, area, contact type, equipment type and serial, primary and secondary item preference, quantities, prep type, packaging, preferred delivery agent, sales agent, preferred courier, flags (`Enabled`, `PredictionDisabled`, `AlwaysSendChkUp`, `NormallyResponds`, `AutoFulfill`, `UsesFilter`, `SendDeliveryConfirmation`, `DeliveryOnBehalf`), notes, contract number.
- **Outputs:** `ContactsTbl` row. New contacts default invoice type Standard (`InvoiceTypeConstants.DefaultForNewContact`). Default sales agent abbreviation `Q` / name `Quaffee` (`PersonConstants`).
- **Dependencies:** `Pages/ContactDetails.aspx`, `Models/Contact.cs`.
- **Business value:** Preferences drive reminders, prep, and who delivers.
- **Screens:** `Pages/ContactDetails.aspx` (menu “New Contact” and edit). `Pages/CustomerDetails.aspx` is an older editor. **Suspected** superseded; verify before deletion.
- **Database:** `ContactsTbl`. Account fields may be edited here or via account info — **verify** the page saves `ContactsAccInfoTbl`.

### B3. Account and commercial profile
- **Purpose:** PO required, VAT, billing and shipping address lines, accounts email, payment terms, credit limit, price level, invoice type, company registration, bank details.
- **User role:** Staff (commercial). No separate role was found.
- **Inputs:** Fields on `Models/ContactsAccInfo.cs`.
- **Outputs:** `ContactsAccInfoTbl`.
- **Dependencies:** `Repositories/ContactsAccInfoRepository.cs`. PO warning text is `UIConstants.PORequiredText` (`!!!PO required!!!`).
- **Business value:** Fulfilment does not ship to an account that must quote a PO, and accounts staff can see terms. Price level is a factor, not a price list.
- **Screens:** Contact details (embedded). **Verify** the exact tab.
- **Database:** `ContactsAccInfoTbl`, `PaymentTermsTbl`, `PriceLevelsTbl`, `InvoiceTypesTbl`.

### B4. Deliver-to addresses (delivery on behalf)
- **Purpose:** One customer pays; goods go to named sites, each with GPS.
- **User role:** Staff.
- **Inputs:** Address kind, lines, coordinates. Kinds in `AltAddressKindsTbl` (`Counts toward tracking`).
- **Outputs:** `ContactDeliverToAddressTbl`. Order header stores `DeliverToAddressID` (`Models/Order.cs`).
- **Dependencies:** `Managers/DeliveryOnBehalfManager.cs`, `Managers/CustomerLocationManager.cs`, `Pages/ContactLocation.aspx`, `Tools/CustomerLocations.aspx`.
- **Business value:** Offices, gifts, and multi-site hospitality clients.
- **Database:** `ContactDeliverToAddressTbl`, `AltAddressKindsTbl`, `CustomerLocationTbl`.

### B5. Contact change audit
- **Purpose:** Record who changed contact data.
- **User role:** System; staff read via logs if exposed.
- **Dependencies:** `Managers/ContactChangeLogManager.cs`, `ContactChangeLogSchemaInstaller`.
- **Database:** `ContactChangeLogTbl`.
- **Business value:** Disputes about address or email changes.

### B6. Customer locations and postal fill
- **Purpose:** Geocode or correct coordinates; fill area from South African postal codes.
- **User role:** Admin (`Tools/CustomerLocations.aspx.cs`, `Tools/ContactPostalFill.aspx.cs`, `Tools/PostalAreaSetup.aspx.cs`).
- **Inputs:** Address, postal code, lat/long.
- **Outputs:** Location rows, area assignment.
- **Dependencies:** `SaPostalCodeManager`, `PostalAreaSetupManager`, `WooPostalAreaMapTbl`.
- **Database:** `CustomerLocationTbl`, `SaPostalCodeTbl`, `WooPostalAreaMapTbl`, `AreasTbl`.

### B7. Contacts away
- **Purpose:** Record a period when the customer should not be reminded or predicted as if they were consuming.
- **User role:** Staff. Customers can also signal absence from a checkup email (message keys `AwayPeriod.*`).
- **Inputs:** Start, end, reason (`AwayReasonsTbl`).
- **Outputs:** `ContactsAwayPeriodTbl`. Confirmation email.
- **Dependencies:** `Repositories/ContactsAwayPeriodRepository.cs`, `CustomerManager.IsCustomerAwayOnDate` used by `CoffeeCheckupManager`.
- **Screens:** `Pages/ContactsAway.aspx`, `Pages/ContactsAwayDetail.aspx`.
- **Business value:** No coffee roasted for a closed café, and predictions shift.

### B8. Disable client (self-service and staff)
- **Purpose:** Customer link in email turns off reminders only, or disables the account. Admin is emailed. Recurring orders are reported for manual action (`MessageKeys.DisableClient.AdminManualActionRequired`).
- **User role:** Public link signed with `DisableClientSecret`. Staff can disable from contact maintenance.
- **Inputs:** Token, choice (reminders vs all).
- **Outputs:** `Enabled` / reminder flags, goodbye email, admin email.
- **Dependencies:** `Managers/DisableClientManager.cs`, `DisableClient.aspx`.
- **Business value:** POPIA / “stop emailing me” without a phone call. Recurring orders are **not** auto-cancelled — a human must finish that. **Verify** the exact columns flipped.

### B9. Portal “my details” and change requests
- **Purpose:** Customer sees their record and can request changes.
- **User role:** Contact.
- **Inputs:** Requested field changes.
- **Outputs:** `ContactPortalChangeRequestTbl`. Optional notify email (`RequestChanges.NotifyEmail`).
- **Dependencies:** `Managers/ContactPortalManager.cs`, `Portal/MyContact.aspx`.
- **Business value:** Customers correct phone and address without staff retyping, with staff approval rather than silent overwrite. **Verify** whether requests are applied automatically.

### B10. Quaffee Lovers membership
- **Purpose:** Invite a contact into the loyalty / member programme and push the WooCommerce role `member`.
- **User role:** Admin (`Tools/QuaffeeLoversInvite.aspx.cs`).
- **Inputs:** Contact, grant.
- **Outputs:** `QuaffeeLoversGrantTbl`, Woo customer role, email (`QuaffeeLoversEmailManager`).
- **Dependencies:** `Managers/QuaffeeLoversMembershipManager.cs`, Woo application password (comment in `WooCommerceApiClient.Customers.cs`).
- **Business value:** Member pricing or content on the shop. The price itself is **not** calculated in QOnT.

---

## C. Orders

### C1. New and edit order
- **Purpose:** Create or change a delivery: customer, required date, who delivers, PO, notes, invoice-type flag, lines (item, qty, prep type, packaging), optional deliver-to and walk-in GPS.
- **User role:** Staff.
- **Inputs:** Header and lines. Query `NewOrder=true`. SKU query parameters can prefill (`OrderManager.ProcessSKUParameters`, `UrlParameterConstants.SKU1`).
- **Outputs:** `OrdersTbl`, `OrderLinesTbl`. Temp lines while editing: `TempOrdersHeaderTbl`, `TempOrdersLinesTbl`.
- **Dependencies:** `Pages/OrderDetail.aspx`, `Managers/OrderDetailManager .cs` (filename contains a space), `Managers/OrderManager.cs`, `Managers/TempOrderSession.cs`.
- **Business value:** The unit of work for the roastery and the driver.
- **Rules:** see `docs/04-business-rules.md` (duplicate header, qty at least 1, sundry customer, PO warning, group-item note).
- **Screens:** `Pages/OrderDetail.aspx`, `Pages/NewOrder.aspx`, `Pages/NewOrderDetail.aspx`, `Pages/OrdersEdit.aspx`, `Pages/DeleteOrderLine.aspx`. **Suspected:** NewOrder* and OrdersEdit are legacy entry points. Verify links before treating them as current.

### C2. Order list
- **Purpose:** Find open and historical orders and open them.
- **User role:** Staff.
- **Screens:** `Pages/OrderEntry.aspx` with `Managers/OrderEntryDataSource.cs`.
- **Database:** `OrdersTbl`, `OrderLinesTbl`, `ContactsTbl`.

### C3. Move delivery date and move lines
- **Purpose:** Shift one order, or move lines onto another order for the same customer (combine drops).
- **User role:** Staff. Bulk date move is a system tool.
- **Dependencies:** `OrderManager.MoveOrderDeliveryDate`, move-line result type on `OrderManager`; `Tools/MoveDeliveryDate.aspx`.
- **Business value:** One van drop instead of two; public-holiday shifts.
- **Database:** `OrdersTbl`, `OrderLinesTbl`.

### C4. Mark invoiced / undo done
- **Purpose:** Flag that accounts have invoiced the order, or reverse completion.
- **User role:** Staff.
- **Dependencies:** `OrderManager.MarkItemAsInvoiced`, `UnDoOrderItem`.
- **Business value:** Prep and delivery screens can hide work already billed. This is a status, not an invoice document.
- **Database:** `OrdersTbl.InvoiceDone`, `Done`.

### C5. Recurring orders
- **Purpose:** A standing basket (items, delivery person, invoice-type flag, enabled, notes) that generates real orders on a cycle.
- **User role:** Staff edit; Contact can view `Portal/MyRecurring.aspx` and `MyRecurringDetail.aspx`.
- **Inputs:** Contact, lines, recurrence type (`RecurringTypesTbl`), enabled flag.
- **Outputs:** `RecurringOrdersTbl`, `RecurringOrderItemsTbl`, and later `OrdersTbl` rows. Email on add/update/disable when `RecurringOrderConstants.NotifyContactOnChange` is true (`RecurringOrderNotificationManager`).
- **Dependencies:** `Pages/RecurringOrders.aspx`, `Pages/RecurringOrderDetails.aspx`, `Repositories/RecurringOrdersRepository.cs`.
- **Business value:** Contract and subscription customers do not need a phone call every cycle.
- **Verify:** the exact generator (which page or method inserts next week’s orders, and whether it uses `NextPreperationDateByAreaTbl`). Do not assume a Windows service; none was found. Generation is in-process on a page or on order-done.

### C6. Order done (close delivery)
- **Purpose:** Capture outcome, remaining customer stock, cup count, courier tracking; write usage; update predictions; email.
- **User role:** Staff. Driver app can queue the same completion (`MobileDeliveryCompletionQueue`, setting `AutoCompleteDeliveries`).
- **Inputs:** Stock text (kg), cup count, status key, tracking number, courier.
- **Outputs:** Usage rows, prediction fields, `OrderWaybillTbl`, emails, `Done=true`.
- **Dependencies:** `Pages/OrderDone.aspx`, `Managers/OrderDoneManager.cs`, `Managers/PredictionManager.cs`.
- **Screens:** also `Tools/DeliveriesAwaitingDone.aspx` when the app delivered but the order is not done.
- **Database:** `OrdersTbl`, `ContactsUsageTbl` / usage lines, `ContactsItemUsageTbl`, `OrderWaybillTbl`, `ItemGroups` usage.

### C7. Public order view
- **Purpose:** Customer opens a link to see one order without a portal login.
- **User role:** Token holder.
- **Dependencies:** `Pages/ViewMyOrder.aspx`, `Managers/PublicOrderViewManager.cs`, `Portal/MyOrder.aspx` for logged-in contacts.
- **Database:** `OrdersTbl`, `OrderLinesTbl`.

### C8. Inter-site transfers
- **Purpose:** Move an order between Vineyard and Buiten.
- **User role:** Staff.
- **Screens:** `Pages/OrderVineyard2Buiten.aspx`, `Pages/OrderBuiten2Vineyard.aspx`.
- **Business value:** Two production or dispatch sites. **Verify** still in daily use.
- **Database:** `OrdersTbl` (site is likely a delivery person or a note — **verify** in those pages before modelling a warehouse).

---

## D. Website channel (WooCommerce)

### D1. Connection settings
- **Purpose:** Store shop URL and API keys (encrypted with `WooCommerceCryptoKey`).
- **User role:** Admin (`Tools/SystemPreferences.aspx`, Woo section).
- **Database:** `WooCommerceSettingsTbl` (`00-WooCommerceSettingsTbl.sql` and `WooCommerceSettingsRepository`).

### D2. Catalogue and attribute mapping
- **Purpose:** Map Woo products, variations, and attributes onto QOnT items, packaging, quantities, or order notes. Category filter decides what may import.
- **User role:** Admin (`Tools/WooCommerceMapping.aspx.cs`).
- **Dependencies:** `WooCommerceMappingManager`, `WooAttributeMapRoles` (both, qty only, packaging only, notes only).
- **Database:** `WooItemMappingsTbl`, `WooAttributeParentTbl`, `WooAttributeMapTbl`, `WooCategoryFilterTbl`, `WooPackagingServiceTypeTbl`, `WooCatalogCacheTbl`.

### D3. Area, shipping, payment maps
- **Purpose:** Woo shipping method and postal area become a QOnT area and delivery person. Payment method is stored for operations, not for taking payment.
- **Database:** `WooShippingMethodMapTbl`, `WooAreaDeliveryDefaultTbl`, `WooPostalAreaMapTbl`, `WooPaymentMethodMapTbl`.
- **Dependencies:** `WooCommerceAreaMappingManager`, `DeliveryPromiseManager` (website promise vs operational prep day).

### D4. Order import
- **Purpose:** Pull new shop orders, match or create the contact, write QOnT orders, remember Woo ids so they are not imported twice.
- **User role:** Admin (`Tools/WooOrderImport.aspx.cs`). Menu text: new order also “checks Woo for newer orders when enabled”.
- **Inputs:** Woo REST orders.
- **Outputs:** `OrdersTbl` / lines, `WooOrderInfoTbl`, `WooContactInfoTbl`.
- **Dependencies:** `WooCommerceOrderImportManager`, `WooContactBootstrap`, `WooImportAddressHelper`.
- **Business value:** The shop and the van share one fulfilment queue.

### D5. Customer role sync
- **Purpose:** Read Woo roles; set member role for Quaffee Lovers.
- **Dependencies:** `WooCommerceApiClient.Customers.cs`.

---

## E. Preparation and fulfilment

### E1. Items required
- **Purpose:** Aggregate lines due for a prep date into what the team must produce.
- **User role:** Staff.
- **Screens:** `Pages/ItemsRequired.aspx`. Model `ItemsRequiredSummary`.
- **Database:** open `OrderLinesTbl` joined to items, packaging, contacts, areas.
- **Business value:** Roast and pack plan for the day.

### E2. Preparation summary
- **Purpose:** Shorter production view of the same demand.
- **Screens:** `Pages/PreperationSummary.aspx`. Repository `PreparationSummaryRepository`.
- **Database:** orders and lines.

### E3. Summary of coffee required
- **Purpose:** Coffee-only production total.
- **Screens:** `Pages/SummaryOFCoffeeRequired.aspx`.
- **Business value:** Green-bean and roast batch sizing. Generalise as “bulk production summary” in RainbowOF.

### E4. Delivery sheet
- **Purpose:** The run sheet: who, where, what, flags (PO, invoice type pill, recurring origin, not-done).
- **User role:** Staff. Mobile layout via `MobileSite.Master` / `Classes/MobileStaffLayout.cs` and the bottom tab bar on `Site.Master`.
- **Dependencies:** `Managers/DeliverySheetManager.cs`, `Repositories/DeliverySheetRepository.cs`, `DeliveryRouteRepository`.
- **Screens:** `Pages/DeliverySheet.aspx`, `Pages/OrderSheet.aspx` (print variant — **verify**).
- **Database:** `OrdersTbl`, `ContactsTbl`, `AreasTbl`, `PeopleTbl`, `DeliveryRouteTbl`, `InvoiceTypesTbl`.

### E5. Delivery order (sequence)
- **Purpose:** Set stop order for a day, by hand or from GPS / learned times.
- **User role:** Staff.
- **Screens:** `Tools/DeliveryOrder.aspx`.
- **Dependencies:** `RouteTimesManager`, `RoadTimesManager`, `RoutePlanner`, `RoadLegTbl`.
- **Database:** `DeliveryRouteTbl`, `RoadLegTbl`, `CustomerLocationTbl`.

### E6. Driver tracking
- **Purpose:** Map driver pings during work hours.
- **User role:** Admin for the data handler (`Handlers/DriverTrackingData.ashx.cs`).
- **Screens:** `Tools/DriverTracking.aspx`.
- **Dependencies:** `DriverTrackingManager`, `POST /api/v1/location`.
- **Database:** `DriverLocationPingTbl`, `GpsSettingsTbl`.

### E7. Driver app administration
- **Purpose:** Download APK, set whether the app auto-completes orders, POD email, terms text, office email, landline prefixes, tracking window.
- **User role:** Admin (`Tools/DriverApp.aspx`).
- **Database:** `MobileApiSettingsTbl`.

### E8. Proof of delivery
- **Purpose:** Store outcome, received-by, signature, missing items; email PDF.
- **User role:** Driver (capture), staff (view any), Contact (view own).
- **Dependencies:** `ProofOfDeliveryManager`, `MobileDeliveryManager`, `Handlers/ProofOfDelivery.ashx`.
- **Database:** `MobileDeliveryProofTbl`, `MobileDeliveryOptionsTbl` (outcomes: delivered, not at client, needs name).

### E9. Awaiting order-done
- **Purpose:** App said delivered; office has not closed the order (cup count still missing).
- **Screens:** `Tools/DeliveriesAwaitingDone.aspx`.
- **Business value:** Predictions do not move until a human confirms consumption.

### E10. Delivery promise rules
- **Purpose:** What date the website promised, from area and time-of-week windows. Separate from the operational prep calendar.
- **User role:** Admin (`Tools/DeliveryPromiseRules.aspx.cs`).
- **Dependencies:** `DeliveryPromiseManager`.
- **Database:** `DeliveryPromiseRuleTbl`.
- **Business value:** Flag an order scheduled sooner than the public promise (`IsSoonerThanPromiseException`). Courier and “Dispatch” promises are excluded from that exception.

### E11. Holiday closures
- **Purpose:** Dates the business or an area is closed; predictions and promises extend across them.
- **Screens:** `Tools/HolidayClosures.aspx`, `HolidayClosureDetail.aspx`.
- **Dependencies:** `HolidayClosureProvider`, `PredictionManager.AddHolidayExtension`.
- **Database:** `HolidayClosuresTbl`, also `ClosureDatesTbl` (**verify** whether both are live).

### E12. Item groups (rotation)
- **Purpose:** A customer on a group item receives the next product in the group on the following order (for example a rotation of blends).
- **User role:** Staff.
- **Screens:** `Pages/ItemGroups.aspx`, `Pages/GroupItemDetail.aspx`.
- **Dependencies:** `ItemGroupDataSource`, `UsedItemGroupRepository`. UI note: `UIConstants.LastOrderGroupNote`.
- **Database:** `ItemGroupsTbl`, `UsedItemGroupsTbl`. Service type constant `GroupItem = 21`.

---

## F. Demand generation (checkups and mail)

### F1. Send coffee checkup
- **Purpose:** Build the list of customers due inside the reminder window, show predicted items, send HTML email, log the send.
- **User role:** Staff.
- **Inputs:** Window days (default 9, min 5, max 30). Per-customer exclude-this-time.
- **Outputs:** Emails, `SentRemindersLogTbl`, temp working set `TempCoffeecheckupCustomerTbl`, `TempCoffeecheckupItemsTbl`.
- **Dependencies:** `Pages/SendCoffeeCheckup.aspx`, `Pages/LoadSendCoffeeCheckup.aspx`, `CoffeeCheckupManager`, `CoffeeCheckupEmailManager`.
- **Rules:** skip internal customers, invalid email, disabled, over max reminders, away on the effective date. Minimum recurring cycle 20 days (`CoffeeCheckupMinMonthlyRecurringDays`) so a short standing order is not also nagged.
- **Database:** predictions on usage tables, `SendCheckupEmailTextsTbl` (editable copy).

### F2. Reminder history
- **Purpose:** See who was emailed and whether it succeeded.
- **Screens:** `Pages/SentRemindersSheet.aspx`.
- **Database:** `SentRemindersLogTbl`.

### F3. Send client mail
- **Purpose:** Filter contacts and send one marketing or operational HTML email. Not the checkup.
- **Screens:** `Pages/SendClientMail.aspx`.
- **Dependencies:** `ClientMailManager`, `ClientMailTemplateRepository`.
- **Database:** `ClientMailTemplatesTbl`, `ClientMailSendLogTbl`, `ContactsTbl`.

### F4. Message catalogue
- **Purpose:** Edit customer-facing strings without a developer.
- **Screens:** `Tools/MessagesEditor.aspx`.
- **Dependencies:** `MessagesResourceManager`, `Classes/MessageKeys.cs` (keys for checkup, order status, disable, away, waybill, recurring).
- **Database:** **Verify** whether texts live in `SysDataTbl`, a messages table, or resx. Keys are compile-time; bodies are data.

---

## G. Repairs and assets

### G1. Repair list and job card
- **Purpose:** Log a machine fault, status, equipment, serial, link to customer and optionally an order.
- **User role:** Staff, especially Repair. Driver app can list, create, and update (`/api/v1/repairs`, `MobileRepairManager` — “same rules as Repair Detail”).
- **Inputs:** `Models/RepairFormData.cs`.
- **Outputs:** `RepairsTbl`. Status change can email (`RepairStatusesTbl.EmailContact`).
- **Dependencies:** `Pages/Repairs.aspx`, `RepairDetail.aspx`, `RepairStatusChange.aspx`, `RepairManager`.
- **Screens:** Portal `MyRepairs.aspx`, `MyRepair.aspx`. `RepairDetailOld.aspx` is legacy.
- **Database:** `RepairsTbl`, `RepairStatusesTbl`, `RepairFaultsTbl`, `EquipTypesTbl`, `EquipConditionsTbl`.
- **Business value:** Field service beside replenishment. A repair can spawn an order so parts ride the same van (`EnsureRelatedOrder`).

### G2. Equipment and condition catalogues
- **Purpose:** Machine types and condition codes.
- **Screens:** `Pages/Lookups.aspx` tabs Equipment Types (and conditions if present — conditions repository exists: `EquipConditionsRepository`).
- **Database:** `EquipTypesTbl`, `EquipConditionsTbl`.

### G3. Tracked service items
- **Purpose:** Which service types drive the prediction engine and which usage-table columns they write (`UsageDateFieldName`, `UsageAveFieldName`, `ThisItemSetsDailyAverage`).
- **Dependencies:** `TrackedServiceItemRepository`, `ContactTrackedServiceItemsRepository`.
- **Database:** `TrackedServiceItemsTbl`, `ContactTrackedServiceItemsTbl`.
- **Business value:** The forecast is data-driven: coffee sets daily average; clean, filter, descale, and service consume that average. Defaults if no install date: daily 5, next coffee +20 days, clean +20, filter and descale +30, service +1 year, clean average 200, filter 300, descale 500, service 10 000 (`PredictionManager.UpdatePredictions`).

### G4. Cup-count tracker
- **Purpose:** Machine counter used as consumption evidence.
- **Database:** `TotalCountTrackerTbl`, `Repositories/TotalCountTrackerRepository.cs`.
- **Business value:** Distinguishes “we delivered 6 kg” from “the machine actually used N cups”.

---

## H. Master data (Lookups)

Page: `Pages/Lookups.aspx`. Tabs found in markup:

| Tab | Table | Why it exists |
|---|---|---|
| Items | `ItemsTbl` | Sellable and service items: description, SKU, enabled, characteristics, detail, service type, replacement item, short name, units per qty, unit, sort |
| People | `PeopleTbl` | Drivers, agents, couriers-as-people. Abbreviation, enabled, delivery type, collections flag, normal delivery weekday, security username |
| Equipment Types | `EquipTypesTbl` | Machines |
| Contact Types | `ContactTypesTbl` | Relationship (see constants). `DriverCollectionOnly` marks supplier-collection contacts |
| Areas | `AreasTbl` + `AreaPrepDaysTbl` | Geography and prep weekday, delay days, delivery order |
| Delivery Days | area week board (`AreaWeekBoardManager`, `NextPrepDateByAreaRepository`) | Which dates are the next prep/delivery for each area |
| Packaging | `ItemPackagingsTbl` | Bag size / colour / symbol shown on sheets (also water-filter pack ids 8 and 9 in constants) |
| Sort Orders | `ItemSortOrderTbl` | Display sequence values |
| Couriers | `CourierServicesTbl` | Name, tracking URL, waybill format, default |
| Delivery Outcomes | `MobileDeliveryOptionsTbl` | Driver choices |
| Repair Statuses | `RepairStatusesTbl` | Status, email-contact flag, sort, note |
| Alt Address Types | `AltAddressKindsTbl` | Deliver-to kinds |
| InvoiceTypes | `InvoiceTypesTbl` | Standard, delivery note, dispatch note, and any later flags. Pill label on the delivery sheet |
| PaymentTerms | `PaymentTermsTbl` | Days or day-of-month |
| PriceLevels | `PriceLevelsTbl` | Name and pricing factor |

Also under System: `Pages/SupportTables.aspx` (**verify** contents; likely a second admin grid).

Service-type and prep-type catalogues: `ItemServiceTypesTbl`, `ItemPrepTypesTbl` (repositories exist; confirm which lookup UI edits them — they may be inside the Items tab or only seeded).

People delivery types used in code: collection, courier, and named ids Collection `10`, Courier `7`, Parcel `5`, default delivery person `3` abbreviation `SQ` (`DeliveryConstants`). **These ids are production data.** RainbowOF must map them, not hardcode them.

---

## I. System tools

| Function | Page | Role | Purpose |
|---|---|---|---|
| All tools hub | `Tools/SystemTools.aspx` | Admin, AgentManager | Index. Code also allows `Agent Manager` spelling — see security doc |
| Move delivery date | `Tools/MoveDeliveryDate.aspx` | Staff via hub | Bulk-shift required dates (closures) |
| Holiday closures | `Tools/HolidayClosures.aspx` | Staff | Closure calendar |
| XML to SQL | `Tools/XMLtoSQL.aspx` | Admin | Run schema XML command batches |
| Messages editor | `Tools/MessagesEditor.aspx` | Admin | Customer wording |
| System preferences | `Tools/SystemPreferences.aspx` | Admin | GPS, driver, Woo, general (`SystemPreferencesHdrTbl`, `GpsSettingsTbl`) |
| Woo mapping | `Tools/WooCommerceMapping.aspx` | Admin | Channel maps |
| Email diagnostics | `Tools/EmailDiagnostics.aspx` | Admin | SMTP test |
| SQL connection test | `Tools/SqlConnectionTest.aspx` | Admin | Connectivity |
| Database backup | `Tools/DatabaseBackup.aspx` | Admin | Backup to `DatabaseBackupFolder` or `App_Data/Backup` |
| Log viewer | `Tools/LogViewer.aspx` | Staff/admin | `LogTbl` |
| API tester | `Tools/ApiTester.aspx` | Admin | Call `/api/v1`, devices, revoke |
| Merge from QuickBooks | `Tools/MergeCustomersFromQB.aspx` | Admin | Master-data merge |
| Depot location | `Tools/DepotLocation.aspx` | Admin | Start point for routes |
| Leave application | `Pages/LeaveApp.aspx` | Staff | Internal leave form. **Verify** whether it persists or only prints/emails |
| About | `Tools/About.aspx` | Staff | Version |
| Machine keys | `Tools/GenMachineKeys.aspx` | Admin | Generate `<machineKey>` material. Treat as sensitive |
| Test people | `Tools/TestPeople.aspx` | Dev | **Suspected** non-production |
| Auto class maker | `Tools/AutoClassMaker.aspx`, `test/*` | Dev | Code generation. Not a business feature |
| SMTP tester | `smpttester/index.aspx` | Dev | Mail debug |

---

## J. Customer portal

| Function | Page | Purpose |
|---|---|---|
| Portal login | `Portal/Login.aspx` | Contact role |
| Home | `Portal/Home.aspx` | Landing |
| My details | `Portal/MyContact.aspx` | Read profile, request changes |
| My orders | `Portal/MyOrders.aspx`, `MyOrder.aspx` | History and POD link |
| Repeat order | `Portal/RepeatOrder.aspx` | Customer asks to repeat a previous order. **Verify** whether this creates an order or a request |
| My repairs | `Portal/MyRepairs.aspx`, `MyRepair.aspx` | Job status |
| My recurring | `Portal/MyRecurring.aspx`, `MyRecurringDetail.aspx` | Standing orders |
| Password | `Portal/ChangePassword.aspx` | Self-service |
| View as | `Portal/ViewAs.aspx` | Staff impersonation of a contact. **Must** be admin-only in RainbowOF; verify the current check |

Portal base: `Portal/PortalPageBase.cs`. Link table: `ContactUserLinkTbl`. Settings: `ContactPortalSettingsTbl`.

---

## K. Mobile API (driver)

Documented in full in `Documentation/MOBILE_API_AND_DRIVER_APP.md`. Capabilities:

- Sign-in bound to a person, token hash only, idle expiry.
- Delivery dates and sheet (me, all, collections, or a person id).
- Offline sync of up to 100 deliveries with client reference de-duplication (`MobileSyncRefTbl`).
- Route order save.
- Proofs and signatures (admin read).
- Repairs CRUD.
- Contact search.
- Location batch.
- APK download link.
- Admin device list and revoke.
- Request log 30 days, no bodies (`MobileApiRequestLogTbl`).

Business value: drivers work without paper and without waiting for the office network. Order Done and prediction still happen on the server, optionally after the HTTP response (`MobileDeliveryCompletionQueue`).

---

## L. Reporting surfaces (detail in doc 06)

There is no report-server catalogue. Operational “reports” are pages:

- Items required, preparation summary, coffee required summary, delivery sheet, order sheet, sent reminders, deliveries awaiting done, driver tracking, log viewer, client-mail send log, QuickBooks merge preview, prediction bulk recalc results (`PredictionManager.RecalculateBulk`).

Exports found: proof PDF; delivery and prep pages are printed from the browser (`Print.master`, `Pages/Print.aspx`); emails are HTML. A dedicated Excel/CSV export framework was **not** found. **Verify** any grid “export” buttons on `OrderEntry` and `Contacts` during UAT.

---

## M. Explicit non-functions

| Missing capability | Evidence | RainbowOF implication |
|---|---|---|
| Stock on hand in a warehouse | No inventory balance table in repositories | Add only if the new product needs it; do not pretend QOnT had it |
| Supplier purchase orders | No PO-to-supplier document. `RequiresPurchOrder` is the **customer’s** PO | Purchasing is new scope |
| Priced quotes | No quote entity | New scope if RainbowOF sells quotes |
| Invoices and payments | Flag plus account fields only | Integrate or build; do not migrate a ledger that is not there |
| Multi-tenant | Single company constants | Design tenancy now if RainbowOF is a product, not only Quaffee |
