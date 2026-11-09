---
title: "Glitch charge becomes a state swap: Glitcheable only Clean/Glitched, a separate Intangible component only Clean/Intangible"
author: Matiabenavide
date: 2026-11-08
branch: main
cost: { minutes: 20, tokens: 90815 }
id: l-a7c483ca
scope: repo
status: active
src: agent
touches:
  - TheLastEngineer/Assets/Scripts/Gameplay/Glitch/Glitcheable.cs
  - TheLastEngineer/Assets/Scripts/Gameplay/Glitch/GlitchState.cs
---

## Decided
- No more 0-1-2 charges. Each container holds a state, and Set/Take swap the whole state between the node in hand and the object (tap only, no hold).
- Node: Clean, Intangible or Glitched. Glitcheable: only Clean<->Glitched. New Intangible MonoBehaviour (own FSM, same structure as Glitcheable): only Clean<->Intangible. Glitcheable and Intangible are mutually exclusive on a GameObject.
- Rejected: Intangible node -> Glitcheable, Glitched node -> Intangible, Take while the node isn't Clean, Set into a non-Clean object or with a Clean node. They use the current error feedback.
- Passing through still needs the object in Intangible AND a node in Intangible in the player's hand. Needing a second Intangible node is the intended puzzle.
- Non-Clean nodes are blocked only for Set/Take. Pick/drop/plug into a Connection are unchanged.
- The Intangible orb (PSType.Intangible + tint) is removed. Lasers split into two prefab variants: Glitcheable (moves) and Intangible (beam passes through Intangibles).
- The user migrates prefabs/scenes by hand in the Editor. Code sessions only list what to touch.

## Tried and ruled out
- Keeping charges conserved (Set of 2 passes 1, Glitched = level >= 1): dropped for a simpler state swap per object type.
- Intangible passable with only the object Intangible: rejected, the two-node rule is the puzzle.
- Splitting Intangible into Model/View/Controller: rejected, it mirrors Glitcheable's single MonoBehaviour + FSM.

## Watch out
- GlitchComponent._currentLevel and GlitcheableOrbitController.PSType are serialized as ints in prefabs: changing the enums silently remaps data.
- GlitcheableLaserContainer.prefab currently starts at level 1 (Intangible).

In the code: Glitcheable.cs, GlitchComponent.cs, GlitchTransferManager.cs, GlitchTransferHandler.cs, PlayerIntangibilityHandler.cs, LaserController.cs, GlitcheableOrbitController.cs. Supersedes the "one holder, one mutator" charge decision in doc/context/decisions.md once implemented.
