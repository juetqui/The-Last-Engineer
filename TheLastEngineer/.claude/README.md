# DiPhot Unity Playbook — Claude Code config

A reusable Claude Code configuration for our Unity projects. Drop it into any Unity repo so every
teammate gets the same context, rules, skills and commands — inherited through git, not set up by
hand. It is built to spend as few tokens as possible: Claude never reads what Unity generates,
rules load only with the files they cover, skills load only when their situation comes up, and
[trail](https://www.npmjs.com/package/@trailhq/trail-code) is the code map and the team's memory.

Built for: **Unity 6+**, C#, **Input System**, **UGUI + UI Toolkit**. Render pipeline,
Addressables, DI container and target platforms are detected per project by `/init-unity`.

---

## 1. Install it in a project (once)

1. **Copy** into the Unity repo root: `CLAUDE.md`, `.claude/`, `doc/`, and merge `.gitignore`
   (keep the project's own entries; add the Claude and trail block at the end).
   Optionally copy `Assets/Editor/Build/` too — the batchmode build method `/build` calls
   (`Project.Build.Editor.BuildCommand.Build`). Skip it if the project already has one, and update
   the Build line in `CLAUDE.md`. Unity generates its `.meta` files on first import: commit them.
   If the project already has `.claude/settings.json`, merge rather than overwrite it.
2. **Check Unity serializes as text:** *Project Settings → Editor → Asset Serialization = Force
   Text* and *Version Control = Visible Meta Files*. `/init-unity` warns if not.
3. **Feed `doc/`** — GDD, specs, design notes. Rough and unstructured is fine.
4. **Run `/init-unity`** — sets up trail (asks before installing anything), reads `doc/`, the
   project settings, `Packages/manifest.json`, the asmdefs and the code, and fills
   `doc/context/` plus the Stack section of `CLAUDE.md`. Re-run it when `doc/` changes a lot.
   `doc/context/` sits outside `.claude/` on purpose: Claude Code protects `.claude/` and would
   ask for approval on every write, even with an allow rule.
5. **Fill what's left** — search for `<!-- team:` (e.g. the path to `Unity.exe`, the build method).
6. **Trust the folder once:** open `claude` interactively in the repo and accept the trust dialog.
   Until then Claude Code ignores the project's `permissions.allow` entries.

Requirements: Node.js (for trail and the hook), git, Unity Hub with the project's editor version.

---

## 2. The workflow — from idea to merged

Steps showing a **`/command`** have one; the rest are plain requests to Claude, because they're a
judgement call or a git operation you should stay in control of.

| # | Step | How |
|---|---|---|
| 1 | Shape the idea | *"let's brainstorm X"* — loads `brainstorming` when requirements aren't clear yet. |
| 2 | Branch | *"create a branch for X"*. Never work on `main` — `rules/rules.md`. |
| 3 | Build | **`/new-feature <description>`** — drafts the plan (with its `Design:` line) in plan mode and waits for your approval before writing code. |
| 4 | Verify | Included in `/new-feature`: **`/run-tests`** and a Play Mode check. Ask for **`qa-tester`** when the change deserves more tests. |
| 5 | Self-review | **`/review-pr`** with no argument — reviews the working diff. |
| 6 | Fix what came back | Ask directly, or **`/apply-fix`** if the findings came from `/audit`. |
| 7 | Close the task | **`/update-unity`** — saves learnings with trail (with your OK) and refreshes the affected context. |
| 8 | Commit | *"commit this"* — loads `git-commit`. **Claude never commits on its own.** |
| 9 | Push + open the PR | *"push and open a PR"*. |
| 10 | Review the PR | **`/review-pr <PR or branch>`** — `reviewer` agent, read-only, blocking / should-fix / nitpick. |
| 11 | Deep sweep (optional) | **`/audit branch <base>`** when the change is risky or large. |
| 12 | Merge | *"merge the PR"* — never over red tests. |
| 13 | Build | **`/build <platform>`** |

### The short version

```
/new-feature <description>   → plan (Design: line) → build → /run-tests
/review-pr                   → self-review the working diff
/update-unity                → learnings + context refresh
"commit this"                → git-commit skill
/review-pr 123               → review the PR (reviewer agent)
/build Win64
```

---

## 3. Commands

- **`/init-unity [file]`** — set up trail and generate `doc/context/` from `doc/`, project settings and
  code. Only writes `doc/context/` and the Stack section of `CLAUDE.md`.
- **`/update-unity [base]`** — close a finished task: `trail check`, propose learnings (saved only
  with your OK), `trail build`, refresh only the context the diff touched. Never commits.
- **`/new-feature <description>`** — understand → orient → plan (plan mode, with a mandatory
  `Design:` line) → build (runtime → editor → UI, EditMode tests for new logic) → verify & report.
- **`/review-pr [PR or branch]`** — review against `rules/`, `practices/` and the
  `csharp-code-review` checklist, including SOLID and whether the design is proportional.
- **`/audit [mode]`** — read-only audit: bugs, security, per-frame GC, broken serialization /
  references, editor code in runtime, missing `.meta`. Every finding has severity and confidence.
- **`/apply-fix [findings]`** — applies `/audit` findings one at a time, diff previewed. Never commits.
- **`/run-tests [editmode|playmode|all] [filter]`** — Unity Test Framework in batchmode; reports
  total / passed / failed and the names of anything red. Close the Editor first.
- **`/build <platform>`** — batchmode player build through the project's build method, with
  pre-flight (clean tree, right branch, green tests) and log/size checks.

## 4. Agents

- **`reviewer`** — read-only review of a diff, branch or PR. `/review-pr` uses it.
- **`qa-tester`** — writes the EditMode/PlayMode tests a change is missing and runs them. Test
  files only; reports production-code problems instead of fixing them.
- **`perf-reviewer`** — read-only check of a diff for per-frame cost, GC and memory.

## 5. Skills — when each one fires

Claude loads a skill when the situation matches its `description`; you rarely name one by hand.

| Skill | Fires when |
|---|---|
| `unity-gameplay-development` | Writing gameplay/runtime C#: MonoBehaviours, ScriptableObjects, input, physics. |
| `unity-editor-tooling` | Custom inspectors, EditorWindows, menu items, validators, build scripts. |
| `unity-ui-development` | In-game UI, UGUI or UI Toolkit (references: `ugui.md`, `ui-toolkit.md`). |
| `unity-performance` | Stutter, frame drops, GC, memory, rendering or load-time work. |
| `unity-testing` | Writing or running EditMode/PlayMode tests. |
| `unity-asset-pipeline` | Import settings, Addressables (`addressables.md`), build settings, folders. |
| `design-pattern-selection` | Planning a feature or refactor — is a pattern warranted, and which (`patterns-catalog.md`). |
| `csharp-code-review` | Reviewing C#/Unity changes (used by `/review-pr` and `reviewer`). |
| `find-bugs` | Hunting bugs in branch changes (behind `/audit changes`). |
| `git-commit` | Once you ask to commit. |
| `clean-code` | Writing or refactoring any C#. |
| `brainstorming` | Requirements still unclear. |
| `test-driven-development` | **Only when you ask** to work test-first (`testing-anti-patterns.md`). |

---

## 6. MCP servers

None by default — see the ADR in `doc/context/decisions.md`. Claude works through files, trail
and Unity batchmode, so tests and builds need the Editor closed.

<!-- team: if the project adopts a Unity MCP server, document it here: what it is, pinned version,
     what it's for (console, tests with the Editor open, scene hierarchy), and the `.mcp.json` +
     `enabledMcpjsonServers` setup. Then supersede the ADR. -->

## 7. What's inside

| Path | What it holds |
|---|---|
| `CLAUDE.md` (repo root) | The **only** entry point — auto-loaded every session. Stack, finding code, how to work, context index. |
| `doc/` (repo root) | Raw project docs — the **input** you write; `/init-unity` reads it (except `doc/context/`). |
| `doc/context/` | Structured project knowledge. **Generated** by `/init-unity`, refreshed by `/update-unity`. |
| `.claude/rules/` | Hard law. `rules.md` always loads; the rest load by `paths:` with the files they cover. |
| `.claude/practices/` | Preferences (softer than rules). |
| `.claude/skills/<name>/SKILL.md` | Task guidance loaded by description, with reference files beside it. |
| `.claude/commands/` | The slash commands above. |
| `.claude/agents/` | Subagent roles. |
| `.claude/hooks/` | `guard-protected-files.cjs` — asks before editing `.meta`, `ProjectSettings/`, package manifests; refuses `Library/`/`Temp/`. |
| `.claude/settings.json` | Checked-in settings: read-only allows for git/trail, read denies for Unity-generated folders, hooks. |
| `.trail/` | Created by `trail init`. `learnings/` is committed; the cache is local. |

## 8. The bar it works to

- **`rules/` — hard law.** Breaking one is a blocking finding. `rules.md` (git, security, code,
  testing & reporting) plus path-scoped rules: `solid.md`, `serialization.md`,
  `runtime-performance.md`, `editor-code.md`, `assets-and-meta.md`, `shaders.md`, `tests.md`.
- **`practices/practices.md` — preferences.** Deviating is at most a nitpick.
- **SOLID, proportionally.** SOLID is required, but it never justifies empty layers: an interface
  with one implementation and no test or assembly boundary behind it is over-engineering, and
  over-engineering is a review finding just like a violation.

## 9. Conventions

- **Read before writing.** Reuse existing systems and patterns; keep changes small.
- **Rules vs practices.** `rules/` is blocking; `practices/` is at most a nitpick.
- **Frontmatter, two styles.** Generated files (`doc/context/*`) use `purpose` + `expected_content`.
  Hand-written config uses what Claude Code reads: `description` (+ `paths` for rules,
  `argument-hint` for commands), `name` + `description` for skills and agents, optional `tools`.
- **Commands carry `disable-model-invocation: true`.** Commands and skills share one namespace and
  Claude can invoke a command on its own (it ran `/new-feature` for "make a pause menu" in testing).
  Commands are workflows you start; skills are knowledge Claude loads.
- **Descriptions that must fire before exploring say so** ("Load it before exploring or editing
  the code"). Without it, Claude went straight to reading code for "the game stutters when I shoot"
  and never loaded `unity-performance`.
- **No `@` references in commands or agents** — they're only documented for `CLAUDE.md`. Write
  plain repo-relative paths (`.claude/rules/solid.md`) and say "read …".
- **Naming.** Skills by **activity** (`unity-ui-development`, `find-bugs`); agents by **role**
  (`reviewer`, `qa-tester`). Never a `*-developer` skill. Never give a skill the same name as a
  command — they share the `/name` namespace and the skill wins.
- **Tests are run *and* reported**: command, pass/fail counts, names of anything red. No tests
  apply? Claude says so and reports the Play Mode check instead.
- **Comments: English, one line, only the "why".** Domain terms stay in their original language.
- **Placeholders.** `<!-- team: ... -->` is filled per project; `TODO:` is unconfirmed knowledge.

### Importing a skill from elsewhere

Put every external skill through this before it lands in `.claude/skills/`:

1. **Rename to the activity** — no product names (`*-pro-max`) and no roles.
2. **Rewrite the `description`** as the situation of use, in this project's terms — it decides
   whether the skill fires.
3. **Strip anything machine-local** — `hooks:`, `~/.claude/...` paths, bundled scripts.
4. **Keep only `name` + `description`** in the frontmatter.
5. **Rewrite the examples to C#/Unity** (NUnit / Unity Test Framework for tests).
6. **Overlaps an existing skill? Fold it in** instead of adding a second one.
7. **Its own folder:** `skills/<name>/SKILL.md`, extra material as sibling files linked from it.
8. **Leave the weight behind** — no binaries or large data files.
