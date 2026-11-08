---
name: unity-testing
description: "Use when writing or running Unity tests with the Unity Test Framework (EditMode/PlayMode), or making game logic testable."
---

# Unity testing

## Before writing tests
- Find the existing test assemblies: `trail grep TestFixture`, `**/Tests/**/*.asmdef`. Copy the
  shape of the tests next to the code you're covering.
- Rules: `.claude/rules/tests.md`. Before writing any fake or mock, read
  `.claude/skills/test-driven-development/testing-anti-patterns.md`.

## EditMode or PlayMode?
| Test this | Where |
|---|---|
| Plain C# logic (rules, calculations, state machines, presenters) | **EditMode** — fast, no scene |
| Editor tools' logic | EditMode, in an Editor test assembly |
| Lifecycle, physics, coroutines, `Awaitable`, scene loading | **PlayMode** with `[UnityTest]` |

If logic is hard to test because it lives inside a `MonoBehaviour`, extract it into a plain class
(that's `rules/solid.md` S + D, not a test-only change).

## Assembly setup
- EditMode: asmdef under `Tests/EditMode/`, *Editor* platform only, references the code's asmdef,
  `UnityEngine.TestRunner`, `UnityEditor.TestRunner`, and `nunit.framework.dll` (override
  references) — `defineConstraints: ["UNITY_INCLUDE_TESTS"]`.
- PlayMode: asmdef under `Tests/PlayMode/`, all platforms, same references minus the editor ones.

## Writing them
```csharp
public class HealthTests
{
    [Test]
    public void TakeDamage_BelowZero_ClampsToZeroAndRaisesDied()
    {
        var health = new Health(max: 10);
        var died = false;
        health.Died += () => died = true;

        health.TakeDamage(15);

        Assert.That(health.Current, Is.EqualTo(0));
        Assert.That(died, Is.True);
    }
}
```
- PlayMode: `[UnityTest] public IEnumerator …` and `yield return null` / `new WaitForFixedUpdate()`;
  create objects in the test, destroy them in `[UnityTearDown]`.
- Use `LogAssert.Expect` when a log is part of the behavior; an unexpected `LogError` fails the test.
- Deterministic: fixed seeds, no real time — inject a time/random source when logic depends on it.

## Running and reporting
- `/run-tests editmode` (or `playmode` / `all`). It runs Unity in batchmode and fails if the
  Editor has the project open.
- Report the command, total / passed / failed and the name of every failure; compilation errors
  count as red.
