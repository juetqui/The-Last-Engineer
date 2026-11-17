---
title: "Platform station lights blink while the platform heads to them, turn white on arrival, and follow power on/off"
author: Matiabenavide
date: 2026-11-17
branch: main
cost: { minutes: 1, tokens: 1650 }
id: l-cd0ced91
scope: repo
status: active
src: agent
---

## Decided
- Only route points marked as stations get lights, because the platform only stops there; pass-through points are skipped when picking the target.
- The next station starts blinking (serialized HDR color <-> black, PrimeTween yoyo, serialized ease and cycle) as soon as the platform arrives at the previous one, because the user wants the player to see the next stop during the wait.
- On arrival with power the station tweens to a serialized HDR white and stays on, so intensity can be tuned per level.
- Power state drives the rest, because the lights must read as "this line is powered": unpowered at load = all black; power on = all white except the target, which blinks; power cut while travelling = the others tween to black at once and the target keeps blinking until the platform arrives, then goes black; power cut while waiting = everything black.
- Blink/white/tween settings live on each station, because the user wants per-station tuning.

## Tried and ruled out
- Blinking only once the platform departs: ruled out because the user wants it to start on arrival at the previous station.
- Leaving the other stations white until the platform comes to rest: ruled out because they should turn off with the same tween as power-on.
- One shared config on the platform: ruled out because each station needs its own.

## Watch out
- The route keys points by Vector3 and drops the Transforms, otherwise lights could hang off it; keep the stop list alongside it.
- Stopping on power loss can end on a pass-through point and never reach the blinking station, so "came to rest unpowered" must turn everything black, otherwise a light keeps blinking forever.
- Don't put the old connection-driven emissive listener on the same renderers, because both write the emissive color and fight.

In the code: PlatformController (_positions2, OnConnectionChanged, AdvanceRouteAndWait, PoweredOn/BecameInactive), RouteManager.Advance, ToStopState; shader property "_EmissiveColor"; EmissiveListener.
