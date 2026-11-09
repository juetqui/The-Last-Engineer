---
title: "Intangible split: invalid glitch states are blocked in the Editor, Intangible reuses the existing state holder, and swap rules get their own test assembly"
author: Matiabenavide
date: 2026-11-09
branch: main
cost: { minutes: 19, tokens: 311669 }
id: l-132187cf
scope: repo
status: active
src: agent
---

## Decided
- Intangible objects keep the existing GlitchComponent as their state holder (no own field), so migrating = swapping Glitcheable for Intangible keeps _currentLevel.
- Glitcheables currently at Intangible (level 1) become Glitcheable Clean, not Intangible objects. Intangible objects and Intangible laser prefabs are built by hand by the user.
- Invalid states are prevented in the Editor: GlitchComponent inspector shows only the states its owner accepts (Glitcheable Clean/Glitched, Intangible Clean/Intangible, node all three). Entering Play Mode with an invalid state, or with Glitcheable and Intangible on the same object, logs an error per object and cancels Play.
- In builds, an invalid state logs an error and starts as Clean.
- Adding Glitcheable to an object with Intangible (or vice versa) warns and removes the newly added one; the first stays.
- Set/Take execute on press (tap only); the cancel input events go away.
- Intangible laser: lives on an Intangible container; beam passes through Intangibles only while its container is Intangible. Its crystal shows the glitch effect like Glitcheable lasers.
- Intangible FSM has three states: Clean, Phased, Exiting (visually Clean, waits for the player to leave before switching layer). Fade is a tween started on Enter, not a state.
- Swap rules live in a pure static class in a small auto-referenced asmdef (with GlitchState) so EditMode tests can reach them; first gameplay asmdef, document as an ADR.

## Tried and ruled out
- OnValidate that reverts invalid values: rejected for a filtered dropdown.
- Warning + treat as Clean in the Editor: rejected, the user wants Play blocked.
- Intangible keeping its own bool/enum: rejected, loses serialized data on migration.
- Converting level-1 Glitcheables into Intangible objects automatically: rejected, they reset to Clean.
- Fade as its own FSM states: rejected as over-built.
- No EditMode tests: rejected; tests in Assembly-CSharp-Editor don't work because nunit is explicitly referenced.

## Watch out
- Glitcheable Container's base level is Glitched: reverting a scene override of 1 makes it Glitched, not Clean.
- The Play Mode guard only sees open scenes.

In the code: GlitchComponent._currentLevel (keep name/type), GlitchState values Clean=0/Intangible=1/Glitched=2 unchanged, Glitcheable._intangibleLayer was layer 20 (m_Bits 1048576), fade 0.4, ease 6. Planned: Assets/Scripts/Gameplay/Glitch/Rules/Glitch.Rules.asmdef, GlitchSwapRules, Assets/Editor/Glitch/GlitchComponentEditor + GlitchStatePlayModeGuard, Assets/Tests/EditMode/Glitch.Rules.Tests.asmdef.
