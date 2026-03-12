using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Handles settings UI controls (volume, fullscreen).
    /// Decoupled from pause menu logic.
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        [SerializeField] private Slider _volumeSlider;
        [SerializeField] private Toggle _fullscreenToggle;

        private const string VolumeKey = "MasterVolume";
        private const string FullscreenKey = "Fullscreen";

        private void Start()
        {
            LoadSettings();
            SetupListeners();
        }

        private void OnDestroy()
        {
            RemoveListeners();
        }

        private void SetupListeners()
        {
            if (this._volumeSlider != null)
                this._volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            if (this._fullscreenToggle != null)
                this._fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        }

        private void RemoveListeners()
        {
            if (this._volumeSlider != null)
                this._volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
            if (this._fullscreenToggle != null)
                this._fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
        }

        private void LoadSettings()
        {
            // Load volume (default 1.0)
            float volume = PlayerPrefs.GetFloat(VolumeKey, 1f);
            if (this._volumeSlider != null)
            {
                this._volumeSlider.value = volume;
            }
            ApplyVolume(volume);

            // Load fullscreen (default true)
            bool fullscreen = PlayerPrefs.GetInt(FullscreenKey, 1) == 1;
            if (this._fullscreenToggle != null)
            {
                this._fullscreenToggle.isOn = fullscreen;
            }
            ApplyFullscreen(fullscreen);
        }

        private void OnVolumeChanged(float value)
        {
            ApplyVolume(value);
            PlayerPrefs.SetFloat(VolumeKey, value);
            PlayerPrefs.Save();
        }

        private void OnFullscreenChanged(bool isOn)
        {
            ApplyFullscreen(isOn);
            PlayerPrefs.SetInt(FullscreenKey, isOn ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void ApplyVolume(float volume)
        {
            AudioListener.volume = volume;
        }

        private void ApplyFullscreen(bool fullscreen)
        {
            Screen.fullScreen = fullscreen;
        }

        /// <summary>
        /// Initialize the settings panel with references.
        /// Allows programmatic setup without reflection.
        /// </summary>
        public void Initialize(Slider volumeSlider, Toggle fullscreenToggle)
        {
            this._volumeSlider = volumeSlider;
            this._fullscreenToggle = fullscreenToggle;
        
            // Reload settings with new references
            LoadSettings();
            SetupListeners();
        }
    }
}
