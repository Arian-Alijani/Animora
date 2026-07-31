# Phase 09: Finance Module Screens

## Goal

Build every screen the Finance module owns: cheque management, professional invoicing with
templates and live preview, the cash-register/till module, and expense/income management, fully
click-through against the Stage A data seam, inside `Animora.Desktop.Modules.Finance`.

## Expected Outcome

A staff member can register/track a cheque through its lifecycle, build and live-preview an
invoice from a template before issuing it, open/reconcile/close a cash session, and record
expenses/income — all through real `Mediator` handlers bound to `Modules.Finance`'s data-seam
interface (fake now, real in phase 20), with the ledger-immutability and state-machine rules
respected in the UI/handler flow even though there is no real ledger storage yet.

## Scope

- Cheque screens: register, list by status, transition actions (InClearing/Cleared/Returned/
  Cancelled/ReIssued) per the state machine, with mandatory `returnReasonCode` on `Returned`
  (DOM-07).
- Invoice screens: template-based builder, live preview (pure read/render, no persistence/ledger
  touch per FIN-08), issue action (Draft -> Issued, the point a ledger posting conceptually occurs,
  FIN-09), void action requiring a linked `CreditNote` (DOM-06/FIN-10) — never an in-place edit of
  an issued invoice.
- Cash session screens: open (enforcing DOM-05's "one open session per till" in the UI/handler,
  real enforcement is server-side at sync-apply time per ADR-0012, this phase enforces it locally
  as the desktop's own single-till reality), cash movement entry, close/reconcile with variance
  entry (FIN-12/FIN-13).
- Expense/income entry screens (categorized, tenant-extendable category list per FIN-16).
- Every money screen keeps ledger semantics conceptually append-only in its handler design (no
  "edit a posted amount" affordance anywhere in the UI) even before phase 20 wires real storage.
- Out of scope: local SQLite/ledger persistence (phase 20), payment gateway integration (P2/
  backend, ZarinPal/Zibal), real ledger trigger enforcement (phase 20 + P2 Postgres trigger).

## Key References

- [`docs/architecture/15-finance-and-ledger.md`](../../../../docs/architecture/15-finance-and-ledger.md) —
  ledger design, invoice lifecycle/template/preview model, cash session model, cheque lifecycle,
  what is forbidden (no in-place edits, no reopening a session).
- [`docs/architecture/05-domain-model.md`](../../../../docs/architecture/05-domain-model.md) —
  Invoice/Cheque/CashSession state machines, DOM-05/06/07.
- [`docs/architecture/adr/ADR-0007-invoice-ledger-immutability.md`](../../../../docs/architecture/adr/ADR-0007-invoice-ledger-immutability.md)
  and [`ADR-0012-cheque-cash-session-state-ownership.md`](../../../../docs/architecture/adr/ADR-0012-cheque-cash-session-state-ownership.md) —
  why these entities are `StateMachine`-classed and what that means for UI affordances.

## Dependencies

Requires phases 02, 03, and 05 (invoices/cheques reference owners). Feeds phase 20 (Finance Local
Data). Phase 10 (Reporting screens) consumes Finance's conceptual ledger shape for KPI screens.

## Completion Criteria

- [ ] Cheque, invoice (with live preview), cash session, and expense/income screens exist and are
      navigable.
- [ ] No screen offers an "edit after issue/post" affordance for invoices, cheques (post-registration
      core fields), or ledger-adjacent amounts (DOM-06, FIN-02).
- [ ] Cash session open enforces one-open-per-till in the local UI/handler flow (DOM-05).
- [ ] Cheque `Returned` transition requires a `returnReasonCode` (DOM-07).
- [ ] Every screen passes an `Avalonia.Headless` RTL smoke test.

---

## Step 0

Not run yet. See [`../../_meta/session-protocol.md`](../../_meta/session-protocol.md).
