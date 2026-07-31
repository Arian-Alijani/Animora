# backend/ — ASP.NET Core modular monolith (Phase 2, BLOCKED)

Scope file. Assumes [`/AGENTS.md`](../AGENTS.md) is already read. Do not read `desktop/` or `web/`.

**Gate**: Phase 2 starts only after the desktop phase is signed off (AG-01). Until then this tree
holds structure only — do not add code here.

Normative docs — open only what the task needs:
[`04-module-catalog.md`](../docs/architecture/04-module-catalog.md) (who owns what),
[`06-api-contract.md`](../docs/architecture/06-api-contract.md) (endpoints, versioning, errors,
paging), [`07-server-data-architecture.md`](../docs/architecture/07-server-data-architecture.md)
(Postgres, RLS, indexes, migrations),
[`10-security-and-access-control.md`](../docs/architecture/10-security-and-access-control.md),
[`09-sync-architecture.md`](../docs/architecture/09-sync-architecture.md),
[`14-jobs-and-notifications.md`](../docs/architecture/14-jobs-and-notifications.md). Stack:
`TECH_STACK.md` §6–§8.

## Project map

| Project | Owns |
|---|---|
| `src/Animora.Api` | Minimal API host, `Composition/` (DI + endpoint mapping), `Configuration/`, `Middleware/` (tenant context, problem+json, rate limits), `OpenApi/`, `Health/` |
| `src/Modules/Animora.Modules.<Name>` | `Contract/` (public `I<Module>Contract` + Mediator types), `Domain/`, `Application/` (handlers, validators), `Data/Writes` (EF Core), `Data/Queries` (Dapper), `Endpoints/`, `Configuration/` (module DI) |
| `src/Animora.Infrastructure` | `Persistence/{Writes,Configurations,Migrations,Rls}`, `Queries/` (Npgsql/Dapper factory), `Storage/` (S3/MinIO), `Caching/` (HybridCache), `Realtime/` (SignalR), `Jobs/` (Hangfire), `Email/`, `Sms/`, `Payments/`, `Security/`, `Observability/`, `Time/` |
| `tests/Animora.ArchTests` | `NetArchTest` rules AT-01…AT-07 |
| `tests/Animora.UnitTests` | Handlers, validators, domain rules |
| `tests/Animora.IntegrationTests` | Testcontainers (Postgres, MinIO); RLS/tenant-isolation cases are mandatory |
| `tests/Animora.SyncTests` | The mandatory sync test matrix ([09](../docs/architecture/09-sync-architecture.md)) |

Modules: `Identity`, `Clients`, `Visits`, `Scheduling`, `Finance`, `Reporting`, `Notifications`,
`Licensing`, `Files`, `Sync`, `PlatformAdmin`.

## Hard rules for this tree

- **BE-01**: `Modules.X` never references `Modules.Y` and never reads another module's tables
  (INV-01, DIR-01). Cross-module calls use the target module's `Mediator` contract.
- **BE-02**: Only `Animora.Api` references more than one module (DIR-04, AT-06).
- **BE-03**: OpenAPI first. Add the operation to `contracts/openapi/`, then implement, then regenerate
  clients (`tools/codegen`). Hand-written DTOs are banned (CONV-19/20).
- **BE-04**: API changes are additive unless a major version bump is justified; the server serves
  current + N-1 (INV-10, API-VER-05). The breaking-change gate compares against
  `contracts/openapi/snapshots/`.
- **BE-05**: Every endpoint is permission-checked server-side (INV-09, SEC-12); every gated feature is
  re-checked against entitlements server-side (INV-16). Client gating is UX only.
- **BE-06**: Every tenant table has an RLS policy plus a query filter (INV-08). An integration test
  proves cross-tenant denial.
- **BE-07**: Every error response is RFC 9457 `problem+json` with a stable `ERR-{MODULE}-{NNN}` code
  (INV-07, CONV-13/14/15).
- **BE-08**: Writes use EF Core (`Data/Writes`), reports/exports use Dapper (`Data/Queries`); never
  the reverse (INV-20, AT-04/AT-05). No `SELECT *`, keyset pagination only (CONV-16).
- **BE-09**: `HybridCache` only — never `IMemoryCache` directly; no in-process session state
  (TECH_STACK §6, §18). All config through environment variables
  ([19](../docs/architecture/19-deployment-topology.md)).
- **BE-10**: Every job is idempotent, cancellable, correlation-id logged, concurrency-capped (JOB-01…
  JOB-04). State the completion-detection mechanism in the job's doc comment (CM-03).
- **BE-11**: Ledger rows are append-only; no UPDATE/DELETE after commit (INV-06,
  [15](../docs/architecture/15-finance-and-ledger.md)). Corrections are new entries.
- **BE-12**: Synced entities: `UUIDv7` PK, HLC field groups, tombstone, entity-type registry entry,
  and every row of the sync test matrix (INV-03/04/19).

## Definition of done

Migration in the CI step (never auto-migrate on boot) · OpenAPI updated and client-drift check green ·
permission enforced and tested · RLS test green · problem+json shape asserted · sync matrix green for
touched entities · owning doc updated in the same PR (AG-16).
