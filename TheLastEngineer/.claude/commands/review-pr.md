---
description: "Review the current changes (or a given PR/branch) against the project's rules and practices."
argument-hint: "[PR number or branch — defaults to the working diff]"
disable-model-invocation: true
---

# /review-pr

Review the changes in `$ARGUMENTS` (default: the current working diff, or the branch against its base).

## How to run it
Delegate to the `reviewer` agent — it's read-only and already knows the bar. Review the diff yourself
only for a change too small to be worth the handoff, and then follow the same checklist.

The checklist is the `csharp-code-review` skill: correctness (destroyed objects, event pairs,
lifecycle), **SOLID and design proportionality**, serialization and `.meta`, per-frame cost,
editor vs runtime, security, tests, consistency. The bar behind it is `.claude/rules/` (blocking)
and `.claude/practices/practices.md` (nitpick at most).

Two criteria every review states explicitly:
- **SOLID** — any violation, with the principle and the fix.
- **Is the design proportional?** Compare the scope of the change with the patterns and
  abstractions it introduces, and with the plan's `Design:` line if there was one.
  Over-engineering is a finding, just like a violation.

Delegate to `qa-tester` when the change needs exercising rather than reading, to `perf-reviewer`
for hot-path changes, and suggest `/audit branch <base>` for risky or large changes.

## How to report
- Group findings by severity: **blocking**, **should-fix**, **nitpick**.
- Each: `file:line`, the problem, a concrete fix.
- Lead with the most important; skip praise. Nothing wrong? Say so briefly.
