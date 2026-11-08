---
name: git-commit
description: "Use when writing a commit, once the user has asked to commit: message format, staging only the right files, .meta pairs, no generated Unity files."
---

# Writing commits

**Commit only when asked.** Finishing a task is not permission to commit it.

## Before writing anything
- `git status` and `git diff --staged` — read the actual change, don't infer it from file names.
- On a branch, not the main one (`.claude/rules/rules.md`).
- Unity checks:
  - Every added, moved or deleted asset has its `.meta` staged with it — and no `.meta` is staged
    without its asset.
  - Nothing generated: `Library/`, `Temp/`, `Obj/`, `Logs/`, `UserSettings/`, builds, `*.csproj`, `*.sln`.
  - Scenes and prefabs: are the changes intended? Unity often re-saves unrelated values
    (lighting data, `m_RootOrder`, editor-only fields). Point them out and ask before including them.
  - `ProjectSettings/` or `Packages/manifest.json`/`packages-lock.json` changes: intended, and
    called out in the message.
- Nothing that shouldn't ship: keys, stray `Debug.Log`, local config (`.claude/settings.local.json`).
- If the staged diff does two unrelated things, say so and offer to split it.
- `.trail/learnings/` saved during the task go in the same commit as the change they came from.

## Message format
```
<subject: imperative, lowercase, ~72 chars, no trailing period>

<body: what changed and why. Wrap around 72.
Bullets when there are several distinct changes.>
```
Plus the attribution trailer this environment specifies, if any.

The subject completes "this commit will…": `pool projectiles to stop the firing hitch`, not
`fixed` or `fixes`. **The body carries the why** — the diff already shows what.

Good:
```
pool projectiles to stop the firing hitch

Each shot instantiated a projectile and destroyed it on impact, which
showed as a 2-3 ms GC spike every few seconds on Android. Projectiles now
come from an ObjectPool<Projectile> owned by the weapon.
```
Bad: `fix` · `Updated Weapon.cs, Projectile.cs and the prefab`.

## Scope
- One logical change per commit; refactor and behavior change in separate commits.
- Never mix a formatting sweep or a mass scene re-save with a real change.
- Reference the issue or ticket in the body when there is one.

<!-- team: if you prefer Conventional Commits (`feat(combat): …`), decide it once and record it here. -->
