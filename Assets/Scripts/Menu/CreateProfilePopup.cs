using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreateProfilePopup : MonoBehaviour
{
    public GameObject popupPanel;
    public TMP_InputField usernameInput;
    public Button createButton;
    public Button cancelButton;

    private NewGameMenuManager menuManager;
    // Reference to the AudioManager
    private AudioManager audioManager;

   
    void Start()
    {
        // Get reference to the menu manager
        menuManager = FindAnyObjectByType<NewGameMenuManager>();
        
        // Get reference to AudioManager singleton
        audioManager = AudioManager.Instance;

        // Clear any existing listeners to avoid duplicates
        createButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();

        // Add new listeners
        createButton.onClick.AddListener(CreateProfile);
        cancelButton.onClick.AddListener(ClosePopup);

        // Hide popup initially
        popupPanel.SetActive(false);

        Debug.Log("CreateProfilePopup initialized");
    }

    public void ShowPopup()
    {
        Debug.Log("Showing create profile popup");
        popupPanel.SetActive(true);
        usernameInput.text = "";
        usernameInput.Select();
    }

    public void ClosePopup()
    {
        // Play button sound
        if (audioManager != null)
            audioManager.PlayMenuButton();
            
        Debug.Log("Closing create profile popup");
        popupPanel.SetActive(false);
    }

    private void CreateProfile()
    {
        // Play button sound
        if (audioManager != null)
            audioManager.PlayMenuButton();
            
        string username = usernameInput.text.Trim();

        if (string.IsNullOrEmpty(username))
        {
            Debug.Log("Username cannot be empty");
            return;
        }

        Debug.Log($"Creating profile: {username}");
        // Implement actual profile creation later

        // Close popup
        ClosePopup();
    }
}