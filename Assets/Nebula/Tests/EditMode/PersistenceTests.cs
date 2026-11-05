using System;
using System.IO;
using NUnit.Framework;

namespace NebulaDefender.Tests
{
    public sealed class PersistenceTests
    {
        private string directory;
        [SetUp] public void Setup() { directory=Path.Combine(Path.GetTempPath(),"NebulaTests-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory); }
        [TearDown] public void Cleanup() { if(Directory.Exists(directory))Directory.Delete(directory,true); }
        [Test] public void MultiplePilotsRoundTripWithSelectionAndUniqueIds()
        {
            var repo=new ProfileRepository(directory);var first=repo.Register("Piloto Águila");var second=repo.Register("Aurora_02");
            Assert.That(first.id,Is.Not.EqualTo(second.id));repo.Select(first.id);
            var loaded=new ProfileRepository(directory);
            Assert.That(loaded.Data.pilots.Count,Is.EqualTo(2));Assert.That(loaded.Selected.nickname,Is.EqualTo("Piloto Águila"));
        }
        [Test] public void RepeatedCheckpointsDoNotDuplicateGamesOrLifetimeStats()
        {
            var repo=new ProfileRepository(directory);repo.Register("Orion");string run=repo.StartRun();
            var stats=new RunStatistics{score=550,asteroids=4,enemies=2,maxSector=2,seconds=90};
            repo.Record(run,stats,3);repo.Record(run,stats,3);
            var loaded=new ProfileRepository(directory);
            Assert.That(loaded.Selected.gamesPlayed,Is.EqualTo(1));Assert.That(loaded.Selected.asteroidsDestroyed,Is.EqualTo(4));
            Assert.That(loaded.Selected.enemiesDestroyed,Is.EqualTo(2));Assert.That(loaded.Selected.maxSector,Is.EqualTo(3));
            Assert.That(loaded.Data.runs.Count,Is.EqualTo(1));
        }
        [Test] public void RankingUsesBestRealRunPerPilotAndOrdersScore()
        {
            var repo=new ProfileRepository(directory);repo.Register("Orion");
            string a=repo.StartRun();repo.Record(a,new RunStatistics{score=500,seconds=20},1);
            string b=repo.StartRun();repo.Record(b,new RunStatistics{score=250,seconds=30},1);
            repo.Register("Vega");string c=repo.StartRun();repo.Record(c,new RunStatistics{score=900,seconds=40},1);
            var top=repo.GetTop(10);Assert.That(top.Count,Is.EqualTo(2));Assert.That(top[0].nickname,Is.EqualTo("Vega"));Assert.That(top[1].score,Is.EqualTo(500));
        }
        [Test] public void CorruptPrimaryRecoversBackupAndPreservesDamagedFile()
        {
            var repo=new ProfileRepository(directory);repo.Register("Vega");repo.Flush();
            string primary=Path.Combine(directory,"pilots-v1.json");File.WriteAllText(primary,"broken-json");
            var recovered=new ProfileRepository(directory);
            Assert.That(recovered.Selected.nickname,Is.EqualTo("Vega"));Assert.That(recovered.Notice,Does.Contain("respaldo"));
            Assert.That(recovered.Flush(),Is.True);Assert.That(Directory.GetFiles(directory,"*.corrupt-*").Length,Is.EqualTo(1));
            Assert.That(new ProfileRepository(directory).Selected.nickname,Is.EqualTo("Vega"));
        }
        [Test] public void UnrecoverablePrimaryIsNeverSilentlyOverwritten()
        {
            string primary=Path.Combine(directory,"pilots-v1.json");File.WriteAllText(primary,"bad-original");
            var repo=new ProfileRepository(directory);Assert.That(repo.Notice,Is.Not.Empty);
            repo.Register("Vega");Assert.That(repo.Flush(),Is.False);Assert.That(File.ReadAllText(primary),Is.EqualTo("bad-original"));
        }
        [Test] public void FutureVersionIsKeptWithoutReplacingWithAnOldBackup()
        {
            var repo=new ProfileRepository(directory);repo.Register("Vega");repo.Flush();
            string primary=Path.Combine(directory,"pilots-v1.json");string future=File.ReadAllText(primary).Replace("\"schemaVersion\": 1","\"schemaVersion\": 2");File.WriteAllText(primary,future);
            var loaded=new ProfileRepository(directory);Assert.That(loaded.Data.pilots.Count,Is.Zero);Assert.That(loaded.Flush(),Is.False);Assert.That(File.ReadAllText(primary),Is.EqualTo(future));
        }
        [TestCase("<script>")][TestCase(" ")][TestCase("a")][TestCase("name\nother")]
        public void InvalidNicknameIsRejected(string value) => Assert.Throws<ArgumentException>(()=>new ProfileRepository(directory).Register(value));
        [Test] public void InvalidSchemaAndDuplicateIdsCannotBeSaved()
        {
            var data=SaveDatabase.Empty();data.schemaVersion=0;Assert.Throws<FormatException>(()=>new LocalSaveStore(directory).Save(data));
            var repo=new ProfileRepository(directory);repo.Register("Orion");repo.Data.pilots.Add(repo.Selected);
            Assert.Throws<FormatException>(()=>SaveValidation.Validate(repo.Data));
        }
    }
}
