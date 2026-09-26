---
name: init-project
description: Genera (o regenera desde cero) el mapa del proyecto Unity "The Last Engineer" en `.claude/PROJECT_MAP.md` — estructura de carpetas, escenas, sistemas y flujos, interfaces, convenciones, packages, herramientas de editor e índice de API pública por clase — y crea el `CLAUDE.md` corto que apunta a ese mapa. Usar cuando el usuario pide inicializar, documentar, mapear o indexar el proyecto, cuando no existe `.claude/PROJECT_MAP.md`, o cuando `/update-project` recomienda regenerar. Para cambios incrementales usar `/update-project`.
allowed-tools: Read, Glob, Grep, Write, Edit, PowerShell, Bash, AskUserQuestion
---

# init-project — Generar el mapa del proyecto

Produce **un solo archivo** `.claude/PROJECT_MAP.md` en la raíz del repo, pensado para que el modelo
lo lea o grepee antes de buscar archivos, y un `CLAUDE.md` corto que lo referencia. El mapa es la
fuente de verdad que después mantiene `/update-project` de forma incremental.

**Fuera de alcance:** modificar cualquier cosa del proyecto Unity. La skill solo lee `Assets/`,
`Packages/` y `ProjectSettings/`, y solo escribe `CLAUDE.md` y `.claude/PROJECT_MAP.md`.

## Reglas duras (no negociables)

1. **Solo lectura del proyecto.** Prohibido escribir, mover o borrar nada dentro de `Assets/`,
   `Packages/`, `ProjectSettings/` o cualquier carpeta que no sea `.claude/` y el `CLAUDE.md` de la
   raíz del repo.
2. **Nada se documenta sin haberlo leído.** Cada fila del índice de clases (§11) y cada descripción
   de flujo (§5) sale de un `Read` o `Grep` real sobre el `.cs`. Nunca inferir comportamiento por el
   nombre del archivo. Si algo no se pudo clasificar, va a la lista "sin clasificar" del reporte.
3. **Si `PROJECT_MAP.md` ya existe**, frenar y preguntar con `AskUserQuestion`: *regenerar completo*
   (se pierden ajustes manuales fuera de "Notas del equipo") o *abortar y usar `/update-project`*.
   Si se regenera, **la sección `## 12. Notas del equipo` se copia tal cual** al archivo nuevo.
4. **No hacer commits ni ningún `git` de escritura.** Solo `git rev-parse`, `git log`, `git status`.
5. **Ignorar** (se listan en §2 como "carpetas a ignorar", pero no se indexan): `Library/`, `Temp/`,
   `obj/`, `Logs/`, `UserSettings/`, `.idea/`, `.vscode/`, todo `*.meta`, `Assets/Basura/`,
   `Assets/Scenes/Posibles referencias (EX OLD)/`, `Assets/TextMesh Pro/`, `Assets/Samples/`,
   `Assets/Plugins/*/internal`.
6. **Los headings de la plantilla son un contrato.** `/update-project` los busca por texto exacto.
   No renombrarlos, no reordenarlos, no omitir ninguno (si una sección queda vacía, dejar el heading
   con `_(vacío)_`).
7. **Idioma:** español, mismo registro que el código y el README existentes. Nombres de clases,
   carpetas y archivos siempre tal cual están en disco (con espacios y mayúsculas).

## Permisos

Esta skill **se auto-autoriza** para el flujo de abajo: todos los comandos son de lectura
(`Get-ChildItem`, `Get-Content`, `git rev-parse/log/status`, `Grep`, `Read`) más la escritura de los
dos archivos de salida. No pedir confirmación por cada comando ni proponer alternativas "por las
dudas". `AskUserQuestion` se usa **solo** en la regla 3.

## Argumentos

`/init-project [--force]`

- `--force`: si el mapa ya existe, regenerar sin preguntar (sigue preservando "Notas del equipo").

---

## Paso 0 — Raíz y layout (1 turno)

1. `git rev-parse --show-toplevel` → **raíz del repo**. Ahí viven `.claude/` y `CLAUDE.md`.
2. **Ruta del proyecto Unity** = la carpeta que contiene `ProjectSettings/ProjectVersion.txt`.
   Resolverla buscando en disco, **no hardcodear** `TheLastEngineer/`: el layout puede cambiar.
3. Si `.claude/PROJECT_MAP.md` existe, aplicar regla 3 (salvo `--force`). Si se regenera, leer y
   guardar el bloque `## 12. Notas del equipo` antes de seguir.

Las tres cosas van en **una sola** llamada de Bash (no PowerShell: el sandbox de Claude Code
bloquea cualquier `-replace '\\'` como si fuera `Remove-Item`):

```bash
root=$(git rev-parse --show-toplevel); cd "$root"
pv=$(find . -name ProjectVersion.txt -path '*/ProjectSettings/*' -not -path '*/Library/*' -not -path '*/Temp/*' | head -1)
rel=$(dirname "$(dirname "$pv")"); rel=${rel#./}
echo "root=$root"; echo "unity_rel=$rel"
echo "map_exists=$([ -f .claude/PROJECT_MAP.md ] && echo true || echo false)"
echo "claude_md_exists=$([ -f CLAUDE.md ] && echo true || echo false)"
```

## Paso 1 — Recolección (3 turnos, llamadas agrupadas)

Dentro de cada turno, **todas las llamadas independientes van en un solo mensaje**.

### Turno A — Inventario (2 PowerShell en paralelo)

Llamada A1, metadatos + packages + escenas + assets:

```powershell
Set-Location "<unity>"
"--- unity ---"; Get-Content ProjectSettings\ProjectVersion.txt | Select-Object -First 1
"--- commit ---"; git rev-parse HEAD; git rev-parse --short HEAD; git log -1 --format=%cs
"--- packages ---"
(Get-Content Packages\manifest.json | ConvertFrom-Json).dependencies.PSObject.Properties |
  Where-Object { $_.Name -notlike 'com.unity.modules.*' } | ForEach-Object { "$($_.Name): $($_.Value)" }
"--- assets nivel 1 ---"
Get-ChildItem Assets -Directory | Select-Object -ExpandProperty Name
"--- assets nivel 2 ---"
Get-ChildItem Assets -Directory | ForEach-Object { $p = $_.Name
  Get-ChildItem $_.FullName -Directory -ErrorAction SilentlyContinue | ForEach-Object { "$p\$($_.Name)" } }
"--- escenas ---"
Get-ChildItem Assets\Scenes -Recurse -Filter *.unity | ForEach-Object { $_.FullName.Replace("$PWD\Assets\Scenes\", '') }
"--- build scenes ---"
Select-String -Path ProjectSettings\EditorBuildSettings.asset -Pattern 'path:|enabled:' | ForEach-Object { $_.Line.Trim() }
"--- resources ---"
Get-ChildItem Assets\Resources -Recurse -File -ErrorAction SilentlyContinue | Where-Object { $_.Extension -ne '.meta' } | ForEach-Object { $_.FullName.Replace("$PWD\Assets\", '') }
"--- scriptable objects (.asset) ---"
Get-ChildItem Assets -Recurse -Filter *.asset -ErrorAction SilentlyContinue | Where-Object { $_.FullName -notmatch '\\(TextMesh Pro|Samples|Settings|RenderPipeline)\\' } | ForEach-Object { $_.FullName.Replace("$PWD\Assets\", '') }
```

Llamada A2, árbol de scripts + editor + tools + conteo:

```powershell
Set-Location "<unity>"
"--- scripts dirs ---"
Get-ChildItem Assets\Scripts -Recurse -Directory | ForEach-Object { $_.FullName.Replace("$PWD\Assets\Scripts\", '') }
"--- scripts files ---"
Get-ChildItem Assets\Scripts -Recurse -Filter *.cs | ForEach-Object { $_.FullName.Replace("$PWD\Assets\Scripts\", '') }
"--- editor/tools ---"
Get-ChildItem Assets\Editor, Assets\Tools -Recurse -File -ErrorAction SilentlyContinue |
  Where-Object { $_.Extension -ne '.meta' } | ForEach-Object { $_.FullName.Replace("$PWD\Assets\", '') }
"--- otros .cs fuera de Scripts/Editor/Tools ---"
Get-ChildItem Assets -Recurse -Filter *.cs | Where-Object { $_.FullName -notmatch '\\Assets\\(Scripts|Editor|Tools|TextMesh Pro|Samples|Plugins)\\' } | ForEach-Object { $_.FullName.Replace("$PWD\Assets\", '') }
"--- count ---"
"scripts_en_Scripts=$((Get-ChildItem Assets\Scripts -Recurse -Filter *.cs | Measure-Object).Count)"
"scripts_total=$((Get-ChildItem Assets -Recurse -Filter *.cs | Where-Object { $_.FullName -notmatch '\\Assets\\(TextMesh Pro|Samples|Plugins|Basura)\\' } | Measure-Object).Count)"
"escenas=$((Get-ChildItem Assets\Scenes -Recurse -Filter *.unity | Measure-Object).Count)"
```

`scripts_total` es el número que va al meta header (`scripts=`): **todos** los `.cs` de `Assets/`
menos las carpetas ignoradas. Los que viven fuera de `Scripts/` (`Editor/`, `Tools/`,
`Inspection System/`, `Art*/`, `Scenes/**/Scripts/`) también se indexan en §11 y en §4.

### Turno B — Firmas (1 Bash o 5 Grep en paralelo)

**Opción rápida (verificada 2026-09-21, 1 turno):** un solo `rg` sobre `<unity>/Assets` que junta
las cinco extracciones en un archivo del scratchpad, ordenado por archivo y línea, y después
`Read` de ese archivo (~1400 líneas para 190 scripts; leerlo en 3 páginas):

```bash
cd "<unity>/Assets" && rg -n --no-heading -g '*.cs' -g '!TextMesh Pro/**' -g '!Samples/**' -g '!Plugins/**' -g '!Basura/**' \
 -e '^\s*(\[[^\]]*\]\s*)*(public|internal|private|protected|abstract|sealed|static|partial|\s)*(class|interface|enum|struct)\s+\w+' \
 -e '\[SerializeField\]' \
 -e '^\s*public\s+(static\s+|override\s+|virtual\s+|abstract\s+|readonly\s+|event\s+)*[\w<>\[\],\. ]+\s+\w+\s*(\(|\{|=>|;|=)' \
 -e '(event\s+|UnityEvent|Action<|Action\s+\w+\s*[;=]|Func<|delegate\s+)' \
 -e 'static\s+\w+\s+Instance' \
 . | sed -E 's#^\./##' | sort -t: -k1,1 -k2,2n > "<scratchpad>/sigs.txt"
```

Excluir `PlayerInputs.cs` del resultado (generado, cientos de líneas de ruido). Después,
`rg --files` con los mismos `-g` y `comm -3` contra los archivos que aparecen en `sigs.txt` para
detectar scripts sin ninguna firma (van al turno C).

**Opción alternativa:** 5 Grep sobre `<unity>/Assets/Scripts` con `glob: *.cs`,
`output_mode: content`, `-n: false`, `head_limit: 0`:

| # | Patrón | Para qué |
|---|---|---|
| B1 | `^\s*(public\|internal\|abstract\|sealed\|static\|partial\|\s)*(class\|interface\|enum\|struct)\s+\w+` | Declaraciones + herencia/interfaces (§6, §11) |
| B2 | `\[SerializeField\]` | SerializeFields por clase (§11) |
| B3 | `^\s*public\s+(static\s+\|override\s+\|virtual\s+\|abstract\s+\|readonly\s+)*[\w<>\[\], ]+\s+\w+\s*(\(\|\{\|=>\|;)` | Métodos, propiedades y campos públicos (§11) |
| B4 | `(event\s+\|Action<\|Action\s+\|UnityEvent\|Func<\|delegate\s+)` | Eventos/callbacks (§5, §11) |
| B5 | `(GetComponent(InChildren\|InParent)?<\|FindObjectOfType\|FindFirstObjectByType\|FindAnyObjectByType\|Instantiate\(\|SceneManager\.\|DontDestroyOnLoad\|static\s+\w+\s+Instance)` | Dependencias entre sistemas y singletons (§5, §10) |

Con B1 armar la lista de tipos y agrupar B2–B4 por archivo. Cada archivo del turno A2 tiene que
aparecer en B1; si alguno no aparece (archivo vacío, solo `#if`, parcial), anotarlo para leerlo en el
turno C.

### Turno C — Lectura de hubs (Read en paralelo, todos en un mensaje)

Leer completos los archivos "hub" de cada sistema; son los que definen el flujo y las FSM:

| Sistema | Archivos hub (relativos a `Assets/Scripts/`) |
|---|---|
| Player | `Player/MVC/PlayerController.cs`, `Player/MVC/PlayerModel.cs`, `Player/StateMachine/PlayerStateMachine.cs`, `Player/InputManager.cs`, `Player/Components/InputHandler.cs`, `Player/Components/InteractableHandler.cs`, `Player/Components/PlayerNodeHandler.cs` |
| Glitch | `Gameplay/Glitch/Glitcheable.cs`, `Gameplay/Glitch/State Machine/GlitchStateMachine.cs`, `Gameplay/Glitch/GlitcheableDetector.cs`, `Gameplay/Glitch/GlitchAttractionController.cs` |
| Platform | `Gameplay/Platform/PlatformController.cs`, `Gameplay/Platform/StateMachine/PlatformStateMachine.cs`, `Gameplay/Platform/RouteManager.cs`, `Gameplay/Platform/PlatformMotor.cs` |
| Laser | `Gameplay/Laser/MVC/LaserController.cs`, `Gameplay/Laser/MVC/LaserModel.cs`, `Gameplay/Laser/LaserActivator.cs`, `Gameplay/Laser/LaserReceptor.cs`, `Gameplay/Laser/CristalController.cs` |
| Nodes / Connections | `Gameplay/Nodes/NodeController.cs`, `Gameplay/Nodes/NodeModel.cs`, `Gameplay/Nodes/Enums.cs`, `Gameplay/Connection/Connection.cs` |
| Doors / Plates | `Gameplay/Doors/DoorsController.cs`, `Gameplay/Pressure Plate/PlatesActivator.cs`, `Gameplay/Pressure Plate/PressurePlate.cs` |
| UI | `UI/Screen Manager/ScreenManager.cs`, `UI/PauseGameController.cs`, `UI/UIButtonsManager.cs` |
| Cinemáticas | `Cinematics/CinematicManager.cs`, `Cinematics/CinematicTrigger.cs` |
| Escenas / carga | `SceneLoader.cs`, `LevelLoader.cs`, `SceneTransition.cs`, `Player/CheckPointController.cs`, `RespawnPosition.cs` |
| Inputs | `Core/Insfrastructure/Services/Inputs/PlayerInputs.cs` — **no leerlo entero** (generado). Los nombres de acciones salen con `rg 'm_(Player\|UI\|PauseUI)_\w+ = m_' PlayerInputs.cs` |
| Interacción | `Player/Components/PlayerInteractionDetector.cs`, `Player/Components/InteractableHandler.cs` (tienen los comentarios que explican el diseño) |
| Inspección | `Assets/Inspection System/Inspection/InspectionPlayerManager.cs`, `InspectionSystem.cs`, `Inspectionable.cs`, `InspectorController.cs`, `Corruption/CorruptionRemover.cs` |
| Editor | `rg -n 'MenuItem\|CustomEditor\|CreateAssetMenu\|InitializeOnLoad' Assets/Editor Assets/Tools Assets/Scripts` alcanza para §9 |

Si esa lista quedó vieja (un archivo no existe, o hay hubs nuevos evidentes en B1 como un
`*StateMachine.cs` o `*Manager.cs` no listado), ajustarla en el momento y **anotarlo en el reporte**
para que se actualice esta tabla.

Los demás scripts (~100) **no se leen enteros**: con B1–B4 alcanza para la fila de §11. Solo se
lee un script no-hub cuando el grep no deja claro su rol (p. ej. clase sin miembros públicos ni
SerializeFields) o cuando hace falta para cerrar un flujo de §5.

## Paso 2 — Escribir `.claude/PROJECT_MAP.md`

Un solo `Write` con la plantilla de abajo. Los `##` numerados son **exactos**.

```markdown
# Mapa del proyecto — The Last Engineer
<!-- meta: commit=<sha completo> fecha=<yyyy-mm-dd> unity=<versión> scripts=<n> escenas=<n> -->

> Generado por `/init-project`. Mantener con `/update-project` después de cambios o `git pull`.
> Para encontrar algo: §10 (recetas) → §11 (grepear el nombre de la clase) → abrir el archivo.

## 1. Qué es y cómo abrirlo
## 2. Estructura del repo y Assets
## 3. Escenas
## 4. Mapa de Assets/Scripts
## 5. Sistemas y flujos
## 6. Interfaces y contratos
## 7. Convenciones
## 8. Packages y plugins
## 9. Herramientas de editor
## 10. Cómo encontrar cosas
## 11. Índice de API pública
## 12. Notas del equipo
## 13. Historial de actualizaciones
```

Qué va en cada sección:

- **§1** — Un párrafo: género/mecánica central (puzzle 3D con "glitch", nodos, láseres,
  plataformas, corrupción), ruta del proyecto Unity relativa a la raíz del repo, versión de Unity,
  render pipeline, cómo abrirlo (Unity Hub → carpeta `<unity>`), branch principal.
- **§2** — Tabla `Carpeta | Qué contiene | Cuándo mirarla` para la raíz del repo y para cada carpeta
  de `Assets/` (nivel 1, y nivel 2 cuando aporta: `Prefabs/*`, `Scenes/*`). Cierra con la lista
  **"Ignorar"** (regla 5) con una línea de por qué.
- **§3** — Tres listas: *Build scenes* en el orden de `EditorBuildSettings.asset` (índice → archivo →
  para qué sirve), *Test levels* agrupadas por persona (`TEST LEVELS/<nombre>/`), *Viejas / referencia*.
- **§4** — Árbol de `Assets/Scripts` (carpetas) con una línea de propósito por carpeta, y una tabla
  para los scripts sueltos en la raíz de `Scripts/` (`Archivo | Rol | Carpeta donde encajaría`).
  Incluir también `Assets/Editor` y `Assets/Tools` como ramas del árbol (referenciando §9).
- **§5** — Un `###` por sistema (misma lista que el turno C). Por sistema:
  *Componentes* (clase → responsabilidad, en orden Controller → Model → View → estados),
  *Quién crea/orquesta a quién* (2–5 líneas), *Eventos que emite/escucha* (tabla), y para cada FSM
  la tabla `Estado | Entra cuando | Sale hacia`. Terminar con "Cómo se conecta con otros sistemas".
- **§6** — Tabla `Interfaz | Método(s) | La implementan | La consumen`. Cubrir todas las de
  `Interfaces/`, `Core/Domain/Puzzles/` y las `IState` / `IPlayerState` / `IPlatformState` /
  `IGlitchInterruptible`.
- **§7** — Bullets de convenciones **observadas en el código actual** (no las del README si ya no se
  cumplen): patrón MVC (Controller es MonoBehaviour, Model/View son clases planas), FSM con
  interfaz de estado + máquina que cachea instancias, `_camelCase` para privados, `[SerializeField]`
  privado en vez de público, eventos `Action` con suscripción simétrica, enums compartidos en
  `Gameplay/Nodes/Enums.cs`, carpetas "MVC"/"StateMachine" por sistema, español en nombres de
  carpetas y comentarios, etc. Cerrar con una nota: *el `README.md` de la raíz describe una
  arquitectura anterior (TaskManagers, PlayerTDController, GenericConnectionController) y no refleja
  el código actual; las reglas de su §6 siguen siendo válidas como intención*.
- **§8** — Tabla `Package | Versión | Para qué se usa | Dónde (scripts/carpetas)`. Solo los no-módulo
  del manifest más `Assets/Plugins/*`.
- **§9** — Un `###` por herramienta de `Assets/Editor` y `Assets/Tools`: qué hace, cómo se abre
  (menú, atributo), archivos.
- **§10** — Lista de recetas `**Para …** → mirar …` (mínimo 15), del tipo: agregar un estado al
  Player, un nuevo tipo de nodo, un interactuable, un láser/receptor, una escena al build, un
  prompt de input, una cinemática, un sonido, una plataforma con ruta, una pantalla de UI, un
  prefab pintable, etc.
- **§11** — Un `###` por carpeta de `Scripts/` (en el mismo orden del árbol de §4, raíz primero;
  se pueden agrupar carpetas chicas en un mismo `###`), y al final `Assets/Editor · Assets/Tools`,
  `Assets/Inspection System` y `Assets/Art · Assets/Art-2 · Scenes/**/Scripts`. Dentro, tabla:
  `Clase | Archivo | Hereda / Implementa | SerializeFields | Públicos (métodos · props · eventos) | Notas`.
  Una fila por tipo (clase, interfaz, enum, struct); si un archivo declara varios tipos, una fila
  por tipo con el mismo archivo. Firmas abreviadas: `Metodo(tipo a, tipo b)`, `Prop {get;}`,
  `event OnX`. Enums: listar los valores en "Públicos". Interfaces: métodos en "Públicos".
  "Notas": una frase de rol, o `⚠️` si es código muerto/test (`DarkRoomTest`, `PreassurePlate` vs
  `PressurePlate`, duplicados, typos en nombres de carpeta como `Insfrastructure` o `Interctables`
  — **documentarlos tal cual, no corregirlos**).
- **§12** — Si venía de un mapa anterior, pegar el bloque tal cual. Si no, dejar:
  `_(Sección manual. Todo lo que se escriba acá lo respetan /init-project y /update-project.)_`
- **§13** — Tabla `Fecha | Commit | Resumen`. Primera fila:
  `<fecha> | <sha corto> | Generado por /init-project (<n> scripts, <n> escenas)`.

## Paso 3 — `CLAUDE.md` (raíz del repo)

- **Si no existe:** crearlo con `Write`, ~30 líneas, con exactamente estas partes:
  1. Una línea de qué es el proyecto, la ruta del proyecto Unity y la versión.
  2. Bloque **"Antes de buscar archivos"**: leer/grepear `.claude/PROJECT_MAP.md` — §10 para
     recetas, §11 para ubicar una clase, §5 para entender un sistema. Preferir `Grep` sobre el mapa
     a `Glob` sobre `Assets/`.
  3. 5–6 convenciones clave (resumen de §7).
  4. Carpetas a ignorar (resumen de regla 5).
  5. **Skills**: `/init-project` (regenerar desde cero) y `/update-project` (incremental).
     Regla: *después de un `git pull`, merge, checkout de branch o de terminar cambios en
     `Assets/Scripts`, correr `/update-project`*.
  6. Nota: el `README.md` de la raíz está desactualizado; el mapa manda.
- **Si existe:** verificar con Grep que mencione `PROJECT_MAP.md`; si no, agregar solo los bloques
  2 y 5 con `Edit` al final. No tocar el resto.

## Paso 4 — Reporte

```
Mapa generado: <raíz>/.claude/PROJECT_MAP.md (<n> líneas) · CLAUDE.md <creado|actualizado|sin cambios>
Base: commit <sha corto> (<fecha>) · Unity <versión>

| Sección | Ítems |
|---|---|
| §2 Estructura | <n> carpetas |
| §3 Escenas | <n> build · <n> test · <n> viejas |
| §5 Sistemas | <n> sistemas, <n> FSM |
| §6 Interfaces | <n> |
| §8 Packages | <n> |
| §9 Herramientas | <n> |
| §10 Recetas | <n> |
| §11 Clases | <n> tipos en <n> archivos (esperados: <n> .cs) |

Sin clasificar: <lista o "ninguno">
Hubs no encontrados / nuevos hubs sugeridos: <lista o "ninguno">
Próximo paso: correr /update-project después de cambios o git pull.
```

Si `§11` no cubre todos los `.cs` contados en A2, **no es un éxito**: listar los archivos que
faltan y explicar por qué (generado, vacío, solo `#if UNITY_EDITOR`, etc.).

## Paso 5 — Mensaje de error (cuando no se puede generar)

```
No pude generar el mapa.
Motivo: <qué falló: no hay repo git / no se encontró ProjectVersion.txt / el usuario abortó por regla 3 / …>
Estado: no se escribió ningún archivo.   ← o: se escribió PROJECT_MAP.md parcial, revisar §<n>
```
