---
name: test-driven-development
description: "Only when the user explicitly asks to work test-first (TDD) in a Unity project. Not the default: /new-feature builds first, then adds tests."
---

# Test-driven development (Unity / NUnit)

The default flow is `/new-feature`: build, then add the tests `.claude/rules/rules.md` requires.
Use this skill only when the user asks for test-first, or to pin a bug with a failing test before
fixing it. Inside a TDD cycle, the steps are not optional.

## The iron law
```
NO PRODUCTION CODE WITHOUT A FAILING TEST FIRST
```
Wrote code before the test? Delete it and start from the test.

## Red → Green → Refactor

### RED — one failing test
One behavior, a clear name, real code (no mocks unless unavoidable — read
[testing-anti-patterns.md](testing-anti-patterns.md) first).

```csharp
[Test]
public void Cooldown_UsedBeforeItExpires_IsNotReady()
{
    var cooldown = new Cooldown(duration: 2f);
    cooldown.Use(now: 10f);

    Assert.That(cooldown.IsReady(now: 11f), Is.False);
}
```
Bad: `public void CooldownWorks()` that asserts a mock's `Received()` instead of a result.

Keep the logic in plain C# classes (time passed in, not read from `Time.time`) so the cycle runs
in **EditMode** — PlayMode cycles are too slow for TDD.

### Verify RED — watch it fail
`/run-tests editmode <TestName>`. It must **fail** (not error), with the expected message, because
the behavior is missing — not because of a typo or a compile error. Passes already? You're testing
existing behavior: fix the test.

### GREEN — the minimal code
The simplest code that makes this test pass. No extra features, no "while I'm here".

### Verify GREEN
Run the test and the whole EditMode suite. All green, or fix the code (not the test).

### REFACTOR
Remove duplication, improve names, extract — staying green. Then the next behavior: back to RED.

## Bugs
Reproduce the bug as a failing test first; then fix; the test stays as regression coverage.

## Red flags — stop and restart the cycle
Code before test · test passed on first run · "I'll test it after" · keeping pre-written code as
reference · mocking what you're testing · editing the test to make it pass.
