using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    // --- Singleton Pattern ---
    public static SettingsManager Instance { get; private set; }

    // --- Public Properties (Read-only for other scripts) ---
    public bool IsMusicEnabled { get; private set; }
    public bool IsSfxEnabled { get; private set; }
    public bool IsVibrationEnabled { get; private set; }

    // --- PlayerPrefs Keys (Constants) ---
    private const string PREFS_MUSIC_KEY = "Settings_Music";
    private const string PREFS_SFX_KEY = "Settings_SFX";
    private const string PREFS_VIBRATION_KEY = "Settings_Vibration";

    void Awake()
    {
        // Enforce Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Make this persist across all scene reloads
        DontDestroyOnLoad(gameObject); 

        LoadSettings();
    }

    // Load saved preferences from the device
    private void LoadSettings()
    {
        // Default value is '1' (meaning "on" by default).
        IsMusicEnabled = PlayerPrefs.GetInt(PREFS_MUSIC_KEY, 1) == 1;
        IsSfxEnabled = PlayerPrefs.GetInt(PREFS_SFX_KEY, 1) == 1;
        IsVibrationEnabled = PlayerPrefs.GetInt(PREFS_VIBRATION_KEY, 1) == 1;
    }

    // --- Public Methods (Called by UIManager) ---

    public void ToggleMusic(bool isOn)
    {
        IsMusicEnabled = isOn;
        PlayerPrefs.SetInt(PREFS_MUSIC_KEY, isOn ? 1 : 0);
        PlayerPrefs.Save();
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicMute(!isOn); // Tell AudioManager
            
        Debug.Log("Music setting saved: " + isOn);
    }

    public void ToggleSFX(bool isOn)
    {
        IsSfxEnabled = isOn;
        PlayerPrefs.SetInt(PREFS_SFX_KEY, isOn ? 1 : 0);
        PlayerPrefs.Save();

        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSfxMute(!isOn); // Tell AudioManager
            
        Debug.Log("SFX setting saved: " + isOn);
    }

    public void ToggleVibration(bool isOn)
    {
        IsVibrationEnabled = isOn;
        PlayerPrefs.SetInt(PREFS_VIBRATION_KEY, isOn ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log("Vibration setting saved: " + isOn);

        if (isOn) TryVibrate(); // Test vibration
    }

    // --- Public Helper Methods ---

    public void TryVibrate()
    {
        if (IsVibrationEnabled)
        {
            Handheld.Vibrate();
            Debug.Log("Vibrating!");
        }
    }
}