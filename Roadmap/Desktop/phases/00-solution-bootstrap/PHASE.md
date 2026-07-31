# Phase 00: Solution Bootstrap

## Goal

Turn the existing empty `desktop/` project skeleton into a buildable, runnable, testable .NET
solution: real `.csproj`/`.sln` files in every already-scaffolded folder, an Avalonia app that opens
a blank window, and the three test projects wired to run (even with zero tests yet).

## Expected Outcome

`dotnet build` and `dotnet test` succeed from `desktop/`; the Avalonia app launches to an empty
window on the target runtime; every project in the existing folder map
([`desktop/AGENTS.md`](../../../../desktop/AGENTS.md) project table) exists as a real project with
correct references in the allowed dependency directions; CI-shaped local scripts (build+test) run
green with nothing to actually test yet.

## Scope

- Create `.csproj` for every project already scaffolded under `desktop/src/*` and `desktop/tests/*`
  (folders with `.gitkeep` files already exist — do not invent new top-level projects, AG-13).
- Wire project references per the allowed dependency graph (DIR-01 through DIR-07, AT-01..AT-09).
- Add `Animora.Desktop.sln` (or confirm/extend a root solution) referencing every project.
- Bootstrap `Animora.Desktop.App`: `Program.cs`, `Microsoft.Extensions.Hosting` generic host, a
  minimal Avalonia `App.axaml`/`App.axaml.cs` that shows one empty window — no shell/navigation yet
  (that is phase 02).
- Add the initial `NetArchTest` project skeleton in `Animora.Desktop.ArchTests` with the rules from
  the solution-structure doc's desktop table, even though most will vacuously pass until modules
  exist.
- Consume `Directory.Build.props`/`Directory.Packages.props` from repo root (already exist) rather
  than redefining TFM/nullable/package versions per project.
- Out of scope: any screen, any theme, any data access, any module content — those are later phases.

## Key References

- [`docs/architecture/03-solution-structure.md`](../../../../docs/architecture/03-solution-structure.md) —
  the exact project list, dependency directions, and NetArchTest rule IDs to scaffold.
- [`desktop/AGENTS.md`](../../../../desktop/AGENTS.md) — desktop project map and DT-01/DT-05/DT-12.
- [`docs/TECH_STACK.md`](../../../../docs/TECH_STACK.md) §2 (version pins) and §4 (desktop runtime/UI
  stack) — exact package choices for host/DI/Avalonia/Mediator/etc.

## Dependencies

None — this is the first phase. Everything else depends on this phase being `complete`.

## Completion Criteria

- [x] Every project listed in `desktop/AGENTS.md`'s project table exists as a real `.csproj`.
- [x] `dotnet build` succeeds for the whole `desktop/` solution.
- [x] `dotnet test` runs (0 tests is fine) for all three test projects without errors.
- [x] The Avalonia app launches to an empty window.
- [x] No project references another in a direction forbidden by DIR-01..DIR-07.
- [x] No business/module code, screens, or data access exist yet (scope stayed bounded).

### Verification evidence (2026-07-31)

- `tools/scripts/ci-desktop.sh` on .NET SDK 10.0.302: restore + build (Release, 0 warnings,
  0 errors) + test green — 33 `ArchTests` passing, `UnitTests`/`UiTests` discovered with 0 tests as
  designed.
- 17 desktop `.csproj` + the 2 `shared/dotnet/*` projects present, all in `Animora.Desktop.sln`;
  dependency directions enforced by the AT-08/AT-09 rules inside those 33 tests, not by inspection.
- Only `Startup/Program.cs`, `App.axaml(.cs)` and `Shell/ShellWindow.axaml(.cs)` contain code under
  `desktop/src` — no module, screen, or data-access code.
- Launch criterion: user-run on the user's own Windows machine (agent environment has no Windows
  host/display) — see [`../../_meta/host-verification-log.md`](../../_meta/host-verification-log.md).

---

## Step 0

Done — see [`TODO.md`](TODO.md) for the generated item list and
[`../../_meta/session-protocol.md`](../../_meta/session-protocol.md) for the resume procedure.
