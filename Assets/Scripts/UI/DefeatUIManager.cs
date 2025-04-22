// Version simplifiée du gestionnaire d'écran de défaite
using UnityEngine;
using TMPro;

public class DefeatUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject defeatPanel;
    public TextMeshProUGUI defeatMessageText;

    private void Start()
    {
        // Cacher le panneau au démarrage
        if (defeatPanel != null)
            defeatPanel.SetActive(false);
    }

    // Afficher l'écran de défaite avec un message personnalisé
    public void ShowDefeatScreen(Player losingPlayer)
    {
        if (defeatPanel == null)
        {
            Debug.LogError("❌ Panneau de défaite non assigné dans DefeatUIManager!");
            return;
        }

        // Afficher le panneau
        defeatPanel.SetActive(true);

        // Mettre à jour le texte du message
        if (defeatMessageText != null)
        {
            string playerName = losingPlayer != null ? losingPlayer.gameObject.name : "Un joueur";
            defeatMessageText.text = $"You lose hhh!";
        }
    }
}