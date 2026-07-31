# Phase 19: Files Local Data

## Goal

Swap phase 08's Files Stage A fake data-seam implementation for real local persistence: EF Core
entity/configuration/migration for `Attachment` metadata plus the local file-system thumbnail/
original cache, wired behind `Modules.Files`'s own interface.

## Expected Outcome

The attachment picker, thumbnail grid, and pending-download placeholder screens from phase 08 now
read/write real local `Attachment` metadata rows and a real local disk cache (LRU-bounded) for
locally-available originals/thumbnails, with the pending-download state reflecting a real
`uploadStatus` column instead of a fake flag.

## Scope

- EF Core entity configuration for `Attachment` metadata (`MutableLWW` per module catalog: `id`,
  `ownerEntityType`, `ownerEntityId`, `s3Key` (nullable/unset until P2 upload exists),
  `contentType`, `sizeBytes`, `checksum`, `uploadStatus`, `thumbnailS3Key`) per FILE-01, with
  `UUIDv7` PK and tombstone support.
- EF Core migration adding the `Attachment` table (DT-11).
- Local file-system cache (`Animora.Desktop.Data/...` or a dedicated cache path per FILE-10) keyed
  by `attachmentId` + checksum, LRU eviction bounded by a configurable size — this is the one place
  binary content touches local disk outside SQLite (FILE-10), populated here from
  locally-attached-and-not-yet-uploaded files (since there is no S3 to download from yet, P2).
  `uploadStatus` for a desktop-originated attachment starts in a locally-meaningful "pending sync"
  state, distinct from FILE-13's post-sync "pending download" state, and the picker/grid UI already
  built in phase 08 must display both correctly.
- `SkiaSharp` thumbnail generation on attach, writing to the local cache, replacing phase 08's fake
  thumbnail placeholder logic where a real local original exists.
- Rebind `Modules.Files`'s data-seam interface to this real implementation; delete the Stage A fake.
- Out of scope: S3/MinIO upload orchestration (P2/backend), server-side thumbnailing for synced-in
  attachments (P2, FILE-09), chunked/resumable upload (P2, FILE-05).

## Key References

- [`docs/architecture/17-files-and-attachments.md`](../../../../docs/architecture/17-files-and-attachments.md) —
  `Attachment` domain model (FILE-01/02), desktop cache design (FILE-10), sync interaction
  (FILE-13) distinguishing pre-sync local state from post-sync pending-download state.
- [`docs/TECH_STACK.md`](../../../../docs/TECH_STACK.md) §4 "Documents & devices" — `SkiaSharp` for
  thumbnail generation, confirming the exact library already used in phase 08.
- [`docs/architecture/09-sync-architecture.md`](../../../../docs/architecture/09-sync-architecture.md) —
  `MutableLWW` sync class conflict rule for `Attachment` metadata (SYNC-R-25), framing the schema
  correctly even though sync is inert until P2.

## Dependencies

Requires phase 14, phase 08 (Files screens + data-seam interface), and phase 16 (owner/patient
attachment context) plus phase 18 (visit/lab-result attachment context) for `ownerEntityType`
targets to reference real rows. Consumed by phase 18/07's attachment reference UI going forward.

## Completion Criteria

- [ ] `Attachment` EF Core entity/configuration/migration exists with `UUIDv7` PK and tombstone
      support.
- [ ] Attaching a file writes a real metadata row and generates a real local thumbnail via
      `SkiaSharp`, cached by `attachmentId` + checksum with LRU eviction.
- [ ] Thumbnail grid and pending-state UI from phase 08 render real local data unchanged.
- [ ] No network/S3 code path exists (still `// TODO(P2)`-marked per DT-12).
- [ ] Phase 08's `Avalonia.Headless` smoke tests still pass unchanged against real data.

---

## Step 0

Not run yet. See [`../../_meta/session-protocol.md`](../../_meta/session-protocol.md).
