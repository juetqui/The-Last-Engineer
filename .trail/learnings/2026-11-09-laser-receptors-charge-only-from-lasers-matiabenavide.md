---
title: "Laser receptors charge only from lasers in their own glitch state, count beams per state, and drive doors and tube lights as activation sources instead of scen…"
author: Matiabenavide
date: 2026-11-09
branch: feature/laser-receptor-refactor
id: l-04934a01
scope: repo
status: active
src: agent
touches:
  - TheLastEngineer/Assets/Prefabs/ReceptorEnergia.prefab
  - TheLastEngineer/Assets/Scripts/Gameplay/Connection/TubeLightController.cs
  - TheLastEngineer/Assets/Scripts/Gameplay/Laser/Laser.cs
  - TheLastEngineer/Assets/Scripts/Gameplay/Laser/LaserActivator.cs
  - TheLastEngineer/Assets/Scripts/Gameplay/Laser/LaserReceptor.cs
  - TheLastEngineer/Assets/Scripts/Gameplay/Laser/MVC/LaserController.cs
  - TheLastEngineer/Assets/Scripts/Gameplay/Laser/MVC/LaserModel.cs
  - TheLastEngineer/Assets/Scripts/Interfaces/ILaserReceptor.cs
  - TheLastEngineer/Assets/Scripts/Player/MVC/PlayerController.cs
  - TheLastEngineer/ProjectSettings/Packages/com.unity.probuilder/Settings.json
---

## Decided
- A receptor requires Clean or Intangible (fixed in the Inspector, filtered dropdown). The emitting laser's container state must match exactly; a laser with no glitch holder counts as Clean, so a moving Glitched laser never charges a receptor.
- Lasers tell receptors their state on enter and the SAME state on exit; a state change while hitting is exit(old) + enter(new). The receptor keeps a plain count of valid beams: charges with the first, empties when the last leaves.
- An invalid laser is ignored entirely (no hit FX, audio, charge); it only calls a negative-feedback hook that for now logs a warning once per entering beam. Its own particles are a later task.
- The receptor is an activation source: active when the charge completes, inactive the moment it starts emptying. The four scene UnityEvents were removed; doors go through the door controller's source list and tube lights through the tube light controller, which now listens to any activation source (Connection or receptor).
- Timings live in a config ScriptableObject per receptor (only one default preset; the 5.5s overrides were accepted as lost). Glow animates with a PrimeTween tween; emission color comes from the glitch palette's emission color, set once.
- The legacy laser script and the legacy receptor checker are left as they are (only made to compile); their scenes are old references.

## Tried and ruled out
- Passing the laser itself or a small emitter interface: the user chose just the state, so the per-state count replaces emitter identity.
- Keeping the UnityEvents or renaming them: replaced, the user rewires doors/tube lights by hand.
- Deleting the legacy laser: a subclass still used in an old scene depends on it.
- Encapsulating the public completed/load fields: kept public because the legacy checker writes them.

## Watch out
- The legacy laser calls the receptor every frame, so the count never drops to 0 in those old scenes.
- The door controller ANDs all its sources: a door that a receptor opened directly but that also lists connections now needs both.
- The receptor crystal's visible pink comes from _ColorTop/_ColorBottom; only _EmissionColor follows the glitch state.

In the code: ILaserReceptor.LaserReceived/LaserNotReceived(GlitchState), LaserModel._lastState, LaserController.EmitterState, LaserReceptor (_config, _requiredState, _validHitCount, IsActive => _isCompleted), LaserReceptorConfig + LaserReceptorConfig_Default.asset (5 / 0.5 / 3 / true) in Scripts/Scriptable Objects, GlitchStateOptionsAttribute + Editor/Glitch/GlitchStateOptionsDrawer, TubeLightController now GetComponent<IActivationSource>. Branch feature/laser-receptor-refactor.
