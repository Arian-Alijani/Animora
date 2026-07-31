# Phase 10: Reporting Module Screens

## Goal

Build every screen the Reporting module owns: financial summary, best-selling products/services,
debt/aging, visitor/traffic stats, clinic KPIs, and export actions, fully click-through against the
Stage A data seam, inside `Animora.Desktop.Modules.Reporting`.

## Expected Outcome

A staff member can view each catalog report/KPI screen with charts (`LiveChartsCore`) and export to
PDF (`QuestPDF`) or Excel (`ClosedXML`) — all through real `Mediator` handlers bound to
`Modules.Reporting`'s data-seam interface (fake now, real Dapper-backed local views in phase 21).

## Scope

- One screen per catalog row in the KPI/report catalog
  ([16-reporting-and-analytics.md](../../../../docs/architecture/16-reporting-and-analytics.md)):
  financial summary, best-selling, debt/aging, visitor/traffic stats, clinic KPIs, cheque status
  summary, cash session reconciliation history.
- Every chart uses the fixed `{ series: [...], meta: {...} }` contract shape (REP-11), matching
  phase 07's growth chart convention.
- Export actions wired as tracked local "job" UX per DESK-ARCH-11 (non-blocking progress surface) —
  actual job-table persistence is phase 26; this phase wires the ViewModel-level job-status pattern
  against a fake instantaneous completion.
- A visible "last synced" timestamp affordance on report screens per REP-08 (even though there is no
  real sync yet — the UI element and its data-seam field exist now so phase 21/P2 only wires real
  data).
- Out of scope: local Dapper/SQLite-backed materialization (phase 21), real job-table tracking
  (phase 26), server-side materialized views (P2/backend).

## Key References

- [`docs/architecture/16-reporting-and-analytics.md`](../../../../docs/architecture/16-reporting-and-analytics.md) —
  full KPI/report catalog table (the exact screen list), chart data contract (REP-11/12), desktop-
  local parity note (REP-06/07/08).
- [`docs/architecture/12-desktop-architecture.md`](../../../../docs/architecture/12-desktop-architecture.md) —
  DESK-ARCH-11 long-operation/job UX pattern for exports.
- [`docs/TECH_STACK.md`](../../../../docs/TECH_STACK.md) §4 — `LiveChartsCore`, `QuestPDF`,
  `ClosedXML` as the fixed chart/export libraries.

## Dependencies

Requires phases 02, 03, and 09 (reports read Finance's conceptual shape). Feeds phase 21 (Reporting
Local Data) and phase 24 (Documents & Printing, which implements the real PDF/Excel export engine
behind this phase's export actions).

## Completion Criteria

- [ ] Every catalog row has a corresponding screen, navigable from the shell.
- [ ] All charts consume the fixed `{ series, meta }` contract shape.
- [ ] Export actions present non-blocking job-style UX (even against fake instant completion).
- [ ] A "last synced" indicator element exists on report screens.
- [ ] Every screen passes an `Avalonia.Headless` RTL smoke test.

---

## Step 0

Not run yet. See [`../../_meta/session-protocol.md`](../../_meta/session-protocol.md).
