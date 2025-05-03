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
        playerElements = new GameObject[] { pyroElement, hydroElement, anemoElement, geoElement };

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


        if (hydroSelectButton != null)
        {
            hydroSelectButton.onClick.RemoveAllListeners();
            hydroSelectButton.onClick.AddListener(() => {
                // Play button sound
                if (audioManager != null)
                    audioManager.PlayMenuButton();

                Debug.Log("Hydro select button clicked");
                ShowProfilesForElement(1);
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
                ShowProfilesForElement(2);
            });
        }
        else
        {
            Debug.LogError("Anemo select button is not assigned!");
        }

        if (geoSelectButton != null)
        {
            geoSelectButton.onClick.RemoveAllListeners();
            geoSelectButton.onClick.AddListener(() => {
                // Play button sound
                if (audioManager != null)
                    audioManager.PlayMenuButton();

                Debug.Log("Geo select button clicked");
                ShowProfilesForElement(3);
            });
        }
        else
        {
            Debug.LogError("Geo select button is not assigned!");
        }
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

        

        try
        {
            // Try to load profiles from database
            List<Profile> profiles = await ProfileManager.Instance.GetAllProfiles();
            Debug.Log($"Loaded {profiles.Count} profiles from database");

            

            // Create profile entries from database
            CreateProfileEntries(profiles);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load profiles: {e.Message}");
           
        }
    }

    // Method to create test profiles for testing
   
 

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
        Debug.Log($"SelectProfileForElement called for profile {profile.Username}");

        if (currentEditingElementIndex >= 0 && currentEditingElementIndex < playerElements.Length)
        {
            GameObject elementObj = playerElements[currentEditingElementIndex];
            Debug.Log($"Selecting profile for element index {currentEditingElementIndex}, element: {elementObj.name}");

            

           
           
            
                // Check if this profile is already assigned to another element
                foreach (var kvp in selectedProfiles)
                {
                    if (kvp.Key != elementObj && kvp.Value.Id == profile.Id)
                    {
                        Debug.LogWarning($"Profile {profile.Username} is already assigned to {kvp.Key.name}");
                      
                        ReturnToPlayerSelection();
                        return;
                    }
                }

                
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
            

            try
            {
                // Update username text
                TMP_Text usernameText = elementObj.transform.Find("User").GetComponent<TMP_Text>();
                if (usernameText != null)
                {
                    usernameText.text = profile.Username;
                    Debug.Log($"Updated username text to {profile.Username}");
                }

                // Update elo text
                TMP_Text eloText = elementObj.transform.Find("elo").GetComponent<TMP_Text>();
                if (eloText != null)
                {
                    eloText.text =  "ELO : " + profile.Elo.ToString();
                    Debug.Log($"Updated ELO text to {profile.Elo}");
                }

               
               
                
                    // We already checked for duplicates above, so we can safely assign here
                    selectedProfiles[elementObj] = profile;
                    Debug.Log($"Stored profile for element {elementObj.name}");
                

                // Return to player selection
                ReturnToPlayerSelection();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error updating element with profile: {e.Message}\n{e.StackTrace}");
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
        Debug.Log("ClearCurrentProfile called");

        if (currentEditingElementIndex >= 0 && currentEditingElementIndex < playerElements.Length)
        {
            GameObject elementObj = playerElements[currentEditingElementIndex];

            try
            {
                // Update username text
                TMP_Text usernameText = elementObj.transform.Find("User").GetComponent<TMP_Text>();
                if (usernameText != null)
                {
                    usernameText.text = "";
                    Debug.Log("Cleared username text");
                }

                // Update elo text
                TMP_Text eloText = elementObj.transform.Find("elo").GetComponent<TMP_Text>();
                if (eloText != null)
                {
                    eloText.text = "";
                    Debug.Log("Cleared ELO text");
                }

               

               
                if (selectedProfiles.ContainsKey(elementObj))
                {
                    selectedProfiles.Remove(elementObj);
                    Debug.Log($"Removed profile for element {elementObj.name}");
                }

              
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
        if (audioManager != null)
            audioManager.PlayMenuButton();

        Debug.Log("StartGame called");

        // Debug element references
        Debug.Log("Element references check:");
        Debug.Log($"pyroElement: {(pyroElement != null ? pyroElement.name : "null")}");
        Debug.Log($"hydroElement: {(hydroElement != null ? hydroElement.name : "null")}");
        Debug.Log($"anemoElement: {(anemoElement != null ? anemoElement.name : "null")}");
        Debug.Log($"geoElement: {(geoElement != null ? geoElement.name : "null")}");

        // Debug which elements have profiles assigned
        Debug.Log("Checking assigned profiles:");
        foreach (var kvp in selectedProfiles)
        {
            string elementName = kvp.Key != null ? kvp.Key.name : "null";
            string profileName = kvp.Value != null ? kvp.Value.Username : "null";
            Debug.Log($"Profile assigned: Element={elementName}, Profile={profileName}, ID={kvp.Value.Id}");

            // Specifically check if this is the hydroElement
            if (kvp.Key == hydroElement)
            {
                Debug.Log($"FOUND HYDRO ELEMENT with profile {profileName}!");
            }
        }

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

            // IMPORTANT FIX: Reset all player active states to inactive by default
            PlayerPrefs.SetInt("PyroPlayer_Active", 0);
            PlayerPrefs.SetInt("GeoPlayer_Active", 0);
            PlayerPrefs.SetInt("HydroPlayer_Active", 0);
            PlayerPrefs.SetInt("AnemoPlayer_Active", 0);
            Debug.Log("Reset all player active states to 0");

            // Prepare PlayerPrefs for each character
            foreach (GameObject element in playerElements)
            {
                string playerName = "";
                if (element == pyroElement) playerName = "PyroPlayer";
                else if (element == geoElement) playerName = "GeoPlayer";
                else if (element == hydroElement) playerName = "HydroPlayer";
                else if (element == anemoElement) playerName = "AnemoPlayer";

                // Debug which element we're processing
                Debug.Log($"Processing element: {element.name}, playerName: {playerName}");
               
                // Check if this element has a real profile assigned (not None)
                if (selectedProfiles.ContainsKey(element) && selectedProfiles[element].Id != -1)
                {
                    Profile profile = selectedProfiles[element];

                    // Store profile data in PlayerPrefs
                    PlayerPrefs.SetInt($"{playerName}_ProfileId", profile.Id);
                    PlayerPrefs.SetString($"{playerName}_ProfileName", profile.Username);
                    PlayerPrefs.SetInt($"{playerName}_ProfileElo", profile.Elo);
                    PlayerPrefs.SetInt($"{playerName}_Active", 1); // Mark as active

                    Debug.Log($"Set {playerName} profile to {profile.Username} (ID: {profile.Id}), Active: 1");
                }
                else
                {
                    // No profile assigned or None profile - mark as inactive
                    PlayerPrefs.SetInt($"{playerName}_Active", 0);
                    Debug.Log($"Set {playerName} as inactive");
                }
            }

            // Final verification of all player states
            Debug.Log("FINAL PLAYER STATES:");
            Debug.Log($"PyroPlayer_Active = {PlayerPrefs.GetInt("PyroPlayer_Active", 0)}");
            Debug.Log($"GeoPlayer_Active = {PlayerPrefs.GetInt("GeoPlayer_Active", 0)}");
            Debug.Log($"HydroPlayer_Active = {PlayerPrefs.GetInt("HydroPlayer_Active", 0)}");
            Debug.Log($"AnemoPlayer_Active = {PlayerPrefs.GetInt("AnemoPlayer_Active", 0)}");

            // Save all PlayerPrefs
            PlayerPrefs.Save();

            // Load the game scene
            Debug.Log("Loading game scene...");
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }
        else
        {
            Debug.LogWarning($"Cannot start game: {selectedCharacterCount} characters selected, but {currentPlayerCount} players expected");
            // Show error message to user
            // You could add a UI element to display this message
        }
    }
 
}