# Mapa del proyecto — The Last Engineer
<!-- meta: commit=fc042895e3b72e4c8427c76f8363aaaa8a219d8f fecha=2026-09-26 unity=6000.3.6f1 scripts=197 escenas=20 -->

> Generado por `/init-project`. Mantener con `/update-project` después de cambios o `git pull`.
> Para encontrar algo: §10 (recetas) → §11 (grepear el nombre de la clase) → abrir el archivo.
> Todas las rutas de scripts son relativas a `TheLastEngineer/Assets/` salvo que se indique otra cosa.

## 1. Qué es y cómo abrirlo

**The Last Engineer** es un puzzle 3D en tercera persona (URP, cámara Cinemachine orbital). El jugador
recorre niveles modulares, levanta **nodos** (`NodeController`, con carga de glitch 0/1/2 en un `GlitchComponent`) y los
enchufa en **conexiones** (`Connection`) para abrir **puertas** y activar **plataformas**; los objetos
**glitcheables** (`Glitcheable`) se desintegran, se mueven entre anclas y se reintegran en ciclo, y el
jugador los prende o apaga pasándoles carga desde el nodo en mano (`Set`) o sacándosela (`Take`). Hay **láseres** que activan
receptores (o matan al jugador), **teletransportes** entre plataformas, placas de presión, un sistema
de **inspección** (limpiar corrupción sobre un objeto en un canvas aparte) y cinemáticas con waypoints.

| Dato | Valor |
|---|---|
| Raíz del repo git | `D:\UnityProjects\The-Last-Engineer\` (branch principal: `main`) |
| Proyecto Unity | `TheLastEngineer/` (contiene `Assets/`, `Packages/`, `ProjectSettings/`) |
| Versión de Unity | **6000.3.6f1** (Unity 6) |
| Render pipeline | Universal RP 17.3.0 + Volumetric Fog (CristianQiu), Render Graph |
| Input | Input System 1.18 — asset `Scripts/Core/Insfrastructure/Services/Inputs/PlayerInputs.inputactions` |
| Tweens | PrimeTween (`Assets/Plugins/PrimeTween`) |
| Abrir | Unity Hub → *Add project from disk* → carpeta `TheLastEngineer` |
| Play rápido | Escena `Scenes/BUILD SCENES/0 - MAIN MENU.unity` (índice 0 del build) o cualquier nivel directo: cada nivel trae su propio `Player`, `InputManager` y `LevelLoader` (singleton `DontDestroyOnLoad`) |

Sin assembly definitions ni namespaces (salvo `Tools.FolderTool.*`): todo el código de gameplay vive
en `Assembly-CSharp`, en el namespace global.

## 2. Estructura del repo y Assets

| Carpeta | Qué contiene | Cuándo mirarla |
|---|---|---|
| `/` (raíz repo) | `.gitignore` (Unity estándar), `README.md` (**desactualizado**, ver §7), `CLAUDE.md`, `.claude/` | Config del repo y de Claude |
| `TheLastEngineer/Assets/Scripts/` | Todo el código de gameplay (147 `.cs`). Ver §4 y §11 | Casi siempre |
| `Assets/Scenes/` | Escenas de build, de test y viejas. Ver §3 | Abrir/agregar niveles |
| `Assets/Prefabs/` | Prefabs jugables agrupados por tema (abajo) | Armar niveles, cablear componentes |
| `Assets/Prefabs/Glitcheables` | Prefabs con `Glitcheable` + `TimerController` + anclas | Objetos que se desintegran/mueven |
| `Assets/Prefabs/Laser` | Emisores (`LaserController`+`LaserView`), receptores, cristales | Puzzles de láser |
| `Assets/Prefabs/Ascensor`, `Capsula` | Ascensor de carga de nivel (`LevelLoader` + `Timelines/ElevatorTimeline.playable`), cápsulas de checkpoint | Transición entre niveles, checkpoints |
| `Assets/Prefabs/Modulos`, `Modulos Paredes`, `Modulo Pisos Cables`, `Level` | Módulos de piso/pared para armar niveles (se pintan con Prefab Painter / Map Builder, §9) | Level design |
| `Assets/Prefabs/Lights`, `Iluiminacion`, `Particles`, `Props`, `prefab-Props`, `AssetAmbientacion`, `AssetMinijuego` | Iluminación, VFX, props decorativos e interactuables del HUB | Ambientación |
| `Assets/Inspection System/` | **Sistema completo** (scripts + prefabs + UI) de inspección/limpieza de corrupción. Tiene sus propios `.cs` fuera de `Scripts/` | Paneles/baterías inspeccionables |
| `Assets/Editor/` | Ventanas de editor (Prefab Painter, Object Replacer, inspector de `LightMovement`) | Herramientas, §9 |
| `Assets/Tools/` | `FolderTool` (carpetas en jerarquía) y `MapCreator` (Map Builder) | Herramientas, §9 |
| `Assets/Resources/` | `Pause_Menu.prefab` (lo instancia `ScreenManager.Push("Pause_Menu")`), `GlitchPalette.asset` (`GlitchPalette.Default`), materiales `M_*` de nodos/corrupción/partículas, texturas `T_*Node` | Cosas cargadas por `Resources.Load` |
| `Assets/Scriptable Objects` (en `Scripts/Scriptable Objects/`) | `PlayerData.asset`, `GlitchSounds.asset`; en `Scripts/Core/Utilities/`: `InputDeviceDetector.asset`, `InputPromptDatabase.asset` | Tunables del jugador, sonidos, prompts |
| `Assets/Art/` | Animación (`Animation/ScriptsAnim/Scroller.cs`), audio, FBX, fuentes, materiales, partículas, shaders (post-proceso toon: `ToonOutlineFeature`, `ToonDepthNormalsFeature`, `PostProcessController`), `Store Meshes/QuickOutline` (`Outline.cs`, asset de tienda), texturas | Arte y shaders |
| `Assets/Art-2/` | Assets por integrante (`AssetLucas`, `AssetsFer`, `Gabi`), `Input Prompts` (sprites), `Dissolve Tutorial` (`SolvingController`, `PosToPosParticles`), `Shaders/ShaderScript` (`GlitchDeathController`, `ShaderFiller`, `StutterTimePerRenderer`) | Shaders de glitch/dissolve, prompts |
| `Assets/Settings/`, `RenderPipeline/` | URP assets, renderers, volume profiles, Build Profiles | Config de render |
| `Assets/Timelines/` | `ElevatorTimeline.playable` (+ copia ` 1`) | Animación del ascensor |
| `Assets/UI Toolkit/`, `TextMesh Pro/`, `Samples/`, `Plugins/`, `MobileDependencyResolver/` | Temas/paquetes de terceros | Casi nunca |
| `Packages/manifest.json`, `ProjectSettings/` | Dependencias y settings | §8, build settings (§3) |

**Ignorar** (no indexar, no proponer cambios ahí):
`Library/`, `Temp/`, `obj/`, `Logs/`, `UserSettings/`, `.idea/`, `.vscode/`, `*.csproj`/`*.sln`
(generados) · todo `*.meta` (solo se tocan al mover/renombrar assets desde Unity) ·
`Assets/Basura/` (descarte) · `Assets/Scenes/Posibles referencias (EX OLD)/` (escenas viejas de
referencia) · `Assets/TextMesh Pro/`, `Assets/Samples/`, `Assets/Plugins/*/internal` (terceros).

## 3. Escenas

**Build scenes** (`ProjectSettings/EditorBuildSettings.asset`, todas habilitadas, en orden):

| # | Escena | Para qué |
|---|---|---|
| 0 | `BUILD SCENES/0 - MAIN MENU.unity` | Menú principal (`MainMenu`, `FadeInController`); carga niveles por nombre con `LevelLoader` |
| 1 | `BUILD SCENES/1 - TUTO 1.unity` | Tutorial 1 (paneles `TutorialAnimController`, prompts `InputPromptIcon`) |
| 2 | `BUILD SCENES/2 - TUTO 2.unity` | Tutorial 2 |
| 3 | `BUILD SCENES/3 - HUB.unity` | HUB con props interactuables (`InteractablePropController`) y salida a niveles |
| 4 | `BUILD SCENES/4 - NIVEL PLATAFORMAS.unity` | Nivel de plataformas móviles (`PlatformController`) y teletransportes |

**Test levels** (`Scenes/TEST LEVELS/`, por persona; no están en el build):

- Raíz: `DiagonalTest`, `TestLights`
- `Fer/`: `IntroGameplayTest`, `New Scene`, `Test VFX`
- `Juli/`: `TEST SaveLoad Data`
- `Mati/`: `Light Tests`, `Mati_Testing`, `PersistentScene` (+ `Mati/Scripts/CameraShakeController.cs`, `CinematicSpaceshipController.cs`: prototipo de cinemática de nave con spline)

**Otras**: `Scenes/NIVEL HUB EfectosVisuales.unity` (pruebas de VFX del HUB). Carpetas `NIVEL 1 - Entregable/`, `NIVEL HUB/`, `MainMenu/`, `LightBakeTest/` solo tienen datos de lighting/occlusion de escenas ya movidas.

**Viejas / referencia** (`Scenes/Posibles referencias (EX OLD)/`, ignorar): `DV START`, `Lvl Espejos definitivo`, `NIVEL gabi`, `NIVEL gabi old`, `Niveles Lucas`.

## 4. Mapa de Assets/Scripts

```
Scripts/
├── (raíz)                    scripts sueltos: cámara, carga de escenas, checkpoints, restos (tabla abajo)
├── Cinematics/               CinematicManager (singleton, secuencia con waypoints) + trigger/starter
├── Core/
│   ├── Domain/Puzzles/       IConnectable, IMovablePassenger, TimerController (duraciones del ciclo glitch)
│   ├── Insfrastructure/Services/   AudioListenerHolder; Inputs/ → PlayerInputs.inputactions + .cs GENERADO
│   └── Utilities/            helpers de cámara/shader/input: FollowController, LineOfSightChecker, InputDeviceDetector,
│                             InputPromptDatabase, EaseUtil, CutoutObject, VolumetricLightHole, LevelSelector (debug)…
├── Gameplay/
│   ├── AttachPlayer.cs       lleva al IMovablePassenger con la plataforma (variante simple por LateUpdate)
│   ├── Connection/           Connection (enchufe de nodo) + views de luz/tipo/feedback + TubeLight(Controller)
│   ├── CorruptedCorridor/    CorruptedCorridor — TODO comentado (código muerto)
│   ├── Doors/                DoorController (cuenta conexiones) + DoorsView + listeners + GoToMenu
│   ├── Glitch/               Glitcheable (hub) + GlitchComponent/GlitchState/GlitchTransferManager/GlitchPalette (carga 0..2) + State Machine/ (Idle→Disintegrating→Moving→Reintegrating) + orbit/attraction VFX
│   ├── Laser/                LaserActivator, receptores (LaserReceptor, EnergyLoad, LaserReceptorFill), CristalController,
│   │   └── MVC/              LaserController + LaserModel + LaserView (versión vigente; Laser.cs/AntiCorruptionLaser son la vieja)
│   ├── Nodes/                NodeController (MVC con NodeModel/NodeView), Enums.cs (InteractablePriority, PlayerState, StopType), outline
│   ├── Platform/             PlatformController + Motor + RouteManager + StateMachine/ (Inactive/Waiting/Moving/Returning/ToStop), PlatformTeleport
│   └── Pressure Plate/       PressurePlate + PressuredDoor + PlatesActivator (+ PlatesGlitcheable vacío)
├── HUB Interctables/         InteractablePropController (props con sonido aleatorio del HUB)
├── Interfaces/               IInteractable, IProximityListener, IObstructionChecker, ILaserReceptor, ICorruptionCanceler, IMaterializable, IScreen
├── MainMenu/                 MainMenu (mueve cámara entre paneles, carga nivel) + FadeInController
├── Player/
│   ├── InputManager.cs       singleton: expone InputAction por acción, cambia action maps, rumble
│   ├── CheckPointController.cs
│   ├── Components/           InputHandler (eventos de input), PlayerNodeHandler (nodo en mano), GlitchTransferHandler (Set/Take), PlayerInteractionDetector
│   │                         (trigger + LOS), InteractableHandler (lista + selección), VFX del player
│   ├── MVC/                  PlayerController (hub) + PlayerModel (movimiento/dash/teleport) + PlayerView (anim/sfx/ps)
│   └── StateMachine/         IPlayerState + PlayerStateMachine + Empty/Grab/Dissolving/Teleport/Cinematic
├── Scriptable Objects/       PlayerData, GlitchSounds (+ sus .asset)
├── UI/                       HUD (UpdatePosToTarget/UpdateCrosshair, InputPromptIcon, UIActionController), pausa
│   │                         (PauseGameController vigente; PauseMenu legacy), botones/sonidos (UIButtonsManager), post-fx (GlobalVolumeController)
│   ├── Screen Manager/       ScreenManager (stack de IScreen) + ScreenPause + Config — sistema de pausa alternativo
│   └── Tutorials/            TutorialAnimController
└── Utilities/                LightMovement (luz que respira, con editor custom), SFXManager (singleton de SFX)

Fuera de Scripts/ (también indexados en §11):
Assets/Editor/                PrefabPainter (ventana + scene tool + palette), ObjectReplacerWindow, LightMovementEditor
Assets/Tools/                 FolderTool/ (HierarchyFolder + editores), MapCreator/ (MapBuilderTool + palette)
Assets/Inspection System/     Inspection/, Corruption/, UI/ — sistema de inspección (ver §5)
Assets/Art/, Assets/Art-2/    scripts de shaders/render features/VFX
Assets/Scenes/TEST LEVELS/Mati/Scripts/   prototipo de cinemática de nave
```

Scripts sueltos en la raíz de `Scripts/`:

| Archivo | Rol | Carpeta donde encajaría |
|---|---|---|
| `CameraFocusManager.cs` | Al entrar el player en el trigger, tweenea el `LookAt` de la Cinemachine hacia `_newTarget` | `Core/Utilities` |
| `CameraMovementController.cs` | Rotación de cámara con mouse/stick limitada en X/Y | `Core/Utilities` |
| `PlayerCameraManager.cs` | Cambia prioridad/target de cámara al entrar en un trigger | `Core/Utilities` |
| `LevelLoader.cs` | **Singleton `DontDestroyOnLoad`**: carga async con pantalla de ascensor (`elevatorAnim`) y fade; `SetScene(name)`, evento `OnLoading` | `Core/Insfrastructure/Services` |
| `SceneTransition.cs` | Transición por shader (`_Progress`) + `LoadNextLevel()` por buildIndex; tecla `P` de debug | `Core/Utilities` |
| `SceneLoader.cs` | ⚠️ vacío (plantilla) | borrar |
| `Entity.cs` | ⚠️ vacío (plantilla) | borrar |
| `DarkRoomTest.cs` | ⚠️ test: activa/desactiva `CurrentRoom`/`PreviousRoom` | `TEST LEVELS` |
| `PreassurePlate.cs` | ⚠️ placa vieja por BoxCast con `UnityEvent OnLoaded/OnUnloaded` (no confundir con `Gameplay/Pressure Plate/PressurePlate.cs`) | `Gameplay/Pressure Plate` o borrar |
| `RespawnPosition.cs` | Trigger que llama `player.SetCheckPointPos(transform.position)` | `Player` |
| `FogShader_Controller.cs` | Setea parámetros del shader de niebla (offsets, colores, noise) | `Core/Utilities` |
| `TextGlitch.cs` | Efecto de texto glitcheado para tutoriales (`ShowText`, `ReturnWithGlitch`) | `UI/Tutorials` |

## 5. Sistemas y flujos

### Player (MVC + FSM)

**Componentes** (todos en el prefab del jugador):
- `PlayerController` (MonoBehaviour, **singleton `Instance`**, `IMovablePassenger`, `ILaserReceptor`) — hub: crea `PlayerModel`, `PlayerView`, `PlayerStateMachine`, `InteractableHandler`, `LineOfSightChecker`; conecta `InputHandler`; expone la *STATE MACHINE API* (`PickUpNode`, `ReleaseNode`, `SetPos`, `SetTeleport`, `SetCollisions`, `StartTeleport`, `Teleport`…).
- `PlayerModel` (clase plana) — movimiento con `CharacterController` relativo a la cámara (`FollowController.GetCameraBasis`), gravedad, dash con coyote time y CD (coroutines), teleport tweenado, `OnPlatformMoving`, modo cinemático.
- `PlayerView` (clase plana) — animator, partículas (walk/orbit/default/corrupted/teleport), sonidos, materiales (`UpdatePlayerMaterials`), `GrabNode`.
- `InputHandler` — traduce `InputManager` a eventos `OnMove/OnDash/OnInteractStart/OnInteractCancel/OnSetStart/OnSetCancel/OnTakeStart/OnTakeCancel/OnCancelSelect/OnDebug`. `EnableInputs` es idempotente (flag `_hooked`) y al desenganchar emite los `canceled` pendientes.
- `PlayerNodeHandler` (**singleton**) — el nodo en la mano: `Pick/Release`, `CurrentLevel` y `CurrentGlitch` **derivados** del nodo (sin copia local), `OnNodeGrabbed`, `OnNodeLevelChanged`, corrupción temporal (`BeginCorruption`, 5 s; su evento `OnAbsorbCorruption` lo escucha `PostProcessController`).
- `GlitchTransferHandler` — lo agrega `PlayerController.Awake` si falta. Traduce `Set` (nodo→objeto) y `Take` (objeto→nodo) en `GlitchTransferManager.ExecuteTransfer`. Al apretar solo valida (`Preview`; error + rumble inmediato si falla); soltar antes de `transferHoldDelay` (`PlayerData`) = tap, 1 carga; cumplir `transferHoldDelay` = mueve de una vez `MaxTransferable` (1 o 2 niveles, sin paso intermedio) y el release posterior se ignora. Objetivo = `PlayerController.FindTransferTarget` filtrado por capacidad. Sin nodo en mano → error + rumble. El hold se cancela sin transferir por objetivo perdido/tapado, nodo soltado/enchufado o muerte.
- `PlayerInteractionDetector` (hijo con SphereCollider trigger + Rigidbody kinemático) — **único dueño** del registro de interactuables: descubre por trigger, revalida por frame contra los colliders reales, calcula línea de visión (`IObstructionChecker`) con debounce y notifica `IProximityListener.OnPlayerProximity`.
- `InteractableHandler` — lista + set de bloqueados; `GetInteractable` filtra por `CanInteract` y elige por `InteractablePriority` desc y distancia; `GetClosestGlitcheable(pos, filter)` **no** filtra por `CanInteract` (por eso los `Glitcheable` siguen siendo targeteables).
- VFX: `DissolvingManager` (shader `_DissolveAmount` al teletransportarse), `PlayerDashParticlesManager`, `BackPackColorChange`, `GlitchAttractionController` (VFX Graph hacia el glitcheable cercano cuando se lleva nodo corrupto).

**Ciclo por frame** (`PlayerController.Update`): input → `_model.OnUpdate` → `View.Walk` → `StateMachine.Tick()` → `GetClosestGlitcheable()` (emite `OnGlitcheableInArea`) → `OnInteractableDetected(target)` (solo Interact: nodo/conexión) y `OnGlitcheableDetected(ResolveGlitchHudTarget())` (Set/Take); los consumen `UpdatePosToTarget`/`UIActionController` según su `UITargetSource`.
**Al apretar interactuar**: `OnInteractPressed` → `_interactableHandler.GetInteractable` → `StateMachine.CurrentState.HandleInteraction(target)` + `OnInteractableSelected` (lo escuchan `InspectionPlayerManager`, `InspectorController`).

**FSM** (`PlayerStateMachine`, estados cacheados, `IPlayerState { Enter, HandleInteraction, Tick, Cancel, Exit }`):

| Estado | Entra cuando | Sale hacia |
|---|---|---|
| `PlayerEmptyState` | Inicio; al soltar/enchufar un nodo; al volver de cinemática sin estado previo | `Grab` (interactuó con `NodeController` OK); queda en Empty tras `PlatformTeleport` (SetPos directo) o `Inspectionable`. Soporta hold (`RequiresHoldInteraction`) con cancelación si se pierde LOS |
| `PlayerGrabState` | `TransitionToGrabState(node)` (hace `PickUpNode`) | `Empty` (enchufó en `Connection` → `ReleaseNode`; o interacción fallida → `DropNode`); `Dissolving` (usó `PlatformTeleport`). Interact ya no glitchea: el glitch va por `GlitchTransferHandler` |
| `PlayerDissolvingState` | Desde Grab (teleport) o desde Teleport (revertir) — alterna `_isDissolving` | `Teleport` cuando termina de disolver; `LastState` cuando termina de revertir |
| `PlayerTeleportState` | Tras disolver | `Dissolving` cuando `PlayerController.OnTeleported` |
| `PlayerCinematicState` | `CinematicManager.OnRequestEnterCinematicState` | `LastState`/`Empty` en `OnRequestExitCinematicState`; navega waypoints con `StartWaypointNavigation` |

**Muerte/respawn**: `RespawnPlayer(CauseOfDeath)` (láser: `LaserReceived`; caída: trigger tag `Void`) → `OnDied` → `GlitchDeathController.Instance.TriggerGlitch()` → `SetRespawnPos(_checkPointPos)` → `OnRespawned`. Checkpoints: `CheckPointController` (cápsula con VFX) y `RespawnPosition`.

**Conexión con otros sistemas**: `LevelLoader.OnLoading` deshabilita inputs; `CinematicManager` pide entrar/salir del estado cinemático y `CinematicTrigger` pide `OnCinematicSetupRequested` (parent + layer + CC off); `InspectionSystem.Instance.CanRotate` gatea `OnCancelSelect`.

### Inputs

- `InputManager` (**singleton**, componente `PlayerInput` + clase generada `PlayerInputs`) expone `InputAction`s públicas: **Player** (`Move, Rotate, Dash, Interact, Set, Take, Pause, ResetCam, CameraLeft, CameraRight, Debug`; Interact = E / ✕, Set = click izq. / □, Take = click der. / ○), **UI** (`Point, Click, RightClick, Rotate, ResetRotation, Cancel, Pause`), **PauseUI** (navegación estándar de UI). `UpdateActionMap(ActionMaps)` cambia de mapa recordando el último no-pausa; `RumblePulse/StopRumble` para gamepad; `SetControlScheme`.
- `InputDeviceDetector` (ScriptableObject) detecta teclado/gamepad (`OnDeviceChanged`); `InputPromptDatabase` (SO) mapea `PromptAction` × dispositivo → sprite; `InputPromptIcon` los muestra en UI.
- `PlayerInputs.cs` es **generado** por el Input System: editar el `.inputactions`, no el `.cs`.

### Glitch (FSM por objeto)

- `Glitcheable` (`IInteractable` prioridad `Highest`, `IProximityListener`, `[DefaultExecutionOrder(-1)]`) crea `GlitchStateMachine` y los 4 estados en `Awake`, encadenados con `SetNext`: `Idle → Dis`, `Dis → Mov` (interrupción → `Idle`), `Mov → Rei`, `Rei → Dis` (interrupción → `Idle`). El estado inicial sale del nivel de su `GlitchComponent` (`Glitched` arranca el ciclo; `Clean`/`Intangible` quedan en `Idle`); `_isPlatform` cambia el manejo de parent/espacio local. Duraciones desde `TimerController` (`TransparencyDuration`, `MoveDuration`). `IsCorrupted = FSM.Current != IdleState`.
- **Carga (0..2)**: `GlitchState {Clean=0, Intangible=1, Glitched=2}`. `GlitchComponent` (en cada nodo y cada glitcheable) es la **única fuente de verdad**: el nivel es el campo serializado `_currentLevel` (válido desde la deserialización, necesario por el `DefaultExecutionOrder(-1)`), `OnGlitchStateChanged` solo se emite en cambios (el inicial se lee por pull en `Start`), `TryApplyDelta` rechaza en vez de clampear, `Ensure(go)` lo agrega en runtime en `Clean` con warning si falta en el prefab. `GlitchTransferManager` (estático) es el **único mutador**: `ExecuteTransfer(source, target)` mueve 1 carga de forma atómica y conserva la suma; `Preview` valida sin efectos. `GlitchPalette` (SO en `Resources/GlitchPalette.asset`, acceso por `GlitchPalette.Default`, sin cableado) es la única fuente de colores: `ColorFor` para UI/partículas, `EmissionFor` (× `_emissionIntensity`) para emisivos. Los shaders siguen siendo binarios: el nivel 1 se distingue por color.
- **Reconciliación**: `Glitcheable.SyncFsmWithLevel` está suscrito a `OnGlitchStateChanged` **y** a `FSM.OnStateChanged`: nivel 2 en `Idle` → `BeginCycle()`; nivel < 2 fuera de `Idle` → `Interrupt()` si el estado es `IGlitchInterruptible`. `GlitchMovingState` no lo es, así que una descarga en movimiento se aplica al entrar a `Reintegrating`. Idempotente, con guard `_syncing`.
- **Intangibilidad**: objeto en 1 + nodo en mano en 1 → `_coll.isTrigger = true` (nunca `enabled = false`, que lo sacaría del detector). Se evalúa por pull en `Update`; `ApplyPhysicsState` es el único que escribe el collider (`SetColliders` de la FSM solo registra lo que pide). Volver a sólido espera a que la cápsula del jugador salga.
- **Interacción**: `CanInteract => false`. Sigue siendo `IInteractable` solo para que `PlayerInteractionDetector` lo registre y `GetClosestGlitcheable` lo devuelva; su `Priority.Highest` queda inerte. El jugador lo afecta con `Set`/`Take` (`GlitchTransferHandler`), y el HUD lo recibe por su propia señal `OnGlitcheableDetected` (`PlayerController.ResolveGlitchHudTarget`).

| Estado | Entra cuando | Sale hacia |
|---|---|---|
| `GlitchIdleState` (`IGlitchInterruptible`) | Inicio con nivel < 2, o interrupción desde Dis/Rei (el nivel bajó de 2). Alpha 1, colliders on, handrails off, si es plataforma se parenta al ancla actual | `DisState` por `Interrupt()` |
| `GlitchDisintegratingState` (`IGlitchInterruptible`) | Inicio con nivel 2, desde Rei, o `BeginCycle()` (el nivel subió a 2). Holograma on, partículas, `startSFX`, alpha 1→0 | `MovState` al terminar (apaga colliders); `IdleState` por `Interrupt()` |
| `GlitchMovingState` | Tras desintegrar. Lerp pos/rot hacia `CurrentTarget` (ancla `_newPosList[_index]`); `AdvanceToNextNode()` al llegar | `ReiState` |
| `GlitchReintegratingState` (`IGlitchInterruptible`) | Tras mover. Colliders on, `endSFX`, alpha →1, `HologramSwitch(false)` | `DisState` al terminar; `IdleState` por `Interrupt()` |

- `GlitchStateMachine.OnStateChanged` lo escuchan `LaserController` (apaga/prende el láser durante Dis/Mov) y las views de orbit.
- Visual: `GlitcheableOrbitController`/`GlitcheableOrbitAnimator` (orbe que rota y escala según proximidad/estado), `GhostWalls` (paredes que solo existen si el jugador lleva nodo corrupto), `CristalController` (shader `_GlitchAmount`).
- `PlatesGlitcheable : Glitcheable` está vacío (todo comentado); `GlitcheableDetector` (OverlapSphere) ya no lo usa nadie — el detector vigente es `PlayerInteractionDetector`.

### Nodes y Connections

- `NodeController` (`IInteractable` prioridad `High`, `IProximityListener`) — MVC con `NodeModel` (posición/parent/escala) y `NodeView` (outline `Outline`, animator, partículas, shader de desintegración). `Interact` → `Attach(..., parentIsPlayer:true)` lo cuelga del `AttachTransform` del jugador. El nivel vive en su `GlitchComponent` (`Level`, `Glitch`); escucha `OnGlitchStateChanged`, resuelve color con `GlitchPalette.Default.ColorFor` y re-emite `OnUpdatedNodeType(GlitchState)`. `Attach` sin player lo deja en una `Connection` (`IConnectable`) o en el piso, y **re-anuncia** su collider al detector (`RescanInteractable`).
- `Connection` (`IInteractable` `Medium`, `IConnectable`, `IProximityListener`) — recibe un nodo (`SetNode`), compara con `_requiredLevel` (hoy todas en `Clean`) y emite `OnNodeConnected(GlitchState, bool)`; `OnPlayerProximity` exige `HasNode`; `UnsetNode` al sacarlo. `OnInitialized` en `Start` (lo usa `PlatformController` para construir su FSM), `StartsConnected` si ya tenía nodo serializado, `OnAvailableToConnect` para feedback de proximidad.
- Views que escuchan a `Connection`: `ConnectionLightView`, `ConnectionTypeView`, `ConnectionPositiveFeedback`, `ConnectionNegativeFeedback`, `ConnectionLightListener`, `ParedFillConnection` (relleno de pared), `EmissiveListener`, `TubeLightController` → secuencia de `TubeLight` (eventos `OnFilled/OnEmptied/OnTransitionCompleted`).
- Enums compartidos en `Gameplay/Nodes/Enums.cs`: `InteractablePriority {Low..MaxPriority}`, `PlayerState`, `StopType` (sin uso). El viejo `NodeType` se reemplazó por `GlitchState` (`Gameplay/Glitch/GlitchState.cs`); no hay valor `None`: "sin nodo" se pregunta con `PlayerNodeHandler.HasNode`.

### Doors y Pressure Plates

- `DoorController` — lista de `Connection`; cuenta las conectadas con el `_requiredLevel` requerido y abre (`DoorsView.OpenDoor`, evento `OnOpen`) cuando **todas** lo están. `DoorLightListener` colorea luces, `DetectPlayerToOpen` anima puertas de proximidad (`IsPlayerNear`), `GoToMenu` carga escena al abrirse.
- Placas: `PressurePlate` (trigger del player → `OnPlayerPressed`, con CD al salir) → `PressuredDoor` cuenta placas y abre (`Animator "Open"`, `OnTaskFinished`). `PlatesActivator` (singleton) emite `OnActivatePlates` al entrar/salir de una zona. `ResetPressurePlate` con `UnityEvent`s.

### Laser

- **Versión vigente (MVC)**: `LaserController` (MonoBehaviour) + `LaserModel` (distancia con transición, `ProcessReceptor` notifica `ILaserReceptor.LaserReceived/LaserNotReceived` al cambiar de objetivo) + `LaserView` (LineRenderer instanciado, partículas beam/hit, audio). Raycast por frame desde `transform.forward` con `_layer`. Si cuelga de un `Glitcheable`, escucha `FSM.OnStateChanged`: idle/reintegrando → prendido, desintegrando/moviendo → se apaga con transición. Si `_startsInitialized == false`, solo emite cuando su padre `ILaserReceptor` (un `LaserActivator`) recibe láser.
- `LaserActivator` (`ILaserReceptor`) — repite `LaserReceived/NotReceived` a todos los `LaserController` hijos; `LaserStartsInitialized` lo lee `CristalController` para el brillo del cristal.
- Receptores: `LaserReceptor` (carga con `_GlowStep`, `UnityEvent OnHit/OnEndHit/OnCompleated/OnDepleated`), `EnergyLoad` (carga/descarga por coroutine, `brotherReceptor` para pares), `LaserReceptorFill`, `LaserReceptorChecker` (todos completos → `OnChecked`). `PlayerController` también es `ILaserReceptor`: el láser lo mata.
- **Legacy**: `Laser.cs` y `AntiCorruptionLaser` (usa `ICorruptionCanceler`, cuerpo comentado) — implementación anterior no-MVC, todavía en prefabs viejos. `ReceptorBroderChecker` está vacío.

### Platform

- `PlatformController` — ruta de `StationsStops[] _positions2` (posición + `IsStation`) → `RouteManager` (índice, dirección, ping-pong o `isReversed`) + `PlatformMotor` (MoveTowards y arrastre del `IMovablePassenger`). Se activa por una `Connection`: en `OnInitialized` construye `PlatformStateMachine(this, StartsConnected)` y escucha `OnNodeConnected` → `ToMoving/ToWaiting` si el tipo coincide, `ToStop`/`ToInactive` si no. Velocidad con tweens PrimeTween (`StartAccelerationToMax`, `DecelerateToTarget`…). El pasajero se toma por `OnTriggerEnter` (`IMovablePassenger`, se limpia en `OnDied`).

| Estado | Entra cuando | Sale hacia |
|---|---|---|
| `InactiveState` | Inicio sin conexión; conexión incorrecta desde Waiting/Inactive; fin de ToStop/Returning | `Waiting`/`Moving` por `OnNodeConnected` |
| `WaitingState` | Inicio conectada; llegó a una estación (`AdvanceRouteAndWait`) | `Moving` al cumplir `WaitCD` |
| `MovingState` | Conexión correcta / fin de espera. Acelera, frena antes de estaciones (`CheckStop`) | `Waiting` en estación; sigue en Moving en puntos intermedios |
| `ToStopState` | Se desconecta en movimiento: decelera hasta el punto actual | `Inactive` (marca `isStopped`) |
| `ReturningState` | (no se llama desde el código actual) fuerza reversa hasta el inicio | `Inactive` |

- `PlatformTeleport` (`IInteractable` `MaxPriority`) — requiere nodo `Glitched` (2 cargas); expone `TargetPos` de la plataforma destino y partículas entrada/salida por proximidad. `AttachPlayer` es una alternativa mínima para arrastrar al jugador con cualquier transform.
- `FractureColorController` colorea fragmentos; `EmissiveListener` refleja la conexión en emisivos.

### UI, pausa y HUD

- **Pausa vigente**: `PauseGameController` (canvas en escena) — `InputManager.pauseInput.started` alterna `Time.timeScale`, cambia a `ActionMaps.PauseUI`, selecciona `_resumeBtn`, `CanvasButtonStateController.Enable/DisableButtons`, `UIButtonsManager.PlaySoundOpenMenu`. Botones: `ResumeGame`, `RestartLevel`, `GoToScene` (vía `LevelLoader`), `GoToDesktop`.
- **Pausa alternativa (Screen Manager)**: `ScreenManager` (singleton, stack de `IScreen {Activate, Deactivate, Free}`; `Push(string)` instancia desde `Resources`), `ScreenPause` (`BTN_*`), `Config` (escucha `pauseInput` e instancia `_pauseMenuPrefab`). `PauseMenu` es una tercera versión legacy con `Input.GetKeyDown(Escape)`. Elegir **una** por escena.
- HUD: `UpdatePosToTarget` (posiciona un RectTransform sobre `OnInteractableDetected` u `OnGlitcheableDetected` según `UITargetSource`, con `UIScaleTween`) → `UpdateCrosshair` (siempre Glitcheable; color según glitch), `UIActionController` (modo Interactable: icono + texto Put/Grab; modo Glitcheable: panel padre con hijos Set/Take que se escalan por separado), `InputPromptIcon` (sprite según dispositivo), `LookAtCamera`, `LightFlicker`, `GlobalVolumeController` (aberración/lens distortion según audio), `DirectionalButtonListener`, `CanvasController` (cambio de canvas para navegación por gamepad), `TutorialAnimController`.
- `UIButtonsManager` (singleton) — sonidos de botones. `SFXManager` (singleton, `Utilities/`) — `PlaySFX/PlayRandomSFX` instanciando `AudioSource`s.

### Cinemáticas

- `CinematicManager` (singleton; `CinematicSequence` serializable con waypoints antes/después, cámaras Cinemachine con prioridades, trigger de animación, `PlayerData`). `StartCinematic()` → `OnCinematicStarted` → cámara cinemática → `OnRequestEnterCinematicState` (Player entra a `PlayerCinematicState`) → navega `waypointsToTrigger` → espera `OnTriggerReached` (lo llama `CinematicTrigger` al entrar el player, tras pedir `OnCinematicSetupRequested` y disparar el animator del padre) → `OnAnimationComplete()` (AnimationEvent) → `RestorePlayerPhysics` → `waypointsAfterAnimation` → `EndCinematic` (`OnRequestExitCinematicState`, `OnCinematicEnded`).
- `CinematicStarter` arranca por `Start` (con delay) o por trigger.

### Carga de escenas y checkpoints

- `LevelLoader` (singleton `DontDestroyOnLoad`) — `SetScene(name)`: fade → `OnLoading` → `LoadSceneAsync` con `allowSceneActivation=false` mientras muestra el ascensor (`elevatorAnim` triggers `NewStartLoading/IsLoading/LoadingComplete`, mínimo `minLoadingDuration`) → fade. `PerformDebug()` carga `debugScene` (input `Debug`). Lo usan `MainMenu`, `PauseGameController`, `GoToMenu`.
- `SceneTransition` (shader) y `LevelSelector` (teclas 1–0 para saltar a checkpoints) son utilidades de test.

### Inspección (Assets/Inspection System/)

- `Inspectionable` (`IInteractable` `Low`, con `CorruptionGenerator`) → al seleccionarlo, `InspectionPlayerManager` (cámara de inspección) prende `InspectionSystem` (rotar el objeto con mouse/stick, `ResetRot`), bloquea al player (`SetCanMove(false)`) y pasa a `ActionMaps.UI`. `InspectorController` (singleton) activa el `UIInspectionable` del mismo `InspectionType` (`Panel`/`Battery`) y le pasa el generador. `CorruptionRemover` (singleton, cámara UI + `GamepadCursor`) raycastea `Corruption` en la RawImage y las elimina manteniendo click (`_holdTimer`, rumble + impulso de cámara) → `CorruptionGenerator.RemoveCorruption` → `OnObjectCleaned` → `Inspectionable.CorruptionCleaned` (`OnFinished`, `OnCleaned`). `ScannerController.OnScanFinished` cierra la inspección. `DoorInspectionController`, `ParticlesFeedbackManager`, `ApproachToTarget`, `UpdateCameras`, `InspectionCanvasController` son soporte visual.

### Shaders y VFX de soporte (Art / Art-2)

- `GlitchDeathController` (singleton) — pantalla glitch al morir. `ShaderFiller` — relleno progresivo con `UnityEvent`s. `SolvingController`/`PosToPosParticles` — tutorial de dissolve (legacy). `PostProcessController`, `ToonOutlineFeature`, `ToonDepthNormalsFeature`, `CustomRenderPassFeature` — render features URP. `TextGlitcher`/`UITextGlitcher` — texto glitcheado (duplican `TextGlitch`). `StutterTimePerRenderer` — `_SteppedTime` por renderer. `Scroller` — scroll de RawImages. `Outline` — QuickOutline (asset de tienda).

## 6. Interfaces y contratos

| Interfaz | Miembros | La implementan | La consumen |
|---|---|---|---|
| `IInteractable` (`Interfaces/`) | `Transform`, `InteractablePriority Priority`, `bool RequiresHoldInteraction`, `CanInteract(PlayerNodeHandler)`, `Interact(PlayerNodeHandler, out bool)` | `NodeController` (High), `Connection` (Medium), `Glitcheable` (Highest, pero `CanInteract => false`), `PlatformTeleport` (MaxPriority), `InteractablePropController` (Low), `Inspectionable` (Low) | `PlayerInteractionDetector`, `InteractableHandler`, estados del player, `UpdatePosToTarget` |
| `IProximityListener` | `OnPlayerProximity(bool inRange, PlayerController)` | `NodeController`, `Connection`, `Glitcheable`, `PlatformTeleport` | `PlayerInteractionDetector.Dispatch` |
| `IObstructionChecker` | `IsObstructed(from, Transform)`, `IsObstructed(from, Transform, aimPoint)` | `LineOfSightChecker` | `PlayerInteractionDetector` |
| `ILaserReceptor` | `LaserReceived()`, `LaserNotReceived()` | `LaserActivator`, `LaserReceptor`, `PlayerController`, (`LaserController`/`Laser` exponen los métodos sin declarar la interfaz) | `LaserModel.ProcessReceptor`, `Laser.ProcessHit` |
| `IConnectable` (`Core/Domain/Puzzles`) | `UnsetNode(NodeController)` | `Connection` | `NodeController.Attach` |
| `IMovablePassenger` (`Core/Domain/Puzzles`) | `OnPlatformMoving(Vector3)` | `PlayerController` | `PlatformMotor`, `PlatformController`, `AttachPlayer` |
| `ICorruptionCanceler` | `CorruptionCancel()`, `CorruptionRestore()`, `CorruptionCheck()` | *(nadie)* | `AntiCorruptionLaser` (legacy) |
| `IMaterializable` | `Materialize(bool)` | *(nadie)* | *(nadie)* |
| `IScreen` | `Activate()`, `Deactivate()`, `string Free()` | `ScreenPause` | `ScreenManager` |
| `IPlayerState` (`Player/StateMachine`) | `Enter(PlayerController, PlayerNodeHandler)`, `HandleInteraction(IInteractable)`, `Tick()`, `Cancel()`, `Exit()` | `PlayerEmptyState`, `PlayerGrabState`, `PlayerDissolvingState`, `PlayerTeleportState`, `PlayerCinematicState` | `PlayerStateMachine` |
| `IState` (`Glitch/State Machine`) | `Enter()`, `Tick(float)`, `Exit()` | `GlitchIdleState`, `GlitchDisintegratingState`, `GlitchMovingState`, `GlitchReintegratingState` | `GlitchStateMachine`, `LaserController` |
| `IGlitchInterruptible` | `Interrupt()` | `GlitchIdleState`, `GlitchDisintegratingState`, `GlitchReintegratingState` | `Glitcheable.SyncFsmWithLevel`, `Laser` |
| `IPlatformState` (`Platform/StateMachine`) | `Enter()`, `Tick(float)`, `Exit()` | `InactiveState`, `WaitingState`, `MovingState`, `ReturningState`, `ToStopState` | `PlatformStateMachine` |
| `PlayerInputs.IPlayerActions/IUIActions/IPauseUIActions` (generadas) | callbacks por acción | *(nadie; se usan las `InputAction` directas vía `InputManager`)* | — |

## 7. Convenciones

- **MVC por sistema**: el `*Controller` es el `MonoBehaviour` (serializa refs, crea Model y View en `Awake`); `*Model` y `*View` son clases planas construidas con lo que necesitan (`PlayerModel(CC, transform, data, collider)`, `LaserView` es la excepción: MonoBehaviour hermano). Carpeta `MVC/` cuando los tres archivos existen.
- **FSM**: interfaz de estado + clase máquina plana que **cachea instancias** en el constructor (`PlayerStateMachine`, `PlatformStateMachine`) o en el `Awake` del dueño (`Glitcheable` encadena con `SetNext`). Transiciones por métodos `ToX()`/`TransitionToX()`; `Tick` desde el `Update` del MonoBehaviour dueño.
- **Eventos**: `public Action<...> OnX` (a veces `= delegate { }`), `public event Action` en los más nuevos; suscripción/desuscripción simétrica en `Start/OnDestroy` o `OnEnable/OnDisable`. Los `UnityEvent` públicos quedan para cablear desde el inspector (láser receptores, `ShaderFiller`, `SceneTransition`).
- **Singletons** por `public static X Instance` asignado en `Awake` (`PlayerController`, `PlayerNodeHandler`, `InputManager`, `LevelLoader` (con `DontDestroyOnLoad`), `CinematicManager`, `ScreenManager`, `UIButtonsManager`, `SFXManager`, `PlatesActivator`, `InspectionSystem`, `InspectorController`, `CorruptionRemover`, `ScannerController`, `GamepadCursor`, `GlitchDeathController`). Se consultan directo (`PlayerController.Instance.X`) — el README pide evitarlo en puzzles, pero es el patrón real.
- **Interacción**: nada llama `OnTriggerEnter` para detectar al jugador desde el objeto; se implementa `IInteractable` + `IProximityListener` y el `PlayerInteractionDetector` hace el resto. Si un objeto apaga/prende su collider o cambia de layer, se re-anuncia con `PlayerController.Instance.RescanInteractable(this, coll)`. Quien "consume" un interactuable lo saca con `player.RemoveInteractable`.
- **Naming**: campos privados `_camelCase` (con excepciones viejas sin `_`), `[SerializeField] private` en vez de campos públicos (los públicos que quedan son legacy), `[Header]`/`[Tooltip]` en español, propiedades `PascalCase` con `=>` o `{ get; private set; }`. Nombres de carpetas/escenas en español y con espacios; hay typos que **se conservan**: `Insfrastructure`, `HUB Interctables`, `Iluiminacion`, `PreassurePlate`, `AntiCorruptionLser`, `ReceptorBroderChecker`, `_recievedNode`.
- **Enums compartidos** en `Gameplay/Nodes/Enums.cs`; enums locales al final del archivo de su clase (`CauseOfDeath`, `ActionMaps`, `PlateType`, `InspectionType`).
- **Tweens** con PrimeTween (`Tween.Custom`, `Tween.Alpha`, `Tween.Delay`, `Ease`), nunca coroutines nuevas para interpolar; coroutines solo para secuencias con esperas. `EaseUtil.InOutQuad` para lerps manuales en los estados glitch.
- **Shaders**: se parametrizan por `material.SetFloat/SetColor` con nombres `_Alpha`, `_IsCorrupted`, `_GlitchAmount`, `_IsInitialized`, `_DissolveAmount`, `_EmissiveColor`, `_GlowStep`, `_Progress`.
- **Comentarios** en español explicando el *por qué* (ver `PlayerInteractionDetector`, `NodeController.Attach`, `LevelLoader`); mantener ese estilo al tocar esos archivos.
- **Código muerto/legacy que convive**: `Laser`/`AntiCorruptionLaser` vs `LaserController`; `PauseMenu` vs `PauseGameController` vs `ScreenManager`; `PreassurePlate` vs `PressurePlate`; `GlitcheableDetector`; `CorruptedCorridor`; `SceneLoader`/`Entity`/`ReceptorBroderChecker` vacíos. Antes de extender algo, confirmar cuál versión está en los prefabs de `BUILD SCENES`.
- El `README.md` de la raíz describe una arquitectura anterior (`TaskManagers`, `PlayerTDController`, `GenericConnectionController`, `UnityInputAdapter`) que **no existe en el código actual**; sus reglas de §6 (Controller ≠ lógica visual, no `new` estados en `Tick`, suscripción simétrica) siguen siendo la intención del equipo.

## 8. Packages y plugins

| Package | Versión | Para qué se usa | Dónde |
|---|---|---|---|
| `com.unity.render-pipelines.universal` | 17.3.0 | Render (URP, Render Graph) | `Assets/Settings`, `RenderPipeline`, render features en `Art/Shaders/Post-Process`, `Art-2/AssetsFer/ShadersFer` |
| `com.cqf.urpvolumetricfog` | git (CristianQiu/Unity-URP-Volumetric-Light) | Niebla volumétrica | `VolumetricLightHole`, volúmenes de escena |
| `com.unity.cinemachine` | 3.1.6 | Cámara orbital del player, cámaras de cinemática, impulsos | `FollowController`, `CameraFocusManager`, `PlayerCameraManager`, `CinematicManager`, `PlayerController` (impulse), `Inspection System/UI` |
| `com.unity.inputsystem` | 1.18.0 | Input, action maps, rumble | `InputManager`, `PlayerInputs` (generado), `InputHandler`, `InputDeviceDetector`, `GamepadCursor` |
| `com.kyrylokuzyk.primetween` | tgz local (`Assets/Plugins/PrimeTween`) | Tweens sin alloc | `PlatformController`, `LevelLoader`, `UIScaleTween` (HUD), `InspectionSystem`, views varias (`Ease`) |
| `com.unity.visualeffectgraph` | 17.3.0 | VFX Graph | `GlitchAttractionController`, `PosToPosParticles`, `SolvingController` |
| `com.unity.timeline` | 1.8.10 | Timeline del ascensor | `Assets/Timelines` |
| `com.unity.probuilder` | 6.0.8 | Geometría de prototipado en escenas | escenas/test levels |
| `com.unity.ugui` | 2.0.0 | UI (Canvas, Button, Image) | `UI/`, `Inspection System/UI` |
| `com.unity.formats.fbx` | 5.1.5 | Import/export FBX | `Art/FBX` |
| `com.unity.2d.sprite` | 1.0.0 | Sprites de prompts | `Art-2/Input Prompts` |
| `com.unity.feature.development` | 1.0.2 | Herramientas de desarrollo (profiler, test framework…) | editor |
| `com.unity.collab-proxy` | 2.11.2 | Version control UI de Unity (no se usa: el repo es git) | — |
| TextMesh Pro | (incluido en ugui 2.0) | Textos | `TextGlitch`, `UIActionController`, `TextGlitcher` |
| QuickOutline | asset de tienda en `Art/Store Meshes/QuickOutline` | Outline de nodos | `NodeView`, `OutlineController` |

## 9. Herramientas de editor

### Prefab Painter — `Assets/Editor/PrefabPainter.cs` + `Editor/PrefabPainter/`
Menú **Tools → Prefab Painter**. `PrefabPainterWindow` (en `PrefabPainter.cs`; la copia en `PrefabPainter/PrefabPainterWindow.cs` está comentada) usa `PrefabPainter.uxml/.uss` y delega en `PrefabPainterSceneTool` (pinta en SceneView con snap `Grid`/`Chain`, alineación a normal, yaw aleatorio, preview). Paleta: `PrefabPainterPalette` (SO, **Create → Tools → Prefab Painter → Palette**).

### Map Builder — `Assets/Tools/MapCreator/`
Menú **Tools → Map Builder**. `MapBuilderTool` (EditorWindow) coloca prefabs de módulos desde `MapBuilderPalette` (SO, **Create → Tools → Map Builder → Palette**; instancia `NewMapBuilderPalette.asset`).

### Object Replacer — `Assets/Editor/ObjectReplacerWindow.cs`
Menú **Tools → Object Replacer**: reemplaza los GameObjects seleccionados por instancias de `_prefab` conservando transform.

### LightMovement inspector — `Assets/Editor/LightMovementEditor.cs`
`[CustomEditor(typeof(LightMovement))]`: previsualiza la curva de rango de la luz que "respira".

### Hierarchy Folder — `Assets/Tools/FolderTool/`
Menú **GameObject → Hierarchy Folder** (`HierarchyFolderEditor`, `[InitializeOnLoad]`). `HierarchyFolder` (`[OnlyScript]`, namespace `Tools.FolderTool`) es un contenedor visual con color; `OnlyScriptProcessor` borra cualquier otro componente que se le agregue; `FolderSceneProcessor` (`IProcessSceneWithReport`) **desparenta los hijos y elimina las carpetas al buildear**, así no llegan al juego.

### Menús de creación de assets
`PlayerData`, `GlitchSounds` (**Create → PlayerData / GlitchSounds**), `InputDeviceDetector` y `InputPromptDatabase` (**Create → Systems → …**).

## 10. Cómo encontrar cosas

- **Para agregar un estado al jugador** → crear `Player/StateMachine/PlayerXState : IPlayerState`, instanciarlo y cachearlo en `PlayerStateMachine` (constructor) y agregar `TransitionToX()`; la lógica de qué hacer con cada `IInteractable` va en `HandleInteraction`.
- **Para hacer un objeto interactuable** → implementar `IInteractable` (+ `IProximityListener` si quiere feedback de rango) y darle un collider **sólido**; no hace falta trigger propio. Elegir `InteractablePriority` según qué debe ganar cuando hay varios al alcance. Si consume la interacción, el estado del player debe llamar `player.RemoveInteractable`.
- **Para el nivel de glitch de un nodo u objeto** → `GlitchComponent._currentLevel` en el prefab o como override de la instancia en escena (0 `Clean`, 1 `Intangible`, 2 `Glitched`). Colores en `Resources/GlitchPalette.asset` (todos los consumidores la leen); shaders binarios en `PlayerView.PlayNodePS` y `CristalNodeView`; lo que pide una conexión, `Connection._requiredLevel`.
- **Para mover carga de glitch** → solo `GlitchTransferManager.ExecuteTransfer(source, target)`; nunca `TryApplyDelta` directo. Input y feedback en `Player/Components/GlitchTransferHandler.cs`, tiempos en `PlayerData` (`transfer*`).
- **Para un objeto glitcheable nuevo** → prefab con `Glitcheable` + `GlitchComponent` (nivel 2 = arranca ciclando) + `TimerController` + `Collider` + `Renderer` con shader que tenga `_Alpha`/`_IsCorrupted`, lista `_newPosList` de anclas (cada ancla puede tener `MeshRenderer` = holograma), `GlitchSounds`. Si es plataforma, `_isPlatform`.
- **Para cambiar el ciclo glitch (duraciones, orden)** → `TimerController` (duraciones) y el encadenado `SetNext` en `Glitcheable.Awake`.
- **Para un láser que se prende con otro láser** → emisor con `LaserController` (`_startsInitialized=false`) hijo de un objeto con `LaserActivator`; el activador es lo que recibe el rayo. Cristal: `CristalController`.
- **Para que algo reaccione a un láser** → implementar `ILaserReceptor` en el objeto con collider en la `_layer` del emisor.
- **Para una plataforma móvil** → `PlatformController` con `_positions2` (transforms hijos, `IsStation` donde debe esperar) y una `Connection` que la habilite; el jugador viaja porque es `IMovablePassenger`.
- **Para una puerta** → `DoorController` + `DoorsView` con la lista de `Connection`s (todas deben tener el `_requiredLevel`), o `PressuredDoor` + `PressurePlate[]`.
- **Para cargar otra escena** → `LevelLoader.Instance.SetScene("nombre")` (nombre exacto del `.unity`), y agregarla a *Build Settings* (`EditorBuildSettings.asset`) si es nueva.
- **Para un checkpoint** → `CheckPointController` (cápsula con VFX) o `RespawnPosition` (trigger simple).
- **Para una cinemática** → `CinematicManager` en escena con un `CinematicSequence` (cámaras, waypoints, trigger de animación) + `CinematicStarter` (arranca) + `CinematicTrigger` (parenta al player y dispara animación; el AnimationEvent llama `AnimationEnded`).
- **Para un input nuevo** → editar `PlayerInputs.inputactions` (regenera `PlayerInputs.cs`), exponer la `InputAction` en `InputManager.OnEnable`, y si es del player, un evento en `InputHandler`. Prompt en pantalla: agregar `PromptAction` en `InputPromptDatabase` + sprites en `InputPromptDatabase.asset`.
- **Para un sonido puntual** → `SFXManager.Instance.PlaySFX(clip, parent, volume)`; sonidos del player en `PlayerData`; de UI en `UIButtonsManager`; del glitch en `GlitchSounds`.
- **Para tunear al jugador** (velocidad, dash, radio de interacción, LOS, rumble) → `Scripts/Scriptable Objects/PlayerData.asset`.
- **Para el menú de pausa** → `PauseGameController` en el canvas de la escena (vigente). No mezclar con `ScreenManager`/`PauseMenu`.
- **Para un HUD que siga a un interactuable** → heredar de `UpdatePosToTarget` (ver `UpdateCrosshair`).
- **Para inspección/corrupción** → `Assets/Inspection System/`: `Inspectionable` en el objeto del mundo + `UIInspectionable` del mismo `InspectionType` bajo `InspectorController`.
- **Para pintar módulos de nivel** → Tools → Prefab Painter / Map Builder (§9); organizar la jerarquía con GameObject → Hierarchy Folder.
- **Para un shader de glitch/dissolve** → parámetros en §7; controladores en `Art-2/Shaders/ShaderScript` y `Player/Components/DissolvingManager`.
- **Para buscar quién emite/escucha un evento** → `Grep "OnNombre"` en `Assets/Scripts`; los `Action` públicos están listados por clase en §11.

## 11. Índice de API pública

Formato de columnas: **SerializeFields** (solo nombre; `pub` = campo público serializado) · **Públicos**: métodos `M()`, props `P`, eventos `ev`.

### Scripts/ (raíz)

| Clase | Archivo | Hereda / Implementa | SerializeFields | Públicos | Notas |
|---|---|---|---|---|---|
| `CameraFocusManager` | `Scripts/CameraFocusManager.cs` | MonoBehaviour | `_camera` (CinemachineCamera), `_newTarget`, `_verticalOffset`, `_transitionTime`, `_easeType` | — | Tween de LookAt al entrar en trigger |
| `CameraMovementController` | `Scripts/CameraMovementController.cs` | MonoBehaviour | `_sensitivity`, `_easeType`, `_horizontalLimit`, `_YLimit`, `_NegativeYLimit` | — | Rotación de cámara limitada |
| `DarkRoomTest` | `Scripts/DarkRoomTest.cs` | MonoBehaviour | `PreviousRoom`, `CurrentRoom` | — | ⚠️ test |
| `Entity` | `Scripts/Entity.cs` | MonoBehaviour | — | — | ⚠️ vacío |
| `FogShader_Controller` | `Scripts/FogShader_Controller.cs` | MonoBehaviour | `OffsetX1/Y1/X2/Y2`, `RemapMin/Max`, `TopColor`, `BottomColor`, `Depth`, `NoiseScale(2)`, `HeightIntensity` | — | Parámetros de shader de niebla |
| `LevelLoader` | `Scripts/LevelLoader.cs` | MonoBehaviour | `loadingAnimator`, `loadingFade` (Image), `elevatorAnim`, `minLoadingDuration`, `fadeDuration`, `levelChangeDelay`, `initialFadeDelay`, `debug`, `debugScene` | `static Instance`; `ev Action OnLoading`; `SetScene(string)`, `PerformDebug()` | Singleton DontDestroyOnLoad, carga async |
| `PlayerCameraManager` | `Scripts/PlayerCameraManager.cs` | MonoBehaviour | `_camera`, `_trigger`, `_tweenType`, `_duration` | — | Cámara por zona |
| `PreassurePlate` | `Scripts/PreassurePlate.cs` | MonoBehaviour | pub `boxSize`, `castDistance`, `_boxCastOrigin`, `obstacleLayers`; `charging`, `uncharging`, `timeToFill`, `timeToUnfill`, `fillAmount` | `UnityEvent OnLoaded/OnUnloaded`; `StartCharging()`, `StartUnCharging()` | ⚠️ legacy, typo en nombre |
| `RespawnPosition` | `Scripts/RespawnPosition.cs` | MonoBehaviour | — | — | Trigger → `SetCheckPointPos` |
| `SceneLoader` | `Scripts/SceneLoader.cs` | MonoBehaviour | — | — | ⚠️ vacío |
| `SceneTransition` | `Scripts/SceneTransition.cs` | MonoBehaviour | `screenTransitionMaterial`, `transitionTime`, `propertyName` | `UnityEvent OnTransitionDone/OnTransitionStart`; `LoadNextLevel()` | Tecla P de debug |
| `TextGlitch` | `Scripts/TextGlitch.cs` | MonoBehaviour | `_tutorialText`, `glitchDuration`, `glitchInterval`, `randomChars` | `ShowText()`, `ReturnWithGlitch()` | Texto de tutorial |

### Scripts/Cinematics

| Clase | Archivo | Hereda / Implementa | SerializeFields | Públicos | Notas |
|---|---|---|---|---|---|
| `CinematicSequence` | `Scripts/Cinematics/CinematicManager.cs` | `[Serializable]` clase | pub `waypointsToTrigger[]`, `waypointsAfterAnimation[]`, `cinematicCamera`, `gameplayCamera`, `cinematicCameraPriority`, `gameplayCameraPriority`, `cameraAnimTrigger`, `playerData` | — | Datos de una cinemática |
| `CinematicManager` | `Scripts/Cinematics/CinematicManager.cs` | MonoBehaviour | `_currentSequence` | `static Instance {get}`; `ev OnCinematicStarted/OnCinematicEnded/OnRequestEnterCinematicState/OnRequestExitCinematicState`; `StartCinematic()`, `StartCinematic(CinematicSequence)`, `OnTriggerReached(PlayerController, bool)`, `OnAnimationComplete()`, `IsPlayingCinematic()` | Singleton |
| `CinematicStarter` | `Scripts/Cinematics/CinematicStarter.cs` | MonoBehaviour | `_startOnTriggerEnter`, `_startOnStart`, `_delayBeforeStart` | `StartCinematic()` | |
| `CinematicTrigger` | `Scripts/Cinematics/CinematicTrigger.cs` | MonoBehaviour | `_parentObject`, `_parentAnimator`, `_animationTriggerName`, `_cinematicLayer` | `Action OnAnimationCompleted`; `ResetTrigger()` | `AnimationEnded()` privado, llamado por AnimationEvent |

### Scripts/Core

| Clase | Archivo | Hereda / Implementa | SerializeFields | Públicos | Notas |
|---|---|---|---|---|---|
| `IConnectable` | `Scripts/Core/Domain/Puzzles/IConnectable.cs` | interfaz | — | `UnsetNode(NodeController)` | |
| `IMovablePassenger` | `Scripts/Core/Domain/Puzzles/IMovablePassenger.cs` | interfaz | — | `OnPlatformMoving(Vector3)` | |
| `TimerController` | `Scripts/Core/Domain/Puzzles/TimerController.cs` | MonoBehaviour | `_transparencyDuration`, `_moveDuration` | `TransparencyDuration`, `MoveDuration`; `ResetToBaseline()` | Duraciones del ciclo glitch |
| `AudioListenerHolder` | `Scripts/Core/Insfrastructure/Services/AudioListenerHolder.cs` | MonoBehaviour | `_cameraTarget` | — | Sigue al target |
| `PlayerInputs` | `Scripts/Core/Insfrastructure/Services/Inputs/PlayerInputs.cs` | `IInputActionCollection2, IDisposable` (partial) | — | `asset`, `Player`, `UI`, `PauseUI` (structs de acciones), `IPlayerActions/IUIActions/IPauseUIActions` | **GENERADO** por Input System, no editar |
| `ChangeRenderQueue` | `Scripts/Core/Utilities/ChangeRenderQueue.cs` | MonoBehaviour | `customRenderQueue` | — | |
| `CutoutObject` | `Scripts/Core/Utilities/CutoutObject.cs` | MonoBehaviour | `_target`, `_cutoutMat`, `_layerMask`, `_targetCutoutSize`, `_timeModifier` | — | Recorte de paredes entre cámara y player |
| `EaseUtil` | `Scripts/Core/Utilities/EaseUtil.cs` | static class | — | `static InOutQuad(float)` | |
| `FollowController` | `Scripts/Core/Utilities/FollowController.cs` | MonoBehaviour | `lookAtObject`, `easeTime` | `CurrentYaw`; `GetCameraBasis(out forward, out right)` | Cinemachine OrbitalFollow; base de movimiento del player |
| `freeCursor` | `Scripts/Core/Utilities/freeCursor.cs` | MonoBehaviour | — | — | Oculta cursor |
| `InputDeviceDetector` | `Scripts/Core/Utilities/InputDeviceDetector.cs` | ScriptableObject | `_currentDevice` | `enum DeviceType`; `ev Action<DeviceType> OnDeviceChanged`; `CurrentDevice`; `Initialize()`, `Dispose()`, `ForceRefresh()` | SO `Core/Utilities/InputDeviceDetector.asset` |
| `InputPromptDatabase` | `Scripts/Core/Utilities/InputPromptDatabase.cs` | ScriptableObject | `_entries` (List<PromptEntry>) | `enum PromptAction` (`…, Cancel, Set, Take`); `class PromptEntry {action, keyboardMouse, xbox, playStation, nintendoSwitch, genericGamepad}`; `GetSprite(PromptAction, DeviceType)` | SO `InputPromptDatabase.asset` |
| `LevelSelector` | `Scripts/Core/Utilities/LevelSelector.cs` | MonoBehaviour | `_checkpoints` | `RestarLevel()` | ⚠️ debug: teclas 1–0 |
| `LineOfSightChecker` | `Scripts/Core/Utilities/LineOfSightChecker.cs` | `IObstructionChecker` | — | `HeightOffset`; `IsObstructed(from, Transform)`, `IsObstructed(from, Transform, aimPoint)` | Ctor `(wallMask, heightOffset, endMargin)` |
| `MaterialPosSetter` | `Scripts/Core/Utilities/MaterialPosSetter.cs` | MonoBehaviour | `targetMinMaxPos` | — | |
| `PlaySound` | `Scripts/Core/Utilities/PlaySound.cs` | MonoBehaviour | `audioClip`, `exitClip`, `volume`, `hasExitClip` | — | Sonido por trigger |
| `VolumetricLightHole` | `Scripts/Core/Utilities/VolumetricLightHole.cs` | MonoBehaviour | `_radius`, `_falloff`, `_fadeSpeed` | — | Agujero en la niebla volumétrica |

### Scripts/Gameplay (raíz, Connection, CorruptedCorridor, Doors)

| Clase | Archivo | Hereda / Implementa | SerializeFields | Públicos | Notas |
|---|---|---|---|---|---|
| `AttachPlayer` | `Scripts/Gameplay/AttachPlayer.cs` | MonoBehaviour | — | — | Arrastra `IMovablePassenger` por LateUpdate |
| `Connection` | `Scripts/Gameplay/Connection/Connection.cs` | MonoBehaviour, `IInteractable`, `IConnectable`, `IProximityListener` | `_recievedNode`, `_nodePos`, `_requiredLevel`, `_emissionOff/Correct/Incorrect` (HDR), `connectDelay` | `Priority=Medium`, `Transform`, `RequiresHoldInteraction=false`, `RequiredLevel`, `StartsConnected`, `IsConnected`; `Action OnInitialized`, `Action<GlitchState,bool> OnNodeConnected`, `Action<bool> OnAvailableToConnect`; `CanInteract`, `Interact`, `UnsetNode`, `OnPlayerProximity` | Enchufe de nodo |
| `ConnectionLightListener` | `Scripts/Gameplay/Connection/ConnectionLightListener.cs` | MonoBehaviour | `_enabledColor`, `_disabledColor` | — | |
| `ConnectionLightView` | `Scripts/Gameplay/Connection/ConnectionLightView.cs` | MonoBehaviour | `_connection`, `_tweenType`, `_lightDefault/Corrupted/Off` | pub `_keepOn`; `SetCorrectNode(GlitchState, bool)` | Luz de la conexión |
| `ConnectionNegativeFeedback` | `Scripts/Gameplay/Connection/ConnectionNegativeFeedback.cs` | MonoBehaviour | `_connection` | — | |
| `ConnectionPositiveFeedback` | `Scripts/Gameplay/Connection/ConnectionPositiveFeedback.cs` | MonoBehaviour | `_connection`, `_defaultColor`, `_corruptedColor` | — | |
| `ConnectionTypeView` | `Scripts/Gameplay/Connection/ConnectionTypeView.cs` | MonoBehaviour | `_connection`, `_tweenType`, `_emissionDefault/Corrupted/Off` | pub `_keepOn`; `SetCorrectNode(GlitchState, bool)` | Emisivo según tipo |
| `TubeLight` | `Scripts/Gameplay/Connection/TubeLight.cs` | MonoBehaviour | (privados) | `ev OnFilled/OnEmptied`, `ev Action<TubeLight> OnTransitionCompleted`; `IsFull`, `IsEmpty`; `PlayFill(bool)`, `TurnOn()`, `TurnOff()`, `SetInstant(bool)` | Tubo de luz individual |
| `TubeLightController` | `Scripts/Gameplay/Connection/TubeLightController.cs` | MonoBehaviour | `_autoReverseOnComplete`, `_tubes` | `Index`; `StartSequence(bool fill)` | Secuencia de tubos al conectar |
| `CorruptedCorridor` | `Scripts/Gameplay/CorruptedCorridor/CorruptedCorridor.cs` | MonoBehaviour | — | — | ⚠️ todo comentado |
| `DetectPlayerToOpen` | `Scripts/Gameplay/Doors/DetectPlayerToOpen.cs` | MonoBehaviour | — | — | Animator bool `IsPlayerNear` |
| `DoorLightListener` | `Scripts/Gameplay/Doors/DoorLightListener.cs` | MonoBehaviour | `_defaultColor`, `_openColor`, `_lerpTime`, `_lerpType` | — | |
| `DoorController` | `Scripts/Gameplay/Doors/DoorsController.cs` | MonoBehaviour | `_requiredLevel` (GlitchState), `_connections` | — | Nombre de archivo ≠ clase |
| `DoorsView` | `Scripts/Gameplay/Doors/DoorsView.cs` | MonoBehaviour | `_isOpen`, `_isBroken`, `_doorLight` (List<Renderer>) | `Action<bool> OnOpen`; `Initialize()`, `OpenDoor(bool)` | |
| `GoToMenu` | `Scripts/Gameplay/Doors/GoToMenu.cs` | MonoBehaviour | `door` (DoorsView), `scene` | — | Carga escena al abrir |

### Scripts/Gameplay/Glitch

| Clase | Archivo | Hereda / Implementa | SerializeFields | Públicos | Notas |
|---|---|---|---|---|---|
| `GhostWalls` | `Scripts/Gameplay/Glitch/GhostWalls.cs` | MonoBehaviour | `_minMaxPos` | pub `playerHasCorruption`, `IsDisolving`; `ResetDesintegrateWall()`, `StopDesintegrateWall()`, `SetDesintegrateWall(float)` | Pared visible solo con nodo corrupto |
| `GlitchAttractionController` | `Scripts/Gameplay/Glitch/GlitchAttractionController.cs` | MonoBehaviour | — | — | VFX de atracción (hijo del player) |
| `GlitchComponent` | `Scripts/Gameplay/Glitch/GlitchComponent.cs` | MonoBehaviour (`[DisallowMultipleComponent]`) | `_currentLevel` (GlitchState) | `const MaxLevel=2`; `CurrentState`, `CurrentLevel` (int); `CanGive()`, `CanReceive()`; `event Action<GlitchState> OnGlitchStateChanged`; `static Ensure(GameObject)`, `TryApplyDelta(int)` | Fuente de verdad del nivel. No emite en Awake/Start |
| `GlitchPalette` | `Scripts/Gameplay/Glitch/GlitchPalette.cs` | ScriptableObject (`[CreateAssetMenu]`) | `_clean`, `_intangible`, `_glitched` (HDR), `_emissionIntensity` | `static Default` (Resources.Load); `ColorFor(GlitchState)`, `EmissionFor(GlitchState)` | Asset en `Resources/GlitchPalette.asset` |
| `GlitchState` | `Scripts/Gameplay/Glitch/GlitchState.cs` | enum | — | `Clean=0, Intangible=1, Glitched=2` | Reemplaza a `NodeType` |
| `GlitchTransferManager` | `Scripts/Gameplay/Glitch/GlitchTransferManager.cs` | static class | — | `Preview(source, target)`, `MaxTransferable(source, target)` → int, `ExecuteTransfer(source, target)` (1 carga), `ExecuteTransfer(source, target, amount)` → `GlitchTransferResult` | Único mutador de niveles; conserva la suma; `amount` en un solo delta por extremo |
| `GlitchTransferResult` | `Scripts/Gameplay/Glitch/GlitchTransferManager.cs` | enum | — | `Transferred, NoSource, NoTarget, SourceEmpty, TargetFull, Invalid` | |
| `Glitcheable` | `Scripts/Gameplay/Glitch/Glitcheable.cs` | MonoBehaviour, `IInteractable`, `IProximityListener` | pub `handrails[]`, `_coll`, `_ps`, `_newPosList`, `_sounds`, `_radialDonutPS`, `_feedbackRenderer`; `_isPlatform`, `debug`, `feedbackMinMaxPS`, `_defaultLayer`, `_glitchedLayer` | `Priority=Highest` (inerte), `IsPlatform`, `FSM`, `IdleState/DisState/MovState/ReiState`, `CurrentTarget/Pos/Rot`, `IsCorrupted`, `Glitch`, `Level`; `Action<PlayerController,bool> OnPlayerInRange`, `Action OnInteractionRejected`; `HologramSwitch(bool)`, `BeginCycle()`, `CanInteract` (=> false), `Interact`, `SetAlpha`, `SetFeedbackAlpha`, `SetBoolCorrupted`, `SetParticles(bool,float)`, `PlaySfx`, `SetColliders(bool)`, `AdvanceToNextNode()`, `OnPlayerProximity` | Hub del sistema glitch. Intangible (trigger) solo con el jugador en rango (`IsInInteractionRange`, por pull) |
| `GlitcheableDetector` | `Scripts/Gameplay/Glitch/GlitcheableDetector.cs` | clase plana | — | `GetNearestGlitcheable(Vector3)` | ⚠️ sin uso |
| `GlitcheableOrbitAnimator` | `Scripts/Gameplay/Glitch/GlitcheableOrbitAnimator.cs` | MonoBehaviour | `primaryAxis`, `primarySpeed`, `secondaryAxis`, `secondarySpeed`, `fadeTime`, `fadeEase`, `debug` | — | Rotación del orbe |
| `GlitcheableOrbitController` | `Scripts/Gameplay/Glitch/GlitcheableOrbitController.cs` | MonoBehaviour | `psType`, `scaleTime`, `upScale`, `downScale`, `interactionUp/DownScale`, `bounceDuration`, `bounceDelay`, `bounceSettleEaseType`, `scaleEaseType`, `debug` | `Action<bool> OnPlayerInRange` | Escala del orbe. `PSType {Idle, Corrupted, Intangible}`; sin orbe Intangible, el Idle se tiñe con la paleta en nivel 1 |
| `GlitchStateMachine` | `Scripts/Gameplay/Glitch/State Machine/GlitchStateMachine.cs` | clase plana | — | `Action<IState> OnStateChanged`; `Current`; `Change(IState)`, `Tick(float)` | |
| `IState` | `…/State Machine/IState.cs` | interfaz | — | `Enter()`, `Tick(float)`, `Exit()` | |
| `IGlitchInterruptible` | `…/State Machine/IGlitchInterruptible.cs` | interfaz | — | `Interrupt()` | |
| `GlitchIdleState` | `…/State Machine/GlitchIdleState.cs` | `IState`, `IGlitchInterruptible` | — | `SetNext(IState)`, `Enter/Tick/Exit`, `Interrupt()` | |
| `GlitchDisintegratingState` | `…/State Machine/GlitchDisintegratingState.cs` | `IState`, `IGlitchInterruptible` | — | `Enter/Tick/Exit`, `ResetAndReturn()`, `SetNext(nextNormal, nextInterrupt)`, `Interrupt()` | |
| `GlitchMovingState` | `…/State Machine/GlitchMovingState.cs` | `IState` | — | `SetNext(IState)`, `Enter/Tick/Exit` | Espacio local si tiene parent |
| `GlitchReintegratingState` | `…/State Machine/GlitchReintegratingState.cs` | `IState`, `IGlitchInterruptible` | — | `SetNext(next, nextInterrupt)`, `Enter/Tick/Exit`, `Interrupt()` | |

### Scripts/Gameplay/Laser

| Clase | Archivo | Hereda / Implementa | SerializeFields | Públicos | Notas |
|---|---|---|---|---|---|
| `AntiCorruptionLser` | `Scripts/Gameplay/Laser/AntiCorruptionLaser.cs` | `Laser` | — | pub `_stoppedObjects`, `_hittedObjects` (List<ICorruptionCanceler>); `Action OnCollition` | ⚠️ legacy, typo, cuerpo comentado |
| `CristalController` | `Scripts/Gameplay/Laser/CristalController.cs` | MonoBehaviour | — | `SetGlitcheableCristalView()`, `SetBrightnes()` | Shader `_GlitchAmount`, `_IsInitialized` |
| `EnergyLoad` | `Scripts/Gameplay/Laser/EnergyLoad.cs` | MonoBehaviour | `_currentLoad`, `_timeToLoad`, `_timeToUnload`, `brotherReceptor` | `UnityEvent OnLoaded/OnBrotherCompleted/OnUnloaded/OnLoading/OnUnloading`; pub `isFirst`, `isLast`, `_isCurrentlyLoading`, `_isCurrentlyUnloading`, `isFull`; `StartLoading()`, `StartUnloading()`, `StopLoading()`, `Unload()`, `TotalUnload()`, `BrotherCheck()` | Receptor con carga por tiempo |
| `Laser` | `Scripts/Gameplay/Laser/Laser.cs` | MonoBehaviour | `_laserRendererPrefab`, `_beamLaser`, `_hitLaser`, `_maxDist`, `_raycastOffsetX/Z`, `_startsInitialized`, `_easeTime`, `_laserLayer`, `debug`, `debugObject` | `LaserReceived()`, `LaserNotReceived()`; `protected virtual CorruptionCheck()`, `CollitionCheck(RaycastHit)` | ⚠️ legacy (no MVC) |
| `LaserActivator` | `Scripts/Gameplay/Laser/LaserActivator.cs` | MonoBehaviour, `ILaserReceptor` | — | `LaserStartsInitialized`; `LaserReceived()`, `LaserNotReceived()` | Reenvía a `LaserController`s hijos |
| `LaserReceptor` | `Scripts/Gameplay/Laser/LaserReceptor.cs` | MonoBehaviour, `ILaserReceptor` | `minBound`, `maxBound`, `unfillTime`, `fillTime`, `duration`, `_completeFillPartcle`, `_FinishFillParticle`, `_hitPS` | `UnityEvent OnEndHit/OnHit/OnCompleated/OnDepleated`; pub `_isCompleted`, `_completed`, `_currentLoad`, `_canBeUnfilled`; `ChargeCompleted()`, `SetUnCompleted()`, `ChargeDepleted()`, `LaserReceived()`, `TurnOffObject()`, `LaserNotReceived()`, `Fill()`, `Empty()` | Shader `_GlowStep` |
| `LaserReceptorChecker` | `Scripts/Gameplay/Laser/LaserReceptorChecker.cs` | MonoBehaviour | `_laserReceptor` (List) | `UnityEvent OnChecked/OnNotChecked`; `ReceptorCheck()` | Todos completos |
| `LaserReceptorFill` | `Scripts/Gameplay/Laser/LaserReceptorFill.cs` | MonoBehaviour | `Ymin`, `Ymax`, `unfillTime`, `fillTime` | pub `_completed`; `UnityEvent OnLoaded/OnUnloaded`; `Fill()`, `SetFull()`, `SetDepleated()`, `UnFill()` | |
| `ReceptorBroderChecker` | `Scripts/Gameplay/Laser/ReceptorBroderChecker.cs` | MonoBehaviour | — | — | ⚠️ vacío |
| `LaserController` | `Scripts/Gameplay/Laser/MVC/LaserController.cs` | MonoBehaviour | `_maxDist`, `_offsetZ`, `_easeTime`, `_layer`, `_startsInitialized`, `_debug`, `_gizmosColor` | `StartsInitialized`; `LaserReceived()`, `LaserNotReceived()` | Emisor vigente; escucha FSM del `Glitcheable` padre |
| `LaserModel` | `Scripts/Gameplay/Laser/MVC/LaserModel.cs` | clase plana | — | `MaxDistance`, `LaserLayer`, `CurrentDist`, `TargetDist`, `IsTransitioning`; `ProcessReceptor(RaycastHit, ILaserReceptor own)`, `ClearReceptor()`, `SetLaserLength(float)`, `SetInstant(float)`, `UpdateRaycastDistance()` | |
| `LaserView` | `Scripts/Gameplay/Laser/MVC/LaserView.cs` | MonoBehaviour | `_lineRendererPrefab`, `_hitLaser`, `_beamLaser` | `Init(float width)`, `SetLaserPositions(start, end)`, `ShowHitEffect(pos, normal)`, `StopHitEffect()`, `EnableBeam(bool)`, `PlayAudio()`, `StopAudio()` | |

### Scripts/Gameplay/Nodes

| Clase | Archivo | Hereda / Implementa | SerializeFields | Públicos | Notas |
|---|---|---|---|---|---|
| `CristalNodeView` | `Scripts/Gameplay/Nodes/CristalNodeView.cs` | MonoBehaviour | `_effectNode` | `Awake()` (público) | Emisión desde `GlitchPalette` |
| `PlayerState` | `Scripts/Gameplay/Nodes/Enums.cs` | enum | — | `Default, Corrupted` | sin uso |
| `StopType` | `Scripts/Gameplay/Nodes/Enums.cs` | enum | — | `None, TimeStop, CorruptedStop` | sin uso |
| `InteractablePriority` | `Scripts/Gameplay/Nodes/Enums.cs` | enum | — | `Low=0, Medium, High, Highest, MaxPriority=4` | |
| `NodeController` | `Scripts/Gameplay/Nodes/NodeController.cs` | MonoBehaviour, `IInteractable`, `IProximityListener` | `_desintegrationShader` | `Priority=High`, `CurrentColor`, `Transform`, `RequiresHoldInteraction=false`, `Glitch`, `Level`; `Action<GlitchState> OnUpdatedNodeType`, `Action<bool> OnEnableOutline`; `CanInteract`, `Interact`, `Attach(newPos, newParent=null, newScale=default, parentIsPlayer=false, newRot=default)`, `OnPlayerProximity`, `StartDesintegrateShader()`, `SetDesintegrateShader(float)`, `StopDesintegrateShader()` | |
| `NodeModel` | `Scripts/Gameplay/Nodes/NodeModel.cs` | clase plana | — | `SetPos(newPos, GlitchState, newParent=null, newScale=default, newRot=default)` | |
| `NodeView` | `Scripts/Gameplay/Nodes/NodeView.cs` | clase plana | — | `IsReseting`; `OnStart()`, `UpdateNodeType(GlitchState, Color)`, `EnableOutline(bool)`, `EnableColl(bool)`, `SetIdleAnim()`, `SetCollectedAnim()`, `SetRangeAnim()`, `StartDisintegrate(Shader, Vector2, alpha=1)`, `SetDisintegrateAlpha(float)`, `StopDisintegrate(Shader)` | |
| `OutlineController` | `Scripts/Gameplay/Nodes/OutlineController.cs` | MonoBehaviour | — | — | Color desde `GlitchPalette.EmissionFor` |
| `ParedFillConnection` | `Scripts/Gameplay/Nodes/ParedFillConnection.cs` | MonoBehaviour | `_connection`, `duration` | `Fill()`, `Empty()` | Relleno de pared al conectar |

### Scripts/Gameplay/Platform

| Clase | Archivo | Hereda / Implementa | SerializeFields | Públicos | Notas |
|---|---|---|---|---|---|
| `EmissiveListener` | `Scripts/Gameplay/Platform/EmissiveListener.cs` | MonoBehaviour | `_connection`, `_enabledColor`, `_disabledColor`, `_lerpTime` | — | |
| `FractureColorController` | `Scripts/Gameplay/Platform/FractureColorController.cs` | MonoBehaviour | `_parent`, `_colors` | — | |
| `StationsStops` | `Scripts/Gameplay/Platform/PlatformController.cs` | `[Serializable]` clase | pub `Position` (Transform), `IsStation` | — | |
| `PlatformController` | `Scripts/Gameplay/Platform/PlatformController.cs` | MonoBehaviour | `_moveTime`, `_accelTime`, `_decelTime`, `_waitCD`, `_accelEase`, `_decelEase`, `_connection`, `_positions2[]` | pub `myDictionary`, `isStopped`, `isReversed`; `CurrentSpeed`, `SegmentSpeed`, `MoveTime`, `WaitCD`, `DecelTime`, `AccelTime`, `WaitTimer {get;set}`, `Passenger`, `Motor`, `Route`, `CurrentTarget`; `SetPositiveFeedback(bool)`, `AdvanceRoute()`, `AdvanceRouteAndWait()`, `BeginWait()`, `StopPassenger()`, `ReachedTarget()`, `CheckStop()`, `MoveStep()`, `CancelSpeedTween()`, `RefreshSegmentSpeed()`, `StartAccelerationToMax()`, `ContinueWithNewSegmentSpeed()`, `StartDeceleration(float)`, `DecelerateToTarget(Vector3)`, `StopImmediate()` | API interna usada por los estados |
| `PlatformMotor` | `Scripts/Gameplay/Platform/PlatformMotor.cs` | clase plana | — | `InTarget(target, speed=0)`, `MoveTowards(target, speed, passenger=null)`, `Stop(passenger)` | |
| `PlatformTeleport` | `Scripts/Gameplay/Platform/PlatformTeleport.cs` | MonoBehaviour, `IInteractable`, `IProximityListener` | `_targetPlatform`, `_entrada`, `_salida` (PS), `_heightThershold` | `Priority=MaxPriority`, `TargetPlatform`, `TargetPos`; `Action<bool> OnPlayerStepped`; `CanInteract`, `Interact`, `OnPlayerProximity` | Requiere nodo `Glitched` |
| `RouteManager` | `Scripts/Gameplay/Platform/RouteManager.cs` | clase plana | — | `IsValid`, `CurrentPoint`, `AtStart()`, `HasToWait(bool)`, `TravelDir`; `Advance()`, `ForceReverse()` | Ctor `(Vector3[], PlatformController)` |
| `IPlatformState` | `…/StateMachine/IPlatformState.cs` | interfaz | — | `Enter()`, `Tick(float)`, `Exit()` | |
| `PlatformStateMachine` | `…/StateMachine/PlatformStateMachine.cs` | clase plana | — | `Inactive`, `Waiting`, `Moving`, `Returning`, `Tostop`, `Current`, `Last`; `Tick(float)`, `TransitionTo(IPlatformState)`, `ToInactive()`, `ToWaiting()`, `ToMoving()`, `ToReturning()`, `ToStop()` | ⚠️ el ctor llama `TransitionTo` antes de crear los estados |
| `InactiveState` | `…/StateMachine/InactiveState.cs` | `IPlatformState` | — | `Enter/Tick/Exit` | Ctor `(PlatformController)` |
| `WaitingState` | `…/StateMachine/WaitingState.cs` | `IPlatformState` | — | `Enter/Tick/Exit` | Ctor `(PlatformController, PlatformStateMachine)` |
| `MovingState` | `…/StateMachine/MovingState.cs` | `IPlatformState` | — | `Enter/Tick/Exit` | Ctor `(PlatformController)`; frena antes de estaciones |
| `ReturningState` | `…/StateMachine/ReturningState.cs` | `IPlatformState` | — | `Enter/Tick/Exit` | Ctor `(PlatformController, PlatformStateMachine)`; sin llamadas actuales |
| `ToStopState` | `…/StateMachine/ToStopState.cs` | `IPlatformState` | — | `Enter/Tick/Exit` | Ctor `(PlatformController, PlatformStateMachine)` |

### Scripts/Gameplay/Pressure Plate

| Clase | Archivo | Hereda / Implementa | SerializeFields | Públicos | Notas |
|---|---|---|---|---|---|
| `PlatesActivator` | `Scripts/Gameplay/Pressure Plate/PlatesActivator.cs` | MonoBehaviour | — | `static Instance`; `Action<bool> OnActivatePlates` | Trigger de zona |
| `PlatesGlitcheable` | `Scripts/Gameplay/Pressure Plate/PlatesGlitcheable.cs` | `Glitcheable` | — | — | ⚠️ todo comentado |
| `PlateType` | `Scripts/Gameplay/Pressure Plate/PlatesGlitcheable.cs` | enum | — | `None, Green, Blue, Purple` | sin uso |
| `PressurePlate` | `Scripts/Gameplay/Pressure Plate/PressurePlate.cs` | MonoBehaviour | `_pressureDoor`, `_commonMat`, `_successMat`, `_pressedCD` | `Action<bool> OnPlayerPressed` | |
| `PressuredDoor` | `Scripts/Gameplay/Pressure Plate/PressuredDoor.cs` | MonoBehaviour | `_commonMat`, `_successMat`, `_plates[]`, `_restartCD` | `Action<bool> OnTaskFinished` | Animator bool `Open` |
| `ResetPressurePlate` | `Scripts/Gameplay/Pressure Plate/ResetPressurePlate.cs` | MonoBehaviour | `OnPress`, `OnRealase` (UnityEvent) | `TaskEnd()`, `ResetTask()` | |

### Scripts/HUB Interctables · Interfaces · MainMenu

| Clase | Archivo | Hereda / Implementa | SerializeFields | Públicos | Notas |
|---|---|---|---|---|---|
| `InteractablePropController` | `Scripts/HUB Interctables/InteractablePropController.cs` | MonoBehaviour, `IInteractable` | `commonAudio`, `chanceAudio`, `brokenAudio`, `pitchMinMax`, `audioMinMax`, `randomChance`, `breakChance` | `Priority=Low`, `Transform`, `RequiresHoldInteraction=false`; `CanInteract`, `Interact` | Props del HUB con sonido aleatorio |
| `ICorruptionCanceler` | `Scripts/Interfaces/ICorruptionCanceler.cs` | interfaz | — | `CorruptionCancel()`, `CorruptionRestore()`, `CorruptionCheck()` | sin implementadores |
| `IInteractable` | `Scripts/Interfaces/IInteractable.cs` | interfaz | — | `Transform`, `Priority`, `RequiresHoldInteraction`; `CanInteract(PlayerNodeHandler)`, `Interact(PlayerNodeHandler, out bool)` | |
| `ILaserReceptor` | `Scripts/Interfaces/ILaserReceptor.cs` | interfaz | — | `LaserReceived()`, `LaserNotReceived()` | |
| `IMaterializable` | `Scripts/Interfaces/IMaterializable.cs` | interfaz | — | `Materialize(bool)` | sin uso |
| `IObstructionChecker` | `Scripts/Interfaces/IObstructionChecker.cs` | interfaz | — | `IsObstructed(from, Transform)`, `IsObstructed(from, Transform, aimPoint)` | |
| `IProximityListener` | `Scripts/Interfaces/IProximityListener.cs` | interfaz | — | `OnPlayerProximity(bool, PlayerController)` | |
| `IScreen` | `Scripts/Interfaces/IScreen.cs` | interfaz | — | `Activate()`, `Deactivate()`, `string Free()` | |
| `FadeInController` | `Scripts/MainMenu/FadeInController.cs` | MonoBehaviour | pub `blackScreen` (Image), `fadeDuration` | — | |
| `MainMenu` | `Scripts/MainMenu/MainMenu.cs` | MonoBehaviour | `_tweentype`, `_duration`, `_blackScreen`, `fadeOutDuration`, `_startPos` | `MoveToPos(Transform)`, `ButtonEnabler(Button)`, `ButtonEnablerRoutine(Button)`, `MoveToPosAndFade(Transform)`, `SetTargetLevel(string)`, `Quit()` | Cámara entre paneles; carga vía `LevelLoader` |

### Scripts/Player

| Clase | Archivo | Hereda / Implementa | SerializeFields | Públicos | Notas |
|---|---|---|---|---|---|
| `CheckPointController` | `Scripts/Player/CheckPointController.cs` | MonoBehaviour | `_vfxRenderer`, `_transitionDuration`, `_enabledMainColor`, `_enabledSecColor`, `_enabledFresnelColor` (HDR), `_objectToActivate` | — | Cápsula de checkpoint |
| `InputManager` | `Scripts/Player/InputManager.cs` | MonoBehaviour | `_deviceDetector` | `static Instance`; `DeviceDetector`; pub `playerInput`, `playerInputs`, `moveInput`, `rotateInput`, `interactInput`, `setInput`, `takeInput`, `dashInput`, `pauseInput`, `resetCamInput`, `cameraRight`, `cameraLeft`, `debugInput`, `rotate`, `click`, `rightClick`, `resetRot`, `cancelInput`; `Action OnInputsEnabled/OnInputsDisabled`; `OnEnable()`, `OnDisable()`, `UpdateToLastActionMap()`, `UpdateActionMap(ActionMaps)`, `SetControlScheme(string)`, `RumblePulse(low, high, duration)`, `RumblePulse(low, high)`, `StopRumble()` | Singleton |
| `ActionMaps` | `Scripts/Player/InputManager.cs` | enum | — | `Player, UI, PauseUI` | |
| `BackPackColorChange` | `Scripts/Player/Components/BackPackColorChange.cs` | MonoBehaviour | — | — | Emisión desde `GlitchPalette.EmissionFor` |
| `DissolvingManager` | `Scripts/Player/Components/DissolvingManager.cs` | MonoBehaviour | — | — | Escucha `OnDissolving` → `_DissolveAmount` |
| `GlitchTransferHandler` | `Scripts/Player/Components/GlitchTransferHandler.cs` | MonoBehaviour (`[DisallowMultipleComponent]`) | — | `Action<GlitchTransferResult, Glitcheable> OnTransferResolved` | Set/Take tap+hold; lo agrega `PlayerController.Awake` |
| `InputHandler` | `Scripts/Player/Components/InputHandler.cs` | MonoBehaviour | — | `ev Action<Vector2> OnMove`, `ev OnDash/OnInteractStart/OnInteractCancel/OnSetStart/OnSetCancel/OnTakeStart/OnTakeCancel/OnCancelSelect/OnDebug`; `EnableInputs()`, `DisableInputs()` | |
| `InteractableHandler` | `Scripts/Player/Components/InteractableHandler.cs` | clase plana | — | `Interactables` (IReadOnlyList); `Add`, `Remove`, `Clear`, `SetLineOfSight(it, bool)`, `HasLineOfSight(it)`, `IsSelectable(it)`, `GetInteractable(PlayerNodeHandler, Vector3)`, `GetClosestGlitcheable(Vector3, Func<Glitcheable,bool> filter=null)` | |
| `PlayerDashParticlesManager` | `Scripts/Player/Components/PlayerDashParticlesManager.cs` | MonoBehaviour | — | — | Escucha `View.OnDashViewPlayed` |
| `PlayerInteractionDetector` | `Scripts/Player/Components/PlayerInteractionDetector.cs` | MonoBehaviour (`[RequireComponent(Rigidbody, SphereCollider)]`) | — | `Radius`; `Initialize(handler, player, obstruction, radius=0, checkInterval=0, debounceTime=0)`, `Rescan(IInteractable, Collider)`, `IsInRange(Collider)`, `Forget(IInteractable)`, `ResetTracking()` | Dueño del registro de interactuables; `IsInRange` es el test geométrico que comparten el barrido y el Glitcheable |
| `PlayerNodeHandler` | `Scripts/Player/Components/PlayerNodeHandler.cs` | MonoBehaviour | `_attachPos`, `_dropPos` | `static Instance`; `CurrentNode`, `CurrentLevel` (derivado), `CurrentGlitch`, `HasNode`, `IsCorrupted`, `AttachTransform`, `AttachPos`; `Action<bool,GlitchState> OnNodeGrabbed`, `Action<bool> OnAbsorbCorruption`, `Action OnNodeLevelChanged`; `Pick(NodeController)`, `Release(isDropping=false)`, `BeginCorruption(Transform, Action<Vector3>)` | |
| `PlayerController` | `Scripts/Player/MVC/PlayerController.cs` | MonoBehaviour, `IMovablePassenger`, `ILaserReceptor` | `_playerData`, `_renderer`, `_walkPS/_orbitPS/_defaultPS/_corruptedPS/_teleportPS`, `_walkSource/_fxSource`, `_followController`, `_interactionDetector`, `debug` | `static Instance`; `CC`, `View`, `StateMachine`, `IsDead`, `TeleportPos`, `NodeHandler`, `Data`; `Action<float,float> OnDash`, `Action<IInteractable> OnInteractableSelected`, `OnPlayerFell`, `OnDied`, `OnRespawned`, `Action<Glitcheable> OnGlitcheableInArea`, `Action<float> OnDissolving`, `OnTeleported`, `Action<IInteractable> OnInteractableDetected` (sin glitcheables), `Action<Glitcheable> OnGlitcheableDetected` (por frame, target de Set/Take), `Action<Transform,LayerMask,bool> OnCinematicSetupRequested`, `Action<LayerMask,bool> OnCinematicRestoreRequested`; `CheckForWalls()`, `PickUpNode`, `ReleaseNode`, `DropNode`, `GetHoldInteractionTime`, `AddInteractable`, `RemoveInteractable`, `RescanInteractable(it, coll)`, `IsInInteractionRange(Collider)` (solo distancia, sin LOS ni registro), `IsInteractableAvailable(it)`, `SetPos`, `SetTeleport`, `GetClosestGlitcheable()`, `FindTransferTarget(Func<Glitcheable,bool>)`, `Dissolving(float)`, `SetCinematicMovement`, `ClearCinematicMovement`, `IsInCinematicMode`, `SetCollisions(bool)`, `StartTeleport()`, `Teleport()`, `PlayTeleportPS()`, `OnPlatformMoving`, `SetCanMove(bool)`, `LaserReceived`, `LaserNotReceived`, `SetCheckPointPos(Vector3)`, `RespawnPlayer(CauseOfDeath)` (IEnumerator), `WalkSound()` | Hub del jugador |
| `CauseOfDeath` | `Scripts/Player/MVC/PlayerController.cs` | enum | — | `Teleport, Fall, Laser` | |
| `PlayerModel` | `Scripts/Player/MVC/PlayerModel.cs` | clase plana | — | `IsDashing`, `CanDash`; `Action<float> OnDashCDStarted`; `OnUpdate(moveDir, camForward, camRight, speed)`, `StopTweens()`, `StartTeleport(pos, duration)`, `Teleport()`, `OnPlatformMoving`, `CanDashWithCoyoteTime()`, `SetPos`, `SetRespawnPos`, `RotatePlayer`, `SetGravity(bool)`, `SetCinematicMovement`, `ClearCinematicMovement`, `IsInCinematicMode`, `Dash(dir)` / `DashCD()` (IEnumerator) | Ctor `(CC, transform, PlayerData, collider)` |
| `PlayerView` | `Scripts/Player/MVC/PlayerView.cs` | clase plana | — | `Action OnDashViewPlayed`; `OnStart()`, `Walk(Vector3)`, `DashSound()`, `SetAnimatorSpeed`, `UpdatePlayerMaterials(bool)`, `RespawnPlayer()`, `DashChargedSound()`, `DeathSound()`, `FallSound()`, `WalkSound()`, `PlayPS(Color)`, `PlayNodePS(GlitchState)`, `TeleportPS()`, `StopPS()`, `GrabNode(bool, Color)`, `PlayErrorSound(clip)` | |
| `IPlayerState` | `Scripts/Player/StateMachine/IPlayerState.cs` | interfaz | — | `Enter(PlayerController, PlayerNodeHandler)`, `HandleInteraction(IInteractable)`, `Tick()`, `Cancel()`, `Exit()` | |
| `PlayerStateMachine` | `Scripts/Player/StateMachine/PlayerStateMachine.cs` | clase plana | — | `CurrentState`, `LastState`, `CinematicState`; `Tick()`, `TransitionToState(IPlayerState)`, `TransitionToEmptyState()`, `TransitionToGrabState(NodeController)`, `TransitionToDissolving()`, `TransitionToTeleport()`, `TransitionToCinematicState()`, `TransitionFromCinematicState()` | |
| `PlayerEmptyState` | `Scripts/Player/StateMachine/PlayerEmptyState.cs` | `IPlayerState` | — | `Enter/HandleInteraction/Tick/Cancel/Exit` | Maneja hold + `Inspectionable.OnFinished` |
| `PlayerGrabState` | `Scripts/Player/StateMachine/PlayerGrabState.cs` | `IPlayerState` | — | `Enter/HandleInteraction/Tick/Cancel/Exit` | Con nodo en mano |
| `PlayerDissolvingState` | `Scripts/Player/StateMachine/PlayerDissolvingState.cs` | `IPlayerState` | — | `Enter/HandleInteraction/Tick/Cancel/Exit` | Alterna disolver/revertir |
| `PlayerTeleportState` | `Scripts/Player/StateMachine/PlayerTeleportState.cs` | `IPlayerState` | — | `Enter/HandleInteraction/Tick/Cancel/Exit` | Escucha `OnTeleported` |
| `PlayerCinematicState` | `Scripts/Player/StateMachine/PlayerCinematicState.cs` | `IPlayerState` | — | + `StartWaypointNavigation(Vector3[], Action onWaypointReached, Action onSequenceComplete)`, `StopNavigation()` | |

### Scripts/Scriptable Objects · UI · Utilities

| Clase | Archivo | Hereda / Implementa | SerializeFields | Públicos | Notas |
|---|---|---|---|---|---|
| `GlitchSounds` | `Scripts/Scriptable Objects/GlitchSounds.cs` | ScriptableObject | pub `startSFX`, `endSFX` | — | `[CreateAssetMenu]` |
| `PlayerData` | `Scripts/Scriptable Objects/PlayerData.cs` | ScriptableObject | pub `moveSpeed`, `upgradedMoveSpeed`, `rotSpeed`, `teleportSpeed`, `fovAngle`, `coyoteTime`, `maxWallDist`, `wallMask`, `glitchDetectionLayer`, `defaultLayer`, `teleportLayer`, `intangibleMat` (Material), `interactionRadius`, `losCheckInterval`, `losDebounceTime`, `losHeightOffset`, `losEndMargin`, `dashSpeed`, `dashDuration`, `dashCD`, `holdInteractionTime`, `lowRumbleFrequency`, `highRumbleFrequency`, `rumbleDuration`, `transferHoldDelay`, `transferInitialRumble`, `transferHoldRumble`, `transferErrorRumble`, `transferErrorRumbleDuration`, `testForce`, clips `walkClip`, `dashClip`, `chargedDashClip`, `liftClip`, `putDownClip`, `emptyHand`, `deathClip`, `fallClip` | — | Tunables del jugador |
| `CanvasButtonStateController` | `Scripts/UI/CanvasButtonStateController.cs` | MonoBehaviour | `_startsEnabled` | `OriginalTBNValues`; `DisableButtons()`, `EnableButtons()`, `GetOriginalButtonValues()` | |
| `CanvasController` | `Scripts/UI/CanvasController.cs` | MonoBehaviour | — | `SetNewUITarget(GameObject)` | Navegación entre canvases |
| `DirectionalButtonListener` | `Scripts/UI/DirectionalButtonListener.cs` | MonoBehaviour, `ISelectHandler`, `IPointerEnterHandler` | — | `OnSelect`, `OnPointerEnter` | |
| `GlobalVolumeController` | `Scripts/UI/GlobalVolumeController.cs` | MonoBehaviour | `_source`, `_maxCAIntensity`, `_maxLDIntensity`, `_minPitch`, `_maxPitch` | — | Post-fx según audio |
| `InputPromptIcon` | `Scripts/UI/InputPromptIcon.cs` | MonoBehaviour | `_detector`, `_database`, `_action`, `_hideWhenNoSprite` | `SetAction(PromptAction)` | |
| `LightFlicker` | `Scripts/UI/LightFlicker.cs` | MonoBehaviour | `_minOnTimer`, `_maxOnTimer`, `_minOffTimer`, `_maxOffTimer` | — | |
| `LookAtCamera` | `Scripts/UI/LookAtCamera.cs` | MonoBehaviour | `_camera` | — | |
| `PauseGameController` | `Scripts/UI/PauseGameController.cs` | MonoBehaviour | `_resumeBtn` | `ResumeGame()`, `RestartLevel()`, `GoToScene(string)`, `GoToDesktop()` | Pausa vigente |
| `PauseMenu` | `Scripts/UI/PauseMenu.cs` | MonoBehaviour | `_pauseRoot`, `_btnContinue/Options/Save/MainMenu`, `_mainMenuSceneName`, `_pauseAudioListener` | `IsPaused`; `ev Action OnOptionsRequested`; `PauseGame()`, `ResumeGame()` | ⚠️ legacy (Input viejo) |
| `Config` | `Scripts/UI/Screen Manager/Config.cs` | MonoBehaviour | `_pauseMenuPrefab` | — | Pausa vía ScreenManager |
| `ScreenManager` | `Scripts/UI/Screen Manager/ScreenManager.cs` | MonoBehaviour | — | `static Instance`; pub `lastResult`; `Pop()`, `Push(IScreen)`, `Push(string resource)` | Stack de pantallas |
| `ScreenPause` | `Scripts/UI/Screen Manager/ScreenPause.cs` | MonoBehaviour, `IScreen` | — | `Start()`, `BTN_Back()`, `BTN_Pause()`, `BTN_Menu()`, `BTN_ResetLevel()`, `Activate()`, `Deactivate()`, `Free()` | |
| `TutorialAnimController` | `Scripts/UI/Tutorials/TutorialAnimController.cs` | MonoBehaviour | `_anim` | — | |
| `UIActionController` | `Scripts/UI/UIActionController.cs` | MonoBehaviour | `_source` (`UITargetSource`); Interactable: `inputBtn` (Image), `inputText` (TMP); Glitcheable: `_glitchRoot`, `_setPanel`/`_takePanel` (`GlitchActionPanel`: `panel`, `inputBtn`, `inputText`), `_minScale`, `_maxScale`, `_scaleDuration`, `_scaleEase` | — | Interactable: prompt Put/Grab sobre `OnInteractableDetected`. Glitcheable: panel padre + hijos Set/Take sobre `OnGlitcheableDetected`; escala (`UIScaleTween`) y apaga texto/imagen solo del panel que cambia; `LayoutElement.ignoreLayout` al terminar de ocultar un hijo |
| `UIScaleTween` | `Scripts/UI/UIScaleTween.cs` | clase plana | — | ctor `(min, max, duration, Ease)`; `MinScale`; `Show(rect)`, `Hide(rect, Action onHidden)`, `SnapHidden(rect)`, `Stop(rect)` | Tween de escala compartido por `UpdatePosToTarget` y `UIActionController` |
| `UITargetSource` | `Scripts/UI/UITargetSource.cs` | enum | — | `Interactable, Glitcheable` | Qué señal del `PlayerController` sigue un HUD |
| `UIButtonsManager` | `Scripts/UI/UIButtonsManager.cs` | MonoBehaviour | `openMenuSound`, `buttonUpSound`, `buttonDownSound`, `clickSound` | `static Instance`; `PlaySoundClick()`, `PlaySoundUp()`, `PlaySoundDown()`, `PlaySoundOpenMenu()` | |
| `UpdateCrosshair` | `Scripts/UI/UpdateCrosshair.cs` | `UpdatePosToTarget` | `_circleImage` | `SetUpdateAnim()`; override `Source => Glitcheable` | Visible si hay transferencia posible; color desde `GlitchPalette` |
| `UpdatePosToTarget` | `Scripts/UI/UpdatePosToTarget.cs` | MonoBehaviour | `_source` (`UITargetSource`), `_camera` (protected), `_rectToMove`, `_visual`, `_yOffset`, `_minTargetScale`, `_maxTargetScale`, `_scaleDuration`, `_scaleDelay`, `_scaleEase` | protected `Rect`, `CurrentTarget`, virtual `Source` | Base de HUD sobre interactuable o glitcheable; escala con `UIScaleTween` |
| `LightMovement` | `Scripts/Utilities/LightMovement.cs` | MonoBehaviour | `_minRange`, `_maxRange`, `_cycleDuration`, `_durationVariation`, `_easeType`, `_useUnscaledTime` | `MinRange`, `MaxRange`, `EaseType`; `InitializeCycle()`, `EvaluateRange(float)`, `ApplyRange(float)` | Editor custom en `Assets/Editor` |
| `SFXManager` | `Scripts/Utilities/SFXManager.cs` | MonoBehaviour | `sourcePrefab` | `static Instance`; `PlaySFX(clip, parent, volume)`, `PlayRandomSFX(clip[], parent, volume)` | |

### Assets/Editor · Assets/Tools

| Clase | Archivo | Hereda / Implementa | SerializeFields | Públicos | Notas |
|---|---|---|---|---|---|
| `LightMovementEditor` | `Editor/LightMovementEditor.cs` | `UnityEditor.Editor` (`[CustomEditor(LightMovement)]`) | — | `OnInspectorGUI()` | |
| `ObjectReplacerWindow` | `Editor/ObjectReplacerWindow.cs` | EditorWindow | `_prefab`, `_selected` | `static ShowWindow()` (`Tools/Object Replacer`) | |
| `PrefabPainterWindow` | `Editor/PrefabPainter.cs` | EditorWindow | — | `static ShowWindow()` (`Tools/Prefab Painter`) | La copia en `Editor/PrefabPainter/PrefabPainterWindow.cs` está comentada |
| `PrefabPainterPalette` | `Editor/PrefabPainter/PrefabPainterPalette.cs` | ScriptableObject | pub `prefabs` | — | `Create → Tools/Prefab Painter/Palette` |
| `PrefabPainterSceneTool` | `Editor/PrefabPainter/PrefabPainterSceneTool.cs` | clase plana | — | `enum SnapMode {Grid, Chain}`, `struct Settings {snapMode, gridSize, alignToSurfaceNormal, randomYaw, yOffset, livePreview}`; `SetSettings`, `SetCurrentPrefab(GameObject)`, `ClearLastPlaced()`, `Dispose()`, `DuringSceneGUI(SceneView)` | |
| `HierarchyFolder` | `Tools/FolderTool/HierarchyFolder.cs` | MonoBehaviour (`[DisallowMultipleComponent, OnlyScript]`, ns `Tools.FolderTool`) | pub `folderColor`, `textColor` | — | Se elimina en build |
| `OnlyScriptAttribute` | `Tools/FolderTool/FolderAttribute/OnlyScriptAttribute.cs` | Attribute | — | — | |
| `FolderSceneProcessor` | `Tools/FolderTool/Editor/FolderSceneProcessor.cs` | `IProcessSceneWithReport` | — | `callbackOrder`, `OnProcessScene(Scene, BuildReport)` | Desparenta y borra carpetas al buildear |
| `HierarchyFolderEditor` | `Tools/FolderTool/Editor/HierarchyFolderEditor.cs` | `UnityEditor.Editor` (`[InitializeOnLoad]`) | — | `static CreateSpawner()` (`GameObject/Hierarchy Folder`) | |
| `OnlyScriptProcessor` | `Tools/FolderTool/Editor/OnlyScriptProcessor.cs` | static-like (`[InitializeOnLoad]`) | — | — | Borra otros componentes de `[OnlyScript]` |
| `MapBuilderPalette` | `Tools/MapCreator/MapBuilderPalette.cs` | ScriptableObject | pub `prefabs` | — | `Create → Tools/Map Builder/Palette` |
| `MapBuilderTool` | `Tools/MapCreator/MapBuilderTool.cs` | EditorWindow | — | `static ShowWindow()` (`Tools/Map Builder`) | |

### Assets/Inspection System

| Clase | Archivo | Hereda / Implementa | SerializeFields | Públicos | Notas |
|---|---|---|---|---|---|
| `ParticlesFeedbackManager` | `Inspection System/ParticlesFeedbackManager.cs` | MonoBehaviour | `_startsEnabled` | `StartParticles()`, `StopParticles()` | |
| `Corruption` | `Inspection System/Corruption/Corruption.cs` | MonoBehaviour | `_minSpeed`, `_maxSpeed`, `_psHitting`, `_psRemoved` | `EnableCorruptionEvents(bool)`, `SetUpGenerator(CorruptionGenerator)`, `SetPos((int, Vector3, Quaternion))`, `TurnOnOff(bool)` | Mancha individual |
| `CorruptionGenerator` | `Inspection System/Corruption/CorruptionGenerator.cs` | MonoBehaviour | `_minInstances`, `_maxInstances`, `_offsetAboveSurface` | `TotalInstances`, `CleanedInstances`; `Action OnUpdatedInstances`, `Action<CorruptionGenerator> OnObjectCleaned`; `RefreshCorruptionVisual(Corruption)`, `RemoveCorruption()` | |
| `CorruptionRemover` | `Inspection System/Corruption/CorruptionRemover.cs` | MonoBehaviour | `_mainCamera`, `_inspectRawImage`, `_canvas`, `_inspectionLayer`, `_holdTimer`, `_shakeIntenity`, `_debugObject`, `_debug` | `static Instance`; `Action<Corruption> OnCorruptionHit`, `Action<float> OnHittingCorruption`, `Action<Corruption> OnCorruptionRemoved` | Click sostenido sobre la RawImage |
| `DoorInspectionController` | `Inspection System/Inspection/DoorInspectionController.cs` | MonoBehaviour | `_inspectionable` | — | |
| `Inspectionable` | `Inspection System/Inspection/Inspectionable.cs` | MonoBehaviour, `IInteractable` | `_type`, `_collider`, `_positiveFM`, `_negativeFM` | `Priority=Low`, `CorruptionGenerator`, `Type`; `ev OnFinished/OnCleaned`; `CanInteract`, `Interact`, `StopInteraction()`, `CorruptionCleaned(CorruptionGenerator)` | |
| `InspectionType` | `Inspection System/Inspection/Inspectionable.cs` | enum | — | `None, Panel, Battery` | |
| `InspectionPlayerManager` | `Inspection System/Inspection/InspectionPlayerManager.cs` | MonoBehaviour | — | `StartInspection(Inspectionable)`, `StopInspection(ctx=default)` | Cámara de inspección; cambia action map |
| `InspectionSystem` | `Inspection System/Inspection/InspectionSystem.cs` | MonoBehaviour | `_inspectedObject`, `_rotSpeed`, `_gamepadRotSpeed`, `_resetRotDuration`, `_resetEasing` | `static Instance`; `CanRotate`; `Action OnResetRot`; `ResetRot()`, `SetHoldValues(bool, bool)` | Rotar objeto inspeccionado |
| `InspectorController` | `Inspection System/Inspection/InspectorController.cs` | MonoBehaviour | — | `static Instance`; `Action<UIInspectionable> OnTargetEnabled` | Activa el `UIInspectionable` por tipo |
| `ScannerController` | `Inspection System/Inspection/ScannerController.cs` | MonoBehaviour | `_targetScale`, `_targetTime` | `static Instance`; `Action OnScanFinished` | |
| `UIInspectionable` | `Inspection System/Inspection/UIInspectionable.cs` | MonoBehaviour | `_type` | `Type`, `CorruptionGenerator`, `UICorruption`; `SetUpGenerator(CorruptionGenerator)` | |
| `ApproachToTarget` | `Inspection System/UI/ApproachToTarget.cs` | MonoBehaviour | `_timeModifier` | — | |
| `GamepadCursor` | `Inspection System/UI/GamepadCursor.cs` | MonoBehaviour | `_cursorTransform`, `_canvasTransform`, `_canvas`, `_cursorSpeed`, `_cursorPadding` | `static Instance`; `const GamepadScheme/MouseScheme`; `PrevControlScheme`, `CurrentMouse`; `IsUsingGamepad()`, `GetCursorPosition()`, `CenterCursor()` | Cursor virtual para gamepad |
| `InspectionCanvasController` | `Inspection System/UI/InspectionCanvasController.cs` | MonoBehaviour | `_CMBrain`, `_inspectionCamera` | — | |
| `UpdateCameras` | `Inspection System/UI/UpdateCameras.cs` | MonoBehaviour | `_CMBrain`, `_mainCam`, `_targetLockCam` | — | |

### Assets/Art · Assets/Art-2 · Scenes/TEST LEVELS/Mati/Scripts

| Clase | Archivo | Hereda / Implementa | SerializeFields | Públicos | Notas |
|---|---|---|---|---|---|
| `Scroller` | `Art/Animation/ScriptsAnim/Scroller.cs` | MonoBehaviour | `images[]` (RawImage), `x`, `y` | — | Scroll de UV |
| `PostProcessController` | `Art/Shaders/Post-Process/Post_Process_Graphs/PostProcessController.cs` | MonoBehaviour | `_rendererData`, `_passiveMat`, `_corruptionMat`, `_shockWaveMat`, `_colorNeg`, `_colorOriginal` | — | Cambia materiales de post-proceso |
| `ToonDepthNormalsFeature` | `…/ToonDepthNormalsFeature.cs` | ScriptableRendererFeature | `_settings` | `class Settings {injectionPoint, opaqueLayerMask, transparentLayerMask}`; `Create()`, `AddRenderPasses()` | Render Graph |
| `ToonOutlineFeature` | `…/ToonOutlineFeature.cs` | ScriptableRendererFeature | `_settings` | `class Settings {injectionPoint, outlineMaterial}`; `Create()`, `AddRenderPasses()` | Render Graph |
| `Outline` | `Art/Store Meshes/QuickOutline/Scripts/Outline.cs` | MonoBehaviour | (privados) | `enum Mode`; `OutlineMode`, `OutlineColor`, `OutlineWidth` | Asset de tienda (QuickOutline) |
| `CustomRenderPassFeature` | `Art-2/AssetsFer/ShadersFer/CustomRenderPassFeature.cs` | ScriptableRendererFeature | — | `Create()`, `AddRenderPasses()` | API vieja (no Render Graph) |
| `TextGlitcher` | `Art-2/AssetsFer/ShadersFer/TextGlitcher.cs` | MonoBehaviour | pub `textMesh` (TextMeshPro), `originalTextDuration`, `glitchDuration`, `glitchChangeFrequency`, `characters` | — | Duplica `TextGlitch` (3D) |
| `UITextGlitcher` | `Art-2/AssetsFer/ShadersFer/UITextGlitcher.cs` | MonoBehaviour | pub `textMesh` (TextMeshProUGUI), `originalTextDuration`, `glitchDuration`, `glitchChangeFrequency`, `characters` | — | Duplica `TextGlitch` (UI) |
| `PosToPosParticles` | `Art-2/Dissolve Tutorial/PosToPosParticles.cs` | MonoBehaviour | pub `VFXGraph`, `ParticleSpeed`, `ParticleTexture` | `SetTarget(Transform)`, `StartParticle(Transform)`, `StopParticle()` | |
| `SolvingController` | `Art-2/Dissolve Tutorial/SolvingController.cs` | MonoBehaviour | pub `playerController`, `playerCollider`, `MySkinnedMeshRenderer`, `MaxBound`, `MinBound`, `secondToDissolve`, `VFXGraph`, `ResetTimer`, `skinnedMaterials[]`, `DissolveShader`, `DesintegrateShader`, `refreshRate`, `particleIncreaseRate`, `particleInintialRate`; `otherParticles`, `startKillerTransform`, `endKillerTransform`, `myMaterials`, `duration`, `killerSize`, `particleSpeed`, `_desintegrateVector` | `Action OnDissolveCompleted`; `RespawnPlayer()`, `BurnShader()`, `StartDesintegrateShader()`, `SetDesintegrateShader(float)`, `StopDesintegrateShader()` | ⚠️ legacy (referencia comentada en `PlayerController`) |
| `GlitchDeathController` | `Art-2/Shaders/ShaderScript/GlitchDeathController.cs` | MonoBehaviour | pub `glitchMaterial`, `glitchDuration` | `static Instance`; `TriggerGlitch()` | Lo llama `PlayerController.RespawnPlayer` |
| `ShaderFiller` | `Art-2/Shaders/ShaderScript/ShaderFiller.cs` | MonoBehaviour | `fillTime`, `unfillTime`; pub `_completed`, `_isLoading`, `_isUnloading`, `startactive`, `timeModifier`, `_currentLoad` | `UnityEvent FinishLoad/FinishUnload/OnLoading/OnUnLoading`; `StartFill()`, `SetCompleated()`, `SeyUnCompleated()`, `StartUnFill()` | |
| `StutterTimePerRenderer` | `Art-2/Shaders/ShaderScript/StutterTimePerRenderer.cs` | MonoBehaviour | pub `shaderFloatName` | — | `_SteppedTime` |
| `CameraShakeController` | `Scenes/TEST LEVELS/Mati/Scripts/CameraShakeController.cs` | MonoBehaviour | `spaceshipController`, `easeType`, `tweenDuration` | — | Prototipo |
| `CameraShakeType` | `Scenes/TEST LEVELS/Mati/Scripts/CameraShakeController.cs` | enum | — | — | |
| `CinematicSpaceshipController` | `Scenes/TEST LEVELS/Mati/Scripts/CinematicSpaceshipController.cs` | MonoBehaviour | — | `Action<CameraShakeType> OnStartAnimation`; `StartAnimation(ctx)`, `AnimationEnded()`, `StartSplineAnimation()` | Prototipo |

## 12. Notas del equipo

_(Sección manual. Todo lo que se escriba acá lo respetan /init-project y /update-project.)_

## 13. Historial de actualizaciones

| Fecha | Commit | Resumen |
|---|---|---|
| 2026-09-26 | `fc042895` | ~3 scripts (wt: hold de Set/Take transfiere el máximo de una vez: `GlitchTransferManager.MaxTransferable` + `ExecuteTransfer(..., amount)`, `GlitchTransferHandler` decide tap/hold al soltar o al cumplir `transferHoldDelay`, se elimina `PlayerData.transferRepeatInterval`; commit `fc042895` ya documentado como wt en la fila anterior); commits: 1 (wt: 3 scripts) |
| 2026-09-25 | `1d22809e` | +2 ~4 scripts (wt: `UITargetSource`, `UIScaleTween`; `PlayerController.OnGlitcheableDetected` separa el HUD de glitch de `OnInteractableDetected`; `UIActionController` con modo Interactable/Glitcheable y paneles Set/Take; `UpdatePosToTarget._source`; commit previo ya documentado: `IsInRange`/`IsInInteractionRange`); commits: 1 (wt: 6 scripts + prefab) |
| 2026-09-25 | `ee2b3df6` | ~7 scripts (`PlayerData.intangibleMat`, ctor de `PlayerView` recibe `GameObject`; wt: `PlayerInteractionDetector.IsInRange`, `PlayerController.IsInInteractionRange`, `Glitcheable` vuelve a sólido fuera de rango); commits: 1 (wt: 4 archivos) |
| 2026-09-23 | `07813b8d` | ~11 scripts (paso 5: `GlitchPalette.Default`/`EmissionFor` + `Resources/GlitchPalette.asset`, crosshair y HUD Set/Take, `PromptAction.Set/Take`, `InputPromptIcon.SetAction`, orbe `PSType.Intangible`); commits: 1 |
| 2026-09-23 | `8840bdb1` | ~1 script (`Glitcheable`: intangibilidad del nivel 1, sin cambios de API pública); commits: 1 |
| 2026-09-23 | `618b8cf0` | +5 ~35 -0 scripts (refactor glitch 0/1/2: `GlitchState` reemplaza `NodeType`, `GlitchComponent`, `GlitchTransferManager`, `GlitchPalette`, `GlitchTransferHandler`; acciones Set/Take); commits: 6 (wt: 15 archivos, sin scripts) |
| 2026-09-21 | `ec2ae366` | Generado por /init-project (190 scripts indexados: 147 en Scripts/ + 13 Editor/Tools + 30 fuera; 20 escenas) |
