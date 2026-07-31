# Phase 29: Performance & Startup Budget

## Goal

Verify and tune the desktop app against its cold-start and runtime performance targets: the ≤ 3 s
shell-interactive budget (DESK-ARCH-16), ReadyToRun/Workstation GC/deferred-background-init
publish settings (DESK-ARCH-17), and query/list performance across every module built so far.

## Expected Outcome

The app launches to an interactive shell within budget on representative old hardware, with
`BackgroundInit` (scheduler, connectivity probe) confirmed non-blocking; every list/report/search
built across phases 04-23 is measured and, where it exceeds a reasonable local-latency target, is
fixed (index, keyset paging, query shape) — not deferred as "acceptable for now."

## Scope

- Publish configuration verification: self-contained, `ReadyToRun`, single-file, trimming disabled
  (Avalonia reflection needs), Workstation GC (TECH_STACK §4) — confirm the actual build output
  matches this, not just the project file intent.
- Startup budget measurement against DESK-ARCH-16's sequence (Host bootstrap -> DB open/migration
  check -> shell show -> background init) on representative low-end hardware; `ShellShow` must not
  be gated on `BackgroundInit` completing.
- Performance sweep across every list/search/report screen from phases 04-23: virtualized
  `DataGrid` usage confirmed everywhere DT-08 applies, FTS5 search latency measured, report query
  latency measured against a reasonable desktop-local target, growth-chart rendering measured for
  a large biometric history.
- Fix, do not defer: any query/list found slow in this sweep gets a real fix (index, paging
  strategy, query restructuring) in this same phase.
- Out of scope: server-side performance (P2/backend, REP-14's p95 ≤ 1.5 s target is server-side;
  this phase defines and meets its own desktop-local equivalent since the desktop budget is a
  distinct, DESK-ARCH-16-driven target, not that server number).

## Key References

- [`docs/architecture/12-desktop-architecture.md`](../../../../docs/architecture/12-desktop-architecture.md) —
  DESK-ARCH-16 startup sequence/budget, DESK-ARCH-17 (trimming disabled, why, and what achieves the
  budget instead).
- [`docs/architecture/01-context-and-drivers.md`](../../../../docs/architecture/01-context-and-drivers.md) —
  the product's startup-time and desktop-DB-growth quality-attribute targets this phase measures
  against.
- [`desktop/AGENTS.md`](../../../../desktop/AGENTS.md) — DT-08 (virtualized `DataGrid` mandatory on
  any list exceeding 200 rows), the mechanical check this sweep re-verifies across all modules.

## Dependencies

Requires every module's local-data phase (15-23) to be `complete` — this phase measures real data
paths, not fakes. Feeds phase 31 (release readiness cites this phase's measured budget as evidence).

## Completion Criteria

- [ ] Cold start reaches an interactive shell within the DESK-ARCH-16 budget on representative
      hardware, with `BackgroundInit` confirmed non-blocking.
- [ ] Publish output is verified self-contained + `ReadyToRun` + single-file + trimming disabled +
      Workstation GC.
- [ ] Every list/search/report screen across phases 04-23 has a measured latency figure; any outlier
      found is fixed, not just documented.
- [ ] No list that can exceed 200 rows loads all-then-filters in memory anywhere in the codebase
      (DT-08 re-verified, not assumed from phase 13's earlier pass).

---

## Step 0

Not run yet. See [`../../_meta/session-protocol.md`](../../_meta/session-protocol.md).
