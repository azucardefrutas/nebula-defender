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
        public BossController Boss { get; private set; }
        public int DroneCount => drones.Count;
        public GameSession Session { get; private set; }
        public int RockCount => rocks.Count;
        public void Configure(AsteroidController[] prefabs) => rockPrefabs = prefabs;
        public void ConfigureEnemies(EnemyController drone, BossController boss, Pickup repair, Pickup doubleShot)
        { dronePrefab = drone; bossPrefab = boss; repairPrefab = repair; doubleShotPrefab = doubleShot; }
        public void Initialize(GameSession owner) => Session = owner;
        public void Begin(LevelDefinition definition)
        {
            Clear(); level = definition; rng = new System.Random(level.seed);
            bossSpawned = false; nextDrone = Time.time + 2;
            pickupRoot = new GameObject("Active pickups").transform; pickupRoot.SetParent(transform);
            for (int i = 0; i < level.initialRocks; i++) SpawnRock(i % 3, SpawnPoint(), RandomDirection() * level.asteroidSpeed);
        }
        private void Update()
        {
            if (Session == null || !Session.IsPlaying || level == null) return;
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
        private Vector3 SpawnPoint()
        {
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
                    SpawnRock(rock.Tier - 1, ArenaBounds.Wrapped(p + direction * 1.25f, Session.Arena.HalfSize), direction * (level.asteroidSpeed + 1.2f * (3 - rock.Tier)));
                }
            }
            Session.AwardRock(rock.Tier);
            if (Session.IsPlaying && rocks.Count < 3 && Session.Mission.Asteroids < level.asteroidGoal)
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
