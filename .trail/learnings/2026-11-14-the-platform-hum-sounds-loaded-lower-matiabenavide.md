---
title: The platform hum sounds loaded (lower pitch plus low-pass) only while the player rides a moving platform
author: Matiabenavide
date: 2026-11-14
branch: feature/platform-loop-audio
cost: { minutes: 16, tokens: 43213 }
id: l-9105efec
scope: repo
status: active
src: agent
touches:
  - TheLastEngineer/Assets/Scripts/Gameplay/Platform/PlatformController.cs
  - TheLastEngineer/Assets/Scripts/Gameplay/Platform/StateMachine/InactiveState.cs
  - TheLastEngineer/Assets/Scripts/Gameplay/Platform/StateMachine/WaitingState.cs
  - TheLastEngineer/Assets/Scripts/Gameplay/Platform/StateMachine/MovingState.cs
  - TheLastEngineer/Assets/Scripts/Gameplay/Platform/Rules/PlatformHumRules.cs
  - TheLastEngineer/Assets/Tests/EditMode/PlatformHumRulesTests.cs
  - TheLastEngineer/Assets/Scripts/Gameplay/Platform/PlatformAudio.cs
  - TheLastEngineer/.claude/commands/run-tests.md
---

## Decided
- Riding changes the hum with pitch and a low-pass filter together, and only while the platform travels (Moving, and ToStop while it decelerates). Boarding a platform stopped at a station or Inactive changes nothing until it starts moving, and the sound goes back to normal at each station stop.
- The cutoff sweeps on a log scale, so the filter change sounds even.
- The landing one-shot plays through the SFX manager only when the player lands on a moving platform. Its clip field stays empty until the sound exists; empty means silence, not an error.

## Watch out
- The platform raises "passenger off" from its own OnDestroy too, so the audio handlers ignore it once the component is disabled.

In the code:
- PlatformController.PassengerChanged / MovingChanged / NotifyMoving (Moving, Waiting and Inactive Enter)
- PlatformHumRules.IsLoaded / LoadedPitch / LoadedCutoff; PlatformAudio adds an AudioLowPassFilter
- Defaults: pitch 1 to 0.92, cutoff 22000 to 3000 Hz, Q 1, load-in 0.35 s OutSine, load-out 0.6 s InOutSine, landing volume 0.8
