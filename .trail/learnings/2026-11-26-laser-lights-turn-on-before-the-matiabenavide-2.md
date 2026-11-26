---
title: "Laser lights turn on before the beam and off after it, glitch toggles included, and interruptions reverse from the current value"
author: Matiabenavide
date: 2026-11-26
branch: main
cost: { minutes: 5, tokens: 3302 }
id: l-c38f39d5
scope: repo
status: active
replaces: l-e2720c38
src: agent
---

## Decided
- On: the laser's Light tweens (PrimeTween) up to a max intensity; the beam starts only when it finishes. Off: the beam retracts first, then the light tweens to 0.
- Glitcheable lasers follow the same order on disintegrate/move (beam, then light) and reintegrate (light, then beam).
- An interruption stops the running step and reverses from the current intensity/length; if the beam never came out, only the light goes down.
- Initial light: max for lasers that start initialized, 0 otherwise. In this project "type" = Ease and "ease" = duration.

## Tried and ruled out
- Moving the beam's length ease to PrimeTween: the user dropped it; the model keeps its own per-frame lerp.
- Queuing the reverse until the running sequence ends, and using the editor intensity as "off": the user chose otherwise.

## Watch out
- Keep _isInitialized false while the light ramps up so receptors aren't fed early.
- The end of the beam retract is detected by polling the model's IsTransitioning, so trigger the light-off once (flag), not every frame.

In the code: Assets/Scripts/Gameplay/Laser/MVC/LaserController.cs (LaserReceived -> StartLaser -> old body on light OnComplete; IdleBehaviour/TransitioningBehaviour/CheckCommonBehaviour).
