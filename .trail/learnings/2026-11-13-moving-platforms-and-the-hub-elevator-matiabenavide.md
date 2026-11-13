---
title: "Moving platforms and the HUB elevator stay in Update and write the Rigidbody pose every frame, not FixedUpdate with interpolation"
author: Matiabenavide
date: 2026-11-13
branch: main
cost: { minutes: 19, tokens: 188605 }
id: l-d9c41d22
scope: repo
status: active
src: agent
touches:
  - TheLastEngineer/doc/plans/platform-player-stutter-plan.md
---

## Decided
- Moving platforms keep moving in Update, run before the player (named execution-order constant, -50, earlier than Glitcheable's -1 and the player's 0), and each frame write rb.position (the physics pose updates immediately, so the CharacterController.Move in the same frame sees it) plus transform.position (visual). Kinematic Rigidbody, no interpolation. This is an explicit, documented exception to the "physics in FixedUpdate / never write transform" rule.
- HUB elevator: same approach. Kinematic Rigidbody on the root, Animator stays in Normal update mode, and AttachPlayer copies the pose to rb.position in LateUpdate and always sends the delta (no 1 cm threshold).
- If an instance has no Rigidbody, the code adds a kinematic one in Awake with a warning (same as GlitchTransferHandler being added at runtime). The prefabs get it in the Editor too.
- Tests: pure step/arrival/accumulator logic moves to a Platform.Rules asmdef with EditMode tests. PlayMode tests use a harness (test CharacterController + test driver), because test asmdefs can't reference Assembly-CSharp.
- Only the player rides platforms; no dynamic rigidbodies are carried.

## Tried and ruled out
- FixedUpdate + MovePosition + Interpolate: the physics pose runs up to one 20 ms step ahead of the visual one. The player's CharacterController (Update) then sinks into or floats above the floor on vertical platforms, so isGrounded keeps flickering.
- Elevator with Animator Animate Physics + Interpolate: same desync, and the elevator is vertical, the worst case.
- Moving Player and Platform code into an asmdef just to test them: too big a refactor.

## Watch out
- Glitcheable Platform Container: once the anchor has a Rigidbody, the GlitcheablePlatform's trigger joins its compound and can send OnTriggerExit to PlatformController. The prefab isn't in any scene yet.
- The elevator player lags one frame visually (delta measured in LateUpdate, applied next Update).

In the code:
- Plan: doc/plans/platform-player-stutter-plan.md
- PlatformMotor.MoveTowards (transform.position +=), PlayerModel.OnPlatformMoving, Gameplay/AttachPlayer.cs, PlatformStateMachine.cs:19 (starts with null state, left as follow-up)
