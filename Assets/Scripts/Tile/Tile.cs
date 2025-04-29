using UnityEngine;

public class Tile : MonoBehaviour
{

    public enum TileType { Question, Card, Intersection }
    public enum Region { Vulkan, Atlanta, Celestyel, Berg, None }
    public int position;
    public TileType type;
    public Region region;

    public virtual void OnPlayerLands()
    {
        // Skip tile effects if this movement was triggered by a question/card effect
        if (GameManager.Instance.isEffectMovement)
        {
            Debug.Log($"🎯 Le joueur a atterri sur une tuile {type} dans la région {region} à la position {position} - effet ignoré car mouvement par effet.");
            return;
        }

        Debug.Log($"🎯 Le joueur a atterri sur une tuile {type} dans la région {region} à la position {position}.");

        // Ne pas enregistrer les intersections comme points d'atterrissage
        if (type != TileType.Intersection)
        {
            // Récupérer le joueur qui a atterri sur la tuile
            Player player = GetLandingPlayer();

            // Si un joueur est trouvé, enregistrer sa position comme point d'atterrissage
            if (player != null)
            {
                player.RegisterLandingPosition();
            }
        }
    }

    // Méthode helper pour obtenir le joueur qui vient d'atterrir
    // Méthode helper pour obtenir le joueur qui vient d'atterrir
    private Player GetLandingPlayer()
    {
        // Vérifier les collisions ou utiliser GameManager pour obtenir le joueur actuel
        if (GameManager.Instance != null && GameManager.Instance.selectedPlayer != null)
        {
            // Convertir le GameObject en Player
            return GameManager.Instance.selectedPlayer.GetComponent<Player>();
        }

        // Alternative: détecter par collision
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.5f);
        foreach (Collider col in colliders)
        {
            Player player = col.GetComponent<Player>();
            if (player != null)
            {
                return player;
            }
        }

        return null;
    }
}