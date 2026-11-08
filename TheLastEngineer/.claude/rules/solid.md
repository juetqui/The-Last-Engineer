---
description: "SOLID principles for C#, with proportionality — no empty layers."
paths:
  - "**/*.cs"
---

# SOLID (proportional)

Violating SOLID is a **should-fix** finding (blocking if it also breaks another rule). So is
over-engineering. To decide whether a pattern is warranted, use the `design-pattern-selection` skill.

## S — Single responsibility
- One `MonoBehaviour`, one responsibility: moving, taking damage and playing audio are separate
  components.
- No god objects ("GameManager" that knows everything). A class with more than one reason to
  change gets split.
- Domain logic lives in plain C# classes that can be tested without a scene; the `MonoBehaviour`
  is a thin adapter that wires Unity to them.

## O — Open/closed
- Extend behavior with new implementations (interfaces, strategy ScriptableObjects,
  `[SerializeReference]`), not with another `switch`/`if` branch per type.

## L — Liskov substitution
- An implementation honors its base contract: it doesn't throw where the base doesn't, doesn't
  ignore parameters, and doesn't demand extra preconditions.
- An `override` that has to "do nothing" means the hierarchy is wrong.

## I — Interface segregation
- Small, capability-based interfaces (`IDamageable`, `IInteractable`, `ISaveable`) — never one fat
  interface per entity.

## D — Dependency inversion
- Depend on injected abstractions: `[SerializeField]`/`[SerializeReference]` from the Inspector,
  constructor parameters in plain classes, or the project's DI container.
- Don't obtain dependencies with `FindObjectOfType`/`FindFirstObjectByType`, `GameObject.Find` or
  new static singletons. If the project already uses singletons, don't add another without
  justifying it in the plan.

## Proportionality
- SOLID never justifies empty layers. No interface with a single implementation unless a test or
  an assembly boundary needs it.
- No abstractions "just in case" — add the second implementation when it exists, not before.
- The smallest design that keeps the code testable and readable wins.
