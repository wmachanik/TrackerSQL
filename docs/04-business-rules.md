# 04 — Business Rules

Rules below are implemented in code unless marked **Verify** or **Suspected**. The “reason” is the operational reason a roastery would encode the rule, inferred from how it is used. RainbowOF may replace a rule, but it must not drop it silently.

Code locations are the current home of the rule. Some pages still duplicate checks; managers are the authority when both exist.

---

## 1. Identity and access

| Rule | Purpose | Reason | Location |
|---|---|---|---|
| Anonymous users are denied. Allowed roles: Administrators, Agent Manager, Agents, Repair | Keep customer data inside the company | One shared operational database | `Web.config` authorization |
| Username `warren` is an administrator even without the role | Break-glass / owner account | Historical single owner | `SystemConstants.UserConstants.AdminUserName`, `SecurityManager.IsAdmin` |
| Admin role aliases are `Administrators` and `Administrator` only. `Admin` is checked on some pages but is **not** an alias inside `SecurityManager` | Inconsistent admin test | Pages grew separately | `SecurityManager` vs `WooOrderImport.aspx.cs`, `Lookups.aspx.cs` (`Administrators` or `Admin`) |
| System tools accept `Administrators`, `Admin`, or `AgentManager` | Senior agents can open the tool hub | Spelling of Agent Manager may not match `Web.config` (`Agent Manager` with a space) | `Tools/SystemTools.aspx.cs` |
| A user whose only role is `repair` cannot use the full customer editor | Technicians see repairs, not commercial fields | Privacy and focus | `Pages/CustomerDetails.aspx.cs` (checks `"repair"`). **Verify** `ContactDetails.aspx` has the same guard |
| Portal users are role `Contact` and linked in `ContactUserLinkTbl` | A customer sees one account | No shared inboxes as logins | `ContactPortalManager`, `ContactPortalSchemaInstaller` |
| Driver API does not accept the website cookie. Bearer token, hash stored, 90-day life, 30-day idle revoke | Lost phone can be killed; cookies are not API keys | Mobile threat model | `MobileAuthManager`, `Documentation/MOBILE_API_AND_DRIVER_APP.md` |
| Login: 5 bad passwords locks the membership account; API adds 10 failures per IP per 15 minutes and 120 requests per device per minute | Stop guessing | Public login and a phone app | `Web.config` membership; `MobileApi.LoginFailuresPerIp`, `RequestsPerMinute` |
| Password: length ≥ 6, ≥ 1 non-alphanumeric, unique email, no security question | Low bar, email is the recovery key | Legacy membership defaults | `Web.config` |
| Register page exists | Could allow open registration | Dangerous if anonymous | `Account/Register.aspx`. **Verify** production authorization |

---

## 2. Customers

| Rule | Purpose | Reason | Location |
|---|---|---|---|
| Contact id 9 is the sundry / walk-in account, name `ZZName`, prefix `ZZ` | One bucket for people who are not accounts | Cash and one-off web or counter sales | `CustomerConstants` |
| Walk-in area and GPS sit on the **order**, not on contact 9 | The pin must be today’s address | Otherwise every walk-in maps to the same point | `Order.WalkInAreaID`, `WalkInLatitude`, `WalkInLongitude` |
| Sundry notes format: name before `:`, optional `[#email#]` | Driver and emails need a human name | Notes field is the only identity | `Classes/SundryNotes.cs` |
| Repairs on the sundry contact require a real name | A job card without a name is useless | `RepairManager.ValidateSundryContactName` |
| New contact invoice type is Standard (1). New order invoice type is Standard | Sensible default paperwork | `InvoiceTypeConstants` |
| New contact sales agent abbreviation `Q` (Quaffee) if none chosen | House account | `PersonConstants` |
| Contact types 1–9 are the commercial relationship (coffee+maintenance through info-only and green-coffee-only) | Drives what “service” means | Contract vs supply-only | `CustomerTypeConstants`. **Verify** rows still match |
| `DriverCollectionOnly` contact type is a collection point, not a delivery customer | Van collects stock (supplier or similar) | Direction of travel | `ContactTypesRepository` |
| `Enabled` false removes the customer from normal operations | Closed account | Churn | `Contact.Enabled`, disable-client flow |
| `PredictionDisabled` stops forecast updates | Irregular or project customers | A café that orders only for events must not get a fake next-coffee date | `Contact.PredictionDisabled`. **Verify** every prediction query honours it |
| `AlwaysSendChkUp` forces inclusion in reminders | Strategic accounts | Even if the date math would skip them | `Contact.AlwaysSendChkUp` |
| `NormallyResponds` is a behaviour flag for reminder priority | Staff know who ignores mail | **Verify** how checkup sorting uses it |
| `AutoFulfill` means the standing pattern can be fulfilled without a fresh confirmation | Trust / contract | `Contact.AutoFulfill`. **Verify** the checkup and recurring generator both read it |
| Reminder count ceiling is 10 | Stop nagging | Reputation and POPIA | `CustomerConstants.MaxReminders`, `CoffeeCheckupManager.IsEligibleForReminder` |
| Internal customers are never reminded | Do not email the roastery’s own rows | `CoffeeCheckupManager.IsInternalCustomer` |
| No email → no reminder | Cannot send | `HasValidEmailAddress` |
| Away on the effective reminder date → do not send; log as not sent | They are closed | `CustomerManager.IsCustomerAwayOnDate` |
| Self-disable offers “reminders only” or “everything”. Admin is told if recurring orders still exist and must cancel them by hand | Customer intent is not the same as tearing up a contract | `MessageKeys.DisableClient` |
| Disable link is HMAC-style secret `DisableClientSecret`. Rotating the secret kills old links | Token must not be guessable | `Web.config`, `DisableClientManager` |
| Delivery on behalf uses extra addresses; kinds may or may not count toward consumption tracking | A gift address must not reset the café’s forecast | `AltAddressKindsTbl` “Counts toward tracking”, `DeliveryOnBehalf` |
| PO required shows `!!!PO required!!!` and short `!!!` | Driver must not leave goods without a PO | `UIConstants`, `ContactsAccInfo.RequiresPurchOrder` |
| Credit limit and price factor are stored, not enforced on the order | Accounts uses them elsewhere | No order-line price exists | `ContactsAccInfo.Limit`, `PriceLevelsTbl.PricingFactor` |
| Contact changes are logged | Dispute and audit | `ContactChangeLogManager` |
| Portal change requests do not silently overwrite (**assumed** from the table name `ContactPortalChangeRequestTbl`) | Staff approve identity data | **Verify** `ContactPortalManager` apply path |
| Equipment serial field is `EquipentSN` (typo preserved) | One machine identity on the contact | Older data had a single serial, not a fleet | `Models/Contact.cs`. RainbowOF should allow many assets |
| Preferred courier is for waybill emails, separate from preferred delivery agent | Local van vs national parcel | `Contact.PreferredCourierServiceID` |

---

## 3. Ordering

| Rule | Purpose | Reason | Location |
|---|---|---|---|
| An order is a header plus lines. Line is item, qty, prep type, packaging. No price, discount, or tax | Fulfilment document | Money is outside | `Models/Order.cs`, `OrderLine.cs` |
| Same contact and same required-by date is one order. A second save conflicts and returns the existing id | One drop, one paperwork set | `OrderManager.FindExistingOrderForHeader`, `FindDuplicateOrderForHeader` |
| Quantity ≤ 0 is saved as 1, including payloads to the driver app | Zero-qty lines are data errors that still need to show on the sheet | `FormatConstants.OrderLineQtyAtLeastOne` |
| Quantities display with up to 3 decimal places, trailing zeros trimmed | Coffee is weighed | `FormatConstants.DigitFormat`, `NumDecimalPoints = 3` |
| Dates display `dd/MM/yyyy`. Business clock is South Africa | Local operations | `FormatConstants.DateFormat`, `TimeZoneUtils` |
| Sentinel null date is 1980-01-01 | Access-era “no date” | `DatabaseConstants.SystemMinDate`. Do not show it as a real date |
| Invoice type on the order is a paperwork flag, default Standard. Recurring orders copy their invoice type onto generated orders | Delivery note vs invoice vs gift | `InvoiceTypeID` comments |
| Who delivers is `ToBeDeliveredByID` → `PeopleTbl` | The run is by person, not by an abstract route code | `Order.ToBeDeliveredByID` |
| Default delivery person id 3, abbreviation `SQ`, when a default is required | Named house driver | `DeliveryConstants`. **Do not hardcode in RainbowOF**; seed and configure |
| Courier person id 7, collection 10, parcel 5 are special | Waybill rules and “not a lucky early delivery” | `DeliveryConstants`, `OrderDoneManager.IsDispatchDeliveryPerson` |
| People also have a delivery type string (courier, collect) | Ids drift; type is the real rule | `PersonsRepository.GetDeliveryType`, `DeliveryTypes` |
| Moving a line can merge into another order and delete the empty source | Combine two orders for one visit | `OrderManager` move-line result (`SourceOrderRemoved`) |
| Group item service type 21 means “pick the next item in the group” | Subscription rotation (blends) | `ServiceTypeConstants.GroupItem`, `UsedItemGroupsTbl`, `UIConstants.LastOrderGroupNote` |
| Item 36 on an order should prompt “should this be a repair?” | Faults are not coffee lines | `ItemConstants.RepairCheckItemID` |
| Replacement item on `ItemsTbl` supersedes a discontinued SKU | Catalogue changes under customers | Lookups Items tab `ReplacementItemID` |
| Notes sort order 10 and group sort order 15 push notes and groups in the grid | Sheet readability | `ItemConstants` |
| Packing-check warning text exists | Wrong bag size is a common error | `UIConstants.PackingCheckText` |
| Website promise is not the prep calendar. Promise uses `DeliveryPromiseRuleTbl` (windows in week-minutes, modes FixedDow, SameWorkday, NextWorkday, Wednesday-after-noon-to-Friday, weekend wrap) | Marketing SLA vs roastery weekday | `DeliveryPromiseManager` |
| “Sooner than promise” is an exception only when the order date is today, the required date is before the promised date, the person is not courier-style, and the promise kind is not Dispatch | Staff should notice a favour, not a courier’s normal next-day | `IsSoonerThanPromiseException` |
| Holiday closures extend predicted and promised dates | Do not schedule a closed Tuesday | `HolidayClosureProvider`, `AddHolidayExtension` |
| Order flags mark “came from recurring” on the delivery sheet | Driver knows it is a standing drop | `OrderFlagsSchemaInstaller` |

---

## 4. Recurring and subscriptions

| Rule | Purpose | Reason | Location |
|---|---|---|---|
| Notify the contact when a recurring order is added, updated, or disabled | No silent contract change | `RecurringOrderConstants.NotifyContactOnChange = true` |
| Do not also send a checkup when the recurring cycle is shorter than 20 days | Double emails | `CoffeeCheckupMinMonthlyRecurringDays` |
| Disabling the customer does not auto-disable recurring orders | Legal / stock commitment | Disable-client admin email |
| Recurrence types are data (`RecurringTypesTbl`) | Weekly, fortnightly, monthly without a code change | `RecurringTypeRepository` |

**Verify:** lead time before the cycle creates the real order, and whether `AutoFulfill` skips confirmation.

---

## 5. Checkup and mail

| Rule | Purpose | Reason | Location |
|---|---|---|---|
| Reminder window default 9 days, allowed 5–30 | Far enough to roast, near enough to be true | `Web.config` CoffeeCheckup* keys, `CoffeeCheckupManager.GetReminderWindowDays` |
| Date matrix cache TTL 10 minutes, buffer 5 days, rebuild if source changes | The due-list is expensive | Same keys |
| SQL matrix is off (`CoffeeCheckupUseSqlMatrix=false`) | C# is the current calculator | Config |
| Failed sends are logged and listed | Staff retry | `LogFailedCustomers`, `SentRemindersLogTbl` |
| Staff can drop one customer from this run only | Exception without turning them off | `ExcludePreparedContactThisTime` |
| Email test mode redirects all mail to `EmailTestRecipient` | Non-production safety | `EmailTestMode` |
| Client mail is a separate template and log from checkups | Marketing must not look like a service due-date | `ClientMailManager` |
| Away confirmation and goodbye emails use message keys | Tone is editable | `MessageKeys.AwayPeriod`, `DisableClient` |

---

## 6. Consumption, stock at customer, and service dates

| Rule | Purpose | Reason | Location |
|---|---|---|---|
| “Stock” on Order Done is kilograms still at the customer, not warehouse stock | Leftover beans are not consumption | `OrderDoneManager` stock parameter |
| Cups represented by leftover stock = round(kg × 100). That many cups are subtracted from the cup count | 1 kg ≈ 100 cups | `BusinessConstants.TypicalCupsPerKg`, `AddItemsToUsageTables` |
| Daily average uses at most 6 historical points | Old behaviour must not dominate | `MaxRollingAveValues` |
| Typical daily consumption 5 if unknown | Cold start | `TypicalAverageConsumption` |
| Typical clean 200, filter 300, descale 500 (cup or count units as stored) | Maintenance ratios until real averages exist | `BusinessConstants` and fallback in `UpdatePredictions` |
| No install date → next coffee today+20, clean +20 more, filter and descale +30, service +1 year, service average 10 000 | A new machine still needs a diary | `PredictionManager.UpdatePredictions` |
| With install date, tracked service items drive the columns. Items with `ThisItemSetsDailyAverage` run first and set the daily rate. Other items schedule `round(lastQty * itemAverage / dailyAverage)` days after last usage, unless delivery cadence returns a date | One consumption clock, many maintenance clocks | `CalcAndSaveNextRequiredDates`, `ApplyTrackedItem` |
| Holiday extension is applied to those dates | Do not land a service on a closure | `AddHolidayExtension` |
| Bulk recalc updates coffee, filter, clean, descale, service, and daily rate. It does not rebuild recurring orders | Forecast repair without creating deliveries | `RecalculateBulk` |
| Cup count must persist or Order Done fails with an explicit message | A closed order without usage freezes the forecast | `CompleteOrderInternal` message in `OrderDoneManager` |
| Filter SKUs `8ClarFltr` and `8ClarBlue`, packaging ids 8 and 9 | Water filters are consumables tied to `UsesFilter` | `ItemConstants` |
| Service type ids 1–19 and 21 are semantic (clean, coffee, count, descale, filter, swop in/out, service, holiday weeks, green bean, group) | The same “item type” column means both a product and a work type | `ServiceTypeConstants` |

Swop types (collect, start, stop, return) are asset exchanges: take a machine away or install one. Holiday service types (one week through two months) are absence encoded as a service, overlapping `ContactsAwayPeriodTbl`. **Verify** which path staff use today.

---

## 7. Fulfilment and delivery

| Rule | Purpose | Reason | Location |
|---|---|---|---|
| Prep day of week plus delay days per area produces the delivery date | Vans run fixed weekdays | `AreaPrepDaysTbl` |
| Delivery order on the area and on the prep-day row is the default sequence | Geography | Lookups Areas |
| Driver route overrides that sequence for one date; empty list restores the suggestion | Local knowledge | `POST delivery/route` |
| Proof sync is idempotent on client reference, max 100 deliveries, max 500 stops, max 200 GPS points | Phones retry | Mobile API doc and controllers |
| Short delivery stores quantities handed over | Customer was promised a line qty and got less | `MobileDeliveryProofTbl.MissingItems` |
| Outcomes can require a received-by name or mean “not at client” | POD quality | Lookups Delivery Outcomes |
| Terms must be accepted when the app version sends them | Dispute | `termsAccepted` on sync |
| Auto-complete is a setting, default false until driver-app settings are saved | Office may still want to type the cup count | `MobileApi.AutoCompleteDeliveries`, `MobileApiSettingsTbl` |
| Waybill required when the person is a dispatch person and settings say required | Customers track parcels | `RequiresTrackingNumber` |
| Duplicate waybill (same number and courier on another order) warns with the other customer and date | Copy-paste errors | `GetDuplicateWaybillWarning` |
| Waybill that does not match the courier’s patterns warns but can be confirmed | New courier formats | `GetWaybillFormatWarning` |
| Tracking email uses courier URL template | One click for the customer | `CourierServicesTbl.TrackingUrl` |
| GPS stored only when tracking is on and inside work days and hours; points in the future or a day old are dropped | Privacy and junk | API location rules |
| Landline prefixes (example `021`) must not be offered WhatsApp | Wrong channel | Driver app settings `landlinePrefixes` |
| Collections runs are a separate sheet | Pickup is not delivery | `person=collections`, `HandsOverCollections` |

---

## 8. Repairs

| Rule | Purpose | Reason | Location |
|---|---|---|---|
| Status can email the contact | Customer hears “ready” or “waiting for part” | `RepairStatusesTbl.EmailContact` |
| A repair should have a related order when parts or a visit are needed | One van | `RepairManager.EnsureRelatedOrder` |
| Repair notes and status notes are copied onto that order | Driver sees the job | `SyncRelatedOrderNotes`, `SyncRelatedOrderRepairStatusNotes` |
| History shown for 48 months | Long enough for a machine life story | `ContactRepairHistoryMonths` |
| Driver repair create uses the same validation as the web form | One rule set | `MobileRepairManager` |

---

## 9. Channel (WooCommerce)

| Rule | Purpose | Reason | Location |
|---|---|---|---|
| Do not import an order whose Woo id is already in `WooOrderInfoTbl` | Idempotent | `WooCommerceOrderImportManager` |
| Category filter limits which products become orders | The shop sells things the roastery does not fulfil in QOnT | `WooCategoryFilterTbl` |
| Attribute map role: quantity, packaging, both, or notes only | A Woo attribute is not always a SKU | `WooAttributeMapRoles` |
| Shipping method and postal code resolve area and delivery person | Website zones are not QOnT areas | `WooShippingMethodMapTbl`, `WooPostalAreaMapTbl`, `WooAreaDeliveryDefaultTbl` |
| API secrets are encrypted at rest | Repo is public; keys must not be plaintext in the table | `WooCommerceCryptoKey`, `WooCommerceSettingsManager` |
| Setting member role on Woo needs a WordPress user that can assign roles | Loyalty | `WooCommerceApiClient.Customers.cs` |
| Payment method is mapped and stored, not charged | Already paid on the shop | `WooPaymentMethodMapTbl` |

---

## 10. Pricing, discount, and ordering constraints — honest gaps

These were requested as examples. In this system:

- **No discount engine.** No percent-off, coupon, or free-goods rule in managers.
- **No price list application.** `PricingFactor` is a label for accounts. RainbowOF may introduce price lists; that is new behaviour.
- **No credit-limit block.** `Limit` is stored. Orders are not rejected when exceeded.
- **No minimum order.** Not found.
- **No stock-availability block.** You can order an item even if the roastery has not bought green coffee. There is no on-hand check.
- **Ordering constraint that does exist:** one order per contact per required date; area calendar; PO warning (warning, **verify** if it is a hard stop); qty at least 1; sundry name required on repairs.

---

## 11. Audit

| Rule | Purpose | Location |
|---|---|---|
| Application messages go to `LogTbl` by category (`SystemConstants.LogTypes`, including Orders and SendCheckup) | Support | `AppLogger` |
| Contact field changes go to `ContactChangeLogTbl` | Master-data audit | `ContactChangeLogManager` |
| API calls (not bodies) go to `MobileApiRequestLogTbl` for `LogDays` (30) | Abuse and latency | `MobileApiHandlers` |
| Reminder attempts, including skips for away, go to `SentRemindersLogTbl` | “We did email you” | `CoffeeCheckupManager` |
| Client-mail sends go to `ClientMailSendLogTbl` | Campaign history | `ClientMailRepository` |
| Email MIME dumps only when `EnableEmailLogging` is true | Debug, not an audit trail | `App_Data` |

There is no general “who changed this order line” history table. **Gap for RainbowOF** if customers dispute quantities.

---

## 12. Rules to confirm with the business before coding

1. Does `AutoFulfill` create the order with no email, or only change the email wording?
2. Does a checkup “yes” insert `OrdersTbl` or only a temp row?
3. Which page generates the next recurring order, and on what day relative to prep?
4. Is `Register.aspx` reachable on production?
5. Are Vineyard/Buiten transfers still used, and do they represent stock or only a change of delivery person?
6. Is customer stock (kg) ever written back as a warehouse movement? Code says no.
7. Do payment terms or credit limit ever block delivery? Code says no.
8. Is `Pages/CustomerDetails.aspx` still in use, or only `ContactDetails.aspx`?
9. What does Leave App persist?
10. Live role names in `aspnet_Roles` versus the strings in code (`Agent Manager`, `AgentManager`, `Admin`, `repair`).
