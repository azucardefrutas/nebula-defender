using UnityEngine;
using UnityEngine.UI;

namespace NebulaDefender
{
    public sealed class PrototypeHUD : MonoBehaviour
    {
        [SerializeField] private GameSession session;
        [SerializeField] private Text status;
        public void Configure(GameSession owner, Text label) { session = owner; status = label; }
        private void Update()
        {
            if (session.Mission == null) return;
            var v = session.Health.Vitals;
            status.text = $"NEBULA DEFENDER    /    {session.Level.displayName}\nSALUD {v.Health}    VIDAS {v.Lives}    PUNTOS {session.Stats.score:000000}\nASTEROIDES {session.Mission.Asteroids}/{session.Mission.AsteroidGoal}    ESCUDO {(session.Health.ShieldRemaining > 0 ? session.Health.ShieldRemaining.ToString("0") + "s" : "LISTO")}\nW/S acelerar/frenar · A/D girar · ESPACIO disparar · SHIFT escudo · ESC pausa\n{session.State}";
        }
    }
}
