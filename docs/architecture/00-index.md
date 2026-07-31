---
id: 00-index
title: Doc Map & Routing
read_when: ["orienting in the docs", "unsure which file to read"]
topics: [routing, meta]
depends_on: []
stability: stable
---

## Contract

This document decides: how the architecture corpus is organized, and how an agent picks the
minimum file set for a task. It does not decide any architecture rule itself — see
[02-principles-and-invariants.md](02-principles-and-invariants.md) for that.

## How to use this corpus

1. Read `_meta/manifest.yaml` first — it is the routing table (`read_when`, `topics`, `depends_on`).
2. Open only the files whose `read_when` matches your task. Follow `depends_on` one level deep only
   if the target file references something you don't understand.
3. Never read the whole corpus to do one task. If a task needs more than 4 files, the docs have a
   gap — flag it in `_meta/open-questions.md` instead of reading everything.
4. `ARCHITECTURE.md` at repo root is the one-screen entry point; it links here.

## Task -> file routing (fast path)

| Task | Read |
|---|---|
| Add a CRUD entity (non-synced) | [04](04-module-catalog.md), [05](05-domain-model.md), [20](20-extensibility-playbook.md) |
| Add a synced entity | [05](05-domain-model.md), [09](09-sync-architecture.md), [20](20-extensibility-playbook.md) |
| Add an API endpoint | [06](06-api-contract.md), [20](20-extensibility-playbook.md) |
| Add a permission | [10](10-security-and-access-control.md), [20](20-extensibility-playbook.md) |
| Add a plan-gated feature | [11](11-licensing-and-entitlements.md), [20](20-extensibility-playbook.md) |
| Add a report/KPI | [16](16-reporting-and-analytics.md), [07](07-server-data-architecture.md) |
| Add a notification type | [14](14-jobs-and-notifications.md), [20](20-extensibility-playbook.md) |
| Add a background job | [14](14-jobs-and-notifications.md), [03](03-solution-structure.md) |
| Add a desktop screen | [12](12-desktop-architecture.md), [08](08-desktop-local-data.md) |
| Add a web page | [13](13-web-architecture.md), [06](06-api-contract.md) |
| Touch money/invoice/cheque/cash | [15](15-finance-and-ledger.md), [05](05-domain-model.md) |
| Touch attachments | [17](17-files-and-attachments.md), [09](09-sync-architecture.md) |
| Deployment/env-var change | [19](19-deployment-topology.md) |
| Naming/ID/money/error-code question | [21](21-conventions.md) |
| Term is ambiguous | [22](22-glossary.md) |
| "Is this even allowed?" | [02](02-principles-and-invariants.md) |
| Why was X decided this way | matching `adr/ADR-00NN-*.md` |

## Document map

| # | File | One-line scope |
|---|---|---|
| 01 | [01-context-and-drivers.md](01-context-and-drivers.md) | Actors, externals, quality targets, constraints |
| 02 | [02-principles-and-invariants.md](02-principles-and-invariants.md) | Numbered rules (INV-xx) all code/docs must obey |
| 03 | [03-solution-structure.md](03-solution-structure.md) | Projects, dependency directions, NetArchTest rules |
| 04 | [04-module-catalog.md](04-module-catalog.md) | Module list + feature -> module -> endpoint map |
| 05 | [05-domain-model.md](05-domain-model.md) | Aggregates, ER map, invariants, state machines |
| 06 | [06-api-contract.md](06-api-contract.md) | Endpoint taxonomy, versioning, errors, paging |
| 07 | [07-server-data-architecture.md](07-server-data-architecture.md) | Postgres schema, RLS, indexing, search, migrations |
| 08 | [08-desktop-local-data.md](08-desktop-local-data.md) | SQLite schema, SQLCipher, FTS5, outbox |
| 09 | [09-sync-architecture.md](09-sync-architecture.md) | Full sync engine and its failure semantics |
| 10 | [10-security-and-access-control.md](10-security-and-access-control.md) | AuthN/Z, RBAC, tenant isolation, threat model |
| 11 | [11-licensing-and-entitlements.md](11-licensing-and-entitlements.md) | Plans, license token, heartbeat, degradation |
| 12 | [12-desktop-architecture.md](12-desktop-architecture.md) | Avalonia layering, MVVM, offline UX, startup |
| 13 | [13-web-architecture.md](13-web-architecture.md) | App Router segmentation, data layer, web offline |
| 14 | [14-jobs-and-notifications.md](14-jobs-and-notifications.md) | Job taxonomy, notification pipeline |
| 15 | [15-finance-and-ledger.md](15-finance-and-ledger.md) | Ledger, invoices, cash session, cheques |
| 16 | [16-reporting-and-analytics.md](16-reporting-and-analytics.md) | Read-model strategy, KPI catalog |
| 17 | [17-files-and-attachments.md](17-files-and-attachments.md) | S3 layout, upload flow, access control |
| 18 | [18-observability-and-operations.md](18-observability-and-operations.md) | Logs/traces/metrics, backup/restore, admin ops |
| 19 | [19-deployment-topology.md](19-deployment-topology.md) | Topology A/B, containers, env-var contract |
| 20 | [20-extensibility-playbook.md](20-extensibility-playbook.md) | Step-by-step recipes for common changes |
| 21 | [21-conventions.md](21-conventions.md) | Naming, IDs, time, money, enums, errors |
| 22 | [22-glossary.md](22-glossary.md) | EN/FA canonical terms |
| 23 | [23-architecture-risks.md](23-architecture-risks.md) | Design-level risks, signals, escape hatches |

`_meta/manifest.yaml` mirrors this table in machine-readable form for agents. `_meta/open-questions.md`
holds unresolved decisions. `adr/` holds one ADR per contested call.

## Reading conventions

- RFC 2119 keywords (MUST/SHOULD/MAY) are normative.
- Stable IDs (`INV-`, `SYNC-R-`, `PERM-`, `ERR-`) are citable from code comments and other docs.
- A fact lives in exactly one file. If you see it twice, one copy is stale — fix it here, not there.

## Keeping docs in sync with code

- A PR that adds/changes a module, entity, endpoint, permission, job, or env var MUST update the
  matching doc in the same PR. Reviewers reject PRs that change behavior without a doc diff.
- A PR that adds a contested technical decision MUST add an ADR (copy `adr/ADR-0000-template.md`).
- `_meta/manifest.yaml` size_lines are approximate budgets, not soft targets — keep files lean.
