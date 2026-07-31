---
id: 08-desktop-data
title: Desktop Local Data
read_when: ["adding a local SQLite table", "outbox/reminder work"]
topics: [sqlite, sqlcipher, fts5, outbox]
depends_on: [05-domain, 07-server-data]
stability: stable
---

## Contract

Decides: SQLite schema strategy and its relationship to the server schema, SQLCipher key
lifecycle, FTS5 design, the outbox/job/reminder/tombstone tables, local migration/snapshot/restore,
and storage growth/pruning. Does not decide the sync protocol itself (see [09](09-sync-architecture.md)).

## Relationship to the server schema

- The desktop SQLite schema is a **per-tenant, single-tenant mirror**: it contains only the one
  tenant's rows, so there is no `tenant_id` column or RLS need locally (DESK-01) — tenancy is a
  server-side and sync-protocol concern only once data lands on a device.
- Entity shape mirrors the server's synced-entity fields 1:1 in name and type (mapped via the same
  `Animora.Contracts`), but the desktop schema additionally carries **sync metadata columns** on
  every synced table: `hlc_timestamp`, `sync_version`, `is_dirty`, `is_tombstone`,
  `server_row_version` (DESK-02). These columns never appear in the server schema; they exist only
  to drive the sync engine (see [09](09-sync-architecture.md)).
- Non-synced, desktop-only concerns (outbox, jobs, reminders, cursors) have no server counterpart
  at all — DESK-03.

## SQLCipher key lifecycle

- One SQLCipher database file per tenant installation. Key is a 256-bit value generated on first
  run, stored via Windows DPAPI (macOS: Keychain, `[later]`) — never in a config file or the
  database itself (DESK-04).
- Key rotation: supported via SQLCipher `rekey`, triggered manually from settings or forced after a
  detected key-store compromise signal; rotation runs inside a maintenance window with the local
  scheduler paused (DESK-05).
- Key recovery: if DPAPI storage is lost (e.g., OS reinstall) the local database is unrecoverable
  by design; recovery path is re-provisioning the device and a full re-seed pull from the server,
  which is the source of truth post-sync (TECH_STACK §20) — DESK-06. This is a deliberate
  trade-off recorded in [23-architecture-risks.md](23-architecture-risks.md).

## FTS5 design

- FTS5 virtual tables shadow the searchable subset of columns for: clients (owners), animals
  (patients), invoices, prescriptions/visit notes (DESK-07), matching the server's `tsvector`
  target set (see [07](07-server-data-architecture.md)) so search UX is consistent online/offline.
- FTS5 tables are maintained via triggers on the shadowed tables (insert/update/delete keeps the
  index current) rather than rebuilt on demand, to keep search instant on old hardware.

## Outbox / job / reminder / tombstone tables

| Table | Purpose | Key columns |
|---|---|---|
| `outbox` | Durable queue of local writes not yet confirmed by the server | `id`, `entity_type`, `entity_id`, `payload`, `idempotency_key`, `attempt_count`, `status` (pending/sent/dead-letter), `created_at_hlc` |
| `sync_cursor` | Per-entity-type last successfully applied server cursor | `entity_type`, `cursor`, `updated_at` |
| `job` | Local scheduled/durable job table backing `BackgroundService` + `PeriodicTimer` (sync, reminders, local backup, update check) | `id`, `job_type`, `next_run_at`, `status`, `last_error`, `attempt_count` |
| `reminder` | Precomputed local reminders (vaccination due, appointment, cheque due) so an offline clinic still fires alerts | `id`, `source_entity_type`, `source_entity_id`, `fire_at`, `fired_at`, `channel` |
| `tombstone` | Local record of entities the user deleted locally, pending outbox confirmation, and entities the server told us were deleted | `entity_type`, `entity_id`, `deleted_at_hlc`, `origin` (local/remote) |

All five are DESK-03 (desktop-only, no server mirror by that name) except that the concept of a
tombstone also exists server-side per INV-04 — the local `tombstone` table is the desktop's queue
of tombstone events to send/receive, not a duplicate of server storage.

## Local migrations

- EF Core migrations, forward-only (matches server policy, DATA-09), but applied automatically at
  app start (unlike the server) because there is no separate deploy step on a single-user desktop
  install (DESK-08).
- Every migration runs inside a `VACUUM INTO`-based pre-migration snapshot (TECH_STACK §4) so a
  failed migration can restore the prior file and surface an error instead of corrupting the store
  — DESK-09.

## Snapshot / restore

- Scheduled encrypted local backup: periodic `VACUUM INTO` snapshot, retention window (count and
  age configurable), restore accessible from the desktop UI (Settings > Backup) — DESK-10.
- Restore always requires re-authentication and triggers a post-restore sync reconciliation pass
  (cursor validity check) before resuming normal outbox processing, since a restored snapshot may
  be older than the last server-acknowledged state (DESK-11).

## Storage growth and pruning

- Append-heavy local tables (visit history, biometric readings, notification delivery mirrors) are
  never pruned automatically — clinical/financial history must remain available offline for the
  life of the install (DESK-12).
- Prunable-by-policy tables: `audit_log` mirror (if any is kept locally) and `delivery_log` mirror
  may be pruned to a rolling window (default: 2 years) since the server retains the authoritative
  full history; pruning is a local job (`job` table) that never touches `is_dirty` rows (DESK-13).
- FTS5 index growth is bounded by the same retention as its shadowed table; no separate pruning
  logic needed.
- Growth budget target: see [01-context-and-drivers.md](01-context-and-drivers.md) "Desktop DB
  growth" quality attribute.
