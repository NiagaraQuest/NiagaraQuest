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

    void Start()
    {
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
        creditsPanel.SetActive(true);
    }

    void HideCredits()
    {
        creditsPanel.SetActive(false);
    }

    void ShowSettings()
    {
        settingsPanel.SetActive(true);
        news.SetActive(false);
    }

    void HideSettings()
    {
        news.SetActive(true);
        settingsPanel.SetActive(false);
    }

    void ShowRules()
    {
        RulesPanel.SetActive(true);
        MainMenuPanel.SetActive(false);
    }

    void HideRules()
    {
        RulesPanel.SetActive(false);
        MainMenuPanel.SetActive(true);
    }

    void ShowNewGame()
    {
        newGamePanel.SetActive(true);
        MainMenuPanel.SetActive(false);
    }

    void HideNewGame()
    {
        newGamePanel.SetActive(false);
        MainMenuPanel.SetActive(true);
    }
   
    void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}