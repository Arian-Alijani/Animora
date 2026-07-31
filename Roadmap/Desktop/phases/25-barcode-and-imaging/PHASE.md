# Phase 25: Barcode & Imaging

## Goal

Build the real barcode/QR generation and scanning infrastructure in
`Animora.Desktop.Infrastructure/Barcode` and extend `/Imaging`: `QRCoder`-based pet ID card and
vaccine/drug label generation, `ZXing.Net`-based scan/lookup, wired into Clients' and Visits'
existing screens as a real capability instead of a deferred concern.

## Expected Outcome

A staff member can generate a pet ID card / vaccine or drug label with an embedded QR code from a
patient record (Clients module, phase 05/16) and scan a QR/barcode to jump straight to a patient's
record or a prescription lookup, through real `QRCoder`/`ZXing.Net` calls composed with phase 24's
print pipeline for label output.

## Scope

- `QRCoder` generation: pet ID card layout and vaccine/drug label layout, each embedding a QR code
  encoding the patient/prescription id, composed with phase 24's `IPrintService` for ESC/POS label
  output.
- `ZXing.Net` scan integration: a scan input surface (camera or connected scanner device feed) that
  resolves a scanned code to a patient lookup (Clients) or prescription lookup (Visits), navigating
  via `INavigationService` to the matching screen.
- New small screens/dialogs only where needed to host generate/scan actions inside their owning
  module (`Modules.Clients` for ID cards, `Modules.Visits` for prescription-lookup scanning),
  following the "add a desktop screen" recipe.
- Out of scope: any DICOM/PACS imaging pipeline (explicitly out of scope, TECH_STACK §19), any new
  domain entity (barcode/QR content only encodes existing ids, it does not introduce a new
  aggregate).

## Key References

- [`docs/TECH_STACK.md`](../../../../docs/TECH_STACK.md) §4 "Documents & devices" — `QRCoder`
  (generate) + `ZXing.Net` (scan) as the fixed, only barcode/QR libraries; §19 confirms DICOM/PACS
  stays out of scope.
- [`docs/architecture/20-extensibility-playbook.md`](../../../../docs/architecture/20-extensibility-playbook.md) —
  "add a desktop screen" recipe for the small generate/scan UI surfaces.
- [`docs/architecture/04-module-catalog.md`](../../../../docs/architecture/04-module-catalog.md) —
  confirms Clients owns patient identity and Visits owns lab/prescription context, so generate/scan
  actions land in the correct module.

## Dependencies

Requires phase 16 (Clients local data — patient ids to encode), phase 18 (Visits local data —
prescription/lab-result ids to encode), and phase 24 (Documents & Printing — label print output).

## Completion Criteria

- [ ] A pet ID card and a vaccine/drug label generate with a correctly embedded QR code and print
      via phase 24's `IPrintService`.
- [ ] Scanning a generated code resolves to the correct patient or prescription record and
      navigates there.
- [ ] No barcode/QR logic duplicates document-generation code already in phase 24 (label content
      composes with the existing print pipeline, it does not fork a parallel one).
- [ ] Every new screen/dialog passes an `Avalonia.Headless` RTL smoke test.

---

## Step 0

Not run yet. See [`../../_meta/session-protocol.md`](../../_meta/session-protocol.md).
