---
name: find-bugs
description: "Use when hunting bugs and security holes in branch changes or a given area of a Unity project. The method behind /audit changes and /audit branch."
---

# Find bugs

Review the changes for bugs, security holes and Unity-specific failure modes. Report; don't fix.

## Phase 1: Gather the complete input
1. Full diff against the base: `git diff <base>...HEAD` (the repo's default branch if not given),
   plus uncommitted changes.
2. If the output is truncated, read each changed file until you have seen every changed line.
   For scenes/prefabs, read only the changed hunks (`.claude/rules/assets-and-meta.md`).
3. List every changed file before going on.

## Phase 2: Map the risk surface
For each changed file, list:
- Lifecycle entry points (`Awake`/`OnEnable`/`Start`/`Update`/`OnDestroy`), coroutines, `async` methods
- Event subscriptions and unsubscriptions
- Serialized fields added, renamed, retyped or removed
- External input: save files, network, deep links, player-entered text, mods
- Per-frame code paths
- Editor/runtime boundary (`UnityEditor` usage, `#if` guards)

## Phase 3: Checklist — every item, every file
- [ ] **Destroyed objects:** `?.`/`??`/`is null` on `UnityEngine.Object`; access after `Destroy`
      or after an `await`/`yield` when the object may be gone.
- [ ] **Event leaks:** `+=` without `-=`; static events holding scene objects; lambdas that can't be unsubscribed.
- [ ] **Orphan coroutines / async:** coroutines that outlive their purpose; `async void`
      swallowing exceptions; no cancellation on destroy; work continuing after scene unload.
- [ ] **Races:** order-dependent `Awake`/`Start`; two `async` paths writing the same state; reentrancy in event handlers.
- [ ] **Serialization:** renamed field without `[FormerlySerializedAs]`; type change losing data;
      `.meta` missing/regenerated; class name ≠ file name.
- [ ] **Editor in runtime:** `UnityEditor` without guard; editor-only APIs in player code.
- [ ] **Per-frame cost causing hitches:** allocations, `Find*`, `GetComponent` in hot paths.
- [ ] **State leaks:** runtime state written to shared ScriptableObjects; statics not reset
      (Enter Play Mode Options with domain reload off).
- [ ] **Physics/time:** physics outside `FixedUpdate`; `Time.deltaTime` in `FixedUpdate` vs
      `fixedDeltaTime`; frame-rate-dependent logic.
- [ ] **Input trust & data:** unvalidated save/network data, unbounded loops or allocations from
      external data, integer overflow in scores/currency.
- [ ] **Secrets & privacy:** keys in code/SOs/`Resources`/`StreamingAssets`; personal data or tokens in logs.
- [ ] **Game logic:** edge cases, state machine violations, off-by-one in timers/cooldowns.

## Phase 4: Verify
For each candidate: check it isn't already handled elsewhere, look for a test covering it, read the
surrounding context. Drop what doesn't survive.

## Phase 5: Pre-conclusion audit
1. Every file reviewed, confirmed read completely.
2. Every checklist item: issue found or clean.
3. What you could NOT verify and why (e.g. needs Play Mode, needs a scene you didn't read).
4. Only then, the findings.

## Output
Running under `/audit`? Use that command's report format — this is the method, not the presentation.

Prioritize: security > data loss / crashes > bugs > code quality. Skip style.
For each issue: **file:line** · **Severity** (Critical/High/Medium/Low) · **Problem** ·
**Evidence** (why it's real) · **Fix**.

Found nothing significant? Say so — don't invent issues. Don't change code.
