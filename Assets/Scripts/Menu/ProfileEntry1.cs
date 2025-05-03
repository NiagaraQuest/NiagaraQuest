using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ProfileEntry1 : MonoBehaviour
{
    public TMP_InputField usernameText;
    public Button saveButton;
    public Button deleteButton;

    private Profile profileData;
    private Action<Profile, string> onSaveAction;
    private Action<Profile> onDeleteAction;
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

        if (deleteButton != null)
            deleteButton.onClick.AddListener(() => {
                // Play button sound
                if (audioManager != null)
                    audioManager.PlayMenuButton();

                // Call the delete action
                if (onDeleteAction != null)
                    onDeleteAction(profileData);
            });

        if (saveButton != null)
            saveButton.onClick.AddListener(() => {
                // Play button sound
                if (audioManager != null)
                    audioManager.PlayMenuButton();

                // Call the save action with the updated username
                if (onSaveAction != null)
                    onSaveAction(profileData, usernameText.text);
            });
    }

    public void SetDeleteAction(Action<Profile> action)
    {
        onDeleteAction = action;
    }

    public void SetSaveAction(Action<Profile, string> action)
    {
        onSaveAction = action;
    }
}