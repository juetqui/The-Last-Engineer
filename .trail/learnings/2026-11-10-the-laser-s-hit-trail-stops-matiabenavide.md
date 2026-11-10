---
title: "The laser's hit trail stops emitting whenever the beam length changes or there is no hit, and comes back cleared once the length is stable"
author: Matiabenavide
date: 2026-11-10
branch: feature/laser-trail-toggle
cost: { minutes: 12, tokens: 72588 }
id: l-c17aef60
scope: repo
status: active
src: agent
touches:
  - TheLastEngineer/Assets/Scripts/Gameplay/Laser/MVC/LaserView.cs
  - TheLastEngineer/Assets/Scripts/Gameplay/Laser/MVC/LaserController.cs
---

## Decided
- The trail is switched from the laser view, called by the controller (keeps the MVC split).
- "Off" means stop emitting, so what is already drawn fades out; turning it on first clears old points so no streak is drawn from the previous position.
- Any change in beam length counts, eased transitions and instant jumps alike (something entering or leaving the beam). The trail comes back on the first frame the length repeats, with no threshold.
- No hit means no trail. Glitcheable lasers keep it off through Disintegrating/Moving and turn it on after re-extending in Idle.

## Tried and ruled out
- Turning the trail on at the beam tip when nothing is hit: the Trail object is a child of the hit-effect root, which is deactivated when there is no hit. Keeping that root active or reparenting the Trail in the prefab were offered and dropped to avoid changing prefabs and hit-effect behaviour.
- A configurable change threshold: the user chose "any change"; revisit if the trail flickers on rotating lasers or moving surfaces.

## Watch out
- With a zero threshold, a rotating laser or a moving object on the beam keeps the trail off almost all the time.

In the code:
- LaserView.SetTrailEmitting(bool) (guarded, Clear() before emitting = true); trail starts off in Init.
- LaserController.StartLengthTransition wraps every SetLaserLength; CheckCommonBehaviour compares dist with _lastDist (Mathf.Approximately, NoDistance = -1 resets).
- Trail lives in Assets/Prefabs/Laser/VFX's/LaserHit.prefab under the LaserHit root, which is _hitLaser; StopHitEffect calls SetActive(false) on it.
