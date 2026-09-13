using System;
using UnityEngine;

/// <summary>
/// Owns the player's HP and Hunger values and their time-based rules:
/// - Hunger drains gradually over time.
/// - While Hunger is at 0, HP drains gradually over time instead.
/// - Eating food (via FoodConsumable) restores Hunger and optionally HP.
/// - When HP reaches 0, the player dies; PlayerVitalsUI listens for the
///   OnDeath event to show the death screen, so this script stays UI-free
///   and safe to reuse (e.g. for a future "hardcore mode" without a UI).
///
/// One instance is expected on the Player object. PlayerVitalsUI and
/// FoodConsumable both look it up via PlayerVitals.Instance rather than a
/// scene reference, so no existing prefab wiring has to change.
/// </summary>
public class PlayerVitals : MonoBehaviour
{
    public static PlayerVitals Instance { get; private set; }

    [Header("HP")]
    public float maxHP = 100f;
    [SerializeField] private float currentHP = 100f;

    [Header("Hunger")]
    public float maxHunger = 100f;
    [SerializeField] private float currentHunger = 100f;

    [Header("Rates (per second)")]
    [Tooltip("How much Hunger drains per second under normal conditions.")]
    public float hungerDrainPerSecond = 0.35f;
    [Tooltip("How much HP drains per second once Hunger has hit 0.")]
    public float starvationDamagePerSecond = 2f;

    public float CurrentHP => currentHP;
    public float CurrentHunger => currentHunger;
    public bool IsDead { get; private set; }

    /// <summary>Raised once, the moment HP reaches 0. string = cause of death.</summary>
    public event Action<string> OnDeath;
    /// <summary>Raised whenever HP or Hunger changes, for UI to refresh from.</summary>
    public event Action OnVitalsChanged;

    private void Awake()
    {
        // Simple singleton - one player in this game, so this avoids every
        // other script needing a manually-dragged reference in the Inspector.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        currentHP = Mathf.Clamp(currentHP, 0f, maxHP);
        currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);
    }

    private void Update()
    {
        if (IsDead) return;

        TickHunger(Time.deltaTime);
    }

    private void TickHunger(float deltaTime)
    {
        bool changed = false;

        if (currentHunger > 0f)
        {
            currentHunger = Mathf.Max(0f, currentHunger - hungerDrainPerSecond * deltaTime);
            changed = true;
        }
        else
        {
            // Hunger is empty - HP drains instead, until death.
            float before = currentHP;
            currentHP = Mathf.Max(0f, currentHP - starvationDamagePerSecond * deltaTime);
            if (!Mathf.Approximately(before, currentHP)) changed = true;

            if (currentHP <= 0f)
            {
                Die("Hunger");
            }
        }

        if (changed) OnVitalsChanged?.Invoke();
    }

    /// <summary>
    /// Restores Hunger (and optionally HP) by the given amounts, clamped to
    /// their max values. Called by FoodConsumable when a food item is eaten.
    /// </summary>
    public void Eat(float hungerAmount, float hpAmount)
    {
        if (IsDead) return;

        currentHunger = Mathf.Clamp(currentHunger + hungerAmount, 0f, maxHunger);
        currentHP = Mathf.Clamp(currentHP + hpAmount, 0f, maxHP);

        OnVitalsChanged?.Invoke();
    }

    /// <summary>
    /// Applies direct damage from other sources (combat, hazards, etc.),
    /// kept separate from starvation so future systems can call this without
    /// touching Hunger at all.
    /// </summary>
    public void TakeDamage(float amount, string causeOfDeath = "Unknown")
    {
        if (IsDead) return;

        currentHP = Mathf.Max(0f, currentHP - amount);
        OnVitalsChanged?.Invoke();

        if (currentHP <= 0f)
        {
            Die(causeOfDeath);
        }
    }

    private void Die(string cause)
    {
        if (IsDead) return;

        IsDead = true;
        OnDeath?.Invoke(cause);
    }

    /// <summary>
    /// Resets HP/Hunger to full and clears the death state. Does not touch
    /// position (DeathScreenController handles moving the player back via
    /// SaveController.LoadGame) or inventory (nothing here ever touched it).
    /// </summary>
    public void Respawn()
    {
        currentHP = maxHP;
        currentHunger = maxHunger;
        IsDead = false;

        OnVitalsChanged?.Invoke();
    }
}