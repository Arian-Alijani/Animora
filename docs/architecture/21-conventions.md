---
id: 21-conventions
title: Conventions
read_when: ["naming, id, time, money, error-code questions"]
topics: [naming, ids, money, errors]
depends_on: []
stability: stable
---

## Contract

Decides: cross-cutting naming canon for code/DB/API/events/files, ID rules, time/Jalali handling,
money handling, enum registry/wire-value policy, error-code taxonomy, pagination/filter shapes,
validation placement, and DTO/contract rules. Does not decide domain-specific naming already fixed
elsewhere (e.g., entity names — see [22-glossary.md](22-glossary.md) for canonical EN/FA terms).

## Naming

| Surface | Convention | Example |
|---|---|---|
| C# types/members | PascalCase types, camelCase locals, `I`-prefixed interfaces | `IFinanceContract`, `invoiceId` |
| Database tables/columns | `snake_case`, singular table names | `ledger_entry`, `tenant_id` |
| API JSON fields | `camelCase` | `tenantId`, `invoiceNumber` |
| API routes | kebab-case, plural resource | `/api/v1/invoice-templates` |
| Domain events | `PascalCase`, past tense | `AppointmentCancelled`, `InvoiceIssued` |
| Files (docs, code) | kebab-case for docs, per-language convention for code | `09-sync-architecture.md`, `InvoiceService.cs` |
| Error codes | `ERR-{MODULE}-{NNN}` | `ERR-FINANCE-014` |
| Permission claims | `{resource}.{action}` | `invoices.void` |

## ID rules

- CONV-01: every synced entity uses `UUIDv7` (INV-03); non-synced, server-only lookup tables may
  use integer surrogate keys.
- CONV-02: IDs are generated client-side for desktop-originated entities (so an offline create has
  a permanent id immediately) and server-side for server-originated entities (e.g., a
  `PaymentTransaction` created purely by a webhook) — either way, `UUIDv7` guarantees no collision
  and preserves rough creation-time ordering.
- CONV-03: IDs are never reused, resequenced, or exposed as sequential/guessable business numbers;
  human-facing sequence numbers (invoice number) are a separate, tenant-scoped counter field, not
  the primary key.

## Time and Jalali handling

- CONV-04: all storage and inter-service exchange is UTC (`timestamptz` server, UTC ticks desktop)
  — INV-13.
- CONV-05: Jalali conversion happens only at the UI binding edge (desktop: `.NET PersianCalendar`
  formatters; web: `date-fns-jalali`) — domain and handler code never constructs or compares Jalali
  dates directly.
- CONV-06: `DateTime.Now`/local-time construction is banned (TECH_STACK §18); use injected UTC time
  providers so tests can control time deterministically.

## Money handling

- CONV-07: `decimal(18,2)` in code, `numeric(18,2)` in PostgreSQL, always (INV-05).
- CONV-08: rounding is banker's rounding applied once, at persistence (FIN-20); intermediate
  calculations keep full precision.
- CONV-09: currency is always `IRR`; no currency field variability is designed for (TECH_STACK
  §19 multi-currency out of scope).

## Enum registry and wire-value policy

- CONV-10: enums are server-authoritative, shared numeric values on the wire, never localized
  strings (TECH_STACK §3). A single enum registry (versioned alongside `Animora.Contracts`) is the
  source of truth for every enum's numeric-to-name mapping.
- CONV-11: enum values are append-only; a retired value is marked deprecated in the registry and
  never renumbered or reused (mirrors API-VER-02's additive-only policy).
- CONV-12: UI-facing labels for enum values are localized (Persian) at the presentation layer via a
  lookup keyed by the enum's stable name, never by its numeric value directly in UI code.

## Error-code taxonomy and namespace

- CONV-13: format `ERR-{MODULE}-{NNN}` where `{MODULE}` matches a module catalog name
  ([04-module-catalog.md](04-module-catalog.md)): `IDENTITY`, `CLIENTS`, `VISITS`, `SCHEDULING`,
  `FINANCE`, `REPORTING`, `NOTIFICATIONS`, `LICENSING`, `FILES`, `SYNC`, `PLATFORMADMIN`, or the
  cross-cutting `COMMON` namespace (e.g., `ERR-COMMON-VERSION-CONFLICT`).
- CONV-14: `{NNN}` is a monotonically assigned 3-digit number per module, never reused after
  retirement (mirrors CONV-11).
- CONV-15: every error code maps to exactly one RFC 9457 `type` URI and one stable `title`; clients
  branch on `code` only (INV-07).

## Pagination and filter shapes

- CONV-16: keyset pagination only (API-PAGE-01, [06](06-api-contract.md)); query shape
  `?after={cursor}&limit={n}`, response `{ data: [...], meta: { nextCursor, hasMore } }`.
- CONV-17: filters are explicit, documented, typed query parameters per resource — no generic
  query-DSL parameter.

## Validation placement

- CONV-18: all business-rule validation lives in `FluentValidation` validators shared between
  backend and desktop (via `Animora.SharedKernel.Validation`, [03](03-solution-structure.md));
  ViewModels and React components perform only presentation-level checks (required-field UX,
  format hints) that mirror but never replace the shared validator, which is what the server always
  runs regardless of client-side outcome.

## DTO / contract rules

- CONV-19: no hand-written API DTOs on any client (TECH_STACK §18); all wire types originate from
  the OpenAPI spec via Kiota (desktop) or openapi-typescript/Orval (web).
- CONV-20: a DTO's shape changes only through the OpenAPI-first workflow ([06](06-api-contract.md));
  editing generated code by hand is forbidden — fix the spec and regenerate.

## Test naming

- CONV-22: test method names use underscore-separated `MethodOrScenario_Condition_ExpectedResult`
  form (e.g. `FormatTomans_rounds_a_fractional_toman_remainder_to_the_nearest_whole_toman`) for
  readability; test methods are not public API surface, so CA1707 is disabled for `**/tests/**.cs`
  in `.editorconfig` rather than suppressed per-method.

## Logging fields

- CONV-21: every structured log line includes, when available: `timestamp` (UTC), `level`,
  `traceId`, `tenantId`, `correlationId` (for job/sync-triggered work), `module`. PII fields are
  scrubbed per OBS-04 ([18](18-observability-and-operations.md)) before emission.
