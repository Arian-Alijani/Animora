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
| 02 | Shell & Navigation | in-progress | 27/29 | Shell, navigation, app state, the Reporting example screen and its data seam complete, with navigation/handler unit tests, the headless RTL shell smoke test and the README seam reference; only the Windows launch check + criteria sign-off (28-29) remain |
| 03 | Shared Kernel & Contracts Seed | not-started | -/- | |
| 04 | Identity & Auth Screens | not-started | -/- | |
| 05 | Clients Module Screens | not-started | -/- | |
| 06 | Scheduling Module Screens | not-started | -/- | |
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
