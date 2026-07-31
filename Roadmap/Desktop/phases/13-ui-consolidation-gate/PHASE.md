# Phase 13: UI Consolidation Gate

## Goal

Review every screen built in phases 00-12 as a single, coherent whole — navigation completeness,
RTL/theme-token consistency, and data-seam pattern conformance — before any local persistence work
begins in Stage C.

## Expected Outcome

Every screen registered by phases 04-12 is reachable from the shell's navigation, visually and
behaviorally consistent (tokens, RTL, Jalali-at-edge, virtualization on large lists), and every
module's data-seam interface follows the pattern phase 02 established. Gaps found here are fixed
here, not carried into Stage C as tech debt.

## Scope

- Full click-through audit of the shell's navigation against every route registered in phases
  04-12: no orphaned screen, no dead navigation entry.
- Cross-screen consistency sweep: design tokens (no hardcoded colors/spacing/fonts, DESK-ARCH-12),
  RTL flow inheritance (no per-screen override, DT-06), Jalali conversion only at binding edges
  (DT-07), virtualized `DataGrid` on every list that can exceed 200 rows (DT-08).
- Data-seam audit: confirm every module's Stage A fake-data interface follows phase 02's pattern
  (fake-now/real-later behind a module-owned interface) so phase 14+ has nothing to restructure.
- `Avalonia.Headless` RTL smoke test coverage audit: every screen from phases 04-12 has one; fill
  any gap found.
- Fix, do not defer: any inconsistency found in this phase is corrected in this phase (small,
  targeted fixes only — this is a gate, not a redesign).
- Out of scope: any new screen, any local persistence, any business logic beyond what already
  exists from phases 04-12.

## Key References

- [`docs/architecture/12-desktop-architecture.md`](../../../../docs/architecture/12-desktop-architecture.md) —
  the full DESK-ARCH-01..19 checklist this gate re-verifies holistically.
- [`desktop/AGENTS.md`](../../../../desktop/AGENTS.md) — DT-01..DT-12 "Definition of done for a
  desktop screen" checklist, applied retroactively across all screens at once.
- [`Roadmap/Desktop/README.md`](../../README.md) — the Stage A/C data seam pattern this phase must
  confirm is followed uniformly before Stage C begins.

## Dependencies

Requires every phase 00-12 to be `complete`. Blocks all of Stage C (phases 14-23): no local-data
phase starts until this gate signs off, since Stage C assumes a stable, consistent screen surface.

## Completion Criteria

- [ ] Every route registered in phases 04-12 is reachable from shell navigation with no dead ends.
- [ ] No hardcoded color/spacing/font value exists in any module view (DESK-ARCH-12 spot-check
      across all modules, not just the project it was introduced in).
- [ ] No screen overrides `FlowDirection` individually (DT-06).
- [ ] Every list that can exceed 200 rows uses a virtualized `DataGrid` (DT-08), verified
      module-by-module.
- [ ] Every screen from phases 04-12 has a passing `Avalonia.Headless` RTL smoke test.
- [ ] Every module's Stage A data-seam interface matches phase 02's documented pattern with no
      outlier implementation.

---

## Step 0

Not run yet. See [`../../_meta/session-protocol.md`](../../_meta/session-protocol.md).
