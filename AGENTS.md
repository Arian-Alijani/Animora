# AGENTS.md — Animora

Agent entry point. Read this file fully, then read **only** the scope file for the platform you were
asked to change. Everything else is opt-in via the routing tables below.

Animora is a multi-tenant veterinary clinic management SaaS: Windows desktop (offline-first,
full-write) + Web (online-first) + one backend, one API contract. Persian / RTL / Jalali, IRR.

## 1. Delivery phases (current gate)

| Phase | Scope | Status |
|---|---|---|
| **P1** | Windows desktop: every feature, screen, table, dialog, report — fully implemented against local SQLite, no server required | **ACTIVE** |
| P2 | Backend: API contract, modules, Postgres, sync server | Blocked until P1 is signed off by the product owner |
| P3 | Web: Next.js app + marketing | Blocked until P2 |

Rules for the active gate:

- **AG-01**: In P1, do not write code in `backend/`, `web/`, or `infra/`. Those trees exist as
  structural seams only. Creating placeholder code there is waste, not preparation.
- **AG-02**: In P1 the desktop is the requirements laboratory. When a feature detail is
  under-specified, ask the user; do not invent business rules or silently pick one.
- **AG-03**: P1 code must never depend on network availability to function (INV-15). Server-bound
  paths (`Animora.Desktop.Sync`, `Animora.Desktop.Infrastructure/Generated`) stay as empty seams
  until P2.
- **AG-04**: Do not propose or start P2/P3 work "while we are here". Phase order is a hard
  constraint.

## 2. Scope isolation (token discipline)

**AG-05**: One task touches one platform tree. Never read another platform's tree to answer a
question about yours — the shared truth lives in `docs/` and `contracts/`, not in sibling code.

| You were asked to work on | Read this scope file | Allowed write paths |
|---|---|---|
| Desktop app (P1) | [`desktop/AGENTS.md`](desktop/AGENTS.md) | `desktop/`, `shared/`, `contracts/` |
| Backend API (P2) | [`backend/AGENTS.md`](backend/AGENTS.md) | `backend/`, `shared/`, `contracts/` |
| Web app (P3) | [`web/AGENTS.md`](web/AGENTS.md) | `web/`, `contracts/` |
| Shared .NET code / wire contracts | [`shared/AGENTS.md`](shared/AGENTS.md) | `shared/`, `contracts/` |
| Deployment, containers, TLS, backups | [`docs/architecture/19-deployment-topology.md`](docs/architecture/19-deployment-topology.md) | `infra/`, `.github/` |

**AG-06**: A change to `shared/` or `contracts/` affects every platform. State the cross-platform
impact in the PR description before making it.

## 3. Repository map

```
AGENTS.md              this file: phases, scope isolation, code standard
ARCHITECTURE.md        one-screen architecture entry point
contracts/             platform-neutral wire artifacts: openapi/, enums/, errors/, sync/
shared/dotnet/         Animora.SharedKernel, Animora.Contracts (backend + desktop, single source)
desktop/               Avalonia Windows client   (P1) — src/, tests/, AGENTS.md
backend/               ASP.NET Core modular monolith (P2) — src/, tests/, AGENTS.md
web/                   Next.js workspace         (P3) — apps/, packages/, AGENTS.md
infra/                 compose, caddy, postgres, minio, backup, env templates
tools/                 codegen/ (Kiota, Orval) and scripts/
docs/                  TECH_STACK.md + architecture/ corpus (the normative rules)
```

## 4. Doc routing — never read the whole corpus

1. `docs/architecture/_meta/manifest.yaml` is the machine-readable routing table (`read_when`,
   `topics`, `depends_on`). Start there.
2. `docs/architecture/00-index.md` has the task → file fast path.
3. **AG-07**: open 2–4 docs per task, maximum. If a task seems to need more, the docs have a gap:
   record it in `docs/architecture/_meta/open-questions.md` instead of reading everything.
4. **AG-08**: `docs/TECH_STACK.md` is immutable. Never propose an alternative library for something
   already chosen; never implement anything from its §18 (Do NOT Use) or §19 (Out of Scope).
5. **AG-09**: A fact lives in exactly one doc. Do not restate a rule in code comments, in a second
   doc, or in a PR description — cite its stable ID (`INV-03`, `SYNC-R-14`, `DESK-ARCH-09`,
   `WEB-10`, `CONV-07`, `AT-05`).

## 5. Non-negotiables

The 10 hard invariants are listed in [`ARCHITECTURE.md`](ARCHITECTURE.md); the full numbered set with
enforcement notes is in
[`docs/architecture/02-principles-and-invariants.md`](docs/architecture/02-principles-and-invariants.md).
The four that get violated most often by accident:

- `UUIDv7` PKs and tombstone soft-delete on every synced entity (INV-03, INV-04).
- Money is `decimal(18,2)` / `numeric(18,2)`, ledger rows are append-only (INV-05, INV-06).
- UTC everywhere in storage and logic; Jalali only at the UI binding edge (INV-13, CONV-04/05).
- No hand-written API DTOs; wire types are generated from the OpenAPI spec (CONV-19/20).

**AG-10**: If a requested change conflicts with an invariant, stop and say so. Do not implement a
workaround around an invariant.

## 6. Code standard

- **AG-11**: All identifiers, comments, commit messages, and PR text are **English**. Persian appears
  only in user-facing resource/localization files.
- **AG-12**: Naming follows [`docs/architecture/21-conventions.md`](docs/architecture/21-conventions.md)
  (C# PascalCase, DB `snake_case`, JSON `camelCase`, routes kebab-case, `ERR-{MODULE}-{NNN}`,
  `{resource}.{action}` permissions).
- **AG-13**: New files go in an existing folder from the platform's directory map. Adding a
  project/assembly or a new top-level folder requires a doc update in the same PR
  ([`03-solution-structure.md`](docs/architecture/03-solution-structure.md)).
- **AG-14**: Prefer editing an existing file over adding one. No parallel implementations of a
  concern that already has an owner (see the module catalog).

## 7. Comment standard

Comments are written for the next agent's context budget, not for decoration.

**Required**

- **CM-01**: Comment **why**, never **what**. If the code says it, the comment repeats it — delete
  the comment instead.
- **CM-02**: Cite the stable rule ID rather than re-explaining the rule.
  Good: `// Outbox row shares this transaction (DESK-ARCH-09).`
  Bad: a 4-line paragraph re-describing the outbox pattern.
- **CM-03**: Comment these cases, because they are invisible in the code: invariant enforcement,
  transaction/concurrency boundaries, idempotency and completion-detection of jobs (JOB-01),
  intentional deviations from an obvious implementation, and any non-obvious performance choice.
- **CM-04**: Workarounds state the cause and the exit condition:
  `// Semi.Avalonia clips RTL headers on nested grids; remove when the theme ships the fix.`
- **CM-05**: `/// ` XML docs only on cross-boundary public surfaces: module contract interfaces,
  `SharedKernel` primitives, public contract types. TSDoc only on exported members of
  `web/packages/*`. Internal members get no doc block unless the contract is non-obvious.
- **CM-06**: Unfinished work uses `// TODO(<phase-or-issue-id>): <action>` — e.g.
  `// TODO(P2): replace the local stub with the Kiota client call.` A TODO without an owner tag or
  action is not allowed.

**Forbidden**

- **CM-07**: No emoji, no ASCII art, no banner/separator comment lines (`// ==== SECTION ====`), no
  decorative headers. Structure comes from files and folders.
- **CM-08**: No history, changelog, author, date, ticket-log, or "modified by" comments. Git owns
  history.
- **CM-09**: No commented-out code. Delete it.
- **CM-10**: No self-referential or conversational notes: no "AI generated", "as requested",
  "note to reviewer", "hopefully this works", no apologies, no restating the prompt.
- **CM-11**: No file-header block comments (no license/description preamble).
- **CM-12**: `FIXME`, `HACK`, `XXX` are banned. Either fix it, or write a `TODO(...)` with an
  action, or raise it in `_meta/open-questions.md`.

## 8. Workflow

- **AG-15**: Follow the matching recipe in
  [`docs/architecture/20-extensibility-playbook.md`](docs/architecture/20-extensibility-playbook.md)
  and satisfy its stated Definition of Done. Do not invent a different sequence.
- **AG-16**: A PR that changes a module, entity, endpoint, permission, job, or env var updates the
  owning doc in the same PR. A contested technical call adds an ADR (copy
  `docs/architecture/adr/ADR-0000-template.md`).
- **AG-17**: Commit messages: `type(scope): summary` — `type` ∈ `feat|fix|refactor|docs|test|chore|
  build`, `scope` = platform or module (`desktop-finance`, `backend-sync`, `web-app`, `shared`,
  `docs`). Imperative, one line, no emoji, no trailing period.
- **AG-18**: Commit after every completed change; squash to one commit per PR; branch
  `genspark_ai_developer` → `main`.
- **AG-19**: Report what you changed and what you deliberately did not change. Never claim a
  verification you did not run.
