---
id: 20-playbook
title: Extensibility Playbook
read_when: ["implementing any new feature"]
topics: [recipes, how-to]
depends_on: [all]
stability: stable
---

## Contract

Decides: the ordered, verbatim steps an agent follows for each common change type, the files/
artifacts touched, and the definition of done. Does not restate the rules being followed — each
step cites the doc that owns the rule.

## Recipe: add a CRUD entity (non-synced)

1. Define the entity in the owning module ([04](04-module-catalog.md)) with `TenantId` (DOM-01).
2. Add EF Core config + migration (forward-only, DATA-09); apply via the CI migration step, not
   auto-migrate ([07](07-server-data-architecture.md)).
3. Add `FluentValidation` rules in the module's validation namespace ([03](03-solution-structure.md)).
4. Add OpenAPI operations for the endpoint group ([06](06-api-contract.md)); declare required
   permission claim(s) on each operation (SEC-12).
5. Regenerate Kiota (desktop) and Orval (web) clients; commit generated output ([03](03-solution-structure.md)).
6. Add permission claim(s) to the catalog if new ([10](10-security-and-access-control.md)).
7. Add unit + integration tests (Testcontainers RLS case included, INV-08).
8. Update [04-module-catalog.md](04-module-catalog.md) if the module's owned-entities list changed.

**Definition of done**: migration applied in CI, OpenAPI spec updated + client-drift check green,
RLS test passing, permission enforced server-side (INV-09), module catalog doc updated.

## Recipe: add a synced entity

1. Do all "add a CRUD entity" steps above.
2. Declare the entity's sync class (`MutableLWW`/`AppendOnly`/`StateMachine`) in `Animora.Contracts`
   (SYNC-R-01, [09](09-sync-architecture.md)); define field groups if `MutableLWW` (SYNC-R-03).
3. Use `UUIDv7` primary key (INV-03); add tombstone support (INV-04).
4. Mirror the entity in the desktop SQLite schema with sync metadata columns (DESK-02,
   [08](08-desktop-local-data.md)); add an EF Core desktop migration.
5. Add the entity to the sync batch protocol's entity-type registry (server + desktop).
6. Add every row of the mandatory sync test matrix for this entity ([09-sync-architecture.md#mandatory-sync-test-matrix](09-sync-architecture.md), INV-19) — this is not optional.
7. If the entity is `StateMachine`-classed, document its state diagram in [05-domain-model.md](05-domain-model.md).

**Definition of done**: all sync test matrix rows pass in `Animora.SyncTests`, desktop and server
schemas both migrated, entity's sync class and field groups documented in `Animora.Contracts`.

## Recipe: add an endpoint

1. Add the OpenAPI operation under the correct existing endpoint group ([06](06-api-contract.md));
   new resource -> new group only if no existing aggregate root fits ([04](04-module-catalog.md)).
2. Declare permission claim(s) (SEC-12); if paginated, use keyset pagination (API-PAGE-01).
3. If mutating and safely retryable, support `Idempotency-Key` (API-IDEM-01); if updating a mutable
   resource, support `If-Match`/ETag (API-CONC-01).
4. Regenerate clients; run the OpenAPI breaking-change gate (API-VER-05) — additive only unless a
   major version bump is justified and documented.
5. Add contract test asserting the RFC 9457 error shape on failure paths (INV-07).

**Definition of done**: spec additive-or-versioned correctly, clients regenerated with zero drift,
permission enforced, error shape conformant.

## Recipe: add a permission

1. Add the claim to the catalog table in [10-security-and-access-control.md](10-security-and-access-control.md)
   under its owning module's group.
2. Wire it into the seed data for the relevant built-in role(s), if any.
3. Attach it to the relevant endpoint operation(s) (SEC-12) and to the corresponding desktop
   command handler's authorization check (mirrors server check, never trusts client-only gating).
4. Add an integration test: calling the endpoint without the claim returns `403`.

**Definition of done**: claim documented, endpoint(s) enforce it server-side, test proves denial
without it.

## Recipe: add a plan-gated feature

1. Add the entitlement flag/limit to the `Plan`/`Entitlement` model ([11](11-licensing-and-entitlements.md)).
2. Gate the server endpoint via `Microsoft.FeatureManagement` bound to the entitlement snapshot
   (LIC-15); this is the enforcement point — never client-only.
3. Add UI-only gating (hide/disable) on desktop and web for UX, explicitly documented as
   non-authoritative (INV-16).
4. Add an integration test: revoke the entitlement, confirm server rejects the action even with a
   stale/cached client-side license token.

**Definition of done**: server-side FeatureManagement gate exists and is tested independent of
client state; UI gating present but not relied upon by the test.

## Recipe: add a report

1. Decide view/materialized-view/on-demand per the decision rule in
   [16-reporting-and-analytics.md#view-vs-materialized-view-vs-on-demand-decision-rule](16-reporting-and-analytics.md).
2. Write the Dapper query (INV-20); add the supporting index (REP-13, [07](07-server-data-architecture.md)).
3. Register refresh scheduling if materialized (REP-04/REP-05).
4. Add the report to the KPI/report catalog table in [16-reporting-and-analytics.md](16-reporting-and-analytics.md).
5. If entitlement-gated (advanced report tier), follow the plan-gated-feature recipe above.
6. Add the desktop-local parity query if the report must work offline (REP-06/REP-07).

**Definition of done**: catalog updated, index/materialization in place, latency budget met
(REP-14), desktop parity added or explicitly noted as server-only in the catalog row.

## Recipe: add a notification type

1. Add a row to the alert-source catalog in [14-jobs-and-notifications.md#alert-source-catalog](14-jobs-and-notifications.md)
   with trigger, default channels, owning module.
2. Raise the domain event from the owning module via `Mediator` (NOTIF-01) — never poll from
   Notifications.
3. Define the dedup key (NOTIF-02) and whether it bypasses quiet hours (NOTIF-06).
4. If the alert must fire while offline, add the equivalent local reminder rule against the desktop
   `reminder` table (NOTIF-08/NOTIF-09, [08](08-desktop-local-data.md)).
5. Add a delivery-log assertion test for at least the primary channel and one failover/escalation
   path.

**Definition of done**: catalog row added, event wired via Mediator, offline-local counterpart
added if applicable, delivery log test passing.

## Recipe: add a background job

1. Decide server (Hangfire) vs desktop-local ([14-jobs-and-notifications.md#job-taxonomy](14-jobs-and-notifications.md)).
2. Implement idempotency explicitly (JOB-01/INV-11) — state the completion-detection mechanism in
   the job's own doc comment.
3. Respect concurrency caps (JOB-02/JOB-03); thread a correlation id (JOB-04).
4. Register recurring schedule or trigger point; add a job-failure path into the alert-source
   catalog if failure is user-relevant (e.g., "backup failure").

**Definition of done**: idempotency proven by test (run twice, same result), correlation id present
in logs, capped concurrency respected.

## Recipe: add a desktop screen

1. Add a ViewModel + View under the owning `Desktop.Modules.X` project; bind via MVVM only
   (DESK-ARCH-01/02, [12](12-desktop-architecture.md)).
2. Register the route with the navigation service (DESK-ARCH-05); do not add shell-level knowledge
   of the screen.
3. Route all state changes through `Mediator` handlers; local write + outbox in one transaction if
   the screen touches a synced entity (DESK-ARCH-09).
4. Apply theme tokens, RTL flow inheritance (no per-screen override, DESK-ARCH-06), Jalali
   formatting at the binding edge only (DESK-ARCH-14).
5. Add an `Avalonia.Headless` RTL smoke test for the new screen.

**Definition of done**: headless RTL smoke test passing, no direct `DbContext`/`HttpClient` usage
in the ViewModel, navigation registered without shell coupling.

## Recipe: add a web page

1. Decide segment: `(marketing)` (SSG/ISR) vs `(app)`/`(admin)` (dynamic) per
   [13-web-architecture.md#app-router-segmentation](13-web-architecture.md).
2. Server component fetches first-paint data via the generated client (WEB-02); client components
   handle interactivity via `TanStack Query` against the same client (WEB-04).
3. Use `zod` schemas derived from the OpenAPI spec for any form (WEB-09); use logical Tailwind
   properties only (WEB-14).
4. If the page needs to work in the whitelisted offline read set, add it explicitly to the `Dexie`
   cache scope (WEB-10) — otherwise it is online-only by default (WEB-11), which is the expected
   default.

**Definition of done**: no hand-written DTO, RTL-correct via logical properties, bundle-budget
check green ([13](13-web-architecture.md)).

## Recipe: evolve the sync protocol

1. Determine if the change is additive (new entity type, new optional field within an existing
   field group) or breaking (field-group restructuring, entity class change, cursor/batch envelope
   change).
2. Additive: no `protocolVersion` bump needed; update `Animora.Contracts` and the sync test matrix
   entries for the affected entity.
3. Breaking: bump `protocolVersion` (SYNC-R-08/09); ensure the server continues serving N-1
   protocol clients unchanged until they age out ([06-api-contract.md#versioning-and-n-1-compatibility](06-api-contract.md));
   document the old-client behavior (refusal + forced update) explicitly in the PR.
4. Re-run the full mandatory sync test matrix ([09](09-sync-architecture.md)), not just the changed
   entity's rows, since protocol-level changes can have cross-entity effects (ordering, batching).

**Definition of done**: protocol version policy applied correctly, N-1 desktop builds verified
(via a pinned old-protocol test client) to still be refused/served as designed, full matrix green.

## Recipe: deprecate a field

1. Never remove a field from the OpenAPI spec directly (API-VER-05 breaking-change gate would
   fail); mark it deprecated in the spec description and stop writing to it from new code paths.
2. Keep serving the field (last-known or computed-equivalent value) for at least one full N-1
   compatibility window ([06](06-api-contract.md)).
3. Only remove it in a deliberate major version bump, with the N-1 sunset plan documented in the
   PR (API-VER-03).
4. If the field backs a synced entity's field group, coordinate removal with a sync protocol
   version bump per the "evolve the sync protocol" recipe, since old desktop builds may still read
   it.

**Definition of done**: spec marks deprecation before removal, removal only happens on a major
version with a documented sunset, sync implications checked if applicable.
