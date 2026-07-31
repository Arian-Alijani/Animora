# Phase 26: Local Job Scheduler & Reminders

## Goal

Build the real local job infrastructure in `Animora.Desktop.Infrastructure/Jobs`:
`BackgroundService` + `PeriodicTimer` over the `job`/`reminder` tables from phase 14, with the
first real jobs being local reminder computation (ADR-0009), long-operation tracking for phase 24's
document generation, and local backup snapshot scheduling (feeding phase 30).

## Expected Outcome

Reminders (vaccination due, appointment, cheque due) compute and fire locally against real synced
data on a recurring schedule, surfacing in phase 11/22's in-app notification feed and (once phase 27
exists) native toast; report exports and invoice printing from phase 24 run as real tracked `job`
rows with non-blocking progress instead of instant fake completion; every job is idempotent,
cancellable, and logged with a correlation id (INV-11/JOB-01..04).

## Scope

- `BackgroundService` + `PeriodicTimer` scheduler over the `job` table (phase 14 schema), capped
  concurrency sized for old hardware, cooperative scheduling (never a long CPU-bound loop on the UI
  thread, JOB-03).
- Local reminder computation job: evaluates the alert-source catalog's time-based rules
  ([14-jobs-and-notifications.md](../../../../docs/architecture/14-jobs-and-notifications.md))
  against phase 17 (Scheduling), phase 18 (Visits/biometric schedule), phase 20 (cheque due dates)
  local data, writing into the `reminder` table and phase 22's delivery-log mirror (ADR-0009: this
  computation is intentionally independent of any server-side equivalent).
- Wrap phase 24's document generation (report export, invoice print) as tracked `job` rows with
  real progress reporting to the job-center UI built in phase 10/11, replacing their remaining
  placeholder completion semantics.
- Local backup snapshot as a recurring job (scheduling only — the `VACUUM INTO` mechanics and
  restore UI are phase 30's scope; this phase registers the recurring trigger point).
- Every job type explicitly states its idempotency/completion-detection mechanism per JOB-01; every
  job logs a correlation id threaded to any notification it triggers (JOB-04).
- Out of scope: real sync-cycle job (P2, `Animora.Desktop.Sync` stays an empty seam per DT-12),
  update-check job (phase 27 owns it), backup restore UX (phase 30).

## Key References

- [`docs/architecture/14-jobs-and-notifications.md`](../../../../docs/architecture/14-jobs-and-notifications.md) —
  job taxonomy (desktop local recurring row), JOB-01..04 idempotency/concurrency/correlation rules,
  the alert-source catalog this phase's reminder job must be able to evaluate.
- [`docs/architecture/adr/ADR-0009-desktop-offline-job-ownership.md`](../../../../docs/architecture/adr/ADR-0009-desktop-offline-job-ownership.md) —
  why reminder computation is intentionally duplicated locally rather than waiting on a server.
- [`docs/architecture/08-desktop-local-data.md`](../../../../docs/architecture/08-desktop-local-data.md) —
  the `job`/`reminder` table schema this scheduler drives.

## Dependencies

Requires phase 14 (job/reminder tables), phase 17 (Scheduling local data), phase 18 (Visits local
data), phase 20 (Finance local data — cheque due dates), phase 22 (Notifications local data —
delivery-log mirror target), and phase 24 (Documents & Printing — the payload jobs wrap). Feeds
phase 27 (native toast consumes fired reminders) and phase 30 (backup job trigger point).

## Completion Criteria

- [ ] The scheduler runs recurring jobs from the `job` table without blocking the UI thread.
- [ ] Local reminder computation fires for at least vaccination/appointment/cheque-due sources
      against real local data, visible in the in-app notification feed.
- [ ] Report export and invoice print run as tracked `job` rows with real, non-blocking progress.
- [ ] Every job type documents its idempotency mechanism in a code comment per JOB-01/CM-03; a
      "run twice" test proves no duplicate effect.
- [ ] Every job's log line carries a correlation id.

---

## Step 0

Not run yet. See [`../../_meta/session-protocol.md`](../../_meta/session-protocol.md).
