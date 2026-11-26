---
title: "Laser sound fades volume and pitch in and out with the light, tuned globally in LaserData"
author: Matiabenavide
date: 2026-11-26
branch: feature/laser-sound-fade
cost: { minutes: 1, tokens: 1816 }
id: l-cbe2d858
scope: repo
status: active
replaces: l-cdf42769
src: agent
---

## Decided
- The laser's AudioSource is driven only by fades: on, volume and pitch tween (PrimeTween) from their current values to a max volume and a pitch picked at random in a range; off, both go to 0 and the source stops when the fade completes. Duration scales with the volume left, like the light.
- The fade targets (max volume, pitch range, ease time, ease type) live in the global LaserData (Resources), not per laser: the user chose to extend LaserData instead of a new per-laser LaserConfig, and tunes them there.
- The sound rises together with the light (not after it, when the beam starts), on LaserReceived, at Start for lasers that start on, and when a glitched laser reintegrates. It falls on LaserNotReceived and when a glitched laser starts moving.
- The user chose pitch 0 on turn-off (the sound "winds down"), the random pitch as the on target, and the fade for glitch toggles too.

## Tried and ruled out
- Keeping a per-frame PlayAudio(): it restarts a stopped source every frame and would cut fades. Removed.
- Calling SetSound from IdleBehaviour/TransitioningBehaviour: those run every frame; it is called from the FSM state-change event, only on the edge.
- A per-laser LaserConfig ScriptableObject for the sound: the user preferred one global place to tune it. Only the sound moved; the other laser parameters were left serialized per laser because prefabs use different values for them.

## Watch out
- Reintegrating -> Idle both count as idle: only the edge into idle fades in, or the fade restarts with a new pitch.
- Each AudioSource's Inspector volume is ignored; LaserData's sound max volume is the one that counts.

In the code: LaserData (Sound header), LaserView.SetSound(bool), LaserController.StartLaser / LaserNotReceived / UpdateGlitchedBehaviour / Start.
