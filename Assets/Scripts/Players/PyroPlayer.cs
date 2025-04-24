using UnityEngine;

public class PyroPlayer : Player
{
    [Header("Pyro Player Settings")]
    [SerializeField] public bool useSecondChance = false; // 🔥 Visible in Inspector
    


    protected override void Start()
    {
        currentPath = "PyroPath";
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        if (!isMoving && HasFinishedMoving) // When Pyro stops moving
        {
            GameObject waypoint = GetCurrentWaypoint();
            if (waypoint != null)
            {
                Tile tile = waypoint.GetComponent<Tile>();
                if (tile != null)
                {
                    HandleSecondChance(tile);
                }
            }
        }
    }

    private void HandleSecondChance(Tile tile)
    {
        if (tile.region == Tile.Region.None)
        {
            return; // Ignore intersections
        }

        if (tile.region == Tile.Region.Vulkan)
        {
            if (!useSecondChance)
            {
                ActivateSecondChance();
            }
            else
            {
                Debug.Log($"🔥 Second chance STILL ACTIVE!");
            }
        }
        else
        {
            if (useSecondChance)
            {
                DeactivateSecondChance();
            }
            else
            {
                Debug.Log($"⚠️ Second chance ALREADY DEACTIVATED!");
            }
        }
    }

    private void ActivateSecondChance()
    {
        useSecondChance = true;
        Debug.Log($"🔥 Second chance ACTIVATED! You can retry a question if you fail.");
    }

    private void DeactivateSecondChance()
    {
        useSecondChance = false;
        Debug.Log($"⚠️ Second chance DEACTIVATED! No retries available.");
    }

    //  OVERRIDING AnswerQuestion TO INCLUDE SECOND CHANCE LOGIC!
    public override void AnswerQuestion(bool isCorrect)
    {
        if (isCorrect)
        {
            Debug.Log("✅ Correct answer! Proceeding...");
            return; // Normal behavior
        }

        // ❌ If the answer is wrong and Second Chance is active, allow a retry
        if (useSecondChance)
        {
            Debug.Log("🔁 Incorrect! But you have a second chance. Try again!");
            useSecondChance = false; // Second chance is used up
        }
        else
        {
            Debug.Log("❌ Incorrect answer. No second chance available.");
        }
    }
}

