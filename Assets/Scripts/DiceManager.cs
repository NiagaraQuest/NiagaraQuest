using UnityEngine;
using UnityEngine.UI; // Ajouté pour Button
using TMPro; // Ajouté pour TextMeshProUGUI
using System.Collections; // Ajouté pour IEnumerator

public class DiceManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI sumText;
    [SerializeField] private theDice dice1;
    [SerializeField] private theDice dice2;
    [SerializeField] private Button rollButton;
    [SerializeField] private WaypointScript playerMovement; // Référence au script de mouvement

    public int LastRollSum { get; private set; }

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

        // Déplacer le joueur en fonction du résultat du dé
        playerMovement.MovePlayer(LastRollSum);
    }
}