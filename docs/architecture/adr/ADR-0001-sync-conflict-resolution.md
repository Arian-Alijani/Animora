# ADR-0001: Sync Conflict Resolution Model

## Status

Accepted

## Context

Desktop is offline-first for days; multiple devices/staff may edit the same patient/appointment
record before reconnecting. TECH_STACK §8 mandates "field-level Last-Write-Wins, except money/
ledger which is append-only." A conflict model must be simple enough to implement correctly on a
weak server and a source-generator-only (`Mediator`, `Mapperly`) desktop stack, with no CRDT
library available (none is listed in TECH_STACK). See [09-sync-architecture.md](../09-sync-architecture.md).

## Decision

Classify every synced entity into exactly one of three conflict classes at design time:
`MutableLWW` (field-group Last-Write-Wins by HLC), `AppendOnly` (no conflict possible by
construction), `StateMachine` (LWW on non-status fields, validated transition on status). Field
groups (not whole-entity granularity) are the LWW unit for `MutableLWW`.

## Alternatives considered

| Alternative | Why rejected |
|---|---|
| Whole-entity LWW (no field groups) | Two staff editing different fields of the same patient would silently clobber each other; unacceptable for clinical trust |
| CRDTs (e.g., automerge-style) | No such library in TECH_STACK; would require a new dependency and a proposal per rule §17/§18, and is disproportionate complexity for this domain |
| Manual merge UI for every conflict | Support/UX cost too high for a small clinic staff; deferred as unnecessary given field-group granularity already resolves the common case |
| Server-wins-always | Defeats the offline-first promise; a desktop edit made confidently offline could be silently discarded |

## Consequences

- Positive: implementable with plain HLC comparison and no new dependency; deterministic and
  testable via the mandatory sync test matrix.
- Negative / accepted trade-off: a genuine same-field-group conflict has a silent loser (logged,
  not merged); acceptable per product risk tolerance, tracked in
  [23-architecture-risks.md](../23-architecture-risks.md).
- Follow-up docs affected: [05-domain-model.md](../05-domain-model.md) (state machines),
  [09-sync-architecture.md](../09-sync-architecture.md) (full rules).
