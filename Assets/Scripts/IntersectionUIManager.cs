using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class IntersectionUIManager : MonoBehaviour
{
    public GameObject panelChoices; // The panel containing buttons for staying or switching paths
    public GameObject panelPathOptions; // Panel that will contain dynamically created buttons
    public Button buttonStayOnPath; // Stay on the same path
    public Button buttonChangePath; // Button to switch paths
    public Button buttonPrefab; // Prefab for dynamically generated buttons

    private WaypointScript playerScript; // Reference to player movement script
    private List<GameObject> availablePaths = new List<GameObject>(); // List of available paths

    void Start()
    {
        panelChoices.SetActive(false);
        panelPathOptions.SetActive(false);
        buttonStayOnPath.onClick.AddListener(StayOnPath);
        buttonChangePath.onClick.AddListener(ShowPathOptions);
    }

    public void ShowUI(WaypointScript player)
    {
        playerScript = player;
        panelChoices.SetActive(true);
    }

    void StayOnPath()
    {
        panelChoices.SetActive(false);
        playerScript.ResumeMovement(null, true); // Continue moving forward
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
            availablePaths = intersection.GetAvailablePaths(playerScript.GetLastPath());

            Debug.Log("✅ Chemins trouvés : " + availablePaths.Count);
            foreach (var path in availablePaths)
            {
                Debug.Log("➡️ Chemin possible : " + path.name);
            }

            if (availablePaths.Count == 0)
            {
                Debug.LogWarning("❌ Aucun chemin possible trouvé !");
                return;
            }

            foreach (Transform child in panelPathOptions.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (GameObject path in availablePaths)
            {
                Button newButton = Instantiate(buttonPrefab, panelPathOptions.transform);
                newButton.transform.localScale = Vector3.one; // Ensure correct scaling

                LayoutElement layout = newButton.GetComponent<LayoutElement>();
                if (layout == null)
                {
                    layout = newButton.gameObject.AddComponent<LayoutElement>();
                }
                layout.preferredHeight = 50; // Ensure spacing

                var textComponent = newButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = path.name;
                }
                else
                {
                    Debug.LogError("⚠️ Aucun composant Text trouvé dans le bouton !");
                }
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

