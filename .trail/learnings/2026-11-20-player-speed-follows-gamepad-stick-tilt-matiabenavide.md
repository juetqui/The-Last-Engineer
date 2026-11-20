---
title: Player speed follows gamepad stick tilt through a designer curve with a minimum speed; keyboard stays at full speed
author: Matiabenavide
date: 2026-11-20
branch: main
cost: { minutes: 2, tokens: 31138 }
id: l-08e3448b
scope: repo
status: active
src: agent
---

## Decided
- Horizontal speed = moveSpeed x fraction, where fraction = max(minFraction, curve(tilt)) and tilt = clamp01(stick magnitude); 0 tilt = 0 speed. Curve and minimum are tuning data in PlayerData, starting linear and ~0.3.
- A minimum speed applies as soon as the stick leaves the deadzone, so a small tilt doesn't crawl.
- Keyboard (WASD) keeps magnitude 1 = full speed; no walk key.
- Animation uses the new Idle/Walk/Run blend tree (float "Blend": 0 / 0.5 / 1), fed with the speed fraction plus damping.
- Cinematic movement and dash stay at full speed (cinematic direction is already normalized).

## Tried and ruled out
- Pure linear (no data): too rigid for tuning feel without code changes.
- Two walk/run steps: the user preferred a continuous curve.
- Scaling the Walk animation speed: dropped because the animator now has a blend tree.
- A walk key on keyboard: not wanted.

## Watch out
- The magnitude is lost where PlayerModel normalizes the camera-relative direction; keep the direction normalized for rotation but read tilt before.
- With minimum 0.3 the blend sits between Idle and Walk; raise the minimum or move the Walk threshold so feet don't slide.
- The old Idle/Run states with IsWalking transitions are orphaned in the controller.

In the code: Assets/Scripts/Player/MVC/PlayerModel.cs (MovePlayer, GetHorizontalMovement), Assets/Scripts/Player/MVC/PlayerView.cs (Walk), Assets/Scripts/Scriptable Objects/PlayerData.cs, Assets/Art/Animations/Controller/Player/AC Player.controller. Pure logic planned in a Player.Rules asmdef so EditMode tests can reach it.
