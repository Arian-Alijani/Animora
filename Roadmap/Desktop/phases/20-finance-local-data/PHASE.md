# Phase 20: Finance Local Data

## Goal

Swap phase 09's Finance Stage A fake data-seam implementation for real local persistence: EF Core
entities/configurations/migration for LedgerEntry, Invoice, InvoiceTemplate, Cheque, CashSession,
Expense, Income, wired behind `Modules.Finance`'s own interface, with append-only ledger semantics
enforced at the local storage layer.

## Expected Outcome

Cheque, invoice (with live preview), cash session, and expense/income screens from phase 09 now
read/write real local SQLite rows; `LedgerEntry` rows are genuinely append-only locally (no
update/delete code path exists), issuing an invoice posts a real local ledger entry, and cash
session enforces one-open-per-till against real persisted state.

## Scope

- EF Core entity configurations for `LedgerEntry` (`AppendOnly`), `Invoice`/`InvoiceTemplate`
  (`StateMachine`/data), `Cheque` (`StateMachine`), `CashSession`+`CashMovement` (`StateMachine`),
  `Expense`/`Income` (`MutableLWW` metadata, `AppendOnly` posting per FIN-16) — `UUIDv7` PKs,
  tombstones where applicable, sync metadata columns (DESK-02).
- EF Core migration adding these tables (DT-11).
- `Data/Writes` structurally prevents `UPDATE`/`DELETE` on `LedgerEntry` after insert (INV-06's
  desktop-local mirror; the real DB-trigger enforcement is server-side per FIN's "what is
  forbidden" list, but the local write path itself never exposes an update/delete method for this
  entity) — corrections are new offsetting rows linked by `reversalOfEntryId` (FIN-02).
- Local posting logic: issuing an invoice inserts the accounts-receivable/revenue `LedgerEntry`
  pair in the same transaction as the state transition (FIN-09); cheque state transitions post/
  reverse the "cheques in clearing" entries (FIN-14); cash movements post immediately (FIN-11);
  session close posts a variance entry when applicable (FIN-12/13).
- Local enforcement of DOM-05 (one open `CashSession` per till) against real persisted rows,
  replacing phase 09's in-memory fake check.
- Rebind `Modules.Finance`'s data-seam interface to this real implementation; delete the Stage A
  fake.
- Out of scope: real ledger DB-trigger (server-side, P2/Postgres), payment gateway integration (P2),
  real cross-device `CashSession` conflict enforcement (P2/sync, ADR-0012's `StateMachine` apply
  rule is a server-side concern once sync exists).

## Key References

- [`docs/architecture/15-finance-and-ledger.md`](../../../../docs/architecture/15-finance-and-ledger.md) —
  the full ledger design (FIN-01..21) this phase's local write paths must honor exactly, including
  "what is forbidden."
- [`docs/architecture/adr/ADR-0007-invoice-ledger-immutability.md`](../../../../docs/architecture/adr/ADR-0007-invoice-ledger-immutability.md)
  and [`ADR-0012-cheque-cash-session-state-ownership.md`](../../../../docs/architecture/adr/ADR-0012-cheque-cash-session-state-ownership.md) —
  why these are `StateMachine`-classed and the local persistence implication.
- [`docs/architecture/05-domain-model.md`](../../../../docs/architecture/05-domain-model.md) —
  DOM-05/06/07 enforcement points this local layer must implement.

## Dependencies

Requires phase 14, phase 09 (Finance screens + data-seam interface), and phase 16 (owners referenced
by invoices/cheques). Feeds phase 21 (Reporting local data derives from this phase's real
`LedgerEntry` rows).

## Completion Criteria

- [ ] `LedgerEntry`, `Invoice`, `InvoiceTemplate`, `Cheque`, `CashSession`/`CashMovement`,
      `Expense`, `Income` EF Core entities/configurations/migration exist.
- [ ] No code path can update or delete a persisted `LedgerEntry` row; a test proves only insert is
      possible and a correction creates a new offsetting row.
- [ ] Issuing an invoice posts a real ledger entry pair in the same transaction as the state
      transition, verified by a test.
- [ ] Opening a second `CashSession` on an already-open till is rejected against real persisted
      state (DOM-05).
- [ ] Phase 09's `Avalonia.Headless` smoke tests still pass unchanged against real data.

---

## Step 0

Not run yet. See [`../../_meta/session-protocol.md`](../../_meta/session-protocol.md).
