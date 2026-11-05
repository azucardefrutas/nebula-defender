using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace NebulaDefender.Editor
{
    [InitializeOnLoad]
    internal static class EditorStartup
    {
        static EditorStartup()
        {
            GameSession.EditorQuitRequested -= StopPlay;
            GameSession.EditorQuitRequested += StopPlay;
            if (!UnityEngine.Application.isBatchMode && !File.Exists(DemoGenerator.SandboxScene)) EditorApplication.delayCall += FirstImport;
        }
        private static void StopPlay() => EditorApplication.isPlaying = false;
        private static void FirstImport()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) { EditorApplication.delayCall += FirstImport; return; }
            if (File.Exists(DemoGenerator.SandboxScene) || EditorApplication.isPlayingOrWillChangePlaymode) return;
            DemoGenerator.GenerateDemo();
        }
        [MenuItem("Tools/Nebula Defender/Open Core Sandbox")]
        public static void OpenCore() => Open(DemoGenerator.SandboxScene);
        [MenuItem("Tools/Nebula Defender/Open Campaign")]
        public static void OpenCampaign() => Open(DemoGenerator.CampaignScene);
        private static void Open(string path)
        {
            if (!File.Exists(path)) DemoGenerator.GenerateDemo();
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(path);
        }
    }
}
