---
description: "How we like to work — conventions and workflow preferences (softer than rules/)."
---

# Practices

Guidance, not hard law — deviating is at most a nitpick. Prefer these unless there's a good reason not to.

## Code style
- Write self-documenting code; a comment is a last resort, not a habit.
- Keep methods small and focused; avoid deep nesting — return early.
- Reuse existing components, systems and utilities before adding new ones.
- Match the style of the surrounding file.

## Comments
- English, even when domain terms stay in their original language.
- Only for a "why" the code can't show: a workaround, an engine quirk, a game rule that looks wrong
  without context. Never a restatement of what the line does.
- One line. If it needs a paragraph, rename or split the code instead.
- No commented-out code and no section banners. XML docs (`///`) only on APIs other systems consume.

## Naming (C#)
- `PascalCase` for types, methods, properties, events and constants; `camelCase` for locals and
  parameters; `_camelCase` for private fields, serialized ones included (`[SerializeField] private
  float _speed;`). <!-- team: change here if the project uses another convention. -->
- Interfaces start with `I` (`IDamageable`); events are named after what happened (`Died`,
  `HealthChanged`), handlers `On<Event>`.
- One public type per file; file name = type name (required for MonoBehaviours/ScriptableObjects).
- Namespaces follow the folder/assembly (`<!-- team: Company.Game -->.Combat`).
- Descriptive names; no abbreviations that aren't already common in the codebase.

## Logging
- `Debug.Log*` with context: pass the object (`Debug.LogWarning(msg, this)`) and say what failed and
  with which values.
- No stray debug logs in a commit. Logs meant to stay go through a wrapper marked
  `[Conditional("UNITY_EDITOR")]` (or a project define) so they compile out of builds.
- `LogError` for something broken, `LogWarning` for something suspicious, `Log` for neither.

## Performance
- Measure with the Profiler (on device for mobile) before optimizing; state the numbers before and after.
- Don't optimize code that isn't on a hot path — readability wins there.

## Unity workflow
- Prefer configuring through the Inspector and ScriptableObjects over hardcoding tuning values.
- Prefer prefabs and prefab variants over duplicated scene objects.
- Keep scene changes out of code PRs when possible — scene diffs are hard to review.

## Documentation
- Update `doc/context/` when architecture or a structural decision changes — `/update-unity`
  does this at the end of a task.

## Pull requests
- Small and single-purpose — easier to review, safer to merge.
- Describe what changed, why, and how to verify it (including Play Mode steps).
- Self-review the diff (`/review-pr`) before asking for review.

## Definition of done
- Meets the acceptance criteria.
- Compiles with no new warnings.
- Tests green (`/run-tests`) and reported, or the Play Mode check reported when no test applies.
- No dead code or stray logs.
- `/update-unity` run.
