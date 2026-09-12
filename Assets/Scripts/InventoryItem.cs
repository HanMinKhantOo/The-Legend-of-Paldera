using UnityEngine;
using TMPro;

public enum ItemType
{
    Branch,
    Stone,
    Log
}

public class InventoryItem : MonoBehaviour
{
    public ItemType itemType;

    [Header("Stack Settings")]
    public int quantity = 1;
    public int maxStack = 99;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI quantityText;

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