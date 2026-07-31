# Phase 12: Licensing Status Screens

## Goal

Build the desktop-facing Licensing screens: current plan/entitlement display, device/seat list
(read view), and the offline-grace/degradation status surface, fully click-through against the
Stage A data seam, inside `Animora.Desktop.Modules.Licensing`.

## Expected Outcome

A staff member can see the tenant's current plan and entitlements, a read-only list of registered
devices, and a status view reflecting the heartbeat/offline-grace state machine
(`Fresh`/`OfflineGrace`/`ReadOnlyDegraded`) — all through real `Mediator` handlers bound to
`Modules.Licensing`'s data-seam interface (fake now, real in phase 23), wired to the same status
indicator concept phase 02 placeholder-established (DESK-ARCH-08).

## Scope

- Plan/entitlement display screen (read-only; purchase/upgrade flow is web-only per module catalog
  — desktop shows current state, never initiates a purchase).
- Device/seat list (read-only view mirroring Identity's device list from phase 04, but from the
  Licensing/seat-limit perspective — LIC-07/08 concepts shown as data, not enforced here since
  enforcement is server-side).
- Offline-grace/degradation status screen and its connection to the shell's persistent status
  indicator (DESK-ARCH-08): wire the real state machine shape now (`Fresh`/`OfflineGrace`/
  `ReadOnlyDegraded`) even though the underlying heartbeat job is a P2/phase-26 concern — this phase
  makes the UI states real and testable against fake transitions.
- `ReadOnlyDegraded` must visibly disable local write actions in a way other module screens can
  query (a shared entitlement-status service other modules' ViewModels can check for UI-only gating
  per INV-16) — this phase defines that shared service's shape; per-screen gating wiring happens
  incrementally as those screens are touched (not retrofitted into every prior phase now).
- Out of scope: real license token handling/PASETO verification (P2/backend), real heartbeat job
  (phase 26), local persistence of entitlement snapshot (phase 23).

## Key References

- [`docs/architecture/11-licensing-and-entitlements.md`](../../../../docs/architecture/11-licensing-and-entitlements.md) —
  heartbeat/offline-grace state machine, degradation behavior table (LIC-10..12), explicit anti-
  tamper limits (LIC-17, read to avoid over-building client-side enforcement that isn't the real
  control).
- [`docs/architecture/12-desktop-architecture.md`](../../../../docs/architecture/12-desktop-architecture.md) —
  DESK-ARCH-08 (status indicator reflects licensing state).
- [`docs/architecture/adr/ADR-0006-entitlement-enforcement-split.md`](../../../../docs/architecture/adr/ADR-0006-entitlement-enforcement-split.md) —
  why the token/local UI gating is UX-only, never the enforcement boundary — keeps this phase's
  scope honest (no client-side "enforcement" logic pretending to be authoritative).

## Dependencies

Requires phases 02 and 03. Feeds phase 23 (Licensing Local Data) and phase 27/26 (real heartbeat
job wiring). Provides the shared entitlement-status service other module phases may reference for
future UI gating (not retrofitted now).

## Completion Criteria

- [ ] Plan/entitlement display, device/seat list, and offline-grace status screens exist and are
      navigable.
- [ ] The `Fresh`/`OfflineGrace`/`ReadOnlyDegraded` state machine is represented in the UI and
      testable against fake transitions.
- [ ] The shared entitlement-status service exists with a clear, documented UI-only-gating contract
      (INV-16) — no code anywhere claims this is the enforcement boundary.
- [ ] Every screen passes an `Avalonia.Headless` RTL smoke test.

---

## Step 0

Not run yet. See [`../../_meta/session-protocol.md`](../../_meta/session-protocol.md).
