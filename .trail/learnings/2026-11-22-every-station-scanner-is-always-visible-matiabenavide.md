---
title: Every station scanner is always visible and tints to the blink color while targeted; the falloff one is a subclass
author: Matiabenavide
date: 2026-11-22
branch: feature/station-scanner-effect
cost: { minutes: 8, tokens: 20139 }
id: l-d0a41a7b
scope: repo
status: active
replaces: l-d0ebb512
src: agent
touches:
  - TheLastEngineer/Assets/Scripts/Gameplay/Platform/StationScannerEffect.cs
  - TheLastEngineer/Assets/Scripts/Gameplay/Platform/StationFalloffScannerEffect.cs
  - TheLastEngineer/Assets/Scripts/Gameplay/Platform/RouteStationLight.cs
---

## Decided
- No scanner touches _Alpha any more; the materials leave it at 1.
- The base scanner owns the color: it fades _Color to the station's blink color while the platform heads there, and back to the material's own color on arrival, departure or power loss (a single fade, not a pulse).
- The falloff scanner inherits from it and only adds _Falloff (ping-pong until arrival, 1 while resting, a set value unpowered), calling the base first.

## Tried and ruled out
- A shared interface for both scanners: dropped once both needed the same color logic. GetComponentsInChildren of the base class already finds the subclass, so the interface added nothing to the lookup.

## Watch out
- The color settings live in the base class with the same field names they had in the falloff scanner, so existing prefab values carry over.

In the code: StationScannerEffect.SetState (virtual), StationFalloffScannerEffect.SetState override, RouteStationLight.SetScannerState.
