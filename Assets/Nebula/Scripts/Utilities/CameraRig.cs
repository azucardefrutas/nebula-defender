using UnityEngine;

namespace NebulaDefender
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraRig : MonoBehaviour
    {
        [SerializeField] private GameBalance balance;
        private Camera view;
        private float aspect;
        public bool ShakeEnabled { get; set; } = true;
        private float shake;
        private Vector3 stablePosition;
        public void Configure(GameBalance b) => balance = b;
        private void Awake() { view = GetComponent<Camera>(); Fit(); }
        private void Fit()
        {
            aspect = view.aspect;
            var direction = new Vector3(0, 1, -0.36f).normalized;
            float halfVertical = Mathf.Max(balance.arenaHalfSize.y + 5, (balance.arenaHalfSize.x + 4) / Mathf.Max(0.5f, aspect));
            float distance = halfVertical / Mathf.Tan(view.fieldOfView * Mathf.Deg2Rad * 0.5f) + 9;
            stablePosition = direction * distance;
            transform.SetPositionAndRotation(stablePosition, Quaternion.LookRotation(-direction));
        }
        private void LateUpdate()
        {
            if (!Mathf.Approximately(aspect, view.aspect)) Fit();
            shake = Mathf.MoveTowards(shake, 0, Time.unscaledDeltaTime * 2);
            transform.position = stablePosition + (ShakeEnabled ? new Vector3(Mathf.Sin(Time.unscaledTime * 83), 0, Mathf.Cos(Time.unscaledTime * 71)) * shake * 0.12f : Vector3.zero);
        }
        public void Shake(float strength) => shake = Mathf.Max(shake, Mathf.Min(strength, 1));
    }
}
