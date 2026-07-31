# ADR-0008: Calendar/Scheduling Data Model

## Status

Accepted

## Context

The product needs a "complete and attractive calendar (day/week/month, resource views)" spanning
multiple doctors and grooming/service resources, with easy rescheduling, while remaining a
synced, offline-capable entity. See [05-domain-model.md](../05-domain-model.md),
[04-module-catalog.md](../04-module-catalog.md).

## Decision

`Appointment` is a single-row `StateMachine`-classed synced entity per booking (not an event-log of
booking changes); rescheduling mutates the same row's time/resource fields rather than creating a
new appointment or a separate "reschedule event" entity. A resource-availability check (DOM-08)
runs both server-side (authoritative) and desktop-side (offline UX, re-validated on sync) against
the same overlap rule.

## Alternatives considered

| Alternative | Why rejected |
|---|---|
| Event-sourced appointment history (append-only event log, current state derived) | Matches ledger-style immutability nicely, but adds real complexity (event replay, snapshotting) not justified for a booking record; overlap-check and calendar rendering become harder to reason about, and no other entity in this system uses full event-sourcing, so it would be an inconsistent pattern |
| Separate `AppointmentSeries` row per single booking too (not just recurring) | Unnecessary indirection for the common single-booking case; `AppointmentSeries` is reserved for genuinely recurring bookings only |
| Optimistic-only overlap checking (server-side only, no local check) | Would let a desktop user book an obviously-conflicting slot while offline with no feedback until sync, harming UX for the offline-first promise |

## Consequences

- Positive: simple `MutableLWW`/`StateMachine` sync behavior consistent with the rest of the
  entity catalog; audit history for changes still available via the general `audit_log` (SEC-15),
  not via appointment-specific event sourcing.
- Negative / accepted trade-off: a detailed "who changed what and when" appointment history relies
  on the audit log rather than a purpose-built event stream; acceptable since audit_log already
  captures this for every entity.
- Follow-up docs affected: [05-domain-model.md](../05-domain-model.md) (appointment state machine),
  [09-sync-architecture.md](../09-sync-architecture.md) (StateMachine class rules).
