---
name: update-project
description: Actualiza de forma incremental el mapa del proyecto `.claude/PROJECT_MAP.md` a partir de los cambios en el proyecto Unity desde el último commit documentado (git diff + working tree) — scripts nuevos/modificados/borrados, escenas, carpetas, packages, versión de Unity. Usar después de terminar cambios propios, de un `git pull`, merge o checkout de branch, o cuando el usuario pide actualizar/refrescar/sincronizar la documentación del proyecto. Si no existe el mapa, o los cambios son demasiados, deriva a `/init-project`.
allowed-tools: Read, Glob, Grep, Write, Edit, PowerShell, Bash, AskUserQuestion
---

# update-project — Mantener el mapa al día

Toma el commit registrado en el meta header de `.claude/PROJECT_MAP.md`, calcula qué cambió en el
proyecto Unity desde entonces (commits + working tree) y **edita solo las secciones afectadas**.
Es la contraparte incremental de `/init-project`: barata, quirúrgica y sin regenerar nada.

**Fuera de alcance:** cambiar código del proyecto, hacer commits, regenerar el mapa completo.

## Reglas duras (no negociables)

1. **Edición quirúrgica con `Edit`.** Nunca reescribir `PROJECT_MAP.md` entero con `Write`. Cada
   cambio es un `Edit` acotado a la fila, tabla o párrafo afectado.
2. **`## 12. Notas del equipo` no se toca.** Jamás. Ni para "mejorarla".
3. **Cada fila de §11 que se agregue o modifique sale de un `Read` del `.cs` actual.** Nunca
   deducir la firma nueva por el nombre del commit o por el diff sin abrir el archivo.
4. **Sin meta header parseable → abortar** con el mensaje del Paso 5 y pedir `/init-project`.
5. **No intentar incremental** (abortar y recomendar `/init-project`) si:
   - el commit base no existe en el historial (`git cat-file -t <sha>` falla: rebase, force-push,
     clone nuevo), o
   - cambiaron **más de 40 scripts** (`.cs` en A/M/D/R), o
   - más de 3 carpetas de primer nivel de `Assets/Scripts` fueron creadas/borradas/renombradas.
6. **No hacer commits ni ningún `git` de escritura.** Solo `git rev-parse`, `git diff`, `git status`,
   `git log`, `git cat-file`.
7. Los headings `## N. …` son un contrato con `/init-project`: no renombrarlos, no reordenarlos.
8. Nombres de archivos y clases tal cual están en disco, incluidos los typos existentes
   (`Insfrastructure`, `Interctables`, `PreassurePlate`, …). Documentar, no corregir.

## Permisos

Esta skill **se auto-autoriza**: comandos de lectura (`git`, `Get-Content`, `Grep`, `Read`) y `Edit`
sobre `.claude/PROJECT_MAP.md`. No pedir confirmación por comando. `AskUserQuestion` no se usa.

## Argumentos

`/update-project [desde=<sha>] [--full]`

- `desde=<sha>`: usar ese commit como base en lugar del que figura en el meta header (útil si el
  header quedó mal o se quiere re-documentar un rango).
- `--full`: no hacer nada incremental; invocar `/init-project --force` y terminar.

---

## Camino rápido

| Turno | Qué | Llamadas |
|---|---|---|
| 1 | Meta header + raíz/layout + diff + status | 1 Read (solo cabecera) + 1 PowerShell, en paralelo |
| 2 | Leer los `.cs` afectados | N Read en paralelo (todos en un mensaje) |
| 3 | Editar el mapa | N Edit en paralelo cuando no se pisan entre sí |
| 4 | Cierre: meta header + §13 | 2 Edit |

### Turno 1 — Base y diff (en paralelo)

Llamada A: `Read` de `.claude/PROJECT_MAP.md` con `limit: 5` — solo para sacar el meta header:

```
<!-- meta: commit=<sha> fecha=<yyyy-mm-dd> unity=<ver> scripts=<n> escenas=<n> -->
```

Llamada B (**Bash**, verificada 2026-09-21), resuelve raíz, proyecto Unity, HEAD y **ambos** diffs
de una vez. Como el meta header todavía no se leyó en esta misma llamada, el sha base se lee del
archivo acá también. Va en Bash y no en PowerShell a propósito: el sandbox de Claude Code marca
como `Remove-Item` cualquier `-replace '\\'` y bloquea el comando.

```bash
root=$(git rev-parse --show-toplevel); cd "$root"
pv=$(find . -name ProjectVersion.txt -path '*/ProjectSettings/*' -not -path '*/Library/*' -not -path '*/Temp/*' | head -1)
rel=$(dirname "$(dirname "$pv")"); rel=${rel#./}
meta=$(head -5 .claude/PROJECT_MAP.md | grep -oE 'commit=[0-9a-f]+' | cut -d= -f2)
echo "root=$root"; echo "rel=$rel"; echo "base=$meta"; echo "head=$(git rev-parse HEAD)"; echo "head_short=$(git rev-parse --short HEAD)"; echo "today=$(date +%F)"; echo "base_exists=$(git cat-file -t $meta 2>/dev/null)"
echo "--- commits desde base ---"; git log --oneline "$meta..HEAD" -- "$rel/Assets" "$rel/Packages/manifest.json" "$rel/ProjectSettings"
echo "--- diff base..HEAD ---"; git diff --name-status -M "$meta" HEAD -- "$rel/Assets" "$rel/Packages/manifest.json" "$rel/ProjectSettings/ProjectVersion.txt" "$rel/ProjectSettings/EditorBuildSettings.asset"
echo "--- working tree ---"; git status --porcelain -- "$rel/Assets" "$rel/Packages/manifest.json" "$rel/ProjectSettings/ProjectVersion.txt" "$rel/ProjectSettings/EditorBuildSettings.asset"
echo "--- count ---"; echo "scripts_total=$(cd "$rel/Assets" && rg --files -g '*.cs' -g '!TextMesh Pro/**' -g '!Samples/**' -g '!Plugins/**' -g '!Basura/**' . | wc -l)"; echo "escenas=$(find "$rel/Assets/Scenes" -name '*.unity' | wc -l)"
```

Si `desde=` vino como argumento, reemplaza `$meta`. Si `base_exists` no es `commit` → regla 5.
Si hay pocos scripts modificados, agregar al final `git diff -- "<ruta>"` de cada uno para ver
en el mismo turno si el cambio toca firmas públicas o solo cuerpos de métodos.

**Filtrar** la unión de ambos diffs:

- Descartar `*.meta` y todo lo que caiga en carpetas ignoradas (`Assets/Basura/`,
  `Assets/Scenes/Posibles referencias (EX OLD)/`, `Assets/TextMesh Pro/`, `Assets/Samples/`,
  `Assets/Plugins/*/internal`).
- Clasificar lo que queda:

| Grupo | Criterio | Marca |
|---|---|---|
| Scripts | cualquier `*.cs` bajo `Assets/` que no esté en carpeta ignorada (incluye `Editor/`, `Tools/`, `Inspection System/`, `Art*/`, `Scenes/**/Scripts/`) | A / M / D / R (renombre: origen → destino) |
| Escenas | `*.unity` | A / D / R |
| Build settings | `ProjectSettings/EditorBuildSettings.asset` | M |
| Carpetas | directorio nuevo o vacío en `Assets/*` o `Assets/Scripts/**` (se deduce de las rutas A/D) | nueva / borrada |
| Prefabs | `*.prefab` — **solo a nivel de subcarpeta de `Assets/Prefabs`**, no por archivo | carpeta nueva / borrada |
| Packages | `Packages/manifest.json` | M |
| Unity | `ProjectSettings/ProjectVersion.txt` | M |
| Otros assets | `.asset`, `.mat`, `.shader`, `.uxml`, … | se ignoran salvo que estén en `Assets/Resources` o sean ScriptableObjects nuevos |

Los cambios del working tree (sin commit) **cuentan igual** y se marcan como `(wt)` en §13.

**Si la lista filtrada queda vacía** → reporte "sin cambios relevantes" y **no tocar el archivo**
(ni siquiera el meta header). Fin.

**Si HEAD == base y lo único que hay son cambios del working tree que no tocan nada documentable**
(solo cuerpos de métodos, comentarios, valores por defecto) → reporte "sin cambios documentables"
listando esos archivos y **tampoco tocar el archivo**: el meta header ya apunta a HEAD y una fila
de §13 por cada edición sin commit sería ruido. Cuando esos cambios se commiteen, la próxima
corrida los verá en `base..HEAD` y los registrará en §13 de una vez.

Si hay más de 40 scripts o > 3 carpetas de primer nivel de `Scripts/` afectadas → regla 5, fin.

### Turno 2 — Leer lo afectado (Read en paralelo)

- Todos los `.cs` con A, M o destino de R: `Read` completo (son los únicos que hace falta abrir).
- Si alguno es "hub" de un sistema (tabla del turno C de `/init-project`) **o** el diff toca una
  interfaz de `Interfaces/` / `Core/Domain/` / `*State*.cs`, además `Read` de la sección `### <sistema>`
  correspondiente de §5 para compararla con el código (usar `Grep` con `-A 40` sobre el mapa
  para no leerlo entero).
- `Grep` en el mapa de cada clase borrada/renombrada (`\bNombreClase\b`) para ubicar todas las
  referencias a corregir en §5, §6, §10 y §11.
- Si cambió `manifest.json` o `EditorBuildSettings.asset`: leerlos (son cortos).

### Turno 3 — Aplicar cambios (Edit)

| Cambio | Qué editar |
|---|---|
| Script **A** | Nueva fila en §11 dentro del `###` de su carpeta (crear el `###` si la carpeta es nueva, en la posición que le toca por orden de árbol). Si implementa una interfaz → sumarlo en §6. Si es un estado de una FSM o un componente de un sistema de §5 → agregarlo a "Componentes" y, si es estado, a la tabla de la FSM. Si la carpeta es nueva → línea en §4. |
| Script **M** | Reemplazar su fila en §11 (SerializeFields, públicos, notas). Si cambió la lista de interfaces → §6. Si es hub, o cambió un evento/método público que otro sistema consume → ajustar el `###` de §5. Si no cambió nada público ni SerializeFields, **no tocar el mapa** por ese archivo (pero sí cuenta en §13). |
| Script **D** | Borrar su fila en §11. Corregir cada referencia encontrada en el turno 2 (§5, §6, §10). Si la carpeta quedó vacía → quitarla de §4. |
| Script **R** | Mover/renombrar la fila en §11 (y de `###` si cambió de carpeta). Reemplazar el nombre en todas las referencias del mapa. |
| Escena A/D/R | Lista correspondiente de §3 (build / test / viejas). |
| `EditorBuildSettings.asset` | Reordenar/actualizar la lista *Build scenes* de §3. |
| Carpeta nueva/borrada en `Assets/*` | Tabla de §2. En `Scripts/**` → árbol de §4 (+ `###` en §11). |
| Subcarpeta de `Prefabs/` | Fila en la tabla de §2. |
| `manifest.json` | Tabla de §8: agregar/quitar/actualizar versión. |
| `ProjectVersion.txt` | Versión en §1 y en el meta header. |
| ScriptableObject / Resource nuevo | Mención en §2 (Resources) o en el sistema de §5 que lo use. |

Recetas de §10: revisar solo si un cambio deja una receta apuntando a un archivo que ya no existe
o si aparece un sistema/hub nuevo que merece receta. No reescribir las existentes.

Todos los `Edit` que no se pisan (distintas secciones) van en **un solo mensaje**.

### Turno 4 — Cierre (2 Edit)

1. Meta header: `commit=<HEAD completo>`, `fecha=<hoy>`, `scripts=<scripts_total>` y
   `escenas=<escenas>` (ya vienen del turno 1).
   Si hay cambios sin commitear, el header apunta igual a HEAD: la próxima corrida los volverá a
   ver en `git status` y solo confirmará que las filas ya están al día.
2. §13: insertar **arriba** de la tabla una fila
   `<hoy> | <head_short> | +<a> ~<m> -<d> scripts, <otros cambios>; commits: <n> (wt: <n> archivos)`
   y recortar la tabla a **15 filas**.

## Paso 4 — Reporte

```
Mapa actualizado: .claude/PROJECT_MAP.md
Base: <sha corto base> → <sha corto HEAD> (<n> commits) · working tree: <n> archivos

| Sección | Qué cambió |
|---|---|
| §3 Escenas | +1 BUILD SCENES/5 - X.unity |
| §5 Sistemas | Laser: nuevo estado / evento OnY |
| §11 Índice | +2 · ~5 · -1 filas |
| §13 Historial | fila agregada |

Sin cambios documentables (solo cuerpo de métodos): <lista de .cs o "ninguno">
No pude clasificar: <lista o "ninguno">
```

Si se abortó por regla 5, en lugar de la tabla:
`Recomendación: correr /init-project (motivo: <base inexistente | N scripts cambiados | N carpetas>)`.

## Paso 5 — Mensaje de error

```
No pude actualizar el mapa.
Motivo: <no existe .claude/PROJECT_MAP.md | meta header ilegible | commit base <sha> no está en el historial | …>
Estado: el mapa no se modificó.
Siguiente paso: /init-project   (o /update-project desde=<sha> si conocés la base correcta)
```
