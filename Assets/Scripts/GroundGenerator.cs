using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

/// <summary>
/// Fills the map's walkable interior with a grass ground tile using Unity's
/// Tilemap system. This is the base floor layer everything else (border walls,
/// resources, later: paths/village/lake) sits on top of.
///
/// Usage:
/// 1. Add this component to an empty GameObject in your scene ("GroundGenerator").
/// 2. Assign SEVERAL grass Sprites from your NinjaAdventure tileset (e.g. slice
///    TilesetField.png / TilesetNature.png and pick 3-5 different plain grass
///    variants) into "Grass Sprites". Using multiple variants avoids an obvious
///    repeating grid pattern - a single repeated tile looks like a checkerboard.
///    - OR assign existing Tile assets directly into "Grass Tiles" if you've
///      already made some.
/// 3. Set Ground Width / Ground Height / Ground Center to match your
///    MapGenerator's Map Width / Map Height / Map Center so the ground and
///    border/scatter line up.
/// 4. Click "Paint Ground" in the Inspector.
///
/// The tile fills the FULL map bounds (border walls will render on top of
/// the edges, which is fine and expected).
/// </summary>
[RequireComponent(typeof(Grid))]
public class GroundGenerator : MonoBehaviour
{
    [Header("Ground Area")]
    [Tooltip("Should match MapGenerator's Map Width so ground and border/scatter line up.")]
    public float groundWidth = 100f;
    [Tooltip("Should match MapGenerator's Map Height.")]
    public float groundHeight = 100f;
    [Tooltip("Should match MapGenerator's Map Center.")]
    public Vector2 groundCenter = Vector2.zero;

    [Header("Tile Source")]
    [Tooltip("One or more grass sprites sliced from your tileset (e.g. TilesetField_0, _2, _4...). Using several variants instead of one avoids an obvious repeating grid pattern. Auto-creates Tile assets for any that don't have one yet in Grass Tiles below.")]
    public Sprite[] grassSprites;
    [Tooltip("The actual Tile assets painted onto the map, one per Grass Sprite above (auto-populated). You can also hand-assign existing Tile assets here directly and leave Grass Sprites empty.")]
    public TileBase[] grassTiles;
    [Tooltip("Same seed = same tile variant pattern every time you regenerate.")]
    public int seed = 12345;

    [Header("Sorting")]
    [Tooltip("Sorting layer name for the ground Tilemap Renderer. Should render behind resources/player - create a 'Ground' sorting layer below 'Default' in Project Settings > Tags and Layers if you haven't already.")]
    public string sortingLayerName = "Default";
    public int sortingOrder = -100;

    private Tilemap tilemap;
    private Grid grid;

    private void EnsureTilemap()
    {
        grid = GetComponent<Grid>();

        Transform existing = transform.Find("Ground");
        GameObject groundObj;
        if (existing != null)
        {
            groundObj = existing.gameObject;
        }
        else
        {
            groundObj = new GameObject("Ground");
            groundObj.transform.SetParent(transform);
            groundObj.transform.localPosition = Vector3.zero;
        }

        tilemap = groundObj.GetComponent<Tilemap>();
        if (tilemap == null) tilemap = groundObj.AddComponent<Tilemap>();

        TilemapRenderer renderer = groundObj.GetComponent<TilemapRenderer>();
        if (renderer == null) renderer = groundObj.AddComponent<TilemapRenderer>();
        renderer.sortingLayerName = sortingLayerName;
        renderer.sortingOrder = sortingOrder;
    }

#if UNITY_EDITOR
    private void EnsureGrassTiles()
    {
        if (grassSprites == null || grassSprites.Length == 0)
        {
            return; // grassTiles may already be hand-assigned directly.
        }

        System.Collections.Generic.List<TileBase> tiles =
            new System.Collections.Generic.List<TileBase>(grassTiles ?? new TileBase[0]);

        string dir = "Assets/Tiles";
        if (!AssetDatabase.IsValidFolder(dir))
        {
            AssetDatabase.CreateFolder("Assets", "Tiles");
        }

        bool created = false;
        foreach (Sprite sprite in grassSprites)
        {
            if (sprite == null) continue;

            // Skip if we already have a tile using this exact sprite.
            bool alreadyExists = false;
            foreach (TileBase existing in tiles)
            {
                if (existing is Tile t && t.sprite == sprite) { alreadyExists = true; break; }
            }
            if (alreadyExists) continue;

            Tile newTile = ScriptableObject.CreateInstance<Tile>();
            newTile.sprite = sprite;

            string path = AssetDatabase.GenerateUniqueAssetPath($"{dir}/GrassTile_{sprite.name}.asset");
            AssetDatabase.CreateAsset(newTile, path);
            tiles.Add(newTile);
            created = true;
        }

        if (created)
        {
            AssetDatabase.SaveAssets();
            grassTiles = tiles.ToArray();
            Debug.Log($"[GroundGenerator] Grass tile assets ready ({grassTiles.Length} variants) in {dir}/");
        }
    }
#endif

    public void PaintGround()
    {
        EnsureTilemap();
#if UNITY_EDITOR
        EnsureGrassTiles();
#endif
        if (grassTiles == null || grassTiles.Length == 0)
        {
            Debug.LogWarning("[GroundGenerator] No tiles available to paint. Assign Grass Sprites (or Grass Tiles directly).");
            return;
        }

        Random.InitState(seed);

        Vector3 cellSizeWorld = grid.cellSize;
        if (cellSizeWorld.x <= 0f) cellSizeWorld.x = 1f;
        if (cellSizeWorld.y <= 0f) cellSizeWorld.y = 1f;

        int cellsX = Mathf.CeilToInt(groundWidth / cellSizeWorld.x);
        int cellsY = Mathf.CeilToInt(groundHeight / cellSizeWorld.y);

        Vector3Int centerCell = grid.WorldToCell(new Vector3(groundCenter.x, groundCenter.y, 0f));

        int halfX = cellsX / 2;
        int halfY = cellsY / 2;

        int painted = 0;
        for (int x = -halfX; x <= halfX; x++)
        {
            for (int y = -halfY; y <= halfY; y++)
            {
                Vector3Int cellPos = new Vector3Int(centerCell.x + x, centerCell.y + y, 0);
                TileBase tile = grassTiles[Random.Range(0, grassTiles.Length)];
                tilemap.SetTile(cellPos, tile);
                painted++;
            }
        }

        Debug.Log($"[GroundGenerator] Painted {painted} ground tiles using {grassTiles.Length} grass variants.");
    }

    public void ClearGround()
    {
        EnsureTilemap();
        tilemap.ClearAllTiles();
        Debug.Log("[GroundGenerator] Ground cleared.");
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(GroundGenerator))]
public class GroundGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GroundGenerator generator = (GroundGenerator)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Paint Ground"))
        {
            generator.PaintGround();
        }
        if (GUILayout.Button("Clear Ground"))
        {
            generator.ClearGround();
        }
    }
}
#endif