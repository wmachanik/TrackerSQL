# 11 — Target Architecture

**Stack (required):** .NET 10, Blazor Web App (interactive server for staff and portal; static or lightweight interactive for public token pages), MudBlazor, EF Core, SQL Server, ASP.NET Identity, OpenTelemetry, Serilog, background services, REST for the driver and for any future channel worker, dependency injection throughout.

**Practices:** Clean-ish modular monolith. One deployable at cutover. Domain rules out of the UI. Persistence out of the domain. No `DbContext` in Blazor components.

---

## 1. Solution structure

```text
RainbowOF.sln
  src/
    RainbowOF.Domain/            Entities, value objects, domain services (pure)
    RainbowOF.Application/       Use cases, ports, DTOs, validation
    RainbowOF.Infrastructure/    EF Core, Identity, email, files, Woo adapter, clock
    RainbowOF.Contracts/         Public DTOs shared by API and (optionally) client
    RainbowOF.App/               Blazor Web App host, UI, composition root
    RainbowOF.Api/               ASP.NET Core controllers for /api/v1 (can be the same host)
  tests/
    RainbowOF.Domain.Tests/
    RainbowOF.Application.Tests/
    RainbowOF.Infrastructure.Tests/
```

One host process is acceptable: `RainbowOF.App` references Infrastructure and exposes both Blazor and minimal/API controllers. A separate `RainbowOF.Api` project is still useful so the driver contract is an explicit assembly. They can deploy as one site.

**Do not** create a project per QOnT page.

---

## 2. Project responsibilities

### Domain

- Entities from `docs/08`: `Customer`, `CustomerLocation`, `Order`, `OrderLine`, `StandingOrder`, `ServiceSchedule`, `ServiceTask`, `Asset`, `DeliveryProof`, `Territory`, `Product`, and the rest.
- Invariants that do not need I/O: quantity at least 1, one logical drop per customer location per required date (the check against the database is an application service; the entity rejects a second line identity that is empty), document intent required, guest name required when there is no customer.
- Domain services: `ConsumptionCalculator` (port of `PredictionManager` math), `PromiseCalculator` (port of `DeliveryPromiseManager`), `OnHandAdjustment` (kg × conversion).
- No MudBlazor, no EF attributes required on entities (use Fluent configuration in Infrastructure). A pragmatic team may use private setters and a persistence-friendly constructor.

### Application

Use-case classes, one public method each, for example:

- `CreateOrMergeOrder`
- `CompleteDelivery`
- `RecalculateSchedules`
- `PrepareInvitationBatch`
- `SendInvitation`
- `ImportChannelOrders`
- `SyncDriverStops`
- `AdvanceProductCycle`
- `ChangeServiceTaskStatus`

Each depends on ports: `IOrderRepository`, `IClock` (business zone, replacing `TimeZoneUtils`), `IEmailSender`, `IAudit`, `IUserContext`.

Fluent validation or data annotations live here, not in `.razor` only.

### Infrastructure

- `RainbowDbContext` and configurations.
- Repositories.
- `Identity` with roles and a permission claim set from `docs/07`.
- `Serilog` sink (SQL or Seq; file sink for the first single-server deploy).
- `OpenTelemetry` traces and meters: HTTP, EF, the invitation send, the import, the driver sync.
- Email adapter (MailKit or the .NET SMTP client) honouring test mode.
- `WooChannelClient` implementing `ISalesChannel`.
- PDF adapter for proof (QuestPDF or similar; do not port `SimplePdf` line-by-line unless licence and output must match exactly — **verify** visual acceptance with one real proof).
- Background queue (`Channel<T>` or `BackgroundService`) for “complete after sync” and “send email”, replacing `MobileDeliveryCompletionQueue`’s fire-and-forget pattern with something that survives a retry.

### App (Blazor)

- Components call application services.
- `MudDialog` and grids do not contain SQL.
- Authorization policies on routes.

---

## 3. Persistence model

- SQL Server, EF Core migrations, no runtime XML DDL page.
- Schema names: `crm`, `catalogue`, `fulfilment`, `service`, `delivery`, `channel`, `identity`, `audit`.
- Forecast rows in `service.Schedules`, not nullable date columns per maintenance kind.
- External ids in `channel.ExternalOrders` and `channel.ExternalCustomers`.
- Concurrency token on `Order` and `Customer` (`rowversion`).
- Dates: `date` for prepare-on and required-on; `datetimeoffset` for ordered-at, proof-at, and audit. Business zone `Africa/Johannesburg` via `IClock`.
- Decimal quantities (3 decimal places) instead of `double`.
- Do not map `Temp*` tables. Drafts are normal orders with status `Draft` or component state.

ETL is a one-shot project or script (`RainbowOF.Import`) that reads `OtterDb` and writes the new schema. It is not part of the running site.

---

## 4. API design (driver and integrations)

Keep the resource shape the current app already depends on, under `/api/v1`, so Quaffee Driver can move with a small client change. Implement with ASP.NET Core, bearer tokens issued by Identity (or a persisted token table equivalent to `MobileDeviceTokensTbl`).

| Method | Path | Use case |
|---|---|---|
| GET | `/api/v1/ping` | Health |
| POST | `/api/v1/auth/login` | Issue token if user is a worker or admin |
| POST | `/api/v1/auth/logout` | Revoke |
| GET | `/api/v1/auth/me` | User and worker |
| GET | `/api/v1/delivery/dates` | Dates with work |
| GET | `/api/v1/delivery/sheet` | Stops |
| POST | `/api/v1/delivery/sync` | Idempotent proofs |
| POST | `/api/v1/delivery/route` | Sequence |
| GET | `/api/v1/delivery/proofs` | Admin |
| GET/POST/PUT | `/api/v1/repairs` | Service tasks |
| GET | `/api/v1/lookups` | Outcomes, settings, terms |
| GET | `/api/v1/contacts` | Search |
| POST | `/api/v1/location` | Pings, same retention rules |
| POST | `/api/v1/devices/{id}/revoke` | Admin |

Contract rules to preserve: camelCase JSON, ETag on sheet and task list, no cookie auth, no bodies in logs, rate limits, HTTPS.

Staff UI does **not** go through this API. It calls application services in-process. A later public API can reuse the same use cases.

Channel import can be a use case invoked by the UI or by a `BackgroundService` polling on a timer. QOnT is manual; start with manual, add a timer only if the business wants it (`IHostedService` with the same import use case).

---

## 5. Cross-cutting

| Concern | Choice |
|---|---|
| Identity | ASP.NET Identity, password reset required at cutover (hashes differ) |
| Authorization | Policy per permission in `docs/07`. Customer portal: claim `customer_id` |
| Logging | Serilog, enrich with user, customer id, order id. Do not log tokens, passwords, signatures, or full email HTML in production |
| Telemetry | OpenTelemetry: AspNetCore, HttpClient, EF Core. Export OTLP. Meter: invitations sent, orders completed, import failures, sync conflicts |
| Errors | Problem Details for API. Blazor error boundary plus `MudAlert`. Correlation id |
| Email | Port `MessageKeys` to template ids in the database |
| Files | Proofs and signatures in a protected blob or file store, not in the web root |
| Config | App settings for non-secrets; user-secrets locally; environment or a vault in production. No secrets in git |
| Time | `IClock` injectable for tests (QOnT already has `TestNow` config — keep that idea in Development only) |
| Background work | `BackgroundService` for email and completion. Persist a `Outbox` row so a crash does not lose a proof’s completion |

---

## 6. Mapping from QOnT code

| QOnT | RainbowOF |
|---|---|
| `Pages/*.aspx` | `.razor` pages in `RainbowOF.App/Components` |
| `Managers/*` | Application use cases + domain calculators |
| `Repositories/*` | EF repositories in Infrastructure. Do not port `RepositoryBase` SQL strings as the design |
| `Models/*` | Domain entities or DTOs, split deliberately |
| `Classes/SystemConstants.cs` | Seed data and options classes. Magic person ids become data |
| `Classes/TrackerDb.cs` | Deleted |
| `Api/MobileApiControllers.cs` | `RainbowOF.Api` controllers calling the same use cases as Blazor |
| `*SchemaInstaller` | EF migrations |
| Membership | Identity + import of usernames only |

---

## 7. Deployment shape

- One ASP.NET Core app behind HTTPS.
- SQL Server (existing host or Azure SQL / SQL MI).
- Single instance is enough at Quaffee’s volume. Design the outbox and token store so a second instance is possible later (no in-memory rate limit as the only control — the current API rate limit is in-memory and resets on recycle; move counters to a distributed cache only when a second instance exists).
- Health endpoint `/health` including DB.
- Static files for a legacy driver web shell only if that app is still shipped.

---

## 8. Testing bar

- Domain tests: consumption math (stock × conversion, 6-sample average, holiday extension, fallback offsets), promise windows (including weekend wrap and Wednesday rule), duplicate drop, qty minimum.
- Application tests with a fake clock and fake email: invitation eligibility, standing-order notify, waybill duplicate, complete-delivery writes schedule.
- One infrastructure test against LocalDB for “import Woo id twice does not duplicate”.
- No UI test requirement beyond the critical Blazor paths the team automates with bUnit for validation messages (PO warning, duplicate dialog).

---

## 9. Explicit non-goals for the architecture

- Microservices per module.
- Event-sourcing the order.
- A generic rules engine for the handful of calculators. C# domain services are easier to audit than a new DSL.
- Sharing the QOnT SQL database as the live schema. Read it for ETL; run RainbowOF on its own schema so renames are possible.
