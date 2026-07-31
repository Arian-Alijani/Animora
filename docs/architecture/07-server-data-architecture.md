---
id: 07-server-data
title: Server Data Architecture
read_when: ["adding a table", "writing an RLS policy", "a report query"]
topics: [postgres, rls, indexing, tsvector, migrations]
depends_on: [05-domain]
stability: stable
---

## Contract

Decides: PostgreSQL schema strategy, tenancy/RLS mechanics, key/index design rules, search design,
audit/ledger table shape, migration policy, and the EF-vs-Dapper read/write split. Does not decide
SQLite mirroring (see [08](08-desktop-local-data.md)) or sync mechanics (see [09](09-sync-architecture.md)).

## Schema strategy

- One PostgreSQL 18 database, one schema per bounded context matching the module names (`clients`,
  `visits`, `scheduling`, `finance`, `identity`, `licensing`, `files`, `sync`, `notifications`,
  `platform_admin`) — DATA-01. This keeps module ownership visible in `\dn` without requiring
  separate databases (which would break single-VPS resource sharing and cross-module transactions
  the platform genuinely needs, e.g., invoice issuance touching Finance only, never another schema).
- No table is written to by more than one module's EF Core `DbContext` (enforced together with
  INV-01 at the application layer, not by Postgres grants, to keep local dev simple).
- Every tenant-scoped table has `tenant_id uuid NOT NULL` as its first non-PK column (DATA-02).

## Tenancy and RLS

- RLS is enabled on every tenant-scoped table (INV-08). Policy: `USING (tenant_id = current_setting('app.tenant_id')::uuid)`.
- The API sets `app.tenant_id` via `SET LOCAL` inside the same transaction as the request's unit of
  work, populated from the authenticated principal — never from a client-supplied header or body
  field (DOM-02). This is DATA-03.
- RLS is defense in depth: every EF Core query MUST also carry an explicit `TenantId` filter
  (global query filter configured once per `DbContext`), so a missing `SET LOCAL` fails closed
  (empty result) rather than open. Testcontainers RLS suite (TECH_STACK §16) asserts this for every
  tenant-scoped table — DATA-04.
- Platform-level tables (`plan`, permission-claim catalog, platform_admin.* except tenant-status
  views) have RLS disabled and are reachable only by the `PlatformAdmin` module's dedicated
  connection role.

## Key design rules

- Primary keys: `UUIDv7` on every synced entity (INV-03); ordinary server-only lookup/reference
  tables (e.g., static enum-backing tables, if any) may use a small integer surrogate — never on a
  table that ever needs to sync.
- Foreign keys are always `(tenant_id, id)` composite-validated at the application layer even when
  the physical FK is on `id` alone, to keep DOM-03 (no cross-tenant FK) testable.
- Natural/business keys (e.g., invoice number) are unique per tenant via a partial unique index
  `UNIQUE (tenant_id, invoice_number)`, never global.

## Indexing and partitioning posture

- Every foreign key column is indexed. Every list endpoint's default sort key has a matching index
  supporting keyset pagination (`(tenant_id, sort_key, id)`) — DATA-05.
- High-volume append-only tables (`ledger_entry`, `audit_log`, `conflict_log`, `delivery_log`) are
  range-partitioned by month on `created_at` (DATA-06) to keep autovacuum and index size bounded on
  the 4 GB host; partition creation is a Hangfire recurring job, not manual DBA work.
- No partitioning on normal entity tables (`patient`, `visit`, etc.) — expected per-tenant volumes
  do not justify the operational complexity on a single small VPS.

## Full-text search (`tsvector` + GIN)

- Search targets: owners, patients, invoices, prescriptions/visit notes (matches TECH_STACK §7).
- Each searchable table has a generated `tsv` column (`GENERATED ALWAYS AS (...) STORED`) combining
  the relevant fields with Persian-appropriate `simple` text search configuration (no Persian
  stemming dictionary is bundled with stock PostgreSQL; `simple` config + trigram fallback via
  `pg_trgm` is used for partial/typo-tolerant matches) — DATA-07.
- A GIN index on `tsv` per table; query endpoint uses `websearch_to_tsquery` semantics.

## Audit and ledger tables

- `audit_log`: hash-chained, append-only, one row per mutating command across all modules (module
  name, entity, action, actor, before/after digest, `prev_hash`, `hash`). See
  [10-security-and-access-control.md#audit-chain](10-security-and-access-control.md) for the chain
  design; this file only fixes its storage shape (partitioned per DATA-06).
- `ledger_entry`: append-only, INV-06 enforced by a `BEFORE UPDATE OR DELETE` trigger raising an
  exception — DATA-08. Full design in [15-finance-and-ledger.md](15-finance-and-ledger.md).
- `conflict_log`: one row per sync conflict resolution outcome, written by the Sync module; see
  [09-sync-architecture.md#conflict-rules](09-sync-architecture.md).

## Migration policy

- EF Core migrations, forward-only, never edited after being merged to main (DATA-09).
- Migrations apply via a dedicated CI/CD step before the API container starts; the API MUST NOT
  auto-migrate on boot in production (TECH_STACK §7) — prevents two replicas racing a migration
  and keeps Topology B (future split) safe.
- A migration that changes a column type or drops a column on a synced-entity table requires a
  paired ADR-worthy review: sync clients on older schema versions must still be servable (see
  [09-sync-architecture.md#schema-gating](09-sync-architecture.md)).

## Read-vs-write path split (EF vs Dapper)

- Writes and single-aggregate reads needed for command validation: EF Core 10, via each module's
  `DbContext` (INV-20, AT-05).
- Report/list/export/dashboard reads: Dapper against hand-tuned SQL or views/materialized views
  (INV-20, AT-04). No `SELECT *`; explicit column lists (TECH_STACK §6).
- Materialized view refresh scheduling and KPI catalog: [16-reporting-and-analytics.md](16-reporting-and-analytics.md).

## PostgreSQL tuning posture for 4 GB

- `shared_buffers` ~ 25% of container memory allocation, `effective_cache_size` ~ 60-70%, capped
  `max_connections` (application uses `NpgsqlDataSource` pooling, not per-request connections) —
  concrete values live in the deploy config, not here (per INV-12, config is externalized; see
  [19-deployment-topology.md](19-deployment-topology.md) for the container memory budget this must
  fit inside).
- Autovacuum tuned more aggressive than default on `ledger_entry`/`audit_log` partitions (high
  insert rate, zero update/delete — cheap to vacuum, important to keep visibility maps current for
  index-only scans backing report queries).
- Statement timeout set per-role: report/Dapper role gets a hard statement timeout; OLTP/EF role
  does not, to fail slow reports fast without killing normal writes.
