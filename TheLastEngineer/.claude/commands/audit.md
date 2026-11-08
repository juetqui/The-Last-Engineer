---
description: "Read-only code audit — report bugs, security, performance, serialization and design issues without changing anything."
argument-hint: "[mode — blank for full project, or \"changes\", \"branch <name>\", a directory, files, \"security\", \"performance\", \"architecture\"]"
disable-model-invocation: true
---

# /audit

Audit the code selected by `$ARGUMENTS` and report. **Read-only: never modify, patch or fix
anything.** Fixes are applied separately with `/apply-fix`.

## Modes
| `$ARGUMENTS` | Scope |
|---|---|
| _(blank)_ | Whole project (`Assets/`, `Packages/` local packages) |
| `changes` | Current working diff (staged + unstaged) |
| `branch <name>` | This branch diffed against `<name>` |
| a directory path | That directory, recursively |
| one or more file paths | Only those files |
| `security` | Whole project, security findings only |
| `performance` | Whole project, per-frame CPU/GC/memory/rendering findings only |
| `architecture` | Whole project, SOLID / design-pattern / assembly-boundary findings only |

Git is read-only: `status`, `diff`, `log`, `show`. For `changes` and `branch`, follow the **method**
of the `find-bugs` skill (complete diff, risk surface, checklist, verification, pre-conclusion
audit) but report in this command's format.

## What "correct" means — read, don't restate
- `.claude/rules/` — a violation is a finding. `.claude/practices/practices.md` — at most Low.
- `csharp-code-review` skill — the Unity checklist. `design-pattern-selection` — for proportionality.
- `doc/context/` — what the project is supposed to do, and the architecture to judge against.

## What to look for
Functional bugs · security (secrets in code/SOs/`Resources`/`StreamingAssets`, untrusted save or
network data) · **serialization and broken references** (renames without `FormerlySerializedAs`,
class/file name mismatch, retyped fields) · **missing or orphaned `.meta` files** · **per-frame GC
and CPU cost** · **editor code in runtime** (`UnityEditor` without guard, editor-only APIs) · event
leaks, orphan coroutines, async without cancellation · destroyed-object access · race conditions
in lifecycle order · **SOLID violations and disproportionate patterns** · assembly-graph problems
(cycles, runtime → editor references) · dead code and duplication · risky or unneeded packages.

Finding `.meta` problems: list assets without `.meta` and `.meta` without assets under `Assets/`
(file names only — never read the assets).

**Out of scope: tests.** Missing or weak tests are `/review-pr`'s job.

## Before reporting a finding
- Read the actual code — never report from a file name, a grep hit or a guess.
- Trace real usage: callers (`trail callers`, confirmed with `trail grep`), scenes/prefabs that
  reference the script (grep its GUID).
- Check protection doesn't already exist elsewhere.
- State the real impact: what breaks, for whom, when. Can't say? It isn't a finding.
- Cite evidence (file, lines, code). Recommend a fix — don't apply it.
- Fewer verified findings beat a long list.

## Severity and confidence
- **Severity:** Critical (exploitable, data-destroying, breaks the build) · High (breaks a real flow
  or a rule) · Medium (fails under realistic conditions) · Low · Informational.
- **Confidence:** High (verified) · Medium (one assumption unverified) · Low (suspected — say what you
  couldn't verify, e.g. needs Play Mode).

## Report format
```markdown
# Code Audit Report
## Audit Scope
<mode, what was covered, what wasn't, what you couldn't access>
## Executive Summary
<overall health, the findings that matter, counts by severity>
## Findings
### [Severity] <Title>
- **Confidence:** High | Medium | Low
- **Category:** <security, serialization, performance, architecture, …>
- **File:** `path` · **Line(s):** <lines>
- **Description / Evidence / Impact**
- **Recommended fix:** <not applied>
- **Manual verification steps:** <including Play Mode steps when relevant>
## Positive Observations
## Prioritized Action Plan
## Final Summary Table
| Severity | Finding | File | Confidence | Priority |
|---|---|---|---|---|
```

## Restrictions
Never modify files or run state-changing commands · never install or update packages · never touch
git history · redact secrets (say where, never the value) · label anything unverified as Low confidence.
