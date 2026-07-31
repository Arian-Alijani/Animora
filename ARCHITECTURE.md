# Animora Architecture

Multi-tenant veterinary clinic management SaaS. Windows desktop (offline-first, full-write, days
without internet) + Web (online-first, read-cache only) + one backend, one API contract. Persian/
RTL/Jalali, IRR-Toman, deployed on a single 2 vCPU / 4 GB Linux VPS. Technology is fixed by
`docs/TECH_STACK.md` — authoritative, immutable, never re-litigated here.

## The 10 hard invariants

| ID | Rule |
|---|---|
| INV-01 | Backend modules communicate only in-process via `Mediator`; never shared tables. |
| INV-03 | Every synced entity uses `UUIDv7`; never DB identity/sequence PKs. |
| INV-04 | Every synced entity soft-deletes via tombstone; hard delete forbidden. |
| INV-06 | Ledger rows are append-only; UPDATE/DELETE after commit is forbidden. |
| INV-07 | Every API error is RFC 9457 `problem+json` with a stable `code`; clients switch on `code`. |
| INV-08 | Every tenant table has PostgreSQL RLS; query filters are defense in depth, not the only control. |
| INV-09 | Every endpoint is permission-checked server-side; client gating is UX only. |
| INV-10 | API changes are additive unless major-versioned; server supports current + N-1. |
| INV-15 | Desktop stays fully write-capable offline for the product's offline target; no silent connectivity requirement. |
| INV-16 | Entitlements are re-checked server-side on every gated request; the license token is an offline UX accelerant, never the sole gate. |

Full list with rationale and enforcement: [`docs/architecture/02-principles-and-invariants.md`](docs/architecture/02-principles-and-invariants.md).

## Where to go next

| Need | Read |
|---|---|
| Orient yourself in the docs | [`docs/architecture/00-index.md`](docs/architecture/00-index.md) |
| Machine-readable routing table | [`docs/architecture/_meta/manifest.yaml`](docs/architecture/_meta/manifest.yaml) |
| Product/quality context | [`docs/architecture/01-context-and-drivers.md`](docs/architecture/01-context-and-drivers.md) |
| Technology choices (immutable) | [`docs/TECH_STACK.md`](docs/TECH_STACK.md) |
| Implement any new feature | [`docs/architecture/20-extensibility-playbook.md`](docs/architecture/20-extensibility-playbook.md) |
| Why a contested call was made | [`docs/architecture/adr/`](docs/architecture/adr/) |

This file is intentionally minimal. Every other fact lives in exactly one file under
`docs/architecture/` — do not duplicate it here.
