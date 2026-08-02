# Phase 05: Clients Module Screens

## Goal

Build every screen the Clients module owns: owner (client) management and patient (animal)
management including the medical file header view, fully click-through against the Stage A data
seam, inside `Animora.Desktop.Modules.Clients`.

## Expected Outcome

A staff member can search/list owners, create/edit an owner, list an owner's patients, create/edit
a patient, and view a patient's medical-file summary screen — all through real `Mediator` handlers
bound to `Modules.Clients`'s own data-seam interface (fake now, real in phase 16).

## Scope

- Owner list (virtualized, searchable — search UI wired now, real FTS5 backing in phase 16) + owner
  create/edit screen.
- Patient list (scoped to an owner, and a global searchable patient list) + patient create/edit
  screen.
- Medical file summary screen (header view only — visit history rendering is Visits' concern,
  phase 07; this screen shows the Patient-owned header fields and links out).
- Every screen follows the "add a desktop screen" recipe; owner/patient are synced entities
  (`MutableLWW`), so this phase's TODO also captures the "add a synced entity" recipe's UI-facing
  steps (entity sync class already declared conceptually in phase 03/16 — screens here just consume
  the shape) without doing any actual sync/local-data work (that is phase 16 + P2).
- Out of scope: local SQLite persistence (phase 16), FTS5 search (phase 16), visit history (phase
  07), attachments (phase 08).

## Key References

- [`docs/architecture/20-extensibility-playbook.md`](../../../../docs/architecture/20-extensibility-playbook.md) —
  "add a desktop screen" and "add a synced entity" recipes (UI-facing steps only at this stage).
- [`docs/architecture/05-domain-model.md`](../../../../docs/architecture/05-domain-model.md) —
  Owner/Patient aggregate boundaries, DOM-03 (no cross-tenant refs — irrelevant to single-tenant
  desktop store but frames the entity shape correctly).
- [`docs/architecture/04-module-catalog.md`](../../../../docs/architecture/04-module-catalog.md) —
  Clients' owned entities and `IClientsContract` shape.

## Dependencies

Requires phases 02 and 03. Feeds phase 16 (Clients Local Data). Phase 07 (Visits screens) and
phase 08 (Files screens) reference patients created here for navigation context.

## Completion Criteria

- [x] Owner list/create/edit and patient list/create/edit screens exist and are navigable.
- [x] Medical file summary screen renders a patient's header fields.
- [x] Every screen passes an `Avalonia.Headless` RTL smoke test.
- [x] Any list that can exceed 200 rows uses a virtualized `DataGrid` (DT-08).
- [x] No ViewModel references `DbContext`/`HttpClient` directly (DT-02).

---

## Step 0

Run on 2026-08-01 -> [`TODO.md`](TODO.md) (38 items). The five calls the list had to make — one
patient-list route for both the global and the owner-scoped mode, the medical-file header read on the
patient seam rather than a seam of its own, Visits/Files links left as markers, only the synced-entity
recipe's sync-class declaration in scope, and read/write seams split per aggregate — are recorded in
`TODO.md`'s header rather than restated per item. The three field-level questions the corpus does not
answer are asked as items 2-4 instead of guessed (AG-02).
