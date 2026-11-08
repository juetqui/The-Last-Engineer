using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Project.Build.Editor
{
    /// <summary>
    /// Batchmode entry point used by /build:
    /// Unity -batchmode -nographics -quit -projectPath . -buildTarget Win64
    ///   -executeMethod Project.Build.Editor.BuildCommand.Build [-buildOutput path] [-development] [-buildAab]
    /// </summary>
    public static class BuildCommand
    {
        private const int FailureExitCode = 1;

        public static void Build()
        {
            // -buildTarget switches the active target before this method runs.
            var target = EditorUserBuildSettings.activeBuildTarget;
            var arguments = BuildArguments.Parse(Environment.GetCommandLineArgs(), target, PlayerSettings.productName);

            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0)
            {
                Fail("No enabled scenes in Build Settings (ProjectSettings/EditorBuildSettings.asset).");
                return;
            }

            EditorUserBuildSettings.buildAppBundle = arguments.AndroidAppBundle;

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                target = target,
                targetGroup = BuildPipeline.GetBuildTargetGroup(target),
                locationPathName = arguments.OutputPath,
                options = arguments.Development ? BuildOptions.Development : BuildOptions.None,
            };

            Debug.Log($"[BuildCommand] Building {target} → {arguments.OutputPath} ({scenes.Length} scenes, development: {arguments.Development})");
            var summary = BuildPipeline.BuildPlayer(options).summary;
            Debug.Log($"[BuildCommand] Result: {summary.result} · size: {summary.totalSize} bytes · errors: {summary.totalErrors} · warnings: {summary.totalWarnings} · time: {summary.totalTime}");

            if (summary.result != BuildResult.Succeeded)
            {
                Fail($"Build {summary.result}.");
            }
        }

        private static void Fail(string message)
        {
            Debug.LogError($"[BuildCommand] {message}");
            EditorApplication.Exit(FailureExitCode);
        }
    }
}
