# Phase 14: Local Data Platform Foundation

## Goal

Build the shared local-data platform in `Animora.Desktop.Data` — SQLCipher-encrypted SQLite,
`DbContext` bootstrap, the migration/pre-migration-snapshot pipeline, FTS5 scaffolding, and the
desktop-only outbox/cursor/job/reminder/tombstone tables — so every module's Stage C phase
(15-23) has a working, tested foundation to plug entities into instead of each reinventing it.

## Expected Outcome

`Animora.Desktop.Data` builds a real SQLCipher-encrypted SQLite database at app startup, applies
EF Core migrations forward-only with a `VACUUM INTO` pre-migration snapshot, exposes the five
desktop-only tables (`outbox`, `sync_cursor`, `job`, `reminder`, `tombstone`) ready for module data
to reference, and demonstrates one working FTS5 virtual table end-to-end. No module entity is
migrated yet — that is each module's own Stage C phase.

## Scope

- SQLCipher key lifecycle: 256-bit key generated on first run, stored via Windows DPAPI (DESK-04),
  wired into the SQLite connection string at `DbContext` construction.
- `Animora.Desktop.Data` `DbContext` bootstrap (EF Core writes) and a separate Dapper connection
  factory (hot reads) — never mixed in the same sub-namespace (DT-05, AT-04/AT-05).
- Local migrations pipeline: EF Core migrations, forward-only, applied automatically at app start
  inside a `VACUUM INTO`-based pre-migration snapshot so a failed migration restores the prior file
  (DESK-08/DESK-09).
- The five desktop-only tables (`outbox`, `sync_cursor`, `job`, `reminder`, `tombstone`) per the
  schema in [08-desktop-local-data.md](../../../../docs/architecture/08-desktop-local-data.md) —
  structure and columns only; the sync engine that drains the outbox is a P2 concern (DT-12), the
  local job scheduler that uses `job`/`reminder` is phase 26.
- FTS5 scaffolding: the virtual-table + trigger pattern proven on one representative shadowed
  table, ready for phases 16 (owners/patients), 09/20 (invoices) to extend.
- Replace phase 02's Stage A fake data-seam wiring point with the composition hook Stage C phases
  will use to rebind real implementations — no module's interface changes shape, only what backs it
  (the pattern phase 02 already documented).
- Out of scope: any module entity's EF Core configuration/migration (each module's own phase 15-23
  does its own), the outbox drain/sync engine itself (P2), the local job scheduler's actual jobs
  (phase 26), local encrypted backup/restore (phase 30).

## Key References

- [`docs/architecture/08-desktop-local-data.md`](../../../../docs/architecture/08-desktop-local-data.md) —
  SQLCipher key lifecycle (DESK-04/05/06), FTS5 design (DESK-07), the five-table schema, migration/
  snapshot rules (DESK-08/09).
- [`docs/architecture/adr/ADR-0002-local-schema-mirroring.md`](../../../../docs/architecture/adr/ADR-0002-local-schema-mirroring.md) —
  why desktop mirrors server entity shape 1:1 plus sync metadata columns, framing how every later
  module migration must be authored.
- [`desktop/AGENTS.md`](../../../../desktop/AGENTS.md) — DT-05 (EF Core writes / Dapper reads split)
  and DT-11 (new table -> EF Core migration, forward-only, pre-migration snapshot).

## Dependencies

Requires phase 13 (gate signed off). Every module's local-data phase (15-23) requires this phase
`complete` before it starts.

## Completion Criteria

- [ ] SQLite database opens SQLCipher-encrypted with a DPAPI-stored key on first run and on
      subsequent runs.
- [ ] `Data/Writes` (EF Core) and `Data/Queries` (Dapper) exist as separate sub-namespaces with no
      cross-reference (AT-04/AT-05).
- [ ] A migration applies successfully with a `VACUUM INTO` pre-migration snapshot taken first, and
      a simulated failed migration restores the prior file instead of corrupting it.
- [ ] `outbox`, `sync_cursor`, `job`, `reminder`, `tombstone` tables exist with the documented key
      columns.
- [ ] One FTS5 virtual table + maintenance triggers work end-to-end on a representative table.
- [ ] Startup still shows the shell without blocking on migration/DB-open exceeding the phase 02
      startup budget expectation (DESK-ARCH-16).

---

## Step 0

Not run yet. See [`../../_meta/session-protocol.md`](../../_meta/session-protocol.md).
