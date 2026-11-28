---
title: "Intangible particle repel reacts only to the player's body while it carries an Intangible node; Edit Mode keeps a preview with any collider"
author: Matiabenavide
date: 2026-11-28
branch: main
cost: { minutes: 10, tokens: 77983 }
id: l-2cc07978
scope: repo
status: active
src: agent
touches:
  - TheLastEngineer/Assets/Art-2/Shaders/Shader Materials/M_Glitch.mat
  - TheLastEngineer/Assets/Art-2/Shaders/Shader Materials/M_Intangible.mat
  - TheLastEngineer/Assets/Art-2/Shaders/Shader_Graphs/S_Cristal.shadergraph
  - TheLastEngineer/Assets/Art-2/Shaders/Shader_Graphs/S_Glitch.shadergraph
  - TheLastEngineer/Assets/Prefabs/Laser/Intangible/IntangibleDoubleLaserContainer.prefab
  - TheLastEngineer/Assets/Prefabs/Laser/Intangible/IntangibleLDoubleLaserContainer.prefab
  - TheLastEngineer/Assets/Prefabs/Laser/Intangible/IntangibleLaserContainer.prefab
  - TheLastEngineer/Assets/Prefabs/Laser/Intangible/IntangibleQuadLaserContainer.prefab
  - TheLastEngineer/Assets/Prefabs/Laser/Intangible/IntangibleTripleLaserContainer.prefab
  - TheLastEngineer/Assets/Prefabs/Laser/NEW Intangible.meta
---

## Decided
- In Play Mode the mesh particles of Intangible objects move away only when the player's body (CharacterController + CapsuleCollider) is in the detection box AND the node in hand is Intangible. Otherwise the repel weight fades back to 0 at the normal fade speed (also when the node is dropped inside the box).
- Edit Mode has no player instance (set in Awake), so there any collider still previews the effect, to keep tuning without Play.
- The filter is fixed in the component, not an Inspector toggle: every current use is an Intangible prefab, so no new field or prefab migration.

## Tried and ruled out
- Counting any collider under the player: child triggers (interaction radius) would start the effect far too early.
- No repel at all in Edit Mode / dropping ExecuteAlways: loses the in-editor tuning the component was built for.
- Instant cut when intangibility is lost: the existing fade was preferred.

## Watch out
- The prefabs' repeler mask is only layer 8 ("Laser Collition"), the player's default layer, so the 8-hit overlap buffer can't be filled by other colliders. Widening the mask brings that risk back.
- During teleport the player moves to layer 14 (Teleport), outside the mask, so the repel cuts out.

In the code:
- Assets/Scripts/Gameplay/VFX/VfxMeshRepeller.cs: CanRepelThisFrame, IsValidRepeller
- PlayerController.IsIntangible and IsOwnCollider (same criteria the Intangible laser uses)
- PlayerData.asset defaultLayer m_Bits 256 = prefabs' _repelerLayers m_Bits 256
