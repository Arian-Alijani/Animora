# Phase 22: Notifications Local Data

## Goal

Swap phase 11's Notifications Stage A fake data-seam implementation for real local persistence:
EF Core entities/configurations/migration for NotificationPreference and the bounded local
delivery-log mirror, wired behind `Modules.Notifications`'s own interface.

## Expected Outcome

The in-app notification/job center, channel preference settings, and delivery-log mirror screens
from phase 11 now read/write real local SQLite rows; channel preferences (quiet hours, per-category
opt-in) persist across restarts; the delivery-log mirror is bounded and prunable per DESK-13
(pruning job itself remains phase 26's concern — this phase only builds the correctly-shaped,
prunable table).

## Scope

- EF Core entity configuration for `NotificationPreference` (`MutableLWW`, per module catalog
  "preferences sync") with quiet-hours window and per-category channel opt-in fields matching
  NOTIF-06's model.
- EF Core entity configuration for a bounded local `DeliveryLog` mirror table (module catalog:
  "log server-only" for the authoritative copy — this is the desktop-local mirror only, populated
  from locally-fired alerts, not a sync target), shaped for DESK-13's rolling-window pruning policy
  (pruning job is phase 26, not built here).
- EF Core migration adding these tables (DT-11).
- Rebind `Modules.Notifications`'s data-seam interface to this real implementation; delete the
  Stage A fake — the in-app feed now reads real (locally-sourced) entries once phase 26 starts
  writing them; until then this phase seeds the schema and a minimal manual-entry path so the
  screen is genuinely backed by real storage, not fake data, even before phase 26 exists.
- Out of scope: real reminder computation writing into this table (phase 26), remote channel
  delivery/logging (P2/backend), the pruning job itself (phase 26).

## Key References

- [`docs/architecture/14-jobs-and-notifications.md`](../../../../docs/architecture/14-jobs-and-notifications.md) —
  NOTIF-04 (bounded local delivery-log mirror), NOTIF-06 (quiet hours model) this phase's schema
  must match exactly.
- [`docs/architecture/08-desktop-local-data.md`](../../../../docs/architecture/08-desktop-local-data.md) —
  DESK-13 pruning policy for the delivery-log mirror (schema must support it even though the job
  runs later).
- [`docs/architecture/20-extensibility-playbook.md`](../../../../docs/architecture/20-extensibility-playbook.md) —
  "add a CRUD entity" recipe steps 1-3 applied to `NotificationPreference`.

## Dependencies

Requires phase 14 and phase 11 (Notifications screens + data-seam interface). Feeds phase 26 (local
reminder computation writes into this phase's delivery-log mirror and reads its preferences).

## Completion Criteria

- [ ] `NotificationPreference` and the local `DeliveryLog` mirror EF Core entities/configurations/
      migration exist.
- [ ] Channel preference settings screen persists and reloads real values across an app restart.
- [ ] Delivery-log mirror table schema supports DESK-13's rolling-window pruning (column shape
      verified, pruning job itself not implemented here).
- [ ] Phase 11's `Avalonia.Headless` smoke tests still pass unchanged against real data.

---

## Step 0

Not run yet. See [`../../_meta/session-protocol.md`](../../_meta/session-protocol.md).
