# Phase 06: Scheduling Module Screens

## Goal

Build every screen the Scheduling module owns: the calendar (day/week/month, resource views),
appointment booking/reschedule/cancel, and resource/service-type/grooming-type management, fully
click-through against the Stage A data seam, inside `Animora.Desktop.Modules.Scheduling`.

## Expected Outcome

A staff member can view a calendar in day/week/month and per-resource views, book/reschedule/cancel
an appointment against a resource, and manage the resource/service-type/grooming-type catalogs —
all through real `Mediator` handlers bound to `Modules.Scheduling`'s data-seam interface (fake now,
real in phase 17).

## Scope

- Calendar screen: day/week/month views, resource-filtered view, using `Appointment`'s state
  machine (Requested/Confirmed/CheckedIn/Completed/Rescheduled/Cancelled/NoShow, see domain model).
- Appointment booking dialog: create/edit against a resource and time slot, with the local overlap
  check per DOM-08 (client-side UX check now; server-side re-validation is a P2 concern, mark with
  `// TODO(P2): ...`).
- Resource management (doctors/groomers/rooms), service-type and grooming-type catalog screens.
- Reschedule flow mutates the same `Appointment` row per ADR-0008 (never creates a new row/event).
- Out of scope: local SQLite persistence (phase 17), reminder computation (phase 26), real
  cross-device conflict resolution (P2/sync).

## Key References

- [`docs/architecture/05-domain-model.md`](../../../../docs/architecture/05-domain-model.md) —
  Appointment state machine and DOM-08 (overlap check both sides).
- [`docs/architecture/adr/ADR-0008-calendar-scheduling-data-model.md`](../../../../docs/architecture/adr/ADR-0008-calendar-scheduling-data-model.md) —
  why appointment is single-row StateMachine, not event-sourced; frames how the reschedule UI must
  behave (mutate in place).
- [`docs/architecture/20-extensibility-playbook.md`](../../../../docs/architecture/20-extensibility-playbook.md) —
  "add a desktop screen" recipe.

## Dependencies

Requires phases 02, 03, and 05 (appointments reference patients/owners for booking context). Feeds
phase 17 (Scheduling Local Data) and phase 26 (reminder computation reads Scheduling's due dates).

## Completion Criteria

- [ ] Calendar renders day/week/month and per-resource views against fake data.
- [ ] Booking/reschedule/cancel dialogs work end-to-end through `Mediator` handlers.
- [ ] Local overlap check (DOM-08) runs client-side with an explicit `// TODO(P2)` for server
      re-validation.
- [ ] Resource/service-type/grooming-type management screens exist.
- [ ] Every screen passes an `Avalonia.Headless` RTL smoke test.

---

## Step 0

Run on 2026-08-02 -> [`TODO.md`](TODO.md) (47 items). The seven calls the list had to make — one
calendar route for day/week/month plus the resource filter, separate commands for booking /
reschedule / cancel / status over the one ADR-0008 row, the DOM-08 overlap check in the handler
rather than the validator, no `Modules.Clients` reference for the patient a booking needs, two
catalog entities with one screen each, only the synced-entity recipe's sync-class declaration in
scope, and `AppointmentSeries`/`ReminderRule` deferred — are recorded in `TODO.md`'s header rather
than restated per item. The five questions the corpus does not answer are asked as items 2-6 instead
of guessed (AG-02).
