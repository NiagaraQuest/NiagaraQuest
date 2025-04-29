using UnityEngine;

// Optional extension class to hook into Player life changes
// This is an alternative to using PlayerMonitor if you prefer
public static class PlayerExtensions
{
    // Extended LoseLife method that updates UI
    public static void LoseLifeWithUI(this Player player)
    {
        int previousLives = player.lives;

        // Call the original method
        player.LoseLife();

        // Notify UI about the life change
        NotifyLifeChange(player.gameObject, previousLives, player.lives);
    }

    // Extended GainLife method that updates UI
    public static void GainLifeWithUI(this Player player)
    {
        int previousLives = player.lives;

        // Call the original method
        player.GainLife();

        // Notify UI about the life change
        NotifyLifeChange(player.gameObject, previousLives, player.lives);
    }

    // Helper method to find and notify the HUD manager
    private static void NotifyLifeChange(GameObject playerObj, int oldValue, int newValue)
    {
        PlayerHUDManager hudManager = Object.FindObjectOfType<PlayerHUDManager>();
        if (hudManager != null)
        {
            hudManager.OnPlayerLifeChanged(playerObj, oldValue, newValue);
        }
    }
}