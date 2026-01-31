using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private GameObject leftMenu;

    private const string FullscreenKey = "settings_fullscreen";
    private const string ResolutionKey = "settings_resolution_index";

    private Resolution[] availableResolutions;

    private void Start()
    {
        availableResolutions = Screen.resolutions;
        BuildResolutionDropdown();

        LoadFullscreen();
        LoadResolution();

        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        resolutionDropdown.onValueChanged.AddListener(SetResolutionByDropdownIndex);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
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


    private void LoadFullscreen()
    {
        bool isFullscreen = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;
        Screen.fullScreen = isFullscreen;
        fullscreenToggle.isOn = isFullscreen;
    }

    private void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt(FullscreenKey, isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void BuildResolutionDropdown()
    {
        resolutionDropdown.ClearOptions();

        var options = new List<string>();
        int currentIndex = 0;

        for (int i = 0; i < availableResolutions.Length; i++)
        {
            Resolution r = availableResolutions[i];
            string option = $"{r.width} x {r.height} @ {r.refreshRateRatio.value:0.#}Hz";
            options.Add(option);

            if (r.width == Screen.currentResolution.width &&
                r.height == Screen.currentResolution.height)
            {
                currentIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);

        resolutionDropdown.value = currentIndex;
        resolutionDropdown.RefreshShownValue();
    }

    private void LoadResolution()
    {
        int savedIndex = PlayerPrefs.GetInt(ResolutionKey, resolutionDropdown.value);
        savedIndex = Mathf.Clamp(savedIndex, 0, availableResolutions.Length - 1);

        resolutionDropdown.value = savedIndex;
        resolutionDropdown.RefreshShownValue();

        ApplyResolution(savedIndex);
    }

    private void SetResolutionByDropdownIndex(int index)
    {
        index = Mathf.Clamp(index, 0, availableResolutions.Length - 1);
        ApplyResolution(index);

        PlayerPrefs.SetInt(ResolutionKey, index);
        PlayerPrefs.Save();
    }

    private void ApplyResolution(int index)
    {
        Resolution r = availableResolutions[index];

        bool fs = Screen.fullScreen;

        Screen.SetResolution(r.width, r.height, fs);
    }
}
