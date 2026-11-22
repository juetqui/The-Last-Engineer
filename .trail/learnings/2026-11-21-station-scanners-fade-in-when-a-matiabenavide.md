---
title: Station scanners fade in when a platform heads there and out when it leaves; falloff is an opt-in subclass
author: Matiabenavide
date: 2026-11-21
branch: feature/station-scanner-effect
cost: { minutes: 10, tokens: 13097 }
id: l-6053cd9a
scope: repo
status: active
replaces: l-aeadfd9b
src: agent
touches:
  - TheLastEngineer/Assets/Scripts/Gameplay/Platform/StationScannerEffect.cs
  - TheLastEngineer/Assets/Scripts/Gameplay/Platform/StationFalloffScannerEffect.cs
  - TheLastEngineer/Assets/Scripts/Gameplay/Platform/RouteStationLight.cs
---

## Decided
- Every scanner among a station's children fades _Alpha to 1 when the platform starts heading there. It fades back to 0 only when the platform really leaves, after its wait. Leaving is the platform starting to move, not the next target being announced.
- Falloff is opt-in. The falloff subclass also widens _Falloff to 1 on arrival and takes it to 0 together with _Alpha. The base scanner ignores falloff.

## Tried and ruled out
- Falloff in the base scanner: dropped by the user, then brought back as a separate subclass.
- Checking the scanner's type in the station: arrival is a virtual Arrive() instead. The base version only makes sure the effect is shown.

## Watch out
- The platform raises no arrival event for the station it starts on. When power comes on, the code finds it by position.
- Scanner children have a Renderer, so the station's light collection skips them.

In the code: StationScannerEffect (Show/Arrive/Hide), StationFalloffScannerEffect, RouteStationLight._scanners, PlatformStationLights.HandleMovingChanged / StationUnderPlatform.
