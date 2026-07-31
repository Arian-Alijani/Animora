# Phase 21: Reporting Local Data

## Goal

Swap phase 10's Reporting Stage A fake data-seam implementation for real local Dapper-backed views
over the data persisted by phases 16/17/18/20, so every catalog screen shows genuine tenant data
with a real "last synced" timestamp instead of a fake instantaneous value.

## Expected Outcome

Every report/KPI screen from phase 10 (financial summary, best-selling, debt/aging, visitor/
traffic stats, clinic KPIs, cheque status summary, cash session reconciliation history) renders
real local data via plain SQLite views/Dapper queries, matching REP-06/07's desktop-local parity
rule; the growth chart from phase 07 (already real since phase 18) is cross-checked for contract
consistency; exports still use the phase 10 job-status UX pattern, now against real data.

## Scope

- Plain SQLite views/Dapper queries (REP-06/07: no scheduled desktop refresh needed, dataset is
  small per tenant device) for each catalog row in
  [16-reporting-and-analytics.md](../../../../docs/architecture/16-reporting-and-analytics.md),
  reading from the real tables built in phases 16 (clients), 17 (scheduling), 18 (visits), 20
  (finance).
- Every chart continues to use the fixed `{ series, meta }` contract shape (REP-11), now populated
  from real aggregation logic matching the server's intended grouping/columns (REP-08 parity is
  logic parity, not freshness parity).
- Real "last synced" timestamp wiring: since sync is inert until P2, this phase surfaces the local
  data's own last-write timestamp as the interim value with a clear label, and documents the exact
  swap point for when real sync cursors exist (`// TODO(P2)` per DT-12).
- Export actions (`QuestPDF`/`ClosedXML`) now render real data; the job-status UX pattern from
  phase 10 is unchanged (this phase does not touch phase 26's real job-table wiring).
- Rebind `Modules.Reporting`'s data-seam interface to this real implementation; delete the Stage A
  fake.
- Out of scope: materialized views (desktop dataset is small enough that plain views suffice per
  REP-07 — do not build server-style materialization here), real job-table tracking (phase 26).

## Key References

- [`docs/architecture/16-reporting-and-analytics.md`](../../../../docs/architecture/16-reporting-and-analytics.md) —
  the full KPI/report catalog table, REP-06/07/08 desktop-local parity rules, REP-11/12 chart
  contract.
- [`docs/architecture/15-finance-and-ledger.md`](../../../../docs/architecture/15-finance-and-ledger.md) —
  FIN-21 (reports derive from `LedgerEntry`, never a separate summary table) — the desktop-local
  queries must honor this too, reading phase 20's real ledger.
- [`docs/architecture/20-extensibility-playbook.md`](../../../../docs/architecture/20-extensibility-playbook.md) —
  "add a report" recipe step 6 (desktop-local parity query).

## Dependencies

Requires phase 14, phase 10 (Reporting screens + data-seam interface), phase 16 (Clients local
data), phase 17 (Scheduling local data), phase 18 (Visits local data), and phase 20 (Finance local
data) — this phase reads all of them.

## Completion Criteria

- [ ] Every catalog row from phase 10 renders real local data via a Dapper query/plain view.
- [ ] Every chart still conforms to the fixed `{ series, meta }` shape, now with real values.
- [ ] Financial reports derive their numbers from `LedgerEntry` only (FIN-21), verified by tracing
      one report's query to the ledger table.
- [ ] "Last synced" indicator reflects a real local timestamp value with a documented P2 swap point.
- [ ] Phase 10's `Avalonia.Headless` smoke tests still pass unchanged against real data.

---

## Step 0

Not run yet. See [`../../_meta/session-protocol.md`](../../_meta/session-protocol.md).
