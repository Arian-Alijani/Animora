# Session Protocol

How any Claude Code session — the first one and the hundredth one — resumes and advances this
roadmap. Read this once per session, before opening a phase.

## Resume procedure (every session, in order)

1. Read `Roadmap/Desktop/PROGRESS.md`. Find the first row that is not `complete`.
2. Open `Roadmap/Desktop/phases/<that-phase>/PHASE.md`.
3. Check whether `TODO.md` exists in the same folder.
   - **No `TODO.md`** -> run Step 0 (below) for this phase, then stop and let the user/next session
     start on item 1 of the new list (a session may do both in one sitting if there is budget left).
   - **`TODO.md` exists** -> open it, find the first unchecked `- [ ]` item, implement it.
4. After finishing one or more items: check them off in `TODO.md`, update the `Done/Total` and
   `Status` columns for this phase in `PROGRESS.md`, commit (repo-root AGENTS.md §8 commit
   convention, scope = the phase's module or `desktop-<area>`).
5. When every item in a phase's `TODO.md` is checked, re-verify the phase's `PHASE.md` Completion
   Criteria explicitly (do not assume checked TODOs equal criteria met), then set that phase's
   `PROGRESS.md` status to `complete`.
6. If a phase cannot proceed (missing decision, conflicts with an invariant), set its status to
   `blocked` in `PROGRESS.md` with a one-line Note, and follow AG-10 (stop and say so; do not
   invent a workaround) or record the gap in `docs/architecture/_meta/open-questions.md` per AG-07.

## Step 0 — generating a phase's TODO list

Step 0 is not implemented by any phase's own content; it is a procedure this file defines once and
every phase reuses. When a session runs Step 0 for phase `NN-name`:

1. Read `phases/NN-name/PHASE.md` in full (Goal, Expected Outcome, Scope, Key References,
   Completion Criteria).
2. Read every doc listed in that phase's **Key References** section — nothing more (mirrors AG-07's
   2-4 files-per-task discipline; a phase's Key References list is already curated to that budget).
3. Read `desktop/AGENTS.md` if not already fresh in context (it is short and applies to every
   phase).
4. Cross-check the phase's Scope against `docs/architecture/20-extensibility-playbook.md`'s matching
   recipe(s) if the phase implements a recipe (e.g., "add a desktop screen") — the recipe's ordered
   steps are the backbone of the generated TODO items for that unit of work.
5. Produce `phases/NN-name/TODO.md` following exactly the structure in
   [`todo-format.md`](todo-format.md). Every item must be small, independent enough to complete in
   one focused session-slice, and cite the rule IDs it must satisfy rather than re-explaining them.
6. Update `PROGRESS.md`: status `todo-ready`, `Done/Total` = `0/<n>`.
7. Stop. Do not start implementing item 1 as part of Step 0 itself unless the user explicitly asked
   for both in one turn — Step 0's job is planning, not coding.

## Rules Step 0 must follow when writing TODO items

- One TODO = one file, one screen, one entity, one handler, or one narrowly-scoped concern — never
  "implement the whole module."
- Order TODO items so each one leaves the build/tests green; do not sequence a TODO that requires a
  later TODO's output.
- Every TODO that touches a synced entity, a permission, a report, or a notification type cites the
  matching Extensibility Playbook recipe step it corresponds to, instead of restating the recipe.
- Never invent a business rule not already specified by the architecture corpus or by the user; if
  the phase's scope is under-specified, add a TODO item of the form "Ask the user: <specific
  question>" rather than guessing (AG-02).
- Keep the whole `TODO.md` short enough to be read in one context load — this is why phases are
  kept small (roadmap-level rule, see `README.md` "Phase design").

## Host-dependent verification (user-run, always last)

Some criteria cannot be observed from an agent session: the agent environment has no Windows host,
no display, no printer or scanner, no native notification centre, and no real data volume. Those
checks belong to the user, run on the user's own machine, and they are scheduled at the **end** of a
phase.

- **The agent never claims a host-dependent result.** Agent-side verification stops at
  restore/build/test, static/`NetArchTest` evidence, and file inspection, and says exactly which
  criterion it could not observe (AG-19).
- **Placement**: a host-dependent item is written at the end of the phase's `TODO.md`, immediately
  before the final completion-criteria item, so that no earlier item is ever blocked waiting on it
  (see [`todo-format.md`](todo-format.md) "Host-dependent items").
- **Wording**: the item starts with `User-run on Windows:` and names one observable outcome plus the
  smallest command that produces it (e.g. `dotnet run --project desktop/src/Animora.Desktop.App`) —
  never a test matrix, never a script the user has to assemble.
- **Automated proxies are optional, not a substitute**: a headless/`Avalonia.Headless` assertion may
  be added when it is cheap and genuinely guards a regression, but when the user has performed the
  real run, the phase signs off on that real run (AG-02 — the user decides what "verified" means for
  their environment).
- **Record, then sign off**: the user's result is logged as one row in
  [`host-verification-log.md`](host-verification-log.md); only then may the TODO item be checked and
  the phase's `PROGRESS.md` status move to `complete`.
- **A pending host check is not `blocked`**: leave the phase `in-progress` with a Note naming the
  awaited run; `blocked` stays reserved for missing decisions and invariant conflicts.

Phases whose sign-off is expected to need this: 00 (app launch), 01/02 (RTL + shell rendering), 24
(printing), 25 (scanner hardware), 27 (native notifications, update), 29 (startup budget on real
hardware), 30/31 (backup/restore, installer).

## Why status lives only in PROGRESS.md

`TODO.md` checkboxes are the granular truth; `PROGRESS.md` is the cheap-to-scan rollup. A session
should never need to open more than one phase's `TODO.md` to know where the project stands overall
— that summary is `PROGRESS.md`'s only job (INV-18 applied to roadmap docs, not just architecture
docs).
