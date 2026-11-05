using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace NebulaDefender.Editor
{
    public static partial class DemoGenerator
    {
        private static LevelDefinition[] CreateLevels()
        {
            var levels=new LevelDefinition[3];
            levels[0]=GetOrCreate<LevelDefinition>(Root+"/Data/Sector01.asset");
            for(int i=1;i<3;i++)
            {
                string path=Root+"/Data/Sector0"+(i+1)+".asset";
                levels[i]=AssetDatabase.LoadAssetAtPath<LevelDefinition>(path);if(levels[i]!=null)continue;
                var data=ScriptableObject.CreateInstance<LevelDefinition>();data.sector=i+1;data.seed=7319+i*113;
                data.displayName=i==1?"CINTURÓN DE ASTEROIDES":"FORTALEZA NÉMESIS";
                data.briefing=i==1?"Asegura el cinturón. Destruye doce asteroides y seis drones.":"Elimina cuatro drones de defensa y derrota a Nemesis Prime.";
                data.asteroidGoal=i==1?12:0;data.droneGoal=i==1?6:4;data.boss=i==2;
                data.initialRocks=i==1?7:4;data.simultaneousDrones=2;data.asteroidSpeed=i==1?2.8f:3.2f;
                data.enemySpeed=i==1?4.6f:5.2f;data.enemyFireInterval=i==1?2.8f:2.2f;data.enemyHealth=i==1?60:75;
                data.spaceColor=i==1?new Color(0.035f,0.015f,0.075f):new Color(0.025f,0.009f,0.025f);
                data.accent=i==1?new Color(0.65f,0.35f,1):new Color(1,0.25f,0.1f);
                AssetDatabase.CreateAsset(data,path);levels[i]=data;
            }return levels;
        }
        private static EnemyController CreateDrone()
        {
            string path=Root+"/Prefabs/InterceptorDrone.prefab";var existing=LoadPrefab<EnemyController>(path);if(existing!=null)return existing;
            var root=new GameObject("MARAUDER / attack drone");root.layer=10;
            var body=root.AddComponent<Rigidbody>();body.useGravity=false;body.linearDamping=0.8f;
            body.constraints=RigidbodyConstraints.FreezePositionY|RigidbodyConstraints.FreezeRotationX|RigidbodyConstraints.FreezeRotationZ;body.interpolation=RigidbodyInterpolation.Interpolate;
            root.AddComponent<SphereCollider>().radius=1.05f;
            var visual=new GameObject("Visual / replaceable").transform;visual.SetParent(root.transform,false);
            ProceduralModels.Part("Dark fuselage",visual,wedge,dark,Vector3.zero,new Vector3(1,0.7f,0.65f));
            ProceduralModels.Part("Burnt orange armor",visual,wing,Material("Hostile armor",new Color(0.55f,0.12f,0.035f),0.5f),new Vector3(0,0,-0.2f),new Vector3(0.8f,1,1));
            ProceduralModels.Primitive("Red targeting eye",PrimitiveType.Sphere,visual,orange,new Vector3(0,0.35f,0.45f),Vector3.one*0.45f);
            foreach(float x in new[]{-0.8f,0.8f})ProceduralModels.Primitive("Thruster",PrimitiveType.Sphere,visual,orange,new Vector3(x,0,-1),new Vector3(0.2f,0.2f,0.6f));
            root.AddComponent<EnemyController>();return SavePrefab<EnemyController>(root,path);
        }
        private static BossController CreateBoss()
        {
            string path=Root+"/Prefabs/NemesisPrime.prefab";var existing=LoadPrefab<BossController>(path);if(existing!=null)return existing;
            var root=new GameObject("NEMESIS PRIME / flagship");root.layer=10;
            var body=root.AddComponent<Rigidbody>();body.useGravity=false;body.isKinematic=true;
            root.AddComponent<BoxCollider>().size=new Vector3(10,2,5);
            var visual=new GameObject("Flagship model").transform;visual.SetParent(root.transform,false);
            ProceduralModels.Part("Command hull",visual,wedge,dark,Vector3.zero,new Vector3(3,2.8f,2.2f)).transform.localRotation=Quaternion.Euler(0,180,0);
            ProceduralModels.Part("Armored wings",visual,wing,dark,new Vector3(0,-0.2f,0),new Vector3(3.5f,4,2)).transform.localRotation=Quaternion.Euler(0,180,0);
            var core=ProceduralModels.Primitive("Reactor core",PrimitiveType.Sphere,visual,orange,new Vector3(0,0.7f,-0.3f),Vector3.one*1.3f);
            foreach(float x in new[]{-4f,-2.5f,2.5f,4f})
            {
                ProceduralModels.Primitive("Turret",PrimitiveType.Cylinder,visual,blue,new Vector3(x,0.4f,-0.3f),new Vector3(0.85f,0.5f,0.85f));
                ProceduralModels.Primitive("Energy cannon",PrimitiveType.Cube,visual,orange,new Vector3(x,0.5f,-1.2f),new Vector3(0.24f,0.28f,1.3f));
                ProceduralModels.Primitive("Drive",PrimitiveType.Sphere,visual,orange,new Vector3(x,0,1.7f),new Vector3(0.4f,0.35f,1));
            }
            root.AddComponent<BossController>().Configure(core.transform);return SavePrefab<BossController>(root,path);
        }
        private static Pickup CreatePickup(bool repair)
        {
            string path=Root+"/Prefabs/"+(repair?"RepairCell":"DoubleLaserCell")+".prefab";
            var existing=LoadPrefab<Pickup>(path);if(existing!=null)return existing;
            var root=new GameObject(repair?"REPAIR +30":"DOUBLE LASER");root.layer=13;
            root.AddComponent<SphereCollider>().isTrigger=true;root.GetComponent<SphereCollider>().radius=1.1f;
            var visual=new GameObject("Pickup model").transform;visual.SetParent(root.transform,false);
            var mat=repair?Material("Repair green",new Color(0.1f,1,0.5f),0,new Color(0.05f,1,0.3f)):cyan;
            ProceduralModels.Primitive("Capsule",PrimitiveType.Cube,visual,dark,Vector3.zero,Vector3.one*0.85f);
            if(repair)
            {
                ProceduralModels.Primitive("Cross",PrimitiveType.Cube,visual,mat,new Vector3(0,0.5f,0),new Vector3(0.65f,0.08f,0.2f));
                ProceduralModels.Primitive("Cross",PrimitiveType.Cube,visual,mat,new Vector3(0,0.5f,0),new Vector3(0.2f,0.08f,0.65f));
            }
            else foreach(float x in new[]{-0.2f,0.2f})ProceduralModels.Primitive("Twin laser",PrimitiveType.Cube,visual,mat,new Vector3(x,0.5f,0),new Vector3(0.1f,0.08f,0.65f));
            root.AddComponent<Pickup>().Configure(visual);return SavePrefab<Pickup>(root,path);
        }
        private static TMP_FontAsset EnsureFont()
        {
            const string path="Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
            var asset=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);if(asset!=null)return asset;
            string package=UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(TMP_Text).Assembly).resolvedPath;
            string essentials=Path.Combine(package,"Package Resources","TMP Essential Resources.unitypackage");
            if(!File.Exists(essentials))throw new FileNotFoundException("Missing bundled TextMeshPro essentials",essentials);
            AssetDatabase.ImportPackage(essentials,false);
            asset=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if(asset==null)throw new InvalidOperationException("TextMeshPro essential font import did not finish.");return asset;
        }
        private static AudioService CreateAudio()
        {
            var service=new GameObject("Audio / local procedural soundtrack").AddComponent<AudioService>();
            var music=new AudioClip[4];for(int i=0;i<music.Length;i++)music[i]=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Nebula/Audio/Music/Sector"+i+".wav");
            string[] names={"Laser","HostileLaser","Impact","Explosion","Shield","Select","Confirm","Complete","Victory","Defeat"};
            var effects=new AudioClip[names.Length];for(int i=0;i<names.Length;i++)effects[i]=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Nebula/Audio/SFX/"+names[i]+".wav");
            service.Configure(music,effects,AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Nebula/Audio/SFX/Engine.wav"));return service;
        }
        private static GameObject CreateFortress(Transform parent)
        {
            var root=new GameObject("NEMESIS / orbital fortress");root.transform.SetParent(parent,false);root.transform.position=new Vector3(0,-12,38);
            ProceduralModels.Primitive("Central bastion",PrimitiveType.Cylinder,root.transform,dark,Vector3.zero,new Vector3(13,2.5f,13));
            ProceduralModels.Primitive("Reactor",PrimitiveType.Cylinder,root.transform,orange,new Vector3(0,3,0),new Vector3(3,1,3));
            for(int i=0;i<8;i++)
            {
                float a=i*Mathf.PI/4;var p=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*12;
                ProceduralModels.Primitive("Station arm",PrimitiveType.Cube,root.transform,dark,p*0.6f,new Vector3(2,2,14)).transform.localRotation=Quaternion.Euler(0,-a*Mathf.Rad2Deg+90,0);
                ProceduralModels.Primitive("Defense pylon",PrimitiveType.Cylinder,root.transform,blue,p,new Vector3(3,3,3));
                ProceduralModels.Primitive("Energy beacon",PrimitiveType.Sphere,root.transform,orange,p+Vector3.up*3,Vector3.one*1.2f);
            }return root;
        }
        private static void AddBloom(Camera camera)
        {
            camera.GetUniversalAdditionalCameraData().renderPostProcessing=true;
            var volume=new GameObject("Subtle ion bloom").AddComponent<Volume>();volume.isGlobal=true;
            string path=Root+"/Data/SpaceVolume.asset";
            var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if(profile==null)
            {
                profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,path);
                var bloom=profile.Add<Bloom>(true);bloom.intensity.Override(0.45f);bloom.threshold.Override(1.1f);bloom.scatter.Override(0.45f);
                AssetDatabase.AddObjectToAsset(bloom,profile);EditorUtility.SetDirty(profile);
            }
            volume.sharedProfile=profile;
        }
    }
}
