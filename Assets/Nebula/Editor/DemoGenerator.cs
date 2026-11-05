using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using TMPro;

namespace NebulaDefender.Editor
{
    public static partial class DemoGenerator
    {
        public const string Root = "Assets/Nebula/Generated";
        public const string SandboxScene = Root + "/Scenes/CoreSandbox.unity";
        public const string CampaignScene = Root + "/Scenes/Campaign.unity";
        private static Material hull, blue, dark, glass, cyan, orange, rock;
        private static Mesh wedge, wing;

        [MenuItem("Tools/Nebula Defender/Generate Demo")]
        public static void GenerateDemo()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before generation.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            foreach(var sub in new[]{"Materials","Meshes","Prefabs","Data","Input","Scenes","UI","Audio"}) Directory.CreateDirectory(Root+"/"+sub);
            AssetDatabase.Refresh();
            ConfigureProject();
            hull=Material("Pearl titanium",new Color(0.68f,0.8f,0.88f),0.65f);
            blue=Material("Cobalt armor",new Color(0.025f,0.16f,0.42f),0.6f);
            dark=Material("Carbon",new Color(0.035f,0.052f,0.085f),0.45f);
            glass=Material("Cockpit",new Color(0.03f,0.32f,0.48f),0.8f,new Color(0.03f,0.25f,0.4f));
            cyan=Material("Ion cyan",new Color(0.08f,0.85f,1),0,new Color(0.12f,1.5f,2));
            orange=Material("Hostile amber",new Color(1,0.22f,0.04f),0,new Color(2,0.2f,0.02f));
            rock=Material("Basalt",new Color(0.27f,0.25f,0.3f),0.1f);
            wedge=MeshAsset("Fuselage",()=>ProceduralModels.Wedge(0.55f,2.1f,0.6f));
            wing=MeshAsset("Swept wing",()=>ProceduralModels.Wedge(1.55f,1.15f,0.11f));
            var balance=GetOrCreate<GameBalance>(Root+"/Data/ArcadeBalance.asset");
            CreateLevels();
            var actions=CreateInput();
            var player=CreatePlayer(balance);
            var rocks=CreateRocks();
            var friendly=CreateProjectile("PlayerLaser",cyan);
            var hostile=CreateProjectile("EnemyLaser",orange);
            var burst=CreateBurst();
            CreateDrone();CreateBoss();CreatePickup(true);CreatePickup(false);EnsureFont();
            CreateScene(balance,CreateLevels(),actions,player,rocks,friendly,hostile,burst,false,CampaignScene);
            CreateScene(balance,CreateLevels(),actions,player,rocks,friendly,hostile,burst,true,SandboxScene);
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(CampaignScene,true),new EditorBuildSettingsScene(SandboxScene,true)};
            WindowsBuilder.EnsureBuildProfile();
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            Debug.Log("NEBULA_GENERATION_OK: " + SandboxScene);
        }
        private static void ConfigureProject()
        {
            var tags=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers=tags.FindProperty("layers"); string[] names={"Player","Asteroids","Enemies","PlayerProjectiles","EnemyProjectiles","Pickups"};
            for(int i=0;i<names.Length;i++) layers.GetArrayElementAtIndex(8+i).stringValue=names[i];
            tags.ApplyModifiedPropertiesWithoutUndo();
            // Only the gameplay layers are changed. Laser pairs never collide with their own team.
            for(int a=8;a<=13;a++) for(int b=8;b<=13;b++) Physics.IgnoreLayerCollision(a,b,true);
            foreach(var pair in new[]{new[]{8,9},new[]{8,10},new[]{8,12},new[]{8,13},new[]{9,11},new[]{10,11}}) Physics.IgnoreLayerCollision(pair[0],pair[1],false);
            PlayerSettings.companyName="UPB Arcade"; PlayerSettings.productName="Nebula Defender Galactic Assault";
            PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;
            PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
            PlayerSettings.runInBackground=false;
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
            PlayerSettings.SetApiCompatibilityLevel(UnityEditor.Build.NamedBuildTarget.Standalone,ApiCompatibilityLevel.NET_Standard);
            PlayerSettings.colorSpace=ColorSpace.Linear;
            var settings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            var input=settings.FindProperty("activeInputHandler");if(input!=null)input.intValue=1;
            settings.ApplyModifiedPropertiesWithoutUndo();
            EditorSettings.serializationMode=SerializationMode.ForceText;
            QualitySettings.vSyncCount=1;
            EditorUtility.SetDirty(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/DynamicsManager.asset")[0]);
        }
        private static T GetOrCreate<T>(string path) where T:ScriptableObject
        {
            var asset=AssetDatabase.LoadAssetAtPath<T>(path);
            if(asset==null){asset=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(asset,path);}return asset;
        }
        private static T LoadPrefab<T>(string path) where T:Component
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            return root!=null?root.GetComponent<T>():null;
        }
        private static T SavePrefab<T>(GameObject root,string path) where T:Component
        {
            PrefabUtility.SaveAsPrefabAsset(root,path);
            UnityEngine.Object.DestroyImmediate(root);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var component=LoadPrefab<T>(path);
            if(component==null||!EditorUtility.IsPersistent(component))throw new InvalidOperationException("Invalid generated prefab: "+path);
            return component;
        }
        private static Material Material(string name,Color color,float metallic,Color emission=default)
        {
            string path=Root+"/Materials/"+name+".mat";
            var existing=AssetDatabase.LoadAssetAtPath<Material>(path);if(existing!=null)return existing;
            var mat=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name};
            mat.SetColor("_BaseColor",color);mat.SetFloat("_Metallic",metallic);mat.SetFloat("_Smoothness",0.43f);
            if(emission.maxColorComponent>0){mat.EnableKeyword("_EMISSION");mat.SetColor("_EmissionColor",emission);}
            AssetDatabase.CreateAsset(mat,path);return mat;
        }
        private static Mesh MeshAsset(string name,Func<Mesh> create)
        {
            string path=Root+"/Meshes/"+name+".asset";
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(mesh!=null)return mesh;
            mesh=create();mesh.name=name;AssetDatabase.CreateAsset(mesh,path);return mesh;
        }
        private static InputActionAsset CreateInput()
        {
            string path=Root+"/Input/NebulaControls.inputactions";
            if(File.Exists(path))return AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
            var asset=ScriptableObject.CreateInstance<InputActionAsset>();asset.name="NebulaControls";
            var map=asset.AddActionMap("Flight");
            var thrust=map.AddAction("Throttle",InputActionType.Value,expectedControlLayout:"Axis");
            thrust.AddCompositeBinding("1DAxis").With("Positive","<Keyboard>/w").With("Negative","<Keyboard>/s");
            thrust.AddCompositeBinding("1DAxis").With("Positive","<Keyboard>/upArrow").With("Negative","<Keyboard>/downArrow");
            thrust.AddBinding("<Gamepad>/leftStick/y");
            var turn=map.AddAction("Turn",InputActionType.Value,expectedControlLayout:"Axis");
            turn.AddCompositeBinding("1DAxis").With("Positive","<Keyboard>/d").With("Negative","<Keyboard>/a");
            turn.AddCompositeBinding("1DAxis").With("Positive","<Keyboard>/rightArrow").With("Negative","<Keyboard>/leftArrow");
            turn.AddBinding("<Gamepad>/leftStick/x");
            map.AddAction("Fire",InputActionType.Button,"<Keyboard>/space").AddBinding("<Gamepad>/rightTrigger");
            var shield=map.AddAction("Shield",InputActionType.Button,"<Keyboard>/leftShift");shield.AddBinding("<Keyboard>/rightShift");shield.AddBinding("<Gamepad>/buttonSouth");
            var system=asset.AddActionMap("System");system.AddAction("Pause",InputActionType.Button,"<Keyboard>/escape").AddBinding("<Gamepad>/start");
            system.AddAction("Confirm",InputActionType.Button,"<Keyboard>/enter").AddBinding("<Gamepad>/buttonSouth");
            File.WriteAllText(path,asset.ToJson());UnityEngine.Object.DestroyImmediate(asset);AssetDatabase.ImportAsset(path);
            return AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
        }
        private static PlayerController CreatePlayer(GameBalance balance)
        {
            string path=Root+"/Prefabs/PlayerShip.prefab";
            var existing=LoadPrefab<PlayerController>(path);if(existing!=null)return existing;
            var root=new GameObject("ND-01 / AURORA");root.layer=8;
            var body=root.AddComponent<Rigidbody>();body.useGravity=false;body.linearDamping=balance.drag;
            body.constraints=RigidbodyConstraints.FreezePositionY|RigidbodyConstraints.FreezeRotationX|RigidbodyConstraints.FreezeRotationZ;
            body.interpolation=RigidbodyInterpolation.Interpolate;body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
            var collider=root.AddComponent<CapsuleCollider>();collider.direction=2;collider.height=3;collider.radius=0.6f;
            var visual=new GameObject("Visual / replace with your model").transform;visual.SetParent(root.transform,false);
            ProceduralModels.Part("Titanium fuselage",visual,wedge,hull,Vector3.zero,Vector3.one);
            ProceduralModels.Part("Cobalt delta wings",visual,wing,blue,new Vector3(0,-0.1f,-0.5f),Vector3.one);
            ProceduralModels.Part("Canopy",visual,wedge,glass,new Vector3(0,0.4f,0.1f),new Vector3(0.5f,0.5f,0.5f));
            var engines=new GameObject("Ion exhaust").transform;engines.SetParent(visual,false);
            foreach(float x in new[]{-0.75f,0.75f}) {
                ProceduralModels.Primitive("Engine nacelle",PrimitiveType.Cylinder,visual,dark,new Vector3(x,0,-0.85f),new Vector3(0.4f,0.6f,0.4f)).transform.localRotation=Quaternion.Euler(90,0,0);
                ProceduralModels.Primitive("Engine glow",PrimitiveType.Sphere,engines,cyan,new Vector3(x,0,-1.5f),new Vector3(0.25f,0.23f,0.7f));
                ProceduralModels.Primitive("Navigation light",PrimitiveType.Sphere,visual,x<0?orange:cyan,new Vector3(x*1.8f,0,-0.9f),Vector3.one*0.12f);
            }
            var shield=ProceduralModels.Primitive("Shield / respawn protection",PrimitiveType.Sphere,root.transform,ShieldMaterial(),Vector3.zero,Vector3.one*3.8f);
            shield.SetActive(false);
            var controller=root.AddComponent<PlayerController>();controller.Configure(balance,engines);
            root.AddComponent<PlayerHealth>().Configure(shield);root.AddComponent<WeaponController>();
            return SavePrefab<PlayerController>(root,path);
        }
        private static Material ShieldMaterial()
        {
            var mat=Material("Shield glass",new Color(0.05f,0.6f,1,0.12f),0,new Color(0,0.2f,0.4f));
            mat.SetFloat("_Surface",1);mat.SetFloat("_Blend",0);mat.SetFloat("_SrcBlend",(int)BlendMode.SrcAlpha);mat.SetFloat("_DstBlend",(int)BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite",0);mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");mat.renderQueue=3000;return mat;
        }
        private static AsteroidController[] CreateRocks()
        {
            var result=new AsteroidController[4];
            for(int i=0;i<result.Length;i++) {
                string path=Root+"/Prefabs/Asteroid"+i+".prefab";
                result[i]=LoadPrefab<AsteroidController>(path);if(result[i]!=null)continue;
                int seed=i+170;
                var root=new GameObject("Basalt asteroid "+i);root.layer=9;
                var body=root.AddComponent<Rigidbody>();body.useGravity=false;body.linearDamping=0;body.angularDamping=0;
                body.constraints=RigidbodyConstraints.FreezePositionY|RigidbodyConstraints.FreezeRotation;
                body.interpolation=RigidbodyInterpolation.Interpolate;root.AddComponent<SphereCollider>().radius=0.9f;
                var mesh=MeshAsset("Asteroid"+i,()=>ProceduralModels.Rock(seed));
                var visual=ProceduralModels.Part("Irregular rock",root.transform,mesh,rock,Vector3.zero,Vector3.one);
                root.AddComponent<AsteroidController>().Configure(visual.transform);
                result[i]=SavePrefab<AsteroidController>(root,path);
            }return result;
        }
        private static Projectile CreateProjectile(string name,Material mat)
        {
            string path=Root+"/Prefabs/"+name+".prefab";
            var existing=LoadPrefab<Projectile>(path);if(existing!=null)return existing;
            var root=new GameObject(name);var body=root.AddComponent<Rigidbody>();body.useGravity=false;body.isKinematic=true;
            root.AddComponent<SphereCollider>().isTrigger=true;root.GetComponent<SphereCollider>().radius=0.13f;
            ProceduralModels.Primitive("Laser core",PrimitiveType.Capsule,root.transform,mat,Vector3.zero,new Vector3(0.13f,0.6f,0.13f)).transform.localRotation=Quaternion.Euler(90,0,0);
            root.AddComponent<Projectile>();return SavePrefab<Projectile>(root,path);
        }
        private static ParticleSystem CreateBurst()
        {
            string path=Root+"/Prefabs/ImpactBurst.prefab";
            var existing=LoadPrefab<ParticleSystem>(path);if(existing!=null)return existing;
            var root=new GameObject("Reusable impact");var ps=root.AddComponent<ParticleSystem>();
            var main=ps.main;main.loop=false;main.playOnAwake=false;main.duration=0.5f;main.startLifetime=0.5f;main.startSpeed=4;main.startSize=0.15f;main.maxParticles=36;main.simulationSpace=ParticleSystemSimulationSpace.World;
            main.startColor=new ParticleSystem.MinMaxGradient(new Color(0.1f,0.85f,1),new Color(1,0.45f,0.1f));
            var emission=ps.emission;emission.rateOverTime=0;emission.SetBursts(new[]{new ParticleSystem.Burst(0,24)});
            var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=0.25f;
            ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=Material("Spark",new Color(1,0.65f,0.25f),0,new Color(2,0.9f,0.25f));
            return SavePrefab<ParticleSystem>(root,path);
        }
        private static void CreateScene(GameBalance balance,LevelDefinition[] levels,InputActionAsset actions,PlayerController playerPrefab,AsteroidController[] rocks,Projectile friendly,Projectile hostile,ParticleSystem burst,bool quick,string scenePath)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            // NewScene can unload unused prefab assets; resolve them after the scene boundary.
            playerPrefab=LoadPrefab<PlayerController>(Root+"/Prefabs/PlayerShip.prefab");
            friendly=LoadPrefab<Projectile>(Root+"/Prefabs/PlayerLaser.prefab");
            hostile=LoadPrefab<Projectile>(Root+"/Prefabs/EnemyLaser.prefab");
            burst=LoadPrefab<ParticleSystem>(Root+"/Prefabs/ImpactBurst.prefab");
            for(int i=0;i<rocks.Length;i++)rocks[i]=LoadPrefab<AsteroidController>(Root+"/Prefabs/Asteroid"+i+".prefab");
            if(playerPrefab==null||!EditorUtility.IsPersistent(playerPrefab))throw new InvalidOperationException("PlayerShip must resolve to a persistent prefab asset after scene creation.");
            balance=AssetDatabase.LoadAssetAtPath<GameBalance>(Root+"/Data/ArcadeBalance.asset");levels=CreateLevels();
            actions=AssetDatabase.LoadAssetAtPath<InputActionAsset>(Root+"/Input/NebulaControls.inputactions");
            var root=new GameObject("NEBULA DEFENDER / Composition root");
            var session=root.AddComponent<GameSession>();
            var input=root.AddComponent<PilotInput>();input.Configure(actions);
            var arena=root.AddComponent<ArenaBounds>();arena.Configure(balance);
            var feedback=new GameObject("Feedback pool").AddComponent<CombatFeedback>();feedback.Configure(burst);
            var pool=new GameObject("Projectile pool").AddComponent<ProjectilePool>();pool.Configure(friendly,hostile);
            var director=new GameObject("Combat director").AddComponent<CombatDirector>();director.Configure(rocks);
            director.ConfigureEnemies(LoadPrefab<EnemyController>(Root+"/Prefabs/InterceptorDrone.prefab"),LoadPrefab<BossController>(Root+"/Prefabs/NemesisPrime.prefab"),LoadPrefab<Pickup>(Root+"/Prefabs/RepairCell.prefab"),LoadPrefab<Pickup>(Root+"/Prefabs/DoubleLaserCell.prefab"));
            var playerObject=(GameObject)PrefabUtility.InstantiatePrefab(playerPrefab.gameObject);
            var player=playerObject.GetComponent<PlayerController>();
            if(player==null)throw new InvalidOperationException("The generated player prefab has no PlayerController.");
            session.Configure(balance,levels,player,input,arena,pool,director,feedback,quick);
            var backdrop=CreateEnvironment(balance,player);
            session.ConfigurePresentation(CreateAudio(),backdrop);
            CreateHUD(session);
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),scenePath);
        }
        private static SpaceBackdrop CreateEnvironment(GameBalance balance,PlayerController player)
        {
            var camera=new GameObject("Combat Camera",typeof(Camera),typeof(AudioListener));camera.tag="MainCamera";
            var view=camera.GetComponent<Camera>();view.fieldOfView=50;view.nearClipPlane=0.3f;view.farClipPlane=350;view.clearFlags=CameraClearFlags.SolidColor;view.backgroundColor=new Color(0.008f,0.018f,0.045f);view.allowHDR=true;
            camera.AddComponent<CameraRig>().Configure(balance);
            AddBloom(view);
            camera.transform.position=new Vector3(0,57,-20.52f);camera.transform.LookAt(Vector3.zero);
            var light=new GameObject("Starlight").AddComponent<Light>();light.type=LightType.Directional;light.color=new Color(0.65f,0.8f,1);light.intensity=2;light.transform.rotation=Quaternion.Euler(35,-25,0);light.shadows=LightShadows.Soft;
            var rim=new GameObject("Violet rim").AddComponent<Light>();rim.type=LightType.Directional;rim.color=new Color(0.65f,0.3f,1);rim.intensity=0.8f;rim.transform.rotation=Quaternion.Euler(155,100,0);
            RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(0.19f,0.22f,0.3f);RenderSettings.fog=false;RenderSettings.skybox=null;
            var scenery=new GameObject("Space / procedural offline scenery").transform;
            var worldMaterial=Material("Ocean world",new Color(0.03f,0.18f,0.38f),0.2f);worldMaterial.shader=Shader.Find("Nebula Defender/Planet");
            var planet=ProceduralModels.Primitive("Ocean planet",PrimitiveType.Sphere,scenery,worldMaterial,new Vector3(-29,-20,34),Vector3.one*23);
            ProceduralModels.Primitive("Moon",PrimitiveType.Sphere,scenery,rock,new Vector3(-12,-28,48),Vector3.one*5);
            var random=new System.Random(2651);
            // A single static mesh draws all stars; no per-star objects or frame allocations.
            var stars=new System.Collections.Generic.List<Vector3>();var triangles=new System.Collections.Generic.List<int>();
            for(int i=0;i<500;i++) {
                var p=new Vector3((float)random.NextDouble()*180-90,-35-(float)random.NextDouble()*20,(float)random.NextDouble()*180-70);
                float s=0.025f+(float)random.NextDouble()*0.12f;int n=stars.Count;
                stars.Add(p+new Vector3(-s,0,0));stars.Add(p+new Vector3(s,0,0));stars.Add(p+new Vector3(0,0,s*1.6f));triangles.AddRange(new[]{n,n+2,n+1});
            }
            var starMesh=new Mesh{name="Star field",vertices=stars.ToArray(),triangles=triangles.ToArray()};starMesh.RecalculateNormals();
            string starPath=Root+"/Meshes/Stars.asset";var stored=AssetDatabase.LoadAssetAtPath<Mesh>(starPath);if(stored==null){AssetDatabase.CreateAsset(starMesh,starPath);stored=starMesh;}else UnityEngine.Object.DestroyImmediate(starMesh);
            var starField=ProceduralModels.Part("Distant stars",scenery,stored,Material("Stars",new Color(0.6f,0.8f,1),0,new Color(0.7f,0.9f,1.2f)),Vector3.zero,Vector3.one);
            var boundary=Material("Boundary",new Color(0.035f,0.12f,0.2f),0,new Color(0.025f,0.14f,0.23f));
            for(int i=-26;i<=26;i+=2) foreach(float z in new[]{-balance.arenaHalfSize.y,balance.arenaHalfSize.y})
                ProceduralModels.Primitive("Wrap marker",PrimitiveType.Cube,scenery,boundary,new Vector3(i,-0.8f,z),new Vector3(0.8f,0.04f,0.08f));
            for(int i=-16;i<=16;i+=2) foreach(float x in new[]{-balance.arenaHalfSize.x,balance.arenaHalfSize.x})
                ProceduralModels.Primitive("Wrap marker",PrimitiveType.Cube,scenery,boundary,new Vector3(x,-0.8f,i),new Vector3(0.08f,0.04f,0.8f));
            var nebula=Material("Nebula cloud",new Color(0.007f,0.014f,0.035f),0);nebula.shader=Shader.Find("Nebula Defender/Nebula");
            var cloud=ProceduralModels.Primitive("Nebula backdrop",PrimitiveType.Quad,scenery,nebula,new Vector3(0,-65,15),new Vector3(330,240,1));cloud.transform.rotation=Quaternion.Euler(90,0,0);
            var station=CreateFortress(scenery);
            var model=player.transform.GetChild(0).gameObject;
            var preview=UnityEngine.Object.Instantiate(model,scenery);preview.name="Aurora / menu display";preview.transform.position=new Vector3(11,0,-1);preview.transform.localScale=Vector3.one*3.5f;
            var backdrop=scenery.gameObject.AddComponent<SpaceBackdrop>();backdrop.Configure(planet,station,preview,starField.transform,nebula);backdrop.ShowSector(1);backdrop.MenuVisible(false);return backdrop;
        }
        private static void CreateHUD(GameSession session)
        {
            var canvas=new GameObject("HUD",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=0.5f;
            canvas.AddComponent<ArcadeUI>().Configure(session,EnsureFont());
            new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
        }
    }
}
