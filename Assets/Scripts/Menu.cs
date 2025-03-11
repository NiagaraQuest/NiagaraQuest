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

    void Start()
    {
        MainMenu.SetActive(true);
        creditsPanel.SetActive(false);
        RulesPanel.SetActive(false);
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
