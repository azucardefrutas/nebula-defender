using UnityEngine;

namespace NebulaDefender
{
    [CreateAssetMenu(menuName = "Nebula Defender/Sector")]
    public sealed class LevelDefinition : ScriptableObject
    {
        public int sector = 1;
        public string displayName = "ÓRBITA EXTERIOR";
        [TextArea] public string briefing = "Destruye diez asteroides y asegura la órbita.";
        public int asteroidGoal = 10;
        public int droneGoal;
        public bool boss;
        public int initialRocks = 5;
        public int simultaneousDrones;
        public float asteroidSpeed = 2;
        public float enemySpeed = 4;
        public float enemyFireInterval = 2.8f;
        public int enemyHealth = 60;
        public int bossHealth = 1100;
        public int seed = 7319;
        public Color spaceColor = new Color(0.012f, 0.026f, 0.065f);
        public Color accent = new Color(0.1f, 0.85f, 1);
        public string Objective => boss ? $"Defensas {Mathf.Max(0, droneGoal)} · Derrota a NEMESIS PRIME" :
            $"Destruye {asteroidGoal} asteroides" + (droneGoal > 0 ? $" y {droneGoal} drones" : "");
    }
}
