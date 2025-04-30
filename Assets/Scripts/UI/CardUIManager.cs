using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class CardUIManager : MonoBehaviour
{
    public static CardUIManager Instance;

    [Header("Card UI Elements")]
    public GameObject cardPanel;
    public TextMeshProUGUI cardTitleText;
    public TextMeshProUGUI cardDescriptionText;
    public Button continueButton;

    [Header("Player Selection UI")]
    public GameObject playerSelectionPanel;
    public TextMeshProUGUI selectionPromptText;
    public Button[] playerButtons; // Array of pre-created player buttons
    public TextMeshProUGUI[] playerButtonTexts; // Array for the text components of each button

    private CardTile currentTile;
    private int currentCardType;
    private Player currentPlayer;
    private Coroutine autoCloseCoroutine;

    [Header("Auto Close")]
    public float autoCloseTime = 3f;

    [Header("Gambler Card UI")]
    public GameObject gambleChoicePanel;
    public TextMeshProUGUI gamblePromptText;
    public Button gambleYesButton;
    public Button gambleNoButton;
    public GameObject gambleResultPanel;
    public TextMeshProUGUI gambleResultText;
    public Button gambleResultOkButton;
    private DiceManager diceManager;

    [Header("UI Scripts")]
    public CameraUIManager cameraUIManager;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        diceManager = DiceManager.Instance;
        if (diceManager == null)
        {
            Debug.LogWarning("❌ DiceManager not found. Roll button control will not work.");
        }
        // Hide all panels at start
        cardPanel.SetActive(false);
        playerSelectionPanel.SetActive(false);
        gambleChoicePanel.SetActive(false);
        gambleResultPanel.SetActive(false);
        // Add listener to the Continue button
        continueButton.onClick.AddListener(CloseCardPanel);

        // Subscribe to the player selection event
        CardManager.OnPlayerSelectionRequested += ShowPlayerSelectionUI;

        // Setup gambler button listeners
        if (gambleYesButton != null)
            gambleYesButton.onClick.AddListener(OnGambleYes);
        if (gambleNoButton != null)
            gambleNoButton.onClick.AddListener(OnGambleNo);
        if (gambleResultOkButton != null)
            gambleResultOkButton.onClick.AddListener(() => OnGambleResultClosed(false, false)); // Default values, will be overridden

        // Initialize playerButtonTexts array if not set in the inspector
        if (playerButtonTexts == null || playerButtonTexts.Length != playerButtons.Length)
        {
            playerButtonTexts = new TextMeshProUGUI[playerButtons.Length];
            for (int i = 0; i < playerButtons.Length; i++)
            {
                playerButtonTexts[i] = playerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            }
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from event
        CardManager.OnPlayerSelectionRequested -= ShowPlayerSelectionUI;
    }

    // Show the card UI with the given card information
    public void ShowCard(string title, string description, CardTile tile, Player player, int cardType)
    {
        // Store references
        currentTile = tile;
        currentPlayer = player;
        currentCardType = cardType;
        diceManager.DisableRollButton();
        if(cameraUIManager.cameraSelectionPanel.activeSelf)
        {
            cameraUIManager.cameraSelectionPanel.SetActive(false);
        }


        // Check if this is the Gambler card (assuming it's index 2)
        if (cardType == 2) // Gambler card
        {
            // Skip showing the regular card panel and show the gamble choice directly
            ShowGambleChoice(player);
            return;
        }

        // Cancel any existing auto-close routine
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }

        // Set UI text
        cardTitleText.text = title;
        cardDescriptionText.text = description;

        // Show the panel
        cardPanel.SetActive(true);

        // Auto-close after delay if set
        if (autoCloseTime > 0)
        {
            autoCloseCoroutine = StartCoroutine(AutoCloseCard(autoCloseTime));
        }
    }

    // Show player selection UI
    private void ShowPlayerSelectionUI(Player player, int cardType)
    {
        // Store current player and card type
        currentPlayer = player;
        currentCardType = cardType;

        // Set selection prompt
        selectionPromptText.text = "Select a player to swap positions with:";

        // Create a list of other players
        GameManager gameManager = GameManager.Instance;
        List<Player> otherPlayers = new List<Player>();

        if (gameManager != null && gameManager.players != null)
        {
            foreach (GameObject playerObj in gameManager.players)
            {
                Player otherPlayer = playerObj.GetComponent<Player>();

                // Skip the current player
                if (otherPlayer == player) continue;

                otherPlayers.Add(otherPlayer);
            }
        }

        // Set up buttons for each player (or hide if not enough players)
        for (int i = 0; i < playerButtons.Length; i++)
        {
            if (i < otherPlayers.Count)
            {
                // Set button active and update text
                playerButtons[i].gameObject.SetActive(true);

                if (playerButtonTexts[i] != null)
                {
                    // Set the button text to the player's name and character
                    Player otherPlayer = otherPlayers[i];
                    string playerType = GetPlayerTypeName(otherPlayer);
                    playerButtonTexts[i].text = $"{otherPlayer.gameObject.name} ({playerType})";
                }

                // Set up click handler (using closure to capture the right player)
                Player targetPlayer = otherPlayers[i];
                playerButtons[i].onClick.RemoveAllListeners();
                playerButtons[i].onClick.AddListener(() => OnPlayerSelected(targetPlayer));
            }
            else
            {
                // Hide this button if there aren't enough players
                playerButtons[i].gameObject.SetActive(false);
            }
        }

        // Show the player selection panel
        playerSelectionPanel.SetActive(true);
    }

    // Helper method to get player type name
    private string GetPlayerTypeName(Player player)
    {
        if (player is PyroPlayer) return "Pyro";
        if (player is HydroPlayer) return "Hydro";
        if (player is AnemoPlayer) return "Anemo";
        if (player is GeoPlayer) return "Geo";
        return "Player";
    }

    public void ShowGambleChoice(Player player)
    {
        currentPlayer = player;

        // Make sure other panels are closed
        cardPanel.SetActive(false);
        playerSelectionPanel.SetActive(false);
        gambleResultPanel.SetActive(false);

        // Set up the prompt text
        gamblePromptText.text = "Are you willing to Gamble one life.";

        // Set up button listeners
        gambleYesButton.onClick.RemoveAllListeners();
        gambleNoButton.onClick.RemoveAllListeners();

        gambleYesButton.onClick.AddListener(OnGambleYes);
        gambleNoButton.onClick.AddListener(OnGambleNo);

        // Show the gamble choice panel
        gambleChoicePanel.SetActive(true);
    }

    // Called when the player chooses to gamble
    private void OnGambleYes()
    {
        // Hide the choice panel
        gambleChoicePanel.SetActive(false);

        // Determine the result (50/50 chance)
        bool win = Random.Range(0, 2) == 0;

        // Get the max lives from GameManager
        int maxLives = GameManager.Instance.maxLives;
        bool hasMaxLives = currentPlayer.lives >= maxLives;

        // Apply the effect
        if (win)
        {
            // Win case
            if (hasMaxLives)
            {
                // Already at max lives, move player forward 8 tiles
                ShowGambleResult(true, true);
            }
            else
            {
                // Gain a life
                currentPlayer.GainLife();
                ShowGambleResult(true, false);
            }
        }
        else
        {
            // Lose case
            currentPlayer.LoseLife();
            ShowGambleResult(false, false);

            // Check if player lost all lives
            if (currentPlayer.lives <= 0)
            {
                GameManager.Instance.CheckPlayerLives();
            }
        }
    }

    // Called when the player chooses not to gamble
    private void OnGambleNo()
    {
        // Hide the choice panel
        gambleChoicePanel.SetActive(false);

        // Continue game flow
        if (currentTile != null)
        {
            currentTile.ContinueGame();
        }
    }

    // Show the gamble result panel
    private void ShowGambleResult(bool win, bool moveForward)
    {
        // Store the result parameters for the OK button callback
        bool finalWin = win;
        bool finalMoveForward = moveForward;

        string resultMessage;

        if (win)
        {
            if (moveForward)
            {
                resultMessage = "<color=#4CAF50>You won!</color>\n<size=55>You already have maximum lives.\nYou will move forward 8 tiles!";
            }
            else
            {
                resultMessage = "<color=#4CAF50>You won!</color>\nYou gained 1 life!";
            }
        }
        else
        {
            resultMessage = "<color=#F44336>You lost!</color>\nYou lost 1 life!";
        }

        // Set up the result text
        gambleResultText.text = resultMessage;

        // Set up button listener
        gambleResultOkButton.onClick.RemoveAllListeners();
        gambleResultOkButton.onClick.AddListener(() => OnGambleResultClosed(finalWin, finalMoveForward));

        // Show the panel
        gambleResultPanel.SetActive(true);
    }

    private void OnGambleResultClosed(bool win, bool moveForward)
    {
        // Hide the result panel
        gambleResultPanel.SetActive(false);
        EnableRollButton();
        CameraManager.Instance.EnableViewToggle();

        // Move player if needed
        if (win && moveForward)
        {
            // Move player forward 8 tiles
            currentPlayer.MovePlayer(8);
            GameManager.Instance.isEffectMovement = true;
        }

        // Continue game flow
        if (currentTile != null)
        {
            currentTile.ContinueGame();
        }
    }

    // Called when a player is selected from the UI
    private void OnPlayerSelected(Player selectedPlayer)
    {
        // Close the selection UI
        playerSelectionPanel.SetActive(false);
        EnableRollButton();
        CameraManager.Instance.EnableViewToggle();

        // Perform the swap
        CardManager.Instance.SwapWithSpecificPlayer(currentPlayer, selectedPlayer);

        // Continue game flow
        if (currentTile != null)
        {
            currentTile.ContinueGame();
        }
    }

    // Close the card panel and apply the effect
    public void CloseCardPanel()
    {
        // Cancel auto-close if running
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }
        if (CardManager.Instance.DrawRandomCard() != 3){
            EnableRollButton();
            CameraManager.Instance.EnableViewToggle();
        }

        // Hide the panel
        cardPanel.SetActive(false);

        // Apply the card effect
        if (currentTile != null)
            currentTile.ApplyCardEffect(currentCardType, currentPlayer);
    }

    private IEnumerator AutoCloseCard(float delay)
    {
        yield return new WaitForSeconds(delay);
        CloseCardPanel();
    }

    private void OnDisable()
    {
        // Clean up coroutines
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }
    }

        public void DisableRollButton()
    {
        if (diceManager != null)
        {
            diceManager.DisableRollButton();
            Debug.Log("Roll button disabled during question");
        }
        else
        {
            Debug.LogWarning("❌ Cannot disable roll button: diceManager is null");
        }
    }

    // Méthode pour activer le bouton de lancement de dé
    public void EnableRollButton()
    {
        if (diceManager != null)
        {
            diceManager.EnableAndSwitchToMainCamera();
        }
        else
        {
            Debug.LogWarning("❌ Cannot enable roll button: diceManager is null");
        }
    }
}