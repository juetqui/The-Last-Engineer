---
title: "Moving platforms will get a kinematic Rigidbody to fix the player stutter, not a transform sync"
author: Matiabenavide
date: 2026-11-13
branch: main
cost: { minutes: 4, tokens: 55436 }
id: l-cbe643d5
scope: repo
status: active
src: agent
touches:
  - TheLastEngineer/doc/diagnostics/platform-player-stutter-plan-prompt.md
---

## Decided
The fix for the player stuttering on moving platforms will give the platforms a kinematic Rigidbody and move them through the Rigidbody API, instead of writing transform.position. The user chose this knowing it means editing the platform prefabs in the Editor. The stutter shows on horizontal and vertical platforms and in builds too, not only in the Editor. Scope: the physics fix, platform-before-player update order, the minor arrival/StopPassenger bugs, the HUB elevator (AttachPlayer) and PlayMode tests.

## Tried and ruled out
- Physics.SyncTransforms() after each platform move (code only, no prefab edits): rejected as the main fix. It hides the problem and the rules forbid moving physics bodies by writing transform.
- Turning on autoSyncTransforms globally: too costly for the whole project.

## Watch out
- Glitcheable Platform Container also has Glitcheable ([DefaultExecutionOrder(-1)], its own position list, toggles colliders). Check how that fits with the Rigidbody and the new execution order.
- Save/load may restore platform positions through transform; with a Rigidbody that needs rb.position or a sync.
- Some quality levels have vSync on and some off, so the fps vs fixed-timestep ratio changes. Test both.

In the code:
- Diagnosis: doc/diagnostics/platform-player-stutter.md; prompt for the planning session: doc/diagnostics/platform-player-stutter-plan-prompt.md; plan goes to doc/plans/platform-player-stutter-plan.md
- PlatformMotor.MoveTowards (transform.position += move), PlayerModel.OnPlatformMoving, PlatformController.StopPassenger (inverted check)
- ProjectSettings/DynamicsManager.asset m_AutoSyncTransforms: 0; Fixed Timestep 0.02
- PlatformController script GUID 6689ebcc1ca873f48958fcb7a88b9bd5 (6 prefabs, scene 4 - NIVEL PLATAFORMAS)
