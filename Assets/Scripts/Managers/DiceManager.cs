
﻿using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DiceManager : MonoBehaviour
{

    public static DiceManager Instance { get; private set; }

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
    }








    [SerializeField] private TextMeshProUGUI sumText;
    [SerializeField] private theDice dice1;
    [SerializeField] private theDice dice2;
    [SerializeField] private Button rollButton;

    public int LastRollSum { get; private set; }
    public GameManager gameManager; // ✅ Reference to GameManager

    public void RollBothDiceAndShowSum()
    {
        rollButton.interactable = false;
        StartCoroutine(RollAndShowSum());
    }

    private IEnumerator RollAndShowSum()
    {
        sumText.text = "Lancer en cours...";
        dice1.RollTheDice();
        dice2.RollTheDice();

        yield return new WaitUntil(() => dice1.HasStopped && dice2.HasStopped);

        LastRollSum = dice1.GetRollValue() + dice2.GetRollValue();
        sumText.text = "Somme : " + LastRollSum;
        rollButton.interactable = true;

        gameManager.OnDiceRolled(); // ✅ Notify GameManager
    }
    // Dans la classe DiceManager
    public void EnableRollButton()
    {
        rollButton.interactable = true;
    }

    // Dans la classe DiceManager
    public void DisableRollButton()
    {
        rollButton.interactable = false;
    }
}
