using UnityEngine;

namespace NebulaDefender
{
    public sealed class CombatFeedback : MonoBehaviour
    {
        [SerializeField] private ParticleSystem burstPrefab;
        private ParticleSystem[] bursts;
        private int next;
        private GameSession session;
        public void SetSession(GameSession owner) => session = owner;
        public void Configure(ParticleSystem prefab) => burstPrefab = prefab;
        public void Initialize()
        {
            bursts = new ParticleSystem[16];
            for (int i = 0; i < bursts.Length; i++) { bursts[i] = Instantiate(burstPrefab, transform); bursts[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); }
        }
        public void Impact(Vector3 point, float size)
        {
            var effect = bursts[next++ % bursts.Length]; effect.transform.position = point;
            effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = effect.main; main.startSizeMultiplier = 0.16f * Mathf.Max(size, 0.4f);
            main.startSpeedMultiplier = 4 * Mathf.Max(size, 0.5f);
            effect.Play();
            if (session != null)
            {
                if (session.Audio != null) session.Audio.Cue(size >= 1 ? AudioCue.Explosion : AudioCue.Impact);
                if (size >= 1.5f) Camera.main.GetComponent<CameraRig>()?.Shake(size * 0.3f);
            }
        }
        public void Laser(Vector3 point, bool enemy) { if (session != null && session.Audio != null) session.Audio.Cue(enemy ? AudioCue.HostileLaser : AudioCue.Laser); }
        public void Shield(Vector3 point) { Impact(point, 0.7f); if (session != null && session.Audio != null) session.Audio.Cue(AudioCue.Shield); }
    }
}
