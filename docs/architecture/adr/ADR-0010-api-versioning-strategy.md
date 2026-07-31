# ADR-0010: API Versioning and N-1 Compatibility Strategy

## Status

Accepted

## Context

Desktop builds ship and stay in the field for months; TECH_STACK §6 fixes `Asp.Versioning` URL
path versioning and "old desktop builds must keep working -> never break a shipped version." A
concrete compatibility window and breaking-change gate are needed. See
[06-api-contract.md](../06-api-contract.md).

## Decision

URL path versioning (`/api/v1`, `/api/v2`, ...). Minor/patch-level changes are additive-only
(INV-10). A breaking change requires a major version bump; the server serves the current and N-1
major versions simultaneously. CI runs an OpenAPI snapshot diff (breaking-change gate) that fails
the build on any non-additive change without a version bump.

## Alternatives considered

| Alternative | Why rejected |
|---|---|
| Header-based versioning (`Accept: application/vnd.animora.v2+json`) | Less discoverable/debuggable in a small team/agent context; URL versioning is simpler to reason about for OpenAPI tooling and desktop client generation |
| No formal versioning, "just don't break things" | Unenforceable without a mechanical gate; a single careless PR could break every shipped desktop build with no warning |
| Support unlimited historical versions indefinitely | Unbounded maintenance burden on a small team/weak server; N-1 (current + immediately prior) is the documented, bounded compromise |

## Consequences

- Positive: mechanically enforced compatibility; desktop update cadence is decoupled from server
  deploy cadence within the N-1 window.
- Negative / accepted trade-off: the server must carry two major-version code paths during any
  transition period; this cost is time-boxed by monitoring N-1 client telemetry and retiring the
  old version once usage reaches zero for a sustained period.
- Follow-up docs affected: [06-api-contract.md](../06-api-contract.md),
  [09-sync-architecture.md](../09-sync-architecture.md) (protocolVersion is a separate, related
  mechanism).
