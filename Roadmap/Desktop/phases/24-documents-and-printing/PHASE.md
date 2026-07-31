# Phase 24: Documents & Printing

## Goal

Build the real document generation and printing infrastructure in
`Animora.Desktop.Infrastructure/Documents` and `/Printing`: `QuestPDF`-based invoice/report
rendering, `ClosedXML` exports, and the `IPrintService` abstraction (Windows spooler + ESC/POS
thermal), wiring it behind the export/print actions phases 09/10 already built.

## Expected Outcome

Invoice printing, report PDF/Excel export (phase 10's job-style export UX), and receipt/label
thermal printing (Finance's cheque/cash-session and Clients' pet ID card contexts) all produce real
output files/print jobs instead of phase 09/10's placeholder instant-completion stubs, through one
`IPrintService` abstraction that screens call by document type.

## Scope

- `QuestPDF` document templates for invoices (using `InvoiceTemplate` data from phase 20, RTL +
  embedded Vazirmatn per FIN-07/TECH_STACK §4) and each report catalog row's PDF export.
- `ClosedXML` export implementation for report catalog rows' Excel export.
- `IPrintService` abstraction (DESK-ARCH-15) with two backends: Windows spooler (A4/A5 via
  `QuestPDF`-rendered documents) and ESC/POS thermal (receipts/labels); screens request by document
  type only, never by screen-specific logic.
- Rewire phase 10's export actions and phase 09's invoice screens from their placeholder/fake
  instant-completion to real document generation, still surfaced as tracked local jobs (DT-10,
  non-blocking) — the job-table wiring itself is phase 26; this phase provides the real work the
  job executes.
- Out of scope: the local job table's own persistence/scheduling mechanics (phase 26 — this phase's
  document generation is the payload a job runs, not the job infrastructure itself).

## Key References

- [`docs/architecture/12-desktop-architecture.md`](../../../../docs/architecture/12-desktop-architecture.md) —
  DESK-ARCH-15 printing abstraction (one `IPrintService`, two backends, picked by document type).
- [`docs/TECH_STACK.md`](../../../../docs/TECH_STACK.md) §4 "Documents & devices" — `QuestPDF`,
  `ClosedXML`, printing backend choices (fixed, no alternatives).
- [`docs/architecture/15-finance-and-ledger.md`](../../../../docs/architecture/15-finance-and-ledger.md) —
  FIN-07/08 (`InvoiceTemplate` is data, live preview is pure read/render) this phase's real renderer
  must preserve exactly.

## Dependencies

Requires phase 20 (Finance local data — real invoice/template data to render) and phase 21
(Reporting local data — real report data to export). Feeds phase 26 (job table wraps this phase's
document generation as tracked long operations) and phase 25 (barcode/label content composes into
ESC/POS output built here).

## Completion Criteria

- [ ] Invoice PDF renders via `QuestPDF` with correct RTL layout and embedded Vazirmatn, matching
      the live-preview template used in phase 09.
- [ ] Every report catalog row exports to both PDF and Excel with real data.
- [ ] `IPrintService` exists with Windows-spooler and ESC/POS backends, selected by document type.
- [ ] Phase 09/10's export/print actions produce real output instead of placeholder completion.
- [ ] No screen picks a print backend by screen-specific logic (DESK-ARCH-15 respected).

---

## Step 0

Not run yet. See [`../../_meta/session-protocol.md`](../../_meta/session-protocol.md).
