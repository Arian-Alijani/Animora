# desktop/ — Avalonia Windows client (Phase 1, ACTIVE)

Scope file. Assumes [`/AGENTS.md`](../AGENTS.md) is already read. Do not read `backend/` or `web/`.

Normative docs for this tree — open only what the task needs:
[`12-desktop-architecture.md`](../docs/architecture/12-desktop-architecture.md) (layering, shell,
offline UX, startup), [`08-desktop-local-data.md`](../docs/architecture/08-desktop-local-data.md)
(SQLite, SQLCipher, FTS5, outbox, reminder/job tables),
[`05-domain-model.md`](../docs/architecture/05-domain-model.md) (entities, state machines),
[`20-extensibility-playbook.md`](../docs/architecture/20-extensibility-playbook.md) ("add a desktop
screen" recipe). Stack: `TECH_STACK.md` §4.

## Project map

| Project | Owns | May reference |
|---|---|---|
| `src/Animora.Desktop.App` | Composition root, DI wiring, single-window shell, `INavigationService`, startup sequence, app assets/fonts | every project below |
| `src/Animora.Desktop.UI` | Design tokens, `Semi.Avalonia` overrides, shared controls, value converters, Jalali/number formatters, `ViewModelBase`, dialog/toast abstractions | `SharedKernel`, `Contracts` |
| `src/Animora.Desktop.Modules.<Name>` | One module's `Views/`, `ViewModels/`, `Handlers/`, `Models/` | `UI`, `Data`, `Infrastructure`, `SharedKernel`, `Contracts` |
| `src/Animora.Desktop.Data` | SQLite: `Entities/`, `Configurations/`, `Writes/` (EF Core), `Queries/` (Dapper), `Migrations/`, `Search/` (FTS5), `Backup/`, `Security/` (SQLCipher key via DPAPI) | `SharedKernel`, `Contracts` |
| `src/Animora.Desktop.Sync` | Outbox drain, cursor store, batch client, conflict application, protocol negotiation — **seam only until P2** | `Data`, `Infrastructure`, modules |
| `src/Animora.Desktop.Infrastructure` | Kiota `Generated/`, `Http/`, `Connectivity/`, `Jobs/` (local scheduler), `Printing/`, `Documents/` (QuestPDF, ClosedXML), `Imaging/`, `Barcode/`, `Notifications/` (Windows toast), `Licensing/`, `SecureStorage/`, `Logging/`, `Update/` (Velopack) | `SharedKernel`, `Contracts` |
| `tests/Animora.Desktop.UnitTests` | Handler, validator, formatter tests | — |
| `tests/Animora.Desktop.UiTests` | `Avalonia.Headless` RTL smoke test per screen (mandatory) | — |
| `tests/Animora.Desktop.ArchTests` | `NetArchTest` rules DIR-05, AT-03/04/05 on desktop assemblies | — |

Desktop modules mirror backend module names: `Identity`, `Clients`, `Visits`, `Scheduling`,
`Finance`, `Reporting`, `Notifications`, `Licensing`, `Files`. `PlatformAdmin` is web-only.

## Hard rules for this tree

- **DT-01**: `Modules.X` never references `Modules.Y` (DIR-01/INV-01). Cross-module work goes through
  a `Mediator` request declared in the target module's contract namespace.
- **DT-02**: ViewModels never touch `DbContext`, `HttpClient`, or `IDbConnection`. Every state change
  is a `Mediator` request handled in `Handlers/` (DESK-ARCH-01/02).
- **DT-03**: Business validation lives in `shared/dotnet/Animora.SharedKernel/Validation` and runs in
  the handler. ViewModels do presentation-level checks only (CONV-18, DESK-ARCH-04).
- **DT-04**: A command that writes a synced entity writes the row **and** its outbox row in one SQLite
  transaction (DESK-ARCH-09), and never waits on the network to acknowledge the user (DESK-ARCH-10).
- **DT-05**: Writes use EF Core (`Data/Writes`), hot reads and reports use Dapper (`Data/Queries`).
  Never mix: `Data/Writes` must not reference Dapper, `Data/Queries` must not reference EF Core
  (AT-04/AT-05, INV-20).
- **DT-06**: `FlowDirection=RightToLeft` is set once at the shell root; screens never override flow
  direction (DESK-ARCH-06). Use `Animora.Desktop.UI` theme tokens — no hardcoded colors, spacing, or
  font sizes in a module view (DESK-ARCH-12).
- **DT-07**: Jalali conversion happens in converters/formatters at the binding edge only; handlers and
  entities are UTC (DESK-ARCH-14, CONV-04/05). `DateTime.Now` is banned (CONV-06).
- **DT-08**: Any list that can exceed 200 rows uses a virtualized `DataGrid` (TECH_STACK §4) with
  keyset-style paging over a Dapper query — never load-all-then-filter in memory.
- **DT-09**: Routes are registered by each module at composition; the shell has no compile-time
  knowledge of module screens (DESK-ARCH-05).
- **DT-10**: Long operations (export, bulk import, backup) run as tracked rows in the local job table
  with non-blocking progress UI — never a modal spinner that blocks the shell (DESK-ARCH-11).
- **DT-11**: New local table or column → EF Core desktop migration, forward-only, applied at startup
  after a pre-migration snapshot ([08](../docs/architecture/08-desktop-local-data.md)).
- **DT-12**: Phase 1 has no server. `Sync/` and `Infrastructure/Generated/` stay empty; do not stub a
  fake API. Where a screen would eventually read server data, use local data and mark the seam with
  `// TODO(P2): ...` (CM-06).

## Definition of done for a desktop screen

Headless RTL smoke test passes · no `DbContext`/`HttpClient` in the ViewModel · route registered
without shell coupling · theme tokens used · Jalali only at the binding edge · validation in the
shared validator · synced writes transactional with the outbox row. Full checklist:
[`20-extensibility-playbook.md`](../docs/architecture/20-extensibility-playbook.md#recipe-add-a-desktop-screen).
