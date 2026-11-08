---
description: "Close out a finished task — record what this session learned with trail and refresh only the context it changed. Run manually once the task is considered done."
argument-hint: "[base branch — defaults to the repo's default branch]"
disable-model-invocation: true
---

# /update-unity — close out a task

Base: `$ARGUMENTS`, or the repo's default branch.

## 1. Get the task's diff
`git diff <base>...HEAD` plus uncommitted changes (`git diff`, `git status` for new files). This
diff is the only scope of everything below.

## 2. Check against existing learnings
`trail check --base <base>`. It only sees tracked files: list the new untracked files of the task
and say they weren't checked. Report every learning the diff conflicts with. **If there's a
conflict, stop and ask** whether the change or the learning is right before going on.

## 3. Propose learnings
What this session worked out that the code doesn't show:
- the cause of a bug after attempts that failed (and what was ruled out);
- an approach that was dropped, and why;
- a design decision and its reason;
- a user correction about how something is done here.

Not a learning: what the diff already says, status updates, or anything generic.
Show the exact text of each one (title + body + files it touches) and **save only the ones the user
approves**, with `trail note --title "<few words>" -m "<learning>" --touches <files>`. If nothing
qualifies, say so and save none.

## 4. Refresh the code map
`trail build`.

## 5. Update `doc/context/` — only where the diff affects it
| The diff… | Update |
|---|---|
| adds/moves key scenes, prefabs, asmdefs, folders | `project-map.md` |
| changes system boundaries or adds a system | `architecture-runtime.md` or `architecture-editor-and-pipeline.md` |
| makes a structural technical decision | `decisions.md` — ADR: date, status, decision, context, alternatives, consequences (newest first) |
| introduces a domain term | `game-design-and-glossary.md` |

Show the diff of each context file **before writing it**. Small decisions stay as trail learnings,
not ADRs. No relevant change → leave `doc/context/` untouched and say so.

`doc/context/` lives outside `.claude/` on purpose: Claude Code protects `.claude/` and would ask
for approval on every write. The diff you show is the review.

## 6. Report — never commit
Learnings saved · context files touched · what's ready for the commit (remind that
`.trail/learnings/` goes in the same commit as the change).
