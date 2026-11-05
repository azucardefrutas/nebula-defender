using UnityEngine;

namespace NebulaDefender
{
    [CreateAssetMenu(menuName = "Nebula Defender/Balance")]
    public sealed class GameBalance : ScriptableObject
    {
        [Header("Flight")]
        [Min(1)] public float acceleration = 24;
        [Min(1)] public float maxSpeed = 13;
        [Min(1)] public float turnSpeed = 165;
        [Min(0)] public float drag = 0.45f;
        [Min(0)] public float reverseAcceleration = 19;
        public Vector2 arenaHalfSize = new Vector2(27, 17);
        public bool wrapEdges = true;
        [Header("Combat")]
        public int startingLives = 3;
        public int healthPerLife = 100;
        public float respawnProtection = 3;
        public float hitProtection = 0.65f;
        public float shieldDuration = 2.8f;
        public float shieldCooldown = 10;
        public float fireInterval = 0.18f;
        public float projectileSpeed = 48;
        public float projectileLifetime = 1.4f;
        public int projectileDamage = 25;
        public int maxProjectiles = 144;
        public int maxAsteroids = 42;
        public bool fragmentAsteroids = true;
        [Header("Score")]
        public int smallPoints = 50;
        public int mediumPoints = 100;
        public int largePoints = 150;
        public int dronePoints = 200;
        public int bossPoints = 2000;
        public int sectorBonus = 500;
        public int flawlessBonus = 250;
        public int AsteroidPoints(int tier) => tier == 2 ? largePoints : tier == 1 ? mediumPoints : smallPoints;
        public int AsteroidHealth(int tier) => tier == 2 ? 75 : tier == 1 ? 50 : 25;
        public float AsteroidRadius(int tier) => tier == 2 ? 1.65f : tier == 1 ? 1.05f : 0.58f;
    }
}
