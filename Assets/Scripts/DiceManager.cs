using UnityEngine;
using UnityEngine.UI; // Ajout� pour Button
using TMPro; // Ajout� pour TextMeshProUGUI
using System.Collections; // Ajout� pour IEnumerator

public class DiceManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI sumText;
    [SerializeField] private theDice dice1;
    [SerializeField] private theDice dice2;
    [SerializeField] private Button rollButton;
    [SerializeField] private WaypointScript playerMovement; // R�f�rence au script de mouvement

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

        // D�placer le joueur en fonction du r�sultat du d�
        playerMovement.MovePlayer(LastRollSum);
    }
}