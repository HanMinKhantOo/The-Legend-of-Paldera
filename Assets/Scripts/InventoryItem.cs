using UnityEngine;
using TMPro;

public enum ItemType
{
    Branch,
    Stone,
    Log,
    Food
}

public class InventoryItem : MonoBehaviour
{
    public ItemType itemType;

    [Header("Stack Settings")]
    public int quantity = 1;
    public int maxStack = 99;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI quantityText;

    [Header("Food Settings (only used when Item Type = Food)")]
    [Tooltip("Hunger restored per item eaten.")]
    public float hungerRestore = 25f;
    [Tooltip("HP restored per item eaten. Leave at 0 for food that only restores Hunger.")]
    public float hpRestore = 0f;

    private void Awake()
    {
        UpdateQuantityText();
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
            quantityText.text = quantity.ToString();
        }
    }
}