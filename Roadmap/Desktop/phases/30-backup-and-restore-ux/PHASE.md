# Phase 30: Backup & Restore UX

## Goal

Build the desktop-local encrypted backup/restore feature end-to-end: scheduled `VACUUM INTO`
snapshots (triggered by phase 26's recurring job), a retention window, and a Settings > Backup UI
for manual backup and restore, including the mandatory post-restore re-authentication and sync
reconciliation placeholder.

## Expected Outcome

A staff member can see backup status/history in Settings > Backup, trigger a manual backup, and
restore from a prior encrypted snapshot; restore always requires re-authentication and marks a
post-restore reconciliation flag for when P2 sync exists, per DESK-11.

## Scope

- `VACUUM INTO`-based encrypted snapshot mechanics in `Animora.Desktop.Data/Backup`, retention
  window (count and age configurable) — the scheduling trigger point was registered in phase 26;
  this phase implements what that trigger calls.
- Settings > Backup screen: backup history/status list, manual "back up now" action, restore action
  with a confirmation step (destructive-operation UX pattern).
- Restore flow requires re-authentication (reuses phase 04's login screen/flow) before completing,
  and sets a "post-restore reconciliation pending" flag consumed by the (currently inert, P2) sync
  engine per DESK-11 — mark the actual reconciliation-pass trigger `// TODO(P2)` per DT-12.
- Out of scope: the real sync reconciliation pass itself (P2, only the flag/seam is built here),
  server-side backup (`pgBackRest`/`rclone`, infra concern, unrelated to this desktop-local
  feature).

## Key References

- [`docs/architecture/08-desktop-local-data.md`](../../../../docs/architecture/08-desktop-local-data.md) —
  DESK-10/11 snapshot/restore rules (retention, re-authentication requirement, post-restore
  reconciliation pass) this phase implements exactly.
- [`docs/architecture/18-observability-and-operations.md`](../../../../docs/architecture/18-observability-and-operations.md) —
  OBS-13 framing: desktop backup is for fast local recovery, not the tenant's disaster-recovery
  plan of record (the server copy is, once P2/sync exists) — keeps this phase's scope honest.
- [`docs/architecture/20-extensibility-playbook.md`](../../../../docs/architecture/20-extensibility-playbook.md) —
  "add a desktop screen" recipe for the Settings > Backup screen.

## Dependencies

Requires phase 14 (local data platform), phase 26 (recurring job trigger point for scheduled
backups), and phase 04 (login flow reused for restore re-authentication).

## Completion Criteria

- [ ] Scheduled `VACUUM INTO` snapshots run on the retention policy and old snapshots are pruned
      per the configured window.
- [ ] Settings > Backup shows real backup history and supports a manual "back up now" action.
- [ ] Restore requires re-authentication and completes successfully from a real snapshot in a test.
- [ ] Restore sets the post-restore reconciliation flag, explicitly marked `// TODO(P2)` for the
      real reconciliation pass.
- [ ] Every new screen passes an `Avalonia.Headless` RTL smoke test.

---

## Step 0

Not run yet. See [`../../_meta/session-protocol.md`](../../_meta/session-protocol.md).
