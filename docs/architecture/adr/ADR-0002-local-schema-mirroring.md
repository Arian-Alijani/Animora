# ADR-0002: Local Schema Mirroring Strategy

## Status

Accepted

## Context

The desktop must hold a full, writable copy of a tenant's data offline. TECH_STACK fixes SQLite +
SQLCipher as the local store and EF Core 10 for writes on both server and desktop. A strategy is
needed for how closely the local schema tracks the server schema. See
[08-desktop-local-data.md](../08-desktop-local-data.md).

## Decision

Mirror synced entities 1:1 in field name/type between server (Postgres) and desktop (SQLite),
sharing the same `Animora.Contracts` definitions, but add desktop-only sync metadata columns
(`hlc_timestamp`, `sync_version`, `is_dirty`, `is_tombstone`, `server_row_version`) that never
exist server-side. The desktop store is single-tenant (no `tenant_id` column needed locally).

## Alternatives considered

| Alternative | Why rejected |
|---|---|
| Fully independent local schema, mapped via a translation layer | Doubles the maintenance surface for every entity change; contradicts INV-02's single-business-logic intent by inviting drift |
| Store sync metadata server-side too, in the same tables | Pollutes the canonical Postgres schema with per-device bookkeeping that has no server meaning; server already tracks per-device state in `sync_cursor`/`conflict_log` at the Sync module level, not per-row |
| Multi-tenant local schema (mirroring server's tenant_id + RLS) | Unnecessary: one desktop install serves one tenant (product scope); adds RLS-equivalent complexity to SQLite for zero benefit |

## Consequences

- Positive: one shared contract definition per entity; schema evolution recipe
  ([20-extensibility-playbook.md](../20-extensibility-playbook.md)) touches both schemas in lockstep
  by construction.
- Negative / accepted trade-off: every synced-entity migration is authored twice (EF Core migration
  for Postgres, EF Core migration for SQLite); mitigated by shared contract types reducing the
  chance of field-level drift.
- Follow-up docs affected: [07-server-data-architecture.md](../07-server-data-architecture.md),
  [08-desktop-local-data.md](../08-desktop-local-data.md), [09-sync-architecture.md](../09-sync-architecture.md).
