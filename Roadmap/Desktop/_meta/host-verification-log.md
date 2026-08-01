# Host Verification Log

Results of the checks an agent session cannot run: everything that needs a real Windows host, a
display, a printer, a scanner, native notifications, an installer, or real-hardware timing. The
procedure and the placement rule live in [`session-protocol.md`](session-protocol.md)
"Host-dependent verification"; this file is only the evidence trail a later session can trust
instead of re-asking.

**Update rule**: append one row per verified check — never rewrite history. A row is added only
after the user reports the outcome, and it must exist before the phase's `PROGRESS.md` status can
become `complete`.

| Date | Phase | TODO item | What was checked | Environment | Result | Reported by |
|---|---|---|---|---|---|---|
| 2026-07-31 | 00 Solution Bootstrap | 18 | `Animora.Desktop.App` starts and shows the empty shell window | User's Windows machine (real desktop session) | Pass — app launched, blank window shown | User |
| 2026-07-31 | 01 Design System Foundation | 36 | Token gallery renders RTL with Vazirmatn, token colors, shadows and controls as in the reference screens | User's Windows machine (real desktop session) | Pass — gallery launched with zero warnings and zero errors, rendering matched the reference | User |
| 2026-08-01 | 02 Shell & Navigation | 28 | `dotnet run --project desktop/src/Animora.Desktop.App` shows the RTL shell — rail, top bar, status indicator, Home route rendered, no wait on background init | User's Windows machine (real desktop session) | Pass — shell launched correctly; two look-and-feel deviations from the reference reported and fixed in the same session (top-bar/content `Divider` hairline missing; nav pill box not at the reference's rail-inner-width x `NavItemHeight` geometry) | User |
| 2026-08-01 | 04 Identity & Auth Screens | 40 | Sign in on the login screen, then reach the staff list, the staff form, role management and the device list from the rail (`dotnet run --project desktop/src/Animora.Desktop.App`) | User's Windows machine (real desktop session) | Pass — sign-in succeeded and all five Identity routes were reached and rendered; user also reported the desktop test run green on the same machine, with no errors to fix | User |

## Notes

- Phase 00 item 18 asked whether launch sign-off should be a user Windows run or an
  `Avalonia.Headless` startup assertion. The user chose the real run and performed it, so phase 00
  signs off on the row above; no headless launch test was added (`UiTests` stays at zero tests until
  the phase that owns screen tests).
