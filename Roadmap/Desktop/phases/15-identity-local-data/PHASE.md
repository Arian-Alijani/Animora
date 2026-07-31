# Phase 15: Identity Local Data

## Goal

Swap phase 04's Identity Stage A fake data-seam implementation for real local persistence: EF Core
entities/configurations/migration for User, Role, PermissionClaim, Device in
`Animora.Desktop.Data`, wired behind `Modules.Identity`'s own interface.

## Expected Outcome

The staff list, staff create/edit, role management, and device list screens from phase 04 now read
and write real local SQLite rows through `Modules.Identity`'s data-seam interface, with zero
changes to any `ViewModel`, `View`, or `Handler` signature from phase 04 — only the concrete
implementation behind the interface changes (the Stage A/C seam pattern from phase 02).

## Scope

- EF Core entity configurations for `User`, `Role`, `PermissionClaim`, `Device` in
  `Animora.Desktop.Data/Entities` and `/Configurations`, mirroring server field shape 1:1 plus sync
  metadata columns (DESK-02) even though sync itself is inert until P2.
- EF Core migration (forward-only, DT-11) adding these tables to the local schema built in phase 14.
- Dapper queries in `Animora.Desktop.Data/Queries` for the staff list (keyset-paginated if it can
  exceed 200 rows, DT-08) and role/permission-claim reads.
- Rebind `Modules.Identity`'s data-seam interface (declared in phase 04) to this real
  implementation in composition; delete the Stage A fake implementation.
- Local-only authorization check mirroring the server's permission model shape (SEC-12 concept) for
  UI-level gating only — never claimed as the enforcement boundary (INV-09, deferred to P2 backend).
- `User`/`Role`/`PermissionClaim` are `ReferenceOnly`/non-synced per module catalog ("No (server
  authoritative)"); `Device` mirrors Identity's device registration shape for local display only —
  no real sync class wiring happens here (P2).
- Out of scope: real authentication/token issuance (P2), real server-side permission enforcement
  (P2), FTS5 search on staff (not in the report/search target set per DESK-07).

## Key References

- [`docs/architecture/08-desktop-local-data.md`](../../../../docs/architecture/08-desktop-local-data.md) —
  entity-mirroring rules and sync metadata columns (DESK-02) to apply even to a non-synced module's
  tables, for schema consistency.
- [`docs/architecture/04-module-catalog.md`](../../../../docs/architecture/04-module-catalog.md) —
  Identity's owned entities and "No (server authoritative)" sync classification.
- [`docs/architecture/20-extensibility-playbook.md`](../../../../docs/architecture/20-extensibility-playbook.md) —
  "add a CRUD entity" recipe steps 1-3 (entity, migration, validation) applied to the desktop-local
  side.

## Dependencies

Requires phase 14 (local data platform) and phase 04 (Identity screens + data-seam interface).
Every later local-data phase's staff/role references (e.g., appointments booked by a staff member)
assume this phase's `User`/`Role` tables exist.

## Completion Criteria

- [ ] `User`, `Role`, `PermissionClaim`, `Device` EF Core entities, configurations, and one
      migration exist and apply cleanly on top of phase 14's schema.
- [ ] Staff list, staff create/edit, role management, and device list screens from phase 04 work
      unchanged against real local data (no ViewModel/View/Handler signature changed).
- [ ] Staff list uses a keyset-paginated Dapper query if the list can exceed 200 rows (DT-08).
- [ ] No `Modules.Identity` type outside `*.Data` sub-namespace references EF Core/Dapper types
      directly (AT-03).
- [ ] Phase 04's `Avalonia.Headless` smoke tests still pass unchanged against real data.

---

## Step 0

Not run yet. See [`../../_meta/session-protocol.md`](../../_meta/session-protocol.md).
