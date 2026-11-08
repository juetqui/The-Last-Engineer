---
description: "Build a new feature end to end, following the project's context, rules, practices and a proportional design."
argument-hint: "<short description of the feature>"
disable-model-invocation: true
---

# /new-feature

Implement the feature described in `$ARGUMENTS`, following this project's conventions.

## 1. Understand
- Restate the feature in one or two sentences and list the acceptance criteria (what the player or
  designer can do when it's done).
- Ask only what you genuinely can't answer from `doc/context/`, `doc/` and the code.

## 2. Orient
- Read the relevant `doc/context/` files (`project-map.md` first) and the `.trail/learnings/`
  that touch the area; `trail ask "<feature or system>"` to find the code.
- Read `.claude/practices/practices.md`. Path-scoped rules load as you open files; `rules.md` is
  already loaded.
- Load the skill for the area: `unity-gameplay-development`, `unity-editor-tooling`,
  `unity-ui-development`, `unity-asset-pipeline`.
- Find existing systems, components and patterns to reuse before writing anything new.

## 3. Plan — in plan mode
- Enter plan mode (`EnterPlanMode`); no edits until the plan is approved.
- Apply `design-pattern-selection`. **The plan always contains the design line:**
  `Design: <pattern> because <problem>; alternative considered: <X>` or
  `Design: no pattern — <reason>`, plus one line on SOLID (each new type's responsibility and what
  it depends on).
- Outline the change in order: runtime → editor → UI. Name the files, the serialized fields added or
  renamed, and what the user must wire in the Editor (scenes, prefabs, Inspector references).
- Smallest change that meets the criteria. New packages need a justification (`rules.md`).
- Present it with `ExitPlanMode` and wait for approval.

## 4. Build
- Runtime first, then editor tooling, then UI; match the existing naming and structure.
- New logic lives in plain C# classes with EditMode tests: a positive case and a negative case
  (`unity-testing`). Keep tests focused.
- Never leave the project not compiling; keep serialized data (`[FormerlySerializedAs]`) and `.meta`
  files with their assets.

## 5. Verify & report
- `/run-tests` (EditMode at least; PlayMode if the feature needs the engine loop). Fix anything red
  before reporting — `.claude/rules/rules.md` (Testing & reporting).
- If it needs a Play Mode check you can't do, give the user exact steps to verify it.
- Delegate to `qa-tester` when the change deserves more coverage than you added.
- Summarize: what changed and why, the design line as built, the test result (command,
  total/passed/failed, red names), what the user must do in the Editor, and follow-ups.
- End with: "When you consider this done, run `/update-unity`."
