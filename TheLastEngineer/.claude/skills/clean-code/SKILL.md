---
name: clean-code
description: "Use when writing or refactoring any C# code: naming, small methods, readability, removing duplication."
---

# Clean code (C#)

Concise, direct, no over-engineering. The user wants working code, not a lesson.

## Principles
| Principle | Rule |
|---|---|
| SRP | Each class/method does one thing (`.claude/rules/solid.md`) |
| DRY | Extract real duplication — not code that merely looks alike |
| KISS | The simplest solution that works |
| YAGNI | Don't build what isn't needed yet |
| Boy Scout | Leave the touched code a bit cleaner — without unrelated rewrites |

## Naming
| Element | Convention |
|---|---|
| Types, methods, properties, events | `PascalCase`; methods are verb + noun: `ApplyDamage()`, not `Damage()` |
| Private fields | `_camelCase` (`practices.md`) |
| Booleans | Question form: `IsAlive`, `HasTarget`, `CanJump` |
| Constants | `PascalCase` (`const float MaxSpeed = 10f;`) |
| Events | What happened: `Died`, `HealthChanged`; handlers `OnDied` |

If a name needs a comment to explain it, rename it.

## Methods and structure
- Small (aim for 5–20 lines), one level of abstraction, at most 3 parameters (group the rest in a
  struct or config SO).
- Guard clauses and early returns; at most 2 levels of nesting.
- No hidden side effects — a `Get…` doesn't change state.
- Keep related code together; a feature's scripts near each other.

## Anti-patterns
| ❌ | ✅ |
|---|---|
| Comment on every line | Delete obvious comments |
| `Utils`/`Helpers` class with one method | Put the code where it's used |
| Factory/manager for one object | Direct `Instantiate` / reference |
| Magic numbers (`speed * 3.5f`) | Named constant or serialized field |
| God component | Split by responsibility |
| `public` fields for the Inspector | `[SerializeField] private` |

## Before editing a file
- Who uses it? `trail callers <symbol>` (confirm with `trail grep`), plus scenes/prefabs that
  reference the component (grep its GUID).
- Public signature or serialized field changing? Update every caller and keep serialized data
  (`[FormerlySerializedAs]`) in the same task. Never leave the project not compiling.

## Before saying "done"
- Goal met exactly; all affected files updated; it compiles; `/run-tests` run and reported
  (`.claude/rules/rules.md`); edge cases considered.
- Read the command output — what failed, where (`file:line`). Ask before fixing anything outside
  the task. Re-run after fixing.
