---
title: "A station's scanner effect hides when the platform actually leaves, not when it arrives"
author: Matiabenavide
date: 2026-11-21
branch: feature/station-scanner-effect
cost: { minutes: 11, tokens: 86961 }
id: l-5f567ee5
scope: repo
status: active
src: agent
touches:
  - TheLastEngineer/Assets/Art-2/AssetLucas/FBX/efecto_plataforma.fbx
  - TheLastEngineer/Assets/Art-2/AssetLucas/FBX/efecto_plataforma.fbx.meta
  - TheLastEngineer/Assets/Art-2/AssetsFer/FBXs/Reemplazar/Prefabs/Plataformas/Plataformas/GrupoReceptores.prefab
  - TheLastEngineer/Assets/Art-2/Shaders/Shader Materials/M_PlatformScanner.mat
  - TheLastEngineer/Assets/Art-2/Shaders/Shader Materials/M_PlatformStop.mat
  - TheLastEngineer/Assets/Art-2/Shaders/Shader Materials/M_PlatformStop.mat.meta
  - TheLastEngineer/Assets/Art-2/Shaders/Shader_Graphs/S_PlatformScanner.shadergraph
  - TheLastEngineer/Assets/Art/Shaders/Post-Process/Post_Process_Materials/M_PP_ToonPosterize.mat
  - TheLastEngineer/Assets/Scenes/BLOCKING/DrillRooms Juli.unity
  - TheLastEngineer/Assets/Art-2/AssetsFer/FBXs/Reemplazar/AssetsMantenimiento.fbx.meta
---

## Decided
- Each station's scanner effect shows (_Alpha to 1) when the platform starts heading there, the same moment its lights start blinking. When the platform arrives, its falloff widens (_Falloff to 1). The effect hides (both to 0) only when the platform really starts moving again after its wait.
- Power works the same way as the station lights: when power is cut, the target stays visible until the platform comes to rest, then everything hides. When power comes back while the platform rests on a station, that station shows with falloff 1.

## Tried and ruled out
- Hiding the station when the next target is announced: the platform raises "station reached" and "target changed" in the same frame on arrival, so the effect would hide right away and cut the falloff tween.
- Hiding when the falloff tween ends: the user wants the effect to stay while the platform waits at the station.

## Watch out
- The platform raises no arrival event for the station it starts on. When power comes on, the code finds the station under the platform by position.
- If the platform leaves for the same station it rests at (a route that heads back to it), the effect stays visible.

In the code: StationScannerEffect (Show/Arrive/Hide), RouteStationLight.Arrived/Departed, PlatformStationLights.HandleMovingChanged (departure = MovingChanged(true)) and StationUnderPlatform (RestingTolerance 0.01).
