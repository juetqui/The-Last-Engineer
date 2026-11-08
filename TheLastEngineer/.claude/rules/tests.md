---
description: "Rules for Unity tests: test asmdefs, EditMode vs PlayMode, determinism."
paths:
  - "**/Tests/**/*.cs"
---

# Tests

- Tests live in dedicated test assemblies (asmdef referencing `UnityEngine.TestRunner` /
  `UnityEditor.TestRunner` with `nunit.framework.dll`), split into EditMode and PlayMode. Never
  in runtime assemblies.
- Pure logic is tested in EditMode, as plain C# — fast and without a scene. Use PlayMode only for
  what needs the engine loop: physics, coroutines, lifecycle, scene loading.
- Tests are deterministic: fixed random seeds, no wall-clock time, no frame-count assumptions, no
  order dependence between tests.
- Don't depend on the game's real scenes when you can avoid it: build the minimal objects in the
  test and destroy them in `TearDown`/`UnityTearDown`.
- One behavior per test, named `Method_Condition_ExpectedResult`. New logic gets a positive and a
  negative case.
- Assert on outcomes, not on the fact that a mock was called.
