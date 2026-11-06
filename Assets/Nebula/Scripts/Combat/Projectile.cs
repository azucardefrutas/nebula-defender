using UnityEngine;

namespace NebulaDefender
{
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public sealed class Projectile : MonoBehaviour
    {
        private ProjectilePool pool;
        private Rigidbody body;
        private Team team;
        private int damage, mask;
        private Vector3 velocity;
        private float expires;
        private bool spent = true;
        private TrailRenderer streak;
        private void Awake()
        {
            var mesh = GetComponentInChildren<MeshRenderer>();
            if (mesh == null) return;
            streak = gameObject.AddComponent<TrailRenderer>();
            streak.sharedMaterial = mesh.sharedMaterial;
            streak.time = 0.055f; streak.minVertexDistance = 0.15f;
            streak.startWidth = 0.12f; streak.endWidth = 0.01f;
            streak.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            streak.receiveShadows = false; streak.emitting = false;
        }
        private void OnDisable() { if (streak != null) { streak.emitting = false; streak.Clear(); } }
        public void Launch(ProjectilePool owner, Team faction, Vector3 p, Vector3 direction, float speed, int power, float lifetime)
        {
            pool = owner; team = faction; damage = power;
            body = GetComponent<Rigidbody>();
            gameObject.layer = team == Team.Player ? 11 : 12;
            mask = team == Team.Player ? (1 << 9) | (1 << 10) : 1 << 8;
            body.position = p; body.rotation = Quaternion.LookRotation(direction);
            transform.SetPositionAndRotation(p, body.rotation);
            velocity = direction.normalized * speed; expires = Time.time + lifetime; spent = false;
            gameObject.SetActive(true);
            if (streak != null) { streak.Clear(); streak.emitting = true; }
        }
        private void FixedUpdate()
        {
            if (spent) return;
            if (Time.time >= expires || pool.Arena.Outside(body.position, 4)) { Release(); return; }
            var step = velocity * Time.fixedDeltaTime;
            // A swept query complements trigger contacts: fast lasers cannot tunnel through a small rock.
            if (Physics.SphereCast(body.position, 0.13f, step.normalized, out var hit, step.magnitude, mask, QueryTriggerInteraction.Ignore))
            { Impact(hit.collider, hit.point); if (spent) return; }
            body.MovePosition(body.position + step);
        }
        private void OnTriggerEnter(Collider other) => Impact(other, body.position);
        private void Impact(Collider other, Vector3 point)
        {
            if (spent || (mask & (1 << other.gameObject.layer)) == 0) return;
            var target = other.GetComponentInParent<DamageTarget>();
            if (target == null || target.Team == team || !target.IsAlive) return;
            spent = true;
            target.Hit(damage, point);
            pool.Return(this);
        }
        public void Release() { if (spent) return; spent = true; pool.Return(this); }
    }
}
