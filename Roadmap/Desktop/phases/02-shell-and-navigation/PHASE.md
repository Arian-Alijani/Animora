# Phase 02: Shell & Navigation

## Goal

Build the single-window app shell, `INavigationService`, the composition-root registration pattern
each module will use to register routes, and the app-state singletons (current user, connectivity/
sync status, entitlement snapshot) — plus establish the Stage A/C "data seam" pattern once so every
later module phase reuses it instead of re-deciding it.

## Expected Outcome

The app launches into a real single-window shell with a region-based content host, applies RTL/
theme once at the root, shows a connectivity/sync status indicator (in a placeholder `Online` state
since `Sync/` is a P2 seam, DT-12), and can navigate to at least one placeholder screen registered
the way every future module will register its screens — with zero compile-time knowledge of that
screen in the shell project (DESK-ARCH-05).

## Scope

- `INavigationService` + route registration API in `Animora.Desktop.App/Navigation`.
- Single-window shell (`Shell/`) with a content region, consuming design tokens from phase 01 only.
- App-state singleton services (current user placeholder, connectivity/sync/licensing status
  placeholder state machine per DESK-ARCH-07/08) — real values arrive in later phases (Identity,
  Licensing); this phase defines the service shape and a placeholder implementation.
- Startup sequence skeleton per DESK-ARCH-16: host bootstrap -> (DB open: placeholder no-op until
  phase 14) -> shell shown -> background init hook (placeholder no-op until phase 26).
- **Establish and document the Stage A data seam pattern** referenced by
  [`README.md`](../../README.md#the-stage-ac-data-seam-read-before-any-module-ui-phase): define the
  convention that each module declares its own read/write interface(s) in its own project, Stage A
  composition binds an in-memory fake, Stage C composition (phase 14+) rebinds the real
  implementation. Implement this once end-to-end with one trivial example (a placeholder "Home"
  screen backed by a fake single-method interface) so later phases copy a working pattern instead of
  inventing one.
- Out of scope: any real module screen or business content.

## Key References

- [`docs/architecture/12-desktop-architecture.md`](../../../../docs/architecture/12-desktop-architecture.md) —
  navigation/shell composition, offline UX state model, command pipeline, startup sequence
  (DESK-ARCH-01..11, 16).
- [`desktop/AGENTS.md`](../../../../desktop/AGENTS.md) — DT-02, DT-09, DT-10 (Mediator-only state
  changes, shell has no compile-time screen knowledge, non-blocking long-operation UX).
- [`docs/architecture/03-solution-structure.md`](../../../../docs/architecture/03-solution-structure.md) —
  DIR-03 (module references infrastructure only through its own interfaces) — the basis for the data
  seam pattern.

## Dependencies

Requires phases 00 and 01. Every module screens phase (04-13) depends on this phase for navigation
registration and the data-seam pattern.

## Completion Criteria

- [x] Shell shows, applies RTL/theme once at the root, never per-screen (DT-06).
- [x] `INavigationService` lets a module register a route with zero shell-side edits.
- [x] Connectivity/sync/licensing status indicator exists as a non-blocking, persistent element
      (DESK-ARCH-07), currently backed by placeholder state.
- [x] The data-seam pattern (fake-now/real-later behind a module-owned interface) is implemented
      once, working, and documented well enough that phase 04+ can copy it without re-deriving it.
- [x] Startup shows the shell without waiting on any background init (DESK-ARCH-16).

### Verification evidence (2026-08-01)

- Shell + root-only RTL/theme: `App.axaml` includes `AnimoraTheme.axaml` once and
  `Theme/Styles/Root.axaml` sets `FlowDirection`/`FontFamily` on `Window`; asserted by
  `UiTests/Shell/ShellWindowSmokeTests`, and launched on Windows by the user —
  [`../../_meta/host-verification-log.md`](../../_meta/host-verification-log.md).
- Zero shell-side edits per route: `Composition/ServiceCollectionExtensions.AddReportingModule()`
  registers the Home route; `ArchTests/ShellDecouplingRules` forbids `Shell`/`Navigation` types from
  naming any `Animora.Desktop.Modules.*` namespace, and `UnitTests/Navigation` covers registry and
  navigation-service behaviour.
- Status indicator: `Shell/StatusIndicator` bound to `IAppStatusState` (placeholder `Online`),
  always present in the top bar, on no code path that blocks input.
- Data seam: `Modules.Reporting/Data/IHomeSummaryReadStore` + `InMemoryHomeSummaryReadStore`
  (`TODO(P1-21)`), swapped in one composition line and covered by the substituted-store handler test;
  referenced for later phases from [`../../README.md`](../../README.md).
- Startup: `Startup/StartupSequence` shows the shell before the background-init hook and never
  awaits it.
- Post-run look-and-feel corrections from the Windows session (reference §6): the top bar now closes
  with a `Divider` hairline (`StrokeThicknessBottom`) and the rail nav pill draws its own box at
  rail-inner-width x `NavItemHeight`, `RadiusBlock`.

---

## Step 0

Run on 2026-07-31 -> [`TODO.md`](TODO.md) (29 items). The two structural calls the list had to make —
module-facing navigation/app-state abstractions in the leaf `Animora.Desktop.UI` project with their
implementations in the composition root (AT-09 forbids a module referencing `Animora.Desktop.App`),
and `Modules.Reporting` as the home of the one example screen — are recorded in `TODO.md`'s header
rather than restated per item.
