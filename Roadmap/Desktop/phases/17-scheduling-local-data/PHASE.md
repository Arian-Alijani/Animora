# Phase 17: Scheduling Local Data

## Goal

Swap phase 06's Scheduling Stage A fake data-seam implementation for real local persistence: EF
Core entities/configurations/migration for Appointment, Resource, ServiceType, GroomingType, wired
behind `Modules.Scheduling`'s own interface, with the local overlap check running against real
data.

## Expected Outcome

The calendar, booking/reschedule/cancel dialogs, and resource/service-type/grooming-type management
screens from phase 06 now read/write real local SQLite rows; the DOM-08 overlap check runs a real
Dapper query against locally persisted appointments instead of an in-memory fake list; reschedule
mutates the same `Appointment` row (ADR-0008), never inserting a new one.

## Scope

- EF Core entity configurations for `Appointment` (`StateMachine` sync class), `Resource`,
  `ServiceType`, `GroomingType` (`MutableLWW`), with `UUIDv7` PKs, tombstones, sync metadata columns
  (DESK-02).
- EF Core migration adding these tables (DT-11).
- Dapper query implementing the real overlap check (DOM-08) against a resource's booked time ranges
  for the local dataset, replacing phase 06's in-memory fake check — same client-side-only caveat
  (`// TODO(P2)` for server re-validation) still applies.
- Calendar read queries (day/week/month, per-resource) as Dapper queries, keyset/range-bounded
  (DT-08 spirit applied to date-range queries).
- Every Appointment write (create/reschedule/cancel/state transition) performs local write +
  outbox row in one transaction (DESK-ARCH-09); reschedule updates the existing row's fields, never
  inserts a new `Appointment` (ADR-0008).
- Rebind `Modules.Scheduling`'s data-seam interface to this real implementation; delete the Stage A
  fake.
- Out of scope: reminder computation (phase 26 uses this phase's due-date data), real sync/outbox
  drain (P2), cross-device conflict resolution for concurrent bookings (P2/sync test matrix).

## Key References

- [`docs/architecture/05-domain-model.md`](../../../../docs/architecture/05-domain-model.md) —
  Appointment state machine, DOM-08 overlap-check rule this phase must implement against real data.
- [`docs/architecture/adr/ADR-0008-calendar-scheduling-data-model.md`](../../../../docs/architecture/adr/ADR-0008-calendar-scheduling-data-model.md) —
  why reschedule mutates the same row; the local persistence layer must honor this exactly.
- [`docs/architecture/09-sync-architecture.md`](../../../../docs/architecture/09-sync-architecture.md) —
  `StateMachine` sync class conflict rule (for Appointment) to keep the local write shape compatible
  with the eventual sync engine.

## Dependencies

Requires phase 14 and phase 06 (Scheduling screens + data-seam interface), and phase 16 (Clients
local data — appointments reference patients/owners by id). Feeds phase 26 (local reminder job
reads due appointment dates from this phase's real tables).

## Completion Criteria

- [ ] `Appointment`, `Resource`, `ServiceType`, `GroomingType` EF Core entities/configurations/
      migration exist with correct sync-class metadata.
- [ ] The DOM-08 overlap check runs as a real Dapper query against persisted appointments and
      rejects an overlapping booking in a test.
- [ ] Reschedule updates the existing `Appointment` row's fields; no new row is created (verified by
      a test asserting row count is unchanged and id is preserved).
- [ ] Calendar screens render real data across day/week/month/resource views.
- [ ] Phase 06's `Avalonia.Headless` smoke tests still pass unchanged against real data.

---

## Step 0

Not run yet. See [`../../_meta/session-protocol.md`](../../_meta/session-protocol.md).
