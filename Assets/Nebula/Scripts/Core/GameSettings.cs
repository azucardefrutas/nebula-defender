using UnityEngine;

namespace NebulaDefender
{
    public sealed class GameSettings
    {
        public float Music { get; private set; }
        public float Effects { get; private set; }
        public float Turn { get; private set; }
        public bool CameraShake { get; private set; }
        public void Load()
        {
            Music = Mathf.Clamp01(PlayerPrefs.GetFloat("nd.music", 0.45f));
            Effects = Mathf.Clamp01(PlayerPrefs.GetFloat("nd.effects", 0.65f));
            Turn = Mathf.Clamp(PlayerPrefs.GetFloat("nd.turn", 1), 0.6f, 1.4f);
            CameraShake = PlayerPrefs.GetInt("nd.shake", 1) != 0;
            int quality = Mathf.Clamp(PlayerPrefs.GetInt("nd.quality", QualitySettings.GetQualityLevel()), 0, QualitySettings.names.Length - 1);
            QualitySettings.SetQualityLevel(quality);
            if (PlayerPrefs.HasKey("nd.width")) Screen.SetResolution(Mathf.Clamp(PlayerPrefs.GetInt("nd.width"), 960, 3840), Mathf.Clamp(PlayerPrefs.GetInt("nd.height"), 540, 2160), PlayerPrefs.GetInt("nd.fullscreen", 0) != 0 ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        }
        public void Apply(float music, float effects, float turn, bool shake, bool fullscreen, int width, int height, int quality)
        {
            Music = Mathf.Clamp01(music); Effects = Mathf.Clamp01(effects); Turn = Mathf.Clamp(turn, 0.6f, 1.4f); CameraShake = shake;
            QualitySettings.SetQualityLevel(Mathf.Clamp(quality, 0, QualitySettings.names.Length - 1));
            Screen.SetResolution(width, height, fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
            PlayerPrefs.SetFloat("nd.music", Music); PlayerPrefs.SetFloat("nd.effects", Effects); PlayerPrefs.SetFloat("nd.turn", Turn);
            PlayerPrefs.SetInt("nd.shake", CameraShake ? 1 : 0); PlayerPrefs.SetInt("nd.fullscreen", fullscreen ? 1 : 0);
            PlayerPrefs.SetInt("nd.width", width); PlayerPrefs.SetInt("nd.height", height); PlayerPrefs.SetInt("nd.quality", QualitySettings.GetQualityLevel());
            PlayerPrefs.Save();
        }
    }
}
