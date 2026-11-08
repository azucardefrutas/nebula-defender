using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NebulaDefender.Editor
{
    /// <summary>Installs only the original models generated in the dedicated AuthoredV1 folder.</summary>
    public static class OriginalArtWorkshop
    {
        private const string Art = "Assets/Nebula/Art/Models/AuthoredV1";
        private const string Materials = "Assets/Nebula/Art/Materials/AuthoredV1";
        private static readonly Dictionary<string, Material> Palette = new Dictionary<string, Material>();

        [MenuItem("Tools/Nebula Defender/Install Original Art")]
        public static void Install()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before changing art.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            foreach (string name in new[] { "Aurora", "Marauder", "Nemesis", "Asteroid0", "Asteroid1", "Asteroid2", "Asteroid3" })
                if (!File.Exists(Art + "/" + name + ".fbx")) throw new FileNotFoundException("Generate original Blender models first: " + name);
            Directory.CreateDirectory(Materials); AssetDatabase.Refresh(); Palette.Clear();
            ColorMaterial("ND_Pearl", new Color(.72f,.83f,.91f), .65f,.7f);
            ColorMaterial("ND_Cobalt", new Color(.018f,.11f,.3f), .65f,.68f);
            ColorMaterial("ND_Carbon", new Color(.028f,.042f,.06f), .6f,.62f);
            ColorMaterial("ND_Steel", new Color(.26f,.33f,.39f), .8f,.73f);
            ColorMaterial("ND_Glass", new Color(.018f,.25f,.34f), .7f,.84f);
            ColorMaterial("ND_Ion", new Color(.015f,.7f,1), .3f,.75f, 2.2f);
            ColorMaterial("ND_Amber", new Color(1,.12f,.008f), .3f,.65f, 1.6f);
            ColorMaterial("ND_Hostile", new Color(.4f,.055f,.018f), .65f,.6f);
            ColorMaterial("ND_Basalt0", new Color(.22f,.2f,.19f), .08f,.1f);
            ColorMaterial("ND_Basalt1", new Color(.32f,.29f,.25f), .08f,.1f);
            ColorMaterial("ND_Basalt2", new Color(.15f,.16f,.18f), .08f,.1f);
            ColorMaterial("ND_Basalt3", new Color(.4f,.36f,.29f), .08f,.1f);
            foreach (string name in new[] { "Aurora", "Marauder", "Nemesis", "Asteroid0", "Asteroid1", "Asteroid2", "Asteroid3" })
            {
                var importer = (ModelImporter)AssetImporter.GetAtPath(Art + "/" + name + ".fbx");
                importer.importAnimation = false; importer.animationType = ModelImporterAnimationType.None;
                importer.importCameras = false; importer.importLights = false;
                importer.importNormals = ModelImporterNormals.Import;
                importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
                foreach (var pair in Palette)
                    importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material),pair.Key),pair.Value);
                importer.SaveAndReimport();
            }
            UpgradePrefab("PlayerShip", "Aurora"); UpgradePrefab("InterceptorDrone", "Marauder"); UpgradePrefab("NemesisPrime", "Nemesis");
            for (int i=0;i<4;i++) UpgradePrefab("Asteroid"+i,"Asteroid"+i);
            UpdateMenuDisplays(); AssetDatabase.SaveAssets();
            ValidateAndRender();
            Debug.Log("NEBULA_ORIGINAL_ART_OK: seven original FBX models installed; physics roots preserved.");
        }

        private static void ColorMaterial(string name, Color color, float metallic, float smoothness, float emission = 0)
        {
            string path = Materials + "/" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            // Preserve artist edits to an existing palette material on regeneration.
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };
                mat.SetColor("_BaseColor",color); mat.SetFloat("_Metallic",metallic); mat.SetFloat("_Smoothness",smoothness);
                if (emission>0) { mat.EnableKeyword("_EMISSION"); mat.SetColor("_EmissionColor",color*emission); }
                AssetDatabase.CreateAsset(mat,path);
            }
            Palette.Add(name,mat);
        }

        private static Transform Find(Transform root, string name) => root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name==name);
        private static GameObject Model(string name, Transform parent)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(Art+"/"+name+".fbx");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset,parent);
            instance.name = "Authored " + name;
            var forward = Find(instance.transform,"ND_FORWARD"); var up = Find(instance.transform,"ND_UP");
            if (forward==null || up==null) throw new InvalidOperationException("Missing model axes: "+name);
            var heading = parent.InverseTransformVector(forward.position-instance.transform.position);
            var vertical = parent.InverseTransformVector(up.position-instance.transform.position);
            instance.transform.localRotation = Quaternion.Inverse(Quaternion.LookRotation(heading,vertical))*instance.transform.localRotation;
            if (name=="Nemesis") instance.transform.localRotation=Quaternion.Euler(0,180,0)*instance.transform.localRotation;
            return instance;
        }

        private static void UpgradePrefab(string prefabName, string modelName)
        {
            string path=DemoGenerator.Root+"/Prefabs/"+prefabName+".prefab";
            var root=PrefabUtility.LoadPrefabContents(path);
            try
            {
                var visual=root.transform.GetChild(0);
                // These paths belong to our generated demo, never arbitrary user prefabs.
                if (prefabName=="PlayerShip" && Find(visual,"Titanium fuselage")==null && Find(visual,"Authored Aurora")==null)
                    throw new InvalidOperationException("Custom player art detected; left unchanged.");
                if(prefabName=="InterceptorDrone" && Find(visual,"Dark fuselage")==null && Find(visual,"Authored Marauder")==null)
                    throw new InvalidOperationException("Custom drone art detected; left unchanged.");
                if(prefabName=="NemesisPrime" && Find(visual,"Command hull")==null && Find(visual,"Authored Nemesis")==null)
                    throw new InvalidOperationException("Custom flagship art detected; left unchanged.");
                if(prefabName.StartsWith("Asteroid",StringComparison.Ordinal) && visual.GetComponent<MeshRenderer>()==null && Find(visual,"Authored "+modelName)==null)
                    throw new InvalidOperationException("Custom asteroid art detected; left unchanged.");
                Transform engines=prefabName=="PlayerShip" ? visual.Find("Ion exhaust") : null;
                if(prefabName=="PlayerShip" && engines==null)throw new InvalidOperationException("Player engine reference must be preserved.");
                foreach (Transform child in visual.Cast<Transform>().ToArray())
                    if (child!=engines) UnityEngine.Object.DestroyImmediate(child.gameObject);
                // The rock's old mesh was on the visual root itself.
                if (prefabName.StartsWith("Asteroid",StringComparison.Ordinal))
                {
                    UnityEngine.Object.DestroyImmediate(visual.GetComponent<MeshRenderer>());
                    UnityEngine.Object.DestroyImmediate(visual.GetComponent<MeshFilter>());
                }
                var model=Model(modelName,visual);
                if (prefabName=="NemesisPrime") root.GetComponent<BossController>().Configure(Find(model.transform,"ND_CORE"));
                PrefabUtility.SaveAsPrefabAsset(root,path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void UpdateMenuDisplays()
        {
            var setup=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                foreach (string path in new[]{DemoGenerator.CampaignScene,DemoGenerator.SandboxScene})
                {
                    var scene=EditorSceneManager.OpenScene(path);
                    var backdrop=UnityEngine.Object.FindFirstObjectByType<SpaceBackdrop>();
                    var serialized=new SerializedObject(backdrop);
                    var preview=(GameObject)serialized.FindProperty("menuShip").objectReferenceValue;
                    foreach (Transform child in preview.transform.Cast<Transform>().ToArray()) UnityEngine.Object.DestroyImmediate(child.gameObject);
                    var filter=preview.GetComponent<MeshFilter>(); if(filter!=null)UnityEngine.Object.DestroyImmediate(filter);
                    var renderer=preview.GetComponent<MeshRenderer>(); if(renderer!=null)UnityEngine.Object.DestroyImmediate(renderer);
                    Model("Aurora",preview.transform);
                    EditorSceneManager.SaveScene(scene);
                }
            }
            finally { if(setup.Any(s=>s.isLoaded))EditorSceneManager.RestoreSceneManagerSetup(setup); }
        }

        [MenuItem("Tools/Nebula Defender/Render Original Art Previews")]
        public static void ValidateAndRender()
        {
            Directory.CreateDirectory("Documentation/Evidence");
            var lines=new List<string> {"Original Blender art / Unity imported validation"};
            foreach(string name in new[]{"Aurora","Marauder","Nemesis","Asteroid0","Asteroid1","Asteroid2","Asteroid3"})
            {
                var root=new GameObject("Preview "+name);
                try
                {
                    var model=Model(name,root.transform);
                    var renderers=model.GetComponentsInChildren<MeshRenderer>();
                    int triangles=model.GetComponentsInChildren<MeshFilter>().Sum(f=>f.sharedMesh.triangles.Length/3);
                    var bounds=new Bounds(renderers[0].bounds.center,Vector3.zero);
                    foreach(var r in renderers)
                    {
                        bounds.Encapsulate(r.bounds);
                        foreach(var mat in r.sharedMaterials)
                            if(mat==null || mat.shader.name!="Universal Render Pipeline/Lit" || ShaderUtil.ShaderHasError(mat.shader))
                                throw new InvalidOperationException("Invalid imported material in "+name);
                    }
                    float longest=Mathf.Max(bounds.size.x,bounds.size.y,bounds.size.z);
                    if (triangles>10000 || longest<1 || longest>12 || model.GetComponentsInChildren<Collider>().Length!=0)
                        throw new InvalidOperationException("Invalid model scale/topology/colliders: "+name+" / "+bounds.size);
                    if(name=="Aurora" && Vector3.Dot((Find(model.transform,"ND_FORWARD").position-model.transform.position).normalized,Vector3.forward)<.99f)
                        throw new InvalidOperationException("Aurora nose must point to +Z.");
                    lines.Add($"{name}: {triangles} triangles; {renderers.Length} renderers; bounds {bounds.size}; URP materials valid; no visual colliders.");
                    if(name=="Asteroid0" || !name.StartsWith("Asteroid",StringComparison.Ordinal)) Render(root,bounds,name);
                }
                finally { UnityEngine.Object.DestroyImmediate(root); }
            }
            File.WriteAllLines("Documentation/Evidence/authored-models-validation.txt",lines);
        }

        private static void Render(GameObject root, Bounds bounds, string name)
        {
            var preview=new PreviewRenderUtility();
            try
            {
                preview.AddSingleGO(root);
                preview.camera.clearFlags=CameraClearFlags.SolidColor; preview.camera.backgroundColor=new Color(.018f,.026f,.045f);
                preview.camera.fieldOfView=32;
                float distance=Mathf.Max(bounds.size.x,bounds.size.z)*2.5f;
                preview.camera.transform.position=bounds.center+new Vector3(-.85f,1.1f,1.5f).normalized*distance;
                preview.camera.transform.LookAt(bounds.center);
                preview.camera.nearClipPlane=.01f; preview.camera.farClipPlane=distance*5;
                preview.lights[0].intensity=2; preview.lights[0].transform.rotation=Quaternion.Euler(40,35,0);
                preview.lights[1].intensity=1.5f; preview.lights[1].color=new Color(.3f,.65f,1); preview.lights[1].transform.rotation=Quaternion.Euler(315,210,0);
                preview.ambientColor=new Color(.22f,.24f,.3f);
                preview.BeginStaticPreview(new Rect(0,0,1100,850)); preview.Render(true);
                var texture=preview.EndStaticPreview();
                File.WriteAllBytes("Documentation/Evidence/model-"+name.ToLowerInvariant()+".png",texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
            }
            finally { preview.Cleanup(); }
        }
    }
}
