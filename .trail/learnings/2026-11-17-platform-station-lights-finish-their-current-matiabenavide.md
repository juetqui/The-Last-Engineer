---
title: Platform station lights finish their current range cycle and rest at the minimum when they stop animating
author: Matiabenavide
date: 2026-11-17
branch: main
cost: { minutes: 3, tokens: 59710 }
id: l-56738079
scope: repo
status: active
src: agent
touches:
  - TheLastEngineer/Assets/Editor/LightMovementEditor.cs
---

## Decided
- Station lights get a LightMovement subclass with two serialized bools: desync (per-instance offset/duration variation vs. coordinated) and animate.
- animate = true while the station blinks (blink color); false on the on color AND on turn-off (black).
- When animate goes false, the oscillation keeps its ease/speed until it reaches the min range, then stops there (no snap, no restore of the original range).
- "Coordinated" means in phase within one station (offset 0, no duration variation, all start together on blink) - not a global clock across stations.
- The base LightMovement keeps its behavior: it is also used by laser containers and the crystal light.
- The user swaps the script on the station prefab's 4 lights in the Editor; code changes don't hand-edit that prefab.

## Tried and ruled out
- Direct short tween to min, or snapping to min: the user wants the cycle to finish naturally.
- Leaving animate untouched on turn-off: a mid-blink power-off would keep oscillating in black.
- Global shared clock for coordinated lights: in-station sync is enough.
- Enabling/disabling the component (previous approach): OnDisable restores the original range instead of resting at min.

## Watch out
- Stopping can take up to almost a full cycle if the light is on its way up.
- The custom editor needs editorForChildClasses so the subclass keeps the Scene View preview.

In the code: Assets/Scripts/Utilities/LightMovement.cs, Assets/Scripts/Gameplay/Platform/RouteStationLight.cs (SetMoving/FadeTo/StartBlinking), Assets/Editor/LightMovementEditor.cs, GrupoReceptores.prefab. Plan to stop at min: PrimeTween SetRemainingCycles(stopAtEndValue: false), fallback compute remaining time from yoyo cycle parity.
