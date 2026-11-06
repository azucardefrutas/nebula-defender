using System.Collections.Generic;
using UnityEngine;

namespace NebulaDefender
{
    public sealed class CombatDirector : MonoBehaviour
    {
        [SerializeField] private AsteroidController[] rockPrefabs;
        [SerializeField] private EnemyController dronePrefab;
        [SerializeField] private BossController bossPrefab;
        [SerializeField] private Pickup repairPrefab, doubleShotPrefab;
        private readonly List<AsteroidController> rocks = new List<AsteroidController>();
        private readonly List<EnemyController> drones = new List<EnemyController>();
        private System.Random rng;
        private LevelDefinition level;
        private float nextDrone;
        private bool bossSpawned;
        private Transform pickupRoot;
        private Camera view;
        private float nextReposition;
        private bool earlyUpgrade;
        public BossController Boss { get; private set; }
        public int DroneCount => drones.Count;
        public GameSession Session { get; private set; }
        public int RockCount => rocks.Count;
        public DamageTarget NearestObjective()
        {
            if (Session == null || Session.Mission == null) return null;
            if (Boss != null && Boss.IsAlive) return Boss;
            DamageTarget nearest = null; float distance = float.MaxValue;
            void Consider(DamageTarget actor)
            {
                if (actor == null || !actor.IsAlive) return;
                float d = (actor.transform.position - Session.Player.Body.position).sqrMagnitude;
                if (d < distance) { distance = d; nearest = actor; }
            }
            if (Session.Mission.Asteroids < Session.Mission.AsteroidGoal) foreach (var rock in rocks) Consider(rock);
            if (Session.Mission.Drones < Session.Mission.DroneGoal) foreach (var drone in drones) Consider(drone);
            return nearest;
        }
        public void Configure(AsteroidController[] prefabs) => rockPrefabs = prefabs;
        public void ConfigureEnemies(EnemyController drone, BossController boss, Pickup repair, Pickup doubleShot)
        { dronePrefab = drone; bossPrefab = boss; repairPrefab = repair; doubleShotPrefab = doubleShot; }
        public void Initialize(GameSession owner) { Session = owner; view = Camera.main; }
        public void Begin(LevelDefinition definition)
        {
            Clear(); level = definition; rng = new System.Random(level.seed);
            bossSpawned = earlyUpgrade = false; nextDrone = Time.time + 2; nextReposition = Time.time + 1;
            pickupRoot = new GameObject("Active pickups").transform; pickupRoot.SetParent(transform);
            for (int i = 0; i < level.initialRocks; i++)
            {
                var p=i==0 ? Session.Player.Body.position+Vector3.forward*8 : SpawnPoint();
                var velocity=i==0 ? Vector3.zero : (Session.Player.Body.position-p).normalized*level.asteroidSpeed*.65f+RandomDirection()*.6f;
                SpawnRock(i%3,p,velocity);
            }
        }
        private void Update()
        {
            if (Session == null || !Session.IsPlaying || level == null) return;
            if (Session.Arena.OpenSpace && Time.time >= nextReposition)
            {
                nextReposition = Time.time + 1;
                RepositionDistantThreats();
            }
            int remaining = level.droneGoal - Session.Mission.Drones;
            if (remaining > drones.Count && drones.Count < level.simultaneousDrones && Time.time >= nextDrone)
            {
                var drone = Instantiate(dronePrefab, transform); drone.Initialize(this, SpawnPoint(), rng.Next()); drones.Add(drone);
                nextDrone = Time.time + 2.5f;
            }
            if (level.boss && !bossSpawned && remaining <= 0)
            {
                bossSpawned = true; Boss = Instantiate(bossPrefab, transform); Boss.Initialize(this);
            }
        }
        private bool Visible(Vector3 p)
        {
            var v = view.WorldToViewportPoint(p);
            return v.z > 0 && v.x > -.1f && v.x < 1.1f && v.y > -.1f && v.y < 1.1f;
        }
        private void RepositionDistantThreats()
        {
            var center = Session.Player.Body.position;
            foreach (var rock in rocks)
                if (rock != null && rock.IsAlive && rock.enabled && (rock.transform.position-center).sqrMagnitude > 38*38 && !Visible(rock.transform.position))
                {
                    var p = SpawnPoint(true);
                    rock.Relocate(p, (center-p).normalized * level.asteroidSpeed);
                }
            foreach (var drone in drones)
                if (drone != null && drone.IsAlive && drone.enabled && (drone.transform.position-center).sqrMagnitude > 42*42 && !Visible(drone.transform.position))
                    drone.Relocate(SpawnPoint(true));
        }
        private Vector3 SpawnPoint(bool offscreen = false)
        {
            if (Session.Arena.OpenSpace)
            {
                var center = Session.Player.Body.position;
                for (int i = 0; i < 24; i++)
                {
                    var direction=RandomDirection();
                    if(!offscreen && direction.z<0)direction.z=-direction.z;
                    var p = center + direction * (offscreen ? Range(25,34) : Range(12,23));
                    if (!offscreen || !Visible(p)) return p;
                }
                var behind = view.transform.forward; behind.y = 0; behind.Normalize();
                return center - behind * 55;
            }
            var h = Session.Arena.HalfSize;
            for (int i = 0; i < 20; i++)
            {
                var p = new Vector3(Range(-h.x + 2, h.x - 2), 0, Range(-h.y + 2, h.y - 2));
                if (Vector3.Distance(p, Session.Player.transform.position) > 9) return p;
            }
            return new Vector3(h.x - 3, 0, h.y - 3);
        }
        private float Range(float a, float b) => a + (float)rng.NextDouble() * (b - a);
        private Vector3 RandomDirection() { float a = Range(0, Mathf.PI * 2); return new Vector3(Mathf.Sin(a), 0, Mathf.Cos(a)); }
        public AsteroidController SpawnRock(int tier, Vector3 p, Vector3 velocity)
        {
            if (rocks.Count >= Session.Balance.maxAsteroids) return null;
            var rock = Instantiate(rockPrefabs[rng.Next(rockPrefabs.Length)], transform);
            rock.Initialize(this, tier, p, velocity, rng.Next()); rocks.Add(rock); return rock;
        }
        public void RockDestroyed(AsteroidController rock, Vector3 velocity)
        {
            if (!rocks.Remove(rock)) return;
            var p = rock.transform.position;
            Session.Feedback.Impact(p, 1 + rock.Tier * 0.4f);
            if (rock.Tier == 2 && rng.NextDouble() < 0.18) DropPickup(p, PickupKind.Repair);
            if (rock.Tier > 0 && Session.Balance.fragmentAsteroids)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector3 direction = Quaternion.Euler(0, (i == 0 ? -65 : 65), 0) * (velocity.sqrMagnitude > 0.01f ? velocity.normalized : Vector3.forward);
                    var fragmentPosition = p + direction * 1.25f;
                    if (!Session.Arena.OpenSpace) fragmentPosition = ArenaBounds.Wrapped(fragmentPosition, Session.Arena.HalfSize);
                    SpawnRock(rock.Tier - 1, fragmentPosition, direction * (level.asteroidSpeed + 1.2f * (3 - rock.Tier)));
                }
            }
            Session.AwardRock(rock.Tier);
            if(Session.IsPlaying && !Session.ExtractionReady && !earlyUpgrade && Session.Mission.Asteroids>=3)
            { earlyUpgrade=true; DropPickup(Session.Player.Body.position+Session.Player.transform.forward*3,PickupKind.DoubleShot); }
            if (Session.IsPlaying && level!=null && rocks.Count < 3 && Session.Mission.Asteroids < level.asteroidGoal)
                SpawnRock(2, SpawnPoint(), RandomDirection() * level.asteroidSpeed);
        }
        public void DroneDestroyed(EnemyController drone)
        {
            if (!drones.Remove(drone)) return;
            if (rng.NextDouble() < 0.32) DropPickup(drone.transform.position, PickupKind.DoubleShot);
            Session.AwardDrone();
        }
        public void BossDestroyed() { Boss = null; Session.AwardBoss(); }
        private void DropPickup(Vector3 position, PickupKind kind)
        {
            if (pickupRoot == null || pickupRoot.childCount >= 4) return;
            var prefab = kind == PickupKind.Repair ? repairPrefab : doubleShotPrefab;
            if (prefab == null) return;
            var item = Instantiate(prefab, position, Quaternion.identity, pickupRoot); item.Initialize(Session, kind);
        }
        public void StopForExtraction() { Clear(); level=null; }
        public Vector3 AssistedDirection(Vector3 muzzle, Vector3 forward)
        {
            DamageTarget best=null; float alignment=Mathf.Cos(7*Mathf.Deg2Rad);
            void Consider(DamageTarget target)
            {
                if(target==null || !target.enabled || !target.IsAlive)return;
                var delta=target.transform.position-muzzle;
                if(delta.sqrMagnitude>32*32 || delta.sqrMagnitude<1)return;
                float dot=Vector3.Dot(forward,delta.normalized);
                if(dot>alignment) { alignment=dot; best=target; }
            }
            foreach(var rock in rocks)Consider(rock);
            foreach(var drone in drones)Consider(drone);
            Consider(Boss);
            if(best==null)return forward;
            var body=best.GetComponent<Rigidbody>();
            float flight=Mathf.Min(.35f,Vector3.Distance(muzzle,best.transform.position)/Session.Balance.projectileSpeed);
            var prediction=best.transform.position+(body!=null?body.linearVelocity*flight:Vector3.zero);
            return (prediction-muzzle).normalized;
        }
        public void Clear()
        {
            foreach (var rock in rocks) if (rock != null) Destroy(rock.gameObject);
            rocks.Clear();
            foreach (var drone in drones) if (drone != null) Destroy(drone.gameObject);
            drones.Clear();
            if (Boss != null) Destroy(Boss.gameObject); Boss = null;
            if (pickupRoot != null) Destroy(pickupRoot.gameObject); pickupRoot = null;
        }
    }
}
