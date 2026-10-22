using System.Collections.Generic;
using UnityEngine;


public class InfiniteGround : MonoBehaviour
{
    public Transform player;
    public Transform mapParent;      // the empty object holding planes
    public GameObject groundPrefab;  // one plane, same size as map planes
    public float tileSize = 3f;     // size of ONE plane in world units
    public int viewRadius = 1;       // how close to the edge before ground spawns (in tiles)

    private Vector3 origin;
    private List<Bounds> mapBounds = new List<Bounds>();
    private Dictionary<Vector2Int, GameObject> spawned = new Dictionary<Vector2Int, GameObject>();
    private Vector2Int lastCell = new Vector2Int(int.MaxValue, int.MaxValue);

    void Start()
    {
        if (mapParent == null || groundPrefab == null || player == null)
    {
        Debug.LogError("InfiniteGround: assign Player, Map Parent and Ground Prefab in the Inspector.");
        enabled = false;
        return;
    }

    Renderer[] renderers = mapParent.GetComponentsInChildren<Renderer>();
    if (renderers.Length == 0)
    {
        Debug.LogError("InfiniteGround: Map Parent has no planes (renderers) inside it.");
        enabled = false;
        return;
    }

    // Measure the real size of one plane
    tileSize = renderers[0].bounds.size.x;
    origin = renderers[0].bounds.center;

    if (tileSize < 0.01f)
    {
        Debug.LogError("InfiniteGround: detected tile size is 0. Check your planes have a visible mesh.");
        enabled = false;
        return;
    }

    foreach (Renderer r in renderers)
        mapBounds.Add(r.bounds);

    Debug.Log("Detected tile size: " + tileSize);
    }

    void Update()
    {
        Vector2Int cell = WorldToCell(player.position);
        if (cell == lastCell) return;
        lastCell = cell;

        for (int x = -viewRadius; x <= viewRadius; x++)
        {
            for (int z = -viewRadius; z <= viewRadius; z++)
            {
                Vector2Int c = new Vector2Int(cell.x + x, cell.y + z);
                if (spawned.ContainsKey(c)) continue;

                Vector3 pos = new Vector3(origin.x + c.x * tileSize, groundPrefab.transform.position.y, origin.z + c.y * tileSize);

                if (IsOnMap(pos)) continue; // never spawn on top of the base map

                spawned.Add(c, Instantiate(groundPrefab, pos, Quaternion.identity, transform));
            }
        }

        // Delete tiles far behind the player
        List<Vector2Int> remove = new List<Vector2Int>();
        foreach (var kvp in spawned)
        {
            if (Mathf.Abs(kvp.Key.x - cell.x) > viewRadius + 1 ||
                Mathf.Abs(kvp.Key.y - cell.y) > viewRadius + 1)
                remove.Add(kvp.Key);
        }
        foreach (var key in remove)
        {
            Destroy(spawned[key]);
            spawned.Remove(key);
        }
    }

    bool IsOnMap(Vector3 pos)
    {
        foreach (Bounds b in mapBounds)
        {
            if (pos.x > b.min.x && pos.x < b.max.x && pos.z > b.min.z && pos.z < b.max.z)
                return true;
        }
        return false;
    }

    Vector2Int WorldToCell(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.RoundToInt((worldPos.x - origin.x) / tileSize),
            Mathf.RoundToInt((worldPos.z - origin.z) / tileSize)
        );
    }
}