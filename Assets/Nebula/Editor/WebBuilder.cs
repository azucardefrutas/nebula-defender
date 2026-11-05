using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NebulaDefender.Editor
{
    public static class WebBuilder
    {
        [MenuItem("Tools/Nebula Defender/Build Web Localhost")]
        public static void BuildWeb()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before building.");
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
                throw new NotSupportedException("Web Build Support is required for Unity 6000.3.15f1.");
            PlayerSettings.WebGL.template = "PROJECT:Nebula";
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = false;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
            PlayerSettings.WebGL.initialMemorySize = 128;
            PlayerSettings.WebGL.maximumMemorySize = 512;
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.WebGL, ManagedStrippingLevel.Low);
#if UNITY_WEBGL
            // Local play builds favor iteration time; the game rules and scenes are identical.
            UnityEditor.WebGL.UserBuildSettings.codeOptimization = UnityEditor.WebGL.WasmCodeOptimization.BuildTimes;
#endif
            PlayerSettings.runInBackground = false;
            AssetDatabase.SaveAssets();
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { DemoGenerator.CampaignScene, DemoGenerator.SandboxScene },
                locationPathName = "Builds/WebGL",
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            });
            Directory.CreateDirectory("Documentation/Evidence");
            File.WriteAllText("Documentation/Evidence/web-build-summary.txt",
                $"Result: {report.summary.result}\nPlatform: {report.summary.platform}\nErrors: {report.summary.totalErrors}\nWarnings: {report.summary.totalWarnings}\nSize: {report.summary.totalSize}\nTime: {report.summary.totalTime}\nOutput: {report.summary.outputPath}\n");
            if (report.summary.result != BuildResult.Succeeded || report.summary.totalErrors != 0)
                throw new BuildFailedException("Web build failed; inspect Documentation/Evidence/web-build.log.");
            if (!File.Exists("Builds/WebGL/index.html")) throw new BuildFailedException("Missing web entry page.");
            Debug.Log("NEBULA_WEB_BUILD_OK: Builds/WebGL/index.html");
        }
    }
}
