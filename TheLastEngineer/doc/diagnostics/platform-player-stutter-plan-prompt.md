# Prompt: plan de acción para el stutter del jugador sobre plataformas

Tu tarea es armar un **plan de acción** para corregir el stutter del jugador cuando lo lleva una
plataforma móvil. **No implementes nada**: no edites código, prefabs, escenas ni settings. El único
archivo que escribís es el plan.

## Punto de partida
1. Leé `doc/diagnostics/platform-player-stutter.md` (diagnóstico por lectura de código; no reproducido
   ni medido con el Profiler). Tratá sus afirmaciones como hipótesis: verificá cada `file:line` que cites
   en el plan contra el código actual.
2. Leé `CLAUDE.md`, `.claude/rules/` (en especial `rules.md`, `solid.md`, `runtime-performance.md`),
   `doc/context/architecture-runtime.md`, `doc/context/project-map.md` y los `.trail/learnings/` que
   toquen plataformas, física o el jugador. Usá `trail ask` / `trail callers` antes de grepear.
3. Cargá las skills `unity-performance`, `unity-gameplay-development`, `design-pattern-selection` y
   `unity-testing`.

## Lo que ya decidió el usuario (no lo vuelvas a preguntar)
- **Síntoma:** pasa en plataformas **horizontales y verticales**, y **también en build** (no es solo
  el Editor). Escena principal afectada: `Assets/Scenes/BUILD SCENES/4 - NIVEL PLATAFORMAS.unity`.
- **Enfoque de física:** **Rigidbody kinematic** (interpolación según corresponda) en los prefabs de
  plataforma, moviendo con APIs de `Rigidbody` en vez de `transform.position +=`. Se aceptan cambios de
  prefab hechos en el Editor. No proponer `Physics.SyncTransforms`/`autoSyncTransforms` como solución
  principal (sí podés mencionarlos si hacen falta puntualmente, justificando).
- **Alcance:** causa principal (física + orden de `Update` entre `PlatformController` y
  `PlayerController`) **+ bugs menores** (condición invertida de `StopPassenger`, snap exacto y último
  delta al llegar a una parada, acumular en vez de sobrescribir `_platformDisplacement`) **+ ascensor**
  (`Assets/Scripts/Gameplay/AttachPlayer.cs`: falta `OnTriggerExit`, umbral de 1 cm/frame) **+ tests
  PlayMode** que cubran al jugador sobre una plataforma en movimiento.
- **Entrega:** el plan va en `doc/plans/platform-player-stutter-plan.md`.

## Hechos verificados después del diagnóstico (revisalos, no los asumas)
- La llegada sin snap no está solo en `MovingState`: `ToStopState` y `ReturningState`
  (`Assets/Scripts/Gameplay/Platform/StateMachine/`) tienen el mismo patrón `ReachedTarget()` → return
  sin mover al punto exacto. `InactiveState` y `WaitingState` también llaman `StopPassenger()`.
- `Glitcheable Platform Container.prefab` combina `PlatformController` con `Glitcheable`
  (`[DefaultExecutionOrder(-1)]`, `IsPlatform`, lista de posiciones `_newPosList`,
  `SetColliders(bool)`). Averiguá si `Glitcheable` mueve/teletransporta el transform de la plataforma o
  apaga sus colliders, y cómo convive eso con un Rigidbody kinematic y con el orden de ejecución que
  propongas.
- `PlatformController` también está en `Assets/Scenes/TEST LEVELS/Juli/TEST SaveLoad Data.unity`:
  verificá si el guardado/carga restaura la posición de plataformas escribiendo `transform` (con
  Rigidbody eso necesita `rb.position` o sync).
- `QualitySettings` mezcla niveles con `vSyncCount: 0` y `1` → el fps (y por lo tanto la relación
  fps / fixed timestep 0.02) depende del nivel de calidad. El plan de verificación debe cubrir ambos.
- El trigger que registra al pasajero y el collider sólido están en el mismo prefab; al agregar el
  Rigidbody, ambos pasan a ser parte de su compuesto. Confirmá que `OnTriggerEnter/Exit` con el
  `CharacterController` siga funcionando.
- Prefabs con `PlatformController` (grep del GUID `6689ebcc1ca873f48958fcb7a88b9bd5`):
  `Art-2/.../Plataformas/Plataforma.prefab`, `Glitcheables/Glitcheable Platform Container.prefab`,
  `Level/Movable Platform.prefab`, `Level/Plataformas01.prefab`,
  `Modulos/PlataformasMoviles/Plataforma 1.prefab`, `Modulos/PlataformasMoviles/PuzzlePlataforma01.prefab`.
  Revisá si hay variantes o anidados que hereden el cambio y en qué prefab conviene poner el Rigidbody.

## Preguntas antes de responder (obligatorio)
Antes de escribir el plan final, **hacé todas las preguntas que el código no resuelva**, con
`AskUserQuestion`, y esperá las respuestas. Reglas:
- Primero investigá: no preguntes nada que se pueda responder leyendo código, prefabs (por grep del
  GUID, nunca leyendo el archivo entero), settings o learnings.
- No repitas lo que ya está decidido arriba.
- Ejemplos de lo que sí puede requerir preguntar: si el loop de movimiento de plataforma y jugador debe
  quedar en `Update` o pasar a `FixedUpdate` cuando ambas opciones son viables; si alguna plataforma
  debe poder llevar otros objetos además del jugador (nodos soltados encima, etc.); quién edita los
  prefabs en el Editor y si hay trabajo de otro integrante abierto sobre esos prefabs o la escena;
  criterio de aceptación del "sin stutter" (a ojo, Profiler, fps objetivo).
- Si después de responder aparecen dudas nuevas, preguntá de nuevo antes de cerrar.

## Contenido del plan
- `Design: <patrón> because <problema>` o `Design: no pattern — <razón>` (skill
  `design-pattern-selection`); nada sobre-diseñado.
- Rama de trabajo propuesta (el repo está en `main`; no se commitea sin pedido).
- Paso 0: reproducción y medición (Profiler: Physics, frames con paso de física, `isGrounded`) en
  horizontal y vertical, Editor y build, con vSync on y off.
- Pasos ordenados y pequeños; para cada uno: archivos y `file:line`, qué cambia, por qué, riesgos,
  qué lo verifica. Separá cambios de código de cambios de prefab/Editor.
- Orden de ejecución: constante nombrada para `[DefaultExecutionOrder]` y su relación con
  `Glitcheable` (-1) y el jugador.
- Impacto en callers (`trail callers` de `PlatformMotor.MoveTowards`, `OnPlatformMoving`,
  `StopPassenger`, `IMovablePassenger`), prefabs y escenas.
- Tests PlayMode: qué casos, dónde viven, qué asmdef necesitan; y el comando `/run-tests` a correr.
- Verificación manual: aceleración, crucero, frenado, paradas, subir/bajar en vertical sin que
  `isGrounded` titile, muerte/respawn sobre la plataforma (`CleanPlayerReferences`), plataforma
  glitcheable, ascensor del `3 - HUB`, save/load.
- Riesgos abiertos y lo que quedó como `TODO:` sin confirmar.

Al terminar, devolvé un resumen corto del plan y la ruta del archivo.
