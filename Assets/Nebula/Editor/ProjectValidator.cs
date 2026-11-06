using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace NebulaDefender.Editor
{
    public static class ProjectValidator
    {
        [MenuItem("Tools/Nebula Defender/Validate Project")]
        public static void Validate()
        {
            var setup=EditorSceneManager.GetSceneManagerSetup();int checkedObjects=0;
            try
            {
                foreach(string scenePath in new[]{DemoGenerator.SandboxScene,DemoGenerator.CampaignScene})
                {
                    if(!File.Exists(scenePath))throw new InvalidOperationException("Missing scene: "+scenePath);
                    var scene=EditorSceneManager.OpenScene(scenePath);
                    foreach(var root in scene.GetRootGameObjects())foreach(var transform in root.GetComponentsInChildren<Transform>(true))
                    {
                        checkedObjects++;
                        if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject)>0)throw new InvalidOperationException("Missing script on "+transform.name);
                        foreach(var component in transform.GetComponents<MonoBehaviour>())if(component!=null)CheckReferences(component);
                    }
                    var session=UnityEngine.Object.FindFirstObjectByType<GameSession>();
                    if(session==null)throw new InvalidOperationException("No GameSession: "+scenePath);
                    var data=new SerializedObject(session);
                    foreach(string name in new[]{"balance","player","input","arena","projectiles","director","feedback","audioService","backdrop"})
                        if(data.FindProperty(name).objectReferenceValue==null)throw new InvalidOperationException(scenePath+": missing "+name);
                    if(data.FindProperty("levels").arraySize!=3)throw new InvalidOperationException("Three sector assets are required.");
                    if(Camera.main==null||Camera.main.GetComponent<CameraRig>()==null)throw new InvalidOperationException("Missing combat camera.");
                }
                foreach(string guid in AssetDatabase.FindAssets("t:Prefab",new[]{DemoGenerator.Root}))
                {
                    var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                    foreach(var component in prefab.GetComponentsInChildren<MonoBehaviour>(true))if(component!=null)CheckReferences(component);
                }
                foreach(string guid in AssetDatabase.FindAssets("t:Material",new[]{DemoGenerator.Root}))
                {
                    var material=AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                    if(material.shader==null||ShaderUtil.ShaderHasError(material.shader))throw new InvalidOperationException("Invalid shader: "+material.name);
                }
                if(UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline is not UniversalRenderPipelineAsset)throw new InvalidOperationException("URP is not configured.");
                foreach(string guid in AssetDatabase.FindAssets("t:Shader",new[]{"Assets/Nebula"}))
                {
                    var shader=AssetDatabase.LoadAssetAtPath<Shader>(AssetDatabase.GUIDToAssetPath(guid));
                    if(shader==null||ShaderUtil.ShaderHasError(shader))throw new InvalidOperationException("Invalid project shader: "+AssetDatabase.GUIDToAssetPath(guid));
                }
                var layers=new[]{"Player","Asteroids","Enemies","PlayerProjectiles","EnemyProjectiles","Pickups"};
                for(int i=0;i<layers.Length;i++)if(LayerMask.LayerToName(i+8)!=layers[i])throw new InvalidOperationException("Incorrect physics layer: "+layers[i]);
                if(!Physics.GetIgnoreLayerCollision(8,11)||Physics.GetIgnoreLayerCollision(9,11)||Physics.GetIgnoreLayerCollision(8,12))throw new InvalidOperationException("Incorrect collision matrix.");
                var profile=WindowsBuilder.EnsureBuildProfile();
                if(profile.GetScenesForBuild().First().path!=DemoGenerator.CampaignScene)throw new InvalidOperationException("Campaign must be the first build scene.");
                string report=$"NEBULA_PROJECT_VALIDATION_OK\nObjects checked: {checkedObjects}\nScenes: CoreSandbox, Campaign\nThree sector assets, required references, prefab scripts, material shaders, URP, gameplay layers, laser collision policy, Windows profile scene order.\n";
                Directory.CreateDirectory("Documentation/Evidence");File.WriteAllText("Documentation/Evidence/project-validation.txt",report);Debug.Log(report);
            }
            finally
            {
                if(setup.Any(s=>s.isLoaded)&&setup.Count(s=>s.isActive)==1)EditorSceneManager.RestoreSceneManagerSetup(setup);
            }
        }
        private static void CheckReferences(MonoBehaviour component)
        {
            var serialized=new SerializedObject(component);var property=serialized.GetIterator();
            while(property.Next(true))
                if(property.propertyType==SerializedPropertyType.ObjectReference&&property.objectReferenceValue==null&&property.objectReferenceInstanceIDValue!=0)
                    throw new InvalidOperationException($"Broken reference: {component.name}/{component.GetType().Name}/{property.propertyPath}");
        }
        [MenuItem("Tools/Nebula Defender/Verify Generator Idempotence")]
        public static void VerifyIdempotence()
        {
            string[] paths=AssetDatabase.FindAssets("",new[]{DemoGenerator.Root}).Select(AssetDatabase.GUIDToAssetPath).OrderBy(p=>p).ToArray();
            var identifiers=paths.ToDictionary(p=>p,p=>AssetDatabase.AssetPathToGUID(p));
            DemoGenerator.GenerateDemo();
            string[] after=AssetDatabase.FindAssets("",new[]{DemoGenerator.Root}).Select(AssetDatabase.GUIDToAssetPath).OrderBy(p=>p).ToArray();
            if(!paths.SequenceEqual(after)||after.Any(p=>AssetDatabase.AssetPathToGUID(p)!=identifiers[p]))throw new InvalidOperationException("Generation changed existing asset paths or GUIDs.");
            Validate();File.WriteAllText("Documentation/Evidence/idempotence.txt",$"PASS: {paths.Length} generated asset paths and GUIDs preserved after regeneration.\n");
        }
    }
}
