using UnityEngine;

// Simple component to disable UI during dice rolling and player movement
public class DisableDuringMovement : MonoBehaviour
{
    [Header("Settings")]
    public bool disableDuringDiceRoll = true;
    public bool disableDuringMovement = true;

    // GameObject to disable
    private GameObject targetObject;
    private bool wasEnabled = false;

    void Awake()
    {
        targetObject = this.gameObject;
        wasEnabled = targetObject.activeSelf;
    }

    void Start()
    {
        // Find dice manager and subscribe to events
        DiceManager diceManager = FindObjectOfType<DiceManager>();
        if (diceManager != null && disableDuringDiceRoll)
        {
            diceManager.OnDiceRollComplete += HandleDiceRollComplete;
        }
    }

    void Update()
    {
        // Check if we're currently in a dice roll or movement
        bool shouldDisable = false;

        if (disableDuringDiceRoll && GameManager.Instance != null)
        {
            shouldDisable = GameManager.Instance.hasDiceBeenRolledThisTurn &&
                          !GameManager.Instance.isEffectMovement;
        }

        if (disableDuringMovement && GameManager.Instance != null &&
            GameManager.Instance.selectedPlayer != null)
        {
            Player player = GameManager.Instance.selectedPlayer.GetComponent<Player>();
            shouldDisable = shouldDisable || (player != null && player.isMoving);
        }

        // Only change state if needed
        if (shouldDisable && targetObject.activeSelf)
        {
            wasEnabled = true;
            targetObject.SetActive(false);
        }
        else if (!shouldDisable && !targetObject.activeSelf && wasEnabled)
        {
            targetObject.SetActive(true);
        }
    }

    void HandleDiceRollComplete(int rollValue)
    {
        // The dice have finished rolling, but movement may be starting
        // We'll let the Update check handle reactivation
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        DiceManager diceManager = FindObjectOfType<DiceManager>();
        if (diceManager != null)
        {
            diceManager.OnDiceRollComplete -= HandleDiceRollComplete;
        }
    }
}