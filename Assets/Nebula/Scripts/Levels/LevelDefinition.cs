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
        public string MissionSteps => boss
            ? $"1. Destruye {droneGoal} drones de defensa.\n2. Derrota a Nemesis Prime cuando aparezca."
            : $"Destruye {asteroidGoal} asteroides." + (droneGoal > 0 ? $"\nElimina {droneGoal} drones enemigos." : "\nCada fragmento destruido también cuenta.");
        public string TacticalAdvice => boss
            ? "Apártate de las salvas del jefe. Por debajo de la mitad de salud dispara más rápido."
            : droneGoal > 0
                ? "Los drones naranjas te persiguen. Mantén la nave en movimiento y dispara hacia su trayectoria."
                : "Gira para apuntar con la proa y mantén Espacio. Las rocas grandes necesitan varios impactos.";
    }
}
