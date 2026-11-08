---
name: brainstorming
description: "Use before building a feature when the intent or requirements are still unclear — shape the idea before planning."
---

# Brainstorming ideas into designs

Turn an idea into a validated design through dialogue, before any code.

## Process

**Understand the idea**
- Look at the current state first: `doc/context/game-design-and-glossary.md`, the relevant
  `doc/` files, `trail ask "<topic>"`, recent commits.
- Ask **one question per message**; prefer multiple choice. Focus on purpose, player experience,
  constraints (platforms, performance budget, existing systems) and success criteria.

**Explore approaches**
- Propose 2–3 approaches with trade-offs, recommended one first, with the reason.
- Apply `design-pattern-selection` to each: what scope it implies and whether a pattern is
  warranted. YAGNI ruthlessly.

**Present the design**
- In sections of 200–300 words, asking after each whether it looks right.
- Cover: player-facing behavior, systems and components, data (ScriptableObjects, save data),
  communication between systems, editor/designer workflow, error cases, testing, performance risk.

## After the design
- Write the validated design to `doc/<topic>-design.md` — `doc/` is the input `/init-unity` folds
  into `doc/context/`. Leave it in the working tree; never commit it yourself.
- Ask: "Ready to start implementing?" → hand off to `/new-feature <description>`.

## Principles
One question at a time · multiple choice when possible · YAGNI · always 2–3 alternatives ·
validate incrementally · go back when something doesn't fit.
