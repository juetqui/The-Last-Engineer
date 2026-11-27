---
title: "Mesh-shaped VFX repel: physics overlap feeds the graph from C#, and it runs in Edit Mode"
author: Matiabenavide
date: 2026-11-27
branch: feature/vfx-mesh-repeller
cost: { minutes: 21, tokens: 146690 }
id: l-e6f19fdc
scope: repo
status: active
src: agent
touches:
  - TheLastEngineer/Assets/Scripts/Gameplay/VFX/VfxMeshRepeller.cs
---

## Decided
- VFX Graph can't query physics, so a C# component finds the closest overlapping collider and pushes its world position plus a 0..1 weight into the graph. The graph moves each particle from its stored rest position along its mesh normal, then lerps back to rest.
- The component runs in Edit Mode so the effect can be tuned without Play Mode.
- Property IDs come from Shader.PropertyToID, not ExposedProperty. ExposedProperty is in the VFX Graph runtime package, which the out-of-Editor compile check can't resolve.

## Tried and ruled out
- "Set Custom Attribute" block: removed in VFX Graph 17 (Unity 6). Declare the attribute in the Blackboard (+ > Attribute) and drag it into a context to get its Set/Get nodes.

## Watch out
- "Particles don't move" was first traced to the repeler layer mask: it only had Intangible, and the only object on that layer was the VFX itself, which the component skips on purpose. This was not confirmed in the Editor. Check the gizmo colour first (cyan = nothing detected), then Scene view "Always Refresh".
- Wiring mistakes found: Set resetPosition in Initialize left unconnected (every particle collapses to the origin), One Minus placed before Divide, and resetPosition + normal multiplied instead of normal * factor added. Exposed position must be World space.
- In Edit Mode, call Physics.SyncTransforms before overlap queries, and use the real-time clock for fades: Time.deltaTime is meaningless there.

In the code:
- Assets/Scripts/Gameplay/VFX/VfxMeshRepeller.cs (exposed names "RepelPos", "RepelWeight")
- Assets/Art-2/VFX/VFXGraph_RepelObject.vfx (attribute resetPosition, normal from the direction attribute written by Set Position (Mesh))
