# UGUI (Canvas) reference

## Canvas setup
- Split canvases by update frequency: static backgrounds on one canvas, frequently changing HUD
  elements on another. Any change rebuilds the whole canvas it belongs to.
- `Canvas Scaler`: *Scale With Screen Size* with the project's reference resolution; match
  width/height deliberately (`0.5` is a safe default for mixed orientations).
- Disable `Raycast Target` on every graphic that isn't clickable (images, labels).
- One `EventSystem` per scene, with `InputSystemUIInputModule` (not `StandaloneInputModule`).

## Layout
- Anchors and pivots for responsive placement; `LayoutGroup`s only where content is dynamic —
  they're expensive and rebuild on every change.
- `ContentSizeFitter` inside a layout group is a common source of jitter; prefer the parent's
  `childControlSize`.
- Safe area: a `RectTransform` that applies `Screen.safeArea` to its anchors, parent of the content.

## Components
- Text: `TextMeshProUGUI`, never the legacy `Text`.
- Buttons: wire `onClick` in code from the view (`button.onClick.AddListener`) and remove it in
  `OnDisable`, or in the Inspector — follow the screen's existing choice, don't mix.
- Lists with many rows: pool the rows (`UnityEngine.Pool`) or use a recycling scroll view.

## Performance
- Hide a canvas by disabling the `Canvas` component (keeps its geometry), not by `SetActive(false)`
  on large hierarchies that are toggled often.
- Avoid `Animator` on UI elements that animate constantly — it dirties the canvas every frame;
  tween in code instead.
- Profile with the UI profiler module and Frame Debugger when the canvas rebuild cost shows up.
