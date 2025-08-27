# Setup Rápido (Nodes + Platform + TaskManagers)


## 1) Carpetas
- `Core/Domain` → Interfaces puras: `IInteractable`, `IConnectable`, `IMovablePassenger`, `ISplinePathProvider`, etc.
- `Gameplay/Nodes` → `NodeController`, `NodeModel`, `NodeView` (+ shaders/outline drivers).
- `Gameplay/Connections` → `Connection`, `GenericConnectionController`, `SpecificConnectionController`, `SpecificConnectionView`.
- `Gameplay/Platform` → `PlatformController`, `PlatformMotor`, `RouteManager`, `States`.
- `Gameplay/TaskManagers` → `RequirementsTaskManager`, `MainTM`, `SecondaryTM` (escuchan `RunningChanged`).
- `UI` → HUD/Screens.


## 2) Diagrama de dependencias
```
Core (domain, puro C#)
↑ ↑ ↑
| | |
Gameplay.Nodes Gameplay.Connections Gameplay.Platform
\ / \
\ / \
Gameplay.TaskManagers -----> UI
```
Regla: Gameplay.* puede depender de `Core` y entre sí solo por **interfaces/eventos**; UI depende de todo lo necesario para mostrar feedback.


## 3) Prefabs & wiring
### Player
- `PlayerTDController` (Controller)
- `PlayerTDView` (referencias a Renderer, Outline, Animator, SFX)
- `UnityInputAdapter` (si usás Input clásico)
- `PlayerData` (SO tunables)


### Node (prefab por tipo)
- `NodeController` (serializa `_originalShader`, `_desintegrationShader`, `_myColor`, `_desintegrationVector`)
- `NodeView` (Renderer, Outline, Animator, Particles opcional)
- *Opcional:* Collider + script que implemente `IInteractable`


### Connection
- `GenericConnectionController` o `SpecificConnectionController`
- `SpecificConnectionView` (luces/FX)
- Asignar `NodeType`/slots según puzzle


### Platform
- `PlatformController` (asignar `RouteManager` con array de puntos, `PlatformMotor`)
- Puntos de ruta: Transforms hijos `P0, P1, P2...`
- **Nota:** si no hay puntos, `RouteManager.IsValid == false` y la plataforma no se mueve.


### Task Manager (puerta)
- `RequirementsTaskManager` en el mismo GameObject que `MainTM`/`SecondaryTM`
- Arrastrá las `Connections` a la lista
- Elegí **Simple Count** (todas ON) o **Required Types** (rellená la lista)
- (Opcional) Animator con parámetro `DoorActivated` + Audio/Particles


## 4) Eventos & Estados
- Conexión → emite `OnNodeConnected(NodeType type, bool active)`
- `RequirementsTaskManager` escucha todas y, cuando la condición se cumple, dispara `RunningChanged(true)`
- `MainTM`/`SecondaryTM` escuchan `RunningChanged` y abren/cerran puerta (anim/sfx)
- Platform FSM: `Inactive` (bloqueada), `Waiting` (delay), `Moving` (avance a `RouteManager.CurrentPoint`)


## 5) Testing recomendado
- *EditMode*: `NodeModel.MoveObject` (oscilación sin drift), `Connection` reglas, `RequirementsTaskManager` (inputs variados).
- *PlayMode*: Integración: conectar nodos, abrir puerta, mover plataforma con ruta válida.


## 6) Estándares
- Controller ≠ lógica visual; View encapsula shaders/particles/anim.
- Sin `new` de estados dentro de `Tick`; cachear instancias.
- Subscribir/desubscribir simétrico (`Awake/OnDestroy` o `OnEnable/OnDisable`).
- Evitar singletons en puzzles: preferir eventos/inyectar interfaces.
