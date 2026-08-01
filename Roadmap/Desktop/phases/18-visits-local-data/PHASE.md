# Phase 18: Visits Local Data

## Goal

Swap phase 07's Visits Stage A fake data-seam implementation for real local persistence: EF Core
entities/configurations/migration for Visit, VisitOutcome, BiometricReading, LabResult,
VaccineDefinition, and VaccinationRecord, wired behind `Modules.Visits`'s own interface, with the
growth chart reading real biometric data and the `supersedes`-link correction flow enforced at the
data layer.

## Expected Outcome

Visit recording, biometric entry with growth chart, lab result entry, visit outcome, and vaccination
management screens from phase 07 now read/write real local SQLite rows; biometric corrections insert
a new row with a `supersedes` link rather than updating the original (DOM-09), enforced structurally
(no update path exists for a persisted `BiometricReading`).

## Scope

- EF Core entity configurations for `Visit`, `VisitOutcome` (`MutableLWW`), `BiometricReading`,
  `LabResult` (`AppendOnly` per sync class table), `VaccineDefinition`, and `VaccinationRecord` with
  `UUIDv7` PKs and appropriate sync metadata.
- EF Core migration adding these tables (DT-11).
- `BiometricReading` configured so no update path exists in `Data/Writes` for an existing row
  (DOM-09 enforced structurally, not just by convention) — a correction is always a new insert with
  a `supersedesId` column pointing at the original.
- Dapper query backing the growth chart, returning the fixed `{ series, meta }` shape (REP-11/12)
  from real persisted readings, replacing phase 07's fake series generator.
- One transaction per visit write (aggregate boundary rule, [05-domain-model.md](../../../../docs/architecture/05-domain-model.md)
  "Visit" row) covering Visit + its VisitOutcome/LabResult/BiometricReading rows created in the same
  screen action, plus the outbox row (DESK-ARCH-09).
- Rebind `Modules.Visits`'s data-seam interface to this real implementation; delete the Stage A
  fake.
- Out of scope: attachment binary/metadata persistence (phase 19 owns `Attachment`; this phase only
  persists the attachment-id reference column already established in phase 07's UI), real sync/
  outbox drain (P2).

## Key References

- [`docs/architecture/05-domain-model.md`](../../../../docs/architecture/05-domain-model.md) —
  Visit aggregate transaction boundary, DOM-09 biometric immutability + supersedes link.
- [`docs/architecture/16-reporting-and-analytics.md`](../../../../docs/architecture/16-reporting-and-analytics.md) —
  REP-11/12 chart contract shape and REP-06/07 desktop-local parity note the growth-chart query must
  satisfy exactly.
- [`docs/architecture/09-sync-architecture.md`](../../../../docs/architecture/09-sync-architecture.md) —
  `AppendOnly` sync class conflict rule for `BiometricReading`/`LabResult` (no update ever accepted).

## Dependencies

Requires phase 14, phase 07 (Visits screens + data-seam interface), and phase 16 (Clients local
data — visits reference patients). Feeds phase 21 (Reporting local data reuses this phase's
biometric/visit query shape for parity), coordinates with phase 19 (Files local data for attachment
binaries).

## Completion Criteria

- [ ] `Visit`, `VisitOutcome`, `BiometricReading`, `LabResult`, `VaccineDefinition`, and
      `VaccinationRecord` EF Core entities/configurations/migration exist with correct sync-class
      semantics.
- [ ] No code path can update a persisted `BiometricReading`; a correction test proves a new row
      with a `supersedesId` is created instead.
- [ ] Growth chart renders real data in the fixed `{ series, meta }` shape.
- [ ] One transaction covers a full visit-recording action including its outbox row.
- [ ] Phase 07's `Avalonia.Headless` smoke tests still pass unchanged against real data.

---

## Step 0

Not run yet. See [`../../_meta/session-protocol.md`](../../_meta/session-protocol.md).
