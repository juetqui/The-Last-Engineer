---
title: The falloff scanner rises to 1 as soon as the platform targets its station and holds until the platform leaves
author: Matiabenavide
date: 2026-11-22
branch: feature/station-scanner-effect
cost: { minutes: 19, tokens: 27239 }
id: l-7ba30c24
scope: repo
status: active
replaces: l-d0a41a7b
src: agent
touches:
  - TheLastEngineer/Assets/Scripts/Gameplay/Platform/StationFalloffScannerEffect.cs
---

## Decided
- Every scanner stays visible (_Alpha untouched) and fades _Color to the station's blink color while targeted, back to its own color otherwise. The falloff scanner is a subclass that adds _Falloff.
- _Falloff ping-pongs 0-1 while the station is idle. When the platform targets the station: if the falloff is rising it finishes that rise with the same pace and ease and stops at 1; if it is falling it turns back right away (the user's choice) and rises at the ping-pong speed. It holds 1 while the platform waits and resumes the ping-pong only when the platform moves again.
- If the platform arrives before the falloff reaches 1, the rise continues at its own pace; the arrive tween is only for resting without having been targeted (power back with the platform already at the station).

## Watch out
- Finishing the rise uses SetRemainingCycles(stopAtEndValue: true) on the running yoyo; with yoyo from 0, an even cyclesDone means it is rising. The first leg down to 0 is a separate tween, so it counts as falling.
- Turning back with a new tween restarts the ease curve, so an InOut ease shows a slight slowdown at the turn.

In the code: StationFalloffScannerEffect (FalloffMode, RiseToMax, StartPingPong, IsFixedAtMax), StationScannerEffect.SetState.
