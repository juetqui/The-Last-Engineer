---
name: design-pattern-selection
description: "Use when planning a feature or refactor in C#, to decide whether a design pattern is warranted for its scope and which one."
---

# Design pattern selection

A pattern is a cost: more types, more indirection, more to read. It has to pay for itself with a
problem that exists **now**. "No pattern" is a valid — and frequent — answer.

## Procedure

### 1. Classify the scope
| Scope | Looks like | Patterns |
|---|---|---|
| **Small** | A fix, a tweak, one isolated component, fewer than ~3 files | **None.** Direct, clear code. Say so explicitly. |
| **Medium** | A feature touching several components or an existing system | At most **one**, only if it solves a problem that is present in this task. |
| **Large** | A new system, an architecture refactor, something other systems will consume | Chosen per problem, each with justification and the alternative considered. |

### 2. Name the problem, not the pattern
What actually hurts? Behavior that varies by type · systems that must react without knowing each
other · an entity with states and transitions · objects created and destroyed constantly ·
undoable actions · variable construction · shared design data · global services · UI with logic ·
startup order. If you can't name the problem, there's no pattern to add.

### 3. Reuse before introducing
Check what the project already uses for that problem: `doc/context/architecture-runtime.md`
("patterns in use"), `trail ask "<problem>"`. If an event bus, a service locator or a state machine
already exists, **use it** — a second mechanism for the same problem is a finding.

### 4. Pick the simplest option that solves it
Use [patterns-catalog.md](patterns-catalog.md): problem → pattern → Unity/C# idiom → when **not** to
use it. Prefer the Unity idiom (C# events, ScriptableObjects, `UnityEngine.Pool`, components) over
the textbook version.

### 5. Write the design line in the plan
```
Design: <pattern> because <problem>; alternative considered: <X> (<why not>)
Design: no pattern — <reason, e.g. small scope: one component, no variation>
```
Add one line on SOLID: which responsibility each new type has and what it depends on.

## Smells
- **Over-engineering:** interface with one implementation and no test/assembly need; factory
  around a single `Instantiate`; event channel between two objects that already reference each
  other; state machine for two states; "manager" wrapping one call.
- **Under-engineering:** a `switch` on type that grows with every feature; the same if/else
  copied across classes; a component that does five things; `FindObjectOfType` to reach a service.

Both are review findings (`csharp-code-review`).
