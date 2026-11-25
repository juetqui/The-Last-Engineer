---
title: "The floor dither sub graph outputs only a visibility mask, without an Alpha input, and shader graphs are wired in the Editor"
author: Matiabenavide
date: 2026-11-25
branch: feature/floor-dither-global
cost: { minutes: 6, tokens: 85669 }
id: l-c358ee81
scope: repo
status: active
src: agent
touches:
  - TheLastEngineer/Assets/Prefabs/IntangibleNode.prefab
  - TheLastEngineer/Assets/Prefabs/IntangibleNode.prefab.meta
  - TheLastEngineer/Assets/Prefabs/Laser/Intangible.meta
  - TheLastEngineer/Assets/Prefabs/Laser/Intangible/IntangibleDoubleLaserContainer.prefab
  - TheLastEngineer/Assets/Prefabs/Laser/Intangible/IntangibleDoubleLaserContainer.prefab.meta
  - TheLastEngineer/Assets/Prefabs/Laser/Intangible/IntangibleLDoubleLaserContainer.prefab
  - TheLastEngineer/Assets/Prefabs/Laser/Intangible/IntangibleLDoubleLaserContainer.prefab.meta
  - TheLastEngineer/Assets/Prefabs/Laser/Intangible/IntangibleLaserContainer.prefab
  - TheLastEngineer/Assets/Prefabs/Laser/Intangible/IntangibleLaserContainer.prefab.meta
---

## Decided
- The floor dither node takes no inputs and outputs Visibility (0/1). Each graph multiplies it into Alpha, or connects it straight to Alpha when nothing else feeds it. Opaque graphs also need Alpha Clipping on.
- The dither logic lives in an HLSL file used by a Custom Function node at Single precision. Shader Graph assets are created and wired in the Editor, never by hand (project rule).

## Tried and ruled out
- A sub graph with an Alpha input written as JSON: on the node that uses it, the input slot IDs (m_PropertyIds) are not System.Guid.GetHashCode of the property guid, so they can't be generated outside the Editor.

## Watch out
- The uncommitted player-radius WIP in the old floor dither graph was discarded on purpose (git restore). It was not lost by accident.

In the code:
- Assets/Art-2/Shaders/Sub-Graphs/FloorDither.hlsl → FloorDither_float(PositionWS, ScreenPosition, out Visibility)
