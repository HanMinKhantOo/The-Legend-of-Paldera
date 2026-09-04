using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Generates a single rectangular quarry/mine zone: stone ground tiles,
/// a rocky border around the perimeter with a gap left open as the
/// entrance, and scattered iron ore / stone boulder resource nodes
/// inside. Designed to sit in one of MapGenerator's reserved empty
/// rectangles (e.g. the top-right one).
/// </summary>
public class MineZoneGenerator : MonoBehaviour
{
    public enum Side { Left, Right, Top, Bottom }

    [Header("Zone Placement")]
    [Tooltip("World-space center of the quarry rectangle.")]
    public Vector2 zoneCenter = new Vector2(20f, 20f);
    [Tooltip("Full width/height of the quarry rectangle, in world units.")]
    public Vector2 zoneSize = new Vector2(16f, 16f);

    [Header("Entrance")]
    [Tooltip("Which edge of the rectangle has the entrance gap (no border rocks placed there).")]
    public Side entranceSide = Side.Left;
    [Tooltip("Width of the entrance gap, in world units.")]
    public float entranceWidth = 4f;

    [Header("Ground")]
    public Tilemap tilemap;
    [Tooltip("Stone ground sprites (auto-converted to Tiles). Leave empty and hand-assign Stone Tiles directly instead if you prefer.")]
    public Sprite[] stoneSprites;
    public TileBase[] stoneTiles;

    [Header("Border")]
    [Tooltip("Rocky border prefabs (wide/narrow variants). Placed edge-to-edge around the perimeter, skipping the entrance gap.")]
    public GameObject[] borderPrefabs;
    [Tooltip("Approximate world-space footprint width of one border piece, used to space them along the edge.")]
    public float borderPieceSpacing = 1.5f;
    [Tooltip("If true, border pieces get a BoxCollider2D so the player can only enter through the gap. If false, they're purely visual.")]
    public bool blockPlayerAtBorder = true;

    [Header("Resource Nodes")]
    public GameObject ironOrePrefab;
    public GameObject stoneBoulderPrefab;
    [Tooltip("How many nodes to attempt placing inside the zone (some may be skipped if space runs out).")]
    public int totalNodeAttempts = 40;
    [Range(0f, 1f)]
    [Tooltip("Chance each successfully placed node is iron ore instead of plain stone.")]
    public float ironChance = 0.45f;
    [Tooltip("Minimum distance kept between any two nodes, so they never overlap.")]
    public float minNodeSpacing = 2f;
    [Tooltip("Empty margin kept clear of nodes near the border/entrance, in world units.")]
    public float nodeMarginFromEdge = 1.5f;

    private Transform root;
    private Transform groundRoot;
    private Transform borderRoot;
    private Transform nodeRoot;
    private readonly List<Vector2> placedNodePositions = new List<Vector2>();

    [ContextMenu("Generate Quarry")]
    public void Generate()
    {
        ClearGenerated();

        root = GetOrCreateRoot("QuarryZone");
        groundRoot = GetOrCreateRoot("QuarryGround", root);
        borderRoot = GetOrCreateRoot("QuarryBorder", root);
        nodeRoot = GetOrCreateRoot("QuarryNodes", root);

        PaintGround();
        PlaceBorder();
        PlaceNodes();
    }

    [ContextMenu("Clear Quarry")]
    public void ClearGenerated()
    {
        Transform existing = transform.Find("QuarryZone");
        if (existing != null)
        {
            if (Application.isPlaying) Destroy(existing.gameObject);
            else DestroyImmediate(existing.gameObject);
        }
        placedNodePositions.Clear();
    }

    private Transform GetOrCreateRoot(string name, Transform parent = null)
    {
        Transform p = parent != null ? parent : transform;
        Transform found = p.Find(name);
        if (found != null) return found;

        GameObject go = new GameObject(name);
        go.transform.SetParent(p, false);
        return go.transform;
    }

    // ---------------- Ground ----------------

    private void PaintGround()
    {
        if (tilemap == null)
        {
            Debug.LogWarning("[MineZoneGenerator] No Tilemap assigned, skipping ground paint.");
            return;
        }

#if UNITY_EDITOR
        EnsureStoneTiles();
#endif
        if (stoneTiles == null || stoneTiles.Length == 0)
        {
            Debug.LogWarning("[MineZoneGenerator] No stone tiles available. Assign Stone Sprites or Stone Tiles.");
            return;
        }

        Grid grid = tilemap.layoutGrid;
        Vector3 cellSize = grid.cellSize;
        if (cellSize.x <= 0f) cellSize.x = 1f;
        if (cellSize.y <= 0f) cellSize.y = 1f;

        int halfX = Mathf.CeilToInt((zoneSize.x * 0.5f) / cellSize.x);
        int halfY = Mathf.CeilToInt((zoneSize.y * 0.5f) / cellSize.y);
        Vector3Int centerCell = grid.WorldToCell(new Vector3(zoneCenter.x, zoneCenter.y, 0f));

        for (int x = -halfX; x <= halfX; x++)
        {
            for (int y = -halfY; y <= halfY; y++)
            {
                Vector3Int cellPos = new Vector3Int(centerCell.x + x, centerCell.y + y, 0);
                TileBase tile = stoneTiles[Random.Range(0, stoneTiles.Length)];
                tilemap.SetTile(cellPos, tile);
            }
        }
    }

#if UNITY_EDITOR
    private void EnsureStoneTiles()
    {
        if (stoneSprites == null || stoneSprites.Length == 0) return;

        List<TileBase> tiles = new List<TileBase>(stoneTiles ?? new TileBase[0]);
        string dir = "Assets/Tiles";
        if (!UnityEditor.AssetDatabase.IsValidFolder(dir))
        {
            UnityEditor.AssetDatabase.CreateFolder("Assets", "Tiles");
        }

        bool created = false;
        foreach (Sprite sprite in stoneSprites)
        {
            if (sprite == null) continue;

            bool exists = false;
            foreach (TileBase existing in tiles)
            {
                if (existing is Tile t && t.sprite == sprite) { exists = true; break; }
            }
            if (exists) continue;

            Tile newTile = ScriptableObject.CreateInstance<Tile>();
            newTile.sprite = sprite;
            string path = UnityEditor.AssetDatabase.GenerateUniqueAssetPath($"{dir}/QuarryStoneTile_{sprite.name}.asset");
            UnityEditor.AssetDatabase.CreateAsset(newTile, path);
            tiles.Add(newTile);
            created = true;
        }

        if (created)
        {
            UnityEditor.AssetDatabase.SaveAssets();
            stoneTiles = tiles.ToArray();
        }
    }
#endif

    // ---------------- Border ----------------

    private void PlaceBorder()
    {
        if (borderPrefabs == null || borderPrefabs.Length == 0)
        {
            Debug.LogWarning("[MineZoneGenerator] No border prefabs assigned, skipping border.");
            return;
        }

        float halfW = zoneSize.x * 0.5f;
        float halfH = zoneSize.y * 0.5f;

        // Each edge is a line of points; skip points that fall inside the
        // entrance gap on whichever edge was chosen.
        PlaceEdge(new Vector2(zoneCenter.x - halfW, zoneCenter.y), zoneSize.y, true, Side.Left);
        PlaceEdge(new Vector2(zoneCenter.x + halfW, zoneCenter.y), zoneSize.y, true, Side.Right);
        PlaceEdge(new Vector2(zoneCenter.x, zoneCenter.y + halfH), zoneSize.x, false, Side.Top);
        PlaceEdge(new Vector2(zoneCenter.x, zoneCenter.y - halfH), zoneSize.x, false, Side.Bottom);
    }

    private void PlaceEdge(Vector2 edgeCenter, float edgeLength, bool vertical, Side side)
    {
        int count = Mathf.Max(1, Mathf.RoundToInt(edgeLength / borderPieceSpacing));
        float start = -edgeLength * 0.5f;

        for (int i = 0; i <= count; i++)
        {
            float t = start + (edgeLength * i / count);
            Vector2 pos = vertical
                ? new Vector2(edgeCenter.x, edgeCenter.y + t)
                : new Vector2(edgeCenter.x + t, edgeCenter.y);

            // Skip the entrance gap on the chosen side.
            if (side == entranceSide && Mathf.Abs(t) < entranceWidth * 0.5f)
            {
                continue;
            }

            GameObject prefab = borderPrefabs[Random.Range(0, borderPrefabs.Length)];
            GameObject piece = Instantiate(prefab, pos, Quaternion.identity, borderRoot);
            piece.name = prefab.name;

            if (blockPlayerAtBorder && piece.GetComponent<Collider2D>() == null)
            {
                BoxCollider2D col = piece.AddComponent<BoxCollider2D>();
                SpriteRenderer sr = piece.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                {
                    col.size = sr.sprite.bounds.size;
                }
            }
        }
    }

    // ---------------- Resource Nodes ----------------

    private void PlaceNodes()
    {
        if (ironOrePrefab == null && stoneBoulderPrefab == null)
        {
            Debug.LogWarning("[MineZoneGenerator] No node prefabs assigned, skipping nodes.");
            return;
        }

        placedNodePositions.Clear();
        float halfW = zoneSize.x * 0.5f - nodeMarginFromEdge;
        float halfH = zoneSize.y * 0.5f - nodeMarginFromEdge;

        int placed = 0;
        int attempts = 0;
        int maxAttempts = totalNodeAttempts * 6; // generous retry budget

        while (placed < totalNodeAttempts && attempts < maxAttempts)
        {
            attempts++;

            Vector2 candidate = new Vector2(
                zoneCenter.x + Random.Range(-halfW, halfW),
                zoneCenter.y + Random.Range(-halfH, halfH)
            );

            bool tooClose = false;
            foreach (Vector2 existing in placedNodePositions)
            {
                if (Vector2.Distance(candidate, existing) < minNodeSpacing)
                {
                    tooClose = true;
                    break;
                }
            }
            if (tooClose) continue;

            bool useIron = ironOrePrefab != null && (stoneBoulderPrefab == null || Random.value < ironChance);
            GameObject prefab = useIron ? ironOrePrefab : stoneBoulderPrefab;
            if (prefab == null) continue;

            GameObject node = Instantiate(prefab, candidate, Quaternion.identity, nodeRoot);
            node.name = prefab.name;

            ResourceNode resNode = node.GetComponent<ResourceNode>();
            if (resNode == null) resNode = node.AddComponent<ResourceNode>();
            resNode.resourceType = useIron ? ResourceNode.ResourceType.Iron : ResourceNode.ResourceType.Stone;

            placedNodePositions.Add(candidate);
            placed++;
        }

        Debug.Log($"[MineZoneGenerator] Placed {placed} resource nodes in quarry zone ({attempts} attempts).");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(zoneCenter, zoneSize);

        // Highlight entrance gap.
        Gizmos.color = Color.yellow;
        float halfW = zoneSize.x * 0.5f;
        float halfH = zoneSize.y * 0.5f;
        Vector3 a, b;
        switch (entranceSide)
        {
            case Side.Left:
                a = new Vector3(zoneCenter.x - halfW, zoneCenter.y - entranceWidth * 0.5f);
                b = new Vector3(zoneCenter.x - halfW, zoneCenter.y + entranceWidth * 0.5f);
                break;
            case Side.Right:
                a = new Vector3(zoneCenter.x + halfW, zoneCenter.y - entranceWidth * 0.5f);
                b = new Vector3(zoneCenter.x + halfW, zoneCenter.y + entranceWidth * 0.5f);
                break;
            case Side.Top:
                a = new Vector3(zoneCenter.x - entranceWidth * 0.5f, zoneCenter.y + halfH);
                b = new Vector3(zoneCenter.x + entranceWidth * 0.5f, zoneCenter.y + halfH);
                break;
            default: // Bottom
                a = new Vector3(zoneCenter.x - entranceWidth * 0.5f, zoneCenter.y - halfH);
                b = new Vector3(zoneCenter.x + entranceWidth * 0.5f, zoneCenter.y - halfH);
                break;
        }
        Gizmos.DrawLine(a, b);
    }
}