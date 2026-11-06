using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NebulaDefender.Tests
{
    public sealed class CombatAcceptanceTests
    {
        private GameSession session;

        [UnitySetUp] public IEnumerator Setup()
        {
            yield return SceneManager.LoadSceneAsync("CoreSandbox");
            yield return null;
            yield return null;
            session = Object.FindFirstObjectByType<GameSession>();
            session.Director.Clear();
            yield return null;
        }

        [UnityTearDown] public IEnumerator Cleanup()
        {
            Time.timeScale = 1;
            yield return null;
        }

#if UNITY_EDITOR
        [UnityTest] public IEnumerator SmallPoolRespectsConfiguredCapacityAndBothFactionsCanFire()
        {
            var root = new GameObject("Isolated bounded pool");
            try
            {
                var pool = root.AddComponent<ProjectilePool>();
                pool.Configure(
                    AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Nebula/Generated/Prefabs/PlayerLaser.prefab").GetComponent<Projectile>(),
                    AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Nebula/Generated/Prefabs/EnemyLaser.prefab").GetComponent<Projectile>());
                pool.Initialize(session.Arena, 5);
                Assert.That(root.transform.childCount, Is.LessThanOrEqualTo(5), "Prewarming must respect the total pool capacity.");
                Assert.That(pool.Fire(Team.Player, Vector3.zero, Vector3.forward, 1, 1, 2), Is.True);
                Assert.That(pool.Fire(Team.Hostile, Vector3.zero, Vector3.back, 1, 1, 2), Is.True);
                for (int i = 0; i < 20; i++) pool.Fire(i % 2 == 0 ? Team.Player : Team.Hostile, Vector3.zero, Vector3.forward, 1, 1, 2);
                Assert.That(pool.ActiveCount, Is.EqualTo(5));
                Assert.That(root.transform.childCount, Is.EqualTo(5));
                pool.Clear();
                Assert.That(pool.ActiveCount, Is.Zero);
                Assert.That(pool.Fire(Team.Player, Vector3.zero, Vector3.forward, 1, 1, 2), Is.True);
            }
            finally { Object.Destroy(root); }
            yield return null;
        }

        [UnityTest] public IEnumerator PhysicalPickupRepairsOnceAndDoubleLaserExpiresAcrossPause()
        {
            yield return new WaitForSeconds(session.Balance.respawnProtection + 0.05f);
            session.Health.Hit(60, session.Player.Body.position);
            Assert.That(session.Health.Vitals.Health, Is.EqualTo(40));
            var repair = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Nebula/Generated/Prefabs/RepairCell.prefab"));
            repair.transform.position = session.Player.Body.position;
            repair.GetComponent<Pickup>().Initialize(session, PickupKind.Repair);
            yield return new WaitForSeconds(0.1f);
            Assert.That(repair == null, Is.True);
            Assert.That(session.Health.Vitals.Health, Is.EqualTo(70), "One physical pickup grants exactly 30 health.");

            var cell = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Nebula/Generated/Prefabs/DoubleLaserCell.prefab"));
            cell.transform.position = session.Player.Body.position;
            cell.GetComponent<Pickup>().Initialize(session, PickupKind.DoubleShot);
            yield return new WaitForSeconds(0.1f);
            Assert.That(cell == null, Is.True);
            Assert.That(session.Weapon.DoubleShot, Is.True);
            session.Projectiles.Clear();
            Assert.That(session.Weapon.TryFire(), Is.True);
            Assert.That(session.Projectiles.ActiveCount, Is.EqualTo(2));
            session.Pause(true);
            yield return new WaitForSecondsRealtime(0.2f);
            Assert.That(session.Weapon.DoubleShot, Is.True);
            session.Pause(false);
            yield return new WaitForSeconds(14.1f);
            Assert.That(session.Weapon.DoubleShot, Is.False);
        }
#endif

        [UnityTest] public IEnumerator PlayerCrossesFormerPerimeterWithoutWrappingOrStopping()
        {
            var body = session.Player.Body;
            body.position = new Vector3(session.Arena.HalfSize.x + 0.5f, 0, 0);
            body.rotation = Quaternion.Euler(0, 75, 0);
            body.linearVelocity = new Vector3(4, 0, 2);
            var velocity = body.linearVelocity;
            var rotation = body.rotation;
            Assert.That(session.Arena.OpenSpace,Is.True);
            var position=body.position;
            session.Arena.Contain(body);
            Assert.That(body.position,Is.EqualTo(position));
            Assert.That(body.linearVelocity, Is.EqualTo(velocity));
            Assert.That(Quaternion.Angle(body.rotation, rotation), Is.LessThan(0.001f));
            yield return new WaitForFixedUpdate();
            Assert.That(body.position.x,Is.GreaterThan(session.Arena.HalfSize.x));
            foreach(var renderer in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
                if(renderer.gameObject.name=="Wrap marker")Assert.That(renderer.enabled,Is.False);
        }

        [UnityTest] public IEnumerator ThreeSectorsCanCompleteThroughWeaponAndColliderHits()
        {
            // A controlled firing range verifies the full damage-to-victory path.
            // It deliberately positions stationary targets; it does not measure human difficulty.
            session.BeginSector(0);
            yield return null;
            for (int sector = 0; sector < 3; sector++)
            {
                if (sector > 0) session.NextSector();
                Assert.That(session.Level.sector, Is.EqualTo(sector + 1));
                float deadline = Time.realtimeSinceStartup + 65;
                int shots = 0;
                while (session.IsPlaying && Time.realtimeSinceStartup < deadline)
                {
                    DamageTarget target = null;
                    foreach (var actor in Object.FindObjectsByType<DamageTarget>(FindObjectsSortMode.None))
                    {
                        if (actor == session.Health || !actor.IsAlive) continue;
                        // Keep surrounding bodies outside the isolated firing lane.
                        var actorBody = actor.GetComponent<Rigidbody>();
                        actorBody.constraints = RigidbodyConstraints.FreezeAll;
                        actor.enabled = false;
                        actorBody.position = new Vector3(20, 0, -12);
                        actor.transform.position = actorBody.position;
                        bool needed = actor is AsteroidController
                            ? session.Mission.Asteroids < session.Mission.AsteroidGoal
                            : actor is EnemyController
                                ? session.Mission.Drones < session.Mission.DroneGoal
                                : actor is BossController;
                        if (needed && target == null) target = actor;
                    }
                    session.Player.ResetPosition();
                    if (target != null)
                    {
                        target.GetComponent<Rigidbody>().position = new Vector3(0, 0, 7);
                        target.transform.position = new Vector3(0, 0, 7);
                        Physics.SyncTransforms();
                        if (session.Weapon.TryFire()) shots++;
                    }
                    // Sector completion pauses scaled time, including pending scaled waits.
                    yield return new WaitForSecondsRealtime(0.2f);
                }
                Assert.That(session.State, Is.EqualTo(sector == 2 ? SessionState.Victory : SessionState.SectorComplete),
                    $"Real projectiles must close sector {sector + 1}: shots={shots}, rocks={session.Mission.Asteroids}/{session.Mission.AsteroidGoal}, drones={session.Mission.Drones}/{session.Mission.DroneGoal}, active shots={session.Projectiles.ActiveCount}, simulation time={Time.time}, player={session.Player.transform.position}, forward={session.Player.transform.forward}");
                Assert.That(session.Projectiles.ActiveCount, Is.Zero);
            }
            Assert.That(session.Stats.victory, Is.True);
            Assert.That(session.Stats.asteroids, Is.GreaterThanOrEqualTo(22));
            Assert.That(session.Stats.enemies, Is.EqualTo(11));
            Assert.That(session.Stats.score, Is.GreaterThan(5000));
        }
    }
}
