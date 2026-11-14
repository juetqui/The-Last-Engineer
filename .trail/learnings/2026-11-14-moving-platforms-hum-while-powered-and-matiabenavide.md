---
title: Moving platforms hum while powered and fade out only once they come to rest unpowered
author: Matiabenavide
date: 2026-11-14
branch: feature/platform-loop-audio
cost: { minutes: 3, tokens: 40069 }
id: l-b4925a59
scope: repo
status: active
src: agent
touches:
  - TheLastEngineer/Assets/Scripts/Gameplay/Platform/Rules/PlatformHumRules.cs
  - TheLastEngineer/Assets/Tests/EditMode/PlatformHumRulesTests.cs
  - TheLastEngineer/Assets/Scripts/Gameplay/Platform/StateMachine/InactiveState.cs
  - TheLastEngineer/Assets/Scripts/Gameplay/Platform/PlatformController.cs
  - TheLastEngineer/Assets/Scripts/Gameplay/Platform/PlatformAudio.cs
---

## Decided
- The platform loop starts (fade-in) when a correct connection powers the platform and keeps playing while connected, even when it waits at a station. On disconnect it keeps sounding while the platform decelerates; the fade-out starts only when the platform enters Inactive, then the AudioSource stops.
- The audio is a separate component that listens to two platform events (powered on / became inactive). The platform controller never plays audio. Decisions are a pure transition table so EditMode tests can cover them (test asmdefs can't reference Assembly-CSharp).
- Reconnecting during the fade-out fades back up from the current volume without restarting the clip; the fade duration scales with the remaining distance.
- Every prefab with a platform controller gets the component, plus the loose one in the platforms level, including the Glitcheable platform container that isn't in any scene yet.
- Pause keeps the loop playing (the pause menu only sets timeScale), scene change just cuts it (the black fade hides it), and player death does nothing.
- Defaults: volume 0.6, fade-in 0.8 s OutSine, fade-out 1.2 s InSine, full 3D, logarithmic 2-20 m, no Doppler, priority 160 so one-shot SFX win voices.

## Tried and ruled out
- Playing the loop through the SFX manager: it only does one-shots.
- Pausing the loop or all audio on pause, fading out during scene loads, ducking on death: they widen the scope beyond the platforms.
- Restarting the clip with a full fade-in on reconnect: it sounds like a new start instead of a continuation.
- Only adding the sound to the prefabs already used in scenes: the user wanted every platform to sound the same.

## Watch out
- A connection's "starts connected" flag is still false when the platform initializes (it is set after the init event). Plugged-at-start platforms power on through the normal connect event, which is also what gives them the load fade-in. Don't rely on that flag.
- The platform state machine is built with a null state (it transitions before creating the states), so it never enters Inactive at startup. The hum rules ignore an inactive signal while silent, so fixing that bug is safe.

In the code:
- Assets/Scripts/Gameplay/Platform/PlatformAudio.cs, Rules/PlatformHumRules.cs, Tests/EditMode/PlatformHumRulesTests.cs
- PlatformController.PoweredOn / BecameInactive / NotifyInactive; InactiveState.Enter calls NotifyInactive
- Clip moving_platform_loop.wav (GUID 95a67d0d77f855241be5eeeea9f8ace1): Decompress On Load, Vorbis, preload on, force to mono
- Connection.Start fires OnInitialized before StartsConnected is set; PlatformStateMachine.cs:19
