using System.Collections.Generic;
using UnityEngine;

namespace NebulaDefender.Editor
{
    internal static class ProceduralModels
    {
        public static Mesh Wedge(float width, float length, float height, float rear = 0.6f)
        {
            Vector3[] vertices = {
                new Vector3(0, height * 0.25f, length * 0.65f), new Vector3(-width, 0, -length * rear),
                new Vector3(width, 0, -length * rear), new Vector3(0, height, -length * 0.25f),
                new Vector3(0, -height * 0.6f, -length * 0.2f)
            };
            return Flat(vertices, new [] {0,3,1, 0,2,3, 1,3,2, 0,1,4, 0,4,2, 1,2,4});
        }
        public static Mesh Rock(int seed)
        {
            var random = new System.Random(seed);
            float t = (1 + Mathf.Sqrt(5)) / 2;
            var v = new List<Vector3> {
                new Vector3(-1,t,0),new Vector3(1,t,0),new Vector3(-1,-t,0),new Vector3(1,-t,0),
                new Vector3(0,-1,t),new Vector3(0,1,t),new Vector3(0,-1,-t),new Vector3(0,1,-t),
                new Vector3(t,0,-1),new Vector3(t,0,1),new Vector3(-t,0,-1),new Vector3(-t,0,1)
            };
            int[] faces = {0,11,5,0,5,1,0,1,7,0,7,10,0,10,11,1,5,9,5,11,4,11,10,2,10,7,6,7,1,8,3,9,4,3,4,2,3,2,6,3,6,8,3,8,9,4,9,5,2,4,11,6,2,10,8,6,7,9,8,1};
            var subdivided = new List<int>();
            var midpoints = new Dictionary<long,int>();
            int Mid(int a, int b) {
                long key = ((long)Mathf.Min(a,b)<<32) | (uint)Mathf.Max(a,b);
                if (midpoints.TryGetValue(key, out int index)) return index;
                index=v.Count; v.Add((v[a]+v[b]).normalized); midpoints[key]=index; return index;
            }
            for (int i=0;i<v.Count;i++) v[i]=v[i].normalized;
            for (int i=0;i<faces.Length;i+=3) {
                int a=faces[i],b=faces[i+1],c=faces[i+2],ab=Mid(a,b),bc=Mid(b,c),ca=Mid(c,a);
                subdivided.AddRange(new[]{a,ab,ca,b,bc,ab,c,ca,bc,ab,bc,ca});
            }
            var scale = new Vector3(1, 0.8f + (float)random.NextDouble() * 0.3f, 0.85f + (float)random.NextDouble()*0.3f);
            for(int i=0;i<v.Count;i++) v[i]=Vector3.Scale(v[i],scale)*(0.78f+(float)random.NextDouble()*0.28f);
            return Flat(v.ToArray(),subdivided.ToArray());
        }
        private static Mesh Flat(Vector3[] vertices, int[] triangles)
        {
            var flat = new Vector3[triangles.Length]; var indices = new int[triangles.Length];
            for(int i=0;i<triangles.Length;i++){ flat[i]=vertices[triangles[i]];indices[i]=i; }
            var mesh = new Mesh { name="Nebula low-poly mesh",vertices=flat,triangles=indices};
            mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
        public static GameObject Part(string name, Transform parent, Mesh mesh, Material material, Vector3 position, Vector3 scale)
        {
            var go = new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));
            go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=scale;
            go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=material;return go;
        }
        public static GameObject Primitive(string name, PrimitiveType type, Transform parent, Material mat, Vector3 p, Vector3 scale)
        {
            var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent,false);
            go.transform.localPosition=p;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=mat;
            Object.DestroyImmediate(go.GetComponent<Collider>());return go;
        }
    }
}
