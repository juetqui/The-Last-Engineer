---
title: "Respawn camera pans to the player with inputs and the death effect held until the pan ends; root cause is the rotation composer's stale orientation"
author: Matiabenavide
date: 2026-11-10
branch: main
cost: { minutes: 1, tokens: 2424 }
id: l-64f99075
scope: repo
status: active
src: agent
---

## Decided
- Root cause of the camera offset after respawn: on death the camera stops following and, 1 s later, stops looking, so its rotation freezes pointing at the death spot. The player is then teleported without telling Cinemachine. On respawn the rotation composer starts from that stale orientation and only turns until the player is inside its large dead zone, so the framing stays off for good.
- Desired behavior (user's choice): keep the camera freezing on death; on respawn the camera does a smooth pan of fixed, configurable duration back to the player and ends centered.
- The whole logical respawn (no longer dead, OnRespawned, inputs back on) must wait until the pan ends; the camera needs a separate signal to start panning.
- The death post-process must stay at the level its fade-out starts from (0.5) while the camera pans, and fade out over 0.4 s (same as the original last segment) once the pan ends.
- The rotation composer values in the camera prefab must not change. Scope is only the main camera prefab, not the test copy.

## Tried and ruled out
- Instant cut on respawn (invalidating the camera's previous state with no pan): user wants a smooth pan.
- Letting the existing position damping decide the pan length: it is distance-dependent and slow (several seconds for long distances), so a fixed duration was chosen.
- Holding the post-process at its peak during the pan: it would hide the pan.
- Delaying only the inputs and firing OnRespawned as before: rejected; everything waits for the pan.
- Retuning the composer's dead zone: ruled out to keep gameplay framing.

## Watch out
- CenterOnActivate does not help: the virtual camera never deactivates, only its targets change.
- The death post-process is a fixed time curve unaware of the camera; it writes to a shared material asset, and its source file uses a legacy (non-UTF-8) encoding.

In the code:
- Assets/Scripts/Core/Utilities/FollowController.cs (StopFollowing/StopLooking/StartFollowing)
- Assets/Scripts/Player/MVC/PlayerController.cs RespawnPlayer, Assets/Scripts/Player/MVC/PlayerModel.cs SetRespawnPos
- Assets/Prefabs/Level/CameraContainer.prefab "CM Camera": RotationComposer DeadZone 0.6x0.7, HardLimits 0.8x1, Damping 0.25; OrbitalFollow WorldSpace, PositionDamping (3,1.5,1.5)
- Assets/Art-2/Shaders/ShaderScript/GlitchDeathController.cs (glitchDuration 2 s; fade segment 80-100%)
