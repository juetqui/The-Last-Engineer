---
title: "Floor transparency moves from per-floor materials to a global shader value read by a shared dither node, with floors split by pixel height"
author: Matiabenavide
date: 2026-11-25
branch: main
cost: { minutes: 14, tokens: 78198 }
id: l-fe59a698
scope: repo
status: active
src: agent
---

## Decided
- Floor opacity is published as a global shader value instead of being written into 4 per-floor materials, so any shader that includes the shared floor-dither node reacts. No per-object events or loops.
- Each pixel's floor comes from its world Y compared against cut heights. These are serialized per scene in the manager, one per floor, and shown with a gizmo.
- The global stores how hidden each floor is (0 = visible), so scenes without the manager show everything.
- Shadows get the same clip: a hidden floor stops casting shadows.
- Shaders that get the node: FillerShader, S_Objects, S_Connections and the Glitch family. The transparent ones use the same dither + clip so the pattern looks the same everywhere. URP Lit stays out for now.
- The 4 M_FloorDither* materials are merged into one. The system moves out of TEST LEVELS/Mati into the game folders.
- XRay excludes dithered floors through a global flag set around its occluder depth pass, not through a Rendering Layer.

## Tried and ruled out
- Per-material _FloorIndex: one material per floor for every look.
- Per-renderer Rendering Layer: needs setup on every object.
- Registering renderers + MaterialPropertyBlock: breaks the SRP Batcher and still loops over objects.
- Render Objects with Override Shader: fragile pipeline setup.
- Keeping shadows while a floor is hidden: the user wanted shadows to follow opacity.
- The uncommitted player-radius dither WIP in S_FloorDither (_PlayerPos, _PlayerFeetY, _DitherCenter, _DitherRadius, _HeightThreshold) was discarded.

## Watch out
- Objects that use stock URP Lit do not fade. Move them to S_Objects if they must.
- The XRay occluder pass uses each object's DepthOnly pass, which also applies the alpha clip. Dithered floors have to be fully discarded there, or the outline comes out dotted.
- The old manager wrote into material assets, which left them modified after Play Mode.

In the code:
- FloorDitherManager / FloorChanger (Assets/Scenes/TEST LEVELS/Mati/Scripts) are the planned rewrite.
- PlayerXRayFeature.OccluderDepthPass filters by ditheredRenderingLayer, which nothing sets.
- Planned globals: _FloorCutHeights, _FloorFade (Vector4), _FloorOccluderPass.
