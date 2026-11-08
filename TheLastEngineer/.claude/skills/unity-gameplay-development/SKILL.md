---
name: unity-gameplay-development
description: "Use whenever adding or changing game behavior in Unity — player abilities and movement, enemies, combat, game systems, MonoBehaviours, ScriptableObjects, input (Input System), physics, coroutines and async. Load it before exploring or editing the code."
---

# Unity gameplay development

## Before coding
- Find the system first: `doc/context/project-map.md`, then `trail ask "<system or behavior>"`.
  Read `doc/context/architecture-runtime.md` for its boundaries and the patterns already used.
- Read the `.trail/learnings/` that touch the area.
- Medium or large change? Decide the design with `design-pattern-selection` before writing code.

## Lifecycle — put each thing where it belongs
| Callback | Use it for |
|---|---|
| `Awake` | Own references and state (`GetComponent` on self, caching). Don't touch other objects' state — their `Awake` may not have run. |
| `OnEnable` / `OnDisable` | Subscribe / unsubscribe to events. Always in pairs. |
| `Start` | Anything that needs other objects initialized. |
| `Update` / `LateUpdate` | Per-frame logic / camera and follow logic after movement. |
| `FixedUpdate` | Physics only. |
| `OnDestroy` | Release what `Awake` acquired (native containers, static registrations). |

Order between scripts is not guaranteed; if it matters, make the dependency explicit (inject or
initialize from one owner) rather than reaching for Script Execution Order.

## Conventions
- **Composition:** small components, one responsibility each (`rules/solid.md`). A `MonoBehaviour`
  is a thin adapter; the rules of the game live in plain C# classes that EditMode tests can reach.
- **Data:** tuning and design data in ScriptableObjects, not hardcoded. Runtime state per instance
  does **not** go in a shared ScriptableObject (it leaks across instances and, in the Editor,
  persists after Play Mode).
- **Communication:** direct serialized references inside a feature; C# events (`event Action<T>`)
  between systems; ScriptableObject event channels when designers wire it in the Inspector. Reuse
  whichever the project already uses (`architecture-runtime.md`).
- **Input:** Input System only. Read actions from an `InputActionAsset` / generated C# class or
  `PlayerInput`; enable/disable action maps with the owner's lifecycle. No `UnityEngine.Input`.
- **Physics:** move rigidbodies with `Rigidbody.MovePosition`/`velocity`/forces in `FixedUpdate`;
  use layers and the collision matrix instead of tag checks in callbacks.
- **Coroutines and async:** a coroutine dies with its GameObject; stop it explicitly when its
  owner is disabled if it shouldn't resume. With `async`/`Awaitable` (Unity 6), pass a
  `CancellationToken` (`destroyCancellationToken`) and check the object still exists after every
  `await`.
- **Destroyed objects:** Unity's `== null` is true for destroyed objects but `?.` and `??` don't
  see it. Compare with `== null` (or `if (obj)`) for `UnityEngine.Object`.
- **Assemblies:** runtime code goes in a runtime asmdef; reference only what it needs.

## Workflow
1. Locate (`trail ask`, project map) → 2. design line (if not small) → 3. plain C# logic + thin
   component → 4. EditMode tests for the logic (`unity-testing`) → 5. wire in the Inspector/prefab
   (tell the user what to assign if you can't do it from code) → 6. `/run-tests` + Play Mode check.

## References
- Per-frame rules: `.claude/rules/runtime-performance.md`. Serialization: `.claude/rules/serialization.md`.
