using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Jam.Audio; // Указываем ваш namespace

public class SettingsMenu : MonoBehaviour
{
    [Header("UI Elements - Screen")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    [Header("UI Elements - Audio")]
    [SerializeField] private Slider mainVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle muteToggle;

    [Header("UI Elements - Controls")]
    [SerializeField] private Toggle alwaysRunToggle;

    [Header("Default Values")]
    [Range(0f, 1f)][SerializeField] private float defMain = 1f;
    [Range(0f, 1f)][SerializeField] private float defMusic = 0.5f;
    [Range(0f, 1f)][SerializeField] private float defSFX = 0.5f;
    [SerializeField] private bool defMute = false;
    [SerializeField] private bool defAlwaysRun = false;

    private Resolution[] resolutions;

    // Ключи сохранения
    private const string KeyMain = "MainVol";
    private const string KeyMusic = "MusicVol";
    private const string KeySFX = "SFXVol";
    private const string KeyMute = "Mute";
    private const string KeyRun = "AlwaysRun";
    private const string KeyRes = "ResIdx";

    private void Start()
    {
        SetupResolutions();
        LoadAndApplySettings();
    }

    private void SetupResolutions()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            options.Add(resolutions[i].width + " x " + resolutions[i].height);
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
                currentResIndex = i;
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.RefreshShownValue();
    }

    // --- ПРИМЕНЕНИЕ И СОХРАНЕНИЕ ---

    public void ApplySettings()
    {
        // 1. СОХРАНЕНИЕ
        PlayerPrefs.SetFloat(KeyMain, mainVolumeSlider.value);
        PlayerPrefs.SetFloat(KeyMusic, musicVolumeSlider.value);
        PlayerPrefs.SetFloat(KeySFX, sfxVolumeSlider.value);
        PlayerPrefs.SetInt(KeyMute, muteToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt(KeyRun, alwaysRunToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt(KeyRes, resolutionDropdown.value);
        PlayerPrefs.Save();

        // 2. ПРИМЕНЕНИЕ ВИЗУАЛА И ЗВУКА
        if (resolutions != null && resolutions.Length > resolutionDropdown.value)
        {
            Resolution res = resolutions[resolutionDropdown.value];
            Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        }

        UpdateAudioPreview();

        // 3. ОБНОВЛЕНИЕ ИГРОКА (Используем новый API)
        // FindAnyObjectByType быстрее, так как не сортирует объекты по иерархии
        var playerInputs = Object.FindAnyObjectByType<StarterAssets.StarterAssetsInputs>();

        if (playerInputs != null)
        {
            playerInputs.RefreshSettings();
        }

        Debug.Log("<color=#00FF00>Настройки применены через.</color>");
    }

    public void ResetToDefaults()
    {
        mainVolumeSlider.value = defMain;
        musicVolumeSlider.value = defMusic;
        sfxVolumeSlider.value = defSFX;
        muteToggle.isOn = defMute;
        alwaysRunToggle.isOn = defAlwaysRun;
        resolutionDropdown.value = resolutions.Length - 1;

        UpdateAudioPreview();
    }

    // --- ЖИВОЙ ПРЕДПРОСМОТР ЗВУКА ---

    public void UpdateAudioPreview()
    {
        if (AudioManager.SOUND_MANAGER == null) return;

        // Если включен Mute, просто ставим общую громкость в 0
        if (muteToggle.isOn)
        {
            AudioManager.SOUND_MANAGER.MainVolume = 0;
        }
        else
        {
            // Передаем значения из слайдеров напрямую в свойства вашего AudioManager
            AudioManager.SOUND_MANAGER.MainVolume = mainVolumeSlider.value;
            AudioManager.SOUND_MANAGER.MelodieVolume = musicVolumeSlider.value;
            AudioManager.SOUND_MANAGER.EffectVolume = sfxVolumeSlider.value;
        }
    }

    private void LoadAndApplySettings()
    {
        // Загружаем данные
        float m = PlayerPrefs.GetFloat(KeyMain, defMain);
        float mu = PlayerPrefs.GetFloat(KeyMusic, defMusic);
        float s = PlayerPrefs.GetFloat(KeySFX, defSFX);
        bool mute = PlayerPrefs.GetInt(KeyMute, 0) == 1;
        bool run = PlayerPrefs.GetInt(KeyRun, defAlwaysRun ? 1 : 0) == 1;
        int resIdx = PlayerPrefs.GetInt(KeyRes, resolutions.Length - 1);

        // Устанавливаем UI
        mainVolumeSlider.value = m;
        musicVolumeSlider.value = mu;
        sfxVolumeSlider.value = s;
        muteToggle.isOn = mute;
        alwaysRunToggle.isOn = run;
        resolutionDropdown.value = resIdx;

        // Применяем
        UpdateAudioPreview();
        Resolution res = resolutions[resIdx];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }
}