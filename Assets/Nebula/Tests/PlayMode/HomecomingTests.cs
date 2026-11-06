using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace NebulaDefender.Tests
{
    public sealed class HomecomingTests
    {
        private GameSession session;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return SceneManager.LoadSceneAsync("CoreSandbox");
            yield return null; yield return null;
            session=Object.FindFirstObjectByType<GameSession>();
            session.LaunchCampaign();
            yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup() { Time.timeScale=1; yield return null; }
        [UnityTest] public IEnumerator GuestCampaignNeedsRealExtractionAndDoesNotCreateProfiles()
        {
            Assert.That(session.GuestRun,Is.True); Assert.That(session.CampaignMode,Is.True);
            Assert.That(session.Profiles,Is.Null); Assert.That(session.TryExtract(),Is.False);
            for(int i=0;i<session.Level.asteroidGoal;i++)session.AwardRock(0);
            Assert.That(session.ExtractionReady,Is.True);
            Assert.That(session.State,Is.EqualTo(SessionState.Playing),"Clearing a route must open the portal, not skip it.");
            Assert.That(session.Director.RockCount,Is.Zero); Assert.That(session.TryExtract(),Is.False,"The pilot is too far from the portal.");
            int score=session.Stats.score; session.AwardRock(0); Assert.That(session.Stats.score,Is.EqualTo(score));
            yield return CrossPortal();
            Assert.That(session.State,Is.EqualTo(SessionState.SectorComplete));
            Assert.That(session.Gate.Ready,Is.False); Assert.That(session.Stats.score,Is.EqualTo(score+session.Balance.sectorBonus+session.Balance.flawlessBonus));
            session.ReturnToMenu(); Assert.That(session.GuestRun,Is.False); Assert.That(session.Profiles,Is.Null);
        }
        [UnityTest] public IEnumerator PausedPortalCannotCompleteAndRestartClosesIt()
        {
            for(int i=0;i<session.Level.asteroidGoal;i++)session.AwardRock(0);
            Assert.That(session.ExtractionReady,Is.True); session.Pause(true);
            session.Player.Body.position=session.Gate.transform.position;
            Assert.That(session.TryExtract(),Is.False); yield return new WaitForSecondsRealtime(.2f);
            Assert.That(session.State,Is.EqualTo(SessionState.Paused));
            session.Restart(); yield return null;
            Assert.That(session.IsPlaying,Is.True); Assert.That(session.Gate.Ready,Is.False); Assert.That(session.Mission.Asteroids,Is.Zero);
        }
        [UnityTest] public IEnumerator ThreeRoutesCompleteByRealLasersAndPhysicalPortalEntries()
        {
            // Controlled targets verify the entire campaign; this does not measure human difficulty.
            for(int sector=0;sector<3;sector++)
            {
                if(sector>0)session.NextSector();
                float deadline=Time.realtimeSinceStartup+70;
                while(!session.ExtractionReady && session.IsPlaying && Time.realtimeSinceStartup<deadline)
                {
                    DamageTarget target=null;
                    foreach(var actor in Object.FindObjectsByType<DamageTarget>(FindObjectsSortMode.None))
                    {
                        if(actor==session.Health || !actor.IsAlive)continue;
                        var body=actor.GetComponent<Rigidbody>(); body.constraints=RigidbodyConstraints.FreezeAll; actor.enabled=false;
                        body.position=new Vector3(20,0,-12); actor.transform.position=body.position;
                        bool needed=actor is AsteroidController ? session.Mission.Asteroids<session.Mission.AsteroidGoal
                            : actor is EnemyController ? session.Mission.Drones<session.Mission.DroneGoal : actor is BossController;
                        if(target==null && needed)target=actor;
                    }
                    session.Player.ResetPosition(); session.Player.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
                    if(target!=null)
                    {
                        var body=target.GetComponent<Rigidbody>(); body.position=Vector3.forward*6; target.transform.position=body.position;
                        Physics.SyncTransforms(); session.Weapon.TryFire();
                    }
                    yield return new WaitForSecondsRealtime(.2f);
                }
                Assert.That(session.ExtractionReady,Is.True,$"Route {sector+1} must open through actual projectile contacts.");
                yield return CrossPortal();
                Assert.That(session.State,Is.EqualTo(sector==2?SessionState.Victory:SessionState.SectorComplete));
            }
            Assert.That(session.Stats.victory,Is.True); Assert.That(session.Stats.maxSector,Is.EqualTo(3));
            Assert.That(session.Stats.enemies,Is.EqualTo(11)); Assert.That(session.Profiles,Is.Null);
        }
        private IEnumerator CrossPortal()
        {
            var gatePosition=session.Gate.transform.position;
            var body=session.Player.Body; body.position=gatePosition-Vector3.forward*6;
            body.rotation=Quaternion.identity; body.linearVelocity=Vector3.forward*10;
            float deadline=Time.realtimeSinceStartup+3;
            while(session.IsPlaying && Time.realtimeSinceStartup<deadline)yield return null;
            Assert.That(session.IsPlaying,Is.False,"A real player/portal trigger contact must complete extraction.");
        }
    }
}
