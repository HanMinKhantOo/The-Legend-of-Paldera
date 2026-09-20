using UnityEngine;
using TMPro;

public enum ItemType
{
    Branch,
    Stone,
    Log,
    Food,
    Plank,
    Stick,

    WoodenSword,
    WoodenPickaxe,
    WoodenAxe,

    GemRed,
    GemGreen,
    GemBlue,
    GemYellow,
    GemRainbow,

    IronBar
}

public enum ItemCategory
{
    Resource,
    Food,
    Sword,
    Pickaxe,
    Axe,
    Armor,
    Gem
}

public class InventoryItem : MonoBehaviour
{
    public ItemType itemType;
    public ItemCategory itemCategory;

    [Header("Stack Settings")]
    public int quantity = 1;
    public int maxStack = 99;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI quantityText;
    [Tooltip("Applied to every item's quantity text at runtime, so changing it here (once) updates every item prefab everywhere - inventory, crafting, and hotbar - without editing each prefab individually.")]
    [SerializeField] private Color quantityTextColor = new Color(0.29f, 0.18f, 0.09f); // dark brown, readable on the cream UI background

    [Header("Food Settings (only used when Item Type = Food)")]
    [Tooltip("Hunger restored per item eaten.")]
    public float hungerRestore = 25f;
    [Tooltip("HP restored per item eaten. Leave at 0 for food that only restores Hunger.")]
    public float hpRestore = 0f;

    private void Awake()
    {
        UpdateQuantityText();
    }

    /// <summary>
    /// Central place to ask "is this ItemType a pickaxe" - used by the
    /// mining hit-detection (PlayerPunch) so any new pickaxe tier just
    /// needs one line added here, rather than every place that checks for
    /// a pickaxe needing its own list of tool types.
    /// </summary>
    public static bool IsPickaxe(ItemType type)
    {
        switch (type)
        {
            case ItemType.WoodenPickaxe:
                return true;

            // Add future pickaxe tiers here, e.g.:
            // case ItemType.StonePickaxe: return true;

            default:
                return false;
        }
    }

    public void SetQuantity(int amount)
    {
        quantity = Mathf.Clamp(amount, 1, maxStack);
        UpdateQuantityText();
    }

    public void AddQuantity(int amount = 1)
    {
        quantity = Mathf.Clamp(quantity + amount, 1, maxStack);
        UpdateQuantityText();
    }

    public bool CanAddMore()
    {
        return quantity < maxStack;
    }

    public void UpdateQuantityText()
    {
        if (quantityText != null)
        {
            quantityText.text = quantity > 1
                ? quantity.ToString()
                : "";
            quantityText.color = quantityTextColor;
        }
    }
}