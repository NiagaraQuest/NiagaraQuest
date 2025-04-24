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

    public void Initialize(Profile profile)
    {
        this.profileData = profile;

        if (usernameText != null)
            usernameText.text = profile.Username;

        if (eloText != null)
            eloText.text = profile.Elo.ToString();

        if (selectButton != null)
            selectButton.onClick.AddListener(() => { if (onSelectAction != null) onSelectAction(); });
    }

    public void SetSelectAction(Action action)
    {
        onSelectAction = action;
    }
}