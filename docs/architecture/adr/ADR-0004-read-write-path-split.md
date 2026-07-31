# ADR-0004: Read/Write Path Split (EF Core vs Dapper)

## Status

Accepted

## Context

TECH_STACK §6/§7/§18 mandates EF Core for writes/domain and Dapper for hot reads/reports, and
explicitly bans "Dapper for writes, EF Core on hot report paths." The server must sustain
acceptable p95 latency for reports on a 2 vCPU/4 GB host while keeping domain write logic
maintainable. See [07-server-data-architecture.md](../07-server-data-architecture.md).

## Decision

All domain writes and single-aggregate validation reads go through EF Core `DbContext` per module.
All list/report/export/dashboard reads go through Dapper against views, materialized views, or
hand-tuned SQL. The split is enforced by `NetArchTest` rules AT-04/AT-05, not left to reviewer
discretion.

## Alternatives considered

| Alternative | Why rejected |
|---|---|
| EF Core everywhere, including reports | EF Core's query translation and change-tracking overhead is measurably worse for large aggregate report queries on a weak server; explicitly banned by TECH_STACK §18 |
| Dapper everywhere, including writes | Loses EF Core's migration tooling, change tracking, and validation integration for domain writes; explicitly banned by TECH_STACK §18 |
| Per-developer/per-agent discretion, no enforced rule | Drift is inevitable across many small PRs/agents; violates INV-18's "cheap to consume, no ambiguity" goal |

## Consequences

- Positive: predictable performance profile; report queries are hand-tunable independent of the
  domain model's EF Core mapping concerns.
- Negative / accepted trade-off: two query technologies to know per module; mitigated by the fixed
  rule (INV-20) removing any per-feature decision cost.
- Follow-up docs affected: [07-server-data-architecture.md](../07-server-data-architecture.md),
  [16-reporting-and-analytics.md](../16-reporting-and-analytics.md), [03-solution-structure.md](../03-solution-structure.md).
