using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject leftMenu;

    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Slider volumeSlider;

    private const string FullscreenKey = "settings_fullscreen";
    private const string VolumeKey = "settings_volume"; // 0..1

    private void Start()
    {
        // Start closed
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        // Load + apply fullscreen
        bool isFullscreen = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;
        Screen.fullScreen = isFullscreen;

        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = isFullscreen;
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        }

        // Load + apply volume
        float vol = PlayerPrefs.GetFloat(VolumeKey, 1f);
        vol = Mathf.Clamp01(vol);
        AudioListener.volume = vol;

        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.wholeNumbers = false;

            volumeSlider.value = vol;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    public void OpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (leftMenu != null) leftMenu.SetActive(false);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (leftMenu != null) leftMenu.SetActive(true);
    }

    private void SetFullscreen(bool value)
    {
        Screen.fullScreen = value;
        PlayerPrefs.SetInt(FullscreenKey, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void SetVolume(float value)
    {
        value = Mathf.Clamp01(value);
        AudioListener.volume = value;

        PlayerPrefs.SetFloat(VolumeKey, value);
        PlayerPrefs.Save();
    }
}
