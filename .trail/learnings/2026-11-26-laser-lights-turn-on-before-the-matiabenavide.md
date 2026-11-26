---
title: "Laser lights turn on before the beam and off after it, glitch toggles included, and interruptions reverse from the current value"
author: Matiabenavide
date: 2026-11-26
branch: main
cost: { minutes: 4, tokens: 66842 }
id: l-e2720c38
scope: repo
status: active
src: agent
---

## Decided
- Turning on: the laser's Light tweens up to a max intensity first; the beam starts only when the light finishes. Turning off: the beam retracts first, then the light tweens down to 0.
- Glitcheable lasers follow the same sequence when they disintegrate/move (beam, then light) and reintegrate (light, then beam), not only on LaserReceived/NotReceived.
- An interruption (on during off, or off during on) stops the running tween and reverses from the current intensity/length, with a duration proportional to what remains. If the beam never came out, only the light goes back down.
- Initial light: max intensity for lasers that start initialized, 0 for the rest; "off" always means 0.
- The beam's eased length moves to PrimeTween: the existing ease time stays the duration and a serialized Ease is added. In this project "type" means the Ease and "ease" means the duration.

## Tried and ruled out
- Letting the running sequence finish and then queuing the reverse: the user chose to reverse at once from the current point.
- Driving the light only from LaserReceived/NotReceived: glitch toggles would leave the light on with no beam.
- Using the light's editor intensity as the "off" value: the user chose 0.

## Watch out
- The receptor must not be fed while the light ramps up: _isInitialized stays false until the light tween completes.
- Reintegrating -> Idle resets the idle setup, so starting the light must do nothing if it is already at max or going up.
- The beam tween targets the plain C# model, so PrimeTween will not stop it when the laser is destroyed: stop it in OnDestroy.

In the code: Assets/Scripts/Gameplay/Laser/MVC/LaserController.cs (LaserReceived -> StartLaser -> old body on light OnComplete; LaserNotReceived -> beam retract -> light off; IdleBehaviour/TransitioningBehaviour), Assets/Scripts/Gameplay/Laser/MVC/LaserModel.cs (SetLaserLength returns a Tween.Custom on CurrentDist; IsTransitioning = tween.isAlive). Legacy Laser.cs is out of scope.
