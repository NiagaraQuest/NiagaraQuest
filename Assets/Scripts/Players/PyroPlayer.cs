using UnityEngine;

public class PyroPlayer : Player
{
    [Header("Pyro Player Settings")]
    [SerializeField] public bool useSecondChance = false;

    protected override void Start()
    {
        currentPath = "PyroPath";
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        if (!isMoving && HasFinishedMoving)
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
        if (tile.region == Tile.Region.Vulkan)
        {
            if (!useSecondChance)
            {
                ActivateSecondChance();
            }
        }
        else if (useSecondChance)
        {
            DeactivateSecondChance();
        }
    }

    private void ActivateSecondChance()
    {
        useSecondChance = true;
        Debug.Log($"🔥 Second chance ACTIVATED in Vulkan region!");
    }

    private void DeactivateSecondChance()
    {
        useSecondChance = false;
        Debug.Log($"⚠️ Second chance DEACTIVATED outside Vulkan region");
    }

    public override void AnswerQuestion(bool isCorrect)
    {
        if (isCorrect)
        {
            useSecondChance = false; // Réinitialiser si bonne réponse
            base.AnswerQuestion(true);
            return;
        }

        // La logique de seconde chance est gérée par QuestionUIManager
        base.AnswerQuestion(false);
    }
}