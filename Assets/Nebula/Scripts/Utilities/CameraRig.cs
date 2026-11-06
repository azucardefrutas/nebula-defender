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
        private PlayerController player;
        private GameSession session;
        private Vector3 lastPlayerPosition;
        private bool wasInFlight;
        public bool TacticalView { get; private set; }
        public void Configure(GameBalance b) => balance = b;
        public void Bind(PlayerController ship, GameSession owner) { player = ship; session = owner; }
        public void ToggleView() { TacticalView = !TacticalView; if (TacticalView) Fit(); wasInFlight = false; }
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
            if (!Mathf.Approximately(aspect, view.aspect)) { Fit(); wasInFlight = false; }
            bool inFlight = session != null && session.State != SessionState.Menu && player != null;
            if (inFlight && !TacticalView)
            {
                var p = player.Body.position;
                var forward = player.transform.forward;
                var destination = p - forward * 16 + Vector3.up * 10;
                var focus = p + forward * 6 + Vector3.up * 0.4f;
                bool snap = !wasInFlight || Vector3.Distance(p, lastPlayerPosition) > 18;
                float blend = snap ? 1 : 1 - Mathf.Exp(-7 * Time.deltaTime);
                stablePosition = Vector3.Lerp(stablePosition, destination, blend);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(focus - stablePosition), blend);
                lastPlayerPosition = p;
            }
            else
            {
                if (wasInFlight) Fit();
            }
            wasInFlight = inFlight && !TacticalView;
            shake = Mathf.MoveTowards(shake, 0, Time.unscaledDeltaTime * 2);
            transform.position = stablePosition + (ShakeEnabled ? new Vector3(Mathf.Sin(Time.unscaledTime * 83), 0, Mathf.Cos(Time.unscaledTime * 71)) * shake * 0.12f : Vector3.zero);
        }
        public void Shake(float strength) => shake = Mathf.Max(shake, Mathf.Min(strength, 1));
    }
}
