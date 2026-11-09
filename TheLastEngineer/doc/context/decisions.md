---
purpose: "Record structural technical decisions (lightweight ADRs) so intentional choices are preserved. Small decisions go to trail learnings instead. Seeded by /init-unity, appended by /update-unity."
expected_content:
  - "One entry per decision, newest first"
  - "Date and status (proposed, accepted, superseded)"
  - "The decision and the context that led to it"
  - "Alternatives considered and why they were rejected"
  - "Consequences and trade-offs"
---

<!-- Seeded by /init-unity, appended by /update-unity. Keep this frontmatter and the existing
     entries; add new ones on top. -->

## `Glitch.Rules`: first gameplay asmdef, for testable rules
- **Date:** 2026-11-09 · **Status:** accepted
- **Decision:** `GlitchState`, `GlitchContainerKind`, `GlitchTransferResult` and the pure
  `GlitchSwapRules` live in `Assets/Scripts/Gameplay/Glitch/Rules/Glitch.Rules.asmdef`
  (auto-referenced, no engine references). EditMode tests reference it from
  `Assets/Tests/EditMode/Glitch.Rules.Tests.asmdef`.
- **Context:** test asmdefs can't reference `Assembly-CSharp`, and tests placed in
  `Assembly-CSharp-Editor` don't see NUnit (it is explicitly referenced).
- **Alternatives:** no EditMode tests, only Play Mode checks — rejected; moving the whole glitch
  system to an asmdef — too big for this change.
- **Consequences:** anything that must be unit-tested goes into a small asmdef of plain C#; an asmdef
  that uses `GlitchState` must reference `Glitch.Rules`.

## Glitch state swap per container
- **Date:** 2026-11-09 · **Status:** accepted (supersedes "Glitch charge: one holder, one mutator")
- **Decision:** no more 0–2 charges. Each container holds a `GlitchState` in its `GlitchComponent`
  (still the only holder): a node holds Clean/Intangible/Glitched, a `Glitcheable` only
  Clean/Glitched, an `Intangible` only Clean/Intangible (`GlitchSwapRules.IsValidState`). Set/Take
  (tap only) swap the whole state between the node in hand and a Clean/charged object; only
  `GlitchTransferManager.Set/Take` mutate (`GlitchComponent.SetState`). Glitcheable and Intangible are
  mutually exclusive on a GameObject. Passing through needs the object Intangible AND an Intangible
  node in hand.
- **Context:** the conserved 0–2 charge with tap/hold was hard to read as a puzzle; one state per
  object type is simpler.
- **Alternatives:** keep conserved charges (Set of 2 passes 1) — dropped; Intangible as a level of
  Glitcheable — split into its own component with its own FSM.
- **Consequences:** invalid serialized states are blocked in the Editor (filtered inspector, Play
  Mode refuses to start) and sanitized to Clean with a LogError at runtime
  (`GlitchComponent.SanitizeOnLoad`, the only write outside the manager). Swap targets implement
  `IGlitchSwapTarget`; the Intangible orb tint and the hold (`transferHoldDelay`) are gone.

## Glitch charge: one holder, one mutator
- **Date:** TODO (before 2026-11-01) · **Status:** superseded by "Glitch state swap per container"
- **Decision:** a node's or glitcheable's level (`GlitchState` 0–2) lives only in its
  `GlitchComponent`; only the static `GlitchTransferManager.ExecuteTransfer(source, target)` changes
  it (atomic, sum-preserving; `Preview` validates without effects). Colors come only from
  `GlitchPalette`. Nodes replaced the old `NodeType` enum with `GlitchState`.
- **Context:** TODO — not recorded; the code comments only state the rule.
- **Alternatives:** TODO — not recorded.
- **Consequences:** never call `TryApplyDelta` directly; consumers react to `OnGlitchStateChanged`;
  `Glitcheable` reconciles its FSM with the level instead of owning it.

## Interaction detection centralized in `PlayerInteractionDetector`
- **Date:** TODO · **Status:** accepted
- **Decision:** interactables implement `IInteractable` (+ `IProximityListener`) and never detect
  the player with their own `OnTriggerEnter`; the player's `PlayerInteractionDetector` owns the
  registry (trigger + per-frame revalidation + line of sight). Objects that toggle their collider or
  layer re-announce themselves with `PlayerController.Instance.RescanInteractable`.
- **Context:** TODO — not recorded.
- **Alternatives:** TODO — not recorded.
- **Consequences:** a new interactable needs a solid collider and an `InteractablePriority`.

## Gameplay code in `Assembly-CSharp`, MVC + cached-state FSMs
- **Date:** TODO · **Status:** accepted (existing state)
- **Decision:** gameplay has no asmdefs or namespaces. Systems use MVC (`*Controller` MonoBehaviour,
  plain `*Model`/`*View` built in `Awake`) and state machines with cached state instances.
  Existing misspelled names (`Insfrastructure`, `HUB Interctables`, `PreassurePlate`, `Iluiminacion`)
  are kept because renaming breaks `.meta`/scene references.
- **Context:** student/team project grown organically; documented in `doc/legacy-project-notes.md`.
- **Alternatives:** per-system asmdefs — not adopted.
- **Consequences:** no compile-time boundaries between systems; test asmdefs can't reference
  gameplay code until it gets its own asmdef (first exception: `Glitch.Rules`, see above).

## No Unity MCP server (template default)
- **Date:** 2026-11-08 · **Status:** accepted (template default — revisit per project)
- **Decision:** Claude works on the project through files, trail and Unity batchmode
  (`/run-tests`, `/build`). No Unity MCP server, so there is no `.mcp.json` and none is enabled in
  `.claude/settings.json`.
- **Context:** the DiPhot Unity template wants low context cost and nothing that sends project code
  to third parties. Batchmode already covers tests and builds without the Editor open.
- **Alternatives:** the official Unity MCP or a community server (e.g. unity-mcp) to read the
  console, run tests with the Editor open and query scene/prefab hierarchies without parsing YAML.
  Not evaluated for this template.
- **Consequences:** Claude can't see the open Editor: tests and builds need it closed, it reads scene
  and prefab YAML by fragment, and Play Mode checks are done by the user. To adopt an MCP later: pin
  its version in `.mcp.json`, list it in `enabledMcpjsonServers`, document it in `.claude/README.md`,
  and supersede this entry.
