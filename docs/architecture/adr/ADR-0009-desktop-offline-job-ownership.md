# ADR-0009: Desktop Offline Job/Reminder Ownership

## Status

Accepted

## Context

Reminders (vaccination due, appointment, cheque due) must fire even when a clinic is offline for
days; TECH_STACK §10 explicitly requires "reminders are computed and fired locally from the SQLite
reminder table; server jobs handle only remote channels." See
[14-jobs-and-notifications.md](../14-jobs-and-notifications.md), [08-desktop-local-data.md](../08-desktop-local-data.md).

## Decision

Reminder *computation* is duplicated intentionally: the desktop's local `BackgroundService` +
`PeriodicTimer` job evaluates the same due-date rules against its own locally synced data to fire
local channels (toast, in-app); the server's Hangfire recurring jobs independently evaluate the
same rules against server data to fire remote channels (SMS, email, push). Neither side waits for
or depends on the other.

## Alternatives considered

| Alternative | Why rejected |
|---|---|
| Server-only computation, desktop just displays what the server already decided | Fails the offline-first requirement outright — an offline clinic gets zero alerts until reconnecting, contradicting TECH_STACK §10 explicitly |
| Desktop-only computation, server never independently evaluates | Web-only tenants (a clinic that only uses the web app, no desktop) would get no reminders at all; SMS/email must originate server-side regardless |
| Single shared "reminder decision service" called by both, over network | Requires desktop to be online to compute reminders, defeating the purpose; not achievable within TECH_STACK's offline constraints |

## Consequences

- Positive: an offline clinic still gets local alerts (TECH_STACK §10 requirement met exactly);
  no cross-process dependency for correctness.
- Negative / accepted trade-off: reminder rule logic exists in two runtimes (server C#, desktop
  C#) and must be kept behaviorally identical; mitigated by sharing the rule definitions through
  `Animora.SharedKernel` where the rule shape allows it, and by the alert-source catalog being the
  single documented definition of each rule's trigger condition ([14-jobs-and-notifications.md](../14-jobs-and-notifications.md)).
- Follow-up docs affected: [14-jobs-and-notifications.md](../14-jobs-and-notifications.md),
  [08-desktop-local-data.md](../08-desktop-local-data.md).
