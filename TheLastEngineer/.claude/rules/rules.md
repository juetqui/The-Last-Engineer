---
description: "Non-negotiable rules for this codebase: git, security, code, testing & reporting. Always loaded."
---

# Rules

Hard constraints. If a change would break one of these, stop and flag it. Language- and
folder-specific rules live beside this file and load with the files they cover.

## Git
- Work on a branch; never commit directly to the main branch.
- Never commit unless the user asks. Finishing a task is not permission to commit it.
- Never force-push to shared branches.
- Don't merge with failing tests or red CI.
- Keep commits focused, with messages that say what changed and why.
- Don't commit generated artifacts (`Library/`, `Temp/`, `Obj/`, `Logs/`, builds, `*.csproj`,
  `*.sln`) or local config (`UserSettings/`, `.claude/settings.local.json`).

## Security
- Never put secrets in code, ScriptableObjects, `Resources/`, `StreamingAssets/` or scenes —
  everything there ships inside the build and can be extracted.
- Service keys and endpoints come from config or environment, per build; never hardcoded.
- Never log personal data, tokens, receipts or credentials — not even in development builds.
- Treat data from saves, the network, mods and deep links as untrusted: validate before use.

## Code
- No magic values — use named constants, enums, or ScriptableObject/config data.
- Fail safely: handle nulls (including destroyed Unity objects) and errors explicitly; never
  swallow exceptions silently.
- Don't break existing behavior: check callers, scenes and prefabs that reference what you change.
- Don't add packages, plugins or assets from the Asset Store without justification — each one is
  build size, attack surface and upgrade cost. Say why in the plan.

## Testing & reporting
- Before reporting anything as done, run `/run-tests` (or the closest applicable suite).
- Always report the result: the command run, total / passed / failed, and the name of every red
  test. Compilation errors count as red.
- Never report a task as complete over a red suite — fix it, or stop and say what's broken and why.
- If no test applies (scene layout, art, UI-only tweak), say so explicitly and report the Play Mode
  verification you did instead.
