---
id: 16-reporting
title: Reporting & Analytics
read_when: ["adding a report or KPI", "chart data work"]
topics: [reporting, materialized-view, kpi, export]
depends_on: [07-server-data, 15-finance]
stability: stable
---

## Contract

Decides: read-model strategy, the view/materialized-view/on-demand decision rule, refresh
scheduling, the KPI/report catalog mapped to sources, desktop-local vs server reporting parity,
export architecture, chart data contracts, and performance guardrails. Does not decide ledger
design itself (see [15](15-finance-and-ledger.md)) or physical indexing (see [07](07-server-data-architecture.md)).

## Read-model strategy

- REP-01: reporting never queries via EF Core (INV-20); all reporting is Dapper against plain
  views, materialized views, or hand-tuned on-demand SQL, chosen per the decision rule below.
- REP-02: reporting queries are read-only and tenant-scoped exactly like any other query (RLS +
  explicit tenant filter, DATA-04) — reporting is not a trusted/elevated path.

## View vs materialized view vs on-demand decision rule

| Choose | When |
|---|---|
| Plain view | Query is cheap (indexed, small result set) and needs always-current data (e.g., today's cash session state) |
| Materialized view, scheduled refresh | Query aggregates a large/append-only source (ledger, visits) and near-real-time (minutes-stale) is acceptable — e.g., monthly revenue, best-selling services |
| On-demand query (no view) | Ad-hoc filter combinations (custom date range export) where pre-materializing every combination is impractical; always keyset/limit-bounded |

REP-03: a report defaults to a materialized view unless it is proven cheap enough for a plain view
or so ad-hoc that materializing does not help; this keeps p95 latency achievable on the 4 GB VPS
([01-context-and-drivers.md](01-context-and-drivers.md) report-latency target).

## Refresh scheduling

- REP-04: materialized views refresh via Hangfire recurring jobs (`REFRESH MATERIALIZED VIEW
  CONCURRENTLY` where the view's index support allows), staggered across off-peak minutes to avoid
  CPU spikes on the shared VPS.
- REP-05: refresh cadence is per-view, declared in the job registration, not hardcoded globally —
  e.g., KPI dashboard views refresh every 15 minutes; best-selling analytics refresh hourly.

## KPI / report catalog

| Report/KPI | Source | Materialization |
|---|---|---|
| Financial summary (revenue, expenses, net) | `LedgerEntry` | Materialized view, hourly |
| Best-selling products/services | `LedgerEntry` + invoice lines | Materialized view, hourly |
| Debt/unpaid invoice aging | `Invoice` + `LedgerEntry` | Materialized view, daily |
| Visitor/traffic statistics | `Visit`, `Appointment` | Materialized view, daily |
| Clinic KPIs (visits/day, no-show rate, avg wait) | `Visit`, `Appointment` | Materialized view, daily |
| Cheque status summary | `Cheque` | Plain view (small dataset) |
| Cash session reconciliation history | `CashSession`, `CashMovement` | Plain view |
| Growth chart (per-patient biometrics) | `BiometricReading` | On-demand query (per-patient, small, indexed) |

This table is the canonical catalog (INV-18); module docs link here rather than restating report
lists.

## Desktop-local reporting vs server reporting parity

- REP-06: the same report definitions exist as SQLite views/Dapper queries on desktop for offline
  use (TECH_STACK feature-map §17 "Chart/KPI: Dapper query or materialized view"); desktop
  materialization uses plain SQLite views refreshed on read (dataset size per tenant device is
  small enough that no scheduled desktop refresh job is needed) — REP-07.
- REP-08: parity means identical aggregation logic (same grouping/columns), not identical
  freshness — desktop reports reflect only locally synced data, which may lag the server by the
  normal sync cadence; this is surfaced as a "last synced" timestamp on desktop report screens, not
  hidden.

## Export architecture

- REP-09: exports (`ClosedXML` for Excel, `QuestPDF` for PDF) are generated from the same
  reporting query layer as on-screen reports — no separate export-only query path.
- REP-10: large exports run as a tracked job (server: Hangfire + job-status endpoint; desktop:
  local `job` table, see [14](14-jobs-and-notifications.md)) rather than a synchronous request, to
  avoid tying up a request thread on the weak server.

## Chart data contracts

- REP-11: every chart endpoint/query returns a fixed shape: `{ series: [{ label, points: [{x, y}] }], meta: { unit, generatedAt } }`
  — one shape reused by `LiveChartsCore` (desktop) and `echarts-for-react` (web) adapters, so chart
  UI code never branches on report type to parse a different structure.
- REP-12: growth-chart series (biometrics over time) use the patient's local timezone-adjusted
  Jalali date only at the presentation adapter; the contract itself carries UTC instants (INV-13).

## Performance guardrails

- REP-13: every report/KPI query has a documented supporting index or materialized view; a report
  added without one fails review (ties to [07-server-data-architecture.md#indexing-and-partitioning-posture](07-server-data-architecture.md)).
- REP-14: report endpoints carry the report-latency quality target (p95 ≤ 1.5 s,
  [01-context-and-drivers.md](01-context-and-drivers.md)) and a statement timeout at the database
  role level (DATA-xx, see [07](07-server-data-architecture.md)) so a runaway ad-hoc query fails
  fast instead of starving the OLTP path.
- REP-15: chart/list result sets are always capped (keyset pagination for lists, fixed max series
  length for charts, e.g., last 24 months) — no unbounded report result ships.
