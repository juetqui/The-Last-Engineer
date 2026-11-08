# <!-- team: project name -->

<!-- team: one line — what the game is and who it is for. -->

## Stack
Filled by `/init-unity` from `ProjectSettings/` and `Packages/manifest.json`; unknowns stay `TODO:`.
- Unity: 6 or newer — exact version <!-- team: from ProjectSettings/ProjectVersion.txt -->
- Unity editor: <!-- team: path to Unity.exe, e.g. C:\Program Files\Unity\Hub\Editor\<version>\Editor\Unity.exe -->
- Render pipeline: <!-- team: Built-in | URP | HDRP -->
- UI: UGUI and UI Toolkit — check which one a screen uses before changing it.
- Input: Input System package. No legacy `UnityEngine.Input` in new code.
- Addressables: <!-- team: yes | no -->
- DI container: <!-- team: VContainer | Zenject | none -->
- Target platforms: <!-- team: e.g. Win64, Android -->
- CI: none.

## Finding code
- Run `trail ask "<query>"` before grepping or reading source. `trail skeleton <file>` gives a
  file's API; `trail callers <symbol>` before changing a signature — in C# it can miss callers of
  a name shared by an interface and its implementation, so confirm with `trail grep <symbol>`.
- Read the `.trail/learnings/` relevant to the task before exploring.
- Start from `doc/context/project-map.md` to know where a system lives.
- Never read Unity-generated folders (`Library/`, `Temp/`, `Obj/`, `Logs/`, `UserSettings/`,
  `Build*/`) or `*.csproj`/`*.sln` — Unity regenerates them.
- Never read a whole `.unity`, `.prefab`, `.asset`, `.anim`, `.controller` or `.mat`: grep it by
  GUID or script name and read only the matching fragment. A script's GUID is in its `.meta`.

## Open, test, build
- Open: Unity Hub, or `"<Unity.exe>" -projectPath .`
- Tests: `/run-tests [editmode|playmode|all]` — batchmode; it fails while the Editor has the
  project open, so close it or run the Test Runner in the Editor and paste the result.
- Build: `/build <platform>` — calls `Project.Build.Editor.BuildCommand.Build`
  (`Assets/Editor/Build/`) <!-- team: replace if the project has its own build method -->

## How to work here
- At the start of a task, read the relevant files in `doc/context/` (index below). `TODO:`
  markers there are open questions, not facts.
- `doc/` holds the authoritative design docs (GDD, specs); `doc/context/` (generated) only summarizes them.
- Follow `.claude/rules/` (hard law — breaking one is a blocking finding) and
  `.claude/practices/` (preferences). Path-scoped rules load with the files they cover.
- Read before writing; reuse existing components, systems and patterns.
- Keep changes small and focused; don't break existing behavior, scenes or prefab references.
- C# follows SOLID (`.claude/rules/solid.md`) with a design proportional to the task's scope:
  every plan states `Design: <pattern> because <problem>` or `Design: no pattern — <reason>`
  (skill `design-pattern-selection`). Over-engineering is a finding, like a violation.
- Work on a branch, never on `main`. Never commit unless the user asks.
- Before calling anything done, run the tests and report the command, pass/fail counts and the
  names of anything red. If no test applies, say so and report the Play Mode check you did.

## Context index (`doc/context/`)
- `game-design-and-glossary.md` — what the game is, core loop, entities, glossary with code names.
- `architecture-runtime.md` — runtime systems, their boundaries, communication, patterns in use, bootstrap, saving.
- `architecture-editor-and-pipeline.md` — asmdefs graph, editor tools, Addressables/Resources, import settings, build.
- `project-map.md` — where things live: folders, build scenes, key prefabs, each system's main script.
- `decisions.md` — structural decisions (ADRs), newest first.

## Closing a task
When the user considers the task done, they run `/update-unity`: it saves the session's learnings
with trail (after confirmation) and refreshes only the context the diff affected. It never commits.

## Conventions
- Config, code comments and context are in English; domain terms keep their original language.
- Generated/filled files (`doc/context/*`) use `purpose` + `expected_content` frontmatter;
  hand-written config uses what Claude Code reads: `description` (+ `paths` in rules;
  `argument-hint` + `disable-model-invocation: true` in commands, so only the user runs them),
  `name` + `description` in skills and agents, optional `tools`.
- Skills are named by activity (`unity-ui-development`, `find-bugs`), agents by role
  (`reviewer`, `qa-tester`). Never a `*-developer` skill.
- A skill loads by its `description` — write it as the situation of use. No routing tables here.
- `TODO:` marks unconfirmed knowledge — never invent. `<!-- team: … -->` marks what each project fills in.
- Importing an external skill: follow the checklist in `.claude/README.md`.
