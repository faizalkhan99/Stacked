using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This helper class lets us create an editable list in the Inspector.
[System.Serializable]
public class Sound
{
    public SoundID id;
    public AudioClip clip;
}

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<AudioManager>();
            if (instance == null) Debug.LogError("AudioManager is NULL.");
            return instance;
        }
    }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private AudioSource[] _sfxSources;

    [Header("Audio Clips Library")]
    [SerializeField] private Sound[] _sfxLibrary;

    private Dictionary<SoundID, AudioClip> _sfxDictionary;
    private bool _isSfxMuted = false; // Internal state tracker

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        // Make this persist across scene reloads
        DontDestroyOnLoad(gameObject); 

        // Populate the Dictionary
        _sfxDictionary = new Dictionary<SoundID, AudioClip>();
        foreach (var sound in _sfxLibrary)
        {
            _sfxDictionary[sound.id] = sound.clip;
        }
    }

    private void Start()
    {
        // When this AudioManager loads, it immediately checks the
        // SettingsManager (which loaded in Awake) and sets its mute state.
        if (SettingsManager.Instance != null)
        {
            SetMusicMute(!SettingsManager.Instance.IsMusicEnabled);
            SetSfxMute(!SettingsManager.Instance.IsSfxEnabled);
        }
        else
        {
            Debug.LogError("AudioManager: Could not find SettingsManager at Start!");
        }

        PlayBGM();
    }

    // --- BGM Controls ---
    public void PlayBGM() => _bgmSource?.PlayDelayed(0.3f);
    public void PauseBGM() => _bgmSource?.Pause();
    public void UnpauseBGM() => _bgmSource?.UnPause();

    // --- SFX Controls ---
    public void PlaySFX(SoundID id)
    {
        // CHECK 1: Is SFX muted?
        if (_isSfxMuted) return;

        if (!_sfxDictionary.ContainsKey(id))
        {
            Debug.LogWarning("AudioManager: Sound ID not found in library: " + id);
            return;
        }

        AudioClip clipToPlay = _sfxDictionary[id];

        // 1. Try to use a pre-made source
        for (int i = 0; i < _sfxSources.Length; i++)
        {
            if (_sfxSources[i] != null && !_sfxSources[i].isPlaying)
            {
                _sfxSources[i].PlayOneShot(clipToPlay);
                return; // Sound played.
            }
        }

        // 2. If all are busy, create a temporary one
        StartCoroutine(CreateTemporarySourceAndPlay(clipToPlay));
    }

    private IEnumerator CreateTemporarySourceAndPlay(AudioClip clip)
    {
        GameObject tempGO = new GameObject("TempAudio_" + clip.name);
        tempGO.transform.SetParent(this.transform);
        AudioSource tempSource = tempGO.AddComponent<AudioSource>();

        if (_sfxSources.Length > 0 && _sfxSources[0] != null)
        {
            tempSource.outputAudioMixerGroup = _sfxSources[0].outputAudioMixerGroup;
            tempSource.spatialBlend = _sfxSources[0].spatialBlend;
            // CHECK 2: Obey the mute setting
            tempSource.mute = _isSfxMuted;
        }

        tempSource.PlayOneShot(clip);
        yield return new WaitForSeconds(clip.length);
        Destroy(tempGO);
    }

    // --- NEW METHODS (Called by SettingsManager) ---
    
    public void SetMusicMute(bool mute)
    {
        if (_bgmSource != null)
        {
            _bgmSource.mute = mute;
        }
    }

    public void SetSfxMute(bool mute)
    {
        _isSfxMuted = mute; // Store this for the PlaySFX() check
        
        // Mute all pooled SFX sources
        foreach (AudioSource source in _sfxSources)
        {
            if (source != null)
            {
                source.mute = mute;
            }
        }
    }
    // ---
}