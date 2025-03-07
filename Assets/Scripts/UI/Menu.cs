#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;


public class CreditsMenu : MonoBehaviour
{
    public GameObject MainMenu;
    public Button ExitButton;
    public GameObject creditsPanel; 
    public Button creditsButton;    
    public Button closeButton;    
    public GameObject RulesPanel;
    public Button ReturnButton;
    public Button RulesButton;
    public GameObject SettingsPanel;
    public Button SettingsButton;
    public Button ReturnSettings;
    

    void Start()
    {
        MainMenu.SetActive(true);
        creditsPanel.SetActive(false);
        RulesPanel.SetActive(false);
        SettingsPanel.SetActive(false);
        ReturnSettings.onClick.AddListener(HideSettings);
        SettingsButton.onClick.AddListener(ShowSettings);
        creditsButton.onClick.AddListener(ShowCredits);
        closeButton.onClick.AddListener(HideCredits);
        RulesButton.onClick.AddListener(ShowRules);
        ReturnButton.onClick.AddListener(HideRules);
        ExitButton.onClick.AddListener(ExitGame);


    }

    void ShowCredits()
    {
        creditsPanel.SetActive(true);
    }

    void HideCredits()
    {
        creditsPanel.SetActive(false);
    }
    void ShowRules()
    {
        RulesPanel.SetActive(true);
        MainMenu.SetActive(false);

    }

    void HideSettings()
    {
        SettingsPanel.SetActive(false);
        MainMenu.SetActive(true);
    }
    void ShowSettings()
    {
        SettingsPanel.SetActive(true);
        MainMenu.SetActive(false);

    }

    void HideRules()
    {
        RulesPanel.SetActive(false);
        MainMenu.SetActive(true);
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
