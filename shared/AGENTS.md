# shared/ — cross-platform .NET source

Scope file. Assumes [`/AGENTS.md`](../AGENTS.md) is already read.

This tree is consumed by **both** `desktop/` and `backend/`. A change here is a change to every
platform: state the impact before editing (AG-06). Duplicating any of this per platform is banned
(INV-02).

## Layout

| Path | Owns | Never contains |
|---|---|---|
| `dotnet/Animora.SharedKernel/Primitives` | `TenantId`, `Result`, base entity, `ISyncedEntity` | anything module-specific |
| `dotnet/Animora.SharedKernel/Money` | `Money` value type, `decimal(18,2)` rules, banker's rounding at persistence (CONV-07/08) | currency variability (IRR only, CONV-09) |
| `dotnet/Animora.SharedKernel/Time` | UTC clock abstraction for deterministic tests (CONV-06) | Jalali formatting — that is a UI concern (CONV-05) |
| `dotnet/Animora.SharedKernel/Sync` | HLC, tombstone contract, field-group metadata | transport/protocol code |
| `dotnet/Animora.SharedKernel/Validation` | `FluentValidation` rules authored once, run by backend **and** desktop handlers (CONV-18) | UI-only presentation checks |
| `dotnet/Animora.Contracts/V1/Dtos` | Wire DTOs derived from the OpenAPI spec | logic, EF Core attributes |
| `dotnet/Animora.Contracts/V1/Enums` | Server-authoritative numeric enums (CONV-10/11) | localized strings |
| `dotnet/Animora.Contracts/Sync` | Sync class per entity (`MutableLWW`/`AppendOnly`/`StateMachine`), field groups, `protocolVersion` (SYNC-R-01/03) | client-specific behavior |
| `dotnet/Animora.Contracts/Errors` | `ERR-{MODULE}-{NNN}` constants mapped 1:1 to RFC 9457 types (CONV-13/15) | message text for UI (localized at the edge) |

## Hard rules

- **SH-01**: `SharedKernel` and `Contracts` are leaf assemblies. They reference **no** module and no
  platform project (DIR-02, AT-02).
- **SH-02**: No platform-conditional code (`#if WINDOWS`, no Avalonia, ASP.NET, EF Core provider, or
  Npgsql references). If it only makes sense on one platform, it does not belong here.
- **SH-03**: Enum values and error codes are append-only; retired entries are marked deprecated and
  never renumbered or reused (CONV-11/14).
- **SH-04**: DTO shapes change only through the OpenAPI-first workflow; generated output is never
  hand-edited (CONV-19/20).
- **SH-05**: Validation rules must be runnable with no I/O — no DB or HTTP lookups inside a shared
  validator, so desktop (offline) and backend behave identically.

## contracts/ (sibling, platform-neutral, non-.NET)

| Path | Holds |
|---|---|
| `contracts/openapi/v1` | The authored API spec — source of truth for all client generation |
| `contracts/openapi/snapshots` | Frozen snapshots for the breaking-change gate (API-VER-05) |
| `contracts/enums` | Enum registry export (numeric ↔ stable name) |
| `contracts/errors` | Error-code registry export |
| `contracts/sync` | Entity-type registry and protocol-version history |

Generation targets (scripts in `tools/codegen`): Kiota →
`desktop/src/Animora.Desktop.Infrastructure/Generated/`; openapi-typescript + Orval →
`web/apps/web/lib/api-client/generated/`. Generated output is committed and drift-checked in CI.
