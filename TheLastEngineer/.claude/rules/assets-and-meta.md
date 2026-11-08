---
description: "Rules for assets and .meta files, and for reading Unity YAML without flooding the context."
paths:
  - "Assets/**"
---

# Assets and .meta files

## Meta files and GUIDs
- Every asset has a `.meta` beside it. Create, move, rename and delete them **as a pair** — a
  missing or orphaned `.meta` breaks references silently.
- Never edit or regenerate a GUID. Every scene and prefab references assets by it.
- The project must use *Asset Serialization = Force Text* (`m_SerializationMode: 2` in
  `ProjectSettings/EditorSettings.asset`) and *Visible Meta Files* (`m_Mode: Visible Meta Files`
  in `ProjectSettings/VersionControlSettings.asset`). If not, stop and tell the user.

## Scenes, prefabs and other Unity YAML
- Don't hand-edit `.unity` / `.prefab` files except for trivial, verified changes; prefer the
  Editor or an editor script.
- Never read a whole `.unity`, `.prefab`, `.asset`, `.anim`, `.controller`, `.overrideController`,
  `.mat`, `.physicMaterial` or `.lighting` file — they are huge YAML.
  - To find where a script is used: read its `.meta` for the `guid:`, then grep the YAML for that
    GUID (`guid: <guid>`) with file names only, and read just the matching fragment
    (`offset`/`limit` around the hit).
  - To find a GameObject or field: grep by its name (`m_Name: <name>`) or the field name, and read
    only that block.
- Read deny rules don't cover `cat`/`head`/`type` through Bash — don't use them to get around this.

## Binary assets
- Never read binaries: textures, audio, models, fonts, video, `.dll`/`.so`, `.bytes`. Their
  `.meta` holds the import settings, which is usually what you need.
