# RainbowOF knowledge transfer

Functional specification and migration package for replacing **QOnT** (this TrackerSQL application, Quaffee’s order tracker) with **RainbowOF**.

These documents were written from the current source: pages, managers, models, repositories, `Web.config` behaviour, and the mobile API notes. They are not a line-by-line code port. Where the source does not prove a behaviour, the text says **Verify**.

| Doc | Contents |
|---|---|
| [01-system-summary.md](01-system-summary.md) | What QOnT is, who uses it, how a delivery cycle works |
| [02-functional-inventory.md](02-functional-inventory.md) | Every capability, role, input, output, and table |
| [03-workflows.md](03-workflows.md) | Page catalogue and end-to-end workflows |
| [04-business-rules.md](04-business-rules.md) | Rules with purpose and code location |
| [05-database-analysis.md](05-database-analysis.md) | Tables, relationships, and what is not in source control |
| [06-reporting.md](06-reporting.md) | Screens, emails, PDF, and calculations |
| [07-security.md](07-security.md) | Membership, roles, API tokens, gaps |
| [08-domain-modernisation.md](08-domain-modernisation.md) | Coffee concepts mapped to a product-agnostic model |
| [09-rainbowof-specification.md](09-rainbowof-specification.md) | Master product specification |
| [10-ui-modernisation.md](10-ui-modernisation.md) | MudBlazor replacement for each legacy surface |
| [11-target-architecture.md](11-target-architecture.md) | .NET 10 solution shape |
| [12-migration-roadmap.md](12-migration-roadmap.md) | Phases, risks, and cutover |

Read `01`, then `09`, then `04` and `12`. Use `05` when writing the ETL, not `Documentation/TABLE_SCHEMA_REFERENCE.md` (that file is the older Access rename and is incomplete).

Older project notes under `Documentation/` remain useful for WooCommerce, the driver API, and deployment. If they disagree with the C# models, trust the code and these `docs/`.
