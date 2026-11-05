using System;

namespace NebulaDefender
{
    public enum SessionState { Menu, Playing, Paused, SectorComplete, Defeat, Victory }
    public enum Team { Player, Hostile, Neutral }

    /// <summary>One authority for damage, lives and game-over. Presentation owns respawn timing.</summary>
    public sealed class PilotVitals
    {
        public int Health { get; private set; }
        public int Lives { get; private set; }
        public int MaxHealth { get; }
        public bool Dead => Lives == 0;
        public PilotVitals(int lives, int health)
        {
            if (lives < 1 || health < 1) throw new ArgumentOutOfRangeException();
            Lives = lives; Health = MaxHealth = health;
        }
        public bool Damage(int amount)
        {
            if (amount <= 0 || Dead) return false;
            Health = Math.Max(0, Health - amount);
            if (Health > 0) return false;
            Lives--;
            if (!Dead) Health = MaxHealth;
            return true;
        }
        public void Repair(int amount) { if (!Dead && amount > 0) Health = Math.Min(MaxHealth, Health + amount); }
    }

    public sealed class MissionProgress
    {
        public int Asteroids { get; private set; }
        public int Drones { get; private set; }
        public bool BossDefeated { get; private set; }
        public int AsteroidGoal { get; }
        public int DroneGoal { get; }
        public bool RequiresBoss { get; }
        public bool Complete => Asteroids >= AsteroidGoal && Drones >= DroneGoal && (!RequiresBoss || BossDefeated);
        public MissionProgress(int rocks, int drones, bool boss)
        { AsteroidGoal = Math.Max(0, rocks); DroneGoal = Math.Max(0, drones); RequiresBoss = boss; }
        public void RockDestroyed() => Asteroids++;
        public void DroneDestroyed() => Drones++;
        public void DefeatBoss() => BossDefeated = true;
    }

    [Serializable]
    public sealed class RunStatistics
    {
        public int score, asteroids, enemies, maxSector = 1;
        public float seconds;
        public bool victory;
        public void AddScore(int points) => score = (int)Math.Min(int.MaxValue, (long)score + Math.Max(0, points));
    }
}
