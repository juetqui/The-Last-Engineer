---
name: unity-asset-pipeline
description: "Use when working with Unity import settings, Addressables, build settings/scenes, or asset folder organization."
---

# Unity asset pipeline

## Before changing anything
- `doc/context/architecture-editor-and-pipeline.md` (Addressables/Resources, import
  conventions, build) and `doc/context/project-map.md` (folders).
- Rules: `.claude/rules/assets-and-meta.md` — `.meta` in pairs, never edit GUIDs, never read
  whole YAML assets or binaries.

## Import settings
Import settings live in the asset's `.meta` — read that, not the asset.
| Type | Check |
|---|---|
| Textures | Max size per platform, compression (ASTC on mobile, BC on desktop), mipmaps off for UI, `Read/Write` off unless code needs pixels, sprite mode/atlas for 2D |
| Audio | Short SFX: *Decompress On Load*; music: *Streaming*; mono where stereo isn't needed; `Load In Background` for big clips |
| Models | `Read/Write` off, import only what's used (cameras/lights off), animation compression, mesh compression |

- Batch changes through an `AssetPostprocessor` or a preset, not by hand-editing `.meta` files.
  Editing `.meta` files triggers a confirmation (protected-files hook) — say why you need to.

## Addressables
If the project uses them (`Packages/manifest.json` has `com.unity.addressables`), read
[addressables.md](addressables.md). If it uses `Resources/`, don't migrate it as a side effect —
propose it as its own task.

## Build settings and scenes
- Build scenes: `ProjectSettings/EditorBuildSettings.asset` — grep `path:`, don't read it whole.
  Change the list from the Editor or an editor script (`EditorBuildSettings.scenes`), not by hand.
- `ProjectSettings/**` and `Packages/manifest.json` changes trigger a confirmation; explain the
  change and its impact on every target platform.

## Folder organization
- Follow the existing layout (project map). New feature → its own folder with its code, prefabs and
  data together, when that's what the project does; otherwise by type.
- Moving assets: move each `.meta` with it (or do it from the Editor) — references survive only
  through the GUID.
- Nothing under `Resources/` unless it must load by path at runtime: everything there ships in
  every build.
