using NUnit.Framework;
using UnityEditor;

namespace NebulaDefender.Tests
{
    public sealed class LevelDataTests
    {
        [Test] public void SectorAssetsHaveRequiredQuotasAndIncreasingDifficulty()
        {
            const string root="Assets/Nebula/Generated/Data/";
            var a=AssetDatabase.LoadAssetAtPath<LevelDefinition>(root+"Sector01.asset");
            var b=AssetDatabase.LoadAssetAtPath<LevelDefinition>(root+"Sector02.asset");
            var c=AssetDatabase.LoadAssetAtPath<LevelDefinition>(root+"Sector03.asset");
            Assert.That(a,Is.Not.Null);Assert.That(b,Is.Not.Null);Assert.That(c,Is.Not.Null);
            Assert.That(a.asteroidGoal,Is.EqualTo(10));Assert.That(b.asteroidGoal,Is.EqualTo(12));Assert.That(b.droneGoal,Is.EqualTo(6));
            Assert.That(c.boss,Is.True);Assert.That(c.droneGoal,Is.EqualTo(4));
            Assert.That(b.asteroidSpeed,Is.GreaterThan(a.asteroidSpeed));Assert.That(c.asteroidSpeed,Is.GreaterThan(b.asteroidSpeed));
            Assert.That(c.enemyFireInterval,Is.LessThan(b.enemyFireInterval));Assert.That(c.enemyHealth,Is.GreaterThan(b.enemyHealth));
        }
    }
}
