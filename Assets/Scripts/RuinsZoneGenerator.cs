using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Generates the ground for a rectangular ruins zone (for the "Exclusion
/// Zone" area): a stone/rubble ground tilemap, plus a single interactable
/// feature (e.g. a statue or altar) that the player can trigger with a
/// keyboard press.
///
/// Perimeter (walls/pillars/arches) and interior scatter (statues, debris)
/// are hand-placed directly in the Scene instead of auto-generated - the
/// generated art has real shading/lighting baked in, so manual placement
/// gives full control over composition without fighting rotation artifacts.
///
/// Modeled on GroundGenerator.cs for the tile-painting step: assign several
/// sprite variants, they're auto-converted to Tile assets, and the zone
/// paints into a Tilemap the same way the world's grass ground does.
/// </summary>
public class RuinsZoneGenerator : MonoBehaviour
{
    public enum Side { Left, Right, Top, Bottom }

    [Header("Zone Placement")]
    [Tooltip("World-space center of the ruins rectangle.")]
    public Vector2 zoneCenter = new Vector2(-30f, 20f);
    [Tooltip("Full width/height of the ruins rectangle, in world units.")]
    public Vector2 zoneSize = new Vector2(20f, 16f);

    [Header("Entrance (flat, no stairs)")]
    [Tooltip("Which edge of the rectangle has the entrance gap - purely a placement reference for you when hand-placing walls; the generator no longer builds a perimeter automatically. Ground stays flat through it either way - no elevation change, so no stairs are needed.")]
    public Side entranceSide = Side.Bottom;
    [Tooltip("Width of the entrance gap, in world units - shown as a gizmo line to guide manual wall placement.")]
    public float entranceWidth = 4f;

    [Header("Ground")]
    public Tilemap tilemap;
    [Tooltip("Cracked stone / rubble floor sprites. Auto-converted to Tile assets, same as GroundGenerator. Use several variants to avoid an obvious repeating pattern.")]
    public Sprite[] ruinsFloorSprites;
    public TileBase[] ruinsFloorTiles;

    [Header("Interactable Feature (the E-press trigger)")]
    [Tooltip("The centerpiece prefab for the interactable spot - typically a statue or altar. Placed once, at Feature Position.")]
    public GameObject interactableFeaturePrefab;
    [Tooltip("Local offset from Zone Center where the interactable feature is placed. Keep it a few units in from the entrance so the player naturally walks up to it.")]
    public Vector2 featureOffsetFromCenter = Vector2.zero;
    [Tooltip("Radius within which the player can press E to trigger the feature.")]
    public float featureInteractRadius = 1.5f;

    private Transform root;
    private Transform featureRoot;

    [ContextMenu("Generate Ruins")]
    public void Generate()
    {
        ClearGenerated();

        root = GetOrCreateRoot("RuinsZone");
        featureRoot = GetOrCreateRoot("RuinsFeature", root);

        PaintGround();
        PlaceInteractableFeature();
    }

    [ContextMenu("Clear Ruins")]
    public void ClearGenerated()
    {
        Transform existing = transform.Find("RuinsZone");
        if (existing != null)
        {
            if (Application.isPlaying) Destroy(existing.gameObject);
            else DestroyImmediate(existing.gameObject);
        }

        // The Tilemap may be shared with GroundGenerator's grass (so the
        // ruins floor sits under trees/stones using the same sorting setup).
        // ClearAllTiles() would wipe the entire shared tilemap, including all
        // grass - so only clear the cells inside this zone's own rectangle.
        ClearOwnFloorTiles();
    }

    private void ClearOwnFloorTiles()
    {
        if (tilemap == null) return;

        Vector3 min = new Vector3(zoneCenter.x - zoneSize.x * 0.5f, zoneCenter.y - zoneSize.y * 0.5f, 0f);
        Vector3 max = new Vector3(zoneCenter.x + zoneSize.x * 0.5f, zoneCenter.y + zoneSize.y * 0.5f, 0f);
        Vector3Int minCell = tilemap.WorldToCell(min);
        Vector3Int maxCell = tilemap.WorldToCell(max);

        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                tilemap.SetTile(new Vector3Int(x, y, 0), null);
            }
        }
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
            Debug.LogWarning("[RuinsZoneGenerator] No Tilemap assigned, skipping ground paint.");
            return;
        }

#if UNITY_EDITOR
        EnsureRuinsFloorTiles();
#endif
        if (ruinsFloorTiles == null || ruinsFloorTiles.Length == 0)
        {
            Debug.LogWarning("[RuinsZoneGenerator] No ruins floor tiles available. Assign Ruins Floor Sprites or Ruins Floor Tiles.");
            return;
        }

        Vector3 min = new Vector3(zoneCenter.x - zoneSize.x * 0.5f, zoneCenter.y - zoneSize.y * 0.5f, 0f);
        Vector3 max = new Vector3(zoneCenter.x + zoneSize.x * 0.5f, zoneCenter.y + zoneSize.y * 0.5f, 0f);
        // Use the Tilemap's own WorldToCell, not the parent Grid's - the
        // Tilemap GameObject can carry its own extra local offset (as it
        // does in this project), and only Tilemap.WorldToCell accounts for
        // that full transform chain. Going through the Grid alone silently
        // ignores that offset and paints a constant distance away from
        // where it visually renders.
        Vector3Int minCell = tilemap.WorldToCell(min);
        Vector3Int maxCell = tilemap.WorldToCell(max);

        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                TileBase tile = ruinsFloorTiles[Random.Range(0, ruinsFloorTiles.Length)];
                tilemap.SetTile(cellPos, tile);
            }
        }
    }

#if UNITY_EDITOR
    private void EnsureRuinsFloorTiles()
    {
        if (ruinsFloorSprites == null || ruinsFloorSprites.Length == 0) return;

        List<TileBase> tiles = new List<TileBase>(ruinsFloorTiles ?? new TileBase[0]);
        string dir = "Assets/Tiles";
        if (!UnityEditor.AssetDatabase.IsValidFolder(dir))
        {
            UnityEditor.AssetDatabase.CreateFolder("Assets", "Tiles");
        }

        bool created = false;
        foreach (Sprite sprite in ruinsFloorSprites)
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
            string path = UnityEditor.AssetDatabase.GenerateUniqueAssetPath($"{dir}/RuinsFloorTile_{sprite.name}.asset");
            UnityEditor.AssetDatabase.CreateAsset(newTile, path);
            tiles.Add(newTile);
            created = true;
        }

        if (created)
        {
            UnityEditor.AssetDatabase.SaveAssets();
            ruinsFloorTiles = tiles.ToArray();
        }
    }
#endif

    // ---------------- Interactable Feature ----------------

    private void PlaceInteractableFeature()
    {
        if (interactableFeaturePrefab == null)
        {
            Debug.LogWarning("[RuinsZoneGenerator] No Interactable Feature Prefab assigned - ruins will have no E-press trigger.");
            return;
        }

        Vector2 pos = zoneCenter + featureOffsetFromCenter;
        GameObject feature = Instantiate(interactableFeaturePrefab, pos, Quaternion.identity, featureRoot);
        feature.name = interactableFeaturePrefab.name + "_Interactable";

        // Give it a solid body (so the player can't stand on top of it)...
        if (feature.GetComponent<Collider2D>() == null)
        {
            SpriteRenderer sr = feature.GetComponent<SpriteRenderer>();
            BoxCollider2D solid = feature.AddComponent<BoxCollider2D>();
            if (sr != null && sr.sprite != null) solid.size = sr.sprite.bounds.size;
        }

        // ...and a separate, larger trigger collider the RuinsInteractable
        // uses to detect proximity, so "press E" works from just outside it.
        RuinsInteractable interactable = feature.GetComponent<RuinsInteractable>();
        if (interactable == null) interactable = feature.AddComponent<RuinsInteractable>();
        interactable.interactRadius = featureInteractRadius;

        Debug.Log($"[RuinsZoneGenerator] Placed interactable feature '{feature.name}' at {pos}.");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(zoneCenter, zoneSize);

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

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(zoneCenter + featureOffsetFromCenter, featureInteractRadius);
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(RuinsZoneGenerator))]
public class RuinsZoneGeneratorEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RuinsZoneGenerator generator = (RuinsZoneGenerator)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Generate Ruins"))
        {
            generator.Generate();
        }
        if (GUILayout.Button("Clear Ruins"))
        {
            generator.ClearGenerated();
        }
    }
}
#endif