using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DiceManager : MonoBehaviour
{
    public static DiceManager Instance { get; private set; }
    private DiceSound diceSound; // New reference to DiceSound
    
    [SerializeField] private TextMeshProUGUI sumText;
    [SerializeField] private theDice dice1;
    [SerializeField] private theDice dice2;
    [SerializeField] public Button rollButton;
    
    public int LastRollSum { get; private set; }
    public GameManager gameManager; // Reference to GameManager
    public bool DiceHaveFinishedRolling { get; private set; } = true; // Start as true (not rolling)
    
    // Event that can be subscribed to when dice finish rolling
    public delegate void DiceRollCompleteDelegate(int rollValue);
    public event DiceRollCompleteDelegate OnDiceRollComplete;
    
    // New event for when dice start rolling
    public delegate void DiceRollStartDelegate();
    public event DiceRollStartDelegate OnDiceRollStart;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        // Find the DiceSound instance
        diceSound = DiceSound.Instance;
        
        // Check if it exists
        if (diceSound == null)
        {
            Debug.LogError("DiceSound not found in the scene. Make sure it's set up properly.");
        }
    }
    
    private void Update()
    {
        // Check for 'D' key press when roll button is interactive
        if (Input.GetKeyDown(KeyCode.D) && rollButton.interactable)
        {
            RollBothDiceAndShowSum();
        }
    }

    public void RollBothDiceAndShowSum()
    {
        rollButton.interactable = false;
        DiceHaveFinishedRolling = false;
        sumText.text = "Lancer en cours...";
        
        // Fire event that dice have started rolling (for camera switching)
        if (OnDiceRollStart != null)
        {
            OnDiceRollStart.Invoke();
        }
        
        // Start the coroutine to handle rolling and showing the sum
        StartCoroutine(RollAndShowSum());
    }

    private IEnumerator RollAndShowSum()
    {
        // Play the rolling sound using our new DiceSound
        if (diceSound != null)
        {
            diceSound.PlayDiceRolling();
        }
        
        // Roll the dice
        dice1.RollTheDice();
        dice2.RollTheDice();
        
        // Wait until both dice have stopped - landing sound is handled by each die
        yield return new WaitUntil(() => dice1.HasStopped && dice2.HasStopped);
        
        // Stop the rolling sound
        if (diceSound != null)
        {
            diceSound.StopDiceRolling();
        }
        
        // Get the sum of the dice values
        LastRollSum = dice1.GetRollValue() + dice2.GetRollValue();
        sumText.text = "Somme : " + LastRollSum;
        
        // Short delay to view the final dice positions before processing results
        yield return new WaitForSeconds(0.7f);
        
        // Set flag that dice have finished rolling
        DiceHaveFinishedRolling = true;
        
        // Trigger event for subscribers
        if (OnDiceRollComplete != null)
        {
            OnDiceRollComplete.Invoke(LastRollSum);
        }
        
        // Notify GameManager
        gameManager.OnDiceRolled();
    }
    
    // Enable roll button
    public void EnableRollButton()
    {
        rollButton.interactable = true;
    }

    // Disable roll button
    public void DisableRollButton()
    {
        rollButton.interactable = false;
    }

    public void EnableAndSwitchToMainCamera(){
        // Enable the roll button
        EnableRollButton();
        
        if(CameraManager.Instance != null)
        {
            CameraManager.Instance.SwitchToMainCamera();
        }
        else
        {
            Debug.LogError("CameraManager instance not found!");
        }
    }
}