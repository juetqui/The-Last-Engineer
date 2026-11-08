---
name: unity-ui-development
description: "Use whenever building or changing in-game UI in Unity — menus, HUD, screens, popups, layout, safe area — with UGUI (Canvas) or UI Toolkit (UXML/USS). Load it before exploring or editing the UI code."
---

# Unity UI development

The project uses **both** UGUI and UI Toolkit. Never mix them inside one screen.

## Before coding
- Find out which system the screen uses:
  - UGUI → a prefab/scene with `Canvas`, `RectTransform`, `UnityEngine.UI`/`TMPro` components.
  - UI Toolkit → `.uxml`/`.uss` files and a `UIDocument`.
  `trail ask "<screen name>"`, `doc/context/project-map.md`.
- Then read the matching reference: [ugui.md](ugui.md) or [ui-toolkit.md](ui-toolkit.md).
- New screen? Follow what neighboring screens use; if there's no precedent, ask the user — don't pick.

## Structure — separate view from logic (MVP)
- **View**: the `MonoBehaviour` / `UIDocument` wrapper. Finds elements, shows data, raises
  user intents (`event Action PlayClicked`). No game rules.
- **Presenter**: plain C# class. Takes the model/services, listens to view intents and model
  changes, decides what the view shows. Testable in EditMode.
- **Model**: the game state it reads (existing systems — don't duplicate it in the UI).
- A static screen with no logic doesn't need a presenter (`design-pattern-selection`: small scope).

## Conventions
- Subscribe in `OnEnable`, unsubscribe in `OnDisable` — UI is enabled/disabled constantly.
- No per-frame UI updates: push changes on events; if polling is unavoidable, throttle it.
- Text: TextMeshPro in UGUI; strings that the player reads go through the project's localization
  if it has one (`architecture-runtime.md`).
- Resolution: test at the target aspect ratios; respect `Screen.safeArea` on mobile.
- Input: navigation and submit through the Input System UI module / UI Toolkit events — no
  legacy `Input`.

## Workflow
1. Identify the system → 2. read its reference → 3. view + presenter (if logic) → 4. EditMode tests
   for the presenter → 5. `/run-tests` → 6. ask the user to check it in Play Mode at the target
   resolutions, or describe exactly what to check.
