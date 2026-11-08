---
description: "Shader rules: render pipeline compatibility, variants, precision, SRP Batcher."
paths:
  - "**/*.{shader,hlsl,cginc,shadergraph}"
---

# Shaders

- Target the project's render pipeline (see Stack in `CLAUDE.md`). Built-in `CGPROGRAM`/surface
  shaders don't work in URP/HDRP; URP/HDRP shaders use `HLSLPROGRAM` and the pipeline's includes.
- Keep variants under control: `shader_feature` (stripped when unused) for material toggles,
  `multi_compile` only for keywords switched at runtime. Prefer the `_local` variants.
  Every new keyword multiplies build time and memory — justify it.
- Use `half` where precision allows (colors, normals, UVs) — it matters on mobile. Keep `float` for
  positions and depth.
- Don't break the SRP Batcher: every material property goes inside
  `CBUFFER_START(UnityPerMaterial)` … `CBUFFER_END`, identical across all passes.
- Avoid dynamic branching and texture samples inside loops in fragment shaders; move work to the
  vertex stage when the result interpolates correctly.
- Shader Graph assets are edited in the Editor, never by hand — read only the fragment you need.
