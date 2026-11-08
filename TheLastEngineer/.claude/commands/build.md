---
description: "Build the player for a platform in batchmode through the project's build method, with pre-flight and post-build checks."
argument-hint: "<platform: Win64 | Android | iOS | WebGL | …>"
disable-model-invocation: true
---

# /build

Target: `$ARGUMENTS` (a Unity `-buildTarget` value: `Win64`, `OSXUniversal`, `Linux64`, `Android`,
`iOS`, `WebGL`, …). Blank → ask.

## 1. Confirm out loud
State the platform, the output folder (default `Builds/<target>/`), the branch and the build method,
and wait for the user's go-ahead.

## 2. Pre-flight
- Clean working tree (`git status`) — or the user explicitly accepts building uncommitted changes.
- On the expected branch.
- `/run-tests` green (EditMode at least). Red → stop.
- The Editor is closed (the build fails if the project is open).
- The build method exists: the Build line in `CLAUDE.md` names it. The template ships
  `Project.Build.Editor.BuildCommand.Build` (`Assets/Editor/Build/`). If the project uses another
  one, use that. **If there's none, don't invent one** — offer to add the template's as its own
  reviewed change.
- At least one enabled scene in Build Settings (the template's method fails without one).
- The project uses Addressables? The template's method does **not** build Addressables content:
  build it first in the Editor, or extend the method (`AddressableAssetSettings.BuildPlayerContent()`)
  as its own change.

## 3. Build (in the background)
```bash
"<Unity.exe>" -batchmode -nographics -quit -projectPath . -buildTarget <target> \
  -executeMethod <BuildMethod> -logFile ./Reports/build-<target>.log
```
The template's method reads, besides `-buildTarget` (`Win64`, `Android`, `iOS`, `WebGL`, …):
`-buildOutput <path>` (default `Builds/<BuildTarget>/<productName>.<ext>`, a folder for iOS/WebGL),
`-development`, and `-buildAab` (Android App Bundle). It exits with code 1 on failure.
A project-specific method: read its code for the arguments it expects.
The editor path comes from `CLAUDE.md`, as in `/run-tests`.

## 4. Post-build
- Exit code and the log: errors (`error CS`, `Build Failed`, `BuildFailedException`), and warnings
  that are new compared with the last build log if there is one.
- The build report summary in the log (size by category) and the artifact's size on disk.
- Where the artifact is.
- Report: command, result, size, location, errors/warnings. Never commit or upload the build.
