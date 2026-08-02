# Desktop Roadmap Progress

Single source of truth for phase status (INV-18 applied to this roadmap). Do not duplicate status
inside a `PHASE.md` or `TODO.md` — those files hold plans and checkboxes; this file holds the
cross-phase index a new session reads first.

**Status values**: `not-started` -> `todo-ready` (Step 0 has generated `TODO.md`) -> `in-progress`
(at least one TODO item checked) -> `complete` (all TODO items checked + phase Completion Criteria
verified) -> `blocked` (needs a decision; see Notes column).

**Update rule**: change exactly one row per session action. `Done/Total` mirrors the checkbox count
in that phase's `TODO.md`; leave it `-/-` until `TODO.md` exists.

| # | Phase | Status | Done/Total | Notes |
|---|---|---|---|---|
| 00 | Solution Bootstrap | complete | 19/19 | `ci-desktop.sh` green (33 arch tests); app launch verified by the user on Windows — `_meta/host-verification-log.md` |
| 01 | Design System Foundation | complete | 37/37 | Tokens/styles/controls/formatters + headless and token-discipline tests in place; gallery deleted; RTL/Vazirmatn rendering verified by the user on Windows — `_meta/host-verification-log.md` |
| 02 | Shell & Navigation | complete | 29/29 | Shell, navigation, app state, the Reporting example screen and its data seam complete, with navigation/handler unit tests, the headless RTL shell smoke test and the README seam reference; Windows launch verified by the user — `_meta/host-verification-log.md` — and the two look-and-feel deviations it surfaced (top-bar `Divider` hairline, rail nav-pill box geometry) fixed against the reference |
| 03 | Shared Kernel & Contracts Seed | complete | 26/26 | `SharedKernel` (`TenantId`, `IEntity`/`ITenantScoped`/`ISyncedEntity`, `Error`, `Result`, `Money`, `Time/UtcClock`, Owner/Patient input interfaces + validators) and the `Contracts` seed (`AppointmentStatus`, `OwnerDto`, `PatientDto`) in place, with unit tests for every primitive and both validators, AT-07 implemented in `PersistenceBoundaryRules` and the new `ArchTests/SharedAssemblyRules` covering SH-01/SH-02/AT-02 and INV-05. Owner/Patient field rules are this phase's recorded default (`TODO.md` item 9), open to correction. Criteria verified by review only — `ci-desktop.sh` was not run in the implementing session |
| 04 | Identity & Auth Screens | complete | 41/41 | Shared Identity validation surfaces (staff, role, credentials) with the Iranian phone formats lifted into one internal helper (INV-02), `IdentityErrors` (`ERR-IDENTITY-001..008`), the Stage A read models and the staff/role/device/credential data seams, all five handlers and all five screens (login, staff list/form, role management, device list) wired through `AddIdentityModule()`, sign-in filling `CurrentUserState` via `StaffSignedInNotification` (resolving `CurrentUserState`'s `TODO(P1-04)`), the product owner's owner-admin username-namespacing rule (SEC-17) enforced in `SaveStaffMemberHandler` and recorded in `10-security-and-access-control.md`, plus the module's handler/validator unit tests and one headless RTL fact per route. Completion Criteria re-verified by inspection (routes registered rail-visible without shell coupling, five `AvaloniaFact` route tests, no `DbContext`/`HttpClient` or validator use in any ViewModel, sign-in seam marked `TODO(P2)`); the Windows run is the user's — `_meta/host-verification-log.md`. The recorded field-format defaults and the six decisions `TODO.md` encodes stay open to correction |
| 05 | Clients Module Screens | complete | 38/38 | Shared Owner/Patient validation surfaces extended to the user's answered field set (owner: address/city/notes/intake date; patient: breed, birth date, weight, sterilization, microchip id + implant date, color, temperament, housing type, diet, barcode, surgical history) with the new `AllowedHousingTypes` registry, `ClientsErrors` (`ERR-CLIENTS-001/002`), the Stage A read models and the split owner/patient read/write seams over one seeded Persian dataset, all seven handlers, and all five screens (owner list/form, patient list/form, medical-file summary) wired through `AddClientsModule()` — one patient-list route serving both the global and owner-scoped modes, the medical-file header read through the patient seam, and the Visits/Files links left as `TODO(P1-07)`/`TODO(P1-08)` markers. Owner/Patient/MedicalFile declared `MutableLWW` with field groups in `Animora.Contracts/Sync` (SYNC-R-01/03). Tests: the two save handlers, the patient-list handler, both extended validators, and six headless RTL facts over the five routes through the new shared `UiTests/ShellRouteHarness`. Completion Criteria re-verified by inspection (routes registered without shell coupling, virtualized `DataGrid` + keyset paging on both lists, no `DbContext`/`HttpClient` in any ViewModel); the Windows run is the user's — `_meta/host-verification-log.md`. Item 4's shared-mobile-number rule stays a documented default, open to correction |
| 06 | Scheduling Module Screens | todo-ready | 0/47 | Step 0 run 2026-08-02: 47 items covering the shared appointment/resource/catalog validation surfaces, `SchedulingErrors`, the module's read models, split read/write seams and Stage A fakes, the calendar/booking/reschedule/cancel/status handlers with the DOM-08 overlap check, five screens, `AddSchedulingModule()`, the sync-class declaration and the unit/headless tests. Items 2-6 are open questions for the user (booking fields, resource and catalog fields, how the booking dialog reaches a patient without a `Modules.Clients` reference, past/out-of-hours bookings, Jalali month and week start) |
| 07 | Visits Module Screens | not-started | -/- | |
| 08 | Files & Attachments Screens | not-started | -/- | |
| 09 | Finance Module Screens | not-started | -/- | |
| 10 | Reporting Module Screens | not-started | -/- | |
| 11 | Notifications Module Screens | not-started | -/- | |
| 12 | Licensing Status Screens | not-started | -/- | |
| 13 | UI Consolidation Gate | not-started | -/- | |
| 14 | Local Data Platform Foundation | not-started | -/- | |
| 15 | Identity Local Data | not-started | -/- | |
| 16 | Clients Local Data | not-started | -/- | |
| 17 | Scheduling Local Data | not-started | -/- | |
| 18 | Visits Local Data | not-started | -/- | |
| 19 | Files Local Data | not-started | -/- | |
| 20 | Finance Local Data | not-started | -/- | |
| 21 | Reporting Local Data | not-started | -/- | |
| 22 | Notifications Local Data | not-started | -/- | |
| 23 | Licensing Local Data | not-started | -/- | |
| 24 | Documents & Printing | not-started | -/- | |
| 25 | Barcode & Imaging | not-started | -/- | |
| 26 | Local Job Scheduler & Reminders | not-started | -/- | |
| 27 | Native Notifications & Update Seam | not-started | -/- | |
| 28 | Security Hardening & At-Rest Protection | not-started | -/- | |
| 29 | Performance & Startup Budget | not-started | -/- | |
| 30 | Backup & Restore UX | not-started | -/- | |
| 31 | Release Readiness | not-started | -/- | |

## Sequencing rules

- Phases 00-13 (Stage A) are strictly sequential: each module screens phase (04-12) depends on
  02-shell-and-navigation and 03-shared-kernel-primitives, but module screens phases are otherwise
  independent of each other's internals — still complete them in listed order to keep navigation
  registration and design-system usage consistent as they accumulate.
- Phase 13 (gate) cannot start until every phase 00-12 is `complete`.
- Phases 14 (data foundation) must be `complete` before any of 15-23 start. 15-23 each depend only
  on 14 and on their matching Stage A screens phase (e.g., 16-clients-local-data depends on
  05-clients-module-screens) — they may be done in a different order than listed if a later module's
  screens are finished first, but default to listed order.
- Phases 24-30 (Stage D) depend on 14-23 being complete (they attach to real local data) but are
  independent of each other; parallelizable across sessions if desired.
- Phase 31 cannot start until every prior phase is `complete`.
