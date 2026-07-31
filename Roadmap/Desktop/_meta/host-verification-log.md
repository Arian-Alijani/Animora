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

## Notes

- Phase 00 item 18 asked whether launch sign-off should be a user Windows run or an
  `Avalonia.Headless` startup assertion. The user chose the real run and performed it, so phase 00
  signs off on the row above; no headless launch test was added (`UiTests` stays at zero tests until
  the phase that owns screen tests).
