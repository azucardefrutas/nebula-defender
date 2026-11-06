using UnityEngine;

namespace NebulaDefender
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private GameBalance balance;
        [SerializeField] private Transform engineGlow;
        private Rigidbody body;
        private GameSession session;
        private PilotInput input;
        private ArenaBounds arena;
        private float throttle, turn;
        private Transform visualRoot;
        private Quaternion visualRest;
        private Camera flightCamera;
        public bool PointerAimValid { get; private set; }
        public Vector3 PointerAim { get; private set; }
        public Rigidbody Body => body;
        public void Configure(GameBalance value, Transform engine) { balance = value; engineGlow = engine; }
        public void Initialize(GameSession owner, PilotInput pilotInput, ArenaBounds bounds)
        {
            session = owner; input = pilotInput; arena = bounds; body = GetComponent<Rigidbody>();
            flightCamera = Camera.main;
            if (engineGlow != null) { visualRoot = engineGlow.parent; visualRest = visualRoot.localRotation; }
            var presentation = GetComponent<ShipPresentation>();
            if (presentation == null) presentation = gameObject.AddComponent<ShipPresentation>();
            presentation.Build(visualRoot);
        }
        private void Update()
        {
            bool active = session != null && session.IsPlaying;
            throttle = active ? input.Throttle : 0; turn = active ? input.Turn : 0;
            PointerAimValid = false;
            if (active && input.MouseSteering && Mathf.Abs(input.Turn) < 0.1f)
            {
                var ray = flightCamera.ScreenPointToRay(input.PointerPosition);
                if (new Plane(Vector3.up, Vector3.zero).Raycast(ray, out float distance))
                {
                    PointerAim = ray.GetPoint(distance); var direction = PointerAim - body.position;
                    direction.y = 0; PointerAimValid = direction.sqrMagnitude > 0.36f;
                    if (PointerAimValid) turn = Mathf.Clamp(Vector3.SignedAngle(transform.forward, direction, Vector3.up) / 24, -1, 1);
                }
            }
            if (engineGlow != null) engineGlow.localScale = new Vector3(1, 1, active && throttle > 0 ? 1.8f + Mathf.Sin(Time.time * 38) * 0.12f : 0.45f);
            if (visualRoot != null) visualRoot.localRotation = Quaternion.Slerp(visualRoot.localRotation, visualRest * Quaternion.Euler(0, 0, -turn * 14), Time.deltaTime * 9);
        }
        private void FixedUpdate()
        {
            if (session == null || !session.IsPlaying) return;
            float turnMultiplier = session.TurnSensitivity;
            body.MoveRotation(body.rotation * Quaternion.Euler(0, turn * balance.turnSpeed * turnMultiplier * Time.fixedDeltaTime, 0));
            body.AddForce(transform.forward * (throttle >= 0 ? balance.acceleration : balance.reverseAcceleration) * throttle, ForceMode.Acceleration);
            // Mouse flight brakes gently when the engine is released; keyboard inertia stays available.
            if (input.MouseSteering && Mathf.Abs(throttle) < 0.01f) body.linearVelocity *= Mathf.Exp(-2.1f * Time.fixedDeltaTime);
            body.linearVelocity = Vector3.ClampMagnitude(body.linearVelocity, balance.maxSpeed);
            arena.Contain(body);
        }
        public void ResetPosition(bool resetToOrigin = true)
        {
            if (resetToOrigin) { body.position = Vector3.zero; body.rotation = Quaternion.identity; }
            body.linearVelocity = body.angularVelocity = Vector3.zero;
        }
    }
}
