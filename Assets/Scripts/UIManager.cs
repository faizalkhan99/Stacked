using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
// Define the Category structure here
[System.Serializable]
public class ScoreCommentCategory
{
    public string categoryName = "New Category";
    [Header("Score Requirements")]
    public int minScore;
    public int maxScore;
    public bool isMaxScoreInfinite;
    [Header("Comment Asset")]
    public ScoreCommentAsset commentAsset;
}

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<UIManager>();
            if (instance == null) Debug.LogError("UIManager instance is null!");
            return instance;
        }
    }

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.3f;

    [Header("Canvas")]
    [SerializeField] private Canvas _mainMenuCanvas;
    [SerializeField] private Canvas _inGameCanvas;

    [Header("Main Menu Canvas Panels")]
    [SerializeField] private GameObject _mainMenuPanel;
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private GameObject _creditsPanel;
    [SerializeField] private TextMeshProUGUI mainMenuHighScoreText;

    [Header("In-Game Canvas Panels")]
    [SerializeField] private GameObject _pauseButtonPanel;
    [SerializeField] private GameObject _pauseMenuPanel;
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private GameObject _scoreTextPanel;

    [Header("Game Over Score Display")]
    [SerializeField] private TextMeshProUGUI gameOverScoreText;
    [SerializeField] private TextMeshProUGUI gameOverHighScoreText;
    [SerializeField] private TextMeshProUGUI gameOverCommentText;

    [Header("Combo UI")]
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private float comboPopScale = 1.5f;
    [SerializeField] private float comboPopDuration = 0.1f;
    [SerializeField] private float comboStayDuration = 0.8f;
    [SerializeField] private float comboFadeDuration = 0.3f;

    [Header("Score Comments")]
    [SerializeField] private ScoreCommentAsset newHighScoreComments;
    [SerializeField] private List<ScoreCommentCategory> scoreCommentCategories;

    // --- NEW: UI ELEMENT REFERENCES ---
    [Header("Settings Toggles")]
    [Tooltip("Drag the 'Music On/Off' Toggle component here.")]
    [SerializeField] private Toggle musicToggle;
    [Tooltip("Drag the 'SFX On/Off' Toggle component here.")]
    [SerializeField] private Toggle sfxToggle;
    [Tooltip("Drag the 'Vibrations On/Off' Toggle component here.")]
    [SerializeField] private Toggle vibrationToggle;

    [Header("Settings Sprites")]
    [Tooltip("Drag the 'Image' of the Music ON sprite.")]
    [SerializeField] private Image musicOnSprite;
    [Tooltip("Drag the 'Image' of the Music OFF sprite.")]
    [SerializeField] private Image musicOffSprite;
    [Tooltip("Drag the 'Image' of the SFX ON sprite.")]
    [SerializeField] private Image sfxOnSprite;
    [Tooltip("Drag the 'Image' of the SFX OFF sprite.")]
    [SerializeField] private Image sfxOffSprite;
    [Tooltip("Drag the 'Image' of the Vibration ON sprite.")]
    [SerializeField] private Image vibrationOnSprite;
    [Tooltip("Drag the 'Image' of the Vibration OFF sprite.")]
    [SerializeField] private Image vibrationOffSprite;
    // ---

    private TextMeshProUGUI _scoreText;
    private Dictionary<CanvasGroup, Coroutine> activeFades = new Dictionary<CanvasGroup, Coroutine>();
    private Coroutine _comboCoroutine;


    // --- Initialization ---
    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
        if (_scoreTextPanel != null)
        {
            _scoreText = _scoreTextPanel.GetComponentInChildren<TextMeshProUGUI>();
            if (_scoreText == null) Debug.LogError("UIManager: No TextMeshProUGUI found in _scoreTextPanel!");
        }

        if (comboText) comboText.gameObject.SetActive(false);
    }
    private void OnEnable()
    {
        // Ensure UI re-syncs every time this object reactivates (after scene reload)
        if (SettingsManager.Instance != null)
            InitializeSettingsUI();
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Re-sync UI after reload — this runs AFTER all persistent singletons are ready
        InitializeSettingsUI();
    }

    void Start()
    {
        InitializeSettingsUI();
        InstantHideAllMainMenuPanels();
        InstantHideAllInGamePanels();

        if (_mainMenuCanvas) _mainMenuCanvas.gameObject.SetActive(true);
        if (_inGameCanvas) _inGameCanvas.gameObject.SetActive(false);

        CanvasGroup cg = GetOrAddCanvasGroup(_mainMenuPanel);
        cg.alpha = 1; cg.interactable = true; cg.blocksRaycasts = true; cg.gameObject.SetActive(true);

        if (GameManager.Instance != null)
            UpdateMainMenuHighScore(GameManager.Instance.GetHighScore());
    }

    // --- NEW: Method to set UI state from SettingsManager ---
    private void InitializeSettingsUI()
    {
        var sm = SettingsManager.Instance;
        if (sm == null) { Debug.LogError("SettingsManager not found when initializing UI."); return; }


        // Get states
        bool musicOn = SettingsManager.Instance.IsMusicEnabled;
        bool sfxOn = SettingsManager.Instance.IsSfxEnabled;
        bool vibOn = SettingsManager.Instance.IsVibrationEnabled;

        // Apply to Toggles
        if (musicToggle != null) musicToggle.isOn = musicOn;
        if (sfxToggle != null) sfxToggle.isOn = sfxOn;
        if (vibrationToggle != null) vibrationToggle.isOn = vibOn;

        //Temporarily remove listeners to avoid reentrant calls / Toggle race conditions
        RemoveToggleListeners();

        // Force toggles to desired state
        if (musicToggle != null) musicToggle.isOn = musicOn;
        if (sfxToggle != null) sfxToggle.isOn = sfxOn;
        if (vibrationToggle != null) vibrationToggle.isOn = vibOn;

        // Force visual updates (use the same UpdateSpriteState helper)
        ForceUpdateSpriteState(musicOnSprite, musicOffSprite, musicOn);
        ForceUpdateSpriteState(sfxOnSprite, sfxOffSprite, sfxOn);
        ForceUpdateSpriteState(vibrationOnSprite, vibrationOffSprite, vibOn);

        // Re-add listeners
        AddToggleListeners();

        Debug.Log($"[UIManager] InitializeSettingsUI applied -> Music:{musicOn} SFX:{sfxOn} Vib:{vibOn} (frame:{Time.frameCount})");
        // Apply to Sprites
        UpdateSpriteState(musicOnSprite, musicOffSprite, musicOn);
        UpdateSpriteState(sfxOnSprite, sfxOffSprite, sfxOn);
        UpdateSpriteState(vibrationOnSprite, vibrationOffSprite, vibOn);
    }

    private void RemoveToggleListeners()
{
    if (musicToggle != null) musicToggle.onValueChanged.RemoveListener(OnMusicToggled);
    if (sfxToggle != null) sfxToggle.onValueChanged.RemoveListener(OnSfxToggled);
    if (vibrationToggle != null) vibrationToggle.onValueChanged.RemoveListener(OnVibrationsToggled);
}

private void AddToggleListeners()
{
    if (musicToggle != null)
    {
        musicToggle.onValueChanged.RemoveListener(OnMusicToggled); // ensure no duplicates
        musicToggle.onValueChanged.AddListener(OnMusicToggled);
    }
    if (sfxToggle != null)
    {
        sfxToggle.onValueChanged.RemoveListener(OnSfxToggled);
        sfxToggle.onValueChanged.AddListener(OnSfxToggled);
    }
    if (vibrationToggle != null)
    {
        vibrationToggle.onValueChanged.RemoveListener(OnVibrationsToggled);
        vibrationToggle.onValueChanged.AddListener(OnVibrationsToggled);
    }
}

// A defensive updater that forces GameObject active + Image.enabled + alpha
private void ForceUpdateSpriteState(Image onSprite, Image offSprite, bool isOn)
{
    try
    {
        if (onSprite != null)
        {
            onSprite.gameObject.SetActive(isOn);
            onSprite.enabled = isOn;
            // ensure fully visible (in case CanvasRenderer alpha altered)
            var cr = onSprite.canvasRenderer;
            if (cr != null) cr.SetAlpha(isOn ? 1f : 0f);
        }
        if (offSprite != null)
        {
            offSprite.gameObject.SetActive(!isOn);
            offSprite.enabled = !isOn;
            var cr2 = offSprite.canvasRenderer;
            if (cr2 != null) cr2.SetAlpha(!isOn ? 1f : 0f);
        }
    }
    catch (System.Exception ex)
    {
        Debug.LogWarning("[UIManager] ForceUpdateSpriteState exception: " + ex.Message);
    }
}

// Use this from Toggle callbacks (do not rely on Toggle visuals to change child GameObjects)
private void UpdateSpriteStateImmediate(Image onSprite, Image offSprite, bool isOn)
{
    // identical to ForceUpdateSpriteState but kept separate for clarity
    ForceUpdateSpriteState(onSprite, offSprite, isOn);
}

// --- Settings Toggle callbacks (replace your handlers) ---

public void OnMusicToggled(bool isOn)
{
    Debug.Log($"[UIManager] OnMusicToggled called. isOn={isOn} frame:{Time.frameCount}");
    // Persist setting
    if (SettingsManager.Instance != null)
        SettingsManager.Instance.ToggleMusic(isOn);

    // Force visual update
    UpdateSpriteStateImmediate(musicOnSprite, musicOffSprite, isOn);

    // Do not play SFX for music toggle to avoid confusing audio feedback
}

public void OnSfxToggled(bool isOn)
{
    Debug.Log($"[UIManager] OnSfxToggled called. isOn={isOn} frame:{Time.frameCount}");
    if (SettingsManager.Instance != null)
        SettingsManager.Instance.ToggleSFX(isOn);

    UpdateSpriteStateImmediate(sfxOnSprite, sfxOffSprite, isOn);

    // Play a confirmation sound only if turning SFX ON
    if (isOn && AudioManager.Instance != null)
    {
        AudioManager.Instance.PlaySFX(SoundID.ButtonClick);
    }
}

public void OnVibrationsToggled(bool isOn)
{
    Debug.Log($"[UIManager] OnVibrationsToggled called. isOn={isOn} frame:{Time.frameCount}");
    if (SettingsManager.Instance != null)
        SettingsManager.Instance.ToggleVibration(isOn);

    UpdateSpriteStateImmediate(vibrationOnSprite, vibrationOffSprite, isOn);

    if (isOn) SettingsManager.Instance.TryVibrate();
}


    // --- NEW: Reusable method to update toggle sprites ---
    private void UpdateSpriteState(Image onSprite, Image offSprite, bool isOn)
    {
        if (onSprite != null) onSprite.enabled = isOn;
        if (offSprite != null) offSprite.enabled = !isOn;
    }

    // --- Panel Management (Fading) ---

    private void HideAllMainMenuPanels()
    {
        StartFade(_mainMenuPanel, false);
        StartFade(_settingsPanel, false);
        StartFade(_creditsPanel, false);
    }

    private void HideAllInGamePanels()
    {
        StartFade(_pauseButtonPanel, false);
        StartFade(_pauseMenuPanel, false);
        StartFade(_gameOverPanel, false);
        if (_scoreTextPanel) _scoreTextPanel.SetActive(false);
        if (comboText) HideComboText();
    }

    public void ShowMainMenu()
    {
        if (_mainMenuCanvas) _mainMenuCanvas.gameObject.SetActive(true);
        if (_inGameCanvas) _inGameCanvas.gameObject.SetActive(false);

        HideAllInGamePanels();
        HideAllMainMenuPanels();
        StartFade(_mainMenuPanel, true);

        // Update High Score when returning to main menu
        UpdateMainMenuHighScore(GameManager.Instance.GetHighScore());
    }

    public void ShowSettings()
    {
        HideAllMainMenuPanels();
        StartFade(_settingsPanel, true);
        InitializeSettingsUI();
    }

    public void ShowCredits()
    {
        HideAllMainMenuPanels();
        StartFade(_creditsPanel, true);
    }

    public void ShowGameUI()
    {
        if (_mainMenuCanvas) _mainMenuCanvas.gameObject.SetActive(false);
        if (_inGameCanvas) _inGameCanvas.gameObject.SetActive(true);

        HideAllMainMenuPanels();
        HideAllInGamePanels();

        StartFade(_pauseButtonPanel, true);
        if (_scoreTextPanel) _scoreTextPanel.SetActive(true);
        if (comboText) comboText.gameObject.SetActive(false);
        UpdateScoreText(0);
    }

    public void ShowPauseMenu(bool show)
    {
        StartFade(_pauseMenuPanel, show);
        StartFade(_pauseButtonPanel, !show);
    }

    public void ShowGameOver(int finalScore, int highScore, bool isNewHighScore)
    {
        HideAllInGamePanels();
        if (gameOverScoreText) gameOverScoreText.text = finalScore.ToString();
        if (gameOverHighScoreText) gameOverHighScoreText.text = highScore.ToString();
        if (gameOverCommentText) gameOverCommentText.text = GetScoreComment(finalScore, isNewHighScore);
        StartFade(_gameOverPanel, true);
    }

    // --- Data Display ---
    public void UpdateScoreText(int score)
    {
        if (_scoreText) _scoreText.text = score.ToString();
    }

    public void UpdateMainMenuHighScore(int score)
    {
        if (mainMenuHighScoreText)
        {
            mainMenuHighScoreText.text = $"High Score: {score}";
        }
    }

    private string GetScoreComment(int finalScore, bool isNewHighScore)
    {
        if (isNewHighScore && newHighScoreComments != null && newHighScoreComments.comments != null && newHighScoreComments.comments.Count > 0)
        {
            return GetRandomCommentFromAsset(newHighScoreComments);
        }
        if (scoreCommentCategories != null)
        {
            foreach (var category in scoreCommentCategories)
            {
                if (category.commentAsset == null) continue;
                bool inRange = (finalScore >= category.minScore && finalScore <= category.maxScore);
                bool aboveMin = (finalScore >= category.minScore);
                if (category.isMaxScoreInfinite && aboveMin)
                {
                    return GetRandomCommentFromAsset(category.commentAsset);
                }
                else if (!category.isMaxScoreInfinite && inRange)
                {
                    return GetRandomCommentFromAsset(category.commentAsset);
                }
            }
            // Fallback to last category if score is higher than all defined ranges
            if (scoreCommentCategories.Count > 0)
            {
                var lastCategory = scoreCommentCategories[^1];
                if (lastCategory.commentAsset != null && lastCategory.commentAsset.comments != null && lastCategory.commentAsset.comments.Count > 0)
                {
                    return GetRandomCommentFromAsset(lastCategory.commentAsset);
                }
            }
        }
        return "Good Game!";
    }

    private string GetRandomCommentFromAsset(ScoreCommentAsset asset)
    {
        if (asset.comments == null || asset.comments.Count == 0)
        {
            return "Well done!";
        }
        return asset.comments[Random.Range(0, asset.comments.Count)];
    }


    public void ShowComboText(int combo)
    {
        if (comboText == null) return;
        if (combo < 2) { HideComboText(); return; }
        comboText.text = $"X{combo}";
        if (_comboCoroutine != null) StopCoroutine(_comboCoroutine);
        _comboCoroutine = StartCoroutine(AnimateComboPop());
    }

    public void HideComboText()
    {
        if (comboText == null) return;
        if (comboText.gameObject.activeSelf)
        {
            if (_comboCoroutine != null) StopCoroutine(_comboCoroutine);
            _comboCoroutine = StartCoroutine(FadeText(comboText, 0f, comboFadeDuration));
        }
    }

    private IEnumerator AnimateComboPop()
    {
        comboText.gameObject.SetActive(true);
        float timer = 0f;
        Color color = comboText.color; color.a = 1f; comboText.color = color;
        Vector3 startScale = Vector3.one;
        Vector3 targetScale = Vector3.one * comboPopScale;
        while (timer < comboPopDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, timer / comboPopDuration);
            comboText.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        comboText.transform.localScale = Vector3.one;
        yield return new WaitForSecondsRealtime(comboStayDuration);
        _comboCoroutine = StartCoroutine(FadeText(comboText, 0f, comboFadeDuration));
    }

    private IEnumerator FadeText(TextMeshProUGUI text, float targetAlpha, float duration)
    {
        if (targetAlpha > 0) text.gameObject.SetActive(true);
        float startAlpha = text.color.a;
        float timer = 0f;
        Color startColor = text.color;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / duration);
            text.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(startAlpha, targetAlpha, t));
            yield return null;
        }
        text.color = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
        if (targetAlpha == 0) text.gameObject.SetActive(false);
        _comboCoroutine = null;
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject panel)
    {
        if (panel == null) return null;
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) { cg = panel.AddComponent<CanvasGroup>(); }
        return cg;
    }

    private void StartFade(GameObject panel, bool fadeIn)
    {
        CanvasGroup cg = GetOrAddCanvasGroup(panel);
        if (cg == null) return;
        if (activeFades.ContainsKey(cg))
        {
            if (activeFades[cg] != null) StopCoroutine(activeFades[cg]);
            activeFades.Remove(cg);
        }
        float targetAlpha = fadeIn ? 1f : 0f;
        Coroutine newFade = StartCoroutine(FadePanel(cg, targetAlpha, fadeDuration));
        activeFades.Add(cg, newFade);
    }

    private IEnumerator FadePanel(CanvasGroup group, float targetAlpha, float duration)
    {
        if (targetAlpha > 0)
        {
            group.alpha = 0;
            group.interactable = false;
            group.blocksRaycasts = true;
            group.gameObject.SetActive(true);
        }
        else { group.interactable = false; }
        float startAlpha = group.alpha;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / duration);
            group.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }
        group.alpha = targetAlpha;
        if (targetAlpha == 0)
        {
            group.blocksRaycasts = false;
            group.gameObject.SetActive(false);
        }
        else { group.interactable = true; }
        if (activeFades.ContainsKey(group)) activeFades.Remove(group);
    }

    private void InstantHidePanel(GameObject panel)
    {
        if (panel == null) return;
        CanvasGroup cg = GetOrAddCanvasGroup(panel);
        cg.alpha = 0; cg.interactable = false; cg.blocksRaycasts = false; cg.gameObject.SetActive(false);
    }

    private void InstantHideAllMainMenuPanels()
    {
        InstantHidePanel(_mainMenuPanel);
        InstantHidePanel(_settingsPanel);
        InstantHidePanel(_creditsPanel);
    }

    private void InstantHideAllInGamePanels()
    {
        InstantHidePanel(_pauseButtonPanel);
        InstantHidePanel(_pauseMenuPanel);
        InstantHidePanel(_gameOverPanel);
        if (_scoreTextPanel) _scoreTextPanel.SetActive(false);
    }

    // -- Main Menu Buttons --
    public void OnPlayButtonClicked()
    {
        PlayButtonClickSFX();
        ShowGameUI();
        GameManager.Instance.StartGame();
    }
    public void OnSettingsButtonClicked()
    {
        PlayButtonClickSFX();
        ShowSettings();
    }
    public void OnCreditsButtonClicked()
    {
        PlayButtonClickSFX();
        ShowCredits();
    }
    public void OnQuitButtonClicked()
    {
        PlayButtonClickSFX();
        Debug.Log("Quitting Game...");
        Application.Quit();
    }

    // -- Settings Buttons --
    public void OnBackFromSettingsClicked()
    {
        PlayButtonClickSFX();
        ShowMainMenu();
    }
    // public void OnMusicToggled(bool isOn)
    // {
    //     // Don't play a click sound when toggling music/sfx
    //     Debug.Log("Music Toggled: " + isOn);
    //     SettingsManager.Instance.ToggleMusic(isOn);
    //     UpdateSpriteState(musicOnSprite, musicOffSprite, isOn); // Update sprites immediately
    // }

    // public void OnSfxToggled(bool isOn)
    // {
    //     Debug.Log("SFX Toggled: " + isOn);
    //     SettingsManager.Instance.ToggleSFX(isOn);
    //     UpdateSpriteState(sfxOnSprite, sfxOffSprite, isOn); // Update sprites immediately

    //     if (isOn) PlayButtonClickSFX(); // Play sound *after* setting
    // }

    // public void OnVibrationsToggled(bool isOn)
    // {
    //     PlayButtonClickSFX();
    //     Debug.Log("Vibrations Toggled: " + isOn);
    //     SettingsManager.Instance.ToggleVibration(isOn);
    //     UpdateSpriteState(vibrationOnSprite, vibrationOffSprite, isOn); // Update sprites immediately
    // }

    // -- Credits Buttons --
    public void OnBackFromCreditsClicked()
    {
        PlayButtonClickSFX();
        ShowMainMenu();
    }

    // -- In-Game Buttons --
    public void OnPauseButtonClicked()
    {
        PlayButtonClickSFX();
        GameManager.Instance.PauseGame();
    }

    // -- Pause Menu Buttons --
    public void OnResumeButtonClicked()
    {
        PlayButtonClickSFX();
        GameManager.Instance.ResumeGame();
    }
    public void OnHomeButtonClicked()
    {
        PlayButtonClickSFX();
        GameManager.Instance.GoToMainMenu();
    }

    // -- Game Over Buttons --
    public void OnRestartButtonClicked()
    {
        PlayButtonClickSFX();
        GameManager.Instance.RestartGame();
    }


    // --- Utility Functions ---
    public void PlayButtonClickSFX()
    {
        // This check now works because SettingsManager is persistent
        if (SettingsManager.Instance != null && SettingsManager.Instance.IsSfxEnabled)
        {
            Debug.Log("Button Click SFX Played.");
            AudioManager.Instance.PlaySFX(SoundID.ButtonClick); 
        }
    }
    public void ExternalLinks(string url)
    {
        PlayButtonClickSFX();
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogError("URL is empty!");
            return;
        }
        Application.OpenURL(url);
    }
}