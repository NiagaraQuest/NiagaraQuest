using UnityEngine;

public class AnemoPlayer : Player
{
    private bool isBoostActive = false; // Tracks if the boost is active

    protected override void Start()
    {
        currentPath = "AnemoPath";
        base.Start();
    }

    public override void MovePlayer(int steps)
    {
        GameObject currentWaypoint = GetCurrentWaypoint();
        if (currentWaypoint == null) return;

        Tile tile = currentWaypoint.GetComponent<Tile>();

        if (tile != null)
        {
            // ✅ Log a message every time the player moves onto a Celestyel tile
            if (tile.region == Tile.Region.Celestyel)
            {
                Debug.Log($"🌪️ Détection: Le joueur est sur une tuile de la région Celestyel (Position: {tile.position})");

                if (!isBoostActive)
                {
                    isBoostActive = true;
                    Debug.Log("🌪️ Entrée dans Celestyel ! Capacité Anemo activée.");
                }

                // ✅ Apply movement boost
                float boostedSteps = steps * (4f / 3f);
                steps = Mathf.RoundToInt(boostedSteps);
                Debug.Log($"🌪️ Bonus Anemo ! Déplacement boosté à {steps} pas dans Celestyel.");
            }
            else
            {
                // ✅ Log when leaving Celestyel
                if (isBoostActive)
                {
                    isBoostActive = false;
                    Debug.Log("🌪️ Sortie de Celestyel. Capacité Anemo désactivée.");
                }
            }
        }

        base.MovePlayer(steps);
    }
}
