using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DiceManager : MonoBehaviour
{
    public static DiceManager Instance { get; private set; }
    private AudioManager audioManager;
    
    [SerializeField] private TextMeshProUGUI sumText;
    [SerializeField] private theDice dice1;
    [SerializeField] private theDice dice2;
    [SerializeField] public Button rollButton;
    
    public int LastRollSum { get; private set; }
    public GameManager gameManager; // Reference to GameManager
    public bool DiceHaveFinishedRolling { get; private set; } = false;
    
    // Event that can be subscribed to when dice finish rolling
    public delegate void DiceRollCompleteDelegate(int rollValue);
    public event DiceRollCompleteDelegate OnDiceRollComplete;

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
        
        audioManager = AudioManager.Instance;
        
        // Check if it exists
        if (audioManager == null)
        {
            Debug.LogError("AudioManager not found in the scene. Make sure it's set up properly.");
        }
    }

    public void RollBothDiceAndShowSum()
    {
        rollButton.interactable = false;
        DiceHaveFinishedRolling = false;
        StartCoroutine(RollAndShowSum());
    }

    private IEnumerator RollAndShowSum()
    {
        sumText.text = "Lancer en cours...";
        dice1.RollTheDice();
        dice2.RollTheDice();
        audioManager.PlayDiceRolling();
        
        yield return new WaitUntil(() => dice1.HasStopped && dice2.HasStopped);
        
        LastRollSum = dice1.GetRollValue() + dice2.GetRollValue();
        sumText.text = "Somme : " + LastRollSum;
        
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
}