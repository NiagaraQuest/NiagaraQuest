using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Threading.Tasks;

public class NewGameMenuManager : MonoBehaviour
{
    [Header("Panels Within New Game")]
    public GameObject playersPanel;
    public GameObject profilesPanel;

    [Header("Player Elements")]
    public GameObject pyroElement;
    public GameObject geoElement;
    public GameObject hydroElement;
    public GameObject anemoElement;

    [Header("Number Selector")]
    public Button twoPlayersButton;
    public Button threePlayersButton;
    public Button fourPlayersButton;

    [Header("Profiles")]
    public Transform profilesContainer;
    public GameObject profileEntryPrefab;
    public Button returnToPlayersButton;
    public Button startGameButton;
    public Button createNewProfileButton;

    [Header("Create Profile Popup")]
    public GameObject createProfilePopup;
    public TMP_InputField usernameInput;
    public Button confirmCreateButton;
    public Button cancelCreateButton;

    [Header("Character Select Buttons")]
    public Button pyroSelectButton;
    public Button geoSelectButton;
    public Button hydroSelectButton;
    public Button anemoSelectButton;

    [Header("Profile Selection")]
    public Button clearProfileButton;
    public GameObject noneProfilePrefab;

    // Reference to the AudioManager
    private AudioManager audioManager;
    private int currentPlayerCount = 2;
    private GameObject[] playerElements;
    private int currentEditingElementIndex = -1;
    private Dictionary<GameObject, Profile> selectedProfiles = new Dictionary<GameObject, Profile>();
    private bool databaseInitialized = false;
    private CreateProfilePopup createProfilePopupScript;

    void Start()
    {
        Debug.Log("NewGameMenuManager Start called");
        
        // Get reference to AudioManager singleton
        audioManager = AudioManager.Instance;

        // Initialize player elements array
        playerElements = new GameObject[] { pyroElement, geoElement, hydroElement, anemoElement };

        // Find create profile popup script
        createProfilePopupScript = FindObjectOfType<CreateProfilePopup>();

        // If no script found, add basic control for the popup
        if (createProfilePopupScript == null && createProfilePopup != null)
        {
            Debug.Log("No CreateProfilePopup script found, setting up basic popup controls");

            // Make sure popup is initially hidden
            createProfilePopup.SetActive(false);

            // Set up popup buttons
            if (confirmCreateButton != null)
            {
                confirmCreateButton.onClick.RemoveAllListeners();
                confirmCreateButton.onClick.AddListener(HandleCreateProfile);
            }

            if (cancelCreateButton != null)
            {
                cancelCreateButton.onClick.RemoveAllListeners();
                cancelCreateButton.onClick.AddListener(HideCreateProfilePopup);
            }
        }
        if (clearProfileButton != null)
            clearProfileButton.onClick.AddListener(ClearCurrentProfile);

        // Add listeners to number selector buttons
        twoPlayersButton.onClick.AddListener(() => SetPlayerCount(2));
        threePlayersButton.onClick.AddListener(() => SetPlayerCount(3));
        fourPlayersButton.onClick.AddListener(() => SetPlayerCount(4));

        // Set up element selection buttons
        SetupElementSelectionListeners();

        // Set up return button
        if (returnToPlayersButton != null)
            returnToPlayersButton.onClick.AddListener(ReturnToPlayerSelection);

        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(StartGame);
            Debug.Log("Start Game button listener set up successfully");
        }

        // Set up create new profile button
        if (createNewProfileButton != null)
            createNewProfileButton.onClick.AddListener(ShowCreateProfilePopup);

        // Show initial player count
        SetPlayerCount(2);

        // Initialize panels within New Game
        if (playersPanel != null)
            playersPanel.SetActive(true);

        if (profilesPanel != null)
            profilesPanel.SetActive(false);

        // Call the async initialization method (will run in the background)
        _ = InitializeDatabaseAsync();
    }

    public void ShowCreateProfilePopup()
    {
        // Play button sound
        if (audioManager != null)
            audioManager.PlayMenuButton();
            
        Debug.Log("Showing create profile popup");

        if (createProfilePopupScript != null)
        {
            createProfilePopupScript.ShowPopup();
        }
        else if (createProfilePopup != null)
        {
            createProfilePopup.SetActive(true);
            if (usernameInput != null)
            {
                usernameInput.text = "";
                usernameInput.Select();
            }
        }
    }

    public void HideCreateProfilePopup()
    {
        // Play button sound
        if (audioManager != null)
            audioManager.PlayMenuButton();
            
        Debug.Log("Hiding create profile popup");

        if (createProfilePopupScript != null)
        {
            createProfilePopupScript.ClosePopup();
        }
        else if (createProfilePopup != null)
        {
            createProfilePopup.SetActive(false);
        }
    }

    private async void HandleCreateProfile()
    {
        // Play button sound
        if (audioManager != null)
            audioManager.PlayMenuButton();
            
        if (usernameInput == null || string.IsNullOrEmpty(usernameInput.text.Trim()))
        {
            Debug.Log("Username cannot be empty");
            return;
        }

        string username = usernameInput.text.Trim();
        Debug.Log($"Creating profile with username: {username}");

        if (databaseInitialized && ProfileManager.Instance != null)
        {
            try
            {
                await ProfileManager.Instance.CreateProfile(username);
                Debug.Log($"Profile created: {username}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to create profile: {e.Message}");
            }
        }

        // Hide popup
        HideCreateProfilePopup();

        // Refresh profiles list
        LoadProfiles();
    }

    // Separate async method for database initialization
    private async Task InitializeDatabaseAsync()
    {
        try
        {
            Debug.Log("Initializing database...");
            await DatabaseManager.Instance.Initialize();
            await ProfileManager.Instance.Initialize();
            databaseInitialized = true;
            Debug.Log("Database initialized successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to initialize database: {e.Message}");
            databaseInitialized = false;
        }
    }

    private void SetupElementSelectionListeners()
    {
        // Set up direct button references instead of trying to find them
        if (pyroSelectButton != null)
        {
            pyroSelectButton.onClick.RemoveAllListeners();
            pyroSelectButton.onClick.AddListener(() => {
                // Play button sound
                if (audioManager != null)
                    audioManager.PlayMenuButton();
                    
                Debug.Log("Pyro select button clicked");
                ShowProfilesForElement(0);
            });
        }
        else
        {
            Debug.LogError("Pyro select button is not assigned!");
        }

        if (geoSelectButton != null)
        {
            geoSelectButton.onClick.RemoveAllListeners();
            geoSelectButton.onClick.AddListener(() => {
                // Play button sound
                if (audioManager != null)
                    audioManager.PlayMenuButton();
                    
                Debug.Log("Geo select button clicked");
                ShowProfilesForElement(1);
            });
        }
        else
        {
            Debug.LogError("Geo select button is not assigned!");
        }

        if (hydroSelectButton != null)
        {
            hydroSelectButton.onClick.RemoveAllListeners();
            hydroSelectButton.onClick.AddListener(() => {
                // Play button sound
                if (audioManager != null)
                    audioManager.PlayMenuButton();
                    
                Debug.Log("Hydro select button clicked");
                ShowProfilesForElement(2);
            });
        }
        else
        {
            Debug.LogError("Hydro select button is not assigned!");
        }

        if (anemoSelectButton != null)
        {
            anemoSelectButton.onClick.RemoveAllListeners();
            anemoSelectButton.onClick.AddListener(() => {
                // Play button sound
                if (audioManager != null)
                    audioManager.PlayMenuButton();
                    
                Debug.Log("Anemo select button clicked");
                ShowProfilesForElement(3);
            });
        }
        else
        {
            Debug.LogError("Anemo select button is not assigned!");
        }
    }
    
    private void SetupElementButton(GameObject elementObj, int index)
    {
        if (elementObj == null)
        {
            Debug.LogError($"Element object at index {index} is null!");
            return;
        }

        Transform innerTransform = elementObj.transform.Find("Inner");
        if (innerTransform == null)
        {
            Debug.LogError($"Cannot find 'Inner' transform in {elementObj.name}");
            return;
        }

        Transform selectTransform = innerTransform.Find("select");
        if (selectTransform == null)
        {
            Debug.LogError($"Cannot find 'select' transform in {elementObj.name}/Inner");
            return;
        }

        Button selectButton = selectTransform.GetComponent<Button>();
        if (selectButton == null)
        {
            Debug.LogError($"No Button component on {elementObj.name}/Inner/select");
            return;
        }

        // Remove any existing listeners to avoid duplicates
        selectButton.onClick.RemoveAllListeners();

        int capturedIndex = index; // Capture the index for the lambda
        selectButton.onClick.AddListener(() => {
            // Play button sound
            if (audioManager != null)
                audioManager.PlayMenuButton();
                
            Debug.Log($"Select button clicked for element {capturedIndex}");
            ShowProfilesForElement(capturedIndex);
        });

        Debug.Log($"Set up button listener for element {elementObj.name} at index {index}");
    }

    public void SetPlayerCount(int count)
    {
        // Play button sound
        if (audioManager != null)
            audioManager.PlayMenuButton();
            
        Debug.Log($"Setting player count to {count}");
        currentPlayerCount = count;

        // Update visual feedback for selected player count
        if (twoPlayersButton != null)
            twoPlayersButton.GetComponent<Image>().color = (count == 2) ? Color.green : Color.white;

        if (threePlayersButton != null)
            threePlayersButton.GetComponent<Image>().color = (count == 3) ? Color.green : Color.white;

        if (fourPlayersButton != null)
            fourPlayersButton.GetComponent<Image>().color = (count == 4) ? Color.green : Color.white;
    }

    public void ShowProfilesForElement(int elementIndex)
    {
        // Play button sound is already handled in the calling methods
        Debug.Log($"ShowProfilesForElement called for index {elementIndex}");

        // Store which element we're selecting a profile for
        currentEditingElementIndex = elementIndex;

        // Make sure create profile popup is hidden
        HideCreateProfilePopup();

        // Switch to profiles panel
        if (playersPanel != null)
        {
            playersPanel.SetActive(false);
            Debug.Log("Set playersPanel to inactive");
        }
        else
        {
            Debug.LogError("Cannot hide playersPanel because it is null!");
        }

        if (profilesPanel != null)
        {
            profilesPanel.SetActive(true);
            Debug.Log("Set profilesPanel to active");
        }
        else
        {
            Debug.LogError("Cannot show profilesPanel because it is null!");
        }

        // Load profiles from database
        LoadProfiles();
    }

    public void ReturnToPlayerSelection()
    {
        // Play button sound
        if (audioManager != null)
            audioManager.PlayMenuButton();
            
        Debug.Log("ReturnToPlayerSelection called");

        // Make sure create profile popup is hidden
        HideCreateProfilePopup();

        if (playersPanel != null)
        {
            playersPanel.SetActive(true);
            Debug.Log("Set playersPanel to active");
        }
        else
        {
            Debug.LogError("Cannot show playersPanel because it is null!");
        }

        if (profilesPanel != null)
        {
            profilesPanel.SetActive(false);
            Debug.Log("Set profilesPanel to inactive");
        }
        else
        {
            Debug.LogError("Cannot hide profilesPanel because it is null!");
        }
    }

    public async void LoadProfiles()
    {
        Debug.Log("LoadProfiles called");

        // Clear existing profiles
        if (profilesContainer != null)
        {
            foreach (Transform child in profilesContainer)
            {
                Destroy(child.gameObject);
            }
        }
        else
        {
            Debug.LogError("Profile container is not assigned!");
            return;
        }

        // First, add the "None" option
        if (noneProfilePrefab != null)
        {
            GameObject noneEntry = Instantiate(noneProfilePrefab, profilesContainer);
            ProfileEntry entry = noneEntry.GetComponent<ProfileEntry>();
            if (entry != null)
            {
                Profile noneProfile = CreateNoneProfile();
                entry.Initialize(noneProfile);
                entry.SetSelectAction(() => SelectProfileForElement(noneProfile));
            }
            else
            {
                Debug.LogError("None profile prefab does not have a ProfileEntry component!");
            }
        }
        else
        {
            // If no special prefab is provided, create a None profile with the regular prefab
            if (profileEntryPrefab != null)
            {
                GameObject noneEntry = Instantiate(profileEntryPrefab, profilesContainer);
                ProfileEntry entry = noneEntry.GetComponent<ProfileEntry>();
                if (entry != null)
                {
                    Profile noneProfile = CreateNoneProfile();
                    entry.Initialize(noneProfile);
                    entry.SetSelectAction(() => SelectProfileForElement(noneProfile));
                }
            }
        }

        // Check if database is initialized
        if (!databaseInitialized || ProfileManager.Instance == null)
        {
            Debug.LogWarning("Database not initialized yet, using test profiles instead");
            CreateTestProfiles();
            return;
        }

        try
        {
            // Try to load profiles from database
            List<Profile> profiles = await ProfileManager.Instance.GetAllProfiles();
            Debug.Log($"Loaded {profiles.Count} profiles from database");

            if (profiles.Count == 0)
            {
                // If no profiles in database, use test profiles
                CreateTestProfiles();
                return;
            }

            // Create profile entries from database
            CreateProfileEntries(profiles);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load profiles: {e.Message}");
            // Fall back to test profiles
            CreateTestProfiles();
        }
    }

    // Method to create test profiles for testing
    private void CreateTestProfiles()
    {
        Debug.Log("Creating test profiles for UI testing");
        List<Profile> testProfiles = new List<Profile>();

        // Create test profiles with different properties
        Profile profile1 = new Profile();
        profile1.Id = 1;
        profile1.Username = "TestUser1";
        profile1.Elo = 1000;
        testProfiles.Add(profile1);

        Profile profile2 = new Profile();
        profile2.Id = 2;
        profile2.Username = "TestUser2";
        profile2.Elo = 1200;
        testProfiles.Add(profile2);

        Profile profile3 = new Profile();
        profile3.Id = 3;
        profile3.Username = "TestUser3";
        profile3.Elo = 1500;
        testProfiles.Add(profile3);

        CreateProfileEntries(testProfiles);
    }

    // Method to create UI entries for profiles
    private void CreateProfileEntries(List<Profile> profiles)
    {
        foreach (Profile profile in profiles)
        {
            if (profileEntryPrefab == null)
            {
                Debug.LogError("Profile entry prefab is not assigned!");
                return;
            }

            GameObject profileEntry = Instantiate(profileEntryPrefab, profilesContainer);
            ProfileEntry entry = profileEntry.GetComponent<ProfileEntry>();
            if (entry != null)
            {
                entry.Initialize(profile);
                Profile capturedProfile = profile; // Capture for lambda
                entry.SetSelectAction(() => SelectProfileForElement(capturedProfile));
            }
            else
            {
                Debug.LogError("Profile entry prefab does not have a ProfileEntry component!");
            }
        }
    }

    // Add this to your SelectProfileForElement method instead of hiding the button
    public void SelectProfileForElement(Profile profile)
    {
        // Sound is already played in the ProfileEntry button click handler
        Debug.Log($"SelectProfileForElement called for profile {profile.Username}");

        if (currentEditingElementIndex >= 0 && currentEditingElementIndex < playerElements.Length)
        {
            GameObject elementObj = playerElements[currentEditingElementIndex];
            bool isNoneProfile = (profile.Id == -1);

            // If selecting a real profile (not None), check player count limit
            if (!isNoneProfile)
            {
                // Count existing active profiles (excluding current element)
                int activeProfileCount = 0;
                foreach (var element in playerElements)
                {
                    if (element != elementObj &&
                        selectedProfiles.ContainsKey(element) &&
                        selectedProfiles[element].Id != -1)
                    {
                        activeProfileCount++;
                    }
                }

                // Check if we'd exceed the limit
                if (activeProfileCount >= currentPlayerCount)
                {
                    Debug.LogWarning($"Cannot select more than {currentPlayerCount} profiles in {currentPlayerCount}-player mode");
                    ReturnToPlayerSelection();
                    return;
                }
            }

            try
            {
                // Update username text
                TMP_Text usernameText = elementObj.transform.Find("Inner/User").GetComponent<TMP_Text>();
                if (usernameText != null)
                {
                    usernameText.text = profile.Username;
                    Debug.Log($"Updated username text to {profile.Username}");
                }

                // Update elo text
                TMP_Text eloText = elementObj.transform.Find("Inner/elo").GetComponent<TMP_Text>();
                if (eloText != null)
                {
                    eloText.text = isNoneProfile ? "" : profile.Elo.ToString();
                    Debug.Log($"Updated ELO text to {profile.Elo}");
                }

                // Store the profile in our dictionary (or remove if it's a None profile)
                if (isNoneProfile)
                {
                    if (selectedProfiles.ContainsKey(elementObj))
                    {
                        selectedProfiles.Remove(elementObj);
                        Debug.Log($"Removed profile for element {elementObj.name}");
                    }
                }
                else
                {
                    selectedProfiles[elementObj] = profile;
                    Debug.Log($"Stored profile for element {elementObj.name}");
                }

                // Return to player selection
                ReturnToPlayerSelection();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error updating element with profile: {e.Message}");
            }
        }
        else
        {
            Debug.LogError($"Invalid element index: {currentEditingElementIndex}");
        }
    }

    public void OnStartGameButtonClick()
    {
        // Play button sound
        if (audioManager != null)
            audioManager.PlayMenuButton();
            
        Debug.Log("Start Game button clicked via direct method");
        StartGame();
    }

    public void ClearCurrentProfile()
    {
        // Play button sound
        if (audioManager != null)
            audioManager.PlayMenuButton();
            
        Debug.Log("ClearCurrentProfile called");

        if (currentEditingElementIndex >= 0 && currentEditingElementIndex < playerElements.Length)
        {
            GameObject elementObj = playerElements[currentEditingElementIndex];

            try
            {
                // Update username text
                TMP_Text usernameText = elementObj.transform.Find("Inner/User").GetComponent<TMP_Text>();
                if (usernameText != null)
                {
                    usernameText.text = "";
                    Debug.Log("Cleared username text");
                }

                // Update elo text
                TMP_Text eloText = elementObj.transform.Find("Inner/elo").GetComponent<TMP_Text>();
                if (eloText != null)
                {
                    eloText.text = "";
                    Debug.Log("Cleared ELO text");
                }

                // Update select button visibility (show it)
                UpdateElementVisibility(elementObj, false);

                // Remove from selected profiles dictionary
                if (selectedProfiles.ContainsKey(elementObj))
                {
                    selectedProfiles.Remove(elementObj);
                    Debug.Log($"Removed profile for element {elementObj.name}");
                }

                // Return to player selection
                ReturnToPlayerSelection();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error clearing profile: {e.Message}");
            }
        }
    }

    public void StartGame()
    {
        // Play button sound
        if (audioManager != null)
            audioManager.PlayMenuButton();
            
        Debug.Log("StartGame called");

        // Count how many characters have actual profiles assigned (not None)
        int selectedCharacterCount = 0;
        foreach (GameObject element in playerElements)
        {
            if (selectedProfiles.ContainsKey(element) && selectedProfiles[element].Id != -1)
            {
                selectedCharacterCount++;
            }
        }

        Debug.Log($"Selected character count: {selectedCharacterCount}, Current player count: {currentPlayerCount}");

        // Check if we have the correct number of characters selected
        if (selectedCharacterCount == currentPlayerCount)
        {
            Debug.Log("Starting game with selected profiles");

            // Set game mode in PlayerPrefs for GameManager to read
            PlayerPrefs.SetInt("GameMode", (int)GameManager.GameMode.TwoPlayers + (currentPlayerCount - 2));
            Debug.Log($"Set game mode to {(GameManager.GameMode)(currentPlayerCount - 2)}");

            // Prepare PlayerPrefs for each character
            foreach (GameObject element in playerElements)
            {
                string playerName = "";
                if (element == pyroElement) playerName = "PyroPlayer";
                else if (element == geoElement) playerName = "GeoPlayer";
                else if (element == hydroElement) playerName = "HydroPlayer";
                else if (element == anemoElement) playerName = "AnemoPlayer";

                // Check if this element has a real profile assigned (not None)
                if (selectedProfiles.ContainsKey(element) && selectedProfiles[element].Id != -1)
                {
                    Profile profile = selectedProfiles[element];

                    // Store profile data in PlayerPrefs
                    PlayerPrefs.SetInt($"{playerName}_ProfileId", profile.Id);
                    PlayerPrefs.SetString($"{playerName}_ProfileName", profile.Username);
                    PlayerPrefs.SetInt($"{playerName}_ProfileElo", profile.Elo);
                    PlayerPrefs.SetInt($"{playerName}_Active", 1); // Mark as active

                    Debug.Log($"Set {playerName} profile to {profile.Username} (ID: {profile.Id})");
                }
                else
                {
                    // No profile assigned or None profile - mark as inactive
                    PlayerPrefs.SetInt($"{playerName}_Active", 0);
                    Debug.Log($"Set {playerName} as inactive");
                }
            }

            // Save all PlayerPrefs
            PlayerPrefs.Save();

            // Load the game scene
            Debug.Log("Loading game scene...");
            UnityEngine.SceneManagement.SceneManager.LoadScene("Game__Movements");
        }
        else
        {
            Debug.LogWarning($"Cannot start game: {selectedCharacterCount} characters selected, but {currentPlayerCount} players expected");
            // Show error message to user
            // You could add a UI element to display this message
        }
    }
    
    // Manual testing methods
    private void UpdateElementVisibility(GameObject elementObj, bool profileSelected)
    {
        if (elementObj == null) return;

        Debug.Log($"Updating visibility for {elementObj.name}, profileSelected: {profileSelected}");

        // Try different naming conventions for the select button
        Transform selectTransform = null;
        Transform innerTransform = elementObj.transform.Find("Inner");

        if (innerTransform != null)
        {
            // Try different variations of the name
            string[] possibleNames = { "select", "Select", "SELECT" };

            foreach (string name in possibleNames)
            {
                selectTransform = innerTransform.Find(name);
                if (selectTransform != null)
                {
                    Debug.Log($"Found select button as '{name}' in {elementObj.name}");
                    break;
                }
            }

            if (selectTransform != null)
            {
                selectTransform.gameObject.SetActive(!profileSelected);
                Debug.Log($"Set {elementObj.name} select button active: {!profileSelected}");
            }
            else
            {
                // If we can't find it by name, try looking for a Button component
                Button[] buttons = innerTransform.GetComponentsInChildren<Button>(true);
                foreach (Button button in buttons)
                {
                    if (button.name.ToLower().Contains("select"))
                    {
                        button.gameObject.SetActive(!profileSelected);
                        Debug.Log($"Found and set {button.name} button in {elementObj.name} active: {!profileSelected}");
                        break;
                    }
                }
            }
        }
    }

    public void TestSwitchToProfiles()
    {
        // Play button sound for testing
        if (audioManager != null)
            audioManager.PlayMenuButton();
            
        Debug.Log("Manual test: switching to profiles panel");
        if (playersPanel != null && profilesPanel != null)
        {
            playersPanel.SetActive(false);
            profilesPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("Cannot perform test because panels are not assigned");
        }
    }

    private Profile CreateNoneProfile()
    {
        Profile noneProfile = new Profile();
        noneProfile.Id = -1; // Special ID for None profile
        noneProfile.Username = "None";
        noneProfile.Elo = 0;
        return noneProfile;
    }

    public void TestSwitchToPlayers()
    {
        // Play button sound for testing
        if (audioManager != null)
            audioManager.PlayMenuButton();
            
        Debug.Log("Manual test: switching to players panel");
        if (playersPanel != null && profilesPanel != null)
        {
            playersPanel.SetActive(true);
            profilesPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("Cannot perform test because panels are not assigned");
        }
    }
}