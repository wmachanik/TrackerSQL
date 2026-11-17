# 10 — UI Modernisation (MudBlazor)

**Target:** Blazor Web App, MudBlazor, responsive, keyboard and screen-reader usable.  
**Do not port:** WebForms postbacks, `UpdatePanel`, `GridView` edit rows, `AjaxControlToolkit` tabs, table-based page layout, `alert()` as the only feedback (`showAppMessage` in `Site.Master`), magic-number grid columns (`GridColumnConstants`), or the 5-second loading overlay that hides a slow postback.

Legacy visual identity (green, QOnT wordmark, coffee-bean spinner) can remain as an optional theme for the Quaffee deployment. The component structure must not assume coffee.

---

## 1. Application shell

| Current | MudBlazor | UX change |
|---|---|---|
| `Site.Master` horizontal `asp:Menu` plus a separate mobile hamburger that duplicates links | `MudLayout` + `MudAppBar` + `MudDrawer` (temporary on small screens, clipped on desktop) | One nav tree. Current mobile menu omits orders list, recurring, prep summary, and admin; the drawer includes them by permission |
| Bottom tab bar (Home, Deliver, Order, Contacts, Checkup) | `MudBottomNavigation` or a persistent `MudNavGroup` of five actions, visible under 960 px for users with those permissions | Keep the five daily actions; do not make them the only navigation |
| Footer version | `MudText` caption | Same |
| Red error banner | `MudAlert` Severity.Error with actions | Link to the log, dismiss |
| Full-page coffee spinner on every load | `MudProgressLinear` on the layout during navigation, `MudOverlay` only for long jobs (import, invitation send) | Do not block the whole app for a filter change |
| `Print.master` | Browser print CSS on the wave and run views, plus “Download CSV” | Print still works for the bench; data also leaves as a file |

Portal shell: same layout components, different nav (Home, Details, Orders, Service, Standing orders, Sign out). No staff drawer items.

---

## 2. Components to standardise

| Need | Component | Notes |
|---|---|---|
| Lists of customers, orders, tasks, products | `MudDataGrid` with server data, column filters, pager | Replaces GridView. Virtualize large contact lists |
| Small lookup editors | `MudTable` inline or a dialog | Price levels, outcomes, faults |
| Create and edit | `MudDialog` for short records; full page for order and customer | Order lines are a grid inside the page, not a dialog maze |
| Filters | `MudForm` + `MudDatePicker` + `MudAutocomplete` | Date format `dd/MM/yyyy` displayed; store UTC or unspecified date with the business zone explicit (`TimeZoneUtils` equivalent) |
| Tabs on a customer | `MudTabs` | Replaces the single enormous Lookups page by splitting catalogues into routes |
| Charts on the home dashboard | `MudChart` | Counts only: due, unpacked, awaiting meter, open tasks. No fake revenue |
| Validation | `MudForm` with `DataAnnotations` | Show PO missing, qty, email, date order |
| Confirmations | `MudMessageBox` | Waybill duplicate, sooner-than-promise, disable customer |
| Empty states | `MudAlert` or `MudText` | “No stops this date” rather than an empty grid with no explanation |

---

## 3. Page-by-page guidance

### Daily staff

| Legacy | Current behaviour | MudBlazor | UX | Mobile | Accessibility |
|---|---|---|---|---|---|
| `Default.aspx` | Link cards | Dashboard: `MudGrid` of `MudPaper` stats and `MudButton` actions | Counts, not only links | Cards stack | Headings in order, buttons named |
| `Contacts.aspx` | Search grid | `MudDataGrid` server-side, `MudAutocomplete` jump | Saved filters in user preferences (QOnT has `UserPreferences`) | Search box first, row opens the record | Grid headers associated, focus moves to the record |
| `ContactDetails.aspx` | Long form | Page with `MudTabs`: Profile, Commercial, Locations, People, Preferences, Assets | Commercial tab permission-gated | One column | Labels on every input, errors on the field |
| `ContactsAway*` | List and detail | Grid plus `MudDialog` | Date range validation visible | Full width dialog | — |
| `SendCoffeeCheckup.aspx` | Prepare, tick, send | Wizard: `MudStepper` (window → review grid → send result) | Show skip reasons in a column (internal, email, cap, away) instead of only the log file | Stepper stacks | Live region for “sent N” |
| `SendClientMail.aspx` | Filter and HTML mail | Form + recipient grid + preview | Preview required before send | Preview below | — |
| `SentRemindersSheet.aspx` | History grid | `MudDataGrid` + CSV | — | — | — |
| `OrderDetail.aspx` | Header and lines, postback per line | One page: header `MudForm`, lines `MudDataGrid` edit, sticky save | Duplicate-order opens the existing order in a dialog with “Go there” | Lines as cards under 600 px | Qty announced when corrected from 0 to 1 |
| `OrderEntry.aspx` | Find orders | `MudDataGrid` date range, status chips | Chips: confirmed, packed, done, billed, promise exception | — | Colour plus text, not colour alone (packs today use colour) |
| `OrderDone.aspx` | Stock, cups, status, waybill | `MudDialog` or page: on-hand, meter, outcome, carrier | Warnings as `MudAlert` before confirm | Large inputs for the bench PC and the phone | — |
| `RecurringOrders.aspx` | List and detail | Grid + editor page | Enable switch with confirm when disabling (email will send) | — | — |
| `ItemsRequired.aspx`, `PreperationSummary.aspx`, `SummaryOFCoffeeRequired.aspx` | Print aggregates | One **Wave** page with `MudTabs` or a kind filter (all / consumable / summary) | One place, three views | Summary first on mobile | Print stylesheet |
| `DeliverySheet.aspx` | Run | **Run** page: date, worker, `MudDataGrid` or timeline | PO and document intent as `MudChip` with text | Card per stop | — |
| `Repairs.aspx`, `RepairDetail.aspx`, `RepairStatusChange.aspx` | List and job | Task grid + task page | Status change confirms when it emails | — | — |

### Delivery tools

| Legacy | MudBlazor direction |
|---|---|
| `Tools/Deliveries.aspx` hub | Drawer group “Delivery”, no extra hub page |
| `DriverTracking.aspx` | Map page. Admin only. Do not load the map on the run sheet by default |
| `DeliveryOrder.aspx` | Drag reorder (`MudDropContainer`) on the run |
| `DriverApp.aspx` settings | Admin form. APK link stays if the external app remains |
| `DeliveriesAwaitingDone.aspx` | Queue page with primary button “Complete” |
| `CustomerLocations.aspx`, `ContactLocation.aspx`, `DepotLocation.aspx` | Map pin editor on the location and on settings |
| `DeliveryPromiseRules.aspx` | Rule grid + dialog. Explain the window in local time |
| `HolidayClosures.aspx` | Calendar list |
| `MoveDeliveryDate.aspx` | Admin dialog: from date, to date, territory, preview count, confirm |
| `Lookups.aspx` | Split routes under Settings: Products, Packs, Territories, Workers, Carriers, Outcomes, Statuses, Terms, Price agreements, Document intents, Address kinds. Do not rebuild one page with 15 tabs |

### Channel, system, identity

| Legacy | Direction |
|---|---|
| `WooOrderImport.aspx` | Import page: fetch, exception grid, commit. Progress in the page, not a full-screen spinner only |
| `WooCommerceMapping.aspx` | Map workspace: unmapped first |
| `SystemPreferences.aspx` | Settings sections as routes |
| `QuaffeeLoversInvite.aspx` | Programme invite dialog |
| `PostalAreaSetup.aspx`, `ContactPostalFill.aspx` | Address tools under Settings |
| `MergeCustomersFromQB.aspx` | Import wizard with a dry run |
| `MessagesEditor.aspx` | Message grid, preview with sample tokens |
| `EmailDiagnostics.aspx` | Settings → Email test |
| `LogViewer.aspx` | Admin log grid (Serilog store or table) |
| `ApiTester.aspx` | Keep only if the driver API remains; otherwise use OpenAPI UI outside the product |
| `DatabaseBackup.aspx`, `XMLtoSQL.aspx`, `GenMachineKeys.aspx`, `AutoClassMaker`, `test/*` | Do not rebuild |
| `ManageUsers.aspx`, `ManageRoles.aspx`, `UserInformation.aspx` | Identity admin: user drawer, role multi-select, no grid postback |
| `LeaveApp.aspx` | Rebuild only after verifying it is still a business process. If it is a form that emails HR, a small `MudForm` is enough |
| `QuaffeeCoffeeTastingSheet.aspx` | Optional print template, not a platform screen |
| `OrderVineyard2Buiten.aspx` and the reverse | If still used: “Transfer to site” on the order. Not two named pages |

### Customer portal and public

| Legacy | Direction |
|---|---|
| Portal pages | Simple `MudList` and read-only grids, `MudDialog` for change requests and repeat-order |
| `ViewAs.aspx` | Admin-only, banner `MudAlert` Severity.Warning that stays on screen |
| `ViewMyOrder.aspx` | Public layout, no staff chrome, token required |
| `DisableClient.aspx` | Public `MudPaper`, two clear choices, plain language, success state without a second look at staff UI |
| Login pages | `MudCard` centered, show password policy, no register link in production |

### Driver

Prefer keeping Quaffee Driver (or its successor) as the offline client. If a Blazor screen is used on the van:

- One stop at a time, `MudStepper`, signature canvas, minimum 44 px targets.
- Offline is a product requirement. A MudBlazor page that only works online is not a replacement for `POST delivery/sync`.

---

## 4. Patterns to drop

| Obsolete pattern | Replacement |
|---|---|
| Edit row inside a GridView that posts the whole page | Grid dialog or inline edit with an explicit Save |
| Query-string customer name (`UrlParameterConstants` Name, CoName, EMail) as identity | Ids and a proper route `/customers/{id}` |
| Session temp order (`TempOrdersHeaderTbl`) | Draft order row or component state with a draft id |
| `alert()` | Snackbar and dialog |
| Colour-only pack identification | Symbol and text as well (pack symbol already exists; do not rely on `BGColour` alone) |
| Hardcoded admin menu items shown to everyone | Drawer built from permissions |
| Duplicate mobile and desktop menus maintained by hand | One `NavMenu` component |
| Page names spelled Preperation | “Preparation” in the new UI. Do not rename the legacy URL until cutover |

---

## 5. Responsive and accessible baseline

- Breakpoints: phone (run card list, checkup stepper), tablet (grids with fewer columns), desktop (full grids).
- Touch targets on run and complete screens at least 44 px.
- Every icon button has `aria-label` (the current home image buttons are ambiguous).
- Status is text plus colour.
- Dates are pickers, not free text, still shown as day/month/year.
- Focus trap in dialogs (MudDialog default) and return focus to the invoking button.
- Do not auto-play motion. The coffee spinner does not carry over as a required asset.
- Language of customer-facing portal and emails stays editable in the message catalogue (English today).

---

## 6. UX improvements that change the work, not only the skin

1. Show **why** a customer was skipped on the invitation review, not only after a failed send log.
2. When a duplicate same-day order exists, navigate to it instead of an error string.
3. Show promise date and prepare date together on the order header.
4. Require a preview count before bulk date move.
5. Keep the user on the run sheet after completing one stop.
6. Dashboard numbers that match the queues, so “awaiting meter” is not a buried tool.
7. Split Lookups so a driver supervisor is not scrolling through price factors.

These are in the functional spec as Improve items. They are safe because they do not change the underlying rules in `docs/04`.
