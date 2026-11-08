---
description: "Set up trail and generate the structured doc/context/ files from doc/, the Unity project settings and the codebase."
argument-hint: "[context file to (re)generate — blank for all]"
disable-model-invocation: true
---

# /init-unity — set up trail and generate the project context

Build this project's `doc/context/` files and the Stack section of `CLAUDE.md` from `doc/`,
the Unity project settings and the code. If `$ARGUMENTS` names one context file, regenerate only
that one (skip step 1 unless trail is missing).

## 1. Trail
1. `trail --version`. If it's missing, show the user
   `npm install -g @trailhq/trail-code` and **ask before running it** — it installs on the machine.
2. If the repo has no `.trail/`: run `trail init --dry-run --no-agents` and show the user what it
   would write, including anything outside the repo (`~/.claude/…`; `--no-global` skips that).
   **Ask before running** `trail init --no-agents` (with `--no-global` if the user prefers).
3. After it: `git status` and `git diff .claude/settings.json`. trail merges its `SessionStart`
   hook into the existing settings — confirm the template's `permissions` and `hooks` survived.
   If trail added instructions anywhere that duplicate `CLAUDE.md`, point it out; don't duplicate.
4. `trail build` if init didn't build, then `trail map` for clusters and hubs. `trail status`
   should show only real code — if `Library/`, `Temp/` or other generated folders show up, the
   project's `.gitignore` is missing them: warn.

## 2. Read the sources
Read each only as far as needed; never whole YAML assets (`.claude/rules/assets-and-meta.md`).
- `ProjectSettings/ProjectVersion.txt` → Unity version.
- The Unity editor path: `C:\Program Files\Unity\Hub\Editor\<version>\Editor\Unity.exe` (Windows),
  `/Applications/Unity/Hub/Editor/<version>/Unity.app/Contents/MacOS/Unity` (macOS). Check it exists.
- `Packages/manifest.json` → render pipeline (`com.unity.render-pipelines.universal` / `.high-definition`
  / neither = Built-in), Input System, Addressables, UI Toolkit/UGUI, DI (`jp.hadashikick.vcontainer`,
  Zenject/Extenject), Test Framework, other notable packages.
- `ProjectSettings/EditorBuildSettings.asset` → build scenes (grep `path:`).
- `ProjectSettings/EditorSettings.asset` → `m_SerializationMode` (2 = Force Text) and
  `ProjectSettings/VersionControlSettings.asset` → visible meta files. **Warn** if not text/visible.
- `ProjectSettings/ProjectSettings.asset` → grep `productName`, `companyName`, scripting backend and
  defines per platform; and the build targets the project is set up for.
- Every `*.asmdef` → assembly graph (name, references, platforms, test assemblies).
- `Assets/` tree, 2–3 levels, no binaries; scenes and key prefabs by name.
- `doc/` — every file **except `doc/context/`** (that's this command's output, not a source). It
  is the **authoritative** source for game design; if it's empty, rely on
  the code and leave design sections as `TODO:`.
- `trail map` / `trail ask` for systems, entry points and patterns in use (events, singletons, DI,
  state machines, pools).

## 3. Write `doc/context/`
For each file, read its frontmatter `expected_content` and write the body:
`game-design-and-glossary.md`, `architecture-runtime.md`, `architecture-editor-and-pipeline.md`,
`project-map.md` (≤ ~100 lines — the highest-value file), `decisions.md` (**keep every existing
entry** — the template ships the "No Unity MCP server" ADR; add only structural decisions the
sources state, newest first. If the project has a `.mcp.json` with a Unity MCP, mark that ADR
superseded and add one describing the server).
- Keep the frontmatter intact; only the body changes.
- Only what the sources support. Unknowns get a short `TODO:` — never invent.
- English; domain terms in their original language, exactly as the code names them.

## 4. Fill the Stack in `CLAUDE.md`
Replace the `<!-- team: … -->` placeholders of the **Stack** section (and the project name/line if
`doc/` or `ProjectSettings` give it) with what was detected. Leave the rest of `CLAUDE.md` alone.

## Scope
Writes only `doc/context/` and the Stack section of `CLAUDE.md` (plus what trail init writes,
with confirmation). Never touches rules, practices, skills, commands or agents. Never commits.

## Report
Per file: what was written. Then the `TODO:` gaps left for the team, and warnings (serialization
not Force Text, hidden meta files, generated folders indexed by trail, Unity editor not found).
