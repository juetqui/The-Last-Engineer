---
title: "Tube lights rewind from the tube in transition when interrupted; direction comes only from fill/drain, no list reversal"
author: Matiabenavide
date: 2026-11-11
branch: main
cost: { minutes: 5, tokens: 77773 }
id: l-08daff81
scope: repo
status: active
src: agent
touches:
  - TheLastEngineer/Assets/Scripts/Gameplay/Connection/TubeLightController.cs
  - TheLastEngineer/Assets/Scripts/Gameplay/Connection/TubeLight.cs
---

## Decided
- Filling goes from the source tube (index 0, next to the Connection) to the far end; draining mirrors it, from the far end back to the source. The direction depends only on fill/drain, and the list is never reversed. The auto-reverse option is being removed.
- If the source changes while a sequence is still running, the new sequence starts from the tube that is in transition and goes back the other way, like a rewind. Lit tubes always form a prefix of the list, so the orientation can't drift.
- The sequence logic (index, direction, next tube, interruption) goes into a plain C# class in its own engine-free assembly, tested in EditMode. The MonoBehaviour is a thin adapter around it.
- If the controller is disabled mid-sequence, it stops and forgets the index. On re-enable it waits for the next activation event, with no resync; tubes already in the requested state complete instantly.
- The controller source is converted from Windows-1252 to UTF-8 (no BOM, LF) as its own step before the fix.
- The user confirmed that every scene's tube list is authored source-first.

## Tried and ruled out
- Keeping the old Windows-1252 encoding and editing through a shell script: too fragile.
- Putting the class inside the glitch rules assembly: it mixes tube logic into the glitch domain.
- Resyncing on enable by adding IsActive to IActivationSource: it touched 3 more classes and wasn't needed.

## Watch out
- Root cause of the bug: an interrupted run reset the index to 0 without reversing the list, so every interruption shifted the orientation by one.
- TubeLight.PlayFill completes synchronously when the tube is already in the requested state. The controller has to absorb that in a loop instead of recursing or re-entering.
- The real tube lists are prefab-instance overrides in about 18 scenes, build scenes included, not in the prefabs. The removed field's key stays in the YAML until those assets are re-saved, which is harmless.

In the code: TubeLightController (Assets/Scripts/Gameplay/Connection), planned TubeLightSequence in Assets/Scripts/Gameplay/Connection/Rules with Connection.Rules.asmdef, referenced from Glitch.Rules.Tests. Script GUID c8f7e72f036de01469f20c79a1e49909. Branch fix/tube-light-sequence-rewind.
