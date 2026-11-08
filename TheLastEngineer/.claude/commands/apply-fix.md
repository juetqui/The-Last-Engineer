---
description: "Apply fixes from an /audit report, one finding at a time, confirming each change first."
argument-hint: "<the audit report, or the finding(s) to fix — blank to work through the last report>"
disable-model-invocation: true
---

# /apply-fix

Fix what `/audit` found. The only command that changes code as a result of an audit.

## Input
`$ARGUMENTS` is a full audit report or specific findings. Blank → the most recent report in this
conversation; ask which findings to take. Fix only findings in the report or named by the user —
anything else you notice goes in the summary as a note.

## Per finding
1. **Restate** it in one line: what, where, why it matters.
2. **Read** the file and enough around it to fix without breaking callers, scenes or prefabs.
3. **Preview** the change as a diff and wait for confirmation.
4. **Apply** it, then say briefly why that resolves the finding.
5. **Next** finding.

One at a time; batch only when the user asks, and still preview every change.

## Constraints
- Follow `.claude/rules/`, `.claude/practices/practices.md` and the skill for the area you touch
  (`unity-gameplay-development`, `unity-editor-tooling`, `unity-ui-development`, …).
- Smallest change that resolves the finding; no refactoring around it.
- Serialized fields: a rename keeps `[FormerlySerializedAs]`; a moved class moves its `.meta`.
- Low-confidence findings: verify first, say what you confirmed, get explicit confirmation.
- Don't touch tests unless asked; if a fix changes tested behavior, flag it.
- Never commit.

## After
- Each finding: fixed / skipped / needs a decision, with files touched.
- Run `/run-tests` and report it; list any Play Mode verification still required.
