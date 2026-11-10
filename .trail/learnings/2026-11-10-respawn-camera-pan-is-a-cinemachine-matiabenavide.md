---
title: Respawn camera pan is a Cinemachine extension that blends the frozen pose to the live one over a fixed time; the respawn waits on it and a level load cancels it
author: Matiabenavide
date: 2026-11-10
branch: fix/respawn-camera-pan
id: l-0d57347d
scope: repo
status: active
src: agent
touches:
  - TheLastEngineer/Assets/Art-2/Shaders/ShaderScript/GlitchDeathController.cs
  - TheLastEngineer/Assets/Scenes/TEST LEVELS/Mati/Mati_Testing.unity
  - TheLastEngineer/Assets/Scripts/Core/Utilities/FollowController.cs
  - TheLastEngineer/Assets/Scripts/Player/MVC/PlayerController.cs
---

## Decided
- The fixed-duration pan is a small CinemachineExtension on the main camera: it blends, at the Finalize stage, from the pose the camera froze at on death to the pose the pipeline computes now. No component values change.
- To end centered, the pan start sets the camera's PreviousStateIsValid to false: the rotation composer then snaps the player to the screen position (CenterOnActivate is on).
- Duration (1 s) and ease (PrimeTween InOutSine) are configurable on the follow controller. The pan starts after the existing 0.5 s wait after the teleport.
- The player controller owns the sequence: it waits on the camera pan, then releases the death effect, then the logical respawn (not dead, OnRespawned, inputs). No new singletons or events.
- The death effect holds at its exit level (0.5) from 80% of its curve until released; if released earlier it just plays its normal curve.
- A level load during the respawn stops the sequence (no OnRespawned, no inputs) and leaves camera and effect as they are.
- Verification is manual in Play Mode (Mati_Testing scene); no new tests.

## Tried and ruled out
- Brain blend with a second vcam: works but adds a GameObject and a custom blend to the prefab.
- Tweaking damping/dead zone at runtime: exponential, so no exact duration, a distance-dependent residual stays frozen as offset, and it touches the composer.
- CancelDamping(true): it updates with deltaTime < 0, which makes the orbital follow run its axis reset handler and could reset the yaw.
- Converting the death effect script to UTF-8, fixing its shared material and the jumps in its curve: out of scope.

## Watch out
- The extension must be added to "CM Camera" in the camera prefab by hand and assigned on the follow controller; without it the respawn cuts with no pan (an error is logged).
- Not verified whether the orbital follow's position damping also skips with PreviousStateIsValid = false; if the camera keeps sliding after the pan, warp the target instead.
- Debug checkpoint jumps (LevelSelector) start the respawn coroutine on themselves, so a level load can't cancel them.
- The death effect script is ISO-8859 + CRLF: edit it byte-safe.

In the code: Assets/Scripts/Core/Utilities/RespawnCameraPan.cs (Begin/End/Weight), FollowController.PanToTarget + _respawnPanDuration/_respawnPanEase/_respawnPan, PlayerController.RespawnPlayer/_respawnRoutine/CancelRespawn, GlitchDeathController.ReleaseHold (FadeStartProgress 0.8, ExitLevel 0.5). Branch fix/respawn-camera-pan.
