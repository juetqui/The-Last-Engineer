using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace Project.Build.Editor
{
    /// <summary>Build options parsed from the command line; plain class so it can be tested without a build.</summary>
    public sealed class BuildArguments
    {
        private const string OutputFlag = "-buildOutput";
        private const string DevelopmentFlag = "-development";
        private const string AppBundleFlag = "-buildAab";
        private const string DefaultRoot = "Builds";

        public BuildTarget Target { get; }
        public string OutputPath { get; }
        public bool Development { get; }
        public bool AndroidAppBundle { get; }

        private BuildArguments(BuildTarget target, string outputPath, bool development, bool androidAppBundle)
        {
            Target = target;
            OutputPath = outputPath;
            Development = development;
            AndroidAppBundle = androidAppBundle;
        }

        public static BuildArguments Parse(IReadOnlyList<string> args, BuildTarget target, string productName)
        {
            var development = Contains(args, DevelopmentFlag);
            var appBundle = Contains(args, AppBundleFlag);
            var output = ValueOf(args, OutputFlag) ?? DefaultOutputPath(target, productName, appBundle);
            return new BuildArguments(target, output, development, appBundle);
        }

        public static string DefaultOutputPath(BuildTarget target, string productName, bool appBundle)
        {
            var folder = Path.Combine(DefaultRoot, target.ToString());
            switch (target)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return Path.Combine(folder, productName + ".exe");
                case BuildTarget.StandaloneOSX:
                    return Path.Combine(folder, productName + ".app");
                case BuildTarget.StandaloneLinux64:
                    return Path.Combine(folder, productName + ".x86_64");
                case BuildTarget.Android:
                    return Path.Combine(folder, productName + (appBundle ? ".aab" : ".apk"));
                default:
                    // iOS (Xcode project), WebGL and others build into a folder.
                    return folder;
            }
        }

        private static bool Contains(IReadOnlyList<string> args, string flag)
        {
            for (var i = 0; i < args.Count; i++)
            {
                if (string.Equals(args[i], flag, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        private static string ValueOf(IReadOnlyList<string> args, string flag)
        {
            for (var i = 0; i < args.Count - 1; i++)
            {
                if (string.Equals(args[i], flag, StringComparison.OrdinalIgnoreCase)) return args[i + 1];
            }
            return null;
        }
    }
}
