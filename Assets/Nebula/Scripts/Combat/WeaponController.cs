using UnityEngine;

namespace NebulaDefender
{
    public sealed class WeaponController : MonoBehaviour
    {
        private GameSession session;
        private float nextFire, doubleUntil;
        public bool DoubleShot => Time.time < doubleUntil;
        public void Initialize(GameSession owner) { session = owner; nextFire = doubleUntil = 0; }
        private void Update()
        {
            if (session != null && session.IsPlaying && session.Input.Fire) TryFire();
        }
        public bool TryFire()
        {
            if (session == null || !session.IsPlaying || Time.time < nextFire) return false;
            var b = session.Balance;
            var muzzle = transform.position + transform.forward * 1.75f;
            bool fired;
            if (DoubleShot)
            {
                fired = session.Projectiles.Fire(Team.Player, muzzle - transform.right * 0.5f, transform.forward, b.projectileSpeed, b.projectileDamage, b.projectileLifetime);
                session.Projectiles.Fire(Team.Player, muzzle + transform.right * 0.5f, transform.forward, b.projectileSpeed, b.projectileDamage, b.projectileLifetime);
            }
            else fired = session.Projectiles.Fire(Team.Player, muzzle, transform.forward, b.projectileSpeed, b.projectileDamage, b.projectileLifetime);
            if (fired) { nextFire = Time.time + b.fireInterval; session.Feedback.Laser(muzzle, false); }
            return fired;
        }
        public void Upgrade(float seconds) => doubleUntil = Time.time + seconds;
    }
}
