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
        public Rigidbody Body => body;
        public void Configure(GameBalance value, Transform engine) { balance = value; engineGlow = engine; }
        public void Initialize(GameSession owner, PilotInput pilotInput, ArenaBounds bounds)
        { session = owner; input = pilotInput; arena = bounds; body = GetComponent<Rigidbody>(); }
        private void Update()
        {
            bool active = session != null && session.IsPlaying;
            throttle = active ? input.Throttle : 0; turn = active ? input.Turn : 0;
            if (engineGlow != null) engineGlow.localScale = new Vector3(1, 1, active && throttle > 0 ? 1.8f : 0.45f);
        }
        private void FixedUpdate()
        {
            if (session == null || !session.IsPlaying) return;
            float turnMultiplier = session.TurnSensitivity;
            body.MoveRotation(body.rotation * Quaternion.Euler(0, turn * balance.turnSpeed * turnMultiplier * Time.fixedDeltaTime, 0));
            body.AddForce(transform.forward * (throttle >= 0 ? balance.acceleration : balance.reverseAcceleration) * throttle, ForceMode.Acceleration);
            body.linearVelocity = Vector3.ClampMagnitude(body.linearVelocity, balance.maxSpeed);
            arena.Contain(body);
        }
        public void ResetPosition()
        {
            body.position = Vector3.zero; body.rotation = Quaternion.identity;
            body.linearVelocity = body.angularVelocity = Vector3.zero;
        }
    }
}
