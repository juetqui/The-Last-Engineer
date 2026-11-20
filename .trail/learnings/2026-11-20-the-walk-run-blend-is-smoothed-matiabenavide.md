---
title: "The walk/run blend is smoothed by our own damper that lands exactly on idle, because the Animator's damping never reaches zero and footstep events kept firing"
author: Matiabenavide
date: 2026-11-20
branch: feature/analog-move-speed
cost: { minutes: 1, tokens: 1891 }
id: l-5d5a11ce
scope: repo
status: active
src: agent
---

## Decided
- The Idle/Walk/Run blend parameter is smoothed in plain C# (exponential approach, frame-rate independent) that snaps onto the target when closer than 0.001, then set on the Animator without damping.
- Movement tests load the tuning (curve, minimum fraction, damp time) from the PlayerData asset through SerializedObject instead of hardcoding numbers, so retuning the asset is covered automatically.

## Tried and ruled out
- Animator.SetFloat(id, value, dampTime, deltaTime): it only approaches the target asymptotically. After the first stick press, releasing left the blend a hair above 0, the Walk clip kept a tiny weight in the blend tree, and its WalkSound animation events kept firing while standing still.

## Watch out
- Blend-tree children fire their animation events whenever their weight is above zero, however small; any blend parameter that must "turn off" a clip has to land exactly on the threshold.
- The test assembly can't reference Assembly-CSharp (PlayerData lives there), hence the SerializedObject lookup by field name: renaming those serialized fields breaks the tests with a clear message.

In the code: Assets/Scripts/Player/Rules/MoveBlendDamper.cs (Step), PlayerView.Walk, Assets/Tests/EditMode/PlayerDataValues.cs, MoveBlendDamperTests.cs. Blend parameter "Blend": Idle 0, Walk 0.5, Run 1.
