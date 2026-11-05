using UnityEngine;

namespace NebulaDefender
{
    public enum PickupKind { Repair, DoubleShot }

    [RequireComponent(typeof(SphereCollider))]
    public sealed class Pickup : MonoBehaviour
    {
        [SerializeField] private Transform visual;
        private GameSession session;
        private PickupKind kind;
        private float expires;
        private bool consumed;
        public void Configure(Transform model) => visual = model;
        public void Initialize(GameSession owner, PickupKind type) { session = owner; kind = type; expires = Time.time + 18; }
        private void Update()
        {
            if (Time.time >= expires) { Destroy(gameObject); return; }
            if (visual != null) visual.Rotate(0, 80 * Time.deltaTime, 0);
        }
        private void OnTriggerEnter(Collider other)
        {
            if (consumed || !session.IsPlaying || other.gameObject.layer != 8) return;
            consumed = true;
            if (kind == PickupKind.Repair) session.Health.Repair(30); else session.Weapon.Upgrade(14);
            session.Feedback.Shield(transform.position); Destroy(gameObject);
        }
    }
}
