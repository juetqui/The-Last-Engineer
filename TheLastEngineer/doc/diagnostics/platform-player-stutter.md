# Diagnóstico: stutter del jugador sobre plataformas móviles

**Estado:** diagnóstico por lectura de código y settings (2026-11-13). **No** reproducido en Play Mode ni
medido con el Profiler. No se cambió código.

**Síntoma:** cuando una plataforma (`PlatformController`) lleva al jugador, el jugador hace "stutter"
(tirones / vibración respecto de la plataforma).

## Cadena actual del movimiento

1. `PlatformController.Update` → `PlatformStateMachine.Tick` → `MovingState.Tick` → `PlatformController.MoveStep`
   → `PlatformMotor.MoveTowards` (`Assets/Scripts/Gameplay/Platform/PlatformMotor.cs:20-34`):
   - mueve la plataforma con `_transform.position += move;`
   - llama `passenger.OnPlatformMoving(move)`.
2. `PlayerController.OnPlatformMoving` (`Assets/Scripts/Player/MVC/PlayerController.cs:430`) →
   `PlayerModel.OnPlatformMoving` (`Assets/Scripts/Player/MVC/PlayerModel.cs:163-166`): solo guarda
   `_platformDisplacement = displacementPerFrame` (sobrescribe, no acumula).
3. `PlayerController.Update` (`PlayerController.cs:153`) → `PlayerModel.OnUpdate`
   (`PlayerModel.cs:103-108`): `_cc.Move(horizontal + vertical + _platformDisplacement)` y luego
   `_platformDisplacement = Vector3.zero`.
4. El pasajero se registra en `PlatformController.OnTriggerEnter/Exit` (`PlatformController.cs:231-262`),
   con un `BoxCollider` trigger en el prefab.

## Causas (en orden de impacto estimado)

### 1. Collider estático movido por transform + `autoSyncTransforms` apagado (causa principal)
- `ProjectSettings/DynamicsManager.asset`: `m_AutoSyncTransforms: 0`. `TimeManager`: Fixed Timestep 0.02.
- Los prefabs de plataforma (`Assets/Prefabs/Level/Movable Platform.prefab`,
  `Assets/Prefabs/Modulos/PlataformasMoviles/Plataforma 1.prefab`) tienen `BoxCollider` sólido **sin
  `Rigidbody`** → collider estático movido escribiendo `transform`.
- La escena de física solo ve la nueva pose del collider en el siguiente paso de simulación (cada 20 ms).
  Con fps > 50, varios frames de `CharacterController.Move` chocan contra el collider en su pose vieja:
  el CC es empujado / bloqueado, `isGrounded` titila, y cuando la física se sincroniza el collider "salta".
- Patrón esperado: tirón periódico ligado a la relación fps / fixed timestep. Peor al **bajar** y en
  plataformas verticales.
- Además viola `.claude/rules/runtime-performance.md` ("move rigidbodies with Rigidbody APIs, never by
  writing transform"); mover colliders estáticos es caro en PhysX.

### 2. Orden de `Update` entre plataforma y jugador no definido
- `PlatformController.cs.meta` y `PlayerController.cs.meta`: `executionOrder: 0`; no hay
  `[DefaultExecutionOrder]`.
- Si el jugador corre primero, en el frame N aplica el desplazamiento del frame N-1 (calculado con otro
  `deltaTime`) y queda un frame atrás. Como la velocidad cambia con los tweens de aceleración/frenado
  (`StartAccelerationToMax`, `ContinueWithNewSegmentSpeed`, `StartDeceleration` en
  `PlatformController.cs:180-213`) y el `deltaTime` varía, el desfase oscila → jitter visible (la cámara
  sigue al jugador y lo amplifica).

### 3. Bugs menores
- `PlatformController.StopPassenger` (`PlatformController.cs:147-152`): condición invertida
  (`if (_passenger != null) return;`) → nunca envía el desplazamiento cero. Hoy es inocuo porque el modelo
  limpia el valor tras usarlo, pero la intención está rota.
- Llegada a parada (`MovingState.Tick`, `MovingState.cs:19-27`): al entrar en el umbral de `InTarget`
  (mín. 0.15 u, dinámico por velocidad, `PlatformMotor.cs:14-18`) hace `StopImmediate()` y `return` sin
  `MoveStep` ni snap al punto → la plataforma no queda exacta en la parada y el último tramo no se
  transmite al jugador; posible micro-salto al frenar. El error de posición además se arrastra al
  siguiente tramo.
- Plataformas verticales: `SetVerticalPlatform(true)` anula la gravedad (`PlayerModel.cs:148`), así que el
  jugador depende solo de `_platformDisplacement` → más sensible a las causas 1 y 2.
- Fuera de alcance directo: `Assets/Scripts/Gameplay/AttachPlayer.cs` (usado en
  `Assets/Prefabs/Ascensor/Asset_Ascensor.prefab` y la escena `3 - HUB`, **no** en plataformas con
  `PlatformController`) hace lo mismo en `LateUpdate`, no tiene `OnTriggerExit` (nunca suelta al jugador) e
  ignora desplazamientos < 1 cm/frame. Puede dar síntomas parecidos en el ascensor.

## Fix propuesto

Design: no pattern — es una corrección de sincronización/orden dentro de componentes existentes.

1. **Física de la plataforma:** agregar `Rigidbody` kinematic (interpolate) a los prefabs de plataforma y
   mover con `Rigidbody.MovePosition` en vez de `transform.position +=` (`PlatformMotor` recibiría el
   `Rigidbody`). Alternativa mínima: `Physics.SyncTransforms()` justo después de mover la plataforma (o
   activar `autoSyncTransforms`, más caro globalmente). Elegir y justificar en el plan.
   - Cuidado: si se pasa a `FixedUpdate`, el jugador (CC en `Update`) vuelve a desincronizarse; mantener
     ambos en el mismo loop o acumular el desplazamiento en el pasajero.
2. **Orden:** `[DefaultExecutionOrder(-50)]` (o similar, con constante nombrada) en `PlatformController`,
   para que la plataforma se mueva antes que el jugador en el mismo frame.
3. **Acumular, no sobrescribir:** `PlayerModel.OnPlatformMoving` debería hacer `+=` para no perder
   desplazamiento si algo lo llama más de una vez por frame.
4. Invertir la condición de `StopPassenger`; en la llegada a parada, hacer snap exacto al punto y
   transmitir ese último delta al pasajero.
5. (Opcional, aparte) `AttachPlayer`: agregar `OnTriggerExit` y quitar el umbral.

## Verificación esperada
- Antes de cambiar: reproducir en Play Mode con una plataforma horizontal y una vertical; mirar en el
  Profiler el tiempo de `Physics` y si el tirón coincide con los frames que tienen paso de física.
- Después: sin tirones en aceleración, crucero, frenado y paradas; subir/bajar en plataforma vertical sin
  que `isGrounded` titile; respawn/muerte sobre la plataforma (`CleanPlayerReferences`) sigue funcionando.
- Revisar que los prefabs que usan `PlatformController` sigan bien (grep del GUID del script):
  `Plataforma.prefab` (Art-2), `Glitcheable Platform Container.prefab`, `Movable Platform.prefab`,
  `Plataformas01.prefab`, `Plataforma 1.prefab`, `PuzzlePlataforma01.prefab`.
- Correr `/run-tests` y reportar resultado (hoy no hay tests específicos de plataformas).

## Notas de trabajo
- El repo está en `main`: crear una rama antes de cambiar nada. No commitear sin pedido.
- No editar prefabs a mano salvo cambios triviales verificados; preferir el Editor.
