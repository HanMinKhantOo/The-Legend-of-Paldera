using System;
using System.Collections;
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

    [Header("Death")]
    [Tooltip("Seconds to keep the sprite visible after death before hiding it, so a Death animation actually has time to play. Set higher for enemies with a longer death clip (like a boss), lower/0 for enemies whose death frames already fade to invisible on their own (like the Wolf).")]
    public float deathHideDelay = 1f;

    public float CurrentHP => currentHP;
    public bool IsDead { get; private set; }

    /// <summary>Raised once, the moment HP reaches 0.</summary>
    public event Action<EnemyHealth> OnDeath;
    /// <summary>Raised whenever HP changes but the enemy survives (for hurt reactions).</summary>
    public event Action OnDamaged;
    /// <summary>
    /// Scene-wide static event fired whenever ANY enemy dies, passing its
    /// display name (EnemyAI.mobName, e.g. "Magical Wolf"). Used by
    /// QuestGiver for KillEnemy-type quests - kept as a static event rather
    /// than per-instance so a QuestGiver doesn't need a reference to every
    /// enemy in the scene, just this one class.
    /// </summary>
    public static event Action<string> OnAnyEnemyDeath;

    private Collider2D col;
    private SpriteRenderer sr;
    private EnemyAI ai;

    private void Awake()
    {
        currentHP = maxHP;
        col = GetComponent<Collider2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        ai = GetComponent<EnemyAI>();
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

        // Collider disabled immediately so a dying enemy stops blocking
        // movement/being hittable right away. The sprite, however, stays
        // visible for deathHideDelay seconds so the Death animation
        // (triggered by EnemyAI in response to OnDeath below) actually has
        // time to play before EnemySpawner reuses/hides this instance -
        // previously the sprite was hidden in this same frame, before the
        // Death trigger even reached the Animator, so nothing ever showed.
        if (col != null) col.enabled = false;

        OnDeath?.Invoke(this);
        OnAnyEnemyDeath?.Invoke(ai != null ? ai.mobName : gameObject.name);

        StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(deathHideDelay);
        if (sr != null) sr.enabled = false;
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