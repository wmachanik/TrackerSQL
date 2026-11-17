# 06 — Reporting Inventory

QOnT does not use SQL Server Reporting Services, a dashboard product, or a shared Excel library. Operational reporting is **pages that aggregate orders**, **emails**, **one PDF (proof of delivery)**, and **the application log**. No `SqlCommand` with a scheduled job runner was found. **Verify** Windows Task Scheduler and SQL Agent on the host before declaring there are zero scheduled outputs.

---

## 1. Operational reports (screens)

### 1.1 Items required — `/Pages/ItemsRequired.aspx`

- **Audience:** Roastery / packers.
- **Grain:** Open order lines for the chosen preparation date.
- **Fields (from `Models/ItemsRequiredSummary.cs` and the page’s purpose):** item, preparation type, packaging, quantity, customer, area. Exact grid columns must be read off the aspx during UI build; the business content is “what to make”.
- **Calculations:** Sum of `QtyOrdered` for lines whose header is not done and whose prep or required date matches the run. Grouping is by item and pack so production sees totals, with a customer breakdown so the sheet can be packed per drop. **Verify** whether done and invoice-done orders are excluded in `DeliveryItemsRepository` or the page query.
- **Output:** Browser print (`Print.master`). No file export found.

### 1.2 Preparation summary — `/Pages/PreperationSummary.aspx`

- **Audience:** Supervisor.
- **Model:** `PreparationSummaryItem` via `PreparationSummaryRepository`.
- **Purpose:** Shorter rollup of the same demand (counts and packs) so the day can be staffed.
- **Output:** Print.

### 1.3 Summary of coffee required — `/Pages/SummaryOFCoffeeRequired.aspx`

- **Audience:** Roast lead.
- **Purpose:** Coffee service-type quantities only, so a roast batch can be sized. Non-coffee lines (filters, cleans) stay on the other sheets.
- **Calculation:** Filter lines to service type Coffee (`ServiceTypeConstants.Coffee = 2`) or to items that set the daily average. **Verify** the page’s WHERE clause; do not assume it uses the constant if the SQL filters on a description.
- **Output:** Print.

### 1.4 Delivery sheet — `/Pages/DeliverySheet.aspx`

- **Audience:** Drivers and the office.
- **Model:** `DeliverySheetModels`, `DeliverySheetManager`.
- **Fields:** Customer, deliver-to if set, area, required date, driver, lines (item, qty, prep, pack), notes, PO warning, invoice-type pill (`InvoiceTypesTbl.PillLabel`), recurring-order flag, packed/done/confirmed, walk-in name parsed from notes.
- **Calculations:** Sequence from `DeliveryRouteTbl` if the driver saved one, otherwise area delivery order and route planner. Not a financial total.
- **Output:** Screen and print. The driver app is a second rendering of the same orders (`GET /api/v1/delivery/sheet`), not a different dataset.

### 1.5 Order sheet — `/Pages/OrderSheet.aspx`

- **Suspected:** Printable single-order or daily order list. **Verify** whether staff still open it or only the delivery sheet.

### 1.6 This week’s order — `/Pages/ThisWeeksOrder.aspx`

- **Suspected:** Week board. **Verify** before promising it in RainbowOF. If it only repeats the delivery sheet for seven days, replace it with a date-range grid.

### 1.7 Sent reminders — `/Pages/SentRemindersSheet.aspx`

- **Audience:** Person who runs checkups.
- **Table:** `SentRemindersLogTbl`.
- **Fields:** Contact, when sent, success or failure, order type, and the message context stored by `CoffeeCheckupManager.LogReminderAttempt`.
- **Calculation:** None beyond filters. Reminder count on the contact is a separate running total (`ReminderCount`), capped at 10.
- **Output:** Screen. Print if the page includes it.

### 1.8 Deliveries awaiting done — `/Tools/DeliveriesAwaitingDone.aspx`

- **Audience:** Office after the vans return.
- **Population:** Proofs or app syncs whose `OrdersTbl.Done` is still false.
- **Fields:** Order, customer, date, link `?OrderID=`.
- **Action:** Complete Now opens Order Done. This is an exception queue, not a historical report.
- **Why it matters:** Until cup count is posted, the forecast does not move.

### 1.9 Driver tracking — `/Tools/DriverTracking.aspx`

- **Audience:** Admin (`DriverTrackingData.ashx` requires Administrators or Admin).
- **Data:** `DriverLocationPingTbl` plus delivery positions from `DriverTrackingManager`.
- **Fields:** Person, time, lat, long, accuracy, source (`APP_OPEN` or `PERIODIC`).
- **Rules that change the data:** Points outside the tracking window are not stored; future or day-old points are skipped; the first ping of the day beyond 300 m from the depot is also written to the delivery log as a start event.
- **Output:** Map. Not an export.

### 1.10 Prediction bulk recalc result

- **Audience:** Admin repairing forecasts.
- **Producer:** `PredictionManager.RecalculateBulk`.
- **Fields per contact:** company, last coffee date, last filter date, previous and new next-coffee, next-filter, next-clean, next-descale, next-service, previous and new daily consumption, result (`Changed`, `Unchanged`, `Failed`, `Error: …`).
- **Sort:** Changed first, then errors, then failures, then unchanged; then company name.
- **Output:** On whatever admin screen calls it. **Verify** the page (system tools or contact maintenance). It does not email.

### 1.11 Log viewer — `/Tools/LogViewer.aspx` and `/Pages/LogTable.aspx`

- **Table:** `LogTbl`.
- **Fields:** Time, category (`SystemConstants.LogTypes`: Orders, SendCheckup, System, and others in that class), message.
- **Retention:** `UIConstants` does not define DB retention. File timing log is optional (`EnableRequestTiming`). API log retention is 30 days (`MobileApi.LogDays`) in `MobileApiRequestLogTbl`, viewed from `Tools/ApiTester.aspx`, not the log viewer.

### 1.12 Client mail send log

- **Table:** `ClientMailSendLogTbl`.
- **Fields:** Who was targeted, template, when, success. **Verify** columns on `Models/ClientMailModels.cs`.
- **Output:** The send page itself.

### 1.13 QuickBooks merge preview — `/Tools/MergeCustomersFromQB.aspx`

- **Audience:** Admin.
- **Input:** Customer extract from QuickBooks (**verify** CSV columns in the page).
- **Output:** Match and merge actions, not a financial report.
- **Fields:** Names, emails, and the QOnT contact chosen as the survivor. Treat unmatched rows as a work list.

### 1.14 Woo import preview — `/Tools/WooOrderImport.aspx`

- **Model:** `WooOrderImportPreview`.
- **Fields:** Woo order id, customer, lines after mapping, errors (unmapped SKU, unmapped shipping method).
- **This is the channel exception report.** Unmapped rows are lost revenue if ignored.

### 1.15 API tester log and devices

- **Audience:** Admin.
- **Fields:** Request time, user, path, status, duration, bytes, IP, short note. No body, token, or signature.
- **Devices:** Token id, user, person, device name, app version, created, last used, expires, revoked.

### 1.16 Home dashboard — `/Default.aspx`

- **Not a data dashboard.** It is a menu of cards. RainbowOF should replace it with real counts: orders due today, checkups not sent, proofs awaiting cup count, repairs in a status, unmapped Woo orders. Those counts do not exist as a query layer today.

### 1.17 Quaffee coffee tasting sheet — `/Pages/QuaffeeCoffeeTastingSheet.aspx`

- **Coffee-specific form.** **Verify** whether it writes rows or only prints a blank sheet. If it prints only, it is a template, not a report.

### 1.18 Leave application — `/Pages/LeaveApp.aspx`

- **Internal form.** **Verify** output (email vs table). Not a customer report.

---

## 2. Emails (the real scheduled-looking outputs)

All are user-triggered unless noted. There is no background `Timer` in `Global.asax` that sends the daily checkup. If the business “always sends at 8:00”, a person opens the page.

| Email | Trigger | Template keys | Data included | Recipient |
|---|---|---|---|---|
| Coffee checkup | Send Coffee Checkup | `SendCheckupEmailTextsTbl` plus message keys | Predicted items, quantities, response links | Contact email |
| Order status | Order Done or explicit send | `MessageKeys` order/email section | Status, items, optional tracking URL | Contact, if `SendDeliveryConfirmation` |
| Waybill updated | `SendWaybillUpdatedEmail` / `UpdateWaybillAndNotify` | Waybill keys | Courier, number, link | Contact |
| Proof of delivery | After sync when POD email is on | `ProofOfDeliveryManager` | PDF: details, lines, terms, signature | Contact; copy behaviour **verify** |
| Driver note to office | Sync flag `noteToOffice` | `MobileDeliveryEmailManager` | Note text | Office address in driver-app settings |
| Recurring add/update/disable | Save recurring order | `MessageKeys` recurring | What changed | Contact (`NotifyContactOnChange`) |
| Away confirmation | Save away period | `MessageKeys.AwayPeriod` | Dates | Contact |
| Disable / goodbye | `DisableClient.aspx` | `MessageKeys.DisableClient` | What was turned off | Contact |
| Disable admin alert | Same | Admin subject and body | Choice, whether recurring orders remain | Orders mailbox (`SysCCEmailAddress` / orders mailbox) |
| Client mail | Send Client Mail | `ClientMailTemplatesTbl`, `MessageKeys.ClientMail` | Free HTML | Filtered contacts |
| Quaffee Lovers invite | Invite tool | `QuaffeeLoversEmailManager` | How to join | Contact |
| Portal change notice | If `RequestChanges.NotifyEmail` | **Verify** body | Requested fields | Office |
| Password recovery | Recover password | Membership provider | Reset link | User email |
| Tracking welcome | **Verify** when sent | `MessageKeys.Customer.TrackingWelcome*` | Portal or tracking intro | Contact |

**Test mode:** `EmailTestMode=true` sends to `EmailTestRecipient` instead of customers. Production must be false. Do not document live mailbox passwords.

**From address:** `SysEmailFrom` (orders mailbox). SMTP Office 365 port 587 STARTTLS (`EmailMailKitCls`).

---

## 3. PDF

| PDF | Builder | Fields | Calculations |
|---|---|---|---|
| Proof of delivery | `Classes/SimplePdf.cs`, `ProofOfDeliveryManager` | Customer, date, lines and quantities handed over, received-by, outcome, terms wording, signature image | No prices. Short quantities come from the driver’s line list |
| Access | `Handlers/ProofOfDelivery.ashx?id=` | Staff: any proof. Portal contact: only own delivered proofs, else 404 | `ContactPortalManager.CanViewProof` |

No invoice PDF, statement PDF, or picking-list PDF generator was found. Picking is the printed HTML sheet.

---

## 4. CSV and Excel

No shared CSV or Excel exporter exists in `Managers` or `Classes` (no ClosedXML/EPPlus reference was required to explain the design; **verify** `TrackerSQL.csproj` references if a grid button is discovered in UAT).

Known file exchange:

| Direction | What | Format |
|---|---|---|
| In | WooCommerce | JSON REST, not a file |
| In | QuickBooks merge | **Verify** in `MergeCustomersFromQB.aspx.cs` (likely CSV or tab-delimited export) |
| In | Schema | XML SQL command files |
| Out | Database backup | SQL `.bak` via `Tools/DatabaseBackup.aspx` |
| Out | Driver install | APK under `Downloads/` |
| Out | Email debug | `.eml` in `App_Data` only when logging is on |

RainbowOF should add explicit exports (delivery sheet CSV, items-required CSV, reminder log CSV) because print-to-PDF is the only office archive today.

---

## 5. Dashboards

| Surface | What it shows | What it does not show |
|---|---|---|
| `Default.aspx` | Links | Counts, trends, revenue |
| Driver tracking | Map | On-time percentage |
| API tester | Call log | Business KPIs |
| Portal home | Links | Usage or next delivery as a chart (**verify** `Portal/Home.aspx`) |

**Suggested RainbowOF dashboard measures**, all derivable from tables that exist:

- Orders required today, not done, not packed.
- Deliveries with proof and `Done = 0`.
- Contacts with `NextCoffeeBy` inside the reminder window and no reminder log this cycle.
- Repairs whose status is not done.
- Woo orders pulled but unmapped.
- Contacts with `RequiresPurchOrder` and a blank `PurchaseOrder` on an open order.

Do not invent revenue KPIs until a price exists on the line or an accounting feed is in scope.

---

## 6. Calculations worth preserving even without a report

| Calculation | Formula in code | Used by |
|---|---|---|
| Cups in leftover customer stock | `round(kg × 100)` subtracted from cup count | Order Done |
| Daily average | Rolling up to 6 usage points, fallback 5.0 | Forecast |
| Next date when a daily rate exists | `round(lastQty × 100 / itemAverage)` days for the item that **sets** the daily average (per-day branch) | `PredictionManager.ApplyTrackedItem` |
| Next date for maintenance | `round(lastQty × itemAverage / dailyAverage)` days, or delivery cadence if present, then holiday extension | Same |
| Fallback new customer | Next coffee = today + 20 days; clean +20; filter and descale +30; service +1 year | `UpdatePredictions` |
| Reminder window | Configured days, clamped 5–30, default 9 | Checkup |
| Short recurring suppression | Cycle &lt; 20 days skips checkup | Checkup |
| Sooner-than-promise | Required date &lt; promised date, ordered today, not courier, not Dispatch kind | Delivery promise |
| Promise date | Fixed weekday, same workday, or next workday, with weekend skip and Wednesday-afternoon courier adjustment | `DeliveryPromiseManager` |
| Pricing factor | Stored, **not applied** | Do not show a money column until RainbowOF defines price |

---

## 7. Human verification checklist

1. Grid export buttons on Contacts, Order Entry, Repairs, Lookups.
2. SQL Agent jobs and Windows tasks on the live host.
3. Whether tasting sheet and leave form write tables.
4. Column list of `SentRemindersLogTbl` and `ClientMailSendLogTbl` from `sys.columns`.
5. Whether the office archives printed sheets or only the email sent folder.
