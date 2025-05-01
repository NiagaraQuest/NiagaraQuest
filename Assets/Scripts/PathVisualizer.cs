using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Ultra simple path highlighting that just changes the color of objects
/// </summary>
public class PathColorChanger : MonoBehaviour
{
    // References to path parent objects
    public GameObject pyroPathParent;
    public GameObject hydroPathParent;
    public GameObject geoPathParent;
    public GameObject anemoPathParent;

    // Bright colors for highlighting
    public Color pyroHighlightColor = Color.red;
    public Color hydroHighlightColor = Color.blue;
    public Color geoHighlightColor = Color.yellow;
    public Color anemoHighlightColor = Color.green;

    // Normal colors when not highlighted
    public Color defaultColor = Color.gray;

    // Store renderers
    private List<Renderer> pyroRenderers = new List<Renderer>();
    private List<Renderer> hydroRenderers = new List<Renderer>();
    private List<Renderer> geoRenderers = new List<Renderer>();
    private List<Renderer> anemoRenderers = new List<Renderer>();

    // Current highlighted path
    private string currentPath = "";

    // Singleton
    public static PathColorChanger Instance;

    void Awake()
    {
        // Setup singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Gather all renderers
        CollectRenderers();

        // Set all to default
        ResetAllPaths();
    }

    void CollectRenderers()
    {
        // Clear all lists
        pyroRenderers.Clear();
        hydroRenderers.Clear();
        geoRenderers.Clear();
        anemoRenderers.Clear();

        // Collect PyroPath renderers
        if (pyroPathParent != null)
        {
            CollectRenderersFromObject(pyroPathParent, pyroRenderers);
            Debug.Log($"Found {pyroRenderers.Count} renderers in PyroPath");
        }

        // Collect HydroPath renderers
        if (hydroPathParent != null)
        {
            CollectRenderersFromObject(hydroPathParent, hydroRenderers);
            Debug.Log($"Found {hydroRenderers.Count} renderers in HydroPath");
        }

        // Collect GeoPath renderers
        if (geoPathParent != null)
        {
            CollectRenderersFromObject(geoPathParent, geoRenderers);
            Debug.Log($"Found {geoRenderers.Count} renderers in GeoPath");
        }

        // Collect AnemoPath renderers
        if (anemoPathParent != null)
        {
            CollectRenderersFromObject(anemoPathParent, anemoRenderers);
            Debug.Log($"Found {anemoRenderers.Count} renderers in AnemoPath");
        }
    }

    void CollectRenderersFromObject(GameObject obj, List<Renderer> renderersList)
    {
        // Get renderer from this object
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderersList.Add(renderer);
        }

        // Get renderers from children
        foreach (Transform child in obj.transform)
        {
            renderer = child.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderersList.Add(renderer);
            }
        }
    }

    public void HighlightPath(string pathName)
    {
        // Skip if same path
        if (pathName == currentPath)
        {
            return;
        }

        // Reset all paths first
        ResetAllPaths();

        // Set current path
        currentPath = pathName;

        // Change color based on path
        switch (pathName)
        {
            case "PyroPath":
                ChangePathColor(pyroRenderers, pyroHighlightColor);
                Debug.Log("Highlighting PyroPath");
                break;

            case "HydroPath":
                ChangePathColor(hydroRenderers, hydroHighlightColor);
                Debug.Log("Highlighting HydroPath");
                break;

            case "GeoPath":
                ChangePathColor(geoRenderers, geoHighlightColor);
                Debug.Log("Highlighting GeoPath");
                break;

            case "AnemoPath":
                ChangePathColor(anemoRenderers, anemoHighlightColor);
                Debug.Log("Highlighting AnemoPath");
                break;

            default:
                Debug.LogWarning($"Unknown path: {pathName}");
                break;
        }
    }

    void ChangePathColor(List<Renderer> renderers, Color color)
    {
        foreach (Renderer r in renderers)
        {
            if (r != null)
            {
                r.material.color = color;
            }
        }
    }

    public void ResetAllPaths()
    {
        // Reset PyroPath
        ChangePathColor(pyroRenderers, defaultColor);

        // Reset HydroPath
        ChangePathColor(hydroRenderers, defaultColor);

        // Reset GeoPath
        ChangePathColor(geoRenderers, defaultColor);

        // Reset AnemoPath
        ChangePathColor(anemoRenderers, defaultColor);

        // Clear current path
        currentPath = "";
    }

    // Call this to highlight the path for a player
    public void HighlightForPlayer(Player player)
    {
        if (player != null)
        {
            HighlightPath(player.currentPath);
        }
    }
}