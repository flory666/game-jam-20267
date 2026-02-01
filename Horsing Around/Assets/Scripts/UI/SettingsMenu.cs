using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject leftMenu;

    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Slider volumeSlider;

    [Header("Audio")]
    [SerializeField] private AudioSource menuMusicSource;

    private const string FullscreenKey = "settings_fullscreen";
    private const string VolumeKey = "menu_music_volume"; 

    private void Start()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        bool isFullscreen = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;
        Screen.fullScreen = isFullscreen;

        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = isFullscreen;
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        }

        float vol = Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey, 0.5f));

        if (menuMusicSource != null)
            menuMusicSource.volume = vol;

        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.wholeNumbers = false;

            volumeSlider.value = vol;
            volumeSlider.onValueChanged.AddListener(SetMusicVolume);
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

    private void SetMusicVolume(float value)
    {
        value = Mathf.Clamp01(value);

        if (menuMusicSource != null)
            menuMusicSource.volume = value;

        PlayerPrefs.SetFloat(VolumeKey, value);
        PlayerPrefs.Save();
    }
}
