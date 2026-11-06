using UnityEngine;

namespace NebulaDefender
{
    public enum DroneState { Patrol, Detect, Pursue, Attack, Reposition, Destroyed }

    [RequireComponent(typeof(Rigidbody))]
    public sealed class EnemyController : DamageTarget
    {
        [SerializeField, Range(0, 30)] private float aimErrorDegrees = 12;
        [SerializeField] private float detectionRange = 40, attackRange = 17;
        private CombatDirector director;
        private Rigidbody body;
        private System.Random rng;
        private int health;
        private float stateUntil, nextFire;
        private Vector3 patrolTarget;
        public DroneState State { get; private set; }
        public override Team Team => Team.Hostile;
        public override bool IsAlive => health > 0;
        public void Initialize(CombatDirector owner, Vector3 position, int seed)
        {
            director = owner; rng = new System.Random(seed); body = GetComponent<Rigidbody>();
            health = owner.Session.Level.enemyHealth; body.position = position; transform.position = position;
            patrolTarget = owner.Session.Player.Body.position * 2 - position; State = DroneState.Patrol;
            nextFire = Time.time + 1.8f + (float)rng.NextDouble();
        }
        private void FixedUpdate()
        {
            if (!IsAlive || !director.Session.IsPlaying) return;
            var session = director.Session;
            var delta = session.Player.Body.position - body.position;
            float distance = delta.magnitude;
            switch (State)
            {
                case DroneState.Patrol:
                    if (distance < detectionRange) { State = DroneState.Detect; stateUntil = Time.time + 0.45f; }
                    break;
                case DroneState.Detect:
                    if (Time.time >= stateUntil) State = DroneState.Pursue;
                    break;
                case DroneState.Pursue:
                    if (distance < attackRange) { State = DroneState.Attack; stateUntil = Time.time + 3.5f; }
                    break;
                case DroneState.Attack:
                    if (distance < 5 || Time.time >= stateUntil) { State = DroneState.Reposition; stateUntil = Time.time + 1.8f; }
                    else if (distance > attackRange + 3) State = DroneState.Pursue;
                    break;
                case DroneState.Reposition:
                    if (Time.time >= stateUntil) State = DroneState.Pursue;
                    break;
            }
            Vector3 heading = State == DroneState.Patrol ? patrolTarget - body.position :
                State == DroneState.Reposition ? -delta + Vector3.Cross(delta, Vector3.up) : delta;
            heading.y = 0;
            if (heading.sqrMagnitude > 0.01f)
                body.MoveRotation(Quaternion.RotateTowards(body.rotation, Quaternion.LookRotation(heading), 130 * Time.fixedDeltaTime));
            float speed = session.Level.enemySpeed * (State == DroneState.Attack ? 0.28f : 1);
            body.AddForce((heading.normalized * speed - body.linearVelocity) * 3, ForceMode.Acceleration);
            body.linearVelocity = Vector3.ClampMagnitude(body.linearVelocity, session.Level.enemySpeed);
            session.Arena.Contain(body);
            if (State == DroneState.Attack && Time.time >= nextFire && delta.sqrMagnitude > 1)
            {
                float error = ((float)rng.NextDouble() * 2 - 1) * aimErrorDegrees;
                var aim = delta + session.Player.Body.linearVelocity * 0.15f;
                var direction = Quaternion.Euler(0, error, 0) * aim.normalized;
                session.Projectiles.Fire(Team.Hostile, body.position + direction * 1.5f, direction, 15, 15, 4);
                session.Feedback.Laser(body.position, true);
                nextFire = Time.time + session.Level.enemyFireInterval + (float)rng.NextDouble() * 0.6f;
            }
        }
        public override void Hit(int damage, Vector3 point)
        {
            if (!IsAlive || damage <= 0 || !director.Session.IsPlaying) return;
            health = Mathf.Max(0, health - damage);
            director.Session.Feedback.Impact(point, health == 0 ? 1.7f : 0.4f);
            if (health > 0) return;
            State = DroneState.Destroyed; GetComponent<Collider>().enabled = false;
            director.DroneDestroyed(this); Destroy(gameObject);
        }
        public void Relocate(Vector3 position)
        {
            body.position = position; transform.position = position; body.linearVelocity = Vector3.zero;
            patrolTarget = director.Session.Player.Body.position; State = DroneState.Patrol;
            nextFire = Mathf.Max(nextFire, Time.time + 1.5f);
        }
    }
}
