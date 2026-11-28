---
title: Intangible fades a single _Alpha and toggles its particles by enabling the VFX; the visual waits for the player to leave
author: Matiabenavide
date: 2026-11-28
branch: feature/intangible-alpha-ghost
cost: { minutes: 12, tokens: 95668 }
id: l-3b91f4f4
scope: repo
status: active
src: agent
touches:
  - TheLastEngineer/.idea/.idea.TheLastEngineer/.idea/indexLayout.xml
  - TheLastEngineer/Assets/Art-2/Shaders/Shader_Graphs/DissolveShader.shadergraph
  - TheLastEngineer/Assets/Art-2/VFX/VFXGraph_RepelObject.vfx
  - TheLastEngineer/Assets/Prefabs/Glitcheables/Intangible/Intangible Container.prefab
  - TheLastEngineer/Assets/Scenes/TEST LEVELS/Mati/Mati_Testing.unity
  - TheLastEngineer/Assets/Scripts/Gameplay/Glitch/Intangible/Intangible.cs
  - TheLastEngineer/Assets/Scripts/Gameplay/Glitch/Intangible/IntangibleExitingState.cs
  - TheLastEngineer/Assets/Scripts/Gameplay/Glitch/Intangible/IntangibleGhost.cs
---

## Decided
- Intangible objects use one material: only its _Alpha is tweened (Clean = 1, Intangible = 0). The old two-slot setup (glitch distortion + overlay, inverted alpha) was dropped because the prefab now has a single material.
- Particles start when the fade to 0 begins and are cleared when the fade back to 1 ends; an interrupted fade keeps them.
- With the player still inside on the way back to Clean (Exiting), the whole visual (alpha, particles, ghost) stays phased until they leave, not only the layer.
- The ghost is a separate listener of an Intangible event, not something Intangible drives directly.

## Tried and ruled out
- vfx.Stop() to "stop emitting": the graph is a single burst with immortal particles, so Stop leaves them on screen.
- Stop() + Reinit() to clear: Reinit re-sends the initial event (OnPlay) and fires the burst again.

## Watch out
- Particles never die on their own yet; there is a TODO to add a particle fade in the VFX graph.
- If the Intangible's Vfx field is not assigned, the VFX keeps playing from OnPlay regardless of state.

In the code:
- Intangible.FadeVisual / SetParticles (toggles VisualEffect.enabled), event OnPhaseVisualChanged(bool), IsVisuallyPhased.
- IntangibleExitingState.Enter no longer fades; IntangibleCleanState applies it when the player leaves.
- IntangibleGhost: _targetAlpha while phased, 0 when clean; own _fadeTime/_ease.
- VFX: Assets/Art-2/VFX/VFXGraph_RepelObject.vfx (one burst spawner, no lifetime).
