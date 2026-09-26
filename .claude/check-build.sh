#!/usr/bin/env bash
# Compila Assembly-CSharp FUERA de Unity, reutilizando las referencias y los defines que el
# propio Unity genero en Assembly-CSharp.csproj. Sirve para validar un refactor grande sin
# tener que abrir el Editor. Uso: bash .claude/check-build.sh
#
# La lista de archivos sale del csproj (descartando los borrados) mas todo .cs de Assets/Scripts
# que todavia no este listado, asi los archivos nuevos entran sin regenerar el csproj.
set -u
PROJ="/d/UnityProjects/The-Last-Engineer/TheLastEngineer"
OUTDIR="${TMPDIR:-/tmp}/tle-buildcheck"
OUTWIN=$(cd "$(dirname "$0")" && pwd -W 2>/dev/null || echo "")
CSC="C:/Program Files/Unity/Hub/Editor/6000.3.6f1/Editor/Data/DotNetSdkRoslyn/csc.dll"

mkdir -p "$OUTDIR"
RSP="$OUTDIR/asmcsharp.rsp"
DLLWIN="$(cygpath -m "$OUTDIR")/Assembly-CSharp-check.dll"
cd "$PROJ" || exit 1

{
  echo '-nologo'; echo '-target:library'; echo '-langversion:9.0'; echo '-nostdlib+'
  echo "-out:\"$DLLWIN\""
  grep -o '<DefineConstants>[^<]*' Assembly-CSharp.csproj | head -1 | sed 's/<DefineConstants>//' \
    | tr ';' '\n' | while read -r d; do [ -n "$d" ] && echo "-define:$d"; done
  grep -o '<HintPath>[^<]*' Assembly-CSharp.csproj | sed 's/<HintPath>//' | sort -u \
    | while read -r h; do echo "-r:\"$h\""; done
} > "$RSP"

grep -o '<Compile Include="[^"]*"' Assembly-CSharp.csproj | sed 's/<Compile Include="//; s/"$//' \
  | tr "\\" "/" 2>/dev/null | sort -u > "$OUTDIR/from-csproj.txt"
find Assets/Scripts -name '*.cs' | sort -u > "$OUTDIR/on-disk.txt"
{
  while read -r f; do [ -f "$f" ] && echo "$f"; done < "$OUTDIR/from-csproj.txt"
  comm -13 "$OUTDIR/from-csproj.txt" "$OUTDIR/on-disk.txt"
} | sort -u | sed 's|^|"|; s|$|"|' >> "$RSP"

rm -f "$OUTDIR/Assembly-CSharp-check.dll"
dotnet "$CSC" "@$RSP" 2>&1 | grep ": error " | head -40
if [ -f "$OUTDIR/Assembly-CSharp-check.dll" ]; then
  echo "== COMPILA OK ($(grep -c '^"' "$RSP") archivos) =="
else
  echo "== FALLA LA COMPILACION =="; exit 1
fi
