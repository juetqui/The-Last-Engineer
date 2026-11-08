---
name: csharp-code-review
description: "Reviewing C#/Unity changes — the checklist /review-pr and the reviewer agent apply: SOLID, pattern proportionality, serialization, per-frame cost, meta/GUIDs, editor vs runtime, tests."
---

# C# / Unity code review

The bar is `.claude/rules/` (blocking) and `.claude/practices/practices.md` (nitpick at most).
This checklist is how to apply it to a Unity diff. Report each finding with severity
(**blocking** / **should-fix** / **nitpick**), `file:line`, the problem and a concrete fix.

## 1. Correctness
- Logic and edge cases; null paths, including **destroyed objects** (`?.`/`??` on
  `UnityEngine.Object` is a bug — use `== null` or `if (obj)`).
- Event subscriptions paired (`OnEnable`/`OnDisable`, `+=`/`-=`); coroutines stopped with their owner;
  `async` code cancels with `destroyCancellationToken` and re-checks the object after `await`.
- Lifecycle order: reads of other objects in `Awake`; initialization that assumes an order.
- Physics in `FixedUpdate`; rigidbodies not moved through `transform`.

## 2. Design — SOLID and proportionality (`.claude/rules/solid.md`)
- One responsibility per class/component; domain logic out of `MonoBehaviour`s.
- New `switch`/`if` per type where an implementation would do (O).
- Overrides that do nothing or throw (L); fat interfaces (I); `Find*`/new singletons to get
  dependencies (D).
- **Proportionality:** does the scope justify the pattern used? Interface with one implementation
  and no test/assembly need, factory for one `Instantiate`, event channel between two linked
  objects → should-fix (over-engineering). Does it reuse the mechanism the project already has?
  The plan's `Design:` line should match what was built.
- SOLID violations are **should-fix**, **blocking** when they also break another rule.

## 3. Serialization and assets (`serialization.md`, `assets-and-meta.md`)
- Renamed serialized field without `[FormerlySerializedAs]` → **blocking** (data loss).
- `public` fields for the Inspector instead of `[SerializeField] private`.
- Class renamed/moved without its `.meta`, or class name ≠ file name → **blocking**.
- Every new/moved/deleted asset has its `.meta` in the diff; no GUID edits.
- Hand-edited `.unity`/`.prefab` changes: trivial and intended? Unrelated scene noise in the diff?

## 4. Per-frame cost (`runtime-performance.md`)
- Allocations in `Update`/`FixedUpdate`/`LateUpdate`: LINQ, `new` collections, closures, string
  building, boxing. `GetComponent`/`Find*`/`Camera.main` per frame. `tag ==`. Spawn/destroy without
  pooling. Logs in hot paths. For a deeper pass, the `perf-reviewer` agent.

## 5. Editor vs runtime (`editor-code.md`)
- `using UnityEditor` in runtime code without `#if UNITY_EDITOR` → **blocking** (player build breaks).
- Editor code outside `Editor/` folders/asmdefs; tools that change assets without `Undo`/`SetDirty`.

## 6. Security (`rules.md`)
- Secrets or keys in code, ScriptableObjects, `Resources/`, `StreamingAssets/` → **blocking**.
- Save files, network data, deep links used without validation; personal data in logs.

## 7. Tests (`tests.md`)
- New logic has EditMode tests with a positive and a negative case; bug fixes have a regression test.
- Tests deterministic and in test asmdefs. Test results reported (command, counts, red names).
- No test applies? Evidence of a Play Mode check instead — otherwise that's a finding.

## 8. Consistency and simplicity
- Naming (`practices.md`: `_camelCase` privates, PascalCase members), namespaces by folder.
- Dead code, commented-out code, stray `Debug.Log`, duplication, new packages without justification.
