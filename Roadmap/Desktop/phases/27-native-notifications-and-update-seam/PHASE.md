# Phase 27: Native Notifications & Update Seam

## Goal

Build native Windows toast notifications in `Animora.Desktop.Infrastructure/Notifications` for
locally-fired reminders (phase 26), and the `Velopack` update-check/staged-apply seam in
`/Update` — the last two `Infrastructure` capabilities the desktop-only feature set needs before
security/performance/backup hardening.

## Expected Outcome

A fired local reminder (phase 26) shows a real Windows toast via `DesktopNotifications.Avalonia`,
falling back to the in-app toast when native toast is unavailable; the app checks a configured
update feed on a recurring local job, downloads staged/resumable updates, and prompts
non-intrusively for restart-to-apply — without ever forcing an update mid-session except where a
future sync-protocol refusal would demand it (a P2 concern, seam only for now).

## Scope

- `DesktopNotifications.Avalonia` wiring: every reminder fired by phase 26's job posts a native
  Windows toast; failure/unavailability falls back to the existing in-app `WindowNotificationManager`
  toast from phase 01, never silently drops the alert (NOTIF-05's channel-abstraction spirit applied
  to this one channel pair).
- `Velopack` update-check as a recurring local job (registered the same way as phase 26's jobs):
  check a configured feed URL (env-var/config, no hardcoded host per INV-12), download
  resumable/staged, prompt for restart non-intrusively (DESK-ARCH-19).
- The forced-update-on-sync-protocol-refusal path (SYNC-R-09) is explicitly a P2 seam here — mark it
  `// TODO(P2)` per DT-12; this phase only builds the voluntary update-check/prompt flow.
- Out of scope: the actual self-hosted Velopack feed/server (infra/deployment concern, P2/`infra/`),
  code-signing pipeline (release/CI concern, phase 31 references it, not built here).

## Key References

- [`docs/TECH_STACK.md`](../../../../docs/TECH_STACK.md) §4 "Connectivity, jobs, updates" —
  `DesktopNotifications.Avalonia`, `Velopack` as the fixed, only choices for these capabilities.
- [`docs/architecture/12-desktop-architecture.md`](../../../../docs/architecture/12-desktop-architecture.md) —
  DESK-ARCH-18/19 (delta updates, non-intrusive prompt, restart-required semantics).
- [`docs/architecture/14-jobs-and-notifications.md`](../../../../docs/architecture/14-jobs-and-notifications.md) —
  the channel abstraction table (native toast + in-app fallback) this phase implements the native
  side of.

## Dependencies

Requires phase 26 (local reminder job — the event source for native toast) and phase 01 (in-app
toast fallback already exists). Feeds phase 31 (release readiness confirms the update feed config
and signing pipeline exist operationally, not code-built here).

## Completion Criteria

- [ ] A fired local reminder shows a native Windows toast; a simulated toast-unavailable condition
      falls back to the in-app toast without losing the alert.
- [ ] The update-check job runs on schedule, reads the feed URL from configuration (no hardcoded
      value, INV-12), and can complete a staged/resumable download in a test/simulated feed.
- [ ] Restart-to-apply prompt is non-intrusive (no forced modal outside the documented P2 exception,
      which is marked `// TODO(P2)` and not implemented).
- [ ] Every job type introduced here follows phase 26's idempotency/correlation-id conventions.

---

## Step 0

Not run yet. See [`../../_meta/session-protocol.md`](../../_meta/session-protocol.md).
