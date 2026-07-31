# ADR-0005: Tenancy Isolation Mechanism

## Status

Accepted

## Context

Animora is multi-tenant SaaS on one shared database; a cross-tenant data leak is a top-tier
business risk. TECH_STACK §7 mandates "PostgreSQL Row-Level Security (RLS) with per-request tenant
context — defense in depth, not a substitute for query filters." See
[07-server-data-architecture.md](../07-server-data-architecture.md), [10-security-and-access-control.md](../10-security-and-access-control.md).

## Decision

Two independent layers, both mandatory: (1) PostgreSQL RLS policy per tenant-scoped table keyed off
a `SET LOCAL app.tenant_id` set from the authenticated principal per request; (2) an EF Core global
query filter on every `DbContext` applying the same `TenantId` filter in application code. Either
layer failing alone still blocks cross-tenant access (fail closed).

## Alternatives considered

| Alternative | Why rejected |
|---|---|
| Database-per-tenant | Operationally infeasible on a single 4 GB VPS for potentially dozens of tenants (connection pool exhaustion, migration fan-out cost) |
| Schema-per-tenant | Same operational cost concern at smaller scale; migration tooling complexity multiplies with tenant count |
| Application-layer filtering only (no RLS) | A single missed `.Where(tenantId)` call becomes a cross-tenant leak with no safety net; explicitly rejected by TECH_STACK §7's "defense in depth" framing |
| RLS only (no application filter) | A missing `SET LOCAL` (e.g., a background job running outside request context) would fail open under RLS-only if misconfigured; the application filter provides a second independent check |

## Consequences

- Positive: a single-layer bug (missing filter, misconfigured RLS policy) does not by itself cause
  a leak; mandatory Testcontainers RLS suite (TECH_STACK §16) makes this testable per table.
- Negative / accepted trade-off: every tenant-scoped table needs both an RLS policy and confirmation
  the global query filter is registered; a new module's `DbContext` setup has a fixed checklist item
  (see [20-extensibility-playbook.md](../20-extensibility-playbook.md)).
- Follow-up docs affected: [07-server-data-architecture.md](../07-server-data-architecture.md),
  [10-security-and-access-control.md](../10-security-and-access-control.md).
