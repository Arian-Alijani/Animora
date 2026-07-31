# Phase 11: Notifications Module Screens

## Goal

Build every screen the Notifications module owns on desktop: the in-app notification/job center,
channel preference settings, and the delivery-log mirror view, fully click-through against the
Stage A data seam, inside `Animora.Desktop.Modules.Notifications`.

## Expected Outcome

A staff member can see an in-app feed of alerts (matching the alert-source catalog), configure
per-user channel preferences (quiet hours, per-channel opt-in where applicable), and view a bounded
local delivery-log mirror — all through real `Mediator` handlers bound to
`Modules.Notifications`'s data-seam interface (fake now, real in phase 22).

## Scope

- In-app notification/job center screen (same ViewModel pattern as report export job UX per
  DESK-ARCH-11 — reused, not reinvented).
- Channel preference settings screen (quiet hours window, per-category channel opt-in, matching
  NOTIF-06's quiet-hours concept and the alert-source catalog's default-channels column).
- Delivery-log mirror view (bounded, matches DESK-13 pruning policy conceptually — this phase builds
  the screen; the pruning job itself is phase 26/22).
- Native toast is NOT built here — that is phase 27 (Windows toast via `DesktopNotifications.
  Avalonia`); this phase only builds the in-app screens.
- Out of scope: local persistence of preferences/delivery-log mirror (phase 22), actual reminder
  computation (phase 26), remote channel delivery (P2/backend).

## Key References

- [`docs/architecture/14-jobs-and-notifications.md`](../../../../docs/architecture/14-jobs-and-notifications.md) —
  alert-source catalog (the exact set of alert types the feed must be able to render), channel
  abstraction table, quiet hours/throttling (NOTIF-06/07).
- [`docs/architecture/12-desktop-architecture.md`](../../../../docs/architecture/12-desktop-architecture.md) —
  DESK-ARCH-11 job/long-operation UX pattern to reuse for the notification/job center.
- [`docs/architecture/20-extensibility-playbook.md`](../../../../docs/architecture/20-extensibility-playbook.md) —
  "add a desktop screen" recipe.

## Dependencies

Requires phases 02 and 03. Feeds phase 22 (Notifications Local Data) and is extended by phase 26
(local reminder computation feeds this feed) and phase 27 (native toast complements this in-app
feed).

## Completion Criteria

- [ ] In-app notification/job center, channel preference settings, and delivery-log mirror screens
      exist and are navigable.
- [ ] The feed can render every alert-source catalog type against fake data.
- [ ] Quiet-hours and channel-preference UI exists per NOTIF-06.
- [ ] Every screen passes an `Avalonia.Headless` RTL smoke test.

---

## Step 0

Not run yet. See [`../../_meta/session-protocol.md`](../../_meta/session-protocol.md).
