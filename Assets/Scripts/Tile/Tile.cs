using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }
    public enum QuestionType
    {
        Input,
        Qcm
    }
    public enum TileType { Question, Card, Intersection }
    public enum Region { Vulkan, Atlanta, Celestyel, Berg , None }

    public int position;
    public TileType type;
    public Region region;


    public virtual void OnPlayerLands()
    {
        Debug.Log($"🎯 Le joueur a atterri sur une tuile {type} dans la région {region} à la position {position}.");
    }
}
