using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Procedurally generates the map's rocky border and scatters resource
/// nodes (trees, stones, boulders) across a chosen area.
///
/// Usage:
/// 1. Add this component to an empty GameObject in your scene ("MapGenerator").
/// 2. Assign your prefabs (border rock, tree, stone, boulder) in the Inspector.
///    Any placeholder GameObject with a SpriteRenderer works for testing.
/// 3. Set mapWidth/mapHeight to match your map size (in world units).
/// 4. Right-click the component header (or use the buttons below) to Generate.
///
/// Placed objects are parented under organized containers ("Border", "Trees",
/// "Stones", "Boulders") so you can inspect, clear, or hand-tweak results.
/// </summary>
public class MapGenerator : MonoBehaviour
{
    [Header("Map Bounds")]
    [Tooltip("Total map width in world units.")]
    public float mapWidth = 100f;
    [Tooltip("Total map height in world units.")]
    public float mapHeight = 100f;
    [Tooltip("Center of the map in world space (e.g. the Yggdrasil tree spawn point).")]
    public Vector2 mapCenter = Vector2.zero;

    [Header("Border (Walls / Mountain Ring)")]
    [Tooltip("Assign your Object_Walls prefab here - this forms the impassable border ring around the map edges.")]
    public GameObject borderRockPrefab;
    [Tooltip("If enabled, tile spacing is calculated from the prefab's actual sprite size. Overrides Border Spacing below.")]
    public bool autoFitBorderSpacing = true;
    [Tooltip("How many world units between each border tile. Only used if Auto Fit Border Spacing is off.")]
    public float borderSpacing = 2f;
    [Tooltip("How many rings deep the wall is. 1 = a single outer line (recommended for a clean border). Higher values stack rings inward for a thicker wall.")]
    public int borderThickness = 1;
    [Range(0.3f, 1f)]
    [Tooltip("Multiplies tile spacing (both along the ring and between rings) so tiles overlap slightly for a solid, realistic look instead of visible gaps. 1 = tiles just touch, no overlap. Lower = more overlap.")]
    public float borderOverlapFactor = 0.85f;
    [Tooltip("Randomize border tile position slightly. Keep this small/zero for tiled wall sprites - large jitter causes visible gaps or messy overlap.")]
    public float borderJitter = 0f;

    [Header("Scatter - Trees")]
    public GameObject[] treePrefabs;
    [Range(0f, 1f)] public float treeDensity = 0.15f;

    [Header("Scatter - Branches")]
    [Tooltip("Fallen wood branches - early, easy-to-collect resource per GDD.")]
    public GameObject[] branchPrefabs;
    [Range(0f, 1f)] public float branchDensity = 0.1f;

    [Header("Scatter - Stones")]
    public GameObject[] stonePrefabs;
    [Range(0f, 1f)] public float stoneDensity = 0.05f;

    [Header("Scatter - Boulders")]
    [Tooltip("Farmable boulder resource, evenly distributed across the map like trees/stones/branches.")]
    public GameObject[] boulderPrefabs;
    [Range(0f, 1f)] public float boulderDensity = 0.05f;

    [Header("Scatter Grid")]
    [Tooltip("Cell size used when rolling scatter density. Smaller = denser possible placement.")]
    public float scatterCellSize = 3f;
    [Tooltip("Empty gap kept between the scatter area and the border/wall ring, so resources never spawn inside or touching the walls.")]
    public float scatterMarginFromWalls = 4f;

    [Header("Randomization")]
    public int seed = 12345;
    [Tooltip("Objects placed within this radius of the spawn point are skipped, to keep the starting area clear.")]
    public float spawnClearRadius = 8f;

    // ---- Internal ----
    private Transform borderRoot;
    private Transform treeRoot;
    private Transform branchRoot;
    private Transform stoneRoot;
    private Transform boulderRoot;

    public void ClearGenerated()
    {
        ClearChild("Border");
        ClearChild("Trees");
        ClearChild("Branches");
        ClearChild("Stones");
        ClearChild("Boulders");
    }

    private void ClearChild(string name)
    {
        Transform t = transform.Find(name);
        if (t == null) return;
#if UNITY_EDITOR
        DestroyImmediate(t.gameObject);
#else
        Destroy(t.gameObject);
#endif
    }

    private Transform GetOrCreateRoot(string name)
    {
        Transform t = transform.Find(name);
        if (t == null)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform);
            t = go.transform;
        }
        return t;
    }

    public void Generate()
    {
        Random.InitState(seed);

        ClearGenerated();

        borderRoot = GetOrCreateRoot("Border");
        treeRoot = GetOrCreateRoot("Trees");
        branchRoot = GetOrCreateRoot("Branches");
        stoneRoot = GetOrCreateRoot("Stones");
        boulderRoot = GetOrCreateRoot("Boulders");

        GenerateBorder();

        // Total physical depth the wall ring actually occupies (accounting for
        // overlap), so scatter can clear it without eating the whole map.
        float actualBorderDepth = GetEffectiveBorderSpacing() * borderOverlapFactor * borderThickness;
        float totalMargin = actualBorderDepth + scatterMarginFromWalls;

        GenerateScatter(treePrefabs, treeDensity, treeRoot, ResourceNode.ResourceType.Tree, null, totalMargin);
        GenerateScatter(branchPrefabs, branchDensity, branchRoot, ResourceNode.ResourceType.Branch, null, totalMargin);
        GenerateScatter(stonePrefabs, stoneDensity, stoneRoot, ResourceNode.ResourceType.Stone, null, totalMargin);
        GenerateScatter(boulderPrefabs, boulderDensity, boulderRoot, ResourceNode.ResourceType.Boulder, null, totalMargin);

        Debug.Log($"[MapGenerator] Generation complete. Border spacing used: {GetEffectiveBorderSpacing():F2}, scatter margin: {totalMargin:F2}");
    }

    private float GetEffectiveBorderSpacing()
    {
        if (!autoFitBorderSpacing || borderRockPrefab == null) return borderSpacing;

        SpriteRenderer sr = borderRockPrefab.GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null)
        {
            Debug.LogWarning("[MapGenerator] Auto Fit Border Spacing is on but the prefab has no Sprite Renderer / sprite assigned. Falling back to manual Border Spacing.");
            return borderSpacing;
        }

        // Sprite bounds already account for the prefab's own PPU and local scale.
        Vector2 size = sr.sprite.bounds.size;
        float worldSize = Mathf.Max(size.x, size.y) * Mathf.Max(borderRockPrefab.transform.localScale.x, borderRockPrefab.transform.localScale.y);
        return worldSize > 0.01f ? worldSize : borderSpacing;
    }

    private void GenerateBorder()
    {
        if (borderRockPrefab == null)
        {
            Debug.LogWarning("[MapGenerator] No border rock prefab assigned - skipping border.");
            return;
        }

        // Effective spacing = raw sprite size * overlap factor, so tiles
        // overlap slightly instead of leaving gaps or sitting far apart.
        float rawTileSize = GetEffectiveBorderSpacing();
        float spacing = rawTileSize * borderOverlapFactor;

        float halfW = mapWidth / 2f;
        float halfH = mapHeight / 2f;

        for (int ring = 0; ring < borderThickness; ring++)
        {
            float w = halfW + ring * spacing;
            float h = halfH + ring * spacing;

            // top and bottom edges
            for (float x = -w; x <= w; x += spacing)
            {
                PlaceBorderRock(new Vector2(x, h));
                PlaceBorderRock(new Vector2(x, -h));
            }

            // left and right edges
            for (float y = -h; y <= h; y += spacing)
            {
                PlaceBorderRock(new Vector2(-w, y));
                PlaceBorderRock(new Vector2(w, y));
            }
        }
    }

    private void PlaceBorderRock(Vector2 localPos)
    {
        Vector2 jitter = new Vector2(
            Random.Range(-borderJitter, borderJitter),
            Random.Range(-borderJitter, borderJitter)
        );
        Vector3 worldPos = mapCenter + localPos + jitter;

        GameObject obj = InstantiatePrefab(borderRockPrefab, worldPos, borderRoot);
        if (obj != null) obj.name = "BorderWall";
    }

    private void GenerateScatter(GameObject[] prefabs, float density, Transform root,
                                  ResourceNode.ResourceType type, Rect? confineTo, float marginFromWalls)
    {
        if (prefabs == null || prefabs.Length == 0 || density <= 0f) return;

        float halfW = mapWidth / 2f;
        float halfH = mapHeight / 2f;

        // Keep resources clear of the border/wall ring entirely.
        float insetW = Mathf.Max(0f, halfW - marginFromWalls);
        float insetH = Mathf.Max(0f, halfH - marginFromWalls);

        Rect area = confineTo ?? new Rect(mapCenter.x - insetW, mapCenter.y - insetH, insetW * 2f, insetH * 2f);

        for (float x = area.xMin; x < area.xMax; x += scatterCellSize)
        {
            for (float y = area.yMin; y < area.yMax; y += scatterCellSize)
            {
                if (Random.value > density) continue;

                Vector2 jitteredPos = new Vector2(
                    x + Random.Range(0f, scatterCellSize),
                    y + Random.Range(0f, scatterCellSize)
                );

                if (Vector2.Distance(jitteredPos, mapCenter) < spawnClearRadius) continue;

                GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
                GameObject obj = InstantiatePrefab(prefab, jitteredPos, root);
                if (obj == null) continue;

                ResourceNode node = obj.GetComponent<ResourceNode>();
                if (node == null) node = obj.AddComponent<ResourceNode>();
                node.resourceType = type;
            }
        }
    }

    private GameObject InstantiatePrefab(GameObject prefab, Vector3 position, Transform parent)
    {
        if (prefab == null) return null;

#if UNITY_EDITOR
        GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        obj.transform.position = position;
        return obj;
#else
        return Instantiate(prefab, position, Quaternion.identity, parent);
#endif
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MapGenerator generator = (MapGenerator)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Generate Map"))
        {
            generator.Generate();
        }
        if (GUILayout.Button("Clear Generated Objects"))
        {
            generator.ClearGenerated();
        }
    }
}
#endif