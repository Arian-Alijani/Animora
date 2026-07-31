# Phase 23: Licensing Local Data

## Goal

Swap phase 12's Licensing Stage A fake data-seam implementation for real local persistence: EF Core
entity/configuration/migration for the cached entitlement snapshot and heartbeat state, wired
behind `Modules.Licensing`'s own interface.

## Expected Outcome

The plan/entitlement display, device/seat list, and offline-grace status screens from phase 12 now
read a real locally-cached entitlement snapshot and a real persisted heartbeat state-machine value
(`Fresh`/`OfflineGrace`/`ReadOnlyDegraded`) that survives an app restart, instead of phase 12's
in-memory fake transitions.

## Scope

- EF Core entity configuration for a local `EntitlementSnapshot` cache (`ReferenceOnly` per module
  catalog: "server authoritative; token cached client-side") holding the last-known plan/
  entitlement values and the heartbeat state machine's current state + timestamps.
- EF Core migration adding this table (DT-11).
- Persist the `Fresh`/`OfflineGrace`/`ReadOnlyDegraded` state across restarts (LIC-10..12) — reading
  it at startup to restore the shared entitlement-status service's state (the service itself, from
  phase 12, does not change shape).
- Local device/seat list read from this cached snapshot (still read-only, no real enforcement here
  per LIC-13/16 — enforcement is server-side, P2).
- Rebind `Modules.Licensing`'s data-seam interface to this real implementation; delete the Stage A
  fake.
- Out of scope: real PASETO token verification (P2), real heartbeat network call (phase 26 wires
  the local job that would eventually call it; until P2 exists there is no server to call, so this
  phase's persisted state is manually/test-transitionable only, per DT-12).

## Key References

- [`docs/architecture/11-licensing-and-entitlements.md`](../../../../docs/architecture/11-licensing-and-entitlements.md) —
  the heartbeat/offline-grace state machine (LIC-10..12) and degradation table this cache must
  represent exactly, and LIC-17's explicit anti-tamper limits (do not over-build local enforcement).
- [`docs/architecture/08-desktop-local-data.md`](../../../../docs/architecture/08-desktop-local-data.md) —
  local table/migration conventions this entity follows (DT-11).
- [`docs/architecture/adr/ADR-0006-entitlement-enforcement-split.md`](../../../../docs/architecture/adr/ADR-0006-entitlement-enforcement-split.md) —
  keeps this phase's scope honest: persistence only, never enforcement.

## Dependencies

Requires phase 14 and phase 12 (Licensing screens + data-seam interface). Feeds phase 26/27 (real
heartbeat job wiring reads/writes this table once a network call exists, P2).

## Completion Criteria

- [ ] `EntitlementSnapshot`/heartbeat-state EF Core entity/configuration/migration exists.
- [ ] The `Fresh`/`OfflineGrace`/`ReadOnlyDegraded` state persists across an app restart.
- [ ] Plan/entitlement display and device/seat list screens from phase 12 read real cached values.
- [ ] No code in this phase claims to be the entitlement enforcement authority (INV-16 respected;
      still UI-only gating).
- [ ] Phase 12's `Avalonia.Headless` smoke tests still pass unchanged against real data.

---

## Step 0

Not run yet. See [`../../_meta/session-protocol.md`](../../_meta/session-protocol.md).
