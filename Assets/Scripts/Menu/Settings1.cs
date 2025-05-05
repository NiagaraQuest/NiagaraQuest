using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using System.Collections.Generic;

public class Settings1 : MonoBehaviour
{
    [Header("Volume Control")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Text musicPercentageText;
    [SerializeField] private TMP_Text sfxPercentageText;


    [Header("Panels")]
    [SerializeField] private GameObject settingsPanel;


    [Header("Buttons")]
    [SerializeField] private Button closeButton;
  

  
    [SerializeField] private AudioManager audioManager;
  

    void Start()
    {
       

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }

     
        // Get reference to AudioManager singleton
        audioManager = AudioManager.Instance;

        if (audioManager == null)
        {
            Debug.LogError("AudioManager instance not found!");
            return;
        }

        // Set initial slider values from AudioManager
        if (musicSlider != null)
        {
            musicSlider.value = audioManager.GetMusicVolume();
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = audioManager.GetSFXVolume();
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        // Update text displays
        UpdateMusicPercentageText(musicSlider != null ? musicSlider.value : 0);
        UpdateSFXPercentageText(sfxSlider != null ? sfxSlider.value : 0);

        // Add button listeners
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseButtonClicked);

        
    }


    #region Volume Control Methods

    private void OnMusicVolumeChanged(float value)
    {
        // Update AudioManager music volume
        if (audioManager != null)
        {
            audioManager.SetMusicVolume(value);
        }

        // Update percentage text
        UpdateMusicPercentageText(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        // Update AudioManager SFX volume
        if (audioManager != null)
        {
            audioManager.SetSFXVolume(value);
        }

        // Update percentage text
        UpdateSFXPercentageText(value);
    }

    private void UpdateMusicPercentageText(float value)
    {
        if (musicPercentageText != null)
        {
            // Convert to percentage and round to nearest integer
            int percentage = Mathf.RoundToInt(value * 100);
            musicPercentageText.text = percentage + "%";
        }
    }

    private void UpdateSFXPercentageText(float value)
    {
        if (sfxPercentageText != null)
        {
            // Convert to percentage and round to nearest integer
            int percentage = Mathf.RoundToInt(value * 100);
            sfxPercentageText.text = percentage + "%";
        }
    }

    #endregion



   

    private void OnCloseButtonClicked()
    {
        // Play button sound
        if (audioManager != null)
        {
            audioManager.PlayMenuButton();
        }

        // Hide or disable the settings panel
        gameObject.SetActive(false);
    }

   

  

  
  
}
