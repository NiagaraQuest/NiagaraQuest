using System.Collections.Generic;
using UnityEngine;

public class IntersectionPoint : MonoBehaviour
{
    [System.Serializable]
    public class ToPathData
    {
        public GameObject toPath; // Destination path
        public bool moveForward;  // ✅ True = Forward, False = Backward
    }

    [System.Serializable]
    public class PathMapping
    {
        public GameObject fromPath; // Path the player arrives from
        public List<ToPathData> toPaths = new List<ToPathData>(); // Possible destinations + move direction
    }

    [System.Serializable]
    public class IntersectionData
    {
        public GameObject intersectionPoint; // The intersection itself
        public List<PathMapping> paths = new List<PathMapping>(); // The mapping for this intersection
    }

    [Header("Manually assign intersection paths")]
    public List<IntersectionData> intersectionList = new List<IntersectionData>();

    private Dictionary<GameObject, Dictionary<GameObject, List<ToPathData>>> intersectionMap =
        new Dictionary<GameObject, Dictionary<GameObject, List<ToPathData>>>();

    void Awake()
    {
        PopulateDictionary();
    }

    private void PopulateDictionary()
    {
        foreach (var intersection in intersectionList)
        {
            if (intersection.intersectionPoint == null)
            {
                Debug.LogError("❌ Intersection point is missing!");
                continue;
            }

            if (!intersectionMap.ContainsKey(intersection.intersectionPoint))
            {
                intersectionMap[intersection.intersectionPoint] = new Dictionary<GameObject, List<ToPathData>>();
            }

            foreach (var mapping in intersection.paths)
            {
                if (mapping.fromPath == null || mapping.toPaths.Count == 0)
                {
                    Debug.LogWarning($"⚠️ Skipping entry with missing paths at {intersection.intersectionPoint.name}");
                    continue;
                }

                intersectionMap[intersection.intersectionPoint][mapping.fromPath] = new List<ToPathData>(mapping.toPaths);
            }
        }
    }

    public List<GameObject> GetAvailablePaths(GameObject intersectionPoint, GameObject arrivingFrom)
    {
        if (intersectionMap.ContainsKey(intersectionPoint) && intersectionMap[intersectionPoint].ContainsKey(arrivingFrom))
        {
            List<GameObject> paths = new List<GameObject>();
            foreach (var data in intersectionMap[intersectionPoint][arrivingFrom])
            {
                paths.Add(data.toPath); // Only return the path, not the direction
            }
            return paths;
        }

        Debug.LogWarning($"⚠️ No valid paths found for {intersectionPoint?.name} coming from {arrivingFrom?.name}");
        return new List<GameObject>();
    }

    public bool GetPathDirection(GameObject intersectionPoint, GameObject arrivingFrom, GameObject selectedPath)
    {
        if (intersectionMap.ContainsKey(intersectionPoint) && intersectionMap[intersectionPoint].ContainsKey(arrivingFrom))
        {
            foreach (var data in intersectionMap[intersectionPoint][arrivingFrom])
            {
                if (data.toPath == selectedPath)
                {
                    return data.moveForward; // ✅ Return stored movement direction
                }
            }
        }

        Debug.LogWarning($"⚠️ No movement direction found for {selectedPath?.name} at {intersectionPoint?.name}");
        return true; // Default: move forward
    }

    public void DebugIntersections()
    {
        foreach (var intersection in intersectionMap)
        {
            Debug.Log($"🔵 Intersection: {intersection.Key.name}");
            foreach (var pathEntry in intersection.Value)
            {
                Debug.Log($"↪️ From: {pathEntry.Key.name}");
                foreach (var toPathData in pathEntry.Value)
                {
                    string direction = toPathData.moveForward ? "Forward" : "Backward";
                    Debug.Log($"➡️ To: {toPathData.toPath.name} ({direction})");
                }
            }
        }
    }
}