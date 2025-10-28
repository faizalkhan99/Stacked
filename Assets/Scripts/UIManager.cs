using TMPro;
using UnityEngine;
using UnityEngine.UI; // Needed for Button potentially, though refs are GameObject

public class UIManager : MonoBehaviour
{
    // --- Singleton Pattern ---
    private static UIManager instance;
    public static UIManager Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("UIManager instance is null!");
            }
            return instance;
        }
    }

    // --- References ---
    [Header("Main Menu Canvas Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;
    // Add references for specific Settings elements if needed (Toggles, etc.)

    [Header("In-Game Canvas Panels")]
    [SerializeField] private GameObject pauseButtonPanel; // Panel holding just the pause button
    [SerializeField] private GameObject pauseMenuPanel;   // Panel shown when paused
    [SerializeField] private GameObject gameOverPanel;    // Panel shown on game over
    [SerializeField] private TextMeshProUGUI scoreText;

    // Optional: Add references for Audio Manager, Settings Manager if handling toggles here

    // --- Initialization ---
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // Enforce singleton
            return;
        }
        instance = this;
        // Optional: DontDestroyOnLoad(gameObject); if UI persists across scenes
    }

    void Start()
    {
        // Initial state: Show Main Menu, hide everything else
        ShowMainMenu(); // Assumes starting in main menu
        // If starting directly in game, call ShowGameUI() instead
        if (scoreText) scoreText.text = "0"; // Initialize score display
    }

    // --- Panel Management ---

    private void HideAllMainMenuPanels()
    {
        if (mainMenuPanel) mainMenuPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (creditsPanel) creditsPanel.SetActive(false);
    }

    private void HideAllInGamePanels()
    {
        if (pauseButtonPanel) pauseButtonPanel.SetActive(false);
        if (pauseMenuPanel) pauseMenuPanel.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (scoreText) scoreText.gameObject.SetActive(false);
    }

    public void ShowMainMenu()
    {
        HideAllInGamePanels(); // Hide game UI
        HideAllMainMenuPanels(); // Hide other main menu panels
        if (mainMenuPanel) mainMenuPanel.SetActive(true); // Show main menu
        // Ensure correct Canvas is active/inactive if needed
        // mainMenuCanvas.SetActive(true);
        // inGameCanvas.SetActive(false);
    }

    public void ShowSettings()
    {
        HideAllMainMenuPanels();
        if (settingsPanel) settingsPanel.SetActive(true);
    }

    public void ShowCredits()
    {
        HideAllMainMenuPanels();
        if (creditsPanel) creditsPanel.SetActive(true);
    }

    public void ShowGameUI()
    {
        HideAllMainMenuPanels(); // Hide main menu UI
        HideAllInGamePanels();   // Hide pause/game over panels initially
        if (pauseButtonPanel) pauseButtonPanel.SetActive(true); // Show the pause button
        if (scoreText) scoreText.gameObject.SetActive(true); // Show the score text
        UpdateScoreText(0); // Reset score display visually
        // Ensure correct Canvas is active/inactive if needed
        // mainMenuCanvas.SetActive(false);
        // inGameCanvas.SetActive(true);
    }

    public void ShowPauseMenu(bool show)
    {
        if (pauseMenuPanel) pauseMenuPanel.SetActive(show);
        if (pauseButtonPanel) pauseButtonPanel.SetActive(!show); // Hide pause button when menu is up
        // Note: GameManager should handle Time.timeScale
    }

     public void ShowGameOver()
    {
        HideAllInGamePanels(); // Hide pause button, score
        if (gameOverPanel) gameOverPanel.SetActive(true); // Show game over screen
         // Note: GameManager should handle Time.timeScale
    }


    // --- Data Display ---

    public void UpdateScoreText(int score)
    {
        if (scoreText)
        {
            scoreText.text = score.ToString();
        }
    }

    // --- Button Actions (Called by UI Button OnClick events) ---

    // -- Main Menu Buttons --
    public void OnPlayButtonClicked()
    {
        PlayButtonClickSFX(); // Optional: Play sound
        ShowGameUI();
        // Tell GameManager to start the actual game logic
        GameManager.Instance.StartGame(); // Assumes GameManager has a Singleton 'Instance' and StartGame() method
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
        Debug.Log("Quitting Game..."); // For editor testing
        Application.Quit();
    }

    // -- Settings Buttons --
    public void OnBackFromSettingsClicked()
    {
        PlayButtonClickSFX();
        ShowMainMenu();
    }
    public void OnMusicToggled(bool isOn) // Example: Connect to Toggle UI element
    {
         PlayButtonClickSFX();
        Debug.Log("Music Toggled: " + isOn);
        // Call your AudioManager or SettingsManager here
        // AudioManager.Instance.SetMusicEnabled(isOn);
    }
     public void OnSfxToggled(bool isOn) // Example
    {
        PlayButtonClickSFX();
        Debug.Log("SFX Toggled: " + isOn);
        // AudioManager.Instance.SetSfxEnabled(isOn);
    }
     public void OnVibrationsToggled(bool isOn) // Example
    {
         PlayButtonClickSFX();
        Debug.Log("Vibrations Toggled: " + isOn);
        // SettingsManager.Instance.SetVibrationsEnabled(isOn);
    }
    // Link buttons use ExternalLinks method below

    // -- Credits Buttons --
     public void OnBackFromCreditsClicked()
    {
        PlayButtonClickSFX();
        ShowMainMenu();
    }
    // Link buttons use ExternalLinks method below


    // -- In-Game Buttons --
    public void OnPauseButtonClicked()
    {
         PlayButtonClickSFX();
        // Tell GameManager to handle pausing game logic and time
        GameManager.Instance.PauseGame(); // Assumes GameManager has PauseGame() method
        // GameManager will then likely call UIManager.ShowPauseMenu(true)
    }

    // -- Pause Menu Buttons --
    public void OnResumeButtonClicked()
    {
         PlayButtonClickSFX();
         // Tell GameManager to handle resuming game logic and time
         GameManager.Instance.ResumeGame(); // Assumes GameManager has ResumeGame() method
         // GameManager will then likely call UIManager.ShowPauseMenu(false)
    }

     public void OnHomeButtonClicked() // From Pause or Game Over
    {
         PlayButtonClickSFX();
         ShowMainMenu();
         // Tell GameManager to reset game state and potentially load Main Menu scene
         GameManager.Instance.GoToMainMenu(); // Assumes GameManager has GoToMainMenu() method
    }

     // -- Game Over Buttons --
     public void OnRestartButtonClicked() // From Game Over
    {
         PlayButtonClickSFX();
         ShowGameUI(); // Show the game UI again immediately
         // Tell GameManager to restart the game logic
         GameManager.Instance.RestartGame(); // Assumes GameManager has RestartGame() method
    }


    // --- Utility Functions ---

    public void PlayButtonClickSFX()
    {
        // Call your AudioManager here if you have one
         Debug.Log("Button Click SFX Played (Placeholder)");
        // AudioManager.Instance.PlaySFX(SoundID.ButtonClick);
    }

    public void ExternalLinks(string url)
    {
        PlayButtonClickSFX(); // Optional sound for link clicks
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogError("ExternalLinks: URL is empty or null!");
            return;
        }
        Application.OpenURL(url);
    }
}