using UnityEngine;

/// <summary>
/// Attach to any resource object (tree, stone, boulder) placed by MapGenerator.
/// Makes it interactable from the start, so we don't have to retrofit
/// placed decoration later when the collect/chop/mine system is built.
/// </summary>
public class ResourceNode : MonoBehaviour
{
    public enum ResourceType { Tree, Stone, Boulder, Branch }

    [Header("Identity")]
    public ResourceType resourceType;

    [Header("Health / Hits")]
    [Tooltip("How many hits (chops/mines) before this node is depleted.")]
    public int maxHits = 3;
    private int currentHits;

    [Header("Drops")]
    [Tooltip("Resource amount granted per hit. Wire this up to your inventory system later.")]
    public int amountPerHit = 1;

    [Header("Respawn")]
    public bool respawns = true;
    [Tooltip("Seconds before this node reappears after being depleted.")]
    public float respawnTime = 60f;

    private SpriteRenderer sr;
    private Collider2D col;

    private void Awake()
    {
        currentHits = maxHits;
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    /// <summary>
    /// Call this from the player's interaction system (hatchet/pickaxe swing, etc).
    /// Returns the amount of resource granted by this hit, or 0 if already depleted.
    /// </summary>
    public int Hit()
    {
        if (currentHits <= 0) return 0;

        currentHits--;

        if (currentHits <= 0)
        {
            Deplete();
        }

        return amountPerHit;
    }

    private void Deplete()
    {
        if (respawns)
        {
            SetVisible(false);
            Invoke(nameof(Respawn), respawnTime);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Respawn()
    {
        currentHits = maxHits;
        SetVisible(true);
    }

    private void SetVisible(bool visible)
    {
        if (sr != null) sr.enabled = visible;
        if (col != null) col.enabled = visible;
    }
}
