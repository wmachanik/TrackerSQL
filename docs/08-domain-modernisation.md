# 08 — Domain Modernisation

QOnT is a coffee roaster’s operations system that has already started to generalise names (`Customer` → `Contact`, `Roast` → `Prep`, `Machine` → `Equipment` in the SQL migration notes). The behaviour is still coffee-shaped. RainbowOF must keep the behaviour and drop the assumption that every product is a kilogram of coffee and every asset is an espresso machine.

---

## 1. Coffee-specific concepts and their generic meaning

| In QOnT | Evidence | Generic concept |
|---|---|---|
| Coffee item / SKU | `ItemsTbl`, service type Coffee = 2 | **Product** (stocked or made-to-order) |
| Prep type | `ItemPrepTypesTbl`, `PrepTypeID` on the line | **Product preparation** or variant (roast profile, grind, pack-out). For tea: leaf grade. For medical: sterile vs bulk. For office: none |
| Packaging | `ItemPackagingsTbl`, colours and symbols | **Pack configuration** (250 g, 1 kg, case of 10, each) |
| Green coffee only contact type (id 8) | `CustomerTypeConstants.GreenCoffeeOnly` | **Supply-only / unprocessed goods** customer. No asset, no service cadence |
| Cups per kg = 100 | `TypicalCupsPerKg` | **Consumption unit conversion**. Domain data: 1 sales unit = N usage units. Coffee: kg → cups. Water: bottle → litres. Medical: box → each. Do not hardcode 100 |
| Cup count on the machine | Order Done, `TotalCountTrackerTbl` | **Meter reading** on an asset. Optional. Coffee machines have counters; a water cooler may have a bottle count; many products have neither |
| Customer stock kg | Order Done stock text | **Customer on-hand** in sales units, subtracted from consumption so leftovers are not “used” |
| Daily consumption | `DailyConsumption`, typical 5 | **Usage rate** per day in usage units |
| Next coffee by | `NextCoffeeBy` | **Next replenishment date** for the primary consumable |
| Clean, descale, filter, service dates | Forecast fields, service types 1, 4, 5, 10 | **Asset maintenance tasks** with their own average life |
| Filter SKUs and `UsesFilter` | `ItemConstants`, `Contact.UsesFilter` | **Consumable linked to an asset** (filter, gasket, reagent) |
| Item groups / next blend | `ItemGroupsTbl`, service type 21 | **Product cycle** or assortment rotation inside a subscription |
| Swop collect / start / stop / return | Service types 6–9 | **Asset exchange** (install, remove, loan, return) |
| Holiday service types 11–16 | One week through two months | **Absence** (prefer `CustomerAbsence`, already partly modelled as away periods) |
| Equipment type and `EquipentSN` | One serial on the contact | **Asset** (many per site) |
| Repair, fault, status email | `RepairsTbl` | **Service task** |
| Area prep weekday and delay | `AreaPrepDaysTbl` | **Fulfilment calendar** for a territory |
| Delivery promise rules | `DeliveryPromiseRuleTbl` | **Customer promise policy** (SLA), separate from the production calendar |
| Roast / preparation summary | Prep pages | **Production batch** or pick wave |
| Vineyard and Buiten pages | Two sites | **Site / warehouse** transfer |
| Quaffee Lovers | Grants, Woo role `member` | **Customer programme** (loyalty, contract tier). Price effect is external today |
| Contact types 1–7 and 9 | Contract, rental, outright, service-only, info-only | **Commercial relationship**, not a product family |
| ZZName contact 9 | Walk-in | **Anonymous party** on an order (guest checkout) |
| Driver collection-only contacts | `DriverCollectionOnly` | **Pickup stop** (supplier or customer return point) |
| People as drivers | `PeopleTbl` | **Worker** with a fulfilment role |
| Courier vs van | Delivery type and courier table | **Fulfilment mode**: own route, collection, third-party carrier |
| Waybill | `OrderWaybillTbl` | **Carrier consignment** |
| Invoice type pill | Delivery note vs dispatch note | **Document intent** on a fulfilment (goods on loan, transfer, sale) |
| PO required | Customer flag | **Customer fulfilment constraint** |
| Price level factor | Unused in the order total | **Price agreement** (future) |
| South African postal table | `SaPostalCodeTbl` | **Address validation provider**, pluggable by country |
| Checkup email | Coffee checkup | **Replenishment invitation** |
| Recurring order | Standing basket | **Standing order** or **subscription schedule** |
| Cups, beans, roast dates in old docs | Historical column names `PrepDate` | **Activity date** (produced on, shipped on, consumed on). Keep three dates, which QOnT already has: ordered, prepared, required |

---

## 2. Reusable domain model

The names below are the RainbowOF language. QOnT tables that feed them are in parentheses.

### Party and place

- **Customer** — legal or trading account (`ContactsTbl` + `ContactsAccInfoTbl`). Flags: enabled, prediction disabled, auto-fulfil, always invite, normally responds, requires purchase order, send fulfilment confirmation, reminder count.
- **CustomerLocation** — a physical site (`ContactsTbl` address + `CustomerLocationTbl` + each `ContactDeliverToAddressTbl`). One location is primary. Each has area, GPS, and “counts toward consumption”.
- **CustomerContact** — a person at the account (today flattened as contact first/last and alt first/last). Many people, one portal user each.
- **CustomerAbsence** — (`ContactsAwayPeriodTbl`).
- **CommercialRelationship** — (`ContactTypesTbl`): subscription-and-service, service-contract, product-only, rental, outright asset sale, service-only, information-only, unprocessed-supply, collection-point.
- **PriceAgreement** — (`PriceLevelsTbl` + terms). Factor only, until real prices exist.
- **CustomerProgramme** — (`QuaffeeLoversGrantTbl`): code, status, external role.

### Catalogue

- **Product** — (`ItemsTbl`): sku, name, enabled, replacement product, unit, sort.
- **ProductCategory** — new. QOnT has service type, not a merchandising tree. Woo categories are a channel filter (`WooCategoryFilterTbl`), not the internal tree.
- **ProductKind** — from `ItemServiceTypesTbl`, generalised: consumable, preparation-variant parent, maintenance, meter, asset-exchange, note, group-header, information.
- **Preparation** — (`ItemPrepTypesTbl`) optional per product.
- **Pack** — (`ItemPackagingsTbl`).
- **UnitOfMeasure** and **UnitConversion** — (`ItemUnitsTbl`, cups-per-kg).
- **ProductCycle** and **ProductCycleStep** — (`ItemGroupsTbl`).
- **CustomerCyclePosition** — (`UsedItemGroupsTbl`).

### Assets and service

- **Asset** — equipment type + serial + location + install date. Today one serial on the customer.
- **AssetMeter** — (`TotalCountTrackerTbl`).
- **ServicePlan** — (`TrackedServiceItemsTbl`): which product kind drives usage rate, typical average, which maintenance kinds depend on that rate.
- **ServiceSchedule** — the forecast row, normalised: one row per customer (or asset) per plan line, instead of `NextCoffeeBy`, `NextCleanOn`, and so on as columns.
- **ServiceTask** — (`RepairsTbl`): fault, status, email-on-status, related fulfilment.
- **AssetExchange** — swop service types, as task types on `ServiceTask` or as fulfilment lines.

### Demand and fulfilment

- **Order** — (`OrdersTbl`): customer, location, ordered-at, prepare-on, required-on, worker, document intent, customer PO, notes, guest identity, status (confirmed, packed, done, invoiced).
- **OrderLine** — product, qty, preparation, pack. Price and tax **nullable** until billing is in scope.
- **StandingOrder** and **StandingOrderLine** — (`RecurringOrdersTbl`).
- **PurchaseCycle** — (`RecurringTypesTbl`): interval, not coffee-specific.
- **Subscription** — a standing order plus a price agreement plus a programme. QOnT has the first and a hint of the others. Do not force every order to be a subscription.
- **ReplenishmentInvitation** — checkup run and `SentRemindersLogTbl`.
- **FulfilmentWave** — items-required / prep summary for a prepare-on date and site.
- **Delivery** — the driver’s stop: order or a group of orders for one location on one date, outcome, received-by, signature, quantities handed over.
- **CarrierConsignment** — waybill.
- **Territory** — (`AreasTbl`).
- **TerritoryCalendar** — (`AreaPrepDaysTbl`).
- **PromisePolicy** — (`DeliveryPromiseRuleTbl`).
- **Closure** — (`HolidayClosuresTbl`).
- **Warehouse** / **Site** — not a table today. Introduce for Vineyard/Buiten and for future stock. QOnT can migrate with a single default site.
- **InventoryItem** — **new and optional**. Do not fake balances from order history. Customer on-hand is not warehouse inventory.
- **Transfer** — the Vineyard/Buiten pages, if still used.

### Channel and identity

- **ChannelAccount** — (`WooContactInfoTbl`, later other shops).
- **ChannelOrder** — (`WooOrderInfoTbl`).
- **ChannelMap** — item, attribute, shipping, payment, postal.
- **Worker** — (`PeopleTbl`) with fulfilment mode and link to the identity user.
- **IdentityUser** — ASP.NET Identity, replacing membership.

### Billing (explicitly thin)

- **Invoice** — not in QOnT. Add as an integration document: external id, status, link to orders. Do not block go-live on a general ledger.
- **Contract** — `Contact.ContractNo` is a string. Promote to **Contract** when more than a reference is required (start, end, relationship, included products).

---

## 3. Worked examples (same model, different industries)

| RainbowOF concept | Coffee (today) | Tea | Water | Medical supplies | Office consumables |
|---|---|---|---|---|---|
| Product | 1 kg blend | 200 g tin | 19 L bottle | Glove box | A4 paper |
| Preparation | Roast profile | None | Still vs sparkling | None | None |
| Pack | Retail bag, colour | Tin | Crate | Case | Ream |
| Usage unit | Cup | Serving | Litre | Each | Sheet |
| Meter | Machine cup count | None | Cooler bottle count | None | None |
| Maintenance | Clean, descale, filter, service | None | Sanitise, filter | Calibration | None |
| Asset | Espresso machine | None | Cooler | Device under contract | Printer (optional) |
| Standing order | Weekly coffee | Monthly tea | Fortnightly bottles | Ward top-up | Monthly stationery |
| Invitation | Checkup email | Same | Same | Same | Same |
| Pickup stop | Green-bean supplier | Garden | Bottler | Wholesaler | — |

Hospitality is a **customer segment** (many locations, delivery on behalf, PO required), not a product type. Retail products are products with a pack and no asset. Industrial supplies are products plus optional assets and meters.

---

## 4. Improvements over QOnT (behaviour preserved, model cleaned)

1. **Many assets per location.** One serial on the contact cannot represent two machines or a machine plus a grinder.
2. **Forecast as rows, not columns.** New maintenance kinds (calibration, UV lamp) must not be a schema change.
3. **Guest customer** as an order party, not a magic contact id.
4. **Customer on-hand** as its own field with a declared unit, not a free-text “stock” that is assumed to be kilograms.
5. **Conversion factors as data.** Cups per kg is a `UnitConversion` for that product family.
6. **Document intent and fulfilment mode as lookups,** including the current ids 5, 7 and 10 only as seed data.
7. **Warehouse optional.** Ship the first release without stock balances so coffee operations do not change, and add `InventoryItem` when a second vertical needs it.
8. **Price on the line optional.** When empty, screens hide money. When a price agreement exists, the line can snapshot unit price. That is new, and it must not be required to complete a delivery.
9. **Service task separate from replenishment order,** with an optional link, as repairs already are.
10. **Channel as a plugin.** Woo maps stay; the core order does not mention Woo.
11. **Country-specific address services** behind an interface. `SaPostalCodeTbl` is one adapter.
12. **Product categories** for navigation and reporting, distinct from product kind (consumable vs maintenance).
13. **Audit of order changes,** which QOnT lacks.
14. **Retire duplicate pages and dual table names** listed in `docs/03` and `docs/05`.

---

## 5. What not to generalise away

These look coffee-specific and are actually the product:

- Proactive replenishment from observed consumption, including leftover on-hand.
- Separate production calendar and customer promise.
- One drop per customer per day, with line moves to merge drops.
- Proof with signature, short quantities, and a queued office completion when a meter reading is still required.
- Standing orders that must not double-invite the customer.
- Status communication (email on repair status, email on delivery, email on standing-order change).
- A way for the customer to stop the invitations without a developer.

If a future category has no meter and no maintenance, those steps are skipped because the service plan is empty, not because a different application is deployed.

---

## 6. Mapping checklist for ETL

| QOnT | RainbowOF | Note |
|---|---|---|
| `ContactsTbl` | `Customer` + primary `CustomerLocation` + primary `CustomerContact` | Split the flattened names |
| Contact id 9 | One `Customer` with `IsGuestBucket` or guest fields on the order only | Stop overloading id 9 |
| `EquipentSN` + `EquipTypeID` | One `Asset` | Install date from usage repository |
| Forecast columns | `ServiceSchedule` rows | Use `TrackedServiceItemsTbl` as the plan |
| `OrdersTbl` / lines | `Order` / `OrderLine` | Keep external Woo ids |
| `InvoiceDone` | `Order.BillingStatus` | Not an invoice |
| `PeopleTbl` | `Worker` | Link to identity |
| `AreasTbl` | `Territory` | |
| Kg stock and cup count | `MeterReading` and `CustomerOnHand` | Store the conversion used, so history stays explainable if the factor changes |

**Assumption:** QuickBooks remains the accounting system of record at cutover. RainbowOF stores external invoice ids when they exist and does not re-implement VAT invoices in the first release unless the business says otherwise.
