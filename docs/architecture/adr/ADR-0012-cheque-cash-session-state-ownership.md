# ADR-0012: Cheque and Cash Session State Ownership

## Status

Accepted

## Context

Cheques and cash sessions are both money-adjacent, stateful, and must sync correctly across
devices (e.g., a cheque registered on desktop while offline, later cleared from web) without
allowing two devices to race the same till open/close or double-clear a cheque. See
[05-domain-model.md](../05-domain-model.md), [15-finance-and-ledger.md](../15-finance-and-ledger.md),
[09-sync-architecture.md](../09-sync-architecture.md).

## Decision

Both `Cheque` and `CashSession` are `StateMachine`-classed synced entities (ADR-0001's third
class): non-status fields resolve by field-group LWW, but status transitions are validated against
the entity's fixed state machine at apply time on the server, with invalid/conflicting transitions
rejected and logged rather than merged. `CashSession` additionally enforces DOM-05 (at most one
open session per till per tenant) as a server-side invariant check at apply time, independent of
sync conflict resolution.

## Alternatives considered

| Alternative | Why rejected |
|---|---|
| Treat status as a plain LWW field like any other | A device with a stale-but-later HLC could silently "win" an invalid transition (e.g., re-opening a closed session), corrupting financial state; the state machine must gate this explicitly |
| Make CashSession and Cheque AppendOnly with a derived "current status" view | Loses the ability to represent a genuinely mutable in-flight state (e.g., editing a cheque's due date before clearing) without an append-only log's added complexity; StateMachine class already covers this need without over-engineering |
| Enforce DOM-05 only client-side | An offline device cannot know another device already opened the till; must be re-validated server-side at sync apply time regardless of client-side pre-checks |

## Consequences

- Positive: money-adjacent state transitions are protected from a class of conflicts that plain
  LWW cannot safely resolve, without introducing a fourth conflict class beyond ADR-0001's three.
- Negative / accepted trade-off: a losing state transition requires manual user re-action (e.g.,
  re-attempt the till close after seeing it was already closed elsewhere) rather than silent
  resolution; acceptable because these are inherently business-meaningful conflicts a human should
  see.
- Follow-up docs affected: [05-domain-model.md](../05-domain-model.md),
  [09-sync-architecture.md](../09-sync-architecture.md) (SYNC-R-19), [15-finance-and-ledger.md](../15-finance-and-ledger.md).
