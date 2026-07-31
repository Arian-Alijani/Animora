# Animora

Multi-tenant veterinary clinic management SaaS for the Iranian market: Windows desktop
(offline-first, fully write-capable without internet), Web (online-first), and a single backend with
one API contract. Persian / RTL / Jalali, IRR-Toman, deployed on a single 2 vCPU / 4 GB Linux VPS.

## Build order

1. **Phase 1 — Desktop (current)**: implement every feature, screen, and table on the Avalonia
   Windows client against local encrypted SQLite. No server required.
2. **Phase 2 — Backend**: API contract, modular monolith, PostgreSQL, sync server.
3. **Phase 3 — Web**: Next.js app and marketing site.

## Repository layout

| Path | Contents |
|---|---|
| `AGENTS.md` | AI agent entry point: phases, scope isolation, code and comment standard |
| `ARCHITECTURE.md` | One-screen architecture entry point |
| `docs/TECH_STACK.md` | Immutable technology choices |
| `docs/architecture/` | Normative architecture corpus (`_meta/manifest.yaml` is the routing table) |
| `contracts/` | Platform-neutral wire artifacts: OpenAPI spec, enum/error/sync registries |
| `shared/dotnet/` | `Animora.SharedKernel`, `Animora.Contracts` — shared by desktop and backend |
| `desktop/` | Avalonia Windows client (`src/`, `tests/`) |
| `backend/` | ASP.NET Core modular monolith (`src/`, `tests/`) |
| `web/` | Next.js workspace (`apps/`, `packages/`) |
| `infra/` | Compose, Caddy, Postgres, MinIO, backup, env templates |
| `tools/` | Client code generation (Kiota, Orval) and dev scripts |

Each platform root has its own `AGENTS.md`; work on one platform never requires reading another.

## Documentation rules

A fact lives in exactly one document. Technology choices are fixed in `docs/TECH_STACK.md`;
architecture rules carry stable IDs (`INV-`, `DESK-ARCH-`, `WEB-`, `SYNC-R-`, `CONV-`) that code and
PRs cite instead of restating.
