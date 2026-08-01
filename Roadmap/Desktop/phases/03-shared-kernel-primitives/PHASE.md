# Phase 03: Shared Kernel & Contracts Seed

## Goal

Seed the minimum of `shared/dotnet/Animora.SharedKernel` and `shared/dotnet/Animora.Contracts`
needed for desktop screens to compile against real primitive types (`TenantId`, `Money`, `Result`,
base entity shape, UTC clock abstraction) instead of ad-hoc placeholder types — without building
anything backend-specific or anything that belongs to P2.

## Expected Outcome

Module screens phases (04-12) can reference real `Animora.SharedKernel` primitives and
`Animora.Contracts` V1 DTO/enum shapes for the entities they render, with `FluentValidation`
validators for the first-needed entities available to run in handlers.

## Scope

- `Animora.SharedKernel/Primitives`: `TenantId`, `Result`, base entity contract, `ISyncedEntity`
  marker (used later by sync/local-data phases, not wired yet).
- `Animora.SharedKernel/Money`: `Money` value type per CONV-07/08 (decimal(18,2), banker's rounding
  at persistence).
- `Animora.SharedKernel/Time`: injectable UTC clock abstraction (CONV-06 — no `DateTime.Now`).
- `Animora.SharedKernel/Validation`: `FluentValidation` validators for the entities the first module
  phases need (Owner, Patient — expand incrementally as later module phases need more; this phase
  seeds the pattern and the first 1-2 real validators, not the full catalog).
- `Animora.Contracts/V1/Dtos` and `/Enums`: hand-author the minimal DTO/enum shapes needed for
  Stage A screens to bind to (note: CONV-19 "no hand-written API DTOs" governs the *real* OpenAPI-
  generated wire DTOs for backend/web; in desktop-only P1 with no backend yet, this phase's DTOs are
  the seam that Contracts will formalize once OpenAPI exists in P2 — keep them minimal and shaped
  like the documented entities so the P2 swap is additive, not a rewrite).
- Out of scope: sync class declarations/field groups (that is real work once sync exists, P2), any
  EF Core/SQLite mapping (phase 14+), full validator catalog for every entity (grows per-module as
  each module screens phase needs it).

## Key References

- [`shared/AGENTS.md`](../../../../shared/AGENTS.md) — layout and hard rules SH-01..SH-05 (leaf
  assembly, no platform-conditional code, validators must be I/O-free).
- [`docs/architecture/05-domain-model.md`](../../../../docs/architecture/05-domain-model.md) —
  entity shapes/invariants for the first entities needed (Owner, Patient).
- [`docs/architecture/21-conventions.md`](../../../../docs/architecture/21-conventions.md) — ID/
  money/time rules (CONV-01..09) these primitives must satisfy exactly.

## Dependencies

Requires phase 00. Phases 04-12 each extend this phase's validator/DTO catalog incrementally for
their own entities — this phase seeds the pattern and the first consumer, it does not front-load
every entity.

## Completion Criteria

- [x] `TenantId`, `Money`, `Result`, UTC clock abstraction exist and are unit-tested in isolation.
- [x] At least one real `FluentValidation` validator exists and runs with no I/O (SH-05).
- [x] `Animora.SharedKernel` and `Animora.Contracts` reference no `Modules.*` or platform project
      (SH-01, AT-02).
- [x] No platform-conditional code exists in either project (SH-02).
- [x] Money uses `decimal(18,2)` exclusively; no `float`/`double` anywhere (INV-05).

---

## Step 0

Run on 2026-08-01 -> [`TODO.md`](TODO.md) (26 items). The three calls the list had to make — the UTC
clock is the already-injected `System.TimeProvider` rather than a second abstraction (AG-14), shared
validators bind to input interfaces a handler's command implements instead of copied input DTOs
(CONV-18, SH-01), and `Animora.Contracts` gets a seed rather than a catalog (CONV-19/20) — are
recorded in `TODO.md`'s header rather than restated per item.
