---
description: "Rules for editor-only code: Editor folders, asmdefs, UNITY_EDITOR guards, Undo."
paths:
  - "**/Editor/**/*.cs"
---

# Editor code

- Editor code lives only in `Editor/` folders with an Editor-only asmdef (`includePlatforms:
  ["Editor"]`), or behind `#if UNITY_EDITOR`.
- Never `using UnityEditor` in runtime code without an `#if UNITY_EDITOR` guard — the player build
  fails to compile.
- Runtime assemblies never reference editor assemblies; editor assemblies may reference runtime ones.
- Changing objects or assets from a tool: `Undo.RecordObject` before the change (so Ctrl+Z works),
  `EditorUtility.SetDirty` after it on assets, and `AssetDatabase.SaveAssets` when it must persist.
- Prefer `SerializedObject`/`SerializedProperty` in inspectors — they handle Undo, prefab overrides
  and multi-object editing.
- Batch `AssetDatabase` operations with `AssetDatabase.StartAssetEditing`/`StopAssetEditing` (in a
  `try`/`finally`) when touching many assets.
- Menu items and tools never modify scenes or assets the user didn't select or confirm.
