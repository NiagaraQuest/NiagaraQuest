using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class IntersectionUIManager : MonoBehaviour
{
    public GameObject panelChoices; // Le premier menu avec Stay / Change Path
    public GameObject panelPathOptions; // Le menu affichant les chemins disponibles
    public Button buttonStayOnPath;
    public Button buttonChangePath;
    public Button buttonPrefab; // Bouton modèle pour générer les options

    private Player playerScript;
    private List<GameObject> availablePaths = new List<GameObject>(); // Chemins disponibles

    void Start()
    {
        panelChoices.SetActive(false);
        panelPathOptions.SetActive(false);
        buttonStayOnPath.onClick.AddListener(StayOnPath);
        buttonChangePath.onClick.AddListener(ShowPathOptions);
    }

    public void ShowUI(Player player)
    {
        playerScript = player;
        panelChoices.SetActive(true);
    }

    void StayOnPath()
    {
        panelChoices.SetActive(false);
        playerScript.ResumeMovement(null, true);
    }

    void ShowPathOptions()
    {
        panelChoices.SetActive(false);
        panelPathOptions.SetActive(true);
        GeneratePathButtons();
    }

    void GeneratePathButtons()
    {
        GameObject currentWaypoint = playerScript.GetCurrentWaypoint();
        if (currentWaypoint == null)
        {
            Debug.LogError("❌ Aucun waypoint actuel trouvé !");
            return;
        }

        IntersectionPoint intersection = currentWaypoint.GetComponent<IntersectionPoint>();
        if (intersection != null)
        {
            availablePaths = intersection.GetAvailablePaths(currentWaypoint, playerScript.GetLastPath());

            if (availablePaths.Count == 0)
            {
                Debug.LogWarning("❌ Aucun chemin possible trouvé !");
                return;
            }

            // Suppression des anciens boutons
            foreach (Transform child in panelPathOptions.transform)
            {
                Destroy(child.gameObject);
            }

            // Génération des nouveaux boutons
            foreach (GameObject path in availablePaths)
            {
                Button newButton = Instantiate(buttonPrefab, panelPathOptions.transform);
                newButton.transform.localScale = Vector3.one;

                var textComponent = newButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = path.name;
                }

                // 🔥 FIXED: Passer un GameObject au lieu d'un string
                newButton.onClick.AddListener(() => SelectPath(path));
            }
        }
        else
        {
            Debug.LogError("❌ L'intersection actuelle n'a pas de script IntersectionPoint !");
        }
    }

    void SelectPath(GameObject selectedPath)
    {
        panelPathOptions.SetActive(false);
        playerScript.ResumeMovement(selectedPath, false);
    }
}











