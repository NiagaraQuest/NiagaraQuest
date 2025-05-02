using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum TileType { Question, Card, Intersection }
    public enum Region { Vulkan, Atlanta, Celestyel, Berg, None }

    public int position;
    public TileType type;
    public Region region;

    // Add path name reference for easier access
    [HideInInspector]
    public string pathName;

    // DEBUGGING VARIABLES
    [Header("Debug Information")]
    [SerializeField] protected bool _debug_isEffectMovement = false;
    [SerializeField] protected bool _debug_isFinalTile = false;
    [SerializeField] protected int _debug_playerIndex = -1;
    [SerializeField] protected bool _debug_effectOverridden = false;
    [SerializeField] protected string _debug_playerName = "None";

    private void Start()
    {
        // Automatically determine the path name based on parent object
        pathName = transform.parent?.name ?? "Unknown";
    }

    // 1. CORRECTION DANS Tile.cs (classe de base pour toutes les tuiles)

    public virtual void OnPlayerLands()
    {
        // Détecter le joueur qui a vraiment atterri sur cette tuile
        // Cette méthode doit être plus précise pour éviter la désynchronisation
        Player landingPlayer = GetLandingPlayer();

        // Si on a trouvé un joueur, mettre à jour currentQuestionPlayer dans GameManager
        // C'est CRUCIAL pour s'assurer que le bon joueur est traité
        if (landingPlayer != null)
        {
            landingPlayer.isMoving = false; 
            GameManager.Instance.SetCurrentQuestionPlayer(landingPlayer);

            // Vérifier si c'est une case finale
            bool isFinalTile = (landingPlayer.currentWaypointIndex >= 50);

            Debug.Log($"🔧 DEBUG: OnPlayerLands - Tuile: {pathName}:{position}, Type: {type}, " +
                     $"Joueur: {landingPlayer.gameObject.name}, Position: {landingPlayer.currentWaypointIndex}, " +
                     $"EstCaseFinale: {isFinalTile}, EstMouvementEffet: {GameManager.Instance.isEffectMovement}");

            // Si ce n'est pas une case finale et c'est un mouvement d'effet, ignorer l'effet de tuile
            if (GameManager.Instance.isEffectMovement && !isFinalTile)
            {
                Debug.Log($"🎯 {landingPlayer.gameObject.name} a atterri sur une tuile {type} - effet ignoré car mouvement par effet.");
                return; // Ne pas déclencher l'effet de tuile
            }

            // Si c'est une case finale avec mouvement d'effet, le noter spécifiquement
            if (isFinalTile && GameManager.Instance.isEffectMovement)
            {
                Debug.Log($"🏁 IMPORTANT: {landingPlayer.gameObject.name} a atterri sur une CASE FINALE via un mouvement d'effet!");
            }

            Debug.Log($"🎯 {landingPlayer.gameObject.name} a atterri sur une tuile {type} dans la région {region}.");
        }
        else
        {
            Debug.LogWarning($"⚠️ Aucun joueur détecté sur la tuile {pathName}:{position}");
        }
    }

    // Amélioration de GetLandingPlayer pour être plus précis
    private Player GetLandingPlayer()
    {
        // 1. Essayer d'abord avec le joueur actuel qui est en train de bouger
        if (GameManager.Instance.selectedPlayer != null)
        {
            Player selectedPlayer = GameManager.Instance.selectedPlayer.GetComponent<Player>();
            if (selectedPlayer != null &&
                selectedPlayer.currentPath == pathName &&
                selectedPlayer.currentWaypointIndex == position)
            {
                return selectedPlayer;
            }
        }

        // 2. Vérifier avec le dernier joueur qui a répondu à une question
        if (GameManager.Instance.currentQuestionPlayer != null &&
            GameManager.Instance.currentQuestionPlayer.currentPath == pathName &&
            GameManager.Instance.currentQuestionPlayer.currentWaypointIndex == position)
        {
            return GameManager.Instance.currentQuestionPlayer;
        }

        // 3. Méthode de secours: détecter par collision
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