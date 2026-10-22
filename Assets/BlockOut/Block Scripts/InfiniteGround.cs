using System.Collections.Generic;
using UnityEngine;


public class InfiniteGround : MonoBehaviour
{

    public Transform player;
    public Transform mapParent;      // empty object holding your 9 planes
    public GameObject groundPrefab;  // a duplicate of one map plane
    public int viewRadius = 1;       // 1 = ground only appears near the map edge

    private float tileSize;
    private float scaleFactor = 1f;
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

        // Work out how much to stretch the prefab so it matches the map planes
        MeshFilter mf = groundPrefab.GetComponentInChildren<MeshFilter>();
        if (mf != null && mf.sharedMesh != null)
        {
            float prefabSize = mf.sharedMesh.bounds.size.x * groundPrefab.transform.lossyScale.x;
            if (prefabSize > 0.01f)
                scaleFactor = tileSize / prefabSize;
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

                GameObject tile = Instantiate(groundPrefab, pos, Quaternion.identity, transform);
                Vector3 s = groundPrefab.transform.localScale;
                tile.transform.localScale = new Vector3(s.x * scaleFactor, s.y, s.z * scaleFactor);
                spawned.Add(c, tile);
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