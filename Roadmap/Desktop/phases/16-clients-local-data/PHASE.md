# Phase 16: Clients Local Data

## Goal

Swap phase 05's Clients Stage A fake data-seam implementation for real local persistence: EF Core
entities/configurations/migration for Owner, Patient, MedicalFile header, plus FTS5 search over
owners/patients, wired behind `Modules.Clients`'s own interface.

## Expected Outcome

Owner and patient list/search/create/edit screens and the medical file summary screen from phase 05
now read/write real local SQLite rows, with FTS5-backed instant search, and every write to these
`MutableLWW`-classed synced entities enqueues an outbox row in the same transaction (DESK-ARCH-09)
— even though nothing drains that outbox until P2.

## Scope

- EF Core entity configurations for `Owner`, `Patient`, `MedicalFile` header in
  `Animora.Desktop.Data/Entities`/`/Configurations`, `UUIDv7` primary keys (INV-03), tombstone
  column present (INV-04), full sync metadata columns (DESK-02) since these are `MutableLWW` synced
  entities (module catalog).
- EF Core migration adding these tables (DT-11) on top of phase 14's schema.
- FTS5 virtual tables shadowing owner/patient searchable columns (DESK-07), triggers keeping the
  index current on insert/update/delete.
- Dapper queries for owner list, patient list (global and owner-scoped), keyset-paginated (DT-08).
- Every command handler that writes Owner/Patient/MedicalFile performs the local write and an
  `outbox` row insert in one SQLite transaction (DESK-ARCH-09) — the outbox row is written and
  parked; nothing drains it until P2's sync engine exists (DT-12).
- Rebind `Modules.Clients`'s data-seam interface to this real implementation; delete the Stage A
  fake.
- Out of scope: the actual sync/outbox-drain engine (P2), the full sync test matrix for
  Owner/Patient (that runs against `Animora.SyncTests`/server once P2 exists — this phase only
  proves the local write+outbox transactionality, not cross-device convergence).

## Key References

- [`docs/architecture/08-desktop-local-data.md`](../../../../docs/architecture/08-desktop-local-data.md) —
  FTS5 design (DESK-07) for exactly this entity set, sync metadata columns (DESK-02).
- [`docs/architecture/12-desktop-architecture.md`](../../../../docs/architecture/12-desktop-architecture.md) —
  DESK-ARCH-09 (local write + outbox in one transaction) — the concrete pattern this phase
  implements for the first synced entities in the roadmap.
- [`docs/architecture/20-extensibility-playbook.md`](../../../../docs/architecture/20-extensibility-playbook.md) —
  "add a synced entity" recipe steps 3-4 (UUIDv7 + tombstone, mirror in desktop schema with sync
  metadata columns).

## Dependencies

Requires phase 14 and phase 05 (Clients screens + data-seam interface). Feeds phase 18 (Visits
Local Data references patients), phase 09/20 (Finance references owners), phase 19 (Files
attachment references).

## Completion Criteria

- [ ] `Owner`, `Patient`, `MedicalFile` EF Core entities/configurations/migration exist with
      `UUIDv7` PKs and tombstone columns.
- [ ] FTS5 search backs the owner/patient search UI from phase 05 with real instant results.
- [ ] Every Owner/Patient/MedicalFile write inserts its local row and its `outbox` row in one
      transaction, verified by a test that inspects both tables after a single command.
- [ ] Owner/patient lists use keyset-paginated Dapper queries (DT-08).
- [ ] Phase 05's `Avalonia.Headless` smoke tests still pass unchanged against real data.

---

## Step 0

Not run yet. See [`../../_meta/session-protocol.md`](../../_meta/session-protocol.md).
