using UnityEngine;

namespace NebulaDefender
{
    // Optional detail layer: custom imported models are left untouched.
    public sealed class ShipPresentation : MonoBehaviour
    {
        private TrailRenderer[] trails;
        public void SetThrust(bool value)
        { if(trails!=null)foreach(var trail in trails)trail.emitting=value; }
        public void ClearTrails()
        { if(trails!=null)foreach(var trail in trails)trail.Clear(); }
        public void Build(Transform visual)
        {
            if (visual == null || visual.Find("Titanium fuselage") == null || visual.Find("Aurora details") != null) return;
            Material metal = null, dark = null, glow = null, blue = null;
            foreach (var r in visual.GetComponentsInChildren<MeshRenderer>())
            {
                var m = r.sharedMaterial; if (m == null) continue;
                if (m.name == "Pearl titanium") metal = m;
                if (m.name == "Carbon") dark = m;
                if (m.name == "Ion cyan") glow = m;
                if (m.name == "Cobalt armor") blue = m;
            }
            var root = new GameObject("Aurora details").transform; root.SetParent(visual,false);
            trails=new TrailRenderer[2]; int trailIndex=0;
            void Part(string name, Vector3 position, Vector3 scale, Material material, Vector3 rotation)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = name;
                go.transform.SetParent(root,false); go.transform.localPosition = position;
                go.transform.localScale = scale; go.transform.localRotation = Quaternion.Euler(rotation);
                go.GetComponent<Collider>().enabled = false; Destroy(go.GetComponent<Collider>());
                go.GetComponent<MeshRenderer>().sharedMaterial = material;
            }
            foreach (float side in new[] { -1f, 1f })
            {
                var nozzle=new GameObject("Ion trail").transform; nozzle.SetParent(root,false);
                nozzle.localPosition=new Vector3(side*.75f,0,-1.7f);
                var trail=nozzle.gameObject.AddComponent<TrailRenderer>();
                trail.sharedMaterial=glow; trail.time=.18f; trail.startWidth=.14f; trail.endWidth=0;
                trail.minVertexDistance=.08f; trail.emitting=false;
                trail.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off; trail.receiveShadows=false;
                trails[trailIndex++]=trail;
                Part("Swept stabilizer",new Vector3(side*.64f,.48f,-.9f),new Vector3(.075f,.8f,.65f),blue,new Vector3(0,0,side*-24));
                Part("Rail cannon",new Vector3(side*1.04f,.05f,.1f),new Vector3(.15f,.17f,1.55f),dark,Vector3.zero);
                Part("Cannon stripe",new Vector3(side*1.04f,.15f,.3f),new Vector3(.055f,.025f,.65f),glow,Vector3.zero);
                Part("Wing armor",new Vector3(side*1.18f,.02f,-.74f),new Vector3(.7f,.075f,.42f),metal,new Vector3(0,side*-27,0));
                Part("Wing light",new Vector3(side*1.32f,.07f,-.78f),new Vector3(.52f,.025f,.035f),glow,new Vector3(0,side*-27,0));
                Part("Engine brace",new Vector3(side*.75f,.18f,-.94f),new Vector3(.34f,.1f,.7f),metal,Vector3.zero);
            }
        }
    }
}
