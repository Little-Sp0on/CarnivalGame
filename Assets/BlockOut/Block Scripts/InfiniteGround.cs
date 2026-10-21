using System.Collections.Generic;
using UnityEngine;


public class InfiniteGround : MonoBehaviour
{
    public Transform player;
    public GameObject groundPrefab;
    public float tileSize = 10f;   // must match your prefab's actual size
    public int viewRadius = 2;     // how many tiles around the player to keep

    private Dictionary<Vector2Int, GameObject> tiles = new Dictionary<Vector2Int, GameObject>();
    private Vector2Int lastPlayerTile = new Vector2Int(int.MaxValue, int.MaxValue);

    void Update()
    {
        Vector2Int currentTile = new Vector2Int(
            Mathf.RoundToInt(player.position.x / tileSize),
            Mathf.RoundToInt(player.position.z / tileSize)
        );

        // Only do work when the player enters a new tile
        if (currentTile == lastPlayerTile) return;
        lastPlayerTile = currentTile;

        // Spawn any missing tiles around the player
        for (int x = -viewRadius; x <= viewRadius; x++)
        {
            for (int z = -viewRadius; z <= viewRadius; z++)
            {
                Vector2Int coord = new Vector2Int(currentTile.x + x, currentTile.y + z);
                if (!tiles.ContainsKey(coord))
                {
                    Vector3 pos = new Vector3(coord.x * tileSize, 0f, coord.y * tileSize);
                    tiles.Add(coord, Instantiate(groundPrefab, pos, Quaternion.identity, transform));
                }
            }
        }

        // Remove tiles that are too far away (keeps memory/performance fine)
        List<Vector2Int> toRemove = new List<Vector2Int>();
        foreach (var kvp in tiles)
        {
            if (Mathf.Abs(kvp.Key.x - currentTile.x) > viewRadius + 1 ||
                Mathf.Abs(kvp.Key.y - currentTile.y) > viewRadius + 1)
            {
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var key in toRemove)
        {
            Destroy(tiles[key]);
            tiles.Remove(key);
        }
    }
}