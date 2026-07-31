---
id: 02-invariants
title: Principles & Invariants
read_when: ["checking if a design is allowed", "writing a lint/test for a rule"]
topics: [invariants, rules]
depends_on: [01-context]
stability: stable
---

## Contract

Decides: the numbered, testable rules every module, doc, and line of code MUST obey. Does not
decide how a specific module implements a rule — that lives in the module's own doc.

## How to cite

Reference by ID in code comments, PR descriptions, and other docs: `// see INV-07`. Enforcement
column states the concrete mechanism (test suite, lint rule, code review gate).

## Invariants

| ID | Statement | Rationale | Enforcement |
|---|---|---|---|
| INV-01 | The backend is one process (modular monolith); modules communicate only via in-process `Mediator` messages, never by querying another module's tables. | Keeps a future split cheap; TECH_STACK §6. | `NetArchTest` rule per [03](03-solution-structure.md); code review |
| INV-02 | Desktop and web MUST call the same API contract; no endpoint or business rule exists for one client only. | TECH_STACK §1: single API contract. | OpenAPI spec review; endpoint owner checklist in [20](20-extensibility-playbook.md) |
| INV-03 | All synced entities use `UUIDv7` primary keys, generated client-side or server-side, never DB identity/sequence. | Sync-safe, time-sortable IDs; TECH_STACK §3. | EF Core model-validation test scanning synced entity configs |
| INV-04 | All synced entities are soft-deleted with a tombstone record; hard delete is forbidden on synced tables. | Sync must propagate deletes without losing history. | Schema linter checking for `IsDeleted`/tombstone column on synced tables; [09](09-sync-architecture.md) |
| INV-05 | Money fields are `decimal(18,2)` / `numeric(18,2)` everywhere; `float`/`double` for money is forbidden. | Rounding correctness; TECH_STACK §3. | Roslyn analyzer + EF model check |
| INV-06 | Financial ledger rows are append-only; UPDATE/DELETE on ledger tables is forbidden after commit. | Auditable, tamper-evident money history; [15](15-finance-and-ledger.md). | DB-level trigger denying UPDATE/DELETE + integration test |
| INV-07 | Every API error response is RFC 9457 `application/problem+json` with a stable `code`; clients MUST switch on `code`, never on message text. | Stable client contracts across languages/versions. | Contract test asserting `code` presence; [06](06-api-contract.md) |
| INV-08 | Every table holding tenant data carries a `tenant_id` and is protected by PostgreSQL RLS; query filters are defense in depth, not the only control. | Multi-tenant isolation must survive an application bug. | Testcontainers RLS test suite (mandatory per TECH_STACK §16) |
| INV-09 | Every endpoint declares required permission claim(s) and is checked server-side; UI-level gating is never the only control. | Desktop/web gating is UX only; server is authoritative. | Endpoint metadata lint + integration test hitting endpoints without claim |
| INV-10 | Every OpenAPI change is additive unless a major version bump is declared; a shipped desktop version keeps working against the current server (N-1 policy). | Field devices update on their own schedule. | OpenAPI snapshot diff in CI (breaking-change gate), [06](06-api-contract.md) |
| INV-11 | Every background job (server Hangfire or desktop local) is idempotent, cancellable, and logged with a correlation id. | Weak server + offline desktop both retry aggressively. | Job base-class contract test; code review checklist |
| INV-12 | Config is environment variables only; no hardcoded host, path, or secret in code. | 12-factor; Topology A->B must be config-only. | CI grep-lint for hardcoded literals; [19](19-deployment-topology.md) |
| INV-13 | Time is stored as UTC `timestamptz`/UTC ticks; Jalali/local conversion happens only at the UI edge. | Sync ordering correctness across timezones. | Roslyn analyzer banning `DateTime.Now`; [21](21-conventions.md) |
| INV-14 | A synced entity's conflict rule is declared explicitly (LWW field-group or append-only); "undefined" conflict behavior is not permitted to ship. | Every entity must have a defined convergence outcome. | PR checklist item + sync test matrix entry required, [09](09-sync-architecture.md) |
| INV-15 | The desktop app remains fully write-capable with zero network for at least the offline quality target ([01](01-context-and-drivers.md)); no feature may silently require connectivity. | Core product promise. | Manual/automated offline smoke test per release |
| INV-16 | Entitlement checks are enforced server-side on every gated request; the desktop license token is a UX/offline accelerant, never the sole gate for server-mediated actions. | Anti-tamper posture; TECH_STACK §9. | Integration test: revoke entitlement, confirm server 403 regardless of local token state |
| INV-17 | No feature, library, or pattern from TECH_STACK §18 (Do NOT Use) or §19 (Out of Scope) may appear in any design or code. | Immutable stack contract. | PR review gate citing TECH_STACK; architecture doc review |
| INV-18 | A fact about the architecture lives in exactly one document; other docs link to it. | Token-efficiency for AI-agent consumption. | Doc review: reject PRs duplicating content across files |
| INV-19 | Every new feature that touches a synced entity MUST add a case to the sync test matrix ([09](09-sync-architecture.md)) before merge. | Sync bugs are the highest business risk (TECH_STACK §20). | CI checklist / PR template field |
| INV-20 | Reads on hot/report paths use Dapper; EF Core is for writes/domain only; EF Core on hot report paths and raw Dapper writes to domain tables are both forbidden. | TECH_STACK §6, §18. | `NetArchTest` rule scanning query stack per path; [03](03-solution-structure.md) |

## Precedence

If a module doc appears to conflict with an invariant here, the invariant wins; fix the module doc.
If TECH_STACK.md and this file appear to conflict on a technology choice, TECH_STACK.md wins; fix
this file.
