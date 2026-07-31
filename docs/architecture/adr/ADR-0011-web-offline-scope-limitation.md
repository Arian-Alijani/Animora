# ADR-0011: Web Offline Scope Limitation

## Status

Accepted

## Context

TECH_STACK §1 states web is "online-first, read-only cache when offline," while desktop is fully
offline-write-capable. TECH_STACK §5 lists `Dexie` and `Serwist` for web, which could technically
support queued mutations, tempting scope creep toward a second offline-write engine. See
[13-web-architecture.md](../13-web-architecture.md).

## Decision

Web offline capability is deliberately limited to a small, explicitly whitelisted read cache
(`Dexie`/IndexedDB) for PWA installability and brief network-flake resilience. Web never queues
mutations offline; a network failure during a write surfaces an inline error for manual retry.

## Alternatives considered

| Alternative | Why rejected |
|---|---|
| Build a web outbox/sync engine mirroring the desktop's | Duplicates [09-sync-architecture.md](../09-sync-architecture.md)'s business logic in a second runtime (TypeScript vs C#), directly violating INV-02's single-business-logic-path intent and roughly doubling the sync test matrix surface for a capability the product does not require of web (TECH_STACK §1 explicitly scopes web as online-first) |
| No offline capability at all on web (not even read cache) | Fails basic PWA resilience expectations (a momentary network blip shouldn't blank the screen) and wastes the `Serwist`/`Dexie` stack choices already made in TECH_STACK §5 |
| Full offline write support only for a narrow subset of screens | Still requires a miniature version of outbox/conflict logic, incurring most of the complexity cost for a fraction of the benefit; rejected as a false economy |

## Consequences

- Positive: web stays simple and fast to build on; the one true offline-write engine (desktop's) is
  never duplicated or allowed to drift out of sync with a second implementation.
- Negative / accepted trade-off: a clinic relying solely on web with an unstable connection gets a
  materially worse offline experience than desktop; this is the intended, product-scoped difference
  between the two clients (TECH_STACK §1), not an oversight.
- Follow-up docs affected: [13-web-architecture.md](../13-web-architecture.md) (WEB-10/WEB-11/WEB-12).
