using UnityEngine;

public class WaypointScript : MonoBehaviour
{
    public GameObject path;
    public IntersectionUIManager uiManager; // Reference to UI
    private GameObject[] waypoints;
    public float speed = 2;

    private int currentWaypointIndex = 0;
    private int targetWaypointIndex = 0;
    private bool isMoving = false;
    private bool reachedIntersection = false;
    private GameObject lastWaypointBeforeIntersection; // Stores last waypoint before reaching an intersection

    void Start()
    {
        if (path != null)
        {
            waypoints = new GameObject[path.transform.childCount];

            for (int i = 0; i < path.transform.childCount; i++)
            {
                waypoints[i] = path.transform.GetChild(i).gameObject;
            }
        }
    }

    void Update()
    {
        if (isMoving && !reachedIntersection && waypoints.Length > 0)
        {
            if (transform.position != waypoints[currentWaypointIndex].transform.position)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    waypoints[currentWaypointIndex].transform.position,
                    speed * Time.deltaTime
                );
            }
            else
            {
                if (waypoints[currentWaypointIndex].CompareTag("Intersection"))
                {
                    reachedIntersection = true;
                    isMoving = false;
                    uiManager.ShowUI(this); // Show path selection UI
                }
                else
                {
                    if (currentWaypointIndex < targetWaypointIndex)
                    {
                        // Store the last waypoint before moving to the next
                        lastWaypointBeforeIntersection = waypoints[currentWaypointIndex];
                        currentWaypointIndex++;
                    }
                    else
                    {
                        isMoving = false;
                    }
                }
            }
        }
    }

    public void MovePlayer(int steps)
    {
        if (!reachedIntersection)
        {
            targetWaypointIndex = Mathf.Min(currentWaypointIndex + steps, waypoints.Length - 1);
            isMoving = true;
        }
    }

    public void ResumeMovement(GameObject newPath, bool stayOnSamePath)
    {
        if (newPath != null)
        {
            path = newPath;
            waypoints = new GameObject[path.transform.childCount];

            for (int i = 0; i < path.transform.childCount; i++)
            {
                waypoints[i] = path.transform.GetChild(i).gameObject;
            }

            currentWaypointIndex = 0;
        }
        else if (stayOnSamePath)
        {
            if (currentWaypointIndex < waypoints.Length - 1)
            {
                currentWaypointIndex++; // Move to the next waypoint
            }
        }

        reachedIntersection = false;
        isMoving = true;
    }

    // Returns the exact waypoint before reaching an intersection
    public GameObject GetCurrentWaypoint()
    {
        return waypoints[currentWaypointIndex];
    }

    // Returns the last waypoint before reaching an intersection
    public GameObject GetLastPath()
    {
        if (lastWaypointBeforeIntersection != null)
        {
            Debug.Log(" Last waypoint before intersection: " + lastWaypointBeforeIntersection.name);
            return lastWaypointBeforeIntersection;
        }
        else
        {
            Debug.LogWarning(" Last waypoint is NULL! The player may not have moved yet.");
            return null;
        }
    }
}




