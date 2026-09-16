#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildScript
{
    public static void BuildAndroid()
    {
        const string outputPath = "Builds/Android/Baby-Run.apk";
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

        var scenes = new[] { "Assets/Scenes/SampleScene.unity" };
        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.Android,
            options = BuildOptions.StrictMode
        };

        var report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new BuildFailedException($"Android build failed: {report.summary.result} ({report.summary.totalErrors} errors)");
        }

        Debug.Log($"Baby Run APK built successfully: {outputPath} ({report.summary.totalSize} bytes)");
    }
}
#endif
