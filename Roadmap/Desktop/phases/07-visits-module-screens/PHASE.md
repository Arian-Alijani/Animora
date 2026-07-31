# Phase 07: Visits Module Screens

## Goal

Build every screen the Visits module owns: visit recording, biometric tracking with growth charts,
lab/test results and visit outcomes (with attachment references), fully click-through against the
Stage A data seam, inside `Animora.Desktop.Modules.Visits`.

## Expected Outcome

A staff member can record a visit for a patient, enter biometric readings and see a growth chart,
record lab results and visit outcomes, and reference attachments by id — all through real
`Mediator` handlers bound to `Modules.Visits`'s data-seam interface (fake now, real in phase 18).

## Scope

- Visit recording screen (one transaction per visit write, per aggregate boundary rules).
- Biometric entry + growth chart (using `LiveChartsCore.SkiaSharpView.Avalonia`, REP-11/REP-12
  chart data contract shape: `{ series: [...], meta: {...} }`); biometric readings are immutable —
  corrections are new readings with a `supersedes` link (DOM-09), never edits.
- Lab result entry + visit outcome recording, with an attachment picker that references attachments
  by id (actual upload/thumbnail flow is phase 08's screens; this phase only wires the reference
  UI).
- Out of scope: local SQLite persistence (phase 18), actual attachment upload/storage (phase 08/P2
  S3), report materialization (phase 21's Reporting parity for growth charts stays a stub in this
  phase — REP-06 desktop-local parity work belongs to phase 18/21).

## Key References

- [`docs/architecture/05-domain-model.md`](../../../../docs/architecture/05-domain-model.md) —
  Visit aggregate (Visit, VisitOutcome, BiometricReading, LabResult) and DOM-09 (biometric
  immutability + supersedes link).
- [`docs/architecture/16-reporting-and-analytics.md`](../../../../docs/architecture/16-reporting-and-analytics.md) —
  REP-11/REP-12 chart data contract shape the growth chart must use.
- [`docs/architecture/20-extensibility-playbook.md`](../../../../docs/architecture/20-extensibility-playbook.md) —
  "add a desktop screen" recipe.

## Dependencies

Requires phases 02, 03, and 05 (visits are recorded against a patient). Feeds phase 18 (Visits Local
Data). Loosely coordinates with phase 08 (attachment reference UI) and phase 10 (Reporting screens
reuse the chart contract).

## Completion Criteria

- [ ] Visit recording, biometric entry + growth chart, lab result, and visit outcome screens exist.
- [ ] Growth chart consumes the fixed `{ series, meta }` contract shape (REP-11).
- [ ] Biometric correction flow creates a new reading with a `supersedes` link, never edits in place
      (DOM-09).
- [ ] Attachment references are by id only; no binary handling occurs in this phase.
- [ ] Every screen passes an `Avalonia.Headless` RTL smoke test.

---

## Step 0

Not run yet. See [`../../_meta/session-protocol.md`](../../_meta/session-protocol.md).
