using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum TileType{
        Question;
        Card;
        Intersection;
    }

    public enum Region{
        Vulkan;
        Atlanta;
        Celestyel;
        Berg;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        public int position;
        public TileType type;
        public Region region;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
