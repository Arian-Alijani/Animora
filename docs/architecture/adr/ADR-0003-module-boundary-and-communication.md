# ADR-0003: Module Boundary and Communication Style

## Status

Accepted

## Context

TECH_STACK §6 fixes "modular monolith, one process, modules communicate via `Mediator` in-process
only (no shared DB tables across modules)" and explicitly bans microservices, RabbitMQ, Kafka,
MassTransit (§18). A concrete boundary/enforcement mechanism is still needed. See
[03-solution-structure.md](../03-solution-structure.md).

## Decision

Each business capability is a separate project (`Animora.Modules.X`) with its own schema
(DATA-01), exposing exactly one public in-process contract interface plus `Mediator` request/
notification types; internal types are `internal`. Cross-module calls are `Mediator` requests only.
Boundaries are enforced by `NetArchTest` rules in CI (AT-01 through AT-07), not by convention alone.

## Alternatives considered

| Alternative | Why rejected |
|---|---|
| Single shared `DbContext`/schema for everything | Makes module boundaries unenforceable and a future split (Topology B / eventual service extraction) far more expensive |
| Microservices with a message broker | Explicitly banned (TECH_STACK §18); wrong fit for a 2 vCPU/4 GB single host |
| Convention-only boundaries (no automated check) | Boundaries erode silently over time with multiple contributors/agents; TECH_STACK §16 explicitly requires NetArchTest for this reason |

## Consequences

- Positive: a future extraction of any module into its own process is mechanically bounded by
  already-enforced dependency rules; cheap to reason about for AI agents (INV-01, INV-18).
- Negative / accepted trade-off: cross-module workflows that would be a single SQL join in a
  monolithic schema now require either a `Mediator` round-trip or a read-model composed at the API
  layer; accepted as the cost of keeping the split-migration path cheap.
- Follow-up docs affected: [03-solution-structure.md](../03-solution-structure.md),
  [04-module-catalog.md](../04-module-catalog.md).
