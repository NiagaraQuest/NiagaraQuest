using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ProfileEntry : MonoBehaviour
{
    public TMP_Text usernameText;
    public TMP_Text eloText;
    public Button selectButton;

    private Profile profileData;
    private Action onSelectAction;
    // Reference to the AudioManager
    private AudioManager audioManager;

    void Awake()
    {
        // Get reference to AudioManager singleton
        audioManager = AudioManager.Instance;
    }

    public void Initialize(Profile profile)
    {
        this.profileData = profile;

        if (usernameText != null)
            usernameText.text = profile.Username;

        if (eloText != null)
            eloText.text = "ELO : " + profile.Elo.ToString();

        if (selectButton != null)
            selectButton.onClick.AddListener(() => {
                // Play button sound
                if (audioManager != null)
                    audioManager.PlayMenuButton();
                    
                // Call the select action
                if (onSelectAction != null) 
                    onSelectAction();
            });
    }

    public void SetSelectAction(Action action)
    {
        onSelectAction = action;
    }
}