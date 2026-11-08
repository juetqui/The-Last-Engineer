---
description: "Per-frame performance rules for runtime scripts."
paths:
  - "Assets/**/Runtime/**/*.cs"
  - "Assets/Scripts/**/*.cs"
---

# Runtime performance

Applies to code that runs every frame (`Update`, `FixedUpdate`, `LateUpdate`, per-frame callbacks
and coroutines). Measure with the Profiler before optimizing anything else — `unity-performance` skill.

- No allocations per frame: no LINQ, no `new` collections, no closures/lambdas capturing state, no
  string concatenation or interpolation, no boxing (e.g. enums as `object`, non-generic APIs).
  Reuse buffers and use the `NonAlloc`/`List<T>` overloads of physics queries.
- Never call `Find*`, `FindObjectOfType`, `GetComponent` or `Camera.main` in per-frame code —
  cache them in `Awake`/`Start` (or inject them).
- Objects created and destroyed frequently (bullets, VFX, UI rows) go through pooling
  (`UnityEngine.Pool.ObjectPool<T>`).
- Compare tags with `CompareTag`, never `tag ==`.
- Physics in `FixedUpdate`; move rigidbodies with `Rigidbody`/`Rigidbody2D` APIs, never by
  writing `transform`.
- Don't use `SendMessage`/`BroadcastMessage`; call through a reference or an event.
- Cache `WaitForSeconds` and other yield instructions instead of allocating one per iteration.
- Don't leave `Debug.Log` in per-frame paths — it allocates and is slow even in builds.
