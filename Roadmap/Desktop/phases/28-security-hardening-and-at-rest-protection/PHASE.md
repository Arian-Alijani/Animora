# Phase 28: Security Hardening & At-Rest Protection

## Goal

Harden the desktop's at-rest and in-process security posture: real DPAPI-backed `SecureStorage` for
refresh/license tokens (separate from the SQLCipher domain database), the hash-chained local audit
log mirror, SQLCipher key rotation support, and a security self-check pass against the full desktop
threat-model rows that apply to this platform.

## Expected Outcome

Refresh tokens and the cached license token live in DPAPI-protected storage, never in the SQLite
file or a plaintext config (SEC-07/13); a hash-chained local `audit_log` mirror records
module/entity/action/actor for every mutating command, detecting tamper via a broken chain link;
SQLCipher `rekey` rotation works from a settings action; every desktop-relevant threat-model row is
verified against real code, not assumed.

## Scope

- `Animora.Desktop.Infrastructure/SecureStorage`: DPAPI-backed storage for refresh token and cached
  license token, distinct from `Animora.Desktop.Data`'s SQLCipher-encrypted domain database
  (SEC-07/13) — a stolen SQLite file alone must not yield a live session.
- Hash-chained local `audit_log` mirror (`AppendOnly` sync class per the audit chain design):
  `prev_hash`, `hash = H(prev_hash, module, entity, action, actor, timestamp, payload_digest)` for
  every mutating command across all modules built so far (SEC-15) — this phase wires the shared
  audit-write path once, applied retroactively to existing handlers.
- SQLCipher key rotation (`rekey`) triggered manually from a settings screen, running inside a
  maintenance window with phase 26's scheduler paused (DESK-05).
- Security self-check pass: walk the desktop-relevant rows of the threat model
  ([10-security-and-access-control.md](../../../../docs/architecture/10-security-and-access-control.md)
  "Threat model" table) and confirm each desktop-side mitigation actually exists in code (stolen
  device -> SQLCipher+DPAPI+revocation seam, tampered audit trail -> hash chain, etc.).
- Out of scope: server-side mitigations from the same threat table (RLS, server revocation, P2),
  Authenticode code-signing (release/CI concern, phase 31).

## Key References

- [`docs/architecture/10-security-and-access-control.md`](../../../../docs/architecture/10-security-and-access-control.md) —
  SEC-07/13/14/15/16 desktop-at-rest rules, audit chain design, and the full threat-model table this
  phase's self-check walks.
- [`docs/architecture/08-desktop-local-data.md`](../../../../docs/architecture/08-desktop-local-data.md) —
  DESK-05 key rotation procedure (maintenance window, scheduler paused).
- [`docs/TECH_STACK.md`](../../../../docs/TECH_STACK.md) §12 "Security" — DPAPI, hash-chained audit
  log, SQLCipher rotation as the fixed mechanisms (no alternative security library to introduce).

## Dependencies

Requires phase 04 (Identity — session/token shape), phase 14 (local data platform), and phase 26
(scheduler to pause during rotation). Every module phase's handlers (04-23) are retroactively
touched once by this phase's shared audit-write wiring; this phase does not redesign those
handlers, only adds the audit-write call.

## Completion Criteria

- [ ] Refresh token and cached license token live in DPAPI storage, verified absent from the
      SQLite file and any config file.
- [ ] Every mutating command across every module writes a hash-chained `audit_log` row; a test
      breaks one row and proves chain verification detects it.
- [ ] SQLCipher `rekey` completes successfully from a settings action with the scheduler paused
      during the operation.
- [ ] Every desktop-relevant threat-model row has a verified, code-level mitigation (checklist
      completed, not assumed).

---

## Step 0

Not run yet. See [`../../_meta/session-protocol.md`](../../_meta/session-protocol.md).
