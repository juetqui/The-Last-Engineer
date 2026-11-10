---
title: "The player's Intangible glow on body and backpack is one shared fade component reacting to the node-grabbed event, linear and proportional in time"
author: Matiabenavide
date: 2026-11-10
branch: fix/player-body-alpha-on-drop
cost: { minutes: 3, tokens: 27146 }
id: l-44010596
scope: repo
status: active
src: agent
touches:
  - TheLastEngineer/Assets/Scripts/Player/Components/PlayerIntangibleOverlay.cs
  - TheLastEngineer/Assets/Scripts/Player/MVC/PlayerView.cs
  - TheLastEngineer/Assets/Scripts/Player/Components/BackPackColorChange.cs
  - TheLastEngineer/Assets/Scripts/Scriptable Objects/PlayerData.cs
  - TheLastEngineer/Assets/Scripts/Player/MVC/PlayerController.cs
---

## Decided
- Body and backpack fade the Intangible overlay (_Alpha) with the same component, one instance per renderer, both subscribed to the node handler's grab event (raised on pick, release and every level change of the node in hand). Sync comes from sharing the code, not from matching timings by hand.
- Timing: linear, 0.5 s for a full 0<->1 fade; a fade that starts mid-way takes proportionally less. A new event always stops the running tween first.
- Releasing the node also stops the body's default and corrupted particle systems with Stop() (live particles finish).
- The material transport field on the PlayerData ScriptableObject was removed: it only carried a runtime scene material instance.

## Tried and ruled out
- Just completing the reset in PlayerView.GrabNode(false): smallest change, but body and backpack would stay two implementations with timings kept equal by hand.
- Copying the backpack's old curve (Lerp from current with a 0->1 timer): frame-rate dependent, did not restart mid-way, and kept writing the material every frame forever.
- Cutting corruption on release: rejected, no gameplay change; the overlay instance just reaches 0 while hidden and shows right when materials are restored.

## Watch out
- The body's material slot 1 is M_PlayerGhost (S_PlayerGhost): it has _Alpha but NOT _HasNode/_IsGlitched (those exist only in S_GlowCharacter, used by no player renderer). PlayerView's writes to them are currently no-ops; left as is on purpose.
- Cache the overlay material once in Awake: reading Renderer.materials while the corruption materials are on instantiates copies of those instead.
- "0 - MAIN MENU" has a BackPackColorChange with no player: do not add the overlay component there (it disables itself with a warning if it finds no handler).

In the code:
- Assets/Scripts/Player/Components/PlayerIntangibleOverlay.cs (new) — must be added in the Editor to the body renderer and the backpack in "NEW Player.prefab", material index 1.
- PlayerView.PlayNodePS / GrabNode(false), BackPackColorChange (color only now), PlayerController.Awake, PlayerData.
