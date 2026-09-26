# Refactor del Sistema Glitch — Arquitectura por Capacidad (0/1/2)

> Documento de continuidad. Tiene el plan completo, lo que ya está hecho, lo que falta y las
> trampas encontradas. Alcanza para retomar el trabajo en otra sesión sin más contexto.
>
> Última actualización: 2026-09-23. Último commit del refactor: `07813b8d`.

---

## 1. Por qué

El glitch del juego era **binario**. Un nodo era `Default` o `Corrupted`, un `Glitcheable` estaba
en `Idle` o ciclando, y una sola pulsación de `Interact` invertía ambos a la vez: el estado del
objeto lo cambiaba `Glitcheable.Interact` y el tipo del nodo lo cambiaba `PlayerGrabState`
invocando `PlayerNodeHandler.OnGlitchChange`. La corrupción era un flag que saltaba de un lado al
otro.

El pedido del cliente (`Prompt Claude - Refactor Sistema Glitch.pdf`, en la raíz del repo) es pasar
a un modelo de **capacidad 0..2**: la carga deja de ser un booleano y pasa a ser un recurso que se
transfiere de a una unidad, con un estado intermedio nuevo (`Intangible`). Tres cambios de fondo:

1. La verdad sobre el nivel deja de estar repartida entre `NodeController._nodeType` y la FSM de
   `Glitcheable`, y pasa a un componente único (`GlitchComponent`) que notifica por evento.
2. `Interact` deja de glitchear: queda exclusivo de manipulación física. La transferencia pasa a
   dos acciones, `Set` (nodo→objeto) y `Take` (objeto→nodo), con tap (1 carga) y hold (repetición).
3. La FSM de `Glitcheable` deja de ser interrumpida por el jugador y pasa a reaccionar al nivel:
   2 arranca el ciclo, 0 y 1 la mantienen en `Idle`.

---

## 2. Decisiones cerradas con el usuario

| Tema | Decisión |
|---|---|
| Portador de carga del jugador | Solo el nodo en mano. Sin nodo, `Set`/`Take` dan error y no hacen nada. |
| Migración de valores | `Default(1)`→`Clean(0)`, `Corrupted(2)`→`Glitched(2)`. `_startInIdle==true`→`Clean`, `false`→`Glitched`. Ningún objeto arranca en `Intangible`. |
| Tap vs hold | Timer propio en C# con umbrales de `PlayerData`. **Sin** interactions del Input System. |
| Soltar el nodo | **Sin cambios**: tap de Interact sin objetivo sigue soltando el nodo (ver §8). |
| Prefabs | El usuario agrega `GlitchComponent` y setea el nivel desde el Editor. Hay fallback en runtime. |
| Alcance del asistente | C# + `.inputactions`. Lo visual (materiales, VFX del nivel 1) y el playtest son del usuario. |

---

## 3. Arquitectura nueva

### `GlitchState` — `Assets/Scripts/Gameplay/Glitch/GlitchState.cs`
```csharp
public enum GlitchState { Clean = 0, Intangible = 1, Glitched = 2 }
```
Reemplazó a `NodeType` en todo el proyecto. `Enums.cs` conserva `InteractablePriority`,
`PlayerState` y `StopType`.

**No hay valor centinela.** El viejo `NodeType.None` desapareció: `Clean(0)` es un nivel legítimo,
así que "sin nodo en mano" se pregunta con `PlayerNodeHandler.HasNode`.

### `GlitchComponent` — `Assets/Scripts/Gameplay/Glitch/GlitchComponent.cs`
Contenedor único del nivel. Puntos no obvios:

- **El nivel ES el campo serializado** (`[SerializeField] private GlitchState _currentLevel`), sin
  paso de inicialización. Es imprescindible: `Glitcheable` corre con `DefaultExecutionOrder(-1)` y
  leería en 0 cualquier init hecha en un `Awake` de orden normal — incluido el de este mismo
  componente, que vive en el mismo GameObject pero corre después.
- **`OnGlitchStateChanged` nunca se emite en `Awake`/`Start`.** El estado inicial se lee por *pull*
  (`CurrentState`) desde el `Start` de cada consumidor. Emitirlo sería una carrera con el orden de
  suscripción — de hecho `GlitcheableOrbitController` ya se pierde hoy el `OnStateChanged` inicial
  de la FSM justamente por eso.
- `TryApplyDelta(int)` **rechaza** en vez de clampear lo que se sale de `[0, MaxLevel]`: el clamp
  silencioso destruiría cargas y rompería la conservación del total.
- `GlitchComponent.Ensure(GameObject)`: red de seguridad que lo agrega en runtime en `Clean` con un
  `LogWarning` si un prefab se quedó sin él.

### `GlitchTransferManager` — clase estática
```csharp
public enum GlitchTransferResult { Transferred, NoSource, NoTarget, SourceEmpty, TargetFull, Invalid }

GlitchTransferResult ExecuteTransfer(GlitchComponent source, GlitchComponent target);
GlitchTransferResult Preview(GlitchComponent source, GlitchComponent target);
```
Único punto de mutación de niveles del proyecto — eso es lo que exige la nota de seguridad del PDF
(la suma nodo + objeto se conserva). Atómico: valida ambos extremos antes de mutar y revierte el
primer delta si el segundo fallara. Devuelve un resultado en vez de disparar feedback, para no
meter dependencias de input ni de audio en el dominio.

### `GlitchTransferHandler` — `Assets/Scripts/Player/Components/GlitchTransferHandler.cs`
MonoBehaviour del jugador. **No necesita cableado**: `PlayerController.Awake` lo agrega si falta
(no tiene campos serializados, resuelve todo desde el controller).

- Gate de entrada igual al de `OnInteractPressed`: `!IsDead && CC.isGrounded`.
- Tap = una carga + rumble inicial. Hold = tras `transferHoldDelay`, una carga cada
  `transferRepeatInterval` con un pulso de rumble por carga.
- **El hold corta por seis condiciones**, todas revalidadas por frame porque ninguna avisa:
  objetivo perdido o tapado (`IsInteractableAvailable`), nodo soltado o enchufado (`CurrentGlitch`
  nulo), capacidad agotada (**final limpio, no error**), muerte, cambio de action map, y
  `StopRumble` implícito por usar pulsos con duración.
- La selección de objetivo **filtra por capacidad** (`CanGive`/`CanReceive`). Sin eso, con dos
  glitcheables en rango, `Take` fallaría sobre el que sí servía solo porque el otro estaba unos
  centímetros más cerca.

### `Glitcheable` — reconciliación FSM ↔ nivel
```csharp
private void SyncFsmWithLevel()
{
    if (_syncing) return;              // la FSM emite OnStateChanged DESPUÉS del Enter
    _syncing = true;
    var wantsCycle = Level == GlitchState.Glitched;

    if (wantsCycle && FSM.Current == IdleState) BeginCycle();
    else if (!wantsCycle && FSM.Current != IdleState && FSM.Current is IGlitchInterruptible ii) ii.Interrupt();

    _syncing = false;
}
```
Suscrito a `Glitch.OnGlitchStateChanged` **y** a `FSM.OnStateChanged`. La doble suscripción es la
clave: `GlitchMovingState` no es `IGlitchInterruptible`, así que una descarga en pleno movimiento
no se aplica al toque (partiría el lerp y el reparent de `GlitchMovingState.Exit`); se deja
terminar el movimiento y el mismo método corre de nuevo al entrar a `Reintegrating`, que sí es
interrumpible. Es idempotente, así que converge siempre.

**Las cuatro clases de estado de la FSM se conservan intactas a propósito**:
`LaserController.UpdateGlitchedBehaviour` tiene un `switch` hardcodeado sobre esos cuatro tipos
concretos, con un branch `_ => (false, false)` que dejaría los láseres sin modo.

### `Glitcheable` como `IInteractable`
```csharp
public bool CanInteract(PlayerNodeHandler player) => false;
```
Sigue implementando `IInteractable` **solo para seguir siendo trackeado**: `PlayerInteractionDetector`
registra por interfaz y `GetClosestGlitcheable` sale de esa misma lista, que a diferencia de
`GetInteractable` **no filtra por `CanInteract`** (`InteractableHandler.cs`). El orbit VFX y
`OnPlayerProximity` siguen funcionando igual.

Efecto secundario buscado: su `Priority.Highest` queda **inerte**, porque `GetInteractable` filtra
por `CanInteract` *antes* de ordenar por prioridad. El Glitcheable dejó de tapar al `NodeController`
o a la `Connection` que el jugador quiere usar con Interact.

El HUD lo recibe como **fallback** de `OnInteractableDetected` (`PlayerController.ResolveGlitchHudTarget`),
no por prioridad, y solo si hay transferencia posible en alguna dirección.

---

## 4. Estado: qué está hecho

| Commit | Paso | Contenido |
|---|---|---|
| `96a25385` | 0 | WIP previo aislado: colliders al terminar la desintegración, no a la mitad |
| `6a7349c3` | 1 | `GlitchState`, `GlitchComponent`, `GlitchTransferManager`, `GlitchPalette` — aislados, sin cablear |
| `ecb22672` | 2 | Rename global `NodeType` → `GlitchState` (32 archivos), comportamiento idéntico |
| `e53768cc` | 3 | `GlitchComponent` como fuente de verdad + `Set`/`Take` (⚠️ no compila solo: le faltan las acciones de input) |
| `509b0046` | 3b | Acciones `Set`/`Take` en el `.inputactions` + wrapper + `InputManager`. Interact pasa a `<Keyboard>/e` |
| `618b8cf0` | 3c | `GlitchComponent` en los 11 prefabs con su nivel inicial |
| `8840bdb1` | 4 | Intangibilidad del nivel 1 en `Glitcheable` (ver §5) |
| `07813b8d` | 5 | `GlitchPalette` para todos, crosshair y HUD Set/Take, nivel 1 por color (ver §5) |
| *(sin commitear)* | 3d | Overrides de escena `_startInIdle` → `_currentLevel` en `2 - TUTO 2` y `4 - NIVEL PLATAFORMAS` (ver §6) |

### Paso 2 en detalle — rename mecánico
`Default`→`Clean`, `Corrupted`→`Glitched` en todo el fan-out. Donde `None` hacía de gate ahora va
`HasNode`:

- `Connection.OnPlayerProximity` — **el crítico**: con `_requiredType` migrando a `Clean`, sin el
  gate *todas* las conexiones se habrían anunciado como disponibles con la mano vacía.
- `Glitcheable.CheckStateChange` recibía `hasNode` (después desapareció entera en el paso 3).
- `BackPackColorChange` — el chequeo viejo habría pintado de negro un nodo limpio.
- `UpdateCrosshair` — ídem, y se explicitó el gate en la comparación de compatibilidad, que sin él
  daba "compatible" con la mano vacía.

Campos serializados renombrados **junto con su valor en los prefabs**, para que el mapeo quedara
explícito en el diff en vez de depender de que Unity descarte la clave vieja:

| Campo viejo | Campo nuevo | Prefabs |
|---|---|---|
| `NodeController._nodeType` | `_glitchLevel` (efímero, desapareció en el paso 3) | `DefaultNode` 1→0, `Corrupted Node` 2→2 |
| `Connection._requiredType` | `_requiredLevel` | `Connection_Pared`, `Connection_Piso`, `Wall_Connection`: 1→0 |
| `DoorController.tipo` | `_requiredLevel` | `PuertaDelgada01`: 1→0 |

`NodeController.NodeType` (la propiedad) pasó a llamarse `Level`: no podía quedar con el mismo
nombre que su tipo.

### Paso 3 en detalle
- `NodeController`: sin `_glitchLevel`; usa `Glitch.CurrentState`, se suscribe a
  `OnGlitchStateChanged` y **re-emite `OnUpdatedNodeType`**. Se fueron `InteractWithGlitcheable`,
  `UpdateNodeType` y las suscripciones a `OnGlitchChange` de `Attach`. Resolución de color a tres
  bandas (cyan `#00FFFF` / amarillo `#FFD600` / magenta `#F200FF`) hardcodeada por ahora.
- `PlayerNodeHandler`: `CurrentLevel` pasa a **derivarse** del nodo (sin copia local que se
  desincronice), se agrega `CurrentGlitch`, muere `OnGlitchChange` y nace `OnNodeLevelChanged`.
- `Glitcheable`: `CanInteract => false`, estado inicial derivado del nivel, `SyncFsmWithLevel`,
  se fueron `_startInIdle` y `CheckStateChange`.
- `PlayerGrabState`: se borró la rama `is Glitcheable` (código muerto con `CanInteract => false`).
- `InputHandler`: expone `OnSetStart/Cancel` y `OnTakeStart/Cancel` (`started`/`canceled`, igual que
  Interact). **Se volvió idempotente** con un flag `_hooked`: `EnableInputs` se llama desde `Start`,
  `OnRespawned` y `SetCollisions(true)` sin `DisableInputs` garantizado en el medio, y con
  transferencias de ±1 una doble suscripción movía dos cargas por pulsación. Al desenganchar emite
  los `canceled`, que de otro modo nunca llegan (un hold seguiría vivo con el jugador muerto).
  Se borró el evento huérfano `OnCorruptionChange`.
- `PlayerData`: campos nuevos bajo `[Header("Glitch Transfer")]` — `transferHoldDelay`,
  `transferRepeatInterval`, `transferInitialRumble`, `transferHoldRumble`, `transferErrorRumble`,
  `transferErrorRumbleDuration`. **No tocar `holdInteractionTime`**: vale `0` en el `.asset` y lo
  lee `PlayerEmptyState`.
- `InteractableHandler.GetClosestGlitcheable` gana un overload con predicado.
- `PlayerController`: accesor `Data`, `FindTransferTarget(filter)`, `ResolveGlitchHudTarget()` y el
  auto-agregado de `GlitchTransferHandler`.
- `PostProcessController` (está en `Assets/Art/Shaders/...`, **fuera de `Assets/Scripts`**): pasa de
  `OnGlitchChange` a `OnNodeLevelChanged`.

---

## 5. Estado: qué falta

### Bloqueante para poder probar
Nada del lado del código ni de los datos. Falta el playtest de §9 (ítems 4–13).

Hecho en `509b0046`: `Set` = `<Mouse>/leftButton` + `<Gamepad>/buttonWest`, `Take` =
`<Mouse>/rightButton` + `<Gamepad>/buttonEast`, cada binding en su control scheme. `Interact` pasó
a `<Keyboard>/e`. `PlayerInputs.cs` embebe el JSON del asset (`InputActionAsset.FromJson`), así que
cualquier cambio futuro de bindings se hace desde la ventana de Input Actions, no a mano.

⚠️ `CameraLeft`/`CameraRight` del map `Player` también están en click izquierdo/derecho. Hoy no
choca porque su único consumidor (`FollowController`) está comentado; si se reactiva la rotación de
cámara por mouse, hay que mover uno de los dos.

### Paso 4 — Intangibilidad (nivel 1) — ✅ implementado en `8840bdb1`, falta playtest (§9, 14–18)
La regla del PDF: si el objeto **y** el nodo en mano están ambos en `Intangible(1)`, el objeto se
atraviesa.

**Cómo quedó** (`Glitcheable.cs`), con dos agregados al diseño de abajo:
- `SetColliders` (lo llaman los 4 estados de la FSM) solo guarda `_fsmWantsSolid`; `ApplyPhysicsState`
  es el único que escribe `enabled`/`isTrigger`. Se respeta el `isTrigger` original del prefab
  (`_baseIsTrigger`).
- Volver a sólido se difiere con `Physics.OverlapCapsuleNonAlloc` sobre la cápsula real del
  `CharacterController` (radio − 2 cm para que apoyarse en una cara no cuente), no con el AABB.
- **Agregado:** tras cambiar `isTrigger`, el objeto se re-anuncia al detector cada frame durante
  0.25 s. PhysX rearma el par con el trigger del detector y un `OnTriggerExit` tardío lo daría de
  baja por `Untrack` → de nuevo el softlock.
- **Consecuencia no prevista:** `m_QueriesHitTriggers: 0` en `DynamicsManager`, así que mientras es
  intangible el objeto **no bloquea láseres ni línea de visión**. Parece coherente con "intangible",
  pero confirmarlo en playtest (un cubo que tapaba un láser deja de taparlo).
- En `Glitcheable Stairs` y `Glitcheable Platform Container`, `_coll` es el collider sólido de un
  hijo; el trigger del objeto raíz es el viejo de detección y no se toca.

**No implementarlo apagando el collider.** `PlayerInteractionDetector.IsPresent` exige al menos un
collider `enabled && activeInHierarchy`; apagarlo saca el objeto del `InteractableHandler`,
`GetClosestGlitcheable` deja de devolverlo y el jugador **no puede hacer `Take` para recuperar la
carga**. El estado se autosostiene → **softlock duro**.

Solución: `_coll.isTrigger = true`. El detector solo mira `enabled`/`activeInHierarchy`, así que
sigue registrado y targeteable, y el `CharacterController` lo atraviesa igual.

Un resolvedor único, porque la FSM también escribe el collider:
```csharp
private void ApplyPhysicsState()   // único que toca _coll + llama a RescanInteractable
{
    _coll.enabled   = _fsmWantsSolid;   // lo que ya deciden Idle/Dis/Mov/Rei
    _coll.isTrigger = _isIntangible;
}
```

La evaluación va **por pull en el `Update`** que `Glitcheable` ya corre para el `FSM.Tick`:
```csharp
var wantsIntangible = Glitch.CurrentState == GlitchState.Intangible
                   && FSM.Current == IdleState
                   && PlayerNodeHandler.Instance != null
                   && PlayerNodeHandler.Instance.HasNode
                   && PlayerNodeHandler.Instance.CurrentLevel == GlitchState.Intangible;
if (wantsIntangible != _isIntangible) ApplyPhysicsState();
```
Por evento no alcanza: si el jugador muere con el nodo en 1, `RespawnPlayer` hace `ResetTracking()`
y el objeto deja de recibir proximidad, quedando intangible para siempre.

Dos casos a manejar explícitamente:
- **Volver a sólido con el jugador adentro del volumen** (suelta el nodo, muere, o el nodo cambia
  de nivel): diferir la restauración mientras el bounds de `_coll` contenga la cápsula del jugador,
  reintentando por frame. Sin esto el `CharacterController` queda trabado o sale disparado.
- **Jugador parado encima** de `Glitcheable Stairs` / `Glitcheable Platform Container`: al volverse
  intangible se cae, y si hay `Void` abajo se muere. Dejar un `Debug.LogWarning` bajo el flag
  `debug` que la clase ya tiene, y decidir en playtest si hay que bloquearlo.

Contrapartida aceptada: `TryGetAimPoint` descarta triggers, así que mientras el objeto es intangible
el chequeo de línea de visión cae al fallback por pivote en vez del centro del collider. Solo aplica
durante esa ventana.

### Paso 5 — Visual y UI — ✅ código en `07813b8d`, falta playtest (§9, 20–26) y trabajo de Editor

**Decisión del usuario (2026-09-23):** una sola `GlitchPalette` para todos los consumidores, aunque
se pierdan los colores HDR afinados por prefab. Para no aplanar intensidades la paleta tiene dos
lecturas: `ColorFor` (base: UI, partículas, `NodeController`) y `EmissionFor` = base ×
`_emissionIntensity` (3 por defecto: outline, cristal, mochila).

- `Assets/Resources/GlitchPalette.asset` + `GlitchPalette.Default` (lazy `Resources.Load`; si falta,
  instancia en memoria con warning). Sin cableado en prefabs ni escenas.
- Se borraron los colores serializados de `OutlineController`, `CristalNodeView`,
  `BackPackColorChange` y `UpdateCrosshair`, y los hardcodeados de `NodeController`. Los valores
  viejos quedan huérfanos en el YAML de los prefabs hasta que Unity los re-guarde (inofensivo).
- **Crosshair**: visible si `Preview` da transferencia posible en alguna dirección (misma regla que
  `ResolveGlitchHudTarget`), color = `ColorFor(glitcheable.Level)`.
- **HUD** (`UIActionController`): Glitcheable → `"Set"`, `"Take"` o `"Set / Take"` (solo pasa con
  objeto y nodo en 1) y cambia el ícono con `InputPromptIcon.SetAction`. `PromptAction` suma `Set` y
  `Take` **al final** del enum. El nodo pasó de `"Take"` a `"Grab"` para no chocar con la acción.
- **Nivel 1 sin tocar shaders**: `_IsGlitched` (S_GlowCharacter) e `_isCorrupted`
  (S_NodeLiquidEffect) son booleanos con `Branch`, así que un 0.5 no sirve. Intangible se distingue
  por color: `PlayerView.PlayNodePS` tiñe `_defaultPS` con la paleta (y restaura el color original
  del prefab al volver a Clean), y el orbe `Idle` se tiñe igual.
- **Orbe**: `PSType` suma `Intangible` (al final). Si un prefab trae un orbe propio con ese tipo,
  reemplaza al `Idle` teñido. Escucha `OnGlitchStateChanged` (0↔1 no cambia la FSM) y ahora tiene
  `OnDestroy` completo.

**Queda del lado del usuario (Editor):**
- Sprites de `Set` y `Take` en `InputPromptDatabase.asset`. Sin ellos el ícono se oculta
  (`_hideWhenNoSprite`) y solo se ve el texto.
- Ajustar `GlitchPalette.asset` (colores e intensidad) mirando el juego.
- Opcional: tercer estado real en los shaders (`S_GlowCharacter`, `S_NodeLiquidEffect`, `S_Glitch`,
  `S_Glitch 1`, `S_ParedConnectionActive`) y orbes `PSType.Intangible` en los prefabs.

### Limpieza oportunista
`GlitcheableDetector` (huérfano, nunca instanciado), `PlatesGlitcheable` (subclase vacía),
`PlayerController.CheckInteractionOutcome` y `CheckForWalls` (sin llamadores),
`TimerController.ResetToBaseline` (sin llamadores), `PlayerState` en `Enums.cs` (sin usos).
~~`OnDestroy` faltante de `GlitcheableOrbitController`~~ (hecho en `07813b8d`).

**No borrar** `PlayerNodeHandler.IsCorrupted` / `BeginCorruption` / `OnAbsorbCorruption` sin mirar
antes `Assets/Art/Shaders/Post-Process/Post_Process_Graphs/PostProcessController.cs`: está fuera de
`Assets/Scripts` y se suscribe a `OnAbsorbCorruption`. `BeginCorruption` sigue sin llamadores, pero
su evento **sí** tiene consumidor.

---

## 6. Migración de datos (prefabs y escenas)

### Prefabs — ✅ commit `618b8cf0`

| Prefab | Nivel | `GlitchComponent` fileID |
|---|---|---|
| `DefaultNode` | `Clean` | `-7960687767686481407` |
| `Corrupted Node` | `Glitched` | — |
| `Glitcheable Container` | `Glitched` | `8869202891173033100` |
| `Glitcheable Platform Container` | `Glitched` | `8554073806419270299` |
| `Glitcheable Stairs` | `Clean` | `1268477227201854727` |
| `Glitcheables/Glitcheable Platform Idle Container` ⚠️ | `Glitched` | `3802240224278305597` |
| `Laser/Glitched/GlitcheableLaserContainer` | `Glitched` | `7995906052468721863` |
| `Laser/Glitched/GlitcheableDoubleLaserContainer` | `Glitched` | `-4509672473723742838` |
| `Laser/Glitched/GlitcheableLDoubleLaserContainer` | `Glitched` | `-5859246148345164490` |
| `Laser/Glitched/GlitcheableTripleLaserContainer` | `Glitched` | `1316344227478579585` |
| `Laser/Glitched/GlitcheableQuadLaserContainer` | `Glitched` | `2135451157074704967` |

### Escenas — ✅ hecho, ⚠️ sin commitear

**Trampa:** borrar `_startInIdle` del prefab no borra los overrides de las instancias en escena.
Quedan huérfanos, Unity los ignora sin avisar, y la instancia hereda el nivel del prefab. Con los
prefabs en `Glitched`, las instancias que tenían `_startInIdle: 1` habrían empezado a ciclar.

Migrado editando el YAML con Unity cerrado: cada override `_startInIdle: 1` pasó a un override
`_currentLevel: 0` sobre el fileID del `GlitchComponent` de ese prefab, y los `_startInIdle: 0` se
borraron (heredan el 2).

| Escena | → `_currentLevel: 0` | borrados |
|---|---|---|
| `2 - TUTO 2` | 5× Container, 1× Platform Idle, 1× Laser, 1× TripleLaser | 1× DoubleLaser |
| `4 - NIVEL PLATAFORMAS` | 3× Container, 1× Platform Idle | 1× Container |
| `1 - TUTO 1` | — (ya tenía un override manual `_currentLevel: 2`, hoy redundante) | — |

Verificado: `grep -c _startInIdle` da 0 en las 5 escenas. **Sin commitear** porque las escenas
tienen además cambios ajenos al refactor; al commitear, revisar el diff de cada escena.

⚠️ *"Platform Idle Container" tenía `_startInIdle: 0`, o sea **no** arrancaba en idle pese al
nombre. El mapeo lo manda a `Glitched`; conviene confirmarlo en playtest.*

Si un prefab se queda sin el componente **no explota**: `GlitchComponent.Ensure` lo agrega en
runtime en `Clean` y deja un `LogWarning` con el nombre del objeto. Pero el nivel inicial se pierde,
así que los objetos que deberían ciclar arrancan estáticos.

Además: crear el `.asset` de `GlitchPalette` y lo visual del nivel 1 (paso 5).

---

## 7. Herramienta: compilar sin abrir Unity

```bash
bash .claude/check-build.sh
```
Compila `Assembly-CSharp` fuera del Editor reusando las referencias y los `DefineConstants` que
Unity ya escribió en `TheLastEngineer/Assembly-CSharp.csproj`, con el Roslyn que trae el Editor.
La lista de archivos sale del csproj más todo `.cs` nuevo bajo `Assets/Scripts`, así que los
archivos agregados entran sin regenerar el csproj.

Detalle: hay **runtime** de .NET pero **no SDK**, así que `dotnet build` no funciona — el script
invoca `csc.dll` directo con un response file.

**No valida nada de Unity en sí** (serialización, prefabs, orden de ejecución): solo que compile.

---

## 8. Riesgos y trampas conocidas

- **Drop accidental del nodo.** Con `CanInteract => false`, apretar Interact con un nodo en mano
  parado frente a un Glitcheable da `target == null` → `HandleFailedInteraction` → `DropNode()`.
  Ya pasaba antes, pero ahora pasa mucho más seguido. Se dejó así por decisión explícita; si molesta
  en playtest, el fix es hacer el drop un hold (campo `dropHoldTime` nuevo, **no** reusar
  `holdInteractionTime`). Ningún `IInteractable` usa `RequiresHoldInteraction` hoy, así que el timer
  de hold en `PlayerGrabState` es territorio virgen.
- **Solvencia de los puzzles.** Con ±1 y conservación, apagar un objeto en `Glitched(2)` cuesta
  **dos** `Take` y el nodo queda lleno. Todas las `Connection` migraron a pedir `Clean(0)`, así que
  para enchufar hay que **descargar** el nodo primero: tiene que haber un sumidero a mano en cada
  escena. `PlatformTeleport` exige un nodo `Glitched(2)`, o sea dos cargas. **Auditar escena por
  escena.**
- **Asimetría preexistente de `Connection`.** `SetNode` no dispara `OnNodeConnected(true)` si el
  nivel no coincide, pero `UnsetNode` **sí** dispara `(nivel, false)`. Antes era casi inalcanzable;
  con tres niveles se vuelve el caso común. Revisar `DoorsController`, `PlatformController` y
  `ParedFillConnection` por si un `false` sin `true` previo los deja en un estado raro.
- **Blip de audio al descargar en movimiento.** Al descargar un objeto en `Moving`, se entra a
  `Reintegrating` (que arranca el `endSFX`) y recién ahí se corta a `Idle`. Es audible. Si molesta,
  se pule en el paso 5.
- **Layers.** `SetBoolCorrupted` sigue alternando entre `Laser Collition` (8) y `Glitched` (18).
  Los 5 prefabs de láser ya tienen ambos en `_layer` (`m_Bits: 262464`) y en la matriz de colisión
  ambos layers colisionan con lo mismo, así que el cambio solo afecta filtrado por máscara.
  No requiere tocar nada.
- **El working tree tiene cambios ajenos al refactor** (materiales, prefabs de `Laser/Common`,
  `CameraContainer`, `Critstal Light`, las 4 escenas). Al commitear, stagear por archivo.

---

## 9. Verificación

### Regresión tras el paso 2 (`ecb22672`) — confirmada por el usuario
1. `1 - TUTO 1`: levantar nodo, glitchear/des-glitchear, conectar. Color del nodo, mochila,
   partículas y crosshair.
2. Con la **mano vacía**, acercarse a una `Connection` → **no** debe prenderse el feedback de
   "disponible".
3. Las 4 escenas de punta a punta.

### Tras el paso 3 (`e53768cc`) — ✅ confirmado por el usuario (2026-09-23)
4. `Take` tap sobre objeto `Glitched`: objeto 2→1, nodo 0→1. Segundo tap: 0 y 2. La suma se mantiene.
5. `Take` hold: primera carga al contacto, después cadencia; corta solo al llegar al tope.
6. `Set`/`Take` con la mano vacía: sonido de error + rumble seco, nada más.
7. Soltar o conectar el nodo a mitad de un hold: corta sin `NullReferenceException` y sin rumble colgado.
8. Morir (láser o caída) a mitad de un hold: no sigue transfiriendo, el pad deja de vibrar.
9. Pausar a mitad de un hold y despausar.
10. `3 - HUB`: `Take` sobre un objeto **en movimiento** → la transferencia entra, el objeto
    **termina el lerp** y recién ahí vuelve a `Idle`, parenteado al anchor correcto. Ningún objeto
    flotando entre anchors.
11. `4 - NIVEL PLATAFORMAS`: los láseres siguen prendiéndose y apagándose con el ciclo.
12. `4 - NIVEL PLATAFORMAS`: `PlatformTeleport` exige nodo `Glitched(2)` — verificar que sea alcanzable.
13. Cada puzzle de las 4 escenas completable con la aritmética ±1.

*Esperado y no-bug en esta etapa:* el prompt del HUD sigue diciendo "Glitch"/"Un-Glitch", la regla
del crosshair sigue binaria, y el nivel 1 no hace nada especial en el objeto (queda en `Idle`).

### Tras el paso 4 (`8840bdb1`) — ✅ confirmado por el usuario (2026-09-23)
14. Objeto en 1 + nodo en 1 → se atraviesa, **y el crosshair lo sigue mostrando y `Take` lo sigue
    pudiendo targetear**, incluso parado adentro. Si no, es el softlock.
15. Estando **dentro** del volumen intangible, soltar el nodo (o hacer Set/Take y que el nodo deje
    de estar en 1) → la solidez no se restaura hasta salir; el jugador no queda trabado ni sale
    disparado.
16. Estando dentro del volumen intangible, **morir** → no queda trabado al reaparecer. Si reaparece
    con el nodo todavía en 1, el objeto sigue intangible (es la regla); si no, vuelve a sólido.
17. Parado **encima** de `Glitcheable Stairs` y volverlo intangible → ver qué pasa y decidir (con
    `debug` activo en el `Glitcheable` sale un warning).
18. Objeto en 1 con nodo en 0 o en 2 → **sólido**. Solo 1+1 da intangible.
19. Objeto intangible entre un láser y su receptor → el láser lo atraviesa. Decidir si es lo buscado.

### Tras el paso 5 (`07813b8d`) — pendiente
20. Nodo en 0 / 1 / 2: nodo, outline, cristal, mochila y partículas del jugador en cyan / amarillo /
    magenta. Al volver a 0, las partículas del jugador recuperan su color original.
21. Crosshair: aparece solo si Set o Take pueden mover carga; con la mano vacía, nunca. Color = nivel
    del objeto.
22. HUD sobre un glitcheable: dice `Set` o `Take` según corresponda, `Set / Take` con objeto y nodo en
    1. Sobre un nodo dice `Grab`; sobre una conexión, `Put`, con el ícono de Interact.
23. Cambiar de dispositivo (teclado ↔ pad) con el HUD visible: el ícono se actualiza.
24. Orbe de un glitcheable al pasar de 0 a 1 y de 1 a 0 estando cerca: se tiñe y se destiñe sin
    apagarse.
25. Comparar la intensidad del brillo con la de antes (la emisión del cristal era ~7.6 y ahora es
    base × 3). Ajustar `_emissionIntensity` en `Resources/GlitchPalette.asset`.
26. Consola sin el warning `[GlitchPalette] No se encontro Resources/GlitchPalette.asset`.

---

## 10. Cómo retomar

1. Leer este archivo y §5 §6 para ver qué falta.
2. Correr `bash .claude/check-build.sh` para confirmar que el árbol compila.
3. ~~Terminar los prefabs de §6 y los bindings de teclado de §5.~~ Hecho (2026-09-23). Falta
   commitear los overrides de las escenas `2 - TUTO 2` y `4 - NIVEL PLATAFORMAS`.
4. ~~Playtestear la lista 4–13 de §9.~~ Confirmado. Controles: Set = click izq. / □,
   Take = click der. / ○, Interact = E / ✕.
5. ~~Paso 4 (intangibilidad).~~ Implementado en `8840bdb1`.
6. ~~Playtest 14–19 de §9.~~ Confirmado. ~~Paso 5 (visual y UI).~~ Código en `07813b8d`.
7. **← Acá estamos.** Sprites de Set/Take en `InputPromptDatabase.asset`, playtest 20–26 de §9 y
   ajuste de `GlitchPalette.asset`. Después, limpieza oportunista (§5) y commitear las escenas.
6. Al terminar cambios en `Assets/Scripts`, correr `/update-project` para refrescar
   `.claude/PROJECT_MAP.md`, como pide el `CLAUDE.md` de la raíz.
