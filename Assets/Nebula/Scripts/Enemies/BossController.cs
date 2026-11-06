using UnityEngine;

namespace NebulaDefender
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class BossController : DamageTarget
    {
        [SerializeField] private Transform core;
        private CombatDirector director;
        private Rigidbody body;
        private int health, maximum;
        private float nextShot, originTime;
        private Vector3 anchor;
        public override Team Team => Team.Hostile;
        public override bool IsAlive => health > 0;
        public float HealthFraction => maximum > 0 ? (float)health / maximum : 0;
        public int Phase => HealthFraction < 0.5f ? 2 : 1;
        public void Configure(Transform energyCore) => core = energyCore;
        public void Initialize(CombatDirector owner)
        {
            director = owner; health = maximum = owner.Session.Level.bossHealth;
            anchor = owner.Session.Arena.OpenSpace ? owner.Session.Player.Body.position : Vector3.zero;
            body = GetComponent<Rigidbody>(); body.position = anchor + new Vector3(0, 0, 10); transform.position = body.position;
            originTime = Time.time; nextShot = Time.time + 2;
        }
        private void FixedUpdate()
        {
            if (!IsAlive || !director.Session.IsPlaying) return;
            float t = Time.time - originTime;
            if (director.Session.Arena.OpenSpace)
                anchor = Vector3.MoveTowards(anchor, director.Session.Player.Body.position, director.Session.Balance.maxSpeed * 1.05f * Time.fixedDeltaTime);
            body.MovePosition(anchor + new Vector3(Mathf.Sin(t * 0.32f) * 15, 0, 10 + Mathf.Sin(t * 0.53f) * 2));
            if (Time.time < nextShot) return;
            var session = director.Session;
            var direction = session.Player.Body.position - body.position;
            direction.y = 0; if (direction.sqrMagnitude < 0.1f) direction = Vector3.back;
            int count = Phase == 1 ? 3 : 5;
            for (int i = 0; i < count; i++)
            {
                var aim = Quaternion.Euler(0, (i - (count - 1) * 0.5f) * 14, 0) * direction.normalized;
                session.Projectiles.Fire(Team.Hostile, body.position + aim * 3.3f, aim, Phase == 1 ? 12 : 14, 18, 5);
            }
            session.Feedback.Laser(body.position, true);
            nextShot = Time.time + (Phase == 1 ? 1.9f : 1.15f);
        }
        private void Update()
        {
            if (core != null && IsAlive) core.localScale = Vector3.one * (1.3f + Mathf.Sin(Time.time * (Phase == 2 ? 8 : 4)) * 0.1f);
        }
        public override void Hit(int damage, Vector3 point)
        {
            if (!IsAlive || damage <= 0 || !director.Session.IsPlaying) return;
            health = Mathf.Max(0, health - damage);
            director.Session.Feedback.Impact(point, health == 0 ? 4 : 0.5f);
            if (health > 0) return;
            GetComponent<Collider>().enabled = false;
            director.BossDestroyed(); Destroy(gameObject);
        }
    }
}
