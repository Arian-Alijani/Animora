# web/ — Next.js workspace (Phase 3, BLOCKED)

Scope file. Assumes [`/AGENTS.md`](../AGENTS.md) is already read. Do not read `desktop/` or
`backend/` — the API surface you need is `contracts/openapi/`, not backend source.

**Gate**: Phase 3 starts after the backend phase (AG-01). Until then this tree holds structure only.

Normative docs — open only what the task needs:
[`13-web-architecture.md`](../docs/architecture/13-web-architecture.md) (segmentation, RSC boundary,
caching, offline scope, budgets), [`06-api-contract.md`](../docs/architecture/06-api-contract.md),
[`20-extensibility-playbook.md`](../docs/architecture/20-extensibility-playbook.md) ("add a web page").
Stack: `TECH_STACK.md` §5.

## Workspace map

| Path | Owns |
|---|---|
| `apps/web/app/(marketing)` | Public pages, SSG/ISR only |
| `apps/web/app/(app)` | Authenticated clinic app, always dynamic |
| `apps/web/app/(admin)` | Platform back-office, always dynamic |
| `apps/web/components` | App-local components (anything reusable across apps belongs in `packages/ui`) |
| `apps/web/lib/api-client/generated` | `openapi-typescript` types + Orval TanStack Query hooks — generated, never hand-edited |
| `apps/web/lib/auth` | httpOnly cookie session handling (WEB-07) |
| `apps/web/lib/offline` | Dexie read cache + Serwist service worker |
| `apps/web/lib/i18n` + `apps/web/messages` | `next-intl` setup, `fa` default, `en` scaffold |
| `apps/web/styles` | Tailwind layers and tokens |
| `packages/ui` | shared shadcn/ui-based components, RTL tokens |
| `packages/config` | shared Biome / Tailwind / TS config |

## Hard rules for this tree

- **WB-01**: Only `(marketing)` may use ISR/static caching; `(app)` and `(admin)` are dynamic —
  tenant/entitlement data is never edge-cached (WEB-01, WEB-06).
- **WB-02**: Server components fetch first-paint data; client components own interactivity and
  refetching via TanStack Query against the same generated client (WEB-02/03/04).
- **WB-03**: `Zustand` holds UI state only. Server data lives in the TanStack Query cache (WEB-05).
- **WB-04**: All wire types and hooks come from `lib/api-client/generated`; `zod` form schemas derive
  from the same spec fields. No hand-written DTOs or fetch wrappers (WEB-08/09, DIR-06).
- **WB-05**: Tokens are never stored in `localStorage`; session transport is the httpOnly cookie
  (WEB-07, TECH_STACK §18).
- **WB-06**: Logical Tailwind properties only (`ps/pe/ms/me`). Physical `left/right` utilities are
  lint-blocked (WEB-14). `dir="rtl"` is set once in the root layout.
- **WB-07**: `date-fns-jalali` for display; ISO/UTC on the wire (WEB-15, INV-13).
- **WB-08**: Web has **no** queued mutations offline. A network failure shows an inline error and the
  user retries (WEB-11, ADR-0011). Offline reads are limited to the whitelisted Dexie set (WEB-10);
  adding a view to that set is an explicit, documented decision.
- **WB-09**: Charts and heavy tables are lazy-loaded/code-split; respect the bundle budgets in
  [13](../docs/architecture/13-web-architecture.md) (≤100 KB marketing, ≤250 KB app shell, gzipped).
- **WB-10**: Realtime (`@microsoft/signalr`) is an accelerant only — correctness never depends on the
  connection (WEB-13).

## Definition of done

No hand-written DTO · RTL-correct via logical properties · correct segment/rendering mode · bundle
budget check green · form schema derived from the spec.
