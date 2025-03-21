using UnityEngine;

public class CardTile : Tile
{
    public string cardEffect; //  Effet de la carte (ex: avancer, reculer...)

    public override void OnPlayerLands()
    {
        Debug.Log($"Player landed on a CARD tile in {region} at position {position}. Effect: {cardEffect}");
        // Ici tu peux déclencher l'effet de la carte
    }
}
