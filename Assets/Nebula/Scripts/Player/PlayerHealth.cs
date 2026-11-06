using UnityEngine;

namespace NebulaDefender
{
    public sealed class PlayerHealth : DamageTarget
    {
        [SerializeField] private GameObject shieldVisual;
        private GameSession session;
        private GameBalance balance;
        private float protectedUntil, shieldUntil, shieldReady;
        public PilotVitals Vitals { get; private set; }
        public override Team Team => Team.Player;
        public override bool IsAlive => Vitals != null && !Vitals.Dead;
        public bool ShieldActive => Time.time < shieldUntil;
        public float ShieldRemaining => Mathf.Max(0, shieldReady - Time.time);
        public float ShieldActiveRemaining => Mathf.Max(0, shieldUntil - Time.time);
        public float SafetyRemaining => Mathf.Max(0, protectedUntil - Time.time);
        public void Configure(GameObject visual) => shieldVisual = visual;
        public void Initialize(GameSession owner, GameBalance config)
        {
            session = owner; balance = config; Vitals = new PilotVitals(config.startingLives, config.healthPerLife);
            protectedUntil = Time.time + config.respawnProtection; shieldUntil = shieldReady = 0;
        }
        private void Update()
        {
            if (session == null) return;
            if (shieldVisual != null) shieldVisual.SetActive(IsAlive && (ShieldActive || Time.time < protectedUntil));
            if (session.IsPlaying && session.Input.ShieldPressed) ActivateShield();
        }
        public bool ActivateShield()
        {
            if (!IsAlive || !session.IsPlaying || Time.time < shieldReady) return false;
            shieldUntil = Time.time + balance.shieldDuration;
            shieldReady = Time.time + balance.shieldCooldown;
            session.Feedback.Shield(transform.position); return true;
        }
        public override void Hit(int damage, Vector3 point)
        {
            if (damage <= 0 || !IsAlive || !session.IsPlaying || ShieldActive || Time.time < protectedUntil) return;
            session.MarkDamage();
            bool lostLife = Vitals.Damage(damage);
            protectedUntil = Time.time + (lostLife ? balance.respawnProtection : balance.hitProtection);
            session.Feedback.Impact(point, lostLife ? 3 : 1);
            if (Vitals.Dead) session.Lose();
            else if (lostLife) session.Player.ResetPosition();
        }
        public void Repair(int amount) => Vitals.Repair(amount);
        private void OnCollisionEnter(Collision collision)
        {
            int layer = collision.gameObject.layer;
            if (layer == 9 || layer == 10) Hit(layer == 9 ? 25 : 30, collision.GetContact(0).point);
        }
    }
}
