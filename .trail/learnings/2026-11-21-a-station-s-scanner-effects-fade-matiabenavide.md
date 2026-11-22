---
title: "A station's scanner effects fade in when a platform heads there and fade out only when it actually leaves"
author: Matiabenavide
date: 2026-11-21
branch: feature/station-scanner-effect
cost: { minutes: 5, tokens: 18995 }
id: l-aeadfd9b
scope: repo
status: active
replaces: l-5f567ee5
src: agent
touches:
  - TheLastEngineer/Assets/Scripts/Gameplay/Platform/StationScannerEffect.cs
  - TheLastEngineer/Assets/Scripts/Gameplay/Platform/RouteStationLight.cs
---

## Decided
- A station can have several scanner effects as children, and all of them get the same treatment. Each one only animates _Alpha. Alpha starts at 0 and goes to 1 when the platform starts heading to the station, the same moment its lights start blinking. It goes back to 0 only when the platform really starts moving again after waiting at the station.
- Power works the same way as the station lights: when power is cut, the target stays visible until the platform comes to rest, then everything hides. When power comes back while the platform rests on a station, that station shows again.

## Tried and ruled out
- Widening _Falloff to 1 on arrival: built first, then dropped by the user. The effect no longer uses falloff.
- A single serialized scanner reference: replaced by collecting every scanner among the station's children.
- Hiding the effect when the next target is announced: the platform raises "station reached" and "target changed" in the same frame on arrival, so the effect would hide while the platform is still waiting at the station.

## Watch out
- The platform raises no arrival event for the station it starts on. When power comes on, the code finds the station under the platform by position.
- If the platform leaves for the same station it rests at, the effect stays visible.
- Scanner children have a Renderer, so the station's light collection must skip them. Otherwise they would get the emissive color and throw off the count of 4 lights.

In the code: StationScannerEffect (Show/Hide on _Alpha), RouteStationLight._scanners (GetComponentsInChildren in Awake), Arrived/Departed, PlatformStationLights.HandleMovingChanged (departure = MovingChanged(true)) and StationUnderPlatform (RestingTolerance 0.01).
