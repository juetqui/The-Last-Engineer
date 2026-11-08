# UI Toolkit reference

## Files and roles
- `.uxml` — structure. `.uss` — style. A `UIDocument` component with `PanelSettings` displays it.
- The view `MonoBehaviour` gets `GetComponent<UIDocument>().rootVisualElement` in `OnEnable` and
  queries elements with `root.Q<Button>("play-button")`. Cache the queries; never query per frame.
- Element names in UXML are the contract with code: renaming one breaks the view silently, so
  grep the C# for the name before changing it.

## Styling
- Style in USS, not inline in UXML or C#. Use classes (`.menu-button`) over names for styling;
  names are for code lookup.
- Use USS variables (`--color-primary`) defined in a theme stylesheet for colors and spacing.
- Toggle states with `AddToClassList` / `RemoveFromClassList` / `EnableInClassList`, not by
  setting style properties in code.

## Layout
- Flexbox (Yoga): `flex-direction`, `flex-grow`, `justify-content`, `align-items`.
  Prefer percentages and flex over fixed pixel sizes.
- `PanelSettings` → *Scale Mode: Scale With Screen Size* with the reference resolution.
- Safe area: apply `Screen.safeArea` as padding on the root container (convert with
  `RuntimePanelUtils.ScreenToPanel`).

## Events and binding
- Register callbacks in `OnEnable` (`button.clicked += OnPlay` or
  `RegisterCallback<ClickEvent>`) and unregister in `OnDisable`.
- Unity 6 runtime data binding (`dataSource` + `SetBinding`) is fine where the project already
  uses it; otherwise push values from the presenter explicitly.
- Long lists: `ListView` with `makeItem`/`bindItem` (it virtualizes); never instantiate one element
  per item for big data.

## Mixing with the game
- Input reaches UI Toolkit through the Input System's UI support; check `PanelSettings` and the
  event system setup match the project.
- World-space UI is limited in UI Toolkit; if a screen needs it, check whether the project uses
  UGUI for world-space instead.
