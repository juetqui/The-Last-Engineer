---
title: Laser sound fades volume and pitch in and out with the light instead of starting and stopping every frame
author: Matiabenavide
date: 2026-11-26
branch: feature/laser-sound-fade
cost: { minutes: 10, tokens: 74202 }
id: l-cdf42769
scope: repo
status: active
src: agent
touches:
  - TheLastEngineer/Assets/Scripts/Gameplay/Laser/MVC/LaserView.cs
  - TheLastEngineer/Assets/Scripts/Gameplay/Laser/MVC/LaserController.cs
---

## Decided
- The laser's AudioSource is driven only by fades: on, volume and pitch tween (PrimeTween) from their current values to a max volume and a pitch picked at random in a range; off, both go to 0 and the source stops when the fade completes. Duration scales with the volume left, like the light.
- The sound rises together with the light (not after it, when the beam starts), on LaserReceived, at Start for lasers that start on, and when a glitched laser reintegrates. It falls on LaserNotReceived and when a glitched laser starts moving.
- The user chose pitch 0 on turn-off (the sound "winds down"), the random pitch as the on target, and the fade for glitch toggles too.

## Tried and ruled out
- Keeping a per-frame PlayAudio(): it restarts a stopped source every frame and would cut fades. Removed.
- Calling SetSound from IdleBehaviour/TransitioningBehaviour: those run every frame; it is called from the FSM state-change event, only on the edge.

## Watch out
- Reintegrating -> Idle both count as idle: only the edge into idle fades in, or the fade restarts with a new pitch.
- The AudioSource's Inspector volume is no longer used; LaserView's sound max volume is.

In the code: LaserView.SetSound(bool), LaserController.StartLaser / LaserNotReceived / UpdateGlitchedBehaviour / Start.
