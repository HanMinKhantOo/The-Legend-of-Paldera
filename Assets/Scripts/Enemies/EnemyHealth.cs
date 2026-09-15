using System;
using UnityEngine;

/// <summary>
/// Owns HP and death/drop behavior for a roaming enemy (Wolf, Boar, etc).
/// Mirrors the shape of PlayerVitals (HP + events) so it stays familiar, but
/// is enemy-specific: no hunger, and it drops loot and reports death to
/// EnemySpawner instead of showing a death screen.
///
/// Damage comes in from PlayerPunch.PerformPunchHit, exactly the way
/// TreeHealth.TakeHit does today - nothing about that existing flow changes.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EnemyHealth : MonoBehaviour
{
    [Header("HP")]
    public float maxHP = 30f;
    [SerializeField] private float currentHP;

    [Header("Loot: Enchanted Meat")]
    [Tooltip("World-pickup prefab dropped on death (a WorldPickup with itemType = Food). See EnchantedMeat setup in the integration guide.")]
    public GameObject lootPrefab;
    public int minDrops = 1;
    public int maxDrops = 2;
    [Tooltip("How far from the death position drops can scatter.")]
    public float dropRadius = 0.5f;

    public float CurrentHP => currentHP;
    public bool IsDead { get; private set; }

    /// <summary>Raised once, the moment HP reaches 0.</summary>
    public event Action<EnemyHealth> OnDeath;
    /// <summary>Raised whenever HP changes but the enemy survives (for hurt reactions).</summary>
    public event Action OnDamaged;

    private Collider2D col;
    private SpriteRenderer sr;

    private void Awake()
    {
        currentHP = maxHP;
        col = GetComponent<Collider2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    /// <summary>
    /// Called by PlayerPunch (or any future damage source) exactly like
    /// TreeHealth.TakeHit. Kept as "damage" (float) rather than "hits" so
    /// weapons with different power can matter later.
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        currentHP = Mathf.Max(0f, currentHP - amount);

        if (currentHP <= 0f)
        {
            Die();
        }
        else
        {
            OnDamaged?.Invoke();
        }
    }

    private void Die()
    {
        if (IsDead) return;
        IsDead = true;

        DropLoot();

        // Hide and disable rather than Destroy - EnemySpawner keeps this
        // instance around and reactivates/repositions it on respawn, the
        // same way ResourceNode hides+shows itself instead of respawning
        // a whole new object.
        if (sr != null) sr.enabled = false;
        if (col != null) col.enabled = false;

        OnDeath?.Invoke(this);
    }

    private void DropLoot()
    {
        if (lootPrefab == null) return;

        int amount = UnityEngine.Random.Range(minDrops, maxDrops + 1);

        for (int i = 0; i < amount; i++)
        {
            Vector2 offset = UnityEngine.Random.insideUnitCircle * dropRadius;
            Vector3 spawnPos = transform.position + new Vector3(offset.x, offset.y, 0f);
            Instantiate(lootPrefab, spawnPos, Quaternion.identity);
        }
    }

    /// <summary>Called by EnemySpawner when this enemy respawns.</summary>
    public void ResetHealth()
    {
        currentHP = maxHP;
        IsDead = false;
        if (sr != null) sr.enabled = true;
        if (col != null) col.enabled = true;
    }
}
