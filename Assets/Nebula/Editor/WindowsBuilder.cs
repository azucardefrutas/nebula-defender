using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Profile;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NebulaDefender.Editor
{
    public static class WindowsBuilder
    {
        public const string ProfilePath=DemoGenerator.Root+"/Data/Windows-x64.asset";
        public static BuildProfile EnsureBuildProfile()
        {
            var profile=AssetDatabase.LoadAssetAtPath<BuildProfile>(ProfilePath);
            if(profile!=null)return profile;
            profile=ScriptableObject.CreateInstance<BuildProfile>();profile.name="Windows x64 / Nebula Defender";
            // Unity 6.3 exposes profile building but platform creation is still internal.
            // Resolve its installed platform GUID, and fail explicitly if the pinned Editor changes schema.
            var discovery=typeof(BuildPipeline).Assembly.GetType("UnityEditor.BuildTargetDiscovery",true);
            var method=discovery.GetMethod("GetGUIDFromBuildTarget",BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic,null,new[]{typeof(NamedBuildTarget),typeof(BuildTarget)},null);
            if(method==null)throw new NotSupportedException("The installed Editor does not expose the expected Unity 6.3 platform discovery API.");
            string guid=method.Invoke(null,new object[]{NamedBuildTarget.Standalone,BuildTarget.StandaloneWindows64}).ToString();
            var serialized=new SerializedObject(profile);
            var target=serialized.FindProperty("m_BuildTarget");var subtarget=serialized.FindProperty("m_Subtarget");var platform=serialized.FindProperty("m_PlatformId");
            if(target==null||subtarget==null||platform==null)throw new NotSupportedException("Unsupported BuildProfile serialization schema.");
            target.intValue=(int)BuildTarget.StandaloneWindows64;subtarget.intValue=(int)StandaloneBuildSubtarget.Player;platform.stringValue=guid;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            typeof(BuildProfile).GetMethod("TryCreatePlatformSettings",BindingFlags.Instance|BindingFlags.NonPublic)?.Invoke(profile,null);
            profile.overrideGlobalScenes=true;
            profile.scenes=new[]{new EditorBuildSettingsScene(DemoGenerator.CampaignScene,true),new EditorBuildSettingsScene(DemoGenerator.SandboxScene,true)};
            AssetDatabase.CreateAsset(profile,ProfilePath);AssetDatabase.SaveAssets();return profile;
        }
        [MenuItem("Tools/Nebula Defender/Build Windows x64")]
        public static void BuildWindows()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode before building.");
            if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone,BuildTarget.StandaloneWindows64))throw new NotSupportedException("Install Windows x64 build support in Unity Hub.");
            if(!File.Exists(DemoGenerator.CampaignScene))DemoGenerator.GenerateDemo();
            var profile=EnsureBuildProfile();
            string project=Path.GetFullPath(".");string output=Path.Combine(project,"Builds","Windows");Directory.CreateDirectory(output);
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
            EditorUserBuildSettings.development=false;
            PlayerSettings.bundleVersion="1.0.0";
            var report=BuildPipeline.BuildPlayer(new BuildPlayerWithProfileOptions {
                buildProfile=profile,locationPathName=Path.Combine(output,"NebulaDefender.exe"),options=BuildOptions.CompressWithLz4HC
            });
            string evidence=Path.Combine(project,"Documentation","Evidence");Directory.CreateDirectory(evidence);
            File.WriteAllText(Path.Combine(evidence,"build-summary.txt"),$"Result: {report.summary.result}\nPlatform: {report.summary.platform}\nErrors: {report.summary.totalErrors}\nWarnings: {report.summary.totalWarnings}\nSize: {report.summary.totalSize}\nTime: {report.summary.totalTime}\nOutput: {report.summary.outputPath}\n");
            if(report.summary.result!=BuildResult.Succeeded||report.summary.totalErrors!=0)throw new BuildFailedException("Windows build failed; inspect Evidence/build.log and build-summary.txt.");
            foreach(string name in new[]{"NebulaDefender.exe","UnityPlayer.dll","NebulaDefender_Data"})
                if(!File.Exists(Path.Combine(output,name))&&!Directory.Exists(Path.Combine(output,name)))throw new BuildFailedException("Missing build artifact: "+name);
            File.WriteAllText(Path.Combine(output,"LEEME.txt"),"NEBULA DEFENDER 1.0 - VUELVE A CASA\r\nAbre tres rutas de salto: despeja cada sector, entra en el portal azul y derrota a Nemesis para volver a casa.\r\nExtrae la carpeta completa y ejecuta NebulaDefender.exe. Conserva todos sus datos y bibliotecas. No necesitas Unity ni Visual Studio.\r\nMouse: apuntar. Clic izquierdo: disparar. Mantener clic derecho: avanzar. Pulsar rueda o Shift: escudo. C: camara. Escape: pausa. WASD y Espacio tambien disponibles.\r\nJUGAR permite entrar directamente; registra un piloto si quieres guardar marcas en el ranking local.\r\n");
            string zip=Path.Combine(project,"Builds","NebulaDefender-Windows-x64.zip");
            if(File.Exists(zip))File.Move(zip,Path.Combine(project,"Builds","NebulaDefender-Windows-x64-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff")+".zip"));
            ZipFile.CreateFromDirectory(output,zip,System.IO.Compression.CompressionLevel.Optimal,true);
            Debug.Log("NEBULA_WINDOWS_BUILD_OK: "+zip);
        }
    }
}
