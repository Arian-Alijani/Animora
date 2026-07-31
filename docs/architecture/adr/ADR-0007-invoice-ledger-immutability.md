# ADR-0007: Invoice/Ledger Immutability Model

## Status

Accepted

## Context

TECH_STACK §3 mandates money is append-only ledger, never rewritten. Invoices must support template
preview, professional presentation, and correction workflows without violating that mandate. See
[15-finance-and-ledger.md](../15-finance-and-ledger.md), [05-domain-model.md](../05-domain-model.md).

## Decision

`Draft` invoices are mutable, unsynced-as-financial-fact working copies. `Issued` (and later)
invoices are immutable; corrections happen exclusively via a linked `CreditNote` that posts its own
offsetting `LedgerEntry`. `LedgerEntry` itself is append-only at the database level (trigger-denied
UPDATE/DELETE), with reversals expressed as new rows referencing the original.

## Alternatives considered

| Alternative | Why rejected |
|---|---|
| Allow editing an issued invoice's line items directly | Breaks append-only ledger guarantee and destroys the audit trail a clinic needs for tax/dispute purposes |
| Soft "correction mode" that mutates ledger rows in place with an audit note | Still violates INV-06 at the storage level; audit note is weaker than a structurally enforced trigger |
| No draft state at all (every invoice creation is immediately a ledger fact) | Breaks the "live invoice preview before issue" requirement, which needs a safe, side-effect-free working state |

## Consequences

- Positive: financial history is tamper-evident and dispute-defensible by construction; preview
  requirement is satisfied without any ledger risk.
- Negative / accepted trade-off: a corrected invoice results in two visible documents (original +
  credit note) rather than a single "fixed" record; this is standard accounting practice and
  accepted as correct behavior, not a workaround.
- Follow-up docs affected: [05-domain-model.md](../05-domain-model.md) (invoice state machine),
  [15-finance-and-ledger.md](../15-finance-and-ledger.md).
