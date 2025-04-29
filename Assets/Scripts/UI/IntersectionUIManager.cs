using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class IntersectionUIManager : MonoBehaviour
{
    [Header("Main UI Panel")]
    public GameObject intersectionPanel; // Single panel containing all buttons

    [Header("Path Buttons")]
    public Button samePathButton;   // "Same Path" button
    public Button forwardPathButton; // "Forward Path" button
    public Button backwardPathButton; // "Backward Path" button

    private Player playerScript;
    private List<GameObject> availablePaths = new List<GameObject>(); // Available paths

    void Start()
    {
        // Hide panel at start
        intersectionPanel.SetActive(false);

        // Set up button listeners
        samePathButton.onClick.AddListener(StayOnPath);
        forwardPathButton.onClick.AddListener(() => SelectPath(0));
        backwardPathButton.onClick.AddListener(() => SelectPath(1));
    }

    // Called when player reaches an intersection
    public void ShowUI(Player player)
    {
        playerScript = player;

        // Get available paths at this intersection
        GameObject currentWaypoint = playerScript.GetCurrentWaypoint();
        if (currentWaypoint == null)
        {
            Debug.LogError("❌ No current waypoint found!");
            return;
        }

        IntersectionPoint intersection = currentWaypoint.GetComponent<IntersectionPoint>();
        if (intersection != null)
        {
            availablePaths = intersection.GetAvailablePaths(currentWaypoint, playerScript.GetLastPath());

            // Show the intersection panel
            intersectionPanel.SetActive(true);

            // Enable/disable path buttons based on available options
            samePathButton.gameObject.SetActive(true); // Always show same path option

            // Show other path buttons only if we have paths available
            forwardPathButton.gameObject.SetActive(availablePaths.Count > 0);
            backwardPathButton.gameObject.SetActive(availablePaths.Count > 1);
        }
        else
        {
            Debug.LogError("❌ The current intersection doesn't have an IntersectionPoint script!");
        }
    }

    // Handler for staying on the same path
    private void StayOnPath()
    {
        intersectionPanel.SetActive(false);
        playerScript.ResumeMovement(null, true);
    }

    // Handler for selecting a different path
    private void SelectPath(int pathIndex)
    {
        if (pathIndex >= 0 && pathIndex < availablePaths.Count)
        {
            intersectionPanel.SetActive(false);
            playerScript.ResumeMovement(availablePaths[pathIndex], false);
        }
        else
        {
            Debug.LogError($"❌ Invalid path index: {pathIndex}");
        }
    }
}