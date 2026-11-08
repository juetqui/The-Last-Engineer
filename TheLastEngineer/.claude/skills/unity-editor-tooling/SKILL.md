---
name: unity-editor-tooling
description: "Use when building Unity editor tools: custom inspectors, property drawers, EditorWindows, menu items, asset validators, build scripts."
---

# Unity editor tooling

## Before coding
- Check what already exists: `doc/context/architecture-editor-and-pipeline.md` and
  `trail ask "editor window"` / `trail grep CustomEditor`. Extend an existing tool before adding one.
- Rules: `.claude/rules/editor-code.md` (loads with any `Editor/` file).

## Where the code goes
- An `Editor/` folder with an Editor-only asmdef that references the runtime asmdef it inspects.
- Runtime types the tool needs (attributes, marker interfaces) stay in the runtime assembly.

## Building blocks
| Need | Use |
|---|---|
| Change how one component looks in the Inspector | `[CustomEditor(typeof(T))]` + `Editor`, `CreateInspectorGUI()` (UI Toolkit) |
| Change how one field type looks everywhere | `PropertyDrawer` (+ a `PropertyAttribute` for opt-in) |
| A standalone tool | `EditorWindow` with `CreateGUI()` (UI Toolkit) |
| A one-shot action | `[MenuItem("Tools/<Project>/…")]`, with a validate method when it needs a selection |
| React to imports | `AssetPostprocessor` |
| Check assets before commit/build | Validator menu item or `IPreprocessBuildWithReport` |
| Build from the command line | static method called with `-executeMethod` (see `/build`) |

## Conventions
- Prefer **UI Toolkit** for new inspectors and windows; IMGUI only to extend existing IMGUI code.
- Edit through `SerializedObject` / `SerializedProperty` (`serializedObject.Update()` …
  `ApplyModifiedProperties()`): Undo, prefab overrides and multi-edit come for free.
- Direct object edits: `Undo.RecordObject` first, `EditorUtility.SetDirty` after, on assets.
- Many assets at once: `AssetDatabase.StartAssetEditing()` / `StopAssetEditing()` in `try/finally`,
  and a progress bar (`EditorUtility.DisplayProgressBar`, cleared in `finally`).
- Destructive actions ask first (`EditorUtility.DisplayDialog`) and touch only what the user selected.
- Tool logic that isn't UI goes in plain classes so EditMode tests can cover it.

## Workflow
1. Locate existing tools → 2. design line → 3. logic class + thin editor UI → 4. EditMode tests
   for the logic → 5. `/run-tests editmode` → 6. ask the user to open the tool and confirm it works
   (you can't click the Editor).
