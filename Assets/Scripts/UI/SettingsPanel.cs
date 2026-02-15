using UnityEngine;
using UnityEngine.UI;

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
        if (_volumeSlider != null)
            _volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        if (_fullscreenToggle != null)
            _fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
    }

    private void RemoveListeners()
    {
        if (_volumeSlider != null)
            _volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        if (_fullscreenToggle != null)
            _fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
    }

    private void LoadSettings()
    {
        // Load volume (default 1.0)
        float volume = PlayerPrefs.GetFloat(VolumeKey, 1f);
        if (_volumeSlider != null)
        {
            _volumeSlider.value = volume;
        }
        ApplyVolume(volume);

        // Load fullscreen (default true)
        bool fullscreen = PlayerPrefs.GetInt(FullscreenKey, 1) == 1;
        if (_fullscreenToggle != null)
        {
            _fullscreenToggle.isOn = fullscreen;
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
}
