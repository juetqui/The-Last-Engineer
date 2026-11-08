---
name: unity-performance
description: "Use as soon as the user reports a Unity game running badly — stutter, hitches, lag, frame drops, low FPS, GC spikes, memory growth, draw calls, slow loads — or asks to profile or optimize. Load it before investigating the cause."
---

# Unity performance

**Measure first.** No optimization without a profile showing where the time goes, and numbers
before and after. You can't run the Profiler yourself: ask the user for a capture, a screenshot
of the hierarchy view, or the numbers — and tell them exactly what to record.

## 1. Classify the symptom
| Symptom | Likely cause | Tool |
|---|---|---|
| Periodic hitch/stutter | GC collection, `Instantiate`/`Destroy`, loading, shader compile | Profiler (CPU + GC Alloc column), Timeline view |
| Constant low FPS, CPU bound | Scripts, physics, animation, UI rebuilds | Profiler CPU hierarchy, deep profile on a short capture |
| Constant low FPS, GPU bound | Overdraw, fill rate, too many passes/lights, heavy shaders | Frame Debugger, GPU profiler module, platform tools |
| Memory growth / crash on mobile | Leaked references, textures too big, Addressables not released | Memory Profiler package (compare two snapshots) |
| Slow loads | Synchronous loads, huge scenes, uncompressed assets | Profiler around the load, `Loading` markers |

Profile a **development build on the target device** for anything mobile; the Editor lies.

## 2. Usual fixes, by cause
- **GC:** remove per-frame allocations (`.claude/rules/runtime-performance.md`); pool frequently
  spawned objects; cache `WaitForSeconds`; avoid LINQ and string building in hot paths; use
  `NonAlloc` physics queries.
- **Scripts:** cache component lookups; do less per frame (spread work over frames, event-driven
  instead of polling); disable components that have nothing to do.
- **Physics:** simpler colliders, layer collision matrix, fewer rigidbodies awake, adjust Fixed Timestep only knowingly.
- **Rendering:** reduce draw calls (SRP Batcher-compatible shaders, GPU instancing, static
  batching for static geometry, atlases for UI/sprites); LODs and occlusion culling for big scenes;
  limit real-time lights and shadow distance; watch overdraw from transparent/particle layers.
- **UI:** split canvases, disable unused raycast targets, no layout groups on constantly changing
  content (`unity-ui-development`).
- **Memory:** texture size and compression per platform (`unity-asset-pipeline`); release
  Addressables handles you load; unload unused scenes/assets.
- **Loading:** async scene/asset loading; split large scenes; Addressables for content not needed at start.

## 3. Report
State: symptom, what the profile showed (marker, ms or KB per frame), the change, and the
before/after numbers — or, if you couldn't measure, say so and what the user should measure.
For a review of a diff's cost, the `perf-reviewer` agent does it read-only.
