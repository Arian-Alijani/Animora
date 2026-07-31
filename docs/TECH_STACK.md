# TECH_STACK

Single source of truth for **technology choices only**. No architecture, no folder layout, no code design.

**Rules for AI agents**

1. Use only what is listed here. Do not introduce an alternative for something already chosen.
2. If a feature needs a capability not listed, first check `§17 Feature -> Stack Map`, then `§18 Do NOT Use`. Only if still uncovered, propose one addition and wait for approval.
3. `[core]` = build now. `[later]` = keep the seam, do not implement now. `[opt]` = add only if the feature is requested.
4. Anything in `§19 Out of Scope` must never be implemented.

---

## 1. Product

- Name: Animora — Veterinary Clinic Management
- Model: Multi-tenant SaaS, subscription purchased on the website, plan activates entitlements
- Clients: Windows desktop `[core]`, Web `[core]`, macOS `[later]`
- Desktop mode: offline-first, fully usable with zero internet, syncs later
- Web mode: online-first, read-only cache when offline
- Market: Iran. UI language: Persian, RTL, Jalali calendar. Currency: IRR / Toman
- Server target: one weak Linux VPS (2 vCPU / 4 GB) must run everything comfortably
- Hard constraint: single backend, single API contract, no per-client business logic

## 2. Version Pins

| Component | Version |
|---|---|
| .NET (desktop + backend) | 10 LTS |
| Avalonia UI | 11.x |
| EF Core | 10.x |
| PostgreSQL | 18 |
| Node.js | 22 LTS |
| Next.js | 15.x (App Router) |
| Tailwind CSS | 4.x |
| Ubuntu Server | 24.04 LTS |

Upgrade only major versions deliberately; never mix runtimes across desktop and backend.

## 3. Global Conventions

- IDs: `UUIDv7` everywhere (client-generatable, time-sortable, sync-safe). No DB identity/sequence PKs on synced tables.
- Time: store `timestamptz` UTC; convert at UI edge only. Jalali is a presentation concern.
- Sync ordering: Hybrid Logical Clock (HLC) per record field group.
- Money: `decimal(18,2)` in code, `numeric(18,2)` in PostgreSQL. Never `float`. Financial history is append-only ledger.
- Errors: RFC 9457 `application/problem+json` with a stable machine `code` string; clients switch on `code`, never on message text.
- API: OpenAPI-first. `Asp.Versioning` URL path versioning (`/api/v1`). Old desktop builds must keep working -> never break a shipped version.
- Enums: server is authoritative; shared numeric values, never localized strings on the wire.
- Config: environment variables only (12-factor). No hardcoded hosts, no machine-local paths in code.
- Deletes: soft delete + tombstones on all synced entities.

---

## 4. Desktop (Windows `[core]`, macOS `[later]`)

**Runtime & UI**

- .NET 10 LTS, Avalonia UI 11, Skia rendering, HarfBuzz text shaping (required for correct Persian shaping)
- MVVM with `CommunityToolkit.Mvvm`
- Host & DI: `Microsoft.Extensions.Hosting`, `Microsoft.Extensions.DependencyInjection`
- In-process messaging: `Mediator` (source generator, zero-reflection, AOT-friendly)
- Validation: `FluentValidation` (rules shared with backend)
- Mapping: `Mapperly` (source generator)

**Look & feel (beautiful UI requirement)**

- Theme base: `Semi.Avalonia` (modern, dense, RTL-tolerant) + project design tokens
- Icons: `Projektanker.Icons.Avalonia` (Material Design Icons set)
- Dialogs/overlays: `DialogHost.Avalonia`
- In-app toasts: Avalonia `WindowNotificationManager`
- Transitions: built-in Avalonia animations/transitions only (no third-party animation lib)
- Fonts: `Vazirmatn` embedded; `FlowDirection=RightToLeft` set at app root
- Data grids: `Avalonia.Controls.DataGrid` with virtualization mandatory on any list that can exceed 200 rows
- Charts: `LiveChartsCore.SkiaSharpView.Avalonia`

**Local data**

- SQLite with WAL mode + `SQLCipher` encryption (key in DPAPI / Keychain)
- Writes: EF Core 10. Hot reads/reports: `Dapper` (raw SQL)
- Search: SQLite `FTS5` (clients, animals, invoices, prescriptions)
- Local backup: scheduled `VACUUM INTO` encrypted snapshot + retention window, restore from UI
- Migrations: EF Core migrations, forward-only, must run offline on app start with pre-migration snapshot

**Documents & devices**

- PDF: `QuestPDF` (RTL + embedded Vazirmatn)
- Excel: `ClosedXML`
- Barcode/QR: `QRCoder` (generate) + `ZXing.Net` (scan) — pet ID cards, vaccine/drug labels, prescription lookup
- Images: `SkiaSharp` for resize/thumbnail of animal photos and attachments
- Printing: custom platform abstraction — Windows spooler (A4/A5 reports) + ESC/POS thermal (receipts, labels)

**Connectivity, jobs, updates**

- HTTP: `HttpClientFactory` + `Microsoft.Extensions.Http.Resilience` (Polly v8: retry, timeout, circuit breaker)
- API client: generated from OpenAPI with `Kiota` — never hand-written DTOs
- Wire format: `System.Text.Json` (source-generated) for API; `MessagePack` + Brotli for sync batches
- Local scheduler: `BackgroundService` + `PeriodicTimer` over a durable SQLite reminder/job table (survives restart, works offline). No external scheduler on desktop.
- Network state: OS connectivity detection + backend health probe; UI shows online/offline/syncing state, never blocks input
- Native notifications: Windows Toast via `DesktopNotifications.Avalonia` (macOS handled by same abstraction `[later]`)
- Auto update: `Velopack` (delta updates, staged rollout, resumable)
- Logging: `Serilog` rolling files, size-capped, PII-scrubbed
- Crash reporting: Sentry .NET SDK pointed at self-hosted endpoint (see `§13`)
- Secure storage: DPAPI (Windows), Apple Keychain (macOS)
- Calendar/locale: .NET `PersianCalendar` + custom Jalali formatters
- Publish: self-contained, `ReadyToRun`, single-file, trimming **disabled** (Avalonia reflection), Workstation GC

## 5. Web

- Next.js 15 App Router + TypeScript (strict), `output: "standalone"`
- Marketing/pricing/blog pages: SSG/ISR so the weak server serves them as static
- Styling: Tailwind CSS 4 with logical properties (`ps/pe/ms/me`) + `dir="rtl"`; no physical left/right utilities
- Components: `shadcn/ui` (Radix primitives)
- Icons: `lucide-react`. Toasts: `sonner`. Motion: `motion` (Framer Motion) for micro-interactions only
- Server state: `TanStack Query`. Client state: `Zustand`
- Tables: `TanStack Table` + `TanStack Virtual`
- Forms: `react-hook-form` + `zod` (schemas generated/derived from OpenAPI)
- Charts: Apache ECharts via `echarts-for-react` (lazy-loaded, tree-shaken)
- Dates: `date-fns-jalali`
- i18n: `next-intl` (fa default, en scaffold)
- API client: `openapi-typescript` types + `Orval` hooks (TanStack Query mode)
- Offline: `Dexie` over IndexedDB (read cache + queued mutations) + `Serwist` service worker (PWA)
- Web push: Web Push API with VAPID keys
- Realtime: `@microsoft/signalr`
- Tooling: `Biome` (lint + format), `Vitest` (unit), `Playwright` (E2E)

## 6. Backend

- Ubuntu 24.04 LTS, .NET 10 LTS, ASP.NET Core 10 **Minimal API**
- Shape: modular monolith, one process, modules communicate via `Mediator` in-process only (no shared DB tables across modules)
- Validation `FluentValidation`, mapping `Mapperly`, versioning `Asp.Versioning`
- OpenAPI: `Microsoft.AspNetCore.OpenApi` + `Scalar` UI (dev/staging only)
- Cache: `HybridCache` (`Microsoft.Extensions.Caching.Hybrid`) — L1 in-memory today, L2 Redis by config later. Never use `IMemoryCache` directly, or scaling out breaks.
- Realtime: `SignalR` (WebSocket; Redis backplane by config `[later]`)
- Background jobs: `Hangfire` + `Hangfire.PostgreSql` storage (no extra infra), dashboard behind admin auth
- Rate limiting: ASP.NET Core Rate Limiting, per-tenant + per-IP partitions
- Output cache + response compression (Brotli)
- Health checks: `/health/live`, `/health/ready` (DB, storage, jobs)
- Email: `MailKit` over SMTP (invoices, receipts, password reset)
- Templating for notifications/emails/reports text: `Scriban`
- Feature/plan gating: `Microsoft.FeatureManagement` driven by subscription entitlements
- Reverse proxy / TLS: `Caddy` (automatic Let's Encrypt, HTTP/2, HSTS, static asset serving)
- Perf posture on weak server: Server GC on, source-generated JSON, no reflection-heavy libs, `NpgsqlDataSource` pooling, paginate everything (hard max page size), no `SELECT *`

## 7. Data & Storage

- PostgreSQL 18 + `Npgsql`
- Writes/domain: EF Core 10. Reads/reports/exports: `Dapper`
- Tenant isolation: PostgreSQL Row-Level Security (RLS) with per-request tenant context — defense in depth, not a substitute for query filters
- Migrations: EF Core migrations, forward-only, applied by a dedicated startup/CI step (never auto-migrate on every boot in production)
- Files/attachments: **S3-compatible API from day one** via `AWSSDK.S3` — pointed at single-node `MinIO` on local disk now, any S3 endpoint later. Never write attachments to app-local paths.
- Uploads: chunked + resumable, server-side content-type/size validation, virus-free policy by extension allowlist
- Full-text search: PostgreSQL `tsvector` + GIN (no Elasticsearch)
- Reporting: SQL views / materialized views refreshed by Hangfire; no OLAP engine
- Sessions/state: stateless API only (JWT) — no in-process session state anywhere

## 8. Synchronization (offline-first, no data loss)

- Custom domain-level delta sync engine, REST + versioned OpenAPI contract, `MessagePack`+Brotli batches
- Identity: `UUIDv7`; ordering: HLC; conflict rule: field-level Last-Write-Wins, except money/ledger which is append-only and never overwritten
- Client -> server: Outbox pattern with durable queue, idempotency keys, exponential backoff, poison/dead-letter after N attempts surfaced in UI
- Server -> client: per-device sync cursor + changed-since pull, `SignalR` push as an accelerator only (correctness never depends on push)
- Attachments: metadata syncs first, binaries stream chunked/resumable in the background with priority queue
- Protocol: explicit `protocolVersion` negotiation; server supports N-1 desktop versions; client refuses sync and prompts update on mismatch
- Schema: sync gated by client schema version so an outdated build can never corrupt server data
- Safety: batch size + payload caps, server backpressure with retry-after, transactional apply per batch, resumable after crash
- Clock: HLC + server time offset; clock tamper detection affects license grace, never data ordering
- Tombstones with retention window; device re-seed (full snapshot pull) when cursor is too old
- Server-side conflict log for auditing and support

## 9. Subscription, Plans & Licensing

- Plans define entitlements: feature flags, seat/device count, record and storage limits, retention
- Purchase on website -> payment verified -> subscription record -> entitlements -> license token issued
- Payments: `ZarinPal` primary, `Zibal` fallback — server-side verify callback, idempotent, signed webhook/return handling, full payment audit trail
- No true auto-recurring billing (Iranian gateway limitation): fixed-term subscriptions + renewal reminder pipeline + dunning jobs in Hangfire
- Invoices/receipts: `QuestPDF`, emailed via MailKit, downloadable from panel
- Desktop license: `PASETO v4.public` token signed `Ed25519` (server holds private key), containing tenant, plan, entitlements, device fingerprint, expiry
- Enforcement: hardware fingerprint binding, periodic heartbeat validation, **offline grace period** (app keeps working fully offline; degrades to read-only only after grace expires), monotonic clock tamper detection
- Web: entitlements delivered as JWT claims, re-checked server-side on every request (client gating is UX only)
- Binaries: Authenticode code signing + lightweight obfuscation of licensing paths (deterrent only, never the sole control)

## 10. Notifications, Reminders & Alerts

Channels: in-app center (SignalR + persisted feed), Windows toast, SMS, email, web push.

- SMS: `SMS.ir` primary, `Kavenegar` failover, template/pattern based, delivery-status callback stored
- Email: MailKit + `Scriban` templates
- Web push: VAPID; Desktop: native toast + in-app toast
- Engine: Hangfire scheduled + recurring jobs; per-user channel preferences, quiet hours, per-tenant throttle, dedup key (no duplicate alert for the same event), retry with backoff, escalation, daily/weekly digest, full delivery log
- Offline desktop: reminders are computed and fired **locally** from the SQLite reminder table; server jobs handle only remote channels — an offline clinic still gets its alerts
- Standard alert sources: cheque due/returned, appointment reminder + no-show follow-up, vaccination/deworming due, treatment follow-up, lab result ready, inventory low stock, drug expiry, unpaid invoice/debt aging, subscription expiry, sync failure/backup failure, staff task due

## 11. Scheduling & Background Jobs

- Server: `Hangfire` (PostgreSQL storage) — recurring (reminders, aging, digests, refreshes, backups verification) and one-off/delayed jobs, all idempotent
- Desktop: `BackgroundService` + `PeriodicTimer` over durable SQLite job table (sync, reminders, local backup, update check)
- Rule: every job idempotent, cancellable, logged with correlation id, capped concurrency (weak server)

## 12. Security

- Transport: TLS 1.3, HSTS, certificate pinning on desktop with a documented pin-rotation path (two pins live at all times)
- AuthN: ASP.NET Core Identity, `Argon2id` hashing (`Isopoh.Cryptography.Argon2`), short-lived access token + rotating refresh token with reuse detection, device/session revocation list
- 2FA: TOTP `[opt]` for owner/admin roles
- AuthZ: RBAC + fine-grained permission claims; every endpoint permission-checked server-side
- Local: SQLCipher DB encryption, keys in DPAPI / Keychain, DB never readable by copying the file
- Audit: hash-chained append-only audit log (server + desktop), tamper-evident, synced
- Secrets: environment variables / Docker secrets; nothing in the repo. Signing keys never leave the server
- Input: FluentValidation on every command, parameterized SQL only, upload allowlist, security headers via Caddy

## 13. Observability

- Logs: `Serilog` structured JSON, rolling + size-capped, correlation/tenant id enriched, PII scrubbed
- Traces/metrics: `OpenTelemetry` with OTLP exporter, sampled (low overhead on weak server)
- Error tracking: Sentry SDKs pointing to **self-hosted `GlitchTip`** (Sentry-protocol compatible, ~200 MB RAM). Sentry SaaS is not reachable/allowed from the target market — treat the endpoint as configuration.
- Uptime + alerting: `Uptime Kuma` (light) hitting `/health/ready`, alerts to SMS/email
- Dashboards `[opt]`: Grafana + Prometheus only if a bigger server exists; otherwise logs + Uptime Kuma are enough

## 14. Backup & Disaster Recovery

- PostgreSQL: `pgBackRest` (full weekly + incremental daily + WAL archiving, point-in-time recovery)
- Secondary: nightly encrypted `pg_dump` (age/GPG) for portability
- Offsite: `rclone` to an S3-compatible object store in-region, retention 30 days, encrypted before leaving the server
- MinIO/attachments: `rclone` sync offsite; config and secrets backed up separately
- Verification: automated weekly restore-check job + integrity report; backup failure raises a notification
- Desktop: encrypted local snapshots + user-triggered restore; server copy is the source of truth after sync

## 15. DevOps

- Containers: `Docker Compose` — `caddy`, `api`, `web`, `postgres`, `minio`, `glitchtip [opt]`, `uptime-kuma [opt]`; each with memory limits and healthchecks
- CI/CD: GitHub Actions — build, test, OpenAPI contract check (breaking-change gate), client-code generation check, publish images, `Velopack` desktop release, notarization for macOS `[later]`
- Desktop distribution: Velopack feed served by Caddy from the same server (no third-party CDN dependency)
- Deploys: image tag pinning, migration step separate from app start, rollback = previous tag
- Topology A (now, one VPS): Caddy -> `web` + `api` + `postgres` + `minio` in one Compose file
- Topology B `[later]` (split): server 1 = Caddy + `web`; server 2 = `api` + `postgres` + `minio` (+ Redis). Migration is config-only: API base URL, S3 endpoint, `HybridCache` L2, SignalR backplane, DB connection string — no code change is permitted to be required for this move
- Registry: pull-through/local cache for base images (upstream registries are unreliable from the target region); vendor NuGet/npm caches in CI

## 16. Testing

- Unit: `xUnit` + `FluentAssertions` (+ `NSubstitute`)
- Integration: `Testcontainers` (PostgreSQL, MinIO) against real DB, RLS/tenant-isolation tests mandatory
- Desktop UI: `Avalonia.Headless`
- Web: `Vitest` + `Playwright`
- Contract: OpenAPI snapshot diff to block accidental breaking changes for shipped desktop versions
- Architecture: `NetArchTest` to enforce module boundaries — this is what keeps the future monolith-to-split migration cheap
- Sync suite (mandatory): conflict resolution, duplicate/replayed requests, crash mid-batch, clock skew, partial sync, tombstone/re-seed, protocol-version mismatch, large attachment resume, multi-device convergence
- Load: `k6` or `NBomber` smoke profile sized for the 2 vCPU / 4 GB target

---

## 17. Feature -> Stack Map

Use this before adding anything. Every new feature must be assembled from these pieces.

| Need | Desktop | Web | Backend |
|---|---|---|---|
| CRUD screen | Avalonia + Mediator + EF Core (SQLite) | shadcn/ui + react-hook-form + zod + TanStack Query | Minimal API + FluentValidation + EF Core |
| Big list / grid | DataGrid (virtualized) + Dapper | TanStack Table + Virtual | keyset pagination |
| Chart / KPI | LiveChartsCore | ECharts | Dapper query or materialized view |
| Report / print | QuestPDF + print abstraction | server-generated PDF download | QuestPDF |
| Data export | ClosedXML | download link | ClosedXML |
| Reminder / due date | SQLite reminder table + local timer | — | Hangfire recurring |
| Alert delivery | toast + in-app feed | web push + sonner + SignalR | notification engine (SMS/email/push) |
| Attachment (photo, lab, x-ray) | SkiaSharp + local file cache | direct upload | S3 (MinIO) chunked upload |
| Label / ID card | QRCoder + ESC/POS | printable page | — |
| Search | SQLite FTS5 | server search endpoint | tsvector + GIN |
| New synced entity | UUIDv7 + HLC + tombstone + outbox | — | sync contract + protocol version bump if breaking |
| Money movement | append-only ledger rows | read-only view | ledger + audit log |
| Paid/plan-limited feature | license entitlement check | entitlement claim (UX) + server check | FeatureManagement + entitlements |
| Long/heavy operation | local job table | job status polling via TanStack Query | Hangfire job + progress endpoint |
| Realtime update | SignalR client | SignalR client | SignalR hub |

**Checklist for every new feature:** OpenAPI first -> regenerate clients (Kiota / Orval) -> permission claim -> validation rules -> sync rules if the entity is synced -> plan entitlement if paid -> notification/reminder if time-based -> tests (unit + integration + sync case).

## 18. Do NOT Use

Rejected alternatives — do not propose, do not add, do not migrate to.

- Desktop: WPF, WinForms, MAUI, Electron, Tauri, Uno, WinUI (Avalonia is the only UI stack)
- Mediator/mapping: MediatR, AutoMapper (commercial licensing) -> use `Mediator` + `Mapperly`
- Data: MongoDB, Realm, LiteDB, Prisma, Drizzle, Elasticsearch; also banned: Dapper for writes, EF Core on hot report paths, raw ADO.NET inside features
- Infra: Kubernetes, RabbitMQ, Kafka, MassTransit, Elasticsearch/ELK, Consul, microservices, serverless
- Redis as a *required* dependency (allowed only as HybridCache L2 / SignalR backplane after a split)
- Web: Redux, MobX, MUI, Chakra, Bootstrap, Ant Design, moment.js, axios (use generated client + fetch), Pages Router, styled-components/CSS-in-JS
- Auth: rolling your own crypto/JWT, Firebase Auth, Auth0, Supabase, Clerk, storing tokens in `localStorage`
- Cloud/SaaS that is unreliable or blocked in the target market: Sentry SaaS, Firebase, Vercel/Netlify hosting for the app, AWS/GCP managed DB, third-party CDNs for app assets
- Anti-patterns: `IMemoryCache` directly, in-process session state, local-disk attachment storage, auto-increment PKs on synced tables, DateTime.Now (use UTC + explicit conversion), business logic duplicated per client, hand-written API DTOs, hard deletes on synced entities

## 19. Out of Scope

Never implement unless explicitly re-scoped:

- **Tax system integration (Samane Moadian / e-invoicing) — explicitly excluded**
- DICOM/PACS imaging pipeline, telemedicine video calls, native mobile apps (iOS/Android), ML/AI diagnostics, multi-currency, marketplace/e-commerce storefront, third-party clinic API/public API program

## 20. Known Risks & Mitigations

| Risk | Mitigation |
|---|---|
| Avalonia RTL gaps in some controls/third-party themes | Persian RTL smoke test per screen in `Avalonia.Headless`; keep a small set of RTL-fixed style overrides; prefer built-in controls |
| Weak server saturated by Next.js + API + Postgres + MinIO | SSG/ISR for public pages, Next `standalone`, container memory limits, aggressive HybridCache + output cache, capped Hangfire workers; Postgres tuned for 4 GB |
| Sync bugs = data loss (highest business risk) | Mandatory sync test suite, append-only ledger, tombstones, dead-letter visibility, conflict log, staged Velopack rollout |
| Old desktop builds in the field | URL API versioning + protocol negotiation + N-1 support + forced-update prompt |
| Splitting web and API onto separate servers later | HybridCache (Redis L2 by config), SignalR Redis backplane by config, S3 storage from day one, env-var config, stateless API, `NetArchTest` module boundaries |
| Payment double-charge / lost callback | Idempotency keys, server-side verify, reconciliation job, full payment audit trail |
| SMS provider outage or pattern rejection | SMS.ir primary + Kavenegar failover, delivery-status tracking, retry with backoff, in-app fallback |
| Registry/package/CDN access from the target region | Local registry cache, vendored NuGet/npm caches in CI, self-hosted Velopack feed and GlitchTip |
| Certificate pinning bricking clients on rotation | Always ship two valid pins, rotate one at a time, remote kill-switch for pinning |
| Encrypted local DB key loss (SQLCipher) | Key in DPAPI/Keychain + recovery path via server after re-auth; server is source of truth post-sync |
