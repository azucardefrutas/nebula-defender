using UnityEngine;

namespace NebulaDefender
{
    public sealed class SpaceBackdrop : MonoBehaviour
    {
        [SerializeField] private GameObject planet, fortress, menuShip;
        [SerializeField] private Transform stars;
        [SerializeField] private Material nebulaMaterial;
        private int sector;
        private Material sky, debrisMaterial;
        private Cubemap reflections;
        private Transform[] depthDebris;
        private void Start()
        {
            // Hide legacy demo markers without rewriting existing scenes or custom art.
            foreach (var renderer in GetComponentsInChildren<MeshRenderer>())
                if (renderer.gameObject.name == "Wrap marker") renderer.enabled = false;
            var shader = Resources.Load<Shader>("NebulaSky");
            if (shader != null)
            {
                sky = new Material(shader); RenderSettings.skybox = sky;
                Camera.main.clearFlags = CameraClearFlags.Skybox;
                if (stars != null) stars.gameObject.SetActive(false);
                foreach (var renderer in GetComponentsInChildren<MeshRenderer>())
                    if (renderer.sharedMaterial == nebulaMaterial) renderer.enabled = false;
            }
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.16f, 0.22f, 0.34f);
            RenderSettings.ambientEquatorColor = new Color(0.065f, 0.09f, 0.14f);
            RenderSettings.ambientGroundColor = new Color(0.015f, 0.025f, 0.045f);
            CreateReflections(); CreateDepthDebris();
        }
        private void CreateReflections()
        {
            const int size = 32;
            reflections = new Cubemap(size, TextureFormat.RGBA32, true);
            var pixels = new Color[size * size];
            var key = new Vector3(-0.5f, 0.7f, -0.4f).normalized;
            for (int face = 0; face < 6; face++)
            {
                for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
                {
                    float u = 2f * x / (size - 1) - 1, v = 2f * y / (size - 1) - 1;
                    var d = face == 0 ? new Vector3(1, v, -u) : face == 1 ? new Vector3(-1, v, u) : face == 2 ? new Vector3(u, 1, -v) : face == 3 ? new Vector3(u, -1, v) : face == 4 ? new Vector3(u, v, 1) : new Vector3(-u, v, -1);
                    d.Normalize();
                    pixels[y * size + x] = Color.Lerp(new Color(.025f, .04f, .08f), new Color(.28f, .4f, .58f), d.y * .5f + .5f) + new Color(.75f, .7f, .55f) * Mathf.Pow(Mathf.Max(0, Vector3.Dot(d, key)), 24);
                }
                reflections.SetPixels(pixels, (CubemapFace)face);
            }
            reflections.Apply(true, false);
            RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Custom;
            RenderSettings.customReflectionTexture = reflections; RenderSettings.reflectionIntensity = .85f;
        }
        private void CreateDepthDebris()
        {
            debrisMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            debrisMaterial.SetColor("_BaseColor", new Color(.16f, .23f, .3f));
            debrisMaterial.SetFloat("_Metallic", .65f); debrisMaterial.SetFloat("_Smoothness", .55f);
            depthDebris = new Transform[8];
            for (int i = 0; i < depthDebris.Length; i++)
            {
                var root = new GameObject("Orbital wreckage / decorative").transform; root.SetParent(transform, false);
                root.localPosition = new Vector3((i % 2 == 0 ? -1 : 1) * (12 + i * 1.7f), -5 - i % 3 * 2, -17 + i * 4.8f);
                root.localRotation = Quaternion.Euler(20 + i * 13, i * 47, 25 - i * 6); depthDebris[i] = root;
                for (int beam = 0; beam < 3; beam++)
                {
                    var piece = GameObject.CreatePrimitive(PrimitiveType.Cube); piece.name = "Hull fragment";
                    piece.transform.SetParent(root, false); piece.transform.localPosition = new Vector3(beam * .5f, 0, 0);
                    piece.transform.localScale = beam == 0 ? new Vector3(2.4f, .25f, .35f) : new Vector3(.18f, .45f, 2.1f);
                    piece.GetComponent<Renderer>().sharedMaterial = debrisMaterial; Destroy(piece.GetComponent<Collider>());
                }
            }
        }
        public void Configure(GameObject world, GameObject station, GameObject preview, Transform starField, Material nebula)
        { planet = world; fortress = station; menuShip = preview; stars = starField; nebulaMaterial = nebula; }
        public void ShowSector(int value)
        {
            sector = value;
            if (planet != null) planet.SetActive(value <= 1);
            if (fortress != null) fortress.SetActive(value == 3);
            if (nebulaMaterial != null) Shader.SetGlobalColor("_NebulaTint", value == 2 ? new Color(0.32f,0.09f,0.5f) : value == 3 ? new Color(0.22f,0.045f,0.12f) : new Color(0.055f,0.2f,0.35f));
        }
        public void MenuVisible(bool value) { if (menuShip != null) menuShip.SetActive(value); }
        private void Update()
        {
            if (menuShip != null && menuShip.activeSelf) menuShip.transform.localRotation = Quaternion.Euler(-8, -25 + Mathf.Sin(Time.unscaledTime * 0.22f) * 12, -5);
            if (planet != null && planet.activeSelf) planet.transform.Rotate(0, Time.deltaTime * 0.5f, 0);
            if (stars != null) stars.localPosition = new Vector3(Mathf.Sin(Time.time * 0.018f) * 0.8f, 0, 0);
            if (depthDebris != null) foreach (var debris in depthDebris) debris.Rotate(2 * Time.deltaTime, Time.deltaTime, 0);
        }
        private void OnDestroy()
        {
            if (RenderSettings.skybox == sky) RenderSettings.skybox = null;
            if (RenderSettings.customReflectionTexture == reflections) RenderSettings.customReflectionTexture = null;
            if (sky != null) Destroy(sky); if (reflections != null) Destroy(reflections); if (debrisMaterial != null) Destroy(debrisMaterial);
        }
    }
}
