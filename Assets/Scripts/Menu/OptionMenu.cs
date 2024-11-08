using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionMenu : MonoBehaviour
{
    public static OptionMenu Instance { get; private set; }

    [Header("Graphics Settings")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle postProcessToggle;
    [SerializeField] private VolumeProfile volumeProfile;

    [Header("Audio Settings")]
    [SerializeField] private Slider masterVolume;
    [SerializeField] private Slider soundFXVolume;
    [SerializeField] private Slider musicVolume;
    [SerializeField] private AudioMixer _audioMixer;

    [Header("Control Settings")]
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private TMP_InputField mouseSensitivityText;

    [Header("Others Settings")]
    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private Toggle hardModeToggle;

    public GameObject canvas;

    List<TMP_Dropdown.OptionData> resolutionOptions = new();
    Resolution[] resolutions;
    string currentResolution = "";
    bool active = false;
    bool postProcess = true;
    bool isFullscreen = true;
    public bool hardMode = false;
    int quality = -1;
    int localeIndex = -1;
    public float mouseSensitivity = 1300.0f;
    float masterVolumeValue = 1;
    float soundFXVolumeValue = 1;
    float musicVolumeValue = 1;
    float brightnessValue = -1;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Debug.LogError("Found more than one Option Menu in the scene.");
            Destroy(gameObject);
            return;
        }
        DataPersistenceManagement.Instance.LoadPlayerSetting();
        resolutions = Screen.resolutions;
        isFullscreen = Screen.fullScreen;
        if (currentResolution.Equals("")) currentResolution = $"{Screen.currentResolution.width} x {Screen.currentResolution.height}";
        StartCoroutine(GetLocales());
        SettingResolutionsDropdown();
        SettingQualityDropdown();
    }

    public void Show()
    {
        canvas.SetActive(true);
    }

    IEnumerator GetLocales()
    {
        // Wait for the localization system to initialize, loading Locales, preloading etc.
        yield return LocalizationSettings.InitializationOperation;

        // Generate list of available Locales
        List<TMP_Dropdown.OptionData> localeList = new();
        int selected = 0;
        for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; ++i)
        {
            var locale = LocalizationSettings.AvailableLocales.Locales[i];
            if ((localeIndex.Equals(-1) && LocalizationSettings.SelectedLocale == locale) || (localeIndex.Equals(i)))
                selected = i;
            localeList.Add(new TMP_Dropdown.OptionData(locale.LocaleName));
        }

        var handle = LocalizationSettings.StringDatabase.GetAllTables(); 
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            List<StringTable> temp = (List<StringTable>) handle.Result;
            foreach (StringTable table in temp)
            {
                DataPersistenceManagement.Instance.StringTables[table.TableCollectionName] = table;
            }
        }

        languageDropdown.options = localeList;
        languageDropdown.value = selected;
        localeIndex = selected;
        DataPersistenceManagement.Instance.SavePlayerSetting();
    }

    private void SettingResolutionsDropdown()
    {
        resolutionOptions = new();
        List<string> alreadyUsed = new();
        int selected = 0;
        string currentResolutionText = string.IsNullOrEmpty(currentResolution) ? $"{Screen.currentResolution.width} x {Screen.currentResolution.height}" : currentResolution;
        for (int i = 0, len = resolutions.Length; i < len; ++i)
        {
            Resolution resolution = resolutions[i];
            int width = resolution.width;
            int height = resolution.height;
            string resolutionText = $"{width} x {height}";
            if (alreadyUsed.Contains(resolutionText)) continue;
            resolutionOptions.Add(new TMP_Dropdown.OptionData(resolutionText));
            alreadyUsed.Add(resolutionText);
            if (currentResolutionText.Equals(resolutionText)) selected = (alreadyUsed.Count - 1);
        }
        resolutionDropdown.options = resolutionOptions;
        resolutionDropdown.value = selected;
    }

    private void SettingQualityDropdown()
    {
        List<TMP_Dropdown.OptionData> qualityNames = new();
        int level = quality;
        if (level.Equals(-1)) level = (int)DevicePerformanceUtil.GetDevicePerformanceLevel();
        for (int i = 0, len = QualitySettings.names.Length; i < len; ++i)
        {
            string localization = LocalizationSettings.StringDatabase.GetLocalizedString("String Table", QualitySettings.names[i]);
            qualityNames.Add(new TMP_Dropdown.OptionData(localization));
        }
        qualityDropdown.options = qualityNames;
        qualityDropdown.value = level;
        quality = level;
    }

    public void SetLocale(int index)
    {
        if (active) return;
        localeIndex = index;
        StartCoroutine(LocaleSelecte(index));
    }

    public void SetResolution(int index)
    {
        Resolution lastResolution = new();
        string currResolutionText = resolutionOptions[index].text;
        bool isSame = false;
        for (int i = 0, len = resolutions.Length; i < len; ++i)
        {
            Resolution resolution = resolutions[i];
            int width = resolution.width;
            int height = resolution.height;
            string resolutionText = $"{width} x {height}";
            if (currResolutionText.Equals(resolutionText)) isSame = true;
            if (!currResolutionText.Equals(resolutionText) && isSame)
            {
                // selected resolution
                currentResolution = $"{ lastResolution.width } x {lastResolution.height}";
                Screen.SetResolution(lastResolution.width, lastResolution.height, isFullscreen);
                return;
            }
            lastResolution = resolution;
        }
        currentResolution = $"{ lastResolution.width } x {lastResolution.height}";
        Screen.SetResolution(lastResolution.width, lastResolution.height, isFullscreen);
    }

    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index, false);
        quality = index;
        if (index < 3)
        {
            postProcessToggle.isOn = false;
            postProcessToggle.interactable = false;
            foreach (VolumeComponent volumeComponent in volumeProfile.components)
            {
                if (volumeComponent.name.Equals("ColorAdjustments")) continue;
                volumeComponent.active = false;
            }
        }
        else
        {
            postProcessToggle.interactable = true;
            foreach (VolumeComponent volumeComponent in volumeProfile.components)
            {
                if (volumeComponent.name.Equals("ColorAdjustments")) continue;
                volumeComponent.active = true;
            }
        }
    }

    public void SetFullscreen(bool value)
    {
        isFullscreen = value;
        Screen.fullScreen = isFullscreen;
    }

    public void SetPostProcessing(bool value)
    {
        postProcess = value;
        if (value)
        {
            foreach (VolumeComponent volumeComponent in volumeProfile.components)
            {
                if (volumeComponent.name.Equals("ColorAdjustments")) continue;
                volumeComponent.active = true;
            }
        }
        else
        {
            foreach (VolumeComponent volumeComponent in volumeProfile.components)
            {
                if (volumeComponent.name.Equals("ColorAdjustments")) continue;
                volumeComponent.active = false;
            }
        }
    }

    public void SetBrightness(float value)
    {
        brightnessValue = value;
        ColorAdjustments colorAdjustments;
        volumeProfile.TryGet<ColorAdjustments>(out colorAdjustments);
        if (colorAdjustments != null)
        {
            colorAdjustments.postExposure.value = value;
        }
    }

    public void SetMasterVolume(float level)
    {
        masterVolumeValue = level;
        _audioMixer.SetFloat("masterVolume", Mathf.Log10(level) * 20f);
    }

    public void SetSoundFXVolume(float level)
    {
        soundFXVolumeValue = level;
        _audioMixer.SetFloat("soundFXVolume", Mathf.Log10(level) * 20f);
    }

    public void SetMusicVolume(float level)
    {
        musicVolumeValue = level;
        _audioMixer.SetFloat("musicVolume", Mathf.Log10(level) * 20f);
    }

    public void SetSoundZero()
    {
        _audioMixer.SetFloat("masterVolume", -80f);
        _audioMixer.SetFloat("soundFXVolume", -80f);
        _audioMixer.SetFloat("musicVolume", -80f);
    }

    public void RevertSound()
    {
        SetMasterVolume(masterVolumeValue);
        SetSoundFXVolume(soundFXVolumeValue);
        SetMusicVolume(musicVolumeValue);

    }

    public void SetMouseSensitivityText(string value)
    {
        mouseSensitivitySlider.value = float.Parse(value);
        mouseSensitivity = float.Parse(value);
    }

    public void SetMouseSensitivity(float value)
    {
        mouseSensitivityText.text = value.ToString();
        mouseSensitivity = value;
    }

    public void ToggleMouseSensitivity()
    {
        mouseSensitivitySlider.gameObject.SetActive(!mouseSensitivitySlider.gameObject.activeSelf);
        mouseSensitivityText.gameObject.SetActive(!mouseSensitivityText.gameObject.activeSelf);
    }

    public void SetHardMode(bool value)
    {
        hardMode = value;
    }

    public void SaveAndBack()
    {
        if (PauseMenu.Instance)
        {
            DataPersistenceManagement.Instance.SaveGame();
            DataPersistenceManagement.Instance.SavePlayerSetting();
            PauseMenu.Instance.ToggleOptionMenu();
            return;
        } 
        else
        {
            DataPersistenceManagement.Instance.SavePlayerSetting();
            StartMenu.Instance.gameObject.SetActive(true);
        }
    }

    IEnumerator LocaleSelecte(int index)
    {
        active = true;
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        SettingQualityDropdown();
        active = false;
    }

    public void LoadPlayerSetting(PlayerSettingData data)
    {
        if (data == null) return;
        quality = data.qualityIndex;
        SetQuality(quality);
        currentResolution = $"{data.screenResolutionWidth} x {data.screenResolutionHeight}";
        fullscreenToggle.isOn = data.fullscreen;
        SetFullscreen(data.fullscreen);
        postProcessToggle.isOn = data.postProcess;
        SetPostProcessing(data.postProcess);
        masterVolume.value = data.masterVolume;
        soundFXVolume.value = data.soundFXVolume;
        musicVolume.value = data.musicVolume;
        SetLocale(data.localeLanguageIndex);
        mouseSensitivitySlider.value = data.mouseSensitivity;
        mouseSensitivityText.text = data.mouseSensitivity.ToString();
        SetBrightness(data.brightnessValue);
    }

    public void SavePlayerSetting(ref PlayerSettingData data)
    {
        data.qualityIndex = quality;
        data.screenResolutionWidth = int.Parse(currentResolution.Split(" x ")[0]);
        data.screenResolutionHeight = int.Parse(currentResolution.Split(" x ")[1]);
        data.fullscreen = isFullscreen;
        data.postProcess = postProcess;
        data.masterVolume = masterVolumeValue;
        data.soundFXVolume = soundFXVolumeValue;
        data.musicVolume = musicVolumeValue;
        data.localeLanguageIndex = localeIndex;
        data.mouseSensitivity = mouseSensitivity;
        data.brightnessValue = brightnessValue;
    }
}
