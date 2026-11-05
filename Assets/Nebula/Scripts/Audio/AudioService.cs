using UnityEngine;

namespace NebulaDefender
{
    public enum AudioCue { Laser, HostileLaser, Impact, Explosion, Shield, Select, Confirm, Complete, Victory, Defeat }

    public sealed class AudioService : MonoBehaviour
    {
        [SerializeField] private AudioClip[] music;
        [SerializeField] private AudioClip[] cues;
        [SerializeField] private AudioClip engineClip;
        private AudioSource musicVoice, engineVoice, uiVoice;
        private AudioSource[] voices;
        private GameSession session;
        private int next;
        private bool combatPaused;
        public void Configure(AudioClip[] loops, AudioClip[] sfx, AudioClip engine) { music = loops; cues = sfx; engineClip = engine; }
        public void Initialize(GameSession owner)
        {
            session = owner; musicVoice = gameObject.AddComponent<AudioSource>(); musicVoice.loop = true; musicVoice.playOnAwake = false;
            engineVoice = gameObject.AddComponent<AudioSource>(); engineVoice.loop = true; engineVoice.clip = engineClip; engineVoice.volume = 0; engineVoice.Play();
            uiVoice = gameObject.AddComponent<AudioSource>(); uiVoice.playOnAwake = false;
            voices = new AudioSource[12]; for (int i = 0; i < voices.Length; i++) { voices[i] = gameObject.AddComponent<AudioSource>(); voices[i].playOnAwake = false; }
            ApplyVolumes();
        }
        private void Update()
        {
            if (session == null) return;
            float thrust = session.IsPlaying ? Mathf.Max(0, session.Input.Throttle) : 0;
            engineVoice.volume = Mathf.MoveTowards(engineVoice.volume, thrust * session.Settings.Effects * 0.22f, Time.unscaledDeltaTime);
            engineVoice.pitch = 0.8f + thrust * 0.35f;
        }
        public void ApplyVolumes()
        {
            if (session == null || musicVoice == null) return;
            musicVoice.volume = session.Settings.Music * (combatPaused && session.State != SessionState.Menu ? 0.45f : 1);
            uiVoice.volume = session.Settings.Effects * 0.45f;
        }
        public void PlayMusic(int sector)
        {
            if (music == null || music.Length == 0) return;
            var clip = music[Mathf.Clamp(sector, 0, music.Length - 1)];
            if (musicVoice.clip == clip && musicVoice.isPlaying) return;
            musicVoice.clip = clip; if (clip != null) musicVoice.Play(); ApplyVolumes();
        }
        public void PauseCombat(bool value)
        {
            combatPaused = value;
            if (voices == null) return;
            foreach (var voice in voices) { if (value) voice.Pause(); else voice.UnPause(); }
            ApplyVolumes();
        }
        public void Cue(AudioCue cue)
        {
            int index = (int)cue;
            if (cues == null || index >= cues.Length || cues[index] == null || session == null) return;
            if (cue >= AudioCue.Select) { uiVoice.PlayOneShot(cues[index]); return; }
            var voice = voices[next++ % voices.Length]; voice.Stop(); voice.clip = cues[index];
            voice.volume = session.Settings.Effects * (cue == AudioCue.Laser ? 0.23f : 0.5f); voice.Play();
        }
    }
}
