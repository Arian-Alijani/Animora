# ADR-0006: Entitlement Enforcement Split Between Token and Server

## Status

Accepted

## Context

Desktop must work fully offline for the product's offline quality target, yet entitlement
enforcement must survive a patched/tampered offline client (anti-crack posture, TECH_STACK §9).
The license token (PASETO v4.public) is the only entitlement signal available while offline. See
[11-licensing-and-entitlements.md](../11-licensing-and-entitlements.md).

## Decision

The license token is authoritative for offline UX gating only (what the desktop shows/allows
locally while disconnected). Every server-mediated action (sync push, payment, notification
dispatch, report generation, any endpoint call) re-validates entitlements against the server's own
`Entitlement` snapshot on every request, independent of what the client's cached token claims.

## Alternatives considered

| Alternative | Why rejected |
|---|---|
| Token is sole authority everywhere, including server endpoints | A modified/expired cached token or a patched client could grant unpaid access to server resources; unacceptable given anti-crack requirement |
| Server-only, no local token at all | Breaks the offline-first promise — desktop would have no way to gate/inform UX while disconnected, and every screen would need a network round-trip just to know what to show |
| Heartbeat-only (no cryptographic token) | Loses the ability to do meaningful offline UX gating during the grace window; heartbeat alone doesn't carry claims for local decisions |

## Consequences

- Positive: the offline promise and the anti-tamper posture are both satisfiable because they
  operate on different boundaries (local UX vs server-mediated action) that never contradict each
  other.
- Negative / accepted trade-off: a patched desktop can always show/allow whatever it wants locally
  for local-only data (accepted, LIC-17); this is a bounded, documented limit, not a silent gap.
- Follow-up docs affected: [11-licensing-and-entitlements.md](../11-licensing-and-entitlements.md),
  [10-security-and-access-control.md](../10-security-and-access-control.md) (INV-16).
