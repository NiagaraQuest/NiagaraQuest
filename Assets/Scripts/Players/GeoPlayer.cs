using UnityEngine;

public class GeoPlayer : Player
{
    [Header("🛡️ GeoPlayer Settings")]

    private bool shieldActive = false;


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
        shieldActive = true;
        lives *= 2;
        Debug.Log($"🛡️ Départ ! Shield ACTIVÉ ! Vies : {lives}");
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
                if (tile != null)
                {
                    HandleShield(tile);
                }
            }
        }
    }

    private void HandleShield(Tile tile)
    {
        if (tile.region == Tile.Region.None)
        {
            return; // Ignorer les intersections
        }

        if (tile.region == Tile.Region.Berg)
        {
            if (!shieldActive)
            {
                ActivateShield();
            }
            else
            {
                Debug.Log($"🛡️ Shield TOUJOURS ACTIVÉ ! Vies : {lives}");
            }
        }
        else
        {
            if (shieldActive)
            {
                DeactivateShield();
            }
            else
            {
                Debug.Log($"⚠️ Shield DÉJÀ DÉSACTIVÉ ! Vies : {lives}");
            }
        }
    }

    private void ActivateShield()
    {
        shieldActive = true;
        lives *= 2; //  Double les vies
        Debug.Log($"🛡️ Shield ACTIVÉ ! Vies : {lives}");
    }

    private void DeactivateShield()
    {
        shieldActive = false;

        // Vérifier si le nombre de vies est impair avant la division
        if (lives % 2 != 0) // Si impair
        {
            // Division avec arrondi supérieur
            lives = (lives + 1) / 2;
            Debug.Log($"⚠️ Shield DÉSACTIVÉ ! Vies impaires arrondies vers le haut : {lives}");
        }
        else // Si pair
        {
            // Division normale pour les nombres pairs
            lives /= 2;
            Debug.Log($"⚠️ Shield DÉSACTIVÉ ! Vies : {lives}");
        }
    }
    public override void GainLife()
    {
        GameObject waypoint = GetCurrentWaypoint();
        if (waypoint != null)
        {
            Tile tile = waypoint.GetComponent<Tile>();
            if (tile != null && tile.region == Tile.Region.Berg)
            {
                lives += 2;
                Debug.Log($"💚 Dans sa région (Berg) → GeoPlayer gagne 2 vies ! Total : {lives}");
                return;
            }
        }

        lives += 1;
        Debug.Log($"💚 Hors région → GeoPlayer gagne 1 vie. Total : {lives}");
    }

}
