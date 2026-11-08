---
purpose: "Record structural technical decisions (lightweight ADRs) so intentional choices are preserved. Small decisions go to trail learnings instead. Seeded by /init-unity, appended by /update-unity."
expected_content:
  - "One entry per decision, newest first"
  - "Date and status (proposed, accepted, superseded)"
  - "The decision and the context that led to it"
  - "Alternatives considered and why they were rejected"
  - "Consequences and trade-offs"
---

<!-- Seeded by /init-unity, appended by /update-unity. Keep this frontmatter and the existing
     entries; add new ones on top. -->

## No Unity MCP server (template default)
- **Date:** 2026-11-08 · **Status:** accepted (template default — revisit per project)
- **Decision:** Claude works on the project through files, trail and Unity batchmode
  (`/run-tests`, `/build`). No Unity MCP server, so there is no `.mcp.json` and none is enabled in
  `.claude/settings.json`.
- **Context:** the DiPhot Unity template wants low context cost and nothing that sends project code
  to third parties. Batchmode already covers tests and builds without the Editor open.
- **Alternatives:** the official Unity MCP or a community server (e.g. unity-mcp) to read the
  console, run tests with the Editor open and query scene/prefab hierarchies without parsing YAML.
  Not evaluated for this template.
- **Consequences:** Claude can't see the open Editor: tests and builds need it closed, it reads scene
  and prefab YAML by fragment, and Play Mode checks are done by the user. To adopt an MCP later: pin
  its version in `.mcp.json`, list it in `enabledMcpjsonServers`, document it in `.claude/README.md`,
  and supersede this entry.
