# Phase 31: Release Readiness

## Goal

Final gate for P1: confirm every Desktop feature required by the architecture is implemented,
tested, and production-shippable — the single checkpoint that must sign off before any P2 (backend)
work is allowed to start (AGENTS.md AG-01/AG-04).

## Expected Outcome

Every module's screens (04-12) are backed by real local persistence (15-23), every cross-cutting
capability (documents/printing, barcode, jobs/reminders, native notifications/update, security
hardening, performance budget, backup/restore) is complete, `dotnet build`/`dotnet test` are green
end-to-end including `Animora.Desktop.ArchTests`, and the product owner can sign off P1 so P2
(backend) is unblocked.

## Scope

- Full-catalog completeness audit: every functional requirement row in
  [04-module-catalog.md](../../../../docs/architecture/04-module-catalog.md)'s feature map has a
  working, real-data-backed desktop screen — cross-check against phases 04-23's completion criteria
  rather than re-deriving the list.
- Full `Animora.Desktop.ArchTests` run: every `NetArchTest` rule from
  [03-solution-structure.md](../../../../docs/architecture/03-solution-structure.md) (DIR-01..07,
  AT-01..09 desktop-applicable subset) passes against the final, fully-populated solution — not just
  the vacuously-passing state from phase 00.
- Full `Avalonia.Headless` RTL smoke suite run across every screen in the product (not per-phase
  spot checks — the complete `Animora.Desktop.UiTests` project run).
- Release packaging verification: self-contained/`ReadyToRun`/single-file publish
  (DESK-ARCH-17), Authenticode code-signing pipeline exists and is exercised at least once,
  `Velopack` feed configuration (phase 27's seam) is confirmed operational end-to-end against a
  real (even if staging) feed.
- Explicit sign-off checklist: every `// TODO(P2)` marker left in the codebase is reviewed and
  confirmed to be an intentional, documented seam (DT-12) — not an accidentally-incomplete P1
  feature masquerading as a P2 deferral.
- Update `AGENTS.md`'s Delivery Phases table status for P1 to reflect sign-off (the only doc-level
  change this phase makes; it does not alter the architecture itself).
- Out of scope: starting any P2 (backend) or P3 (web) work — that remains blocked until the product
  owner explicitly signs off per AG-01/AG-04, which this phase's completion enables but does not
  itself constitute.

## Key References

- [`AGENTS.md`](../../../../AGENTS.md) — AG-01/AG-04 (P1 sign-off is the literal gate for P2/P3);
  this phase exists to satisfy that gate honestly.
- [`docs/architecture/04-module-catalog.md`](../../../../docs/architecture/04-module-catalog.md) —
  the feature -> module map used as the completeness-audit checklist.
- [`docs/architecture/03-solution-structure.md`](../../../../docs/architecture/03-solution-structure.md) —
  the full NetArchTest rule set this phase confirms passes for real, on the finished solution.

## Dependencies

Requires every prior phase (00-30) to be `complete`. This is the last phase; nothing in this
roadmap depends on it except the human/product decision to unblock P2.

## Completion Criteria

- [ ] Every functional requirement row in the module catalog's feature map has a corresponding,
      real-data-backed, navigable desktop screen.
- [ ] `dotnet build` and `dotnet test` (all three test projects) pass with zero failures on the
      complete solution.
- [ ] Every `NetArchTest` rule in `Animora.Desktop.ArchTests` passes against the fully-populated
      solution.
- [ ] Every screen in the product has a passing `Avalonia.Headless` RTL smoke test.
- [ ] Every remaining `// TODO(P2)` marker is reviewed and confirmed intentional (list produced and
      checked, not assumed clean).
- [ ] Release packaging (ReadyToRun/single-file/signing/update feed) is verified operational.
- [ ] `AGENTS.md`'s P1 row is updated to reflect sign-off status.

---

## Step 0

Not run yet. See [`../../_meta/session-protocol.md`](../../_meta/session-protocol.md).
