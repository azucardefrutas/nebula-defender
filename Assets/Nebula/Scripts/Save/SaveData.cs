using System;
using System.Collections.Generic;

namespace NebulaDefender
{
    [Serializable] public sealed class PilotProfile
    {
        public string id, nickname, registeredUtc;
        public int bestScore, maxSector = 1, enemiesDestroyed, asteroidsDestroyed, gamesPlayed;
    }
    [Serializable] public sealed class RunRecord
    {
        public string id, pilotId, nickname, updatedUtc;
        public int score, sector, asteroids, enemies;
        public float survivalSeconds;
        public bool victory;
    }
    [Serializable] public sealed class SaveDatabase
    {
        public string format;
        public int schemaVersion;
        public string selectedPilotId;
        public List<PilotProfile> pilots;
        public List<RunRecord> runs;
        public static SaveDatabase Empty() => new SaveDatabase {
            format = "nebula-defender", schemaVersion = 1, selectedPilotId = "",
            pilots = new List<PilotProfile>(), runs = new List<RunRecord>()
        };
    }
    public interface ILeaderboardProvider
    {
        IReadOnlyList<RunRecord> GetTop(int count);
    }

    public static class SaveValidation
    {
        public static string NormalizeNickname(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Escribe un apodo de 2 a 24 caracteres.");
            value = value.Trim();
            if (value.Length < 2 || value.Length > 24) throw new ArgumentException("El apodo debe tener de 2 a 24 caracteres.");
            foreach (char c in value)
                if (!char.IsLetterOrDigit(c) && c != ' ' && c != '_' && c != '-')
                    throw new ArgumentException("Usa letras, números, espacios, guion o guion bajo.");
            return value;
        }
        public static void Validate(SaveDatabase data)
        {
            if (data == null || data.format != "nebula-defender" || data.schemaVersion != 1 || data.pilots == null || data.runs == null)
                throw new FormatException("Formato o versión de guardado no compatible.");
            if (data.pilots.Count > 64 || data.runs.Count > 500) throw new FormatException("El guardado supera el límite de registros.");
            var ids = new HashSet<string>();
            foreach (var p in data.pilots)
            {
                if (p == null || !Guid.TryParse(p.id, out _) || !ids.Add(p.id) || NormalizeNickname(p.nickname) != p.nickname ||
                    !DateTime.TryParse(p.registeredUtc, out _) || p.maxSector < 1 || p.maxSector > 3 || p.bestScore < 0 ||
                    p.enemiesDestroyed < 0 || p.asteroidsDestroyed < 0 || p.gamesPlayed < 0)
                    throw new FormatException("Perfil inválido en el guardado.");
            }
            if (!string.IsNullOrEmpty(data.selectedPilotId) && !ids.Contains(data.selectedPilotId)) throw new FormatException("Piloto seleccionado desconocido.");
            var runIds = new HashSet<string>();
            foreach (var r in data.runs)
            {
                if (r == null || !Guid.TryParse(r.id, out _) || !runIds.Add(r.id) || !ids.Contains(r.pilotId) ||
                    r.score < 0 || r.asteroids < 0 || r.enemies < 0 || r.sector < 1 || r.sector > 3 ||
                    float.IsNaN(r.survivalSeconds) || float.IsInfinity(r.survivalSeconds) || r.survivalSeconds < 0 ||
                    !DateTime.TryParse(r.updatedUtc, out _) || NormalizeNickname(r.nickname) != r.nickname)
                    throw new FormatException("Partida inválida en el guardado.");
            }
        }
    }
}
