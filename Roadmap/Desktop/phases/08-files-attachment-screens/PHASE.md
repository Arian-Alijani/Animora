# Phase 08: Files & Attachments Screens

## Goal

Build the screens/controls the Files module owns: attachment picker/uploader UI, thumbnail grid,
and pending-download placeholder state, fully click-through against the Stage A data seam, inside
`Animora.Desktop.Modules.Files`.

## Expected Outcome

A staff member can attach a photo/document to a visit/lab-result/medical-file record from the UI,
see a thumbnail grid with a placeholder icon for non-image types, and see a "pending download"
placeholder state for attachments not yet fully local — all through real `Mediator` handlers bound
to `Modules.Files`'s data-seam interface (fake now, real in phase 19). No actual S3/network upload
exists yet (that is a P2/backend concern).

## Scope

- Attachment picker control (reusable across Visits/Clients screens) enforcing the extension
  allowlist client-side (FILE-14 UX mirror; server-side is the real control, P2).
- Thumbnail grid using `SkiaSharp` for local resize/preview generation (desktop-side thumbnailing
  for locally-available originals; server-side thumbnailing per FILE-09 is a P2 concern for
  synced-in attachments).
- Pending-download placeholder state UI (FILE-13: "there is an attachment" before bytes arrive).
- Out of scope: local SQLite metadata persistence (phase 19), chunked/resumable upload (P2/backend,
  FILE-05), S3 orchestration (P2/backend).

## Key References

- [`docs/architecture/17-files-and-attachments.md`](../../../../docs/architecture/17-files-and-attachments.md) —
  attachment domain model, thumbnailing/desktop cache (FILE-10), sync interaction (FILE-13),
  malware/type policy (FILE-14).
- [`docs/TECH_STACK.md`](../../../../docs/TECH_STACK.md) §4 "Documents & devices" — `SkiaSharp` for
  image resize/thumbnail.
- [`docs/architecture/20-extensibility-playbook.md`](../../../../docs/architecture/20-extensibility-playbook.md) —
  "add a desktop screen" recipe.

## Dependencies

Requires phases 02 and 03. Consumed by phase 07 (Visits attachment references) and phase 05
(medical file attachments). Feeds phase 19 (Files Local Data).

## Completion Criteria

- [ ] Attachment picker enforces the extension allowlist client-side.
- [ ] Thumbnail grid renders local previews via `SkiaSharp`; non-image types show a fixed
      placeholder icon (FILE-09's desktop-side mirror).
- [ ] Pending-download placeholder state exists and is visually distinct from a ready attachment.
- [ ] No network/S3 code exists in this phase (P2 seam only, marked `// TODO(P2)`).
- [ ] Every screen/control passes an `Avalonia.Headless` RTL smoke test.

---

## Step 0

Not run yet. See [`../../_meta/session-protocol.md`](../../_meta/session-protocol.md).
