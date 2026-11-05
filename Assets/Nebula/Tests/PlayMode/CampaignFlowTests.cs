using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace NebulaDefender.Tests
{
    public sealed class CampaignFlowTests
    {
        private GameSession session;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return SceneManager.LoadSceneAsync("CoreSandbox");yield return null;yield return null;
            session=Object.FindFirstObjectByType<GameSession>();Assert.That(session.IsPlaying,Is.True);
        }
        [UnityTearDown] public IEnumerator Cleanup(){Time.timeScale=1;yield return null;}
        [UnityTest] public IEnumerator AllThreeMissionConditionsTransitionToFinalVictory()
        {
            // Damage API drives deterministic completion; this is not a human full-campaign playthrough.
            for(int i=0;i<10;i++)session.AwardRock(0);
            Assert.That(session.State,Is.EqualTo(SessionState.SectorComplete));
            int score=session.Stats.score;session.AwardRock(0);Assert.That(session.Stats.score,Is.EqualTo(score));
            Assert.That(session.Director.RockCount,Is.Zero);Assert.That(session.Projectiles.ActiveCount,Is.Zero);
            session.NextSector();Assert.That(session.Level.sector,Is.EqualTo(2));
            for(int i=0;i<12;i++)session.AwardRock(0);for(int i=0;i<5;i++)session.AwardDrone();
            Assert.That(session.IsPlaying,Is.True);session.AwardDrone();Assert.That(session.State,Is.EqualTo(SessionState.SectorComplete));
            session.NextSector();Assert.That(session.Level.sector,Is.EqualTo(3));
            for(int i=0;i<4;i++)session.AwardDrone();yield return null;
            Assert.That(session.Director.Boss,Is.Not.Null);
            var boss=session.Director.Boss;boss.Hit(session.Level.bossHealth/2+1,boss.transform.position);
            Assert.That(boss.Phase,Is.EqualTo(2));boss.Hit(99999,boss.transform.position);yield return null;
            Assert.That(session.State,Is.EqualTo(SessionState.Victory));Assert.That(session.Stats.victory,Is.True);
            Assert.That(session.Director.Boss==null,Is.True);
        }
        [UnityTest] public IEnumerator DronesSpawnPursueAndFireHostileProjectiles()
        {
            session.BeginSector(1);yield return new WaitForSeconds(2.2f);
            var drone=Object.FindFirstObjectByType<EnemyController>();Assert.That(drone,Is.Not.Null);
            session.Player.Body.position=drone.GetComponent<Rigidbody>().position+Vector3.back*11;
            float deadline=Time.time+5;
            while(session.Projectiles.ActiveCount==0&&Time.time<deadline)yield return null;
            Assert.That(drone.State,Is.Not.EqualTo(DroneState.Patrol));
            Assert.That(session.Projectiles.ActiveCount,Is.GreaterThan(0),"At least one hostile shot should be active.");
        }
        [UnityTest] public IEnumerator ActualRockContactDamagesPlayerAfterSpawnProtection()
        {
            session.Director.Clear();yield return new WaitForSeconds(session.Balance.respawnProtection+0.1f);
            session.Director.SpawnRock(0,new Vector3(0,0,3),Vector3.back*5);
            yield return new WaitForSeconds(0.75f);
            Assert.That(session.Health.Vitals.Health,Is.LessThan(100));
        }
        [UnityTest] public IEnumerator LosingAllLivesEntersDefeatAndBlocksWeapons()
        {
            session.Director.Clear();
            for(int i=0;i<3;i++)
            {
                yield return new WaitForSeconds(session.Balance.respawnProtection+0.05f);
                session.Health.Hit(1000,session.Player.transform.position);
            }
            Assert.That(session.Health.Vitals.Lives,Is.Zero);Assert.That(session.State,Is.EqualTo(SessionState.Defeat));
            Assert.That(session.Weapon.TryFire(),Is.False);Assert.That(session.Player.gameObject.activeSelf,Is.False);
        }
        [UnityTest] public IEnumerator PooledLaserReuseDoesNotCarrySpentDamageAcrossShots()
        {
            session.Director.Clear();yield return null;
            for(int i=0;i<3;i++)
            {
                var rock=session.Director.SpawnRock(0,new Vector3(0,0,6),Vector3.zero);
                session.Projectiles.Fire(Team.Player,new Vector3(0,0,2),Vector3.forward,48,25,1);
                yield return new WaitForSeconds(0.16f);Assert.That(rock==null,Is.True);session.Director.Clear();yield return null;
            }
            Assert.That(session.Mission.Asteroids,Is.EqualTo(3));Assert.That(session.Stats.score,Is.EqualTo(150));
        }
        [UnityTest] public IEnumerator AsteroidTreeTerminatesAtSmallAndPoolNeverExceedsLimit()
        {
            session.Director.Clear();yield return null;
            session.Director.SpawnRock(2,new Vector3(10,0,5),Vector3.right);
            int count=0;
            while(session.IsPlaying&&count<7)
            {
                var rocks=Object.FindObjectsByType<AsteroidController>(FindObjectsSortMode.None);
                // Replenishment can add a new large root; finish the original seven-node tree only.
                AsteroidController next=null;foreach(var rock in rocks)if(rock.IsAlive&&(count==0||rock.Tier<2)){next=rock;break;}
                Assert.That(next,Is.Not.Null);next.Hit(9999,next.transform.position);count++;yield return null;
            }
            Assert.That(session.Mission.Asteroids,Is.EqualTo(7));
            for(int i=0;i<400;i++)session.Projectiles.Fire(Team.Player,Vector3.zero,Vector3.forward,1,1,1);
            Assert.That(session.Projectiles.ActiveCount,Is.LessThanOrEqualTo(session.Balance.maxProjectiles));
        }
    }
}
