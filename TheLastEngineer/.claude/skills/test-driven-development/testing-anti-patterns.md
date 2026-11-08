# Testing anti-patterns (C# / Unity)

Read before writing any mock, fake or test helper. Tests verify real behavior; mocks only isolate.

## The iron laws
1. Never test mock behavior.
2. Never add test-only methods to production classes.
3. Never mock without understanding what the real dependency does.

## 1. Testing mock behavior
```csharp
// ❌ The only assertion is that the mock was called
var audio = Substitute.For<IAudioService>();
new Weapon(audio).Fire();
audio.Received(1).Play(Arg.Any<string>());
```
Passes whatever `Fire()` really does. Assert the outcome instead:
```csharp
// ✅ A tiny hand-written fake that records, and an assertion on the real result
var audio = new RecordingAudio();
var weapon = new Weapon(audio, ammo: 1);
weapon.Fire();
Assert.That(weapon.Ammo, Is.EqualTo(0));
Assert.That(audio.Played, Is.EqualTo(new[] { "shot" }));
```
Gate: *am I testing the code, or that a mock was wired up?* If the latter, delete the assertion.

## 2. Test-only methods in production
```csharp
// ❌ Exists only so tests can reset a singleton
public static void ResetForTests() => _instance = null;
```
It's dangerous in production and a sign the dependency should be injected. Inject it (constructor /
serialized field) so the test builds its own instance; put cleanup helpers in the test assembly.
Gate: *is this only used by tests?* → don't add it. (`InternalsVisibleTo` for the test assembly is
fine when a real internal API needs coverage.)

## 3. Mocking without understanding
Mocking a method whose side effect the test depends on (e.g. a fake inventory that doesn't actually
store items, then asserting a duplicate is rejected) makes the test pass or fail for the wrong
reason. Run with the real implementation first; mock only the slow or external edge (network,
platform services, file I/O, time), never the logic under test.

## 4. Incomplete fakes
A fake save file or server response with only the fields this test reads hides assumptions — the
code breaks on the real data. Mirror the complete real structure (copy a real JSON fixture into the
test assembly).

## 5. Unity-specific
- **Testing Unity instead of your code:** asserting that `transform.position` changed after setting
  it tests the engine. Test your logic in a plain class.
- **PlayMode for everything:** slow and flaky. If it doesn't need the engine loop, it's EditMode.
- **Real scenes as fixtures:** loading the game's scenes couples tests to content. Build the
  minimum objects in the test; destroy them in `TearDown`/`UnityTearDown`.
- **Time and randomness:** `Time.time`, `Random.value` and frame counts make tests flaky. Inject
  a clock/random source; fixed seeds.
- **Leaking objects between tests:** every `new GameObject`/`Instantiate` gets destroyed
  (`Object.DestroyImmediate` in EditMode).

## Red flags
"I'll mock it to be safe" · mock setup longer than the test · asserting `Received()` only ·
a production method nobody but tests calls · a test that passes with the implementation deleted.
