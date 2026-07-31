# Phase 01: Design System Foundation

## Goal

Build `Animora.Desktop.UI` as a real, leaf design-system assembly: theme tokens, `Semi.Avalonia`
integration, RTL/Vazirmatn font setup, shared controls/converters, and `ViewModelBase` — everything
every module screen will consume, with nothing in it that couples back to a module or the shell.

## Expected Outcome

A themed, RTL, Persian-fonted Avalonia resource system exists and can be previewed (e.g., via a
throwaway sample window deleted before merge, or `Avalonia.Headless` snapshot) independent of any
real screen. Any future module view can reference tokens/controls/converters from this project and
nothing else for look-and-feel.

## Scope

- Design tokens (colors, spacing, typography scale) as a shared resource dictionary
  (DESK-ARCH-12).
- `Semi.Avalonia` theme wired at the resource level; `Vazirmatn` embedded and set as default font;
  `Projektanker.Icons.Avalonia` (Material Design set) wired (DESK-ARCH-13).
- `FlowDirection=RightToLeft` established as an app-root-level concern here (the token/resource
  layer that phase 02's shell will apply once) — DT-06/DESK-ARCH-06.
- Shared value converters (Jalali date formatting, number/currency formatting per CONV-05/CONV-07)
  living in `Converters/` — implementation only; wiring at real binding edges happens per-screen in
  later phases.
- `ViewModelBase` (`Mvvm/`) built on `CommunityToolkit.Mvvm`.
- Shared dialog/toast/overlay abstractions (`DialogHost.Avalonia`, `WindowNotificationManager`)
  exposed as services module ViewModels can inject, without the UI project depending on any module.
- Out of scope: navigation/shell composition (phase 02), any module-specific screen.

## Key References

- [`docs/architecture/12-desktop-architecture.md`](../../../../docs/architecture/12-desktop-architecture.md) —
  theming/RTL/localization architecture section (DESK-ARCH-12..14).
- [`docs/TECH_STACK.md`](../../../../docs/TECH_STACK.md) §4 "Look & feel" — exact libraries
  (Semi.Avalonia, Projektanker.Icons.Avalonia, DialogHost.Avalonia, Vazirmatn, DataGrid
  virtualization requirement).
- [`docs/architecture/03-solution-structure.md`](../../../../docs/architecture/03-solution-structure.md) —
  DIR-07/AT-08 (`Animora.Desktop.UI` must not depend on any other `Desktop.*` project).
- [`../../_meta/design-reference.md`](../../_meta/design-reference.md) — the token *values* (palette,
  type scale, geometry, elevation, component anatomy) extracted from the product owner's reference
  screens; this phase turns that file into the resource dictionaries.

## Dependencies

Requires phase 00 (buildable solution). Every module screens phase (04-12) and phase 02 depend on
this phase.

## Completion Criteria

- [x] `Animora.Desktop.UI` builds with zero references to any `Desktop.Modules.*`, `Desktop.App`,
      `Desktop.Data`, `Desktop.Sync`, or `Desktop.Infrastructure` project (AT-08 satisfied).
- [x] RTL + Vazirmatn + theme tokens render correctly in at least one headless smoke check.
- [x] Jalali and money/number converters exist and have unit tests independent of any screen.
- [x] `ViewModelBase` and dialog/toast abstractions are ready for module ViewModels to consume.
- [x] No hardcoded colors/spacing/fonts exist anywhere in this project (they define the tokens,
      they don't bypass them).

### Verification evidence (2026-07-31)

- AT-08: enforced by `ArchTests/AssemblyBoundaryRules` (`DesignSystemProjectReferencesNoOtherDesktopProject`,
  `DesignSystemTypesDependOnNoOtherDesktopNamespace`), not by inspection.
- RTL/Vazirmatn/token rendering: `UiTests/Theme/AnimoraThemeSmokeTests` (flow direction, effective
  default family, `StatCard`/`StatusChip` token brushes) plus the user's real Windows run of the
  item-35 gallery — [`../../_meta/host-verification-log.md`](../../_meta/host-verification-log.md).
- Converters/formatters: `UnitTests/Converters` + `UnitTests/Localization`, screen-independent.
- `Mvvm/ViewModelBase`, `Services/IDialogService`, `Services/IToastService` exist and are wired for
  consumers through `Services/ServiceCollectionExtensions.AddDesktopUi()`.
- No hardcoded values: enforced by `ArchTests/TokenDisciplineRules.XamlOutsideTokensHasNoLiteralColorFontOrRadiusValues`.
- The throwaway gallery (item 35) was deleted and `App` shows `Shell/ShellWindow` again; the theme
  include stays at the application root, which is where phase 02 keeps it (DT-06).

---

## Step 0

Run on 2026-07-31 -> [`TODO.md`](TODO.md) (37 items). The reference screens supplied with the request
were sampled into [`../../_meta/design-reference.md`](../../_meta/design-reference.md) first, so the
TODO items name token *groups* and cite that file for their values instead of carrying hex codes.
