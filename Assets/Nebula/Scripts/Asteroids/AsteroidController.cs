using UnityEngine;

namespace NebulaDefender
{
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public sealed class AsteroidController : DamageTarget
    {
        [SerializeField] private Transform visual;
        private Rigidbody body;
        private CombatDirector director;
        private int health;
        private Vector3 spin;
        private Renderer surface;
        private MaterialPropertyBlock tint;
        private Color rockColor;
        private float flashUntil;
        public int Tier { get; private set; }
        public override Team Team => Team.Neutral;
        public override bool IsAlive => health > 0;
        public void Configure(Transform mesh) => visual = mesh;
        public void Initialize(CombatDirector owner, int tier, Vector3 position, Vector3 velocity, int seed)
        {
            director = owner; Tier = Mathf.Clamp(tier, 0, 2); health = owner.Session.Balance.AsteroidHealth(Tier);
            body = GetComponent<Rigidbody>();
            float radius = owner.Session.Balance.AsteroidRadius(Tier);
            transform.localScale = Vector3.one * radius;
            body.position = position; transform.position = position; body.linearVelocity = velocity;
            var rng = new System.Random(seed);
            spin = new Vector3((float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble()) * 42 - Vector3.one * 21;
            body.mass = 1 + Tier * 2;
            surface = visual != null ? visual.GetComponentInChildren<Renderer>() : null;
            if (surface != null) { tint = new MaterialPropertyBlock(); rockColor = surface.sharedMaterial.GetColor("_BaseColor"); }
        }
        private void Update()
        {
            if (IsAlive && visual != null) visual.Rotate(spin * Time.deltaTime, Space.Self);
            if (flashUntil > 0 && Time.time >= flashUntil) { flashUntil = 0; surface.SetPropertyBlock(null); }
        }
        private void FixedUpdate() { if (IsAlive) director.Session.Arena.Contain(body); }
        public override void Hit(int damage, Vector3 point)
        {
            if (!IsAlive || damage <= 0 || !director.Session.IsPlaying) return;
            health = Mathf.Max(0, health - damage);
            if (health > 0)
            {
                if (surface != null) { tint.SetColor("_BaseColor", Color.Lerp(rockColor, Color.white, 0.8f)); surface.SetPropertyBlock(tint); flashUntil = Time.time + 0.12f; }
                director.Session.Feedback.Impact(point, 0.25f); return;
            }
            // Mark dead before notifying; repeated contacts in this physics step cannot award twice.
            GetComponent<Collider>().enabled = false;
            director.RockDestroyed(this, body.linearVelocity);
            Destroy(gameObject);
        }
    }
}
