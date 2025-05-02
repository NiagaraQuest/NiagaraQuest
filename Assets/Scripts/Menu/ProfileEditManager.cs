using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ProfileEditManager : MonoBehaviour
{
    [Header("Profile Container")]
    [SerializeField] private Transform profileContainer;
    [SerializeField] private GameObject profileEntryPrefab;
    [SerializeField] private Button ReturnToSettings;

    private List<Profile> profiles = new List<Profile>();
    private AudioManager audioManager;
    private bool isDatabaseInitialized = false;

    void Start()
    {
       
        // Get reference to AudioManager singleton
        audioManager = AudioManager.Instance;

        // Initialize database and load profiles
        _ = InitializeDatabaseAndLoadProfiles();
    }

    private async Task InitializeDatabaseAndLoadProfiles()
    {
        try
        {
            // Initialize database
            await DatabaseManager.Instance.Initialize();
            isDatabaseInitialized = true;

            // Load profiles
            await LoadProfiles();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to initialize database: {e.Message}");
        }
    }

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

        // Set the save action
        entry.SetSaveAction(async (profileToUpdate, newUsername) => {
            await SaveProfile(profileToUpdate, newUsername);
        });

        // Set the delete action
        entry.SetDeleteAction(async (profileToDelete) => {
            await DeleteProfile(profileToDelete);
        });
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
            if (!profile.IsValidUsername(newUsername))           
            {
                Debug.LogWarning("Username not valid");
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
            await DatabaseManager.Instance.Delete<Profile>(profile.Id);

            // Reload the profiles
            await LoadProfiles();

            Debug.Log("Profile deleted successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error deleting profile: {e.Message}");
        }
    }

    // Public method to refresh profiles (can be called from UI button)
    public async void RefreshProfiles()
    {
        if (audioManager != null)
            audioManager.PlayMenuButton();

        await LoadProfiles();
    }
}