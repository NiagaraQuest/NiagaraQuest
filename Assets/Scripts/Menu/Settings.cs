using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using System.Collections.Generic;

public class Settings : MonoBehaviour
{
    [Header("Volume Control")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Text musicPercentageText;
    [SerializeField] private TMP_Text sfxPercentageText;

    [Header("Profile Container")]
    [SerializeField] private Transform profileContainer;
    [SerializeField] private GameObject profileEntryPrefab;

    [Header("Panels")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject editPanel;

    [Header("Buttons")]
    [SerializeField] private Button editProfilesButton;
    [SerializeField] private Button clearCacheButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button returnToSettingsButton;

    private List<Profile> profiles = new List<Profile>();
    private AudioManager audioManager;
    private bool isDatabaseInitialized = false;

    void Start()
    {
        // Initialize panels
        if (editPanel != null)
        {
            editPanel.SetActive(false);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }

        returnToSettingsButton.onClick.AddListener(OnReturnToSettingsButton);
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

        if (editProfilesButton != null)
            editProfilesButton.onClick.AddListener(OnEditProfilesButtonClicked);

        if (clearCacheButton != null)
            clearCacheButton.onClick.AddListener(OnClearCacheButtonClicked);

        if (returnToSettingsButton != null)
            returnToSettingsButton.onClick.AddListener(OnReturnToSettingsButton);

        // Initialize database in the background
        _ = InitializeDatabaseAsync();
    }

    #region Database Methods

    private async Task InitializeDatabaseAsync()
    {
        try
        {
            Debug.Log("Initializing database...");
            await DatabaseManager.Instance.Initialize();
            isDatabaseInitialized = true;
            Debug.Log("Database initialized successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to initialize database: {e.Message}");
            isDatabaseInitialized = false;
        }
    }

    #endregion

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

    #region Profile Management Methods

    private async Task LoadProfiles()
    {
        if (!isDatabaseInitialized)
        {
            Debug.LogWarning("Database not initialized yet");
            return;
        }

        try
        {
            // Clear existing profile entries
            foreach (Transform child in profileContainer)
            {
                Destroy(child.gameObject);
            }

            // Load profiles from database
            profiles = await ProfileManager.Instance.GetAllProfiles();
            Debug.Log($"Loaded {profiles.Count} profiles from database");

            // Create UI entries for each profile
            foreach (var profile in profiles)
            {
                CreateProfileEntry(profile);
            }

            if (profiles.Count == 0)
            {
                Debug.Log("No profiles found in database.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading profiles: {e.Message}");
        }
    }

    private void CreateProfileEntry(Profile profile)
    {
        if (profileEntryPrefab == null || profileContainer == null)
        {
            Debug.LogError("Profile entry prefab or container is not assigned");
            return;
        }

        // Instantiate the profile entry prefab
        GameObject entryObject = Instantiate(profileEntryPrefab, profileContainer);
        ProfileEntry1 entry = entryObject.GetComponent<ProfileEntry1>();

        if (entry == null)
        {
            Debug.LogError("ProfileEntry1 component not found on prefab");
            return;
        }

        // Initialize the entry with profile data
        entry.Initialize(profile);

        // Set the save action - Check both method signatures
        if (entry.GetType().GetMethod("SetSaveAction") != null)
        {
            // If it has the SetSaveAction method
            entry.SetSaveAction(async (profileToUpdate, newUsername) =>
            {
                await SaveProfile(profileToUpdate, newUsername);
            });

            // Set the delete action
            entry.SetDeleteAction(async (profileToDelete) =>
            {
                await DeleteProfile(profileToDelete);
            });
        }
        else if (entry.GetType().GetMethod("SetSavAction") != null)
        {
            entry.SetSaveAction(async (profileToUpdate, newUsername) =>
            {
                await SaveProfile(profileToUpdate, newUsername);
            });

            // Set the delete action
            entry.SetDeleteAction(async (profileToDelete) =>
            {
                await DeleteProfile(profileToDelete);
            });
        }
    }

  

    private async Task SaveProfile(Profile profile, string newUsername)
    {
        if (!isDatabaseInitialized)
        {
            Debug.LogWarning("Database not initialized yet");
            return;
        }

        try
        {
            // Check if the username is valid
            if (string.IsNullOrEmpty(newUsername) || newUsername.Trim().Length < 3)
            {
                Debug.LogWarning("Username must be at least 3 characters");
                return;
            }

            // Check if the username is changed
            if (profile.Username != newUsername)
            {
                profile.Username = newUsername;

                // Save the updated profile to the database
                await ProfileManager.Instance.UpdateProfile(profile);

                Debug.Log("Profile updated successfully");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving profile: {e.Message}");
        }
    }

    private async Task DeleteProfile(Profile profile)
    {
        if (!isDatabaseInitialized)
        {
            Debug.LogWarning("Database not initialized yet");
            return;
        }

        try
        {
            // Delete the profile from the database
            await ProfileManager.Instance.DeleteProfile(profile.Id);

            // Reload the profiles
            await LoadProfiles();

            Debug.Log("Profile deleted successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error deleting profile: {e.Message}");
        }
    }

    #endregion

    #region Button Event Handlers

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

    private  async void OnReturnToSettingsButton()
    {
        // Play button sound
        if (audioManager != null)
        {
            audioManager.PlayMenuButton();
        }

        // Switch panels
        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (editPanel != null) editPanel.SetActive(false);
        RefreshProfiles();
    }   

    private async void OnEditProfilesButtonClicked()
    {
        // Play button sound
        if (audioManager != null)
        {
            audioManager.PlayMenuButton();
        }

        // Switch panels
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (editPanel != null)
        {
            editPanel.SetActive(true);

            // Load profiles when entering the edit panel
            await LoadProfiles();
        }
    }

    private async void OnClearCacheButtonClicked()
    {
        // Play button sound
        if (audioManager != null)
        {
            audioManager.PlayMenuButton();
        }

        Debug.Log("Clear Cache button clicked");

        // Delete all profiles from the database
        if (isDatabaseInitialized)
        {
            try
            {
                var profiles = await ProfileManager.Instance.GetAllProfiles();
                Debug.Log($"Found {profiles.Count} profiles to delete");

                foreach (var profile in profiles)
                {
                    await DatabaseManager.Instance.Delete<Profile>(profile.Id);
                }

                // Also clear PlayerPrefs for good measure
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();

                Debug.Log("All profiles deleted successfully!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to delete profiles: {e.Message}");
            }
        }
        else
        {
            // If database isn't initialized yet, just clear PlayerPrefs
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("Database not initialized, cleared PlayerPrefs only");
        }
    }

    #endregion

    // Public method to refresh profiles (can be called from UI button)
    public async void RefreshProfiles()
    {
        if (audioManager != null)
            audioManager.PlayMenuButton();

        await LoadProfiles();
    }
}
