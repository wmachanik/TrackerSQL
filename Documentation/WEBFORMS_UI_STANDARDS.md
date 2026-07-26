# WebForms UI Standards

**Date:** 2026-07-14  
**Last Updated:** 2026-07-21  
**Status:** ACTIVE — required for all new/changed pages; retrofit existing pages when touched  

**Reference implementations:**

| Concern | Canonical page |
|--------|----------------|
| UpdatePanel / Save / Save & Return / Back / status strip | `Tools/HolidayClosureDetail.aspx` |
| **Page tone panel** (accent bar + single header icon + toolbar inside panel) | `Pages/SentRemindersSheet.aspx` |
| **Unsaved-changes guard** (`TrackerUnsaved.init`) | `Pages/ContactDetails.aspx` (+ `Scripts/unsavedChanges.js`) |
| **Icon + label green button** (`span.image-button`) | `Pages/DeliverySheet.aspx` (calendar); `Pages/RecurringOrders.aspx` (Edit/Delete) |
| **Grouped list** (`grouping-table` / `grouping-card`) | `Pages/RecurringOrders.aspx` |

CSS base: `.page-tone-panel` + tone modifier in `Styles/Site.css` (colours match `Default.aspx` `home-tone-*` cards).

**Cache-bust:** any edit to `Styles/Site.css` must bump `Rev: YYYYMMDD-n` in the CSS header **and** `Site.Master` `?v=YYYYMMDD-n` (same-day changes increment `n`).

---

## 1. Status messages — bottom of the form

Project-wide: **user-facing status / result messages go at the bottom of the main form panel**, after action buttons — not above the fields.

Prefer **names** (contact/company, item, status) over raw IDs in status text. IDs may stay on the form for operators (e.g. small “ID:” label) but messages like “Loaded …” / “Updated …” should say who/what, not `#123`.

Use the shared classes:

- `status-message` (base)
- `status-message status-info`
- `status-message status-success`
- `status-message status-error`
- `status-message status-warn` (when needed)

```aspx
<%-- After the button row, still inside the main panel --%>
<div class="status-message" id="pnlStatus" runat="server">
    <asp:Literal ID="ltrlStatus" runat="server" />
</div>
```

Progress wait UI (`UpdateProgress` / Quaffee spinner) stays **above** the UpdatePanel content (or beside the title area) so it is visible as soon as the async postback starts. That is separate from the result status strip.

---

## 2. UpdatePanel is the default interaction model

From now on, interactive tools and detail pages should use **ScriptManager + UpdatePanel + UpdateProgress**, matching Holiday Closure Detail:

| Concern | Standard |
|--------|----------|
| Async work (Save, Filter, refresh) | `AsyncPostBackTrigger` / `RegisterAsyncPostBackControl` so **UpdateProgress** can show |
| Navigating away (Save & Return, Back, Delete→list) | `PostBackTrigger` / `RegisterPostBackControl` so `Response.Redirect` is reliable |
| Progress indicator | `UpdateProgress` with `AssociatedUpdatePanelID`, prefer `DisplayAfter="0"`, Quaffee / BlueArrows anim under `images/animi/` |
| Panel update mode | Prefer `UpdateMode="Conditional"` and call `.Update()` when needed after async handlers |

### Canonical button set (detail forms)

Where applicable:

1. **Save** — stay on page (async)
2. **Save & Return** — save then redirect (full postback); default return target is the owning list page
3. **Delete** — when editing an existing row (full postback if it redirects)
4. **Back** — real `<asp:Button>`, not a HyperLink; return without saving

Default return for Holiday Closure Detail: `~/Tools/HolidayClosures.aspx`. Other detail pages use their owning list as the default return URL.

For pages opened from **many** callers (e.g. `ContactDetails`), prefer **return to referrer**: capture `UrlReferrer` (or `?ReturnUrl=`) on first load into Session, and use that for **Save & Return** / **Back**, with the owning list as fallback. Same-site URLs only (no open redirects). Pattern: `Pages/ContactDetails.aspx.cs` / `OrderDetail.aspx.cs`.

---

## 3. Panels and icons

- **Do not** put a separate `<h1>` above the form panel — that duplicates the home-card icon and leaves the header outside the accent bar.
- Use one **`page-tone-panel`** wrapping the whole page content: title, filter/toolbar, form, grid, and status.
- Structure inside the panel:
  1. **`page-tone-header tool-card-header`** — one icon from `images/imgButtons/` + **`page-tone-title`** (h1) + optional **`page-tone-subtitle`** (matches home dashboard card copy).
  2. **`page-tone-toolbar button-row`** — date filter, Refresh, Back, etc.
  3. Main content (grid, fields, status at bottom).
- **Page tone** class on the same panel (left accent bar + soft gradient), matching the home dashboard card for that menu item:
  - Reminder History → `page-tone-panel page-tone-reminders` (same colours as `home-tone-reminders` on `Default.aspx`)
  - Contacts / Contact Details → `page-tone-panel page-tone-contacts` (`home-tone-contacts`)
  - Send Checkup → `page-tone-panel page-tone-checkup` (`home-tone-checkup`)
  - Repairs / Repair Detail → `page-tone-panel page-tone-repairs` (`home-tone-repairs`)
  - Order Done → `page-tone-panel page-tone-orders` (`home-tone-orders`)
  - Order Detail (existing) → `page-tone-panel page-tone-orders`; New Order → `page-tone-panel page-tone-neworder` (`home-tone-neworder`)
  - Delivery Sheet (screen) → `page-tone-panel page-tone-delivery` (`home-tone-delivery`); print view (`?Print=Y` / `Print.master`) stays plain
  - Weekly Summary → `page-tone-panel page-tone-summary` (`home-tone-summary`); page file may still be `PreperationSummary.aspx`
  - Recurring Orders / Recurring Order Details → `page-tone-panel page-tone-recurring` (`home-tone-recurring`)
  - Lookups → `page-tone-panel page-tone-lookups` (`home-tone-lookups`)
  - Item Groups / Group Item Detail → `page-tone-panel page-tone-groups` (`home-tone-groups`)
  - Holiday Closure Detail → `closure-detail-panel` (legacy; migrate to `page-tone-*` when touched)
  - System Tools pages → `tool-tone-*` / `xmltosql-panel` etc.
  - Account / Administration (User menu) → `page-tone-panel page-tone-users`
- Buttons use the same button classes as other forms (`filter-panel-btn` / `.button-row`); **Back must be a Button**, not a styled HyperLink.

Reference layouts: `Pages/SentRemindersSheet.aspx` (list shell), `Pages/Contacts.aspx` + `Pages/ContactDetails.aspx` (linked list/detail pair), `Pages/RecurringOrders.aspx` (grouped list + image-button).

---

## 3a. Reusable control styles (prefer these — do not invent page-prefixed copies)

Defined once in `Styles/Site.css`. Use on any page that needs the same UI.

### `image-button` — green icon + optional label

Shell carries the chrome; icon and text link are **siblings** (so `.results-table a { display:table-cell }` does not stack icon above text).

```aspx
<span class="image-button" title="Delete">
    <img src="../images/imgButtons/DelItem.gif" alt="" />
    <asp:LinkButton Text="Delete" ... />
</span>
```

| Class | Use |
|--------|-----|
| `span.image-button` | Labeled or icon-only green control |
| `image-button-end` | Optional — push to the right of a flex toolbar (`margin-left: auto`) |
| Icon-only | `<span class="image-button"><asp:ImageButton ... /></span>` (e.g. DeliverySheet calendar) |

Do **not** put `image-button` only on a nested table link with an inner `<img>` — table link CSS breaks layout.

### Grouped list — header card + nested grid

| Class | Use |
|--------|-----|
| `grouping-table` | Outer GridView (`results-table grouping-table`) — transparent rows hosting cards |
| `grouping-card` | One group block (bordered card) |
| `grouping-card-header` / `-main` / `-meta` | Title row: actions + name left; meta (status, counts) right |
| `grouping-card-title-link` / `-label` | Group title text |
| `grouping-card-count` | e.g. “3 line(s)” |
| `status-badge` + `is-enabled` / `is-disabled` / `is-done` | Compact status chip (`is-enabled` = green/do-this; `is-done` = orange/completed) |
| `nested-results-table` | Detail GridView inside a card (chrome only) |
| `recurring-summary-grid` | Recurring Orders nested list — full width, fixed % columns (`col-ro-sum-*`) |
| `results-table-fit` / `in-panel-grid` | Hug content width (`max-content`) — admin/edit grids |
| `results-table-full` | Optional full-width helper; prefer `recurring-summary-grid` for RO list |
| `col-align-left` / `col-align-center` | Keep header and cell alignment consistent |
| `dual-list-layout` / `dual-list-panel` / `dual-list-actions` | Two grids with transfer buttons between (e.g. Item Groups) |

**Width choice:**
- **Fit to contents** (`results-table-fit`): Manage Roles and similar small grids.
- **Full width with fixed shares** (`recurring-summary-grid`): Recurring Orders nested list.
- Contacts / Delivery Sheet: plain `results-table`.
- Do not use inline `style="width:100%"` or `col-fill` on the Recurring Orders list.

**Reference:** `Pages/RecurringOrders.aspx` (2026-07-20). Reuse these names on future grouped lists instead of `recurring-*` or other page-specific prefixes. Dual-list reference: `Pages/ItemGroups.aspx` (2026-07-23).

---

## 4. Unsaved-changes guard (shared script)

Do **not** paste large dirty-form / `beforeunload` scripts into page headers. Use the shared module:

- Script: `Scripts/unsavedChanges.js` → `window.TrackerUnsaved`
- Loaded once from `Site.Master` (before `HeadContent`)

Each page adds a **short** init in **`MainContent`** (bottom of the page) with local ClientIDs and optional aliases. Do **not** put `<%= %>` / `<%# %>` code blocks in `HeadContent` on ScriptManager pages — ASP.NET then throws *“The Controls collection cannot be modified because the control contains code blocks”* when ScriptManager injects into `<head>`.

```aspx
<%-- At end of MainContent — not HeadContent --%>
<script type="text/javascript">
TrackerUnsaved.init({
    dirtyFieldId: '<%= hdnDirty.ClientID %>',
    rootId: '<%= pnlForm.ClientID %>',   // or fieldIds: ['id1','id2']
    leaveMessage: 'You have unsaved changes. Leave without saving?',
    aliases: {
        markDirty: 'pageMarkDirty',
        clearDirty: 'pageClearDirty',
        allowNavigate: 'pageAllowNavigate',
        confirmLeave: 'pageConfirmLeave'
    }
});
</script>
```

| Action | Client wiring |
|--------|----------------|
| Save (stay) / deliberate postback | `OnClientClick="return pageAllowNavigate();"` or `TrackerUnsaved.allowNavigate()` |
| Back / leave without save | `OnClientClick="return pageConfirmLeave();"` |
| After successful save (code-behind) | `TrackerUnsaved.clearDirty()` via `RegisterStartupScript` |
| Validation failed / keep dirty | `TrackerUnsaved.markDirty()` |

Page-specific JS (overlays, redirects) stays local; dirty state always goes through `TrackerUnsaved`. Reference: `Pages/ContactDetails.aspx`, `Pages/OrderDetail.aspx`.

---

## 5. Retrofit policy

- **New pages and any page we edit** must follow this standard (status bottom + UpdatePanel + **page-tone panel** when the page has a home-dashboard counterpart).
- Pages that still put status at the top, use HyperLink-as-Back, lack UpdatePanel/UpdateProgress, or keep a duplicate `<h1>` outside the accent panel should be brought into line when that page is next worked on.
- Reference while retrofitting: `HolidayClosureDetail.aspx` (interaction) + `SentRemindersSheet.aspx` (page-tone shell).

### Retrofit checklist (copy onto each page)

- [ ] No standalone `<h1>` + icon above the form — one icon inside `page-tone-header`
- [ ] `simpleForm page-tone-panel page-tone-*` wraps title, toolbar, content, status
- [ ] Tone class matches the home card for that menu item (`Default.aspx` `home-tone-*`)
- [ ] Status / result at **bottom** of panel
- [ ] `ScriptManager` + `UpdatePanel` + `UpdateProgress`
- [ ] Async for stay-on-page actions; full postback for Back / Save & Return / redirects
- [ ] **Back** is `<asp:Button>`
- [ ] Dirty-form leave guard uses `TrackerUnsaved.init(...)` — no duplicated `beforeunload` blobs in the header

---

## Related

- Hard rules pointer: `Documentation/HARD_PROJECT_RULES.md` (Rule: WebForms UI)
- Root README §11 (WebForms Page Migration Rules) — thin pages + this UI shell
- Cursor rule: `.cursor/rules/webforms-ui-standards.mdc`
- Shared dirty guard: `Scripts/unsavedChanges.js`
