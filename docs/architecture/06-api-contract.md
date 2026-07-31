---
id: 06-api
title: API Contract
read_when: ["adding an endpoint", "versioning a breaking change"]
topics: [rest, openapi, pagination, idempotency, etag]
depends_on: [04-modules, 05-domain]
stability: stable
---

## Contract

Decides: endpoint-group taxonomy, versioning/compatibility policy, error-code registry shape,
pagination/filtering conventions, idempotency, concurrency control, and the OpenAPI-first workflow.
Does not decide per-module endpoint lists (see [04](04-module-catalog.md)) or auth token mechanics
(see [10](10-security-and-access-control.md)).

## Resource / endpoint-group taxonomy

- One endpoint group per aggregate root (see [05](05-domain-model.md)), path `/api/v{n}/{plural-kebab-resource}`.
- Sub-resources nest one level max: `/api/v1/patients/{patientId}/biometrics`. Deeper relationships
  are query parameters, not path nesting (`?patientId=`).
- Endpoint groups are enumerated per functional requirement in
  [04-module-catalog.md#feature--module--endpoint-group-map](04-module-catalog.md).

## Versioning and N-1 compatibility

- `Asp.Versioning` URL path versioning: `/api/v1/...`. This is API-VER-01.
- API-VER-02: a version bump is MAJOR only; MINOR/PATCH changes are additive-only (new optional
  field, new endpoint, new enum value appended at the end of the shared registry — never reordered,
  see [21](21-conventions.md) enum wire-value policy).
- API-VER-03: the server MUST support the current major version and the immediately prior major
  version (N-1) simultaneously, for at least the duration of one desktop release cycle's worst-case
  field-update lag (target: indefinite until telemetry shows zero N-1 clients for 90 days).
- API-VER-04: the sync protocol carries its own `protocolVersion` header independent of the URL
  API version (see [09-sync-architecture.md#protocol-negotiation](09-sync-architecture.md)); an
  outdated protocol version is refused with a forced-update prompt, never silently degraded.
- Breaking-change gate (API-VER-05): CI runs an OpenAPI snapshot diff against the last released
  spec; any removed field, removed endpoint, changed type, or renumbered enum fails the build
  unless the PR also bumps the major version and documents the N-1 sunset plan in the PR body.

## OpenAPI-first workflow

1. Author/modify the OpenAPI spec (source of truth, versioned in-repo).
2. Regenerate: Kiota (desktop), `openapi-typescript` + Orval (web). Generated code is committed.
3. CI fails if regenerated output differs from committed output ("client drift" check).
4. Hand-written DTOs are forbidden on either client (TECH_STACK §18); all wire types originate
   from the spec.

## Error-code registry

- Transport: RFC 9457 `application/problem+json` (INV-07). Body always includes: `type`, `title`,
  `status`, `code` (machine, stable), `traceId`, and optional `errors[]` for field-level validation.
- `code` namespace: `ERR-{MODULE}-{NNN}` where `{MODULE}` matches the module catalog names
  (`FINANCE`, `SCHEDULING`, `CLIENTS`, `VISITS`, `LICENSING`, `IDENTITY`, `SYNC`, `FILES`). See
  [21-conventions.md#error-code-taxonomy](21-conventions.md) for the full allocation rule.
- Codes are append-only and never reused after removal; a retired code is documented, not deleted,
  in the registry file (registry lives beside the OpenAPI spec, not duplicated here).

## Pagination and filtering

- All list endpoints are keyset-paginated (API-PAGE-01): `?after={cursor}&limit={n}`, never
  offset/skip. Hard max `limit` is 100; default 20 (TECH_STACK §6 "paginate everything").
- Cursor is an opaque, server-signed token encoding `(sortKey, id)`; clients MUST NOT parse it.
- Filtering uses explicit query parameters per resource (documented per-endpoint in the OpenAPI
  spec), never a generic free-form query DSL. Full-text search uses a dedicated `?q=` parameter
  backed by `tsvector`/GIN server-side or FTS5 locally (see [07](07-server-data-architecture.md),
  [08](08-desktop-local-data.md)).
- Sort is fixed per resource (documented default) with at most one alternate `?sort=` option to
  keep index design tractable on the weak server.

## Idempotency

- All non-GET mutating requests that can be safely retried (payment verification, sync batch push,
  notification dispatch triggers) accept an `Idempotency-Key` header (API-IDEM-01). Server stores
  the key -> response mapping for 24 hours and replays the original response on a duplicate.
- Sync batch push idempotency is a stronger, dedicated mechanism — see
  [09-sync-architecture.md#batch-protocol](09-sync-architecture.md); it is not just this header.

## Concurrency control

- Mutable resources (e.g., `Invoice` while `Draft`, `Appointment`) expose a `version` field and
  require an `If-Match` ETag header on update (API-CONC-01). Mismatch returns
  `409 ERR-COMMON-VERSION-CONFLICT`.
- Append-only resources (`LedgerEntry`, ledger-derived states) never accept updates, so no ETag
  applies (INV-06); the only valid mutation is a new row.
- Synced entities use HLC-based field-group versioning for conflict resolution between devices
  (see [09](09-sync-architecture.md)) independent of the HTTP-layer ETag, which governs
  single-request concurrency only.

## Standard response shapes

| Shape | Used for |
|---|---|
| Single resource | `{ data: {...}, meta?: {...} }` |
| Paginated list | `{ data: [...], meta: { nextCursor, hasMore } }` |
| Problem details | RFC 9457 as above |
| Long-running job accepted | `202 Accepted` + `{ jobId, statusUrl }` (see [14](14-jobs-and-notifications.md)) |

## API surface exclusions

Per TECH_STACK §19, there is no third-party public API program: all endpoint groups are for the
first-party desktop/web clients and platform back-office only. No public API keys, no external
developer portal.
