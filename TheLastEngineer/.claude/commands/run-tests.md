---
description: "Run the Unity Test Framework suite in batchmode and report total/passed/failed with the names of anything red."
argument-hint: "[editmode|playmode|all] [optional test filter]"
disable-model-invocation: true
---

# /run-tests

Platform: first word of `$ARGUMENTS` — `editmode` (default), `playmode` or `all` (EditMode, then
PlayMode). The rest, if any, is a test filter (full name, class or regex).

## 1. Find the editor
Use the Unity editor path from the Stack in `CLAUDE.md`. If it's still a placeholder, derive it from
`ProjectSettings/ProjectVersion.txt` (`C:\Program Files\Unity\Hub\Editor\<version>\Editor\Unity.exe`
on Windows) and check it exists; if not, ask the user for the path.

## 2. Run (in the background — it takes minutes)
```bash
"<Unity.exe>" -batchmode -nographics -projectPath . -runTests -testPlatform EditMode \
  -testResults ./Reports/TestResults-EditMode.xml -logFile ./Reports/unity-tests-EditMode.log
```
- `-testPlatform PlayMode` and matching file names for PlayMode.
- Filter: add `-testFilter "<filter>"`.
- **Never add `-quit`** with `-runTests` — Unity quits by itself when the run ends, and `-quit` cuts it short.
- Delete the previous XML first, so a stale file is never read as this run's result.
- Output goes to `Reports/` (git-ignored), **not** `Logs/`: `Logs/` is Unity's and is read-denied in
  `.claude/settings.json`, so results written there can't be read back.
- Don't trust the exit code alone — it can be 0 with failed tests. The XML is the result.

## 3. Read the result
- **Project locked:** if the log says another Unity instance has the project open (e.g. "another
  Unity instance is running with this project open" / "Multiple Unity instances cannot open the same
  project"), stop: tell the user to close the Editor and re-run — or to run the tests in the Editor's
  Test Runner and paste the result.
- **Compilation errors** appear in the log (`error CS`) before any test runs: report each
  (`file(line): error CSxxxx: message`) as **red** — the suite didn't run.
- **Results:** the XML is NUnit 3. Read the `<test-run>` attributes (`total`, `passed`, `failed`,
  `skipped`, `inconclusive`) and, for every `<test-case result="Failed">`, its `fullname` and the
  first lines of **its own** `<failure><message>` (the suites' messages just say "One or more child
  tests had errors"). Don't read the whole XML if it's large — grep it.
- No XML and no lock/compile error → report the last 40 lines of the log as the failure.

## 4. Report
```
Command: <the exact command>
EditMode: <total> total · <passed> passed · <failed> failed · <skipped> skipped
Failed:  <Namespace.Class.Test> — <first line of message>
```
Never call a run green if it didn't complete. Never summarize failures as "some tests failed" —
name them.
