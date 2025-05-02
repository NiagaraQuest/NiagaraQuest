#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject MainMenuPanel;
    public Button ExitButton;
    public GameObject creditsPanel;
    public Button creditsButton;
    public Button closeButton;
    public GameObject RulesPanel;
    public Button ReturnButton;
    public Button RulesButton;
    public Button exitSettings;
    public Button settings;
    public GameObject settingsPanel;
    public GameObject news;
    public GameObject newGamePanel;
    public Button newGameButton;
    public Button backFromNewGameButton;

    // Reference to the AudioManager
    private AudioManager audioManager;

    void Start()
    {
        // Get reference to AudioManager singleton
        audioManager = AudioManager.Instance;

        // Initialize panel states
        MainMenuPanel.SetActive(true);
        creditsPanel.SetActive(false);
        RulesPanel.SetActive(false);
        settingsPanel.SetActive(false);
        newGamePanel.SetActive(false);

        // Set up button listeners
        creditsButton.onClick.AddListener(ShowCredits);
        closeButton.onClick.AddListener(HideCredits);
        RulesButton.onClick.AddListener(ShowRules);
        ReturnButton.onClick.AddListener(HideRules);
        ExitButton.onClick.AddListener(ExitGame);
        settings.onClick.AddListener(ShowSettings);
        exitSettings.onClick.AddListener(HideSettings);
        newGameButton.onClick.AddListener(ShowNewGame);
        backFromNewGameButton.onClick.AddListener(HideNewGame);
    }

    void ShowCredits()
    {
        // Play button sound
        if (audioManager != null)
            audioManager.PlayMenuButton();
        
        creditsPanel.SetActive(true);
    }

    void HideCredits()
    {
        // Play button sound
        if (audioManager != null)
            audioManager.PlayMenuButton();
            
        creditsPanel.SetActive(false);
    }

    void ShowSettings()
    {
        // Play button sound
        if (audioManager != null)
            audioManager.PlayMenuButton();
            
        settingsPanel.SetActive(true);
        
    }

    void HideSettings()
    {
        // Play button sound
        if (audioManager != null)
            audioManager.PlayMenuButton();
            
        
        settingsPanel.SetActive(false);
    }

    void ShowRules()
    {
        // Play button sound
        if (audioManager != null)
            audioManager.PlayMenuButton();
            
        RulesPanel.SetActive(true);
        MainMenuPanel.SetActive(false);
    }

    void HideRules()
    {
        // Play button sound
        if (audioManager != null)
            audioManager.PlayMenuButton();
            
        RulesPanel.SetActive(false);
        MainMenuPanel.SetActive(true);
    }

    void ShowNewGame()
    {
        // Play button sound
        if (audioManager != null)
            audioManager.PlayMenuButton();
            
        newGamePanel.SetActive(true);
        MainMenuPanel.SetActive(false);
    }

    void HideNewGame()
    {
        // Play button sound
        if (audioManager != null)
            audioManager.PlayMenuButton();
            
        newGamePanel.SetActive(false);
        MainMenuPanel.SetActive(true);
    }
   
    void ExitGame()
    {
        // Play button sound
        if (audioManager != null)
            audioManager.PlayMenuButton();
            
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}