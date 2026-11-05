using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NebulaDefender
{
    public sealed class ProfileRepository : ILeaderboardProvider
    {
        private readonly LocalSaveStore store;
        public SaveDatabase Data { get; }
        public string Notice { get; private set; }
        public PilotProfile Selected => Data.pilots.Find(p => p.id == Data.selectedPilotId);
        public ProfileRepository(string directory)
        { store = new LocalSaveStore(directory); Data = store.Load(); Notice = store.Notice; }
        public PilotProfile Register(string nickname)
        {
            nickname = SaveValidation.NormalizeNickname(nickname);
            if (Data.pilots.Count >= 64) throw new ArgumentException("Se alcanzó el máximo de 64 pilotos locales.");
            if (Data.pilots.Exists(p => string.Equals(p.nickname, nickname, StringComparison.OrdinalIgnoreCase))) throw new ArgumentException("Ese apodo ya existe. Selecciónalo en la lista.");
            var profile = new PilotProfile { id = Guid.NewGuid().ToString("N"), nickname = nickname, registeredUtc = DateTime.UtcNow.ToString("O") };
            Data.pilots.Add(profile); Data.selectedPilotId = profile.id; Flush(); return profile;
        }
        public void Select(string id)
        {
            if (!Data.pilots.Exists(p => p.id == id)) throw new ArgumentException("Piloto inexistente.");
            Data.selectedPilotId = id; Flush();
        }
        public string StartRun()
        {
            if (Selected == null) throw new InvalidOperationException("Selecciona un piloto.");
            string id = Guid.NewGuid().ToString("N");
            Data.runs.Add(new RunRecord { id = id, pilotId = Selected.id, nickname = Selected.nickname, updatedUtc = DateTime.UtcNow.ToString("O"), sector = 1 });
            Selected.gamesPlayed++; TrimHistory(); Flush(); return id;
        }
        public void Record(string runId, RunStatistics stats, int unlockedSector)
        {
            var run = Data.runs.Find(r => r.id == runId);
            if (run == null) throw new InvalidOperationException("La partida no pertenece a este registro.");
            var profile = Data.pilots.Find(p => p.id == run.pilotId);
            profile.asteroidsDestroyed = Saturate(profile.asteroidsDestroyed, Math.Max(0, stats.asteroids - run.asteroids));
            profile.enemiesDestroyed = Saturate(profile.enemiesDestroyed, Math.Max(0, stats.enemies - run.enemies));
            profile.bestScore = Math.Max(profile.bestScore, stats.score); profile.maxSector = Math.Max(profile.maxSector, Math.Min(3, unlockedSector));
            run.score = stats.score; run.sector = stats.maxSector; run.asteroids = stats.asteroids; run.enemies = stats.enemies;
            run.survivalSeconds = stats.seconds; run.victory = stats.victory; run.updatedUtc = DateTime.UtcNow.ToString("O");
            Flush();
        }
        private static int Saturate(int a, int b) => (int)Math.Min(int.MaxValue, (long)a + b);
        private void TrimHistory()
        {
            // Keep each pilot's best result and most recent runs. No synthetic ranking entries.
            while (Data.runs.Count > 500)
            {
                var protectedIds = new HashSet<string>(Data.runs.GroupBy(r => r.pilotId).Select(g => g.OrderByDescending(r => r.score).First().id));
                var oldest = Data.runs.FirstOrDefault(r => !protectedIds.Contains(r.id) && r != Data.runs[Data.runs.Count - 1]);
                if (oldest == null) break; Data.runs.Remove(oldest);
            }
        }
        public IReadOnlyList<RunRecord> GetTop(int count) => Data.runs.Where(r => r.survivalSeconds > 0)
            .GroupBy(r => r.pilotId).Select(g => g.OrderByDescending(r => r.score).ThenByDescending(r => r.sector).ThenByDescending(r => r.survivalSeconds).First())
            .OrderByDescending(r => r.score).ThenByDescending(r => r.sector).ThenByDescending(r => r.survivalSeconds).Take(Math.Max(0, Math.Min(10, count))).ToList();
        public bool Flush()
        {
            try { store.Save(Data); Notice = ""; return true; }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is FormatException)
            { Notice = "No se pudo guardar el progreso: " + e.Message; return false; }
        }
    }
}
