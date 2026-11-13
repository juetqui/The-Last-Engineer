# Plan: corregir el stutter del jugador sobre plataformas móviles

**Estado:** plan (2026-11-13). No se implementó nada. Parte de `doc/diagnostics/platform-player-stutter.md`
(hipótesis por lectura de código); cada `file:line` de este plan se verificó contra el código actual.

**Design:** no pattern — es una corrección de sincronización y orden dentro de componentes que ya existen
(`PlatformController` + FSM, `PlatformMotor`, `PlayerModel`, `AttachPlayer`). Lo único nuevo es extraer
la aritmética de paso, llegada y desplazamiento a un asmdef `Platform.Rules`. Sigue el precedente de
`Glitch.Rules` y `Connection.Rules`, y lo pide la testabilidad: los asmdefs de test no pueden referenciar
`Assembly-CSharp`. Alternativa descartada: mover Player y Platform a un asmdef (refactor grande, fuera
de proporción).
SOLID: `Platform.Rules` = cálculo puro (sin `MonoBehaviour`, sin estado de escena); `PlatformMotor` =
aplica la pose al `Rigidbody` y avisa al pasajero; `PlatformController` = adapter de Unity (lifecycle,
triggers, FSM). Sin interfaces nuevas.

**Rama propuesta:** `fix/platform-player-stutter` desde `main`. No se commitea sin pedido.

## Desvíos al implementar (2026-11-13)

- **Llegada exacta sin snap de hasta 0.15 u:** `PlatformStep` no salta al target cuando entra en el umbral,
  porque eso movía hasta 15 cm en un frame. En cambio, la plataforma llega exacta cuando el paso del frame
  cubre la distancia que falta. Dentro de `ApproachRadius` (0.15 u) nunca va más lenta que
  `MinApproachSpeed` (0.5 u/s), porque el tween de frenado puede llegar a 0 un poco antes del punto. El
  sobrante del paso al pasar un punto de ruta se gasta en el tramo siguiente (`MoveDistance`), así que
  no hay caída de velocidad en los puntos de paso. `PlatformMotor.InTarget` y
  `PlatformController.ReachedTarget` se eliminaron.
- **Dos `CharacterController.Move` en `PlayerModel.MovePlayer`:** primero el desplazamiento de la
  plataforma y después el movimiento propio. `isGrounded` solo se marca en un `Move` cuyo neto vertical
  baja: con un único `Move` combinado, una plataforma que sube dejaba al jugador "no grounded" todos los
  frames, y la gravedad lo hacía rebotar. Sin plataforma no cambia nada: el primer `Move` se omite.
- **Dash:** mientras dura, el desplazamiento acumulado se descarta, como antes, para que no se aplique de
  golpe al terminar.

## Decisiones tomadas (usuario, 2026-11-13)

| Tema | Decisión |
|---|---|
| Síntoma | Horizontales y verticales, también en build. Escena: `Assets/Scenes/BUILD SCENES/4 - NIVEL PLATAFORMAS.unity`. |
| Física | `Rigidbody` kinematic en los prefabs de plataforma; mover con API de `Rigidbody`. |
| Loop | **La plataforma sigue en `Update`**, corre **antes** que el jugador y, cada frame, escribe `rb.position` (pose física inmediata, la ve el `CharacterController.Move` de ese frame) y `transform.position` (visual). **Sin interpolación.** Es una excepción explícita a `.claude/rules/runtime-performance.md` ("Physics in FixedUpdate… never by writing transform"); ver la justificación más abajo. |
| Pasajeros | Solo el jugador. Ningún `Rigidbody` dinámico viaja en plataformas. |
| Sin `Rigidbody` | Si una instancia no lo trae, se agrega en `Awake` (kinematic) con un `LogWarning`. Es el precedente de `GlitchTransferHandler` en `PlayerController.Awake`. |
| Prefabs | Los edita el usuario en el Editor; no hay trabajo abierto de otros sobre esos prefabs ni sobre la escena 4. |
| Ascensor | Mismo criterio que las plataformas: `Rigidbody` kinematic en la raíz, el `Animator` **sigue en Update normal**, y `AttachPlayer` sincroniza `rb.position` en `LateUpdate` y manda el delta. |
| Extras dentro del alcance | Limpieza completa al soltar al pasajero (`OnTriggerExit`/muerte), `print` de `SetVerticalPlatform`, `isVertical` en la escena 4 y el campo muerto `onVerticalPlatform`. |
| Fuera del alcance (seguimiento) | El FSM arranca sin estado (`PlatformStateMachine.cs:19`); ver "Riesgos y seguimiento". |
| Aceptación | A ojo sin tirones (horizontal y vertical, vSync on y off, Editor y build) **y** Profiler: `isGrounded` estable y sin saltos de pose en los frames con paso de física. |

### Por qué la excepción a la regla de física
El jugador se mueve con `CharacterController.Move` en `Update` (`PlayerModel.cs:107`), y `Move` barre
contra la pose **física** de los colliders. Si la plataforma se moviera en `FixedUpdate` con
`MovePosition` + Interpolate, la pose física iría hasta un paso fijo (20 ms, unos 4 cm a 2 m/s) adelante
de la visual. En las verticales el CC se hundiría en el piso al subir y se despegaría al bajar, y
`isGrounded` seguiría titilando. Escribir `rb.position` es API de `Rigidbody`: actualiza el cuerpo en la
escena de física al instante, sin `Physics.SyncTransforms` y sin `autoSyncTransforms`. La escritura de
`transform` se hace solo porque, si no, la malla se actualizaría recién en el siguiente paso de
simulación. El motivo queda en un comentario de una línea en `PlatformMotor`.

## Hechos verificados (contra el código actual)

- `PlatformMotor.MoveTowards` (`Assets/Scripts/Gameplay/Platform/PlatformMotor.cs:20-34`) mueve con
  `_transform.position += move` (`:30`) y llama `passenger.OnPlatformMoving(move)` (`:32-33`). El
  constructor recibe un `passenger` que no usa (`:8`). Su único caller es `PlatformController.MoveStep`
  (`PlatformController.cs:164-167`).
- `ProjectSettings/DynamicsManager.asset:22` `m_AutoSyncTransforms: 0`; `TimeManager.asset:6` Fixed
  Timestep 0.02.
- Orden: `PlatformController.Update` (`PlatformController.cs:88-92`) y `PlayerController.Update`
  (`PlayerController.cs:153`) no tienen `[DefaultExecutionOrder]`. `Glitcheable` tiene
  `[DefaultExecutionOrder(-1)]` (`Assets/Scripts/Gameplay/Glitch/Glitcheable.cs:10`).
- `PlayerModel.OnPlatformMoving` sobrescribe (`PlayerModel.cs:163-166`); `MovePlayer` suma
  `_platformDisplacement`, hace `_cc.Move` y lo pone en cero (`PlayerModel.cs:105-108`).
- `StopPassenger` tiene la condición invertida (`PlatformController.cs:147-152`). Sus callers son
  `InactiveState.Enter:9`, `WaitingState.Enter:19`, `MovingState.Tick:24`, `ReturningState.Tick:23` y
  `ToStopState.Tick:23`.
- La llegada sin snap aparece en tres estados: `MovingState.Tick:19-27`, `ToStopState.Tick:20-28` y
  `ReturningState.Tick:19-26`. Hacen `ReachedTarget()` → return/avance sin mover al punto exacto. El
  umbral es `InTarget` (`PlatformMotor.cs:14-18`, mínimo 0.15 u). En `MovingState`, cuando el punto no
  es parada (`:29-32`), se avanza al siguiente sin cerrar el tramo y el error se arrastra.
- Liberación del pasajero:
  - `OnTriggerExit` (`PlatformController.cs:251-262`) no desuscribe `_player.OnDied` ni limpia `_player`.
  - `CleanPlayerReferences` (`:222-229`) no restaura la gravedad: si el jugador muere sobre una
    plataforma `isVertical`, reaparece con `_gravity = 0`.
  - `PlayerController.SetVerticalPlatform` tiene un `print(bul)` (`PlayerController.cs:119`).
- `PlayerModel.onVerticalPlatform` (`PlayerModel.cs:21`, leído en `:148`) nunca se asigna: es código
  muerto. `SetVerticalPlatform` cambia `_gravity` (`:65-75`).
- `isVertical`: ninguna instancia de la escena 4 lo tiene en `true` (sin overrides; el prefab
  `Plataforma 1.prefab` lo tiene en 0). Solo `Assets/Scenes/BLOCKING/Salas 2.unity` lo usa. Así que las
  verticales de la escena 4 corren hoy con gravedad.
- **`Glitcheable` + plataforma** (`Assets/Prefabs/Glitcheables/Glitcheable Platform Container.prefab`):
  - `PlatformController` está en los anclas `Pos 1` / `Pos 2` (cada uno con `BoxCollider` sólido +
    trigger); `Glitcheable` está en otro objeto, `GlitcheablePlatform` (con sus propios sólido + trigger,
    sin `Rigidbody`).
  - `GlitchIdleState.Enter` lo emparenta al ancla actual con `localPosition = 0`.
  - `GlitchMovingState` lo desemparenta (`SetParent(null)`, `GlitchMovingState.cs:35`), lo lleva por
    `transform` (`ApplyPose`, `:109-123`) leyendo cada frame la pose del ancla (`:76-77`), apaga **solo
    el primer** collider (`Glitcheable.SetColliders` → `_coll`, `Glitcheable.cs:305-309`) y al terminar
    lo re-emparenta (`:95-106`).
  - Con `Rigidbody` en el ancla, los colliders de `GlitcheablePlatform` pasan al compuesto del ancla
    mientras esté emparentado.
  - **Este prefab no está en ninguna escena.**
- Prefabs con `PlatformController` (GUID `6689ebcc1ca873f48958fcb7a88b9bd5`), sin variantes ni anidados
  que los hereden:

  | Prefab | Objeto con `PlatformController` | Uso |
  |---|---|---|
  | `Assets/Prefabs/Level/Plataformas01.prefab` | `Plataforma` (sólido + trigger) | escena 4 (17 instancias), Tuto 1/2, LVL 1, BLOCKING, SaveLoad |
  | `Assets/Prefabs/Modulos/PlataformasMoviles/Plataforma 1.prefab` | raíz (sólido + trigger) | escena 4 (13), anidado en `Modulos/PlataformasMoviles/Plataformas01.prefab`, Tuto 1, LVL 1, BLOCKING, SaveLoad |
  | `Assets/Art-2/AssetsFer/FBXs/Reemplazar/Prefabs/Plataformas/Plataforma.prefab` | `Plataforma` (sólido + trigger) | DrillRooms (BLOCKING), DV START, SaveLoad |
  | `Assets/Prefabs/Level/Movable Platform.prefab` | raíz | sin uso |
  | `Assets/Prefabs/Modulos/PlataformasMoviles/PuzzlePlataforma01.prefab` | `Plataforma` | sin uso |
  | `Assets/Prefabs/Glitcheables/Glitcheable Platform Container.prefab` | `Pos 1`, `Pos 2` | sin uso |

  Además, en la escena 4 hay un objeto de escena, `Plataforma (1)` (no es prefab), con
  `PlatformController` y **sin ningún collider**: no puede llevar al jugador. Lo cubre el agregado en
  runtime.
- **Save/load:** no hay sistema de guardado. `PauseMenu.SaveGameStub` (`Assets/Scripts/UI/PauseMenu.cs:146-154`)
  es un stub, y ningún código restaura posiciones de plataforma. `TEST SaveLoad Data.unity` solo contiene
  instancias de los prefabs. No hay nada que adaptar a `rb.position` por ahora (ver Riesgos).
- `QualitySettings`: Very Low/Low `vSyncCount: 0`, Medium..Ultra `vSyncCount: 1`; Standalone usa Ultra
  (5) por defecto. Ningún script toca `targetFrameRate`/`vSyncCount`.
- **Triggers con `Rigidbody`:** el jugador ya tiene un hijo con `Rigidbody` kinematic + `SphereCollider`
  trigger (`PlayerInteractionDetector`, `Assets/Scripts/Player/Components/PlayerInteractionDetector.cs:27`),
  además del `CharacterController`. `PlatformController.OnTriggerEnter/Exit` filtra con
  `TryGetComponent<IMovablePassenger>` sobre el GameObject del collider: el hijo detector no tiene
  `PlayerController`, así que se ignora. Con el `Rigidbody` en la plataforma, los eventos se siguen
  entregando al GameObject del trigger, que es el mismo del `Rigidbody`. Lo confirma el Paso 0 y el
  checklist manual.
- **Ascensor** (`Assets/Prefabs/Ascensor/Asset_Ascensor.prefab`, en `3 - HUB.unity` y
  `NIVEL HUB EfectosVisuales.unity`):
  - La raíz tiene `Animator` (Update normal, sin root motion), un `BoxCollider` trigger y `AttachPlayer`.
  - El clip `ElevatorNextLevel.anim` anima `m_LocalPosition` de la raíz (path vacío).
  - Pisos y paredes son `BoxCollider` estáticos hijos: misma causa raíz que las plataformas.
  - `AttachPlayer` (`Assets/Scripts/Gameplay/AttachPlayer.cs:9-30`) manda el delta en `LateUpdate`, tiene
    un umbral de 1 cm/frame (`:16`), no tiene `OnTriggerExit` y no suelta al jugador al morir.
- **Tests:** solo existe `Assets/Tests/EditMode/Glitch.Rules.Tests.asmdef` (refs `Glitch.Rules`,
  `Connection.Rules`). No hay PlayMode. Todo el gameplay está en `Assembly-CSharp`.

## Orden de ejecución

- Constante nueva: `Assets/Scripts/Core/ExecutionOrder.cs` → `public static class ExecutionOrder { public const int Platform = -50; }`.
  Comentario: "antes que Glitcheable (-1) y que el jugador (0)". `PlatformController` y `AttachPlayer`
  no la necesitan más allá de esto (`AttachPlayer` usa `LateUpdate`).
- `[DefaultExecutionOrder(ExecutionOrder.Platform)]` en `PlatformController`.
- Resultado por frame: plataforma (-50: `rb.position` + `transform` + delta al pasajero) →
  `Glitcheable` (-1: el lerp de `GlitchMovingState` lee la pose del ancla ya movida) → `PlayerController`
  (0: `CC.Move` con el delta del mismo frame contra la pose física actualizada) → animación →
  `LateUpdate` (`AttachPlayer`).
- El literal `-1` de `Glitcheable` no se toca (fuera de alcance).

## Paso 0 — Reproducir y medir (antes de cambiar nada)

Escena 4, una plataforma horizontal y una vertical (identificar en el Editor cuáles tienen ruta en Y).

1. Editor, Quality **Ultra** (vSync on) y **Low** (vSync off, fps > 50). Viajar en aceleración,
   crucero, frenado y parada.
2. Profiler (CPU, Timeline) con un marcador temporal o `Debug` de solo lectura del jugador. Registrar por
   frame: `isGrounded`, posición del jugador relativa a la plataforma y si el frame tuvo
   `FixedUpdate.PhysicsFixedUpdate`. Hipótesis a confirmar: los tirones caen en frames con paso de
   física y son peores al bajar. Ese logging no se commitea.
3. Development build (Windows) con vSync on y off: confirmar a ojo y con el Profiler conectado.
4. Anotar números de referencia: frames con `isGrounded == false` por viaje y amplitud del salto
   relativo. Son la línea base del criterio de aceptación.

## Pasos de código

Cada paso compila solo y se verifica antes del siguiente.

### Paso 1 — `Platform.Rules` (lógica pura) + tests EditMode
- **Nuevo** `Assets/Scripts/Gameplay/Platform/Rules/Platform.Rules.asmdef`: como `Glitch.Rules`, pero con
  `noEngineReferences: false` (usa `Vector3`/`Mathf`). `autoReferenced: true`, así `Assembly-CSharp` lo
  ve.
- **Nuevo** `PlatformStep.cs` (static), con los valores mágicos actuales como constantes nombradas
  (0.15 u, 1.1):
  - `Advance(Vector3 current, Vector3 target, float speed, float dt, float baseThreshold, out bool arrived) → Vector3 delta`.
  - Paso = `speed * dt` recortado a la distancia. Si después del paso la distancia queda dentro del
    umbral (`max(baseThreshold, speed*dt*1.1)`), el delta **llega exacto al target** y `arrived = true`.
  - Unifica `InTarget` + `MoveTowards` y resuelve el snap y el último delta.
- **Nuevo** `PassengerDisplacement.cs` (struct o clase chica): `Add(Vector3)`, `Consume() → Vector3` (la
  devuelve y la resetea). Es la lógica de "acumular, no sobrescribir", testeable.
- **Modificar** `Assets/Tests/EditMode/Glitch.Rules.Tests.asmdef`: agregar la referencia
  `Platform.Rules` (es el asmdef EditMode del proyecto; evita un asmdef más).
- **Nuevo** `Assets/Tests/EditMode/PlatformStepTests.cs`, `PassengerDisplacementTests.cs`:
  - `Advance_FarFromTarget_MovesSpeedTimesDt`, `Advance_StepOvershoots_ClampsToTarget`.
  - `Advance_WithinThreshold_SnapsExactlyAndFlagsArrived`, `Advance_ZeroSpeed_NoDeltaNotArrived`.
  - `Add_TwiceBeforeConsume_Accumulates`, `Consume_ResetsToZero`.
- **Riesgo:** ninguno en runtime (código nuevo sin uso todavía).
- **Verifica:** `/run-tests editmode`.

### Paso 2 — `PlatformMotor` mueve el `Rigidbody`
- `Assets/Scripts/Gameplay/Platform/PlatformMotor.cs`:
  - El constructor (`:8`) recibe `Rigidbody` + `Transform` y pierde el `passenger` que no usaba.
  - `MoveTowards` (`:20-34`) se reemplaza por `Step(target, speed, passenger) → bool arrived`. Usa
    `PlatformStep.Advance` con `Time.deltaTime`, escribe `_rb.position = p` y `_transform.position = p`
    (comentario con el motivo; ver "Por qué la excepción…") y llama `passenger?.OnPlatformMoving(delta)`
    también en el tramo final.
  - `InTarget` (`:14-18`) queda solo si algún estado lo necesita sin mover (`ReachedTarget` en
    `DecelerateToTarget`/estados); si no, se elimina.
  - `Stop` (`:36-39`) se mantiene.
- Nuevo `static class KinematicBody` (en `Assets/Scripts/Gameplay/Platform/`) con
  `Ensure(GameObject) → Rigidbody`: si falta, `AddComponent<Rigidbody>()` con `isKinematic = true`,
  `useGravity = false`, `interpolation = None`, y `LogWarning` (una vez, en `Awake`, nombre del objeto).
  Si existe pero no es kinematic, lo fuerza a kinematic con el mismo warning.
- `PlatformController.Awake` (`PlatformController.cs:53-70`, `Motor = new` en `:64`):
  `var rb = KinematicBody.Ensure(gameObject); Motor = new PlatformMotor(rb, transform);`.
- **Riesgo:** callers de `MoveTowards`: solo `MoveStep` (`trail callers MoveTowards`; los demás hits son
  `Mathf.MoveTowards`). El cambio de firma del constructor afecta solo a `PlatformController.cs:64`.
- **Verifica:** compila; PlayMode harness (Paso 7).

### Paso 3 — Orden de ejecución
- Nuevo `Assets/Scripts/Core/ExecutionOrder.cs` (ver arriba); `[DefaultExecutionOrder(ExecutionOrder.Platform)]`
  en `PlatformController` (`PlatformController.cs:13`).
- **Riesgo:** `PlatformController.Awake` también corre antes (-50). Solo suscribe a `_connection.OnInitialized`
  y lee transforms propios: sin dependencia de otros `Awake`.
- **Verifica:** Play Mode, la escena 4 arranca igual; checklist manual.

### Paso 4 — Llegada exacta en los tres estados + `StopPassenger`
- `PlatformController.MoveStep` (`:164-167`) devuelve el `arrived` de `Motor.Step`.
  `ReachedTarget` (`:154-157`) se mantiene para los chequeos previos al movimiento.
- `MovingState.Tick` (`MovingState.cs:17-52`): mover primero y decidir después. El frame que llega hace
  el tramo exacto (delta transmitido) y recién entonces aplica la lógica de parada (`:21-26`) o de punto
  de paso (`:29-32`). Así no hay frame sin movimiento ni error arrastrado al siguiente tramo.
- `ToStopState.Tick` (`ToStopState.cs:18-31`) y `ReturningState.Tick` (`ReturningState.cs:17-32`):
  mismo patrón.
- `StopPassenger` (`PlatformController.cs:147-152`): `if (_passenger == null) return;`. Así se manda el
  cero explícito. Es inocuo con el acumulador, porque cero suma cero.
- **Riesgo:**
  - Cambia el frame exacto de llegada (antes llegaba a ≤ 0.15 u; ahora al punto). Las plataformas
    quedan en sus anclas exactas: visualmente debería mejorar, pero conviene revisar que ningún diseño
    dependía del sobrepaso.
  - `DecelerateToTarget` (`:207-213`) calcula la duración con la distancia: sin cambios.
- **Verifica:** EditMode del Paso 1; manual: paradas y estaciones.

### Paso 5 — Pasajero: acumular y liberar bien
- `PlayerModel.cs`:
  - `_platformDisplacement` pasa a `PassengerDisplacement`: `OnPlatformMoving` (`:163-166`) hace `Add`, y
    `MovePlayer` (`:105-108`) usa `Consume()`.
  - Eliminar `onVerticalPlatform` (`:21`) y su uso en `:148` (siempre era `false`, así que el
    comportamiento no cambia).
- `PlayerController.SetVerticalPlatform` (`PlayerController.cs:117-121`): quitar `print(bul)`.
- `PlatformController`: un único `ReleasePassenger()` privado, usado por `OnTriggerExit` (`:251-262`) y
  `CleanPlayerReferences` (`:222-229`). Hace:
  - manda cero;
  - `SetVerticalPlatform(false)` si `isVertical` (corrige la gravedad 0 después de morir sobre una
    vertical);
  - desuscribe `OnDied`;
  - nulea `_player` y `_passenger`.

  `OnTriggerEnter` (`:231-249`) no cambia, salvo que use el mismo helper en la rama de jugador muerto.
  Comparar con `== null` (objetos destruidos).
- **Riesgo:** `CleanPlayerReferences` se dispara desde `OnDied` en medio de la corrutina de respawn
  (`PlayerController.cs:463-499`). Restaurar la gravedad ahí es lo correcto, porque el jugador reaparece
  fuera de la plataforma.
- **Verifica:** manual: muerte/respawn sobre plataforma horizontal y vertical; salir y volver a subir.

### Paso 6 — Ascensor (`AttachPlayer`)
- `Assets/Scripts/Gameplay/AttachPlayer.cs`:
  - `Awake`: `_rb = KinematicBody.Ensure(gameObject)`.
  - `LateUpdate` (`:9-20`): después de la animación, `_rb.position = transform.position` (la pose física
    alcanza a la visual sin `SyncTransforms`), delta = pos − `_lastPosition`, se manda **siempre** (sin
    el umbral de `:16`).
  - `OnTriggerExit` nuevo: suelta si es el mismo pasajero (manda cero y limpia).
  - Al morir el jugador (`OnDied`, si el pasajero es `PlayerController`): soltar y desuscribir, con el
    mismo criterio que el Paso 5. `OnDestroy` desuscribe.
- El `Animator` de la raíz **queda en Update normal** (no Animate Physics).
- **Riesgo:**
  - El jugador aplica en el `Update` del frame N+1 el delta medido en el `LateUpdate` del frame N. Contra
    la física es consistente (el piso físico está en la pose N), pero visualmente el jugador queda un
    frame atrás (v·dt; ≈ 1.4 cm a 2 m/s y 144 fps).
  - Si se nota, la alternativa es que la plataforma del ascensor se mueva por código en `Update` antes
    del jugador; queda como `TODO:` que se evalúa con el Paso 0 del ascensor.
  - La puerta (`puerta_Ascensor`, collider animado como hijo) pasa a formar parte del compuesto de la
    raíz: verificar que siga bloqueando.
- **Verifica:** manual en `3 - HUB` (subir, viaje completo, carga de nivel). El ascensor también
  participa en `LevelLoader` con `allowSceneActivation = false` mientras corre la animación.

## Cambios de prefab / escena (Editor, los hace el usuario)

Hacerlos **después** del Paso 2. Antes son inocuos, pero el `Ensure` del código ya cubre cualquier
instancia que falte. En todos: `Rigidbody` con **Is Kinematic = on**, **Use Gravity = off**, **Interpolate
= None**, Collision Detection = Discrete y sin constraints.

1. `Assets/Prefabs/Level/Plataformas01.prefab` → objeto `Plataforma`.
2. `Assets/Prefabs/Modulos/PlataformasMoviles/Plataforma 1.prefab` → raíz. Hereda a
   `Modulos/PlataformasMoviles/Plataformas01.prefab` (anidado).
3. `Assets/Art-2/AssetsFer/FBXs/Reemplazar/Prefabs/Plataformas/Plataforma.prefab` → `Plataforma`.
4. `Assets/Prefabs/Level/Movable Platform.prefab` → raíz.
5. `Assets/Prefabs/Modulos/PlataformasMoviles/PuzzlePlataforma01.prefab` → `Plataforma`.
6. `Assets/Prefabs/Glitcheables/Glitcheable Platform Container.prefab` → `Pos 1` y `Pos 2` (no en
   `GlitcheablePlatform`).
7. `Assets/Prefabs/Ascensor/Asset_Ascensor.prefab` → raíz (`Asset_Ascensor`). El `Animator` sigue en
   **Update Mode: Normal**.
8. Escena 4: marcar `isVertical = true` en las instancias cuya ruta es vertical. Hay que identificarlas en
   el Editor (puntos de `_positions2` con distinta Y). `Plataforma (1)` (sin collider) no se toca: el
   `Ensure` le agrega el `Rigidbody` y no lleva pasajeros.
   - **Gate:** después del arreglo de física, comparar una vertical con y sin `isVertical` (gravedad 0 vs
     −50). Si sin el flag `isGrounded` ya es estable, se decide con el usuario si el flag sigue teniendo
     sentido. `TODO:` decisión de diseño pendiente de esa prueba.
9. Revisar en cada escena que use estos prefabs que no haya overrides que quiten o modifiquen el
   `Rigidbody` (por ahora no existen).

## Paso 7 — Tests PlayMode (harness)

- **Nuevo** `Assets/Tests/PlayMode/Platform.PlayMode.Tests.asmdef`:
  - refs `Platform.Rules`, `UnityEngine.TestRunner`;
  - `overrideReferences: true` + `nunit.framework.dll`;
  - `includePlatforms: []` (todas);
  - `defineConstraints: ["UNITY_INCLUDE_TESTS"]`, `autoReferenced: false`.
- **Nuevo** `Assets/Tests/PlayMode/PlatformCarryTests.cs`. Cada test arma en código:
  - una plataforma (`BoxCollider` + `Rigidbody` kinematic);
  - un driver de prueba (`MonoBehaviour` del asmdef de test, `[DefaultExecutionOrder(-50)]`) que mueve
    con `PlatformStep` + `rb.position`/`transform` y entrega deltas a un `PassengerDisplacement`;
  - un pasajero de prueba con `CharacterController`, que en `Update` hace `Move(gravedad + Consume())`;
  - fija `Time.captureDeltaTime`, lo destruye todo en `[UnityTearDown]` y restaura `captureDeltaTime`.

  Así el test es determinista: corre a 1/144 (vSync off, más frames que pasos de física) y a 1/30 (menos
  frames que pasos).

  Casos:
  - `HorizontalRide_At144Fps_PassengerOffsetStaysConstant` (tolerancia ≈ skin width) y
    `…_At30Fps_…`.
  - `VerticalRideUp_PassengerStaysGroundedEveryFrame` y `VerticalRideDown_PassengerStaysGroundedEveryFrame`.
  - `RideToStop_PlatformEndsExactlyOnTarget_AndPassengerReceivedFullPath`.
  - Negativo: `PassengerOffPlatform_ReceivesNoDisplacement`.
- **Límite de cobertura:** el harness valida el contrato (rules + `rb.position` + orden + CC), **no** el
  cableado real de `PlatformController`/`PlayerController`/triggers, porque `Assembly-CSharp` no se puede
  referenciar. Eso lo cubre la verificación manual.
- **Comando:** `/run-tests all` (EditMode y después PlayMode). Falla si el Editor tiene el proyecto
  abierto: cerrarlo o correr el Test Runner en el Editor y pegar el resultado. Reportar el total, los
  pasados, los fallados y el nombre de cada rojo; los errores de compilación cuentan como rojo.

## Impacto en callers, prefabs y escenas

| Símbolo | Callers (`trail callers` / `trail grep`) | Impacto |
|---|---|---|
| `PlatformMotor.MoveTowards` | `PlatformController.MoveStep:166` (el resto son `Mathf.MoveTowards`) | Se reemplaza por `Step`; un solo caller. |
| `PlatformMotor` ctor | `PlatformController.Awake:64` | Nueva firma `(Rigidbody, Transform)`. |
| `IMovablePassenger.OnPlatformMoving` | `PlatformMotor.MoveTowards:33`, `PlatformMotor.Stop:38`, `PlatformController.OnTriggerExit:255`, `AttachPlayer.LateUpdate:17`; implementación `PlayerController:430` → `PlayerModel:163` | La firma no cambia; la semántica pasa de "set" a "add". Todos los emisores mandan como mucho un delta por frame, o cero. |
| `StopPassenger` | `InactiveState:9`, `WaitingState:19`, `MovingState:24`, `ReturningState:23`, `ToStopState:23` | Ahora sí manda el cero. Sin efecto con el acumulador. |
| `SetVerticalPlatform` | `PlatformController.OnTriggerEnter:245`, `OnTriggerExit:258` (+ el nuevo `ReleasePassenger`) | Se agrega el reset al morir. |
| `IMovablePassenger` | implementado solo por `PlayerController` | Sin cambios. |

Prefabs: los 7 de la sección anterior. Escenas con instancias (heredan del prefab): `4 - NIVEL
PLATAFORMAS`, `1 - TUTO 1`, `2 - TUTO 2`, `3 - HUB` (ascensor), `LVL 1/*`, `BLOCKING/*`, `TEST
SaveLoad Data`, `Posibles referencias (EX OLD)/*`. Los objetos de escena sueltos con `PlatformController`
(como `Plataforma (1)`) los cubre el `Ensure` en runtime.

## Verificación manual (después de los pasos 2-6 y de los prefabs)

Escena 4, Editor y development build, con Quality Ultra (vSync on) y Low (vSync off). En cada caso,
Profiler con las mismas métricas del Paso 0, comparadas contra la línea base:

- [ ] Horizontal: aceleración, crucero, frenado, parada en estación, punto de paso sin parada.
- [ ] Vertical: subir y bajar sin que `isGrounded` titile; con y sin `isVertical` (gate del punto 8).
- [ ] Subir y bajar de la plataforma en movimiento y quieta; volver a subir (el registro y la liberación
      del pasajero funcionan).
- [ ] Muerte y respawn sobre una plataforma (láser y caída): `CleanPlayerReferences` suelta al jugador;
      en una vertical, la gravedad vuelve a −50 después del respawn.
- [ ] La plataforma desconectada se detiene (`ToStopState`) y queda exacta en el punto; al reconectar,
      sigue.
- [ ] Glitcheable sobre plataforma (meter `Glitcheable Platform Container` en una escena de prueba):
      ciclo completo de glitch (desintegrar → mover → reintegrar) mientras el ancla se mueve; el jugador
      no queda registrado/soltado por el trigger de `GlitcheablePlatform` (ver Riesgos).
- [ ] Ascensor de `3 - HUB`: subir, viaje completo con `ElevatorNextLevel`, la puerta bloquea, la carga
      de nivel sigue igual.
- [ ] Save/load: no aplica (no hay sistema; ver Riesgos). Abrir `TEST SaveLoad Data.unity` y comprobar
      que sus plataformas se mueven.
- [ ] Consola sin el `LogWarning` del `Ensure` en la escena 4, salvo `Plataforma (1)`, que se espera.

## Riesgos y seguimiento

- **Triggers del compuesto en el Glitcheable:** con `Rigidbody` en `Pos 1`/`Pos 2`, el trigger de
  `GlitcheablePlatform` (emparentado al ancla) también entrega `OnTriggerEnter/Exit` al GameObject del
  `Rigidbody`, que es donde está `PlatformController`. Salir de ese trigger podría soltar al pasajero
  aunque siga en el trigger propio del ancla.
  - Mitigación, si aparece: mover el trigger del pasajero a un hijo con su propio relay, o darle a
    `GlitcheablePlatform` su propio `Rigidbody` kinematic.
  - Prioridad baja: el prefab no está en ninguna escena. `TODO:` confirmar en Play Mode.
- **`GlitchMovingState` mueve `GlitcheablePlatform` por `transform`** sin `Rigidbody` y apaga solo el
  primer collider. Queda fuera de alcance; anotarlo si da problemas en el ciclo.
- **Excepción a la regla de física:** queda documentada aquí y en el comentario de `PlatformMotor`. Si
  el reviewer la objeta, la alternativa (`FixedUpdate` + Interpolate) tiene el problema descrito en
  "Por qué la excepción…".
- **Ascensor un frame atrás visualmente:** ver Paso 6. `TODO:` medir si se nota.
- **`isVertical`:** que sea útil después del arreglo es `TODO:` (gate del punto 8).
- **Save/load futuro:** cuando exista, restaurar plataformas con `rb.position` + `transform.position` (o
  un método del motor), nunca solo `transform`. `TODO:` ningún sistema hoy.
- **Paso 0 sin hacer:** el diagnóstico sigue siendo hipótesis hasta medir. Si el Profiler muestra otra
  causa (GC, Cinemachine), se replanifica antes del Paso 2.
- **Seguimiento fuera de alcance (no se toca):** `PlatformStateMachine` (`PlatformStateMachine.cs:16-26`)
  llama `TransitionTo(startsActive ? Waiting : Inactive)` en `:19`, antes de crear los estados (`:21-25`).
  `_current` queda en `null` y la plataforma no hace nada hasta el primer `OnNodeConnected`. Confirmar si
  `Connection` dispara ese evento al inicializar; si no, las plataformas `StartsConnected` arrancan
  quietas.
- **Cerrar:** `/run-tests all` en verde y checklist manual completo antes de dar el trabajo por hecho;
  después `/update-unity`.
