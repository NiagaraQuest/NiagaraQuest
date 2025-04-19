using UnityEngine;
public class HydroPlayer : Player
{
    [Header(" Hydro Settings")]
    public bool SkipQuestion = false;
    public bool hasUsedSkipInCurrentRegion = false;
    private Tile.Region lastRegion = Tile.Region.None;

    protected override void Start()
    {
        currentPath = "HydroPath";
        base.Start();

        if (playerProfile == null)
        {
            Debug.LogError($"{gameObject.name} → ❌ Pas de profil assigné !");
        }
        else
        {
            Debug.Log($" {gameObject.name} → Profil: {playerProfile.Username}");
        }

        // Activer l'abilité dès le départ
        SkipQuestion = true;
        hasUsedSkipInCurrentRegion = false;

        Debug.Log($" Départ ! Le joueur a l'abilité de skipper une question ");
    }

    public void InitializeSkip()
    {
        SkipQuestion = true;
        hasUsedSkipInCurrentRegion = false;

        Debug.Log($" Initialisation spéciale! Propriété SKIP ACTIVÉE ! ");
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.S))
        {
            UseSkipAbility();
            Debug.Log("TEST: Touche S pressée - tentative d'utilisation de la capacité skip");
        }

        if (!isMoving && HasFinishedMoving) // WHEN le joueur termine son mouvement
        {
            GameObject waypoint = GetCurrentWaypoint();
            if (waypoint != null)
            {
                Tile tile = waypoint.GetComponent<Tile>();
                if (tile != null)
                {
                    HandleSecondchance(tile);
                }
            }
        }
        // Ajoutez ceci dans Update() pour un débogage plus détaillé
        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log($"DEBUG - État actuel: SkipQuestion={SkipQuestion}, " +
                      $"hasUsedSkipInCurrentRegion={hasUsedSkipInCurrentRegion}, " +
                      $"lastRegion={lastRegion}");
        }
    }

    private void HandleSecondchance(Tile tile)
    {
        // Si on est sur une intersection, on ne fait rien
        if (tile.region == Tile.Region.None)
        {
            return; // Ignorer les intersections
        }

        // Si le joueur entre dans une nouvelle région (qui n'est pas None)
        if (lastRegion != tile.region && lastRegion != Tile.Region.None)
        {
            // Réinitialiser le flag seulement si on change de véritable région
            hasUsedSkipInCurrentRegion = false;
            Debug.Log($"Changement de région: {tile.region}. hasUsedSkipInCurrentRegion réinitialisé à false");
        }

        // Mettre à jour la dernière région non-None
        if (tile.region != Tile.Region.None)
        {
            lastRegion = tile.region;
        }

        if (tile.region == Tile.Region.Atlanta)
        {
            if (!hasUsedSkipInCurrentRegion)
            {
                ActivateChance();
            }
            else
            {
                Debug.Log($"Second chance déjà utilisée dans cette région! Attendez de sortir puis revenir.");
            }
        }
        else
        {
            if (SkipQuestion)
            {
                DeactivateChance();
            }
            else
            {
                Debug.Log($" Second chance DÉJÀ DÉSACTIVÉE ! ");
            }
        }
    }

    // Méthode pour utiliser effectivement la capacité (à appeler quand le joueur l'utilise)
    // Méthode pour utiliser effectivement la capacité (à appeler quand le joueur l'utilise)
    public void UseSkipAbility()
    {
        if (SkipQuestion)
        {
            if (!hasUsedSkipInCurrentRegion)
            {
                hasUsedSkipInCurrentRegion = true;
                Debug.Log($"Capacité de skip utilisée! Ne sera pas disponible jusqu'au prochain passage dans la région.");

                QuestionUIManager uiManager = FindObjectOfType<QuestionUIManager>();
                if (uiManager != null && uiManager.isProcessingQuestion)
                {
                    uiManager.SkipQuestion();
                }
            }
            else
            {
                Debug.LogError($"ERREUR: Capacité de skip déjà utilisée dans cette région! Changez de région pour la réactiver.");
            }
        }
        else
        {
            Debug.LogError($"ERREUR: Impossible d'utiliser la capacité de skip: elle n'est pas activée!");
        }
    }

    private void ActivateChance()
    {
        SkipQuestion = true;

        Debug.Log($"Second chance ACTIVÉE ! ");
    }

    private void DeactivateChance()
    {
        SkipQuestion = false;

        Debug.Log($"Second chance DÉSACTIVÉE ! ");
    }
}