using UnityEngine;
public class GeoPlayer : Player
{
    [Header("🛡️ GeoPlayer Settings")]
    [SerializeField] private bool isInBerg = false;
    [SerializeField] private bool shield = false; // Visible in inspector for debugging

    protected override void Start()
    {
        currentPath = "GeoPath";
        base.Start();
        if (playerProfile == null)
        {
            Debug.LogError($"{gameObject.name} → ❌ Pas de profil assigné !");
        }
        else
        {
            Debug.Log($"✅ {gameObject.name} → Profil: {playerProfile.Username}");
        }
        //  Activer le shield dès le départ 
        isInBerg = true;
        shield = true;
        Debug.Log($"🛡️ Départ ! Shield ACTIVÉ !");
    }

    public void InitializeShield()
    {
        isInBerg = true;
        shield = true;
        Debug.Log($"🛡️ Initialisation spéciale! Shield ACTIVÉ !");
    }

    protected override void Update()
    {
        base.Update();
        if (!isMoving && HasFinishedMoving) //  WHEN le joueur termine son mouvement
        {
            GameObject waypoint = GetCurrentWaypoint();
            if (waypoint != null)
            {
                Tile tile = waypoint.GetComponent<Tile>();
                if (tile != null && tile.region != Tile.Region.None)
                {
                    HandleRegionChange(tile);
                }
            }
        }
    }

    private void HandleRegionChange(Tile tile)
    {
        if (tile.region == Tile.Region.None)
        {
            return; // Ignorer les intersections
        }

        bool wasInBerg = isInBerg;
        isInBerg = (tile.region == Tile.Region.Berg);

        // Si le joueur entre dans Berg (n'y était pas avant)
        if (isInBerg && !wasInBerg)
        {
            // Reset shield à true quand on entre dans Berg
            shield = true;
            Debug.Log($"🛡️ Entrée dans la région Berg ! Shield ACTIVÉ !");
        }
    }

    // Override pour gérer le shield UNIQUEMENT avec LoseLife
    public override void LoseLife()
    {
        if (isInBerg && shield)
        {
            // Utiliser le shield au lieu de perdre une vie
            shield = false;
            Debug.Log($"🛡️ Shield utilisé ! {gameObject.name} est protégé contre la perte de vie !");
        }
        else
        {
            // Comportement normal - perdre une vie
            if (lives > 0)
            {
                lives--;
                Debug.Log($"❌ {gameObject.name} a perdu une vie ! Vies restantes : {lives}");

                // Après avoir perdu une vie dans Berg, réactiver le shield
                if (isInBerg)
                {
                    shield = true;
                    Debug.Log($"🛡️ Shield réactivé après la perte de vie !");
                }
            }
            else
            {
                Debug.Log($"💀 {gameObject.name} n'a plus de vies !");
            }
        }

        // Important: check lives after any life loss
        GameManager.Instance.CheckPlayerLives();
    }

    public override void GainLife()
    {
        // Toujours gagner 1 vie, même dans Berg
        lives += 1;
        Debug.Log($"💚 {gameObject.name} gagne 1 vie. Total : {lives}");
    }
}