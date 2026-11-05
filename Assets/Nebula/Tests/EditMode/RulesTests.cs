using NUnit.Framework;
using UnityEngine;

namespace NebulaDefender.Tests
{
    public sealed class RulesTests
    {
        [Test] public void DamageConsumesExactlyThreeLivesAndNeverRevivesAfterDefeat()
        {
            var health=new PilotVitals(3,100);
            Assert.That(health.Damage(45),Is.False);Assert.That(health.Health,Is.EqualTo(55));
            Assert.That(health.Damage(100),Is.True);Assert.That(health.Lives,Is.EqualTo(2));Assert.That(health.Health,Is.EqualTo(100));
            health.Damage(100);health.Damage(100);health.Repair(100);
            Assert.That(health.Dead,Is.True);Assert.That(health.Health,Is.Zero);
            Assert.That(health.Damage(100),Is.False);Assert.That(health.Lives,Is.Zero);
        }
        [Test] public void RepairIsLimitedAndNegativeDamageIsIgnored()
        {
            var health=new PilotVitals(3,100);health.Damage(25);health.Repair(1000);health.Damage(-20);
            Assert.That(health.Health,Is.EqualTo(100));Assert.That(health.Lives,Is.EqualTo(3));
        }
        [Test] public void AsteroidFragmentsCountAsDestructionsAndQuotaDoesNotRequireEmptyArena()
        {
            var progress=new MissionProgress(10,0,false);
            for(int i=0;i<9;i++)progress.RockDestroyed();Assert.That(progress.Complete,Is.False);
            progress.RockDestroyed();Assert.That(progress.Complete,Is.True);
        }
        [Test] public void SecondSectorRequiresBothQuotas()
        {
            var progress=new MissionProgress(12,6,false);
            for(int i=0;i<12;i++)progress.RockDestroyed();for(int i=0;i<5;i++)progress.DroneDestroyed();
            Assert.That(progress.Complete,Is.False);progress.DroneDestroyed();Assert.That(progress.Complete,Is.True);
        }
        [Test] public void FinalVictoryRequiresDefensesAndBoss()
        {
            var progress=new MissionProgress(0,4,true);progress.DefeatBoss();
            Assert.That(progress.Complete,Is.False);for(int i=0;i<4;i++)progress.DroneDestroyed();Assert.That(progress.Complete,Is.True);
        }
        [Test] public void ScoreCannotBecomeNegativeOrOverflow()
        {
            var run=new RunStatistics();run.AddScore(-40);Assert.That(run.score,Is.Zero);
            run.AddScore(int.MaxValue);run.AddScore(500);Assert.That(run.score,Is.EqualTo(int.MaxValue));
        }
        [Test] public void WrappingPreservesOvershootAndThePlayablePlane()
        {
            var wrapped=ArenaBounds.Wrapped(new Vector3(28.5f,7,-18),new Vector2(27,17));
            Assert.That(wrapped.x,Is.EqualTo(-25.5f).Within(0.001));Assert.That(wrapped.z,Is.EqualTo(16).Within(0.001));Assert.That(wrapped.y,Is.Zero);
        }
        [Test] public void LargeRockHasFiniteThreeTierFragmentationAndHigherValue()
        {
            var b=ScriptableObject.CreateInstance<GameBalance>();
            Assert.That(b.AsteroidPoints(2),Is.GreaterThan(b.AsteroidPoints(1)));Assert.That(b.AsteroidPoints(1),Is.GreaterThan(b.AsteroidPoints(0)));
            Assert.That(b.AsteroidHealth(0),Is.EqualTo(b.projectileDamage));Assert.That(b.maxAsteroids,Is.LessThanOrEqualTo(64));Object.DestroyImmediate(b);
        }
    }
}
