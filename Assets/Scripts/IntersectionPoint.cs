using System.Collections.Generic;
using UnityEngine;

public class IntersectionPoint : MonoBehaviour
{
    [System.Serializable]
    public class PathOptions
    {
        public GameObject fromPath; // The path the player arrives from
        public List<GameObject> toPaths = new List<GameObject>(); // Possible next paths
    }

    [Header("Define paths and their possible destinations")]
    public List<PathOptions> intersectionPaths = new List<PathOptions>();

    // Function to get possible paths based on the player's arrival path
    public List<GameObject> GetAvailablePaths(GameObject arrivingFrom)
    {
        Debug.Log("🔎 Checking available paths for: " + (arrivingFrom != null ? arrivingFrom.name : "NULL"));

        if (intersectionPaths.Count == 0)
        {
            Debug.LogError("❌ No intersection paths are assigned in the Inspector!");
            return new List<GameObject>();
        }

        List<GameObject> availablePaths = new List<GameObject>(); // This will store the valid paths

        foreach (PathOptions option in intersectionPaths)
        {
            if (option.fromPath == null)
            {
                Debug.LogError("⚠️ A 'fromPath' entry in intersectionPaths is NULL!");
                continue;
            }

            Debug.Log("📍 From Path: " + option.fromPath.name);

            if (option.fromPath == arrivingFrom) // This is where it checks the reference
            {
                Debug.Log("✅ Match Found! Adding paths...");

                if (option.toPaths == null || option.toPaths.Count == 0)
                {
                    Debug.LogError("⚠️ From path " + option.fromPath.name + " has NO assigned destinations!");
                }
                else
                {
                    foreach (GameObject path in option.toPaths)
                    {
                        Debug.Log("➡️ Adding path: " + path.name);
                        availablePaths.Add(path);
                    }
                }
            }
        }

        Debug.Log("📌 Final available paths count: " + availablePaths.Count);
        return availablePaths;
    }




}