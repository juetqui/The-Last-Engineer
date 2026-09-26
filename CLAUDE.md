# The Last Engineer

Puzzle 3D en Unity **6000.3.6f1** (URP + Input System + Cinemachine + PrimeTween). El proyecto Unity
está en `TheLastEngineer/` (no en la raíz del repo). Código en `TheLastEngineer/Assets/Scripts/`,
sin namespaces ni asmdefs.

## Antes de buscar archivos

Leé o grepeá **`.claude/PROJECT_MAP.md`** en vez de recorrer `Assets/` con Glob:

- §10 *Cómo encontrar cosas* — recetas "para X → mirar Y".
- §11 *Índice de API pública* — `Grep "NombreClase"` sobre el mapa da archivo, herencia,
  SerializeFields y miembros públicos de las ~190 clases.
- §5 *Sistemas y flujos* — cómo se conectan Player, Glitch, Nodes/Connections, Laser, Platform,
  Doors, UI, Cinemáticas, Inspección (con las tablas de estados de cada FSM).
- §7 *Convenciones* y la lista de código legacy que convive con el vigente.

## Convenciones clave

- MVC por sistema: `*Controller` es el MonoBehaviour; `*Model`/`*View` son clases planas creadas en `Awake`.
- FSM: interfaz de estado + máquina que cachea instancias; transiciones por `ToX()`/`TransitionToX()`.
- Interacción: implementar `IInteractable` (+ `IProximityListener`); el `PlayerInteractionDetector`
  detecta, nunca un `OnTriggerEnter` propio. Si un objeto apaga/prende su collider, `RescanInteractable`.
- Eventos `Action`/`event Action` con suscripción simétrica; `UnityEvent` solo para cablear desde inspector.
- Privados `_camelCase` con `[SerializeField] private`; comentarios en español explicando el *por qué*.
- Los typos existentes en nombres (`Insfrastructure`, `HUB Interctables`, `PreassurePlate`…) se
  conservan: renombrar rompe `.meta`/referencias.
- Nunca editar `PlayerInputs.cs` (generado): se edita el `.inputactions`.

## Ignorar

`Library/`, `Temp/`, `obj/`, `Logs/`, `UserSettings/`, `*.meta`, `Assets/Basura/`,
`Assets/Scenes/Posibles referencias (EX OLD)/`, `Assets/TextMesh Pro/`, `Assets/Samples/`, `Assets/Plugins/`.

## Skills del proyecto

- `/init-project` — regenera `.claude/PROJECT_MAP.md` desde cero (y este archivo si no existe).
- `/update-project` — actualización incremental del mapa según `git diff` + working tree.

**Regla:** después de un `git pull`, merge, checkout de branch, o al terminar cambios en
`Assets/Scripts` (clases nuevas, borradas, firmas públicas), correr `/update-project`.

El `README.md` de la raíz describe una arquitectura anterior y está desactualizado: manda el mapa.
