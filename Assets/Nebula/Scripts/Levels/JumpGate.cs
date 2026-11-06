using UnityEngine;

namespace NebulaDefender
{
    [RequireComponent(typeof(SphereCollider),typeof(Rigidbody))]
    public sealed class JumpGate : MonoBehaviour
    {
        private GameSession session;
        private SphereCollider entry;
        private Transform model, iris;
        private Mesh ringMesh;
        private Material metal, energy, membrane;
        public bool Ready { get; private set; }
        public void Initialize(GameSession owner)
        {
            session=owner; gameObject.layer=13;
            var body=GetComponent<Rigidbody>(); body.isKinematic=true; body.useGravity=false;
            entry=GetComponent<SphereCollider>(); entry.isTrigger=true; entry.radius=3.1f; entry.enabled=false;
            metal=new Material(Shader.Find("Universal Render Pipeline/Lit"));
            metal.SetColor("_BaseColor",new Color(.07f,.15f,.22f)); metal.SetFloat("_Metallic",.8f); metal.SetFloat("_Smoothness",.65f);
            energy=new Material(Shader.Find("Universal Render Pipeline/Lit"));
            energy.SetColor("_BaseColor",new Color(.05f,.8f,1)); energy.SetColor("_EmissionColor",new Color(.05f,2,3)); energy.EnableKeyword("_EMISSION");
            membrane=new Material(Resources.Load<Shader>("ShieldRim"));
            model=new GameObject("Active jump portal").transform; model.SetParent(transform,false);
            ringMesh=CreateRing();
            var rim=new GameObject("Titanium ring",typeof(MeshFilter),typeof(MeshRenderer)); rim.transform.SetParent(model,false);
            rim.GetComponent<MeshFilter>().sharedMesh=ringMesh; rim.GetComponent<MeshRenderer>().sharedMaterial=metal;
            iris=Part("Energy iris",PrimitiveType.Sphere,Vector3.zero,new Vector3(6.8f,6.8f,.18f),membrane);
            for(int i=0;i<12;i++)
            {
                float angle=i*Mathf.PI/6; var p=new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),0)*3.85f;
                var node=Part("Jump coil",PrimitiveType.Cube,p,new Vector3(.28f,.75f,.5f),energy);
                node.localRotation=Quaternion.Euler(0,0,angle*Mathf.Rad2Deg-90);
            }
            Close();
        }
        private Transform Part(string name,PrimitiveType type,Vector3 position,Vector3 scale,Material material)
        {
            var go=GameObject.CreatePrimitive(type); go.name=name; go.transform.SetParent(model,false);
            go.transform.localPosition=position; go.transform.localScale=scale;
            var collider=go.GetComponent<Collider>(); collider.enabled=false; Destroy(collider);
            go.GetComponent<Renderer>().sharedMaterial=material; return go.transform;
        }
        private static Mesh CreateRing()
        {
            const int segments=64, sides=8;
            var vertices=new Vector3[segments*sides]; var indices=new int[segments*sides*6];
            for(int i=0;i<segments;i++)for(int j=0;j<sides;j++)
            {
                float a=i*Mathf.PI*2/segments,b=j*Mathf.PI*2/sides;
                vertices[i*sides+j]=new Vector3(Mathf.Cos(a),Mathf.Sin(a),0)*(3.85f+Mathf.Cos(b)*.32f)+Vector3.forward*Mathf.Sin(b)*.32f;
                int n=(i*sides+j)*6, x=i*sides+j,y=((i+1)%segments)*sides+j,z=i*sides+(j+1)%sides,w=((i+1)%segments)*sides+(j+1)%sides;
                indices[n]=x;indices[n+1]=z;indices[n+2]=y;indices[n+3]=y;indices[n+4]=z;indices[n+5]=w;
            }
            var mesh=new Mesh {name="Jump ring / original procedural mesh",vertices=vertices,triangles=indices};
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }
        public void Open(Vector3 position,Vector3 heading)
        {
            transform.SetPositionAndRotation(position,Quaternion.LookRotation(-heading));
            model.localScale=Vector3.one*.2f; model.gameObject.SetActive(true);
            Ready=true; entry.enabled=true;
        }
        public void Close()
        {
            Ready=false;
            if(entry!=null)entry.enabled=false;
            if(model!=null)model.gameObject.SetActive(false);
        }
        private void Update()
        {
            if(!Ready)return;
            model.localScale=Vector3.Lerp(model.localScale,Vector3.one,1-Mathf.Exp(-5*Time.deltaTime));
            model.Rotate(0,0,8*Time.deltaTime,Space.Self);
            iris.localScale=new Vector3(6.8f,6.8f,.18f)*(1+Mathf.Sin(Time.time*3)*.025f);
        }
        private void OnTriggerEnter(Collider other) { if(other.gameObject.layer==8)session.TryExtract(); }
        private void OnTriggerStay(Collider other) { if(other.gameObject.layer==8)session.TryExtract(); }
        private void OnDestroy()
        {
            if(ringMesh!=null)Destroy(ringMesh);
            if(metal!=null)Destroy(metal); if(energy!=null)Destroy(energy); if(membrane!=null)Destroy(membrane);
        }
    }
}
