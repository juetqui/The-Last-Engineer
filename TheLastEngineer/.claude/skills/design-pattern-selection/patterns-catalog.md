# Patterns catalog

Problem first. If the "Don't use when" column describes your case, don't use the pattern.

| Problem | Pattern | In Unity/C# | Don't use when |
|---|---|---|---|
| Systems react to events without knowing each other | Observer | `event Action<T>`, `UnityEvent` (designer wiring), ScriptableObject event channels | One fixed sender and one fixed receiver — call it directly |
| Swappable behavior (AI, weapons, movement) | Strategy | Interface + ScriptableObject assets, or `[SerializeReference]` list | Only one variant exists |
| Entity with clear states and transitions | State | State classes + a small state machine; Animator only for animation | Two states — a `bool` or enum is enough |
| Many objects created/destroyed often | Object Pool | `UnityEngine.Pool.ObjectPool<T>` | Objects are created once |
| Input, undoable actions, replays, editor tools | Command | Command classes; `Undo` in the editor | A simple action with no history |
| Variable construction of objects/prefabs | Factory | Class or ScriptableObject that instantiates configured prefabs | A direct `Instantiate` is enough |
| Shared, designer-editable data | Data container | ScriptableObject | Data that changes per instance at runtime |
| Access to global services (audio, save) | Service Locator / DI | The project's container (VContainer/Zenject) or a minimal locator | One already exists — reuse it |
| UI with logic | MVP / MVVM | View (MonoBehaviour / UI Toolkit) + plain, testable presenter | Static screen |
| Composing entity behavior | Component | Small components on the GameObject | — (it's Unity's foundation) |
| Loading and lifetime of scenes/services | Bootstrapper | Bootstrap scene or `RuntimeInitializeOnLoadMethod` | Single-scene project |

## Notes per pattern

**Observer** — subscribe in `OnEnable`, unsubscribe in `OnDisable`; a missing unsubscribe is a leak
and a call on a destroyed object. Event channels (SO) trade traceability for designer wiring —
use them when designers actually wire things.

**Strategy** — ScriptableObject strategies are shared assets: keep them stateless, or create a
runtime instance per user. `[SerializeReference]` gives per-instance data but needs `[MovedFrom]`
on renames.

**State** — keep transitions in one place (the machine or each state's `Next()`), not scattered.
Log transitions in development builds; it's the cheapest debugger.

**Object Pool** — reset state on `Get`/`Release`; a pooled object must not keep subscriptions or
coroutines from its previous life.

**Service Locator / DI** — register at bootstrap, resolve once (in `Awake`/construct), never in
`Update`. Don't add a container to a project that doesn't have one as a side effect of a feature.

**MVP** — the presenter never touches `UnityEngine.UI`/`UIElements` types; the view never contains
game rules. That's what makes the presenter testable in EditMode.
