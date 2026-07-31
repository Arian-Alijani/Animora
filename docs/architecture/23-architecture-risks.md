---
id: 23-risks
title: Architecture Risks
read_when: ["risk review", "postmortem"]
topics: [risk]
depends_on: [09-sync, 11-licensing]
stability: stable
---

## Contract

Decides: architecture-level risks distinct from the stack-level risks already in TECH_STACK §20 —
where this specific design is most likely to fail, the early-warning signal, the containment
strategy, and the pre-designed escape hatch. Does not repeat TECH_STACK's own risk table.

## Risks

| Risk | Early-warning signal | Containment | Escape hatch |
|---|---|---|---|
| Field-group LWW silently drops a clinically meaningful edit when two staff edit the same field group offline for days | Rising `conflict_log` volume for a specific entity/field group | `conflict_log` visible to support tooling ([18](18-observability-and-operations.md)); alert on abnormal conflict rate per tenant | Split the offending field into its own finer-grained field group (additive, non-breaking sync change, see [09](09-sync-architecture.md) recipe) |
| SQLCipher key loss on a device with no recent sync bricks that device's unsynced local-only work | Support ticket volume for "cannot open app" post OS reinstall | DPAPI + documented recovery-via-reauth path (DESK-06); server is source of truth post-sync | Reduce the acceptable offline window operationally (shorter recommended sync cadence) if losses recur; cannot fully eliminate without weakening at-rest encryption, which is not acceptable |
| A tenant's device count silently exceeds intended usage via shared logins, straining the single-VPS API under peak sync load | API p95 latency degradation correlated with sync batch volume, not user-request volume | Per-tenant rate limiting (ASP.NET Core Rate Limiting, TECH_STACK §6) already partitions by tenant | Introduce a soft per-plan sync-frequency cap enforced via `Entitlement` (additive to [11](11-licensing-and-entitlements.md), no new dependency) |
| Materialized view refresh jobs contend with OLTP writes during business hours on the 4 GB host | Report refresh job duration trending upward; OLTP p95 latency spikes during refresh windows | Refresh scheduling staggered off-peak (REP-04); statement timeout isolates report role | Move refresh cadence to lower frequency for lower-tier plans; defer to nightly for the heaviest views if the VPS remains the bottleneck |
| An offline-patched desktop binary bypasses UI-level license gating entirely (LIC-17 accepted limit) | Anomalous entitlement-mismatch patterns at server re-validation (many requests from a device whose cached token implies a lower plan than attempted actions) | Server re-validates every server-mediated action regardless of client claims (INV-16) — the patched binary cannot forge server-side effects | None needed beyond the existing server-side re-check; this risk is structurally bounded by design, not merely mitigated |
| Attachment tenant isolation relies on application-issued signed URLs rather than storage-native RLS (FILE-04), so a Files-module bug could leak a cross-tenant key | No signal until an incident/audit surfaces it — this is the weakest isolation layer in the system | Short-lived pre-signed URLs limit blast radius (FILE-07); permission check precedes every issuance | If storage-native tenant isolation becomes necessary, revisit ADR-0005's scope to add per-tenant bucket policies — requires MinIO operational changes, tracked as an open question, not implemented speculatively |
| Extension-allowlist-only malware policy (FILE-14) has no scanning engine in the stack | A malicious file matching an allowed extension (e.g., a crafted PDF) is never inspected beyond type sniffing | Allowlist restricts the practical attack surface; no execution path exists for uploaded files in this product (attachments are never executed, only rendered/downloaded) | If risk tolerance changes, this is the one place TECH_STACK §17's "propose one addition" process would apply (a scanning capability) — logged in `_meta/open-questions.md`, not preemptively added |
| Topology A -> B migration is proven config-only on paper but untested until actually exercised | No signal until the first real split attempt | The full env-var contract ([19](19-deployment-topology.md)) is the executable checklist for that first attempt | Run a staging-environment split rehearsal before a real production split is needed, using the same Compose/Caddy configs with different env values |
