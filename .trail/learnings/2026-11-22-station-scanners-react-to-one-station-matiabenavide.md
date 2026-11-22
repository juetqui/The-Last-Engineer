---
title: Station scanners react to one station state; the falloff scanner is always visible and ping-pongs until the platform arrives
author: Matiabenavide
date: 2026-11-22
branch: feature/station-scanner-effect
cost: { minutes: 9, tokens: 85281 }
id: l-d0ebb512
scope: repo
status: active
replaces: l-6053cd9a
src: agent
touches:
  - TheLastEngineer/Assets/Scripts/Gameplay/Platform/StationScanState.cs
  - TheLastEngineer/Assets/Scripts/Gameplay/Platform/IStationScanner.cs
  - TheLastEngineer/Assets/Scripts/Gameplay/Platform/StationScannerEffect.cs
  - TheLastEngineer/Assets/Scripts/Gameplay/Platform/StationFalloffScannerEffect.cs
  - TheLastEngineer/Assets/Scripts/Gameplay/Platform/RouteStationLight.cs
---

## Decided
- The station tells each scanner a single state: Idle (powered, not involved), Targeted (the platform heads here), Resting (it arrived or rests here) or Unpowered (the lights are off). Each scanner kind decides how it looks in each state.
- The plain scanner keeps fading _Alpha: shown while Targeted or Resting, hidden otherwise.
- The falloff scanner no longer touches _Alpha; it is always visible. _Falloff ping-pongs 0 to 1 while Idle or Targeted, goes to 1 on arrival and stays until the platform leaves, and goes to a value set in the Inspector while unpowered. While Targeted, _Color fades to the station's blink color; it fades back to the material's own color on arrival (the user chose arrival, not departure).
- Targeted after Idle keeps the running ping-pong instead of restarting it.

## Tried and ruled out
- Keeping the falloff scanner as a subclass of the alpha scanner: it would have to override the alpha methods to do nothing, so both now implement a small interface instead.

## Watch out
- Arrival and "target changed" happen in the same frame, so "goes back when the target changes" means "goes back on arrival".
- When power comes back while the platform rests at a station, that station first gets Idle (TurnOn) and right after Resting (Arrived); the last state wins.
- Interface references skip Unity's destroyed-object check, so the station checks the component as a UnityEngine.Object before calling it.

In the code: StationScanState, IStationScanner.SetState, StationScannerEffect, StationFalloffScannerEffect (StartPingPong / PingPongLoop / _unpoweredFalloff), RouteStationLight.SetScannerState (TurnOn=Idle, StartBlinking=Targeted, Arrived=Resting, Departed=Idle, TurnOff=Unpowered).
