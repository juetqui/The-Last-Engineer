# Addressables reference

## Loading and releasing
- Every load has a release. `Addressables.LoadAssetAsync<T>` → keep the handle →
  `Addressables.Release(handle)` when the owner is done (usually `OnDestroy`).
- `Addressables.InstantiateAsync` → `Addressables.ReleaseInstance(go)` (not `Destroy`).
- Prefer `AssetReference` / `AssetReferenceT<T>` fields over string keys — they survive renames
  and show in the Inspector.
- Await handles (`await handle.Task` or the `Completed` event); check `handle.Status` before using
  the result. Never `WaitForCompletion()` on the main thread outside loading screens.
- A leaked handle keeps the whole bundle in memory — the Memory Profiler and the Addressables
  Event Viewer/Profiler module show it.

## Groups and bundles
- Group by what loads together (a level, a character, a UI screen), not by asset type.
- *Pack Together* for content always used together; *Pack Separately* for content loaded à la carte.
- Local vs remote groups: remote only if the project has a content delivery setup (`TODO:` in
  `architecture-editor-and-pipeline.md` if unknown).
- Watch duplicated dependencies: run *Analyze → Check Duplicate Bundle Dependencies* after
  reorganizing groups.

## Builds
- The content build (`Build → New Build → Default Build Script`) must run before the player build
  if the build method doesn't do it; `/build` should call `AddressableAssetSettings.BuildPlayerContent()`.
- Changing groups after release requires a *content update* build against the previous state
  (`addressables_content_state.bin`) — don't rebuild from scratch for a patch.

## Don't
- Don't mix `Resources.Load` and Addressables for the same asset — it ends up duplicated in the build.
- Don't move an addressable asset outside Unity — the address entry follows the GUID only if the
  `.meta` moves with it.
