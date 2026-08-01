# Phase 04: Identity & Auth Screens

## Goal

Build every screen the Identity module owns: login, staff (user) management, role/permission
management — fully click-through against the Stage A data seam (phase 02 pattern), inside
`Animora.Desktop.Modules.Identity`.

## Expected Outcome

A staff member can (in the demo/fake-data sense) log in, see a staff list, create/edit a staff
member, assign a role, and manage the role/permission-claim catalog for the tenant — all through
real `Mediator` handlers in `Modules.Identity/Handlers`, bound to `Modules.Identity`'s own
data-seam interface (fake now, real in phase 15).

## Scope

- Login screen (credential form only — no real token/network call yet, DT-12: mark the seam with
  `// TODO(P2): ...` where a server auth call will eventually go).
- Staff list (virtualized `DataGrid` if it can exceed 200 rows, DT-08) + staff create/edit screen.
- Role management screen: create/edit a role, assign permission claims from the catalog
  ([10-security-and-access-control.md](../../../../docs/architecture/10-security-and-access-control.md)
  permission table).
- Staff create/edit enforces the owner-admin username namespacing rule for every other account
  (SEC-17 in the same doc): a subordinate account's username is checked against, never invented by,
  the staff save handler.
- Device list (read-only view of the current tenant's registered devices — real seat/limit
  enforcement is server-side per LIC-08 and out of scope here; this is the Identity-side listing
  screen only).
- Every screen follows the "add a desktop screen" recipe (route registration, Mediator dispatch,
  RTL/token compliance, headless smoke test) — see Extensibility Playbook.
- Out of scope: real authentication/token issuance (P2 seam), real permission enforcement beyond
  UI-level gating (server-side enforcement doesn't exist until P2, per INV-09/DT-12), local SQLite
  persistence (phase 15).

## Key References

- [`docs/architecture/20-extensibility-playbook.md`](../../../../docs/architecture/20-extensibility-playbook.md) —
  "add a desktop screen" recipe, followed verbatim per screen.
- [`docs/architecture/10-security-and-access-control.md`](../../../../docs/architecture/10-security-and-access-control.md) —
  RBAC model, permission claim catalog grouped by module (what the role screen must render).
- [`docs/architecture/04-module-catalog.md`](../../../../docs/architecture/04-module-catalog.md) —
  Identity's owned entities and `IIdentityContract` shape (User, Role, PermissionClaim,
  RefreshToken, Device).

## Dependencies

Requires phases 02 (shell/navigation/data-seam pattern) and 03 (shared validators/DTOs for User/
Role). Feeds phase 15 (Identity Local Data) and every later module phase (all screens assume a
logged-in staff context from here).

## Completion Criteria

- [x] Login, staff list, staff create/edit, role management, and device list screens exist and are
      navigable from the shell.
- [x] Every screen passes an `Avalonia.Headless` RTL smoke test.
- [x] No ViewModel references `DbContext`/`HttpClient` directly (DT-02).
- [x] Validation runs in the shared validator via the handler, not in the ViewModel (DT-03).
- [x] Server-bound seams (login network call) are marked `// TODO(P2): ...` per DT-12, not stubbed
      with a fake API client.

---

## Step 0

Run. See [`TODO.md`](TODO.md) and [`../../_meta/session-protocol.md`](../../_meta/session-protocol.md).
