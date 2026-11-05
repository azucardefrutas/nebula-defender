using UnityEngine;

namespace NebulaDefender
{
    public sealed class SpaceBackdrop : MonoBehaviour
    {
        [SerializeField] private GameObject planet, fortress, menuShip;
        [SerializeField] private Transform stars;
        [SerializeField] private Material nebulaMaterial;
        private int sector;
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
        }
    }
}
