---
title: The platform hum sounds loaded (lower pitch plus low-pass) from the moment the player gets on until they get off
author: Matiabenavide
date: 2026-11-14
branch: feature/platform-loop-audio
cost: { minutes: 2, tokens: 11637 }
id: l-e36bfc16
scope: repo
status: active
replaces: l-9105efec
src: agent
touches:
  - TheLastEngineer/Assets/Scripts/Gameplay/Platform/Rules/PlatformHumRules.cs
  - TheLastEngineer/Assets/Tests/EditMode/PlatformHumRulesTests.cs
  - TheLastEngineer/Assets/Scripts/Gameplay/Platform/PlatformAudio.cs
---

## Decided
- Riding changes the hum with pitch and a low-pass filter together, held from the moment the player gets on until they get off, whether the platform moves, waits at a station or is off. The user corrected an earlier version that only applied it while the platform travelled.
- The cutoff sweeps on a log scale so the filter change sounds even.
- The landing one-shot plays through the SFX manager only when the player lands on a moving platform. Its clip field stays empty until the sound exists; empty means silence, not an error.

## Tried and ruled out
- Loaded only while the platform moves (reset at every station stop): the user wants the loaded sound for the whole ride.

## Watch out
- The platform raises "passenger off" from its own OnDestroy too, so the audio handlers ignore it once the component is disabled.

In the code:
- PlatformController.PassengerChanged; MovingChanged / NotifyMoving now only gate the landing sound
- PlatformHumRules.LoadedPitch / LoadedCutoff; PlatformAudio adds an AudioLowPassFilter
- Defaults: pitch 1 to 0.92, cutoff 22000 to 3000 Hz, Q 1, load-in 0.35 s OutSine, load-out 0.6 s InOutSine, landing volume 0.8
